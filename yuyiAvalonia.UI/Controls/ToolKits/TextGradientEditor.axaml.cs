using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace yuyiAvalonia.UI.Controls;

public partial class TextGradientEditor : UserControl
{
    public static readonly StyledProperty<bool> HasTextGradientProperty =
        AvaloniaProperty.Register<TextGradientEditor, bool>(
            nameof(HasTextGradient),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> TextGradientStartColorProperty =
        AvaloniaProperty.Register<TextGradientEditor, string>(
            nameof(TextGradientStartColor),
            defaultValue: "#00f5d4",
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> TextGradientEndColorProperty =
        AvaloniaProperty.Register<TextGradientEditor, string>(
            nameof(TextGradientEndColor),
            defaultValue: "#7b2fff",
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> TextGradientAngleProperty =
        AvaloniaProperty.Register<TextGradientEditor, double>(
            nameof(TextGradientAngle),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public bool HasTextGradient
    {
        get => GetValue(HasTextGradientProperty);
        set => SetValue(HasTextGradientProperty, value);
    }

    public string TextGradientStartColor
    {
        get => GetValue(TextGradientStartColorProperty);
        set => SetValue(TextGradientStartColorProperty, value);
    }

    public string TextGradientEndColor
    {
        get => GetValue(TextGradientEndColorProperty);
        set => SetValue(TextGradientEndColorProperty, value);
    }

    public double TextGradientAngle
    {
        get => GetValue(TextGradientAngleProperty);
        set => SetValue(TextGradientAngleProperty, value);
    }

    public TextGradientEditor()
    {
        InitializeComponent();
    }
}