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
    /// 六星形容器控件（默认 6 外尖，可动态改 PointCount 在 [2, 32] 之间切换任意角数）。
    /// 算法与 RoundedSixStarGeometry 完全一致（夹角平分线 + uIn×uOut 局部 sweep）。
    /// 当 RoundedStarElement.DrawPoint = true 时叠加顶点/切点/圆心调试标记。
    /// </summary>
    public class RoundedStarControl : ShapeFlowControlBase
    {
        private const double IniWidth = 140;
        private const double IniHeight = 140;

        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false,
        };

        public RoundedStarControl(RoundedStarElement element) : base()
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
                case nameof(RoundedStarElement.DrawPoint):
                case nameof(RoundedStarElement.DrawVertex):
                case nameof(RoundedStarElement.DrawTangent):
                case nameof(RoundedStarElement.DrawCenter):
                case nameof(RoundedStarElement.DrawVertexLine):
                    RedrawDebugOverlay();
                    break;
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not RoundedStarElement el) return;

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
            if (_currentElement is not RoundedStarElement element)
                return;

            double width = _originRect!.Value.Width;
            double height = _originRect!.Value.Height;

            var newPointCount = element.PointCount;
            int n = 2 * newPointCount;

            var outerRatio = element.OuterRatio;
            var innerRatio = element.InnerRatio;
            var rotDeg = element.RotDeg;

            var points = new Avalonia.Point[n];

            double cx = width / 2.0, cy = height / 2.0;
            double outerR = outerRatio * Math.Min(width, height);
            double innerR = outerR * Math.Max(0, innerRatio);
            double start = rotDeg * Math.PI / 180.0;

            for (int i = 0; i < n; i++)
            {
                double a = start + i * Math.PI / newPointCount;
                double r = (i % 2 == 0) ? outerR : innerR;
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
            if (_currentElement is not RoundedStarElement element || ActualPoints.Count() == 0)
                return null;

            var radii = element.GetRadii();
            return RoundedPathBuilder.Build(ActualPoints, radii).ToAvaloniaGeometry();
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not RoundedStarElement element)
                return null;

            var points = ActualPoints;
            var radii = element.GetRadii();

            Point[] newPoints = new Point[points.Count()];
            if (offsetX == 0 && offsetY == 0)
            {
                offsetX = 0.001;
            }

            // AdjustPoints 已经基于 element.Width/Height 在 (0,0) 位置返回点；
            // 如果将来 element.X/Y 被占用，再加 (offsetX,offsetY) 偏移。
            if (offsetX != 0 || offsetY != 0)
            {
                for (int i = 0; i < points.Count(); i++)
                {
                    newPoints[i] = new Avalonia.Point(points[i].X + offsetX, points[i].Y + offsetY);
                }
            }

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
