using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace yuyiAvalonia.UI.Controls;

public partial class ElementTreeNode : UserControl
{
    public event EventHandler<ElementTreeItem>? ItemSelected;
    public event EventHandler<ElementTreeItem>? ItemCtrlSelected;
    public event EventHandler<ElementTreeItem>? ItemShiftSelected;
    public event EventHandler<ElementTreeItem>? VisibilityToggled;

    public ElementTreeItem? Item => DataContext as ElementTreeItem;

    public ElementTreeNode()
    {
        InitializeComponent();
    }

    private void OnItemPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Item == null) return;

        var properties = e.GetCurrentPoint(this).Properties;
        var isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var isShiftPressed = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (properties.IsLeftButtonPressed)
        {
            if (isCtrlPressed)
            {
                // Ctrl + 点击：切换选中状态
                ItemCtrlSelected?.Invoke(this, Item);
            }
            else if (isShiftPressed)
            {
                // Shift + 点击：范围选择
                ItemShiftSelected?.Invoke(this, Item);
            }
            else
            {
                // 普通点击：单选
                ItemSelected?.Invoke(this, Item);
            }
        }
    }

    private void ToggleExpand_Click(object? sender, RoutedEventArgs e)
    {
        if (Item != null)
        {
            Item.IsExpanded = !Item.IsExpanded;
        }
    }

    private void OnVisibilityToggle_Click(object? sender, RoutedEventArgs e)
    {
        if (Item != null)
        {
            VisibilityToggled?.Invoke(this, Item);
        }
    }

    private void OnChildItemSelected(object? sender, ElementTreeItem item)
    {
        ItemSelected?.Invoke(sender, item);
    }

    private void OnChildItemCtrlSelected(object? sender, ElementTreeItem item)
    {
        ItemCtrlSelected?.Invoke(sender, item);
    }

    private void OnChildItemShiftSelected(object? sender, ElementTreeItem item)
    {
        ItemShiftSelected?.Invoke(sender, item);
    }

    private void OnChildVisibilityToggled(object? sender, ElementTreeItem item)
    {
        VisibilityToggled?.Invoke(sender, item);
    }
}
