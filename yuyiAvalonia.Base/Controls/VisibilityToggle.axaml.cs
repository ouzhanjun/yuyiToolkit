using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace yuyiAvalonia.Base.Controls;

public partial class VisibilityToggle : UserControl
{
    public static new readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<VisibilityToggle, bool>(nameof(IsVisible), true,
            defaultBindingMode: BindingMode.TwoWay);

    public new bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<VisibilityToggle, double>(nameof(IconSize), 16);

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly StyledProperty<bool> ToggleOnClickProperty =
        AvaloniaProperty.Register<VisibilityToggle, bool>(nameof(ToggleOnClick), true);

    public bool ToggleOnClick
    {
        get => GetValue(ToggleOnClickProperty);
        set => SetValue(ToggleOnClickProperty, value);
    }

    public static readonly StyledProperty<object?> ToolTipProperty =
        AvaloniaProperty.Register<VisibilityToggle, object?>(nameof(ToolTip), "切换可见性");

    public object? ToolTip
    {
        get => GetValue(ToolTipProperty);
        set => SetValue(ToolTipProperty, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> ToggledEvent =
        RoutedEvent.Register<VisibilityToggle, RoutedEventArgs>(nameof(Toggled), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Toggled
    {
        add => AddHandler(ToggledEvent, value);
        remove => RemoveHandler(ToggledEvent, value);
    }

    public VisibilityToggle()
    {
        InitializeComponent();
    }

    private void OnButtonClick(object? sender, RoutedEventArgs e)
    {
        if (ToggleOnClick)
            IsVisible = !IsVisible;

        RaiseEvent(new RoutedEventArgs(ToggledEvent));
        e.Handled = true;
    }
}
