using Avalonia.Data.Converters;
using System.Globalization;
using yuyiAvalonia.ImageEdit.Common;

namespace yuyiAvalonia.ImageEdit.Converters;

public class EditToolModeToHitTestConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EditToolMode mode)
        {
            // 文字模式下禁用点击，其他模式启用
            return mode != EditToolMode.Text;
        }
        return true;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
