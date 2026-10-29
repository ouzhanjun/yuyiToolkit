using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace yuyiAvalonia.ImageEdit.Controls;

/// <summary>
/// 选择框叠加层控件
/// 管理选择框的 8 个调整手柄（4个角 + 4个边），支持拖拽调整大小和移动
/// </summary>
public partial class SelectionOverlayControl : UserControl
{
    public static readonly StyledProperty<Rect> SelectionRectProperty =
        AvaloniaProperty.Register<SelectionOverlayControl, Rect>(nameof(SelectionRect), default);

    public static readonly StyledProperty<double> ZoomLevelProperty =
        AvaloniaProperty.Register<SelectionOverlayControl, double>(nameof(ZoomLevel), 1.0);

    public static readonly StyledProperty<double> ImageWidthProperty =
        AvaloniaProperty.Register<SelectionOverlayControl, double>(nameof(ImageWidth), 0);

    public static readonly StyledProperty<double> ImageHeightProperty =
        AvaloniaProperty.Register<SelectionOverlayControl, double>(nameof(ImageHeight), 0);

    public static readonly StyledProperty<bool> IsSelectionVisibleProperty =
        AvaloniaProperty.Register<SelectionOverlayControl, bool>(nameof(IsSelectionVisible), false);

    public Rect SelectionRect
    {
        get => GetValue(SelectionRectProperty);
        set => SetValue(SelectionRectProperty, value);
    }

