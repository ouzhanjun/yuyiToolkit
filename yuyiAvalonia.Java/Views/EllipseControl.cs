using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Point = Avalonia.Point;
using Vector = Avalonia.Vector;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 椭圆 / 弧 / 扇形 / 圆环 / 扇环 控件。
/// 渲染：填充路径 + 描边路径 + 调试 overlay（圆心 / 端点切线方向 / 起止点 / 外接矩形 / 圆角中心）。
/// 圆角策略：完全复用六星形做法（夹角平分线），每个尖点独立处理。
///   扇环（inner&gt;0, |sweep|&lt;360）→ 4 个尖点：A / B / b / a
///   扇形（inner=0, |sweep|&lt;360）→ 3 个尖点：A / B / O
///   椭圆 / 椭圆环（|sweep|≥360）→ 0 个尖点，CornerRadius 忽略
/// </summary>
public class EllipseControl : Panel
{
    public static readonly StyledProperty<EllipseElement?> ElementProperty =
        AvaloniaProperty.Register<EllipseControl, EllipseElement?>(nameof(Element));

    public EllipseElement? Element
    {
        get => GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }

    private readonly Path _fillPath = new() { IsHitTestVisible = false };
    private readonly Path _strokePath = new() { IsHitTestVisible = false };
    private readonly Canvas _debugOverlay = new() { IsHitTestVisible = false };
    private EllipseElement? _current;

    // 调试 overlay 用：每个尖点的几何数据。
    private Point[] _dbgVertices = Array.Empty<Point>();
    private Vector[] _dbgUIn = Array.Empty<Vector>();
    private Vector[] _dbgUOut = Array.Empty<Vector>();
    private Vector[] _dbgBisect = Array.Empty<Vector>();
    private double[] _dbgThetaIn = Array.Empty<double>();   // aStart[i] 对应的椭圆 θ（NaN = 非椭圆端点）
    private double[] _dbgThetaOut = Array.Empty<double>();  // aEnd[i] 对应的椭圆 θ（NaN = 非椭圆端点）
    private double[] _dbgRxAt = Array.Empty<double>();      // 端点所在椭圆 rx
    private double[] _dbgRyAt = Array.Empty<double>();      // 端点所在椭圆 ry

    public bool ShowDebug { get; set; } = true;

    // 切线拖拽状态
    private bool _isDraggingTangent;
    private int _dragTangentIndex;  // 0=A0, 1=A1, 2=A2
    private Point _dragStartPos;
    private double _dragStartAngle;

    public EllipseControl()
    {
        Children.Add(_fillPath);
        Children.Add(_strokePath);
        Children.Add(_debugOverlay);

        // 鼠标事件 - 用于拖拽切线
        _debugOverlay.PointerPressed += OnDebugOverlayPointerPressed;
        _debugOverlay.PointerMoved += OnDebugOverlayPointerMoved;
        _debugOverlay.PointerReleased += OnDebugOverlayPointerReleased;

        ElementProperty.Changed.AddClassHandler<EllipseControl>((c, e) =>
        {
            if (_current != null) _current.PropertyChanged -= OnElChanged;
            _current = e.NewValue as EllipseElement;
            if (_current != null) _current.PropertyChanged += OnElChanged;
            Rebuild();
        });
        BoundsProperty.Changed.AddClassHandler<EllipseControl>((c, _) => Rebuild());
    }

