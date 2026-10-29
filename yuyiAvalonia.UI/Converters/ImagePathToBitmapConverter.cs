using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System.Globalization;

namespace yuyiAvalonia.UI.Converters;

/// <summary>
/// Converts an image path string to a Bitmap for preview display.
/// Returns null if the path is empty or the file does not exist.
/// </summary>
public class ImagePathToBitmapConverter : IMultiValueConverter
{
    public static readonly ImagePathToBitmapConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 0 || values[0] is not string path || string.IsNullOrEmpty(path))
            return null;

        if (!System.IO.File.Exists(path))
            return null;

        try
        {
            return new Bitmap(path);
        }
        catch
        {
            return null;
        }
    }
}
