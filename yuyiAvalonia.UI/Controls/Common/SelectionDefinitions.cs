using Avalonia;

namespace yuyiAvalonia.UI.Controls
{

    /// <summary>
    /// 多选覆盖层控件
    /// 功能：框选、选中样式显示、多选调整大小
    /// </summary>
    public enum ResizeHandle
    {
        None,
        TopLeft, TopCenter, TopRight,
        MiddleLeft, MiddleRight,
        BottomLeft, BottomCenter, BottomRight
    }

    /// <summary>
    /// 元素在顶级容器中的视觉信息
    /// </summary>
    public readonly struct AxisAlignedElementInfo
    {
        /// <summary>
        /// 轴对齐外包矩形的左上角（未旋转时的位置）
        /// </summary>
        public Point TopLeft { get; init; }

        /// <summary>
        /// 元素中心点在顶级容器中的位置
        /// </summary>
        public Point Center { get; init; }

        /// <summary>
        /// 轴对齐外包矩形的宽度
        /// </summary>
        public double Width { get; init; }

        /// <summary>
        /// 轴对齐外包矩形的高度
        /// </summary>
        public double Height { get; init; }

        /// <summary>
        /// 从元素到顶级容器的累积旋转角（度数）
        /// </summary>
        public double TotalRotation { get; init; }

        public AxisAlignedElementInfo(Point topLeft, Point center, double width, double height, double totalRotation)
        {
            TopLeft = topLeft;
            Center = center;
            Width = width;
            Height = height;
            TotalRotation = totalRotation;
        }
    }

}