    private void OnDebugOverlayPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_current == null) return;
        var pos = e.GetPosition(_debugOverlay);

        // 检测是否点击了某个 I 点（A0=0, A1=1, A2=2）
        // I 点的绘制位置在 _debugOverlay 中的坐标需要计算
        // 简化：点击调试层任意位置开始拖拽，根据位置判断是哪个切线
        var el = _current!;
        double w = Bounds.Width, h = Bounds.Height;
        double cx = w * 0.5, cy = h * 0.5;
        double rx = w * 0.5, ry = h * 0.5;
        double innerRatio = Math.Clamp(el.InnerRadius, 0, 0.9999);
        double rxInner = rx * innerRatio, ryInner = ry * innerRatio;
        bool isRing = innerRatio > 0;
        double startAngleRad = el.StartAngle * Math.PI / 180.0;
        double endAngleRad = (el.StartAngle + el.SweepAngle) * Math.PI / 180.0;

        // 计算 I 点位置
        Point A0 = new Point(cx + rx * Math.Cos(startAngleRad), cy - ry * Math.Sin(startAngleRad));
        Point A1 = new Point(cx + rx * Math.Cos(endAngleRad), cy - ry * Math.Sin(endAngleRad));
        Point A3 = isRing ? new Point(cx + rxInner * Math.Cos(startAngleRad), cy - ryInner * Math.Sin(startAngleRad)) : A0;

        // A0 的切线与 L3 的交点 I
        var T_A0 = new Vector(-rx * Math.Sin(startAngleRad), -ry * Math.Cos(startAngleRad));
        var Tlen = Math.Sqrt(T_A0.X * T_A0.X + T_A0.Y * T_A0.Y);
        if (Tlen > 1e-6) T_A0 = new Vector(T_A0.X / Tlen, T_A0.Y / Tlen);
        var D_A0 = new Vector(A3.X - A0.X, A3.Y - A0.Y);
        var Dlen = Math.Sqrt(D_A0.X * D_A0.X + D_A0.Y * D_A0.Y);
        if (Dlen > 1e-6) D_A0 = new Vector(D_A0.X / Dlen, D_A0.Y / Dlen);
        double denom = T_A0.X * D_A0.Y - T_A0.Y * D_A0.X;
        Point I_A0 = A0;
        if (Math.Abs(denom) > 0.01)
        {
            double t = ((A3.X - A0.X) * D_A0.Y - (A3.Y - A0.Y) * D_A0.X) / denom;
            I_A0 = new Point(A0.X + T_A0.X * t, A0.Y + T_A0.Y * t);
        }

        // A1 的交点
        var T_A1 = new Vector(-rx * Math.Sin(endAngleRad), -ry * Math.Cos(endAngleRad));
        Tlen = Math.Sqrt(T_A1.X * T_A1.X + T_A1.Y * T_A1.Y);
        if (Tlen > 1e-6) T_A1 = new Vector(T_A1.X / Tlen, T_A1.Y / Tlen);
        Point A2 = isRing ? new Point(cx + rxInner * Math.Cos(endAngleRad), cy - ryInner * Math.Sin(endAngleRad)) : new Point(cx, cy);
        var D_A1 = new Vector(A2.X - A1.X, A2.Y - A1.Y);
        Dlen = Math.Sqrt(D_A1.X * D_A1.X + D_A1.Y * D_A1.Y);
        if (Dlen > 1e-6) D_A1 = new Vector(D_A1.X / Dlen, D_A1.Y / Dlen);
        denom = T_A1.X * D_A1.Y - T_A1.Y * D_A1.X;
        Point I_A1 = A1;
        if (Math.Abs(denom) > 0.01)
        {
            double t = ((A2.X - A1.X) * D_A1.Y - (A2.Y - A1.Y) * D_A1.X) / denom;
            I_A1 = new Point(A1.X + T_A1.X * t, A1.Y + T_A1.Y * t);
        }

        // 检测点击位置与哪个 I 点最近
        double d0 = Math.Sqrt((pos.X - I_A0.X) * (pos.X - I_A0.X) + (pos.Y - I_A0.Y) * (pos.Y - I_A0.Y));
        double d1 = Math.Sqrt((pos.X - I_A1.X) * (pos.X - I_A1.X) + (pos.Y - I_A1.Y) * (pos.Y - I_A1.Y));

        const double HIT_RADIUS = 20;
        if (d0 < HIT_RADIUS)
        {
            _isDraggingTangent = true;
            _dragTangentIndex = 0;
            _dragStartPos = I_A0;
            _dragStartAngle = el.StartAngle;
            e.Pointer.Capture(_debugOverlay);
        }
        else if (d1 < HIT_RADIUS)
        {
            _isDraggingTangent = true;
            _dragTangentIndex = 1;
            _dragStartPos = I_A1;
            _dragStartAngle = el.StartAngle + el.SweepAngle;
            e.Pointer.Capture(_debugOverlay);
        }
    }

    private void OnDebugOverlayPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingTangent) return;
        var pos = e.GetPosition(_debugOverlay);

        double w = Bounds.Width, h = Bounds.Height;
        double cx = w * 0.5, cy = h * 0.5;
        double rx = w * 0.5, ry = h * 0.5;
        double innerRatio = Math.Clamp(_current.InnerRadius, 0, 0.9999);
        bool isRing = innerRatio > 0;

        // 计算从中心到鼠标位置的角度
        double dx = pos.X - cx;
        double dy = cy - pos.Y;  // Y轴翻转
        double angleRad = Math.Atan2(dy, dx);
        double angleDeg = angleRad * 180.0 / Math.PI;

        if (_dragTangentIndex == 0)
        {
            // 拖拽 A0 的切线 - 调整起始角
            _current.StartAngle = angleDeg;
            if (_current.StartAngle < 0) _current.StartAngle += 360;
        }
        else if (_dragTangentIndex == 1)
        {
            // 拖拽 A1 的切线 - 调整结束角（sweep）
            double startRad = _current.StartAngle * Math.PI / 180.0;
            double sweepRad = angleRad - startRad;
            double sweepDeg = sweepRad * 180.0 / Math.PI;
            if (sweepDeg < 0) sweepDeg += 360;
            if (sweepDeg > 360) sweepDeg = 360;
            _current.SweepAngle = sweepDeg;
        }

        Rebuild();
    }

    private void OnDebugOverlayPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingTangent = false;
        e.Pointer.Capture(null);
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

        double w = Bounds.Width;
        double h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        double cx = w * 0.5;
        double cy = h * 0.5;
        double rx = w * 0.5;
        double ry = h * 0.5;

        double innerRatio = Math.Clamp(el.InnerRadius, 0, 0.9999);
        double startAngleRad = el.StartAngle * Math.PI / 180.0;
        double endAngleRad   = (el.StartAngle + el.SweepAngle) * Math.PI / 180.0;
        double rxInner = rx * innerRatio;
        double ryInner = ry * innerRatio;
        bool isRing = innerRatio > 0;

        var geo = EllipseGeometry.Build(
            cx, cy, rx, ry,
            el.StartAngle, el.SweepAngle,
            innerRatio,
            el.CornerRadius,
            out var corners, out var arcStarts, out var arcEnds,
            out _dbgVertices, out _dbgUIn, out _dbgUOut, out _dbgBisect,
            out _dbgThetaIn, out _dbgThetaOut, out _dbgRxAt, out _dbgRyAt,
            out _, out _);

        _fillPath.Data = geo;
        _strokePath.Data = geo;
        _fillPath.Fill = new SolidColorBrush(ParseHex(el.FillColor));
        _fillPath.Stroke = null;
        _strokePath.Fill = null;
        _strokePath.Stroke = new SolidColorBrush(ParseHex(el.StrokeColor));
        _strokePath.StrokeThickness = el.StrokeWidth;
        _fillPath.InvalidateVisual();
        _strokePath.InvalidateVisual();

        RedrawDebug(cx, cy, rx, ry, corners, arcStarts, arcEnds,
                    _dbgVertices, _dbgUIn, _dbgUOut, _dbgBisect,
                    _dbgThetaIn, _dbgThetaOut, _dbgRxAt, _dbgRyAt,
                    startAngleRad, endAngleRad, innerRatio, rxInner, ryInner, isRing);
    }

    private void RedrawDebug(
        double cx, double cy, double rx, double ry,
        IReadOnlyList<(Point Center, double Radius)> corners,
        IReadOnlyList<Point> arcStarts,
        IReadOnlyList<Point> arcEnds,
        IReadOnlyList<Point> vertices,
        IReadOnlyList<Vector> uInEdges,
        IReadOnlyList<Vector> uOutEdges,
        IReadOnlyList<Vector> bisectEdges,
        IReadOnlyList<double> thetaInEllipse,
        IReadOnlyList<double> thetaOutEllipse,
        IReadOnlyList<double> rxAt,
        IReadOnlyList<double> ryAt,
        double startAngleRad,
        double endAngleRad,
        double innerRatio,
        double rxInner,
        double ryInner,
        bool isRing)
    {
        _debugOverlay.Children.Clear();
        if (!ShowDebug) return;

        const double cs = 6.0;
        const double TANGENT_LEN = 80.0;   // 切线 / 平分线 调试线长度

        // 外接椭圆：灰色虚线
        _debugOverlay.Children.Add(new Ellipse
        {
            Width = rx * 2, Height = ry * 2,
            Stroke = new SolidColorBrush(Color.FromArgb(120, 200, 200, 200)),
            StrokeThickness = 1,
            StrokeDashArray = new AvaloniaList<double> { 3, 3 },
            [Canvas.LeftProperty] = cx - rx,
            [Canvas.TopProperty] = cy - ry
        });

        // 圆心：黄色十字 + 中心点
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(cx - cs, cy), EndPoint = new Point(cx + cs, cy),
            Stroke = Brushes.Yellow, StrokeThickness = 1.5
        });
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(cx, cy - cs), EndPoint = new Point(cx, cy + cs),
            Stroke = Brushes.Yellow, StrokeThickness = 1.5
        });
        _debugOverlay.Children.Add(new Ellipse
        {
            Width = 4, Height = 4, Fill = Brushes.Yellow,
            [Canvas.LeftProperty] = cx - 2, [Canvas.TopProperty] = cy - 2
        });

        // ============================================================
        // 代号标注：顶点 A0/A1/A2，线 L1/L2/L3/L4，弧线切线 T
        // L1: 直线 B→b, L2: 外弧 A→B, L3: 直线 A→b, L4: 内弧 b→a
        // ============================================================
        const int ARC_SEGS = 24;
        string[] lineLabels = ["L1", "L2", "L3", "L4"];

        // 计算顶点位置
        Point A0, A1, A2, A3;
        A0 = new Point(cx + rx * Math.Cos(startAngleRad), cy - ry * Math.Sin(startAngleRad));
        A1 = new Point(cx + rx * Math.Cos(endAngleRad),   cy - ry * Math.Sin(endAngleRad));
        if (isRing)
        {
            A2 = new Point(cx + rxInner * Math.Cos(endAngleRad),   cy - ryInner * Math.Sin(endAngleRad));
            A3 = new Point(cx + rxInner * Math.Cos(startAngleRad), cy - ryInner * Math.Sin(startAngleRad));
        }
        else
        {
            A2 = new Point(cx, cy);  // 中心 O
            A3 = A2;
        }

        // L1: 直线 B→b (青色)
        Point L1_p1 = A1, L1_p2 = isRing ? A2 : A2;
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = L1_p1, EndPoint = L1_p2,
            Stroke = new SolidColorBrush(Color.FromRgb(80, 200, 255)),
            StrokeThickness = 2.5
        });
        {
            var mid = new Point((L1_p1.X + L1_p2.X) * 0.5, (L1_p1.Y + L1_p2.Y) * 0.5);
            double dx = L1_p2.X - L1_p1.X, dy = L1_p2.Y - L1_p1.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len > 1e-6)
            {
                double off = 18;
                DrawLabel("L1", new Point(mid.X - dy / len * off, mid.Y + dx / len * off),
                          Color.FromRgb(80, 200, 255), 0, 0);
            }
        }

        // L2: 外弧 A→B (橙色) - 画弧线并标注
        for (int s = 0; s < ARC_SEGS; s++)
        {
            double t0 = startAngleRad + (endAngleRad - startAngleRad) * s / ARC_SEGS;
            double t1 = startAngleRad + (endAngleRad - startAngleRad) * (s + 1) / ARC_SEGS;
            var pp0 = new Point(cx + rx * Math.Cos(t0), cy - ry * Math.Sin(t0));
            var pp1 = new Point(cx + rx * Math.Cos(t1), cy - ry * Math.Sin(t1));
            _debugOverlay.Children.Add(new Line
            {
                StartPoint = pp0, EndPoint = pp1,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                StrokeThickness = 2.5
            });
        }
        {
            double tMid = (startAngleRad + endAngleRad) * 0.5;
            var midPt = new Point(cx + rx * Math.Cos(tMid), cy - ry * Math.Sin(tMid));
            double off = 28;
            double nx = -Math.Sin(tMid), ny = Math.Cos(tMid);
            DrawLabel("L2", new Point(midPt.X + nx * off, midPt.Y + ny * off),
                      Color.FromRgb(255, 140, 0), 0, 0);
        }

        // L3: 直线 A→b (绿色)
        Point L3_p1 = isRing ? A3 : A0, L3_p2 = A0;
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = L3_p1, EndPoint = L3_p2,
            Stroke = new SolidColorBrush(Color.FromRgb(100, 255, 150)),
            StrokeThickness = 2.5
        });
        {
            var mid = new Point((L3_p1.X + L3_p2.X) * 0.5, (L3_p1.Y + L3_p2.Y) * 0.5);
            double dx = L3_p2.X - L3_p1.X, dy = L3_p2.Y - L3_p1.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            if (len > 1e-6)
            {
                double off = 18;
                DrawLabel("L3", new Point(mid.X - dy / len * off, mid.Y + dx / len * off),
                          Color.FromRgb(100, 255, 150), 0, 0);
            }
        }

        // L4: 内弧 b→a (粉色)
        if (isRing)
        {
            for (int s = 0; s < ARC_SEGS; s++)
            {
                double t0 = endAngleRad + (startAngleRad - endAngleRad) * s / ARC_SEGS;
                double t1 = endAngleRad + (startAngleRad - endAngleRad) * (s + 1) / ARC_SEGS;
                var pp0 = new Point(cx + rxInner * Math.Cos(t0), cy - ryInner * Math.Sin(t0));
                var pp1 = new Point(cx + rxInner * Math.Cos(t1), cy - ryInner * Math.Sin(t1));
                _debugOverlay.Children.Add(new Line
                {
                    StartPoint = pp0, EndPoint = pp1,
                    Stroke = new SolidColorBrush(Color.FromRgb(255, 100, 200)),
                    StrokeThickness = 2.5
                });
            }
            {
                double tMid = (startAngleRad + endAngleRad) * 0.5;
                var midPt = new Point(cx + rxInner * Math.Cos(tMid), cy - ryInner * Math.Sin(tMid));
                double off = 28;
                double nx = -Math.Sin(tMid), ny = Math.Cos(tMid);
                DrawLabel("L4", new Point(midPt.X - nx * off, midPt.Y - ny * off),
                          Color.FromRgb(255, 100, 200), 0, 0);
            }
        }

        // 顶点标注
        DrawLabel("A0", new Point(A0.X - 20, A0.Y - 20), Colors.Lime, 0, 0);
        DrawLabel("A1", new Point(A1.X + 10, A1.Y - 10), Colors.Lime, 0, 0);
        DrawLabel("A2", new Point(A2.X + 10, A2.Y + 5),  Colors.Lime, 0, 0);

        // ============================================================
        // 夹角平分线几何构造（按你的方法）
        // A0: L2弧线切线T, L3直线
        //   1. 求L2切线T与L3的交点I
        //   2. 以I为圆心旋转T角度r1/2得到bisector B
        //   3. C = 切线垂线与B的交点, 半径=|切点-C|
        // ============================================================
        const double BISECT_LEN = 100.0;
        const double T_EXT = 200.0;

        // A0: L2切线 T 与 L3 直线的交点
        // L2在A0点的切线方向: T = (-rx*sin(θ), -ry*cos(θ))
        var T_A0 = new Vector(-rx * Math.Sin(startAngleRad), -ry * Math.Cos(startAngleRad));
        var tLen = Math.Sqrt(T_A0.X * T_A0.X + T_A0.Y * T_A0.Y);
        if (tLen > 1e-6) T_A0 = new Vector(T_A0.X / tLen, T_A0.Y / tLen);

        // L3 方向: A0→A3
        var D_A0 = new Vector(A3.X - A0.X, A3.Y - A0.Y);
        var dLen = Math.Sqrt(D_A0.X * D_A0.X + D_A0.Y * D_A0.Y);
        if (dLen > 1e-6) D_A0 = new Vector(D_A0.X / dLen, D_A0.Y / dLen);

        // 求交点 I
        double denom = T_A0.X * D_A0.Y - T_A0.Y * D_A0.X;
        Point I_A0 = A0;
        if (Math.Abs(denom) > 0.01)
        {
            double t = ((A3.X - A0.X) * D_A0.Y - (A3.Y - A0.Y) * D_A0.X) / denom;
            I_A0 = new Point(A0.X + T_A0.X * t, A0.Y + T_A0.Y * t);
        }

        // 标注 T (弧线切线，紫色)
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(I_A0.X - T_A0.X * T_EXT, I_A0.Y - T_A0.Y * T_EXT),
            EndPoint   = new Point(I_A0.X + T_A0.X * T_EXT, I_A0.Y + T_A0.Y * T_EXT),
            Stroke = new SolidColorBrush(Color.FromRgb(200, 100, 255)),
            StrokeThickness = 2.5
        });
        DrawLabel("T", new Point(I_A0.X + T_A0.X * 50, I_A0.Y + T_A0.Y * 50),
                  Color.FromRgb(200, 100, 255), 10, -8);

        // 标注 I (白色)
        _debugOverlay.Children.Add(new Ellipse { Width = 10, Height = 10, Stroke = Brushes.White, StrokeThickness = 2,
            Fill = Brushes.White, [Canvas.LeftProperty] = I_A0.X - 5, [Canvas.TopProperty] = I_A0.Y - 5 });
        DrawLabel("I", I_A0, Colors.White, 10, -8);

        // 计算夹角 r1 = angle(T, D)
        double dot = T_A0.X * D_A0.X + T_A0.Y * D_A0.Y;
        dot = Math.Max(-1, Math.Min(1, dot));
        double angle_r1 = Math.Acos(dot);
        // 判断方向（叉积）
        double cross = T_A0.X * D_A0.Y - T_A0.Y * D_A0.X;
        if (cross < 0) angle_r1 = -angle_r1;  // 负角度表示顺时针

        // B = T 旋转 angle_r1/2 角度（从 I 出发）
        double halfAngle = angle_r1 * 0.5;
        double cosH = Math.Cos(halfAngle), sinH = Math.Sin(halfAngle);
        var B_A0 = new Vector(
            T_A0.X * cosH - T_A0.Y * sinH,
            T_A0.X * sinH + T_A0.Y * cosH
        );
        var bLen = Math.Sqrt(B_A0.X * B_A0.X + B_A0.Y * B_A0.Y);
        if (bLen > 1e-6) B_A0 = new Vector(B_A0.X / bLen, B_A0.Y / bLen);

        // 标注 B (红色，从 I 出发)
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = I_A0,
            EndPoint   = new Point(I_A0.X + B_A0.X * BISECT_LEN, I_A0.Y + B_A0.Y * BISECT_LEN),
            Stroke = Brushes.Red, StrokeThickness = 2.5
        });
        DrawLabel("B", new Point(I_A0.X + B_A0.X * 50, I_A0.Y + B_A0.Y * 50), Colors.Red, 10, -8);

        // C = 切线垂线与 B 的交点
        // 切线垂线 N = (-T_y, T_x)，从切点 A0 出发
        var N_A0 = new Vector(-T_A0.Y, T_A0.X);
        // 求 N 与 B 的交点（过 A0 的直线与过 I 的直线的交点）
        // 参数方程: A0 + s*N, I + t*B
        // (A0 + s*N) = (I + t*B)
        denom = N_A0.X * B_A0.Y - N_A0.Y * B_A0.X;
        Point C_A0 = A0;
        if (Math.Abs(denom) > 0.001)
        {
            double t = ((I_A0.X - A0.X) * B_A0.Y - (I_A0.Y - A0.Y) * B_A0.X) / denom;
            C_A0 = new Point(A0.X + N_A0.X * t, A0.Y + N_A0.Y * t);
        }
        // 标注 C (红色)
        _debugOverlay.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red,
            [Canvas.LeftProperty] = C_A0.X - 4, [Canvas.TopProperty] = C_A0.Y - 4 });
        DrawLabel("C", C_A0, Colors.Red, 10, -8);

        // A1: L2切线T 与 L1直线的交点
        var T_A1 = new Vector(-rx * Math.Sin(endAngleRad), -ry * Math.Cos(endAngleRad));
        tLen = Math.Sqrt(T_A1.X * T_A1.X + T_A1.Y * T_A1.Y);
        if (tLen > 1e-6) T_A1 = new Vector(T_A1.X / tLen, T_A1.Y / tLen);

        // L1 方向: A1→A2
        var D_A1 = new Vector(A2.X - A1.X, A2.Y - A1.Y);
        dLen = Math.Sqrt(D_A1.X * D_A1.X + D_A1.Y * D_A1.Y);
        if (dLen > 1e-6) D_A1 = new Vector(D_A1.X / dLen, D_A1.Y / dLen);

        // 求交点 I
        denom = T_A1.X * D_A1.Y - T_A1.Y * D_A1.X;
        Point I_A1 = A1;
        if (Math.Abs(denom) > 0.01)
        {
            double t = ((A2.X - A1.X) * D_A1.Y - (A2.Y - A1.Y) * D_A1.X) / denom;
            I_A1 = new Point(A1.X + T_A1.X * t, A1.Y + T_A1.Y * t);
        }
        _debugOverlay.Children.Add(new Ellipse { Width = 10, Height = 10, Stroke = Brushes.White, StrokeThickness = 2,
            Fill = Brushes.White, [Canvas.LeftProperty] = I_A1.X - 5, [Canvas.TopProperty] = I_A1.Y - 5 });
        DrawLabel("I", I_A1, Colors.White, 10, -8);

        // T
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(I_A1.X - T_A1.X * T_EXT, I_A1.Y - T_A1.Y * T_EXT),
            EndPoint   = new Point(I_A1.X + T_A1.X * T_EXT, I_A1.Y + T_A1.Y * T_EXT),
            Stroke = new SolidColorBrush(Color.FromRgb(200, 100, 255)),
            StrokeThickness = 2.5
        });
        DrawLabel("T", new Point(I_A1.X + T_A1.X * 50, I_A1.Y + T_A1.Y * 50),
                  Color.FromRgb(200, 100, 255), 10, -8);

        // B = T 旋转 angle/2
        dot = T_A1.X * D_A1.X + T_A1.Y * D_A1.Y;
        dot = Math.Max(-1, Math.Min(1, dot));
        angle_r1 = Math.Acos(dot);
        cross = T_A1.X * D_A1.Y - T_A1.Y * D_A1.X;
        if (cross < 0) angle_r1 = -angle_r1;
        halfAngle = angle_r1 * 0.5;
        cosH = Math.Cos(halfAngle); sinH = Math.Sin(halfAngle);
        var B_A1 = new Vector(T_A1.X * cosH - T_A1.Y * sinH, T_A1.X * sinH + T_A1.Y * cosH);
        bLen = Math.Sqrt(B_A1.X * B_A1.X + B_A1.Y * B_A1.Y);
        if (bLen > 1e-6) B_A1 = new Vector(B_A1.X / bLen, B_A1.Y / bLen);
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = I_A1,
            EndPoint   = new Point(I_A1.X + B_A1.X * BISECT_LEN, I_A1.Y + B_A1.Y * BISECT_LEN),
            Stroke = Brushes.Red, StrokeThickness = 2.5
        });
        DrawLabel("B", new Point(I_A1.X + B_A1.X * 50, I_A1.Y + B_A1.Y * 50), Colors.Red, 10, -8);

        // C
        var N_A1 = new Vector(-T_A1.Y, T_A1.X);
        denom = N_A1.X * B_A1.Y - N_A1.Y * B_A1.X;
        Point C_A1 = A1;
        if (Math.Abs(denom) > 0.001)
        {
            double t = ((I_A1.X - A1.X) * B_A1.Y - (I_A1.Y - A1.Y) * B_A1.X) / denom;
            C_A1 = new Point(A1.X + N_A1.X * t, A1.Y + N_A1.Y * t);
        }
        _debugOverlay.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red,
            [Canvas.LeftProperty] = C_A1.X - 4, [Canvas.TopProperty] = C_A1.Y - 4 });
        DrawLabel("C", C_A1, Colors.Red, 10, -8);

        // A2: L4内弧切线T 与 L1直线的交点 (仅环)
        if (isRing)
        {
            var T_A2 = new Vector(-rxInner * Math.Sin(endAngleRad), -ryInner * Math.Cos(endAngleRad));
            tLen = Math.Sqrt(T_A2.X * T_A2.X + T_A2.Y * T_A2.Y);
            if (tLen > 1e-6) T_A2 = new Vector(T_A2.X / tLen, T_A2.Y / tLen);

            // L1 方向: A2→A1
            var D_A2 = new Vector(A1.X - A2.X, A1.Y - A2.Y);
            dLen = Math.Sqrt(D_A2.X * D_A2.X + D_A2.Y * D_A2.Y);
            if (dLen > 1e-6) D_A2 = new Vector(D_A2.X / dLen, D_A2.Y / dLen);

            denom = T_A2.X * D_A2.Y - T_A2.Y * D_A2.X;
            Point I_A2 = A2;
            if (Math.Abs(denom) > 0.01)
            {
                double t = ((A1.X - A2.X) * D_A2.Y - (A1.Y - A2.Y) * D_A2.X) / denom;
                I_A2 = new Point(A2.X + T_A2.X * t, A2.Y + T_A2.Y * t);
            }
            _debugOverlay.Children.Add(new Ellipse { Width = 10, Height = 10, Stroke = Brushes.White, StrokeThickness = 2,
                Fill = Brushes.White, [Canvas.LeftProperty] = I_A2.X - 5, [Canvas.TopProperty] = I_A2.Y - 5 });
            DrawLabel("I", I_A2, Colors.White, 10, -8);

            _debugOverlay.Children.Add(new Line
            {
                StartPoint = new Point(I_A2.X - T_A2.X * T_EXT, I_A2.Y - T_A2.Y * T_EXT),
                EndPoint   = new Point(I_A2.X + T_A2.X * T_EXT, I_A2.Y + T_A2.Y * T_EXT),
                Stroke = new SolidColorBrush(Color.FromRgb(200, 100, 255)),
                StrokeThickness = 2.5
            });
            DrawLabel("T", new Point(I_A2.X + T_A2.X * 50, I_A2.Y + T_A2.Y * 50),
                      Color.FromRgb(200, 100, 255), 10, -8);

            dot = T_A2.X * D_A2.X + T_A2.Y * D_A2.Y;
            dot = Math.Max(-1, Math.Min(1, dot));
            angle_r1 = Math.Acos(dot);
            cross = T_A2.X * D_A2.Y - T_A2.Y * D_A2.X;
            if (cross < 0) angle_r1 = -angle_r1;
            halfAngle = angle_r1 * 0.5;
            cosH = Math.Cos(halfAngle); sinH = Math.Sin(halfAngle);
            var B_A2 = new Vector(T_A2.X * cosH - T_A2.Y * sinH, T_A2.X * sinH + T_A2.Y * cosH);
            bLen = Math.Sqrt(B_A2.X * B_A2.X + B_A2.Y * B_A2.Y);
            if (bLen > 1e-6) B_A2 = new Vector(B_A2.X / bLen, B_A2.Y / bLen);
            _debugOverlay.Children.Add(new Line
            {
                StartPoint = I_A2,
                EndPoint   = new Point(I_A2.X + B_A2.X * BISECT_LEN, I_A2.Y + B_A2.Y * BISECT_LEN),
                Stroke = Brushes.Red, StrokeThickness = 2.5
            });
            DrawLabel("B", new Point(I_A2.X + B_A2.X * 50, I_A2.Y + B_A2.Y * 50), Colors.Red, 10, -8);

            var N_A2 = new Vector(-T_A2.Y, T_A2.X);
            denom = N_A2.X * B_A2.Y - N_A2.Y * B_A2.X;
            Point C_A2 = A2;
            if (Math.Abs(denom) > 0.001)
            {
                double t = ((I_A2.X - A2.X) * B_A2.Y - (I_A2.Y - A2.Y) * B_A2.X) / denom;
                C_A2 = new Point(A2.X + N_A2.X * t, A2.Y + N_A2.Y * t);
            }
            _debugOverlay.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = Brushes.Red,
                [Canvas.LeftProperty] = C_A2.X - 4, [Canvas.TopProperty] = C_A2.Y - 4 });
            DrawLabel("C", C_A2, Colors.Red, 10, -8);
        }

        // 尖点 + 切点（黄色虚线圆 = 圆角中心 / 半径）
        for (int i = 0; i < corners.Count; i++)
        {
            var (c, r) = corners[i];

            // 切点（arcStart / arcEnd 用黄色半透明小圆标出）
            if (i < arcStarts.Count) DrawTangent(arcStarts[i]);
            if (i < arcEnds.Count)   DrawTangent(arcEnds[i]);

            // 切点沿椭圆的切线（仅当该端点在椭圆上）：暗橙色，从端点双向画 80px
            if (i < arcStarts.Count && i < thetaInEllipse.Count
                && !double.IsNaN(thetaInEllipse[i]) && i < rxAt.Count && i < ryAt.Count)
            {
                DrawEllipseTangentAt(arcStarts[i], rxAt[i], ryAt[i], thetaInEllipse[i], TANGENT_LEN);
            }
            if (i < arcEnds.Count && i < thetaOutEllipse.Count
                && !double.IsNaN(thetaOutEllipse[i]) && i < rxAt.Count && i < ryAt.Count)
            {
                DrawEllipseTangentAt(arcEnds[i], rxAt[i], ryAt[i], thetaOutEllipse[i], TANGENT_LEN);
            }

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

            // 尖点本身：黄色十字 + 中心小黄点
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

    /// <summary>
    /// 在椭圆参数 θ 处，沿椭圆切线方向从端点 P 向两侧画各 len 像素的橙色虚线。
    /// 椭圆参数式 P(θ) = (cx + rx cosθ, cy − ry sinθ)，切线方向 = (−rx sinθ, −ry cosθ)。
    /// </summary>
    private void DrawEllipseTangentAt(Point p, double rx, double ry, double theta, double len)
    {
        var t = new Vector(-rx * Math.Sin(theta), -ry * Math.Cos(theta));
        var tlen = Math.Sqrt(t.X * t.X + t.Y * t.Y);
        if (tlen < 1e-6) return;
        var dir = new Vector(t.X / tlen, t.Y / tlen);
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(p.X - dir.X * len, p.Y - dir.Y * len),
            EndPoint   = new Point(p.X + dir.X * len, p.Y + dir.Y * len),
            Stroke     = new SolidColorBrush(Color.FromRgb(255, 140, 0)),   // 暗橙
            StrokeThickness = 1.4,
            StrokeDashArray = new AvaloniaList<double> { 5, 3 }
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

    /// <summary>
    /// 在屏幕上绘制文本标签。
    /// </summary>
    private void DrawLabel(string text, Point pos, Color color, double offsetX = 10, double offsetY = -10)
    {
        var tb = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(color),
            Background = new SolidColorBrush(Color.FromArgb(180, 30, 30, 30)),
            Padding = new Thickness(2, 1)
        };
        tb.Measure(Size.Infinity);
        Canvas.SetLeft(tb, pos.X + offsetX - tb.DesiredSize.Width * 0.5);
        Canvas.SetTop(tb, pos.Y + offsetY - tb.DesiredSize.Height * 0.5);
        _debugOverlay.Children.Add(tb);
    }
}

