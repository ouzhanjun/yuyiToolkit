using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Media;

namespace yuyiAvalonia.UI.Helpers;

/// <summary>
/// 路径指令类型 —— 对应 SVG 路径命令 + Avalonia <see cref="PathSegment"/> 子集。
/// 所有 2D 矢量图形（直线/多段线/矩形/圆角矩形/椭圆/圆弧/贝塞尔/多边形/圆角多边形）
/// 都由以下 7 种指令组合而成：
///   Move(M)  - 移动到起点（不开新 subpath）
///   Line(L)  - 直线到终点
///   Horizontal(H) - 水平直线（SVG 优化）
///   Vertical(V)   - 垂直直线（SVG 优化）
///   Arc(A)  - 椭圆弧（圆角、扇形、圆弧统一表示）
///   Quadratic(Q) - 二次贝塞尔
///   Cubic(C)     - 三次贝塞尔
/// </summary>
public enum PathCommandType
{
    /// <summary>M：移动到起点</summary>
    Move,
    /// <summary>L：直线</summary>
    Line,
    /// <summary>H：水平线</summary>
    Horizontal,
    /// <summary>V：垂直线</summary>
    Vertical,
    /// <summary>Q：二次贝塞尔</summary>
    Quadratic,
    /// <summary>T：平滑二次贝塞尔</summary>
    SmoothQuadratic,
    /// <summary>C：三次贝塞尔</summary>
    Cubic,
    /// <summary>S：平滑三次贝塞尔</summary>
    SmoothCubic,
    /// <summary>A：椭圆弧</summary>
    Arc,
    /// <summary>Z：闭合路径</summary>
    Close,
}

/// <summary>
/// 图形分类 —— 表示这段路径流描述的是哪种几何图形。
/// 用于序列化 / 反序列化 / UI 提示。
/// </summary>
public enum ShapeKind
{
    Unknown,
    Line,
    Polyline,
    Rect,
    RoundedRect,
    Ellipse,
    Circle,
    Arc,
    Polygon,
    RoundedPolygon,
    RoundedStar,
    BezierPath,
}

/// <summary>
/// 路径段抽象基类 —— 所有图形共用一个 List&lt;PathSegment&gt; 容器，
/// 通过多态派发避免 record-struct 的字段浪费，同时方便子类扩展关键几何信息
/// （如 ArcSegment 的圆心/切点、贝塞尔的控制点）。
/// </summary>
public abstract class PathSegment
{
    /// <summary>指令类型。派生类通过重写 <see cref="Type"/> 字段返回固定值。</summary>
    public abstract PathCommandType Type { get; }

    /// <summary>段终点 —— Line / Horizontal / Vertical / Arc / 贝塞尔 的终点 EndPoint。</summary>
    public Point EndPoint { get; set; }
}

#region 具体段类型

/// <summary>移动到新起点（M）—— <see cref="EndPoint"/> 即新 subpath 的起点。</summary>
public sealed class MoveSegment : PathSegment
{
    public override PathCommandType Type => PathCommandType.Move;
}

/// <summary>直线段（L）—— 从上一段终点连到 <see cref="EndPoint"/>。</summary>
public sealed class LineSegmentData : PathSegment
{
    public override PathCommandType Type => PathCommandType.Line;
}

/// <summary>
/// 椭圆圆弧段（A）—— 与 SVG / Avalonia <see cref="ArcSegment"/> 完全对齐。
/// 除基础参数外，额外记录"弧圆心 / 弧入切点 / 弧出切点"作为内部调试/标注关键点
/// （SVG 不存，仅用于可视化圆角源数据）。
/// </summary>
public sealed class ArcSegmentData : PathSegment
{
    public override PathCommandType Type => PathCommandType.Arc;
    // 参数说明：
    // RadiusX：椭圆弧段的X轴半径
    // RadiusY：椭圆弧段的Y轴半径
    // RotationAngle：椭圆弧段的旋转角度
    // IsLargeArc：椭圆弧段是否为大弧段
    // SweepDir：椭圆弧段的扫描方向
    // ArcCenter：椭圆弧段的圆心
    // ArcTangentStart：椭圆弧段的入切点
    // ArcTangentEnd：椭圆弧段的出切点
    // RadiusX：圆角时 = |圆心 - 切点| = RadiusY；椭圆弧时 = X轴半轴
    public double RadiusX { get; set; }
    // RadiusY：椭圆弧时 = Y轴半轴（圆角/圆弧时 RadiusX == RadiusY）
    public double RadiusY { get; set; }
    public double RotationAngle { get; set; }
    // 椭圆弧段方向参数
    public bool IsLargeArc { get; set; }
    public SweepDirection SweepDir { get; set; }

    // 椭圆弧段关键点
    /// <summary>圆心</summary>
    public Point? ArcCenter { get; set; }
    /// <summary>弧入切点</summary>
    public Point? ArcTangentStart { get; set; }
    /// <summary>弧出切点</summary>
    public Point? ArcTangentEnd { get; set; }
}

/// <summary>二次贝塞尔（Q）—— 单控制点 + 终点。</summary>
public sealed class QuadraticBezierSegmentData : PathSegment
{
    public override PathCommandType Type => PathCommandType.Quadratic;

    public Point ControlPoint { get; set; }
}

/// <summary>三次贝塞尔（C）—— 双控制点 + 终点。</summary>
public sealed class CubicBezierSegmentData : PathSegment
{
    public override PathCommandType Type => PathCommandType.Cubic;

    public Point Control1 { get; set; }
    public Point Control2 { get; set; }
}

// ============================================================
// 路径段互转工具
// ============================================================

/// <summary>
/// 路径段转换工具 —— 直线 ↔ 贝塞尔（等分点控制点）、贝塞尔采样/近似。
/// </summary>
public static class PathSegmentConversion
{
    /// <summary>
    /// 两点直线 → 二次贝塞尔（控制点为线段中点）。
    /// 等价于：<c>new QuadraticBezierSegmentData { ControlPoint = lerp(a, b, 0.5), EndPoint = b }</c>
    /// </summary>
    public static QuadraticBezierSegmentData LineToQuadratic(Point a, Point b)
    {
        return new QuadraticBezierSegmentData
        {
            ControlPoint = new Point((a.X + b.X) * 0.5, (a.Y + b.Y) * 0.5),
            EndPoint = b,
        };
    }

    /// <summary>
    /// 两点直线 → 三次贝塞尔（两控制点为线段三等分点）。
    /// 控制点：C1 = lerp(a, b, 1/3)，C2 = lerp(a, b, 2/3)。
    /// </summary>
    public static CubicBezierSegmentData LineToCubic(Point a, Point b)
    {
        double t1 = 1.0 / 3.0;
        double t2 = 2.0 / 3.0;
        return new CubicBezierSegmentData
        {
            Control1 = new Point(a.X + (b.X - a.X) * t1, a.Y + (b.Y - a.Y) * t1),
            Control2 = new Point(a.X + (b.X - a.X) * t2, a.Y + (b.Y - a.Y) * t2),
            EndPoint = b,
        };
    }

    /// <summary>
    /// 二次贝塞尔采样点（de Casteljau 一层展开）。
    /// 返回参数 t 处贝塞尔曲线上的点：lerp(lerp(a, control, t), lerp(control, b, t), t)。
    /// </summary>
    public static Point SampleQuadratic(Point a, Point control, Point b, double t)
    {
        double u = 1.0 - t;
        return new Point(
            u * u * a.X + 2 * u * t * control.X + t * t * b.X,
            u * u * a.Y + 2 * u * t * control.Y + t * t * b.Y);
    }

    /// <summary>
    /// 二次贝塞尔 → 直线段（采样两端点连线，或采样多个点取平均方向）。
    /// 返回 a → sampledB(t=1) 的直线段。<see cref="EndPoint"/> = b。
    /// </summary>
    public static LineSegmentData QuadraticToLine(Point a, Point control, Point b)
    {
        return new LineSegmentData { EndPoint = b };
    }

    /// <summary>
    /// 三次贝塞尔采样点（de Casteljau 两层展开）。
    /// </summary>
    public static Point SampleCubic(Point p0, Point p1, Point p2, Point p3, double t)
    {
        double u = 1.0 - t;
        double u2 = u * u, u3 = u2 * u, t2 = t * t, t3 = t2 * t;
        return new Point(
            u3 * p0.X + 3 * u2 * t * p1.X + 3 * u * t2 * p2.X + t3 * p3.X,
            u3 * p0.Y + 3 * u2 * t * p1.Y + 3 * u * t2 * p2.Y + t3 * p3.Y);
    }

    /// <summary>
    /// 三次贝塞尔 → 直线段（返回贝塞尔终点）。<see cref="EndPoint"/> = b。
    /// </summary>
    public static LineSegmentData CubicToLine(Point a, Point c1, Point c2, Point b)
    {
        return new LineSegmentData { EndPoint = b };
    }

