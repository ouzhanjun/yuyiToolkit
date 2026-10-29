using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Path = Avalonia.Controls.Shapes.Path;
using Point = Avalonia.Point;
using Vector = Avalonia.Vector;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角星形控件（自包含 demo 版本）。算法与 RoundedTriangleControl 完全一致：
///   uIn  = normalize(prev - v)        v→prev
///   uOut = normalize(next - v)        v→next
///   α    = acos(uIn · uOut)           内角
///   bisect = normalize(uIn + uOut)     V 字对称轴；按整体卷绕方向取反，固定指向多边形内部
///   dC   = r / sin(α/2)               圆心到 v 的距离
///   dT   = r / tan(α/2)               切点到 v 的距离
///   center = v + bisect * dC          圆心（在多边形内部）
///   arcStart = v + uIn  * dT          入边切点
///   arcEnd   = v + uOut * dT          出边切点
///   sweep = CounterClockwise          圆心在内 → 弧绕过 v 的外侧
/// </summary>
public class RoundedStarControl : Panel
{
    public static readonly StyledProperty<RoundedStarElement?> ElementProperty =
        AvaloniaProperty.Register<RoundedStarControl, RoundedStarElement?>(nameof(Element));

    public RoundedStarElement? Element
    {
        get => GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }

    private readonly Path _fillPath = new() { IsHitTestVisible = false };
    private readonly Path _strokePath = new() { IsHitTestVisible = false };
    private readonly Canvas _debugOverlay = new() { IsHitTestVisible = false };
    private RoundedStarElement? _current;

    public bool ShowDebugCenters { get; set; } = true;

    public RoundedStarControl()
    {
        Children.Add(_fillPath);
        Children.Add(_strokePath);
        Children.Add(_debugOverlay);

        ElementProperty.Changed.AddClassHandler<RoundedStarControl>((c, e) =>
        {
            if (_current != null)
            {
                _current.PropertyChanged -= OnElChanged;
                _current.Radii.CollectionChanged -= OnRadiiChanged;
            }
            _current = e.NewValue as RoundedStarElement;
            if (_current != null)
            {
                _current.PropertyChanged += OnElChanged;
                _current.Radii.CollectionChanged += OnRadiiChanged;
            }
            Rebuild();
        });
        BoundsProperty.Changed.AddClassHandler<RoundedStarControl>((c, _) => Rebuild());
    }

