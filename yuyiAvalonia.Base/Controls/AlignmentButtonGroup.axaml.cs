using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using yuyiAvalonia.Base.Helpers;

namespace yuyiAvalonia.Base.Controls;

public partial class AlignmentButtonGroup : UserControl
{
    public static readonly StyledProperty<StreamGeometry> Icon1Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, StreamGeometry>(nameof(Icon1), null);

    public static readonly StyledProperty<StreamGeometry> Icon2Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, StreamGeometry>(nameof(Icon2), null);

    public static readonly StyledProperty<StreamGeometry> Icon3Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, StreamGeometry>(nameof(Icon3), null);

    public static readonly StyledProperty<StreamGeometry> Icon4Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, StreamGeometry>(nameof(Icon4), null);

    public static readonly StyledProperty<int> SelectedIndexProperty =
        AvaloniaProperty.Register<AlignmentButtonGroup, int>(nameof(SelectedIndex), 0);

    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<AlignmentButtonGroup, double>(nameof(Height), 28);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<AlignmentButtonGroup, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<string> Option1Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, string>(nameof(Option1), "Stretch");

    public static readonly StyledProperty<string> Option2Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, string>(nameof(Option2), "Left");

    public static readonly StyledProperty<string> Option3Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, string>(nameof(Option3), "Center");

    public static readonly StyledProperty<string> Option4Property =
        AvaloniaProperty.Register<AlignmentButtonGroup, string>(nameof(Option4), "Right");

    public static readonly StyledProperty<bool> ShowThirdButtonProperty =
        AvaloniaProperty.Register<AlignmentButtonGroup, bool>(nameof(ShowThirdButton), true);

    public static readonly StyledProperty<bool> Show4ButtonProperty =
        AvaloniaProperty.Register<AlignmentButtonGroup, bool>(nameof(Show4Button), true);

    public StreamGeometry Icon1
    {
        get => GetValue(Icon1Property);
        set => SetValue(Icon1Property, value);
    }

    public StreamGeometry Icon2
    {
        get => GetValue(Icon2Property);
        set => SetValue(Icon2Property, value);
    }

    public StreamGeometry Icon3
    {
        get => GetValue(Icon3Property);
        set
        {
            SetValue(Icon3Property, value);
            ShowThirdButton = value != null;
        }
    }

    public StreamGeometry Icon4
    {
        get => GetValue(Icon4Property);
        set
        {
            SetValue(Icon4Property, value);
            Show4Button = value != null;
        }
    }
    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool ShowThirdButton
    {
        get => GetValue(ShowThirdButtonProperty);
        set => SetValue(ShowThirdButtonProperty, value);
    }

    public bool Show4Button
    {
        get => GetValue(Show4ButtonProperty);
        set => SetValue(Show4ButtonProperty, value);
    }

    public string Option1
    {
        get => GetValue(Option1Property);
        set => SetValue(Option1Property, value);
    }

    public string Option2
    {
        get => GetValue(Option2Property);
        set => SetValue(Option2Property, value);
    }

    public string Option3
    {
        get => GetValue(Option3Property);
        set => SetValue(Option3Property, value);
    }

    public string Option4
    {
        get => GetValue(Option4Property);
        set => SetValue(Option4Property, value);
    }

    public event EventHandler<int>? SelectionChanged;

    private Button? _button1;
    private Button? _button2;
    private Button? _button3;
    private Button? _button4;
    private bool _isUpdating; // 防止循环更新

    IBrush? selectedBrush = null;
    IBrush? normalBrush = null;

    public AlignmentButtonGroup()
    {
        InitializeComponent();

        _button1 = this.FindControl<Button>("PART_Button1");
        _button2 = this.FindControl<Button>("PART_Button2");
        _button3 = this.FindControl<Button>("PART_Button3");
        _button4 = this.FindControl<Button>("PART_Button4");

        ShowThirdButton = Icon3 != null;
        Show4Button = Icon4 != null;

        PART_PropertyBorder.Attach(this);

        SelectedIndexProperty.Changed.AddClassHandler<AlignmentButtonGroup>((control, e) =>
        {
            if (!control._isUpdating)
            {
                control.UpdateButtonStyles();
            }
        });

    }

    private void UpdateButtonStyles()
    {
        object? hoverBrush = null;

        if (selectedBrush == null)
        {
            if (this.TryFindResource("ButtonBackgroundPointerOver", this.ActualThemeVariant, out hoverBrush))
            {
                selectedBrush = hoverBrush as IBrush;
            }
        }

        if (_button1 != null)
        {
            _button1.Background = SelectedIndex == 0 ? selectedBrush : normalBrush;
        }
        if (_button2 != null)
        {
            _button2.Background = SelectedIndex == 1 ? selectedBrush : normalBrush;
        }
        if (_button3 != null && ShowThirdButton)
        {
            _button3.Background = SelectedIndex == 2 ? selectedBrush : normalBrush;
        }
        if (_button4 != null && Show4Button)
        {
            _button4.Background = SelectedIndex == 3 ? selectedBrush : normalBrush;
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        SelectedIndex = 0;
        UpdateButtonStyles();
        SelectionChanged?.Invoke(this, 0);
        _isUpdating = false;
        this.Focus();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        SelectedIndex = 1;
        UpdateButtonStyles();
        SelectionChanged?.Invoke(this, 1);
        _isUpdating = false;
        this.Focus();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        SelectedIndex = 2;
        UpdateButtonStyles();
        SelectionChanged?.Invoke(this, 2);
        _isUpdating = false;
        this.Focus();
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        SelectedIndex = 3;
        UpdateButtonStyles();
        SelectionChanged?.Invoke(this, 3);
        _isUpdating = false;
        this.Focus();
    }

}