    /// <summary>
    /// 二次贝塞尔 → 等效三次贝塞尔（标准提升公式）。
    /// 转换关系：C1 = lerp(control, a, 2/3)，C2 = lerp(control, b, 2/3)。
    /// </summary>
    public static CubicBezierSegmentData QuadraticToCubic(Point a, Point control, Point b)
    {
        double t = 2.0 / 3.0;
        return new CubicBezierSegmentData
        {
            Control1 = new Point(a.X + (control.X - a.X) * t, a.Y + (control.Y - a.Y) * t),
            Control2 = new Point(b.X + (control.X - b.X) * t, b.Y + (control.Y - b.Y) * t),
            EndPoint = b,
        };
    }

    /// <summary>
    /// 三次贝塞尔 → 近似二次贝塞尔（单控制点 = 曲线 t=0.5 处的切线交点）。
    /// 适用于视觉容差内的简化。
    /// </summary>
    public static QuadraticBezierSegmentData CubicToQuadratic(Point p0, Point p1, Point p2, Point p3)
    {
        Point mid = SampleCubic(p0, p1, p2, p3, 0.5);
        double x1 = 2 * mid.X - 0.5 * p0.X - 0.5 * p3.X;
        double y1 = 2 * mid.Y - 0.5 * p0.Y - 0.5 * p3.Y;
        return new QuadraticBezierSegmentData
        {
            ControlPoint = new Point(x1, y1),
            EndPoint = p3,
        };
    }

    // ============================================================
    // 圆角（椭圆弧）互转
    // ============================================================

    // P0 = 圆弧起点，P1 = 圆弧终点，c = 圆心，r = 半径，ccw = 是否逆时针
    // 圆角切点 = 圆心 + r * 沿边的单位法向量（90°偏移）
    // 设边向量 u = P1 - P0，|u| = L，法向量 n = (-uy/L, ux/L)
    // 切点 T = c + r * n

    private static Point ArcTangent(Point center, Point a, double radius, bool ccw)
    {
        double dx = a.X - center.X;
        double dy = a.Y - center.Y;
        // 沿边的单位法向量（逆时针转 90°）
        double len = Math.Sqrt(dx * dx + dy * dy);
        if (len < 1e-10) return a;
        // n = (-dy/len, dx/len)
        // 切点 = center + r * (dx/r, dy/r) + r * n = center + (dx + r*(-dy/len))/r, ...
        // 更直接：切点在 a 到 center 的连线 r 处，再沿切线方向偏移
        // 实际上切点 = a + (r / |a-center|) * (a - center) 沿切线方向的投影
        // 简化：a 就在圆上，T 在 a 基础上沿圆切线方向移动
        // 圆心到 a 的向量就是半径方向，a 到圆心的反方向是内法线
        // 沿边的切线方向 = (-dy/len, dx/len) * sign(ccw)
        double nx = ccw ? -dy / len : dy / len;
        double ny = ccw ? dx / len : -dx / len;
        // 确保 T 在 a 的圆切线方向上偏移 r
        // 用 a 作为圆上点，偏移 r 到切点
        // T = a + r * tangent_n
        return new Point(a.X + radius * nx, a.Y + radius * ny);
    }

    /// <summary>
    /// 圆角 → 直线段（返回圆角起止点）。
    /// </summary>
    public static LineSegmentData ArcToLine(Point arcStart, Point arcEnd)
    {
        return new LineSegmentData { EndPoint = arcEnd };
    }

    /// <summary>
    /// 圆角 → 二次贝塞尔（控制点 = 圆弧中点）。
    /// </summary>
    public static QuadraticBezierSegmentData ArcToQuadratic(Point arcStart, Point arcEnd, Point arcMid)
    {
        return new QuadraticBezierSegmentData
        {
            ControlPoint = arcMid,
            EndPoint = arcEnd,
        };
    }

    /// <summary>
    /// 圆角 → 三次贝塞尔（两控制点为圆弧三等分点）。
    /// </summary>
    public static CubicBezierSegmentData ArcToCubic(Point arcStart, Point arcEnd, Point control1, Point control2)
    {
        return new CubicBezierSegmentData
        {
            Control1 = control1,
            Control2 = control2,
            EndPoint = arcEnd,
        };
    }

    /// <summary>
    /// 圆角（已知圆心、半径、起止角）→ 二次贝塞尔（等分采样）。
    /// </summary>
    public static QuadraticBezierSegmentData ArcToQuadraticByAngle(
        double cx, double cy, double radius, double angleStart, double angleEnd, bool ccw)
    {
        double tMid = 0.5;
        double midAngle = ccw ? angleStart + (angleEnd - angleStart) * tMid : angleStart - (angleStart - angleEnd) * tMid;
        // 确保角度在 [0, 2π) 范围内
        midAngle = ((midAngle % (2 * Math.PI)) + 2 * Math.PI) % (2 * Math.PI);

        double endX = cx + radius * Math.Cos(angleEnd);
        double endY = cy + radius * Math.Sin(angleEnd);
        double midX = cx + radius * Math.Cos(midAngle);
        double midY = cy + radius * Math.Sin(midAngle);

        return new QuadraticBezierSegmentData
        {
            ControlPoint = new Point(midX, midY),
            EndPoint = new Point(endX, endY),
        };
    }

    /// <summary>
    /// 圆角（已知圆心、半径、起止角）→ 三次贝塞尔（两控制点三等分采样）。
    /// </summary>
    public static CubicBezierSegmentData ArcToCubicByAngle(
        double cx, double cy, double radius, double angleStart, double angleEnd, bool ccw)
    {
        double t1 = 1.0 / 3.0;
        double t2 = 2.0 / 3.0;

        double delta = angleEnd - angleStart;
        if (!ccw && delta > 0) delta -= 2 * Math.PI;
        if (ccw && delta < 0) delta += 2 * Math.PI;

        double mid1 = angleStart + delta * t1;
        double mid2 = angleStart + delta * t2;

        double endX = cx + radius * Math.Cos(angleEnd);
        double endY = cy + radius * Math.Sin(angleEnd);
        double c1X = cx + radius * Math.Cos(mid1);
        double c1Y = cy + radius * Math.Sin(mid1);
        double c2X = cx + radius * Math.Cos(mid2);
        double c2Y = cy + radius * Math.Sin(mid2);

        return new CubicBezierSegmentData
        {
            Control1 = new Point(c1X, c1Y),
            Control2 = new Point(c2X, c2Y),
            EndPoint = new Point(endX, endY),
        };
    }
}

/// <summary>
/// 一段完整图形路径流（起点 → 终点）的统一抽象容器。
///   - <see cref="OriginMove"/>：全局起点（M 指令坐标）。
///   - <see cref="Segments"/>：所有指令（M / L / H / V / A / Q / C）。
///   - <see cref="IsClosed"/>：是否闭合（影响末尾是否加 Z）。
///   - <see cref="ShapeKind"/>：图形分类（Line / Rect / Bezier ...）。
///
/// 该容器是所有几何构建的"通用中间表示"：
///   - 通过 <see cref="ToAvaloniaGeometry"/> 还原为 Avalonia <see cref="PathGeometry"/>
///   - 通过 <see cref="ToSvgPath"/> 输出 SVG `d` 字符串
///   - 通过 <see cref="FromAvalonia"/> 从 PathGeometry 反序列化（重绘 / 格式转换）
/// </summary>
public sealed class ShapeDrawStream
{
    /// <summary>全局起点（M 指令坐标），与 Segments[0] 为 Move 时一致。</summary>
    public Point OriginMove { get; set; }

    public List<PathSegment> Segments { get; } = new();

    /// <summary>是否闭合（影响 ToAvaloniaGeometry 的 IsClosed 与 ToSvgPath 末尾的 Z）。</summary>
    public bool IsClosed { get; set; }

    /// <summary>图形分类。</summary>
    public ShapeKind ShapeType { get; set; } = ShapeKind.Unknown;

    /// <summary>调试 / 标注用元数据：每个原始顶点对应的"圆角圆心"。仅 N 顶点圆角图形会填充。</summary>
    public IReadOnlyList<Point>? CornerCenters { get; set; }

    /// <summary>调试 / 标注用：每个原始顶点对应的"弧入切点"。</summary>
    public IReadOnlyList<Point>? ArcStarts { get; set; }

    /// <summary>调试 / 标注用：每个原始顶点对应的"弧出切点"。</summary>
    public IReadOnlyList<Point>? ArcEnds { get; set; }

    // ============================================================
    // 流式构造助手 —— 让 builder 写起来跟 SVG `d` 一样直观
    // ============================================================

    public ShapeDrawStream MoveTo(Point p)
    {
        if (Segments.Count == 0) OriginMove = p;
        Segments.Add(new MoveSegment { EndPoint = p });
        return this;
    }

    public ShapeDrawStream LineTo(Point p)
    {
        Segments.Add(new LineSegmentData { EndPoint = p });
        return this;
    }

