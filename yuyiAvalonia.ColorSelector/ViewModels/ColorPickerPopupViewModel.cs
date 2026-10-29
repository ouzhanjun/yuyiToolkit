using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ColorSelector.ViewModels;

/// <summary>
/// 拾色器弹出窗口视图模型
/// </summary>
public partial class ColorPickerPopupViewModel : ObservableObject
{
    [ObservableProperty]
    private string _currentColor = "#1890FF";

    [ObservableProperty]
    private byte _red = 24;

    [ObservableProperty]
    private byte _green = 144;

    [ObservableProperty]
    private byte _blue = 255;

    [ObservableProperty]
    private byte _alpha = 255;

    [ObservableProperty]
    private double _hue;

    [ObservableProperty]
    private double _saturation = 100;

    [ObservableProperty]
    private double _brightness = 100;

    [ObservableProperty]
    private string _hexColor = "#1890FF";

    private bool _isUpdating = false;

    public ColorPickerPopupViewModel()
    {
    }

    public ColorPickerPopupViewModel(string initialColor)
    {
        SetColor(initialColor);
    }

    partial void OnRedChanged(byte value) => UpdateFromRgb();
    partial void OnGreenChanged(byte value) => UpdateFromRgb();
    partial void OnBlueChanged(byte value) => UpdateFromRgb();
    partial void OnAlphaChanged(byte value) => UpdateFromRgb();

    partial void OnHueChanged(double value) => UpdateFromHsb();
    partial void OnSaturationChanged(double value) => UpdateFromHsb();
    partial void OnBrightnessChanged(double value) => UpdateFromHsb();

    partial void OnHexColorChanged(string value)
    {
        if (!_isUpdating && IsValidHex(value))
        {
            SetColorFromHex(value);
        }
    }

    private void UpdateFromRgb()
    {
        if (_isUpdating) return;
        _isUpdating = true;

        var color = Color.FromArgb(Alpha, Red, Green, Blue);
        HexColor = ToHex(color);
        (Hue, Saturation, Brightness) = RgbToHsb(Red, Green, Blue);

        _isUpdating = false;
    }

    private void UpdateFromHsb()
    {
        if (_isUpdating) return;
        _isUpdating = true;

        (Red, Green, Blue) = HsbToRgb(Hue, Saturation, Brightness);
        var color = Color.FromArgb(Alpha, Red, Green, Blue);
        HexColor = ToHex(color);

        _isUpdating = false;
    }

    public void SetColor(string colorStr)
    {
        if (string.IsNullOrEmpty(colorStr)) return;

        try
        {
            if (colorStr.StartsWith("#") && (colorStr.Length == 7 || colorStr.Length == 9))
            {
                var hex = colorStr.TrimStart('#');
                if (hex.Length == 6)
                {
                    Red = Convert.ToByte(hex.Substring(0, 2), 16);
                    Green = Convert.ToByte(hex.Substring(2, 2), 16);
                    Blue = Convert.ToByte(hex.Substring(4, 2), 16);
                    Alpha = 255;
                }
                else if (hex.Length == 8)
                {
                    Alpha = Convert.ToByte(hex.Substring(0, 2), 16);
                    Red = Convert.ToByte(hex.Substring(2, 2), 16);
                    Green = Convert.ToByte(hex.Substring(4, 2), 16);
                    Blue = Convert.ToByte(hex.Substring(6, 2), 16);
                }
            }
        }
        catch
        {
            Red = 24;
            Green = 144;
            Blue = 255;
            Alpha = 255;
        }

        UpdateFromRgb();
    }

    private void SetColorFromHex(string hex)
    {
        if (!IsValidHex(hex)) return;
        SetColor(hex);
    }

    private bool IsValidHex(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return false;
        hex = hex.TrimStart('#');
        return (hex.Length == 6 || hex.Length == 8) &&
               hex.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }

    private string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public string GetFullHex()
    {
        return $"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
    }

    private (double h, double s, double b) RgbToHsb(byte r, byte g, byte b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        double h = 0;
        double s = max == 0 ? 0 : delta / max * 100;
        double brightness = max * 100;

        if (delta > 0)
        {
            if (max == rd)
                h = 60 * (((gd - bd) / delta) % 6);
            else if (max == gd)
                h = 60 * (((bd - rd) / delta) + 2);
            else
                h = 60 * (((rd - gd) / delta) + 4);
        }

        if (h < 0) h += 360;

        return (h, s, brightness);
    }

    private (byte r, byte g, byte b) HsbToRgb(double h, double s, double brightness)
    {
        double sNorm = s / 100.0;
        double v = brightness / 100.0;

        if (sNorm == 0)
        {
            byte val = (byte)(v * 255);
            return (val, val, val);
        }

        double c = v * sNorm;
        double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        double m = v - c;

        double r1, g1, b1;

        if (h < 60)
        {
            (r1, g1, b1) = (c, x, 0.0);
        }
        else if (h < 120)
        {
            (r1, g1, b1) = (x, c, 0.0);
        }
        else if (h < 180)
        {
            (r1, g1, b1) = (0.0, c, x);
        }
        else if (h < 240)
        {
            (r1, g1, b1) = (0.0, x, c);
        }
        else if (h < 300)
        {
            (r1, g1, b1) = (x, 0.0, c);
        }
        else
        {
            (r1, g1, b1) = (c, 0.0, x);
        }

        return (
            (byte)((r1 + m) * 255),
            (byte)((g1 + m) * 255),
            (byte)((b1 + m) * 255)
        );
    }
}
