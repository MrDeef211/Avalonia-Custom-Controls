using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Globalization;


namespace Common.Controls;

public class RangeSlider : TemplatedControl
{
    private Border _lowerThumb;
    private Border _upperThumb;
    private Canvas _trackCanvas;
    private bool _isDraggingLower;
    private bool _isDraggingUpper;
    private double _dragOffsetX;
    private TextBox _lowerTextBox;
    private TextBox _upperTextBox;

    private bool _isLowerTextValid = true;
    private bool _isUpperTextValid = true;

    #region Styled Property

    public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<RangeSlider, double>(nameof(Minimum), 0.0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Maximum), 100.0);

    public static readonly StyledProperty<double> LowerValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(LowerValue), 0.0);

    public static readonly StyledProperty<double> UpperValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(UpperValue), 100.0);

    public static readonly StyledProperty<double> StepProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Step), 1.0);

    public static readonly StyledProperty<bool> ShowTextBoxesProperty =
            AvaloniaProperty.Register<RangeSlider, bool>(nameof(ShowTextBoxes), true);

    public static readonly StyledProperty<double> TextBoxWidthProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(TextBoxWidth), 50.0);

    public static readonly StyledProperty<bool> IsLowerThumbEnabledProperty =
            AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsLowerThumbEnabled), true);

    public static readonly StyledProperty<bool> IsUpperThumbEnabledProperty =
        AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsUpperThumbEnabled), true);

    public static readonly StyledProperty<IBrush> LowerThumbBrushProperty =
            AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(LowerThumbBrush), Brushes.Gray);

    public static readonly StyledProperty<IBrush> UpperThumbBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(UpperThumbBrush), Brushes.Gray);

    public static readonly StyledProperty<IBrush> TrackBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(TrackBrush), Brushes.LightGray);

    public static readonly StyledProperty<FillMode> FillModeProperty =
            AvaloniaProperty.Register<RangeSlider, FillMode>(nameof(FillMode), FillMode.Between);

    public static readonly StyledProperty<IBrush> FillBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(FillBrush), Brushes.DodgerBlue);

    public static readonly StyledProperty<bool> ShowEndStopsProperty =
            AvaloniaProperty.Register<RangeSlider, bool>(nameof(ShowEndStops), true);

    public static readonly StyledProperty<GridLength> TickStepProperty =
            AvaloniaProperty.Register<RangeSlider, GridLength>(nameof(TickStep), GridLength.Auto);

    public static readonly StyledProperty<IBrush> TickBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(TickBrush), Brushes.Black);

    public static readonly StyledProperty<double> TickLengthProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(TickLength), 5.0);

    #endregion

    #region Direct Property



    #endregion

    static RangeSlider()
    {
        LowerValueProperty.Changed.AddClassHandler<RangeSlider>((x, e) => x.OnLowerValueChanged(e));
        UpperValueProperty.Changed.AddClassHandler<RangeSlider>((x, e) => x.OnUpperValueChanged(e));

        AffectsRender<RangeSlider>(
                MinimumProperty,
                MaximumProperty,
                TickStepProperty,
                TickBrushProperty,
                TickLengthProperty,
                ShowEndStopsProperty,
                FillModeProperty,
                FillBrushProperty,
                LowerThumbBrushProperty,
                UpperThumbBrushProperty,
                TrackBrushProperty,
                LowerValueProperty,
                UpperValueProperty);
    }


    #region Свойства

    /// <summary>
    /// Минимальное значение шкалы
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Максимальное значение шкалы
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>
    /// Значение нижнего ползунка
    /// </summary>
    public double LowerValue
    {
        get => GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    /// <summary>
    /// Значение верхнего ползунка
    /// </summary>
    public double UpperValue
    {
        get => GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    /// <summary>
    /// Шаг прокрутки
    /// </summary>
    public double Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>
    /// Показывать блоки с текстовым отображением значений ползунков
    /// </summary>
    public bool ShowTextBoxes
    {
        get => GetValue(ShowTextBoxesProperty);
        set => SetValue(ShowTextBoxesProperty, value);
    }

    /// <summary>
    /// Ширина блоков с текстом
    /// </summary>
    public double TextBoxWidth
    {
        get => GetValue(TextBoxWidthProperty);
        set => SetValue(TextBoxWidthProperty, value);
    }

    /// <summary>
    /// Включение перетаскивания нижнего ползунка
    /// </summary>
    /// <remarks>
    /// Используется для блокировки ручного управления, может использоваться как наглядный ограничитель (например при динамическом ограничении нижней величины)
    /// </remarks>
    public bool IsLowerThumbEnabled
    {
        get => GetValue(IsLowerThumbEnabledProperty);
        set => SetValue(IsLowerThumbEnabledProperty, value);
    }

    /// <summary>
    /// Включение перетаскивания верхнего ползунка
    /// </summary>
    /// <remarks>
    /// Используется для блокировки ручного управления, может использоваться как наглядный ограничитель (например при динамическом ограничении верхней величины)
    /// </remarks>
    public bool IsUpperThumbEnabled
    {
        get => GetValue(IsUpperThumbEnabledProperty);
        set => SetValue(IsUpperThumbEnabledProperty, value);
    }

    /// <summary>
    /// Цвет нижнего ползунка
    /// </summary>
    public IBrush LowerThumbBrush
    {
        get => GetValue(LowerThumbBrushProperty);
        set => SetValue(LowerThumbBrushProperty, value);
    }

    /// <summary>
    /// Цвет верхнего ползунка
    /// </summary>
    public IBrush UpperThumbBrush
    {
        get => GetValue(UpperThumbBrushProperty);
        set => SetValue(UpperThumbBrushProperty, value);
    }

    /// <summary>
    /// Цвет направляющей
    /// </summary>
    public IBrush TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>
    /// Шкала заполнения рядом с ползунками
    /// </summary>
    public FillMode FillMode
    {
        get => GetValue(FillModeProperty);
        set => SetValue(FillModeProperty, value);
    }

    /// <summary>
    /// Цвет шкалы заполнения
    /// </summary>
    public IBrush FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    /// <summary>
    /// Показывать ограничители на краях направляющей
    /// </summary>
    public bool ShowEndStops
    {
        get => GetValue(ShowEndStopsProperty);
        set => SetValue(ShowEndStopsProperty, value);
    }

    /// <summary>
    /// Шаг засечек на направляющей
    /// </summary>
    /// <remarks>
    /// - Auto: автоматический шаг (примерно 5-25 пикселей)
    /// - *: фиксированное количество засечек
    /// - Фиксированное значение: шаг в единицах значения
    /// </remarks>
    public GridLength TickStep
    {
        get => GetValue(TickStepProperty);
        set => SetValue(TickStepProperty, value);
    }

    /// <summary>
    /// Цвет засечек
    /// </summary>
    public IBrush TickBrush
    {
        get => GetValue(TickBrushProperty);
        set => SetValue(TickBrushProperty, value);
    }

    /// <summary>
    /// Длина сасечек
    /// </summary>
    public double TickLength
    {
        get => GetValue(TickLengthProperty);
        set => SetValue(TickLengthProperty, value);
    }

    #endregion

    #region События

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _lowerThumb = e.NameScope.Find<Border>("PART_LowerThumb");
        _upperThumb = e.NameScope.Find<Border>("PART_UpperThumb");
        _trackCanvas = e.NameScope.Find<Canvas>("PART_TrackCanvas");
        _lowerTextBox = e.NameScope.Find<TextBox>("LowerTextBox");
        _upperTextBox = e.NameScope.Find<TextBox>("UpperTextBox");

        if (_lowerThumb != null)
        {
            _lowerThumb.PointerPressed += OnLowerThumbPointerPressed;
            _lowerThumb.PointerMoved += OnThumbPointerMoved;
            _lowerThumb.PointerReleased += OnThumbPointerReleased;
        }
        if (_upperThumb != null)
        {
            _upperThumb.PointerPressed += OnUpperThumbPointerPressed;
            _upperThumb.PointerMoved += OnThumbPointerMoved;
            _upperThumb.PointerReleased += OnThumbPointerReleased;
        }
        if (_lowerTextBox != null)
        {
            _lowerTextBox.TextChanged += OnLowerTextBoxTextChanged;
            _lowerTextBox.Text = LowerValue.ToString(CultureInfo.CurrentCulture);
        }
        if (_upperTextBox != null)
        {
            _upperTextBox.TextChanged += OnUpperTextBoxTextChanged;
            _upperTextBox.Text = UpperValue.ToString(CultureInfo.CurrentCulture);
        }

        this.GetObservable(LowerValueProperty).Subscribe(_ => UpdateThumbPositions());
        this.GetObservable(UpperValueProperty).Subscribe(_ => UpdateThumbPositions());
        this.GetObservable(MinimumProperty).Subscribe(_ => { UpdateThumbPositions(); InvalidateVisual(); });
        this.GetObservable(MaximumProperty).Subscribe(_ => { UpdateThumbPositions(); InvalidateVisual(); });

        if (_trackCanvas != null)
        {
            _trackCanvas.GetObservable(BoundsProperty).Subscribe(bounds =>
            {
                if (bounds.Width > 0)
                {
                    UpdateThumbPositions();
                    InvalidateVisual();
                }
            });
        }

        Dispatcher.UIThread.Post(() =>
        {
            UpdateThumbPositions();
            InvalidateVisual();
        }, DispatcherPriority.Loaded);
    }

    private void OnLowerValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        double newValue = (double)e.NewValue;
        double coerced = Math.Max(Minimum, Math.Min(UpperValue - Step, newValue));
        if (Math.Abs(coerced - newValue) > double.Epsilon)
            SetCurrentValue(LowerValueProperty, coerced);
    }

    private void OnUpperValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        double newValue = (double)e.NewValue;
        double coerced = Math.Min(Maximum, Math.Max(LowerValue + Step, newValue));
        if (Math.Abs(coerced - newValue) > double.Epsilon)
            SetCurrentValue(UpperValueProperty, coerced);
    }

    private void OnLowerThumbPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!IsLowerThumbEnabled) return;
        _isDraggingLower = true;
        var point = e.GetPosition(_trackCanvas);
        double thumbLeft = Canvas.GetLeft(_lowerThumb);
        _dragOffsetX = point.X - thumbLeft;
        e.Pointer.Capture(_lowerThumb);
    }

    private void OnUpperThumbPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (!IsUpperThumbEnabled) return;
        _isDraggingUpper = true;
        var point = e.GetPosition(_trackCanvas);
        double thumbLeft = Canvas.GetLeft(_upperThumb);
        _dragOffsetX = point.X - thumbLeft;
        e.Pointer.Capture(_upperThumb);
    }

    private void OnThumbPointerMoved(object sender, PointerEventArgs e)
    {
        if (!_isDraggingLower && !_isDraggingUpper) return;
        var point = e.GetPosition(_trackCanvas);
        double trackWidth = _trackCanvas.Bounds.Width;
        if (trackWidth <= 0) return;

        if (_isDraggingLower)
        {
            double newLeft = point.X - _dragOffsetX;
            newLeft = Math.Clamp(newLeft, 0, trackWidth - _lowerThumb.Width);
            double newValue = PositionToValue(newLeft, trackWidth, _lowerThumb.Width);
            newValue = Math.Max(Minimum, Math.Min(UpperValue - Step, newValue));
            newValue = RoundToStep(newValue);
            SetCurrentValue(LowerValueProperty, newValue);
        }
        else if (_isDraggingUpper)
        {
            double newLeft = point.X - _dragOffsetX;
            newLeft = Math.Clamp(newLeft, 0, trackWidth - _upperThumb.Width);
            double newValue = PositionToValue(newLeft, trackWidth, _upperThumb.Width);
            newValue = Math.Max(LowerValue + Step, Math.Min(Maximum, newValue));
            newValue = RoundToStep(newValue);
            SetCurrentValue(UpperValueProperty, newValue);
        }
    }

    private void OnThumbPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        _isDraggingLower = false;
        _isDraggingUpper = false;
        e.Pointer.Capture(null);
    }

    private void OnLowerTextBoxTextChanged(object sender, RoutedEventArgs e)
    {
        if (_lowerTextBox == null) return;
        string text = _lowerTextBox.Text;
        bool isValid = double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double newValue);
        if (string.Equals(text, LowerValue.ToString(CultureInfo.CurrentCulture))) return;
        if (isValid)
        {
            newValue = Math.Max(Minimum, Math.Min(UpperValue - Step, newValue));
            newValue = RoundToStep(newValue);
            if (Math.Abs(newValue - LowerValue) > double.Epsilon)
            {
                SetCurrentValue(LowerValueProperty, newValue);
            }
            _lowerTextBox.Text = LowerValue.ToString(CultureInfo.CurrentCulture);
        }
        else if (text == "")
        {
            if (LowerValue != 0) SetCurrentValue(LowerValueProperty, 0);
            else
            {
                // Если ноль присваиваем временнное значение
                // Нужно чтобы SetCurrentValue вызвалось, и вместо пустой строки дальше прошли нормальные значения
                SetCurrentValue(LowerValueProperty, 1);
                SetCurrentValue(LowerValueProperty, 0);
            }
            _lowerTextBox.Text = LowerValue.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            // Сбрасываем значение через смену туда сюда
            var temp = LowerValue;
            SetCurrentValue(LowerValueProperty, temp - 1);
            SetCurrentValue(LowerValueProperty, temp);
            _lowerTextBox.Text = LowerValue.ToString(CultureInfo.CurrentCulture);
        }
    }

    private void OnUpperTextBoxTextChanged(object sender, RoutedEventArgs e)
    {
        if (_upperTextBox == null) return;
        string text = _upperTextBox.Text;
        bool isValid = double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out double newValue);
        if (string.Equals(text, UpperValue.ToString(CultureInfo.CurrentCulture))) return;

        if (isValid)
        {
            newValue = Math.Max(LowerValue + Step, Math.Min(Maximum, newValue));
            newValue = RoundToStep(newValue);
            if (Math.Abs(newValue - UpperValue) > double.Epsilon)
            {
                SetCurrentValue(UpperValueProperty, newValue);
            }
            _upperTextBox.Text = UpperValue.ToString(CultureInfo.CurrentCulture);
        }
        else if (text == "")
        {
            if (UpperValue != 0) SetCurrentValue(UpperValueProperty, 0);
            else
            {
                // Если ноль присваиваем временнное значение
                // Нужно чтобы SetCurrentValue вызвалось, и вместо пустой строки дальше прошли нормальные значения
                SetCurrentValue(UpperValueProperty, 1);
                SetCurrentValue(UpperValueProperty, 0);
            }
            _upperTextBox.Text = UpperValue.ToString(CultureInfo.CurrentCulture);
        }
        else
        {
            // Сбрасываем значение через смену туда сюда
            var temp = UpperValue;
            SetCurrentValue(UpperValueProperty, temp - 1);
            SetCurrentValue(UpperValueProperty, temp);
            _upperTextBox.Text = UpperValue.ToString(CultureInfo.CurrentCulture);
        }
    }

    #endregion

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_trackCanvas == null) return;

        double trackWidth = _trackCanvas.Bounds.Width;
        if (trackWidth <= 0) return;
        double trackHeight = 4; // толщина трека
        double centerY = _trackCanvas.Bounds.Height / 2;
        double startX = _trackCanvas.Bounds.X;

        // Трек
        var trackPen = new Pen(TrackBrush, trackHeight);
        context.DrawLine(trackPen, new Point(startX, centerY), new Point(startX + trackWidth, centerY));

        // Позиции ползунков
        double lowerPos = ValueToPosition(LowerValue, trackWidth);
        double upperPos = ValueToPosition(UpperValue, trackWidth);

        // Заливка в зависимости от FillMode
        if (FillMode != FillMode.None && FillBrush != null)
        {
            var fillPen = new Pen(FillBrush, trackHeight);
            if (FillMode == FillMode.Between)
            {
                context.DrawLine(fillPen, new Point(startX + lowerPos, centerY), new Point(startX + upperPos, centerY));
            }
            else if (FillMode == FillMode.Outside)
            {
                if (lowerPos > 0)
                    context.DrawLine(fillPen, new Point(startX, centerY), new Point(startX + lowerPos, centerY));
                if (upperPos < trackWidth)
                    context.DrawLine(fillPen, new Point(startX + upperPos, centerY), new Point(startX + trackWidth, centerY));
            }
        }

        // Ограничители по краям
        if (ShowEndStops)
        {
            var stopPen = new Pen(TickBrush, 1);
            double stopHeight = 12;
            context.DrawLine(stopPen, new Point(startX, centerY - stopHeight / 2), new Point(startX, centerY + stopHeight / 2));
            context.DrawLine(stopPen, new Point(startX + trackWidth, centerY - stopHeight / 2), new Point(startX + trackWidth, centerY + stopHeight / 2));
        }

        // Засечки
        var tickPen = new Pen(TickBrush, 1);
        var tickPositions = GetTickPositions(Minimum, Maximum, trackWidth);
        foreach (double value in tickPositions)
        {
            double x = ValueToPosition(value, trackWidth);
            if (x >= 0 && x <= trackWidth)
            {
                context.DrawLine(tickPen, new Point(startX + x, centerY - TickLength / 2), new Point(startX + x, centerY + TickLength / 2));
            }
        }
    }

    #region Вспомогательные методы для отрисовки

    private List<double> GetTickPositions(double min, double max, double trackLength)
    {
        var ticks = new List<double>();
        double range = max - min;
        if (range <= 0) return ticks;

        double step;
        if (TickStep.IsAuto)
        {
            int desiredCount = Math.Max(2, (int)(trackLength / 15));
            int count = GetNearestDivisor(desiredCount);
            step = range / (count - 1);
            step = RoundToNiceNumber(step);
            count = (int)Math.Round(range / step) + 1;
        }
        else if (TickStep.IsStar)
        {
            int count = (int)TickStep.Value;
            if (count < 2) return ticks;
            step = range / (count);
        }
        else
        {
            step = TickStep.Value;
        }

        double first = Math.Ceiling(min / step) * step;
        for (double v = first; v <= max + step * 0.001; v += step)
        {
            if (v >= min - step * 0.001)
                ticks.Add(v);
        }
        return ticks;
    }

    private void UpdateThumbPositions()
    {
        if (_lowerThumb == null || _upperThumb == null || _trackCanvas == null) return;
        double trackWidth = _trackCanvas.Bounds.Width;
        double trackHeight = _trackCanvas.Bounds.Height;
        if (trackWidth <= 0 || trackHeight <= 0) return;

        double lowerPos = ValueToPosition(LowerValue, trackWidth, _lowerThumb.Width);
        double upperPos = ValueToPosition(UpperValue, trackWidth, _upperThumb.Width);
        double centerY = (trackHeight - _lowerThumb.Height) / 2;

        Canvas.SetLeft(_lowerThumb, lowerPos);
        Canvas.SetLeft(_upperThumb, upperPos);
        Canvas.SetTop(_lowerThumb, centerY);
        Canvas.SetTop(_upperThumb, centerY);
    }

    #endregion

    #region Методы для расчётов

    private int GetNearestDivisor(int desiredCount)
    {
        var divisors = new[] { 2, 4, 5, 10, 20, 25, 50, 100 };
        int best = divisors[0];
        foreach (var d in divisors)
        {
            if (Math.Abs(d - desiredCount) < Math.Abs(best - desiredCount))
                best = d;
        }
        return best;
    }

    private double RoundToNiceNumber(double value)
    {
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
        double normalized = value / magnitude;
        if (normalized <= 1) return 1 * magnitude;
        if (normalized <= 2) return 2 * magnitude;
        if (normalized <= 5) return 5 * magnitude;
        return 10 * magnitude;
    }

    private double PositionToValue(double pos, double trackWidth)
    {
        double range = Maximum - Minimum;
        if (range <= 0 || trackWidth <= 0) return Minimum;
        double ratio = Math.Clamp(pos / trackWidth, 0, 1);
        return Minimum + ratio * range;
    }

    private double ValueToPosition(double value, double trackWidth)
    {
        double range = Maximum - Minimum;
        if (range <= 0 || trackWidth <= 0) return 0;
        double ratio = (value - Minimum) / range;
        return Math.Clamp(ratio * trackWidth, 0, trackWidth);
    }

    private double ValueToPosition(double value, double trackWidth, double thumbWidth)
    {
        double range = Maximum - Minimum;
        if (range <= 0 || trackWidth <= thumbWidth) return 0;
        double ratio = (value - Minimum) / range;
        ratio = Math.Clamp(ratio, 0, 1);
        return ratio * (trackWidth - thumbWidth);
    }

    private double PositionToValue(double left, double trackWidth, double thumbWidth)
    {
        double range = Maximum - Minimum;
        if (range <= 0 || trackWidth <= thumbWidth) return Minimum;
        double ratio = left / (trackWidth - thumbWidth);
        ratio = Math.Clamp(ratio, 0, 1);
        return Minimum + ratio * range;
    }

    private double RoundToStep(double value)
    {
        if (Step <= 0 || double.IsNaN(value) || double.IsInfinity(value)) return value;
        return Math.Round(value / Step) * Step;
    }

    #endregion

}

public enum FillMode
{
    // не заполнять
    None,

    // заполнить участок между ползунками
    Between,

    // заполнить участки слева от Lower и справа от Upper
    Outside
}