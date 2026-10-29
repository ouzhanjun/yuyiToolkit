
using Avalonia.Data.Converters;
using System.Globalization;


namespace yuyiAvalonia.UI.Converters
{
    public class OpacityToPercentConverter : IValueConverter
    {
        // 单例，直接在 XAML 用
        public static readonly OpacityToPercentConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double opacity)
            {
                // 核心：×100 并显示整数
                return (opacity * 100).ToString("0");
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
