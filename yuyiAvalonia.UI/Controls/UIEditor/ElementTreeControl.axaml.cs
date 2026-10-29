using Avalonia.Controls;
using Avalonia.Interactivity;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class ElementTreeControl : UserControl
{
    public ElementTreeControl()
    {
        InitializeComponent();
    }

    public ElementTreeControl(ElementTreeViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    private void OnItemSelected(object? sender, ElementTreeItem item)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            vm.OnItemTapped(item);
        }
    }

    private void OnItemCtrlSelected(object? sender, ElementTreeItem item)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            vm.OnItemCtrlTapped(item);
        }
    }

    private void OnItemShiftSelected(object? sender, ElementTreeItem item)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            vm.OnItemShiftTapped(item);
        }
    }

    private void OnVisibilityToggled(object? sender, ElementTreeItem item)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            vm.ToggleVisibility(item);
        }
    }

    private void ExpandAll_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            SetExpandedRecursive(vm.RootItems, true);
        }
    }

    private void CollapseAll_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            SetExpandedRecursive(vm.RootItems, false);
        }
    }

    private void Refresh_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ElementTreeViewModel vm)
        {
            vm.RefreshTree();
        }
    }

    private void SetExpandedRecursive(IEnumerable<ElementTreeItem>? items, bool expanded)
    {
        if (items == null) return;

        foreach (var item in items)
        {
            item.IsExpanded = expanded;
            SetExpandedRecursive(item.Children, expanded);
        }
    }
}
