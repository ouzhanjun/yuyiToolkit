using Avalonia.Data.Converters;
using yuyiAvalonia.ImageEdit.Common;

namespace yuyiAvalonia.ImageEdit.Converters;

public class ToolModeToTextVisibleConverter : IValueConverter
{
    public static readonly ToolModeToTextVisibleConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is EditToolMode mode)
        {
            return mode == EditToolMode.Text;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