/// <summary>
/// 椭圆 / 弧 / 扇形 / 圆环 / 扇环 几何构建器。
/// 圆角策略：复用 RoundedSixStarGeometry 的"夹角平分线"通用算法（uIn/uOut → bisect → dC / dT）。
/// 每个尖点 v：两条入射边方向分别是 "椭圆弧切线" 与 "连向对侧端点的直线"。
///
///   cx, cy:           中心
///   rx, ry:           外接椭圆半径
///   startAngleDeg:    起始角（0° = +x，CCW 为正）
///   sweepAngleDeg:    扫描角（[-360, 360]；正数 = CCW）
///   innerRatio:       内圈半径相对外圈比例，[0, 1)
///   cornerRadius:     尖角圆角半径（像素）
///
/// 派生形态：
///   inner=0, |sweep|>=360 → 实心椭圆（无尖点，圆角忽略）
///   inner>0, |sweep|>=360 → 椭圆环（无尖点，圆角忽略）
///   inner=0, |sweep|< 360 → 扇形  尖点 [A, B, O]
///   inner>0, |sweep|< 360 → 扇环  尖点 [A, B, b, a]
/// </summary>
public static class EllipseGeometry
{
    public static PathGeometry Build(
        double cx, double cy, double rx, double ry,
        double startAngleDeg, double sweepAngleDeg, double innerRatio,
        double cornerRadius)
        => Build(cx, cy, rx, ry, startAngleDeg, sweepAngleDeg, innerRatio, cornerRadius,
                out _, out _, out _, out _, out _, out _, out _,
                out _, out _, out _, out _, out _, out _);

