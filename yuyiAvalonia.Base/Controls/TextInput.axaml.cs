using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System.Text.Json;

namespace yuyiAvalonia.Base.Controls;

public partial class TextInput : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<TextInput, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<TextInput, string>(nameof(Watermark), string.Empty);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<TextInput, double>(nameof(FontSize), 10);

    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<TextInput, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<TextInput, VerticalAlignment>(nameof(VerticalContentAlignment), VerticalAlignment.Center);

    public static readonly StyledProperty<bool> AcceptsReturnProperty =
        AvaloniaProperty.Register<TextInput, bool>(nameof(AcceptsReturn), false);

    public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.Register<TextInput, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Suffix), string.Empty);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<StreamGeometry?> IconProperty =
        AvaloniaProperty.Register<NumericUpDownEx, StreamGeometry?>(nameof(Icon));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public StreamGeometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public bool AcceptsReturn
    {
        get => GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    public TextWrapping TextWrapping
    {
        get => GetValue(TextWrappingProperty);
        set => SetValue(TextWrappingProperty, value);
    }

    public event EventHandler<string>? TextChanged;

    private TextBox? _textBox;

    public TextInput()
    {
        InitializeComponent();

        _textBox = this.FindControl<TextBox>("PART_TextBox");

        PART_PropertyBorder.Attach(this);

        if (_textBox != null)
        {
            _textBox.TextChanged += (s, e) =>
            {
                TextChanged?.Invoke(this, Text);
            };

            _textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && !AcceptsReturn)
                {
                    this.Focus();
                }
            };
        }

        IconProperty.Changed.AddClassHandler<TextInput>((control, _) => control.UpdateHeaderDisplay());
        TitleProperty.Changed.AddClassHandler<TextInput>((control, _) => control.UpdateHeaderDisplay());

        TextProperty.Changed.AddClassHandler<TextInput>((control, e) =>
        {
            if (control._textBox != null && control._textBox.Text != e.NewValue?.ToString())
            {
                control._textBox.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        });

        this.Loaded += (s, e) =>
        {
            UpdateHeaderDisplay();
        };

    }

    private void UpdateHeaderDisplay()
    {
        if (PART_Title == null || PART_Icon == null )
            return;

        var hasIcon = Icon != null;
        var hasTitle = !string.IsNullOrWhiteSpace(Title);
        var hasHeader = hasIcon || hasTitle;

        PART_Icon.IsVisible = hasIcon;
        PART_Title.IsVisible = !hasIcon && hasTitle;

        if (hasIcon)
            PART_Icon.Data = Icon;
        else
            PART_Title.Text = Title;

    }

}
