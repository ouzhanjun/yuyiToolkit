using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using System.Globalization;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;

namespace yuyiAvalonia.UI.Converters;

public class AlignmentToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return 0;

        var alignment = value.ToString();
        return alignment switch
        {
            "Left" or "Top" => 0,
            "Center" => 1,
            "Right" or "Bottom" => 2,
            _ => 0
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            var paramStr = parameter?.ToString();
            if (paramStr == "Horizontal")
            {
                return index switch
                {
                    0 => HorizontalAlignment.Left,
                    1 => HorizontalAlignment.Center,
                    2 => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Left
                };
            }
            else // Vertical
            {
                return index switch
                {
                    0 => VerticalAlignment.Top,
                    1 => VerticalAlignment.Center,
                    2 => VerticalAlignment.Bottom,
                    _ => VerticalAlignment.Top
                };
            }
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}

public static class Converters
{
    public static StringToBrushConverter StringToBrush { get; } = new();
    public static StringToColorConverter StringToColor { get; } = new();
    public static EnumToBoolConverter EnumToBool { get; } = new();
    public static IsTextElementConverter IsTextElement { get; } = new();
    public static IsCardElementConverter IsCardElement { get; } = new();
    public static IsButtonElementConverter IsButtonElement { get; } = new();
    public static IsImageElementConverter IsImageElement { get; } = new();
    public static IsStackPanelElementConverter IsStackPanelElement { get; } = new();
    public static IsPanelElementConverter IsPanelElement { get; } = new();
    public static IsGridElementConverter IsGridElement { get; } = new();
    public static IsCanvasContainerElementConverter IsCanvasContainerElement { get; } = new();
    public static IsShapeElementConverter IsShapeElement { get; } = new();
    public static IsRoundedStarElementConverter IsRoundedStarElement { get; } = new();
    public static IsRoundedPolygonElementConverter IsRoundedPolygonElement { get; } = new();
    public static IsRoundedRectangleElementConverter IsRoundedRectangleElement { get; } = new();
    public static IsRoundedRectangleStackPanelElementConverter IsRoundedRectangleStackPanelElement { get; } = new();
    public static IsRoundedRectangleGridElementConverter IsRoundedRectangleGridElement { get; } = new();
    public static IsRoundedRectangleCanvasElementConverter IsRoundedRectangleCanvasElement { get; } = new();
    public static IsSectorElementConverter IsSectorElement { get; } = new();
    public static CategoryBackgroundColorConverter CategoryBackground { get; } = new();
    public static ComponentTypeBackgroundConverter ComponentTypeBackground { get; } = new();
    public static ComponentTypeIconConverter ComponentTypeIcon { get; } = new();
    public static ShapeIconConverter ShapeIcon { get; } = new();
    public static IsShapeIconConverter IsShapeIcon { get; } = new();
    public static IsNotShapeIconConverter IsNotShapeIcon { get; } = new();
    public static BorderColorConverter BorderColor { get; } = new();
    public static CategoryForeColorConverter CategoryForeColor { get; } = new();
    public static GradientToBackgroundConverter GradientToBackground { get; } = new();
    public static AlignmentToIndexConverter AlignmentToIndex { get; } = new();
    public static VisibilityIconConverter VisibilityIcon { get; } = new();
    public static VisibilityToColorConverter VisibilityToColor { get; } = new();
    public static ExpandIconConverter ExpandIcon { get; } = new();
    public static FillTypeIndexToVisibilityConverter FillTypeIndexToVisibility { get; } = new();
    public static StrokeStyleIsDashedConverter StrokeStyleIsDashed { get; } = new();
}

public class InverseBoolConverter : IValueConverter
{
    public static readonly InverseBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return !b;
        }
        return false;
    }
}

