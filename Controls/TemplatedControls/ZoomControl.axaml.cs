using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Controls;

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
        AvaloniaProperty.Register<ZoomControl, ZoomState?>(nameof(ZoomState), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Состояние положения камеры
    /// </summary>
    public ZoomState? ZoomState
    {
        get => GetValue(ZoomStateProperty);
        set => SetValue(ZoomStateProperty, value);
    }

    public static readonly StyledProperty<bool> RestrictPanProperty =
        AvaloniaProperty.Register<ZoomControl, bool>(nameof(RestrictPan), true);

    /// <summary>
    /// Ограничивать область перемещения границами видимой зоны при минимальном зуме
    /// </summary>
    public bool RestrictPan
    {
        get => GetValue(RestrictPanProperty);
        set => SetValue(RestrictPanProperty, value);
    }

    #endregion

    #region Назначения клавиш

    // Клавиши панорамирования
    public static readonly StyledProperty<Key> PanUpKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanUpKey), Key.W);
    public static readonly StyledProperty<Key> PanDownKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanDownKey), Key.S);
    public static readonly StyledProperty<Key> PanLeftKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanLeftKey), Key.A);
    public static readonly StyledProperty<Key> PanRightKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanRightKey), Key.D);

    // Альтернативные клавиши (стрелки) — можно отключить, установив Key.None
    public static readonly StyledProperty<Key> PanUpAltKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanUpAltKey), Key.Up);
    public static readonly StyledProperty<Key> PanDownAltKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanDownAltKey), Key.Down);
    public static readonly StyledProperty<Key> PanLeftAltKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanLeftAltKey), Key.Left);
    public static readonly StyledProperty<Key> PanRightAltKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(PanRightAltKey), Key.Right);

    // Клавиши масштабирования
    public static readonly StyledProperty<Key> ZoomInKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(ZoomInKey), Key.Add);
    public static readonly StyledProperty<Key> ZoomOutKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(ZoomOutKey), Key.Subtract);

    // Сброс масштаба
    public static readonly StyledProperty<Key> ResetZoomKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(ResetZoomKey), Key.D0);
    public static readonly StyledProperty<KeyModifiers> ResetZoomModifiersProperty =
        AvaloniaProperty.Register<ZoomControl, KeyModifiers>(nameof(ResetZoomModifiers), KeyModifiers.Control);

    // Вписать в экран
    public static readonly StyledProperty<Key> FitToScreenKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(FitToScreenKey), Key.Home);
    public static readonly StyledProperty<KeyModifiers> FitToScreenModifiersProperty =
        AvaloniaProperty.Register<ZoomControl, KeyModifiers>(nameof(FitToScreenModifiers), KeyModifiers.None);

    // История (Undo/Redo)
    public static readonly StyledProperty<Key> UndoKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(UndoKey), Key.Z);
    public static readonly StyledProperty<KeyModifiers> UndoModifiersProperty =
        AvaloniaProperty.Register<ZoomControl, KeyModifiers>(nameof(UndoModifiers), KeyModifiers.Control);
    public static readonly StyledProperty<Key> RedoKeyProperty =
        AvaloniaProperty.Register<ZoomControl, Key>(nameof(RedoKey), Key.Y);
    public static readonly StyledProperty<KeyModifiers> RedoModifiersProperty =
        AvaloniaProperty.Register<ZoomControl, KeyModifiers>(nameof(RedoModifiers), KeyModifiers.Control);

    // Включение клавиатурного управления
    public static readonly StyledProperty<bool> EnableKeyboardNavigationProperty =
        AvaloniaProperty.Register<ZoomControl, bool>(nameof(EnableKeyboardNavigation), true);


    public Key PanUpKey { get => GetValue(PanUpKeyProperty); set => SetValue(PanUpKeyProperty, value); }
    public Key PanDownKey { get => GetValue(PanDownKeyProperty); set => SetValue(PanDownKeyProperty, value); }
    public Key PanLeftKey { get => GetValue(PanLeftKeyProperty); set => SetValue(PanLeftKeyProperty, value); }
    public Key PanRightKey { get => GetValue(PanRightKeyProperty); set => SetValue(PanRightKeyProperty, value); }
    public Key PanUpAltKey { get => GetValue(PanUpAltKeyProperty); set => SetValue(PanUpAltKeyProperty, value); }
    public Key PanDownAltKey { get => GetValue(PanDownAltKeyProperty); set => SetValue(PanDownAltKeyProperty, value); }
    public Key PanLeftAltKey { get => GetValue(PanLeftAltKeyProperty); set => SetValue(PanLeftAltKeyProperty, value); }
    public Key PanRightAltKey { get => GetValue(PanRightAltKeyProperty); set => SetValue(PanRightAltKeyProperty, value); }
    public Key ZoomInKey { get => GetValue(ZoomInKeyProperty); set => SetValue(ZoomInKeyProperty, value); }
    public Key ZoomOutKey { get => GetValue(ZoomOutKeyProperty); set => SetValue(ZoomOutKeyProperty, value); }
    /// <summary>
    /// Клавиша сброса зума
    /// </summary>
    public Key ResetZoomKey { get => GetValue(ResetZoomKeyProperty); set => SetValue(ResetZoomKeyProperty, value); }
    /// <summary>
    /// Клавиша модификатор сброса зума
    /// </summary>
    public KeyModifiers ResetZoomModifiers { get => GetValue(ResetZoomModifiersProperty); set => SetValue(ResetZoomModifiersProperty, value); }
    /// <summary>
    /// Клавиша вписывания в экран
    /// </summary>
    public Key FitToScreenKey { get => GetValue(FitToScreenKeyProperty); set => SetValue(FitToScreenKeyProperty, value); }
    /// <summary>
    /// Клавиша модификатор вписывания в экран
    /// </summary>
    public KeyModifiers FitToScreenModifiers { get => GetValue(FitToScreenModifiersProperty); set => SetValue(FitToScreenModifiersProperty, value); }
    /// <summary>
    /// Клавиша отмены
    /// </summary>
    public Key UndoKey { get => GetValue(UndoKeyProperty); set => SetValue(UndoKeyProperty, value); }
    public KeyModifiers UndoModifiers { get => GetValue(UndoModifiersProperty); set => SetValue(UndoModifiersProperty, value); }
    /// <summary>
    /// Клавиша возврата отмены
    /// </summary>
    public Key RedoKey { get => GetValue(RedoKeyProperty); set => SetValue(RedoKeyProperty, value); }
    public KeyModifiers RedoModifiers { get => GetValue(RedoModifiersProperty); set => SetValue(RedoModifiersProperty, value); }
    /// <summary>
    /// Включить навигацию через клавиатуру
    /// </summary>
    public bool EnableKeyboardNavigation { get => GetValue(EnableKeyboardNavigationProperty); set => SetValue(EnableKeyboardNavigationProperty, value); }

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

    #region События

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _presenter = e.NameScope.Find<Border>("PART_Presenter");
        if (_presenter != null)
        {
            RenderOptions.SetBitmapInterpolationMode(_presenter, BitmapInterpolationMode.HighQuality);
            RenderOptions.SetEdgeMode(_presenter, EdgeMode.Antialias);
        }
        UpdateTransform();
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        if (!EnableKeyboardNavigation) return;
        double dx = 0, dy = 0;
        double step = 10.0;

        if (_pressedKeys.Contains(PanUpKey) || _pressedKeys.Contains(PanUpAltKey)) dy += step;
        if (_pressedKeys.Contains(PanDownKey) || _pressedKeys.Contains(PanDownAltKey)) dy -= step;
        if (_pressedKeys.Contains(PanLeftKey) || _pressedKeys.Contains(PanLeftAltKey)) dx += step;
        if (_pressedKeys.Contains(PanRightKey) || _pressedKeys.Contains(PanRightAltKey)) dx -= step;

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

        SetMatrix(ClampMatrix(nextMatrix));
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e) => ResetZoom();

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        var point = e.GetPosition(this);
        var delta = e.Delta.Y;

        ZoomAtPoint(point, delta);

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
        if (!EnableKeyboardNavigation) return;

        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        if (e.Key == ZoomInKey || e.Key == Key.OemPlus)
        {
            ZoomAtPoint(center, 1);
            e.Handled = true;
        }
        else if (e.Key == ZoomOutKey || e.Key == Key.OemMinus)
        {
            ZoomAtPoint(center, -1);
            e.Handled = true;
        }
        else if (e.Key == ResetZoomKey && e.KeyModifiers == ResetZoomModifiers)
        {
            ResetZoom();
            e.Handled = true;
        }
        else if (e.Key == FitToScreenKey && e.KeyModifiers == FitToScreenModifiers)
        {
            FitToScreen();
            e.Handled = true;
        }
        else if (e.Key == UndoKey && e.KeyModifiers == UndoModifiers)
        {
            Undo();
            e.Handled = true;
        }
        else if (e.Key == RedoKey && e.KeyModifiers == RedoModifiers)
        {
            Redo();
            e.Handled = true;
        }
    }

    #endregion

    #region Основные действия

    private void UpdateTransform()
    {
        if (_presenter != null)
        { 
            _presenter.RenderTransform = new MatrixTransform(_matrix);
            _presenter.Opacity = _presenter.Opacity == 1.0 ? 0.9999 : 1.0;
            InvalidateVisual();
        }
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

    private void JumpToMatrix(Matrix newMatrix)
    {
        var clamped = ClampMatrix(newMatrix);
        _targetMatrix = clamped;
        _matrix = clamped;
        UpdateTransform();
        SyncZoomState();
    }

    private void SmoothSetMatrix(Matrix target)
    {
        _targetMatrix = ClampMatrix(target);
        if (!_animationTimer.IsEnabled) _animationTimer.Start();
    }

    private void PushState()
    {
        _undoStack.AddLast(_matrix);
        if (_undoStack.Count > MaxHistorySize)
            _undoStack.RemoveFirst();
        _redoStack.Clear();
    }

    #endregion

    #region Обработка команд

    private void ZoomAtPoint(Point point, double delta)
    {
        double oldScale = _targetMatrix.M11;
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

    private void Undo()
    {
        if (_undoStack.Count == 0) return;
        _redoStack.Push(_matrix);
        var previous = _undoStack.Last.Value;
        _undoStack.RemoveLast();
        JumpToMatrix(previous);
    }

    private void Redo()
    {
        if (_redoStack.Count == 0) return;
        _undoStack.AddLast(_matrix);
        JumpToMatrix(_redoStack.Pop());
    }

    private void ResetZoom()
    {
        PushState();
        JumpToMatrix(Matrix.Identity);
    }

    /// <summary>
    /// Подогнать содержимое под размеры контрола с сохранением пропорций.
    /// </summary>
    private void FitToScreen()
    {
        if (_presenter?.Child == null) return;

        var content = _presenter.Child;
        if (content.Bounds.Width <= 0 || content.Bounds.Height <= 0)
        {
            content.Measure(Size.Infinity);
            content.Arrange(new Rect(content.DesiredSize));
        }
        double contentWidth = content.Bounds.Width;
        double contentHeight = content.Bounds.Height;
        double availableWidth = Bounds.Width;
        double availableHeight = Bounds.Height;

        if (contentWidth <= 0 || contentHeight <= 0 || availableWidth <= 0 || availableHeight <= 0)
            return;

        double scaleX = availableWidth / contentWidth;
        double scaleY = availableHeight / contentHeight;
        double scale = Math.Min(scaleX, scaleY);
        scale = Math.Clamp(scale, MinScale, MaxScale);

        double offsetX = (availableWidth - contentWidth * scale) / 2;
        double offsetY = (availableHeight - contentHeight * scale) / 2;

        PushState();
        JumpToMatrix(new Matrix(scale, 0, 0, scale, offsetX, offsetY));
    }

    #endregion

    #region Вспомогательные вычисления

    private Matrix ClampMatrix(Matrix matrix)
    {
        double scale = matrix.M11;
        double clampedScale = Math.Clamp(scale, MinScale, MaxScale);

        Matrix correctedMatrix = matrix;
        if (Math.Abs(scale - clampedScale) > 0.0001)
        {
            correctedMatrix = new Matrix(
                clampedScale, matrix.M12,
                matrix.M21, clampedScale,
                matrix.M31, matrix.M32);

            scale = clampedScale;
        }

        if (!RestrictPan || _presenter?.Child == null) return correctedMatrix;

        var content = _presenter.Child;

        double virtualAreaWidth = Bounds.Width / MinScale;
        double virtualAreaHeight = Bounds.Height / MinScale;

        double minX = Bounds.Width - content.Bounds.Width * scale - (virtualAreaWidth - Bounds.Width) / 2;
        double maxX = (virtualAreaWidth - Bounds.Width) / 2;
        double minY = Bounds.Height - content.Bounds.Height * scale - (virtualAreaHeight - Bounds.Height) / 2;
        double maxY = (virtualAreaHeight - Bounds.Height) / 2;

        if (minX > maxX) (minX, maxX) = (maxX, minX);
        if (minY > maxY) (minY, maxY) = (maxY, minY);

        return new Matrix(
            correctedMatrix.M11, correctedMatrix.M12,
            correctedMatrix.M21, correctedMatrix.M22,
            Math.Clamp(correctedMatrix.M31, minX, maxX),
            Math.Clamp(correctedMatrix.M32, minY, maxY));
    }

    private bool IsMovementKeyPressed() =>
        _pressedKeys.Any(k => k == PanUpKey || k == PanDownKey || k == PanLeftKey || k == PanRightKey ||
                              k == PanUpAltKey || k == PanDownAltKey || k == PanLeftAltKey || k == PanRightAltKey);

    #endregion
}