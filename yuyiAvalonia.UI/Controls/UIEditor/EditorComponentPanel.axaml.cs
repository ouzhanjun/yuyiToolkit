using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class EditorComponentPanel : UserControl
{
    public static readonly StyledProperty<ObservableCollection<ComponentGroup>> GroupsProperty =
        AvaloniaProperty.Register<EditorComponentPanel, ObservableCollection<ComponentGroup>>(
            nameof(Groups));

    public ObservableCollection<ComponentGroup> Groups
    {
        get => GetValue(GroupsProperty);
        set => SetValue(GroupsProperty, value);
    }

    public event EventHandler<(ComponentDefinition Definition, Avalonia.Input.PointerPressedEventArgs Args)>? WhenComponentPressed;
    public event EventHandler<(ComponentDefinition Definition, Avalonia.Input.PointerEventArgs Args)>? WhenComponentMoved;
    public event EventHandler<(ComponentDefinition Definition, Avalonia.Input.PointerReleasedEventArgs Args)>? WhenComponentReleased;

    public EditorComponentPanel()
    {
        InitializeComponent();
    }

    private async void OnGroupHeaderTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (sender is Border border && border.Tag is ComponentGroup group)
        {
            // 找到 StackPanel
            var dockPanel = border.Parent as DockPanel;
            var stackPanel = dockPanel?.Children.OfType<StackPanel>().FirstOrDefault();
            if (stackPanel == null) return;

            if (group.IsExpanded)
            {
                // 折叠：先淡出
                await FadeOut(stackPanel);
                group.IsExpanded = false;
            }
            else
            {
                // 展开：先显示，再淡入
                group.IsExpanded = true;
                stackPanel.Opacity = 0;
                await FadeIn(stackPanel);
            }
        }
    }

    private async Task FadeOut(Control control)
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(100),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Control.OpacityProperty, 1.0) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Control.OpacityProperty, 0.0) }
                }
            }
        };
        await animation.RunAsync(control);
    }

    private async Task FadeIn(Control control)
    {
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(100),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = { new Setter(Control.OpacityProperty, 0.0) }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = { new Setter(Control.OpacityProperty, 1.0) }
                }
            }
        };
        await animation.RunAsync(control);
    }

    private void OnComponentPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is ComponentDefinition definition)
        {
            WhenComponentPressed?.Invoke(this, (definition, e));
        }
    }

    private void OnComponentMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (sender is Border border && border.DataContext is ComponentDefinition definition)
        {
            WhenComponentMoved?.Invoke(this, (definition, e));
        }
    }

    private void OnComponentReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        if (sender is Border border && border.DataContext is ComponentDefinition definition)
        {
            WhenComponentReleased?.Invoke(this, (definition, e));
        }
    }
}
