using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;

namespace yuyiAvalonia.ImageEdit.Helpers;

/// <summary>
/// 选择框调整帮助类
/// 管理选择框的 8 个调整手柄（4个角 + 4个边），支持拖拽调整大小
/// </summary>
public class SelectionHelper
{
    private readonly Canvas _canvas;
    private readonly Image _image;
    private double _zoomLevel;

    // 手柄大小
    private const double HandleSize = 10;

    // 8 个手柄 Border 控件
    private Border? _handleTopLeft;
    private Border? _handleTopRight;
    private Border? _handleBottomLeft;
    private Border? _handleBottomRight;
    private Border? _handleTop;
    private Border? _handleBottom;
    private Border? _handleLeft;
    private Border? _handleRight;

    // 选择框移动区域
    private Border? _moveArea;

    // 当前调整状态
    private HandleType _currentHandle = HandleType.None;
    private Point _startDragPoint;
    private Rect _startRect;

    // 回调函数
    private Action<Rect>? _onSelectionChanged;

    /// <summary>
    /// 手柄类型
    /// </summary>
    private enum HandleType
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Top,
        Bottom,
        Left,
        Right,
        Move // 移动整个选择框
    }

    public SelectionHelper(Canvas canvas, Image image, double zoomLevel)
    {
        _canvas = canvas;
        _image = image;
        _zoomLevel = zoomLevel;
    }

    /// <summary>
    /// 初始化选择框和手柄
    /// </summary>
    public void Initialize(Rect selectionRect, Action<Rect>? onSelectionChanged = null)
    {
        _onSelectionChanged = onSelectionChanged;
        CreateHandles();
        UpdateHandlePositions(selectionRect);
    }

    /// <summary>
    /// 更新选择框位置
    /// </summary>
    public void UpdateSelection(Rect selectionRect)
    {
        UpdateHandlePositions(selectionRect);
    }

    /// <summary>
    /// 更新缩放级别
    /// </summary>
    public void UpdateZoomLevel(double newZoomLevel)
    {
        _zoomLevel = newZoomLevel;
        // 重新计算手柄位置
        if (_lastRect.Width > 0 && _lastRect.Height > 0)
        {
            UpdateHandlePositions(_lastRect);
        }
    }

    private Rect _lastRect;

    // 图像实际像素大小（用于边界检查）
    private double _imageWidth;
    private double _imageHeight;

    /// <summary>
    /// 设置图像大小（用于边界检查）
    /// </summary>
    public void SetImageSize(double width, double height)
    {
        _imageWidth = width;
        _imageHeight = height;
    }

    /// <summary>
    /// 销毁手柄
    /// </summary>
    public void Dispose()
    {
        RemoveHandles();
    }

    private void CreateHandles()
    {
        RemoveHandles();

        var handleBrush = new SolidColorBrush(Colors.White);
        var handleBorderBrush = new SolidColorBrush(Color.Parse("#00AAFF"));

        // 创建 8 个手柄
        _handleTopLeft = CreateHandle("tl", handleBrush, handleBorderBrush);
        _handleTopRight = CreateHandle("tr", handleBrush, handleBorderBrush);
        _handleBottomLeft = CreateHandle("bl", handleBrush, handleBorderBrush);
        _handleBottomRight = CreateHandle("br", handleBrush, handleBorderBrush);
        _handleTop = CreateHandle("t", handleBrush, handleBorderBrush);
        _handleBottom = CreateHandle("b", handleBrush, handleBorderBrush);
        _handleLeft = CreateHandle("l", handleBrush, handleBorderBrush);
        _handleRight = CreateHandle("r", handleBrush, handleBorderBrush);

        // 创建移动区域（选择框内部区域）
        _moveArea = CreateMoveArea();

        _canvas.Children.Add(_handleTopLeft);
        _canvas.Children.Add(_handleTopRight);
        _canvas.Children.Add(_handleBottomLeft);
        _canvas.Children.Add(_handleBottomRight);
        _canvas.Children.Add(_handleTop);
        _canvas.Children.Add(_handleBottom);
        _canvas.Children.Add(_handleLeft);
        _canvas.Children.Add(_handleRight);
        _canvas.Children.Add(_moveArea);

        // 订阅事件
        AttachHandleEvents(_handleTopLeft, HandleType.TopLeft);
        AttachHandleEvents(_handleTopRight, HandleType.TopRight);
        AttachHandleEvents(_handleBottomLeft, HandleType.BottomLeft);
        AttachHandleEvents(_handleBottomRight, HandleType.BottomRight);
        AttachHandleEvents(_handleTop, HandleType.Top);
        AttachHandleEvents(_handleBottom, HandleType.Bottom);
        AttachHandleEvents(_handleLeft, HandleType.Left);
        AttachHandleEvents(_handleRight, HandleType.Right);

        // 订阅移动区域事件
        AttachMoveAreaEvents();
    }

    private Border CreateMoveArea()
    {
        return new Border
        {
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
            ZIndex = 50
        };
    }

    private void AttachMoveAreaEvents()
    {
        if (_moveArea == null) return;

        _moveArea.PointerPressed += (s, e) =>
        {
            var point = e.GetCurrentPoint(_canvas);
            if (point.Properties.IsLeftButtonPressed)
            {
                _currentHandle = HandleType.Move;
                _startDragPoint = e.GetPosition(_canvas);
                _startRect = GetSelectionRectFromCanvas();
                e.Handled = true;
            }
        };

        _moveArea.PointerMoved += (s, e) =>
        {
            // 鼠标悬停时显示移动光标
            if (_currentHandle == HandleType.None)
            {
                _moveArea.Cursor = new Cursor(StandardCursorType.SizeAll);
            }

            if (_currentHandle == HandleType.Move)
            {
                var currentPoint = e.GetPosition(_canvas);
                var delta = currentPoint - _startDragPoint;

                var imageDeltaX = delta.X / _zoomLevel;
                var imageDeltaY = delta.Y / _zoomLevel;

                // 移动整个选择框
                var newX = _startRect.X + imageDeltaX;
                var newY = _startRect.Y + imageDeltaY;

                var newRect = new Rect(newX, newY, _startRect.Width, _startRect.Height);
                UpdateHandlePositions(newRect);
                _onSelectionChanged?.Invoke(newRect);
                e.Handled = true;
            }
        };

        _moveArea.PointerReleased += (s, e) =>
        {
            if (_currentHandle == HandleType.Move)
            {
                _currentHandle = HandleType.None;
                e.Handled = true;
            }
        };
    }

    private Border CreateHandle(string tag, IBrush background, IBrush borderBrush)
    {
        return new Border
        {
            Width = HandleSize,
            Height = HandleSize,
            Background = background,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            IsHitTestVisible = true,
            ZIndex = 100 // 确保手柄显示在最上层
        };
    }

    private void AttachHandleEvents(Border handle, HandleType handleType)
    {
        handle.PointerPressed += (s, e) =>
        {
            var point = e.GetCurrentPoint(_canvas);
            if (point.Properties.IsLeftButtonPressed)
            {
                _currentHandle = handleType;
                // 获取相对于 Canvas 的起始拖动点
                _startDragPoint = e.GetPosition(_canvas);
                _startRect = GetSelectionRectFromCanvas();
                e.Handled = true;
            }
        };

        handle.PointerMoved += (s, e) =>
        {
            // 鼠标悬停时改变光标
            UpdateCursor(handleType);

            if (_currentHandle != HandleType.None)
            {
                // 获取相对于 Canvas 的当前位置
                var currentPoint = e.GetPosition(_canvas);
                var delta = currentPoint - _startDragPoint;

                // 将 Canvas 坐标差转换为图像坐标差
                var imageDeltaX = delta.X / _zoomLevel;
                var imageDeltaY = delta.Y / _zoomLevel;
                var imageDelta = new Point(imageDeltaX, imageDeltaY);

                var newRect = CalculateNewRect(_startRect, imageDelta, _currentHandle);

                UpdateHandlePositions(newRect);
                _onSelectionChanged?.Invoke(newRect);
                e.Handled = true;
            }
        };

        handle.PointerReleased += (s, e) =>
        {
            if (_currentHandle != HandleType.None)
            {
                _currentHandle = HandleType.None;
                e.Handled = true;
            }
        };
    }

    private void UpdateCursor(HandleType handleType)
    {
        var cursor = handleType switch
        {
            // 对角线调整使用 SizeAll 代替（或者使用其他可用的光标）
            HandleType.TopLeft or HandleType.BottomRight => new Cursor(StandardCursorType.SizeAll),
            HandleType.TopRight or HandleType.BottomLeft => new Cursor(StandardCursorType.SizeAll),
            HandleType.Top or HandleType.Bottom => new Cursor(StandardCursorType.SizeNorthSouth),
            HandleType.Left or HandleType.Right => new Cursor(StandardCursorType.SizeWestEast),
            HandleType.Move => new Cursor(StandardCursorType.SizeAll),
            _ => new Cursor(StandardCursorType.Arrow)
        };

        // 设置手柄光标
        if (_handleTopLeft != null) _handleTopLeft.Cursor = cursor;
        if (_handleTopRight != null) _handleTopRight.Cursor = cursor;
        if (_handleBottomLeft != null) _handleBottomLeft.Cursor = cursor;
        if (_handleBottomRight != null) _handleBottomRight.Cursor = cursor;
        if (_handleTop != null) _handleTop.Cursor = cursor;
        if (_handleBottom != null) _handleBottom.Cursor = cursor;
        if (_handleLeft != null) _handleLeft.Cursor = cursor;
        if (_handleRight != null) _handleRight.Cursor = cursor;

        // 设置选择框移动区域的光标
        if (_moveArea != null)
        {
            _moveArea.Cursor = new Cursor(StandardCursorType.SizeAll);
        }
    }

    private Rect CalculateNewRect(Rect originalRect, Point delta, HandleType handleType)
    {
        var x = originalRect.X;
        var y = originalRect.Y;
        var width = originalRect.Width;
        var height = originalRect.Height;

        // delta 已经是图像坐标差
        var imageDeltaX = delta.X;
        var imageDeltaY = delta.Y;

        switch (handleType)
        {
            case HandleType.TopLeft:
                // 新位置不能超出左边界
                var newX1 = x + imageDeltaX;
                if (newX1 < 0)
                {
                    imageDeltaX = -x; // 限制只能移动到边界
                }
                // 新位置不能超出上边界
                var newY1 = y + imageDeltaY;
                if (newY1 < 0)
                {
                    imageDeltaY = -y;
                }
                x += imageDeltaX;
                y += imageDeltaY;
                width -= imageDeltaX;
                height -= imageDeltaY;
                break;
            case HandleType.TopRight:
                // 新位置不能超出上边界
                var newY2 = y + imageDeltaY;
                if (newY2 < 0)
                {
                    imageDeltaY = -y;
                }
                // 右边界检查
                var newRight2 = x + width + imageDeltaX;
                if (newRight2 > _imageWidth)
                {
                    imageDeltaX = _imageWidth - x - width;
                }
                y += imageDeltaY;
                width += imageDeltaX;
                height -= imageDeltaY;
                break;
            case HandleType.BottomLeft:
                // 新位置不能超出左边界
                var newX3 = x + imageDeltaX;
                if (newX3 < 0)
                {
                    imageDeltaX = -x;
                }
                // 下边界检查
                var newBottom3 = y + height + imageDeltaY;
                if (newBottom3 > _imageHeight)
                {
                    imageDeltaY = _imageHeight - y - height;
                }
                x += imageDeltaX;
                width -= imageDeltaX;
                height += imageDeltaY;
                break;
            case HandleType.BottomRight:
                // 右边界检查
                var newRight4 = x + width + imageDeltaX;
                if (newRight4 > _imageWidth)
                {
                    imageDeltaX = _imageWidth - x - width;
                }
                // 下边界检查
                var newBottom4 = y + height + imageDeltaY;
                if (newBottom4 > _imageHeight)
                {
                    imageDeltaY = _imageHeight - y - height;
                }
                width += imageDeltaX;
                height += imageDeltaY;
                break;
            case HandleType.Top:
                // 新位置不能超出上边界
                var newY5 = y + imageDeltaY;
                if (newY5 < 0)
                {
                    imageDeltaY = -y;
                }
                y += imageDeltaY;
                height -= imageDeltaY;
                break;
            case HandleType.Bottom:
                // 下边界检查
                var newBottom6 = y + height + imageDeltaY;
                if (newBottom6 > _imageHeight)
                {
                    imageDeltaY = _imageHeight - y - height;
                }
                height += imageDeltaY;
                break;
            case HandleType.Left:
                // 新位置不能超出左边界
                var newX7 = x + imageDeltaX;
                if (newX7 < 0)
                {
                    imageDeltaX = -x;
                }
                x += imageDeltaX;
                width -= imageDeltaX;
                break;
            case HandleType.Right:
                // 右边界检查
                var newRight8 = x + width + imageDeltaX;
                if (newRight8 > _imageWidth)
                {
                    imageDeltaX = _imageWidth - x - width;
                }
                width += imageDeltaX;
                break;
        }

        // 确保宽高为正
        if (width < 0)
        {
            x += width;
            width = -width;
        }
        if (height < 0)
        {
            y += height;
            height = -height;
        }

        return new Rect(x, y, width, height);
    }

    private Rect GetSelectionRectFromCanvas()
    {
        // 返回当前存储的选择矩形
        return _lastRect;
    }

    private void UpdateHandlePositions(Rect selectionRect)
    {
        _lastRect = selectionRect;

        // 将图像坐标转换为 Canvas 坐标
        var scaledX = selectionRect.X * _zoomLevel;
        var scaledY = selectionRect.Y * _zoomLevel;
        var scaledWidth = selectionRect.Width * _zoomLevel;
        var scaledHeight = selectionRect.Height * _zoomLevel;

        // 更新移动区域位置
        if (_moveArea != null)
        {
            Canvas.SetLeft(_moveArea, scaledX);
            Canvas.SetTop(_moveArea, scaledY);
            _moveArea.Width = Math.Abs(scaledWidth);
            _moveArea.Height = Math.Abs(scaledHeight);
        }

        // 更新 8 个手柄的位置
        Canvas.SetLeft(_handleTopLeft!, scaledX - HandleSize / 2);
        Canvas.SetTop(_handleTopLeft!, scaledY - HandleSize / 2);

        Canvas.SetLeft(_handleTopRight!, scaledX + scaledWidth - HandleSize / 2);
        Canvas.SetTop(_handleTopRight!, scaledY - HandleSize / 2);

        Canvas.SetLeft(_handleBottomLeft!, scaledX - HandleSize / 2);
        Canvas.SetTop(_handleBottomLeft!, scaledY + scaledHeight - HandleSize / 2);

        Canvas.SetLeft(_handleBottomRight!, scaledX + scaledWidth - HandleSize / 2);
        Canvas.SetTop(_handleBottomRight!, scaledY + scaledHeight - HandleSize / 2);

        // 边上的手柄
        Canvas.SetLeft(_handleTop!, scaledX + scaledWidth / 2 - HandleSize / 2);
        Canvas.SetTop(_handleTop!, scaledY - HandleSize / 2);

        Canvas.SetLeft(_handleBottom!, scaledX + scaledWidth / 2 - HandleSize / 2);
        Canvas.SetTop(_handleBottom!, scaledY + scaledHeight - HandleSize / 2);

        Canvas.SetLeft(_handleLeft!, scaledX - HandleSize / 2);
        Canvas.SetTop(_handleLeft!, scaledY + scaledHeight / 2 - HandleSize / 2);

        Canvas.SetLeft(_handleRight!, scaledX + scaledWidth - HandleSize / 2);
        Canvas.SetTop(_handleRight!, scaledY + scaledHeight / 2 - HandleSize / 2);

        // 显示/隐藏手柄
        var hasSelection = selectionRect.Width > 5 && selectionRect.Height > 5;
        SetHandlesVisible(hasSelection);
    }

    private void SetHandlesVisible(bool visible)
    {
        if (_handleTopLeft != null) _handleTopLeft.IsVisible = visible;
        if (_handleTopRight != null) _handleTopRight.IsVisible = visible;
        if (_handleBottomLeft != null) _handleBottomLeft.IsVisible = visible;
        if (_handleBottomRight != null) _handleBottomRight.IsVisible = visible;
        if (_handleTop != null) _handleTop.IsVisible = visible;
        if (_handleBottom != null) _handleBottom.IsVisible = visible;
        if (_handleLeft != null) _handleLeft.IsVisible = visible;
        if (_handleRight != null) _handleRight.IsVisible = visible;
        if (_moveArea != null) _moveArea.IsVisible = visible;
    }

    public void HideHandles()
    {
        SetHandlesVisible(false);
    }

    public void ShowHandles()
    {
        SetHandlesVisible(true);
    }

    private void RemoveHandles()
    {
        if (_handleTopLeft != null) { _canvas.Children.Remove(_handleTopLeft); _handleTopLeft = null; }
        if (_handleTopRight != null) { _canvas.Children.Remove(_handleTopRight); _handleTopRight = null; }
        if (_handleBottomLeft != null) { _canvas.Children.Remove(_handleBottomLeft); _handleBottomLeft = null; }
        if (_handleBottomRight != null) { _canvas.Children.Remove(_handleBottomRight); _handleBottomRight = null; }
        if (_handleTop != null) { _canvas.Children.Remove(_handleTop); _handleTop = null; }
        if (_handleBottom != null) { _canvas.Children.Remove(_handleBottom); _handleBottom = null; }
        if (_handleLeft != null) { _canvas.Children.Remove(_handleLeft); _handleLeft = null; }
        if (_handleRight != null) { _canvas.Children.Remove(_handleRight); _handleRight = null; }
        if (_moveArea != null) { _canvas.Children.Remove(_moveArea); _moveArea = null; }
    }
}
