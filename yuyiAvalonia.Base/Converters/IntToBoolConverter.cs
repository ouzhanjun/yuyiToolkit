using Avalonia.Data.Converters;
using System.Globalization;

namespace yuyiAvalonia.Base.Converters;

/// <summary>
/// 将整数与指定值比较，返回布尔值（用于 RadioButton 和 IsVisible 绑定）
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public static readonly IntToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            if (parameter is string strParam && int.TryParse(strParam, out int targetValue))
            {
                return intValue == targetValue;
            }
            if (parameter is int intParam)
            {
                return intValue == intParam;
            }
        }
        // IsVisible 需要 bool，参数不匹配或类型不对时也要返回 false
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue)
        {
            if (parameter is string strParam && int.TryParse(strParam, out int targetValue))
            {
                return targetValue;
            }
            if (parameter is int intParam)
            {
                return intParam;
            }
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
