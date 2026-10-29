using Avalonia.Data.Converters;
using Avalonia.Media;
using yuyiAvalonia.Software.Models;

namespace yuyiAvalonia.Software.ViewModels;

/// <summary>
/// 软件状态到颜色的转换器
/// </summary>
public class StatusToColorConverter : IValueConverter
{
    public static readonly StatusToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is SoftwareStatus status)
        {
            return status switch
            {
                SoftwareStatus.Downloaded => Color.FromArgb(255, 0x52, 0xC4, 0x1A),
                SoftwareStatus.Downloading => Color.FromArgb(255, 0x18, 0x90, 0xFF),
                SoftwareStatus.Error => Color.FromArgb(255, 0xFF, 0x4D, 0x4F),
                SoftwareStatus.UpdateAvailable => Color.FromArgb(255, 0xFA, 0xAD, 0x14),
                _ => Color.FromArgb(255, 0x8C, 0x8C, 0x8C)
            };
        }
        return Color.FromArgb(255, 0x8C, 0x8C, 0x8C);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 整数到布尔值转换器（用于列表项数量判断）
/// </summary>
public class IntToBoolConverterForVisibility : IValueConverter
{
    public static readonly IntToBoolConverterForVisibility Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue > 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
