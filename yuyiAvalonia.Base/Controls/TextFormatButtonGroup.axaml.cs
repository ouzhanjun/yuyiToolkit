using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace yuyiAvalonia.Base.Controls;

public partial class TextFormatButtonGroup : UserControl
{
    public static readonly StyledProperty<bool> IsBoldProperty =
        AvaloniaProperty.Register<TextFormatButtonGroup, bool>(nameof(IsBold), defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsItalicProperty =
        AvaloniaProperty.Register<TextFormatButtonGroup, bool>(nameof(IsItalic), defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsUnderlineProperty =
        AvaloniaProperty.Register<TextFormatButtonGroup, bool>(nameof(IsUnderline), defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsStrikethroughProperty =
        AvaloniaProperty.Register<TextFormatButtonGroup, bool>(nameof(IsStrikethrough), defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<TextFormatButtonGroup, double>(nameof(Height), 28);

    public bool IsBold
    {
        get => GetValue(IsBoldProperty);
        set => SetValue(IsBoldProperty, value);
    }

    public bool IsItalic
    {
        get => GetValue(IsItalicProperty);
        set => SetValue(IsItalicProperty, value);
    }

    public bool IsUnderline
    {
        get => GetValue(IsUnderlineProperty);
        set => SetValue(IsUnderlineProperty, value);
    }

    public bool IsStrikethrough
    {
        get => GetValue(IsStrikethroughProperty);
        set => SetValue(IsStrikethroughProperty, value);
    }

    public new double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public TextFormatButtonGroup()
    {
        InitializeComponent();
    }
}
