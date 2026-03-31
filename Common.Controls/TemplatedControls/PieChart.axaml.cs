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
        AvaloniaProperty.Register<PieChart, double>(nameof(InnerRadius), 0.0);

    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<PieChart, double>(nameof(StartAngle), -90.0);

    public static readonly StyledProperty<IBrush> HoleColorProperty =
        AvaloniaProperty.Register<PieChart, IBrush>(nameof(HoleColor), Brushes.White);

    public static readonly StyledProperty<IImage> CenterImageProperty =
        AvaloniaProperty.Register<PieChart, IImage>(nameof(CenterImage), null);

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
            CenterImageProperty);
    }

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
    /// Внутренний радиус в процентах (0 = обычная круговая, 100 = полностью открытый центр)
    /// </summary>
    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    /// <summary>
    /// Начальный угол для рапределения секторов
    /// </summary>
    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
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
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        double dx = point.X - center.X;
        double dy = point.Y - center.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double size = Math.Min(Bounds.Width, Bounds.Height);
        double outerR = size / 2;
        double innerR = (InnerRadius / 100.0) * outerR;

        if (distance > outerR || distance < innerR) return null;

        double angleRad = Math.Atan2(dy, dx);
        double angleDeg = (angleRad * 180 / Math.PI + 360.0) % 360.0;

        for (int i = 0; i < _sectors.Count; i++)
        {
            var sec = _sectors[i];
            double start = sec.StartAngle % 360.0;
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
        if (_sectors.Count == 0)
        {
            var text = new FormattedText("No data", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.Gray);
            context.DrawText(text, new Point(Bounds.Width / 2 - text.Width / 2, Bounds.Height / 2 - text.Height / 2));
            return;
        }

        double size = Math.Min(Bounds.Width, Bounds.Height);
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);
        double outerRadius = size / 2;
        double innerRadiusPx = (InnerRadius / 100.0) * outerRadius;

        for (int i = 0; i < _sectors.Count; i++)
        {
            var sec = _sectors[i];
            bool isHighlighted = HighlightSector && _hoverSectorIndex == i;
            DrawSector(context, center, outerRadius, innerRadiusPx, sec.StartAngle, sec.SweepAngle, sec.Brush, isHighlighted);
        }

        if (innerRadiusPx > 0)
        {
            context.DrawEllipse(HoleColor, null, center, innerRadiusPx, innerRadiusPx);
            if (CenterImage != null)
            {
                double imgSize = innerRadiusPx * 1.6;
                var imgRect = new Rect(center.X - imgSize / 2, center.Y - imgSize / 2, imgSize, imgSize);
                context.DrawImage(CenterImage, imgRect);
            }
        }

        if (ShowLabels)
        {
            foreach (var sec in _sectors)
                DrawLabel(context, sec, center, outerRadius, innerRadiusPx);
        }
    }

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

    /// <summary>
    /// Отрисовка подписи в зависимости от LabelPlacement
    /// </summary>
    private void DrawLabel(DrawingContext dc, SectorData sector, Point center, double outerR, double innerR)
    {
        double midAngleDeg = sector.StartAngle + sector.SweepAngle / 2;
        double midAngleRad = midAngleDeg * Math.PI / 180;
        string labelText = ShowPercentages
            ? $"{sector.Name} ({sector.Percentage:F1}%)"
            : sector.Name;

        var typeface = new Typeface("Arial");
        var formatted = new FormattedText(labelText, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, typeface, 12, Brushes.Black);

        formatted.TextAlignment = TextAlignment.Left;

        if (LabelPlacement == LabelPlacement.Inside)
        {
            double labelRadius = (outerR + innerR) / 2;
            if (innerR <= 0) labelRadius = outerR * 0.7;
            double x = center.X + labelRadius * Math.Cos(midAngleRad) - formatted.Width / 2;
            double y = center.Y + labelRadius * Math.Sin(midAngleRad) - formatted.Height / 2;
            dc.DrawText(formatted, new Point(x, y));
        }
        else if (LabelPlacement == LabelPlacement.Outside)
        {
            double x = center.X + (outerR + 5) * Math.Cos(midAngleRad);
            double y = center.Y + (outerR + 5) * Math.Sin(midAngleRad);
            double offsetX = (midAngleRad > Math.PI / 2 && midAngleRad < 3 * Math.PI / 2) ? -formatted.Width : 0;
            dc.DrawText(formatted, new Point(x + offsetX, y - formatted.Height / 2));
        }
        else // Callout
        {
            double lineEndX = center.X + (outerR + 8) * Math.Cos(midAngleRad);
            double lineEndY = center.Y + (outerR + 8) * Math.Sin(midAngleRad);
            double startX = center.X + outerR * Math.Cos(midAngleRad);
            double startY = center.Y + outerR * Math.Sin(midAngleRad);
            double textX = lineEndX + (midAngleRad > Math.PI / 2 && midAngleRad < 3 * Math.PI / 2 ? -formatted.Width - 5 : 5);
            double textY = lineEndY - formatted.Height / 2;

            var pen = new Pen(Brushes.Gray, 1);
            dc.DrawLine(pen, new Point(startX, startY), new Point(lineEndX, lineEndY));
            dc.DrawEllipse(Brushes.Gray, null, new Point(lineEndX, lineEndY), 2, 2);
            dc.DrawText(formatted, new Point(textX, textY));
        }
    }

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
                StartAngle = currentAngle,
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

internal class SectorData
{
    public string Name { get; set; }
    public double Value { get; set; }
    public double StartAngle { get; set; }   // в градусах
    public double SweepAngle { get; set; }   // в градусах
    public IBrush Brush { get; set; }
    public double Percentage => Value / Total * 100;
    public double Total { get; set; }
}
