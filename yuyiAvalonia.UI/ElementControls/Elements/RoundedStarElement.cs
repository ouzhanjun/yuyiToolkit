using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 圆角六星形容器元素（可作为容器，内部放子元素 + 文本）。
    /// 与 yuyiAvalonia.Java.Views.RoundedSixStarElement 算法一致；
    /// 保留"外尖数 PointCount"动态属性，便于在属性面板切换角数。
    ///
    /// 顶点 = 2 × PointCount 个，按 (外尖→内凹) 交替排列、绕中心闭合。
    /// 偶数下标 = 外尖；奇数下标 = 内凹。
    /// 角度步进 = π / PointCount；首顶点由 RotDeg 决定（默认 -90° 即朝上）。
    ///
    /// 为兼容 UI 滑块绑定，保留 R0..R11 便利字段，修改时自动同步到 Radii。
    /// 高于 6 角时（PointCount ≥ 7）通过 Radii 集合维护。
    /// 
    /// 调试显示功能：
    ///   - DrawPoint: 总开关
    ///   - DrawVertex: 显示图形点（顶点）- 蓝色
    ///   - DrawTangent: 显示控制点（切点）- 绿色
    ///   - DrawCenter: 显示圆心 - 橙色
    /// </summary>
    public partial class RoundedStarElement : ShapeElement
    {
        // —— 几何参数 ——
        [ObservableProperty] private double _outerRatio = 0.5;   // 外尖相对 min 边的比例
        [ObservableProperty] private double _innerRatio = 0.4;   // 内凹占外尖的比例
        [ObservableProperty] private double _rotDeg = -90;        // 起始角度（°）

        [ObservableProperty]
        private ObservableCollection<double> _radii = new();

        /// <summary>
        /// 外尖数量（决定顶点总数 = 2 × PointCount）。最小 2，最大 32。
        /// </summary>
        [ObservableProperty] private int _pointCount = 5;

        /// <summary>
        /// 逐顶点圆角半径（长度 = PointCount）。新增位取这个默认值。
        /// </summary>
        [ObservableProperty]
        private double _cornerRadius = 0;

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

        public RoundedStarElement() : base()
        {
            ElementType = ComponentType.Star;
            _cornerRadius = 0;
            Width = 120;
            Height = 120;
            IsContainer = false;
            BackgroundFill.BackgroundColor = "#9C27B0";
            PropertyChanged += OnPropertyChanged;
            PointCount = 5;
            UpdateRadii(PointCount);
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(PointCount):
                    UpdateRadii(PointCount);
                    break;

                case nameof(CornerRadius):
                    var halfR = Math.Min(Width, Height) / 2;

                    if (_cornerRadius >= halfR)
                    {
                        _cornerRadius = halfR;
                    }
                    break;
            }
        }

        /// <summary>完整或部分修改 PointCount，自动扩缩 Radii；
        /// 已有半径保留，新增位用"外尖 12 / 内凹 6"补齐；删除位截断。
        /// 注意：必须先同步 Radii/_radiusList，最后才公开 PointCount，避免外部 Rebuild 看到中间状态。
        /// </summary>
        public void UpdateRadii(int pointCount)
        {
            if (_radii == null)
                return;
            pointCount = Math.Clamp(pointCount, 2, 32);
            if (pointCount == PointCount && _radii.Count == 2 * PointCount)
                return;
            int targetN = 2 * pointCount;
            var keep = new double[targetN];
            for (int i = 0; i < targetN; i++)
            {
                if (i < _radii.Count)
                    keep[i] = _radii[i];
                else
                    keep[i] = _cornerRadius;
            }

            _radii.Clear();
            for (int i = 0; i < targetN; i++)
            {
                _radii.Add(Math.Max(0, keep[i]));
            }

            if (pointCount != PointCount)
            {
                PointCount = pointCount;
            }
        }

        public double[] GetRadii()
        {
            return _radii.ToArray();
        }

        /// <summary>设置 i 位置的圆角（i ∈ [0, 2·PointCount)）。</summary>
        public void SetRadiusAt(int i, double r)
        {
            if (i < 0 || i >= Radii.Count)
                throw new ArgumentOutOfRangeException(nameof(i));
            Radii[i] = Math.Max(0, r);
        }

        public override CanvasElement Clone()
        {
            var c = (RoundedStarElement)base.Clone();

            c._outerRatio = OuterRatio;
            c._innerRatio = InnerRatio;
            c._rotDeg = RotDeg;
            c._pointCount = PointCount;
            c._cornerRadius = CornerRadius;

            // 同步顶点圆角列表（长度须与新的 PointCount 匹配）
            c._radii.Clear();
            for (int i = 0; i < _radii.Count; i++)
                c._radii.Add(_radii[i]);

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