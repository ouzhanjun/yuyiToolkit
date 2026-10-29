using Avalonia.Collections;
using Avalonia.Data.Converters;
using System.Globalization;

namespace yuyiAvalonia.UI.Converters
{

    public class StrokeStyleDashArrayConverter : IValueConverter
    {
        public static readonly StrokeStyleDashArrayConverter Instance = new();

        private static readonly string[] DashArrays = { "", "4,2", "1,3", "6,2,1,2", "6,2,1,2,1,2" };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string dashStr && !string.IsNullOrEmpty(dashStr))
            {
                var parts = dashStr.Split(',');
                var doubles = new AvaloniaList<double>();
                foreach (var p in parts)
                {
                    if (double.TryParse(p.Trim(), out double d))
                        doubles.Add(d);
                }
                return doubles;
            }
            return new AvaloniaList<double>();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Avalonia.Data.BindingOperations.DoNothing;
    }

    public class StrokeStyleIndexConverter : IValueConverter
    {
        public static readonly StrokeStyleIndexConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                if (parameter is string strParam && int.TryParse(strParam, out int target))
                {
                    return index == target;
                }
                if (parameter is int intParam)
                {
                    return index == intParam;
                }
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                if (parameter is string strParam && int.TryParse(strParam, out int target))
                {
                    return target;
                }
                if (parameter is int intParam)
                {
                    return intParam;
                }
            }
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

    public class StrokeFillIndexConverter : IValueConverter
    {
        public static readonly StrokeFillIndexConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                int target = -1;
                if (parameter is string strParam && int.TryParse(strParam, out target))
                {
                    return index == target;
                }
                if (parameter is int intParam)
                {
                    return index == intParam;
                }
            }

            // IsVisible 需要 bool，参数不匹配或类型不对时也要返回 false，而不是 DoNothing
            if (targetType == typeof(bool) || targetType == typeof(bool?))
                return false;

            return Avalonia.Data.BindingOperations.DoNothing;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked)
            {
                if (parameter is string strParam && int.TryParse(strParam, out int target))
                {
                    return target;
                }
                if (parameter is int intParam)
                {
                    return intParam;
                }
            }
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

}
