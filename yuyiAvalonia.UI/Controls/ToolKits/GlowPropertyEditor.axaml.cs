using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace yuyiAvalonia.UI.Controls;

public partial class GlowPropertyEditor : UserControl
{
    #region HasGlow
    public static readonly StyledProperty<bool> HasGlowProperty =
        AvaloniaProperty.Register<GlowPropertyEditor, bool>(
            nameof(HasGlow),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool HasGlow
    {
        get => GetValue(HasGlowProperty);
        set => SetValue(HasGlowProperty, value);
    }
    #endregion

    #region GlowColor
    public static readonly StyledProperty<string> GlowColorProperty =
        AvaloniaProperty.Register<GlowPropertyEditor, string>(
            nameof(GlowColor),
            defaultValue: "#0099FF",
            defaultBindingMode: BindingMode.TwoWay);

    public string GlowColor
    {
        get => GetValue(GlowColorProperty);
        set => SetValue(GlowColorProperty, value);
    }
    #endregion

    #region GlowBlurRadius
    public static readonly StyledProperty<double> GlowBlurRadiusProperty =
        AvaloniaProperty.Register<GlowPropertyEditor, double>(
            nameof(GlowBlurRadius),
            defaultValue: 15,
            defaultBindingMode: BindingMode.TwoWay);

    public double GlowBlurRadius
    {
        get => GetValue(GlowBlurRadiusProperty);
        set => SetValue(GlowBlurRadiusProperty, value);
    }
    #endregion

    #region GlowOffsetX
    public static readonly StyledProperty<double> GlowOffsetXProperty =
        AvaloniaProperty.Register<GlowPropertyEditor, double>(
            nameof(GlowOffsetX),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double GlowOffsetX
    {
        get => GetValue(GlowOffsetXProperty);
        set => SetValue(GlowOffsetXProperty, value);
    }
    #endregion

    #region GlowOffsetY
    public static readonly StyledProperty<double> GlowOffsetYProperty =
        AvaloniaProperty.Register<GlowPropertyEditor, double>(
            nameof(GlowOffsetY),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double GlowOffsetY
    {
        get => GetValue(GlowOffsetYProperty);
        set => SetValue(GlowOffsetYProperty, value);
    }
    #endregion

    #region GlowOpacity
    public static readonly StyledProperty<double> GlowOpacityProperty =
        AvaloniaProperty.Register<GlowPropertyEditor, double>(
            nameof(GlowOpacity),
            defaultValue: 0.9,
            defaultBindingMode: BindingMode.TwoWay);

    public double GlowOpacity
    {
        get => GetValue(GlowOpacityProperty);
        set => SetValue(GlowOpacityProperty, value);
    }
    #endregion

    public GlowPropertyEditor()
    {
        InitializeComponent();

        // 使用 AddClassHandler 处理属性变化（绑定会自动更新元素）
        HasGlowProperty.Changed.AddClassHandler<GlowPropertyEditor>((sender, e) => { });
        GlowColorProperty.Changed.AddClassHandler<GlowPropertyEditor>((sender, e) => { });
        GlowBlurRadiusProperty.Changed.AddClassHandler<GlowPropertyEditor>((sender, e) => { });
        GlowOffsetXProperty.Changed.AddClassHandler<GlowPropertyEditor>((sender, e) => { });
        GlowOffsetYProperty.Changed.AddClassHandler<GlowPropertyEditor>((sender, e) => { });
        GlowOpacityProperty.Changed.AddClassHandler<GlowPropertyEditor>((sender, e) => { });
    }
}