    // 参数说明：
    // to：椭圆弧段的终点
    // rx：椭圆弧段的X轴半径
    // ry：椭圆弧段的Y轴半径
    // large：椭圆弧段是否为大弧段
    // sweep：椭圆弧段的扫描方向
    // center：椭圆弧段的圆心
    // tangentStart：椭圆弧段的入切点
    // tangentEnd：椭圆弧段的出切点
    public ShapeDrawStream ArcTo(
        Point to, double rx, double ry, bool large, SweepDirection sweep,
        Point? center = null, Point? tangentStart = null, Point? tangentEnd = null)
    {
        Segments.Add(new ArcSegmentData
        {
            EndPoint = to,
            RadiusX = rx,
            RadiusY = ry,
            IsLargeArc = large,
            SweepDir = sweep,
            ArcCenter = center,
            ArcTangentStart = tangentStart,
            ArcTangentEnd = tangentEnd,
        });
        return this;
    }

    public ShapeDrawStream ArcTo(
        Point to, double rx, double ry, bool large, bool sweepClockwise)
        => ArcTo(to, rx, ry, large, sweepClockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise);

    public ShapeDrawStream QuadraticTo(Point control, Point to)
    {
        Segments.Add(new QuadraticBezierSegmentData { ControlPoint = control, EndPoint = to });
        return this;
    }

    public ShapeDrawStream CubicTo(Point c1, Point c2, Point to)
    {
        Segments.Add(new CubicBezierSegmentData { Control1 = c1, Control2 = c2, EndPoint = to });
        return this;
    }

    // ============================================================
    // 序列化
    // ============================================================

    /// <summary>把当前 ShapeDrawStream 还原为 Avalonia <see cref="PathGeometry"/>。</summary>
    public PathGeometry ToAvaloniaGeometry()
    {
        var geo = new PathGeometry();
        if (Segments.Count == 0) return geo;

        PathFigure? fig = null;
        Point currentPoint = OriginMove;

        for (int i = 0; i < Segments.Count; i++)
        {
            var s = Segments[i];
            switch (s)
            {
                case MoveSegment m:
                    if (fig != null) geo.Figures!.Add(fig);
                    fig = new PathFigure { StartPoint = m.EndPoint, IsClosed = IsClosed };
                    currentPoint = m.EndPoint;
                    break;

                case LineSegmentData l:
                    EnsureFigure(ref fig, currentPoint);
                    fig!.Segments!.Add(new LineSegment { Point = l.EndPoint });
                    currentPoint = l.EndPoint;
                    break;

                case ArcSegmentData a:
                    EnsureFigure(ref fig, currentPoint);
                    fig!.Segments!.Add(new ArcSegment
                    {
                        Point = a.EndPoint,
                        Size = new Size(Math.Max(0, a.RadiusX), Math.Max(0, a.RadiusY)),
                        RotationAngle = a.RotationAngle,
                        IsLargeArc = a.IsLargeArc,
                        SweepDirection = a.SweepDir,
                    });
                    currentPoint = a.EndPoint;
                    break;

                case QuadraticBezierSegmentData q:
                    EnsureFigure(ref fig, currentPoint);
                    fig!.Segments!.Add(new QuadraticBezierSegment { Point1 = q.ControlPoint, Point2 = q.EndPoint });
                    currentPoint = q.EndPoint;
                    break;

                case CubicBezierSegmentData c:
                    EnsureFigure(ref fig, currentPoint);
                    fig!.Segments!.Add(new BezierSegment { Point1 = c.Control1, Point2 = c.Control2, Point3 = c.EndPoint });
                    currentPoint = c.EndPoint;
                    break;
            }
        }
        if (fig != null) geo.Figures!.Add(fig);
        return geo;

        static void EnsureFigure(ref PathFigure? fig, Point startIfNeeded)
        {
            if (fig == null)
                fig = new PathFigure { StartPoint = startIfNeeded, IsClosed = false };
        }
    }

    /// <summary>输出 SVG `d` 属性字符串，可与 ToAvaloniaGeometry 互换用于跨平台渲染。</summary>
    public string ToSvgPath()
    {
        if (Segments.Count == 0) return string.Empty;

        var ci = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        Point currentPoint = OriginMove;

        for (int i = 0; i < Segments.Count; i++)
        {
            var s = Segments[i];
            switch (s)
            {
                case MoveSegment m:
                    sb.Append('M').Append(Fmt(m.EndPoint.X)).Append(',').Append(Fmt(m.EndPoint.Y));
                    currentPoint = m.EndPoint;
                    break;

                case LineSegmentData l:
                    sb.Append('L').Append(Fmt(l.EndPoint.X)).Append(',').Append(Fmt(l.EndPoint.Y));
                    currentPoint = l.EndPoint;
                    break;

                case ArcSegmentData a:
                    sb.Append('A')
                      .Append(Fmt(a.RadiusX)).Append(',')
                      .Append(Fmt(a.RadiusY)).Append(',')
                      .Append(Fmt(a.RotationAngle)).Append(',')
                      .Append(a.IsLargeArc ? '1' : '0').Append(',')
                      .Append(a.SweepDir == SweepDirection.Clockwise ? '1' : '0').Append(',')
                      .Append(Fmt(a.EndPoint.X)).Append(',')
                      .Append(Fmt(a.EndPoint.Y));
                    currentPoint = a.EndPoint;
                    break;

                case QuadraticBezierSegmentData q:
                    sb.Append('Q')
                      .Append(Fmt(q.ControlPoint.X)).Append(',').Append(Fmt(q.ControlPoint.Y)).Append(' ')
                      .Append(Fmt(q.EndPoint.X)).Append(',').Append(Fmt(q.EndPoint.Y));
                    currentPoint = q.EndPoint;
                    break;

                case CubicBezierSegmentData c:
                    sb.Append('C')
                      .Append(Fmt(c.Control1.X)).Append(',').Append(Fmt(c.Control1.Y)).Append(' ')
                      .Append(Fmt(c.Control2.X)).Append(',').Append(Fmt(c.Control2.Y)).Append(' ')
                      .Append(Fmt(c.EndPoint.X)).Append(',').Append(Fmt(c.EndPoint.Y));
                    currentPoint = c.EndPoint;
                    break;
            }
        }

        if (IsClosed)
        {
            sb.Append('Z');
        }

        return sb.ToString();

        string Fmt(double v) => v.ToString("0.###", ci);
    }

    /// <summary>从已有 <see cref="PathGeometry"/> 反序列化为 ShapeDrawStream（用于格式互转 / 重绘）。</summary>
    public static ShapeDrawStream FromAvalonia(PathGeometry geo, ShapeKind kind = ShapeKind.Unknown)
    {
        var stream = new ShapeDrawStream { ShapeType = kind };
        if (geo.Figures == null) return stream;

        var first = geo.Figures.FirstOrDefault();
        if (first != null)
        {
            stream.OriginMove = first.StartPoint;
            stream.IsClosed = first.IsClosed;
            stream.MoveTo(first.StartPoint);
        }

        if (first?.Segments == null) return stream;
        foreach (var seg in first.Segments)
        {
            switch (seg)
            {
                case LineSegment ls:
                    stream.LineTo(ls.Point);
                    break;
                case ArcSegment arc:
                    stream.ArcTo(
                        arc.Point,
                        arc.Size.Width,
                        arc.Size.Height,
                        arc.IsLargeArc,
                        arc.SweepDirection);
                    break;
                case QuadraticBezierSegment qb:
                    stream.QuadraticTo(qb.Point1, qb.Point2);
                    break;
                case BezierSegment cb:
                    stream.CubicTo(cb.Point1, cb.Point2, cb.Point3);
                    break;
            }
        }
        return stream;
    }
}

#endregion

#region 基础图形

/// <summary>
/// 直线 / 多段线 / 多边形 builder。
/// 折点 ≥2 ⇒ Polyline，>=3 + IsClosed ⇒ Polygon。
/// </summary>
public static class PolylineBuilder
{
    public static ShapeDrawStream BuildLine(Point a, Point b)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.Line, IsClosed = false };
        s.MoveTo(a).LineTo(b);
        return s;
    }

    public static ShapeDrawStream BuildPolyline(IReadOnlyList<Point> pts)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.Polyline, IsClosed = false };
        if (pts == null || pts.Count == 0) return s;
        s.MoveTo(pts[0]);
        for (int i = 1; i < pts.Count; i++) s.LineTo(pts[i]);
        return s;
    }

    public static ShapeDrawStream BuildPolygon(IReadOnlyList<Point> pts)
    {
        var s = BuildPolyline(pts);
        s.ShapeType = ShapeKind.Polygon;
        s.IsClosed = true;
        return s;
    }
}

/// <summary>
/// 矩形 builder（轴对齐 4 顶点 L 序列）。
/// </summary>
public static class RectBuilder
{
    public static ShapeDrawStream BuildRect(double x, double y, double width, double height)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.Rect, IsClosed = true };
        s.MoveTo(new Point(x, y))
         .LineTo(new Point(x + width, y))
         .LineTo(new Point(x + width, y + height))
         .LineTo(new Point(x, y + height));
        return s;
    }
}

