using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;
using System.ComponentModel;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 圆角多边形控件（默认圆角三角形，可动态改 PointCount 在 [3, 32] 之间切换任意边数）。
    /// 算法复用 RoundedStarControl：PointCount 个顶点等距分布在外接圆上，
    /// 每个顶点独立一个圆角半径，用 <see cref="StarGeometry.Build"/> 计算圆弧+切线几何。
    ///
    /// 与星形的区别：星形用 2×PointCount 个交替顶点（外尖/内凹）；
    /// 多边形只用 PointCount 个顶点，因此同样数值的 PointCount 多边形比星形边数多一倍。
    /// 当 RoundedPolygonElement.DrawPoint = true 时叠加顶点/切点/圆心调试标记。
    /// </summary>
    public class RoundedPolygonControl : ShapeFlowControlBase
    {
        private const double IniWidth = 120;
        private const double IniHeight = 120;

        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public RoundedPolygonControl(RoundedPolygonElement element) : base()
        {
            _originRect = new Size(IniWidth, IniHeight);
            _currentElement = element;

            this.Children.Add(_debugOverlay);

            element.PropertyChanged += OnElementPropertyChanged;

            InitialPoints();
            UpdateElement(element);
            UpdateShape(element);
            RedrawDebugOverlay();
        }

        private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RoundedPolygonElement.DrawPoint):
                case nameof(RoundedPolygonElement.DrawVertex):
                case nameof(RoundedPolygonElement.DrawTangent):
                case nameof(RoundedPolygonElement.DrawCenter):
                case nameof(RoundedPolygonElement.DrawVertexLine):
                    RedrawDebugOverlay();
                    break;
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not RoundedPolygonElement el) return;

            var desc = RoundedPathBuilder.Build(
                ActualPoints,
                el.GetRadii(),
                out var centers,
                out var aStart,
                out var aEnd);

            RoundedDebugOverlay.Redraw(
                _debugOverlay,
                ActualPoints,
                el.GetRadii(),
                centers,
                aStart,
                aEnd,
                show: el.DrawPoint,
                showVertex: el.DrawVertex,
                showTangent: el.DrawTangent,
                showCenter: el.DrawCenter,
                drawVertexLines: el.DrawVertexLine);
        }

        public override void InitialPoints()
        {
            if (_currentElement is not RoundedPolygonElement element)
                return;

            double width = _originRect!.Value.Width;
            double height = _originRect!.Value.Height;

            var newPointCount = element.PointCount;
            int n = Math.Max(3, newPointCount);

            var points = new Avalonia.Point[n];
            double cx = width / 2.0, cy = height / 2.0;
            double r = Math.Min(width, height) / 2.0;
            double start = element.RotDeg * Math.PI / 180.0;

            for (int i = 0; i < n; i++)
            {
                double a = start + i * 2.0 * Math.PI / n;
                points[i] = new Avalonia.Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
            }

            OriginalPoints.Clear();
            OriginalPoints.AddRange(points);

            ActualPoints.Clear();
            ActualPoints.AddRange(points);

            base.ResizeActualPoints(ActualPoints.ToArray());
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not RoundedPolygonElement element || ActualPoints.Count < 3)
                return null;

            var radii = element.GetRadii();
            return RoundedPathBuilder.Build(ActualPoints, radii).ToAvaloniaGeometry();
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not RoundedPolygonElement element || ActualPoints.Count < 3)
                return null;

            if (offsetX == 0 && offsetY == 0)
                offsetX = 0.001;

            int n = ActualPoints.Count;
            var newPoints = new Avalonia.Point[n];
            if (offsetX != 0 || offsetY != 0)
            {
                for (int i = 0; i < n; i++)
                    newPoints[i] = new Avalonia.Point(ActualPoints[i].X + offsetX, ActualPoints[i].Y + offsetY);
            }

            var radii = element.GetRadii();
            return RoundedPathBuilder.Build(newPoints, radii).ToAvaloniaGeometry();
        }

        public override void ShowWorkspaceOverlay()
        {
            _isShowOverlay = true;
        }

        public override void HideWorkspaceOverlay()
        {
            _isShowOverlay = false;
        }

        public override void UpdateShape(CanvasElement? shapeElement)
        {
            base.UpdateShape(shapeElement);
            RedrawDebugOverlay();
        }
    }
}
