using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 圆角矩形 + Canvas 容器元素。
    /// 继承自 <see cref="RoundedRectangleElement"/>，保留 4 角独立圆角、调试点显示等所有形状能力，
    /// 同时新增 Canvas 容器属性（子元素通过 Left/Top 绝对定位）。
    /// 与 <see cref="RoundedRectangleStackPanelElement"/> 平级，后者按 Orientation/Spacing 排列子元素，
    /// 本类按子元素自身的 Left/Top/Right/Bottom 自由定位。
    /// </summary>
    public partial class RoundedRectangleCanvasElement : RoundedRectangleElement
    {
        public RoundedRectangleCanvasElement() : base()
        {
            // 标识这是一个可容纳子元素的容器（覆盖基类的 IsContainer = false）。
            IsContainer = true;

            // 使用专门的枚举值，便于工厂方法区分。
            ElementType = ComponentType.RoundedRectangleCanvas;
        }

        public override CanvasElement Clone()
        {
            var c = (RoundedRectangleCanvasElement)base.Clone();
            // 父类 Clone 已经复制所有 ObservableProperty（包括 Width/Height/Radii/Padding/CornerRadius/...），
            // 这里无需再额外复制字段。
            return c;
        }
    }
}
