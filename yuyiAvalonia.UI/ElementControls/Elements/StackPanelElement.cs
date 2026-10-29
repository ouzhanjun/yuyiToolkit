using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{

    /// <summary>
    /// StackPanel容器元素
    /// </summary>
    public partial class StackPanelElement : PanelElementBase
    {
        public StackPanelElement() : base()
        {
            ElementType = ComponentType.VerticalStackPanel;
        }

        [ObservableProperty]
        private StackPanelOrientation _orientation = StackPanelOrientation.Vertical;

        [ObservableProperty]
        private double _spacing = 0;

        public override CanvasElement Clone()
        {
            var element = (StackPanelElement)base.Clone();

            element.Orientation = Orientation;
            element.Spacing = Spacing;

            return element;
        }

    }

}
