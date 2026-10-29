using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Concurrent;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Renders;

/// <summary>
/// 图形图标渲染器 - 将图形 Shape 绘制到位图并缓存
/// </summary>
public static class ShapeIconRenderer
{
    private static readonly ConcurrentDictionary<string, Bitmap?> _cache = new();

    private const int IconSize = 32;
    private static readonly Vector DPI = new(96, 96);

    /// <summary>
    /// 获取指定组件定义的图形图标位图（带缓存）
    /// </summary>
    public static Bitmap? GetShapeIcon(ComponentDefinition definition)
    {
        if (definition?.Defaults == null)
            return null;

        return _cache.GetOrAdd(definition.Icon, _ => RenderShapeIcon(definition));
    }

    /// <summary>
    /// 根据组件类型获取图形图标位图（使用默认样式）
    /// </summary>
    public static Bitmap? GetShapeIcon(ComponentType type)
    {
        var key = GetIconKeyByType(type);
        if (string.IsNullOrEmpty(key))
            return null;

        return _cache.GetOrAdd(key, _ =>
        {
            var definition = CreateDefaultDefinition(type);
            return definition != null ? RenderShapeIcon(definition) : null;
        });
    }

    private static string GetIconKeyByType(ComponentType type) => type switch
    {
        ComponentType.Triangle => "shape_triangle",
        ComponentType.Rectangle => "shape_rect",
        ComponentType.RoundedRectangle => "shape_roundedrect",
        ComponentType.Ellipse => "shape_circle",
        ComponentType.Parallelogram => "shape_parallelogram",
        ComponentType.Star => "shape_star",
        ComponentType.RoundedPolygon => "shape_roundedpolygon",
        ComponentType.Sector => "shape_sector",
        _ => string.Empty
    };

    private static ComponentDefinition? CreateDefaultDefinition(ComponentType type)
    {
        return type switch
        {
            ComponentType.Triangle => new ComponentDefinition
            {
                Icon = "shape_triangle", Type = ComponentType.Triangle,
                Defaults = new ComponentDefaults { Width = 100, Height = 100, BackgroundColor = "#FF6B6B", StrokeColor = "#333333", StrokeThickness = 2 }
            },
            ComponentType.Rectangle => new ComponentDefinition
            {
                Icon = "shape_rect", Type = ComponentType.Rectangle,
                Defaults = new ComponentDefaults { Width = 100, Height = 60, BackgroundColor = "#4ECDC4", StrokeColor = "#333333", StrokeThickness = 2 }
            },
            ComponentType.RoundedRectangle => new ComponentDefinition
            {
                Icon = "shape_roundedrect", Type = ComponentType.RoundedRectangle,
                Defaults = new ComponentDefaults { Width = 100, Height = 60, BackgroundColor = "#4ECDC4", StrokeColor = "#333333", StrokeThickness = 2 }
            },
            ComponentType.Ellipse => new ComponentDefinition
            {
                Icon = "shape_circle", Type = ComponentType.Ellipse,
                Defaults = new ComponentDefaults { Width = 100, Height = 100, BackgroundColor = "#45B7D1", StrokeColor = "#333333", StrokeThickness = 2 }
            },
            ComponentType.Parallelogram => new ComponentDefinition
            {
                Icon = "shape_parallelogram", Type = ComponentType.Parallelogram,
                Defaults = new ComponentDefaults { Width = 120, Height = 80, BackgroundColor = "#96CEB4", StrokeColor = "#333333", StrokeThickness = 2 }
            },
            ComponentType.Star => new ComponentDefinition
            {
                Icon = "shape_star", Type = ComponentType.Star,
                Defaults = new ComponentDefaults { Width = 120, Height = 120, BackgroundColor = "#9C27B0", StrokeColor = "#333333", StrokeThickness = 1 }
            },
            ComponentType.RoundedPolygon => new ComponentDefinition
            {
                Icon = "shape_roundedpolygon", Type = ComponentType.RoundedPolygon,
                Defaults = new ComponentDefaults { Width = 120, Height = 100, BackgroundColor = "#FF9F43", StrokeColor = "#333333", StrokeThickness = 1 }
            },
            ComponentType.Sector => new ComponentDefinition
            {
                Icon = "shape_sector", Type = ComponentType.Sector,
                Defaults = new ComponentDefaults { Width = 120, Height = 120, BackgroundColor = "#7B61FF", StrokeColor = "#333333", StrokeThickness = 1 }
            },
            _ => null
        };
    }

    private static Bitmap? RenderShapeIcon(ComponentDefinition definition)
    {
        var defaults = definition.Defaults;
        if (defaults == null)
            return null;

        var pixelSize = new PixelSize(IconSize, IconSize);
        var bitmap = new RenderTargetBitmap(pixelSize, DPI);

        var fill = ParseColorBrush(defaults.BackgroundColor);
        var stroke = ParseColorBrush(defaults.StrokeColor);
        var strokeThickness = defaults.StrokeThickness;
        var geometry = CreateGeometry(definition.Type, defaults.Width, defaults.Height);

        if (geometry == null)
            return null;

        using var ctx = bitmap.CreateDrawingContext();

        // 计算缩放比例，让图形完整放入 32x32，保留 2px 边距
        double effectiveSize = IconSize - 4;
        double scale = Math.Min(effectiveSize / defaults.Width, effectiveSize / defaults.Height);
        double offsetX = (IconSize - defaults.Width * scale) / 2;
        double offsetY = (IconSize - defaults.Height * scale) / 2;

        // 对 geometry 应用缩放和平移
        geometry.Transform = new MatrixTransform(Matrix.CreateTranslation(offsetX, offsetY) * Matrix.CreateScale(scale, scale));
        ctx.DrawGeometry(fill, new Pen(stroke, strokeThickness), geometry);
        return bitmap;
    }

