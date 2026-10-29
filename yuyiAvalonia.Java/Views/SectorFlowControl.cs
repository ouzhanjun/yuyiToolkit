using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Point = Avalonia.Point;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 扇形 / 弧 / 椭圆 / 圆环 / 扇环 控件。
/// 渲染：填充路径 + 描边路径 + 调试覆盖层（圆心/顶点/外接椭圆）。
/// 派生形态：
///   InnerRadius=0, |sweep|>=360 → 完整椭圆
///   InnerRadius=0, |sweep|< 360 → 扇形 (pie)
///   InnerRadius>0, |sweep|>=360 → 圆环 (donut)
///   InnerRadius>0, |sweep|< 360 → 扇环 (ring sector)
/// </summary>
public class SectorFlowControl : Panel
{
    public static readonly StyledProperty<SectorElement?> ElementProperty =
        AvaloniaProperty.Register<SectorFlowControl, SectorElement?>(nameof(Element));

    public SectorElement? Element
    {
        get => GetValue(ElementProperty);
        set => SetValue(ElementProperty, value);
    }

    private readonly Path _fillPath = new() { IsHitTestVisible = false };
    private readonly Path _strokePath = new() { IsHitTestVisible = false };
    private readonly Canvas _debugOverlay = new() { IsHitTestVisible = false };
    private SectorElement? _current;

    public bool ShowDebug { get; set; } = true;

    public SectorFlowControl()
    {
        Children.Add(_fillPath);
        Children.Add(_strokePath);
        Children.Add(_debugOverlay);

        ElementProperty.Changed.AddClassHandler<SectorFlowControl>((c, e) =>
        {
            if (_current != null) _current.PropertyChanged -= OnElChanged;
            _current = e.NewValue as SectorElement;
            if (_current != null) _current.PropertyChanged += OnElChanged;
            Rebuild();
        });
        BoundsProperty.Changed.AddClassHandler<SectorFlowControl>((c, _) => Rebuild());
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
        double inner = Math.Clamp(el.InnerRadius, 0, 0.9999);
        double startRad = el.StartAngle * Math.PI / 180.0;
        double sweepRad = el.SweepAngle * Math.PI / 180.0;
        double endRad = startRad + sweepRad;
        double rxIn = rx * inner, ryIn = ry * inner;
        bool isRing = inner > 0;
        bool isFull = Math.Abs(el.SweepAngle) >= 360;

        var geo = EllipseArcGeometry.Build(
            0, 0, w, h,
            el.StartAngle, el.SweepAngle, el.InnerRadius,
            el.CornerRadius);

        _fillPath.Data = geo;
        _strokePath.Data = geo;
        _fillPath.Fill = new SolidColorBrush(ParseHex(el.FillColor));
        _fillPath.Stroke = null;
        _strokePath.Fill = null;
        _strokePath.Stroke = new SolidColorBrush(ParseHex(el.StrokeColor));
        _strokePath.StrokeThickness = el.StrokeWidth;
        _fillPath.InvalidateVisual();
        _strokePath.InvalidateVisual();

        RedrawDebug(cx, cy, rx, ry, rxIn, ryIn, startRad, endRad, isRing, isFull, el.CornerRadius, el.StartAngle, el.SweepAngle);
    }

