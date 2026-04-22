using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Controls.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

namespace Controls;

public class PieChart : TemplatedControl
{
    // индекс сектора под курсором
    private int? _hoverSectorIndex = null;
    // кэшированные данные секторов
    private List<SectorData> _sectors = new();

    #region Styled Property

    public static readonly StyledProperty<PieChartDataBase> ContentProperty =
        AvaloniaProperty.Register<PieChart, PieChartDataBase>(nameof(Content), new PieChartDataBase());

    public static readonly StyledProperty<IList<IBrush>> SectorColorsProperty =
        AvaloniaProperty.Register<PieChart, IList<IBrush>>(nameof(SectorColors), new List<IBrush>());

    public static readonly StyledProperty<bool> HighlightSectorProperty =
        AvaloniaProperty.Register<PieChart, bool>(nameof(HighlightSector), true);

    public static readonly StyledProperty<HighlightType> HighlightTypeProperty =
        AvaloniaProperty.Register<PieChart, HighlightType>(nameof(HighlightType), HighlightType.Push);

    public static readonly StyledProperty<bool> ShowLabelsProperty =
        AvaloniaProperty.Register<PieChart, bool>(nameof(ShowLabels), true);

    public static readonly StyledProperty<LabelPlacement> LabelPlacementProperty =
        AvaloniaProperty.Register<PieChart, LabelPlacement>(nameof(LabelPlacement), LabelPlacement.Outside);

