using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.Platform;
using Common.Controls.Models;
using Common.Controls.TemplatedControls;
using DynamicData.Aggregation;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Common.Controls;

public class ChartControl : TemplatedControl
{

    // Поля для координат области рисования
    private double _left;
    private double _right;
    private double _top;
    private double _bottom;
    private double _axisX;
    private double _axisY;

    // Ограничения: X - глобально (весь график), Y - локально (область отрисовки)
    private double _minX;
    private double _maxX;
    private double _minY;
    private double _maxY;

    // Для работы с указателем
    private Point? _mousePosition;
    private Point _prevMousePosition;
    private bool _isDraging = false;

    // Кешированные данные
    private List<KeyValuePair<double, double>> _sortedContentPoints; 
    private List<KeyValuePair<double, double>> _sortedVisiblePoints; 

    // Кешированные даные кистей
    private Pen _gridPen;
    private Pen _axisPen;
    private Pen _chartPen;
    private Pen _pointsPen;

    #region Styled Property

    public static readonly StyledProperty<ChartDataBase> ContentProperty =
        AvaloniaProperty.Register<ChartControl, ChartDataBase>(nameof(Content), new());

    public static readonly StyledProperty<ChartDataBase> SortedDataProperty =
        AvaloniaProperty.Register<ChartControl, ChartDataBase>(nameof(SortedData), new(), defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ChartControl, double>(nameof(Minimum), defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ChartControl, double>(nameof(Maximum), defaultBindingMode: BindingMode.OneWayToSource);

    public static readonly StyledProperty<ChartStyle> ChartStyleProperty =
        AvaloniaProperty.Register<ChartControl, ChartStyle>(nameof(ChartStyle), ChartStyle.Simple);

    public static readonly StyledProperty<IBrush> ChartColorProperty =
        AvaloniaProperty.Register<ChartControl, IBrush>(nameof(ChartColor), Brushes.DodgerBlue);

    public static readonly StyledProperty<double> ChartThicknessProperty =
        AvaloniaProperty.Register<ChartControl, double>(nameof(ChartThickness), 2);

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

    public static readonly StyledProperty<IBrush> PointsLabelsColorProperty =
        AvaloniaProperty.Register<ChartControl, IBrush>(nameof(PointsLabelsColor), Brushes.Black);

    public static readonly StyledProperty<bool> InteractiveProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(Interactive), false);

    public static readonly StyledProperty<KeyModifiers> InteractiveModifierProperty =
        AvaloniaProperty.Register<ChartControl, KeyModifiers>(nameof(InteractiveModifier), KeyModifiers.None);

    #endregion 

    static ChartControl()
    {
        AffectsRender<ChartControl>(
            ContentProperty,
            SortedDataProperty,
            MinimumProperty,
            MaximumProperty,
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
            ShowPointsLabelsProperty,
            InteractiveProperty);
    }

    public ChartControl()
    {
        _gridPen = new Pen(GridColor, 0.5);
        _axisPen = new Pen(AxisColor, 1);
        _chartPen = new Pen(ChartColor, ChartThickness);
        _pointsPen = new Pen(ChartColor, 2);

        Focusable = true;
    }

    #region Свойства

    /// <summary>
    /// Данные для графика
    /// </summary>
    public ChartDataBase Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Список точек используемых для построения в данный момент. Используется для расчётов
    /// </summary>
    public ChartDataBase SortedData
    {
        get => GetValue(SortedDataProperty);
        set => SetValue(SortedDataProperty, value);
    }

    /// <summary>
    /// Ограничение снизу по X
    /// </summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>
    /// Ограничение сверху по X
    /// </summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
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
    /// Толщина линии графика
    /// </summary>
    public double ChartThickness
    {
        get => GetValue(ChartThicknessProperty);
        set => SetValue(ChartThicknessProperty, value);
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
    /// Auto - автоматические целые значения, примерно 80 px между линиями; "число + *" - конкретно конечное количество ячеек
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
    /// Auto - автоматические целые значения, примерно 80 px между линиями; "число + *" - конкретно количество ячеек
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

    /// <summary>
    /// Цвет подписей на точках
    /// </summary>
    public IBrush PointsLabelsColor
    {
        get => GetValue(PointsLabelsColorProperty);
        set => SetValue(PointsLabelsColorProperty, value);
    }

    /// <summary>
    /// Сделать график интерактивным (масштабирование, перетаскивание)
    /// </summary>
    public bool Interactive
    {
        get => GetValue(InteractiveProperty);
        set => SetValue(InteractiveProperty, value);
    }

    /// <summary>
    /// Клавиша модификатор для интерактивного взаимодействия с графиком
    /// </summary>
    public KeyModifiers InteractiveModifier
    {
        get => GetValue(InteractiveModifierProperty);
        set => SetValue(InteractiveModifierProperty, value);
    }

    #endregion

    #region События

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 800 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 400 : availableSize.Height;

        return new Size(width, height);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == ContentProperty || e.Property == InteractiveProperty)
            RefreshData();
        else if (e.Property == MinimumProperty || e.Property == MaximumProperty)
            UpdateData();
        else if (e.Property == GridColorProperty)
            _gridPen = new Pen(GridColor, 0.5);
        else if (e.Property == AxisColorProperty)
            _axisPen = new Pen(AxisColor, 1);
        else if (e.Property == ChartColorProperty || e.Property == ChartThicknessProperty)
        {
            _chartPen = new Pen(ChartColor, ChartThickness);
            _pointsPen = new Pen(ChartColor, 2);
        }

    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        var props = e.GetCurrentPoint(this).Properties;
        bool isLeft = props.IsLeftButtonPressed;

        if (!isLeft || InteractiveModifier != KeyModifiers.None
            && !e.KeyModifiers.HasFlag(InteractiveModifier))
        {
            base.OnPointerPressed(e);
            return;
        }

        _isDraging = true;
        _mousePosition = e.GetPosition(this);
        _prevMousePosition = e.GetPosition(this);

        e.Handled = true;

        base.OnPointerPressed(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        _mousePosition = e.GetPosition(this);
        if (_isDraging)
        {
            var dx = PointToValue(_mousePosition.Value.X, 0).X - PointToValue(_prevMousePosition.X, 0).X;

            double range = Maximum - Minimum;
            double threshold = Math.Max(range * 0.005, 0.1);

            if (Math.Abs(dx) > threshold)
            {
                MoveChart(-dx);

                _prevMousePosition = e.GetPosition(this); 

                e.Handled = true;
            }
        }
        else
        {
            InvalidateVisual();
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (Interactive && !IsFocused)
            Focus();
        _isDraging = false;
        base.OnPointerReleased(e);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _mousePosition = null;
        InvalidateVisual();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (InteractiveModifier != KeyModifiers.None 
            && !e.KeyModifiers.HasFlag(InteractiveModifier))
        {
            base.OnPointerWheelChanged(e);
            return;
        }

        var center = PointToValue(e.GetPosition(this).X, 0);

        ZoomChart(center, e.Delta.Y / Math.Abs(e.Delta.Y) * 0.1);

        e.Handled = true;

        base.OnPointerWheelChanged(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {

        if (InteractiveModifier != KeyModifiers.None
            && !e.KeyModifiers.HasFlag(InteractiveModifier))
        {
            base.OnKeyDown(e);
            return;
        }

        double dx = (Maximum - Minimum) * 0.05;

        if (e.Key == Key.Left) MoveChart(-dx);
        else if (e.Key == Key.Right) MoveChart(dx);
        else if (e.Key == Key.Up) ZoomChart(new Point((Maximum + Minimum) / 2, 0), 0.2);
        else if (e.Key == Key.Down) ZoomChart(new Point((Maximum + Minimum) / 2, 0), -0.2);

        e.Handled = true;
        base.OnKeyDown(e);
    }

    #endregion

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (SortedData == null || SortedData.Chart.Count < 2) return;

        if (Bounds.Width <= Padding.Left + Padding.Right ||
            Bounds.Height <= Padding.Top + Padding.Bottom)
            return;

        _left = Padding.Left;
        _right = Bounds.Width - Padding.Right;
        _top = Padding.Top;
        _bottom = Bounds.Height - Padding.Bottom;

        // Подготовка данных для сетки и осей
        List<double> xGridLines = null;
        List<double> yGridLines = null;
        List<double> xLabels = null;
        List<double> yLabels = null;

        var points = _sortedVisiblePoints;

        if (Grid || Axis)
        {
            var origin = ValueToPoint(0, 0);
            _axisX = Math.Clamp(origin.X, _left, _right);
            _axisY = Math.Clamp(origin.Y, _top, _bottom);

            double availableWidth = _right - _left;
            double availableHeight = _bottom - _top;

            xGridLines = GetGridValues(Minimum, Maximum, GridSizeX, availableWidth, 200);
            yGridLines = GetGridValues(_minY, _maxY, GridSizeY, availableHeight, 100);
            xLabels = GetLabelValues(Minimum, Maximum, LabelModeX, GridSizeX, availableWidth, points, false);
            yLabels = GetLabelValues(_minY, _maxY, LabelModeY, GridSizeY, availableHeight, points, true);
        }

        // 1. Фон
        context.FillRectangle(Background ?? Brushes.Transparent, new Rect(Bounds.Size));

        // 2. Сетка и оси
        DrawGridAndAxesLines(context, xGridLines, yGridLines, ValueToPoint);

        // 3. Геометрия графика
        var screenPoints = points.Select(p => ValueToPoint(p.Key, p.Value)).ToList();
        DrawChartGeometry(context, screenPoints);

        // 4. Точки и подписи
        if (HighlightPoints)
        {
            DrawPointsAndLabels(context, points, ValueToPoint);
        }

        // 5. Рамка
        var pen = new Pen(Background, 1);
        context.DrawRectangle(Background, pen, new Rect(0, 0, _left, Bounds.Height));
        context.DrawRectangle(Background, pen, new Rect(_right, 0, Bounds.Width, Bounds.Height));

        // 6. Подписи осей
        if (Axis && xLabels != null && yLabels != null)
        {
            DrawAxesTicksAndLabels(context, xLabels, yLabels, points, ValueToPoint);
        }

        // 7. Подсказка мыши
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
            foreach (var val in xGridLines)
            {
                var p = normalize(val, 0);
                context.DrawLine(_gridPen, new Point(p.X, _top), new Point(p.X, _bottom));
            }
            foreach (var val in yGridLines)
            {
                var p = normalize(0, val);
                context.DrawLine(_gridPen, new Point(_left, p.Y), new Point(_right, p.Y));
            }
        }

        if (Axis)
        {
            context.DrawLine(_axisPen, new(_axisX, _top), new(_axisX, _bottom));
            context.DrawLine(_axisPen, new(_left, _axisY), new(_right, _axisY));
        }
    }

    /// <summary>
    /// Строит геометрию графика (линию, ступеньку, сплайн) и отрисовывает её.
    /// </summary>
    private void DrawChartGeometry(DrawingContext context, List<Point> screenPoints)
    {
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
            context.DrawGeometry(Fill ? ChartColor : null, _chartPen, geometry);
        }
        else
        {
            // Для стиля Simple рисуем точки или вертикальные линии
            foreach (var p in screenPoints)
            {
                if (Fill)
                    context.DrawLine(_chartPen, p, new Point(p.X, Bounds.Height));
                else
                    context.DrawEllipse(ChartColor, _chartPen, p, ChartThickness / 2, ChartThickness / 2);
            }
        }
    }

    /// <summary>
    /// Отрисовывает текстовые подписи осей.
    /// </summary>
    private void DrawAxesTicksAndLabels(DrawingContext context, List<double> xLabels, List<double> yLabels,
    List<KeyValuePair<double, double>> points, Func<double, double, Point> normalize)
    {
        // Засечки и подписи по X
        double angleX = xLabels.Count > 10 ? (xLabels.Count > 40 ? 90 : 45) : 0;
        foreach (var val in xLabels)
        {
            var p = normalize(val, 0);
            if (p.X < _left - 1 || p.X > _right + 1) continue;

            // Засечка
            context.DrawLine(_axisPen, new Point(p.X, _axisY - 3), new Point(p.X, _axisY + 3));

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
            context.DrawLine(_axisPen, new Point(_axisX - 3, p.Y), new Point(_axisX + 3, p.Y));

            // Подпись
            DrawText(context, FormatNumber(val), new Point(_axisX - 20, p.Y), 0, TextAlignment.Right);
        }
    }


    /// <summary>
    /// Отрисовывает точки графика и их подписи (если включено).
    /// </summary>
    private void DrawPointsAndLabels(DrawingContext context, List<KeyValuePair<double, double>> points, Func<double, double, Point> normalize)
    {
        foreach (var point in points)
        {
            var nPoint = normalize(point.Key, point.Value);
            context.DrawEllipse(ChartColor, _pointsPen, nPoint, 2, 2);

            if (ShowPointsLabels)
            {
                double TextAngle = 0;
                TextAngle = SortedData.Count >= 50 ? -45 : TextAngle;
                TextAngle = SortedData.Count >= 100 ? -60 : TextAngle;

                DrawText(context, FormatNumber(point.Value), new Point(nPoint.X, nPoint.Y - 15),
                    TextAngle, TextAlignment.Center, PointsLabelsColor);
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

            context.DrawEllipse(null, new Pen(ChartColor, ChartThickness), p, ChartThickness * 2.5, ChartThickness * 2.5);

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

    #region Вычисление значений

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
            step = range / size.Value;
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

    /// <summary>
    /// Обновляет данные при привязке
    /// </summary>
    private void RefreshData()
    {
        if (Content == null || Content.Count < 2)
            return;

        SortedData = new(Content.Chart);
        _sortedContentPoints = Content.Chart.OrderBy(p => p.Key).ToList();

        _minX = _sortedContentPoints[0].Key;
        _maxX = _sortedContentPoints[^1].Key;
        _minY = _sortedContentPoints.Min(p => p.Value);
        _maxY = _sortedContentPoints.Max(p => p.Value);

        Minimum = _minX;
        Maximum = _maxX;
    }

    /// <summary>
    /// Обновить отсортированные данные
    /// </summary>
    private void UpdateData()
    {
        if (Content == null || Content.Count < 2)
            return;

        int firstIdx = _sortedContentPoints.BinarySearch(new KeyValuePair<double, double>(Minimum, 0), 
            Comparer<KeyValuePair<double, double>>.Create((a, b) => a.Key.CompareTo(b.Key)));
        if (firstIdx < 0) firstIdx = ~firstIdx;

        int lastIdx = _sortedContentPoints.BinarySearch(new KeyValuePair<double, double>(Maximum, 0), 
            Comparer<KeyValuePair<double, double>>.Create((a, b) => a.Key.CompareTo(b.Key)));
        if (lastIdx < 0) lastIdx = ~lastIdx - 1;

        firstIdx = Math.Max(0, firstIdx - 1);

        lastIdx = Math.Min(_sortedContentPoints.Count - 1, lastIdx + 1);

        _sortedVisiblePoints = _sortedContentPoints.GetRange(firstIdx, lastIdx - firstIdx + 1);

        SortedData = new ChartDataBase(_sortedVisiblePoints.ToDictionary(p => p.Key, p => p.Value));

        _minY = _sortedVisiblePoints.Min(p => p.Value);
        _maxY = _sortedVisiblePoints.Max(p => p.Value);
    }

    private void MoveChart(double dx)
    {
        dx = -dx;
        if (dx > 0)
        {
            var min = Minimum;
            var delta = (min - dx >= _minX) ? dx : min - _minX;
            SetValue(MinimumProperty, Minimum - delta);
            SetValue(MaximumProperty, Maximum - delta);
        }
        else if (dx < 0)
        {
            var max = Maximum;
            var delta = (max - dx <= _maxX) ? dx : max - _maxX;
            SetValue(MinimumProperty, Minimum - delta);
            SetValue(MaximumProperty, Maximum - delta);
        }
    }

    private void ZoomChart(Point point, double delta)
    {
        
        double step = Math.Abs(delta);
        double center = point.X;

        double dxUp = (Maximum - center) * step;
        double dxDw = (center - Minimum) * step;
        dxUp = dxUp < 1 ? 1 : dxUp;
        dxDw = dxDw < 1 ? 1 : dxDw;

        var sortedKeys = SortedData.Chart.Keys.OrderBy(k => k).ToList();

        double aproxDelta = sortedKeys
            .Zip(sortedKeys.Skip(1), (current, next) => next - current)
            .Average();

        if (delta > 0)
        {
            var newMax = Maximum - (Maximum - dxUp > center + aproxDelta ? dxUp : 0);
            var newMin = Minimum + (Minimum + dxDw < center - aproxDelta ? dxDw : 0);
            SetValue(MaximumProperty, newMax);
            SetValue(MinimumProperty, newMin);
        }
        else if (delta < 0)
        {
            var newMax = (Maximum + dxUp < _maxX) ? Maximum + dxUp : _maxX;
            var newMin = (Minimum - dxDw > _minX) ? Minimum - dxDw : _minX;
            SetValue(MaximumProperty, newMax);
            SetValue(MinimumProperty, newMin);
        }
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

    private Point ValueToPoint(double x, double y)
    {
        double availableWidth = Bounds.Width - Padding.Left - Padding.Right;
        double availableHeight = Bounds.Height - Padding.Top - Padding.Bottom;

        double px = Padding.Left + (x - Minimum) / (Maximum - Minimum) * availableWidth;
        double py = (Padding.Top + availableHeight) - ((y - _minY) / (_maxY - _minY) * availableHeight);

        return new Point(px, py);
    }

    private Point PointToValue(double x, double y)
    {
        double availableWidth = Bounds.Width - Padding.Left - Padding.Right;
        double availableHeight = Bounds.Height - Padding.Top - Padding.Bottom;

        double vx = Minimum + ((x - Padding.Left) / availableWidth) * (Maximum - Minimum);
        double vy = _minY + ((Padding.Top + availableHeight - y) / availableHeight) * (_maxY - _minY);

        return new Point(vx, vy);
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