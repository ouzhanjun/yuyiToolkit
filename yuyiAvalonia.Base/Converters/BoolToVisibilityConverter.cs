using Avalonia.Data.Converters; using System.Globalization;

namespace yuyiAvalonia.Base.Converters;

/// <summary>
/// 将布尔值转换为可见性
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public static readonly BoolToVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "Visible" : "Collapsed";
        }
        return "Collapsed";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string visibilityStr)
        {
            return visibilityStr == "Visible";
        }
        return false;
    }
}
