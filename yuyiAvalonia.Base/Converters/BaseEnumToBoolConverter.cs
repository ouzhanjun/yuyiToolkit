using Avalonia.Data.Converters;
using System.Globalization;

namespace yuyiAvalonia.Base.Converters;

/// <summary>
/// 将枚举值与指定的枚举名称字符串比较，返回布尔值。
/// 用于 RadioButton.IsChecked 和 IsVisible 绑定到枚举属性。
/// </summary>
public class BaseEnumToBoolConverter : IValueConverter
{
    public static readonly BaseEnumToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;

        var enumValue = value.ToString();
        var paramValue = parameter.ToString();

        if (enumValue == null || paramValue == null) return false;

        return enumValue == paramValue || enumValue.EndsWith(paramValue);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && boolValue && parameter != null)
        {
            var paramStr = parameter.ToString();
            if (paramStr != null)
            {
                var enumType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                if (enumType.IsEnum)
                {
                    return Enum.Parse(enumType, paramStr);
                }
            }
        }
        return Avalonia.Data.BindingOperations.DoNothing;
    }
}
