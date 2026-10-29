using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace yuyiAvalonia.Base.Controls;

public partial class FourButtonGroup : UserControl
{
    public static readonly StyledProperty<StreamGeometry> Icon1Property =
        AvaloniaProperty.Register<FourButtonGroup, StreamGeometry>(nameof(Icon1), null);

    public static readonly StyledProperty<StreamGeometry> Icon2Property =
        AvaloniaProperty.Register<FourButtonGroup, StreamGeometry>(nameof(Icon2), null);

    public static readonly StyledProperty<StreamGeometry> Icon3Property =
        AvaloniaProperty.Register<FourButtonGroup, StreamGeometry>(nameof(Icon3), null);

    public static readonly StyledProperty<StreamGeometry> Icon4Property =
        AvaloniaProperty.Register<FourButtonGroup, StreamGeometry>(nameof(Icon4), null);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<FourButtonGroup, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<string?> Tip1Property =
        AvaloniaProperty.Register<FourButtonGroup, string?>(nameof(Tip1), null);

    public static readonly StyledProperty<string?> Tip2Property =
        AvaloniaProperty.Register<FourButtonGroup, string?>(nameof(Tip2), null);

    public static readonly StyledProperty<string?> Tip3Property =
        AvaloniaProperty.Register<FourButtonGroup, string?>(nameof(Tip3), null);

    public static readonly StyledProperty<string?> Tip4Property =
        AvaloniaProperty.Register<FourButtonGroup, string?>(nameof(Tip4), null);

    #region ItemsSource

    public static readonly StyledProperty<IList<object>?> ItemsSourceProperty =
        AvaloniaProperty.Register<FourButtonGroup, IList<object>?>(
            nameof(ItemsSource),
            defaultValue: null);

    public IList<object>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<FourButtonGroup, object?>(
            nameof(SelectedItem),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    #endregion

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
        set => SetValue(Icon3Property, value);
    }

    public StreamGeometry Icon4
    {
        get => GetValue(Icon4Property);
        set => SetValue(Icon4Property, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public string? Tip1
    {
        get => GetValue(Tip1Property);
        set => SetValue(Tip1Property, value);
    }

    public string? Tip2
    {
        get => GetValue(Tip2Property);
        set => SetValue(Tip2Property, value);
    }

    public string? Tip3
    {
        get => GetValue(Tip3Property);
        set => SetValue(Tip3Property, value);
    }

    public string? Tip4
    {
        get => GetValue(Tip4Property);
        set => SetValue(Tip4Property, value);
    }

    public event EventHandler<object?>? SelectionChanged;

    IBrush? selectedBrush = null;
    IBrush? normalBrush = null;

    public FourButtonGroup()
    {
        InitializeComponent();

        PART_PropertyBorder.Attach(this);

        PART_Button1!.Click += Button1_Click;
        PART_Button2!.Click += Button2_Click;
        PART_Button3!.Click += Button3_Click;
        PART_Button4!.Click += Button4_Click;

        normalBrush = Brushes.Transparent;

        SelectedItemProperty.Changed.AddClassHandler<FourButtonGroup>((control, e) =>
        {
            control.UpdateButtonStyles();
        });

        this.Loaded += (object? sender, RoutedEventArgs e) =>
        {
            this.UpdateButtonStyles();
        };
    }

    private void UpdateButtonStyles()
    {
        if (ItemsSource == null)
        {
            return;
        }

        object? hoverBrush = null;
        if (selectedBrush == null)
        {
            if (this.TryFindResource("ButtonBackgroundPointerOver", this.ActualThemeVariant, out hoverBrush))
            {
                selectedBrush = hoverBrush as IBrush;
            }
        }

        int index = ItemsSource!.IndexOf(SelectedItem!);
        PART_Button1.Background = index == 0 ? selectedBrush : normalBrush;
        PART_Button2.Background = index == 1 ? selectedBrush : normalBrush;
        PART_Button3.Background = index == 2 ? selectedBrush : normalBrush;
        PART_Button4.Background = index == 3 ? selectedBrush : normalBrush;
        SelectionChanged?.Invoke(this, SelectedItem);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        int index = 0;
        SelectedItem = ItemsSource != null && ItemsSource.Count >= 1 ? ItemsSource[index] : null;
        UpdateButtonStyles();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        int index = 1;
        SelectedItem = ItemsSource != null && ItemsSource.Count >= 2 ? ItemsSource[index] : null;
        UpdateButtonStyles();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        int index = 2;
        SelectedItem = ItemsSource != null && ItemsSource.Count >= 3 ? ItemsSource[index] : null;
        UpdateButtonStyles();
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        int index = 3;
        SelectedItem = ItemsSource != null && ItemsSource.Count >= 4 ? ItemsSource[index] : null;
        UpdateButtonStyles();
    }
}