    public static PathGeometry Build(
        double cx, double cy, double rx, double ry,
        double startAngleDeg, double sweepAngleDeg, double innerRatio,
        double cornerRadius,
        out (Point Center, double Radius)[] cornersOut,
        out Point[] arcStartsOut,
        out Point[] arcEndsOut,
        out Point[] verticesOut,
        out Vector[] uInOut,
        out Vector[] uOutOut,
        out Vector[] bisectOut,
        out double[] thetaInEllipseOut,
        out double[] thetaOutEllipseOut,
        out double[] rxAtOut,
        out double[] ryAtOut,
        out Vector[] bisectDirsNewOut,
        out Point[] ptsNewOut)
    {
        cornersOut = Array.Empty<(Point, double)>();
        arcStartsOut = Array.Empty<Point>();
        arcEndsOut = Array.Empty<Point>();
        verticesOut = Array.Empty<Point>();
        uInOut = Array.Empty<Vector>();
        uOutOut = Array.Empty<Vector>();
        bisectOut = Array.Empty<Vector>();
        thetaInEllipseOut = Array.Empty<double>();
        thetaOutEllipseOut = Array.Empty<double>();
        rxAtOut = Array.Empty<double>();
        ryAtOut = Array.Empty<double>();
        bisectDirsNewOut = Array.Empty<Vector>();
        ptsNewOut = Array.Empty<Point>();

        double sweep = Clamp(sweepAngleDeg, -360, 360);
        double inner = Clamp(innerRatio, 0, 0.9999);
        double r0 = Math.Max(0, cornerRadius);

        if (Math.Abs(sweep) < 1e-6 || rx <= 0 || ry <= 0)
            return new PathGeometry();

        double startRad = startAngleDeg * Math.PI / 180.0;
        double endRad = startRad + sweep * Math.PI / 180.0;
        bool ccw = sweep > 0;
        SweepDirection outerSweep = ccw ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        SweepDirection innerSweep = ccw ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;

        var geo = new PathGeometry();

        // ============ 完整闭合（|sweep|>=360）：无尖点 ============
        if (Math.Abs(sweep) >= 360.0)
        {
            var fig = new PathFigure { StartPoint = new Point(cx + rx, cy), IsClosed = true };
            if (inner <= 0)
            {
                fig.Segments!.Add(new ArcSegment
                {
                    Point = new Point(cx - rx, cy),
                    Size = new Size(rx, ry),
                    SweepDirection = outerSweep,
                    IsLargeArc = false
                });
                fig.Segments!.Add(new ArcSegment
                {
                    Point = fig.StartPoint,
                    Size = new Size(rx, ry),
                    SweepDirection = outerSweep,
                    IsLargeArc = false
                });
            }
            else
            {
                double rxInC = rx * inner, ryInC = ry * inner;
                fig.Segments!.Add(new ArcSegment
                {
                    Point = fig.StartPoint,
                    Size = new Size(rx, ry),
                    SweepDirection = outerSweep,
                    IsLargeArc = true
                });
                fig.Segments!.Add(new LineSegment { Point = new Point(cx + rxInC, cy) });
                fig.Segments!.Add(new ArcSegment
                {
                    Point = new Point(cx + rxInC, cy),
                    Size = new Size(rxInC, ryInC),
                    SweepDirection = innerSweep,
                    IsLargeArc = true
                });
            }
            geo.Figures!.Add(fig);
            return geo;
        }

        // ============ 部分扫描：扇形 / 扇环，按尖点跑通用圆角算法 ============
        double rxIn = rx * inner;
        double ryIn = ry * inner;

        // 关键锚点
        Point A  = EllipsePoint(cx, cy, rx,    ry,    startRad);          // 外圈起点
        Point B  = EllipsePoint(cx, cy, rx,    ry,    endRad);            // 外圈终点
        Point a  = EllipsePoint(cx, cy, rxIn,  ryIn,  startRad);          // 内圈起点
        Point b  = EllipsePoint(cx, cy, rxIn,  ryIn,  endRad);            // 内圈终点
        Point O  = new Point(cx, cy);                                       // 圆心

        bool isRingSector = inner > 0;
        Point[] pts = isRingSector ? new[] { A, B, b, a } : new[] { A, B, O };

        // 修复：尖点切线方向按边类型决定（椭圆弧那一边的方向用椭圆切线方向），
        // 避免圆角让椭圆位置偏移。
        int n = pts.Length;
        var corners       = new (Point C, double R)[n];
        var aStart        = new Point[n];
        var aEnd          = new Point[n];
        var tangentRadius = new double[n];   // 每个圆角的自适应实际半径（由切点决定）
        var uInEdges      = new Vector[n];
        var uOutEdges     = new Vector[n];
        var bisectDirs    = new Vector[n];   // 每个尖点的夹角平分线方向（调试用）
        var bisectDirsNew = new Vector[n];  // 迭代后的 bisect（通过椭圆切点 P 计算）
        var ptsNew        = new Point[n];   // 椭圆切点 P（迭代收敛后）
        var thetaInEllipse = new double[n];  // 端点 aStart[i] 对应的椭圆参数 θ（NaN = 非椭圆）
        var thetaOutEllipse = new double[n]; // 端点 aEnd[i] 对应的椭圆参数 θ（NaN = 非椭圆）
        var rxAt           = new double[n];  // 端点所在椭圆的 rx
        var ryAt           = new double[n];  // 端点所在椭圆的 ry

        // 第一遍：算每个尖点处的入射/出射方向（椭圆弧那一边的方向用椭圆切线）。
        for (int i = 0; i < n; i++)
        {
            var prev = pts[(i + n - 1) % n];
            var v    = pts[i];
            var next = pts[(i + 1) % n];
            (uInEdges[i], uOutEdges[i]) = EdgeDirections(
                i, v, prev, next,
                cx, cy, rx, ry, rxIn, ryIn,
                startRad, endRad, isRingSector);
        }

        // 第二遍：套通用六星形公式算圆角。
        // 椭圆边的端点必须严格落在椭圆上，否则 ArcSegment 会重定向椭圆。
        for (int i = 0; i < n; i++)
        {
            var uIn  = uInEdges[i];
            var uOut = uOutEdges[i];
            var v    = pts[i];
            var prev = pts[(i + n - 1) % n];
            var next = pts[(i + 1) % n];

            // 正确的圆心/切点距离公式。
            // uIn = 沿运动方向进入 v 的单位向量，uOut = 沿运动方向离开 v 的单位向量。
            // 两条边线 = (v, v - uIn) 和 (v, v + uOut)，它们的夹角（夹在 -uIn 与 uOut 之间）
            // 是多边形在 v 处的"内角"。设 cos(内角) = -uIn·uOut，则
            //   sin(内角/2) = sqrt((1 - cos(内角))/2) = sqrt((1 + uIn·uOut) / 2)
            //   cos(内角/2) = sqrt((1 + cos(内角))/2) = sqrt((1 - uIn·uOut) / 2)
            // 把圆心 C 放在沿 (内角) 平分线方向上 (即 -uIn 和 uOut 的角平分线)，
            // 且 |C - v| = r / sin(内角/2)，则 C 到两条边线的垂直距离都等于 r，
            // 同时 aStart = v - uIn · dTangent、aEnd = v + uOut · dTangent 就是
            // 两边线上的垂足，|C - aStart| = |C - aEnd| = r。
            double interiorDot = Clamp(-uIn.X * uOut.X - uIn.Y * uOut.Y, -1, 1);
            double sinHalfInt  = Math.Sqrt((1.0 - interiorDot) * 0.5);
            double cosHalfInt  = Math.Sqrt((1.0 + interiorDot) * 0.5);

            // 圆角半径需要限制：不能超过两边长度的最小值
            double lenIn  = Dist(prev, v);
            double lenOut = Dist(next, v);
            // 半弦上限：保证 dCenter ≤ min/2（圆心离 v 不超过半边长），且 dTangent ≤ min/2。
            //   dCenter = r / sin(内角/2) ≤ min/2 ⇒ r ≤ sinHalf · min / 2
            //   dTangent = r · cosHalf / sinHalf ≤ min/2 ⇒ r ≤ sinHalf / cosHalf · min / 2 = tan(内角/2) · min/2
            // 综合 r ≤ min(lenIn, lenOut) / 2 · min(sinHalf, cosHalf)
            double rMax = Math.Min(lenIn, lenOut) * 0.5 * Math.Min(sinHalfInt, cosHalfInt);
            double r    = Math.Min(r0, rMax);
            // r=0 时直接走尖角
            if (r < 0.5)
            {
                corners[i] = (v, 0);
                aStart[i] = v;
                aEnd[i]   = v;
                continue;
            }

            double dCenter  = r / Math.Max(sinHalfInt, 1e-4);
            // dTangent 的符号：凸角时切点沿 in/out 边朝 prev/next 方向（dTangent > 0）；
            // 凹角时切点沿 in/out 边朝 past prev / past next 方向（dTangent < 0）。
            // 当 flip bisect 时同步翻转 dTangent 符号。
            double dTangent = r * cosHalfInt / Math.Max(sinHalfInt, 1e-4);

            // 圆心方向 = (-uIn + uOut) 的角平分线（normalize(-uIn + uOut)）。
            //   对凸角：bisect 指向多边形内部 → C 在内部 → 垂足 |C - 垂足| = r。
            //   对凹角：bisect 仍指向"小角"那一侧，C 落在外部、垂足在 v 朝 past prev / past next
            //     方向延伸，且 |C - 垂足| = r。
            Vector bisect;
            double bx = -uIn.X + uOut.X;
            double by = -uIn.Y + uOut.Y;
            double bisectLen = Math.Sqrt(bx * bx + by * by);
            if (bisectLen > 1e-6)
                bisect = new Vector(bx / bisectLen, by / bisectLen);
            else
                bisect = new Vector(uIn.Y, -uIn.X);

            bisectDirs[i] = bisect;

            // 椭圆切点 bisect 迭代（Newton θ clamp 到 [startRad,endRad] 防止跑出弧段）：
            //   P_iter → 切线 T_P → bisect → C = P + bisect*r → P' = ClosestEllipse(C) → 检查收敛。
            double minTh = Math.Min(startRad, endRad), maxTh = Math.Max(startRad, endRad);
            bool outIsEll = IsOuterArcEdge(i, isRingSector, true);
            bool inIsEll  = IsOuterArcEdge(i, isRingSector, false);
            if (outIsEll || inIsEll)
            {
                double erx = rx, ery = ry;
                double baseAngle = startRad;
                if (outIsEll)
                {
                    erx = (i == 2) ? rxIn : rx;
                    ery = (i == 2) ? ryIn : ry;
                    baseAngle = (i == 0) ? startRad : endRad;
                }
                else
                {
                    erx = (i == 3) ? rxIn : rx;
                    ery = (i == 3) ? ryIn : ry;
                    baseAngle = (i == 1) ? endRad : startRad;
                }
                Point P_iter = new Point(cx + erx * Math.Cos(baseAngle), cy - ery * Math.Sin(baseAngle));
                Vector bisect_iter = bisect;
                for (int iter = 0; iter < 8; iter++)
                {
                    double th = Math.Atan2(cy - P_iter.Y, P_iter.X - cx);
                    var T = new Vector(-erx * Math.Sin(th), -ery * Math.Cos(th));
                    var tLen = Math.Sqrt(T.X * T.X + T.Y * T.Y);
                    if (tLen > 1e-6) T = new Vector(T.X / tLen, T.Y / tLen);
                    var D = Normalize(Sub(next, P_iter));
                    var bs = new Vector(T.X + D.X, T.Y + D.Y);
                    var bLen = Math.Sqrt(bs.X * bs.X + bs.Y * bs.Y);
                    if (bLen > 1e-6) bs = new Vector(bs.X / bLen, bs.Y / bLen);
                    bisect_iter = bs;
                    var C_iter = AddPoint(P_iter, Mul(bs, r));
                    var P_new = ClosestEllipsePoint(C_iter, cx, cy, erx, ery, th, minTh, maxTh);
                    if (Dist(P_new, P_iter) < 0.05) { P_iter = P_new; break; }
                    P_iter = P_new;
                }
                ptsNew[i] = P_iter;
                bisectDirsNew[i] = bisect_iter;
            }
            else
            {
                ptsNew[i] = pts[i];
                bisectDirsNew[i] = bisect;
            }

            corners[i] = (AddPoint(v, Mul(bisect, dCenter)), r);

            // 切点：
            //   - 直线边：圆心 C 到直线 (P1-P2) 的垂足，|C - 切点| = r 严格，端点在直线上。
            //   - 椭圆边：Newton 迭代找椭圆曲线上"离 C 最近"的点 → 端点严格在椭圆上，
            //     |C - 切点| = r_ellipse（自适应半径，可能不等于用户输入的 r）。
            //     Avalonia ArcSegment(Size=rx,ry) 端点严格在椭圆上 ⇒ 不会自动缩放 size
            //     ⇒ "外圆往里缩 / 内圆变大" 问题消除。
            // corner arc 半径 = 自适应值（保证两个端点都在 corner arc 上）。
            ApplyTangentPoints(i, v, prev, next, corners[i].C, r, dTangent, uIn, uOut,
                               cx, cy, rx, ry, rxIn, ryIn,
                               startRad, endRad, isRingSector, ccw,
                               aStart, aEnd, tangentRadius,
                               thetaInEllipse, thetaOutEllipse, rxAt, ryAt);

            // corners[i].R 用自适应半径更新（确保 corner arc 经过两个端点）。
            corners[i] = (corners[i].C, tangentRadius[i]);
        }

        // 构造路径：
        //   段 0: A → B → 外弧
        //   段 1: B → b → 直线（环）；B → O → 直线（pie）
        //   段 2: b → a → 内弧（环）；O → A → 直线（pie）
        //   段 3: a → A → 直线（仅环）
        bool isLargeArc = Math.Abs(sweep) > 180.0;
        var figFull = new PathFigure { IsClosed = true, StartPoint = aStart[0] };

        if (inner > 0)
        {
            // ===== 扇环：A → (外弧) → B → (直线) → b → (内弧) → a → (直线) → A =====
            // 1) A 圆角弧 aStart[0] → aEnd[0]
            AppendCornerArc(figFull, corners[0].R, aStart[0], aEnd[0], corners[0].C);
            // 2) 外弧 aEnd[0] → aStart[1]
            figFull.Segments!.Add(new ArcSegment
            {
                Point = aStart[1],
                Size = new Size(rx, ry),
                SweepDirection = outerSweep,
                IsLargeArc = isLargeArc
            });
            // 3) B 圆角弧 aStart[1] → aEnd[1]
            AppendCornerArc(figFull, corners[1].R, aStart[1], aEnd[1], corners[1].C);
            // 4) 直线 aEnd[1] → aStart[2]
            if (Dist(aEnd[1], aStart[2]) > 1e-6)
                figFull.Segments!.Add(new LineSegment { Point = aStart[2] });
            // 5) b 圆角弧 aStart[2] → aEnd[2]
            AppendCornerArc(figFull, corners[2].R, aStart[2], aEnd[2], corners[2].C);
            // 6) 内弧 aEnd[2] → aStart[3]
            figFull.Segments!.Add(new ArcSegment
            {
                Point = aStart[3],
                Size = new Size(rxIn, ryIn),
                SweepDirection = innerSweep,
                IsLargeArc = isLargeArc
            });
            // 7) a 圆角弧 aStart[3] → aEnd[3]
            AppendCornerArc(figFull, corners[3].R, aStart[3], aEnd[3], corners[3].C);
            // 8) 直线 aEnd[3] → aStart[0]（首尾闭合由 IsClosed 完成，但首点已 = aStart[0]，所以这段由闭合自动连线）
        }
        else
        {
            // ===== 扇形：A → (外弧) → B → (直线) → O → (直线) → A =====
            // 1) A 圆角弧
            AppendCornerArc(figFull, corners[0].R, aStart[0], aEnd[0], corners[0].C);
            // 2) 外弧
            figFull.Segments!.Add(new ArcSegment
            {
                Point = aStart[1],
                Size = new Size(rx, ry),
                SweepDirection = outerSweep,
                IsLargeArc = isLargeArc
            });
            // 3) B 圆角弧
            AppendCornerArc(figFull, corners[1].R, aStart[1], aEnd[1], corners[1].C);
            // 4) 直线到 O 圆角起点
            if (Dist(aEnd[1], aStart[2]) > 1e-6)
                figFull.Segments!.Add(new LineSegment { Point = aStart[2] });
            // 5) O 圆角弧
            AppendCornerArc(figFull, corners[2].R, aStart[2], aEnd[2], corners[2].C);
            // 6) 直线回到 aStart[0]（由 IsClosed 完成）
        }

        geo.Figures!.Add(figFull);
        cornersOut        = corners;
        arcStartsOut      = aStart;
        arcEndsOut        = aEnd;
        verticesOut       = pts;
        uInOut            = uInEdges;
        uOutOut           = uOutEdges;
        bisectOut         = bisectDirs;
        thetaInEllipseOut = thetaInEllipse;
        thetaOutEllipseOut = thetaOutEllipse;
        rxAtOut           = rxAt;
        ryAtOut           = ryAt;
        bisectDirsNewOut  = bisectDirsNew;
        ptsNewOut         = ptsNew;
        return geo;
    }

