using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using ExCSS;

namespace yuyiAvalonia.UI.Controls.ToolKits;

/// <summary>
/// 虚线预设枚举，与 <see cref="Models.StrokeStyleType"/> 对应：
/// Solid=0, Dashed=1, Dot=2, DashDot=3, DashDotDot=4, Custom=5
/// </summary>
public enum StrokeDashPreset
{
    /// <summary>实线（无 DashStyle）</summary>
    Solid = 0,
    /// <summary>虚线 "8,4"</summary>
    Dashed = 1,
    /// <summary>点线 "1.5,3.5"</summary>
    Dot = 2,
    /// <summary>线点 "8,3,1.5,3"</summary>
    DashDot = 3,
    /// <summary>线点点 "8,3,1.5,3,1.5,3"</summary>
    DashDotDot = 4,
    /// <summary>自定义（读取 CustomDashArray）</summary>
    Custom = 5
}

/// <summary>
/// 自定义控件，用 DrawingContext 渲染描边（支持实线 / 预设虚线 / 自定义虚线，
/// 描边画刷支持纯色 / 渐变 / 图片）。
///
/// 关键优势：不受 Path.Data.Bounds 裁切，尖角 spike 超出原路径 bounds 也能完整展示。
///
/// 使用方式：
/// 1) 设置 <see cref="Geometry"/> 为任意 Geometry（矩形/椭圆/Path/复合几何等）；
/// 2) 设置 <see cref="StrokeBrush"/> 为任意 IBrush（SolidColorBrush / LinearGradientBrush / ImageBrush / DrawingBrush）；
/// 3) 设置 <see cref="StrokeWidth"/>；
/// 4) 设置 <see cref="DashPreset"/> 选择预设样式，或设 Custom + <see cref="CustomDashArray"/>；
/// 5) 可选设置 <see cref="StrokeLineJoin"/> / <see cref="StrokeLineCap"/> / <see cref="StrokeMiterLimit"/>。
/// </summary>
public class DashedStrokeControl : Control
{
    #region Instance Cache (per-control, GC'd with the control)

    // 缓存 key 各组件
    private Geometry? _cacheSource;
    private double[]? _cacheDashPattern;
    private double _cacheDashOffset;
    private double _cacheStrokeWidth;
    private PenLineJoin _cacheLineJoin;
    private PenLineCap _cacheLineCap;
    private double _cacheMiterLimit;
    private Geometry? _cacheWidened;

    #region Styled Properties

    public static readonly StyledProperty<Geometry?> GeometryProperty =
        AvaloniaProperty.Register<DashedStrokeControl, Geometry?>(nameof(Geometry));

    /// <summary>
    /// <summary>描边画刷（SolidColorBrush / LinearGradientBrush / ImageBrush / DrawingBrush）</summary>
    /// </summary>
    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<DashedStrokeControl, IBrush?>(nameof(StrokeBrush));

    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<DashedStrokeControl, double>(nameof(StrokeWidth), 1.0);

    /// <summary>
    /// 虚线预设：Solid（实线）/ Dashed / Dot / DashDot / DashDotDot / Custom。
    /// 优先级高于 <see cref="StrokeDashArray"/>。
    /// </summary>
    public static readonly StyledProperty<StrokeDashPreset> DashPresetProperty =
        AvaloniaProperty.Register<DashedStrokeControl, StrokeDashPreset>(nameof(DashPreset), StrokeDashPreset.Solid);

    /// <summary>
    /// 原始虚线数组（按设备像素，DrawingContext 直接渲染，无需归一化）。
    /// 与 <see cref="DashPreset"/> == Solid 冲突时，DashPreset 优先。
    /// </summary>
    public static readonly StyledProperty<AvaloniaList<double>?> StrokeDashArrayProperty =
        AvaloniaProperty.Register<DashedStrokeControl, AvaloniaList<double>?>(nameof(StrokeDashArray));

    /// <summary>
    /// 自定义虚线数组字符串（格式如 "8,4" 或 "4,2,1,2"）。
    /// 仅在 <see cref="DashPreset"/> == Custom 时生效。
    /// </summary>
    public static readonly StyledProperty<string?> CustomDashArrayProperty =
        AvaloniaProperty.Register<DashedStrokeControl, string?>(nameof(CustomDashArray));