    private void RedrawDebug(
        double cx, double cy, double rx, double ry,
        double rxIn, double ryIn,
        double startRad, double endRad,
        bool isRing, bool isFull,
        double cornerRadius,
        double startAngleDeg, double sweepAngleDeg)
    {
        _debugOverlay.Children.Clear();
        if (!ShowDebug) return;

        const double cs = 6.0;
        const double LABEL_OFF = 16;

        // === 外接椭圆：灰色虚线 ===
        _debugOverlay.Children.Add(new Ellipse
        {
            Width = rx * 2, Height = ry * 2,
            Stroke = new SolidColorBrush(Color.FromArgb(120, 180, 180, 180)),
            StrokeThickness = 1,
            StrokeDashArray = new AvaloniaList<double> { 4, 4 },
            [Canvas.LeftProperty] = cx - rx,
            [Canvas.TopProperty] = cy - ry
        });

        // === 中心点 O ===
        DrawCross(cx, cy, cs, Brushes.Yellow);
        DrawLabel("O", cx, cy, Colors.Yellow, LABEL_OFF, -LABEL_OFF);

        if (isFull)
        {
            // 完整椭圆：只画外接椭圆和圆心
            if (isRing)
            {
                // 内椭圆（虚线）
                _debugOverlay.Children.Add(new Ellipse
                {
                    Width = rxIn * 2, Height = ryIn * 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 180, 180, 180)),
                    StrokeThickness = 1,
                    StrokeDashArray = new AvaloniaList<double> { 4, 4 },
                    [Canvas.LeftProperty] = cx - rxIn,
                    [Canvas.TopProperty] = cy - ryIn
                });
                DrawDot(cx + rxIn, cy, 3, Brushes.Lime);
                DrawLabel("rx", cx + rx, cy + LABEL_OFF, Colors.Lime, 0, 0);
            }
            return;
        }

        // === 计算关键顶点 ===
        // A0: 外圈起点
        Point A0 = new Point(cx + rx * Math.Cos(startRad), cy - ry * Math.Sin(startRad));
        // A1: 外圈终点
        Point A1 = new Point(cx + rx * Math.Cos(endRad), cy - ry * Math.Sin(endRad));
        // a0: 内圈起点
        Point a0 = new Point(cx + rxIn * Math.Cos(startRad), cy - ryIn * Math.Sin(startRad));
        // a1: 内圈终点
        Point a1 = new Point(cx + rxIn * Math.Cos(endRad), cy - ryIn * Math.Sin(endRad));

        // === 计算圆角化后的"实际顶点"（与 EllipseArcGeometry.Build 中的回退量一致）===
        bool ccw = sweepAngleDeg > 0;
        double R = Math.Max(0, cornerRadius);
        double rTanOS = EllipseTangentLengthDbg(rx, ry, startRad);
        double rTanOE = EllipseTangentLengthDbg(rx, ry, endRad);
        double radOS = DistanceDbg(A0, new Point(cx, cy));
        double radOE = DistanceDbg(A1, new Point(cx, cy));
        double dOS = ComputeCornerDeltaDbg(R, rTanOS, radOS);
        double dOE = ComputeCornerDeltaDbg(R, rTanOE, radOE);
        double rTanIS = isRing ? EllipseTangentLengthDbg(rxIn, ryIn, startRad) : 0;
        double rTanIE = isRing ? EllipseTangentLengthDbg(rxIn, ryIn, endRad) : 0;
        double radIS = isRing ? DistanceDbg(a0, new Point(cx, cy)) : 0;
        double radIE = isRing ? DistanceDbg(a1, new Point(cx, cy)) : 0;
        double dIS = isRing ? ComputeCornerDeltaDbg(R, rTanIS, radIS) : 0;
        double dIE = isRing ? ComputeCornerDeltaDbg(R, rTanIE, radIE) : 0;
        double daOS = dOS / Math.Max(rTanOS, 1e-9);
        double daOE = dOE / Math.Max(rTanOE, 1e-9);
        double daIS = dIS / Math.Max(rTanIS, 1e-9);
        double daIE = dIE / Math.Max(rTanIE, 1e-9);
        double thetaOS = ccw ? startRad + daOS : startRad - daOS;
        double thetaOE = ccw ? endRad - daOE : endRad + daOE;
        double thetaIS = ccw ? startRad + daIS : startRad - daIS;
        double thetaIE = ccw ? endRad - daIE : endRad + daIE;
        Point A_OS = new Point(cx + rx * Math.Cos(thetaOS), cy - ry * Math.Sin(thetaOS));
        Point A_OE = new Point(cx + rx * Math.Cos(thetaOE), cy - ry * Math.Sin(thetaOE));
        Point A_IS = isRing ? new Point(cx + rxIn * Math.Cos(thetaIS), cy - ryIn * Math.Sin(thetaIS)) : new Point(cx, cy);
        Point A_IE = isRing ? new Point(cx + rxIn * Math.Cos(thetaIE), cy - ryIn * Math.Sin(thetaIE)) : new Point(cx, cy);

        // 径向退 d 的"假设切点"（用于画径向线终点 / 圆弧圆心方向估算）
        Point T_OS_radial = MoveRadialDbg(A0, cx, cy, -dOS);
        Point T_OE_radial = MoveRadialDbg(A1, cx, cy, -dOE);
        Point T_IS_radial = isRing ? MoveRadialDbg(a0, cx, cy, +dIS) : new Point(cx, cy);
        Point T_IE_radial = isRing ? MoveRadialDbg(a1, cx, cy, +dIE) : new Point(cx, cy);

        // === 在圆角后的顶点处画 80px 切线（虚线青）+ 80px 径向线（实线品红）+ 圆角圆心/切点 ===
        const double TANGENT_LEN = 80.0;
        const double RADIAL_LEN = 80.0;
        IBrush tangentBrush = new SolidColorBrush(Color.FromRgb(0, 220, 255));
        AvaloniaList<double> tangentDash = new AvaloniaList<double> { 6.0, 4.0 };
        IBrush radialBrush = new SolidColorBrush(Color.FromRgb(255, 80, 200));
        IBrush cornerCenterBrush = new SolidColorBrush(Color.FromRgb(255, 220, 0));
        IBrush cornerContactBrush = new SolidColorBrush(Color.FromRgb(255, 230, 80));

        var ellipseCenter = new Point(cx, cy);

        // A0 - 起始角外圆角: isStartAngle=true, isOuter=true
        DrawCornerDebug(A0, T_OS_radial, A_OS, rx, ry, thetaOS, ellipseCenter, dOS,
            TANGENT_LEN, RADIAL_LEN,
            tangentBrush, tangentDash, radialBrush, cornerCenterBrush, cornerContactBrush,
            isStartAngle: true, isOuter: true, idPrefix: "OS");
        // A1 - 扫描角外圆角: isStartAngle=false, isOuter=true
        DrawCornerDebug(A1, T_OE_radial, A_OE, rx, ry, thetaOE, ellipseCenter, dOE,
            TANGENT_LEN, RADIAL_LEN,
            tangentBrush, tangentDash, radialBrush, cornerCenterBrush, cornerContactBrush,
            isStartAngle: false, isOuter: true, idPrefix: "OE");
        if (isRing)
        {
            // a0 - 起始角内圆角: isStartAngle=true, isOuter=false
            DrawCornerDebug(a0, T_IS_radial, A_IS, rxIn, ryIn, thetaIS, ellipseCenter, dIS,
                TANGENT_LEN, RADIAL_LEN,
                tangentBrush, tangentDash, radialBrush, cornerCenterBrush, cornerContactBrush,
                isStartAngle: true, isOuter: false, idPrefix: "IS");
            // a1 - 扫描角内圆角: isStartAngle=false, isOuter=false
            DrawCornerDebug(a1, T_IE_radial, A_IE, rxIn, ryIn, thetaIE, ellipseCenter, dIE,
                TANGENT_LEN, RADIAL_LEN,
                tangentBrush, tangentDash, radialBrush, cornerCenterBrush, cornerContactBrush,
                isStartAngle: false, isOuter: false, idPrefix: "IE");
        }
        // === 画角度指示弧 ===
        DrawAngleArc(cx, cy, Math.Min(rx, ry) * 0.15, startRad, endRad);

        // === 顶点圆圈（原始尖角 A0/A1/a0/a1）===
        DrawDot(A0.X, A0.Y, 5, Brushes.Lime);
        DrawDot(A1.X, A1.Y, 5, Brushes.Lime);

        // === 标签 ===
        DrawLabel("A0 开始", A0.X, A0.Y, Colors.Lime, -LABEL_OFF, -LABEL_OFF);
        DrawLabel("A1 结束", A1.X, A1.Y, Colors.Lime, LABEL_OFF * 0.5f, -LABEL_OFF);
        DrawLabel("O 圆心", cx, cy, Colors.Yellow, LABEL_OFF, LABEL_OFF);

        if (isRing)
        {
            DrawDot(a0.X, a0.Y, 4, new SolidColorBrush(Color.FromRgb(255, 140, 60)));
            DrawDot(a1.X, a1.Y, 4, new SolidColorBrush(Color.FromRgb(255, 140, 60)));
            DrawLabel("a0", a0.X, a0.Y, Color.FromRgb(255, 140, 60), -LABEL_OFF, LABEL_OFF);
            DrawLabel("a1", a1.X, a1.Y, Color.FromRgb(255, 140, 60), LABEL_OFF * 0.5f, LABEL_OFF);

            // 内椭圆（虚线）
            _debugOverlay.Children.Add(new Ellipse
            {
                Width = rxIn * 2, Height = ryIn * 2,
                Stroke = new SolidColorBrush(Color.FromArgb(100, 180, 180, 180)),
                StrokeThickness = 1,
                StrokeDashArray = new AvaloniaList<double> { 4, 4 },
                [Canvas.LeftProperty] = cx - rxIn,
                [Canvas.TopProperty] = cy - ryIn
            });
        }

        // === 画角度值 ===
        //double startDeg = startRad * 180 / Math.PI;
        //double endDeg = endRad * 180 / Math.PI;
        //double sweepDeg = endDeg - startDeg;
        //if (sweepDeg < 0) sweepDeg += 360;
        // 已删除角度显示
    }

    private void DrawCross(double x, double y, double size, IBrush brush)
    {
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(x - size, y), EndPoint = new Point(x + size, y),
            Stroke = brush, StrokeThickness = 1.5
        });
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(x, y - size), EndPoint = new Point(x, y + size),
            Stroke = brush, StrokeThickness = 1.5
        });
        _debugOverlay.Children.Add(new Ellipse
        {
            Width = 4, Height = 4, Fill = brush,
            [Canvas.LeftProperty] = x - 2, [Canvas.TopProperty] = y - 2
        });
    }

    private void DrawDot(double x, double y, double radius, IBrush brush)
    {
        _debugOverlay.Children.Add(new Ellipse
        {
            Width = radius * 2, Height = radius * 2, Fill = brush,
            [Canvas.LeftProperty] = x - radius, [Canvas.TopProperty] = y - radius
        });
    }

    private void DrawLabel(string text, double x, double y, Color color, double offX, double offY)
    {
        var tb = new TextBlock
        {
            Text = text,
            FontSize = 11,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(color),
            Background = new SolidColorBrush(Color.FromArgb(160, 30, 30, 30)),
            Padding = new Thickness(2, 1)
        };
        tb.Measure(Size.Infinity);
        Canvas.SetLeft(tb, x + offX - tb.DesiredSize.Width * 0.5);
        Canvas.SetTop(tb, y + offY - tb.DesiredSize.Height * 0.5);
        _debugOverlay.Children.Add(tb);
    }

    /// <summary>
    /// 在每个"尖角"处画调试覆盖层：
    ///   - 80px 切线（虚线、青色）：从原始尖角出发，沿椭圆切线方向
    ///   - 80px 径向线（实线、品红）：从原始尖角出发，沿径向（双向）
    ///   - L1（浅蓝）/L2（粉红）/角平分线（绿）：几何分析辅助线
    ///   - L2 切点垂线（橙色虚线）：用于求 c' 的辅助线
    ///   - 深蓝色小叉 + "c'"：新的圆角圆心 = L2 切点垂线 × 角平分线
    ///   - 深蓝色垂线（c' → T_radial）：c' 往 L1 方向做垂线，垂足即 T_radial 切点
    ///   - 切点 × 2（T_radial, T_arc）
    /// </summary>
    /// <param name="cornerTip">原始尖角（椭圆上 a0 或 a1），即圆角前的扇形顶点</param>
    /// <param name="tRadial">径向边切点：尖角沿径向退 d 后的点，圆角弧的一端（T_radial）</param>
    /// <param name="tArc">椭圆主弧切点：主弧起点/终点，圆角弧的另一端（T_arc）</param>
    /// <param name="rx">外椭圆 X 半径（用于计算切线方向向量）</param>
    /// <param name="ry">外椭圆 Y 半径</param>
    /// <param name="theta">尖角在椭圆上的极角（弧度，用于 dP/dθ 计算切线向量）</param>
    /// <param name="ellipseCenter">椭圆圆心（用于画径向线）</param>
    /// <param name="cornerR">圆角半径 d = dOS/dOE/dIS/dIE（等于 ArcSegment.Size）</param>
    /// <param name="tangentLen">切线方向画多长（像素，用于可视化）</param>
    /// <param name="radialLen">径向线双向多长（像素，用于可视化）</param>
    /// <param name="tangentBrush">切线画笔（青色）</param>
    /// <param name="tangentDash">切线虚线样式</param>
    /// <param name="radialBrush">径向线画笔（品红）</param>
    /// <param name="cornerCenterBrush">圆角圆心点画笔（黄）</param>
    /// <param name="cornerContactBrush">切点画笔（橙黄）</param>
    private void DrawCornerDebug(
        Point cornerTip, Point tRadial, Point tArc,
        double rx, double ry, double theta,
        Point ellipseCenter, double cornerR,
        double tangentLen, double radialLen,
        IBrush tangentBrush, AvaloniaList<double> tangentDash,
        IBrush radialBrush,
        IBrush cornerCenterBrush, IBrush cornerContactBrush,
        bool isStartAngle, bool isOuter,
        string idPrefix = "") // 例如 "OS"/"OE"/"IS"/"IE"，用于 T_radial/T_arc 标签
    {
        const double LABEL_OFF = 16;
        // 切线向量：dP/dθ = (-rx·sinθ, -ry·cosθ)，即椭圆在该角的切线方向
        double tdx = -rx * Math.Sin(theta);
        double tdy = -ry * Math.Cos(theta);
        double tMag = Math.Sqrt(tdx * tdx + tdy * tdy);
        if (tMag < 1e-9) return;

        // ========== 以下为绘制部分（不做几何计算）==========

        // 切线：从 cornerTip 出发，沿 dP/dθ 方向画 tangentLen 像素
        //_debugOverlay.Children.Add(new Line
        //{
        //    StartPoint = cornerTip,
        //    EndPoint = new Point(cornerTip.X + tdx * tangentLen / tMag, cornerTip.Y + tdy * tangentLen / tMag),
        //    Stroke = tangentBrush,
        //    StrokeThickness = 1.5,
        //    StrokeDashArray = tangentDash
        //});

        //// 径向线：从 cornerTip 出发，沿径向（ellipseCenter → cornerTip 方向）双向各 radialLen 像素
        //double rdx = cornerTip.X - ellipseCenter.X;
        //double rdy = cornerTip.Y - ellipseCenter.Y;
        //double rLen = Math.Sqrt(rdx * rdx + rdy * rdy);
        //if (rLen > 1e-9)
        //{
        //    double rk = radialLen / rLen;
        //    _debugOverlay.Children.Add(new Line
        //    {
        //        StartPoint = new Point(cornerTip.X - rdx * rk, cornerTip.Y - rdy * rk),
        //        EndPoint = new Point(cornerTip.X + rdx * rk, cornerTip.Y + rdy * rk),
        //        Stroke = radialBrush,
        //        StrokeThickness = 1.5
        //    });
        //}

        // 圆角圆心改用 L2 切点垂线 × 角平分线 计算，由函数末尾蓝色叉标记
        // （旧 chord-perp+legLen 算法已弃用）

        // ============================================================
        // 椭圆弧一侧的切点切线（粉红色）
        // 原理：椭圆参数式 P(θ) = (cx + rx·cosθ, cy - ry·sinθ)
        //       切线向量 dP/dθ = (-rx·sinθ, -ry·cosθ)
        //       T_arc 与 cornerTip 共享相同的 θ，故 (tdx, tdy) 就是椭圆在 T_arc 处的切线方向
        // ============================================================
        const double arcContactTangentLen = 50.0; // 切线段半长（像素）
        IBrush ellipseTangentBrush = new SolidColorBrush(Color.FromRgb(255, 100, 180)); // 粉红色
        double tkEllipse = arcContactTangentLen / tMag;
        Point ellipseTangentStart = new Point(tArc.X - tdx * tkEllipse, tArc.Y - tdy * tkEllipse);
        Point ellipseTangentEnd = new Point(tArc.X + tdx * tkEllipse, tArc.Y + tdy * tkEllipse);
        // 画圆弧切线
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = ellipseTangentStart,
            EndPoint = ellipseTangentEnd,
            Stroke = ellipseTangentBrush,
            StrokeThickness = 1.5,
            StrokeDashArray = new AvaloniaList<double> { 4, 4 }
        });

        // ============================================================
        // 径向线 L1 与椭圆切线 L2 的交点（红色小点）
        // L1：同侧径向线（通过 cornerTip 和 ellipseCenter）
        // L2：椭圆弧切线（通过 tArc，方向为 (tdx, tdy)）
        // ============================================================
        Point intersection = LineIntersection(
            cornerTip, ellipseCenter,
            ellipseTangentStart, ellipseTangentEnd);
        DrawDot(intersection.X, intersection.Y, 6, Brushes.Red);
        //if (!string.IsNullOrEmpty(idPrefix))
        //    DrawLabel(idPrefix + "·X", intersection.X, intersection.Y, Colors.Red, 0, -LABEL_OFF);

        // ============================================================
        // L1：尖角径向线，从交点出发指向扇形外
        // L1 方向：从 ellipseCenter 指向 cornerTip（朝外）
        // ============================================================
        double l1dx = cornerTip.X - ellipseCenter.X;
        double l1dy = cornerTip.Y - ellipseCenter.Y;
        double l1Len = Math.Sqrt(l1dx * l1dx + l1dy * l1dy);
        if (l1Len > 1e-9)
        {
            l1dx /= l1Len;
            l1dy /= l1Len;
        }
        IBrush l1Brush = new SolidColorBrush(Color.FromRgb(100, 200, 255)); // 浅蓝色 L1
        Point l1Start = intersection;
        Point l1End = new Point(intersection.X + l1dx * 80, intersection.Y + l1dy * 80);
        //_debugOverlay.Children.Add(new Line
        //{
        //    StartPoint = l1Start,
        //    EndPoint = l1End,
        //    Stroke = l1Brush,
        //    StrokeThickness = 2
        //});
        //DrawLabel("L1", l1End.X + 5, l1End.Y - 5, Colors.LightBlue, 0, 0);

        //// L2：椭圆弧切线，从 intersection 画（与 ellipseTangentStart/End 平行）
        double l2dx = tdx / tMag;
        double l2dy = tdy / tMag;
        Point l2Start = new Point(intersection.X - l2dx * 50, intersection.Y - l2dy * 50);
        Point l2End = new Point(intersection.X + l2dx * 50, intersection.Y + l2dy * 50);
        //DrawLabel("L2", l2End.X + 5, l2End.Y - 5, Colors.HotPink, 0, 0);

        // ============================================================
        // 角平分线：根据位置和内外圆角决定方向
        // 起始角(A0/a0): 外圆角用L1-L2(顺时针), 内圆角用L1+L2(逆时针)
        // 扫描角(A1/a1): 外圆角用L1+L2(逆时针), 内圆角用L1-L2(顺时针)
        // ============================================================
        double bisX, bisY;
        bool useClockwise; // true=L1-L2(顺时针), false=L1+L2(逆时针)

        if (isStartAngle)
        {
            useClockwise = isOuter; // 起始角: 外圆角用顺时针(L1-L2), 内圆角用逆时针(L1+L2)
        }
        else
        {
            useClockwise = !isOuter; // 扫描角: 外圆角用逆时针(L1+L2), 内圆角用顺时针(L1-L2)
        }

        if (useClockwise)
        {
            bisX = l1dx - l2dx;
            bisY = l1dy - l2dy;
        }
        else
        {
            bisX = l1dx + l2dx;
            bisY = l1dy + l2dy;
        }

        double bisLen = Math.Sqrt(bisX * bisX + bisY * bisY);
        if (bisLen > 1e-9)
        {
            bisX /= bisLen;
            bisY /= bisLen;
        }

        // 角平分线应该指向扇形内部（更靠近 tRadial）
        double toTRadialX = tRadial.X - intersection.X;
        double toTRadialY = tRadial.Y - intersection.Y;
        double toTRadialLen = Math.Sqrt(toTRadialX * toTRadialX + toTRadialY * toTRadialY);
        if (toTRadialLen > 1e-9)
        {
            toTRadialX /= toTRadialLen;
            toTRadialY /= toTRadialLen;
            double dot = bisX * toTRadialX + bisY * toTRadialY;
            if (dot < 0)
            {
                bisX = -bisX;
                bisY = -bisY;
            }
        }

        // 画角平分线
        IBrush bisectorBrush = new SolidColorBrush(Color.FromRgb(50, 255, 50)); // 绿色
        Point bisStart = new Point(intersection.X - bisX * 30, intersection.Y - bisY * 30);
        Point bisEnd = new Point(intersection.X + bisX * 80, intersection.Y + bisY * 80);
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = bisStart,
            EndPoint = bisEnd,
            Stroke = bisectorBrush,
            StrokeThickness = 2,
            StrokeDashArray = new AvaloniaList<double> { 6, 3 }
        });
        //DrawLabel("角平分线", bisEnd.X + 5, bisEnd.Y - 5, Colors.Lime, 0, 0);

        // ============================================================
        // 圆角圆心 c' = L2 切点垂线 × 角平分线
        //   L2 切点垂线：通过 T_arc（弧切点），方向 ⊥ L2 = (-l2dy, l2dx)
        //   角平分线：通过 intersection（L1×L2 交点），方向 (bisX, bisY)
        // ============================================================
        // l2PerpP1: L2 切点垂线1
        Point l2PerpP1 = new Point(tArc.X - l2dy * 100, tArc.Y + l2dx * 100);
        // l2PerpP2: L2 切点垂线2
        Point l2PerpP2 = new Point(tArc.X + l2dy * 100, tArc.Y - l2dx * 100);
        // bisLineP1: 角平分线1
        Point bisLineP1 = new Point(intersection.X - bisX * 100, intersection.Y - bisY * 100);
        // bisLineP2: 角平分线2
        Point bisLineP2 = new Point(intersection.X + bisX * 100, intersection.Y + bisY * 100);
        // cPrime: 圆角圆心
        Point cPrime = LineIntersection(l2PerpP1, l2PerpP2, bisLineP1, bisLineP2);

        // 画 L2 切点垂线（橙色虚线，辅助线）
        IBrush l2PerpBrush = new SolidColorBrush(Color.FromRgb(255, 165, 0));
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = l2PerpP1,
            EndPoint = l2PerpP2,
            Stroke = l2PerpBrush,
            StrokeThickness = 1,
            StrokeDashArray = new AvaloniaList<double> { 3, 3 }
        });

        // ============================================================
        // 从 c' 向 L1（径向方向）做垂线，垂足 = T_radial 切点
        // L1 方向单位向量 (l1dx, l1dy) 从 intersection 出发
        // 垂足 = intersection + (c' - intersection) · (l1dx, l1dy) * (l1dx, l1dy)
        // 同时验证 |c' - T_radial| == cornerR（数值校验）
        // ============================================================
        IBrush darkBlueBrush = new SolidColorBrush(Color.FromRgb(20, 60, 200)); // 深蓝色
        double projT = (cPrime.X - intersection.X) * l1dx + (cPrime.Y - intersection.Y) * l1dy;
        Point tRadialFromPerp = new Point(intersection.X + l1dx * projT, intersection.Y + l1dy * projT);

        // 画垂线（深蓝色实线，c' → T_radial 切点）
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = cPrime,
            EndPoint = tRadialFromPerp,
            Stroke = darkBlueBrush,
            StrokeThickness = 1.5
        });

        // 画 c' 深蓝色小叉（新圆角圆心）
        DrawCross(cPrime.X, cPrime.Y, 5, darkBlueBrush);
        //DrawLabel(string.IsNullOrEmpty(idPrefix) ? "c'" : idPrefix + "_c'", cPrime.X, cPrime.Y, Color.FromRgb(50, 200, 80), 10.0, -10.0);

        // 新圆角圆心在径向线的切点，在垂足处标出 T_radial 切点（小蓝点）
        DrawDot(tRadialFromPerp.X, tRadialFromPerp.Y, 4, darkBlueBrush);

        // 切点：T_radial（径向边切点）和 T_arc（椭圆主弧切点）
        //DrawDot(tRadial.X, tRadial.Y, 3, cornerContactBrush);
        // 新圆角圆心在弧线的切点
        DrawDot(tArc.X, tArc.Y, 3, darkBlueBrush);
        if (!string.IsNullOrEmpty(idPrefix))
        {
            // T_radial 标签偏外（远离 c' 一侧）
            //DrawLabel(idPrefix + "_Tr", tRadial.X, tRadial.Y, Color.FromRgb(255, 220, 0), LABEL_OFF, -LABEL_OFF);
            //// T_arc 标签偏内（靠近 c' 一侧）
            //DrawLabel(idPrefix + "_Ta", tArc.X, tArc.Y, Color.FromRgb(50, 200, 80), -LABEL_OFF, LABEL_OFF);
        }

        // ============================================================
        // 圆角弧（蓝色）：圆心 c'，半径 r = |c' - T_arc|
        //   从 T_radial（径向切点）开始，到 T_arc（圆弧切点）结束
        //   用 PathGeometry + ArcSegment 画一段椭圆（rx=ry=r）弧
        // ============================================================
        double rArc = Math.Sqrt((cPrime.X - tArc.X) * (cPrime.X - tArc.X)
                               + (cPrime.Y - tArc.Y) * (cPrime.Y - tArc.Y));
        if (rArc > 1e-3)
        {
            // SweepDirection：c' 相对 chord (tArc → tRadial) 在哪一侧
            // Y 轴向下时，cross > 0 ⇒ CCW
            double arcCross = (tRadialFromPerp.X - tArc.X) * (cPrime.Y - tArc.Y)
                            - (tRadialFromPerp.Y - tArc.Y) * (cPrime.X - tArc.X);
            var blueArcGeom = new Avalonia.Media.PathGeometry();
            var blueFig = new Avalonia.Media.PathFigure
            {
                StartPoint = tRadialFromPerp,
                IsClosed = false
            };
            blueFig.Segments!.Add(new Avalonia.Media.ArcSegment
            {
                Point = tArc,
                Size = new Size(rArc, rArc),
                SweepDirection = arcCross > 0
                    ? Avalonia.Media.SweepDirection.CounterClockwise
                    : Avalonia.Media.SweepDirection.Clockwise,
                IsLargeArc = false
            });
            blueArcGeom.Figures!.Add(blueFig);
            _debugOverlay.Children.Add(new Avalonia.Controls.Shapes.Path
            {
                Data = blueArcGeom,
                Stroke = darkBlueBrush,
                StrokeThickness = 2,
                Fill = null
            });
        }
    }

    private static Point MoveRadialDbg(Point p, double cx, double cy, double delta)
    {
        double dx = p.X - cx, dy = p.Y - cy;
        double r = Math.Sqrt(dx * dx + dy * dy);
        if (r < 1e-9) return new Point(cx, cy);
        double k = (r + delta) / r;
        return new Point(cx + dx * k, cy + dy * k);
    }

    private static double EllipseTangentLengthDbg(double rx, double ry, double theta)
    {
        double dx = -rx * Math.Sin(theta);
        double dy = -ry * Math.Cos(theta);
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double DistanceDbg(Point a, Point b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // 两直线交点（参数式求解）
    // 直线1: p1 + t*(p2-p1)，直线2: p3 + s*(p4-p3)
    private static Point LineIntersection(Point p1, Point p2, Point p3, Point p4)
    {
        double x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
        double x3 = p3.X, y3 = p3.Y, x4 = p4.X, y4 = p4.Y;
        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Math.Abs(denom) < 1e-9) return p1; // 平行或重合
        double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        return new Point(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
    }

    private static double ComputeCornerDeltaDbg(double R, double rTangent, double radialLen)
    {
        if (R <= 0 || rTangent < 1e-9) return 0;
        double chordHalf = rTangent * Math.Sin(Math.Min(R / (2.0 * rTangent), Math.PI));
        double radHalf = radialLen / 2.0;
        return Math.Max(0, Math.Min(R, Math.Min(chordHalf, radHalf)));
    }

    /// <summary>
    /// 画一段小弧表示角度（从 startRad 到 endRad）。
    /// </summary>
    private void DrawAngleArc(double cx, double cy, double r, double startRad, double endRad)
    {
        // 用 12 段折线近似弧
        int segs = 12;
        double sweep = endRad - startRad;
        double prevX = cx + r * Math.Cos(startRad);
        double prevY = cy - r * Math.Sin(startRad);
        for (int i = 1; i <= segs; i++)
        {
            double t = startRad + sweep * i / segs;
            double nx = cx + r * Math.Cos(t);
            double ny = cy - r * Math.Sin(t);
            _debugOverlay.Children.Add(new Line
            {
                StartPoint = new Point(prevX, prevY),
                EndPoint = new Point(nx, ny),
                Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
                StrokeThickness = 1
            });
            prevX = nx;
            prevY = ny;
        }
        // 画两条边
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(cx, cy),
            EndPoint = new Point(cx + r * Math.Cos(startRad), cy - r * Math.Sin(startRad)),
            Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
            StrokeThickness = 1
        });
        _debugOverlay.Children.Add(new Line
        {
            StartPoint = new Point(cx, cy),
            EndPoint = new Point(cx + r * Math.Cos(endRad), cy - r * Math.Sin(endRad)),
            Stroke = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200)),
            StrokeThickness = 1
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
/// 椭圆 / 弧 / 扇形 / 圆环 / 扇环 几何构建器（带尖角圆角化）。
///   - cornerRadius (像素): 在每个尖角处插入半径=R 的小圆弧。
///       外圈/内圈端点尖角：圆弧半径 = min(R, 弧弦长/2, 径向边长/2)
///       扇形中心尖角：圆弧半径 = min(R/tan(|sweep|/2), 径向边长/2)
/// 派生形态：
///   inner=0, |sweep|>=360 → 实心椭圆（无尖角，自动忽略 cornerRadius）
///   inner=0, |sweep|< 360 → 扇形 (pie)
///   inner>0, |sweep|>=360 → 椭圆环 (donut)（无尖角，自动忽略 cornerRadius）
///   inner>0, |sweep|< 360 → 扇环 (ring sector)
/// </summary>
public static class EllipseArcGeometry
{
    public static PathGeometry Build(
        double x, double y, double width, double height,
        double startAngleDeg, double sweepAngleDeg, double innerRatio,
        double cornerRadius = 0)
    {
        double sweep = Clamp(sweepAngleDeg, -360, 360);
        double inner = Clamp(innerRatio, 0, 0.9999);

        double cx = x + width * 0.5;
        double cy = y + height * 0.5;
        double rx = width * 0.5;
        double ry = height * 0.5;

        if (Math.Abs(sweep) < 1e-6 || rx <= 0 || ry <= 0)
            return new PathGeometry();

        double R = Math.Max(0, cornerRadius);

        var geo = new PathGeometry();
        var fig = new PathFigure();

        double startRad = startAngleDeg * Math.PI / 180.0;
        double sweepRad = sweep * Math.PI / 180.0;
        double endRad = startRad + sweepRad;

        Point outerStart = new Point(cx + rx * Math.Cos(startRad), cy - ry * Math.Sin(startRad));
        Point outerEnd = new Point(cx + rx * Math.Cos(endRad), cy - ry * Math.Sin(endRad));

        // 完整椭圆/圆环: 无尖角，圆角无效
        if (Math.Abs(sweep) >= 360.0)
        {
            fig.StartPoint = new Point(cx + rx, cy);
            fig.IsClosed = true;
            bool ccwFull = sweep > 0;
            if (inner <= 0)
            {
                fig.Segments!.Add(new ArcSegment { Point = new Point(cx - rx, cy), Size = new Size(rx, ry), SweepDirection = ccwFull ? SweepDirection.CounterClockwise : SweepDirection.Clockwise, IsLargeArc = false });
                fig.Segments!.Add(new ArcSegment { Point = fig.StartPoint, Size = new Size(rx, ry), SweepDirection = ccwFull ? SweepDirection.CounterClockwise : SweepDirection.Clockwise, IsLargeArc = false });
            }
            else
            {
                fig.Segments!.Add(new ArcSegment { Point = fig.StartPoint, Size = new Size(rx, ry), SweepDirection = ccwFull ? SweepDirection.CounterClockwise : SweepDirection.Clockwise, IsLargeArc = true });
                fig.Segments!.Add(new LineSegment { Point = new Point(cx + rx * inner, cy) });
                fig.Segments!.Add(new ArcSegment { Point = new Point(cx + rx * inner, cy), Size = new Size(rx * inner, ry * inner), SweepDirection = ccwFull ? SweepDirection.Clockwise : SweepDirection.CounterClockwise, IsLargeArc = true });
            }
            geo.Figures!.Add(fig);
            return geo;
        }

        // 部分弧/扇形/扇环: 在尖角处插入小圆弧

        // ============================================================
        // 第一步：计算圆角参数（核心数据准备）
        // ============================================================

        // 外圈/内圈方向标志（ccw=true 表示逆时针）
        bool ccw = sweep > 0;

        // ----- 外圈（Outer Ellipse）参数 -----
        // 外圈尖角点（圆心 center 到椭圆弧的两个端点）
        Point outerCornerA0 = outerStart;  // 外圈起点尖角
        Point outerCornerA1 = outerEnd;    // 外圈终点尖角

        // 外圈椭圆在尖角处的切线向量长度（用于限制圆角半径）
        double outerTangentLenA0 = EllipseTangentLength(rx, ry, startRad);
        double outerTangentLenA1 = EllipseTangentLength(rx, ry, endRad);

        // 外圈尖角到圆心的距离（径向边长度）
        double outerRadialLenA0 = Distance(outerCornerA0, new Point(cx, cy));
        double outerRadialLenA1 = Distance(outerCornerA1, new Point(cx, cy));

        // 外圈圆角半径（用于计算角度偏移）：取用户设定值、切线长度、径向长度的一半 三者中的最小值
        double arcOffsetA0 = ComputeCornerDelta(R, outerTangentLenA0, outerRadialLenA0);
        double arcOffsetA1 = ComputeCornerDelta(R, outerTangentLenA1, outerRadialLenA1);

        // ----- 内圈（Inner Ellipse）参数 -----
        // 内圈椭圆半径 = 外圈 * inner 比例
        double innerRx = rx * inner, innerRy = ry * inner;

        // 内圈尖角点，是两个吗？
        // 是两个，因为内圈椭圆的起点和终点是不同的，所以内圈尖角点也是不同的。
        Point innerCornera0 = new Point(cx + innerRx * Math.Cos(startRad), cy - innerRy * Math.Sin(startRad));
        Point innerCornera1 = new Point(cx + innerRx * Math.Cos(endRad), cy - innerRy * Math.Sin(endRad));

        // 内圈椭圆切线长度
        double innerTangentLena0 = inner > 0 ? EllipseTangentLength(innerRx, innerRy, startRad) : 0;
        double innerTangentLena1 = inner > 0 ? EllipseTangentLength(innerRx, innerRy, endRad) : 0;

        // 内圈径向边长度
        double innerRadialLena0 = inner > 0 ? Distance(innerCornera0, new Point(cx, cy)) : 0;
        double innerRadialLena1 = inner > 0 ? Distance(innerCornera1, new Point(cx, cy)) : 0;

        // 内圈圆角半径（用于计算角度偏移）
        double arcOffseta0 = inner > 0 ? ComputeCornerDelta(R, innerTangentLena0, innerRadialLena0) : 0;
        double arcOffseta1 = inner > 0 ? ComputeCornerDelta(R, innerTangentLena1, innerRadialLena1) : 0;

        // ----- center 圆弧参数（仅实心扇形 inner<=0 时使用）-----
        // 算法移植自 RoundedPolygonGeometry.BuildWithTangents（line 220-242）：
        //   圆心 (cx,cy) 作为尖角 v，两条径向边端点 = outerRadialPointA0/A1
        //   uIn = normalize(outerRadialPointA1 - (cx,cy))
        //   uOut= normalize(outerRadialPointA0 - (cx,cy))
        //   α = acos(uIn · uOut)
        //   half = α/2
        //   r = R
        //   dCenter  = r / sin(half)
        //   dTangent = r / tan(half)
        //   bisect   = normalize(uIn + uOut)
        //   centerC = (cx,cy) + bisect · dCenter
        //   arcStart= (cx,cy) + uIn   · dTangent    (此处 = centerArcPointA1)
        //   arcEnd  = (cx,cy) + uOut  · dTangent    (此处 = centerArcPointA0)
        //   sweep  = sign(uIn × uOut)
        // 与外圈尖角圆角联动：
        //   outerRadialPointA0/A1 与 cornerRadiusA0/A1 需在 SolveCorner 后才能得到，
        //   故具体计算放在后面（依赖处），此处只声明变量。
        double centerArcRadius = 0;
        Point centerArcPointA0 = new Point(cx, cy);
        Point centerArcPointA1 = new Point(cx, cy);
        SweepDirection centerSweep = SweepDirection.CounterClockwise;
        

        // ============================================================
        // 第二步：计算四个切点坐标 + 圆角圆心 c' + 实际圆角半径 r
        //   几何关系：
        //     T_arc  = 椭圆上一点（沿椭圆切线方向从尖角后退 R 得到）
        //     L2     = 椭圆在 T_arc 处的切线
        //     L1     = 径向线（过尖角与椭圆中心）
        //     bisect = L1 与 L2 的角平分线
        //     c'     = L2 切点垂线 × bisect  ← 圆角圆心
        //     r      = |c' - T_arc|
        //     T_radial = c' 在 L1 上的垂足
        // ============================================================

        bool ccw_ = sweep > 0;

        // T_arc（椭圆主弧端点）：沿椭圆切线方向从尖角后退 R
        //   外尖角：朝扇形内部移动（向椭圆中心方向 θ 增大/减小）
        //   内尖角：朝扇形内部移动（远离椭圆中心方向）
        // angleOffsetA0: 外尖角A0的圆角偏移角度
        // angleOffsetA1: 外尖角A1的圆角偏移角度
        // angleOffseta0: 内尖角a0的圆角偏移角度
        // angleOffseta1: 内尖角a1的圆角偏移角度
        double angleOffsetA0 = arcOffsetA0 / Math.Max(outerTangentLenA0, 1e-9);
        double angleOffsetA1 = arcOffsetA1 / Math.Max(outerTangentLenA1, 1e-9);
        double angleOffseta0 = inner > 0 ? arcOffseta0 / Math.Max(innerTangentLena0, 1e-9) : 0;
        double angleOffseta1 = inner > 0 ? arcOffseta1 / Math.Max(innerTangentLena1, 1e-9) : 0;

        // outerArcAngleA0: 外尖角A0的椭圆弧角度
        // outerArcAngleA1: 外尖角A1的椭圆弧角度
        // innerArcAnglea0: 内尖角a0的椭圆弧角度
        // innerArcAnglea1: 内尖角a1的椭圆弧角度
        double outerArcAngleA0 = ccw_ ? startRad + angleOffsetA0 : startRad - angleOffsetA0;
        double outerArcAngleA1 = ccw_ ? endRad - angleOffsetA1 : endRad + angleOffsetA1;
        double innerArcAnglea0 = ccw_ ? startRad + angleOffseta0 : startRad - angleOffseta0;
        double innerArcAnglea1 = ccw_ ? endRad - angleOffseta1 : endRad + angleOffseta1;

        // outerArcPointA0: 外尖角A0的椭圆弧点,是切点吗？
        // 是切点，因为外尖角A0的椭圆弧角度是startRad + angleOffsetA0，所以外尖角A0的椭圆弧点是cx + rx * Math.Cos(outerArcAngleA0), cy - ry * Math.Sin(outerArcAngleA0)
        // outerArcPointA1: 外尖角A1的椭圆弧点,是切点吗？
        // 是切点，因为外尖角A1的椭圆弧角度是endRad - angleOffsetA1，所以外尖角A1的椭圆弧点是cx + rx * Math.Cos(outerArcAngleA1), cy - ry * Math.Sin(outerArcAngleA1)
        // innerArcPointa0: 内尖角a0的椭圆弧点,是切点吗？
        // 是切点，因为内尖角a0的椭圆弧角度是startRad + angleOffseta0，所以内尖角a0的椭圆弧点是cx + innerRx * Math.Cos(innerArcAnglea0), cy - innerRy * Math.Sin(innerArcAnglea0)
        // innerArcPointa1: 内尖角a1的椭圆弧点,是切点吗？
        // 是切点，因为内尖角a1的椭圆弧角度是endRad - angleOffseta1，所以内尖角a1的椭圆弧点是cx + innerRx * Math.Cos(innerArcAnglea1), cy - innerRy * Math.Sin(innerArcAnglea1)
        // innerArcPointa1: 内尖角a1的椭圆弧点
        Point outerArcPointA0 = new Point(cx + rx * Math.Cos(outerArcAngleA0), cy - ry * Math.Sin(outerArcAngleA0));
        Point outerArcPointA1 = new Point(cx + rx * Math.Cos(outerArcAngleA1), cy - ry * Math.Sin(outerArcAngleA1));
        Point innerArcPointa0 = inner > 0
            ? new Point(cx + innerRx * Math.Cos(innerArcAnglea0), cy - innerRy * Math.Sin(innerArcAnglea0))
            : new Point(cx, cy);
        Point innerArcPointa1 = inner > 0
            ? new Point(cx + innerRx * Math.Cos(innerArcAnglea1), cy - innerRy * Math.Sin(innerArcAnglea1))
            : new Point(cx, cy);

        // SolveCorner: 解决尖角圆角几何问题,参数？
        // cornerTip: 尖角点
        // rX: 椭圆x半径
        // rY: 椭圆y半径
        // thetaArc: 椭圆弧角度
        // isStartAngle: 是否是起始角度
        // isOuter: 是否是外尖角
        // 返回值：SolveCornerGeometry
        // Valid: 是否有效
        // TArc: 椭圆弧点
        // TArcTheta: 椭圆弧角度
        // CPrime: 圆角圆心
        // RActual: 实际圆角半径
        // TRadial: 径向点
        // Sweep: 圆角弧方向
        // L1UnitX: 径向单位向量x分量
        // L1UnitY: 径向单位向量y分量
        // 算法与 DrawCornerDebug 完全一致：
        //   1) L1 = 径向线（cornerTip → ellipse center）
        //   2) L2 = 椭圆在 tArc 处的切线（方向 dP/dθ = (-rX·sinθ, -rY·cosθ)）
        //   3) intersection = L1 × L2
        //   4) 角平分线方向由 (isStartAngle, isOuter) 决定：
        //        起始角: 外=L1-L2(顺时针), 内=L1+L2(逆时针)
        //        扫描角: 外=L1+L2(逆时针), 内=L1-L2(顺时针)
        //      用 toTRadial 自动修正方向（指向扇形内部）
        //   5) c' = L2 切点垂线（通过 tArc）× 角平分线（通过 intersection）
        //   6) tRadial = c' 在 L1 上的垂足（= intersection + l1u · proj(c'-intersection)）
        SolveCornerGeometry SolveCorner(
            Point cornerTip, double rX, double rY, double thetaArc,
            bool isStartAngle, bool isOuter, Point tRadialHint)
        {
            // T_arc 在该尖角处的椭圆坐标
            Point tArc = new Point(cx + rX * Math.Cos(thetaArc), cy - rY * Math.Sin(thetaArc));

            // L2 单位向量（椭圆在 T_arc 处的切线方向 dP/dθ）
            double tdx = -rX * Math.Sin(thetaArc);
            double tdy = -rY * Math.Cos(thetaArc);
            double tMag = Math.Sqrt(tdx * tdx + tdy * tdy);
            // 如果切线向量长度小于1e-9，则返回无效
            if (tMag < 1e-9)
                return new SolveCornerGeometry { Valid = false, TArc = cornerTip };
            // l2ux: 切线单位向量x分量
            // l2uy: 切线单位向量y分量
            double l2ux = tdx / tMag;
            double l2uy = tdy / tMag;

            // L1 单位向量（径向方向，从 ellipse center 指向 cornerTip）
            double l1dx = cornerTip.X - cx;
            double l1dy = cornerTip.Y - cy;
            // l1Mag: 径向向量长度
            double l1Mag = Math.Sqrt(l1dx * l1dx + l1dy * l1dy);
            // 如果径向向量长度小于1e-9，则返回无效
            if (l1Mag < 1e-9)
                return new SolveCornerGeometry { Valid = false, TArc = cornerTip };
            // l1ux: 径向单位向量x分量
            // l1uy: 径向单位向量y分量
            double l1ux = l1dx / l1Mag;
            double l1uy = l1dy / l1Mag;

            // ============================================================
            // intersection = L1 × L2（L1：径向线；L2：椭圆切线）
            //   为求 c' 所需的角平分线必须通过此交点（与 DrawCornerDebug 一致）
            // ============================================================
            // L1 参数式两端：cornerTip（朝向 cornerTip）和 cornerTip + (-l1ux, -l1uy)（朝向椭圆中心）
            // L2 参数式两端：tArc ± (l2ux, l2uy)
            Point l1P1 = cornerTip;
            Point l1P2 = new Point(cornerTip.X - l1ux, cornerTip.Y - l1uy);
            Point l2P1 = new Point(tArc.X - l2ux, tArc.Y - l2uy);
            Point l2P2 = new Point(tArc.X + l2ux, tArc.Y + l2uy);
            Point intersection = LineIntersect(l1P1, l1P2, l2P1, l2P2);

            // ============================================================
            // 角平分线方向：根据 (isStartAngle, isOuter) 决定 +/− 组合，
            // 再用 "指向 tRadial 一侧" 自动校正方向
            // ============================================================
            bool useClockwise; // true=L1-L2(顺时针), false=L1+L2(逆时针)
            if (isStartAngle)
                useClockwise = isOuter;   // 起始角: 外圆角用顺时针(L1-L2), 内圆角用逆时针(L1+L2)
            else
                useClockwise = !isOuter;  // 扫描角: 外圆角用逆时针(L1+L2), 内圆角用顺时针(L1-L2)

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

            // 角平分线方向应该指向扇形内部（指向 tRadialHint 一侧）。
            //   tRadialHint 是 cornerTip 沿径向回退 R 后的点：
            //     外尖角：朝椭圆中心方向回退 → toTRadial ≈ -l1u 方向
            //     内尖角：朝远离椭圆中心方向回退 → toTRadial ≈ +l1u 方向
            //   这与 DrawCornerDebug 中 toTRadial 的几何意义完全一致。
            double toTRadialX = tRadialHint.X - intersection.X;
            double toTRadialY = tRadialHint.Y - intersection.Y;
            double toTRadialLen = Math.Sqrt(toTRadialX * toTRadialX + toTRadialY * toTRadialY);
            if (toTRadialLen > 1e-9)
            {
                toTRadialX /= toTRadialLen;
                toTRadialY /= toTRadialLen;
                double dot = bisX * toTRadialX + bisY * toTRadialY;
                if (dot < 0)
                {
                    bisX = -bisX;
                    bisY = -bisY;
                }
            }

            // ============================================================
            // L2 切点垂线（通过 T_arc，方向 ⊥ L2 = (-l2uy, l2ux)）
            //   角平分线（通过 intersection，方向 (bisX, bisY)）
            //   c' = 两线交点
            // ============================================================
            double perpLx = -l2uy;
            double perpLy = l2ux;
            Point l2PerpA = new Point(tArc.X - perpLx * 1e4, tArc.Y - perpLy * 1e4);
            Point l2PerpB = new Point(tArc.X + perpLx * 1e4, tArc.Y + perpLy * 1e4);
            Point bisA    = new Point(intersection.X - bisX * 1e4, intersection.Y - bisY * 1e4);
            Point bisB    = new Point(intersection.X + bisX * 1e4, intersection.Y + bisY * 1e4);
            Point cPrime  = LineIntersect(l2PerpA, l2PerpB, bisA, bisB);

            // 实际圆角半径 = |c' - T_arc|
            double dx = cPrime.X - tArc.X;
            double dy = cPrime.Y - tArc.Y;
            // rActual: 实际圆角半径
            double rActual = Math.Sqrt(dx * dx + dy * dy);
            // 如果实际圆角半径小于1e-6，则返回无效
            if (rActual < 1e-6)
                return new SolveCornerGeometry { Valid = false, TArc = cornerTip };

            // T_radial = c' 在 L1 上的垂足
            //   垂足 = intersection + ((c' - intersection) · l1u) * l1u
            //   （与 DrawCornerDebug 完全一致：从 intersection 出发沿 L1 投影）
            double proj = (cPrime.X - intersection.X) * l1ux + (cPrime.Y - intersection.Y) * l1uy;
            Point tRadial = new Point(intersection.X + l1ux * proj, intersection.Y + l1uy * proj);

            // 圆角弧 SweepDirection：用 c' 相对 chord (T_radial → T_arc) 的位置决定
            //   cross > 0 ⇒ CCW（Y 轴向下时）
            //   取消 A0 硬编码 CCW，让四类尖角都按几何规则计算
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
                L1UnitX = l1ux, L1UnitY = l1uy,
            };
        }

        // tRadialHint: 扇形内部参考点（与 DrawCornerDebug 传入的 tRadial 同源）
        //   外尖角：从尖角向椭圆中心方向回退 R
        //   内尖角：从尖角向外（远离椭圆中心）回退 R
        // 用于 SolveCorner 内部校正角平分线方向（指向扇形内部）。
        Point hintA0 = MoveRadial(outerCornerA0, cx, cy, -arcOffsetA0);
        Point hintA1 = MoveRadial(outerCornerA1, cx, cy, -arcOffsetA1);
        Point hinta0 = inner > 0 ? MoveRadial(innerCornera0, cx, cy, +arcOffseta0) : new Point(cx, cy);
        Point hinta1 = inner > 0 ? MoveRadial(innerCornera1, cx, cy, +arcOffseta1) : new Point(cx, cy);

        // sA0: 外尖角A0的圆角几何问题
        // sA1: 外尖角A1的圆角几何问题
        var sA0 = SolveCorner(outerCornerA0, rx, ry, outerArcAngleA0, isStartAngle: true, isOuter: true, tRadialHint: hintA0);
        var sA1 = SolveCorner(outerCornerA1, rx, ry, outerArcAngleA1, isStartAngle: false, isOuter: true, tRadialHint: hintA1);

        // outerRadialPointA0: 外尖角A0的径向点
        // outerRadialPointA1: 外尖角A1的径向点
        // cornerRadiusA0: 外尖角A0的实际圆角半径
        // cornerRadiusA1: 外尖角A1的实际圆角半径
        // outerCornerSweepA0: 外尖角A0的圆角弧方向
        // outerCornerSweepA1: 外尖角A1的圆角弧方向
        Point outerRadialPointA0 = sA0.Valid ? sA0.TRadial : outerCornerA0;
        Point outerRadialPointA1 = sA1.Valid ? sA1.TRadial : outerCornerA1;
        // cornerRadiusA0: 外尖角A0的实际圆角半径
        // cornerRadiusA1: 外尖角A1的实际圆角半径
        double cornerRadiusA0 = sA0.Valid ? sA0.RActual : 0;
        double cornerRadiusA1 = sA1.Valid ? sA1.RActual : 0;
        // outerCornerSweepA0: 外尖角A0的圆角弧方向
        // outerCornerSweepA1: 外尖角A1的圆角弧方向
        SweepDirection outerCornerSweepA0 = sA0.Valid ?  SweepDirection.CounterClockwise: SweepDirection.Clockwise;
        //SweepDirection outerCornerSweepA0 = sA0.Valid ? sA0.Sweep : SweepDirection.CounterClockwise;
        SweepDirection outerCornerSweepA1 = sA1.Valid ? sA1.Sweep : SweepDirection.CounterClockwise;

        SolveCornerGeometry sA0Inner = default;
        SolveCornerGeometry sA1Inner = default;
        // innerRadialPointa0: 内尖角a0的径向点
        // innerRadialPointa1: 内尖角a1的径向点
        Point innerRadialPointa0 = new Point(cx, cy);
        Point innerRadialPointa1 = new Point(cx, cy);
        // cornerRadiusa0: 内尖角a0的实际圆角半径
        // cornerRadiusa1: 内尖角a1的实际圆角半径
        double cornerRadiusa0 = 0;
        double cornerRadiusa1 = 0;
        // innerCornerSweepA0: 内尖角A0的圆角弧方向
        // innerCornerSweepA1: 内尖角A1的圆角弧方向
        SweepDirection innerCornerSweepA0 = SweepDirection.CounterClockwise;
        SweepDirection innerCornerSweepA1 = SweepDirection.CounterClockwise;
        if (inner > 0)
        {
            // sA0Inner: 内尖角A0的圆角几何问题
            // sA1Inner: 内尖角A1的圆角几何问题
            sA0Inner = SolveCorner(innerCornera0, innerRx, innerRy, innerArcAnglea0, isStartAngle: true, isOuter: false, tRadialHint: hinta0);
            sA1Inner = SolveCorner(innerCornera1, innerRx, innerRy, innerArcAnglea1, isStartAngle: false, isOuter: false, tRadialHint: hinta1);
            // innerRadialPointa0: 内尖角a0的径向点
            // innerRadialPointa1: 内尖角a1的径向点
            innerRadialPointa0 = sA0Inner.Valid ? sA0Inner.TRadial : innerCornera0;
            innerRadialPointa1 = sA1Inner.Valid ? sA1Inner.TRadial : innerCornera1;
            // cornerRadiusa0: 内尖角a0的实际圆角半径
            // cornerRadiusa1: 内尖角a1的实际圆角半径
            cornerRadiusa0 = sA0Inner.Valid ? sA0Inner.RActual : 0;
            cornerRadiusa1 = sA1Inner.Valid ? sA1Inner.RActual : 0;
            // innerCornerSweepA0: 内尖角A0的圆角弧方向
            // innerCornerSweepA1: 内尖角A1的圆角弧方向
            innerCornerSweepA0 = sA0Inner.Valid ? sA0Inner.Sweep : SweepDirection.CounterClockwise;
            //innerCornerSweepA1 = sA1Inner.Valid ? sA1Inner.Sweep : SweepDirection.CounterClockwise;
            innerCornerSweepA1 = sA1Inner.Valid ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        }

        // ====================================================================
        // center 圆弧的实际计算（依赖 outerRadialPointA0/A1 和 cornerRadiusA0/A1）
        // 算法与 RoundedPolygonGeometry.BuildWithTangents 完全同源：
        //   v = (cx, cy)
        //   uIn = normalize(outerRadialPointA1 - v)
        //   uOut= normalize(outerRadialPointA0 - v)
        //   α = acos(uIn · uOut)
        //   half = α/2
        //   r = min(R, min(cornerRadiusA0, cornerRadiusA1))   // 跟外圈圆角联动
        //   dCenter  = r / sin(half)
        //   dTangent = r / tan(half)
        //   bisect   = normalize(uIn + uOut)
        //   arcStart = v + uIn  · dTangent   → centerArcPointA1
        //   arcEnd   = v + uOut · dTangent   → centerArcPointA0
        //   sweep    = sign(uIn × uOut)
        // ====================================================================
       
        Point v = new Point(cx, cy);
        double uInX = outerRadialPointA1.X - v.X;
        double uInY = outerRadialPointA1.Y - v.Y;
        double uOutX = outerRadialPointA0.X - v.X;
        double uOutY = outerRadialPointA0.Y - v.Y;
        double uInMag = Math.Sqrt(uInX * uInX + uInY * uInY);
        double uOutMag = Math.Sqrt(uOutX * uOutX + uOutY * uOutY);

        if (uInMag > 1e-9 && uOutMag > 1e-9)
        {
            uInX /= uInMag; uInY /= uInMag;
            uOutX /= uOutMag; uOutY /= uOutMag;
            double dot = Math.Clamp(uInX * uOutX + uInY * uOutY, -1, 1);
            double half = Math.Acos(dot) * 0.5;
            double sinHalf = Math.Max(Math.Sin(half), 1e-4);
            double tanHalf = Math.Max(Math.Tan(half), 1e-4);
            // r：圆心圆角半径 ≤ R，且 ≤ min(外圈两个尖角圆角)（视觉联动）
            double rMaxFromCorners = Math.Min(cornerRadiusA0, cornerRadiusA1);
            double rActual = (rMaxFromCorners > 1e-6) ? Math.Min(R, rMaxFromCorners) : R;
            double dCenter = rActual / sinHalf;
            double dTangent = rActual / tanHalf;
            double bxRaw = uInX + uOutX, byRaw = uInY + uOutY;
            double bMag = Math.Sqrt(bxRaw * bxRaw + byRaw * byRaw);
            // dCenter 必须能塞进两条径向边剩余长度内
            if (bMag > 1e-9 && dCenter <= Math.Min(uInMag, uOutMag))
            {
                centerArcRadius = rActual;
                centerArcPointA1 = new Point(v.X + uInX * dTangent, v.Y + uInY * dTangent);
                centerArcPointA0 = new Point(v.X + uOutX * dTangent, v.Y + uOutY * dTangent);
                double cross = uInX * uOutY - uInY * uOutX;
                centerSweep = cross > 0 ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
            }
        }
        // ============================================================
        // 第三步：构建几何路径
        //
        //   画线从 outerRadialPointA0 开始（外圈起点侧的径向切点 T_radial_A0，
        //   即尖角 A0 处圆角的切点），逆时针/顺时针沿外圈绕一圈后
        //   经内圈（或圆心）回到该起点闭合。
        //
        //   完整轨迹（实心扇形 inner<=0）：
        //     outerRadialPointA0  (起点 T_radial_A0)
        //       → [外圈 A0 角圆角 Arc]
        //       → outerArcPointA0   (外主弧端点 T_arc_A0)
        //       → [外圈主弧 Arc，沿椭圆 sweep 方向]
        //       → outerArcPointA1   (外主弧端点 T_arc_A1)
        //       → [外圈 A1 角圆角 Arc]
        //       → outerRadialPointA1
        //       → centerArcPointA1  (经径向线)
        //       → [center 圆角 Arc]
        //       → centerArcPointA0  → 闭合回起点
        //
        //   完整轨迹（扇环 inner>0）：
        //     outerRadialPointA0  (起点 T_radial_A0)
        //       → [外圈 A0 角圆角 Arc]
        //       → outerArcPointA0   → [外圈主弧 Arc] → outerArcPointA1
        //       → [外圈 A1 角圆角 Arc] → outerRadialPointA1
        //       → innerRadialPointa1 (经径向线)
        //       → [内圈 a1 角圆角 Arc] → innerArcPointa1
        //       → [内圈主弧 Arc，反向] → innerArcPointa0
        //       → [内圈 a0 角圆角 Arc] → innerRadialPointa0
        //       → 闭合回起点 outerRadialPointA0
        // ============================================================

        SweepDirection outerMainSweep = sweep > 0 ? SweepDirection.CounterClockwise : SweepDirection.Clockwise;
        SweepDirection innerMainSweep = ccw_ ? SweepDirection.Clockwise : SweepDirection.CounterClockwise;

        // 路径起点：外圈起点侧的径向切点 outerRadialPointA0（T_radial_A0）
        fig.StartPoint = outerRadialPointA0;
        fig.IsClosed = true;

        // ----- 外圈 A0 角圆角：outerRadialPointA0 → outerArcPointA0 -----
        // cornerRadiusA0: 外尖角A0的实际圆角半径
        // outerCornerSweepA0: 外尖角A0的圆角弧方向
        if (cornerRadiusA0 > 1e-6)
            // outerArcPointA0: 外尖角A0的椭圆弧点
            fig.Segments!.Add(new ArcSegment { Point = outerArcPointA0, Size = new Size(cornerRadiusA0, cornerRadiusA0), SweepDirection = outerCornerSweepA0, IsLargeArc = false });

        // ----- 外圈主弧：outerArcPointA0 → outerArcPointA1 -----
        fig.Segments!.Add(new ArcSegment { Point = outerArcPointA1, Size = new Size(rx, ry), SweepDirection = outerMainSweep, IsLargeArc = Math.Abs(outerArcAngleA1 - outerArcAngleA0) > Math.PI });

        // ----- 外圈 A1 角圆角：outerArcPointA1 → outerRadialPointA1 -----
        if (cornerRadiusA1 > 1e-6)
            fig.Segments!.Add(new ArcSegment { Point = outerRadialPointA1, Size = new Size(cornerRadiusA1, cornerRadiusA1), SweepDirection = outerCornerSweepA1, IsLargeArc = false });

        if (inner <= 0)
        {
            // ============================================================
            // 实心扇形（inner<=0）：扇形内尖用圆弧连接到圆心
            // ============================================================
            if (centerArcRadius > 1e-6)
            {
                fig.Segments!.Add(new LineSegment { Point = centerArcPointA1 });
                fig.Segments!.Add(new ArcSegment
                {
                    Point = centerArcPointA0,
                    Size = new Size(centerArcRadius, centerArcRadius),
                    SweepDirection = centerSweep,
                    IsLargeArc = false
                });
            }
            else
            {
                fig.Segments!.Add(new LineSegment { Point = new Point(cx, cy) });
            }
        }
        else
        {
            // ============================================================
            // 扇环（inner>0）：内圈圆角 + 内圈主弧 + 回到外圈起点
            // ============================================================

            // ----- 内圈 a1 角圆角：innerRadialPointa1 → innerArcPointa1 -----
            fig.Segments!.Add(new LineSegment { Point = innerRadialPointa1 });
            if (cornerRadiusa1 > 1e-6)
                fig.Segments!.Add(new ArcSegment { Point = innerArcPointa1, Size = new Size(cornerRadiusa1, cornerRadiusa1), SweepDirection = innerCornerSweepA1, IsLargeArc = false });

            // ----- 内圈主弧：innerArcPointa1 → innerArcPointa0 -----
            fig.Segments!.Add(new ArcSegment { Point = innerArcPointa0, Size = new Size(innerRx, innerRy), SweepDirection = innerMainSweep, IsLargeArc = Math.Abs(innerArcAnglea0 - innerArcAnglea1) > Math.PI });

            // ----- 内圈 a0 角圆角：innerArcPointa0 → innerRadialPointa0 -----
            if (cornerRadiusa0 > 1e-6)
                fig.Segments!.Add(new ArcSegment { Point = innerRadialPointa0, Size = new Size(cornerRadiusa0, cornerRadiusa0), SweepDirection = innerCornerSweepA0, IsLargeArc = false });

            // ----- 连接回外圈起点 -----
            fig.Segments!.Add(new LineSegment { Point = outerRadialPointA0 });
        }

        geo.Figures!.Add(fig);
        return geo;
    }

    private static double Clamp(double v, double min, double max)
        => v < min ? min : (v > max ? max : v);

    private static double EllipseTangentLength(double rx, double ry, double theta)
    {
        double dx = -rx * Math.Sin(theta);
        double dy = -ry * Math.Cos(theta);
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static double Distance(Point a, Point b)
    {
        double dx = a.X - b.X, dy = a.Y - b.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static Point MoveRadial(Point p, double cx, double cy, double delta)
    {
        double dx = p.X - cx, dy = p.Y - cy;
        double r = Math.Sqrt(dx * dx + dy * dy);
        if (r < 1e-9) return new Point(cx, cy);
        double k = (r + delta) / r;
        return new Point(cx + dx * k, cy + dy * k);
    }

    /// <summary>
    /// 尖角处"实际可回退量"：d = min(R, 弧弦长一半, 径向边长一半)。
    /// 弦长一半 = r_tangent · sin(R / (2·r_tangent))
    /// </summary>
    private static double ComputeCornerDelta(double R, double rTangent, double radialLen)
    {
        if (R <= 0 || rTangent < 1e-9) return 0;
        double chordHalf = rTangent * Math.Sin(Math.Min(R / (2.0 * rTangent), Math.PI));
        double radHalf = radialLen / 2.0;
        return Math.Max(0, Math.Min(R, Math.Min(chordHalf, radHalf)));
    }

    /// <summary>
    /// 直线 p1-p2 与直线 p3-p4 的交点（参数式求解）。两线平行时退化为 p1。
    /// </summary>
    private static Point LineIntersect(Point p1, Point p2, Point p3, Point p4)
    {
        double x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
        double x3 = p3.X, y3 = p3.Y, x4 = p4.X, y4 = p4.Y;
        double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
        if (Math.Abs(denom) < 1e-9) return p1;
        double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
        return new Point(x1 + t * (x2 - x1), y1 + t * (y2 - y1));
    }

    /// <summary>
    /// 单个尖角的圆角求解结果。Valid=false 表示该尖角退化（无圆角）。
    /// </summary>
    private struct SolveCornerGeometry
    {
        public bool Valid;
        public Point TArc;          // 椭圆主弧端点 T_arc
        public double TArcTheta;    // T_arc 对应的椭圆参数 θ
        public Point CPrime;        // 圆角圆心 c'（= L2 切点垂线 × 角平分线）
        public double RActual;      // 实际圆角半径 r = |c' - T_arc|
        public Point TRadial;       // 径向线切点（c' 在 L1 上的垂足）
        public SweepDirection Sweep; // 圆角弧的 SweepDirection
        public double L1UnitX, L1UnitY;
    }
}
