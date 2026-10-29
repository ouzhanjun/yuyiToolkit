using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace yuyiAvalonia.UI.Controls;

public partial class ShadowPropertyEditor : UserControl
{
    #region ShadowColor
    public static readonly StyledProperty<string> ShadowColorProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, string>(
            nameof(ShadowColor),
            defaultValue: "#000000",
            defaultBindingMode: BindingMode.TwoWay);

    public string ShadowColor
    {
        get => GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }
    #endregion

    #region ShadowBlur
    public static readonly StyledProperty<double> ShadowBlurProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, double>(
            nameof(ShadowBlur),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double ShadowBlur
    {
        get => GetValue(ShadowBlurProperty);
        set => SetValue(ShadowBlurProperty, value);
    }
    #endregion

    #region ShadowOffsetX
    public static readonly StyledProperty<double> ShadowOffsetXProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, double>(
            nameof(ShadowOffsetX),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double ShadowOffsetX
    {
        get => GetValue(ShadowOffsetXProperty);
        set => SetValue(ShadowOffsetXProperty, value);
    }
    #endregion

    #region ShadowOffsetY
    public static readonly StyledProperty<double> ShadowOffsetYProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, double>(
            nameof(ShadowOffsetY),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double ShadowOffsetY
    {
        get => GetValue(ShadowOffsetYProperty);
        set => SetValue(ShadowOffsetYProperty, value);
    }
    #endregion

    #region ShadowSpread
    public static readonly StyledProperty<double> ShadowSpreadProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, double>(
            nameof(ShadowSpread),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double ShadowSpread
    {
        get => GetValue(ShadowSpreadProperty);
        set => SetValue(ShadowSpreadProperty, value);
    }
    #endregion

    #region IsInset
    public static readonly StyledProperty<bool> IsInsetProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, bool>(
            nameof(IsInset),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsInset
    {
        get => GetValue(IsInsetProperty);
        set => SetValue(IsInsetProperty, value);
    }
    #endregion

    #region HasShadow
    public static readonly StyledProperty<bool> HasShadowProperty =
        AvaloniaProperty.Register<ShadowPropertyEditor, bool>(
            nameof(HasShadow),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool HasShadow
    {
        get => GetValue(HasShadowProperty);
        set => SetValue(HasShadowProperty, value);
    }
    #endregion

    public ShadowPropertyEditor()
    {
        InitializeComponent();
    }
}
