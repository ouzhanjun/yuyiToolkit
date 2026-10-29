using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;
using System.ComponentModel;
using Point = Avalonia.Point;

namespace yuyiAvalonia.UI.ElementControls.Controls
{

    /// <summary>
    /// 椭圆 / 弧 / 扇形容器控件（Figma 风格参数化）。
    /// 支持分类显示调试点：
    ///   - DrawPoint: 总开关
    ///   - DrawVertex: 显示图形点（椭圆边框控制点）- 蓝色
    ///   - DrawTangent: 显示控制点（弧起止点、尖角点）- 绿色
    ///   - DrawCenter: 显示圆心 - 橙色
    /// </summary>
    public class RoundEllipseControl : ShapeFlowControlBase
    {
        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public RoundEllipseControl(RoundEllipseElement element) : base()
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
                case nameof(RoundEllipseElement.DrawPoint):
                case nameof(RoundEllipseElement.DrawVertex):
                case nameof(RoundEllipseElement.DrawTangent):
                case nameof(RoundEllipseElement.DrawCenter):
                case nameof(RoundEllipseElement.DrawVertexLine):
                    RedrawDebugOverlay();
                    break;
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not RoundEllipseElement el) return;

            _debugOverlay.Children.Clear();

            if (!el.DrawPoint) return;

            double cx = _originPosition.X + el.Width / 2;
            double cy = _originPosition.Y + el.Height / 2;
            double rx = el.Width / 2;
            double ry = el.Height / 2;

            // 颜色配置
            var vertexFill = new SolidColorBrush(Color.FromRgb(30, 136, 229));    // 蓝色
            var vertexStroke = new SolidColorBrush(Color.FromRgb(21, 101, 192));
            var tangentFill = new SolidColorBrush(Color.FromRgb(76, 175, 80));    // 绿色
            var tangentStroke = new SolidColorBrush(Color.FromRgb(46, 125, 50));
            var centerFill = new SolidColorBrush(Color.FromRgb(255, 152, 0));     // 橙色
            var centerStroke = new SolidColorBrush(Color.FromRgb(230, 81, 0));

            double vertexSize = 10;
            double tangentSize = 8;
            double centerSize = 10;

            // 1) 图形点（顶点）- 椭圆边框的4个控制点
            if (el.DrawVertex)
            {
                var cornerPoints = new Point[]
                {
                    new Point(_originPosition.X, _originPosition.Y),           // 左上
                    new Point(_originPosition.X + el.Width, _originPosition.Y), // 右上
                    new Point(_originPosition.X + el.Width, _originPosition.Y + el.Height), // 右下
                    new Point(_originPosition.X, _originPosition.Y + el.Height)  // 左下
                };

                foreach (var p in cornerPoints)
                {
                    _debugOverlay.Children.Add(new Ellipse
                    {
                        Width = vertexSize,
                        Height = vertexSize,
                        Fill = vertexFill,
                        Stroke = vertexStroke,
                        StrokeThickness = 1.5,
                        [Canvas.LeftProperty] = p.X - vertexSize * 0.5,
                        [Canvas.TopProperty] = p.Y - vertexSize * 0.5
                    });
                }
            }

            // 2) 控制点（切点）- 弧的起止点
            if (el.DrawTangent)
            {
                double startRad = el.StartAngle * Math.PI / 180.0;
                double endRad = startRad + el.SweepAngle * Math.PI / 180.0;

                // 起始点
                Point startPoint = new Point(
                    cx + rx * Math.Cos(startRad),
                    cy - ry * Math.Sin(startRad));

                // 终止点
                Point endPoint = new Point(
                    cx + rx * Math.Cos(endRad),
                    cy - ry * Math.Sin(endRad));

                // 起始点 - 绿色方块
                AddDebugSquare(_debugOverlay, startPoint, tangentSize, tangentFill, tangentStroke);
                // 终止点 - 绿色方块
                AddDebugSquare(_debugOverlay, endPoint, tangentSize, tangentFill, tangentStroke);

                // 如果是圆环/扇环，添加内圈起止点
                if (el.InnerRadius > 0)
                {
                    double innerRx = rx * el.InnerRadius;
                    double innerRy = ry * el.InnerRadius;

                    Point innerStart = new Point(
                        cx + innerRx * Math.Cos(startRad),
                        cy - innerRy * Math.Sin(startRad));
                    Point innerEnd = new Point(
                        cx + innerRx * Math.Cos(endRad),
                        cy - innerRy * Math.Sin(endRad));

                    AddDebugSquare(_debugOverlay, innerStart, tangentSize, tangentFill, tangentStroke);
                    AddDebugSquare(_debugOverlay, innerEnd, tangentSize, tangentFill, tangentStroke);
                }
            }

            // 3) 圆心 - 橙色菱形
            if (el.DrawCenter)
            {
                Point center = new Point(cx, cy);
                _debugOverlay.Children.Add(new Border
                {
                    Width = centerSize,
                    Height = centerSize,
                    Background = centerFill,
                    BorderBrush = centerStroke,
                    BorderThickness = new Thickness(1.5),
                    CornerRadius = new CornerRadius(2),
                    [Canvas.LeftProperty] = center.X - centerSize * 0.5,
                    [Canvas.TopProperty] = center.Y - centerSize * 0.5,
                    RenderTransform = new RotateTransform(45),
                    RenderTransformOrigin = new RelativePoint(centerSize * 0.5, centerSize * 0.5, RelativeUnit.Absolute)
                });
            }
        }

        private static void AddDebugSquare(Canvas canvas, Point p, double size, IBrush fill, IBrush stroke)
        {
            canvas.Children.Add(new Border
            {
                Width = size,
                Height = size,
                Background = fill,
                BorderBrush = stroke,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(1),
                [Canvas.LeftProperty] = p.X - size * 0.5,
                [Canvas.TopProperty] = p.Y - size * 0.5
            });
        }

        public override void InitialPoints()
        {
            if (_currentElement is not RoundEllipseElement element) return;

            _originRect = new Size(element.Width, element.Height);
            var p1 = new Avalonia.Point(_originPosition.X, _originPosition.Y);
            var p2 = new Avalonia.Point(_originPosition.X + element.Width, _originPosition.Y + element.Height);

            OriginalPoints.Clear();
            OriginalPoints.AddRange(new Point[] { p1, p2 });

            ActualPoints.Clear();
            ActualPoints.AddRange(new Point[] { p1, p2 });
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not RoundEllipseElement element) return null;

            return EllipseArcPathBuilder.Build(
                _originPosition.X, _originPosition.Y, element.Width, element.Height,
                element.StartAngle, element.SweepAngle, element.InnerRadius,
                element.CornerRadius).ToAvaloniaGeometry();
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not RoundEllipseElement element) return null;

            if (offsetX == 0 && offsetY == 0) offsetX = 0.001;

            return EllipseArcPathBuilder.Build(
                _originPosition.X + offsetX, _originPosition.Y + offsetY,
                element.Width, element.Height,
                element.StartAngle, element.SweepAngle, element.InnerRadius,
                element.CornerRadius).ToAvaloniaGeometry();
        }

        public override void ShowWorkspaceOverlay() => _isShowOverlay = true;
        public override void HideWorkspaceOverlay() => _isShowOverlay = false;

        public override void UpdateShape(CanvasElement? shapeElement)
        {
            base.UpdateShape(shapeElement);
            RedrawDebugOverlay();
        }
    }
}