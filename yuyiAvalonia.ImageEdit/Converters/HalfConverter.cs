using Avalonia.Data.Converters;
using System.Globalization;

namespace yuyiAvalonia.ImageEdit.Converters;

public class HalfConverter : IValueConverter
{
    public static readonly HalfConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            double offset = 0;
            if (parameter is string s && double.TryParse(s, out double param))
            {
                offset = param;
            }
            return (d - offset) / 2;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