    /// <summary>虚线相位偏移（像素）</summary>
    public static readonly StyledProperty<double> StrokeDashOffsetProperty =
        AvaloniaProperty.Register<DashedStrokeControl, double>(nameof(StrokeDashOffset));

    public static readonly StyledProperty<PenLineJoin> StrokeLineJoinProperty =
        AvaloniaProperty.Register<DashedStrokeControl, PenLineJoin>(nameof(StrokeLineJoin), PenLineJoin.Miter);

    public static readonly StyledProperty<PenLineCap> StrokeLineCapProperty =
        AvaloniaProperty.Register<DashedStrokeControl, PenLineCap>(nameof(StrokeLineCap), PenLineCap.Flat);

    public static readonly StyledProperty<double> StrokeMiterLimitProperty =
        AvaloniaProperty.Register<DashedStrokeControl, double>(nameof(StrokeMiterLimit), 4.0);

    #endregion
    private bool TryHitCache(Geometry source, double[]? dashPattern, double dashOffset,
        out Geometry? widened)
    {
        if (_cacheWidened == null || _cacheSource != source)
        {
            widened = null;
            return false;
        }
        if (_cacheStrokeWidth != StrokeWidth ||
            _cacheDashOffset != dashOffset ||
            _cacheLineJoin != StrokeLineJoin ||
            _cacheLineCap != StrokeLineCap ||
            _cacheMiterLimit != StrokeMiterLimit)
        {
            widened = null;
            return false;
        }
        if (!DashPatternEquals(_cacheDashPattern, dashPattern))
        {
            widened = null;
            return false;
        }
        widened = _cacheWidened;
        return true;
    }