    /// <summary>
    /// 向 fig 追加"圆角尖点 v[i] 处的过渡弧"：从当前 aStart[i] 弧到 aEnd[i]。
    /// 如果 r=0 则跳过。
    ///
    /// Avalonia 的 ArcSegment 用中心点隐式求解：
    ///   SweepDirection.CounterClockwise → 圆心在弦（aStart→aEnd）的 LEFT 侧
    ///   SweepDirection.Clockwise       → 圆心在弦的 RIGHT 侧
    /// （其中 LEFT/RIGHT 是沿弦方向行走时的左侧/右侧，在 screen y-down 中）。
    /// 我们希望 Avalonia 隐式圆心 = 我们的圆心 C（在多边形内部一侧），
    /// 所以要按 C 相对于弦方向的位置选 SweepDirection。
    /// </summary>
    private static void AppendCornerArc(PathFigure fig, double r, Point arcStart, Point arcEnd, Point cornerCenter)
    {
        if (r < 0.5) return;
        // 弦方向 (aEnd - aStart)
        double chX = arcEnd.X - arcStart.X;
        double chY = arcEnd.Y - arcStart.Y;
        // 弦的 LEFT 方向（沿行走方向左侧）= (chY, -chX)（Petzold 公式）。
        // 如果 C 在弦的 LEFT 侧（dot > 0），Avalonia CCW 会把隐式圆心放在这里；
        // 如果在 RIGHT 侧（dot < 0），Avalonia CW 会放在 RIGHT。
        // 我们希望隐式圆心 = C，所以按符号选 sweep。
        double mX = (arcStart.X + arcEnd.X) * 0.5;
        double mY = (arcStart.Y + arcEnd.Y) * 0.5;
        double side = (cornerCenter.X - mX) * chY - (cornerCenter.Y - mY) * chX;
        fig.Segments!.Add(new ArcSegment
        {
            Point          = arcEnd,
            Size           = new Size(r, r),
            SweepDirection = side > 0 ? SweepDirection.CounterClockwise
                                       : SweepDirection.Clockwise,
            IsLargeArc     = false
        });
    }