    private void OnElChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e) => Rebuild();
    private void OnRadiiChanged(object? s, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Rebuild();

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

        var pts = el.GetPts();
        var radii = el.GetRadii();

        var geo = RoundedStarGeometry.BuildWithTangents(
            pts, radii,
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

            if (r > 0.0001)
            {
                _debugOverlay.Children.Add(new Ellipse
                {
                    Width = r * 2, Height = r * 2,
                    Stroke = Brushes.Yellow, StrokeThickness = 1,
                    StrokeDashArray = new AvaloniaList<double> { 4, 3 },
                    [Canvas.LeftProperty] = c.X - r,
                    [Canvas.TopProperty] = c.Y - r
                });
            }

            const double cs = 6.0;
            _debugOverlay.Children.Add(new Line
            {
                StartPoint = new Point(c.X - cs, c.Y), EndPoint = new Point(c.X + cs, c.Y),
                Stroke = Brushes.Yellow, StrokeThickness = 1.5
            });
            _debugOverlay.Children.Add(new Line
            {
                StartPoint = new Point(c.X, c.Y - cs), EndPoint = new Point(c.X, c.Y + cs),
                Stroke = Brushes.Yellow, StrokeThickness = 1.5
            });
            _debugOverlay.Children.Add(new Ellipse
            {
                Width = 4, Height = 4, Fill = Brushes.Yellow,
                [Canvas.LeftProperty] = c.X - 2, [Canvas.TopProperty] = c.Y - 2
            });
        }
    }

    private void DrawTangent(Point p)
    {
        const double sz = 7.0;
        _debugOverlay.Children.Add(new Ellipse
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
/// 圆角星形几何构建器（任意 N 个相邻顶点）。
/// 输入：顺时针绕行的顶点列表（首顶点 → next → … → 闭合），每个顶点的圆角半径。
/// 对每个顶点用与 RoundedTriangleGeometry 完全相同的方法求圆心、切点、sweep。
/// </summary>
public static class RoundedStarGeometry
{
    public static PathGeometry Build(IReadOnlyList<Point> pts, IReadOnlyList<double> radii)
        => BuildWithTangents(pts, radii, out _, out _, out _);

    public static PathGeometry BuildWithTangents(
        IReadOnlyList<Point> pts,
        IReadOnlyList<double> radii,
        out (Point Center, double Radius)[] cornerOut,
        out Point[] arcStarts,
        out Point[] arcEnds)
    {
        cornerOut = Array.Empty<(Point, double)>();
        arcStarts = Array.Empty<Point>();
        arcEnds = Array.Empty<Point>();

        int n = pts.Count;
        if (n < 3 || radii.Count != n)
            return new PathGeometry();

        var centers = new (Point C, double R)[n];
        var aStart  = new Point[n];
        var aEnd    = new Point[n];

        // 通用"夹角平分线"圆角算法（与卷绕方向、奇偶性无关）：
        //   uIn  = normalize(prev - v)，uOut = normalize(next - v)
        //   α    = acos(uIn · uOut) ∈ (0, π]   ← v 处相邻点的张角
        //   dC   = r / sin(α/2)                 ← 圆心到 v 距离（在 α 那一侧）
        //   dT   = r / tan(α/2)                 ← 切点到 v 距离（沿两邻边）
        //   bisect = normalize(uIn + uOut)       ← 指向 α 那一侧
        //     - α < π：uIn ≠ -uOut，sum 非零
        //     - α → π：sum → 0，由 s/t 保护（r→0 时也安全）
        //   center = v + bisect * dC
        //   aStart = v + uIn  * dT
        //   aEnd   = v + uOut * dT
        // α 是局部量，只取决于前一/后一点，顺序、奇偶、整体卷绕都不参与。
        // 弧必须绕过 v 的"α 侧反向"那一侧（即远离 bisect 那一侧），
        // 从而把外尖削尖 / 把内凹打圆，最终闭合。
        for (int i = 0; i < n; i++)
        {
            var prev = pts[(i + n - 1) % n];
            var v    = pts[i];
            var next = pts[(i + 1) % n];

            var uIn  = Normalize(Sub(prev, v));
            var uOut = Normalize(Sub(next, v));

            double dot  = Clamp(uIn.X * uOut.X + uIn.Y * uOut.Y, -1, 1);
            double half = Math.Acos(dot) * 0.5;
            double s    = Math.Max(Math.Sin(half), 1e-4);
            double t    = Math.Max(Math.Tan(half), 1e-4);

            double r        = Math.Max(0, radii[i]);
            double dCenter  = r / s;
            double dTangent = r / t;

            var bisect = Normalize(Add(uIn, uOut));
            centers[i] = (AddPoint(v, Mul(bisect, dCenter)), r);
            aStart[i]  = AddPoint(v, Mul(uIn,  dTangent));
            aEnd[i]    = AddPoint(v, Mul(uOut, dTangent));
        }

        // 路径：StartPoint = aStart[0]（路径上第一个切点）。
        // 第 i 步：从 aStart[i] 弧 → aEnd[i]，再直线 → aStart[(i+1) % n]。
        // IsClosed = true 自动回到 StartPoint，不画多余的 Line→aStart[0]。
        //
        // 弧方向：在每个角 v 处，uIn × uOut（屏幕叉积，y 朝下）符号决定
        //   环在 v 处的局部卷绕方向。
        //   > 0：局部逆时针（凸角常见） → 弧绕过 v 的外侧，sweep = CCW
        //   < 0：局部顺时针（凹角常见） → 弧兜进 v，sweep = CW
        // 用 uIn × uOut 局部判定，与全局走向无关、与 i 奇偶无关，
        //   对任意点集都自洽（凸多边形、凹多边形、星形都成立）。
        var fig = new PathFigure { IsClosed = true, StartPoint = aStart[0] };

        for (int i = 0; i < n; i++)
        {
            int nextIdx = (i + 1) % n;
            var v = pts[i];
            var prev = pts[(i + n - 1) % n];
            var next = pts[nextIdx];
            var uIn = Normalize(Sub(prev, v));
            var uOut = Normalize(Sub(next, v));
            double cross = uIn.X * uOut.Y - uIn.Y * uOut.X;

            if (radii[i] > 0.0001)
            {
                fig.Segments!.Add(new ArcSegment
                {
                    Point           = aEnd[i],
                    Size            = new Size(radii[i], radii[i]),
                    SweepDirection  = cross > 0 ? SweepDirection.CounterClockwise
                                                  : SweepDirection.Clockwise,
                    IsLargeArc      = false
                });
            }
            fig.Segments!.Add(new LineSegment { Point = aStart[nextIdx] });
        }

        var geo = new PathGeometry();
        geo.Figures!.Add(fig);

        cornerOut = centers;
        arcStarts = aStart;
        arcEnds   = aEnd;
        return geo;
    }
    /// <summary>
    /// 将点a和点b相减，返回新的向量。
    /// </summary>
    private static Vector Sub(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// 将向量a和向量b相加，返回新的向量。
    /// </summary>
    private static Vector Add(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// 将向量a乘以标量s，返回新的向量。
    /// </summary>
    private static Vector Mul(Vector a, double s) => new(a.X * s, a.Y * s);
    /// <summary>
    /// 将向量a取反，返回新的向量。
    /// </summary>
    private static Vector Neg(Vector a) => new(-a.X, -a.Y);

    /// <summary>
    /// 将点a和向量b相加，返回新的点。
    /// </summary>
    private static Point AddPoint(Point a, Vector b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// 规范化向量（单位向量），忽略非常小的长度。
    /// </summary>
    private static Vector Normalize(Vector v)
    {
        // 计算向量的长度
        var len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
        // 如果长度非常小，则返回0向量
        if (len < 1e-6) return new Vector(0, 0);
        // 返回单位向量
        return new Vector(v.X / len, v.Y / len);
    }

    /// <summary>
    /// 将值限制在最小值和最大值之间。
    /// </summary>
    private static double Clamp(double v, double min, double max)
        => v < min ? min : (v > max ? max : v);
}