public class StringToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string colorStr && !string.IsNullOrEmpty(colorStr))
        {
            try { return new SolidColorBrush(Color.Parse(colorStr)); }
            catch { }
        }
        return Brushes.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class StringToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string colorStr && !string.IsNullOrEmpty(colorStr))
        {
            try { return Color.Parse(colorStr); }
            catch { return Colors.White; }
        }
        return Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class EnumToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;

        var enumValue = value.ToString();
        var paramValue = parameter.ToString();

        if (enumValue == null || paramValue == null) return false;

        return enumValue.EndsWith(paramValue) || enumValue == paramValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            var paramStr = parameter.ToString();
            if (paramStr != null)
            {
                // FillType 枚举 - 优先处理，因为 "None" 可能匹配多个枚举
                if (paramStr == "None" || paramStr == "Solid" || paramStr == "Gradient" || paramStr == "Image")
                {
                    return Enum.Parse(typeof(Models.FillType), paramStr);
                }
                // AlignmentX 枚举
                else if (paramStr == "Left" || paramStr == "Center" || paramStr == "Right")
                {
                    // 判断是 AlignmentX 还是 HorizontalAlignment
                    var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    if (enumType == typeof(Avalonia.Media.AlignmentX))
                    {
                        return Enum.Parse(typeof(Avalonia.Media.AlignmentX), paramStr);
                    }
                    return Enum.Parse(typeof(Avalonia.Layout.HorizontalAlignment), paramStr);
                }
                // AlignmentY / VerticalAlignment 枚举
                else if (paramStr == "Top" || paramStr == "Bottom")
                {
                    var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    if (enumType == typeof(Avalonia.Media.AlignmentY))
                    {
                        return Enum.Parse(typeof(Avalonia.Media.AlignmentY), paramStr);
                    }
                    return Enum.Parse(typeof(Avalonia.Layout.VerticalAlignment), paramStr);
                }
                // Stretch 枚举
                else if (paramStr == "Fill" || paramStr == "Uniform" || paramStr == "UniformToFill" || paramStr == "Stretch")
                {
                    return Enum.Parse(typeof(Avalonia.Media.Stretch), paramStr);
                }
                // TileMode 枚举
                else if (paramStr == "FlipX" || paramStr == "FlipY" || paramStr == "FlipXY")
                {
                    return Enum.Parse(typeof(Avalonia.Media.TileMode), paramStr);
                }
                else
                {
                    // 尝试根据 value 的类型推断枚举类型
                    var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                    if (enumType.IsEnum)
                    {
                        return Enum.Parse(enumType, paramStr);
                    }
                }
            }
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}

public class IsTextElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is TextElement && value is not ButtonElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsCardElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is CardElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsButtonElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is ButtonElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsImageElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is ImageElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsStackPanelElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is StackPanelElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsPanelElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 匹配 PanelElement 但排除 StackPanelElement 和 GridElement（它们有自己的属性面板）
        return value is PanelElement ;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsGridElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is GridElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsCanvasContainerElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is CanvasPanelElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsShapeElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is ShapeElement && value is not RoundedPolygonElement && value is not RoundedStarElement && value is not RoundedRectangleElement && value is not RoundEllipseElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsRoundedStarElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundedStarElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsRoundedRectangleElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundedRectangleElement
           && value is not RoundedRectangleStackPanelElement
           && value is not RoundedRectangleGridElement
           && value is not RoundedRectangleCanvasElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 仅匹配 <see cref="RoundedRectangleStackPanelElement"/> 精确类型（排除父类）。
/// 顺序在 <see cref="IsRoundedRectangleElementConverter"/> 之前挂载，
/// 避免子类被父类面板截走。
/// </summary>
public class IsRoundedRectangleStackPanelElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundedRectangleStackPanelElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsRoundedRectangleGridElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundedRectangleGridElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 仅匹配 <see cref="RoundedRectangleCanvasElement"/> 精确类型（排除父类）。
/// </summary>
public class IsRoundedRectangleCanvasElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundedRectangleCanvasElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsRoundedPolygonElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundedPolygonElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class IsSectorElementConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is RoundEllipseElement;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

