using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    
    /// <summary>
    /// 三角形容器元素 - 可在三角形内放置内容
    /// </summary>
    public partial class TriangleContainerElement : ShapeElement
    {
        /// <summary>
        /// 顶部点X偏移（相对于宽度的比例，0-1）
        /// </summary>
        [ObservableProperty]
        private double _topPointOffsetX = 0.5;

        /// <summary>
        /// 顶部点Y偏移（相对于高度的比例，0-1）
        /// </summary>
        [ObservableProperty]
        private double _topPointOffsetY = 0.0;

        /// <summary>
        /// 左下点X偏移（相对于宽度的比例，0-1）
        /// </summary>
        [ObservableProperty]
        private double _bottomLeftOffsetX = 0.0;

        /// <summary>
        /// 左下点Y偏移（相对于高度的比例，0-1）
        /// </summary>
        [ObservableProperty]
        private double _bottomLeftOffsetY = 1.0;

        /// <summary>
        /// 右下点X偏移（相对于宽度的比例，0-1）
        /// </summary>
        [ObservableProperty]
        private double _bottomRightOffsetX = 1.0;

        /// <summary>
        /// 右下点Y偏移（相对于高度的比例，0-1）
        /// </summary>
        [ObservableProperty]
        private double _bottomRightOffsetY = 1.0;

        public TriangleContainerElement() : base()
        {
            ElementType = ComponentType.Triangle;
            IsContainer = true;
            BackgroundFill.BackgroundColor = "#FF6B6B";

            // 三角形控制点（与 TriangleElement 保持一致）
            TopPointOffsetX = 0.5;
            TopPointOffsetY = 0.0;
            BottomLeftOffsetX = 0.0;
            BottomLeftOffsetY = 1.0;
            BottomRightOffsetX = 1.0;
            BottomRightOffsetY = 1.0;
        }

        public override CanvasElement Clone()
        {
            var element = (TriangleContainerElement)base.Clone();
            element.TopPointOffsetX = TopPointOffsetX;
            element.TopPointOffsetY = TopPointOffsetY;
            element.BottomLeftOffsetX = BottomLeftOffsetX;
            element.BottomLeftOffsetY = BottomLeftOffsetY;
            element.BottomRightOffsetX = BottomRightOffsetX;
            element.BottomRightOffsetY = BottomRightOffsetY;
            return element;
        }
    }

    /// <summary>
    /// 矩形容器元素 - 可在矩形内放置内容
    /// </summary>
    public partial class RectangleContainerElement : ShapeElement
    {
        public RectangleContainerElement() : base()
        {
            ElementType = ComponentType.Rectangle;
            IsContainer = true;
            BackgroundFill.BackgroundColor = "#4ECDC4";
        }

        public override CanvasElement Clone()
        {
            return (RectangleContainerElement)base.Clone();
        }
    }

    /// <summary>
    /// 椭圆容器元素 - 可在椭圆内放置内容
    /// </summary>
    public partial class EllipseContainerElement : ShapeElement
    {
        public EllipseContainerElement() : base()
        {
            ElementType = ComponentType.Ellipse;
            IsContainer = true;
            BackgroundFill.BackgroundColor = "#45B7D1";
        }

        public override CanvasElement Clone()
        {
            return (EllipseContainerElement)base.Clone();
        }
    }

    /// <summary>
    /// 平行四边形容器元素 - 可在平行四边形内放置内容
    /// </summary>
    public partial class ParallelogramContainerElement : ShapeElement
    {
        /// <summary>
        /// 水平倾斜比例（-1 到 1，正值向右倾斜，负值向左倾斜）
        /// </summary>
        [ObservableProperty]
        private double _skewX = 0.0;

        public ParallelogramContainerElement() : base()
        {
            ElementType = ComponentType.Parallelogram;
            IsContainer = true;
            BackgroundFill.BackgroundColor = "#96CEB4";

            // 平行四边形默认倾斜角度
            SkewX = 0.15;
        }

        public override CanvasElement Clone()
        {
            var element = (ParallelogramContainerElement)base.Clone();
            element.SkewX = SkewX;
            return element;
        }
    }
}