    /// <summary>
    /// 返回尖点 v[i] 处的"入射方向 / 出射方向"（指向 v / 离开 v 的单位向量）。
    /// 椭圆弧段的方向用椭圆切线（避免圆角让椭圆位置偏移）；直线段用直线方向。
    ///   扇环 [A, B, b, a]：
    ///     i=0 in = 直线 a→A ；out = 外弧 A→B 切线
    ///     i=1 in = 外弧 A→B 切线 ；out = 直线 B→b
    ///     i=2 in = 直线 B→b ；out = 内弧 b→a 切线
    ///     i=3 in = 内弧 b→a 切线 ；out = 直线 a→A
    ///   扇形 [A, B, O]：
    ///     i=0 in = 直线 O→A ；out = 外弧 A→B 切线
    ///     i=1 in = 外弧 A→B 切线 ；out = 直线 B→O
    ///     i=2 in = 直线 B→O ；out = 直线 O→A
    /// </summary>
    private static (Vector uIn, Vector uOut) EdgeDirections(
        int i, Point v, Point prev, Point next,
        double cx, double cy, double rx, double ry, double rxIn, double ryIn,
        double startRad, double endRad, bool isRingSector)
    {
        Vector uIn, uOut;

        if (isRingSector)
        {
            // in
            // i=0 (A): 入射 = 直线 a→A；外弧在 A 处的"运动方向"是 CCW（从 A 到 B），故 uIn 是直线方向
            if      (i == 0) uIn = Normalize(Sub(v, prev));
            // i=1 (B): 入射 = 外弧，CCW 方向 = ccw:true
            else if (i == 1) uIn = EllipseTangentAt(rx, ry, cx, cy, v, endRad,   ccw: true);
            else if (i == 2) uIn = Normalize(Sub(v, prev));
            // i=3 (a): 入射 = 内弧，CW 方向（与外弧相反）= ccw:false
            else             uIn = EllipseTangentAt(rxIn, ryIn, cx, cy, v, startRad, ccw: false);
            // out
            if      (i == 0) uOut = EllipseTangentAt(rx, ry, cx, cy, v, startRad, ccw: true);
            else if (i == 1) uOut = Normalize(Sub(next, v));
            // i=2 (b): 出射 = 内弧，CW 方向 = ccw:false
            else if (i == 2) uOut = EllipseTangentAt(rxIn, ryIn, cx, cy, v, endRad, ccw: false);
            else             uOut = Normalize(Sub(next, v));
        }
        else
        {
            if      (i == 0) uIn = Normalize(Sub(v, prev));
            // pie 的 i=1 (B): 入射 = 外弧，CCW 方向 = ccw:true
            else if (i == 1) uIn = EllipseTangentAt(rx, ry, cx, cy, v, endRad, ccw: true);
            else             uIn = Normalize(Sub(v, prev));
            if      (i == 0) uOut = EllipseTangentAt(rx, ry, cx, cy, v, startRad, ccw: true);
            else if (i == 1) uOut = Normalize(Sub(next, v));
            else             uOut = Normalize(Sub(next, v));
        }

        return (uIn, uOut);
    }

