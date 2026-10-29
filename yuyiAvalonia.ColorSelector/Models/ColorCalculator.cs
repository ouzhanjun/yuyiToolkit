using Avalonia.Media;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 颜色计算工具类
/// 提供从基础色自动计算状态颜色的算法
/// </summary>
public static class ColorCalculator
{
    /// <summary>
    /// 将 hex 颜色转为 Color
    /// </summary>
    public static Color ParseColor(string hex)
    {
        try
        {
            if (hex.StartsWith("rgba(") || hex.StartsWith("rgb("))
            {
                return Colors.Gray;
            }
            if (hex.StartsWith("#"))
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    var r = Convert.ToByte(hex.Substring(0, 2), 16);
                    var g = Convert.ToByte(hex.Substring(2, 2), 16);
                    var b = Convert.ToByte(hex.Substring(4, 2), 16);
                    return Color.FromRgb(r, g, b);
                }
                else if (hex.Length == 8)
                {
                    var a = Convert.ToByte(hex.Substring(0, 2), 16);
                    var r = Convert.ToByte(hex.Substring(2, 2), 16);
                    var g = Convert.ToByte(hex.Substring(4, 2), 16);
                    var b = Convert.ToByte(hex.Substring(6, 2), 16);
                    return Color.FromArgb(a, r, g, b);
                }
            }
            return Colors.Gray;
        }
        catch
        {
            return Colors.Gray;
        }
    }

    /// <summary>
    /// 将 Color 转为 hex 字符串
    /// </summary>
    public static string ToHex(Color color, bool includeAlpha = false)
    {
        if (includeAlpha)
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    /// <summary>
    /// 使颜色变亮
    /// </summary>
    public static string Lighten(string hex, double amount = 0.2)
    {
        var c = ParseColor(hex);
        var r = Math.Min(255, (int)(c.R + (255 - c.R) * amount));
        var g = Math.Min(255, (int)(c.G + (255 - c.G) * amount));
        var b = Math.Min(255, (int)(c.B + (255 - c.B) * amount));
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 使颜色变暗
    /// </summary>
    public static string Darken(string hex, double amount = 0.15)
    {
        var c = ParseColor(hex);
        var r = Math.Max(0, (int)(c.R * (1 - amount)));
        var g = Math.Max(0, (int)(c.G * (1 - amount)));
        var b = Math.Max(0, (int)(c.B * (1 - amount)));
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 降低颜色饱和度
    /// </summary>
    public static string Desaturate(string hex, double amount = 0.3)
    {
        var c = ParseColor(hex);
        var gray = (byte)((c.R + c.G + c.B) / 3);
        var r = c.R + (int)((gray - c.R) * amount);
        var g = c.G + (int)((gray - c.G) * amount);
        var b = c.B + (int)((gray - c.B) * amount);
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 获取浅色背景（适合叠加）
    /// </summary>
    public static string GetLightBg(string hex)
    {
        var c = ParseColor(hex);
        var r = c.R + (int)((255 - c.R) * 0.9);
        var g = c.G + (int)((255 - c.G) * 0.9);
        var b = c.B + (int)((255 - c.B) * 0.9);
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 获取浅色边框
    /// </summary>
    public static string GetLightBorder(string hex)
    {
        var c = ParseColor(hex);
        var r = c.R + (int)((255 - c.R) * 0.85);
        var g = c.G + (int)((255 - c.G) * 0.85);
        var b = c.B + (int)((255 - c.B) * 0.85);
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 获取深色边框
    /// </summary>
    public static string GetDarkBorder(string hex)
    {
        return Darken(hex, 0.2);
    }

    /// <summary>
    /// 根据背景色获取前景色（自动对比）
    /// </summary>
    public static string GetContrastColor(string bgHex)
    {
        var c = ParseColor(bgHex);
        var brightness = (c.R * 299 + c.G * 587 + c.B * 114) / 1000;
        return brightness > 128 ? "#333333" : "#FFFFFF";
    }

    /// <summary>
    /// 计算悬停背景色（带透明度）
    /// </summary>
    public static string GetHoverOverlay(string baseHex)
    {
        return $"#{baseHex.TrimStart('#')}08";
    }

    /// <summary>
    /// 计算选中背景色
    /// </summary>
    public static string GetSelectedBg(string primaryHex)
    {
        var c = ParseColor(primaryHex);
        var r = c.R + (int)((255 - c.R) * 0.9);
        var g = c.G + (int)((255 - c.G) * 0.9);
        var b = c.B + (int)((255 - c.B) * 0.9);
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 计算聚焦边框色（更亮的主色）
    /// </summary>
    public static string GetFocusBorder(string primaryHex)
    {
        return Lighten(primaryHex, 0.15);
    }

    /// <summary>
    /// 计算聚焦阴影颜色（带透明度的浅主色）
    /// </summary>
    public static string GetFocusShadow(string primaryHex)
    {
        return $"{primaryHex}33";
    }

    /// <summary>
    /// 获取禁用背景色
    /// </summary>
    public static string GetDisabledBg(string baseBgHex)
    {
        var c = ParseColor(baseBgHex);
        var r = c.R + (int)((238 - c.R) * 0.5);
        var g = c.G + (int)((238 - c.G) * 0.5);
        var b = c.B + (int)((238 - c.B) * 0.5);
        return ToHex(Color.FromRgb((byte)r, (byte)g, (byte)b));
    }

    /// <summary>
    /// 获取禁用前景色
    /// </summary>
    public static string GetDisabledFg()
    {
        return "#BFBFBF";
    }

    /// <summary>
    /// 判断是否为深色主题
    /// </summary>
    public static bool IsDarkTheme(string bgHex)
    {
        var c = ParseColor(bgHex);
        var brightness = (c.R * 299 + c.G * 587 + c.B * 114) / 1000;
        return brightness < 128;
    }

    /// <summary>
    /// 计算从主色派生的语义色
    /// </summary>
    public static (string light, string dark, string bg, string border) DeriveFromPrimary(string primaryHex)
    {
        return (
            Lighten(primaryHex, 0.15),
            Darken(primaryHex, 0.15),
            GetLightBg(primaryHex),
            GetLightBorder(primaryHex)
        );
    }
}
