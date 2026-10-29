using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{

    /// <summary>
    /// 椭圆 / 弧 / 扇形 元素（Figma 风格参数化）。
    /// 通过 StartAngle / SweepAngle / InnerRadius 决定形态：
    ///   InnerRadius = 0, SweepAngle = 360  → 实心椭圆
    ///   InnerRadius = 0, SweepAngle <  360  → 扇形 (pie)
    ///   InnerRadius > 0, SweepAngle = 360  → 椭圆环 (donut)
    ///   InnerRadius > 0, SweepAngle <  360  → 扇环 (ring sector)
    /// 默认 SweepAngle = 360 / InnerRadius = 0 表现为普通椭圆。
    /// 
    /// 调试显示功能：
    ///   - DrawPoint: 总开关
    ///   - DrawVertex: 显示图形点（顶点）- 蓝色
    ///   - DrawTangent: 显示控制点（切点）- 绿色
    ///   - DrawCenter: 显示圆心 - 橙色
    /// </summary>
    public partial class RoundEllipseElement : ShapeElement
    {
        [ObservableProperty]
        private double _startAngle = 0;     // deg, 0 = +x, CCW

        [ObservableProperty]
        private double _sweepAngle = 360;   // deg, [0, 360]

        [ObservableProperty]
        private double _innerRadius = 0;    // [0, 1)

        [ObservableProperty]
        private double _cornerRadius = 0;   // 尖角圆角半径（像素）

        // —— 调试显示控制 ——

        /// <summary>是否显示调试点（总开关）</summary>
        [ObservableProperty]
        private bool _drawPoint = false;

        /// <summary>是否显示图形点（顶点）</summary>
        [ObservableProperty]
        private bool _drawVertex = true;

        /// <summary>是否显示控制点（切点）</summary>
        [ObservableProperty]
        private bool _drawTangent = true;

        /// <summary>是否显示圆心</summary>
        [ObservableProperty]
        private bool _drawCenter = true;

        /// <summary>是否用虚线连接图形点</summary>
        [ObservableProperty]
        private bool _drawVertexLine = false;

        public RoundEllipseElement() : base()
        {
            ElementType = ComponentType.Sector;
            IsContainer = true;
            BackgroundFill.BackgroundColor = "#7B61FF";
            Width = 120;
            Height = 120;
        }

        public override CanvasElement Clone()
        {
            var element = (RoundEllipseElement)base.Clone();
            element.StartAngle = StartAngle;
            element.SweepAngle = SweepAngle;
            element.InnerRadius = InnerRadius;
            element.CornerRadius = CornerRadius;

            // 调试显示属性
            element._drawPoint = DrawPoint;
            element._drawVertex = DrawVertex;
            element._drawTangent = DrawTangent;
            element._drawCenter = DrawCenter;
            element._drawVertexLine = DrawVertexLine;

            return element;
        }
    }
}
