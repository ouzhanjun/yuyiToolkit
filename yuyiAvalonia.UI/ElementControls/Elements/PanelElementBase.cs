using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{

    /// <summary>
    /// Panel容器元素
    /// </summary>
    public partial class PanelElementBase : CanvasElement
    {
        public PanelElementBase() : base()
        {
            ElementType = ComponentType.Panel;
            Children = new ObservableCollection<CanvasElement>();
            IsContainer = true;
        }

    }
}
