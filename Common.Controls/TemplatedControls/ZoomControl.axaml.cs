using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Common.Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Common.Controls;

public class ZoomControl : ContentControl
{
    private Border? _presenter;
    private Matrix _targetMatrix;
    private Matrix _matrix = Matrix.Identity;
    private Point _dragStart;
    private bool _isDragging;
    private DispatcherTimer _animationTimer;

    private readonly HashSet<Key> _pressedKeys = new();

    private readonly LinkedList<Matrix> _undoStack = new();
    private readonly Stack<Matrix> _redoStack = new();
    private const int MaxHistorySize = 50;

    #region Свойства

    public static readonly StyledProperty<double> MinScaleProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(MinScale), 0.1);

    /// <summary>
    /// Минимальное увеличение (максимальное удаление)
    /// </summary>
    public double MinScale
    {
        get => GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public static readonly StyledProperty<double> MaxScaleProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(MaxScale), 10.0);

    /// <summary>
    /// Максимальное увеличение
    /// </summary>
    public double MaxScale
    {
        get => GetValue(MaxScaleProperty);
        set => SetValue(MaxScaleProperty, value);
    }

    public static readonly StyledProperty<double> ZoomSpeedProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(ZoomSpeed), 1.2);

    /// <summary>
    /// Скорость приближения
    /// </summary>
    public double ZoomSpeed
    {
        get => GetValue(ZoomSpeedProperty);
        set => SetValue(ZoomSpeedProperty, value);
    }

    public static readonly StyledProperty<double> SmoothnessProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(Smoothness), 0.5);

    /// <summary>
    /// Плавность движений, чем меньше, тем плавнее (0.1 - 0.5)
    /// </summary>
    public double Smoothness
    {
        get => GetValue(SmoothnessProperty);
        set
        {
            if (value >= 0.1 && value <= 0.5)
                SetValue(SmoothnessProperty, value);
            else
                throw new ArgumentOutOfRangeException(nameof(Smoothness) + " должен быть в диапазоне от 0.1 до 0.5");
        }
    }

    public static readonly StyledProperty<bool> EnablePanWithRightButtonProperty =
        AvaloniaProperty.Register<ZoomControl, bool>(nameof(EnablePanWithRightButton), true);

    /// <summary>
    /// Включить перетаскивание правой кнопкой мыши
    /// </summary>
    public bool EnablePanWithRightButton
    {
        get => GetValue(EnablePanWithRightButtonProperty);
        set => SetValue(EnablePanWithRightButtonProperty, value);
    }

    public static readonly StyledProperty<double> WheelSensitivityProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(WheelSensitivity), 100.0);

    /// <summary>
    /// Чуствительность колеса мыши
    /// </summary>
    public double WheelSensitivity
    {
        get => GetValue(WheelSensitivityProperty);
        set => SetValue(WheelSensitivityProperty, value);
    }

    public static readonly StyledProperty<ZoomState?> ZoomStateProperty =
        AvaloniaProperty.Register<ZoomControl, ZoomState?>(nameof(ZoomState));

    /// <summary>
    /// Состояние положения камеры
    /// </summary>
    public ZoomState? ZoomState
    {
        get => GetValue(ZoomStateProperty);
        set => SetValue(ZoomStateProperty, value);
    }

    #endregion

    #region Команды

    private ReactiveCommand<Unit, Unit>? _resetZoomCommand;
    public ReactiveCommand<Unit, Unit> ResetZoomCommand =>
        _resetZoomCommand ??= ReactiveCommand.Create(ResetZoom);

    private ReactiveCommand<Unit, Unit>? _fitToScreenCommand;
    public ReactiveCommand<Unit, Unit> FitToScreenCommand =>
        _fitToScreenCommand ??= ReactiveCommand.Create(FitToScreen);

    private ReactiveCommand<Unit, Unit>? _undoCommand;
    public ReactiveCommand<Unit, Unit> UndoCommand =>
        _undoCommand ??= ReactiveCommand.Create(Undo);

    private ReactiveCommand<Unit, Unit>? _redoCommand;
    public ReactiveCommand<Unit, Unit> RedoCommand =>
        _redoCommand ??= ReactiveCommand.Create(Redo);

    #endregion

    public ZoomControl()
    {
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Focusable = true;
        DoubleTapped += OnDoubleTapped;
        KeyDown += OnKeyDown;

        _targetMatrix = Matrix.Identity;

        _animationTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(10),
            DispatcherPriority.Render,
            OnAnimationTick);

        this.WhenAnyValue(x => x.ZoomState)
            .Subscribe(state =>
            {
                if (state != null)
                {
                    var newMatrix = Matrix.CreateScale(state.Scale, state.Scale) *
                                    Matrix.CreateTranslation(state.OffsetX, state.OffsetY);
                    if (_matrix != newMatrix)
                    {
                        _matrix = newMatrix;
                        UpdateTransform();
                    }
                }
            });
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        double dx = 0, dy = 0;
        double step = 10.0;

        if (_pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.Up)) dy += step;
        if (_pressedKeys.Contains(Key.S) || _pressedKeys.Contains(Key.Down)) dy -= step;
        if (_pressedKeys.Contains(Key.A) || _pressedKeys.Contains(Key.Left)) dx += step;
        if (_pressedKeys.Contains(Key.D) || _pressedKeys.Contains(Key.Right)) dx -= step;

        if (dx != 0 || dy != 0)
        {
            _targetMatrix *= Matrix.CreateTranslation(dx, dy);
        }

        double diff = Math.Abs(_matrix.M11 - _targetMatrix.M11) +
                      Math.Abs(_matrix.M31 - _targetMatrix.M31) +
                      Math.Abs(_matrix.M32 - _targetMatrix.M32);

        if (diff < 0.0001 && dx == 0 && dy == 0)
        {
            SetMatrix(_targetMatrix);
            _animationTimer.Stop();
            return;
        }

        var nextMatrix = new Matrix(
            _matrix.M11 + (_targetMatrix.M11 - _matrix.M11) * Smoothness, 0,
            0, _matrix.M22 + (_targetMatrix.M22 - _matrix.M22) * Smoothness,
            _matrix.M31 + (_targetMatrix.M31 - _matrix.M31) * Smoothness,
            _matrix.M32 + (_targetMatrix.M32 - _matrix.M32) * Smoothness
        );

        SetMatrix(nextMatrix);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _presenter = e.NameScope.Find<Border>("PART_Presenter");
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (_presenter != null)
            _presenter.RenderTransform = new MatrixTransform(_matrix);
    }

    private void SyncZoomState()
    {
        if (ZoomState != null)
        {
            double scale = Math.Sqrt(_matrix.M11 * _matrix.M11 + _matrix.M12 * _matrix.M12);
            double offsetX = _matrix.M31;
            double offsetY = _matrix.M32;
            ZoomState.Scale = scale;
            ZoomState.OffsetX = offsetX;
            ZoomState.OffsetY = offsetY;
        }
    }

    private void SetMatrix(Matrix newMatrix)
    {
        _matrix = newMatrix;
        UpdateTransform();
        SyncZoomState();
    }

    private void SmoothSetMatrix(Matrix target)
    {
        _targetMatrix = target;
        if (!_animationTimer.IsEnabled) _animationTimer.Start();
    }

    private void PushState()
    {
        _undoStack.AddLast(_matrix);
        if (_undoStack.Count > MaxHistorySize)
            _undoStack.RemoveFirst();
        _redoStack.Clear();
    }

    private void Undo()
    {
        if (_undoStack.Count == 0) return;
        _redoStack.Push(_matrix);
        var previous = _undoStack.Last.Value;
        _undoStack.RemoveLast();
        SetMatrix(previous);
    }

    private void Redo()
    {
        if (_redoStack.Count == 0) return;
        _undoStack.AddLast(_matrix);
        SetMatrix(_redoStack.Pop());
    }

    private void ResetZoom() => SetMatrix(Matrix.Identity);
    private void FitToScreen() => ResetZoom();
    private void OnDoubleTapped(object? sender, TappedEventArgs e) => ResetZoom();

    private void ZoomAtPoint(Point point, double delta)
    {
        double oldScale = _matrix.M11;
        double scaleFactor = delta > 0 ? ZoomSpeed : 1 / ZoomSpeed;
        double newScale = Math.Clamp(oldScale * scaleFactor, MinScale, MaxScale);

        double actualFactor = newScale / oldScale;
        if (Math.Abs(actualFactor - 1.0) < 0.0001) return;

        PushState();

        var nextTarget = _targetMatrix *
                        Matrix.CreateTranslation(-point.X, -point.Y) *
                        Matrix.CreateScale(actualFactor, actualFactor) *
                        Matrix.CreateTranslation(point.X, point.Y);

        SmoothSetMatrix(nextTarget);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var point = e.GetPosition(this);
        var delta = e.Delta.Y;

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            PushState();
            var translate = Matrix.CreateTranslation(delta * (WheelSensitivity / _matrix.M11), 0);
            SetMatrix(_matrix * translate);
        }
        else
        {
            ZoomAtPoint(point, delta);
        }
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var props = e.GetCurrentPoint(this).Properties;
        bool isLeft = props.IsLeftButtonPressed;
        bool isRight = props.IsRightButtonPressed;

        if (isLeft || (isRight && EnablePanWithRightButton))
        {
            PushState();
            _isDragging = true;
            _dragStart = e.GetPosition(this);
            Cursor = new Cursor(StandardCursorType.SizeAll);
            e.Pointer.Capture(this);
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_isDragging)
        {
            var current = e.GetPosition(this);
            var delta = current - _dragStart;

            if (delta.X != 0 || delta.Y != 0)
            {
                SmoothSetMatrix(_targetMatrix * Matrix.CreateTranslation(delta.X, delta.Y));
                _dragStart = current;
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_isDragging)
        {
            _isDragging = false;
            Cursor = Cursor.Default;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }


    private bool IsMovementKeyPressed() =>
        _pressedKeys.Any(k => k is Key.W or Key.A or Key.S or Key.D or Key.Up or Key.Down or Key.Left or Key.Right);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _pressedKeys.Add(e.Key);

        ProcessStaticKeys(e);

        if (!_animationTimer.IsEnabled && IsMovementKeyPressed())
            _animationTimer.Start();

        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        _pressedKeys.Remove(e.Key);
        base.OnKeyUp(e);
    }

    private void ProcessStaticKeys(KeyEventArgs e)
    {
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        if (e.Key == Key.Add || e.Key == Key.OemPlus)
        {
            ZoomAtPoint(center, 1);
            e.Handled = true;
        }
        else if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
        {
            ZoomAtPoint(center, -1);
            e.Handled = true;
        }
        else if (e.Key == Key.D0 && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            ResetZoom();
            e.Handled = true;
        }
        else if (e.Key == Key.Home)
        {
            FitToScreen();
            e.Handled = true;
        }
        else if (e.Key == Key.Z && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            Undo();
            e.Handled = true;
        }
        else if (e.Key == Key.Y && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            Redo();
            e.Handled = true;
        }
    }
}