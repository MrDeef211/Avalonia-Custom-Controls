using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Common.Controls.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using ReactiveUI;

namespace Common.Controls;

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
            ImageScalingProperty);
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

    public bool HighlightSector
    {
        get => GetValue(HighlightSectorProperty);
        set => SetValue(HighlightSectorProperty, value);
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

        if (distance > outerR * 1.05 || distance < innerR) return null;

        double angleRad = Math.Atan2(dy, dx);
        double angleDeg = (angleRad * 180 / Math.PI + 360.0) % 360.0;

        for (int i = 0; i < _sectors.Count; i++)
        {
            var sec = _sectors[i];
            // Нормализуем, если меньше нуля
            // Нужно из за около костыльного сдвига угла на 90 градусов
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

        double startRad = startDeg * Math.PI / 180;
        double sweepRad = sweepDeg * Math.PI / 180;
        double endRad = startRad + sweepRad;

        Point offset = new Point();
        if (isHighlighted)
        {
            double midRad = startRad + sweepRad / 2;
            double shift = outerR * 0.05;
            offset = new Point(Math.Cos(midRad) * shift, Math.Sin(midRad) * shift);
        }

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            bool isLargeArc = sweepDeg > 180;

            if (innerR <= 0)
            {
                // Обычный сектор
                context.BeginFigure(center + offset, true);
                context.LineTo(new Point(
                    center.X + outerR * Math.Cos(startRad) + offset.X,
                    center.Y + outerR * Math.Sin(startRad) + offset.Y));
                context.ArcTo(
                    new Point(center.X + outerR * Math.Cos(endRad) + offset.X,
                              center.Y + outerR * Math.Sin(endRad) + offset.Y),
                    new Size(outerR, outerR), 0, isLargeArc, SweepDirection.Clockwise);
            }
            else
            {
                // Кольцевой сектор
                Point outerStart = new Point(center.X + outerR * Math.Cos(startRad) + offset.X,
                                             center.Y + outerR * Math.Sin(startRad) + offset.Y);
                Point outerEnd = new Point(center.X + outerR * Math.Cos(endRad) + offset.X,
                                           center.Y + outerR * Math.Sin(endRad) + offset.Y);
                Point innerStart = new Point(center.X + innerR * Math.Cos(startRad) + offset.X,
                                             center.Y + innerR * Math.Sin(startRad) + offset.Y);
                Point innerEnd = new Point(center.X + innerR * Math.Cos(endRad) + offset.X,
                                           center.Y + innerR * Math.Sin(endRad) + offset.Y);

                context.BeginFigure(outerStart, true);
                context.ArcTo(outerEnd, new Size(outerR, outerR), 0, isLargeArc, SweepDirection.Clockwise);
                context.LineTo(innerEnd);
                context.ArcTo(innerStart, new Size(innerR, innerR), 0, isLargeArc, SweepDirection.CounterClockwise);
            }

            context.EndFigure(true);
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

        var typeface = new Typeface("Arial");
        var formatted = new FormattedText(labelText, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, typeface, 12, Brushes.Black);

        double shift = isHighlighted ? outerR * 0.05 : 0;
        double midRad = (sector.StartAngle + sector.SweepAngle / 2) * Math.PI / 180;
        Point offset = new Point(Math.Cos(midRad) * shift, Math.Sin(midRad) * shift);

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
    // Асимитриное растяжение
    Fill,

    // По длинной стороне (изображение целиком)
    Contain,

    // По короткой стороне (заполнить целиком)
    Cover
}