/// <summary>
/// 椭圆 / 圆 / 圆弧 builder（基于两条半弧闭合 → 整圆，单条主弧 → 开放弧）。
/// 角点圆角联动算法沿用原 EllipseArcGeometry。
///   - start==sweep==360 ⇨ 完整椭圆（用 4 个圆心对齐点开 2 subpath 也 OK，但单 subpath 更简）
///   - 其余 ⇨ 圆弧 / 扇形 / 圆环
///   - innerRadius > 0 ⇨ 椭圆环 / 扇环（带空洞）
///   - cornerRadius > 0 ⇨ 尖角圆角化
///   - 默认 IsClosed=true（圆/环/扇形/扇环）；sweep < 360 可设 IsClosed=false 做开放弧
/// </summary>
public static class EllipseArcBuilder
{
    public static ShapeDrawStream Build(
        double x, double y, double width, double height,
        double startAngleDeg, double sweepAngleDeg, double innerRatio,
        double cornerRadius = 0, bool isClosed = true)
        => EllipseArcPathBuilder.Build(
            x, y, width, height,
            startAngleDeg, sweepAngleDeg, innerRatio,
            cornerRadius, isClosed);
}

/// <summary>
/// 贝塞尔路径 builder —— 给定控制点序列直接拼装。
///   - 折点 + 控制点配对：偶数索引=控制点 [c1, c2]，奇数索引=曲线终点
///   - 或者全三次：每 3 点（c1, c2, end）一组
/// </summary>
public static class BezierBuilder
{
    /// <summary>三次贝塞尔折线：pts 必须 3 的倍数，每 3 点为 (c1, c2, end)。</summary>
    public static ShapeDrawStream BuildCubicPath(IReadOnlyList<Point> c1c2EndTriples, bool closed = false)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.BezierPath, IsClosed = closed };
        if (c1c2EndTriples == null || c1c2EndTriples.Count == 0) return s;
        if (c1c2EndTriples.Count % 3 != 0) return s;
        s.MoveTo(c1c2EndTriples[2]);
        for (int i = 3; i < c1c2EndTriples.Count; i += 3)
            s.CubicTo(c1c2EndTriples[i - 2], c1c2EndTriples[i - 1], c1c2EndTriples[i]);
        return s;
    }

    /// <summary>三次贝塞尔 S 形：从 anchorStart → c1, c2 → anchorEnd 一条曲线。</summary>
    public static ShapeDrawStream BuildSingleCubic(Point anchorStart, Point c1, Point c2, Point anchorEnd, bool closed = false)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.BezierPath, IsClosed = closed };
        s.MoveTo(anchorStart).CubicTo(c1, c2, anchorEnd);
        return s;
    }

    /// <summary>二次贝塞尔折线：每 2 点为 (control, end)。</summary>
    public static ShapeDrawStream BuildQuadraticPath(IReadOnlyList<Point> controlEndPairs, bool closed = false)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.BezierPath, IsClosed = closed };
        if (controlEndPairs == null || controlEndPairs.Count == 0) return s;
        if (controlEndPairs.Count % 2 != 0) return s;
        s.MoveTo(controlEndPairs[1]);
        for (int i = 2; i < controlEndPairs.Count; i += 2)
            s.QuadraticTo(controlEndPairs[i - 1], controlEndPairs[i]);
        return s;
    }

    /// <summary>二次贝塞尔 S 形：从 anchorStart → control → anchorEnd。</summary>
    public static ShapeDrawStream BuildSingleQuadratic(Point anchorStart, Point control, Point anchorEnd, bool closed = false)
    {
        var s = new ShapeDrawStream { ShapeType = ShapeKind.BezierPath, IsClosed = closed };
        s.MoveTo(anchorStart).QuadraticTo(control, anchorEnd);
        return s;
    }
}

#endregion

#region 圆角图形（专用 builder，逻辑保留原算法）

/// <summary>
/// N 顶点圆角多边形 / 星形 / 矩形几何构建器。
///
/// 算法（夹角平分线 + uIn×uOut 局部 sweep）：
///   uIn   = normalize(prev - v)
///   uOut  = normalize(next - v)
///   α     = acos(uIn · uOut)
///   bisect= normalize(uIn + uOut)
///   dC    = r / sin(α/2)
///   dT    = r / tan(α/2)
///   center    = v + bisect * dC
///   arcStart  = v + uIn  * dT
///   arcEnd    = v + uOut * dT
///   sweep 由 uIn × uOut 局部叉积符号决定
/// 适用于任意 ≥3 顶点的相邻序列（凸、凹、星形都成立）。
/// </summary>
public static class RoundedPathBuilder
{
    public static ShapeDrawStream Build(IReadOnlyList<Point> pts, IReadOnlyList<double> radii)
        => Build(pts, radii, out _, out _, out _);

    /// <summary>
    /// 构建 ShapeDrawStream 同时输出每个顶点的圆心 / 弧入切点 / 弧出切点（用于调试叠加）。
    /// </summary>
    public static ShapeDrawStream Build(
        IReadOnlyList<Point> pts,
        IReadOnlyList<double> radii,
        out Point[] cornerCenters,
        out Point[] arcStarts,
        out Point[] arcEnds)
    {
        cornerCenters = Array.Empty<Point>();
        arcStarts = Array.Empty<Point>();
        arcEnds = Array.Empty<Point>();

        int n = pts.Count;
        if (n < 3 || radii.Count != n)
            return new ShapeDrawStream { ShapeType = ShapeKind.RoundedPolygon };

        // 圆心、弧入切点、弧出切点
        var centers = new Point[n];
        var aStart = new Point[n];
        var aEnd = new Point[n];

        for (int i = 0; i < n; i++)
        {
            var prev = pts[(i + n - 1) % n];
            var v = pts[i];
            var next = pts[(i + 1) % n];
            // 入出切向量, 原理: 夹角平分线 + uIn×uOut 局部 sweep
            var uIn = Normalize(Sub(prev, v));
            var uOut = Normalize(Sub(next, v));
            // 入出切向量点积, 原理: 夹角余弦
            double dot = Clamp(uIn.X * uOut.X + uIn.Y * uOut.Y, -1, 1);
            // 夹角一半, 原理: 夹角余弦
            double half = Math.Acos(dot) * 0.5;
            // 正弦和正切, 原理: 夹角余弦
            double s = Math.Max(Math.Sin(half), 1e-4);
            double t = Math.Max(Math.Tan(half), 1e-4);

            // 半径
            double r = Math.Max(0, radii[i]);
            // 圆心到顶点距离
            double dCenter = r / s;
            // 切点到顶点距离
            double dTangent = r / t;
            // 夹角平分线
            var bisect = Normalize(Add(uIn, uOut));
            // 圆心
            centers[i] = AddPoint(v, Mul(bisect, dCenter));
            // 弧入切点
            aStart[i] = AddPoint(v, Mul(uIn, dTangent));
            // 弧出切点
            aEnd[i] = AddPoint(v, Mul(uOut, dTangent));
        }

        cornerCenters = centers;
        arcStarts = aStart;
        arcEnds = aEnd;

        var stream = new ShapeDrawStream
        {
            ShapeType = ShapeKind.RoundedPolygon,
            IsClosed = true,
            CornerCenters = centers,
            ArcStarts = aStart,
            ArcEnds = aEnd,
        };
        stream.MoveTo(aStart[0]);

        for (int i = 0; i < n; i++)
        {
            var prev = pts[(i + n - 1) % n];
            var v = pts[i];
            var next = pts[(i + 1) % n];
            var uIn = Normalize(Sub(prev, v));
            var uOut = Normalize(Sub(next, v));
            double cross = uIn.X * uOut.Y - uIn.Y * uOut.X;
            double r = Math.Max(0, radii[i]);

            if (r > 0.0001)
            {
                stream.ArcTo(
                    aEnd[i], r, r, large: false,
                    sweep: cross <= 0 ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
                    center: centers[i],
                    tangentStart: aStart[i],
                    tangentEnd: aEnd[i]);
            }
            stream.LineTo(aStart[(i + 1) % n]);
        }
        return stream;
    }

    /// <summary>4 顶点矩形特例（自动识别为 RoundedRect）。</summary>
    public static ShapeDrawStream BuildRoundedRectangle(
        double x, double y, double width, double height, double[] radii)
    {
        var pts = new[]
        {
            new Point(x, y),
            new Point(x + width, y),
            new Point(x + width, y + height),
            new Point(x, y + height),
        };
        var s = Build(pts, radii);
        s.ShapeType = ShapeKind.RoundedRect;
        return s;
    }

    // ============================================================
    // 向量 / 数值辅助（与原 StarGeometry 同名同义，集中管理便于复用）
    // ============================================================