//public class IsFlowShapeElementConverter : IValueConverter
//{
//    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
//        => value is ShapeElement;

//    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
//        => Avalonia.Data.BindingOperations.DoNothing;
//}

public class CategoryBackgroundColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#1890FF"))
                : new SolidColorBrush(Color.Parse("#2A2A2A"));
        }
        return new SolidColorBrush(Color.Parse("#2A2A2A"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class ComponentTypeBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ComponentType type)
        {
            return type switch
            {
                ComponentType.Text => new SolidColorBrush(Color.Parse("#1890FF")),
                ComponentType.Card => new SolidColorBrush(Color.Parse("#52C41A")),
                ComponentType.Button => new SolidColorBrush(Color.Parse("#FA8C16")),
                ComponentType.Panel => new SolidColorBrush(Color.Parse("#722ED1")),
                ComponentType.VerticalStackPanel => new SolidColorBrush(Color.Parse("#13C2C2")),
                ComponentType.HorizontalStackPanel => new SolidColorBrush(Color.Parse("#13C2C2")),
                ComponentType.Grid => new SolidColorBrush(Color.Parse("#1677FF")),
                ComponentType.Canvas => new SolidColorBrush(Color.Parse("#EB2F96")),
                ComponentType.Image => new SolidColorBrush(Color.Parse("#52C41A")),
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class ComponentTypeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ComponentType type)
        {
            return type switch
            {
                ComponentType.Text => Icon.Text,
                ComponentType.Card => Icon.Panel,
                ComponentType.Button => Icon.Button,
                ComponentType.Panel => Icon.Panel,
                ComponentType.VerticalStackPanel => Icon.VStack,
                ComponentType.HorizontalStackPanel => Icon.HStack,
                ComponentType.Grid => Icon.Grid,
                ComponentType.Canvas => Icon.Canvas,
                ComponentType.Image => Icon.Image,
                ComponentType.Triangle => Icon.Triangle,
                ComponentType.Rectangle => Icon.BasicRectangle,
                ComponentType.Ellipse => Icon.Ellipse,
                ComponentType.Parallelogram => Icon.Parallelogram,
                // ComponentType.Star 没有原生 Icon 资源，工具箱通过 ShapeIconConverter 用位图渲染。
                ComponentType.Star => null,
                _ => null
            };
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 图形图标转换器 - 将 shape_* 图标字符串转换为预渲染的位图
/// </summary>
public class ShapeIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string iconStr || string.IsNullOrEmpty(iconStr))
            return null;

        return iconStr switch
        {
            "shape_triangle" => ShapeIconRenderer.GetShapeIcon(ComponentType.Triangle),
            "shape_rect" => ShapeIconRenderer.GetShapeIcon(ComponentType.Rectangle),
            "shape_roundedrect" => ShapeIconRenderer.GetShapeIcon(ComponentType.RoundedRectangle),
            "shape_circle" => ShapeIconRenderer.GetShapeIcon(ComponentType.Ellipse),
            "shape_parallelogram" => ShapeIconRenderer.GetShapeIcon(ComponentType.Parallelogram),
            "shape_sixstar" => ShapeIconRenderer.GetShapeIcon(ComponentType.Star),
            "shape_roundedpolygon" => ShapeIconRenderer.GetShapeIcon(ComponentType.RoundedPolygon),
            "shape_sector" => ShapeIconRenderer.GetShapeIcon(ComponentType.Sector),
            "shape_triangle_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.Triangle),
            "shape_rect_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.Rectangle),
            "shape_roundedrect_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.RoundedRectangle),
            "shape_circle_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.Ellipse),
            "shape_parallelogram_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.Parallelogram),
            "shape_sixstar_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.Star),
            "shape_roundedpolygon_container" => ShapeIconRenderer.GetShapeIcon(ComponentType.RoundedPolygon),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 判断是否为图形组件图标
/// </summary>
public class IsShapeIconConverter : IValueConverter
{
    private static readonly string[] ShapeIcons = {
        "shape_triangle", "shape_rect", "shape_roundedrect", "shape_circle", "shape_parallelogram", "shape_sixstar", "shape_roundedpolygon", "shape_sector",
        "shape_triangle_container", "shape_rect_container", "shape_roundedrect_container", "shape_circle_container", "shape_parallelogram_container", "shape_sixstar_container", "shape_roundedpolygon_container"
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconStr)
            return ShapeIcons.Contains(iconStr);
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 判断是否为非图形组件图标（用于控制 PathIcon 可见性）
/// </summary>
public class IsNotShapeIconConverter : IValueConverter
{
    private static readonly string[] ShapeIcons = {
        "shape_triangle", "shape_rect", "shape_roundedrect", "shape_circle", "shape_parallelogram", "shape_sixstar", "shape_roundedpolygon", "shape_sector",
        "shape_triangle_container", "shape_rect_container", "shape_roundedrect_container", "shape_circle_container", "shape_parallelogram_container", "shape_sixstar_container", "shape_roundedpolygon_container"
    };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconStr)
            return !ShapeIcons.Contains(iconStr);
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class BorderColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#1890FF"))
                : Brushes.Transparent;
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class CategoryForeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? Brushes.White : new SolidColorBrush(Color.Parse("#AAAAAA"));
        }
        return new SolidColorBrush(Color.Parse("#AAAAAA"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 当启用渐变背景时返回透明，否则返回指定的纯色背景
/// </summary>
public class GradientToBackgroundConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is bool isGradient && values[1] is string colorStr)
        {
            if (isGradient)
            {
                return Brushes.Transparent;
            }
            try
            {
                return new SolidColorBrush(Color.Parse(colorStr));
            }
            catch
            {
                return Brushes.White;
            }
        }
        return Brushes.Transparent;
    }
}

public static class ColorExtensions
{
    public static Color ToColor(this string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr))
            return Colors.White;

        try
        {
            return Color.Parse(colorStr);
        }
        catch
        {
            return Colors.White;
        }
    }
}

