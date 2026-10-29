using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class Layout : UserControl
{
    public static readonly StyledProperty<CanvasElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<Layout, CanvasElement?>(
            nameof(ElementProperty),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

    public CanvasElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    private Control? parentContainer = null;

    public Layout()
    {
        InitializeComponent();

        this.AttachedToVisualTree += Layout_AttachedToVisualTree;

        ElementPropertyProperty.Changed.AddClassHandler<Layout>((sender, e) =>
        {
            PART_Position.IsVisible = false;
            PART_Alignment.IsVisible = false;
            PART_Margin.IsVisible = false;
            PART_Padding.IsVisible = false;

            if (e.OldValue != null && e.OldValue is CanvasElement oldElement)
            {
                oldElement.PropertyChanged -= Element_PropertyChanged;
            }

            if (e.NewValue != null && e.NewValue is CanvasElement newElement)
            {
                newElement.PropertyChanged += Element_PropertyChanged;

                if (newElement.Parent is CanvasPanelElement)
                {
                    PART_Position.IsVisible = true;
                }
                else if (newElement.Parent is StackPanelElement)
                {
                    PART_Margin.IsVisible = true;
                    PART_Alignment.IsVisible = true;
                }
                else if (newElement is StackPanelElement)
                {
                    PART_Padding.IsVisible = true;
                }

                if (parentContainer != null)
                {
                    parentContainer.IsVisible = PART_Position.IsVisible || PART_Padding.IsVisible || PART_Margin.IsVisible || PART_Alignment.IsVisible;
                }
            }
        });

    }

    private void Layout_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (((Control)this.Parent!).Parent != null)
        {
            parentContainer = ((Control)((Control)this.Parent!).Parent);

            if (ElementProperty != null && ElementProperty is CanvasElement element)
            {
                if (element.Parent is CanvasPanelElement)
                {
                    PART_Position.IsVisible = true;
                }
                else if (element.Parent is StackPanelElement)
                {
                    PART_Margin.IsVisible = true;
                    PART_Alignment.IsVisible = true;
                }
                else if (element is StackPanelElement)
                {
                    PART_Padding.IsVisible = true;
                }

                if (parentContainer != null)
                {
                    parentContainer.IsVisible = PART_Position.IsVisible || PART_Padding.IsVisible || PART_Margin.IsVisible || PART_Alignment.IsVisible;
                }
            }
        }
    }

    private void Element_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CanvasElement.Parent):
                PART_Position.IsVisible = false;
                PART_Alignment.IsVisible = false;
                PART_Margin.IsVisible = false;
                PART_Padding.IsVisible = false;

                if (ElementProperty != null && ElementProperty is CanvasElement)
                {
                    if (ElementProperty.Parent is CanvasPanelElement)
                    {
                        PART_Position.IsVisible = true;
                    }
                    else if (ElementProperty.Parent is StackPanelElement)
                    {
                        PART_Margin.IsVisible = true;
                        PART_Alignment.IsVisible = true;
                    }
                    else if (ElementProperty.Parent is StackPanelElement)
                    {
                        PART_Padding.IsVisible = true;
                    }
                }

                if (parentContainer != null)
                {
                    parentContainer.IsVisible = PART_Position.IsVisible || PART_Padding.IsVisible || PART_Margin.IsVisible || PART_Alignment.IsVisible;
                }
                break;
        }
    }
}