using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class ElementTreeItem : ObservableObject
{
    [ObservableProperty]
    private CanvasElement _element = null!;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isSelected = false;

    public ObservableCollection<ElementTreeItem> Children { get; } = new();

    public string DisplayName => string.IsNullOrEmpty(Element.Name)
        ? ComponentTypeHelper.GetTypeDisplayName(Element.ElementType)
        : Element.Name;

    public string TypeIcon => ComponentTypeHelper.GetTypeIcon(Element.ElementType);

    public string ElementId => Element.Id;

    public ElementTreeItem(CanvasElement element)
    {
        Element = element;
        IsVisible = element.IsVisible;

        element.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CanvasElement.IsVisible))
            {
                IsVisible = element.IsVisible;
            }
            if (e.PropertyName == nameof(CanvasElement.Name))
            {
                OnPropertyChanged(nameof(DisplayName));
            }
        };

        foreach (var child in element.Children)
        {
            Children.Add(new ElementTreeItem(child));
        }

        element.Children.CollectionChanged += (s, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (CanvasElement child in e.NewItems)
                {
                    Children.Add(new ElementTreeItem(child));
                }
            }
            if (e.OldItems != null)
            {
                foreach (CanvasElement child in e.OldItems)
                {
                    var item = Children.FirstOrDefault(c => c.Element == child);
                    if (item != null)
                    {
                        Children.Remove(item);
                    }
                }
            }
        };
    }

    public void RefreshVisibility()
    {
        IsVisible = Element.IsVisible;
    }

    public void RefreshChildren()
    {
        Children.Clear();
        foreach (var child in Element.Children)
        {
            Children.Add(new ElementTreeItem(child));
        }
    }

}
