using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Point = Avalonia.Point;
using Vector = Avalonia.Vector;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角三角形控件（自包含 demo 版本，不依赖 UI 库其他文件）。
/// 算法：对每个顶点，沿两边分别向内回退 d = r / sin(α/2) 得到切点，
/// 再把切点夹住的部分替换为以顶点为中心、半径 r 的圆弧。
/// </summary>
public class RoundedTriangleControl : Panel
{
    public static readonly StyledProperty<RoundedTriangleElement?> ElementProperty =
        AvaloniaProperty.Register<RoundedTriangleControl, RoundedTriangleElement?>(nameof(Element));

    public RoundedTriangleElement? Element
    {
        get => GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }

    private readonly Path _fillPath = new() { IsHitTestVisible = false };
    private readonly Path _strokePath = new() { IsHitTestVisible = false };
    private readonly Canvas _debugOverlay = new() { IsHitTestVisible = false };
    private RoundedTriangleElement? _current;

    public bool ShowDebugCenters { get; set; } = true;

    public RoundedTriangleControl()
    {
        Children.Add(_fillPath);
        Children.Add(_strokePath);
        Children.Add(_debugOverlay);

        ElementProperty.Changed.AddClassHandler<RoundedTriangleControl>((c, e) =>
        {
            if (_current != null) _current.PropertyChanged -= OnElChanged;
            _current = e.NewValue as RoundedTriangleElement;
            if (_current != null) _current.PropertyChanged += OnElChanged;
            Rebuild();
        });
        BoundsProperty.Changed.AddClassHandler<RoundedTriangleControl>((c, _) => Rebuild());
    }

    private void OnElChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e) => Rebuild();

    public void Refresh() => Rebuild();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Rebuild();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Rebuild();
    }

    private void Rebuild()
    {
        var el = Element;
        if (el == null) return;

        var geo = RoundedTriangleGeometry.BuildWithTangents(
            el.GetP1(), el.GetP2(), el.GetP3(), el.R1, el.R2, el.R3,
            out var corners, out var arcStarts, out var arcEnds);

        _fillPath.Data = geo;
        _strokePath.Data = geo;
        _fillPath.Fill = new SolidColorBrush(ParseHex(el.FillColor));
        _fillPath.Stroke = null;
        _strokePath.Fill = null;
        _strokePath.Stroke = new SolidColorBrush(ParseHex(el.StrokeColor));
        _strokePath.StrokeThickness = el.StrokeWidth;
        _fillPath.InvalidateVisual();
        _strokePath.InvalidateVisual();

        RedrawDebug(corners, arcStarts, arcEnds);
    }

    private void RedrawDebug(
        System.Collections.Generic.IReadOnlyList<(Point Center, double Radius)> corners,
        System.Collections.Generic.IReadOnlyList<Point> arcStarts,
        System.Collections.Generic.IReadOnlyList<Point> arcEnds)
    {
        _debugOverlay.Children.Clear();
        if (!ShowDebugCenters) return;
        for (int i = 0; i < corners.Count; i++)
        {
            var (c, r) = corners[i];
            if (i < arcStarts.Count) DrawTangent(arcStarts[i]);
            if (i < arcEnds.Count) DrawTangent(arcEnds[i]);

            // 虚线圆：r>0 时画在圆心
            if (r > 0.0001)
            {
                _debugOverlay.Children.Add(new Avalonia.Controls.Shapes.Ellipse
                {
                    Width = r * 2, Height = r * 2,
                    Stroke = Brushes.Yellow, StrokeThickness = 1,
                    StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 3 },
                    [Canvas.LeftProperty] = c.X - r,
                    [Canvas.TopProperty] = c.Y - r
                });
            }

            const double cs = 6.0;
            _debugOverlay.Children.Add(new Avalonia.Controls.Shapes.Line
            {
                StartPoint = new Point(c.X - cs, c.Y), EndPoint = new Point(c.X + cs, c.Y),
                Stroke = Brushes.Yellow, StrokeThickness = 1.5
            });
            _debugOverlay.Children.Add(new Avalonia.Controls.Shapes.Line
            {
                StartPoint = new Point(c.X, c.Y - cs), EndPoint = new Point(c.X, c.Y + cs),
                Stroke = Brushes.Yellow, StrokeThickness = 1.5
            });
            _debugOverlay.Children.Add(new Avalonia.Controls.Shapes.Ellipse
            {
                Width = 4, Height = 4, Fill = Brushes.Yellow,
                [Canvas.LeftProperty] = c.X - 2, [Canvas.TopProperty] = c.Y - 2
            });
        }
    }

    private void DrawTangent(Point p)
    {
        const double sz = 7.0;
        _debugOverlay.Children.Add(new Avalonia.Controls.Shapes.Ellipse
        {
            Width = sz, Height = sz,
            Stroke = Brushes.Yellow, StrokeThickness = 1.5,
            Fill = new SolidColorBrush(Color.FromArgb(120, 255, 255, 0)),
            [Canvas.LeftProperty] = p.X - sz * 0.5, [Canvas.TopProperty] = p.Y - sz * 0.5
        });
    }

    private static Color ParseHex(string hex)
    {
        try
        {
            if (hex.StartsWith("#") && (hex.Length == 7 || hex.Length == 9))
            {
                var a = hex.Length == 9 ? Convert.ToByte(hex.Substring(1, 2), 16) : (byte)255;
                var r = Convert.ToByte(hex.Substring(hex.Length - 6, 2), 16);
                var g = Convert.ToByte(hex.Substring(hex.Length - 4, 2), 16);
                var b = Convert.ToByte(hex.Substring(hex.Length - 2, 2), 16);
                return Color.FromArgb(a, r, g, b);
            }
        }
        catch { }
        return Colors.Red;
    }
}