    /// <summary>
    /// 椭圆在 angle 处的单位切线方向。
    ///   ccw = true  -> 沿 angle 增大方向
    ///   ccw = false -> 沿 angle 减小方向
    /// 椭圆参数式 P(θ) = (cx + rx cosθ, cy - ry sinθ)，dP/dθ = (-rx sinθ, -ry cosθ)。
    /// </summary>
    private static Vector EllipseTangentAt(double rx, double ry, double cx, double cy, Point pt, double angle, bool ccw)
    {
        var t = new Vector(-rx * Math.Sin(angle), -ry * Math.Cos(angle));
        var len = Math.Sqrt(t.X * t.X + t.Y * t.Y);
        if (len < 1e-6) return new Vector(1, 0);
        t = new Vector(t.X / len, t.Y / len);
        return ccw ? t : new Vector(-t.X, -t.Y);
    }

    /// <summary>
    /// 对顶点 v[i] 的 in 边和 out 边，把切点设为"圆心 C 到边线的垂足"。
    /// 垂足保证 |C - 切点| = r，圆与边严格相切。
    ///
    ///   直线边：C 到直线 (P1, P2) 的垂足。
    ///           foot = P1 + t·(P2-P1)，t = ((C-P1)·(P2-P1)) / |P2-P1|²。
    ///           此时 |C - foot| = r（因为圆心 C 已在边线法线方向上离边 r 处）。
    ///
    ///   椭圆边：把椭圆在 v 处的切线当作边的"局部线性近似"，用 C 到切线的垂足
    ///           作切点（同时 |C - 切点| = r）。垂足不严格落在椭圆上（曲线与切线
    ///           有二阶误差），但 Avalonia ArcSegment 用 (start, end, radius) 渲染，
    ///           与是否在椭圆上无关；圆角弧保证 |C - 切点| = r，从而弧段中心、半径
    ///           都正确，光滑衔接相邻椭圆/直线段。
    /// </summary>
    private static void ApplyTangentPoints(
        int i, Point v, Point prev, Point next, Point C, double r, double dTangent,
        Vector uIn, Vector uOut,
        double cx, double cy, double rx, double ry, double rxIn, double ryIn,
        double startRad, double endRad, bool isRingSector, bool ccw,
        Point[] aStart, Point[] aEnd, double[] tangentRadius,
        double[] thetaInOut, double[] thetaOutOut, double[] rxOut, double[] ryOut)
    {
        // 圆角策略（用户确认）：
        //   1) 圆心 C 的位置按"直线方向 + 椭圆切线方向"的 bisect 计算（保持不变）。
        //   2) 直线边切点：C 到直线的垂足，|C - 切点| = r 严格，端点严格在直线上。
        //   3) 椭圆边切点：椭圆曲线上"离 C 最近"的点（Newton 迭代沿椭圆 θ 求解）。
        //      端点严格落在椭圆曲线上 ⇒ Avalonia ArcSegment(Size=rx,ry) 不缩放 size
        //      ⇒ 外圆 / 内圆不会被缩放（解决 "外圆往里缩 / 内圆变大" 问题）。
        //      此情况下 |C - 切点| = r_ellipse 是自适应半径（不一定等于用户输入的 r）。
        //   4) corner arc 用自适应半径 r_eff = min(|C-aStart|, |C-aEnd|)，保证两个端点
        //      都在 corner arc 上（必要时 chord/2 修正让 Avalonia 能正确渲染）。

        // ============ out 边 ============
        bool outIsEllipse = IsOuterArcEdge(i, isRingSector, true);
        if (outIsEllipse)
        {
            double erx = (i == 2) ? rxIn : rx;
            double ery = (i == 2) ? ryIn : ry;
            double baseAngle = (i == 0) ? startRad : endRad;
            var (pt, th) = ClosestEllipsePointWithTheta(C, cx, cy, erx, ery, baseAngle);
            aEnd[i] = pt;
            thetaOutOut[i] = th;
            rxOut[i] = erx;
            ryOut[i] = ery;
        }
        else
        {
            // 直线 (v, next) — 非椭圆端点，θ 用 NaN 标记
            double t = FootParam(C, v, next);
            aEnd[i] = FootPoint(v, next, t);
            thetaOutOut[i] = double.NaN;
        }

        // ============ in 边 ============
        bool inIsEllipse = IsOuterArcEdge(i, isRingSector, false);
        if (inIsEllipse)
        {
            double erx = (i == 3) ? rxIn : rx;
            double ery = (i == 3) ? ryIn : ry;
            double baseAngle = (i == 1) ? endRad : startRad;
            var (pt, th) = ClosestEllipsePointWithTheta(C, cx, cy, erx, ery, baseAngle);
            aStart[i] = pt;
            thetaInOut[i] = th;
            rxOut[i] = erx;
            ryOut[i] = ery;
        }
        else
        {
            // 直线 (prev, v) — 非椭圆端点，θ 用 NaN 标记
            double t = FootParam(C, prev, v);
            aStart[i] = FootPoint(prev, v, t);
            thetaInOut[i] = double.NaN;
        }

        // 自适应半径：取 min(|C-aStart|, |C-aEnd|)，保证两端点都在 corner arc 上。
        double dCStart = Dist(C, aStart[i]);
        double dCEnd   = Dist(C, aEnd[i]);
        tangentRadius[i] = Math.Min(dCStart, dCEnd);
    }

