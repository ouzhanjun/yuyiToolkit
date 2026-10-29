using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// Panel容器元素
    /// </summary>
    public partial class PanelElement : PanelElementBase
    {
        public PanelElement() : base()
        {
            ElementType = ComponentType.Panel;
            Children = new ObservableCollection<CanvasElement>();
            IsContainer = true;
        }

        public override CanvasElement Clone()
        {
            var element = (PanelElement)base.Clone();
            return element;
        }
    }
}
