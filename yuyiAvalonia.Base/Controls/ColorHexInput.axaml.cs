using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace yuyiAvalonia.Base.Controls;

public partial class ColorHexInput : UserControl
{
    public enum ColorInputMode { Hex, Css }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ColorHexInput, string>(nameof(Text), "#000000");

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<ColorHexInput, string>(nameof(Watermark), "#HEX");

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<ColorHexInput, double>(nameof(FontSize), 10);

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ColorHexInput, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<ColorHexInput, string>(nameof(Suffix), string.Empty);

    public static readonly StyledProperty<StreamGeometry?> IconProperty =
        AvaloniaProperty.Register<ColorHexInput, StreamGeometry?>(nameof(Icon));

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<ColorHexInput, FontFamily>(nameof(FontFamily), new FontFamily("Consolas"));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<ColorHexInput, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<string> CssTextProperty =
        AvaloniaProperty.Register<ColorHexInput, string>(nameof(CssText), string.Empty);

    public static readonly StyledProperty<string> CssWatermarkProperty =
        AvaloniaProperty.Register<ColorHexInput, string>(nameof(CssWatermark), "rgba(0,0,0,1)");

    public static readonly StyledProperty<ColorInputMode> ModeProperty =
        AvaloniaProperty.Register<ColorHexInput, ColorInputMode>(nameof(Mode), ColorInputMode.Hex);

    public static readonly StyledProperty<byte> RedProperty =
        AvaloniaProperty.Register<ColorHexInput, byte>(nameof(Red), 0);

    public static readonly StyledProperty<byte> GreenProperty =
        AvaloniaProperty.Register<ColorHexInput, byte>(nameof(Green), 0);

    public static readonly StyledProperty<byte> BlueProperty =
        AvaloniaProperty.Register<ColorHexInput, byte>(nameof(Blue), 0);

    public static readonly StyledProperty<double> AlphaPercentProperty =
        AvaloniaProperty.Register<ColorHexInput, double>(nameof(AlphaPercent), 100);

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
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

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public string CssText
    {
        get => GetValue(CssTextProperty);
        set => SetValue(CssTextProperty, value);
    }

    public string CssWatermark
    {
        get => GetValue(CssWatermarkProperty);
        set => SetValue(CssWatermarkProperty, value);
    }

    public ColorInputMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public byte Red
    {
        get => GetValue(RedProperty);
        set => SetValue(RedProperty, value);
    }

    public byte Green
    {
        get => GetValue(GreenProperty);
        set => SetValue(GreenProperty, value);
    }

    public byte Blue
    {
        get => GetValue(BlueProperty);
        set => SetValue(BlueProperty, value);
    }

    public double AlphaPercent
    {
        get => GetValue(AlphaPercentProperty);
        set => SetValue(AlphaPercentProperty, value);
    }


    public event EventHandler<string>? TextChanged;
    public event EventHandler<string>? CssTextChanged;
    public event EventHandler<ColorInputMode>? ModeChanged;

    private TextBox? _textBox;
    private TextBox? _cssTextBox;