    private static bool DashPatternEquals(double[]? a, double[]? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if (a == null || b == null) return false;
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    #endregion

    #region CLR Properties

    public Geometry? Geometry
    {
        get => GetValue(GeometryProperty);
        set => SetValue(GeometryProperty, value);
    }

    public IBrush? StrokeBrush
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public StrokeDashPreset DashPreset
    {
        get => GetValue(DashPresetProperty);
        set => SetValue(DashPresetProperty, value);
    }

    public AvaloniaList<double>? StrokeDashArray
    {
        get => GetValue(StrokeDashArrayProperty);
        set => SetValue(StrokeDashArrayProperty, value);
    }

    public string? CustomDashArray
    {
        get => GetValue(CustomDashArrayProperty);
        set => SetValue(CustomDashArrayProperty, value);
    }

    public double StrokeDashOffset
    {
        get => GetValue(StrokeDashOffsetProperty);
        set => SetValue(StrokeDashOffsetProperty, value);
    }

    public PenLineJoin StrokeLineJoin
    {
        get => GetValue(StrokeLineJoinProperty);
        set => SetValue(StrokeLineJoinProperty, value);
    }

    public PenLineCap StrokeLineCap
    {
        get => GetValue(StrokeLineCapProperty);
        set => SetValue(StrokeLineCapProperty, value);
    }

    public double StrokeMiterLimit
    {
        get => GetValue(StrokeMiterLimitProperty);
        set => SetValue(StrokeMiterLimitProperty, value);
    }

    #endregion

    static DashedStrokeControl()
    {
        AffectsRender<DashedStrokeControl>(
            GeometryProperty,
            StrokeProperty,
            StrokeWidthProperty,
            DashPresetProperty,
            StrokeDashArrayProperty,
            CustomDashArrayProperty,
            StrokeDashOffsetProperty,
            StrokeLineJoinProperty,
            StrokeLineCapProperty,
            StrokeMiterLimitProperty);
    }

    public override void Render(DrawingContext context)
    {
        var geometry = Geometry;
        var strokeBrush = StrokeBrush;
        if (geometry == null || strokeBrush == null || StrokeWidth <= 0)
            return;

        // 1) 优先用预设
        // 2) 次选 StrokeDashArray（兼容性）
        // 3) 末选 CustomDashArray 字符串（仅当 DashPreset==Custom）
        // 4) 全空时走实线
        double[]? dashPattern = null;
        double dashOffset = 0;
        if (DashPreset != StrokeDashPreset.Solid)
        {
            (dashPattern, dashOffset) = BuildDashPattern();
        }
        else if (StrokeDashArray != null && StrokeDashArray.Count > 0)
        {
            dashPattern = StrokeDashArray.ToArray();
            dashOffset = StrokeDashOffset;
        }

        // === 实例级缓存命中直接走 brush 填充 ===
        if (TryHitCache(geometry, dashPattern, dashOffset, out var cachedWidened) && cachedWidened != null)
        {
            context.DrawGeometry(strokeBrush, null, cachedWidened);
            return;
        }

        // 构造用于加宽的"无虚线" pen（LineCap 不影响 spike，仅影响端点）
        var widenPen = new Pen(
            strokeBrush,
            StrokeWidth,
            null,
            StrokeLineCap,
            StrokeLineJoin,
            StrokeMiterLimit);

        // === 统一走"加宽几何 + brush 填充"路径 ===
        // 思路：
        //   1) 若有 dash 模式：先把整条 path 沿轮廓按 dashPattern 切成多段 dash 子段，
        //      每段是独立 Geometry（TryGetSegment），合并成一个新的 StreamGeometry；
        //   2) widened = dashGeo.GetWidenedGeometry(widenPen)
        //      关键：spike 现在是 widened 几何内部的一部分，不再被 Skia 按原 geometry.Bounds 裁切；
        //   3) DrawGeometry(strokeBrush, null, widened) —— 用 brush 填充（pen 为 null），spike 完整保留。
        Geometry? toWiden;
        if (dashPattern != null && dashPattern.Length > 0)
        {
            toWiden = BuildDashedGeometry(geometry, dashPattern, dashOffset);
        }
        else
        {
            toWiden = geometry;
        }

        if (toWiden == null)
            return;

        var widened = toWiden.GetWidenedGeometry(widenPen);

        if (widened != null)
        {
            // 写缓存（实例级，随控件 GC 自动释放，零内存泄漏）
            _cacheSource = geometry;
            _cacheDashPattern = dashPattern;
            _cacheDashOffset = dashOffset;
            _cacheStrokeWidth = StrokeWidth;
            _cacheLineJoin = StrokeLineJoin;
            _cacheLineCap = StrokeLineCap;
            _cacheMiterLimit = StrokeMiterLimit;
            _cacheWidened = widened;
            
            context.DrawGeometry(strokeBrush, null, widened);
        }

    }

    /// <summary>
    /// 按 dash 模式沿 path 轮廓切出每段 dash 子段，合并为一个新的 <see cref="Geometry"/>。
    /// 等价于 WPF 的 <c>Geometry.GetDashGeometry(pen)</c>（Avalonia 11 无此 API，
    /// 故基于 <see cref="Geometry.TryGetPointAndTangentAtDistance"/> 沿轮廓采样后用 SVG 字符串重建）。
    /// </summary>
    private static Geometry? BuildDashedGeometry(Geometry source, double[] dashPattern, double dashOffset)
    {
        // 用 source.ContourLength 拿总长（O(1)）
        double totalLength;
        try
        {
            totalLength = source.ContourLength;
        }
        catch
        {
            totalLength = ApproximateContourLength(source, source.Bounds);
        }
        if (totalLength <= 0 || double.IsNaN(totalLength) || double.IsInfinity(totalLength))
            return source;

        // 闭合 path 防御：避免最后一段 dash 跨过起点接缝造成重复像素
        // （GetPointAndTangentAtDistance(totalLength) 起点 == 0 处相同点）
        if (source.TryGetPointAndTangentAtDistance(0, out var p0, out _) &&
            source.TryGetPointAndTangentAtDistance(totalLength, out var pN, out _))
        {
            double dx = p0.X - pN.X, dy = p0.Y - pN.Y;
            if (dx * dx + dy * dy < 1e-6)
            {
                // 闭合 path：把总长减去极小值，防止末段 dash 跨越接缝
                totalLength -= 0.5;
                if (totalLength <= 0) return source;
            }
        }

        double cycle = Cycle(dashPattern);
        if (cycle <= 0) return source;
        // dashOffset 正向 = 把 dash pattern 起点向前推（沿 path）
        double shift = ((dashOffset % cycle) + cycle) % cycle;

        // 收集所有 dash 实线段
        var segments = new List<(double Start, double End)>();
        // 第一个 dash 起点 = -shift（因为 path 从 0 开始）
        double cursor = -shift;
        int idx = 0;
        // 走完整个 path
        while (cursor < totalLength)
        {
            double dashLen = dashPattern[idx % dashPattern.Length];
            double gapLen = dashPattern[(idx + 1) % dashPattern.Length];
            double segStart = cursor;
            double segEnd = cursor + dashLen;
            if (segEnd > 0 && segStart < totalLength)
            {
                double clampedStart = Math.Max(0, segStart);
                double clampedEnd = Math.Min(totalLength, segEnd);
                if (clampedEnd > clampedStart)
                    segments.Add((clampedStart, clampedEnd));
            }
            cursor += dashLen + gapLen;
            idx += 2;
        }

        if (segments.Count == 0)
            return source;

        // 段内采样步长：约 1/4 描边宽度，或 2 像素（取较小）
        double sampleStep = 2.0;
        var sb = new System.Text.StringBuilder();
        for (int s = 0; s < segments.Count; s++)
        {
            var (segStart, segEnd) = segments[s];
            if (segEnd <= segStart) continue;
            // 段内采样点数
            int n = Math.Max(2, (int)Math.Ceiling((segEnd - segStart) / sampleStep) + 1);
            // 起点 M
            if (source.TryGetPointAndTangentAtDistance(segStart, out var startPt, out _))
            {
                sb.Append('M').Append(F(startPt.X)).Append(',').Append(F(startPt.Y)).Append(' ');
            }
            // 段内点列 L
            for (int k = 1; k <= n; k++)
            {
                double t = segStart + (segEnd - segStart) * k / n;
                if (t > segEnd) t = segEnd;
                if (source.TryGetPointAndTangentAtDistance(t, out var p, out _))
                {
                    sb.Append('L').Append(F(p.X)).Append(',').Append(F(p.Y)).Append(' ');
                }
            }
        }
        if (sb.Length == 0)
            return source;
        try
        {
            return StreamGeometry.Parse(sb.ToString());
        }
        catch
        {
            return source;
        }
    }

    private static string F(double v) => v.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// 估算 path 轮廓总长：用 <see cref="Geometry.TryGetPointAtDistance"/> 二分定位。
    /// 备用方案（<see cref="Geometry.ContourLength"/> 不可用时）。
    /// </summary>
    private static double ApproximateContourLength(Geometry geometry, Rect bounds)
    {
        double lo = 0;
        double hi = Math.Max(bounds.Width, bounds.Height) * 2 + 1000;
        while (geometry.TryGetPointAtDistance(hi, out _))
        {
            lo = hi;
            hi *= 2;
            if (hi > 1e9) break;
        }
        for (int k = 0; k < 30; k++)
        {
            double mid = (lo + hi) / 2;
            if (geometry.TryGetPointAtDistance(mid, out _))
                lo = mid;
            else
                hi = mid;
        }
        return lo;
    }

    private static double Cycle(double[] dashPattern)
    {
        double sum = 0;
        foreach (var d in dashPattern) sum += Math.Abs(d);
        return sum <= 0 ? 1 : sum;
    }

    /// <summary>
    /// 根据 DashPreset 返回原始 dash 数组（像素）与偏移。
    /// </summary>
    private (double[]? Pattern, double Offset) BuildDashPattern()
    {
        double[]? rawDashes = DashPreset switch
        {
            StrokeDashPreset.Dashed     => new[] { 8.0, 4.0 },
            StrokeDashPreset.Dot        => new[] { 1.5, 3.5 },
            StrokeDashPreset.DashDot    => new[] { 8.0, 3.0, 1.5, 3.0 },
            StrokeDashPreset.DashDotDot => new[] { 8.0, 3.0, 1.5, 3.0, 1.5, 3.0 },
            StrokeDashPreset.Custom     => ParseCustomDashArray(CustomDashArray),
            _ => null
        };

        if (rawDashes == null || rawDashes.Length == 0)
            return (null, 0);

        return (rawDashes, StrokeDashOffset);
    }

    private static double[]? ParseCustomDashArray(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Split(new[] { ',', ' ', ';' },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            return null;

        var dashes = new List<double>(parts.Length);
        foreach (var p in parts)
        {
            if (double.TryParse(p, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double v))
                dashes.Add(Math.Abs(v));
        }

        return dashes.Count > 0 ? dashes.ToArray() : null;
    }
}
