using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// Grid容器元素 - 使用网格布局的容器
    /// </summary>
    public partial class GridElement : PanelElementBase
    {
        public GridElement() : base()
        {
            ElementType = ComponentType.Grid;
            BackgroundFill.BackgroundColor = "#F5F5F5";
        }

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
        /// 或 "2*,*" 表示第一行是第二行的2倍
        /// Auto 用 "Auto" 表示，固定值用数字如 "100" 表示
        /// 默认空字符串表示平均分配（等同于 "*,*,*"）
        /// </summary>
        [ObservableProperty]
        private string _rowDefinitions = string.Empty;

        /// <summary>
        /// 列定义字符串，格式如 "*,*,*" 表示三列平均分配
        /// 或 "2*,*" 表示第一列是第二列的2倍
        /// Auto 用 "Auto" 表示，固定值用数字如 "100" 表示
        /// 默认空字符串表示平均分配（等同于 "*,*,*"）
        /// </summary>
        [ObservableProperty]
        private string _columnDefinitions = string.Empty;

        public override CanvasElement Clone()
        {
            var element = (GridElement)base.Clone();

            element.Rows = Rows;
            element.Columns = Columns;
            element.RowSpacing = RowSpacing;
            element.ColumnSpacing = ColumnSpacing;
            element.RowDefinitions = RowDefinitions;
            element.ColumnDefinitions = ColumnDefinitions;

            return element;
        }
    }
}
