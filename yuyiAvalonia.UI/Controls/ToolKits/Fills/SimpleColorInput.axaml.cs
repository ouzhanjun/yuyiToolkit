using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using yuyiAvalonia.Base.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class SimpleColorInput : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SimpleColorInput, string>(nameof(Text), "#000000");

    public static readonly StyledProperty<string> WatermarkProperty =
        AvaloniaProperty.Register<SimpleColorInput, string>(nameof(Watermark), "#HEX");

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<SimpleColorInput, double>(nameof(FontSize), 10);

    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<SimpleColorInput, double>(nameof(Width), 80);

    public static readonly StyledProperty<double> OpacityValueProperty =
        AvaloniaProperty.Register<SimpleColorInput, double>(nameof(OpacityValue), 0);

    public static readonly StyledProperty<bool> IsFrontColorProperty =
        AvaloniaProperty.Register<SimpleColorInput, bool>(nameof(IsFrontColor), false);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SimpleColorInput, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

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

    public bool IsFrontColor
    {
        get => GetValue(IsFrontColorProperty);
        set => SetValue(IsFrontColorProperty, value);
    }

    public event EventHandler<string>? TextChanged;
    public event EventHandler? ColorPickerClicked;

    private TextBox? _textBox;
    private ColorPickerPanel? _colorPicker;

    public SimpleColorInput()
    {
        InitializeComponent();

        _colorPicker = this.FindControl<ColorPickerPanel>("PART_ColorPicker");

        PART_PropertyBorder.Attach(this);

        PART_TextBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        };

        if (_colorPicker != null)
        {
            _colorPicker.ColorSelected += (s, result) =>
            {
                Text = result.Color;
                UpdateColorLayer();
                TextChanged?.Invoke(this, Text);
            };
        }

        TextProperty.Changed.AddClassHandler<SimpleColorInput>((control, e) =>
        {
            control.UpdateColorLayer();
        });

        IsFrontColorProperty.Changed.AddClassHandler<SimpleColorInput>((control, e) =>
        {
            if (((bool)e.NewValue!) == true)
            {
                PART_ColorPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                PART_ColorPanel.Margin = new Thickness(8, -2,-2,-2);
                PART_TextBox.Padding=new Thickness(28, 0, 6, 0);
            }
            else
            {
                PART_ColorPanel.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                PART_ColorPanel.Margin = new Thickness(-2, -2, 8, -2);
                PART_TextBox.Padding = new Thickness(6, 0, 28, 0);
            }
        });

    }

    private bool _isDragging;
    private Point _lastPos;

    // 鼠标按下：开始拖动
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            if (e.GetPosition((Visual)sender!).Y < 40)
            {
                _isDragging = true;
                _lastPos = e.GetPosition(this);
                e.Handled = true;
            }
        }
    }

    // 鼠标移动：更新 Popup 位置
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastPos;

            _lastPos = pos;
            e.Handled = true;
        }
    }

    // 鼠标松开：结束拖动
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
            string colorText = Text;

            bool res = Avalonia.Media.Color.TryParse(colorText, out Color color);

            if (!res)
            {
                PART_ColorPreview.Background = Brushes.Transparent;
                return;
            }

            PART_ColorPreview.Background = new SolidColorBrush(color);
        }
        catch (Exception ex)
        {
            PART_ColorPreview.Background = Brushes.Transparent;
        }
    }

    private void ColorPreview_Click(object? sender, PointerPressedEventArgs e)
    {
        ColorPickerClicked?.Invoke(this, EventArgs.Empty);
    }

}