    internal static Vector Sub(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    internal static Vector Add(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);
    internal static Vector Mul(Vector a, double s) => new(a.X * s, a.Y * s);
    internal static Point AddPoint(Point a, Vector b) => new(a.X + b.X, a.Y + b.Y);
    internal static Vector Normalize(Vector v)
    {
        var len = Math.Sqrt(v.X * v.X + v.Y * v.Y);
        if (len < 1e-6) return new Vector(0, 0);
        return new Vector(v.X / len, v.Y / len);
    }
    internal static double Clamp(double v, double min, double max)
    {
        if (v < min) return min;
        if (v > max) return max;
        return v;
    }
}

/// <summary>
/// 椭圆 / 弧 / 扇形 / 圆环 / 扇环 几何构建器。
///
/// ─────────────────────────────────────────────────────────────────
/// 几何模型（图示以"扇形"为例，圆环 = 扇形 × 2 闭合）
/// ─────────────────────────────────────────────────────────────────
///
/// 输入：(cx, cy) 椭圆中心, (rx, ry) 半轴, startAngle 起始角度(0°=右), sweepAngle 扫描角(>0 逆时针),
///        innerRatio 内/外半径比 (0=实心), cornerRadius 尖角圆角半径。
///
/// 设起始点为 A0，终止点为 A1。从 A0 沿外圈椭圆逆时针扫到 A1 形成一条"圆弧"，
/// 在圆弧两端各有一个尖角（指 A0、A1 与中心 cx,cy 的连线和圆弧相交产生的不连续点）。
///
///            A0 (start, 尖角)
///           ╱
///       ╱  ╲                外圈椭圆
///     ╱     ╲
///   ╱        ╲
///  ●───────────●  cx,cy   ← 中心（实心扇形角点）
///   ╲        ╱
///     ╲     ╱
///       ╲  ╱
///           ╲
///            A1 (end, 尖角)
///
/// 圆角化后：尖角 A0/A1 退化成一段圆弧，A0 处圆角与 A1 处圆角之间是原椭圆主弧。
/// 扇形"中心尖角"是 cx,cy 到 A0/A1 的两条径向边相交形成的尖角，center 圆弧对它做圆角化。
///
/// ─────────────────────────────────────────────────────────────────
/// 算法总览（沿用 SolveCorner 公式，与 RoundedPathBuilder 同源）
/// ─────────────────────────────────────────────────────────────────
///
/// 1. 把每个尖角建模为"两条边的交点 + 圆角圆心 c'"
///      L1 = 径向边（尖角 → 椭圆中心），长度 |L1| = rTangentA0 / radialLen
///      L2 = 椭圆主弧在尖角处的切线（外圈），长度 rTangent
///      L1 与 L2 交于点 intersection
///      角平分线 bis = normalize(L1 ± L2)（± 由"外圈 vs 内圈、起始 vs 终止"决定）
///      c' = (L2 切点垂线) ∩ bis    ← 这就是圆角圆弧的圆心
///      rActual = |c' - tArc|        ← 圆角半径
///      tRadial = c' 在 L1 上的投影  ← 圆角与径向边的衔接点
///
/// 2. 自适应求圆角半径（不让圆角溢出尖角区域）
///      arcOffsetA = min(R, min(chordHalf, radHalf))
///        chordHalf = 弦长一半（保证圆角起点 tArc 还在椭圆上）
///        radHalf  = 径向边一半（保证圆角终点 tRadial 还在径向边内）
///      真实半径 rActual 在 SolveCorner 内重算，因为圆角圆心 c' 与尖角中心的几何关系
///      决定了实际能达到的半径上界。
///
/// 3. center 圆弧（扇形/扇环专属）
///      cx,cy 是外圈两条径向边 L1A0、L1A1 的交点，把它也当成"尖角"调用同套圆角公式。
///      半径联动：rMaxFromCorners = min(cornerRadiusA0, cornerRadiusA1)；
///               实际中心圆角 = min(R, rMaxFromCorners)
///      保证中心圆角不超过两端圆角，避免圆角相互侵入。
///
/// ─────────────────────────────────────────────────────────────────
/// 输出
/// ─────────────────────────────────────────────────────────────────
/// 一段 <see cref="ShapeDrawStream"/>：起点 = A0 径向衔接点，
/// 顺序：A0 圆角 → 外圈主弧 → A1 圆角 → [center 圆角 or 内圈镜像路径]。
/// </summary>
public static class EllipseArcPathBuilder
{
    /// <summary>
    /// 构建椭圆/弧/扇形/圆环/扇环路径。
    /// </summary>
    /// <param name="x,y,width,height">外接矩形。</param>
    /// <param name="startAngleDeg">起始角度（度，0°=右侧，90°=上）。</param>
    /// <param name="sweepAngleDeg">扫描角度（度，>0 逆时针，&lt;0 顺时针）。绝对值 ≥360 视为整圆。</param>
    /// <param name="innerRatio">内/外半径比。0=实心，(0,1)=圆环/扇环。</param>
    /// <param name="cornerRadius">尖角圆角半径。</param>
    /// <param name="isClosed">true=扇形/圆环(返回闭合 subpath)；false=开放圆弧(返回开放式 subpath)。</param>
    public static ShapeDrawStream Build(
        double x, double y, double width, double height,
        double startAngleDeg, double sweepAngleDeg, double innerRatio,
        double cornerRadius = 0, bool isClosed = true)
    {
        // ────────────── 1. 解析输入参数 ──────────────
        double sweep = RoundedPathBuilder.Clamp(sweepAngleDeg, -360, 360);
        double inner = RoundedPathBuilder.Clamp(innerRatio, 0, 0.9999);

        double cx = x + width * 0.5;   // 椭圆中心 X
        double cy = y + height * 0.5;  // 椭圆中心 Y
        double rx = width * 0.5;       // X 半轴
        double ry = height * 0.5;      // Y 半轴

        var kind = (inner <= 0 && Math.Abs(sweep) >= 360)
            ? ShapeKind.Ellipse
            : ShapeKind.Arc;
        var stream = new ShapeDrawStream { ShapeType = kind, IsClosed = isClosed };

        // 退化形状：宽度/高度为 0 或扫描角为 0 → 空白图形
        if (Math.Abs(sweep) < 1e-6 || rx <= 0 || ry <= 0)
            return stream;

        double R = Math.Max(0, cornerRadius);  // 用户指定的尖角圆角半径

        double startRad = startAngleDeg * Math.PI / 180.0;
        double endRad = startRad + sweep * Math.PI / 180.0;

        // ────────────── 1a. 整圆特例：无尖角，圆角参数无效 ──────────────
        if (Math.Abs(sweep) >= 360.0)
        {
            BuildFullEllipse(stream, cx, cy, rx, ry, inner, sweep > 0, isClosed);
            return stream;
        }

        bool ccw = sweep > 0;
        // 外圈主弧方向 = sweep 方向；
        // 内圈主弧方向（圆环/扇环时）= 与外圈相反（保证内外圈互相"咬合"，形成闭环孔洞）
        SweepDirection outerMainSweep = ccw ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        SweepDirection innerMainSweep = ccw ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;

        // ────────────── 2. 计算外/内圈尖角的"几何原料" ──────────────
        //
        // 每个尖角需要这些数据才能求解圆角：
        //   cornerTip    : 尖角点 = 椭圆上 startRad/endRad 处的坐标
        //   rTangent     : 椭圆切线在该点的"单位弧长"，用于把弧长→角度（arcOffset / rTangent = 角度偏移）
        //   radialLen    : 尖角到椭圆中心的距离（径向边长），约束圆角不能超出径向边
        //   arcOffset    : 自适应后的圆角半径上界

        // 外圈
        Point outerCornerA0 = new(cx + rx * Math.Cos(startRad), cy - ry * Math.Sin(startRad));
        Point outerCornerA1 = new(cx + rx * Math.Cos(endRad), cy - ry * Math.Sin(endRad));
        double outerTangentLenA0 = EllipseTangentLength(rx, ry, startRad);
        double outerTangentLenA1 = EllipseTangentLength(rx, ry, endRad);
        double outerRadialLenA0 = Distance(outerCornerA0, new Point(cx, cy));
        double outerRadialLenA1 = Distance(outerCornerA1, new Point(cx, cy));
        // arcOffsetA* = 圆角半径上界，clamp 到 "切线半弦" 和 "径向半边" 之内，离尖角的距离，即所谓的半径。
        double arcOffsetA0 = ComputeCornerDelta(R, outerTangentLenA0, outerRadialLenA0);
        double arcOffsetA1 = ComputeCornerDelta(R, outerTangentLenA1, outerRadialLenA1);

        // 内圈（innerRatio=0 时全部置零并指向中心，标记"无内圈"）
        double innerRx = rx * inner, innerRy = ry * inner;
        Point innerCornera0 = inner > 0 ? new Point(cx + innerRx * Math.Cos(startRad), cy - innerRy * Math.Sin(startRad)) : new Point(cx, cy);
        Point innerCornera1 = inner > 0 ? new Point(cx + innerRx * Math.Cos(endRad), cy - innerRy * Math.Sin(endRad)) : new Point(cx, cy);
        double innerTangentLenA0 = inner > 0 ? EllipseTangentLength(innerRx, innerRy, startRad) : 0;
        double innerTangentLenA1 = inner > 0 ? EllipseTangentLength(innerRx, innerRy, endRad) : 0;
        double innerRadialLenA0 = inner > 0 ? Distance(innerCornera0, new Point(cx, cy)) : 0;
        double innerRadialLenA1 = inner > 0 ? Distance(innerCornera1, new Point(cx, cy)) : 0;
        double arcOffseta0 = inner > 0 ? ComputeCornerDelta(R, innerTangentLenA0, innerRadialLenA0) : 0;
        double arcOffseta1 = inner > 0 ? ComputeCornerDelta(R, innerTangentLenA1, innerRadialLenA1) : 0;

        // ────────────── 3. 弧长 → 角度偏移（圆角起止点对应的椭圆参数角） ──────────────
        // Arc是弧，Radial 是径向，是径向线。
        // 椭圆参数化是 (rx*cosθ, -ry*sinθ)。对于椭圆，弧长 ≠ 角度，
        // 真正的切线长度 ds/dθ = sqrt((rx·sinθ)² + (ry·cosθ)²)，
        // 所以"沿椭圆切线后退 rTangent"对应的角度偏移 = arcOffset / rTangent。根据公式，rTangent是切线长度，arcOffset是半径，所以角度偏移是半径除以切线长度，平分角的余弦。
        double angleOffsetA0 = arcOffsetA0 / Math.Max(outerTangentLenA0, 1e-9);
        double angleOffsetA1 = arcOffsetA1 / Math.Max(outerTangentLenA1, 1e-9);
        double angleOffseta0 = inner > 0 ? arcOffseta0 / Math.Max(innerTangentLenA0, 1e-9) : 0;
        double angleOffseta1 = inner > 0 ? arcOffseta1 / Math.Max(innerTangentLenA1, 1e-9) : 0;

        // 圆角段两端在椭圆上的参数角（ccw 时向 sweep 方向推，cw 时反向）
        double outerArcAngleA0 = ccw ? startRad + angleOffsetA0 : startRad - angleOffsetA0;
        double outerArcAngleA1 = ccw ? endRad - angleOffsetA1 : endRad + angleOffsetA1;
        double innerArcAnglea0 = ccw ? startRad + angleOffseta0 : startRad - angleOffseta0;
        double innerArcAnglea1 = ccw ? endRad - angleOffseta1 : endRad + angleOffseta1;

        // 圆角段两端在椭圆上的实际坐标（圆角弧的起/止点），根据上面计算出来的余弦值来计算弧上的点。
        Point outerArcPointA0 = new(cx + rx * Math.Cos(outerArcAngleA0), cy - ry * Math.Sin(outerArcAngleA0));
        Point outerArcPointA1 = new(cx + rx * Math.Cos(outerArcAngleA1), cy - ry * Math.Sin(outerArcAngleA1));
        Point innerArcPointa0 = inner > 0
            ? new Point(cx + innerRx * Math.Cos(innerArcAnglea0), cy - innerRy * Math.Sin(innerArcAnglea0))
            : new Point(cx, cy);
        Point innerArcPointa1 = inner > 0
            ? new Point(cx + innerRx * Math.Cos(innerArcAnglea1), cy - innerRy * Math.Sin(innerArcAnglea1))
            : new Point(cx, cy);

        // ────────────── 4. 求解外/内圈尖角圆角的几何结果 ──────────────
        //
        // hintA*：尖角径向偏移后的"目标点"，用于 SolveCorner 内部纠正角平分线方向
        //   （椭圆切线不像直线那样对称，必须用 hint 提示"bis 应指向哪一侧"）
        Point hintA0 = MoveRadial(outerCornerA0, cx, cy, -arcOffsetA0);
        Point hintA1 = MoveRadial(outerCornerA1, cx, cy, -arcOffsetA1);
        Point hinta0 = inner > 0 ? MoveRadial(innerCornera0, cx, cy, +arcOffseta0) : new Point(cx, cy);
        Point hinta1 = inner > 0 ? MoveRadial(innerCornera1, cx, cy, +arcOffseta1) : new Point(cx, cy);

        // 求解外圈尖角圆角
        var sA0 = SolveCorner(cx, cy, outerCornerA0, rx, ry, outerArcAngleA0, isStartAngle: true, isOuter: true, tRadialHint: hintA0);
        var sA1 = SolveCorner(cx, cy, outerCornerA1, rx, ry, outerArcAngleA1, isStartAngle: false, isOuter: true, tRadialHint: hintA1);

        // 外圈尖角圆角结果
        //   outerRadialPointA* : 圆角与径向边的衔接点 tRadial（即圆角弧终点）
        //   cornerRadiusA*     : 实际圆角半径（可能 < R，因几何约束被压缩）
        //   outerCornerSweepA* : 圆角弧扫描方向
        // 计算径向衔接点
        Point outerRadialPointA0 = sA0.Valid ? sA0.TRadial : outerCornerA0;
        Point outerRadialPointA1 = sA1.Valid ? sA1.TRadial : outerCornerA1;
        double cornerRadiusA0 = sA0.Valid ? sA0.RActual : 0;
        double cornerRadiusA1 = sA1.Valid ? sA1.RActual : 0;
        SweepDirection outerCornerSweepA0 = sA0.Valid ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        SweepDirection outerCornerSweepA1 = sA1.Valid ? sA1.Sweep : SweepDirection.CounterClockwise;

        // 内圈尖角圆角结果（仅 inner>0 时有效）
        SolveCornerGeometry sA0Inner = default, sA1Inner = default;
        Point innerRadialPointa0 = new(cx, cy);
        Point innerRadialPointa1 = new(cx, cy);
        double cornerRadiusa0 = 0, cornerRadiusa1 = 0;
        SweepDirection innerCornerSweepA0 = SweepDirection.CounterClockwise;
        SweepDirection innerCornerSweepA1 = SweepDirection.CounterClockwise;
        if (inner > 0)
        {
            sA0Inner = SolveCorner(cx, cy, innerCornera0, innerRx, innerRy, innerArcAnglea0, isStartAngle: true, isOuter: false, tRadialHint: hinta0);
            sA1Inner = SolveCorner(cx, cy, innerCornera1, innerRx, innerRy, innerArcAnglea1, isStartAngle: false, isOuter: false, tRadialHint: hinta1);
            innerRadialPointa0 = sA0Inner.Valid ? sA0Inner.TRadial : innerCornera0;
            innerRadialPointa1 = sA1Inner.Valid ? sA1Inner.TRadial : innerCornera1;
            cornerRadiusa0 = sA0Inner.Valid ? sA0Inner.RActual : 0;
            cornerRadiusa1 = sA1Inner.Valid ? sA1Inner.RActual : 0;
            innerCornerSweepA0 = sA0Inner.Valid ? sA0Inner.Sweep : SweepDirection.CounterClockwise;
            innerCornerSweepA1 = sA1Inner.Valid ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        }

        // ────────────── 5. 扇形中心尖角（cx,cy 处的尖角）圆角 ──────────────
        //
        // 几何：扇形的两条径向边从 cx,cy 出发到 outerRadialPointA0/A1，
        //       它们在 cx,cy 处相交形成尖角。对这个尖角做圆角化 = 在中心画一段圆弧。
        //
        // 公式沿用 RoundedPathBuilder 的同源圆角公式：
        //   uIn  = normalize(outerRadialPointA1 - center)   ← 入射径向单位向量
        //   uOut = normalize(outerRadialPointA0 - center)   ← 出射径向单位向量
        //   half = acos(uIn·uOut) / 2                       ← 半角，uIn·uOut是两个向量的点积，即两个向量的模乘以它们夹角的余弦。
        //   dCenter = r / sin(half)                          ← 圆角圆心到尖角的距离
        //   dTangent = r / tan(half)                         ← 圆角弧端点到尖角的距离（沿径向边）
        //   sweep    = sign(uIn × uOut)                      ← 圆角弧扫描方向
        //
        // 半径联动：实际中心圆角 ≤ min(R, min(cornerRadiusA0, cornerRadiusA1))
        //   避免中心圆角超过两端圆角，造成几何穿插。
        double centerArcRadius = 0;
        Point centerArcPointA0 = new(cx, cy);
        Point centerArcPointA1 = new(cx, cy);
        SweepDirection centerSweep = SweepDirection.CounterClockwise;

        // ── 单位化（uIn、uOut 单位向量 + 长度 uInMag、uOutMag）
        //   uIn  = normalize(outerRadialPointA1 - v)  ← 入射径向（来自终止尖角方向）
        //   uOut = normalize(outerRadialPointA0 - v)  ← 出射径向（朝起始尖角方向）
        //   uInMag、uOutMag = 两条径向边的原始长度（保留供后面"压缩"判断用）
        Point v = new(cx, cy);
        double uInX = outerRadialPointA1.X - v.X;
        double uInY = outerRadialPointA1.Y - v.Y;
        double uOutX = outerRadialPointA0.X - v.X;
        double uOutY = outerRadialPointA0.Y - v.Y;
        double uInMag = Math.Sqrt(uInX * uInX + uInY * uInY);
        double uOutMag = Math.Sqrt(uOutX * uOutX + uOutY * uOutY);

        // 退化情况：两条径向边之一退化（退化成点）→ 圆心尖角消失，跳过 center 圆角
        if (uInMag > 1e-9 && uOutMag > 1e-9)
        {
            // 单位化（uInX/Y、uOutX/Y 现在是单位向量）
            uInX /= uInMag; uInY /= uInMag;
            uOutX /= uOutMag; uOutY /= uOutMag;

            // ── 角参量：扇形两条径向边的夹角
            //   dot   = uIn·uOut = cos(夹角)
            //   half  = 夹角 / 2   （半角，圆角半径公式都用这个）
            double dot = RoundedPathBuilder.Clamp(uInX * uOutX + uInY * uOutY, -1, 1);
            double half = Math.Acos(dot) * 0.5;
            double sinHalf = Math.Max(Math.Sin(half), 1e-4);  // 用于 dCenter = r/sin(half)
            double tanHalf = Math.Max(Math.Tan(half), 1e-4);  // 用于 dTangent = r/tan(half)

            // ── 半径联动：中心圆角不能超过两端任一外圈圆角
            //   这样保证三个圆角（中心 + A0 + A1）在尖角交汇处不会互相侵入。
            double rMaxFromCorners = Math.Min(cornerRadiusA0, cornerRadiusA1);
            double rActual = (rMaxFromCorners > 1e-6) ? Math.Min(R, rMaxFromCorners) : R;

            // ── 半径溢出保护
            //   dCenter = r / sin(half) = 圆角圆心到尖角的距离。
            //   如果 dCenter 比两条径向边都长，圆角圆心会落到 v 之外（径向边之外），
            //   这种情况下把 rActual 压缩到"圆角圆心刚好在最短径向边上"的极限值。
            double limit = Math.Min(uInMag, uOutMag);
            double dCenter = rActual / sinHalf;
            if (dCenter > limit) rActual = limit * sinHalf;

            // ── 圆角弧端点（沿两条径向边后退 dTangent 的位置）
            //   dTangent = r / tan(half) = 沿径向边从尖角量到圆角端点的距离
            double dTangent = rActual / tanHalf;
            if (rActual > 1e-6)
            {
                centerArcRadius = rActual;
                centerArcPointA1 = new Point(v.X + uInX * dTangent, v.Y + uInY * dTangent);  // 入射侧端点
                centerArcPointA0 = new Point(v.X + uOutX * dTangent, v.Y + uOutY * dTangent); // 出射侧端点

                // ── 扫描方向：用 uIn × uOut 叉积的符号
                //   正值 = 逆时针扫（uOut 在 uIn 左侧），负值 = 顺时针扫
                //   这与扇形整体扫掠方向无关，因为这里画的是"内凹圆角"，方向跟外圈相反。
                double cross = uInX * uOutY - uInY * uOutX;
                centerSweep = cross > 0 ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
            }
        }

        // ────────────── 6. 写出路径 ──────────────
        //
        // 6a. 外圈（顺时针：起点径向衔接点 → A0 圆角 → 外圈主弧 → A1 圆角）
        stream.MoveTo(outerRadialPointA0);

        if (cornerRadiusA0 > 1e-6)
            stream.ArcTo(outerArcPointA0, cornerRadiusA0, cornerRadiusA0, false, outerCornerSweepA0,
                center: outerRadialPointA0, tangentStart: outerRadialPointA0, tangentEnd: outerArcPointA0);

        stream.ArcTo(outerArcPointA1, rx, ry, Math.Abs(outerArcAngleA1 - outerArcAngleA0) > Math.PI, outerMainSweep);

        if (cornerRadiusA1 > 1e-6)
            stream.ArcTo(outerRadialPointA1, cornerRadiusA1, cornerRadiusA1, false, outerCornerSweepA1,
                outerArcPointA1, outerArcPointA1, outerRadialPointA1);

        // 开放圆弧：到此为止（不闭合到中心/内圈）
        if (!isClosed)
            return stream;

        // 6b. 实心扇形（A1 后回中心 cx,cy）
        if (inner <= 0)
        {
            if (centerArcRadius > 1e-6)
            {
                stream.LineTo(centerArcPointA1);
                stream.ArcTo(centerArcPointA0, centerArcRadius, centerArcRadius, false, centerSweep);
            }
            else
            {
                stream.LineTo(new Point(cx, cy));
            }
        }
        // 6c. 扇环（A1 后 → 内圈 a1 → 内圈主弧 → 内圈 a0 → 回到 A0）
        else
        {
            stream.LineTo(innerRadialPointa1);
            if (cornerRadiusa1 > 1e-6)
                stream.ArcTo(innerArcPointa1, cornerRadiusa1, cornerRadiusa1, false, innerCornerSweepA1);

            stream.ArcTo(innerArcPointa0, innerRx, innerRy, Math.Abs(innerArcAnglea0 - innerArcAnglea1) > Math.PI, innerMainSweep);

            if (cornerRadiusa0 > 1e-6)
                stream.ArcTo(innerRadialPointa0, cornerRadiusa0, cornerRadiusa0, false, innerCornerSweepA0);

            stream.LineTo(outerRadialPointA0);
        }
        return stream;
    }