    private static Geometry? CreateGeometry(ComponentType type, double width, double height)
    {
        return type switch
        {
            ComponentType.Triangle => CreateTriangleGeometry(width, height),
            ComponentType.Rectangle => new RectangleGeometry(new Rect(0, 0, width, height)),
            ComponentType.RoundedRectangle => CreateRoundedRectangleGeometry(width, height),
            ComponentType.Ellipse => new EllipseGeometry(new Rect(0, 0, width, height)),
            ComponentType.Parallelogram => CreateParallelogramGeometry(width, height),
            ComponentType.Star => CreateStarGeometry(width, height),
            ComponentType.RoundedPolygon => CreateRoundedPolygonGeometry(width, height),
            ComponentType.Sector => CreateSectorGeometry(width, height),

            _ => null
        };
    }

    /// <summary>
    /// 圆角多边形图标几何：默认 3 边形（圆角三角形），均匀分布 n 个顶点，
    /// 每个顶点统一圆角 R = min(w,h) * 0.18。复用 RoundedRectangleGeometry 的算法。
    /// </summary>
    private static PathGeometry CreateRoundedPolygonGeometry(double width, double height)
    {
        const int n = 3;
        double cx = width / 2.0, cy = height / 2.0;
        double r = Math.Min(width, height) / 2.0;
        double start = -Math.PI / 2.0; // 首顶点朝上
        var pts = new Avalonia.Point[n];
        for (int i = 0; i < n; i++)
        {
            double a = start + i * 2.0 * Math.PI / n;
            pts[i] = new Avalonia.Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }
        double cornerR = 1;
        //double cornerR = Math.Min(width, height) * 0.18;
        var radii = new[] { cornerR, cornerR, cornerR };
        return RoundedPathBuilder.Build(pts, radii).ToAvaloniaGeometry();
    }

    private static PathGeometry CreateTriangleGeometry(double width, double height)
    {
        var figure = new PathFigure
        {
            StartPoint = new Point(width / 2, 0),
            IsClosed = true
        };
        figure.Segments.Add(new LineSegment { Point = new Point(0, height) });
        figure.Segments.Add(new LineSegment { Point = new Point(width, height) });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    private static PathGeometry CreateParallelogramGeometry(double width, double height)
    {
        double skewOffset = width * 0.2;

        var figure = new PathFigure
        {
            StartPoint = new Point(skewOffset, 0),
            IsClosed = true
        };
        figure.Segments.Add(new LineSegment { Point = new Point(width, 0) });
        figure.Segments.Add(new LineSegment { Point = new Point(width - skewOffset, height) });
        figure.Segments.Add(new LineSegment { Point = new Point(0, height) });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// 六星形图标几何（尖角版本；图标里不画圆角更清晰）。
    /// 外尖/内凹半径用 RoundedStarElement 风格：outerR = 0.46 * min(w,h)，innerR = 0.42 * outerR。
    /// </summary>
    private static PathGeometry CreateStarGeometry(double width, double height)
    {
        const int pointCount = 5;             // 外尖数
        const int n = pointCount * 2;         // 顶点总数
        double cx = width / 2.0;
        double cy = height / 2.0;
        double outerR = 0.5 * Math.Min(width, height);
        double innerR = outerR * 0.42;
        double start = -Math.PI / 2;          // 起点在正上方

        var pts = new Point[n];
        for (int i = 0; i < n; i++)
        {
            double a = start + i * Math.PI / pointCount;
            double r = (i % 2 == 0) ? outerR : innerR;
            pts[i] = new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }

        var figure = new PathFigure
        {
            StartPoint = pts[0],
            IsClosed = true
        };
        for (int i = 1; i < n; i++)
            figure.Segments.Add(new LineSegment { Point = pts[i] });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// 圆角矩形图标几何：4 个角统一 R = min(w,h) * 0.18，复用 RoundedRectangleGeometry 算法。
    /// </summary>
    private static PathGeometry CreateRoundedRectangleGeometry(double width, double height)
    {
        double r = Math.Min(width, height) * 0.18;
        var pts = new[]
        {
            new Avalonia.Point(0, 0),
            new Avalonia.Point(width, 0),
            new Avalonia.Point(width, height),
            new Avalonia.Point(0, height),
        };
        var radii = new[] { r, r, r, r };
        return RoundedPathBuilder.Build(pts, radii).ToAvaloniaGeometry();
    }

    /// <summary>
    /// 椭圆 / 弧 / 扇形 图标几何：默认实心椭圆，预览 0° 起 360° 扫描。
    /// 复用 EllipseArcPathBuilder。
    /// </summary>
    private static PathGeometry CreateSectorGeometry(double width, double height)
    {
        return EllipseArcPathBuilder.Build(0, 0, width, height, 0, 360, 0).ToAvaloniaGeometry();
    }

    private static IBrush ParseColorBrush(string? colorStr)
    {
        if (string.IsNullOrEmpty(colorStr))
            return Brushes.Transparent;

        try
        {
            var color = Avalonia.Media.Color.Parse(colorStr);
            return new SolidColorBrush(color);
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public static void ClearCache()
    {
        foreach (var bmp in _cache.Values)
            bmp?.Dispose();
        _cache.Clear();
    }
}