    public static readonly StyledProperty<bool> ShowPercentagesProperty =
        AvaloniaProperty.Register<PieChart, bool>(nameof(ShowPercentages), true);

    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(InnerRadius), 0);

    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(StartAngle), 0);

    public static readonly StyledProperty<bool> PaintHoleProperty =
        AvaloniaProperty.Register<PieChart, bool>(nameof(PaintHole), false);

    public static readonly StyledProperty<IBrush> HoleColorProperty =
        AvaloniaProperty.Register<PieChart, IBrush>(nameof(HoleColor), Brushes.White);

    public static readonly StyledProperty<IImage> CenterImageProperty =
        AvaloniaProperty.Register<PieChart, IImage>(nameof(CenterImage), null);

    public static readonly StyledProperty<double> ImageZoomProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(ImageZoom), 100);

    public static readonly StyledProperty<ImageScaling> ImageScalingProperty =
        AvaloniaProperty.Register<PieChart, ImageScaling>(nameof(ImageScaling), ImageScaling.Fill);

    public static readonly StyledProperty<double> LabelFontSizeProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(LabelFontSize), 12.0);

    public static readonly StyledProperty<FontFamily> LabelFontFamilyProperty =
        AvaloniaProperty.Register<PieChart, FontFamily>(nameof(LabelFontFamily), FontFamily.Default);

    public static readonly StyledProperty<FontWeight> LabelFontWeightProperty =
        AvaloniaProperty.Register<PieChart, FontWeight>(nameof(LabelFontWeight), FontWeight.Normal);

    public static readonly StyledProperty<FontStyle> LabelFontStyleProperty =
        AvaloniaProperty.Register<PieChart, FontStyle>(nameof(LabelFontStyle), FontStyle.Normal);

    public static readonly StyledProperty<double> SizeShiftProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(SizeShift), 0.05);

    public static readonly StyledProperty<ICommand?> ClickCommandProperty =
        AvaloniaProperty.Register<PieChart, ICommand?>(nameof(ClickCommand));

    #endregion

    static PieChart()
    {
        AffectsRender<PieChart>(
            ContentProperty,
            SectorColorsProperty,
            HighlightSectorProperty,
            ShowLabelsProperty,
            LabelPlacementProperty,
            ShowPercentagesProperty,
            InnerRadiusProperty,
            StartAngleProperty,
            HoleColorProperty,
            CenterImageProperty,
            ImageZoomProperty,
            ImageScalingProperty,
            LabelFontSizeProperty,
            LabelFontFamilyProperty,
            LabelFontWeightProperty,
            LabelFontStyleProperty,
            SizeShiftProperty);
    }

    #region Свойства

    /// <summary>
    /// Объект базы данных для диаграммы
    /// </summary>
    public PieChartDataBase Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Список цветов
    /// </summary>
    public IList<IBrush> SectorColors
    {
        get => GetValue(SectorColorsProperty);
        set => SetValue(SectorColorsProperty, value);
    }

    /// <summary>
    /// Выделять сектор указанный курсором
    /// </summary>
    public bool HighlightSector
    {
        get => GetValue(HighlightSectorProperty);
        set => SetValue(HighlightSectorProperty, value);
    }

    /// <summary>
    /// Способ выделения сектора
    /// </summary>
    public HighlightType HighlightType
    {
        get => GetValue(HighlightTypeProperty);
        set => SetValue(HighlightTypeProperty, value);
    }

    /// <summary>
    /// Подсвечивать сектор при наведении
    /// </summary>
    public bool ShowLabels
    {
        get => GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    /// <summary>
    /// Расположение подписей секторов
    /// </summary>
    public LabelPlacement LabelPlacement
    {
        get => GetValue(LabelPlacementProperty);
        set => SetValue(LabelPlacementProperty, value);
    }

    /// <summary>
    /// Показывать проценты сектора
    /// </summary>
    public bool ShowPercentages
    {
        get => GetValue(ShowPercentagesProperty);
        set => SetValue(ShowPercentagesProperty, value);
    }

    /// <summary>
    /// Внутренний радиус (0 = обычная круговая, 1 = полностью открытый центр)
    /// </summary>
    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set
        {
            if (value < 0 || value > 1)
            {
                throw new ArgumentOutOfRangeException("InnerRadius дожен входить в диапазон от 0 до 1");
            }

            SetValue(InnerRadiusProperty, value);
        } 
    }

    /// <summary>
    /// Начальный угол для рапределения секторов
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set
        {
            if (value < 0 || value > 360)
            {
                throw new ArgumentOutOfRangeException("StartAngle дожен входить в диапазон от 0 до 360");
            }                
            SetValue(StartAngleProperty, value);
        }
    }

    /// <summary>
    /// Закрашивать центр отдельным цветом
    /// </summary>
    public bool PaintHole
    {
        get => GetValue(PaintHoleProperty);
        set => SetValue(PaintHoleProperty, value);
    }

    /// <summary>
    /// Цвет центрального отверстия
    /// </summary>
    public IBrush HoleColor
    {
        get => GetValue(HoleColorProperty);
        set => SetValue(HoleColorProperty, value);
    }

    /// <summary>
    /// Изображение в центре отверсия
    /// </summary>
    public IImage CenterImage
    {
        get => GetValue(CenterImageProperty);
        set => SetValue(CenterImageProperty, value);
    }

    /// <summary>
    /// Относительный размер изображения в процентах. По умолчанию растягивается по радиусу внутреннего отверстия
    /// </summary>
    public double ImageZoom
    {
        get => GetValue(ImageZoomProperty);
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("ImageZoom дожен быть больше 0");
            }

            SetValue(ImageZoomProperty, value);
        }
    }

    /// <summary>
    /// Способ масштабирования изображения в центре
    /// </summary>
    public ImageScaling ImageScaling
    {
        get => GetValue(ImageScalingProperty);
        set => SetValue(ImageScalingProperty, value);
    }

    /// <summary>
    /// Размер шрифта подписей   
    /// </summary>
    public double LabelFontSize
    {
        get => GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public FontFamily LabelFontFamily
    {
        get => GetValue(LabelFontFamilyProperty);
        set => SetValue(LabelFontFamilyProperty, value);
    }

    /// <summary>
    /// Тип шрифта подписей   
    /// </summary>
    public FontWeight LabelFontWeight
    {
        get => GetValue(LabelFontWeightProperty);
        set => SetValue(LabelFontWeightProperty, value);
    }

    /// <summary>
    /// Стиль шрифта подписей   
    /// </summary>
    public FontStyle LabelFontStyle
    {
        get => GetValue(LabelFontStyleProperty);
        set => SetValue(LabelFontStyleProperty, value);
    }

    public double SizeShift
    {
        get => GetValue(SizeShiftProperty);
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("SizeShift дожен быть больше 0");
            }

            SetValue(SizeShiftProperty, value);
        }
    }

    /// <summary>
    /// Команда, выполняемая после клика на выделенный сектор
    /// </summary>
    /// <remarks>
    /// Параметр обьект сектора из словаря <string, double>.
    /// </remarks>
    public ICommand? ClickCommand
    {
        get => GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    #endregion

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!HighlightSector) return;
        var pos = e.GetPosition(this);
        int? hitIndex = HitTestSector(pos);
        if (_hoverSectorIndex != hitIndex)
        {
            _hoverSectorIndex = hitIndex;
            InvalidateVisual();
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_hoverSectorIndex != null)
        {
            _hoverSectorIndex = null;
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        bool isLeft = props.IsLeftButtonPressed;
        if (HighlightSector && _hoverSectorIndex != null && isLeft)
        {
            var sector = _sectors[_hoverSectorIndex ?? 0];
            ClickCommand.Execute((sector.Name, sector.Value));
            e.Handled = true;
        }
        base.OnPointerPressed(e);
    }

    /// <summary>
    /// Определение сектора по координатам мыши
    /// </summary>
    private int? HitTestSector(Point point)
    {
        if (_sectors.Count == 0) return null;
        var center = new Point((Bounds.Width + Padding.Left - Padding.Right) / 2, (Bounds.Height + Padding.Top - Padding.Bottom) / 2);
        double size = Math.Min(Bounds.Width - Padding.Left - Padding.Right, Bounds.Height - Padding.Top - Padding.Bottom);
        double dx = point.X - center.X;
        double dy = point.Y - center.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double outerR = size / 2;
        double innerR = (InnerRadius) * outerR;

        double shift = HighlightType == HighlightType.Decrease || HighlightType == HighlightType.Reduce ? 0 : SizeShift;

        if (distance > outerR * (1 + shift) || distance < innerR) return null;

        double angleRad = Math.Atan2(dy, dx);
        double angleDeg = (angleRad * 180 / Math.PI + 360.0) % 360.0;

        for (int i = 0; i < _sectors.Count; i++)
        {
            var sec = _sectors[i];
            // Нормализуем, если меньше нуля
            // Нужно из-за около костыльного сдвига угла на 90 градусов
            double start = sec.StartAngle < 0 ? (sec.StartAngle + 360) % 360.0 : sec.StartAngle % 360.0;
            double end = (start + sec.SweepAngle) % 360.0;
            if (sec.SweepAngle > 0)
            {
                if (start < end)
                {
                    if (angleDeg >= start && angleDeg <= end) return i;
                }
                else
                {
                    if (angleDeg >= start || angleDeg <= end) return i;
                }
            }
        }
        return null;
    }


    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;

        UpdateSectors();

        context.FillRectangle(Background ?? Brushes.White, new Rect(Bounds.Size));
        // Позиция графика (позиция колизии вычисляется отдельно)
        var center = new Point((Bounds.Width + Padding.Left - Padding.Right) / 2, (Bounds.Height + Padding.Top - Padding.Bottom) / 2);
        double size = Math.Min(Bounds.Width - Padding.Left - Padding.Right, Bounds.Height - Padding.Top - Padding.Bottom);
        double outerRadius = size / 2;
        double innerRadiusPx = (InnerRadius) * outerRadius;

        // Фон
        if (PaintHole && innerRadiusPx > 0)
        {
            context.DrawEllipse(HoleColor, null, center, innerRadiusPx, innerRadiusPx);
        }

        // Центральное изображение
        if (CenterImage != null && innerRadiusPx > 0)
        {
            DrawImage(context, center, innerRadiusPx);
        }

        // Сектора
        for (int i = 0; i < _sectors.Count; i++)
        {
            var sec = _sectors[i];
            bool isHighlighted = HighlightSector && _hoverSectorIndex == i;            
            DrawSector(context, center, outerRadius, innerRadiusPx, sec.StartAngle, sec.SweepAngle, sec.Brush, isHighlighted);
        }

        // Подписи
        if (ShowLabels)
        {
            for (int i = 0; i < _sectors.Count; i++)
            {
                var sec = _sectors[i];
                bool isHighlighted = HighlightSector && _hoverSectorIndex == i;
                DrawLabel(context, sec, center, outerRadius, innerRadiusPx, isHighlighted);
            }
        }
    }

    #region Вспомогательные методы отрисовки

    /// <summary>
    /// Отрисовка одного сектора
    /// </summary>
    private void DrawSector(DrawingContext dc, Point center, double outerR, double innerR,
    double startDeg, double sweepDeg, IBrush brush, bool isHighlighted)
    {
        if (sweepDeg <= 0) return;

        var (offset, effectiveOuterR, effectiveInnerR, effectiveStartDeg, effectiveSweepDeg)
            = GetHighlightTransform(center.X, center.Y, outerR, innerR, startDeg, sweepDeg, isHighlighted);

        double startRad = effectiveStartDeg * Math.PI / 180;
        double sweepRad = effectiveSweepDeg * Math.PI / 180;
        double endRad = startRad + sweepRad;

        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            bool isLargeArc = effectiveSweepDeg > 180;

            if (effectiveInnerR <= 0)
            {
                ctx.BeginFigure(center + offset, true);
                ctx.LineTo(new Point(
                    center.X + effectiveOuterR * Math.Cos(startRad) + offset.X,
                    center.Y + effectiveOuterR * Math.Sin(startRad) + offset.Y));
                ctx.ArcTo(
                    new Point(center.X + effectiveOuterR * Math.Cos(endRad) + offset.X,
                              center.Y + effectiveOuterR * Math.Sin(endRad) + offset.Y),
                    new Size(effectiveOuterR, effectiveOuterR), 0, isLargeArc, SweepDirection.Clockwise);
            }
            else
            {
                Point outerStart = new Point(center.X + effectiveOuterR * Math.Cos(startRad) + offset.X,
                                             center.Y + effectiveOuterR * Math.Sin(startRad) + offset.Y);
                Point outerEnd = new Point(center.X + effectiveOuterR * Math.Cos(endRad) + offset.X,
                                           center.Y + effectiveOuterR * Math.Sin(endRad) + offset.Y);
                Point innerStart = new Point(center.X + effectiveInnerR * Math.Cos(startRad) + offset.X,
                                             center.Y + effectiveInnerR * Math.Sin(startRad) + offset.Y);
                Point innerEnd = new Point(center.X + effectiveInnerR * Math.Cos(endRad) + offset.X,
                                           center.Y + effectiveInnerR * Math.Sin(endRad) + offset.Y);

                ctx.BeginFigure(outerStart, true);
                ctx.ArcTo(outerEnd, new Size(effectiveOuterR, effectiveOuterR), 0, isLargeArc, SweepDirection.Clockwise);
                ctx.LineTo(innerEnd);
                ctx.ArcTo(innerStart, new Size(effectiveInnerR, effectiveInnerR), 0, isLargeArc, SweepDirection.CounterClockwise);
            }

            ctx.EndFigure(true);
        }

        dc.DrawGeometry(brush, null, geometry);
    }

    private void DrawImage(DrawingContext context, Point center, double innerRadiusPx)
    {
        // Исходный размер изображения
        Size sourceSize = CenterImage.Size;
        double imgWidth = sourceSize.Width;
        double imgHeight = sourceSize.Height;

        // Целевой диаметр
        double targetDiameter = innerRadiusPx * 2;

        // Коэффициенты масштабирования
        double scaleX = targetDiameter / imgWidth;
        double scaleY = targetDiameter / imgHeight;

        double finalWidth = targetDiameter;
        double finalHeight = targetDiameter;

        // Скейлинг
        switch (ImageScaling)
        {
            case ImageScaling.Fill:
                break;

            case ImageScaling.Contain:
                double scaleContain = Math.Min(scaleX, scaleY);
                finalWidth = imgWidth * scaleContain;
                finalHeight = imgHeight * scaleContain;
                break;

            case ImageScaling.Cover:
                double scaleCover = Math.Max(scaleX, scaleY);
                finalWidth = imgWidth * scaleCover;
                finalHeight = imgHeight * scaleCover;
                break;
        }

        finalWidth *= ImageZoom;
        finalHeight *= ImageZoom;

        var imgRect = new Rect(
            center.X - finalWidth / 2,
            center.Y - finalHeight / 2,
            finalWidth,
            finalHeight);

        var clipRect = new Rect(center.X - innerRadiusPx, center.Y - innerRadiusPx, targetDiameter, targetDiameter);
        var clipGeometry = new EllipseGeometry(clipRect);

        using (context.PushGeometryClip(clipGeometry))
        {
            context.DrawImage(CenterImage, imgRect);
        }
    }

    /// <summary>
    /// Отрисовка подписи в зависимости от LabelPlacement
    /// </summary>
    private void DrawLabel(DrawingContext dc, SectorData sector, Point center, double outerR, double innerR, bool isHighlighted)
    {
        double midAngleDeg = sector.StartAngle + sector.SweepAngle / 2;
        double midAngleRad = midAngleDeg * Math.PI / 180;
        string labelText = ShowPercentages
            ? $"{sector.Name} ({sector.Percentage:F1}%)"
            : sector.Name;

        var typeface = new Typeface(LabelFontFamily, LabelFontStyle, LabelFontWeight);
        var formatted = new FormattedText(labelText, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, typeface, LabelFontSize, Brushes.Black);

        double pushFactor = SizeShift;
        double shift = isHighlighted ? outerR * pushFactor : 0;
        double midRad = (sector.StartAngle + sector.SweepAngle / 2) * Math.PI / 180;

        Point offset;
        switch (HighlightType)
        {
            case HighlightType.Decrease:
                offset = new Point(Math.Cos(midRad) * -shift, Math.Sin(midRad) * -shift);
                break;
            case HighlightType.Reduce:
                if ((1 - InnerRadius) / 2 < pushFactor * 2)
                {
                    shift = isHighlighted ? outerR * (1 - InnerRadius) / 4 : 0;
                }
                offset = new Point(Math.Cos(midRad) * -shift, Math.Sin(midRad) * -shift);
                break;
            default:
                offset = new Point(Math.Cos(midRad) * shift, Math.Sin(midRad) * shift);
                break;
        }

        formatted.TextAlignment = TextAlignment.Left;

        if (LabelPlacement == LabelPlacement.Inside)
        {
            double labelRadius = (outerR + innerR) / 2;
            if (innerR <= 0) labelRadius = outerR * 0.7;
            double x = center.X + labelRadius * Math.Cos(midAngleRad) + offset.X - formatted.Width / 2;
            double y = center.Y + labelRadius * Math.Sin(midAngleRad) + offset.Y - formatted.Height / 2;
            dc.DrawText(formatted, new Point(x, y));
        }
        else if (LabelPlacement == LabelPlacement.Outside)
        {
            double x = center.X + (outerR + 5) * Math.Cos(midAngleRad) + offset.X;
            double y = center.Y + (outerR + 5) * Math.Sin(midAngleRad) + offset.Y;
            double offsetX = (midAngleRad > Math.PI / 2 && midAngleRad < 3 * Math.PI / 2) ? -formatted.Width : 0;
            dc.DrawText(formatted, new Point(x + offsetX, y - formatted.Height / 2));
        }
        else // Callout
        {
            double lineEndX = center.X + (outerR + 8) * Math.Cos(midAngleRad) + offset.X;
            double lineEndY = center.Y + (outerR + 8) * Math.Sin(midAngleRad) + offset.Y;
            double startX = center.X + outerR * Math.Cos(midAngleRad) + offset.X;
            double startY = center.Y + outerR * Math.Sin(midAngleRad) + offset.Y;
            double textX = lineEndX + offset.X + (midAngleRad > Math.PI / 2 && midAngleRad < 3 * Math.PI / 2 ? -formatted.Width - 5 : 5);
            double textY = lineEndY + offset.Y - formatted.Height / 2;

            var pen = new Pen(Brushes.Gray, 1);
            dc.DrawLine(pen, new Point(startX, startY), new Point(lineEndX, lineEndY));
            dc.DrawEllipse(Brushes.Gray, null, new Point(lineEndX, lineEndY), 2, 2);
            dc.DrawText(formatted, new Point(textX, textY));
        }
    }

    /// <summary>
    /// Рассчитывает модифицированные параметры для отрисовки выделенного сектора.
    /// </summary>
    private (Point offset, double outerRadius, double innerRadius, double startAngle, double sweepAngle)
        GetHighlightTransform(double centerX, double centerY, double originalOuterR, double originalInnerR,
                              double originalStartDeg, double originalSweepDeg, bool isHighlighted)
    {
        if (!isHighlighted || !HighlightSector)
            return (new Point(0, 0), originalOuterR, originalInnerR, originalStartDeg, originalSweepDeg);

        double pushFactor = SizeShift;      
        double reduceAngleFactor = SizeShift; 

        double midRad = (originalStartDeg + originalSweepDeg / 2) * Math.PI / 180;
        double shift = originalOuterR * pushFactor;
        Point offset = new Point(0, 0);

        double newOuterR = originalOuterR;
        double newInnerR = originalInnerR;
        double newStartDeg = originalStartDeg;
        double newSweepDeg = originalSweepDeg;

        switch (HighlightType)
        {
            case HighlightType.Push:
                // только смещение
                offset = new Point(Math.Cos(midRad) * shift, Math.Sin(midRad) * shift);
                break;

            case HighlightType.Increase:
                // увеличение внешнего радиуса, без смещения
                newOuterR = originalOuterR * (1 + pushFactor);
                break;

            case HighlightType.IncreaseOut:
                // смещение + увеличение внешнего радиуса
                newInnerR = originalInnerR * (1 + pushFactor * 0.5);
                offset = new Point(Math.Cos(midRad) * shift * 0.5, Math.Sin(midRad) * shift * 0.5);
                newOuterR = originalOuterR * (1 + pushFactor * 0.5);
                break;

            case HighlightType.Decrease:
                // уменьшение внешнего радиуса, без смещения
                newOuterR = Math.Max(originalOuterR * (1 - pushFactor), originalInnerR + 1);
                break;

            case HighlightType.Reduce:
                // пропорциональное уменьшение
                if ((1 - InnerRadius) / 2 < pushFactor * 2)
                    shift = originalOuterR * (1 - InnerRadius) / 4;

                offset = new Point(Math.Cos(midRad) * shift, Math.Sin(midRad) * shift);
                double ScalOffset = Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y);
                newOuterR = Math.Max(originalOuterR * (1 - pushFactor) - ScalOffset, (originalOuterR + originalInnerR) / 2);

                if (InnerRadius < 0.1 || InnerRadius > 0.75)
                    newInnerR = Math.Min(originalInnerR * (1 + pushFactor * 0.3), (originalOuterR + 2 * originalInnerR) / 3);
                else
                    newInnerR = Math.Min(originalInnerR + originalOuterR * pushFactor * 0.5, (originalOuterR + 2 * originalInnerR) / 3);

                double angleDelta = originalSweepDeg * reduceAngleFactor / 2;

                newStartDeg = originalStartDeg + angleDelta;
                newSweepDeg = originalSweepDeg - 2 * angleDelta;
                if (newSweepDeg <= 0) newSweepDeg = 0.1; 
                break;
        }

        return (offset, newOuterR, newInnerR, newStartDeg, newSweepDeg);
    }

    #endregion

    /// <summary>
    /// Пересчёт углов и кистей для секторов
    /// </summary>
    private void UpdateSectors()
    {
        _sectors.Clear();
        if (Content?.Chart == null || Content.Chart.Count == 0) return;
        var total = Content.Sum();
        if (total <= 0) return;

        double currentAngle = StartAngle;
        var colors = SectorColors ?? new List<IBrush>();
        var defaultColors = GetDefaultColors();
        int idx = 0;
        foreach (var pair in Content.Chart)
        {
            double value = pair.Value;
            double sweep = (value / total) * 360.0;
            var brush = idx < colors.Count ? colors[idx] : defaultColors[idx % defaultColors.Length];
            _sectors.Add(new SectorData
            {
                Name = pair.Key,
                Value = value,
                // Беру - 90, чтобы отсчёт шёл сверху, а не с тригонометрического нуля
                // Переписывать логику отрисовки (и колизии) будет довольно сложно, т.к Cos и Sin для X и Y превратятся в -Sin и Cos
                // Если их везде начать менять по любому что-то сломается, плюс не так интуитивно будет в будущем
                StartAngle = currentAngle - 90,
                SweepAngle = sweep,
                Brush = brush,
                Total = total
            });
            currentAngle += sweep;
            idx++;
        }
    }

    /// <summary>
    /// Цвета по умолчанию
    /// </summary>
    /// <returns></returns>
    private IBrush[] GetDefaultColors()
    {
        return new IBrush[]
        {
                Brushes.DodgerBlue,
                Brushes.OrangeRed,
                Brushes.Gold,
                Brushes.MediumSeaGreen,
                Brushes.MediumPurple,
                Brushes.HotPink,
                Brushes.Teal,
                Brushes.Coral
        };
    }


}

public enum LabelPlacement
{
    // Внутри
    Inside,

    // Снаружи
    Outside,

    // Выноска
    Callout
}

public enum ImageScaling
{
    /// <summary>
    /// Асимитриное растяжение
    /// </summary>
    Fill,

    /// <summary>
    /// По длинной стороне (изображение целиком)
    /// </summary>
    Contain,

    /// <summary>
    /// По короткой стороне (заполнить целиком)
    /// </summary>
    Cover
}

public enum HighlightType
{
    /// <summary>
    /// Выталкивать без изменения формы
    /// </summary>
    Push,

    /// <summary>
    /// Увеличить радиус без выталкивания
    /// </summary>
    Increase,

    /// <summary>
    /// Увеличить с выталкиванием
    /// </summary>
    IncreaseOut,

    /// <summary>
    /// Уменьшить только радиус
    /// </summary>
    Decrease,

    /// <summary>
    /// Уменьшить пропорционально
    /// </summary>
    Reduce
}
