using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Controls;
using yuyiAvalonia.UI.ElementControls;

namespace yuyiAvalonia.UI.Models;

public partial class ElementTreeViewModel : ObservableObject
{
    //private readonly WebContainerManager? _containerManager;

    private IUIControlCreator _controlCreator = null;

    public IUIControlCreator ControlCreator => _controlCreator;

    private readonly Dictionary<CanvasElement, ElementTreeItem> _elementItemMap = new();


    [ObservableProperty]
    private ObservableCollection<CanvasElement> _canvasElements;

    [ObservableProperty]
    private ObservableCollection<ElementTreeItem> _rootItems = new();

    [ObservableProperty]
    private ElementTreeItem? _selectedItem;

    /// <summary>
    /// 多选集合
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ElementTreeItem> _selectedItems = new();

    /// <summary>
    /// 用于 Shift 多选，记录上一次选中的项
    /// </summary>
    private ElementTreeItem? _lastSelectedItem;

    /// <summary>
    /// 所有展平的树项列表（用于 Shift 范围选择）
    /// </summary>
    private List<ElementTreeItem> _flattenedItems = new();

    public event EventHandler<ElementTreeItem>? ElementSelected;
    public event EventHandler<List<CanvasElement>>? ElementsSelectionChanged;

    public ElementTreeViewModel()
    {
    }

    public ElementTreeViewModel(IUIControlCreator controlCreator, ObservableCollection<CanvasElement> canvasElements)
    {
        //_containerManager = containerManager;
        _controlCreator = controlCreator;
        _canvasElements = canvasElements;

        CanvasElements.CollectionChanged += OnCanvasElementsChanged;

        BuildTree();
    }

