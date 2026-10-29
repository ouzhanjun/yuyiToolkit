using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 表示一个可作为容器的画布面板元素，用于在画布上承载和组织其他元素。支持自定义边框、圆角和阴影效果。
    /// </summary>
    /// <remarks>CanvasPanelElement 适用于需要分组、布局或装饰子元素的场景。作为容器元素，可以通过相关属性自定义其外观，包括边框颜色、宽度、圆角半径以及阴影样式。</remarks>
    public partial class CanvasPanelElement : CanvasElement
    {
        public CanvasPanelElement() : base()
        {
            ElementType = ComponentType.Canvas;
            IsContainer = true;
        }

        public override CanvasElement Clone()
        {
            var element = (CanvasPanelElement)base.Clone();
            return element;
        }
    }
}
