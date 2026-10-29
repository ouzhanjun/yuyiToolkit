using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{

    /// <summary>
    /// 卡片元素
    /// </summary>
    public partial class CardElement : PanelElementBase
    {
        public CardElement() : base()
        {
            ElementType = ComponentType.Card;
            IsContainer = true;
        }

        [ObservableProperty]
        private double _spacing = 0;

        public override CanvasElement Clone()
        {
            var element = (CardElement)base.Clone();
            element.Spacing = Spacing;
            return element;
        }

    }

}
