using Avalonia.Data.Converters;
using Avalonia.Media;

namespace yuyiAvalonia.ImageEdit.Converters;

public class BoolToFontWeightConverter : IValueConverter
{
    public static readonly BoolToFontWeightConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isBold && isBold)
        {
            return FontWeight.Bold;
        }
        return FontWeight.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToFontStyleConverter : IValueConverter
{
    public static readonly BoolToFontStyleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is bool isItalic && isItalic)
        {
            return FontStyle.Italic;
        }
        return FontStyle.Normal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
