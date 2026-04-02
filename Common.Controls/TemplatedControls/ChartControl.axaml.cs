using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;
using Common.Controls.Models;
using Common.Controls.TemplatedControls;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Common.Controls;

public class ChartControl : TemplatedControl
{

    // Константы отступов от внутренних краёв
    private const double PaddingLeft = 50;
    private const double PaddingRight = 30;
    private const double PaddingTop = 30;
    private const double PaddingBottom = 50;

    // Поля для кэширования координат области рисования
    private double _left;
    private double _right;
    private double _top;
    private double _bottom;
    private double _axisX;
    private double _axisY;

    #region Styled Property

    public static readonly StyledProperty<ChartDataBase> ContentProperty =
        AvaloniaProperty.Register<ChartControl, ChartDataBase>(nameof(Content), new());

    public static readonly StyledProperty<ChartStyle> ChartStyleProperty =
        AvaloniaProperty.Register<ChartControl, ChartStyle>(nameof(ChartStyle), ChartStyle.Simple);

    public static readonly StyledProperty<IBrush> ChartColorProperty =
        AvaloniaProperty.Register<ChartControl, IBrush>(nameof(ChartColor), Brushes.DodgerBlue);

    public static readonly StyledProperty<bool> FillProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(Fill), false);

    public static readonly StyledProperty<bool> HighlightPointsProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(HighlightPoints), true);

    public static readonly StyledProperty<bool> GridProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(Grid), true);

    public static readonly StyledProperty<IBrush> GridColorProperty =
        AvaloniaProperty.Register<ChartControl, IBrush>(nameof(GridColor), Brushes.LightGray);

    public static readonly StyledProperty<GridLength> GridSizeXProperty =
        AvaloniaProperty.Register<ChartControl, GridLength>(nameof(GridSizeX), GridLength.Auto);

    public static readonly StyledProperty<GridLength> GridSizeYProperty =
        AvaloniaProperty.Register<ChartControl, GridLength>(nameof(GridSizeY), GridLength.Auto);

    public static readonly StyledProperty<bool> AxisProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(Axis), true);

    public static readonly StyledProperty<IBrush> AxisColorProperty =
        AvaloniaProperty.Register<ChartControl, IBrush>(nameof(AxisColor), Brushes.Green);

    public static readonly StyledProperty<AxisLabelMode> LabelModeXProperty =
        AvaloniaProperty.Register<ChartControl, AxisLabelMode>(nameof(LabelModeX), AxisLabelMode.Grid);

    public static readonly StyledProperty<AxisLabelMode> LabelModeYProperty =
        AvaloniaProperty.Register<ChartControl, AxisLabelMode>(nameof(LabelModeY), AxisLabelMode.Grid);

    public static readonly StyledProperty<bool> ShowPointsLabelsProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(ShowPointsLabels), false);

    #endregion

    private Point? _mousePosition;

    static ChartControl()
    {
        AffectsRender<ChartControl>(
            ContentProperty, 
            ChartStyleProperty, 
            ChartColorProperty, 
            FillProperty,
            HighlightPointsProperty,
            GridProperty,
            GridColorProperty,
            GridSizeXProperty,
            GridSizeYProperty,
            AxisProperty,
            AxisColorProperty,
            LabelModeXProperty,
            LabelModeYProperty,
            ShowPointsLabelsProperty);
    }

    #region Свойства

    /// <summary>
    /// Массив точек для построения
    /// </summary>
    public ChartDataBase Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Стиль отрисовки соеденения точек
    /// </summary>
    public ChartStyle ChartStyle
    {
        get => GetValue(ChartStyleProperty);
        set => SetValue(ChartStyleProperty, value);
    }

    /// <summary>
    /// Цвет графика
    /// </summary>
    public IBrush ChartColor
    {
        get => GetValue(ChartColorProperty);
        set => SetValue(ChartColorProperty, value);
    }

    /// <summary>
    /// Заполнять область под графиком
    /// </summary>
    public bool Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// Выделение точек
    /// </summary>
    public bool HighlightPoints
    {
        get => GetValue(HighlightPointsProperty);
        set => SetValue(HighlightPointsProperty, value);
    }

    /// <summary>
    /// Рисовать сетку
    /// </summary>
    public bool Grid
    {
        get => GetValue(GridProperty);
        set => SetValue(GridProperty, value);
    }

    /// <summary>
    /// Цвет сетки
    /// </summary>
    public IBrush GridColor
    {
        get => GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    /// <summary>
    /// Размер сетки по X в еденицах измерения графика
    /// </summary>
    /// <remarks>
    /// Auto - автоматические целые значения, примерно 80 px между линиями; * - автоматически примерно 10 ячеек сетки
    /// </remarks>
    public GridLength GridSizeX
    {
        get => GetValue(GridSizeXProperty);
        set => SetValue(GridSizeXProperty, value);
    }

    /// <summary>
    /// Размер сетки по Y в еденицах измерения графика 
    /// </summary>
    /// <remarks>
    /// Auto - автоматические целые значения, примерно 80 px между линиями; * - автоматически примерно 10 ячеек сетки
    /// </remarks>
    public GridLength GridSizeY
    {
        get => GetValue(GridSizeYProperty);
        set => SetValue(GridSizeYProperty, value);
    }

    /// <summary>
    /// Рисовать оси координат
    /// </summary>
    public bool Axis
    {
        get => GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    /// <summary>
    /// Цвет осей координат
    /// </summary>
    public IBrush AxisColor
    {
        get => GetValue(AxisColorProperty);
        set => SetValue(AxisColorProperty, value);
    }

    /// <summary>
    /// Подписи по оси X
    /// </summary>
    public AxisLabelMode LabelModeX
    {
        get => GetValue(LabelModeXProperty);
        set => SetValue(LabelModeXProperty, value);
    }

    /// <summary>
    /// Подписи по оси Y
    /// </summary>
    public AxisLabelMode LabelModeY
    {
        get => GetValue(LabelModeYProperty);
        set => SetValue(LabelModeYProperty, value);
    }

    /// <summary>
    /// Подписи точек
    /// </summary>
    public bool ShowPointsLabels
    {
        get => GetValue(ShowPointsLabelsProperty);
        set => SetValue(ShowPointsLabelsProperty, value);
    }

    #endregion

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _mousePosition = e.GetPosition(this);
        InvalidateVisual();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _mousePosition = null;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Content == null || Content.Chart.Count < 2) return;

        var points = Content.Chart.OrderBy(x => x.Key).ToList();
        double minX = points.Min(p => p.Key);
        double maxX = points.Max(p => p.Key);
        double minY = points.Min(p => p.Value);
        double maxY = points.Max(p => p.Value);

        _left = PaddingLeft;
        _right = Bounds.Width - PaddingRight;
        _top = PaddingTop;
        _bottom = Bounds.Height - PaddingBottom;

        // Функция нормализации
        Point Normalize(double x, double y)
        {
            double availableWidth = Bounds.Width - PaddingLeft - PaddingRight;
            double availableHeight = Bounds.Height - PaddingTop - PaddingBottom;

            double px = PaddingLeft + (x - minX) / (maxX - minX) * availableWidth;
            double py = (PaddingTop + availableHeight) - ((y - minY) / (maxY - minY) * availableHeight);
            return new Point(px, py);
        }

        // Подготовка данных для сетки и осей
        List<double> xGridLines = null;
        List<double> yGridLines = null;
        List<double> xLabels = null;
        List<double> yLabels = null;

        

        if (Grid || Axis)
        {
            var origin = Normalize(0, 0);
            _axisX = Math.Clamp(origin.X, _left, _right);
            _axisY = Math.Clamp(origin.Y, _top, _bottom);

            double availableWidth = _right - _left;
            double availableHeight = _bottom - _top;

            xGridLines = GetGridValues(minX, maxX, GridSizeX, availableWidth, 200);
            yGridLines = GetGridValues(minY, maxY, GridSizeY, availableHeight, 100);
            xLabels = GetLabelValues(minX, maxX, LabelModeX, GridSizeX, availableWidth, points, false);
            yLabels = GetLabelValues(minY, maxY, LabelModeY, GridSizeY, availableHeight, points, true);
        }

        // 1. Фон
        context.FillRectangle(Background ?? Brushes.Transparent, new Rect(Bounds.Size));

        // 2. Сетка и оси (линии)
        DrawGridAndAxesLines(context, xGridLines, yGridLines, Normalize);

        // 3. Геометрия графика
        var screenPoints = points.Select(p => Normalize(p.Key, p.Value)).ToList();
        DrawChartGeometry(context, screenPoints);

        // 4. Подписи осей
        if (Axis && xLabels != null && yLabels != null)
        {
            DrawAxesTicksAndLabels(context, xLabels, yLabels, points, Normalize);
        }

        // 5. Точки и подписи
        if (HighlightPoints)
        {
            DrawPointsAndLabels(context, points, Normalize);
        }

        // 6. Подсказка мыши
        if (_mousePosition.HasValue)
        {
            DrawTooltip(context, points, screenPoints);
        }
    }

    #region Вспомогательные методы отрисовки

    /// <summary>
    /// Отрисовывает сетку и линии осей (без текстовых подписей).
    /// </summary>
    private void DrawGridAndAxesLines(DrawingContext context, List<double> xGridLines, List<double> yGridLines,
    Func<double, double, Point> normalize)
    {
        if (Grid && xGridLines != null && yGridLines != null)
        {
            var gridPen = new Pen(GridColor, 0.5);
            foreach (var val in xGridLines)
            {
                var p = normalize(val, 0);
                context.DrawLine(gridPen, new Point(p.X, _top), new Point(p.X, _bottom));
            }
            foreach (var val in yGridLines)
            {
                var p = normalize(0, val);
                context.DrawLine(gridPen, new Point(_left, p.Y), new Point(_right, p.Y));
            }
        }

        if (Axis)
        {
            var axisPen = new Pen(AxisColor, 1);
            context.DrawLine(axisPen, new(_axisX, _top), new(_axisX, _bottom));
            context.DrawLine(axisPen, new(_left, _axisY), new(_right, _axisY));
        }
    }

    /// <summary>
    /// Строит геометрию графика (линию, ступеньку, сплайн) и отрисовывает её.
    /// </summary>
    private void DrawChartGeometry(DrawingContext context, List<Point> screenPoints)
    {
        var pen = new Pen(ChartColor, 2);
        var geometry = new StreamGeometry();

        using (var sgc = geometry.Open())
        {
            double bottomY = Bounds.Height;

            sgc.BeginFigure(Fill ? new Point(screenPoints[0].X, bottomY) : screenPoints[0], Fill);
            if (Fill) sgc.LineTo(screenPoints[0]);

            switch (ChartStyle)
            {
                case ChartStyle.Line:
                    for (int i = 1; i < screenPoints.Count; i++)
                        sgc.LineTo(screenPoints[i]);
                    break;

                case ChartStyle.Step:
                    for (int i = 0; i < screenPoints.Count - 1; i++)
                    {
                        sgc.LineTo(new Point(screenPoints[i + 1].X, screenPoints[i].Y));
                        sgc.LineTo(screenPoints[i + 1]);
                    }
                    break;

                case ChartStyle.Spline:
                    for (int i = 0; i < screenPoints.Count - 1; i++)
                    {
                        var p1 = screenPoints[i];
                        var p2 = screenPoints[i + 1];
                        var p0 = (i > 0) ? screenPoints[i - 1] : p1;
                        var p3 = (i + 2 < screenPoints.Count) ? screenPoints[i + 2] : p2;

                        Point cp1 = p1 + (p2 - p0) / 6;
                        Point cp2 = p2 - (p3 - p1) / 6;

                        sgc.CubicBezierTo(cp1, cp2, p2);
                    }
                    break;
            }

            if (Fill && ChartStyle != ChartStyle.Simple)
            {
                sgc.LineTo(new Point(screenPoints.Last().X, bottomY));
                sgc.LineTo(new Point(screenPoints[0].X, bottomY));
            }
            sgc.EndFigure(Fill);
        }

        if (ChartStyle != ChartStyle.Simple)
        {
            context.DrawGeometry(Fill ? ChartColor : null, pen, geometry);
        }
        else
        {
            // Для стиля Simple рисуем точки или вертикальные линии
            foreach (var p in screenPoints)
            {
                if (Fill)
                    context.DrawLine(pen, p, new Point(p.X, Bounds.Height));
                else
                    context.DrawEllipse(ChartColor, pen, p, 1, 1);
            }
        }
    }

    /// <summary>
    /// Отрисовывает текстовые подписи осей.
    /// </summary>
    private void DrawAxesTicksAndLabels(DrawingContext context, List<double> xLabels, List<double> yLabels,
    List<KeyValuePair<double, double>> points, Func<double, double, Point> normalize)
    {
        var axisPen = new Pen(AxisColor, 1);

        // Засечки и подписи по X
        double angleX = xLabels.Count > 10 ? (xLabels.Count > 40 ? 90 : 45) : 0;
        foreach (var val in xLabels)
        {
            var p = normalize(val, 0);
            if (p.X < _left - 1 || p.X > _right + 1) continue;

            // Засечка
            context.DrawLine(axisPen, new Point(p.X, _axisY - 3), new Point(p.X, _axisY + 3));

            // Подпись
            DrawText(context, FormatNumber(val), new Point(p.X, _axisY + 15), angleX,
                angleX != 0 ? TextAlignment.Right : TextAlignment.Center);
        }

        // Засечки и подписи по Y
        var yVals = LabelModeY == AxisLabelMode.Points
            ? points.Select(p => p.Value).Distinct().ToList()
            : yLabels;

        foreach (var val in yVals)
        {
            var p = normalize(0, val);
            if (p.Y < _top - 1 || p.Y > _bottom + 1) continue;

            // Засечка
            context.DrawLine(axisPen, new Point(_axisX - 3, p.Y), new Point(_axisX + 3, p.Y));

            // Подпись
            DrawText(context, FormatNumber(val), new Point(_axisX - 20, p.Y), 0, TextAlignment.Right);
        }
    }


    /// <summary>
    /// Отрисовывает точки графика и их подписи (если включено).
    /// </summary>
    private void DrawPointsAndLabels(DrawingContext context, List<KeyValuePair<double, double>> points, Func<double, double, Point> normalize)
    {
        var pen = new Pen(ChartColor, 2);
        foreach (var point in points)
        {
            var nPoint = normalize(point.Key, point.Value);
            context.DrawEllipse(ChartColor, pen, nPoint, 2, 2);

            if (ShowPointsLabels)
            {
                DrawText(context, FormatNumber(point.Value), new Point(nPoint.X + 5, nPoint.Y - 15),
                    -45, TextAlignment.Center, ChartColor);
            }
        }
    }

    /// <summary>
    /// Отрисовывает всплывающую подсказку при наведении мыши.
    /// </summary>
    private void DrawTooltip(DrawingContext context, List<KeyValuePair<double, double>> points, List<Point> screenPoints)
    {
        double minDistance = double.MaxValue;
        Point? closestScreenPoint = null;
        KeyValuePair<double, double>? closestDataPoint = null;

        for (int i = 0; i < points.Count; i++)
        {
            var sp = screenPoints[i];
            double dist = Math.Sqrt(Math.Pow(sp.X - _mousePosition.Value.X, 2) +
                                    Math.Pow(sp.Y - _mousePosition.Value.Y, 2));

            if (dist < 30 && dist < minDistance)
            {
                minDistance = dist;
                closestScreenPoint = sp;
                closestDataPoint = points[i];
            }
        }

        if (closestScreenPoint.HasValue && closestDataPoint.HasValue)
        {
            var p = closestScreenPoint.Value;
            var data = closestDataPoint.Value;

            context.DrawEllipse(null, new Pen(ChartColor, 2), p, 5, 5);

            string tooltipText = $"X: {FormatNumber(data.Key)}\nY: {FormatNumber(data.Value)}";
            var ft = new FormattedText(
                tooltipText,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Typeface.Default,
                12,
                Brushes.Black);

            var rect = new Rect(p.X + 10, p.Y - 40, ft.Width + 10, ft.Height + 6);
            context.FillRectangle(new SolidColorBrush(Color.Parse("#CCFFFFFF")), rect);
            context.DrawRectangle(new Pen(Brushes.Gray, 1), rect);
            context.DrawText(ft, new Point(p.X + 15, p.Y - 37));
        }
    }

    /// <summary>
    /// Отрисовка текста с возможностью поворота и выравнивания.
    /// </summary>
    private void DrawText(DrawingContext context, string text, Point pos, double angle = 0,
        TextAlignment align = TextAlignment.Center, IBrush? brush = null)
    {
        var ft = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            10,
            brush ?? AxisColor);

        ft.TextAlignment = align;

        var transform = Matrix.CreateTranslation(pos.X, pos.Y);
        if (angle != 0)
        {
            transform = Matrix.CreateRotation(Math.PI * angle / 180) * transform;
        }

        using (context.PushTransform(transform))
        {
            double xOff = align == TextAlignment.Right ? -5 : (align == TextAlignment.Center ? -ft.Width / 2 : 5);
            context.DrawText(ft, new Point(xOff, -ft.Height / 2));
        }
    }

    #endregion

    #region Вычисление значений сетки и подписей

    /// <summary>
    /// Возвращает список значений для линий сетки с автоматическим или заданным шагом.
    /// </summary>
    private List<double> GetGridValues(double min, double max, GridLength size, double pixels, double autoPx)
    {
        var res = new List<double>();
        double range = max - min;
        if (range <= 0) return res;

        double step;
        if (size.IsAuto)
        {
            double rawStep = range / (pixels / autoPx);
            double mag = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            step = Math.Ceiling(rawStep / mag) * mag;
        }
        else if (size.IsStar)
        {
            step = range / 10.0;
        }
        else
        {
            step = size.Value;
        }

        if (step <= 0) return res;

        double start = Math.Floor(min / step) * step;
        for (double v = start; v <= max + step * 0.001; v += step)
        {
            if (v >= min - step * 0.001)
            {
                res.Add(v);
            }
        }
        return res;
    }

    /// <summary>
    /// Возвращает список значений для подписей осей в зависимости от режима.
    /// </summary>
    private List<double> GetLabelValues(double min, double max, AxisLabelMode mode, GridLength gSize,
        double pixels, List<KeyValuePair<double, double>> points, bool isY)
    {
        if (mode == AxisLabelMode.None) return new List<double>();
        if (mode == AxisLabelMode.Points)
            return isY ? points.Select(p => p.Value).Distinct().ToList() : points.Select(p => p.Key).ToList();

        if (mode == AxisLabelMode.Grid)
            return GetGridValues(min, max, gSize, pixels, isY ? 100 : 200);

        // AxisLabelMode.Auto
        var res = new List<double>();
        double range = max - min;
        if (range <= 0) return res;

        double rawStep = range / (pixels / 80);
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        double residual = rawStep / magnitude;
        double step = residual switch
        {
            < 1.5 => 1 * magnitude,
            < 3.5 => 2 * magnitude,
            < 7.5 => 5 * magnitude,
            _ => 10 * magnitude
        };

        for (double v = Math.Floor(min / step) * step; v <= max + step * 0.1; v += step)
            if (v >= min) res.Add(v);
        return res;
    }

    #endregion

    /// <summary>
    /// Форматирует число для отображения на осях и в подсказках.
    /// </summary>
    private string FormatNumber(double value)
    {
        double abs = Math.Abs(value);
        if (abs >= 1_000_000) return (value / 1_000_000).ToString("0.#") + "M";
        if (abs >= 1_000) return (value / 1_000).ToString("0.#") + "k";
        if (abs < 1 && abs > 0) return value.ToString("0.##");
        return value.ToString("0.#");
    }


}

public enum ChartStyle
{
    // Точки
    Simple,

    // Линии
    Line,

    // Ступенчатый
    Step,

    // Кривые
    Spline,
}

public enum AxisLabelMode
{   // Нет
    None,

    // По сетке
    Grid,

    // По точкам
    Points,

    // Авто
    Auto
}