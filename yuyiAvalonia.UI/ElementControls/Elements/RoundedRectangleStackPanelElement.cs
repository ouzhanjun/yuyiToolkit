using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 圆角矩形 + StackPanel 容器元素。
    /// 继承自 <see cref="RoundedRectangleElement"/>，保留 4 角独立圆角、调试点显示等所有形状能力，
    /// 同时新增 StackPanel 容器属性（<see cref="Orientation"/>、<see cref="Spacing"/>），
    /// 用于在圆角矩形内按 StackPanel 规则堆叠子元素。
    /// </summary>
    public partial class RoundedRectangleStackPanelElement : RoundedRectangleElement
    {
        /// <summary>子元素方向（Vertical / Horizontal）。</summary>
        [ObservableProperty]
        private StackPanelOrientation _orientation = StackPanelOrientation.Vertical;

        /// <summary>子元素间距（垂直/水平方向共用，与 Orientation 联动）。</summary>
        [ObservableProperty]
        private double _spacing = 0;

        /// <summary>水平子元素间距（仅在 Horizontal 方向生效，独立于 <see cref="Spacing"/>）。</summary>
        [ObservableProperty]
        private double _hSpacing = 0;

        /// <summary>垂直子元素间距（仅在 Vertical 方向生效，独立于 <see cref="Spacing"/>）。</summary>
        [ObservableProperty]
        private double _vSpacing = 0;

        public RoundedRectangleStackPanelElement() : base()
        {
            // 标识这是一个可容纳子元素的容器（覆盖基类的 IsContainer = false）。
            IsContainer = true;

            // 使用专门的枚举值，便于工厂方法区分。
            ElementType = ComponentType.RoundedRectangleStackPanel;
        }

        public override CanvasElement Clone()
        {
            var c = (RoundedRectangleStackPanelElement)base.Clone();
            c.Orientation = Orientation;
            c.Spacing = Spacing;
            c.HSpacing = HSpacing;
            c.VSpacing = VSpacing;
            return c;
        }
    }
}