/// <summary>
/// SVG图标转换器 - 根据Icon名称加载SVG资源
/// </summary>
public class SvgIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 检查字符串是否为空的转换器 - 用于控制元素可见性
/// </summary>
public class StringNullOrEmptyToBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 当字符串为null或空时返回true（显示元素）
        // 当字符串不为空时返回false（隐藏元素）
        if (value is string str)
            return string.IsNullOrEmpty(str);
        return value == null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 获取字符串首字母转换器
/// </summary>
public class FirstLetterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && !string.IsNullOrEmpty(str))
        {
            return str[0].ToString().ToUpper();
        }
        return "?";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 索引到布尔值转换器（用于填充类型）
/// </summary>
public class FillTypeIndexToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            if (parameter is string strParam && int.TryParse(strParam, out int target))
            {
                return index == target;
            }
            if (parameter is int intParam)
            {
                return index == intParam;
            }
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}

/// <summary>
/// 边框样式是否为虚线的转换器
/// </summary>
public class StrokeStyleIsDashedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index >= 1 && index <= 4;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}

/// <summary>
/// 选中状态到背景色的转换器
/// </summary>
public class SelectedBackgroundConverter : IValueConverter
{
    public static readonly SelectedBackgroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#1890FF"))
                : Brushes.Transparent;
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

/// <summary>
/// 选中状态到前景色的转换器
/// </summary>
public class SelectedForegroundConverter : IValueConverter
{
    public static readonly SelectedForegroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? Brushes.White
                : new SolidColorBrush(Color.Parse("#CCCCCC"));
        }
        return new SolidColorBrush(Color.Parse("#CCCCCC"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Avalonia.Data.BindingOperations.DoNothing;
}

public class StringEqualsConverter : IValueConverter
{
    public static readonly StringEqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            return parameter.ToString();
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