/// <summary>
/// 圆角三角形几何构建器。
/// 输入：三角顶点（顺时针 P1→P2→P3），三个角各自的圆角半径 r1,r2,r3（像素）。
/// 对每个角：边方向单位向量 u1（指向前一顶点）、u2（指向下一顶点）。
/// 夹角 α = acos(-u1·u2)（取 0~π，因为顺时针绕行内角小于 180°）。
/// 圆心在角平分线（角外侧方向）上，离顶点距离 d = r / sin(α/2)。
/// 切点：顶点 + u1 · (r / tan(α/2))；顶点 + u2 · (r / tan(α/2))。
/// 圆弧从切点1 扫到 切点2（顺时针）。
/// </summary>
public static class RoundedTriangleGeometry
{
    public static PathGeometry Build(Point p1, Point p2, Point p3, double r1, double r2, double r3)
        => BuildWithTangents(p1, p2, p3, r1, r2, r3, out _, out _, out _);

    public static PathGeometry BuildWithTangents(
        Point p1, Point p2, Point p3, double r1, double r2, double r3,
        out (Point Center, double Radius)[] cornerOut,
        out Point[] arcStarts,
        out Point[] arcEnds)
    {
        var corners = new (Point V, double R)[]
        {
            (p1, Math.Max(0, r1)),
            (p2, Math.Max(0, r2)),
            (p3, Math.Max(0, r3))
        };

        // 上一顶点（进入此角的边方向）：c0 来自 corners[i-1].V；下一顶点（离开此角的边方向）：c2 来自 corners[i+1].V
        var tangents = new Point[3];   // 每角：边进入方向的切点（沿入边离顶点 r/tan(α/2)）
        var arcStart = new Point[3];   // 每角：弧起点 = 沿入边的切点
        var arcEnd = new Point[3];     // 每角：弧终点 = 沿出边的切点
        var center = new (Point C, double R)[3];   // 每角：(圆心, 半径)
        var alpha = new double[3];     // 每角：内角 α

        for (int i = 0; i < 3; i++)
        {
            var prev = corners[(i + 2) % 3].V;        // 上一顶点
            var next = corners[(i + 1) % 3].V;        // 下一顶点
            var v = corners[i].V;

            // 入边单位向量（指向 prev 的反向，因为入边方向 = v→prev）
            var uIn = Normalize(Sub(prev, v));
            // 出边单位向量 = v→next
            var uOut = Normalize(Sub(next, v));

            // 内角 α：uIn 和 uOut 之间夹角（顺时针绕行时内角 < π）
            double dot = Clamp(uIn.X * uOut.X + uIn.Y * uOut.Y, -1, 1);
            double a = Math.Acos(dot);

            // d = r / sin(α/2)（圆心到顶点距离）
            double dCenter = corners[i].R / Math.Max(Math.Sin(a * 0.5), 1e-4);
            // 切点到顶点距离 = r / tan(α/2)
            double dTangent = corners[i].R / Math.Max(Math.Tan(a * 0.5), 1e-4);

            // 圆心在三角形内部（沿角平分线的内侧方向）。
            // 当三角形顺时针（cross > 0），uIn+uOut 的合成指向外；要反转才能指内。
            // 顺时针在 Avalonia（Y 向下）中：cross < 0。
            var bisect = Normalize(Add(uIn, uOut));
            double cross = (p2.X - p1.X) * (p3.Y - p1.Y) - (p2.Y - p1.Y) * (p3.X - p1.X);
            if (cross > 0) bisect = Neg(bisect);

            // 圆心到顶点距离 d = r / sin(α/2)。圆心在内时，弧从切点1 扫到切点2 视觉上向角凸出。
            center[i] = (AddPoint(v, Mul(bisect, dCenter)), corners[i].R);
            arcStart[i] = AddPoint(v, Mul(uIn, dTangent));
            arcEnd[i] = AddPoint(v, Mul(uOut, dTangent));
            alpha[i] = a;
        }

        var fig = new PathFigure { IsClosed = true, StartPoint = arcEnd[0] };

        // 边 0：arcEnd[0] → arcStart[1]
        fig.Segments!.Add(new LineSegment { Point = arcStart[1] });

        // 顶点 1（角 P2）弧：arcStart[1] → arcEnd[1]
        if (corners[1].R > 0.0001)
        {
            fig.Segments.Add(new ArcSegment
            {
                Point = arcEnd[1],
                Size = new Size(corners[1].R, corners[1].R),
                SweepDirection = SweepDirection.CounterClockwise,
                IsLargeArc = false
            });
        }

        // 边 1：arcEnd[1] → arcStart[2]，直线
        fig.Segments.Add(new LineSegment { Point = arcStart[2] });

        // 顶点 2（角 P3）弧
        if (corners[2].R > 0.0001)
        {
            fig.Segments.Add(new ArcSegment
            {
                Point = arcEnd[2],
                Size = new Size(corners[2].R, corners[2].R),
                SweepDirection = SweepDirection.CounterClockwise,
                IsLargeArc = false
            });
        }

        // 边 2：arcEnd[2] → arcStart[0]，直线
        fig.Segments.Add(new LineSegment { Point = arcStart[0] });

        // 顶点 0（角 P1）弧
        if (corners[0].R > 0.0001)
        {
            fig.Segments.Add(new ArcSegment
            {
                Point = arcEnd[0],
                Size = new Size(corners[0].R, corners[0].R),
                SweepDirection = SweepDirection.CounterClockwise,
                IsLargeArc = false
            });
        }

        var geo = new PathGeometry();
        geo.Figures!.Add(fig);

        cornerOut = center;
        arcStarts = arcStart;
        arcEnds = arcEnd;
        return geo;
    }

    private static Vector Sub(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    private static Vector Add(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);
    private static Vector Mul(Vector a, double s) => new(a.X * s, a.Y * s);
    private static Vector Neg(Vector a) => new(-a.X, -a.Y);
    private static Point AddPoint(Point a, Vector b) => new(a.X + b.X, a.Y + b.Y);

    private static Vector Normalize(Vector v)
    {
        var len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
        if (len < 1e-6) return new Vector(0, 0);
        return new Vector(v.X / len, v.Y / len);
    }

    private static double Clamp(double v, double min, double max) => v < min ? min : (v > max ? max : v);
}