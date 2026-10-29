using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls.Elements;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;
using Avalonia.Controls.Shapes;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 三角形容器控件 - 可在三角形内放置子元素和文本。
    /// 参考 RoundedPolygonControl 的初始化/订阅/调试覆盖模式。
    /// </summary>
    public class TriangleFlowControl : ShapeFlowControlBase
    {
        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public TriangleFlowControl(TriangleContainerElement element) : base()
        {
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
            if (e.PropertyName == nameof(TriangleContainerElement.TopPointOffsetX) ||
                e.PropertyName == nameof(TriangleContainerElement.TopPointOffsetY) ||
                e.PropertyName == nameof(TriangleContainerElement.BottomLeftOffsetX) ||
                e.PropertyName == nameof(TriangleContainerElement.BottomLeftOffsetY) ||
                e.PropertyName == nameof(TriangleContainerElement.BottomRightOffsetX) ||
                e.PropertyName == nameof(TriangleContainerElement.BottomRightOffsetY))
            {
                InitialPoints();
                UpdateShape(_currentElement);
                RedrawDebugOverlay();
            }
            else if (e.PropertyName == nameof(TriangleContainerElement.DrawPoint))
            {
                RedrawDebugOverlay();
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not TriangleContainerElement el) return;

            _debugOverlay.Children.Clear();

            if (!el.DrawPoint) return;

            foreach (var pt in ActualPoints)
            {
                _debugOverlay.Children.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(170, 80, 220)),
                    Stroke = new SolidColorBrush(Avalonia.Media.Color.FromRgb(120, 40, 170)),
                    StrokeThickness = 1,
                    [Canvas.LeftProperty] = pt.X - 4,
                    [Canvas.TopProperty] = pt.Y - 4
                });
            }
        }

        public override void InitialPoints()
        {
            if (_currentElement is not TriangleContainerElement element)
                return;

            _originRect = new Size(element.Width, element.Height);
            var p1 = new Avalonia.Point(element.TopPointOffsetX * element.Width, element.TopPointOffsetY * element.Height);
            var p2 = new Avalonia.Point(element.BottomLeftOffsetX * element.Width, element.BottomLeftOffsetY * element.Height);
            var p3 = new Avalonia.Point(element.BottomRightOffsetX * element.Width, element.BottomRightOffsetY * element.Height);

            OriginalPoints.Clear();
            OriginalPoints.AddRange(new Avalonia.Point[] { p1, p2, p3 });

            ActualPoints.Clear();
            ActualPoints.AddRange(new Avalonia.Point[] { p1, p2, p3 });

            base.ResizeActualPoints(ActualPoints.ToArray());
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not TriangleContainerElement element || ActualPoints.Count != 3)
                return null;

            var p1 = ActualPoints[0];
            var p2 = ActualPoints[1];
            var p3 = ActualPoints[2];

            var pathFigure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true
            };

            pathFigure.Segments!.Add(new LineSegment { Point = p2 });
            pathFigure.Segments.Add(new LineSegment { Point = p3 });

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures!.Add(pathFigure);

            return pathGeometry;
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not TriangleContainerElement element || ActualPoints.Count != 3)
                return null;

            if (offsetX == 0 && offsetY == 0)
                offsetX = 0.001;

            var p1 = new Avalonia.Point(ActualPoints[0].X + offsetX, ActualPoints[0].Y + offsetY);
            var p2 = new Avalonia.Point(ActualPoints[1].X + offsetX, ActualPoints[1].Y + offsetY);
            var p3 = new Avalonia.Point(ActualPoints[2].X + offsetX, ActualPoints[2].Y + offsetY);

            var pathFigure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true
            };

            pathFigure.Segments!.Add(new LineSegment { Point = p2 });
            pathFigure.Segments.Add(new LineSegment { Point = p3 });

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures!.Add(pathFigure);

            return pathGeometry;
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

    /// <summary>
    /// 矩形容器控件 - 可在矩形内放置子元素和文本。
    /// 参考 RoundedPolygonControl 的初始化/订阅/调试覆盖模式。
    /// </summary>
    public class RectangleFlowControl : ShapeFlowControlBase
    {
        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public RectangleFlowControl(RectangleContainerElement element) : base()
        {
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
            if (e.PropertyName == nameof(RectangleContainerElement.Width) ||
                e.PropertyName == nameof(RectangleContainerElement.Height))
            {
                InitialPoints();
                UpdateShape(_currentElement);
                RedrawDebugOverlay();
            }
            else if (e.PropertyName == nameof(RectangleContainerElement.DrawPoint))
            {
                RedrawDebugOverlay();
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not RectangleContainerElement el) return;

            _debugOverlay.Children.Clear();

            if (!el.DrawPoint) return;

            foreach (var pt in ActualPoints)
            {
                _debugOverlay.Children.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(170, 80, 220)),
                    Stroke = new SolidColorBrush(Avalonia.Media.Color.FromRgb(120, 40, 170)),
                    StrokeThickness = 1,
                    [Canvas.LeftProperty] = pt.X - 4,
                    [Canvas.TopProperty] = pt.Y - 4
                });
            }
        }

        public override void InitialPoints()
        {
            if (_currentElement is not RectangleContainerElement element)
                return;

            _originRect = new Size(element.Width, element.Height);
            var p1 = new Avalonia.Point(0, 0);
            var p2 = new Avalonia.Point(element.Width, element.Height);

            OriginalPoints.Clear();
            OriginalPoints.AddRange(new Avalonia.Point[] { p1, p2 });

            ActualPoints.Clear();
            ActualPoints.AddRange(new Avalonia.Point[] { p1, p2 });

            base.ResizeActualPoints(ActualPoints.ToArray());
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not RectangleContainerElement element || ActualPoints.Count != 2)
                return null;

            var p1 = ActualPoints[0];
            var p2 = ActualPoints[1];

            return new RectangleGeometry(new Avalonia.Rect(p1, p2));
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not RectangleContainerElement element || ActualPoints.Count != 2)
                return null;

            if (offsetX == 0 && offsetY == 0)
                offsetX = 0.001;

            var p1 = new Avalonia.Point(ActualPoints[0].X + offsetX, ActualPoints[0].Y + offsetY);
            var p2 = new Avalonia.Point(ActualPoints[1].X + offsetX, ActualPoints[1].Y + offsetY);

            return new RectangleGeometry(new Avalonia.Rect(p1, p2));
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

    /// <summary>
    /// 椭圆容器控件 - 可在椭圆内放置子元素和文本。
    /// 参考 RoundedPolygonControl 的初始化/订阅/调试覆盖模式。
    /// </summary>
    public class EllipseFlowControl : ShapeFlowControlBase
    {
        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public EllipseFlowControl(EllipseContainerElement element) : base()
        {
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
            if (e.PropertyName == nameof(EllipseContainerElement.Width) ||
                e.PropertyName == nameof(EllipseContainerElement.Height))
            {
                InitialPoints();
                UpdateShape(_currentElement);
                RedrawDebugOverlay();
            }
            else if (e.PropertyName == nameof(EllipseContainerElement.DrawPoint))
            {
                RedrawDebugOverlay();
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not EllipseContainerElement el) return;

            _debugOverlay.Children.Clear();

            if (!el.DrawPoint) return;

            foreach (var pt in ActualPoints)
            {
                _debugOverlay.Children.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(170, 80, 220)),
                    Stroke = new SolidColorBrush(Avalonia.Media.Color.FromRgb(120, 40, 170)),
                    StrokeThickness = 1,
                    [Canvas.LeftProperty] = pt.X - 4,
                    [Canvas.TopProperty] = pt.Y - 4
                });
            }
        }

        public override void InitialPoints()
        {
            if (_currentElement is not EllipseContainerElement element)
                return;

            _originRect = new Size(element.Width, element.Height);
            var p1 = new Avalonia.Point(0, 0);
            var p2 = new Avalonia.Point(element.Width, element.Height);

            OriginalPoints.Clear();
            OriginalPoints.AddRange(new Avalonia.Point[] { p1, p2 });

            ActualPoints.Clear();
            ActualPoints.AddRange(new Avalonia.Point[] { p1, p2 });

            base.ResizeActualPoints(ActualPoints.ToArray());
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not EllipseContainerElement element || ActualPoints.Count != 2)
                return null;

            var p1 = ActualPoints[0];
            var p2 = ActualPoints[1];

            return new EllipseGeometry(new Avalonia.Rect(p1, p2));
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not EllipseContainerElement element || ActualPoints.Count != 2)
                return null;

            if (offsetX == 0 && offsetY == 0)
                offsetX = 0.001;

            var p1 = new Avalonia.Point(ActualPoints[0].X + offsetX, ActualPoints[0].Y + offsetY);
            var p2 = new Avalonia.Point(ActualPoints[1].X + offsetX, ActualPoints[1].Y + offsetY);

            return new EllipseGeometry(new Avalonia.Rect(p1, p2));
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

    /// <summary>
    /// 平行四边形容器控件 - 可在平行四边形内放置子元素和文本。
    /// 参考 RoundedPolygonControl 的初始化/订阅/调试覆盖模式。
    /// </summary>
    public class ParallelogramFlowControl : ShapeFlowControlBase
    {
        private readonly Canvas _debugOverlay = new()
        {
            IsHitTestVisible = false
        };

        public ParallelogramFlowControl(ParallelogramContainerElement element) : base()
        {
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
            if (e.PropertyName == nameof(ParallelogramContainerElement.Width) ||
                e.PropertyName == nameof(ParallelogramContainerElement.Height) ||
                e.PropertyName == nameof(ParallelogramContainerElement.SkewX))
            {
                InitialPoints();
                UpdateShape(_currentElement);
                RedrawDebugOverlay();
            }
            else if (e.PropertyName == nameof(ParallelogramContainerElement.DrawPoint))
            {
                RedrawDebugOverlay();
            }
        }

        private void RedrawDebugOverlay()
        {
            if (_currentElement is not ParallelogramContainerElement el) return;

            _debugOverlay.Children.Clear();

            if (!el.DrawPoint) return;

            foreach (var pt in ActualPoints)
            {
                _debugOverlay.Children.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Avalonia.Media.Color.FromRgb(170, 80, 220)),
                    Stroke = new SolidColorBrush(Avalonia.Media.Color.FromRgb(120, 40, 170)),
                    StrokeThickness = 1,
                    [Canvas.LeftProperty] = pt.X - 4,
                    [Canvas.TopProperty] = pt.Y - 4
                });
            }
        }

        public override void InitialPoints()
        {
            if (_currentElement is not ParallelogramContainerElement element)
                return;

            double w = element.Width;
            double h = element.Height;
            _originRect = new Size(w, h);
            double skewOffset = w * element.SkewX;

            var p1 = new Avalonia.Point(skewOffset, 0);
            var p2 = new Avalonia.Point(w, 0);
            var p3 = new Avalonia.Point(w - skewOffset, h);
            var p4 = new Avalonia.Point(0, h);

            OriginalPoints.Clear();
            OriginalPoints.AddRange(new Avalonia.Point[] { p1, p2, p3, p4 });

            ActualPoints.Clear();
            ActualPoints.AddRange(new Avalonia.Point[] { p1, p2, p3, p4 });

            base.ResizeActualPoints(ActualPoints.ToArray());
        }

        public override Geometry? BuildGeometry()
        {
            if (_currentElement is not ParallelogramContainerElement element || ActualPoints.Count != 4)
                return null;

            var p1 = ActualPoints[0];
            var p2 = ActualPoints[1];
            var p3 = ActualPoints[2];
            var p4 = ActualPoints[3];

            var pathFigure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true
            };

            pathFigure.Segments!.Add(new LineSegment { Point = p2 });
            pathFigure.Segments.Add(new LineSegment { Point = p3 });
            pathFigure.Segments.Add(new LineSegment { Point = p4 });

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures!.Add(pathFigure);

            return pathGeometry;
        }

        public override Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            if (_currentElement is not ParallelogramContainerElement element || ActualPoints.Count != 4)
                return null;

            if (offsetX == 0 && offsetY == 0)
                offsetX = 0.001;

            var p1 = new Avalonia.Point(ActualPoints[0].X + offsetX, ActualPoints[0].Y + offsetY);
            var p2 = new Avalonia.Point(ActualPoints[1].X + offsetX, ActualPoints[1].Y + offsetY);
            var p3 = new Avalonia.Point(ActualPoints[2].X + offsetX, ActualPoints[2].Y + offsetY);
            var p4 = new Avalonia.Point(ActualPoints[3].X + offsetX, ActualPoints[3].Y + offsetY);

            var pathFigure = new PathFigure
            {
                StartPoint = p1,
                IsClosed = true
            };

            pathFigure.Segments!.Add(new LineSegment { Point = p2 });
            pathFigure.Segments.Add(new LineSegment { Point = p3 });
            pathFigure.Segments.Add(new LineSegment { Point = p4 });

            var pathGeometry = new PathGeometry();
            pathGeometry.Figures!.Add(pathFigure);

            return pathGeometry;
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
