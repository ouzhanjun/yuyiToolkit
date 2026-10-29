using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;

namespace yuyiAvalonia.UI.Converters;

#region 分类背景转换器

public class CategoryBackgroundConverter : IValueConverter
{
    public static readonly CategoryBackgroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#3B82F6"))
                : new SolidColorBrush(Color.Parse("#F3F4F6"));
        }
        return new SolidColorBrush(Color.Parse("#F3F4F6"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region 分类前景(文字)转换器

public class CategoryForegroundConverter : IValueConverter
{
    public static readonly CategoryForegroundConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Color.Parse("#6B7280"));
        }
        return new SolidColorBrush(Color.Parse("#6B7280"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region 分类边框转换器

public class CategoryBorderConverter : IValueConverter
{
    public static readonly CategoryBorderConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected
                ? new SolidColorBrush(Color.Parse("#3B82F6"))
                : new SolidColorBrush(Colors.Transparent);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region 分类字体粗细转换器

public class CategoryFontWeightConverter : IValueConverter
{
    public static readonly CategoryFontWeightConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isSelected)
        {
            return isSelected ? FontWeight.SemiBold : FontWeight.Normal;
        }
        return FontWeight.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion

#region 计数可见性转换器

public class CountToVisibilityConverter : IValueConverter
{
    public static readonly CountToVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count > 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

#endregion

