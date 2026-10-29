using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 圆角矩形容器元素 - 可在圆角矩形内放置内容。
    /// 4 个顶点（左上 → 右上 → 右下 → 左下），每个顶点独立圆角 R0..R3。
    /// 0 = 尖角。算法与 RoundedRectangleGeometry（夹角平分线 + uIn×uOut 局部 sweep）一致。
    /// CornerRadius 是统一联动属性：4 个角全部相等时显示具体值，否则为 NaN（Mixed）。
    /// 
    /// 调试显示功能：
    ///   - DrawPoint: 总开关
    ///   - DrawVertex: 显示图形点（顶点）- 蓝色
    ///   - DrawTangent: 显示控制点（切点）- 绿色
    ///   - DrawCenter: 显示圆心 - 橙色
    /// </summary>
    public partial class RoundedRectangleElement : ShapeElement
    {
        [ObservableProperty]
        private ObservableCollection<double> _radii = new();

        /// <summary>
        /// 统一圆角（联动 4 个角）。4 角全等时为该值，否则为 NaN（Mixed 状态）。
        /// 设置此值会同步 R0..R3。
        /// </summary>
        [ObservableProperty]
        private double _cornerRadius = 8;

        [ObservableProperty] private double _r0 = 8;   // 左上
        [ObservableProperty] private double _r1 = 8;   // 右上
        [ObservableProperty] private double _r2 = 8;   // 右下
        [ObservableProperty] private double _r3 = 8;   // 左下

        /// <summary>角半径是否为统一模式（UI 编辑器用）。true=统一模式，false=分开模式。</summary>
        [ObservableProperty]
        private bool _cornerRadiusIsUniform = true;

        /// <summary>Mixed 占位符值，表示 4 角不一致。</summary>
        public const double MixedValue = double.NaN;

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

        public RoundedRectangleElement() : base()
        {
            ElementType = ComponentType.RoundedRectangle;
            IsContainer = false;
            CornerRadius = 8;
            Width = 120;
            Height = 120;
            BackgroundFill.BackgroundColor = "#4ECDC4";
            InitRadii();
            this.PropertyChanged += RoundedRectangleElement_PropertyChanged;
        }

        private void RoundedRectangleElement_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ElementControls.Elements.RoundedRectangleElement.R0):
                    _radii[0] = this.R0;
                    RefreshCornerRadiusFromCorners();
                    break;
                case nameof(ElementControls.Elements.RoundedRectangleElement.R1):
                    _radii[1] = this.R1;
                    RefreshCornerRadiusFromCorners();
                    break;
                case nameof(ElementControls.Elements.RoundedRectangleElement.R2):
                    _radii[2] = this.R2;
                    RefreshCornerRadiusFromCorners();
                    break;
                case nameof(ElementControls.Elements.RoundedRectangleElement.R3):
                    _radii[3] = this.R3;
                    RefreshCornerRadiusFromCorners();
                    break;
                case nameof(ElementControls.Elements.RoundedRectangleElement.CornerRadius):
                    ApplyCornerRadiusToCorners();
                    break;
                case nameof(ElementControls.Elements.RoundedRectangleElement.CornerRadiusIsUniform):
                    if (CornerRadiusIsUniform && double.IsNaN(CornerRadius))
                        CornerRadius = R0;
                    break;
            }
        }

        /// <summary>
        /// 根据 4 角是否相等，重算 CornerRadius（相等 = 该值，不等 = NaN）。
        /// 不触发 ApplyCornerRadiusToCorners，避免循环。
        /// </summary>
        private void RefreshCornerRadiusFromCorners()
        {
            // 4 角全等则赋具体值（用 R0 作代表），否则 NaN
            double unified = R0 == R1 && R1 == R2 && R2 == R3 ? R0 : MixedValue;

            if (_isSyncingCornerRadius) return;

            if (double.IsNaN(CornerRadius) && double.IsNaN(unified)) return;
            if (CornerRadius == unified) return;

            _isSyncingCornerRadius = true;
            CornerRadius = unified;
            _isSyncingCornerRadius = false;
        }

        private bool _isSyncingCornerRadius;

        /// <summary>
        /// CornerRadius 写入后，同步到 R0..R3。
        /// </summary>
        private void ApplyCornerRadiusToCorners()
        {
            if (double.IsNaN(CornerRadius)) return;

            double r = Math.Max(0, CornerRadius);
            if (R0 == r && R1 == r && R2 == r && R3 == r) return;

            R0 = r;
            R1 = r;
            R2 = r;
            R3 = r;
            CornerRadiusIsUniform = true;
        }

        private void InitRadii()
        {
            if (_radii != null)
            {
                _radii.Add(Math.Max(0, R0));
                _radii.Add(Math.Max(0, R1));
                _radii.Add(Math.Max(0, R2));
                _radii.Add(Math.Max(0, R3));
            }
        }

        /// <summary>设置 4 个角的圆角（同时同步到 R0..R3）。</summary>
        public void SetAllCorners(double r)
        {
            r = Math.Max(0, r);
            CornerRadius = r;
        }

        /// <summary>返回 4 个顶点的逐顶点半径序列（长度 = 4）。</summary>
        public double[] GetRadii()
        {
            return _radii.ToArray();
        }

        public override CanvasElement Clone()
        {
            var c = (RoundedRectangleElement)base.Clone();
            c.R0 = R0;
            c.R1 = R1;
            c.R2 = R2;
            c.R3 = R3;
            c.CornerRadius = CornerRadius;

            // 调试显示属性
            c._drawPoint = DrawPoint;
            c._drawVertex = DrawVertex;
            c._drawTangent = DrawTangent;
            c._drawCenter = DrawCenter;
            c._drawVertexLine = DrawVertexLine;

            return c;
        }
    }
}