    /// <summary>
    /// 完整椭圆 / 椭圆环 写出。
    /// 整圆没有尖角，圆角参数无效，所以直接用两条 180° 半弧画圆，
    /// 内圈椭圆反向扫描以形成"环"（even-odd 填充规则自动识别为孔洞）。
    /// </summary>
    private static void BuildFullEllipse(ShapeDrawStream stream, double cx, double cy, double rx, double ry, double inner, bool ccw, bool isClosed)
    {
        if (inner <= 0)
        {
            // 实心椭圆：从右侧 (cx+rx, cy) 起画半弧到左侧，再画半弧回来
            var start = new Point(cx + rx, cy);
            stream.MoveTo(start);
            stream.ArcTo(new Point(cx - rx, cy), rx, ry, false, sweepClockwise: !ccw);
            if (isClosed)
                stream.ArcTo(start, rx, ry, false, sweepClockwise: !ccw);
        }
        else
        {
            double rxIn = rx * inner, ryIn = ry * inner;
            // 外椭圆（与 sweep 同向）
            var startOut = new Point(cx + rx, cy);
            stream.MoveTo(startOut);
            stream.ArcTo(new Point(cx - rx, cy), rx, ry, false, sweepClockwise: !ccw);
            if (isClosed)
                stream.ArcTo(startOut, rx, ry, false, sweepClockwise: !ccw);

            // 开放模式跳过内圈
            if (!isClosed) return;

            // 内椭圆（与外圈反向，画成"孔洞"）
            var startIn = new Point(cx + rxIn, cy);
            stream.MoveTo(startIn);
            stream.ArcTo(new Point(cx - rxIn, cy), rxIn, ryIn, false, sweepClockwise: ccw);
            stream.ArcTo(startIn, rxIn, ryIn, false, sweepClockwise: ccw);
        }
    }

