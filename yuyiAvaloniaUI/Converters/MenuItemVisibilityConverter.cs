using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace yuyiAvaloniaUI.Converters;

public class MenuItemVisibilityConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return false;

        var isSeparator = values[0] is bool b1 && b1;
        var isComplex = values[1] is bool b2 && b2;

        // 普通按钮：非分隔线且非复杂控件
        if (parameter?.ToString() == "Button")
            return !isSeparator && !isComplex;

        // 分隔线
        if (parameter?.ToString() == "Separator")
            return isSeparator;

        // 复杂控件
        if (parameter?.ToString() == "Complex")
            return isComplex;

        return false;
    }
}
