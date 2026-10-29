using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace yuyiAvalonia.ImageEdit.Converters;

public class BoolToScrollBarVisibilityConverter : IValueConverter
{
    public static readonly BoolToScrollBarVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
        }
        return ScrollBarVisibility.Auto;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