    private void OnCanvasElementsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (CanvasElement element in e.NewItems)
            {
                AddElementToTree(element);
            }
        }
        else
        {
            RefreshTree();
        }

        if (e.OldItems != null)
        {
            foreach (CanvasElement element in e.OldItems)
            {
                RemoveElementFromTree(element);
            }
        }
    }

    private void AddElementToTree(CanvasElement element)
    {
        if (_elementItemMap.ContainsKey(element)) return;

        element.ParentChanged += OnElementParentChanged;
        SubscribeToChildrenChanges(element);

        var treeItem = new ElementTreeItem(element);
        _elementItemMap[element] = treeItem;

        if (element.Parent == null)
        {
            RootItems.Add(treeItem);
        }
    }

    private void RemoveElementFromTree(CanvasElement element)
    {
        if (!_elementItemMap.TryGetValue(element, out var treeItem)) return;

        element.ParentChanged -= OnElementParentChanged;
        UnsubscribeFromChildrenChanges(element);

        RootItems.Remove(treeItem);
        _elementItemMap.Remove(element);
    }

    private void OnElementParentChanged(object? sender, CanvasElement? newParent)
    {
        if (sender is not CanvasElement element) return;
        if (!_elementItemMap.TryGetValue(element, out var treeItem)) return;

        if (newParent == null)
        {
            if (!RootItems.Contains(treeItem))
                RootItems.Add(treeItem);
        }
        else
        {
            RootItems.Remove(treeItem);
        }
    }

    private void SubscribeToChildrenChanges(CanvasElement element)
    {
        foreach (var child in element.Children)
        {
            if (!_elementItemMap.ContainsKey(child))
            {
                child.ParentChanged += OnChildParentChanged;
                SubscribeToChildrenChanges(child);
            }
        }
    }

    private void UnsubscribeFromChildrenChanges(CanvasElement element)
    {
        foreach (var child in element.Children)
        {
            child.ParentChanged -= OnChildParentChanged;
            UnsubscribeFromChildrenChanges(child);
        }
    }

    private void OnChildParentChanged(object? sender, CanvasElement? newParent)
    {
        if (sender is not CanvasElement child) return;

        if (_elementItemMap.TryGetValue(child, out var childTreeItem))
        {
            if (newParent == null)
            {
                if (!RootItems.Contains(childTreeItem))
                    RootItems.Add(childTreeItem);
            }
            else
            {
                RootItems.Remove(childTreeItem);
            }
        }
    }

    public void BuildTree()
    {
        RootItems.Clear();
        _elementItemMap.Clear();

        foreach (var element in CanvasElements)
        {
            AddElementToTree(element);
        }
    }

    public void RefreshTree()
    {
        BuildTree();
    }

    public void SelectElement(CanvasElement? element)
    {
        // 先清除所有项的选中状态
        ClearAllSelection();
        SelectedItems.Clear();

        if (element == null)
        {
            SelectedItem = null;
            return;
        }
        SelectedItem = FindTreeItem(RootItems, element);
        if (SelectedItem != null)
        {
            SelectedItem.IsSelected = true;
            SelectedItems.Add(SelectedItem);
        }
    }

    /// <summary>
    /// 根据多选列表更新树形视图中的选中状态
    /// </summary>
    public void SelectElements(IList<CanvasElement> elements)
    {
        // 先清除所有项的选中状态
        ClearAllSelection();
        SelectedItems.Clear();

        if (elements == null || elements.Count == 0)
        {
            SelectedItem = null;
            return;
        }

        var elementSet = new HashSet<CanvasElement>(elements);
        foreach (var element in elementSet)
        {
            var item = FindTreeItem(RootItems, element);
            if (item != null)
            {
                item.IsSelected = true;
                SelectedItems.Add(item);
            }
        }
        // SelectedItem 设为列表中第一个元素
        SelectedItem = FindTreeItem(RootItems, elements[0]);
    }

    /// <summary>
    /// 清除所有项的选中状态
    /// </summary>
    private void ClearAllSelection()
    {
        ClearSelectionRecursive(RootItems);
    }

    private void ClearSelectionRecursive(IEnumerable<ElementTreeItem> items)
    {
        foreach (var item in items)
        {
            item.IsSelected = false;
            ClearSelectionRecursive(item.Children);
        }
    }

    private ElementTreeItem? FindTreeItem(IEnumerable<ElementTreeItem> items, CanvasElement element)
    {
        foreach (var item in items)
        {
            if (item.Element == element)
                return item;

            var found = FindTreeItem(item.Children, element);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <summary>
    /// 处理普通点击选中（单选）
    /// </summary>
    public void OnItemTapped(ElementTreeItem item)
    {
        // 记录上一次选中的项
        _lastSelectedItem = item;

        // 更新扁平列表
        RefreshFlattenedItems();

        // 清除所有选中状态
        ClearAllSelection();

        // 设置新选中项
        item.IsSelected = true;
        SelectedItem = item;

        // 更新多选集合
        SelectedItems.Clear();
        SelectedItems.Add(item);

        // 通知外部
        ElementsSelectionChanged?.Invoke(this, new List<CanvasElement> { item.Element });
        ElementSelected?.Invoke(this, item);
    }

    /// <summary>
    /// 处理 Ctrl + 点击（切换选中状态）
    /// </summary>
    public void OnItemCtrlTapped(ElementTreeItem item)
    {
        // 更新扁平列表
        RefreshFlattenedItems();

        // 记录上一次选中的项
        _lastSelectedItem = item;

        // 切换该项的选中状态
        if (item.IsSelected)
        {
            // 取消选中
            item.IsSelected = false;
            SelectedItems.Remove(item);
        }
        else
        {
            // 添加选中
            item.IsSelected = true;
            if (!SelectedItems.Contains(item))
                SelectedItems.Add(item);
        }

        // 更新 SelectedItem（保持一个主要选中项）
        if (SelectedItems.Count > 0)
        {
            SelectedItem = SelectedItems.LastOrDefault();
        }
        else
        {
            SelectedItem = null;
        }

        // 通知外部
        ElementsSelectionChanged?.Invoke(this, SelectedItems.Select(x => x.Element).ToList());
    }

    /// <summary>
    /// 处理 Shift + 点击（范围选择）
    /// </summary>
    public void OnItemShiftTapped(ElementTreeItem item)
    {
        // 更新扁平列表
        RefreshFlattenedItems();

        if (_lastSelectedItem == null || _flattenedItems.Count == 0)
        {
            // 没有上一次选中的项，执行单选
            OnItemTapped(item);
            return;
        }

        // 找到起始和结束索引
        int startIndex = _flattenedItems.IndexOf(_lastSelectedItem);
        int endIndex = _flattenedItems.IndexOf(item);

        if (startIndex < 0 || endIndex < 0)
        {
            // 索引无效，执行单选
            OnItemTapped(item);
            return;
        }

        // 确保 startIndex < endIndex
        if (startIndex > endIndex)
        {
            (startIndex, endIndex) = (endIndex, startIndex);
        }

        // 清除所有选中状态
        ClearAllSelection();

        // 选中范围内的所有项
        for (int i = startIndex; i <= endIndex; i++)
        {
            var currentItem = _flattenedItems[i];
            currentItem.IsSelected = true;
            if (!SelectedItems.Contains(currentItem))
                SelectedItems.Add(currentItem);
        }

        // 更新 SelectedItem
        SelectedItem = item;

        // 通知外部
        ElementsSelectionChanged?.Invoke(this, SelectedItems.Select(x => x.Element).ToList());
    }

    /// <summary>
    /// 刷新扁平化的项列表
    /// </summary>
    private void RefreshFlattenedItems()
    {
        _flattenedItems.Clear();
        CollectFlattenedItems(RootItems);
    }

    private void CollectFlattenedItems(IEnumerable<ElementTreeItem> items)
    {
        foreach (var item in items)
        {
            _flattenedItems.Add(item);
            if (item.IsExpanded && item.Children.Count > 0)
            {
                CollectFlattenedItems(item.Children);
            }
        }
    }

    public void ToggleVisibility(ElementTreeItem item)
    {
        if (item == null ) return;

        var newVisibility = !item.Element.IsVisible;
        item.Element.IsVisible = newVisibility;
        item.RefreshVisibility();

        if (ControlCreator.TryGetControl(item.Element, out var control))
        {
            control.IsVisible = newVisibility;
        }

        if (item.Element.Children.Count > 0)
        {
            UpdateChildrenVisibility(item.Element, newVisibility);
        }
    }

    private void UpdateChildrenVisibility(CanvasElement parent, bool visible)
    {
        foreach (var child in parent.Children)
        {
            child.IsVisible = visible;
            if (ControlCreator.TryGetControl(child, out var control))
            {
                control!.IsVisible = visible;
            }
            if (child.Children.Count > 0)
            {
                UpdateChildrenVisibility(child, visible);
            }
        }
    }
}
