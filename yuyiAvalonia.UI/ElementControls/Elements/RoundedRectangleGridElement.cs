using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 圆角矩形 + Grid 容器元素。
    /// 继承自 <see cref="RoundedRectangleElement"/>，保留 4 角独立圆角、调试点显示等所有形状能力，
    /// 同时新增 Grid 容器属性（<see cref="Rows"/>、<see cref="Columns"/>、<see cref="RowSpacing"/>、
    /// <see cref="ColumnSpacing"/>、<see cref="RowDefinitions"/>、<see cref="ColumnDefinitions"/>）。
    /// 子元素按 Row/Column 落入对应单元格，由内部 Grid 自动布局。
    /// </summary>
    public partial class RoundedRectangleGridElement : RoundedRectangleElement
    {
        [ObservableProperty]
        private int _rows = 3;

        [ObservableProperty]
        private int _columns = 3;

        [ObservableProperty]
        private double _rowSpacing = 5;

        [ObservableProperty]
        private double _columnSpacing = 5;

        /// <summary>
        /// 行定义字符串，格式如 "*,*,*" 表示三行平均分配
        /// 或 "2*,*" 表示第一行是第二行的 2 倍
        /// Auto 用 "Auto" 表示，固定值用数字如 "100" 表示
        /// 默认空字符串表示平均分配。
        /// </summary>
        [ObservableProperty]
        private string _rowDefinitions = string.Empty;

        /// <summary>
        /// 列定义字符串，格式与 <see cref="RowDefinitions"/> 相同。
        /// </summary>
        [ObservableProperty]
        private string _columnDefinitions = string.Empty;

        public RoundedRectangleGridElement() : base()
        {
            // 标识这是一个可容纳子元素的容器（覆盖基类的 IsContainer = false）。
            IsContainer = true;

            // 使用专门的枚举值，便于工厂方法区分。
            ElementType = ComponentType.RoundedRectangleGrid;
        }

        public override CanvasElement Clone()
        {
            var c = (RoundedRectangleGridElement)base.Clone();
            c.Rows = Rows;
            c.Columns = Columns;
            c.RowSpacing = RowSpacing;
            c.ColumnSpacing = ColumnSpacing;
            c.RowDefinitions = RowDefinitions;
            c.ColumnDefinitions = ColumnDefinitions;
            return c;
        }
    }
}