    /// <summary>
    /// Newton 迭代：在椭圆 (cx, cy, rx, ry) 上找离点 C 最近的点 P（minimize |P(θ) - C|²）。
    /// 令 f(θ) = (P(θ) - C) · dP/dθ = 0。
    /// 初始 θ₀ = baseAngle（v 对应的椭圆参数），迭代直到 |Δθ| < 1e-9 或达到 16 步。
    /// 解 θ 返回的 P 严格在椭圆上，端点不会被 Avalonia ArcSegment 缩放。
    /// </summary>
    private static Point ClosestEllipsePoint(Point C, double cx, double cy, double rx, double ry, double baseAngle, double minTheta = double.MinValue, double maxTheta = double.MaxValue)
    {
        double theta = baseAngle;
        for (int iter = 0; iter < 16; iter++)
        {
            double cosT = Math.Cos(theta), sinT = Math.Sin(theta);
            double px = cx + rx * cosT;
            double py = cy - ry * sinT;
            double dx = px - C.X, dy = py - C.Y;
            double tx = -rx * sinT, ty = -ry * cosT;
            double f  = dx * tx + dy * ty;
            double df = tx * tx + ty * ty + dx * (-rx * cosT) + dy * (ry * sinT);
            if (Math.Abs(df) < 1e-12) break;
            double dTheta = -f / df;
            theta += dTheta;
            theta = Math.Max(minTheta, Math.Min(maxTheta, theta));
            if (Math.Abs(dTheta) < 1e-12) break;
        }
        return new Point(cx + rx * Math.Cos(theta), cy - ry * Math.Sin(theta));
    }

    /// <summary>
    /// Newton 迭代：在椭圆 (cx, cy, rx, ry) 上找离点 C 最近的点 P（minimize |P(θ) - C|²）。
    /// 同时返回 P 对应的椭圆参数 θ（用于调试画"切点处的椭圆切线"）。
    /// theta 迭代时 clamp 到 [minTheta, maxTheta] 范围内，防止跑出椭圆弧段导致偏移。
    /// </summary>
    private static (Point P, double Theta) ClosestEllipsePointWithTheta(Point C, double cx, double cy, double rx, double ry, double baseAngle, double minTheta = double.MinValue, double maxTheta = double.MaxValue)
    {
        double theta = baseAngle;
        for (int iter = 0; iter < 16; iter++)
        {
            double cosT = Math.Cos(theta), sinT = Math.Sin(theta);
            double px = cx + rx * cosT;
            double py = cy - ry * sinT;
            double dx = px - C.X, dy = py - C.Y;
            double tx = -rx * sinT, ty = -ry * cosT;
            double f  = dx * tx + dy * ty;
            double df = tx * tx + ty * ty + dx * (-rx * cosT) + dy * (ry * sinT);
            if (Math.Abs(df) < 1e-12) break;
            double dTheta = -f / df;
            theta += dTheta;
            theta = Math.Max(minTheta, Math.Min(maxTheta, theta));
            if (Math.Abs(dTheta) < 1e-12) break;
        }
        return (new Point(cx + rx * Math.Cos(theta), cy - ry * Math.Sin(theta)), theta);
    }

    /// <summary>
    /// 直线 (P1, P2) 参数化 foot = P1 + t·(P2-P1)。
    /// 返回 C 在直线上的垂足参数 t：t = ((C-P1)·(P2-P1)) / |P2-P1|²。
    /// t=0 → P1；t=1 → P2；t<0 → 落在 P1 反方向；t>1 → 落在 P2 之后。
    /// </summary>
    private static double FootParam(Point C, Point P1, Point P2)
    {
        double dx = P2.X - P1.X, dy = P2.Y - P1.Y;
        double len2 = dx * dx + dy * dy;
        if (len2 < 1e-12) return 0;
        return ((C.X - P1.X) * dx + (C.Y - P1.Y) * dy) / len2;
    }

    /// <summary>
    /// 直线参数 t 对应的点。
    /// </summary>
    private static Point FootPoint(Point P1, Point P2, double t)
        => new Point(P1.X + t * (P2.X - P1.X), P1.Y + t * (P2.Y - P1.Y));

    /// <summary>
    /// 判断顶点 i 的边是否为椭圆。
    ///   outEdge = true  → out 边（next 方向）
    ///   outEdge = false → in 边（prev 方向）
    /// ring sector 顶点序列：A, B, b, a
    ///   i=0 (A): in=line(a-A), out=outer arc
    ///   i=1 (B): in=outer arc,  out=line(B-b)
    ///   i=2 (b): in=line(B-b),  out=inner arc
    ///   i=3 (a): in=inner arc,  out=line(a-A)
    /// pie 顶点序列：A, B, O
    ///   i=0 (A): in=line(O-A), out=outer arc
    ///   i=1 (B): in=outer arc, out=line(B-O)
    ///   i=2 (O): in=line(B-O), out=line(O-A)
    /// </summary>
    private static bool IsOuterArcEdge(int i, bool isRingSector, bool outEdge)
    {
        if (isRingSector)
        {
            if (outEdge) return i == 0 || i == 2;
            else         return i == 1 || i == 3;
        }
        else
        {
            if (outEdge) return i == 0;
            else         return i == 1;
        }
    }

    // ===== helpers =====
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

    private static Point EllipsePoint(double cx, double cy, double rx, double ry, double angle)
        => new Point(cx + rx * Math.Cos(angle), cy - ry * Math.Sin(angle));

    private static double Dist(Point a, Point b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double Clamp(double v, double min, double max)
        => v < min ? min : (v > max ? max : v);
}