    public ColorHexInput()
    {
        InitializeComponent();

        PART_PropertyBorder.Attach(this);

        _textBox = this.PART_TextBox;
        _cssTextBox = this.PART_CssTextBox;

        if (_textBox != null)
        {
            _textBox.TextChanged += (s, e) =>
            {
                Text = _textBox.Text ?? string.Empty;
                TextChanged?.Invoke(this, Text);
            };

            _textBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    this.Focus();
                }
            };
        }

        if (_cssTextBox != null)
        {
            _cssTextBox.TextChanged += (s, e) =>
            {
                CssText = _cssTextBox.Text ?? string.Empty;
                CssTextChanged?.Invoke(this, CssText);
            };

            _cssTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    this.Focus();
                }
            };
        }

        TitleProperty.Changed.AddClassHandler<ColorHexInput>((control, _) => control.UpdateHeaderDisplay());
        ModeProperty.Changed.AddClassHandler<ColorHexInput>((control, _) =>
        {
            control.ApplyMode();
            control.ModeChanged?.Invoke(control, control.Mode);
        });

        TextProperty.Changed.AddClassHandler<ColorHexInput>((control, e) =>
        {
            if (control._textBox != null && control._textBox.Text != e.NewValue?.ToString())
            {
                control._textBox.Text = e.NewValue?.ToString() ?? string.Empty;
            }
            control.SyncRgbaFromHexOrCss();
        });

        CssTextProperty.Changed.AddClassHandler<ColorHexInput>((control, e) =>
        {
            if (control._cssTextBox != null && control._cssTextBox.Text != e.NewValue?.ToString())
            {
                control._cssTextBox.Text = e.NewValue?.ToString() ?? string.Empty;
            }
            control.SyncRgbaFromHexOrCss();
        });

        RedProperty.Changed.AddClassHandler<ColorHexInput>((control, _) => control.SyncHexCssFromRgba());
        GreenProperty.Changed.AddClassHandler<ColorHexInput>((control, _) => control.SyncHexCssFromRgba());
        BlueProperty.Changed.AddClassHandler<ColorHexInput>((control, _) => control.SyncHexCssFromRgba());
        AlphaPercentProperty.Changed.AddClassHandler<ColorHexInput>((control, _) => control.SyncHexCssFromRgba());

        Loaded += (s, e) =>
        {
            UpdateHeaderDisplay();
            BuildModeItems();
            ApplyMode();
        };
    }


    private void UpdateHeaderDisplay()
    {
        if (PART_Title == null || PART_Icon == null || PART_GridContent == null || PART_TitleTrigger == null)
            return;

        var hasIcon = Icon != null;
        var hasTitle = !string.IsNullOrWhiteSpace(Title);

        PART_Icon.IsVisible = hasIcon;
        // Title trigger 仅在「没有 Icon」时显示，作为可点击的 Hex / CSS 标签
        PART_TitleTrigger.IsVisible = !hasIcon && hasTitle;
        PART_Title.Text = Title;

        PART_GridContent.ColumnDefinitions[0].Width = (hasIcon || hasTitle) ? PART_GridContent.ColumnDefinitions[0].Width : new GridLength(0);
    }

    private void BuildModeItems()
    {
        if (PART_ModeItems == null) return;
        PART_ModeItems.Children.Clear();

        var items = new[] { (Label: "Hex", Mode: ColorInputMode.Hex), (Label: "CSS", Mode: ColorInputMode.Css) };
        foreach (var it in items)
        {
            IBrush fgBrush = Brushes.Black;
            if (Application.Current?.Resources.TryGetResource("TextPrimary", Application.Current.ActualThemeVariant, out var res) == true && res is IBrush b)
                fgBrush = b;

            var item = new Border
            {
                Background = new SolidColorBrush(Colors.Transparent),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(8, 4),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = it.Mode
            };
            var text = new TextBlock { Text = it.Label, FontSize = 10, Foreground = fgBrush };
            item.Child = text;
            item.PointerPressed += ModeItem_PointerPressed;
            PART_ModeItems.Children.Add(item);
        }
    }

    private void TitleTrigger_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (PART_ModePopup == null) return;
        PART_ModePopup.Open();
        e.Handled = true;
    }

    private void ModeItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border b && b.Tag is ColorInputMode mode)
        {
            Mode = mode;
            PART_ModePopup?.Close();
        }
        e.Handled = true;
    }

    private void ApplyMode()
    {
        if (_textBox != null) _textBox.IsVisible = Mode == ColorInputMode.Hex;
        if (_cssTextBox != null) _cssTextBox.IsVisible = Mode == ColorInputMode.Css;

        if (PART_Title != null)
        {
            // 当用户已经设定了 Title（如 "Hex"），跟随模式切换标签文字
            if (string.Equals(Title, "Hex", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Title, "CSS", StringComparison.OrdinalIgnoreCase))
            {
                PART_Title.Text = Mode == ColorInputMode.Hex ? "Hex" : "CSS";
            }
        }
    }

    /// <summary>
    /// 把 R/G/B/AlphaPercent 同步成 Hex 文本 + Css 文本。
    /// 当 Red/Green/Blue/AlphaPercent 任意变化时调用。
    /// </summary>
    private void SyncHexCssFromRgba()
    {
        byte a255 = (byte)Math.Clamp(Math.Round(AlphaPercent / 100.0 * 255.0), 0, 255);
        string hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", Red, Green, Blue, a255);
        if (Text != hex) Text = hex;

        double a = Math.Round(AlphaPercent / 100.0, 2);
        string css = $"rgba({Red}, {Green}, {Blue}, {a:0.##})";
        if (CssText != css) CssText = css;
    }

    /// <summary>
    /// 从 Hex 文本 / Css 文本解析回 R/G/B/AlphaPercent。
    /// 当 Text / CssText 任意变化时调用。
    /// </summary>
    private void SyncRgbaFromHexOrCss()
    {
        var (r, g, b, a) = ParseColor(Text);
        if (r.HasValue && r.Value != Red) Red = r.Value;
        if (g.HasValue && g.Value != Green) Green = g.Value;
        if (b.HasValue && b.Value != Blue) Blue = b.Value;
        if (a.HasValue)
        {
            double pct = Math.Round(a.Value / 255.0 * 100.0);
            if (Math.Abs(pct - AlphaPercent) > 0.0001) AlphaPercent = pct;
        }
    }

    private static (byte? r, byte? g, byte? b, byte? a) ParseColor(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return (null, null, null, null);
        var s = text.Trim();

        // CSS rgba(R, G, B, A)
        if (s.StartsWith("rgba", StringComparison.OrdinalIgnoreCase))
        {
            int open = s.IndexOf('(');
            int close = s.LastIndexOf(')');
            if (open >= 0 && close > open)
            {
                var inner = s.Substring(open + 1, close - open - 1);
                var parts = inner.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length >= 3 &&
                    byte.TryParse(parts[0], out var rr) &&
                    byte.TryParse(parts[1], out var gg) &&
                    byte.TryParse(parts[2], out var bb))
                {
                    byte aa = 255;
                    if (parts.Length >= 4)
                    {
                        if (double.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var af))
                        {
                            aa = (byte)Math.Clamp(Math.Round(af * 255.0), 0, 255);
                        }
                    }
                    return (rr, gg, bb, aa);
                }
            }
            return (null, null, null, null);
        }

        // #RRGGBB 或 #RRGGBBAA
        if (s.StartsWith("#")) s = s.Substring(1);
        if (s.Length == 6)
        {
            if (byte.TryParse(s.AsSpan(0, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var rr) &&
                byte.TryParse(s.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var gg) &&
                byte.TryParse(s.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var bb))
            {
                return (rr, gg, bb, (byte)255);
            }
        }
        else if (s.Length == 8)
        {
            if (byte.TryParse(s.AsSpan(0, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var rr) &&
                byte.TryParse(s.AsSpan(2, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var gg) &&
                byte.TryParse(s.AsSpan(4, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var bb) &&
                byte.TryParse(s.AsSpan(6, 2), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var aa))
            {
                return (rr, gg, bb, aa);
            }
        }
        return (null, null, null, null);
    }

}