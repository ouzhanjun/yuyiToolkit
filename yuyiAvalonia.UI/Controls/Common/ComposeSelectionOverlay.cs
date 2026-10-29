using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using System.Collections.ObjectModel;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 组合选择覆盖层：合并单选和多选功能
/// - 单选时：使用跟随元素旋转的手柄和旋转操作
/// - 多选时：使用轴对齐的手柄和整体旋转操作
/// - 移动功能：单选和多选都支持
/// </summary>
public class ComposeSelectionOverlay : Canvas
{
    private const string AuxLineColor = "#1E90FF";

    private const double SelectionBorderWidth = 1;
    private const double SelectionPadding = 8;
    private const double MinMarqueeSize = 5;
    private const double HandleSize = 8;
    private const double MinElementSize = 20;

    // 状态字段
    private bool _isMarqueeSelecting;
    private bool _isSelectionActive;
    private bool _isMultiMoving;
    private bool _isMultiMovingControlsReady;
    private bool _isResizing;
    private bool _isCtrlPressed;


    private bool _isRotating;
    private Point _marqueeStartPoint;
    private Point _multiMoveStartPoint;
    private Point _resizeStartPoint;
    private Point _rotateCenterPoint;
    private double _rotateStartAngle;
    private double _selectionStartRotation;
    private Dictionary<CanvasElement, double> _elementStartRotations = new();
    private Dictionary<CanvasElement, (double X, double Y, double Width, double Height)> _elementStartStates = new();
    private ResizeHandle _activeResizeHandle = ResizeHandle.None;
    private Rect _originalSelectionBounds;

    // 拖放元素的父容器左上角的位置（用于计算相对移动）
    private Point _parentStartPosition;

    public bool IsHandling
    {
        get
        {
            return _isResizing || _isMultiMoving || IsRotating;
        }
    }

    // 数据集合
    private readonly Dictionary<CanvasElement, (double X, double Y)> _multiMoveStartPositions = new();
    private readonly Dictionary<CanvasElement, (double X, double Y, double Width, double Height)> _resizeStartStates = new();

    private ObservableCollection<CanvasElement>? _canvasElements;
    public ObservableCollection<CanvasElement>? CanvasElements { get => _canvasElements; set => _canvasElements = value; }

    private Dictionary<CanvasElement, Control>? _elementControlMap;
    public Dictionary<CanvasElement, Control>? ElementControlMap { get => _elementControlMap; set => _elementControlMap = value; }

    private readonly ObservableCollection<CanvasElement> _selectedElements = new();
    public ObservableCollection<CanvasElement> SelectedElements => _selectedElements;

    private RenderControl? _renderControl;

    public bool HasSelection => _selectedElements.Count > 0;
    public bool IsMarqueeSelecting => _isMarqueeSelecting;
    public bool IsSelectionActive => _isSelectionActive;
    public bool IsMultiMoving { get => _isMultiMoving; set => _isMultiMoving = value; }
    public bool IsRotating { get => _isRotating; set => _isRotating = value; }
    public RenderControl? RenderControl { get => _renderControl; set => _renderControl = value; }

    // 控件
    private readonly Rectangle _selectionRect;
    private readonly Ellipse[] _resizeHandles;
    private readonly Border _rotateButton;
    private readonly Avalonia.Controls.Shapes.Path _rotateButtonIcon;
    private readonly Ellipse _rotateCenterIndicator;
    private readonly Ellipse _rotateGuideCircle;

    private double _rotateRadius;

    // 参考线
    private const double SnapThreshold = 5;
    private const double SnapAngleStep = 5;
    private readonly Line[] _referenceLines;
    private readonly Line[] _angleReferenceLines;

    // 事件
    public event EventHandler<IReadOnlyList<CanvasElement>>? SelectionChanged;
    public event EventHandler<ElementResizedEventArgs>? ElementResized;
    public event EventHandler<ElementAlignedEventArgs>? ElementAligned;
    public event EventHandler<ElementRotatedEventArgs>? ElementRotated;
    public event EventHandler<ElementRotatedEventArgs>? ElementRotating;
    public event EventHandler? DeleteRequested;

    GridDimensionMarker? _gridDimensionMarker = null;

    public ComposeSelectionOverlay()
    {
        InitializeSingleSelection();

        _selectionRect = new Rectangle
        {
            Fill = new SolidColorBrush(Color.Parse("#301E90FF")),
            Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
            StrokeThickness = SelectionBorderWidth,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 },
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
            _resizeHandles[i] = handle;
        }

        // 旋转按钮图标
        _rotateButtonIcon = new Avalonia.Controls.Shapes.Path
        {
            Data = Icon.RotationLT,
            Fill = new SolidColorBrush(Color.Parse(AuxLineColor)),
            Width = 16,
            Height = 16,
            Stretch = Stretch.Uniform,
            IsHitTestVisible = false
        };

        // 旋转按钮（圆形）
        _rotateButton = new Border
        {
            Width = 20,
            Height = 20,
            CornerRadius = new CornerRadius(10),
            Background = Brushes.Transparent,
            Child = _rotateButtonIcon,
            Cursor = new Cursor(StandardCursorType.SizeAll),
            IsVisible = false,
            IsHitTestVisible = true,
            [ToolTip.TipProperty] = "拖动旋转"
        };
        MultiSelectionHandleBind();

        // 旋转中心点指示器
        _rotateCenterIndicator = new Ellipse
        {
            Width = 6,
            Height = 6,
            Fill = new SolidColorBrush(Color.Parse(AuxLineColor)),
            IsHitTestVisible = false,
            IsVisible = false
        };

