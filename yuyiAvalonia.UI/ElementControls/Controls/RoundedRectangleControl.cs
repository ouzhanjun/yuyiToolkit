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
    /// 圆角矩形容器控件 - 4 个顶点（左上 → 右上 → 右下 → 左下），
    /// 每个顶点独立圆角 R0..R3。复用 RoundedRectangleGeometry 算法。
    /// 当 RoundedRectangleElement.DrawPoint = true 时叠加顶点/切点/圆心调试标记。
    /// </summary>
    public class RoundedRectangleControl : ShapeFlowControlBase
    {
        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public RoundedRectangleControl(RoundedRectangleElement element) : base()
        {
            _originRect = new Size(element.Width, element.Height);
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
                case nameof(RoundedRectangleElement.DrawPoint):
                case nameof(RoundedRectangleElement.DrawVertex):
                case nameof(RoundedRectangleElement.DrawTangent):
                case nameof(RoundedRectangleElement.DrawCenter):
                case nameof(RoundedRectangleElement.DrawVertexLine):
                    RedrawDebugOverlay();
                    break;
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not RoundedRectangleElement el) return;

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
            if (_currentElement is not RoundedRectangleElement element)
                return;

            double width = _originRect!.Value.Width;
            double height = _originRect!.Value.Height;

            var points = new Avalonia.Point[]
            {
                new Avalonia.Point(0, 0),   // 左上  R0
                new Avalonia.Point(width, 0),   // 右上  R1
                new Avalonia.Point(width, height),   // 右下  R2
                new Avalonia.Point(0, height),   // 左下  R3
            };

            OriginalPoints.Clear();
            OriginalPoints.AddRange(points);

            ActualPoints.Clear();
            ActualPoints.AddRange(points);

            base.ResizeActualPoints(ActualPoints.ToArray());
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not RoundedRectangleElement element || ActualPoints.Count() == 0)
                return null;

            var radii = element.GetRadii();
            return RoundedPathBuilder.Build(ActualPoints, radii).ToAvaloniaGeometry();
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not RoundedRectangleElement element)
                return null;

            var points = ActualPoints;
            var radii = element.GetRadii();

            if (offsetX == 0 && offsetY == 0)
                offsetX = 0.001;

            Point[] newPoints = new Point[points.Count()];
            for (int i = 0; i < points.Count(); i++)
                newPoints[i] = new Avalonia.Point(points[i].X + offsetX, points[i].Y + offsetY);

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
