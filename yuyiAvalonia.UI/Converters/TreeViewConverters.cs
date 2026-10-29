using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace yuyiAvalonia.UI.Converters;

public class VisibilityIconConverter : IValueConverter
{
    public static VisibilityIconConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            return isVisible ? "\uE8B4" : "\uE8B5";
        }
        return "\uE8B4";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class VisibilityToColorConverter : IValueConverter
{
    public static VisibilityToColorConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isVisible)
        {
            return isVisible
                ? new SolidColorBrush(Color.Parse("#F8F8F8"))
                : new SolidColorBrush(Color.Parse("#FF6B6B"));
        }
        return new SolidColorBrush(Color.Parse("#F8F8F8"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class ExpandIconConverter : IValueConverter
{
    public static ExpandIconConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isExpanded)
        {
            return isExpanded ? "\uE70D" : "\uE76C";
        }
        return "\uE76C";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToOpacityConverter : IValueConverter
{
    public static BoolToOpacityConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? 1.0 : 0.5;
        }
        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
