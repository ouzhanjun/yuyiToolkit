using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 圆角多边形元素（默认圆角三角形，可动态改 PointCount 在 [3, 32] 之间切换任意边数）。
    ///
    /// 与 RoundedStarElement 的区别：星形是 2×PointCount 个交替顶点（外尖/内凹），
    /// 多边形只有 PointCount 个顶点，全部在以中心为基准的同一外接圆上等距分布。
    /// 角度步进 = 2π / PointCount；首顶点由 RotDeg 决定（默认 -90° 即朝上）。
    ///
    /// 每个顶点独立配一个圆角半径（Radii[i]），复用 RoundedStarElement 的
    /// "扩缩容时已有半径保留，新增位用 Radius 字段补齐"算法。
    /// 
    /// 调试显示功能：
    ///   - DrawPoint: 总开关
    ///   - DrawVertex: 显示图形点（顶点）- 蓝色
    ///   - DrawTangent: 显示控制点（切点）- 绿色
    ///   - DrawCenter: 显示圆心 - 橙色
    /// </summary>
    public partial class RoundedPolygonElement : ShapeElement
    {
        // —— 几何参数 ——

        /// <summary>起始角度（°）。默认 -90° 即首个顶点在正上方。</summary>
        [ObservableProperty] private double _rotDeg = -90;

        /// <summary>
        /// 顶点数 = 多边形边数。最小 3，最大 32。
        /// </summary>
        [ObservableProperty] private int _pointCount = 3;

        /// <summary>
        /// 逐顶点圆角半径（长度 = PointCount）。新增位取这个默认值。
        /// </summary>
        [ObservableProperty]
        private double _cornerRadius = 0;

        /// <summary>逐顶点圆角半径集合，长度随 PointCount 自动维护。</summary>
        [ObservableProperty]
        private ObservableCollection<double> _radii = new();

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

        public RoundedPolygonElement() : base()
        {
            ElementType = ComponentType.RoundedPolygon;
            IsContainer = true;
            Width = 120;
            Height = 120;
            _cornerRadius = 0;
            BackgroundFill.BackgroundColor = "#FF9F43";
            PropertyChanged += OnPropertyChanged;
            PointCount = 3;
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
                    //for (int i = 0; i < _radii.Count(); i++)
                    //{
                    //    _radii[i] = _cornerRadius;
                    //}
                    break;

            }
        }

        /// <summary>
        /// 完整或部分修改 PointCount，自动扩缩 Radii；
        /// 已有半径保留，新增位用 Radius 字段补齐。
        /// 与 RoundedStarElement 的 UpdateRadii 行为一致。
        /// </summary>
        public void UpdateRadii(int pointCount)
        {
            if (_radii == null)
                return;
            pointCount = Math.Clamp(pointCount, 3, 32);
            if (pointCount == PointCount && _radii.Count == PointCount)
                return;

            var keep = new double[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                if (i < _radii.Count) 
                    keep[i] = _radii[i];
                else                 
                    keep[i] = _cornerRadius;
            }

            _radii.Clear();
            for (int i = 0; i < pointCount; i++)
                _radii.Add(Math.Max(0, keep[i]));

            if (pointCount != PointCount)
                PointCount = pointCount;
        }

        /// <summary>返回逐顶点半径序列（长度 = PointCount）。</summary>
        public double[] GetRadii() => _radii.ToArray();

        /// <summary>设置 i 位置的圆角（i ∈ [0, PointCount)）。</summary>
        public void SetRadiusAt(int i, double r)
        {
            if (i < 0 || i >= Radii.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(i));
            }
            var halfR = Math.Min(Width, Height) / 2;

            if (r >= halfR)
            {
                r = halfR;
            }
            Radii[i] = Math.Max(0, r);
        }

        public override CanvasElement Clone()
        {
            var c = (RoundedPolygonElement)base.Clone();

            c._rotDeg = RotDeg;
            c._pointCount = PointCount;
            c._cornerRadius = CornerRadius;

            c._radii.Clear();
            for (int i = 0; i < _radii.Count; i++)
                c._radii.Add(_radii[i]);

            // 调试显示属性
            c._drawPoint = DrawPoint;
            c._drawVertex = DrawVertex;
            c._drawTangent = DrawTangent;
            c._drawCenter = DrawCenter;
            c._drawVertexLine = DrawVertexLine;

            // 走 backing field 不触发 OnPropertyChanged；
            // 这里不需要像 Star 那样重算 OriginalPoints（OriginalPoints 由控件侧维护）。
            return c;
        }
    }
}
