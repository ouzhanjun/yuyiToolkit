using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;

namespace yuyiAvalonia.Base.Controls;

public partial class ColorInput : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<ColorInput, string>(nameof(Text), "#000000");

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<ColorInput, string>(nameof(Watermark), "#HEX");

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<ColorInput, double>(nameof(FontSize), 10);

    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<ColorInput, double>(nameof(Width), 80);

    public static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<ColorInput, bool>(nameof(IsPickerOpen), false);

    public static readonly StyledProperty<double> OpacityValueProperty =
        AvaloniaProperty.Register<ColorInput, double>(nameof(OpacityValue), 0);

    public static readonly StyledProperty<bool> IsFrontColorProperty =
        AvaloniaProperty.Register<ColorInput, bool>(nameof(IsFrontColor), false);

    public static readonly StyledProperty<string> DisplayTextProperty =
        AvaloniaProperty.Register<ColorInput, string>(nameof(DisplayText), "000000");

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<ColorInput, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    #region PickerColor / PickerAlpha (TwoWay -> 内嵌 ColorPickerPanel)
    public static readonly StyledProperty<Color> PickerColorProperty =
        AvaloniaProperty.Register<ColorInput, Color>(
            nameof(PickerColor),
            defaultValue: Colors.White,
            defaultBindingMode: BindingMode.TwoWay);

    public Color PickerColor
    {
        get => GetValue(PickerColorProperty);
        set => SetValue(PickerColorProperty, value);
    }

    public static readonly StyledProperty<double> PickerAlphaProperty =
        AvaloniaProperty.Register<ColorInput, double>(
            nameof(PickerAlpha),
            defaultValue: 100,
            defaultBindingMode: BindingMode.TwoWay);

    public double PickerAlpha
    {
        get => GetValue(PickerAlphaProperty);
        set => SetValue(PickerAlphaProperty, Math.Clamp(value, 0, 100));
    }
    #endregion

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

    protected string DisplayText
    {
        get => GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public string Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public double OpacityValue
    {
        get => GetValue(OpacityValueProperty);
        set => SetValue(OpacityValueProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public new double Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    public bool IsFrontColor
    {
        get => GetValue(IsFrontColorProperty);
        set => SetValue(IsFrontColorProperty, value);
    }

    public event EventHandler<string>? TextChanged;
    public event EventHandler? ColorPickerClicked;

    private bool _isSyncingFromPicker;
    private bool _isSyncingToPicker;

    private TextBox? _textBox;
    private Border? _colorLayer;
    private ColorPickerPanel? _colorPicker;

    public ColorInput()
    {
        InitializeComponent();

        PART_PropertyBorder.Attach(this);

        _colorLayer = this.FindControl<Border>("ColorLayer");
        _colorPicker = this.FindControl<ColorPickerPanel>("PART_ColorPicker");

        DragHost.PointerPressed += OnPointerPressed;
        DragHost.PointerMoved += OnPointerMoved;
        DragHost.PointerReleased += OnPointerReleased;

        PART_TextBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        };

        // 外部 -> PickerColor / PickerAlpha
        PickerColorProperty.Changed.AddClassHandler<ColorInput>((control, _) => control.SyncToPicker());
        PickerAlphaProperty.Changed.AddClassHandler<ColorInput>((control, _) => control.SyncToPicker());

        if (_colorPicker != null)
        {
            // 内嵌拾色器变化 -> 同步到 PickerColor / PickerAlpha（TwoWay 绑定反向推回外部）
            _colorPicker.ColorSelected += (s, result) =>
            {
                if (_isSyncingToPicker) return;
                _isSyncingFromPicker = true;
                try
                {
                    if (Color.TryParse(result.Color, out var color) && PickerColor != color)
                    {
                        PickerColor = color;
                    }
                    if (Math.Abs(PickerAlpha - result.Alpha) > 0.001)
                    {
                        PickerAlpha = result.Alpha;
                    }
                    OpacityValue = result.Alpha;

                    var hex = $"#{result.Color.TrimStart('#')}";
                    if (Text != hex)
                    {
                        Text = hex;
                        TextChanged?.Invoke(this, Text);
                    }
                    UpdateColorLayer();
                }
                finally
                {
                    _isSyncingFromPicker = false;
                }
            };

            _colorPicker.CloseRequested += (s, e) =>
            {
                IsPickerOpen = false;
            };
        }

        TextProperty.Changed.AddClassHandler<ColorInput>((control, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                control.UpdateColorLayer();
            });
        });

        IsFrontColorProperty.Changed.AddClassHandler<ColorInput>((control, e) =>
        {
            if (((bool)e.NewValue!) == true)
            {
                PART_ColorPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                PART_ColorPanel.Margin = new Thickness(8, -2, -2, -2);
                PART_TextBox.Padding = new Thickness(28, 0, 6, 0);
            }
            else
            {
                PART_ColorPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                PART_ColorPanel.Margin = new Thickness(-2, -2, 8, -2);
                PART_TextBox.Padding = new Thickness(6, 0, 28, 0);
            }
        });

        PART_ColorPickerPopup.Opened += PART_ColorPickerPopup_Opened;
        PART_ColorPickerPopup.Closed += PART_ColorPickerPopup_Closed;
    }

    /// <summary>
    /// 把外部 PickerColor / PickerAlpha 推到内嵌拾色器。
    /// </summary>
    private void SyncToPicker()
    {
        if (_isSyncingFromPicker || _colorPicker == null) return;
        _isSyncingToPicker = true;
        try
        {
            if (_colorPicker.Color != PickerColor) _colorPicker.Color = PickerColor;
            if (Math.Abs(_colorPicker.Alpha - PickerAlpha) > 0.001) _colorPicker.Alpha = PickerAlpha;
        }
        finally
        {
            _isSyncingToPicker = false;
        }
    }

    private void PART_ColorPickerPopup_Closed(object? sender, EventArgs e)
    {
        IsPickerOpen = false;
    }

    private void PART_ColorPickerPopup_Opened(object? sender, EventArgs e)
    {
        PART_ColorPickerPopup.HorizontalOffset = 0;
        PART_ColorPickerPopup.VerticalOffset = 0;
    }

    private bool _isDragging;
    private Point _lastPos;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            _isDragging = true;
            _lastPos = e.GetPosition(this);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastPos;

            PART_ColorPickerPopup.HorizontalOffset += delta.X;
            PART_ColorPickerPopup.VerticalOffset += delta.Y;

            _lastPos = pos;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    private void UpdateColorLayer()
    {
        if (PART_ColorPreview == null)
            return;

        try
        {
            bool res = Color.TryParse(Text, out Color color);
            if (string.IsNullOrWhiteSpace(Text) || !res)
            {
                PART_ColorPreview.Background = Brushes.Transparent;
                return;
            }

            PART_ColorPreview.Background = new SolidColorBrush(color);
        }
        catch
        {
            PART_ColorPreview.Background = Brushes.Transparent;
        }
    }

    private void ColorPreview_Click(object? sender, PointerPressedEventArgs e)
    {
        if (_colorPicker == null)
        {
            return;
        }

        _colorPicker.Alpha = OpacityValue;
        _colorPicker.SetColor(Text);
        IsPickerOpen = true;
        PART_ColorPickerPopup.Open();
        ColorPickerClicked?.Invoke(this, EventArgs.Empty);
    }
}