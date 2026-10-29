using Avalonia.Media;
using System.Text.RegularExpressions;

namespace yuyiAvalonia.UI.ElementControls
{
    /// <summary>
    /// 属性值验证帮助类
    /// </summary>
    public static class PropertyValidator
    {
        // 颜色值正则表达式 (#RGB, #RRGGBB, #AARRGGBB, #RRGGBBAA)
        private static readonly Regex ColorRegex = new(@"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8}|[0-9A-Fa-f]{4})$", RegexOptions.Compiled);

        /// <summary>
        /// 验证并修正颜色值
        /// </summary>
        /// <param name="color">颜色字符串</param>
        /// <param name="fallbackColor">验证失败时的默认值</param>
        /// <returns>有效的颜色字符串</returns>
        public static string ValidateColor(string? color, string fallbackColor = "#FFFFFF")
        {
            if (string.IsNullOrWhiteSpace(color))
                return fallbackColor;

            color = color.Trim();

            // 如果已经是有效格式，直接返回
            if (IsValidColor(color))
                return color;

            // 尝试添加#前缀
            if (!color.StartsWith('#') && IsValidColor('#' + color))
                return '#' + color;

            return fallbackColor;
        }

        /// <summary>
        /// 检查颜色值是否有效
        /// </summary>
        public static bool IsValidColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return false;

            return ColorRegex.IsMatch(color.Trim());
        }

        /// <summary>
        /// 尝试解析颜色值
        /// </summary>
        /// <param name="color">颜色字符串</param>
        /// <param name="result">解析结果</param>
        /// <param name="fallback">默认值</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseColor(string? color, out Color result, Color fallback = default)
        {
            result = fallback;
            if (string.IsNullOrWhiteSpace(color))
                return false;

            try
            {
                var validatedColor = ValidateColor(color);
                result = Color.Parse(validatedColor);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 验证并限制数值在指定范围内
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// 验证尺寸值（宽/高），最小为1
        /// </summary>
        public static double ValidateSize(double size, double min = 1, double max = 10000)
        {
            if (double.IsNaN(size) || double.IsInfinity(size))
                return min;
            return Clamp(size, min, max);
        }

        /// <summary>
        /// 验证间距值（内边距、外边距、圆角等），最小为0
        /// </summary>
        public static double ValidateSpacing(double spacing, double min = 0, double max = 500)
        {
            if (double.IsNaN(spacing) || double.IsInfinity(spacing))
                return min;
            return Clamp(spacing, min, max);
        }

        /// <summary>
        /// 验证透明度值（0-1之间）
        /// </summary>
        public static double ValidateOpacity(double opacity)
        {
            return Clamp(opacity, 0, 1);
        }

        /// <summary>
        /// 验证字体大小，最小为1
        /// </summary>
        public static double ValidateFontSize(double fontSize, double min = 1, double max = 200)
        {
            if (double.IsNaN(fontSize) || double.IsInfinity(fontSize))
                return 14;
            return Clamp(fontSize, min, max);
        }

        /// <summary>
        /// 安全创建颜色画刷
        /// </summary>
        public static IBrush CreateColorBrush(string? color, IBrush? fallbackBrush = null)
        {
            fallbackBrush ??= Brushes.Transparent;

            if (TryParseColor(color, out var parsedColor))
            {
                return new SolidColorBrush(parsedColor);
            }

            return fallbackBrush;
        }
    }
}
