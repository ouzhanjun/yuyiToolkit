using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.UI.Controls;

public class SingleElementResizedEventArgs : EventArgs
{
    public CanvasElement Element { get; }
    public SingleElementResizedEventArgs(CanvasElement element) => Element = element;
}

public class SingleElementRotatedEventArgs : EventArgs
{
    public CanvasElement Element { get; }
    public double NewRotation { get; }
    public SingleElementRotatedEventArgs(CanvasElement element, double newRotation)
    {
        Element = element;
        NewRotation = newRotation;
    }
}

public class SingleSelectionOverlay : Canvas
{
    private const string AuxLineColor = "#1E90FF";

    private const double SelectionBorderWidth = 1;
    private const double SelectionPadding = 2;
    private const double HandleSize = 8;
    private const double MinElementSize = 20;
    private const double SnapAngleStep = 5;

    // 状态
    private bool _isResizing;
    private bool _isRotating;
    private ResizeHandle _activeResizeHandle = ResizeHandle.None;
    private Point _resizeStartPoint;
    private Point _rotateStartPoint;
    private Point _rotateCenterPoint;
    private double _rotateStartAngle;
    private double _elementStartRotation;
    private (double X, double Y, double Width, double Height, double Rotation) _elementStartState;

    private CanvasElement? _selectedElement;
    private RenderControl? _renderControl;

    public CanvasElement? SelectedElement => _selectedElement;
    public RenderControl? RenderControl
    {
        get => _renderControl;
        set => _renderControl = value;
    }

    public bool IsHandling => _isResizing || _isRotating;
    public bool IsRotating => _isRotating;

    // 事件
    public event EventHandler<SingleElementResizedEventArgs>? ElementResized;
    public event EventHandler<SingleElementRotatedEventArgs>? ElementRotated;

    // 控件
    private readonly Rectangle _selectionRect;
    private readonly Polygon _rotatedSelectionPolygon;
    private readonly Ellipse[] _resizeHandles;
    private readonly Border _rotateButton;
    private readonly Path _rotateButtonIcon;
    private readonly Ellipse _rotateCenterIndicator;
    private readonly Ellipse _rotateGuideCircle;
    private double _rotateRadius;

    // 角度参考线
    private readonly Line[] _angleReferenceLines;

    private Dictionary<CanvasElement, Control>? _elementControlMap;
    public Dictionary<CanvasElement, Control>? ElementControlMap
    {
        get => _elementControlMap;
        set => _elementControlMap = value;
    }

    public SingleSelectionOverlay()
    {
        _selectionRect = new Rectangle
        {
            Fill = new SolidColorBrush(Color.Parse("#301E90FF")),
            Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
            StrokeThickness = SelectionBorderWidth,
            IsHitTestVisible = false,
            IsVisible = false
        };

        _rotatedSelectionPolygon = new Polygon
        {
            Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
            StrokeThickness = SelectionBorderWidth,
            Fill = new SolidColorBrush(Color.Parse("#301E90FF")),
            IsHitTestVisible = false,
            IsVisible = false
        };

        _resizeHandles = new Ellipse[8];
        for (int i = 0; i < 8; i++)
        {
            var handle = new Ellipse
            {
                Width = HandleSize,
                Height = HandleSize,
                Fill = Brushes.White,
                Stroke = Brushes.DodgerBlue,
                StrokeThickness = 1,
                Cursor = GetCursorForHandle((ResizeHandle)(i + 1)),
                Tag = (ResizeHandle)(i + 1),
                IsVisible = false,
                IsHitTestVisible = true
            };
            handle.PointerPressed += OnResizeHandlePressed;
            handle.PointerMoved += OnResizeHandleMoved;
            handle.PointerReleased += OnResizeHandleReleased;
            _resizeHandles[i] = handle;
        }

        _rotateButtonIcon = new Path
        {
            Data = Icon.RotationLT,
            Fill = new SolidColorBrush(Color.Parse(AuxLineColor)),
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            IsHitTestVisible = false
        };

        _rotateButton = new Border
        {
            Width = 20,
            Height = 20,
            //BorderThickness = new Thickness(1),
            //BorderBrush = Brushes.AliceBlue,
            CornerRadius = new CornerRadius(10),
            Background = Brushes.Transparent,
            Child = _rotateButtonIcon,
            Cursor = new Cursor(StandardCursorType.SizeAll),
            IsVisible = false,
            IsHitTestVisible = true,
            [ToolTip.TipProperty] = "拖动旋转"
        };
        _rotateButton.PointerPressed += OnRotateButtonPressed;
        _rotateButton.PointerMoved += OnRotateButtonMoved;
        _rotateButton.PointerReleased += OnRotateButtonReleased;

        _rotateCenterIndicator = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(Color.Parse(AuxLineColor)),
            IsHitTestVisible = false,
            IsVisible = false
        };

