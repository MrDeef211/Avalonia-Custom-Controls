using Avalonia;
using Avalonia.Controls.Primitives;
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
    public static readonly StyledProperty<ChartDataBase> ContentProperty =
        AvaloniaProperty.Register<ChartControl, ChartDataBase>(nameof(Content), new());

    public static readonly StyledProperty<ChartStyle> ChartStyleProperty =
        AvaloniaProperty.Register<ChartControl, ChartStyle>(nameof(ChartStyle), ChartStyle.Simple);

    public static readonly StyledProperty<bool> IsFillProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(IsFill), false);

    public static readonly StyledProperty<bool> GridProperty =
        AvaloniaProperty.Register<ChartControl, bool>(nameof(Grid), false);

    public static readonly StyledProperty<IBrush> ChartColorProperty =
        AvaloniaProperty.Register<ChartControl, IBrush>(nameof(ChartColor), Brushes.DodgerBlue);

    static ChartControl()
    {
        AffectsRender<ChartControl>(ContentProperty, ChartStyleProperty, ChartColorProperty, IsFillProperty);
    }

    public ChartDataBase Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsFill
    {
        get => GetValue(IsFillProperty);
        set => SetValue(IsFillProperty, value);
    }

    public bool Grid
    {
        get => GetValue(GridProperty);
        set => SetValue(GridProperty, value);
    }

    public ChartStyle ChartStyle
    {
        get => GetValue(ChartStyleProperty);
        set => SetValue(ChartStyleProperty, value);
    }

    public IBrush ChartColor
    {
        get => GetValue(ChartColorProperty);
        set => SetValue(ChartColorProperty, value);
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

        Point Normalize(double x, double y)
        {
            double px = (x - minX) / (maxX - minX) * Bounds.Width;
            double py = Bounds.Height - ((y - minY) / (maxY - minY) * Bounds.Height);
            return new Point(px, py);
        }

        var pen = new Pen(ChartColor, 2);

        // Фон
        var background = Background ?? Brushes.Transparent;
        context.FillRectangle(background, new Rect(Bounds.Size));

        //Сетка


        var geometry = new StreamGeometry();
        using (var sgc = geometry.Open())
        {
            var screenPoints = points.Select(p => Normalize(p.Key, p.Value)).ToList();
            double bottom = Bounds.Height;

            sgc.BeginFigure(IsFill ? new Point(screenPoints[0].X, bottom) : screenPoints[0], IsFill);
            if (IsFill) sgc.LineTo(screenPoints[0]);

            switch (ChartStyle)
            {
                case ChartStyle.Simple:
                    break;

                case ChartStyle.Line:
                    for (int i = 1; i < screenPoints.Count; i++) sgc.LineTo(screenPoints[i]);
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
                        var p0 = screenPoints[i];
                        var p1 = screenPoints[i + 1];
                        double tension = 0.5;
                        double dx = (p1.X - p0.X) * tension;
                        sgc.CubicBezierTo(
                            new Point(p0.X + dx, p0.Y), 
                            new Point(p1.X - dx, p1.Y), 
                            p1                          
                        );
                    }
                    break;
            }

            if (IsFill && ChartStyle != ChartStyle.Simple)
            {
                sgc.LineTo(new Point(screenPoints.Last().X, bottom));
                sgc.LineTo(new Point(screenPoints[0].X, bottom));
            }
            sgc.EndFigure(IsFill);
        }

        if (ChartStyle != ChartStyle.Simple)
        {
            context.DrawGeometry(IsFill ? ChartColor : null, pen, geometry);
        }
        else 
        {
            foreach (var p in points.Select(p => Normalize(p.Key, p.Value)))
            {
                if (IsFill) context.DrawLine(pen, p, new Point(p.X, Bounds.Height));
                else context.DrawEllipse(ChartColor, pen, p, 1, 1);
            }
        }
    }
}

public enum ChartStyle
{
    Simple,
    Line,
    Step,
    Spline,
}