    public double ZoomLevel
    {
        get => GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public double ImageWidth
    {
        get => GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public double ImageHeight
    {
        get => GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public bool IsSelectionVisible
    {
        get => GetValue(IsSelectionVisibleProperty);
        set => SetValue(IsSelectionVisibleProperty, value);
    }

    public event Action<Rect>? SelectionChanged;

    /// <summary>
    /// 滚轮事件（用于转发到 View 处理缩放）
    /// </summary>
    public event Action<PointerWheelEventArgs>? WheelChanged;

    private HandleType _currentHandle = HandleType.None;
    private Point _startDragPoint;
    private Rect _startRect;
    private const double HandleSize = 10;

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
        Move
    }

    public SelectionOverlayControl()
    {
        InitializeComponent();

        // 订阅属性变化
        this.PropertyChanged += OnPropertyChangedHandler;
    }

    private void OnPropertyChangedHandler(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == SelectionRectProperty || 
            e.Property == ZoomLevelProperty ||
            e.Property == IsSelectionVisibleProperty)
        {
            UpdateHandlePositions();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachEvents();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        // 触发滚轮事件，让外部处理
        WheelChanged?.Invoke(e);
        base.OnPointerWheelChanged(e);
    }

    private void AttachEvents()
    {
        // 订阅手柄事件
        AttachHandleEvents(HandleTopLeft, HandleType.TopLeft);
        AttachHandleEvents(HandleTopRight, HandleType.TopRight);
        AttachHandleEvents(HandleBottomLeft, HandleType.BottomLeft);
        AttachHandleEvents(HandleBottomRight, HandleType.BottomRight);
        AttachHandleEvents(HandleTop, HandleType.Top);
        AttachHandleEvents(HandleBottom, HandleType.Bottom);
        AttachHandleEvents(HandleLeft, HandleType.Left);
        AttachHandleEvents(HandleRight, HandleType.Right);

        // 订阅移动区域事件
        AttachMoveAreaEvents();
    }

    private void AttachMoveAreaEvents()
    {
        MoveArea.PointerPressed += (s, e) =>
        {
            var point = e.GetCurrentPoint(SelectionCanvas);
            if (point.Properties.IsLeftButtonPressed)
            {
                _currentHandle = HandleType.Move;
                _startDragPoint = e.GetPosition(SelectionCanvas);
                _startRect = SelectionRect;
                e.Handled = true;
            }
        };

        MoveArea.PointerMoved += (s, e) =>
        {
            if (_currentHandle == HandleType.None)
            {
                MoveArea.Cursor = new Cursor(StandardCursorType.SizeAll);
            }

            if (_currentHandle == HandleType.Move)
            {
                var currentPoint = e.GetPosition(SelectionCanvas);
                var delta = currentPoint - _startDragPoint;

                // delta 是屏幕坐标变化，转换为图像坐标
                var newX = _startRect.X + delta.X / ZoomLevel;
                var newY = _startRect.Y + delta.Y / ZoomLevel;

                // 边界检查：不能超出图像边界，但保持原大小
                if (newX < 0) newX = 0;
                if (newY < 0) newY = 0;
                if (newX + _startRect.Width > ImageWidth) newX = ImageWidth - _startRect.Width;
                if (newY + _startRect.Height > ImageHeight) newY = ImageHeight - _startRect.Height;

                var newRect = new Rect(newX, newY, _startRect.Width, _startRect.Height);
                SelectionRect = newRect;
                
                // 回调传图像坐标
                SelectionChanged?.Invoke(newRect);
                e.Handled = true;
            }
        };

        MoveArea.PointerReleased += (s, e) =>
        {
            if (_currentHandle == HandleType.Move)
            {
                _currentHandle = HandleType.None;
                e.Handled = true;
            }
        };
    }

    private void AttachHandleEvents(Border handle, HandleType handleType)
    {
        handle.PointerPressed += (s, e) =>
        {
            var point = e.GetCurrentPoint(SelectionCanvas);
            if (point.Properties.IsLeftButtonPressed)
            {
                _currentHandle = handleType;
                _startDragPoint = e.GetPosition(SelectionCanvas);
                _startRect = SelectionRect;
                e.Handled = true;
            }
        };

        handle.PointerMoved += (s, e) =>
        {
            UpdateCursor(handleType);

            if (_currentHandle != HandleType.None)
            {
                var currentPoint = e.GetPosition(SelectionCanvas);
                var delta = currentPoint - _startDragPoint;

                // delta 是屏幕坐标变化，需要转换为图像坐标变化
                var imageDeltaX = delta.X / ZoomLevel;
                var imageDeltaY = delta.Y / ZoomLevel;
                var imageDelta = new Point(imageDeltaX, imageDeltaY);

                // _startRect 是图像坐标，CalculateNewRect 计算出的也是图像坐标
                var newRect = CalculateNewRect(_startRect, imageDelta, handleType);

                // 直接存图像坐标到 SelectionRect
                SelectionRect = newRect;
                SelectionChanged?.Invoke(newRect);
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
            HandleType.TopLeft or HandleType.BottomRight => new Cursor(StandardCursorType.SizeAll),
            HandleType.TopRight or HandleType.BottomLeft => new Cursor(StandardCursorType.SizeAll),
            HandleType.Top or HandleType.Bottom => new Cursor(StandardCursorType.SizeNorthSouth),
            HandleType.Left or HandleType.Right => new Cursor(StandardCursorType.SizeWestEast),
            HandleType.Move => new Cursor(StandardCursorType.SizeAll),
            _ => new Cursor(StandardCursorType.Arrow)
        };

        SetHandleCursor(HandleTopLeft, cursor);
        SetHandleCursor(HandleTopRight, cursor);
        SetHandleCursor(HandleBottomLeft, cursor);
        SetHandleCursor(HandleBottomRight, cursor);
        SetHandleCursor(HandleTop, cursor);
        SetHandleCursor(HandleBottom, cursor);
        SetHandleCursor(HandleLeft, cursor);
        SetHandleCursor(HandleRight, cursor);
        MoveArea.Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    private void SetHandleCursor(Border handle, Cursor cursor)
    {
        if (handle != null)
            handle.Cursor = cursor;
    }

    private Rect CalculateNewRect(Rect originalRect, Point delta, HandleType handleType)
    {
        var x = originalRect.X;
        var y = originalRect.Y;
        var width = originalRect.Width;
        var height = originalRect.Height;

        var imageDeltaX = delta.X;
        var imageDeltaY = delta.Y;

        switch (handleType)
        {
            case HandleType.TopLeft:
                var newX1 = x + imageDeltaX;
                if (newX1 < 0) imageDeltaX = -x;
                var newY1 = y + imageDeltaY;
                if (newY1 < 0) imageDeltaY = -y;
                x += imageDeltaX;
                y += imageDeltaY;
                width -= imageDeltaX;
                height -= imageDeltaY;
                break;
            case HandleType.TopRight:
                var newY2 = y + imageDeltaY;
                if (newY2 < 0) imageDeltaY = -y;
                var newRight2 = x + width + imageDeltaX;
                if (newRight2 > ImageWidth) imageDeltaX = ImageWidth - x - width;
                y += imageDeltaY;
                width += imageDeltaX;
                height -= imageDeltaY;
                break;
            case HandleType.BottomLeft:
                var newX3 = x + imageDeltaX;
                if (newX3 < 0) imageDeltaX = -x;
                var newBottom3 = y + height + imageDeltaY;
                if (newBottom3 > ImageHeight) imageDeltaY = ImageHeight - y - height;
                x += imageDeltaX;
                width -= imageDeltaX;
                height += imageDeltaY;
                break;
            case HandleType.BottomRight:
                var newRight4 = x + width + imageDeltaX;
                if (newRight4 > ImageWidth) imageDeltaX = ImageWidth - x - width;
                var newBottom4 = y + height + imageDeltaY;
                if (newBottom4 > ImageHeight) imageDeltaY = ImageHeight - y - height;
                width += imageDeltaX;
                height += imageDeltaY;
                break;
            case HandleType.Top:
                var newY5 = y + imageDeltaY;
                if (newY5 < 0) imageDeltaY = -y;
                y += imageDeltaY;
                height -= imageDeltaY;
                break;
            case HandleType.Bottom:
                var newBottom6 = y + height + imageDeltaY;
                if (newBottom6 > ImageHeight) imageDeltaY = ImageHeight - y - height;
                height += imageDeltaY;
                break;
            case HandleType.Left:
                var newX7 = x + imageDeltaX;
                if (newX7 < 0) imageDeltaX = -x;
                x += imageDeltaX;
                width -= imageDeltaX;
                break;
            case HandleType.Right:
                var newRight8 = x + width + imageDeltaX;
                if (newRight8 > ImageWidth) imageDeltaX = ImageWidth - x - width;
                width += imageDeltaX;
                break;
        }

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

    private void UpdateHandlePositions()
    {
        var selectionRect = SelectionRect;
        var hasSelection = selectionRect.Width > 5 && selectionRect.Height > 5 && IsSelectionVisible;

        var visible = hasSelection;
        SetHandleVisible(HandleTopLeft, visible);
        SetHandleVisible(HandleTopRight, visible);
        SetHandleVisible(HandleBottomLeft, visible);
        SetHandleVisible(HandleBottomRight, visible);
        SetHandleVisible(HandleTop, visible);
        SetHandleVisible(HandleBottom, visible);
        SetHandleVisible(HandleLeft, visible);
        SetHandleVisible(HandleRight, visible);
        SetHandleVisible(MoveArea, visible);
        SetHandleVisible(SelectionBorder, visible);

        if (!hasSelection) return;

        // SelectionRect 是图像坐标，需要转换为屏幕坐标
        var scaledX = selectionRect.X * ZoomLevel;
        var scaledY = selectionRect.Y * ZoomLevel;
        var scaledWidth = selectionRect.Width * ZoomLevel;
        var scaledHeight = selectionRect.Height * ZoomLevel;

        // 更新移动区域
        Canvas.SetLeft(MoveArea, scaledX);
        Canvas.SetTop(MoveArea, scaledY);
        MoveArea.Width = Math.Abs(scaledWidth);
        MoveArea.Height = Math.Abs(scaledHeight);

        // 更新选择边框
        Canvas.SetLeft(SelectionBorder, scaledX);
        Canvas.SetTop(SelectionBorder, scaledY);
        SelectionBorder.Width = Math.Abs(scaledWidth);
        SelectionBorder.Height = Math.Abs(scaledHeight);

        // 更新角手柄
        Canvas.SetLeft(HandleTopLeft, scaledX - HandleSize / 2);
        Canvas.SetTop(HandleTopLeft, scaledY - HandleSize / 2);

        Canvas.SetLeft(HandleTopRight, scaledX + scaledWidth - HandleSize / 2);
        Canvas.SetTop(HandleTopRight, scaledY - HandleSize / 2);

        Canvas.SetLeft(HandleBottomLeft, scaledX - HandleSize / 2);
        Canvas.SetTop(HandleBottomLeft, scaledY + scaledHeight - HandleSize / 2);

        Canvas.SetLeft(HandleBottomRight, scaledX + scaledWidth - HandleSize / 2);
        Canvas.SetTop(HandleBottomRight, scaledY + scaledHeight - HandleSize / 2);

        // 更新边手柄
        Canvas.SetLeft(HandleTop, scaledX + scaledWidth / 2 - HandleSize / 2);
        Canvas.SetTop(HandleTop, scaledY - HandleSize / 2);

        Canvas.SetLeft(HandleBottom, scaledX + scaledWidth / 2 - HandleSize / 2);
        Canvas.SetTop(HandleBottom, scaledY + scaledHeight - HandleSize / 2);

        Canvas.SetLeft(HandleLeft, scaledX - HandleSize / 2);
        Canvas.SetTop(HandleLeft, scaledY + scaledHeight / 2 - HandleSize / 2);

        Canvas.SetLeft(HandleRight, scaledX + scaledWidth - HandleSize / 2);
        Canvas.SetTop(HandleRight, scaledY + scaledHeight / 2 - HandleSize / 2);
    }

    private void SetHandleVisible(Border? handle, bool visible)
    {
        if (handle != null)
            handle.IsVisible = visible;
    }
}