    // ============================================================
    // 几何辅助函数
    // ============================================================

    /// <summary>
    /// 椭圆切线速度大小 |dP/dθ|。
    /// P(θ) = (rx·cosθ, -ry·sinθ) → dP/dθ = (-rx·sinθ, -ry·cosθ) → |dP/dθ| = sqrt((rx·sinθ)² + (ry·cosθ)²)。
    /// 用于把"圆角弧长"换算成"参数角偏移"。
    /// </summary>
    private static double EllipseTangentLength(double rx, double ry, double theta)
    {
        double dx = -rx * Math.Sin(theta);
        double dy = -ry * Math.Cos(theta);
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>两点欧氏距离。</summary>
    private static double Distance(Point a, Point b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// 沿径向（从椭圆中心指向 p 的方向）平移 p。
    ///   delta > 0 ：向外移动
    ///   delta &lt; 0 ：向内移动
    /// </summary>
    private static Point MoveRadial(Point p, double cx, double cy, double delta)
    {
        double dx = p.X - cx, dy = p.Y - cy;
        double r = Math.Sqrt(dx * dx + dy * dy);
        if (r < 1e-9) return new Point(cx, cy);
        double k = (r + delta) / r;
        return new Point(cx + dx * k, cy + dy * k);
    }

    /// <summary>
    /// 把"用户指定的圆角半径 R"自适应压缩到不会越界的上界。
    ///
    /// 上界来自两个几何约束：
    ///   1) 圆角起点在椭圆上 ⇒ 圆角半径不超过"弦长一半"：
    ///        圆角是弧长 R 对应的圆弧，弦长 = 2·R·sin(R/(2·rTangent))，
    ///        沿椭圆切线后退 R 仍能停在椭圆上，要求 rTangent 方向不溢出 ⇒ chordHalf = rTangent·sin(R/(2·rTangent))
    ///   2) 圆角终点落在径向边上 ⇒ 圆角半径不超过"径向边一半"：
    ///        圆角终点距尖角 = 圆角半径对应的弦投影 ≤ 径向边总长的一半 ⇒ radHalf = radialLen / 2
    ///
    /// 最终 arcOffset = min(R, chordHalf, radHalf)，保证两端都在合法几何范围内。
    /// </summary>
    private static double ComputeCornerDelta(double R, double rTangent, double radialLen)
    {
        if (R <= 0 || rTangent < 1e-9) return 0;
        double chordHalf = rTangent * Math.Sin(Math.Min(R / (2.0 * rTangent), Math.PI));
        double radHalf = radialLen / 2.0;
        return Math.Max(0, Math.Min(R, Math.Min(chordHalf, radHalf)));
    }

    /// <summary>
    /// 尖角圆角几何求解器（核心算法）。
    ///
    /// 输入：在椭圆上的尖角点 cornerTip（位于 (cx,cy) 指向的射线与椭圆的交点），
    ///       椭圆主弧在 tArc = (cx + rx·cosθ, cy - ry·sinθ) 处已经切了 arcOffset 长度。
    /// 输出：圆角圆心 c'、实际圆角半径 rActual、与径向边的衔接点 tRadial、扫描方向。
    ///
    /// ────────────── 几何构造（图示） ──────────────
    ///
    ///  L1（径向边）         L2（椭圆切线）
    ///       │                  ↗
    ///   A0  ●─────────→   intersection   ← L1 ∩ L2
    ///       │                ╲
    ///       │                 ╲  bis（角平分线）
    ///       │                  ╲
    ///       │                   ● c'         ← (L2 切点垂线) ∩ bis
    ///       │                  ╱
    ///       │                 ╱ 半径 rActual = |c' - tArc|
    ///       │                ╱
    ///       │   tArc ●─────╱  ← 圆角弧起点（在椭圆上）
    ///       │
    ///       │  tRadial ●         ← 圆角弧终点（c' 在 L1 上的投影）
    ///       │
    ///
    /// 步骤：
    ///   1) 取椭圆切线 L2 = (-rx·sinθ, -ry·cosθ)，单位化得 l2u
    ///   2) 取径向边 L1 = cornerTip - center，单位化得 l1u
    ///   3) intersection = L1 ∩ L2（直线交点公式）
    ///   4) bis = l1u ± l2u，正负号由"是否起始角"+"是否外圈"决定：
    ///        bis 朝向扇形内部那一侧
    ///   5) 用 tRadialHint 自动校正 bis 方向（hint 是预先沿径向偏移的小段，
    ///      保证 bis 真正指向 tRadial 一侧）
    ///   6) c' = (L2 在 tArc 处的法线) ∩ bis
    ///   7) rActual = |c' - tArc|；tRadial = c' 在 L1 上的正交投影
    ///   8) sweep = sign((tArc - tRadial) × (c' - tRadial))
    /// </summary>
    private static SolveCornerGeometry SolveCorner(
        double cx, double cy,
        Point cornerTip, double rX, double rY, double thetaArc,
        bool isStartAngle, bool isOuter, Point tRadialHint)
    {
        // 步骤 1：椭圆主弧上"圆角起点" tArc + 切线方向
        Point tArc = new(cx + rX * Math.Cos(thetaArc), cy - rY * Math.Sin(thetaArc));
        double tdx = -rX * Math.Sin(thetaArc);
        double tdy = -rY * Math.Cos(thetaArc);
        double tMag = Math.Sqrt(tdx * tdx + tdy * tdy);
        if (tMag < 1e-9)
            return new SolveCornerGeometry { Valid = false, TArc = cornerTip };
        double l2ux = tdx / tMag;   // 切线单位向量 X
        double l2uy = tdy / tMag;   // 切线单位向量 Y

        // 步骤 2：径向边方向（尖角 → 椭圆中心）
        double l1dx = cornerTip.X - cx;
        double l1dy = cornerTip.Y - cy;
        double l1Mag = Math.Sqrt(l1dx * l1dx + l1dy * l1dy);
        if (l1Mag < 1e-9)
            return new SolveCornerGeometry { Valid = false, TArc = cornerTip };
        double l1ux = l1dx / l1Mag;  // 径向单位向量 X
        double l1uy = l1dy / l1Mag;  // 径向单位向量 Y

        // 步骤 3：两条线的交点 intersection
        //   L1: cornerTip → (cornerTip - l1u)      （沿 L1 方向一条有向线段）
        //   L2: tArc ± l2u                          （沿 L2 方向一条有向线段）
        Point l1P1 = cornerTip;
        Point l1P2 = new(cornerTip.X - l1ux, cornerTip.Y - l1uy);
        Point l2P1 = new(tArc.X - l2ux, tArc.Y - l2uy);
        Point l2P2 = new(tArc.X + l2ux, tArc.Y + l2uy);
        Point intersection = LineIntersect(l1P1, l1P2, l2P1, l2P2);

        // 步骤 4：角平分线方向（4 种组合 = 起始/终止 × 外/内）
        //   起始 + 外圈：bis = l1u - l2u    （扫掠方向 = CCW）
        //   终止 + 外圈：bis = l1u + l2u    （扫掠方向 = CW）
        //   起始 + 内圈：bis = l1u + l2u    （反向扫描）
        //   终止 + 内圈：bis = l1u - l2u
        bool useClockwise;
        if (isStartAngle)
            useClockwise = isOuter;
        else
            useClockwise = !isOuter;

        double bisX, bisY;
        if (useClockwise)
        {
            bisX = l1ux - l2ux;
            bisY = l1uy - l2uy;
        }
        else
        {
            bisX = l1ux + l2ux;
            bisY = l1uy + l2uy;
        }
        double bMag = Math.Sqrt(bisX * bisX + bisY * bisY);
        if (bMag < 1e-9)
            return new SolveCornerGeometry { Valid = false, TArc = cornerTip };
        bisX /= bMag;
        bisY /= bMag;

        // 步骤 5：用 tRadialHint 自动校正 bis 朝向（保证 bis 指向圆角一侧）
        //   tRadialHint 是预先沿径向偏移的小点（radialLen - arcOffset 处），
        //   如果 bis 与 hint 反向，说明 ± 选错了，翻转。
        double toTRadialX = tRadialHint.X - intersection.X;
        double toTRadialY = tRadialHint.Y - intersection.Y;
        double toTRadialLen = Math.Sqrt(toTRadialX * toTRadialX + toTRadialY * toTRadialY);
        if (toTRadialLen > 1e-9)
        {
            toTRadialX /= toTRadialLen;
            toTRadialY /= toTRadialLen;
            double dot = bisX * toTRadialX + bisY * toTRadialY;
            if (dot < 0) { bisX = -bisX; bisY = -bisY; }
        }

        // 步骤 6：圆角圆心 c' = (L2 在 tArc 处的法线) ∩ bis
        //   L2 法线 = (-l2uy, l2ux)
        double perpLx = -l2uy;
        double perpLy = l2ux;
        Point l2PerpA = new(tArc.X - perpLx * 1e4, tArc.Y - perpLy * 1e4);
        Point l2PerpB = new(tArc.X + perpLx * 1e4, tArc.Y + perpLy * 1e4);
        Point bisA = new(intersection.X - bisX * 1e4, intersection.Y - bisY * 1e4);
        Point bisB = new(intersection.X + bisX * 1e4, intersection.Y + bisY * 1e4);
        Point cPrime = LineIntersect(l2PerpA, l2PerpB, bisA, bisB);

        // 步骤 7：实际圆角半径 = |c' - tArc|
        double dx = cPrime.X - tArc.X;
        double dy = cPrime.Y - tArc.Y;
        double rActual = Math.Sqrt(dx * dx + dy * dy);
        if (rActual < 1e-6)
            return new SolveCornerGeometry { Valid = false, TArc = cornerTip };

        // 步骤 8：圆角与径向边的衔接点 tRadial = c' 在 L1 上的正交投影
        double proj = (cPrime.X - intersection.X) * l1ux + (cPrime.Y - intersection.Y) * l1uy;
        Point tRadial = new(intersection.X + l1ux * proj, intersection.Y + l1uy * proj);

        // 步骤 9：圆角弧扫描方向（叉积符号）
        double arcCross = (tArc.X - tRadial.X) * (cPrime.Y - tRadial.Y)
                        - (tArc.Y - tRadial.Y) * (cPrime.X - tRadial.X);
        SweepDirection sweepDir = arcCross > 0
            ? SweepDirection.CounterClockwise
            : SweepDirection.Clockwise;

        return new SolveCornerGeometry
        {
            Valid = true,
            TArc = tArc,
            TArcTheta = thetaArc,
            CPrime = cPrime,
            RActual = rActual,
            TRadial = tRadial,
            Sweep = sweepDir,
            L1UnitX = l1ux,
            L1UnitY = l1uy,
        };
    }

    /// <summary>两线段 p1-p2 与 p3-p4 的交点（标准参数化公式）。</summary>
    private static Point LineIntersect(Point p1, Point p2, Point p3, Point p4)
    {
        double x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
        double x3 = p3.X, y3 = p3.Y, x4 = p4.X, y4 = p4.Y;
        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Math.Abs(denom) < 1e-9) return p1;
        double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        return new Point(x1 + (x2 - x1) * t, y1 + (y2 - y1) * t);
    }

    private struct SolveCornerGeometry
    {
        public bool Valid;
        public Point TArc;                  // 圆角起点（在椭圆上）
        public double TArcTheta;            // 起点的椭圆参数角
        public Point CPrime;                // 圆角圆心
        public double RActual;              // 实际圆角半径（可能 < 用户指定 R）
        public Point TRadial;               // 圆角终点（在径向边上）
        public SweepDirection Sweep;        // 圆角弧扫描方向
        public double L1UnitX, L1UnitY;     // 径向单位向量（备用）
    }
}

#endregion