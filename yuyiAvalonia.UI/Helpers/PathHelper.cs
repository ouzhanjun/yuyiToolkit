using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.Models;
using DoubleCollection = Avalonia.Collections.AvaloniaList<double>;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.UI.Helpers;

/// <summary>
/// Path 渲染辅助类，提供线条样式渲染、画刷创建、几何配置等静态方法
/// </summary>
public static class PathHelper
{
    #region 线条样式

    public static DoubleCollection? GetStrokeDashArray(string value)
    {
        if(value== null) 
            return null;
        var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length > 0)
        {
            var dashArrary = new DoubleCollection();
            for (int i = 0; i < parts.Length; i++)
            {
                if (double.TryParse(parts[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
                    dashArrary.Add(v);
                else
                    dashArrary.Add(4);
            }
            return dashArrary;
        }

        return null;
    }

    /// <summary>
    /// 根据 StrokeStyle 和自定义虚线数组获取虚线集合
    /// </summary>
    public static DoubleCollection? GetStrokeDashArray(StrokeStyleType style, double[]? customDashArray)
    {
        if (customDashArray != null && customDashArray.Length > 0)
            return new DoubleCollection(customDashArray);

        var dashArray = GetStrokeStyleItems();

        return GetStrokeDashArray(dashArray.FirstOrDefault(x => x.Value == style)!.DashArray);
    }

    public static List<StrokeStyleItem> GetStrokeStyleItems()
    {
        List<StrokeStyleItem> items = new List<StrokeStyleItem>
        {
            new(StrokeStyleType.Dashed, "4,2",StrokeType.Dashed),
            new(StrokeStyleType.Dot, "1,3",StrokeType.Dashed),
            new(StrokeStyleType.DashDot, "6,2,1,2",StrokeType.Dashed),
            new(StrokeStyleType.DashDotDot, "6,2,1,2,1,2",StrokeType.Dashed)
        };

        return items;
    }

    public static List<StrokeTypeItem> GetStrokeTypeItems()
    {
        List<StrokeTypeItem> items = new List<StrokeTypeItem>
        {
            new(StrokeType.Solid, "实线", Icon.Solid),
            new(StrokeType.Dashed, "虚线", Icon.Dashed),
            new(StrokeType.Custom, "自定义", Icon.Custom),
        };

        return items;
    }

    /// <summary>
    /// 将虚线样式应用到 Path（归一化到线宽倍数）
    /// </summary>
    public static void ApplyDashStyle(Path path, DoubleCollection? dashArray, double dashOffset, double strokeWidth)
    {
        if (dashArray == null || dashArray.Count == 0)
        {
            path.StrokeDashArray = null;
            path.StrokeDashOffset = 0;
            return;
        }
        var normalized = new DoubleCollection();
        foreach (var v in dashArray)
            normalized.Add(strokeWidth > 0 ? v / strokeWidth : v);
        path.StrokeDashArray = normalized;
        path.StrokeDashOffset = strokeWidth > 0 ? dashOffset / strokeWidth : dashOffset;
    }

    /// <summary>
    /// 将 PathRenderParams 中的线条样式（宽度、线帽、线连接、虚线）应用到 Path 对象
    /// </summary>
    public static void ApplyStyle(Path path, PathRenderParams prms)
    {
        path.StrokeThickness = prms.StrokeWidth;
        path.StrokeJoin = prms.StrokeJoin switch
        {
            StrokeJoin.Round => PenLineJoin.Round,
            StrokeJoin.Bevel => PenLineJoin.Bevel,
            _ => PenLineJoin.Miter
        };
        path.StrokeMiterLimit = prms.MiterLimit;
        path.StrokeLineCap = prms.StrokeEndpoints switch
        {
            StrokeEndpoints.Round => PenLineCap.Round,
            StrokeEndpoints.Square => PenLineCap.Square,
            _ => PenLineCap.Flat
        };

        if (prms.StrokeStyle == StrokeStyleType.Solid)
        {
            path.StrokeDashArray = null;
            path.StrokeDashOffset = 0;
        }
        else
        {
            var dashArray = GetStrokeDashArray(prms.StrokeStyle, null);
            ApplyDashStyle(path, dashArray, prms.StrokeDashOffset, prms.StrokeWidth);
        }

        path.Width = prms.Width;
        path.Height = prms.Height;
        path.Margin = prms.Margin;
        path.Data = prms.Data;
        path.IsVisible = prms.IsVisible;

        ApplyFill(path, prms.Fill, prms.FillOpacity);
    }

    /// <summary>
    /// 将填充画刷应用到 Path 的 Fill 属性
    /// </summary>
    public static void ApplyFill(Path path, IBrush? fill, double fillOpacity = 1.0)
    {
        if (fill == null)
        {
            path.Fill = Brushes.Transparent;
        }
        else
        {
            path.Fill = fill;
            path.Opacity = fillOpacity;
        }
    }

    #endregion

    #region 画刷创建

    /// <summary>
    /// 根据 StrokeFillType 和相关属性创建边线画刷（支持渐变和图片填充）
    /// </summary>
    public static IBrush? CreateStrokeBrush(StrokeFillType fillType, string solidColor, double solidOpacity,
        string gradientStartColor, string gradientEndColor, double gradientAngle, double gradientOpacity,
        string? imagePath, Stretch imageStretch, TileMode imageTileMode, double imageOpacity,
        AlignmentX alignX, AlignmentY alignY)
    {
        switch (fillType)
        {
            case StrokeFillType.None:
                return null;
            case StrokeFillType.Solid:
                if (PropertyValidator.TryParseColor(solidColor, out var color))
                    return new SolidColorBrush(Color.FromArgb((byte)(solidOpacity * 255), color.R, color.G, color.B));
                return null;
            case StrokeFillType.Gradient:
                return CreateGradientBrush(gradientStartColor, gradientEndColor, gradientAngle, gradientOpacity);
            case StrokeFillType.Image:
                if (string.IsNullOrEmpty(imagePath))
                    return null;
                return CreateImageBrush(imagePath, imageStretch, imageTileMode, alignX, alignY, imageOpacity);
            default:
                return null;
        }
    }

    /// <summary>
    /// 创建渐变画刷（角度渐变，从中心沿角度方向延伸到边框）
    /// </summary>
    public static IBrush CreateGradientBrush(string startColor, string endColor, double angle, double opacity)
    {
        var angleRad = angle * Math.PI / 180.0;
        var cosA = Math.Cos(angleRad);
        var sinA = Math.Sin(angleRad);
        double endX, endY;

        if (Math.Abs(cosA) < 0.001)
        {
            endX = 0.5;
            endY = sinA > 0 ? 1.0 : 0.0;
        }
        else if (Math.Abs(sinA) < 0.001)
        {
            endY = 0.5;
            endX = cosA > 0 ? 1.0 : 0.0;
        }
        else
        {
            var tRight = 1.0 / cosA;
            var tLeft = -1.0 / cosA;
            var tBottom = 1.0 / sinA;
            var tTop = -1.0 / sinA;
            var tCandidates = new[] { tRight, tBottom, tTop, tLeft }.Where(t => t > 0.001).ToList();
            var t = tCandidates.Min();
            endX = Math.Max(0.0, Math.Min(1.0, 0.5 + cosA * t));
            endY = Math.Max(0.0, Math.Min(1.0, 0.5 + sinA * t));
        }

        var startX = 0.5 - (endX - 0.5);
        var startY = 0.5 - (endY - 0.5);
        PropertyValidator.TryParseColor(startColor, out var start, Colors.White);
        PropertyValidator.TryParseColor(endColor, out var end, Colors.Black);
        var alpha = (byte)(opacity * 255);

        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, startY, RelativeUnit.Relative),
            EndPoint = new RelativePoint(endX, endY, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Color.FromArgb(alpha, start.R, start.G, start.B), 0),
                new GradientStop(Color.FromArgb(alpha, end.R, end.G, end.B), 1)
            }
        };
    }

    /// <summary>
    /// 创建简单的两点渐变画刷（中心点向角度方向延伸 50%）
    /// </summary>
    public static IBrush CreateSimpleGradientBrush(Color startColor, Color endColor, double angle, double opacity)
    {
        var radians = angle * Math.PI / 180.0;
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);
        double startX = 0.5 - dx * 0.5, startY = 0.5 - dy * 0.5;
        double endX = 0.5 + dx * 0.5, endY = 0.5 + dy * 0.5;

        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, startY, RelativeUnit.Relative),
            EndPoint = new RelativePoint(endX, endY, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Color.FromArgb((byte)(opacity * 255), startColor.R, startColor.G, startColor.B), 0),
                new GradientStop(Color.FromArgb((byte)(opacity * 255), endColor.R, endColor.G, endColor.B), 1)
            }
        };
    }

    /// <summary>
    /// 创建边线图片画刷（Border 变体，使用 DestinationRect）
    /// </summary>
    public static IBrush? CreateBorderImageBrush(string imagePath, Stretch stretch, TileMode tileMode, double opacity)
    {
        if (string.IsNullOrEmpty(imagePath))
            return null;
        try
        {
            var uri = new Uri(imagePath);
            var bitmap = new Bitmap(uri.LocalPath);

            var brush = new ImageBrush(bitmap)
            {
                TileMode = tileMode,
                Opacity = opacity
            };

            if (tileMode != TileMode.None)
            {
                brush.Stretch = Stretch.None;
                brush.DestinationRect = new RelativeRect(
                    0, 0,
                    (double)bitmap.Size.Width,
                    (double)bitmap.Size.Height,
                    RelativeUnit.Absolute);
            }
            else
            {
                brush.Stretch = stretch;
                brush.DestinationRect = new RelativeRect(0, 0, 1, 1, RelativeUnit.Relative);
            }

            return brush;
        }
        catch { return null; }
    }

    /// <summary>
    /// 创建边线图片画刷（ShapeFlow 变体，使用 AlignmentX/Y）
    /// </summary>
    public static IBrush? CreateImageBrush(string imagePath, Stretch stretch, TileMode tileMode,
        AlignmentX alignX, AlignmentY alignY, double opacity)
    {
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            return null;
        try
        {
            var bitmap = new Bitmap(imagePath);

            var brush = new ImageBrush(bitmap)
            {
                AlignmentX = alignX,
                AlignmentY = alignY,
                TileMode = tileMode,
                Opacity = opacity
            };

            if (tileMode != TileMode.None)
            {
                brush.Stretch = Stretch.None;
                brush.DestinationRect = new RelativeRect(
                    0, 0,
                    (double)bitmap.Size.Width,
                    (double)bitmap.Size.Height,
                    RelativeUnit.Absolute);
            }
            else
            {
                brush.Stretch = stretch;
            }

            return brush;
        }
        catch { return null; }
    }

    #endregion

    #region 几何配置

    /// <summary>
    /// 从 CanvasElement 获取边线宽度（支持均匀和四边独立模式）
    /// </summary>
    public static double GetStrokeWidth(StrokeWidthMode mode, double uniform, double left, double top, double right, double bottom)
    {
        return mode == StrokeWidthMode.Uniform
            ? uniform
            : Math.Max(left, Math.Max(top, Math.Max(right, bottom)));
    }

    /// <summary>
    /// 构建 Path 的几何数据、大小和边距
    /// </summary>
    public static void ConfigurePathGeometry(Path path, double width, double height, double strokeWidth, double cornerRadius)
    {
        var halfStroke = strokeWidth / 2;
        path.Width = width + strokeWidth;
        path.Height = height + strokeWidth;
        path.Margin = new Thickness(-halfStroke);
        path.Data = new RectangleGeometry(new Rect(halfStroke, halfStroke, width, height), cornerRadius, cornerRadius);
    }

    #endregion

    #region 复合画刷与填充绘制

    #endregion

    #region 多层 FillItem 填充绘制

    /// <summary>
    /// 根据 FillItem 集合构建复合 DrawingBrush，支持多层填充叠加。
    /// 当用作 Path 的描边画刷时，需要保证画刷坐标系覆盖 stroke 整个绘制区域
    /// （含尖角外凸部分），否则三角形等尖角处会被截断。
    /// </summary>
    public static DrawingBrush? BuildMultiFillBrush(FillItem[] fills, double width, double height)
    {
        if (fills == null || fills.Length == 0)
            return null;

        var drawingGroup = new DrawingGroup();

        // 画刷坐标系用更大的范围并居中，保证 stroke 区域（含尖角外凸）都被覆盖
        
        double padding = 10 * 2.5;
        var bounds = new Rect(-padding, -padding, width + padding * 2, height + padding * 2);

        using (var context = drawingGroup.Open())
        {

            foreach (var fill in fills)
            {
                if (!fill.IsEnabled)
                    continue;

                if (fill.FillType == FillType.Solid)
                {
                    var brush = fill.CreateBrush(width, height);
                    if (brush != null)
                        context.DrawRectangle(brush, null, bounds);
                }
                else if (fill.FillType == FillType.Gradient)
                {
                    if (fill.GradientKind == GradientKind.Angle)
                    {
                        DrawAngleGradientFill(context, bounds, fill);
                    }
                    else if (fill.GradientKind == GradientKind.Diamond)
                    {
                        DrawDiamondGradientFill(context, bounds, fill);
                    }
                    else
                    {
                        var brush = fill.CreateBrush(width, height);
                        if (brush != null)
                            context.DrawRectangle(brush, null, bounds);
                    }
                }
                else if (fill.FillType == FillType.Image)
                {
                    var imgBrush = fill.CreateBrush(width, height);
                    if (imgBrush != null)
                        context.DrawRectangle(imgBrush, null, bounds);
                }
            }
        }

        return new DrawingBrush(drawingGroup)
        {
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Center,
            AlignmentY = AlignmentY.Center,
            SourceRect = new RelativeRect(-padding, -padding, width + padding * 2, height + padding * 2, RelativeUnit.Absolute),
            DestinationRect = new RelativeRect(0, 0, 1, 1, RelativeUnit.Relative)
        };
    }

    /// <summary>
    /// 绘制角度渐变填充（从中心按角度延伸的多段扇形渐变）
    /// </summary>
    public static void DrawAngleGradientFill(DrawingContext context, Rect bounds, FillItem fill)
    {
        var stops = fill.GradientStops;
        if (stops == null || stops.Count < 2)
        {
            if (!Color.TryParse(fill.GradientStartColor, out var startColor))
                startColor = Colors.White;
            if (!Color.TryParse(fill.GradientEndColor, out var endColor))
                endColor = Colors.Black;
            DrawAngleGradientWithStops(context, bounds, fill.GradientAngle, fill.Opacity,
                new List<(double offset, Color color)> { (0.0, startColor), (1.0, endColor) });
            return;
        }

        var sortedStops = stops.OrderBy(s => s.Offset).ToList();
        var stopList = new List<(double offset, Color color)>();
        foreach (var s in sortedStops)
        {
            Color.TryParse(s.Color, out var c);
            stopList.Add((s.Offset, c));
        }
        DrawAngleGradientWithStops(context, bounds, fill.GradientAngle, fill.Opacity, stopList);
    }

    /// <summary>
    /// 绘制角度渐变（给定停止点列表），使用超采样单色扇片实现平滑锥形渐变
    /// </summary>
    public static void DrawAngleGradientWithStops(DrawingContext context, Rect bounds, double baseAngle,
        double globalOpacity, List<(double offset, Color color)> stops)
    {
        if (stops.Count < 2)
            return;

        var cx = bounds.Width / 2;
        var cy = bounds.Height / 2;
        var perimeter = (bounds.Width + bounds.Height) * 2.0;
        var stepCount = Math.Max(2048, (int)Math.Ceiling(perimeter * 4));

        for (int i = 0; i < stepCount; i++)
        {
            var t1 = (double)i / stepCount;
            var t2 = (double)(i + 1) / stepCount;
            var midT = (t1 + t2) * 0.5;
            var angle1 = baseAngle + t1 * 360.0;
            var angle2 = baseAngle + t2 * 360.0;
            var rad1 = angle1 * Math.PI / 180.0;
            var rad2 = angle2 * Math.PI / 180.0;

            var outer1 = GetRectRayIntersection(bounds, cx, cy, Math.Cos(rad1), Math.Sin(rad1));
            var outer2 = GetRectRayIntersection(bounds, cx, cy, Math.Cos(rad2), Math.Sin(rad2));
            var color = InterpolateColor(stops, midT, globalOpacity);

            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(cx, cy), IsClosed = true };
            figure.Segments.Add(new LineSegment { Point = outer1 });
            figure.Segments.Add(new LineSegment { Point = outer2 });
            geometry.Figures.Add(figure);

            context.DrawGeometry(new SolidColorBrush(color), null, geometry);
        }
    }

    /// <summary>
    /// 绘制菱形渐变填充（按曼哈顿距离从中心向四角扩散）
    /// </summary>
    public static void DrawDiamondGradientFill(DrawingContext context, Rect bounds, FillItem fill)
    {
        var stops = fill.GradientStops;
        if (stops == null || stops.Count < 2)
        {
            if (!Color.TryParse(fill.GradientStartColor, out var startColor))
                startColor = Colors.White;
            if (!Color.TryParse(fill.GradientEndColor, out var endColor))
                endColor = Colors.Black;
            DrawDiamondGradientWithStops(context, bounds, fill.GradientAngle, fill.Opacity,
                new List<(double offset, Color color)> { (0.0, startColor), (1.0, endColor) });
            return;
        }

        var sortedStops = stops.OrderBy(s => s.Offset).ToList();
        var stopList = new List<(double offset, Color color)>();
        foreach (var s in sortedStops)
        {
            Color.TryParse(s.Color, out var c);
            stopList.Add((s.Offset, c));
        }

        DrawDiamondGradientWithStops(context, bounds, fill.GradientAngle, fill.Opacity, stopList);
    }

    /// <summary>
    /// 绘制菱形渐变（给定停止点列表），使用覆盖整个 bounds 的分层菱形实现
    /// </summary>
    public static void DrawDiamondGradientWithStops(DrawingContext context, Rect bounds, double angle,
        double globalOpacity, List<(double offset, Color color)> stops)
    {
        if (stops.Count < 2 || bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var cx = bounds.Width / 2;
        var cy = bounds.Height / 2;
        var layerCount = Math.Max(256, (int)Math.Ceiling((bounds.Width + bounds.Height) * 1.5));
        var transform = Math.Abs(angle % 360.0) > 0.001
            ? new RotateTransform(angle, cx, cy)
            : null;

        for (int i = layerCount; i >= 1; i--)
        {
            var t = (double)i / layerCount;
            var color = InterpolateColor(stops, t, globalOpacity);
            var geometry = CreateCoverageDiamondGeometry(bounds, t, cx, cy, transform);
            context.DrawGeometry(new SolidColorBrush(color), null, geometry);
        }

        context.DrawRectangle(new SolidColorBrush(InterpolateColor(stops, 0, globalOpacity)), null,
            new Rect(cx - 0.5, cy - 0.5, 1, 1));
    }

    private static Geometry CreateCoverageDiamondGeometry(Rect bounds, double scale, double cx, double cy, Transform? transform)
    {
        var clampedScale = Math.Clamp(scale, 0.0, 1.0);
        var maxRadius = bounds.Width + bounds.Height;
        var halfExtent = maxRadius * 0.5 * clampedScale;

        var geometry = new PathGeometry();
        var figure = new PathFigure
        {
            StartPoint = new Point(cx, cy - halfExtent),
            IsClosed = true
        };

        figure.Segments.Add(new LineSegment { Point = new Point(cx + halfExtent, cy) });
        figure.Segments.Add(new LineSegment { Point = new Point(cx, cy + halfExtent) });
        figure.Segments.Add(new LineSegment { Point = new Point(cx - halfExtent, cy) });
        geometry.Figures.Add(figure);

        if (transform != null)
            geometry.Transform = transform;

        return geometry;
    }

    private static Point GetRectRayIntersection(Rect bounds, double originX, double originY, double dirX, double dirY)
    {
        const double epsilon = 1e-9;
        double distance = double.PositiveInfinity;

        void TryCandidate(double t)
        {
            if (t > epsilon && t < distance)
                distance = t;
        }

        if (Math.Abs(dirX) > epsilon)
        {
            TryCandidate((bounds.Left - originX) / dirX);
            TryCandidate((bounds.Right - originX) / dirX);
        }

        if (Math.Abs(dirY) > epsilon)
        {
            TryCandidate((bounds.Top - originY) / dirY);
            TryCandidate((bounds.Bottom - originY) / dirY);
        }

        if (double.IsInfinity(distance))
            return new Point(originX, originY);

        var hitX = Math.Clamp(originX + dirX * distance, bounds.Left, bounds.Right);
        var hitY = Math.Clamp(originY + dirY * distance, bounds.Top, bounds.Bottom);
        return new Point(hitX, hitY);
    }

    /// <summary>
    /// 在停止点列表中插值颜色
    /// </summary>
    public static Color InterpolateColor(List<(double offset, Color color)> stops, double t, double globalOpacity)
    {
        if (stops.Count == 0)
            return Color.FromArgb((byte)(globalOpacity * 255), 128, 128, 128);
        if (t <= stops[0].offset)
            return Color.FromArgb((byte)(stops[0].color.A * globalOpacity), stops[0].color.R, stops[0].color.G, stops[0].color.B);
        if (t >= stops[^1].offset)
            return Color.FromArgb((byte)(stops[^1].color.A * globalOpacity), stops[^1].color.R, stops[^1].color.G, stops[^1].color.B);

        for (int i = 0; i < stops.Count - 1; i++)
        {
            if (t >= stops[i].offset && t <= stops[i + 1].offset)
            {
                var range = stops[i + 1].offset - stops[i].offset;
                if (range < 0.0001)
                    return Color.FromArgb((byte)(stops[i].color.A * globalOpacity), stops[i].color.R, stops[i].color.G, stops[i].color.B);
                var localT = (t - stops[i].offset) / range;
                var r = (byte)(stops[i].color.R + (stops[i + 1].color.R - stops[i].color.R) * localT);
                var g = (byte)(stops[i].color.G + (stops[i + 1].color.G - stops[i].color.G) * localT);
                var b = (byte)(stops[i].color.B + (stops[i + 1].color.B - stops[i].color.B) * localT);
                var a = (byte)((stops[i].color.A + (stops[i + 1].color.A - stops[i].color.A) * localT) * globalOpacity);
                return Color.FromArgb(a, r, g, b);
            }
        }
        return Color.FromArgb((byte)(stops[^1].color.A * globalOpacity), stops[^1].color.R, stops[^1].color.G, stops[^1].color.B);
    }

    #endregion
}