        // 旋转引导圆
        _rotateGuideCircle = new Ellipse
        {
            Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
            StrokeThickness = 1,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 },
            Fill = Avalonia.Media.Brushes.Transparent,
            IsHitTestVisible = false,
            IsVisible = false
        };

        // 参考线（移动吸附用，最多8条：4条水平+4条垂直）
        _referenceLines = new Line[8];
        for (int i = 0; i < 8; i++)
        {
            var line = new Line
            {
                Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
                StrokeThickness = 1,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 3 },
                IsHitTestVisible = false,
                IsVisible = false
            };
            _referenceLines[i] = line;
        }

        // 角度参考线（旋转吸附用，2条）
        _angleReferenceLines = new Line[2];
        for (int i = 0; i < 2; i++)
        {
            var line = new Line
            {
                Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
                StrokeThickness = 1,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 3 },
                IsHitTestVisible = false,
                IsVisible = false
            };
            _angleReferenceLines[i] = line;
        }

        //Grid尺寸标注浮层
        _gridDimensionMarker = new();
        _gridDimensionMarker.Name = "GridDimensionMarker";
        _gridDimensionMarker.IsVisible = false;
        _gridDimensionMarker.IsFloating = true;
        _gridDimensionMarker.FloatingOffset = 8;
        _gridDimensionMarker.ClipToBounds = false;
        _gridDimensionMarker.Opacity = 0.9;

        Children.Add(_selectionRect);
        foreach (var handle in _resizeHandles)
        {
            Children.Add(handle);
        }
        Children.Add(_rotateCenterIndicator);
        Children.Add(_rotateGuideCircle);
        Children.Add(_rotateButton);
        foreach (var line in _referenceLines)
        {
            Children.Add(line);
        }
        foreach (var line in _angleReferenceLines)
        {
            Children.Add(line);
        }


        this.Children.Add(_gridDimensionMarker);
    }

    private void MultiSelectionHandleBind()
    {
        for (int i = 0; i < 8; i++)
        {
            Ellipse handle = _resizeHandles[i];
            handle.PointerPressed -= OnSingleResizeHandlePressed;
            handle.PointerMoved -= OnSingleResizeHandleMoved;
            handle.PointerReleased -= OnSingleResizeHandleReleased;

            handle.PointerPressed += OnResizeHandlePressed;
            handle.PointerMoved += OnResizeHandleMoved;
            handle.PointerReleased += OnResizeHandleReleased;
        }

        _rotateButton.PointerPressed -= OnSingleRotateButtonPressed;
        _rotateButton.PointerMoved -= OnSingleRotateButtonMoved;
        _rotateButton.PointerReleased -= OnSingleRotateButtonReleased;

        _rotateButton.PointerPressed += OnRotateButtonPressed;
        _rotateButton.PointerMoved += OnRotateButtonMoved;
        _rotateButton.PointerReleased += OnRotateButtonReleased;
    }

    private void SingleSelectionHandleBind()
    {
        for (int i = 0; i < 8; i++)
        {
            Ellipse handle = _resizeHandles[i];
            handle.PointerPressed += OnSingleResizeHandlePressed;
            handle.PointerMoved += OnSingleResizeHandleMoved;
            handle.PointerReleased += OnSingleResizeHandleReleased;
            handle.PointerPressed -= OnResizeHandlePressed;
            handle.PointerMoved -= OnResizeHandleMoved;
            handle.PointerReleased -= OnResizeHandleReleased;
        }

        _rotateButton.PointerPressed -= OnRotateButtonPressed;
        _rotateButton.PointerMoved -= OnRotateButtonMoved;
        _rotateButton.PointerReleased -= OnRotateButtonReleased;

        _rotateButton.PointerPressed += OnSingleRotateButtonPressed;
        _rotateButton.PointerMoved += OnSingleRotateButtonMoved;
        _rotateButton.PointerReleased += OnSingleRotateButtonReleased;

    }

    #region 属性面板

    /// <summary>
    /// 更新Grid尺寸标注显示
    /// </summary>
    private void UpdateGridDimensionMarker(CanvasElement? element)
    {
        _gridDimensionMarker!.IsVisible = false;
        if (_gridDimensionMarker == null || element == null)
        {
            _gridDimensionMarker?.ClearFloatingPosition();
            return;
        }

        // 检查是否是Grid元素
        if (element is not ElementControls.Elements.GridElement gridElement)
        {
            _gridDimensionMarker.ClearFloatingPosition();
            return;
        }

        // 获取Grid控件的位置信息
        if (!ElementControlMap!.TryGetValue(element, out var control))
        {
            _gridDimensionMarker.ClearFloatingPosition();
            return;
        }

        // 获取Grid相对于ElementsCanvas的位置
        var position = control.TranslatePoint(new Point(0, 0), this);
        if (position == null)
        {
            _gridDimensionMarker.ClearFloatingPosition();
            return;
        }

        // 获取GridElement的行列定义
        var rowDefs = gridElement.RowDefinitions ?? string.Empty;
        var colDefs = gridElement.ColumnDefinitions ?? string.Empty;

        // 计算行列数
        var rowCount = string.IsNullOrEmpty(rowDefs) ? gridElement.Rows :
            Math.Max(1, rowDefs.Split(',', StringSplitOptions.RemoveEmptyEntries).Length);
        var colCount = string.IsNullOrEmpty(colDefs) ? gridElement.Columns :
            Math.Max(1, colDefs.Split(',', StringSplitOptions.RemoveEmptyEntries).Length);

        // 设置标注控件属性
        _gridDimensionMarker.RowDefinitions = string.IsNullOrEmpty(rowDefs) ? "*" : rowDefs;
        _gridDimensionMarker.ColumnDefinitions = string.IsNullOrEmpty(colDefs) ? "*" : colDefs;
        _gridDimensionMarker.RowCount = rowCount;
        _gridDimensionMarker.ColumnCount = colCount;
        _gridDimensionMarker.TotalWidth = element.Width;
        _gridDimensionMarker.TotalHeight = element.Height;

        _gridDimensionMarker!.IsVisible = true;
        // 设置浮动位置
        _gridDimensionMarker.SetFloatingPosition(position.Value.X, position.Value.Y, element.Width, element.Height);
    }

    #endregion

    #region 公共方法

    public void SelectionStart(PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint((Visual?)(e.Source as IInputElement)).Properties.IsLeftButtonPressed)
            return;
        OnPointerPressed(e);
    }

    public void SelectionMoved(PointerEventArgs e) => OnPointerMoved(e);

    public void SelectionEnd(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton != MouseButton.Left) return;
        OnPointerReleased(e);
    }

    public bool IsPointInSelectionRect(Point point)
    {
        if (!_isSelectionActive || _selectedElements.Count == 0) 
            return false;

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var element in _selectedElements)
        {
            var bounds = GetGlobalBounds(element);
            minX = Math.Min(minX, bounds.MinX);
            minY = Math.Min(minY, bounds.MinY);
            maxX = Math.Max(maxX, bounds.MaxX);
            maxY = Math.Max(maxY, bounds.MaxY);
        }

        return point.X >= minX && point.X <= maxX && point.Y >= minY && point.Y <= maxY;
    }

    public void ClearAllSelection()
    {
        SetSelectedSingleElement(null);

        _selectedElements.Clear();
        ClearSelectionOverlay();
        SelectionChanged?.Invoke(this, Array.Empty<CanvasElement>());
    }

    public void AddSingleSelectedElement(CanvasElement? element)
    {
        if (element == null || !element!.IsVisible)
        {
            return;
        }

        ClearAllSelection();

        if (element != null)
        {
            _selectedElements.Add(element);
            _isSelectionActive = true;
            SetSelectedSingleElement(element);
            UpdateSelectionOverlay(_selectedElements.ToList());
        }

        SelectionChanged?.Invoke(this, _selectedElements);
    }

    public void AddSelectedElement(CanvasElement? element)
    {
        if (element == null || !element!.IsVisible)
        {
            return;
        }

        if (!_selectedElements.Contains(element))
        {
            _selectedElements.Add(element);

            if (_selectedElements.Count == 1)
            {
                SetSelectedSingleElement(_selectedElements[0]);
            }
            else
            {
                SetSelectedSingleElement(null);
            }
        }

        _isSelectionActive = _selectedElements.Count > 0;
        SelectionChanged?.Invoke(this, _selectedElements);
    }

    public void AddSelectedElements(CanvasElement[] elements)
    {
        if (elements == null)
            return;

        ClearAllSelection();

        foreach (CanvasElement element in elements)
        {
            if (element != null && element.IsVisible && !_selectedElements.Contains(element))
            {
                _selectedElements.Add(element);
            }
        }

        if (_selectedElements.Count() == 1)
        {
            SetSelectedSingleElement(_selectedElements[0]);
        }
        else
        {
            SetSelectedSingleElement(null);
            this.RefreshSelectionOverlay();
        }

        _isSelectionActive = _selectedElements.Count > 0;
        SelectionChanged?.Invoke(this, _selectedElements);
    }

    public void SwitchSelectedElement(CanvasElement? element)
    {
        if (element == null )
            return;

        if (_selectedElements.Contains(element))
        {
            _selectedElements.Remove(element);
        }
        else
        {
            _selectedElements.Add(element);
        }
        _isSelectionActive = _selectedElements.Count > 0;

        if (_selectedElements.Count == 1)
        {
            SetSelectedSingleElement(_selectedElements[0]);
        }
        else
        {
            SetSelectedSingleElement(null);
        }

        UpdateSelectionOverlay(_selectedElements.ToList());
        SelectionChanged?.Invoke(this, _selectedElements);
    }

    public void RemoveSelectedElement(CanvasElement? element)
    {
        if (element == null)
            return;

        if (_selectedElements.Contains(element))
        {
            _selectedElements.Remove(element);
        }

        _isSelectionActive = _selectedElements.Count > 0;

        if (_selectedElements.Count == 1)
        {
            SetSelectedSingleElement(_selectedElements[0]);
        }
        else
        {
            SetSelectedSingleElement(null);
        }

        UpdateSelectionOverlay(_selectedElements.ToList());
        SelectionChanged?.Invoke(this, _selectedElements);
    }

    public void RefreshSelectionOverlay()
    {
        ((Panel)this.Parent!)!.UpdateLayout();
        if (!_isSelectionActive)
            return;

        if (_selectedElements.Count > 0)
            UpdateSelectionOverlay(_selectedElements.ToList());
        else
            ClearSelectionOverlay();
    }

    public void EndMultiMove()
    {
        _isMultiMoving = false;
        _isMultiMovingControlsReady = false;
        _multiMoveStartPositions.Clear();
        HideReferenceLines();
    }

    public void EndMultiResize()
    {
        if (_isResizing)
        {
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            _resizeStartStates.Clear();
            ElementResized?.Invoke(this, new ElementResizedEventArgs(new List<CanvasElement>(_selectedElements)));
        }
    }

    public void AlignElements(AlignmentType alignmentType, Canvas parentPanel)
    {
        if (_selectedElements == null || _selectedElements.Count <= 1) return;

        var elements = _selectedElements.ToList();
        switch (alignmentType)
        {
            case AlignmentType.Left: AlignLeft(elements, parentPanel); break;
            case AlignmentType.HorizontalCenter: AlignHorizontalCenter(elements, parentPanel); break;
            case AlignmentType.Right: AlignRight(elements, parentPanel); break;
            case AlignmentType.Top: AlignTop(elements, parentPanel); break;
            case AlignmentType.VerticalCenter: AlignVerticalCenter(elements, parentPanel); break;
            case AlignmentType.Bottom: AlignBottom(elements, parentPanel); break;
            case AlignmentType.HorizontalSpacing: DistributeHorizontalSpacing(elements, parentPanel); break;
            case AlignmentType.VerticalSpacing: DistributeVerticalSpacing(elements, parentPanel); break;
            case AlignmentType.SameWidth: MakeSameWidth(elements); break;
            case AlignmentType.SameHeight: MakeSameHeight(elements); break;
            case AlignmentType.SameSize: MakeSameSize(elements); break;
        }

        this.RefreshSelectionOverlay();

        ElementAligned?.Invoke(this, new ElementAlignedEventArgs(elements, alignmentType));
    }

    #endregion

    #region 多选移动

    public void ControlsMoveStart(Point startPoint, Canvas canvas)
    {
        _isMultiMoving = true;
        _multiMoveStartPoint = startPoint;
        _multiMoveStartPositions.Clear();
        _isMultiMovingControlsReady = false;

    }

    /// <summary>
    /// 从控件的父链向上遍历，累加所有 CanvasElement 祖先的旋转角
    /// </summary>
    private double GetParentRotationFromControl(Control control, Control stopAt)
    {
        double total = 0;
        var parent = control.Parent as Control;
        while (parent != null && parent != stopAt)
        {
            if (parent is IElementControlBase parentEcb && parent.Tag is CanvasElement parentElem)
                total += parentElem.Rotation;
            parent = parent.Parent as Control;
        }
        return total;
    }

    /// <summary>
    /// 反推元素在层层上级容器（带旋转角）情况下，在顶级容器中的视觉位置（未旋转时）和旋转角。
    /// 使用 TranslatePoint 获取四个角点在顶级容器中的视觉位置，计算中心点，再用中心点和宽高计算左上角。
    /// </summary>
    /// <param name="control">控件（对应 element）</param>
    /// <param name="element">CanvasElement</param>
    /// <param name="canvas">目标顶级 Canvas</param>
    /// <returns>元素在顶级容器中的轴对齐视觉信息，如果无法计算则返回 null</returns>
    private AxisAlignedElementInfo? GetAxisAlignedTopLeft(Control control, CanvasElement element, Canvas canvas)
    {
        if (control == null || element == null)
            return null;

        // 元素四个角点在本地坐标系中的位置
        var localCorners = new[]
        {
            new Point(0, 0),                         // 左上
            new Point(element.Width, 0),              // 右上
            new Point(element.Width, element.Height),  // 右下
            new Point(0, element.Height)              // 左下
        };

        // 使用 TranslatePoint 获取四个角点在 canvas 坐标系中的视觉位置
        Point?[] visualCorners = new Point?[4];
        for (int i = 0; i < 4; i++)
        {
            visualCorners[i] = control.TranslatePoint(localCorners[i], canvas);
            if (visualCorners[i] == null)
                return null;
        }

        // 计算四个视觉角点的中心点
        double centerX = (visualCorners[0]!.Value.X + visualCorners[1]!.Value.X +
                          visualCorners[2]!.Value.X + visualCorners[3]!.Value.X) / 4;
        double centerY = (visualCorners[0]!.Value.Y + visualCorners[1]!.Value.Y +
                          visualCorners[2]!.Value.Y + visualCorners[3]!.Value.Y) / 4;
        var visualCenter = new Point(centerX, centerY);

        // 遍历父链，收集所有旋转角度（直到 canvas 为止）
        double totalRotation = 0;
        var parent = control.Parent as Control;
        while (parent != null && parent != canvas)
        {
            if (parent is BorderControlBase parentEcb && parentEcb.Tag is CanvasElement parentElem)
            {
                totalRotation += parentElem.Rotation;
            }
            parent = parent.Parent as Control;
        }

        // 用中心点和元素宽高计算轴对齐外包矩形的左上角
        double topLeftX = centerX - element.Width / 2;
        double topLeftY = centerY - element.Height / 2;

        return new AxisAlignedElementInfo(
            topLeft: new Point(topLeftX, topLeftY),
            center: visualCenter,
            width: element.Width,
            height: element.Height,
            totalRotation: totalRotation
        );
    }

    private bool ReadyControls(Canvas canvas)
    {
        if (SelectedElements == null || _isMultiMovingControlsReady)
            return _isMultiMovingControlsReady;

        _isMultiMovingControlsReady = true;

        foreach (var element in SelectedElements)
        {
            _multiMoveStartPositions[element] = (element.X, element.Y);

            if (ElementControlMap?.TryGetValue(element, out var control) == true)
            {
                if (element.HorizontalAlignment == Avalonia.Layout.HorizontalAlignment.Stretch)
                {
                    element.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;

                    _renderControl!.UpdateElementControl(element, control, "HorizontalAlignment");
                }

                if (element.VerticalAlignment == Avalonia.Layout.VerticalAlignment.Stretch)
                {
                    element.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                    _renderControl!.UpdateElementControl(element, control, "VerticalAlignment");
                }

                if (control.Parent is Panel parent && parent != canvas)
                {
                    // 用 GetAxisAlignedTopLeft 获取元素在顶级容器中的轴对齐视觉信息和累积旋转角
                    //var axisAligned = GetAxisAlignedTopLeft(control, element, canvas);
                    var axisAligned = CanvasHelper.GetAxisAlignedTopLeft(control, canvas);

                    if (axisAligned.HasValue)
                    {
                        // 用轴对齐外包矩形左上角作为新的 X/Y 坐标
                        element.X = axisAligned.Value.TopLeft.X;
                        element.Y = axisAligned.Value.TopLeft.Y;

                        // 累加父容器旋转角，保持视觉转角不变
                        element.Rotation += axisAligned.Value.TotalRotation;
                    }

                    // 解除原父子关系
                    parent.Children.Remove(control);
                    var parentElement = element.Parent;
                    element.Parent = null;
                    parentElement?.Children.Remove(element);

                    canvas.Children.Add(control);

                    _renderControl!.UpdateElementControl(element, control, nameof(element.Rotation));

                    if (!_canvasElements!.Contains(element))
                        _canvasElements.Add(element);

                    _multiMoveStartPositions[element] = (element.X, element.Y);
                }
            }
        }

        return true;
    }

    public void ControlsMoveTo(Point currentPoint, Canvas canvas, bool isCtrlPressed)
    {
        if (!_isMultiMoving)
        {
            return;
        }

        ReadyControls(canvas);

        var offsetX = currentPoint.X - _multiMoveStartPoint.X;
        var offsetY = currentPoint.Y - _multiMoveStartPoint.Y;

        double snapOffsetX = 0;
        double snapOffsetY = 0;
        ClearReferenceLines();

        if (isCtrlPressed && _canvasElements != null)
        {
            var (xSnaps, ySnaps, selMinX, selMaxX, selMinY, selMaxY) = CalculateSnap(offsetX, offsetY, canvas);
            double selCenterX = (selMinX + selMaxX) / 2;
            double selCenterY = (selMinY + selMaxY) / 2;

            foreach (var x in xSnaps)
            {
                DrawReferenceLinesForX(x.SnapValue, x.SnapEdgeIndex, canvas);
            }
            foreach (var y in ySnaps)
            {
                DrawReferenceLinesForY(y.SnapValue, y.SnapEdgeIndex, canvas);
            }

            // 取距离最小的吸附作为偏移量
            if (xSnaps.Count > 0)
            {
                var bestX = xSnaps.OrderBy(x => x.SnapValue).First();
                snapOffsetX = bestX.SnapValue - (bestX.SnapEdgeIndex == 0 ? selMinX : bestX.SnapEdgeIndex == 1 ? selMaxX : selCenterX);
            }
            if (ySnaps.Count > 0)
            {
                var bestY = ySnaps.OrderBy(y => y.SnapValue).First();
                snapOffsetY = bestY.SnapValue - (bestY.SnapEdgeIndex == 0 ? selMinY : bestY.SnapEdgeIndex == 1 ? selMaxY : selCenterY);
            }
        }

        foreach (var element in _selectedElements)
        {
            if (!_multiMoveStartPositions.TryGetValue(element, out var startPos)) continue;

            var newX = Math.Max(-element.Width, Math.Min(startPos.X + offsetX + snapOffsetX, canvas.Width + element.Width));
            var newY = Math.Max(-element.Height, Math.Min(startPos.Y + offsetY + snapOffsetY, canvas.Height + element.Height));

            element.X = newX;
            element.Y = newY;

            if (ElementControlMap?.TryGetValue(element, out var control) == true)
            {
                _renderControl!.UpdateX(element, control);
                _renderControl!.UpdateY(element, control);
            }
        }
    }

    // 每个吸附边缘：(snapX, snapXEdge) 或 (snapY, snapYEdge)
    private record SnapEdge(double SnapValue, int SnapEdgeIndex);
    // 返回：所有吸附的边缘 + 选中区边界
    private (List<SnapEdge> X, List<SnapEdge> Y, double SelMinX, double SelMaxX, double SelMinY, double SelMaxY)
        CalculateSnap(double offsetX, double offsetY, Canvas canvas)
    {
        double selMinX = double.MaxValue, selMaxX = double.MinValue;
        double selMinY = double.MaxValue, selMaxY = double.MinValue;

        foreach (var element in _selectedElements)
        {
            if (!_multiMoveStartPositions.TryGetValue(element, out var startPos)) continue;

            var tempElement = new CanvasElement
            {
                X = startPos.X + offsetX,
                Y = startPos.Y + offsetY,
                Width = element.Width,
                Height = element.Height,
                Rotation = element.Rotation
            };
            var bounds = GetGlobalBounds(tempElement);
            selMinX = Math.Min(selMinX, bounds.MinX);
            selMaxX = Math.Max(selMaxX, bounds.MaxX);
            selMinY = Math.Min(selMinY, bounds.MinY);
            selMaxY = Math.Max(selMaxY, bounds.MaxY);
        }

        double selCenterX = (selMinX + selMaxX) / 2;
        double selCenterY = (selMinY + selMaxY) / 2;

        var xSnaps = new List<SnapEdge>();
        var ySnaps = new List<SnapEdge>();
        double bestXDist = double.MaxValue, bestYDist = double.MaxValue;

        foreach (var element in _canvasElements!)
        {
            if (_selectedElements.Contains(element)) continue;

            var targetBounds = GetGlobalBounds(element);
            double bMinX = targetBounds.MinX, bMaxX = targetBounds.MaxX;
            double bMinY = targetBounds.MinY, bMaxY = targetBounds.MaxY;
            double bCenterX = (bMinX + bMaxX) / 2;
            double bCenterY = (bMinY + bMaxY) / 2;

            double distL = Math.Abs(selMinX - bMinX);
            double distR = Math.Abs(selMaxX - bMaxX);
            double distCX = Math.Abs(selCenterX - bCenterX);
            double distT = Math.Abs(selMinY - bMinY);
            double distB = Math.Abs(selMaxY - bMaxY);
            double distCY = Math.Abs(selCenterY - bCenterY);

            if (distL <= SnapThreshold)
            {
                if (distL < bestXDist) { bestXDist = distL; xSnaps.Clear(); xSnaps.Add(new SnapEdge(bMinX, 0)); }
                else if (distL == bestXDist) xSnaps.Add(new SnapEdge(bMinX, 0));
            }
            if (distR <= SnapThreshold)
            {
                if (distR < bestXDist) { bestXDist = distR; xSnaps.Clear(); xSnaps.Add(new SnapEdge(bMaxX, 1)); }
                else if (distR == bestXDist) xSnaps.Add(new SnapEdge(bMaxX, 1));
            }
            if (distCX <= SnapThreshold)
            {
                if (distCX < bestXDist) { bestXDist = distCX; xSnaps.Clear(); xSnaps.Add(new SnapEdge(bCenterX, 2)); }
                else if (distCX == bestXDist) xSnaps.Add(new SnapEdge(bCenterX, 2));
            }
            if (distT <= SnapThreshold)
            {
                if (distT < bestYDist) { bestYDist = distT; ySnaps.Clear(); ySnaps.Add(new SnapEdge(bMinY, 0)); }
                else if (distT == bestYDist) ySnaps.Add(new SnapEdge(bMinY, 0));
            }
            if (distB <= SnapThreshold)
            {
                if (distB < bestYDist) { bestYDist = distB; ySnaps.Clear(); ySnaps.Add(new SnapEdge(bMaxY, 1)); }
                else if (distB == bestYDist) ySnaps.Add(new SnapEdge(bMaxY, 1));
            }
            if (distCY <= SnapThreshold)
            {
                if (distCY < bestYDist) { bestYDist = distCY; ySnaps.Clear(); ySnaps.Add(new SnapEdge(bCenterY, 2)); }
                else if (distCY == bestYDist) ySnaps.Add(new SnapEdge(bCenterY, 2));
            }
        }

        return (xSnaps, ySnaps, selMinX, selMaxX, selMinY, selMaxY);
    }

    private void DrawReferenceLinesForX(double snapX, int edge, Canvas canvas)
    {
        // 0=left 画在索引0，1=right 画在索引1，2=center 画在索引2
        int index = edge;
        ShowReferenceLine(index, snapX, 0, snapX, canvas.Height);
    }

    private void DrawReferenceLinesForY(double snapY, int edge, Canvas canvas)
    {
        // 0=top 画在索引4，1=bottom 画在索引5，2=center 画在索引6
        int index = 4 + edge;
        ShowReferenceLine(index, 0, snapY, canvas.Width, snapY);
    }

    private void DrawAngleReferenceLines(double snappedAngle)
    {
        // 从中心向外画两条相互垂直的线，代表吸附到的角度方向
        double rad = snappedAngle * Math.PI / 180;
        double length = 2000;
        double perpRad = (snappedAngle + 90) * Math.PI / 180;

        // 主方向线
        ShowAngleReferenceLine(0,
            _rotateCenterPoint.X - length * Math.Cos(rad),
            _rotateCenterPoint.Y - length * Math.Sin(rad),
            _rotateCenterPoint.X + length * Math.Cos(rad),
            _rotateCenterPoint.Y + length * Math.Sin(rad));

        // 垂直方向线
        ShowAngleReferenceLine(1,
            _rotateCenterPoint.X - length * Math.Cos(perpRad),
            _rotateCenterPoint.Y - length * Math.Sin(perpRad),
            _rotateCenterPoint.X + length * Math.Cos(perpRad),
            _rotateCenterPoint.Y + length * Math.Sin(perpRad));
    }

    #endregion

    #region 参考线

    private void HideReferenceLines()
    {
        for (int i = 0; i < _referenceLines.Length; i++)
            _referenceLines[i].IsVisible = false;
        for (int i = 0; i < _angleReferenceLines.Length; i++)
            _angleReferenceLines[i].IsVisible = false;
    }

    private void ClearReferenceLines()
    {
        for (int i = 0; i < _referenceLines.Length; i++)
        {
            _referenceLines[i].IsVisible = false;
            _referenceLines[i].StartPoint = new Point(0, 0);
            _referenceLines[i].EndPoint = new Point(0, 0);
        }
        for (int i = 0; i < _angleReferenceLines.Length; i++)
        {
            _angleReferenceLines[i].IsVisible = false;
            _angleReferenceLines[i].StartPoint = new Point(0, 0);
            _angleReferenceLines[i].EndPoint = new Point(0, 0);
        }
    }

    private void ShowReferenceLine(int index, double x1, double y1, double x2, double y2)
    {
        if (index < 0 || index >= _referenceLines.Length) return;
        _referenceLines[index].StartPoint = new Point(x1, y1);
        _referenceLines[index].EndPoint = new Point(x2, y2);
        _referenceLines[index].IsVisible = true;
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
        for (int i = 0; i < _angleReferenceLines.Length; i++)
            _angleReferenceLines[i].IsVisible = false;
    }

    #endregion

    #region 框选

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (IsHandling)
        {
            return;
        }

        ClearSelectionOverlay();
        _isMarqueeSelecting = true;
        _marqueeStartPoint = e.GetPosition(this);

        Canvas.SetLeft(_selectionRect, _marqueeStartPoint.X);
        Canvas.SetTop(_selectionRect, _marqueeStartPoint.Y);
        _selectionRect.Width = 0;
        _selectionRect.Height = 0;
        _selectionRect.IsVisible = true;
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_isMarqueeSelecting)
        {
            var currentPoint = e.GetPosition(this);
            var x = Math.Min(_marqueeStartPoint.X, currentPoint.X);
            var y = Math.Min(_marqueeStartPoint.Y, currentPoint.Y);

            Canvas.SetLeft(_selectionRect, x);
            Canvas.SetTop(_selectionRect, y);
            _selectionRect.Width = Math.Abs(currentPoint.X - _marqueeStartPoint.X);
            _selectionRect.Height = Math.Abs(currentPoint.Y - _marqueeStartPoint.Y);
        }
    }

    protected void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isMarqueeSelecting)
        {
            _isMarqueeSelecting = false;
            var currentPoint = e.GetPosition(this);
            var x = Math.Min(_marqueeStartPoint.X, currentPoint.X);
            var y = Math.Min(_marqueeStartPoint.Y, currentPoint.Y);
            var width = Math.Abs(currentPoint.X - _marqueeStartPoint.X);
            var height = Math.Abs(currentPoint.Y - _marqueeStartPoint.Y);

            if (width < MinMarqueeSize || height < MinMarqueeSize)
            {
                ClearAllSelection();
                return;
            }

            SelectElementsInRect(x, y, width, height);
        }
    }

    private void SelectElementsInRect(double x, double y, double width, double height)
    {
        ClearAllSelection();
        if (_canvasElements == null) return;

        var selectionRect = new Rect(x, y, width, height);

        // 先收集所有在框内的元素
        var toBeSelected = new List<CanvasElement>();

        foreach (var element in _canvasElements)
        {
            if (element.Parent != null) continue; // 跳过子元素

            var bounds = GetGlobalBounds(element);

            // 检查顶级元素本身是否在选择框内
            if (bounds.MinX >= selectionRect.X &&
                bounds.MinY >= selectionRect.Y &&
                bounds.MaxX <= selectionRect.Right &&
                bounds.MaxY <= selectionRect.Bottom)
            {
                // 顶级元素在框内，添加它
                toBeSelected.Add(element);
            }
            else
            {
                // 顶级元素不在框内，检查子元素
                CollectChildElementsInRect(element, selectionRect, toBeSelected);
            }
        }

        // 过滤：如果某个元素的父元素也在 toBeSelected 中，则只保留父元素
        var filtered = new List<CanvasElement>();
        foreach (var element in toBeSelected)
        {
            var parent = element.Parent as CanvasElement;
            // 如果父元素不在 toBeSelected 中，才添加此元素
            if (parent == null || !toBeSelected.Contains(parent))
            {
                filtered.Add(element);
            }
        }

        // 应用选中状态
        foreach (var element in filtered)
        {
            // 如果添加的是父元素，先移除所有子元素的选中状态
            foreach (var child in GetAllChildren(element))
            {
                _selectedElements.Remove(child);
            }

            _selectedElements.Add(element);
        }

        if (_selectedElements.Count > 0)
        {
            if (_selectedElements.Count == 1)
            {
                SetSelectedSingleElement(_selectedElements[0]);
            }
            UpdateSelectionOverlay(_selectedElements.ToList());
            _isSelectionActive = _selectedElements.Count > 0;
        }
        else
        {
            SetSelectedSingleElement(null);
            ClearSelectionOverlay();
        }

        SelectionChanged?.Invoke(this, _selectedElements);
    }

    private void CollectChildElementsInRect(CanvasElement parent, Rect selectionRect, List<CanvasElement> result)
    {
        foreach (var child in parent.Children)
        {
            if (child is not CanvasElement childElement) continue;

            var bounds = GetGlobalBounds(childElement);

            // 检查子元素的所有角点是否都在选择框内
            if (bounds.MinX >= selectionRect.X &&
                bounds.MinY >= selectionRect.Y &&
                bounds.MaxX <= selectionRect.Right &&
                bounds.MaxY <= selectionRect.Bottom)
            {
                result.Add(childElement);
            }

            // 递归检查子元素的子元素
            if (childElement.Children.Count > 0)
            {
                CollectChildElementsInRect(childElement, selectionRect, result);
            }
        }
    }

    private IEnumerable<CanvasElement> GetAllChildren(CanvasElement element)
    {
        foreach (var child in element.Children)
        {
            if (child is CanvasElement childElement)
            {
                yield return childElement;
                foreach (var grandChild in GetAllChildren(childElement))
                {
                    yield return grandChild;
                }
            }
        }
    }

    #endregion

    #region 选择框更新

    private void UpdateSelectionOverlay(List<CanvasElement> elements, Canvas? parentCanvas = null)
    {
        if (elements.Count == 0)
        {
            ClearSelectionOverlay();
            return;
        }

        if (_selectedElement != null)
        {
            UpdateSingleOverlay();
            return;
        }

        _parentCanvasForBounds = parentCanvas ?? _parentCanvasForBounds;

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var element in elements)
        {
            var bounds = GetGlobalBounds(element);
            minX = Math.Min(minX, bounds.MinX);
            minY = Math.Min(minY, bounds.MinY);
            maxX = Math.Max(maxX, bounds.MaxX);
            maxY = Math.Max(maxY, bounds.MaxY);
        }

        minX -= SelectionPadding;
        minY -= SelectionPadding;
        maxX += SelectionPadding;
        maxY += SelectionPadding;

        Canvas.SetLeft(_selectionRect, minX);
        Canvas.SetTop(_selectionRect, minY);
        _selectionRect.Width = maxX - minX;
        _selectionRect.Height = maxY - minY;
        _selectionRect.IsVisible = true;

        UpdateResizeHandles(minX, minY, _selectionRect.Width, _selectionRect.Height);

        // 更新旋转按钮位置（选择框右上角外侧）
        var rotateButtonX = maxX + 4;
        var rotateButtonY = minY - 10;

        Canvas.SetLeft(_rotateButton, rotateButtonX);
        Canvas.SetTop(_rotateButton, rotateButtonY);
        _rotateButton.IsVisible = true;

        // 更新Grid尺寸标注
        if (this.SelectedElements.Count == 1)
        {
            UpdateGridDimensionMarker(this.SelectedElements[0]);
        }
        else
        {
            UpdateGridDimensionMarker(null);
        }
    }

    private void ClearSelectionOverlay()
    {
        _selectionRect.IsVisible = false;
        HideResizeHandles();
        _isSelectionActive = false;
        _rotateButton.IsVisible = false;
        _rotateCenterIndicator.IsVisible = false;
        _rotatedSelectionPolygon!.IsVisible = false;
        _rotateButton.RenderTransform = new RotateTransform(0);
        // 更新Grid尺寸标注
        UpdateGridDimensionMarker(null);
    }

    public void HideSelectionOverlay()
    {
        _selectionRect.IsVisible = false;
        HideResizeHandles();
        _rotateButton.IsVisible = false;
    }

    private void UpdateResizeHandles(double rectX, double rectY, double rectWidth, double rectHeight)
    {
        var halfW = HandleSize / 2;
        var positions = new[]
        {
            (rectX - halfW, rectY - halfW),                           // TopLeft
            (rectX + rectWidth / 2 - halfW, rectY - halfW),         // TopCenter
            (rectX + rectWidth - halfW, rectY - halfW),               // TopRight
            (rectX - halfW, rectY + rectHeight / 2 - halfW),         // MiddleLeft
            (rectX + rectWidth - halfW, rectY + rectHeight / 2 - halfW), // MiddleRight
            (rectX - halfW, rectY + rectHeight - halfW),             // BottomLeft
            (rectX + rectWidth / 2 - halfW, rectY + rectHeight - halfW), // BottomCenter
            (rectX + rectWidth - halfW, rectY + rectHeight - halfW)    // BottomRight
        };

        for (int i = 0; i < 8; i++)
        {
            Canvas.SetLeft(_resizeHandles[i], positions[i].Item1);
            Canvas.SetTop(_resizeHandles[i], positions[i].Item2);
            _resizeHandles[i].IsVisible = true;
        }
    }

    private void HideResizeHandles()
    {
        foreach (var handle in _resizeHandles)
            handle.IsVisible = false;
    }

    private Rect GetSelectionBounds()
    {
        if (_selectedElements.Count == 0) return default;

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var element in _selectedElements)
        {
            var bounds = GetGlobalBounds(element);
            minX = Math.Min(minX, bounds.MinX);
            minY = Math.Min(minY, bounds.MinY);
            maxX = Math.Max(maxX, bounds.MaxX);
            maxY = Math.Max(maxY, bounds.MaxY);
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    #endregion

    #region 旋转边界计算

    private const double AlignPrecision = 0.01;

    /// <summary>
    /// 获取元素在 overlay 坐标系下的外包矩形（考虑所有变换：平移、旋转、投影）。
    /// 通过 TransformToVisual 获取完整变换矩阵，对四个角点进行矩阵变换后取外包矩形。
    /// </summary>
    private (double MinX, double MinY, double MaxX, double MaxY) GetGlobalBounds(CanvasElement element, Canvas? canvas = null)
    {
        if (!ElementControlMap!.TryGetValue(element, out var control) || control == null)
        {
            // 没有 control 时，使用元素自身属性计算旋转后的边界
            return GetRotatedBounds(element.X, element.Y, element.Width, element.Height, element.Rotation);
        }

        double width = element.Width;
        double height = element.Height;

        // 使用 TransformToVisual 获取从元素坐标系到 overlay 坐标系的完整变换矩阵
        // 这会考虑元素的 RenderTransform 和所有祖先的变换（包括投影）
        Matrix? transform = null;

        if (canvas == null)
        {
            transform = control.TransformToVisual(this);
        }
        else
        {
            transform = control.TransformToVisual(canvas);
        }
        if (transform == null)
            return GetRotatedBounds(element.X, element.Y, width, height, element.Rotation);

        // 元素的四个角点在元素坐标系中的位置（应用变换之前）
        Point[] corners = new[]
        {
            new Point(0, 0),
            new Point(width, 0),
            new Point(width, height),
            new Point(0, height)
        };

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var corner in corners)
        {
            // 使用变换矩阵转换角点
            var transformed = transform.Value.Transform(corner);

            minX = Math.Min(minX, transformed.X);
            minY = Math.Min(minY, transformed.Y);
            maxX = Math.Max(maxX, transformed.X);
            maxY = Math.Max(maxY, transformed.Y);
        }

        return (minX, minY, maxX, maxY);
    }

    /// <summary>
    /// 计算旋转后的外包矩形边界
    /// </summary>
    private (double MinX, double MinY, double MaxX, double MaxY) GetRotatedBounds(double x, double y, double width, double height, double rotation)
    {
        if (rotation == 0)
        {
            return (x, y, x + width, y + height);
        }

        // 计算中心点
        double centerX = x + width / 2;
        double centerY = y + height / 2;

        // 将角度转换为弧度
        double radians = rotation * Math.PI / 180;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);

        // 四个角点（相对于中心点）
        double[] cornersX = { -width / 2, width / 2, width / 2, -width / 2 };
        double[] cornersY = { -height / 2, -height / 2, height / 2, height / 2 };

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        for (int i = 0; i < 4; i++)
        {
            // 旋转角点
            double rotatedX = cornersX[i] * cos - cornersY[i] * sin;
            double rotatedY = cornersX[i] * sin + cornersY[i] * cos;

            // 转换到全局坐标
            double globalX = centerX + rotatedX;
            double globalY = centerY + rotatedY;

            minX = Math.Min(minX, globalX);
            minY = Math.Min(minY, globalY);
            maxX = Math.Max(maxX, globalX);
            maxY = Math.Max(maxY, globalY);
        }

        return (minX, minY, maxX, maxY);
    }

    /// <summary>
    /// 四舍五入到指定精度，避免浮点数精度问题导致的对齐漂移
    /// </summary>
    private static double RoundToPrecision(double value)
    {
        return Math.Round(value / AlignPrecision) * AlignPrecision;
    }

    private Canvas? _parentCanvasForBounds;

    #endregion

    #region 所有元素调整大小

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
        if (sender is not Ellipse ellipse || _selectedElements.Count == 0)
            return;

        _isResizing = true;
        _activeResizeHandle = (ResizeHandle)ellipse.Tag!;
        _resizeStartPoint = e.GetPosition(this);
        _resizeStartStates.Clear();

        foreach (var element in _selectedElements)
        {
            _resizeStartStates[element] = (element.X, element.Y, element.Width, element.Height);
        }

        _originalSelectionBounds = GetSelectionBounds();

        //记录父容器位置（如果有）
        if (ElementControlMap?.TryGetValue(_selectedElements[0], out var control) == true)
        {
            if (this.Parent != ((Panel?)control.Parent)!.Parent && control.Parent is Canvas)
            {
                var pos = ((Panel)control.Parent).TranslatePoint(new Point(0, 0), this);

                _parentStartPosition = pos!.Value;
            }
        }

        e.Handled = true;
    }

    private void OnResizeHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing) return;
        if (sender is not Ellipse ellipse || _selectedElements.Count == 0)
        {
            _isResizing = false;
            return;
        }

        _activeResizeHandle = (ResizeHandle)ellipse.Tag!;
        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _resizeStartPoint.X;
        var deltaY = currentPoint.Y - _resizeStartPoint.Y;
        _isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        var newBounds = CalculateNewBounds(deltaX, deltaY);

        if (_isCtrlPressed)
            newBounds = ApplyAspectRatioConstraint(newBounds, _originalSelectionBounds);

        newBounds = new Rect(newBounds.X, newBounds.Y,
            Math.Max(MinElementSize, newBounds.Width),
            Math.Max(MinElementSize, newBounds.Height));

        // 轴对齐元素的调整
        switch (_activeResizeHandle)
        {
            case ResizeHandle.TopLeft:
                if (newBounds.X >= (_originalSelectionBounds.X + _originalSelectionBounds.Width - MinElementSize) || newBounds.Y >= (_originalSelectionBounds.Y + _originalSelectionBounds.Height - MinElementSize))
                {
                    return;
                }
                break;

            case ResizeHandle.TopRight:
                if (newBounds.Width < MinElementSize || newBounds.Y >= (_originalSelectionBounds.Y + _originalSelectionBounds.Height - MinElementSize))
                {
                    return;
                }
                break;

            case ResizeHandle.BottomLeft:
                if (newBounds.X >= (_originalSelectionBounds.X + _originalSelectionBounds.Width - MinElementSize) || newBounds.Height <= MinElementSize)
                {
                    return;
                }
                break;

            case ResizeHandle.BottomRight:
                if (newBounds.Width <= MinElementSize || newBounds.Height <= MinElementSize)
                {
                    return;
                }
                break;

            // 边中点：只改变对应维度，对边固定
            case ResizeHandle.TopCenter:
                if (newBounds.Y > (_originalSelectionBounds.Y + _originalSelectionBounds.Height - MinElementSize) || newBounds.Height <= MinElementSize)
                {
                    return;
                }
                break;

            case ResizeHandle.BottomCenter:
                if (newBounds.Height <= MinElementSize)
                {
                    return;
                }
                break;

            case ResizeHandle.MiddleLeft:
                if (newBounds.Width <= MinElementSize || newBounds.X >= (_originalSelectionBounds.X + _originalSelectionBounds.Width - MinElementSize))
                {
                    return;
                }
                break;

            case ResizeHandle.MiddleRight:
                if (newBounds.Width <= MinElementSize)
                {
                    return;
                }
                break;
        }

        ApplyUniformResize(newBounds);
        UpdateSelectionOverlay(_selectedElements.ToList());
    }

    private void OnResizeHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isResizing)
        {
            _parentStartPosition = default;
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            _isCtrlPressed = false;
            _resizeStartStates.Clear();
            ElementResized?.Invoke(this, new ElementResizedEventArgs(new List<CanvasElement>(_selectedElements)));
        }
    }

    private Rect CalculateNewBounds(double deltaX, double deltaY)
    {
        var orig = _originalSelectionBounds;
        double newX, newY, newW, newH;

        switch (_activeResizeHandle)
        {
            // 角点：对角固定，拖动角点跟随移动
            case ResizeHandle.TopLeft:
                newX = orig.X + deltaX;
                newY = orig.Y + deltaY;
                newW = orig.Width - deltaX;
                newH = orig.Height - deltaY;
                break;
            case ResizeHandle.TopRight:
                newX = orig.X;
                newY = orig.Y + deltaY;
                newW = orig.Width + deltaX;
                newH = orig.Height - deltaY;
                break;
            case ResizeHandle.BottomLeft:
                newX = orig.X + deltaX;
                newY = orig.Y;
                newW = orig.Width - deltaX;
                newH = orig.Height + deltaY;
                break;
            case ResizeHandle.BottomRight:
                newX = orig.X;
                newY = orig.Y;
                newW = orig.Width + deltaX;
                newH = orig.Height + deltaY;
                break;
            // 边中点：对边固定，被拖动的边跟随移动
            case ResizeHandle.TopCenter:
                // 拖动上边，上边移动(Y减少)，下边固定
                newX = orig.X;
                newY = orig.Y + deltaY;
                newW = orig.Width;
                newH = orig.Height - deltaY;
                break;
            case ResizeHandle.BottomCenter:
                // 拖动下边，下边移动(Height增加)，上边固定
                newX = orig.X;
                newY = orig.Y;
                newW = orig.Width;
                newH = orig.Height + deltaY;
                break;
            case ResizeHandle.MiddleLeft:
                // 拖动左边，左边移动(X减少)，右边固定
                newX = orig.X + deltaX;
                newY = orig.Y;
                newW = orig.Width - deltaX;
                newH = orig.Height;
                break;
            case ResizeHandle.MiddleRight:
                // 拖动右边，右边移动(Width增加)，左边固定
                newX = orig.X;
                newY = orig.Y;
                newW = orig.Width + deltaX;
                newH = orig.Height;
                break;
            default:
                return orig;
        }

        // 整数化以避免浮点误差累积
        return new Rect(Math.Round(newX), Math.Round(newY), Math.Round(newW), Math.Round(newH));
    }

    private Rect ApplyAspectRatioConstraint(Rect newBounds, Rect originalBounds)
    {
        if (originalBounds.Width <= 0 || originalBounds.Height <= 0 ||
            newBounds.Width <= 0 || newBounds.Height <= 0)
            return newBounds;

        double widthRatio = newBounds.Width / originalBounds.Width;
        double heightRatio = newBounds.Height / originalBounds.Height;
        double minRatio = Math.Min(Math.Abs(widthRatio), Math.Abs(heightRatio));

        double constrainedWidth = originalBounds.Width * minRatio;
        double constrainedHeight = originalBounds.Height * minRatio;

        double newX = originalBounds.X;
        double newY = originalBounds.Y;

        switch (_activeResizeHandle)
        {
            // 角点：对角固定
            case ResizeHandle.TopLeft:
                newX = originalBounds.X + originalBounds.Width - constrainedWidth;
                newY = originalBounds.Y + originalBounds.Height - constrainedHeight;
                break;
            case ResizeHandle.TopRight:
                newY = originalBounds.Y + originalBounds.Height - constrainedHeight;
                break;
            case ResizeHandle.BottomLeft:
                newX = originalBounds.X + originalBounds.Width - constrainedWidth;
                break;
            case ResizeHandle.BottomRight:
                // 全部跟随，无需调整
                break;
            // 边中点：对边固定，被拖动的边跟随
            case ResizeHandle.TopCenter:
                // top边跟随移动，bottom边固定，X居中
                newX = originalBounds.X + (originalBounds.Width - constrainedWidth) / 2;
                break;
            case ResizeHandle.BottomCenter:
                // bottom边跟随移动，top边固定，X居中
                newX = originalBounds.X + (originalBounds.Width - constrainedWidth) / 2;
                newY = originalBounds.Y + originalBounds.Height - constrainedHeight;
                break;
            case ResizeHandle.MiddleLeft:
                // left边跟随移动，right边固定，Y居中
                newY = originalBounds.Y + (originalBounds.Height - constrainedHeight) / 2;
                break;
            case ResizeHandle.MiddleRight:
                // right边跟随移动，left边固定，Y居中
                newX = originalBounds.X + originalBounds.Width - constrainedWidth;
                newY = originalBounds.Y + (originalBounds.Height - constrainedHeight) / 2;
                break;
        }

        // 整数化以避免浮点误差累积
        return new Rect(Math.Round(newX), Math.Round(newY), Math.Round(constrainedWidth), Math.Round(constrainedHeight));
    }

    private void ApplyUniformResize(Rect newBounds)
    {
        var oldBounds = _originalSelectionBounds;
        double scaleX = oldBounds.Width > 0 ? newBounds.Width / oldBounds.Width : 1;
        double scaleY = oldBounds.Height > 0 ? newBounds.Height / oldBounds.Height : 1;

        foreach (var element in _selectedElements)
        {
            if (!_resizeStartStates.TryGetValue(element, out var startState)) continue;

            double rotation = element.Rotation;
            double elemWidth = startState.Width;
            double elemHeight = startState.Height;
            double elemCenterX = startState.X + elemWidth / 2;
            double elemCenterY = startState.Y + elemHeight / 2;
            double relX = (elemCenterX - oldBounds.Center.X) / oldBounds.Width;
            double relY = (elemCenterY - oldBounds.Center.Y) / oldBounds.Height;

            double newElemCenterX = newBounds.Center.X + relX * newBounds.Width;
            double newElemCenterY = newBounds.Center.Y + relY * newBounds.Height;

            double newWidth, newHeight;
            if (rotation == 0)
            {
                (newWidth, newHeight) = CalculateNonRotatedSize(elemWidth, elemHeight, scaleX, scaleY);
            }
            else
            {
                (newWidth, newHeight) = CalculateRotatedSize(elemWidth, elemHeight, scaleX, scaleY, rotation);
            }

            newWidth = Math.Max(MinElementSize, newWidth);
            newHeight = Math.Max(MinElementSize, newHeight);

            // 整数化以避免浮点误差
            element.X = Math.Round(newElemCenterX - newWidth / 2);
            element.Y = Math.Round(newElemCenterY - newHeight / 2);
            element.Width = Math.Round(newWidth);
            element.Height = Math.Round(newHeight);

            if (ElementControlMap?.TryGetValue(element, out var control) == true)
            {
                if (this.Parent == ((Panel?)control.Parent)!.Parent && control.Parent is Canvas)
                {
                    element.Left = element.X;
                    element.Top = element.Y;
                }
                else if (control.Parent is Canvas)
                {
                    var pos = _parentStartPosition;

                    element.Left = element.X - pos!.X;
                    element.Top = element.Y - pos!.Y;
                }

                //Canvas.SetLeft(control, element.Left);
                //Canvas.SetTop(control, element.Top);

                _renderControl!.UpdateLeft(element, control);
                _renderControl!.UpdateTop(element, control);

                if (control is IElementControlBase elementControl)
                {
                    elementControl!.UpdateWidth(element.Width);
                    elementControl!.UpdateHeight(element.Height);
                }
            }
        }
    }

    private (double Width, double Height) CalculateNonRotatedSize(double elemWidth, double elemHeight, double scaleX, double scaleY)
    {
        // 角点：对角固定，尺寸按比例变化
        // 边中点：对边固定，只变化对应维度
        return _activeResizeHandle switch
        {
            ResizeHandle.TopLeft or ResizeHandle.TopRight or ResizeHandle.BottomLeft or ResizeHandle.BottomRight =>
                (elemWidth * scaleX, elemHeight * scaleY),
            ResizeHandle.TopCenter or ResizeHandle.BottomCenter =>
                (elemWidth, elemHeight * scaleY),
            ResizeHandle.MiddleLeft or ResizeHandle.MiddleRight =>
                (elemWidth * scaleX, elemHeight),
            _ => (elemWidth * scaleX, elemHeight * scaleY)
        };
    }

    private (double Width, double Height) CalculateRotatedSize(double elemWidth, double elemHeight, double scaleX, double scaleY, double rotation)
    {
        // 对于旋转元素的尺寸计算，同样遵循：
        // - 角点：对角固定，尺寸按比例变化
        // - 边中点：对边固定，只变化对应维度
        double rad = rotation * Math.PI / 180;
        double cosA = Math.Cos(rad);
        double sinA = Math.Sin(rad);

        bool affectsWidth = false, affectsHeight = false;

        switch (_activeResizeHandle)
        {
            case ResizeHandle.TopLeft or ResizeHandle.TopRight or ResizeHandle.BottomLeft or ResizeHandle.BottomRight:
                affectsWidth = affectsHeight = true;
                break;
            case ResizeHandle.TopCenter or ResizeHandle.BottomCenter:
                affectsHeight = true;
                break;
            case ResizeHandle.MiddleLeft or ResizeHandle.MiddleRight:
                affectsWidth = true;
                break;
        }

        double localScaleX, localScaleY;

        if (affectsWidth && affectsHeight)
        {
            // 角点：对角固定，按比例缩放
            localScaleX = scaleX;
            localScaleY = scaleY;
        }
        else if (affectsWidth)
        {
            // 左右边中点：只改变宽度
            localScaleX = scaleX;
            localScaleY = 1;
        }
        else if (affectsHeight)
        {
            // 上下边中点：只改变高度
            localScaleX = 1;
            localScaleY = scaleY;
        }
        else
        {
            localScaleX = localScaleY = 1;
        }

        return (elemWidth * localScaleX, elemHeight * localScaleY);
    }

    #endregion

    #region 元素对齐

    private bool CheckCanvasParent(List<CanvasElement> elements, out Panel? parent)
    {
        parent = null;
        bool isSameParent = true, isCanvas = true;

        foreach (var element in elements)
        {
            if (ElementControlMap?.TryGetValue(element, out var control) != true) continue;

            if (parent == null)
            {
                parent = control!.Parent as Panel;
            }
            else if (isSameParent)
            {
                isSameParent = (parent == control!.Parent);
                isCanvas = control.Parent is Canvas;
            }

            if (!isCanvas || !isSameParent)
                break;
        }

        return isSameParent && isCanvas;
    }

    private void AlignLeft(List<CanvasElement> elements, Canvas parentPanel)
    {
        //必须统一容器下才进行排序
        if (elements.Count < 2 || !CheckCanvasParent(elements, out var parent))
            return;

        var minElement = elements.OrderBy(e => GetGlobalBounds(e, parent as Canvas).MinX).FirstOrDefault();
        int targetLeft = (int)Math.Ceiling(GetGlobalBounds(minElement!, parent as Canvas).MinX);
        var alignedElements = elements.Where(x => x != minElement).ToArray();

        foreach (var element in alignedElements)
        {
            if (ElementControlMap?.TryGetValue(element, out Control? control) == true && parent is Canvas)
            {
                int officeX = (int)GetGlobalBounds(element, parent as Canvas).MinX - targetLeft;

                if (element.Parent != null)
                {
                    element.Left -= officeX;
                }
                else
                {
                    element.X -= officeX;
                    element.Left = element.X;
                }
                _renderControl!.UpdateLeft(element, control);
            }
        }
    }

    private void AlignHorizontalCenter(List<CanvasElement> elements, Canvas parentPanel)
    {
        //必须统一容器下才进行排序
        if (!CheckCanvasParent(elements, out var parent)) return;

        int targetCenter;
        var bounds = elements.Select(e => GetGlobalBounds(e, parent as Canvas)).ToList();
        targetCenter = (int)((bounds.Min(b => b.MinX) + bounds.Max(b => b.MaxX)) / 2);

        foreach (var element in elements)
        {
            if (ElementControlMap?.TryGetValue(element, out Control? control) == true && parent is Canvas)
            {
                var value = GetGlobalBounds(element, parent as Canvas);
                int center = (int)((value.MinX + value.MaxX) / 2);
                int offsetX = targetCenter - center;

                if (element.Parent != null)
                {
                    element.Left += offsetX;
                }
                else
                {
                    element.X += offsetX;
                    element.Left = element.X;
                }
                _renderControl!.UpdateLeft(element, control);
            }
        }
    }

    private void AlignRight(List<CanvasElement> elements, Canvas parentPanel)
    {
        //必须统一容器下才进行排序
        if (elements.Count < 2 || !CheckCanvasParent(elements, out var parent))
            return;

        var maxElement = elements.OrderByDescending(e => GetGlobalBounds(e, parent as Canvas).MaxX).FirstOrDefault();
        int targetRight = (int)Math.Floor(GetGlobalBounds(maxElement!, parent as Canvas).MaxX);
        var alignedElements = elements.Where(x => x != maxElement).ToArray();

        foreach (var element in alignedElements)
        {
            if (ElementControlMap?.TryGetValue(element, out Control? control) == true && parent is Canvas)
            {
                int offsetX = (int)GetGlobalBounds(element, parent as Canvas).MaxX - targetRight;

                if (element.Parent != null)
                {
                    element.Left -= offsetX;
                }
                else
                {
                    element.X -= offsetX;
                    element.Left = element.X;
                }
                _renderControl!.UpdateLeft(element, control);
            }
        }
    }

    private void AlignTop(List<CanvasElement> elements, Canvas parentPanel)
    {
        //必须统一容器下才进行排序
        if (elements.Count < 2 || !CheckCanvasParent(elements, out var parent))
            return;

        var minElement = elements.OrderBy(e => GetGlobalBounds(e, parent as Canvas).MinY).FirstOrDefault();
        int targetTop = (int)Math.Ceiling(GetGlobalBounds(minElement!, parent as Canvas).MinY);
        var alignedElements = elements.Where(x => x != minElement).ToArray();

        foreach (var element in alignedElements)
        {
            if (ElementControlMap?.TryGetValue(element, out Control? control) == true && parent is Canvas)
            {
                int offsetY = (int)GetGlobalBounds(element, parent as Canvas).MinY - targetTop;

                if (element.Parent != null)
                {
                    element.Top -= offsetY;
                }
                else
                {
                    element.Y -= offsetY;
                    element.Top = element.Y;
                }
                _renderControl!.UpdateTop(element, control);
            }
        }
    }

    private void AlignVerticalCenter(List<CanvasElement> elements, Canvas parentPanel)
    {
        //必须统一容器下才进行排序
        if (!CheckCanvasParent(elements, out var parent)) return;

        int targetCenter;

        var bounds = elements.Select(e => GetGlobalBounds(e, parent as Canvas)).ToList();
        targetCenter = (int)RoundToPrecision((bounds.Min(b => b.MinY) + bounds.Max(b => b.MaxY)) / 2);

        foreach (var element in elements)
        {
            if (ElementControlMap?.TryGetValue(element, out Control? control) == true && parent is Canvas)
            {
                var value = GetGlobalBounds(element, parent as Canvas);
                int center = (int)((value.MinY + value.MaxY) / 2);
                int offsetY = targetCenter - center;

                if (element.Parent != null)
                {
                    element.Top += offsetY;
                }
                else
                {
                    element.Y += offsetY;
                    element.Top = element.Y;
                }
                _renderControl!.UpdateTop(element, control);
            }
        }
    }

    private void AlignBottom(List<CanvasElement> elements, Canvas parentPanel)
    {
        //必须统一容器下才进行排序
        if (!CheckCanvasParent(elements, out var parent)) return;

        var maxElement = elements.OrderByDescending(e => GetGlobalBounds(e, parent as Canvas).MaxY).FirstOrDefault();
        int targetBottom = (int)Math.Floor(GetGlobalBounds(maxElement!, parent as Canvas).MaxY);
        var alignedElements = elements.Where(x => x != maxElement).ToArray();

        foreach (var element in alignedElements)
        {
            if (ElementControlMap?.TryGetValue(element, out Control? control) == true && parent is Canvas)
            {
                int offsetY = (int)GetGlobalBounds(element, parent as Canvas).MaxY - targetBottom;

                if (element.Parent != null)
                {
                    element.Top -= offsetY;
                }
                else
                {
                    element.Y -= offsetY;
                    element.Top = element.Y;
                }
                _renderControl!.UpdateTop(element, control);
            }
        }
    }

    private void DistributeHorizontalSpacing(List<CanvasElement> elements, Control? parentPanel)
    {
        if (elements.Count <= 2) return;

        if (!CheckCanvasParent(elements, out var parent)) return;

        var sorted = elements.OrderBy(e =>
        {
            if (parent != parentPanel)
            {
                var a = GetGlobalBounds(e);
                return a.MinX + (a.MaxX - a.MinX) / 2;
            }
            else
            {
                var b = GetGlobalBounds(e);
                return b.MinX + (b.MaxX - b.MinX) / 2;
            }
        }).ToList();

        double firstPos, lastPos;

        if (parent != parentPanel)
        {
            var firstBounds = GetGlobalBounds(sorted[0], parent as Canvas);
            var lastBounds = GetGlobalBounds(sorted[^1], parent as Canvas);
            firstPos = firstBounds.MinX;
            lastPos = lastBounds.MaxX;
        }
        else
        {
            var firstBounds = GetGlobalBounds(sorted[0]);
            var lastBounds = GetGlobalBounds(sorted[^1]);
            firstPos = firstBounds.MinX;
            lastPos = lastBounds.MaxX;
        }

        double spacing = (lastPos - firstPos - sorted.Sum(e => GetGlobalBounds(e).MaxX - GetGlobalBounds(e).MinX)) / (sorted.Count - 1);

        //第一个元素位置固定
        int offsetX = (int)(GetGlobalBounds(sorted[0]).MinX - firstPos);
        sorted[0].X -= offsetX;
        sorted[0].Left = sorted[0].X;
        if (ElementControlMap?.TryGetValue(sorted[0], out var firstControl) == true)
        {
            _renderControl!.UpdateLeft(sorted[0], firstControl);
        }

        double startPos = GetGlobalBounds(sorted[0]).MinX;

        for (int i = 1; i < sorted.Count; i++)
        {
            var element = sorted[i];
            var prevElement = sorted[i - 1];

            var currentBounds = GetGlobalBounds(element);
            var prevBounds = GetGlobalBounds(prevElement);

            startPos = startPos + (prevBounds.MaxX - prevBounds.MinX) + spacing;
            offsetX = (int)(currentBounds.MinX - startPos);

            element.X -= offsetX;
            element.Left = element.X;

            if (ElementControlMap?.TryGetValue(element, out var control) == true)
            {
                _renderControl!.UpdateLeft(element, control);
            }
        }
    }

    private void DistributeVerticalSpacing(List<CanvasElement> elements, Control? parentPanel)
    {
        if (elements.Count <= 2) return;

        if (!CheckCanvasParent(elements, out var parent)) return;

        var sorted = elements.OrderBy(e =>
        {
            if (parent != parentPanel)
            {
                var a = GetGlobalBounds(e, parent as Canvas);
                return a.MinY + (a.MaxY - a.MinY) / 2;
            }
            else
            {
                var b = GetGlobalBounds(e);
                return b.MinY + (b.MaxY - b.MinY) / 2;
            }
        }).ToList();

        double firstPos, lastPos;

        if (parent != parentPanel)
        {
            var firstBounds = GetGlobalBounds(sorted[0], parent as Canvas);
            var lastBounds = GetGlobalBounds(sorted[^1], parent as Canvas);
            firstPos = firstBounds.MinY;
            lastPos = lastBounds.MaxY;
        }
        else
        {
            var firstBounds = GetGlobalBounds(sorted[0]);
            var lastBounds = GetGlobalBounds(sorted[^1]);
            firstPos = firstBounds.MinY;
            lastPos = lastBounds.MaxY;
        }

        double spacing = (lastPos - firstPos - sorted.Sum(e => GetGlobalBounds(e).MaxY - GetGlobalBounds(e).MinY)) / (sorted.Count - 1);

        //第一个元素位置固定
        int offsetY = (int)(GetGlobalBounds(sorted[0]).MinY - firstPos);
        sorted[0].Y -= offsetY;
        sorted[0].Top = sorted[0].Y;
        if (ElementControlMap?.TryGetValue(sorted[0], out var firstControl) == true)
        {
            _renderControl!.UpdateTop(sorted[0], firstControl);
        }

        double startPos = GetGlobalBounds(sorted[0]).MinY;

        for (int i = 1; i < sorted.Count; i++)
        {
            var element = sorted[i];
            var prevElement = sorted[i - 1];

            var currentBounds = GetGlobalBounds(element);
            var prevBounds = GetGlobalBounds(prevElement);

            startPos = startPos + (prevBounds.MaxY - prevBounds.MinY) + spacing;
            offsetY = (int)(currentBounds.MinY - startPos);

            element.Y -= offsetY;
            element.Top = element.Y;

            if (ElementControlMap?.TryGetValue(element, out var control) == true)
            {
                _renderControl!.UpdateTop(element, control);
            }
        }
    }

    private void MakeSameWidth(List<CanvasElement> elements)
    {
        if (elements.Count == 0) return;
        double refWidth = elements[0].Width;
        foreach (var element in elements.Where(e => Math.Abs(e.Width - refWidth) > 0.1))
        {
            element.Width = refWidth;
            element.Right = element.Left + refWidth;
            if (ElementControlMap?.TryGetValue(element, out var control) == true && control is IElementControlBase elementControl)
            {
                elementControl.UpdateWidth(refWidth);
            }
        }
    }

    private void MakeSameHeight(List<CanvasElement> elements)
    {
        if (elements.Count == 0) return;
        double refHeight = elements[0].Height;
        foreach (var element in elements.Where(e => Math.Abs(e.Height - refHeight) > 0.1))
        {
            element.Height = refHeight;
            element.Bottom = element.Top + refHeight;
            if (ElementControlMap?.TryGetValue(element, out var control) == true && control is IElementControlBase elementControl)
            {
                elementControl.UpdateHeight(refHeight);
            }
        }
    }

    private void MakeSameSize(List<CanvasElement> elements)
    {
        if (elements.Count == 0) return;
        double refWidth = elements[0].Width;
        double refHeight = elements[0].Height;

        foreach (var element in elements)
        {
            bool wChanged = Math.Abs(element.Width - refWidth) > 0.1;
            bool hChanged = Math.Abs(element.Height - refHeight) > 0.1;

            if (wChanged || hChanged)
            {
                element.Width = refWidth;
                element.Height = refHeight;
                element.Right = element.Left + refWidth;
                element.Bottom = element.Top + refHeight;

                if (ElementControlMap?.TryGetValue(element, out var control) == true && control is IElementControlBase elementControl)
                {
                    if (wChanged)
                    {
                        elementControl.UpdateWidth(refWidth);
                    }
                    if (hChanged)
                    {
                        elementControl.UpdateHeight(refHeight);
                    }
                }
            }
        }
    }

    #endregion

    #region 所有元素旋转

    private void OnRotateButtonPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_selectedElements.Count == 0)
        {
            return;
        }

        IsRotating = true;

        // 计算选择框的旋转中心（所有选中元素整体的中心点）
        var bounds = GetSelectionBounds();
        _rotateCenterPoint = bounds.Center;

        // 隐藏选择框和调整手柄
        _selectionRect.IsVisible = false;
        foreach (var handle in _resizeHandles)
            handle.IsVisible = false;

        // 显示旋转中心指示器
        Canvas.SetLeft(_rotateCenterIndicator, _rotateCenterPoint.X - 3);
        Canvas.SetTop(_rotateCenterIndicator, _rotateCenterPoint.Y - 3);
        _rotateCenterIndicator.IsVisible = true;

        // 计算旋转半径：圆心到旋转按钮初始位置的距离
        var rotateButtonPos = new Point(
            Canvas.GetLeft(_rotateButton) + _rotateButton.Bounds.Width / 2,
            Canvas.GetTop(_rotateButton) + _rotateButton.Bounds.Height / 2);
        _rotateRadius = Math.Sqrt(
            Math.Pow(rotateButtonPos.X - _rotateCenterPoint.X, 2) +
            Math.Pow(rotateButtonPos.Y - _rotateCenterPoint.Y, 2));

        // 显示引导圆
        _rotateGuideCircle.Width = _rotateRadius * 2;
        _rotateGuideCircle.Height = _rotateRadius * 2;
        Canvas.SetLeft(_rotateGuideCircle, _rotateCenterPoint.X - _rotateRadius);
        Canvas.SetTop(_rotateGuideCircle, _rotateCenterPoint.Y - _rotateRadius);
        _rotateGuideCircle.IsVisible = true;

        // 计算初始角度（从中心到旋转按钮位置）
        double dx = rotateButtonPos.X - _rotateCenterPoint.X;
        double dy = rotateButtonPos.Y - _rotateCenterPoint.Y;
        _rotateStartAngle = Math.Atan2(dy, dx) * 180 / Math.PI;

        // 记录每个选中元素的初始状态
        _elementStartRotations.Clear();
        _elementStartStates.Clear();
        foreach (var element in _selectedElements)
        {
            _elementStartRotations[element] = element.Rotation;
            _elementStartStates[element] = (element.X, element.Y, element.Width, element.Height);
        }
        _selectionStartRotation = _selectedElements[0].Rotation;

        e.Handled = true;
    }

    private void OnRotateButtonMoved(object? sender, PointerEventArgs e)
    {
        if (!IsRotating || _selectedElements.Count == 0)
            return;

        bool isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        var currentPoint = e.GetPosition(this);

        // 将鼠标位置约束到引导圆上
        double dx = currentPoint.X - _rotateCenterPoint.X;
        double dy = currentPoint.Y - _rotateCenterPoint.Y;
        double currentAngleRad = Math.Atan2(dy, dx);
        double constrainedX = _rotateCenterPoint.X + _rotateRadius * Math.Cos(currentAngleRad);
        double constrainedY = _rotateCenterPoint.Y + _rotateRadius * Math.Sin(currentAngleRad);
        var constrainedPoint = new Point(constrainedX, constrainedY);

        // 将旋转按钮移动到圆上
        Canvas.SetLeft(_rotateButton, constrainedPoint.X - _rotateButton.Bounds.Width / 2);
        Canvas.SetTop(_rotateButton, constrainedPoint.Y - _rotateButton.Bounds.Height / 2);

        // 计算角度差并更新所有元素
        double currentAngleDeg = currentAngleRad * 180 / Math.PI;
        double angleDelta = currentAngleDeg - _rotateStartAngle;

        // Ctrl 按下时吸附到 5 的倍数角度
        if (isCtrlPressed)
        {
            double rawAngle = _selectionStartRotation + angleDelta;
            double snappedAngle = Math.Round(rawAngle / SnapAngleStep) * SnapAngleStep;
            double snapCorrection = snappedAngle - rawAngle;

            angleDelta += snapCorrection;

            DrawAngleReferenceLines(_selectionStartRotation + angleDelta);
        }
        else
        {
            HideAngleReferenceLines();
        }

        // 修改旋转把的角度
        _rotateButton.RenderTransform = new RotateTransform(angleDelta);

        foreach (var element in _selectedElements)
        {
            if (!_elementStartRotations.TryGetValue(element, out var startRotation)) continue;
            if (!_elementStartStates.TryGetValue(element, out var startState)) continue;

            double elemCenterX = startState.X + startState.Width / 2;
            double elemCenterY = startState.Y + startState.Height / 2;

            double offsetX = elemCenterX - _rotateCenterPoint.X;
            double offsetY = elemCenterY - _rotateCenterPoint.Y;

            double rad = angleDelta * Math.PI / 180;
            double cosA = Math.Cos(rad);
            double sinA = Math.Sin(rad);

            double newOffsetX = offsetX * cosA - offsetY * sinA;
            double newOffsetY = offsetX * sinA + offsetY * cosA;

            double newCenterX = _rotateCenterPoint.X + newOffsetX;
            double newCenterY = _rotateCenterPoint.Y + newOffsetY;
            double newX = newCenterX - startState.Width / 2;
            double newY = newCenterY - startState.Height / 2;

            element.X = newX;
            element.Y = newY;
            element.Rotation = startRotation + angleDelta;

            if (ElementControlMap?.TryGetValue(element, out var control) == true)
            {
                if (control.Parent != null && control.Parent is Canvas canvas && element.Parent != null)
                {
                    var parentPos = ((Panel)control.Parent).TranslatePoint(new Point(0, 0), this);
                    element.Left = element.X - parentPos!.Value.X;
                    element.Top = element.Y - parentPos.Value.Y;

                    _renderControl!.UpdateLeft(element, control);
                    _renderControl!.UpdateTop(element, control);
                }
                else
                {
                    _renderControl!.UpdateX(element, control);
                    _renderControl!.UpdateY(element, control);
                }

                if (control is IElementControlBase elementControl)
                {
                    elementControl.UpdateRotation(element.Rotation);

                    ElementRotating?.Invoke(this, new ElementRotatedEventArgs(new List<CanvasElement>(_selectedElements)));
                }
            }
        }
    }

    private void OnRotateButtonReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!IsRotating)
        {
            return;
        }

        IsRotating = false;
        _elementStartRotations.Clear();
        _elementStartStates.Clear();

        _rotateButton.RenderTransform = new RotateTransform(0);
        // 隐藏旋转指示器和引导圆
        _rotateCenterIndicator.IsVisible = false;
        _rotateGuideCircle.IsVisible = false;
        HideAngleReferenceLines();

        // 恢复选择框和手柄
        if (_selectedElements.Count > 0)
        {
            UpdateSelectionOverlay(_selectedElements.ToList());
        }

        ElementRotated?.Invoke(this, new ElementRotatedEventArgs(new List<CanvasElement>(_selectedElements)));
    }

    #endregion

    #region 遮罩

    private void UpdateSingleOverlay()
    {
        if (_selectedElement == null || !_elementControlMap!.TryGetValue(_selectedElement, out var control))
        {
            ClearSingleOverlay();
            return;
        }

        _selectionRect.IsVisible = false;

        double elemWidth = _selectedElement.Width;
        double elemHeight = _selectedElement.Height;
        double rotation = _selectedElement.Rotation;

        // 应该跟Pressed时候的位置一致
        var corners = GetElementVisualCorners(control, _selectedElement);
        if (corners != null)
        {
            _rotateCenterPoint = corners.Value.Center;
        }

        // 半径 = 中心到右上角的距离（对角线的一半）
        double halfW = _selectedElement.Width / 2;
        double halfH = _selectedElement.Height / 2;
        _rotateRadius = Math.Sqrt(halfW * halfW + halfH * halfH) + 10;
        double rotationRad = rotation * Math.PI / 180;

        _rotatedSelectionPolygon!.IsVisible = true;

        if (corners == null)
        {
            ClearSingleOverlay();
            return;
        }

        var c = corners.Value;

        _rotatedSelectionPolygon.Points = new AvaloniaList<Point>
            {
                new Point(c.TopLeft.X, c.TopLeft.Y),
                new Point(c.TopRight.X, c.TopRight.Y),
                new Point(c.BottomRight.X, c.BottomRight.Y),
                new Point(c.BottomLeft.X, c.BottomLeft.Y)
            };

        UpdateSingleRotatedResizeHandles(c);

        if (!IsRotating)
        {
            // 获取上级容器的旋转角度
            double parentRotation = 0;
            if (_elementControlMap?.TryGetValue(_selectedElement, out var ctrl) == true)
            {
                parentRotation = GetParentRotationFromControl(ctrl, this);
            }

            // 旋转按钮位置：使用正确的向量旋转计算
            // 中心到右上角的初始角度（未旋转时的角度）
            double initialAngle = Math.Atan2(-halfH, halfW);
            // 总旋转角度 = 元素自身旋转 + 上级容器旋转
            double totalRotation = rotation + parentRotation;
            double totalRotationRad = totalRotation * Math.PI / 180;
            double finalAngle = totalRotationRad + initialAngle;
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

    public void ClearSingleOverlay()
    {
        _selectedElement = null;
        _selectionRect.IsVisible = false;
        _rotatedSelectionPolygon!.IsVisible = false;
        HideResizeHandles();
        _rotateButton.IsVisible = false;
        _rotateCenterIndicator.IsVisible = false;
        _rotateGuideCircle.IsVisible = false;
        _rotatedSelectionPolygon.IsVisible = false;
        HideAngleReferenceLines();
    }

    #endregion


    #region 单项旋转

    private Polygon? _rotatedSelectionPolygon;
    private double _elementStartRotation;
    private CanvasElement? _selectedElement;
    private (double X, double Y, double Width, double Height, double Rotation) _elementStartState;
    private Point _resizeFixedAnchorWorld;
    private (Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft) _pressedVisualCorners;

    public void SetSelectedSingleElement(CanvasElement? element)
    {
        if (_selectedElement == element) 
            return;

        _selectedElement = element;

        if (_selectedElement != null)
        {
            SingleSelectionHandleBind();
            UpdateSingleOverlay();
        }
        else
        {
            MultiSelectionHandleBind();
            ClearSingleOverlay();
        }
    }

    private void InitializeSingleSelection()
    {
        if (_rotatedSelectionPolygon == null)
        {
            _rotatedSelectionPolygon = new Polygon
            {
                Stroke = new SolidColorBrush(Color.Parse(AuxLineColor)),
                StrokeThickness = SelectionBorderWidth,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 4 },
                Fill = new SolidColorBrush(Color.Parse("#301E90FF")),
                IsHitTestVisible = false,
                IsVisible = false
            };

            Children.Add(_rotatedSelectionPolygon);
        }
    }

    private void OnSingleRotateButtonPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_selectedElement == null) return;

        _isRotating = true;

        if (_elementControlMap?.TryGetValue(_selectedElement, out var control) == true)
        {
            var corners = GetElementVisualCorners(control, _selectedElement);
            if (corners != null)
                _rotateCenterPoint = corners.Value.Center;
        }

        _selectionRect.IsVisible = false;
        _rotatedSelectionPolygon!.IsVisible = false;
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

        // 获取上级容器的旋转角度
        double parentRotation = 0;
        if (_elementControlMap?.TryGetValue(_selectedElement, out var ctrl) == true)
        {
            parentRotation = GetParentRotationFromControl(ctrl, this);
        }

        // 初始角度：当前元素旋转下，旋转把相对于圆心的角度
        // top-right 在元素局部坐标中的角度
        double initialAngle = Math.Atan2(-halfH, halfW);
        // 总旋转角度 = 元素自身旋转 + 上级容器旋转
        double totalRotation = _selectedElement.Rotation + parentRotation;
        // 旋转把在圆周上的实际角度 = 总旋转 + 初始角度
        double finalAngleRad = totalRotation * Math.PI / 180 + initialAngle;
        // 用圆几何计算起始角度（而非从按钮屏幕位置读），保证与 Moved 中的约束一致
        _rotateStartAngle = finalAngleRad * 180 / Math.PI;

        _elementStartRotation = _selectedElement.Rotation;

        e.Handled = true;
    }

    private void OnSingleRotateButtonMoved(object? sender, PointerEventArgs e)
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
            DrawSingleAngleReferenceLines(snappedAngle);
        }
        else
        {
            HideSingleAngleReferenceLines();
        }

        ApplySingleRotation(newRotation);
        UpdateSingleOverlay();

    }

    private void OnSingleRotateButtonReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isRotating || _selectedElement == null) return;

        _isRotating = false;

        _rotateCenterIndicator.IsVisible = false;
        _rotateGuideCircle.IsVisible = false;
        HideSingleAngleReferenceLines();

        UpdateSingleOverlay();
        ElementRotated?.Invoke(this, new ElementRotatedEventArgs(new List<CanvasElement>([_selectedElement])));
    }

    private void ApplySingleRotation(double newRotation)
    {
        if (_selectedElement == null) return;

        _selectedElement.Rotation = newRotation;

        if (_elementControlMap?.TryGetValue(_selectedElement, out var control) == true &&
            control is IElementControlBase ecb)
        {
            ecb.UpdateRotation(newRotation);
        }
    }

    private (Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft, Point Center)? GetElementVisualCorners(Control control, CanvasElement element)
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

    private void UpdateSingleRotatedResizeHandles((Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft, Point Center) corners)
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

    #endregion

    #region 旋转参考线

    private void DrawSingleAngleReferenceLines(double snappedAngle)
    {
        double rad = snappedAngle * Math.PI / 180;
        double length = 2000;
        double perpRad = (snappedAngle + 90) * Math.PI / 180;

        ShowSingleAngleReferenceLine(0,
            _rotateCenterPoint.X - length * Math.Cos(rad),
            _rotateCenterPoint.Y - length * Math.Sin(rad),
            _rotateCenterPoint.X + length * Math.Cos(rad),
            _rotateCenterPoint.Y + length * Math.Sin(rad));

        ShowSingleAngleReferenceLine(1,
            _rotateCenterPoint.X - length * Math.Cos(perpRad),
            _rotateCenterPoint.Y - length * Math.Sin(perpRad),
            _rotateCenterPoint.X + length * Math.Cos(perpRad),
            _rotateCenterPoint.Y + length * Math.Sin(perpRad));
    }

    private void ShowSingleAngleReferenceLine(int index, double x1, double y1, double x2, double y2)
    {
        if (index < 0 || index >= _angleReferenceLines.Length)
            return;
        _angleReferenceLines[index].StartPoint = new Point(x1, y1);
        _angleReferenceLines[index].EndPoint = new Point(x2, y2);
        _angleReferenceLines[index].IsVisible = true;
    }

    private void HideSingleAngleReferenceLines()
    {
        foreach (var line in _angleReferenceLines)
            line.IsVisible = false;
    }

    #endregion

    #region 单项调整大小

    /// <summary>
    /// 符号方向：用于旋转矩形缩放的垂直/水平方向判定
    /// </summary>
    public enum SignDirection
    {
        None ,
        /// <summary>
        /// 负方向（向下/向右）
        /// </summary>
        Negative ,

        /// <summary>
        /// 零（无方向）
        /// </summary>
        Zero ,

        /// <summary>
        /// 正方向（向上/向左）
        /// </summary>
        Positive ,

    };

    private SignDirection _signMoveDirection;
    private SignDirection _signXMoveDirection;
    private SignDirection _signYMoveDirection;

    (Point TopLeft, Point TopRight, Point BottomRight, Point BottomLeft, Point Center)? corners;
    private void OnSingleResizeHandlePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Ellipse ellipse || _selectedElement == null) return;

        _signMoveDirection = SignDirection.None;
        _signXMoveDirection = SignDirection.None;
        _signYMoveDirection = SignDirection.None;

        _isResizing = true;
        _activeResizeHandle = (ResizeHandle)ellipse.Tag!;
        _resizeStartPoint = e.GetPosition(this);

        _elementStartState = (
            _selectedElement.X, _selectedElement.Y,
            _selectedElement.Width, _selectedElement.Height,
            _selectedElement.Rotation);

        double centerX = _elementStartState.X + _elementStartState.Width / 2;
        double centerY = _elementStartState.Y + _elementStartState.Height / 2;
        double rad = _elementStartState.Rotation * Math.PI / 180;
        double cosA = Math.Cos(rad);
        double sinA = Math.Sin(rad);
        double halfW = _elementStartState.Width / 2;
        double halfH = _elementStartState.Height / 2;
        _resizeFixedAnchorWorld = GetFixedCornerWorld(_activeResizeHandle, centerX, centerY, halfW, halfH, cosA, sinA);

        // 在 Press 时一次性记录视觉角点，拖动全程使用这些固定坐标作为锚点
        if (ElementControlMap!.TryGetValue(_selectedElement, out var control))
        {
            var corners = GetElementVisualCorners(control, _selectedElement);
            if (corners != null)
            {
                _pressedVisualCorners = (corners.Value.TopLeft, corners.Value.TopRight,
                    corners.Value.BottomRight, corners.Value.BottomLeft);
                _rotateCenterPoint = corners.Value.Center;
            }
        }

        e.Handled = true;
    }

    private void OnSingleResizeHandleMoved(object? sender, PointerEventArgs e)
    {
        if (!_isResizing || _selectedElement == null)
        {
            _activeResizeHandle = ResizeHandle.None;
            return;
        }
        if (!e.Properties.IsLeftButtonPressed && !e.Properties.IsRightButtonPressed)
        {
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            return;
        }

        var currentPoint = e.GetPosition(this);
        var delta = currentPoint - _resizeStartPoint;

        double newX = _elementStartState.X;
        double newY = _elementStartState.Y;
        double newWidth = _elementStartState.Width;
        double newHeight = _elementStartState.Height;

        bool isRotated = _elementStartState.Rotation != 0;

        if (isRotated)
        {
            // 旋转元素的调整：使用固定锚点的方式
            ApplySingleRotatedResize(currentPoint);
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
                    if (newX >= (_elementStartState.X + _elementStartState.Width) || newY >= (_elementStartState.Y + _elementStartState.Height))
                    {
                        return;
                    }
                    break;

                case ResizeHandle.TopRight:
                    newY = _elementStartState.Y + delta.Y;
                    newWidth = _elementStartState.Width + delta.X;
                    newHeight = _elementStartState.Height - delta.Y;
                    if (newWidth < 0 || newY >= (_elementStartState.Y + _elementStartState.Height))
                    {
                        return;
                    }
                    break;

                case ResizeHandle.BottomLeft:
                    newX = _elementStartState.X + delta.X;
                    newWidth = _elementStartState.Width - delta.X;
                    newHeight = _elementStartState.Height + delta.Y;
                    if (newX >= (_elementStartState.X + _elementStartState.Width) || newHeight <= 0)
                    {
                        return;
                    }
                    break;

                case ResizeHandle.BottomRight:
                    newWidth = _elementStartState.Width + delta.X;
                    newHeight = _elementStartState.Height + delta.Y;
                    if (newWidth <= 0 || newHeight <= 0)
                    {
                        return;
                    }
                    break;

                // 边中点：只改变对应维度，对边固定
                case ResizeHandle.TopCenter:
                    newY = _elementStartState.Y + delta.Y;
                    newHeight = _elementStartState.Height - delta.Y;
                    if (newY > (_elementStartState.Y + _elementStartState.Height) || newHeight <= 0)
                    {
                        return;
                    }
                    break;

                case ResizeHandle.BottomCenter:
                    newHeight = _elementStartState.Height + delta.Y;
                    if (newHeight <= 0)
                    {
                        return;
                    }
                    break;

                case ResizeHandle.MiddleLeft:
                    newX = _elementStartState.X + delta.X;
                    newWidth = _elementStartState.Width - delta.X;
                    if (newWidth <= 0 || newX >= (_elementStartState.X + _elementStartState.Width))
                    {
                        return;
                    }
                    break;

                case ResizeHandle.MiddleRight:
                    newWidth = _elementStartState.Width + delta.X;
                    if (newWidth <= 0)
                    {
                        return;
                    }
                    break;
            }

            // 应用尺寸约束
            //if (newWidth < MinElementSize)
            //{
            //    if (_activeResizeHandle == ResizeHandle.TopLeft ||
            //        _activeResizeHandle == ResizeHandle.MiddleLeft)
            //        newX = _elementStartState.X + _elementStartState.Width - MinElementSize;
            //    newWidth = MinElementSize;
            //}
            //if (newHeight < MinElementSize)
            //{
            //    if (_activeResizeHandle == ResizeHandle.TopLeft ||
            //        _activeResizeHandle == ResizeHandle.TopCenter)
            //        newY = _elementStartState.Y + _elementStartState.Height - MinElementSize;
            //    newHeight = MinElementSize;
            //}

            ApplySingleResize(newX, newY, newWidth, newHeight);
        }
        UpdateSingleOverlay();
    }

    private void ApplySingleRotatedResize(Point currentPoint)
    {
        if (_selectedElement == null) return;

        double signedDist = 0;
        double centerX = _elementStartState.X + _elementStartState.Width / 2;
        double centerY = _elementStartState.Y + _elementStartState.Height / 2;
        double rad = _elementStartState.Rotation * Math.PI / 180;
        double cosA = Math.Cos(rad);
        double sinA = Math.Sin(rad);

        double halfW = _elementStartState.Width / 2;
        double halfH = _elementStartState.Height / 2;

        // 将鼠标移动从屏幕坐标转换到元素局部坐标系
        var delta = currentPoint - _resizeStartPoint;
        double localDeltaX = delta.X * cosA + delta.Y * sinA;
        double localDeltaY = -delta.X * sinA + delta.Y * cosA;

        double newHalfW = halfW;
        double newHalfH = halfH;
        double newCenterX = centerX;
        double newCenterY = centerY;

        double newX = newCenterX - newHalfW;
        double newY = newCenterY - newHalfH;
        double newWidth = newHalfW * 2;
        double newHeight = newHalfH * 2;

        switch (_activeResizeHandle)
        {
            case ResizeHandle.TopLeft:
            case ResizeHandle.TopRight:
            case ResizeHandle.BottomLeft:
            case ResizeHandle.BottomRight:
                {
                    // 角点：对角固定，拖动改变尺寸，中心移动
                    // 在局部坐标系中，四个角点的初始位置：
                    // - TopLeft: (-halfW, -halfH)
                    // - TopRight: (+halfW, -halfH)
                    // - BottomLeft: (-halfW, +halfH)
                    // - BottomRight: (+halfW, +halfH)
                    // 对角关系：
                    // - TopLeft ↔ BottomRight
                    // - TopRight ↔ BottomLeft
                    // - BottomLeft ↔ TopRight
                    // - BottomRight ↔ TopLeft

                    double newCornerX, newCornerY;
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

                    newCornerX += localDeltaX;
                    newCornerY += localDeltaY;

                    //newHalfW = Math.Max(MinElementSize / 2, Math.Abs(fixedX - newCornerX) / 2);
                    //newHalfH = Math.Max(MinElementSize / 2, Math.Abs(fixedY - newCornerY) / 2);
                    double dX = fixedX - newCornerX;
                    double dY = fixedY - newCornerY;

                    if (_signXMoveDirection == SignDirection.None || _signYMoveDirection == SignDirection.None)
                    {
                        if (dX > 0)
                        {
                            _signXMoveDirection = SignDirection.Positive;
                        }
                        else if (dX < 0)
                        {
                            _signXMoveDirection = SignDirection.Negative;
                        }
                        else if (dX == 0)
                        {
                            _signXMoveDirection = SignDirection.Zero;
                        }
                        if (dY > 0)
                        {
                            _signYMoveDirection = SignDirection.Positive;
                        }
                        else if (dY < 0)
                        {
                            _signYMoveDirection = SignDirection.Negative;
                        }
                        else if (dY == 0)
                        {
                            _signYMoveDirection = SignDirection.Zero;
                        }
                    }
                    else
                    {
                        switch (_signXMoveDirection)
                        {
                            case SignDirection.Negative:
                                if (dX >= 0)
                                {
                                    return;
                                }
                                break;
                            case SignDirection.Positive:
                                if (dX <= 0)
                                {
                                    return;
                                }
                                break;
                            case SignDirection.Zero:
                                if (dX!= 0)
                                {
                                    return;
                                }
                                break;
                        }
                        switch (_signYMoveDirection)
                        {
                            case SignDirection.Negative:
                                if (dY >= 0)
                                {
                                    return;
                                }
                                break;
                            case SignDirection.Positive:
                                if (dY <= 0)
                                {
                                    return;
                                }
                                break;
                            case SignDirection.Zero:
                                if (dY != 0)
                                {
                                    return;
                                }
                                break;
                        }
                    }

                    newHalfW = Math.Abs(dX) / 2;
                    newHalfH = Math.Abs(dY) / 2;

                    double newCenterLocalX = (newCornerX + fixedX) / 2;
                    double newCenterLocalY = (newCornerY + fixedY) / 2;

                    newCenterX = centerX + newCenterLocalX * cosA - newCenterLocalY * sinA;
                    newCenterY = centerY + newCenterLocalX * sinA + newCenterLocalY * cosA;
                }

                newX = newCenterX - newHalfW;
                newY = newCenterY - newHalfH;
                newWidth = newHalfW * 2;
                newHeight = newHalfH * 2;
                break;

            case ResizeHandle.TopCenter:
            case ResizeHandle.BottomCenter:
                {
                    // 上下边中点手柄：TopCenter 固定底边（BottomLeft-BottomRight），BottomCenter 固定顶边（TopLeft-TopRight）
                    // 以 Press 时记录的 _pressedVisualCorners 作为固定锚点

                    // 固定边两端的视觉角点（世界坐标）
                    Point fixedEdgeP1, fixedEdgeP2;
                    if (_activeResizeHandle == ResizeHandle.TopCenter)
                    {
                        fixedEdgeP1 = _pressedVisualCorners.BottomLeft;
                        fixedEdgeP2 = _pressedVisualCorners.BottomRight;
                    }
                    else
                    {
                        fixedEdgeP1 = _pressedVisualCorners.TopLeft;
                        fixedEdgeP2 = _pressedVisualCorners.TopRight;
                    }

                    // 计算 currentPoint 到 fixedEdgeP1 和 fixedEdgeP2 所在直线的距离
                    // 公式：d = |(P2-P1) × (P1-P0)| / |P2-P1|
                    // 向量 P2-P1
                    double edgeX = fixedEdgeP2.X - fixedEdgeP1.X;
                    double edgeY = fixedEdgeP2.Y - fixedEdgeP1.Y;
                    double edgeLen = Math.Sqrt(edgeX * edgeX + edgeY * edgeY);
                    if (edgeLen < 0.001) edgeLen = 0.001;

                    // 向量 P1-P0
                    double v1x = fixedEdgeP1.X - currentPoint.X;
                    double v1y = fixedEdgeP1.Y - currentPoint.Y;

                    // 叉积的绝对值：|v1 × edge| = |v1x * edgeY - v1y * edgeX|
                    double cross = Math.Abs(v1x * edgeY - v1y * edgeX);

                    // edgePerp 是指向顶边的垂直方向单位向量（逆时针旋转 90°）
                    double edgePerpX = -edgeY / edgeLen;
                    double edgePerpY = edgeX / edgeLen;

                    // 底边中点
                    double bottomMidX = (fixedEdgeP1.X + fixedEdgeP2.X) / 2;
                    double bottomMidY = (fixedEdgeP1.Y + fixedEdgeP2.Y) / 2;

                    // 鼠标到底边的有符号垂直距离
                    // toMouse 到 edgePerp 的点积就是投影距离
                    double toMouseX = currentPoint.X - bottomMidX;
                    double toMouseY = currentPoint.Y - bottomMidY;
                    signedDist = toMouseX * edgePerpX + toMouseY * edgePerpY;

                    // 新的半高 = |signedDist|（始终为正）
                    //newHalfH = Math.Max(MinElementSize, Math.Abs(signedDist)) / 2;
                    newHalfH = Math.Abs(signedDist) / 2;

                    // 新的顶边中点 = 底边中点 + edgePerp * signedDist
                    double topMidX = bottomMidX + edgePerpX * signedDist;
                    double topMidY = bottomMidY + edgePerpY * signedDist;

                    // TopLeft = topMid - halfWidth * edgeDir, TopRight = topMid + halfWidth * edgeDir
                    double halfWidthX = edgeX / 2;
                    double halfWidthY = edgeY / 2;
                    double newTopLeftX = topMidX - halfWidthX;
                    double newTopLeftY = topMidY - halfWidthY;
                    double newTopRightX = topMidX + halfWidthX;
                    double newTopRightY = topMidY + halfWidthY;

                    // 世界坐标中新矩形中心 = (顶边中点 + 底边中点) / 2
                    newCenterX = (topMidX + bottomMidX) / 2;
                    newCenterY = (topMidY + bottomMidY) / 2;

                    // 宽度不变
                    newHalfW = halfW;

                    // 将 newTopLeft 从世界坐标转换到局部坐标（未旋转状态）
                    // localX = cosA * (worldX - centerX) + sinA * (worldY - centerY)
                    // localY = -sinA * (worldX - centerX) + cosA * (worldY - centerY)
                    double newTopLeftLocalX = cosA * (newTopLeftX - newCenterX) + sinA * (newTopLeftY - newCenterY);
                    double newTopLeftLocalY = -sinA * (newTopLeftX - newCenterX) + cosA * (newTopLeftY - newCenterY);

                    // 元素的 Left、Top（局部坐标中左上角位置）
                    double newElementLeft = newTopLeftLocalX - newHalfW;
                    double newElementTop = newTopLeftLocalY - newHalfH;

                    // 计算并显示未旋转状态的 TopLeft（绿点）
                    var unrotatedTopLeft = CalculateAndShowUnrotatedTopLeft(newCenterX, newCenterY, _elementStartState.Rotation, newHalfH);

                    newX = unrotatedTopLeft.X;
                    newY = unrotatedTopLeft.Y;
                    newWidth = newHalfW * 2;
                    newHeight = newHalfH * 2;
                }
                break;

            case ResizeHandle.MiddleLeft:
            case ResizeHandle.MiddleRight:
                {
                    // 左右边中点手柄：
                    //   MiddleLeft  →  固定右边缘（fixedEdgeP1=TopRight, fixedEdgeP2=BottomRight），拉动左边缘
                    //   MiddleRight →  固定左边缘（fixedEdgeP1=TopLeft, fixedEdgeP2=BottomLeft），拉动右边缘
                    // 以 Press 时记录的 _pressedVisualCorners 作为固定锚点

                    // 固定边两端的视觉角点（世界坐标）
                    //   ⓵ fixedEdgeP1 (TopRight 或 TopLeft)
                    //   ⓶ fixedEdgeP2 (BottomRight 或 BottomLeft)
                    //   ⓷ fixedMid = (fixedEdgeP1 + fixedEdgeP2) / 2  固定边中点
                    Point fixedEdgeP1, fixedEdgeP2;
                    if (_activeResizeHandle == ResizeHandle.MiddleLeft)
                    {
                        fixedEdgeP1 = _pressedVisualCorners.TopRight;
                        fixedEdgeP2 = _pressedVisualCorners.BottomRight;
                    }
                    else
                    {
                        fixedEdgeP1 = _pressedVisualCorners.TopLeft;
                        fixedEdgeP2 = _pressedVisualCorners.BottomLeft;
                    }

                    //计算鼠标点到fixedEdgeP1和fixedEdgeP2所在线段的垂直距离
                    // 向量 P2-P1（固定边方向）
                    double edgeX = fixedEdgeP2.X - fixedEdgeP1.X;
                    double edgeY = fixedEdgeP2.Y - fixedEdgeP1.Y;
                    double edgeLen = Math.Sqrt(edgeX * edgeX + edgeY * edgeY);
                    if (edgeLen < 0.001) edgeLen = 0.001;

                    // 向量 P1-P0（从固定边P1指向鼠标点P0）
                    double v1x = fixedEdgeP1.X - currentPoint.X;
                    double v1y = fixedEdgeP1.Y - currentPoint.Y;

                    // 叉积的绝对值：|v1 × edge| = |v1x * edgeY - v1y * edgeX|
                    // 这是计算点到直线距离的关键
                    double cross = Math.Abs(v1x * edgeY - v1y * edgeX);

                    // edgePerp 是指向活动边的垂直方向单位向量
                    // 顺时针旋转 90°：(_x, _y) -> (_y, -_x)
                    double edgePerpX = edgeY / edgeLen;
                    double edgePerpY = -edgeX / edgeLen;

                    // 固定边中点
                    double fixedMidX = (fixedEdgeP1.X + fixedEdgeP2.X) / 2;
                    double fixedMidY = (fixedEdgeP1.Y + fixedEdgeP2.Y) / 2;

                    // 鼠标到固定边的有符号垂直距离
                    // toMouse 到 edgePerp 的点积就是投影距离
                    double toMouseX = currentPoint.X - fixedMidX;
                    double toMouseY = currentPoint.Y - fixedMidY;
                    signedDist = toMouseX * edgePerpX + toMouseY * edgePerpY;

                    // 新的半高 = |signedDist|（始终为正）
                    //newHalfW = Math.Max(MinElementSize, Math.Abs(signedDist)) / 2;
                    newHalfW = Math.Abs(signedDist) / 2;

                    // 根据fixedEdgeP1和fixedEdgeP2和高度、角度计算活动边的两个端点
                    // 活动边与固定边平行，长度相同（都是元素高度）
                    // 活动边中点 = 固定边中点 + edgePerp * signedDist
                    double movingMidX = fixedMidX + edgePerpX * signedDist;
                    double movingMidY = fixedMidY + edgePerpY * signedDist;
                    // 活动边的两个端点 = 活动边中点 ± edge / 2
                    double movingP1X = movingMidX - edgeX / 2;
                    double movingP1Y = movingMidY - edgeY / 2;
                    double movingP2X = movingMidX + edgeX / 2;
                    double movingP2Y = movingMidY + edgeY / 2;

                    //中心点位置
                    newCenterX = (fixedEdgeP1.X + movingP2X) / 2;
                    newCenterY = (fixedEdgeP1.Y + movingP2Y) / 2;

                    // 将活动边的角点从世界坐标转换为局部坐标（考虑旋转）
                    // 世界坐标到局部坐标的反向旋转：
                    // localX = cosA * (worldX - centerX) + sinA * (worldY - centerY)
                    // localY = -sinA * (worldX - centerX) + cosA * (worldY - centerY)
                    // 注意：Avalonia 中旋转角度为正时是顺时针，所以反向旋转用 -sinA

                    // 将移动边的 TopLeft (movingP1) 转换到局部坐标
                    double localMovingP1X = cosA * (movingP1X - newCenterX) + sinA * (movingP1Y - newCenterY);
                    double localMovingP1Y = -sinA * (movingP1X - newCenterX) + cosA * (movingP1Y - newCenterY);

                    // 元素的 Left、Top（局部坐标中左上角位置）
                    // 注意：局部坐标中 TopLeft 的 Y 是负的半高
                    double newElementLeft = localMovingP1X - newHalfW;
                    double newElementTop = localMovingP1Y - newHalfH;

                    // 计算并显示未旋转状态的 TopLeft（绿点）
                    // 这里使用活动边的局部 TopLeft 作为新矩形的左上角参考点
                    var unrotatedTopLeft = CalculateAndShowUnrotatedTopLeft(newCenterX, newCenterY, _elementStartState.Rotation, newHalfH, newHalfW);

                    newX = unrotatedTopLeft.X;
                    newY = unrotatedTopLeft.Y;
                    newWidth = newHalfW * 2;
                    newHeight = newHalfH * 2;
                }
                break;
        }

        switch (_signMoveDirection)
        {
            case SignDirection.None:
                if (signedDist < 0)
                {
                    _signMoveDirection = SignDirection.Negative;
                }
                else if (signedDist > 0)
                {
                    _signMoveDirection = SignDirection.Positive;
                }
                else if (signedDist == 0)
                {
                    _signMoveDirection = SignDirection.Zero;
                }
                break;
            case SignDirection.Negative:
                if (signedDist >= 0)
                {
                    return;
                }
                break;
            case SignDirection.Positive:
                if (signedDist <= 0)
                {
                    return;
                }
                break;
            case SignDirection.Zero:
                if (signedDist != 0)
                {
                    return;
                }
                break;
        }

        ApplySingleResize(newX, newY, newWidth, newHeight);
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

    private void OnSingleResizeHandleReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isResizing && _selectedElement != null)
        {
            _isResizing = false;
            _activeResizeHandle = ResizeHandle.None;
            ElementResized?.Invoke(this, new ElementResizedEventArgs(new List<CanvasElement>([_selectedElement])));
        }
    }

    /// <summary>
    /// 根据新的中心、角度和高度计算未旋转状态的 TopLeft，并显示绿点
    /// </summary>
    /// <param name="newCenterX">新中心X</param>
    /// <param name="newCenterY">新中心Y</param>
    /// <param name="newRotation">新角度（度）</param>
    /// <param name="newHeight">新高度</param>
    /// <param name="newHalfW">新半宽（可选，默认使用元素宽度）</param>
    /// <returns>未旋转状态的 TopLeft 点</returns>
    private Point CalculateAndShowUnrotatedTopLeft(double newCenterX, double newCenterY, double newRotation, double newHalfH, double? newHalfW = null)
    {
        if (_selectedElement == null) return new Point(0, 0);

        double halfW = newHalfW ?? _selectedElement.Width / 2;
        double halfH = newHalfH;

        // 未旋转状态的 TopLeft（局部坐标）
        // 未旋转时 TopLeft = (centerX - halfW, centerY - halfH)
        double unrotatedTopLeftX = newCenterX - halfW;
        double unrotatedTopLeftY = newCenterY - halfH;

        return new Point(unrotatedTopLeftX, unrotatedTopLeftY);
    }

    private void ApplySingleResize(double newX, double newY, double newWidth, double newHeight)
    {
        if (_selectedElement == null) 
            return;

        _selectedElement.X = newX;
        _selectedElement.Y = newY;
        _selectedElement.Width = newWidth;
        _selectedElement.Height = newHeight;

        if (ElementControlMap?.TryGetValue(_selectedElement, out var control) == true && _renderControl != null)
        {
            if (control is IElementControlBase ecb)
            {
                if (this.Parent == ((Panel?)control.Parent)?.Parent && control.Parent is Canvas)
                {
                    _selectedElement.Left = newX;
                    _selectedElement.Top = newY;
                    ecb.UpdateLeft(newX);
                    ecb.UpdateTop(newY);
                }
                else if (control.Parent is Canvas parentCanvas)
                {
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
        }
    }

    #endregion
}