        _rotateGuideCircle = new Ellipse
        {
            Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
            StrokeThickness = 1,
            StrokeDashArray = new AvaloniaList<double> { 4, 2 },
            Fill = Avalonia.Media.Brushes.Transparent,
            IsHitTestVisible = false,
            IsVisible = false
        };

        _angleReferenceLines = new Line[2];
        for (int i = 0; i < 2; i++)
        {
            var line = new Line
            {
                Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
                StrokeThickness = 1,
                StrokeDashArray = new AvaloniaList<double> { 4, 3 },
                IsHitTestVisible = false,
                IsVisible = false
            };
            _angleReferenceLines[i] = line;
        }

        Children.Add(_selectionRect);
        Children.Add(_rotatedSelectionPolygon);
        foreach (var handle in _resizeHandles)
            Children.Add(handle);
        Children.Add(_rotateCenterIndicator);
        Children.Add(_rotateGuideCircle);
        Children.Add(_rotateButton);
        foreach (var line in _angleReferenceLines)
            Children.Add(line);
    }

    #region 公共方法

    public void SetSelectedElement(CanvasElement? element)
    {
        if (_selectedElement == element) return;
        _selectedElement = element;

        if (_selectedElement != null)
            UpdateOverlay();
        else
            ClearOverlay();
    }

    public void RefreshOverlay()
    {
        if (_selectedElement != null)
            UpdateOverlay();
    }

    public void ClearOverlay()
    {
        _selectedElement = null;
        _selectionRect.IsVisible = false;
        _rotatedSelectionPolygon.IsVisible = false;
        HideResizeHandles();
        _rotateButton.IsVisible = false;
        _rotateCenterIndicator.IsVisible = false;
        _rotateGuideCircle.IsVisible = false;
        HideAngleReferenceLines();
    }

    #endregion

    #region 内部更新

    private void UpdateOverlay()
    {
        if (_selectedElement == null || !_elementControlMap!.TryGetValue(_selectedElement, out var control))
        {
            ClearOverlay();
            return;
        }

        double elemWidth = _selectedElement.Width;
        double elemHeight = _selectedElement.Height;
        double rotation = _selectedElement.Rotation;


        // 应该跟Pressed时候的位置一致
        var corners = GetElementVisualCorners(control, _selectedElement);
        if (corners != null)
            _rotateCenterPoint = corners.Value.Center;

        // 半径 = 中心到右上角的距离（对角线的一半）
        double halfW = _selectedElement.Width / 2;
        double halfH = _selectedElement.Height / 2;
        _rotateRadius = Math.Sqrt(halfW * halfW + halfH * halfH) + 10;
        double rotationRad = rotation * Math.PI / 180;

        //if (rotation == 0)
        //{
        //    _selectionRect.IsVisible = true;
        //    _rotatedSelectionPolygon.IsVisible = false;

        //    var tl = control.TranslatePoint(new Point(0, 0), this);
        //    if (tl == null) { ClearOverlay(); return; }

        //    double _x = tl.Value.X - SelectionPadding;
        //    double _y = tl.Value.Y - SelectionPadding;
        //    double w = elemWidth + SelectionPadding * 2;
        //    double h = elemHeight + SelectionPadding * 2;

        //    Canvas.SetLeft(_selectionRect, _x);
        //    Canvas.SetTop(_selectionRect, _y);
        //    _selectionRect.Width = w;
        //    _selectionRect.Height = h;

        //    UpdateResizeHandles(_x, _y, w, h);

        //    // 旋转按钮位置：中心到右上角的初始角度 + 元素当前旋转角
        //    double initialAngle = Math.Atan2(-halfH, halfW);
        //    double finalAngle = rotationRad + initialAngle;
        //    double rotateX = _rotateCenterPoint.X + _rotateRadius * Math.Cos(finalAngle);
        //    double rotateY = _rotateCenterPoint.Y + _rotateRadius * Math.Sin(finalAngle);
        //    Canvas.SetLeft(_rotateButton, rotateX - _rotateButton.Bounds.Width / 2);
        //    Canvas.SetTop(_rotateButton, rotateY - _rotateButton.Bounds.Height / 2);
        //    _rotateButton.RenderTransform = new RotateTransform(rotation);
        //    _rotateButton.IsVisible = true;
        //}
        //else
        {
            _selectionRect.IsVisible = false;
            _rotatedSelectionPolygon.IsVisible = true;

            if (corners == null) { ClearOverlay(); return; }

            var c = corners.Value;

            _rotatedSelectionPolygon.Points = new AvaloniaList<Point>
            {
                new Point(c.TopLeft.X, c.TopLeft.Y),
                new Point(c.TopRight.X, c.TopRight.Y),
                new Point(c.BottomRight.X, c.BottomRight.Y),
                new Point(c.BottomLeft.X, c.BottomLeft.Y)
            };

            UpdateRotatedResizeHandles(c);

            if (!IsRotating)
            {
                // 旋转按钮位置：使用正确的向量旋转计算
                // 中心到右上角的初始角度（未旋转时的角度）
                double initialAngle = Math.Atan2(-halfH, halfW);
                double finalAngle = rotationRad + initialAngle;
                // 旋转把位置 = 中心 + 半径 * cos/sin(最终角度)
                double rotateX = _rotateCenterPoint.X + _rotateRadius * Math.Cos(finalAngle);
                double rotateY = _rotateCenterPoint.Y + _rotateRadius * Math.Sin(finalAngle);
                Canvas.SetLeft(_rotateButton, rotateX - _rotateButton.Bounds.Width / 2);
                Canvas.SetTop(_rotateButton, rotateY - _rotateButton.Bounds.Height / 2);
                _rotateButton.RenderTransform = new RotateTransform(rotation);

                _rotateButton.IsVisible = true;

                Canvas.SetLeft(_rotateCenterIndicator, _rotateCenterPoint.X - 3);
                Canvas.SetTop(_rotateCenterIndicator, _rotateCenterPoint.Y - 3);
            }

        }
    }

    private (Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft, Point Center)?
        GetElementVisualCorners(Control control, CanvasElement element)
    {
        double width = element.Width;
        double height = element.Height;

        var transform = control.TransformToVisual(this);
        if (transform == null) return null;

        // 考虑元素自身旋转，计算实际视觉角点
        Point topLeft = transform.Value.Transform(new Point(0, 0));
        Point topRight = transform.Value.Transform(new Point(width, 0));
        Point bottomRight = transform.Value.Transform(new Point(width, height));
        Point bottomLeft = transform.Value.Transform(new Point(0, height));

        double cx = (topLeft.X + topRight.X + bottomRight.X + bottomLeft.X) / 4;
        double cy = (topLeft.Y + topRight.Y + bottomRight.Y + bottomLeft.Y) / 4;

        return (topLeft, topRight, bottomRight, bottomLeft, new Point(cx, cy));
    }

    
    private void UpdateRotatedResizeHandles(
        (Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft, Point Center) corners)
    {
        var halfW = HandleSize / 2;
        var handles = new[]
        {
            corners.TopLeft,
            new Point((corners.TopLeft.X + corners.TopRight.X) / 2, (corners.TopLeft.Y + corners.TopRight.Y) / 2),
            corners.TopRight,
            new Point((corners.TopLeft.X + corners.BottomLeft.X) / 2, (corners.TopLeft.Y + corners.BottomLeft.Y) / 2),
            new Point((corners.TopRight.X + corners.BottomRight.X) / 2, (corners.TopRight.Y + corners.BottomRight.Y) / 2),
            corners.BottomLeft,
            new Point((corners.BottomLeft.X + corners.BottomRight.X) / 2, (corners.BottomLeft.Y + corners.BottomRight.Y) / 2),
            corners.BottomRight
        };

        for (int i = 0; i < 8; i++)
        {
            Canvas.SetLeft(_resizeHandles[i], handles[i].X - halfW);
            Canvas.SetTop(_resizeHandles[i], handles[i].Y - halfW);
            _resizeHandles[i].IsVisible = true;
        }
    }

    private void HideResizeHandles()
    {
        foreach (var handle in _resizeHandles)
            handle.IsVisible = false;
    }

    #endregion

    #region 调整大小

    private static Cursor GetCursorForHandle(ResizeHandle handle) => handle switch
    {
        ResizeHandle.TopLeft or ResizeHandle.BottomRight => new Cursor(StandardCursorType.BottomRightCorner),
        ResizeHandle.TopRight or ResizeHandle.BottomLeft => new Cursor(StandardCursorType.TopRightCorner),
        ResizeHandle.TopCenter or ResizeHandle.BottomCenter => new Cursor(StandardCursorType.SizeNorthSouth),
        ResizeHandle.MiddleLeft or ResizeHandle.MiddleRight => new Cursor(StandardCursorType.SizeWestEast),
        _ => Cursor.Default
    };

    private void OnResizeHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Ellipse ellipse || _selectedElement == null) return;

        _isResizing = true;
        _activeResizeHandle = (ResizeHandle)ellipse.Tag!;
        _resizeStartPoint = e.GetPosition(this);

        _elementStartState = (
            _selectedElement.X, _selectedElement.Y,
            _selectedElement.Width, _selectedElement.Height,
            _selectedElement.Rotation);

        e.Handled = true;
    }

    private void OnResizeHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || _selectedElement == null) return;

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _resizeStartPoint;

        double newX = _elementStartState.X;
        double newY = _elementStartState.Y;
        double newWidth = _elementStartState.Width;
        double newHeight = _elementStartState.Height;

        bool isRotated = _elementStartState.Rotation != 0;

        if (isRotated)
        {
            // 旋转元素的调整：使用中心点+半径的方式
            ApplyRotatedResize(delta);
        }
        else
        {
            // 轴对齐元素的调整
            switch (_activeResizeHandle)
            {
                // 角点：对角固定，拖动端改变位置+尺寸
                case ResizeHandle.TopLeft:
                    newX = _elementStartState.X + delta.X;
                    newY = _elementStartState.Y + delta.Y;
                    newWidth = _elementStartState.Width - delta.X;
                    newHeight = _elementStartState.Height - delta.Y;
                    break;

                case ResizeHandle.TopRight:
                    newY = _elementStartState.Y + delta.Y;
                    newWidth = _elementStartState.Width + delta.X;
                    newHeight = _elementStartState.Height - delta.Y;
                    break;

                case ResizeHandle.BottomLeft:
                    newX = _elementStartState.X + delta.X;
                    newWidth = _elementStartState.Width - delta.X;
                    newHeight = _elementStartState.Height + delta.Y;
                    break;

                case ResizeHandle.BottomRight:
                    newWidth = _elementStartState.Width + delta.X;
                    newHeight = _elementStartState.Height + delta.Y;
                    break;

                // 边中点：只改变对应维度，对边固定
                case ResizeHandle.TopCenter:
                    newY = _elementStartState.Y + delta.Y;
                    newHeight = _elementStartState.Height - delta.Y;
                    break;

                case ResizeHandle.BottomCenter:
                    newHeight = _elementStartState.Height + delta.Y;
                    break;

                case ResizeHandle.MiddleLeft:
                    newX = _elementStartState.X + delta.X;
                    newWidth = _elementStartState.Width - delta.X;
                    break;

                case ResizeHandle.MiddleRight:
                    newWidth = _elementStartState.Width + delta.X;
                    break;
            }

            // 应用尺寸约束
            if (newWidth < MinElementSize)
            {
                if (_activeResizeHandle == ResizeHandle.TopLeft ||
                    _activeResizeHandle == ResizeHandle.MiddleLeft)
                    newX = _elementStartState.X + _elementStartState.Width - MinElementSize;
                newWidth = MinElementSize;
            }
            if (newHeight < MinElementSize)
            {
                if (_activeResizeHandle == ResizeHandle.TopLeft ||
                    _activeResizeHandle == ResizeHandle.TopCenter)
                    newY = _elementStartState.Y + _elementStartState.Height - MinElementSize;
                newHeight = MinElementSize;
            }

            ApplyResize(newX, newY, newWidth, newHeight);
            UpdateOverlay();
        }
    }

    private void ApplyRotatedResize(Point delta)
    {
        if (_selectedElement == null) return;

        double centerX = _elementStartState.X + _elementStartState.Width / 2;
        double centerY = _elementStartState.Y + _elementStartState.Height / 2;
        double rad = _elementStartState.Rotation * Math.PI / 180;
        double cosA = Math.Cos(rad);
        double sinA = Math.Sin(rad);

        double halfW = _elementStartState.Width / 2;
        double halfH = _elementStartState.Height / 2;

        // 对角点（固定点）在世界坐标中的位置
        var fixedCorner = GetFixedCornerWorld(_activeResizeHandle, centerX, centerY, halfW, halfH, cosA, sinA);

        // 将鼠标移动从屏幕坐标转换到元素局部坐标系
        double localDeltaX = delta.X * cosA + delta.Y * sinA;
        double localDeltaY = -delta.X * sinA + delta.Y * cosA;

        double newHalfW = halfW;
        double newHalfH = halfH;
        double newCenterX = centerX;
        double newCenterY = centerY;

        switch (_activeResizeHandle)
        {
            case ResizeHandle.TopLeft:
            case ResizeHandle.TopRight:
            case ResizeHandle.BottomLeft:
            case ResizeHandle.BottomRight:
                {
                    // 角点：对角固定，拖动改变尺寸，中心移动
                    //
                    // 在局部坐标系中，四个角点的初始位置：
                    // - TopLeft: (-halfW, -halfH)
                    // - TopRight: (+halfW, -halfH)
                    // - BottomLeft: (-halfW, +halfH)
                    // - BottomRight: (+halfW, +halfH)
                    //
                    // 对角关系：
                    // - TopLeft ↔ BottomRight
                    // - TopRight ↔ BottomLeft
                    // - BottomLeft ↔ TopRight
                    // - BottomRight ↔ TopLeft

                    // 新角点的初始位置（局部坐标）
                    double newCornerX, newCornerY;
                    // 固定点（对角）的初始位置（局部坐标）
                    double fixedX, fixedY;

                    switch (_activeResizeHandle)
                    {
                        case ResizeHandle.TopLeft:
                            newCornerX = -halfW;
                            newCornerY = -halfH;
                            fixedX = halfW;
                            fixedY = halfH;
                            break;
                        case ResizeHandle.TopRight:
                            newCornerX = halfW;
                            newCornerY = -halfH;
                            fixedX = -halfW;
                            fixedY = halfH;
                            break;
                        case ResizeHandle.BottomLeft:
                            newCornerX = -halfW;
                            newCornerY = halfH;
                            fixedX = halfW;
                            fixedY = -halfH;
                            break;
                        case ResizeHandle.BottomRight:
                            newCornerX = halfW;
                            newCornerY = halfH;
                            fixedX = -halfW;
                            fixedY = -halfH;
                            break;
                        default:
                            newCornerX = 0;
                            newCornerY = 0;
                            fixedX = 0;
                            fixedY = 0;
                            break;
                    }

                    // 新角点跟随鼠标移动
                    newCornerX += localDeltaX;
                    newCornerY += localDeltaY;

                    // 新的 halfW, halfH = 中心到两个角点的距离
                    newHalfW = Math.Max(MinElementSize / 2, Math.Abs(fixedX - newCornerX) / 2);
                    newHalfH = Math.Max(MinElementSize / 2, Math.Abs(fixedY - newCornerY) / 2);

                    // 新的中心在局部坐标中 = (新角点 + 固定点) / 2
                    double newCenterLocalX = (newCornerX + fixedX) / 2;
                    double newCenterLocalY = (newCornerY + fixedY) / 2;

                    // 变换到世界坐标
                    newCenterX = centerX + newCenterLocalX * cosA - newCenterLocalY * sinA;
                    newCenterY = centerY + newCenterLocalX * sinA + newCenterLocalY * cosA;
                }
                break;

            case ResizeHandle.TopCenter:
            case ResizeHandle.BottomCenter:
                {
                    // 上下边中点：只改变高度，对边固定
                    double dragSign = (_activeResizeHandle == ResizeHandle.TopCenter) ? 1 : -1;
                    double deltaH = -localDeltaY * dragSign;
                    newHalfH = Math.Max(MinElementSize / 2, halfH + deltaH);

                    // 计算实际的高度变化（考虑最小尺寸限制）
                    double actualDeltaH = newHalfH - halfH;
                    
                    // 中心需要移动以保持对边固定
                    // 对于旋转元素，局部Y方向的移动会同时影响世界X和世界Y
                    // 正确的旋转逆变换：世界X偏移 = -局部Y偏移 * (-sinA)，世界Y偏移 = -局部Y偏移 * cosA
                    // 简化：世界X偏移 = 局部Y偏移 * sinA，世界Y偏移 = -局部Y偏移 * cosA
                    double centerShiftX = actualDeltaH * sinA * dragSign;
                    double centerShiftY = -actualDeltaH * cosA * dragSign;
                    newCenterX = centerX + centerShiftX;
                    newCenterY = centerY + centerShiftY;
                }
                break;

            case ResizeHandle.MiddleLeft:
            case ResizeHandle.MiddleRight:
                {
                    // 左右边中点：被拖动的边跟随移动，对边固定
                    // MiddleLeft：左边跟随移动，右边固定
                    // MiddleRight：右边跟随移动，左边固定
                    //
                    // 坐标系理解：
                    // localDeltaX > 0 表示鼠标在世界坐标中向右移动
                    // MiddleLeft 向右拖（localDeltaX > 0）：左边向右移动，宽度减少
                    // MiddleLeft 向左拖（localDeltaX < 0）：左边向左移动，宽度增加
                    // MiddleRight 向右拖（localDeltaX > 0）：右边向右移动，宽度增加
                    // MiddleRight 向左拖（localDeltaX < 0）：右边向左移动，宽度减少
                    double deltaW = (_activeResizeHandle == ResizeHandle.MiddleLeft)
                        ? -localDeltaX  // MiddleLeft: 向左拖(localDeltaX<0)时deltaW>0宽度增加
                        : localDeltaX;  // MiddleRight: 向右拖(localDeltaX>0)时deltaW>0宽度增加
                    newHalfW = Math.Max(MinElementSize / 2, halfW + deltaW);

                    // 计算实际的宽度变化（考虑最小尺寸限制）
                    double actualDeltaW = newHalfW - halfW;

                    // 中心需要移动以保持对边固定
                    // 固定边在局部坐标中的位置：
                    // - MiddleLeft 固定右边，右边缘在局部 (+halfW, 0)，中心向-X方向移动
                    // - MiddleRight 固定左边，左边缘在局部 (-halfW, 0)，中心向+X方向移动
                    double centerShiftSign = (_activeResizeHandle == ResizeHandle.MiddleLeft) ? -1 : 1;
                    double centerShiftX = actualDeltaW * cosA * centerShiftSign;
                    double centerShiftY = actualDeltaW * sinA * centerShiftSign;
                    newCenterX = centerX + centerShiftX;
                    newCenterY = centerY + centerShiftY;
                }
                break;
        }

        double newX = newCenterX - newHalfW;
        double newY = newCenterY - newHalfH;
        double newWidth = newHalfW * 2;
        double newHeight = newHalfH * 2;

        ApplyResize(newX, newY, newWidth, newHeight);
    }

    private Point GetFixedCornerWorld(
        ResizeHandle handle, double centerX, double centerY,
        double halfW, double halfH, double cosA, double sinA)
    {
        Point localFixed = handle switch
        {
            ResizeHandle.TopLeft => new Point(halfW, halfH),
            ResizeHandle.TopRight => new Point(-halfW, halfH),
            ResizeHandle.BottomLeft => new Point(halfW, -halfH),
            ResizeHandle.BottomRight => new Point(-halfW, -halfH),
            ResizeHandle.TopCenter => new Point(0, halfH),
            ResizeHandle.BottomCenter => new Point(0, -halfH),
            ResizeHandle.MiddleLeft => new Point(halfW, 0),
            ResizeHandle.MiddleRight => new Point(-halfW, 0),
            _ => new Point(0, 0)
        };

        return new Point(
            centerX + localFixed.X * cosA - localFixed.Y * sinA,
            centerY + localFixed.X * sinA + localFixed.Y * cosA);
    }

    private void ApplyResizeFromCorners(int topLeftX, int topLeftY, int topRightX, int topRightY,
        int bottomLeftX, int bottomLeftY, int bottomRightX, int bottomRightY, double rotation)
    {
        if (rotation == 0)
        {
            int minX = Math.Min(Math.Min(topLeftX, topRightX), Math.Min(bottomLeftX, bottomRightX));
            int minY = Math.Min(Math.Min(topLeftY, topRightY), Math.Min(bottomLeftY, bottomRightY));
            int maxX = Math.Max(Math.Max(topLeftX, topRightX), Math.Max(bottomLeftX, bottomRightX));
            int maxY = Math.Max(Math.Max(topLeftY, topRightY), Math.Max(bottomLeftY, bottomRightY));
            ApplyResize(minX, minY, maxX - minX, maxY - minY);
        }
        else
        {
            double rad = rotation * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            int[] visX = { topLeftX, topRightX, bottomRightX, bottomLeftX };
            int[] visY = { topLeftY, topRightY, bottomRightY, bottomLeftY };

            double centerX = (topLeftX + topRightX + bottomLeftX + bottomRightX) / 4.0;
            double centerY = (topLeftY + topRightY + bottomLeftY + bottomRightY) / 4.0;

            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;

            for (int i = 0; i < 4; i++)
            {
                double dx = visX[i] - centerX;
                double dy = visY[i] - centerY;
                double localX = dx * cosA - dy * sinA + centerX;
                double localY = dx * sinA + dy * cosA + centerY;
                minX = Math.Min(minX, localX);
                minY = Math.Min(minY, localY);
                maxX = Math.Max(maxX, localX);
                maxY = Math.Max(maxY, localY);
            }

            int finalWidth = (int)Math.Round(maxX - minX);
            int finalHeight = (int)Math.Round(maxY - minY);
            if (finalWidth < MinElementSize) finalWidth =(int) MinElementSize;
            if (finalHeight < MinElementSize) finalHeight = (int)MinElementSize;

            ApplyResize((int)Math.Round(minX), (int)Math.Round(minY), finalWidth, finalHeight);
        }
    }

    private void OnResizeHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isResizing && _selectedElement != null)
        {
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            ElementResized?.Invoke(this, new SingleElementResizedEventArgs(_selectedElement));
        }
    }

    private void ApplyResize(double newX, double newY, double newWidth, double newHeight)
    {
        if (_selectedElement == null) return;

        _selectedElement.X = newX;
        _selectedElement.Y = newY;
        _selectedElement.Width = newWidth;
        _selectedElement.Height = newHeight;

        if (_elementControlMap?.TryGetValue(_selectedElement, out var control) == true && _renderControl != null)
        {
            if (control is IElementControlBase ecb)
            {
                // 判断父容器是否为顶级Canvas
                if (this.Parent == ((Panel?)control.Parent)?.Parent && control.Parent is Canvas)
                {
                    // 顶级Canvas：直接使用X/Y
                    _selectedElement.Left = newX;
                    _selectedElement.Top = newY;
                    ecb.UpdateLeft(newX);
                    ecb.UpdateTop(newY);
                }
                else if (control.Parent is Canvas parentCanvas)
                {
                    // 子Canvas：需要计算相对于子Canvas的位置
                    var pos = parentCanvas.TranslatePoint(new Point(0, 0), this);
                    if (pos.HasValue)
                    {
                        _selectedElement.Left = newX - pos.Value.X;
                        _selectedElement.Top = newY - pos.Value.Y;
                        ecb.UpdateLeft(_selectedElement.Left);
                        ecb.UpdateTop(_selectedElement.Top);
                    }
                    else
                    {
                        ecb.UpdateLeft(newX);
                        ecb.UpdateTop(newY);
                    }
                }
                else
                {
                    ecb.UpdateLeft(newX);
                    ecb.UpdateTop(newY);
                }
                ecb.UpdateWidth(newWidth);
                ecb.UpdateHeight(newHeight);
            }
            else
            {
                _renderControl.UpdateX(_selectedElement, control);
                _renderControl.UpdateY(_selectedElement, control);
            }
        }
    }

    #endregion

    #region 旋转

    private void OnRotateButtonPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_selectedElement == null) return;

        _isRotating = true;
        _rotateStartPoint = e.GetPosition(this);

        if (_elementControlMap?.TryGetValue(_selectedElement, out var control) == true)
        {
            var corners = GetElementVisualCorners(control, _selectedElement);
            if (corners != null)
                _rotateCenterPoint = corners.Value.Center;
        }

        _selectionRect.IsVisible = false;
        _rotatedSelectionPolygon.IsVisible = false;
        HideResizeHandles();

        Canvas.SetLeft(_rotateCenterIndicator, _rotateCenterPoint.X - 3);
        Canvas.SetTop(_rotateCenterIndicator, _rotateCenterPoint.Y - 3);
        _rotateCenterIndicator.IsVisible = true;

        // 半径 = 中心到右上角的距离（对角线的一半）
        double halfW = _selectedElement.Width / 2;
        double halfH = _selectedElement.Height / 2;
        _rotateRadius = Math.Sqrt(halfW * halfW + halfH * halfH) + 10;

        _rotateGuideCircle.Width = _rotateRadius * 2;
        _rotateGuideCircle.Height = _rotateRadius * 2;
        Canvas.SetLeft(_rotateGuideCircle, _rotateCenterPoint.X - _rotateRadius);
        Canvas.SetTop(_rotateGuideCircle, _rotateCenterPoint.Y - _rotateRadius);
        _rotateGuideCircle.IsVisible = true;

        // 初始角度：当前元素旋转下，旋转把相对于圆心的角度
        // top-right 在元素局部坐标中的角度
        double initialAngle = Math.Atan2(-halfH, halfW);
        // 旋转把在圆周上的实际角度 = 元素旋转 + 初始角度
        double finalAngleRad = _selectedElement.Rotation * Math.PI / 180 + initialAngle;
        // 用圆几何计算起始角度（而非从按钮屏幕位置读），保证与 Moved 中的约束一致
        _rotateStartAngle = finalAngleRad * 180 / Math.PI;

        _elementStartRotation = _selectedElement.Rotation;

        e.Handled = true;
    }

    private void OnRotateButtonMoved(object? sender, PointerEventArgs e)
    {
        if (!_isRotating || _selectedElement == null) return;

        bool isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        var currentPoint = e.GetPosition(this);

        double dx = currentPoint.X - _rotateCenterPoint.X;
        double dy = currentPoint.Y - _rotateCenterPoint.Y;
        double currentAngleRad = Math.Atan2(dy, dx);

        // 约束到固定半径的圆周上
        double constrainedX = _rotateCenterPoint.X + _rotateRadius * Math.Cos(currentAngleRad);
        double constrainedY = _rotateCenterPoint.Y + _rotateRadius * Math.Sin(currentAngleRad);
        var constrainedPoint = new Point(constrainedX, constrainedY);

        Canvas.SetLeft(_rotateButton, constrainedPoint.X - _rotateButton.Bounds.Width / 2);
        Canvas.SetTop(_rotateButton, constrainedPoint.Y - _rotateButton.Bounds.Height / 2);

        double currentAngleDeg = currentAngleRad * 180 / Math.PI;
        double angleDelta = currentAngleDeg - _rotateStartAngle;
        double newRotation = _elementStartRotation + angleDelta;

        _rotateButton.RenderTransform = new RotateTransform(newRotation);

        if (isCtrlPressed)
        {
            double snappedAngle = Math.Round(newRotation / SnapAngleStep) * SnapAngleStep;
            newRotation = snappedAngle;
            DrawAngleReferenceLines(snappedAngle);
        }
        else
        {
            HideAngleReferenceLines();
        }

        ApplyRotation(newRotation);
        UpdateOverlay();

    }

    private void OnRotateButtonReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isRotating || _selectedElement == null) return;

        _isRotating = false;

        _rotateCenterIndicator.IsVisible = false;
        _rotateGuideCircle.IsVisible = false;
        HideAngleReferenceLines();

        UpdateOverlay();
        ElementRotated?.Invoke(this, new SingleElementRotatedEventArgs(_selectedElement, _selectedElement.Rotation));
    }

    private void ApplyRotation(double newRotation)
    {
        if (_selectedElement == null) return;

        _selectedElement.Rotation = newRotation;

        if (_elementControlMap?.TryGetValue(_selectedElement, out var control) == true &&
            control is IElementControlBase ecb)
        {
            ecb.UpdateRotation(newRotation);
        }
    }

    private void DrawAngleReferenceLines(double snappedAngle)
    {
        double rad = snappedAngle * Math.PI / 180;
        double length = 2000;
        double perpRad = (snappedAngle + 90) * Math.PI / 180;

        ShowAngleReferenceLine(0,
            _rotateCenterPoint.X - length * Math.Cos(rad),
            _rotateCenterPoint.Y - length * Math.Sin(rad),
            _rotateCenterPoint.X + length * Math.Cos(rad),
            _rotateCenterPoint.Y + length * Math.Sin(rad));

        ShowAngleReferenceLine(1,
            _rotateCenterPoint.X - length * Math.Cos(perpRad),
            _rotateCenterPoint.Y - length * Math.Sin(perpRad),
            _rotateCenterPoint.X + length * Math.Cos(perpRad),
            _rotateCenterPoint.Y + length * Math.Sin(perpRad));
    }

    private void ShowAngleReferenceLine(int index, double x1, double y1, double x2, double y2)
    {
        if (index < 0 || index >= _angleReferenceLines.Length) return;
        _angleReferenceLines[index].StartPoint = new Point(x1, y1);
        _angleReferenceLines[index].EndPoint = new Point(x2, y2);
        _angleReferenceLines[index].IsVisible = true;
    }

    private void HideAngleReferenceLines()
    {
        foreach (var line in _angleReferenceLines)
            line.IsVisible = false;
    }

    #endregion
}