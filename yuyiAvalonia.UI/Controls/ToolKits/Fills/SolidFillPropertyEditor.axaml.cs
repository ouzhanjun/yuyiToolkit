using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace yuyiAvalonia.UI.Controls;

public partial class SolidFillPropertyEditor : UserControl
{
    #region FillColor (双向 #RRGGBB 字符串)
    public static readonly StyledProperty<string> FillColorProperty =
        AvaloniaProperty.Register<SolidFillPropertyEditor, string>(
            nameof(FillColor),
            defaultValue: "#FFFFFF",
            defaultBindingMode: BindingMode.TwoWay);

    public string FillColor
    {
        get => GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }
    #endregion

    #region Opacity (0-1)
    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<SolidFillPropertyEditor, double>(
            nameof(Opacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }
    #endregion

    #region OpacityPercent (0-100)
    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<SolidFillPropertyEditor, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, Math.Clamp(value, 0, 100));
    }
    #endregion

    #region PickerColor (双向 Avalonia.Media.Color)
    public static readonly StyledProperty<Color> PickerColorProperty =
        AvaloniaProperty.Register<SolidFillPropertyEditor, Color>(
            nameof(PickerColor),
            defaultValue: Colors.White,
            defaultBindingMode: BindingMode.TwoWay);

    public Color PickerColor
    {
        get => GetValue(PickerColorProperty);
        set => SetValue(PickerColorProperty, value);
    }
    #endregion

    #region PickerAlpha (双向 0-100)
    public static readonly StyledProperty<double> PickerAlphaProperty =
        AvaloniaProperty.Register<SolidFillPropertyEditor, double>(
            nameof(PickerAlpha),
            defaultValue: 100,
            defaultBindingMode: BindingMode.TwoWay);

    public double PickerAlpha
    {
        get => GetValue(PickerAlphaProperty);
        set => SetValue(PickerAlphaProperty, Math.Clamp(value, 0, 100));
    }
    #endregion

    private bool _isSyncingFromPicker;
    private bool _isSyncingToPicker;

    public SolidFillPropertyEditor()
    {
        InitializeComponent();

        // 外部 -> FillColor / Opacity -> 推给拾色器
        OpacityPercentProperty.Changed.AddClassHandler<SolidFillPropertyEditor>((control, args) =>
        {
            if (args.NewValue is double percent)
            {
                var newOpacity = percent / 100.0;
                if (Math.Abs(newOpacity - control.Opacity) > 0.001)
                {
                    control.SetValue(OpacityProperty, newOpacity);
                }
            }
            SyncToPicker();
        });

        OpacityProperty.Changed.AddClassHandler<SolidFillPropertyEditor>((control, args) =>
        {
            var percent = control.Opacity * 100.0;
            if (Math.Abs(percent - control.OpacityPercent) > 0.01)
            {
                control.SetValue(OpacityPercentProperty, percent);
            }
            SyncToPicker();
        });

        FillColorProperty.Changed.AddClassHandler<SolidFillPropertyEditor>((control, _) => SyncToPicker());

        // 拾色器 -> 同步 FillColor / Opacity
        PickerColorProperty.Changed.AddClassHandler<SolidFillPropertyEditor>((control, args) => SyncFromPicker());
        PickerAlphaProperty.Changed.AddClassHandler<SolidFillPropertyEditor>((control, args) => SyncFromPicker());
    }

    /// <summary>
    /// 把外部 FillColor / OpacityPercent 推到拾色器（picker.Color / picker.Alpha）。
    /// </summary>
    private void SyncToPicker()
    {
        if (_isSyncingFromPicker) return;
        _isSyncingToPicker = true;
        try
        {
            var parsed = TryParseColor(FillColor);
            if (parsed is Color c && PickerColor != c) PickerColor = c;
            if (Math.Abs(PickerAlpha - OpacityPercent) > 0.001) PickerAlpha = OpacityPercent;
        }
        finally
        {
            _isSyncingToPicker = false;
        }
    }

    /// <summary>
    /// 把拾色器 Color / Alpha 推回外部 FillColor / Opacity。
    /// </summary>
    private void SyncFromPicker()
    {
        if (_isSyncingToPicker) return;
        _isSyncingFromPicker = true;
        try
        {
            var hex = $"#{PickerColor.R:X2}{PickerColor.G:X2}{PickerColor.B:X2}";
            if (FillColor != hex) FillColor = hex;
            if (Math.Abs(OpacityPercent - PickerAlpha) > 0.001) OpacityPercent = PickerAlpha;
            var newOpacity = PickerAlpha / 100.0;
            if (Math.Abs(Opacity - newOpacity) > 0.001) SetValue(OpacityProperty, newOpacity);
        }
        finally
        {
            _isSyncingFromPicker = false;
        }
    }

    private static Color? TryParseColor(string? hex)
    {
        if (string.IsNullOrEmpty(hex)) return null;
        try
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                return Color.FromRgb(
                    Convert.ToByte(hex[..2], 16),
                    Convert.ToByte(hex[2..4], 16),
                    Convert.ToByte(hex[4..6], 16));
            }
            if (hex.Length == 8)
            {
                return Color.FromArgb(
                    Convert.ToByte(hex[..2], 16),
                    Convert.ToByte(hex[2..4], 16),
                    Convert.ToByte(hex[4..6], 16),
                    Convert.ToByte(hex[6..8], 16));
            }
        }
        catch { }
        return null;
    }

}