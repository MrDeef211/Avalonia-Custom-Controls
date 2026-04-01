using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Common.Controls;

public class RangeSlider : TemplatedControl
{
    #region Styled Property

    public static readonly StyledProperty<double> MinimumProperty =
    AvaloniaProperty.Register<RangeSlider, double>(nameof(Minimum), 0.0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(Maximum), 100.0);

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<RangeSlider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<IBrush> TrackBrushProperty =
    AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(TrackBrush), Brushes.LightGray);

    public static readonly StyledProperty<IBrush> SelectionBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(SelectionBrush), Brushes.DodgerBlue);

    public static readonly StyledProperty<IBrush> ThumbBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(ThumbBrush), Brushes.White);

    public static readonly StyledProperty<bool> IsLowerReadOnlyProperty =
        AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsLowerReadOnly), false);

    public static readonly StyledProperty<bool> IsUpperReadOnlyProperty =
        AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsUpperReadOnly), false);

    public static readonly StyledProperty<bool> ShowTextBoxesProperty =
        AvaloniaProperty.Register<RangeSlider, bool>(nameof(ShowTextBoxes), true);

    public static readonly StyledProperty<Dock> TextBoxDockProperty =
        AvaloniaProperty.Register<RangeSlider, Dock>(nameof(TextBoxDock), Dock.Left);

    public static readonly StyledProperty<TickPlacement> TickPlacementProperty =
        AvaloniaProperty.Register<RangeSlider, TickPlacement>(nameof(TickPlacement), TickPlacement.Between);

    public static readonly StyledProperty<TickFrequencyMode> TickFrequencyModeProperty =
        AvaloniaProperty.Register<RangeSlider, TickFrequencyMode>(nameof(TickFrequencyMode), TickFrequencyMode.Auto);

    public static readonly StyledProperty<double> TickFrequencyProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(TickFrequency), 10.0);

    public static readonly StyledProperty<IBrush> TickBrushProperty =
        AvaloniaProperty.Register<RangeSlider, IBrush>(nameof(TickBrush), Brushes.Black);

    public static readonly StyledProperty<double> TickLengthProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(TickLength), 5.0);

    #endregion


    #region Direct Property

    private double _lowerValue;
    public static readonly DirectProperty<RangeSlider, double> LowerValueProperty =
        AvaloniaProperty.RegisterDirect<RangeSlider, double>(
            nameof(LowerValue),
            o => o.LowerValue,
            (o, v) => o.LowerValue = v);

    public double LowerValue
    {
        get => _lowerValue;
        set => SetAndRaise(LowerValueProperty, ref _lowerValue, CoerceValue(value));
    }

    private double _upperValue;
    public static readonly DirectProperty<RangeSlider, double> UpperValueProperty =
        AvaloniaProperty.RegisterDirect<RangeSlider, double>(
            nameof(UpperValue),
            o => o.UpperValue,
            (o, v) => o.UpperValue = v);

    public double UpperValue
    {
        get => _upperValue;
        set => SetAndRaise(UpperValueProperty, ref _upperValue, CoerceValue(value));
    }

    #endregion



    // Метод приведения значений в диапазон 
    private double CoerceValue(double value) => Math.Max(Minimum, Math.Min(Maximum, value));


}