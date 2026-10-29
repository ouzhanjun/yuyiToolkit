using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;
using yuyiAvalonia.UI.Services;

namespace yuyiAvalonia.UI.Controls.ElementFlow;

/// <summary>
/// 管理画布元素的操作
/// </summary>
public partial class FlowContainerManager : ObservableObject
{
    #region 字段定义

    private Canvas? _topLevelCanvas;
    private Canvas? _workCanvas;
    private IUIControlCreator? _controlCreator = null;

    private readonly CanvasHelper _canvasHelper = new();

    private RenderControl? _renderControl = null;

    private CanvasHistoryManager? _historyManager = null;
    public CanvasHistoryManager? HistoryManager { get => _historyManager; }

    /// <summary>
    /// 控件创建器（用于预览等场景）
    /// </summary>
    private IUIControlCreator? ControlCreator => _controlCreator;

    [ObservableProperty]
    private ObservableCollection<CanvasElement> _canvasElements;

    [ObservableProperty]
    private CanvasElement? _selectedElement;
    public event EventHandler<CanvasElement?> SelectedElementChanged;

    private ComposeSelectionOverlay? _selectionOverlay;

    #endregion

    #region 构造函数

    public FlowContainerManager(Canvas mainCanvas, 
        Canvas? workCanvas, 
        ObservableCollection<CanvasElement> canvasElements, 
        IUIControlCreator controlCreator, 
        RenderControl renderControl, 
        ComposeSelectionOverlay multiSelectionOverlay, 
        CanvasHistoryManager historyManager)
    {
        _topLevelCanvas = mainCanvas;
        _workCanvas = workCanvas;

        _controlCreator = controlCreator;
        _renderControl = renderControl;
        _historyManager = historyManager;

        _selectionOverlay = multiSelectionOverlay;
        _selectionOverlay.CanvasElements = canvasElements;
        _selectionOverlay.ElementControlMap = ControlCreator!.ElementControlMap;

        _selectionOverlay.SelectionChanged += _selectionOverlay_SelectionChanged;

        CanvasElements = canvasElements;
        CanvasElements.CollectionChanged += CanvasElements_CollectionChanged;
    }

    private void CanvasElements_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Reset:
                {
                    var list = _workCanvas.Children.ToList();
                    foreach (var child in list)
                    {
                        DeleteElement(child.Tag as CanvasElement);
                    }
                    _selectionOverlay!.ClearAllSelection();
                    list.Clear();

                    SelectedElementChanged?.Invoke(this, null);
                }
                break;
        }
    }

    /// <summary>
    /// 当SelectedElement属性触发时回调方法
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    partial void OnSelectedElementChanged(CanvasElement? oldValue, CanvasElement? newValue)
    {
        Task.Run((Action)(() =>
        {
            
            // 切回 UI 线程，批量渲染界面
            Dispatcher.UIThread.Post((Action)(() =>
            {
                // 如果旧的选中元素是容器，隐藏覆盖层
                if (oldValue != null && oldValue.IsContainer && ControlCreator.TryGetControl(oldValue, out var oldControl))
                {
                    if (oldControl is GridControl oldGridControl)
                    {
                        oldGridControl.HideWorkspaceOverlay();
                    }
                    else if (oldControl is CardControl oldCardControl)
                    {
                        oldCardControl.HideWorkspaceOverlay();
                    }
                    else if (oldControl is PanelControl oldPanelControl)
                    {
                        oldPanelControl.HideWorkspaceOverlay();
                    }
                    else if (oldControl is StackPanelControl oldStackControl)
                    {
                        oldStackControl.HideWorkspaceOverlay();
                    }
                    else if (oldControl is CanvasContainerControl oldCanvasControl)
                    {
                        oldCanvasControl.HideWorkspaceOverlay();
                    }
                    else if (oldControl is RoundedRectangleStackPanelControl oldRrectStack)
                    {
                        oldRrectStack.HideWorkspaceOverlay();
                    }
                    else if (oldControl is RoundedRectangleCanvasControl oldRrectCanvas)
                    {
                        oldRrectCanvas.HideWorkspaceOverlay();
                    }
                    else if (oldControl is RoundedRectangleGridControl oldRrectGrid)
                    {
                        oldRrectGrid.HideWorkspaceOverlay();
                    }
                }

                if (newValue != null && ControlCreator.TryGetControl(newValue, out var newControl))
                {
                    // 如果新的选中元素是容器，显示覆盖层
                    if (newValue != null && newValue.IsContainer && newControl != null)
                    {
                        if (newControl is GridControl newGridControl)
                        {
                            newGridControl.ShowWorkspaceOverlay();
                        }
                        else if (newControl is CardControl newCardControl)
                        {
                            newCardControl.ShowWorkspaceOverlay();
                        }
                        else if (newControl is PanelControl newPanelControl)
                        {
                            newPanelControl.ShowWorkspaceOverlay();
                        }
                        else if (newControl is StackPanelControl newStackControl)
                        {
                            newStackControl.ShowWorkspaceOverlay();
                        }
                        else if (newControl is CanvasContainerControl newCanvasControl)
                        {
                            newCanvasControl.ShowWorkspaceOverlay();
                        }
                        else if (newControl is RoundedRectangleStackPanelControl newRrectStack)
                        {
                            newRrectStack.ShowWorkspaceOverlay();
                        }
                        else if (newControl is RoundedRectangleCanvasControl newRrectCanvas)
                        {
                            newRrectCanvas.ShowWorkspaceOverlay();
                        }
                        else if (newControl is RoundedRectangleGridControl newRrectGrid)
                        {
                            newRrectGrid.ShowWorkspaceOverlay();
                        }
                    }

                    // 如果是 Shape 控件（三角形、矩形、椭圆、平行四边形），显示角点覆盖层
                    if (newControl is ShapeFlowControlBase shapeControl)
                    {
                        shapeControl.ShowWorkspaceOverlay();
                    }
                }

                // 如果旧的选中元素是 Shape 控件，隐藏角点覆盖层
                if (oldValue != null && !oldValue.IsContainer &&
                    ControlCreator.TryGetControl(oldValue, out var oldControl2))
                {
                    if (oldControl2 is ShapeFlowControlBase oldShapeControl)
                    {
                        oldShapeControl.HideWorkspaceOverlay();
                    }
                }

            }));
        }));
    }

    #endregion

    #region 外部调用方法

    public void MainCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this._workCanvas);

        // 如果右键按下且有多选，尝试多选移动
        if (_selectionOverlay?.IsSelectionActive == true)
        {
            // 检查是否在选中框内右键点击
            if (point.Properties.IsRightButtonPressed && _selectionOverlay.IsPointInSelectionRect(point.Position))
            {
                _selectionOverlay.ControlsMoveStart(point.Position, this._workCanvas!);
                SetSelectedElement();
                e.Handled = true;
                return;
            }
        }

        var clickedElement = FindClickedElementAtPosition(point.Position);

        if (clickedElement != null && ControlCreator!.TryGetControl(clickedElement, out var control))
        {
            bool isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            if (!isCtrlPressed)
            {
                // 如果点击的元素不在当前多选中，则清除多选并添加该元素到多选中
                _selectionOverlay!.AddSingleSelectedElement(clickedElement);
                SetSelectedElement();

                this.HistoryManager!.RecordSelection(_selectionOverlay.SelectedElements.Select(x => x.Id).ToArray());

                if (point.Properties.IsRightButtonPressed && _selectionOverlay.IsPointInSelectionRect(point.Position))
                {
                    _selectionOverlay.ControlsMoveStart(point.Position, this._workCanvas!);
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                _selectionOverlay!.SwitchSelectedElement(clickedElement);

                this.HistoryManager!.RecordSelection(_selectionOverlay.SelectedElements.Select(x => x.Id).ToArray());

                SetSelectedElement();
                e.Handled = true;
                return;
            }
        }
        else
        {
            _selectionOverlay!.ClearAllSelection();

            // 将事件转发给多选覆盖层处理框选
            _selectionOverlay?.SelectionStart(e);
            SetSelectedElement();
        }
    }

    #endregion

    #region 选中框逻辑

    private void _selectionOverlay_SelectionChanged(object? sender, IReadOnlyList<CanvasElement> selectedElements)
    {
        if (selectedElements.Count == 1 && SelectedElement != selectedElements[0])
        {
            SetSelectedElement();
        }
    }

    #endregion

    #region 工具箱组件拖拽

    // 组件拖拽状态
    private ComponentDefinition? _draggingComponent;
    private Control? _dragPreviewIcon;
    private bool _isDraggingFromComponent;

    public ComponentDefinition? ComponentDefinition
    {
        get => _draggingComponent;
    }

    public void CreateDraggingFromComponentPanel(ComponentDefinition definition, PointerPressedEventArgs e)
    {
        ClearDragPreviewIcon();

        Point position = e.GetPosition(this._topLevelCanvas);
        _draggingComponent = definition;
        _isDraggingFromComponent = true;

        var childControl = CreateElementControl(definition, out CanvasElement? element);
        if (childControl != null)
        {
            _dragPreviewIcon = childControl;
            _dragPreviewIcon.Width = childControl.Width;
            _dragPreviewIcon.Height = childControl.Height;

            if (this._topLevelCanvas != null)
            {
                this._topLevelCanvas.Children.Add(_dragPreviewIcon);
                MoveDraggingFromComponentPanel(position);
            }
        }
    }

    public void MoveDraggingFromComponentPanel(Point position)
    {
        if (_dragPreviewIcon != null)
        {
            Canvas.SetLeft(_dragPreviewIcon, position.X - _dragPreviewIcon.Width / 2);
            Canvas.SetTop(_dragPreviewIcon, position.Y - _dragPreviewIcon.Height / 2);
        }
    }

    public bool IsDraggingFromComponentPanel()
    {
        return _isDraggingFromComponent;
    }

    private void ClearDragPreviewIcon()
    {
        if (_dragPreviewIcon != null)
        {
            var workCanvas = this._topLevelCanvas;

            if (workCanvas != null && workCanvas.Children.Contains(_dragPreviewIcon))
            {
                _dragPreviewIcon.Tag = null;
                workCanvas.Children.Remove(_dragPreviewIcon);
            }

            _dragPreviewIcon = null;
        }
    }

    public void MoveDraggingComponentToWorkCanvas(PointerEventArgs e)
    {
        if (_dragPreviewIcon == null)
        {
            return;
        }

        var canvasPosition = e.GetPosition(this._workCanvas);

        double x = canvasPosition.X;
        double y = canvasPosition.Y;

        if (_dragPreviewIcon != null && _dragPreviewIcon is Control && _dragPreviewIcon.Tag is CanvasElement element)
        {
            _selectionOverlay!.ClearAllSelection();

            if (_dragPreviewIcon.Parent != null && _dragPreviewIcon.Parent is Canvas parentCanvas)
            {
                parentCanvas.Children.Remove(_dragPreviewIcon);
            }

            element.X = x - _dragPreviewIcon.Width / 2;
            element.Y = y - _dragPreviewIcon.Height / 2;

            if (!this.CanvasElements.Contains(element))
            {
                this.CanvasElements.Add(element);
            }

            this._workCanvas!.Children.Add(_dragPreviewIcon);

            _selectionOverlay!.AddSingleSelectedElement(element);
            _selectionOverlay.ControlsMoveStart(canvasPosition, this._workCanvas!);
            _selectionOverlay.ControlsMoveTo(canvasPosition, this._workCanvas!, false);

            // 超出画布范围，取消添加
            _isDraggingFromComponent = false;
            _draggingComponent = null;
            _dragPreviewIcon = null;
        }

        this.ClearAllStatus();
    }

    public void EndDraggingFromComponentPanel(PointerReleasedEventArgs e)
    {
        var canvasPosition = e.GetPosition(this._workCanvas);

        if (canvasPosition.X < 0 || canvasPosition.Y < 0 || canvasPosition.X > _workCanvas.Width || canvasPosition.Y > _workCanvas.Height)
        {
            // 超出画布范围，取消添加
            _isDraggingFromComponent = false;
            _draggingComponent = null;
            ClearDragPreviewIcon();
            return;
        }

        double x = canvasPosition.X;
        double y = canvasPosition.Y;

        CanvasElement? element = null;

        if (_dragPreviewIcon == null)
        {
            return;
        }

        if (_dragPreviewIcon != null && _dragPreviewIcon.Tag is CanvasElement)
        {
            element = (CanvasElement)_dragPreviewIcon.Tag;
            element.X = x - _dragPreviewIcon.Width / 2;
            element.Y = y - _dragPreviewIcon.Height / 2;

            // 从左侧工具箱拖动组件时，释放鼠标的坐标是相对于画布的，需要根据坐标找到对应的元素，作为新元素的父级。
            var parentElement = FindContainerElementAtPosition(canvasPosition, this.CanvasElements, element);

            Panel? parentPanel = this._workCanvas;
            Grid? gridPanel = null;
            int? gridRow = null;
            int? gridColumn = null;
            GridControl? gridControl = null;

            if (parentElement != null && parentElement.IsContainer && ControlCreator.TryGetControl(parentElement, out var border))
            {
                if (border is CardControl cardControl)
                {
                    parentPanel = cardControl.Control;
                }
                else if (border is PanelControl panelControl)
                {
                    parentPanel = panelControl.Control;
                }
                else if (border is StackPanelControl stackControl)
                {
                    parentPanel = stackControl.Control;
                }
                else if (border is GridControl gc)
                {
                    parentPanel = gc.Control;
                    gridPanel = gc.Control;
                    gridControl = gc;

                    // 计算停靠到哪个单元格（需要先转换为 Grid 内部坐标）
                    var localPosition = gc.GetLocalPosition(canvasPosition);
                    var cellInfo = gc.GetCellAtPosition(localPosition);
                    if (cellInfo.HasValue)
                    {
                        gridRow = cellInfo.Value.Row;
                        gridColumn = cellInfo.Value.Column;
                    }
                }
                else if (border is CanvasContainerControl canvasControl)
                {
                    // Canvas 容器支持绝对定位
                    parentPanel = canvasControl.Control;
                }
                else if (border is RoundedRectangleStackPanelControl rrectStackControl)
                {
                    // 圆角矩形 + StackPanel 容器：子元素按 Orientation/HSpacing 排列
                    parentPanel = rrectStackControl.Container;
                }
                else if (border is RoundedRectangleCanvasControl rrectCanvasControl)
                {
                    // 圆角矩形 + Canvas 容器：子元素按自身 Left/Top 绝对定位
                    parentPanel = rrectCanvasControl.Container;
                }
                else if (border is RoundedRectangleGridControl rrectGridControl)
                {
                    // 圆角矩形 + Grid 容器：子元素按 Row/Column 落入对应单元格
                    parentPanel = rrectGridControl.Container;
                    gridPanel = rrectGridControl.Container as Grid;
                    gridControl = null; // RoundedRectangleGridControl 是 Canvas 类型，复用 GridControl 反而类型不匹配

                    if (gridPanel != null)
                    {
                        // 计算停靠到哪个单元格（按内部 Grid 坐标 + Padding）
                        var localPosition = rrectGridControl.GetLocalPosition(canvasPosition);
                        var cellInfo = rrectGridControl.GetCellAtPosition(localPosition);
                        if (cellInfo.HasValue)
                        {
                            gridRow = cellInfo.Value.Row;
                            gridColumn = cellInfo.Value.Column;
                        }
                    }
                }
            }

            if (parentPanel == null)
            {
                return;
            }

            int? index = FindIndexInContainer(canvasPosition, parentElement);

            // 从组件预览图创建的元素，直接添加到目标容器中，不应该存在 element==parentElement 的情况
            AddElementToNewContainer(_dragPreviewIcon, element, parentPanel, parentElement, index, gridRow, gridColumn);
        }

        _selectionOverlay!.AddSingleSelectedElement(element);

        SetSelectedElement();

        //历史记录
        _historyManager.RecordElements(this.CanvasElements, this._selectionOverlay.SelectedElements.Select(x => x.Id).ToArray());

        _isDraggingFromComponent = false;
        _draggingComponent = null;
        ClearDragPreviewIcon();

        if (hoverContainerControl != null && hoverContainerControl.Tag != this.SelectedElement)
        {
            if (hoverContainerControl is GridControl oldGrid)
            {
                oldGrid.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is CardControl oldCard)
            {
                oldCard.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is PanelControl oldPanel)
            {
                oldPanel.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is StackPanelControl oldStack)
            {
                oldStack.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is CanvasContainerControl oldCanvas)
            {
                oldCanvas.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is RoundedRectangleStackPanelControl oldRrectStack2)
            {
                oldRrectStack2.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is RoundedRectangleCanvasControl oldRrectCanvas2)
            {
                oldRrectCanvas2.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is RoundedRectangleGridControl oldRrectGrid2)
            {
                oldRrectGrid2.HideWorkspaceOverlay();
            }
            hoverContainerControl = null;
            hoverGridControl = null;
        }
    }

    #endregion

    #region 移动元素

    GridControl? hoverGridControl = null;
    Control? hoverContainerControl = null;

    public void HideContainerControlOverlay()
    {
        if (hoverContainerControl != null && hoverContainerControl.Tag != this.SelectedElement)
        {
            if (hoverContainerControl is GridControl oldGrid)
            {
                oldGrid.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is CardControl oldCard)
            {
                oldCard.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is PanelControl oldPanel)
            {
                oldPanel.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is StackPanelControl oldStack)
            {
                oldStack.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is CanvasContainerControl oldCanvas)
            {
                oldCanvas.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is RoundedRectangleStackPanelControl oldRrectStack3)
            {
                oldRrectStack3.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is RoundedRectangleCanvasControl oldRrectCanvas3)
            {
                oldRrectCanvas3.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is RoundedRectangleGridControl oldRrectGrid3)
            {
                oldRrectGrid3.HideWorkspaceOverlay();
            }

            hoverContainerControl = null;
            hoverGridControl = null;
        }
    }

    /// <summary>
    /// 更新容器覆盖层显示（仅在移动/拖拽时用于显示悬停容器的覆盖层）
    /// </summary>
    public void UpdateContainerControlOverlay(Point currentPoint)
    {
        // 隐藏之前悬停的容器覆盖层
        if (hoverContainerControl != null)
        {
            if (hoverContainerControl is GridControl oldGrid)
            {
                oldGrid.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is CardControl cardCtrl)
            {
                cardCtrl.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is PanelControl panelCtrl)
            {
                panelCtrl.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is StackPanelControl stackCtrl)
            {
                stackCtrl.HideWorkspaceOverlay();
            }
            else if (hoverContainerControl is CanvasContainerControl canvasCtrl)
            {
                canvasCtrl.HideWorkspaceOverlay();
            }
            hoverContainerControl = null;
            hoverGridControl = null;
        }

        // 检测当前悬停的容器（排除选中的元素）
        var hoverElement = FindContainerElementAtPosition(currentPoint, this.CanvasElements, this.SelectedElement);
        if (hoverElement != null && hoverElement != this.SelectedElement &&
            hoverElement.IsContainer && ControlCreator.TryGetControl(hoverElement, out var hoverControl))
        {
            if (hoverControl is GridControl gridControl)
            {
                gridControl.ShowWorkspaceOverlay();
                hoverGridControl = gridControl;
                hoverContainerControl = gridControl;
            }
            else if (hoverControl is CardControl cardCtrl)
            {
                cardCtrl.ShowWorkspaceOverlay();
                hoverContainerControl = cardCtrl;
            }
            else if (hoverControl is PanelControl panelCtrl)
            {
                panelCtrl.ShowWorkspaceOverlay();
                hoverContainerControl = panelCtrl;
            }
            else if (hoverControl is StackPanelControl stackCtrl)
            {
                stackCtrl.ShowWorkspaceOverlay();
                hoverContainerControl = stackCtrl;
            }
            else if (hoverControl is CanvasContainerControl canvasCtrl)
            {
                canvasCtrl.ShowWorkspaceOverlay();
                hoverContainerControl = canvasCtrl;
            }
            else if (hoverControl is RoundedRectangleStackPanelControl rrectStackHover)
            {
                rrectStackHover.ShowWorkspaceOverlay();
                hoverContainerControl = rrectStackHover;
            }
            else if (hoverControl is RoundedRectangleCanvasControl rrectCanvasHover)
            {
                rrectCanvasHover.ShowWorkspaceOverlay();
                hoverContainerControl = rrectCanvasHover;
            }
            else if (hoverControl is RoundedRectangleGridControl rrectGridHover)
            {
                rrectGridHover.ShowWorkspaceOverlay();
                hoverContainerControl = rrectGridHover;
            }
        }
    }

    public void MultiSelectionMovedEnd(PointerReleasedEventArgs e)
    { 
        if (_selectionOverlay!.IsMultiMoving)
        {
            var canvasPosition = e.GetPosition(this._workCanvas);

            var selectedElements = _selectionOverlay?.SelectedElements;
            if (selectedElements == null) 
                return;

            var parentElement = FindContainerElementAtPosition(canvasPosition, this.CanvasElements, selectedElements.ToArray());

            Panel? parentPanel = _workCanvas;
            Grid? gridPanel = null;
            int? gridRow = null;
            int? gridColumn = null;
            GridControl? gridControl = null;

            if (parentElement != null && parentElement.IsContainer && ControlCreator.TryGetControl(parentElement, out var border))
            {
                if (border is CardControl cardControl)
                {
                    parentPanel = cardControl.Control;
                }
                else if (border is PanelControl panelControl)
                {
                    parentPanel = panelControl.Control;
                }
                else if (border is StackPanelControl stackControl)
                {
                    parentPanel = stackControl.Control;
                }
                else if (border is GridControl gc)
                {
                    parentPanel = gc.Control;
                    gridPanel = gc.Control;
                    gridControl = gc;

                    // 计算停靠到哪个单元格（需要先转换为 Grid 内部坐标）
                    var localPosition = gc.GetLocalPosition(canvasPosition);
                    var cellInfo = gc.GetCellAtPosition(localPosition);
                    if (cellInfo.HasValue)
                    {
                        gridRow = cellInfo.Value.Row;
                        gridColumn = cellInfo.Value.Column;
                    }
                }
                else if (border is CanvasContainerControl canvasControl)
                {
                    // Canvas 容器支持绝对定位
                    parentPanel = canvasControl.Control;
                }
                else if (border is RoundedRectangleStackPanelControl rrectStackControl)
                {
                    // 圆角矩形 + StackPanel 容器：子元素按 Orientation/HSpacing 排列
                    parentPanel = rrectStackControl.Container;
                }
                else if (border is RoundedRectangleCanvasControl rrectCanvasControl)
                {
                    // 圆角矩形 + Canvas 容器：子元素按自身 Left/Top 绝对定位
                    parentPanel = rrectCanvasControl.Container;
                }
                else if (border is RoundedRectangleGridControl rrectGridControl)
                {
                    // 圆角矩形 + Grid 容器：子元素按 Row/Column 落入对应单元格
                    parentPanel = rrectGridControl.Container;
                    gridPanel = rrectGridControl.Container as Grid;
                    gridControl = null; // RoundedRectangleGridControl 是 Canvas 类型，复用 GridControl 反而类型不匹配

                    if (gridPanel != null)
                    {
                        // 计算停靠到哪个单元格（按内部 Grid 坐标 + Padding）
                        var localPosition = rrectGridControl.GetLocalPosition(canvasPosition);
                        var cellInfo = rrectGridControl.GetCellAtPosition(localPosition);
                        if (cellInfo.HasValue)
                        {
                            gridRow = cellInfo.Value.Row;
                            gridColumn = cellInfo.Value.Column;
                        }
                    }
                }
            }

            foreach (var element in selectedElements)
            {
                if  (ControlCreator.TryGetControl(element, out var control))
                {
                    if (element != null && element != parentElement)
                    {
                        int? index = FindIndexInContainer(canvasPosition, parentElement);

                        AddElementToNewContainer(control, element, parentPanel, parentElement, index, gridRow, gridColumn);
                    }
                    else if (gridRow.HasValue && gridColumn.HasValue && gridControl != null)
                    {
                        gridControl.ShowWorkspaceOverlay();
                        // 如果释放在 Grid 内但 parentElement 是自己，说明是从 Grid 的一个单元格移动到另一个
                        // 将控件添加到目标单元格
                        //gridControl.AddChildToCell(control, gridRow.Value, gridColumn.Value);
                        if (control != null && control is IElementControlBase)
                        {
                            gridControl.AddChildToCell((BorderControlBase)control, gridRow.Value, gridColumn.Value);
                        }
                    }
                    else if (gridRow.HasValue && gridColumn.HasValue && parentElement is RoundedRectangleGridElement)
                    {
                        // 圆角矩形 + Grid 容器：在自己内部的不同单元格之间移动
                        if (ControlCreator.TryGetControl(parentElement, out var parentContainer))
                        {
                            if (parentContainer is RoundedRectangleGridControl rrectGridMove)
                            {
                                rrectGridMove.ShowWorkspaceOverlay();
                                rrectGridMove.AddChildToCell(control, gridRow.Value, gridColumn.Value);
                            }
                        }
                    }
                }
            }

            //历史记录
            _historyManager.RecordElements(this.CanvasElements, this._selectionOverlay!.SelectedElements.Select(x => x.Id).ToArray(),HistoryActionType.ElementMoved);

            SetSelectedElement();

            parentPanel?.UpdateLayout();
            _selectionOverlay?.RefreshSelectionOverlay();

        }

        // 结束多选移动
        if (_selectionOverlay!.IsMultiMoving)
        {
            _selectionOverlay.EndMultiMove();
        }
    }

    /// <summary>
    /// 获取元素相对于 ElementsCanvas 的实际渲染位置, 递归累加所有父容器的偏移
    /// </summary>
    public (double X, double Y) GetElementActualPosition(CanvasElement element)
    {
        if (!ControlCreator.TryGetControl(element, out var control))
            return (element.X, element.Y);

        // 获取控件在父容器中的渲染位置
        double x = control.Bounds.X;
        double y = control.Bounds.Y;

        var position = control.TranslatePoint(new Point(0, 0), this._workCanvas!);

        return (position.Value.X, position.Value.Y);
    }

    #endregion

    #region 选择元素

    public CanvasElement? FindClickedElementAtPosition(Point position)
    {
        return FindClickedElementAtPosition(position, this.CanvasElements);
    }

    private CanvasElement? FindClickedElementAtPosition(Point position, ObservableCollection<CanvasElement>? canvasElements)
    {
        if (canvasElements == null)
            return null;

        var orderList = canvasElements.OrderBy(e => e.ZIndex).ToList();

        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            var element = orderList[i];
            if (element.IsContainer && element.Children.Count > 0)
            {
                var childCanvas = FindClickedElementAtPosition(position, element.Children);
                if (childCanvas != null)
                {
                    return childCanvas;
                }
            }

            // 先进行快速边界框检测（考虑旋转）
            var bounds = GetRotatedBounds(element);
            if (position.X >= bounds.MinX - 2 && position.X <= bounds.MaxX + 4 && position.Y >= bounds.MinY - 2 && position.Y <= bounds.MaxY + 4)
            {
                // 进行精确命中测试
                if (CheckIsPointInElement(position, element))
                {
                    return element;
                }
            }

        }

        return null;
    }

    /// <summary>
    /// 从 ElementControlMap 获取元素的实际控件，并用 TranslatePoint 获取其在 _workCanvas 坐标系中的视觉左上角
    /// </summary>
    private (double X, double Y)? GetElementTopLeftInCanvas(CanvasElement element)
    {
        if (_workCanvas == null || !ControlCreator.ElementControlMap.TryGetValue(element, out var control) || control == null)
            return null;

        var topLeft = control.TranslatePoint(new Point(0, 0), _workCanvas);
        return topLeft.HasValue ? (topLeft.Value.X, topLeft.Value.Y) : null;
    }

    /// <summary>
    /// 从控件父链向上遍历，累加所有 CanvasElement 祖先的旋转角 + 自身旋转角
    /// </summary>
    private double GetTotalRotationFromControlChain(Control control)
    {
        double total = 0;
        var parent = control.Parent as Control;
        while (parent != null && parent != _workCanvas)
        {
            if (parent is BorderControlBase parentEcb && parentEcb.Tag is CanvasElement parentElem)
                total += parentElem.Rotation;
            parent = parent.Parent as Control;
        }
        // 累加元素自身的旋转角
        if (control is IElementControlBase ecb && control.Tag is CanvasElement selfElem)
            total += selfElem.Rotation;
        return total;
    }

    /// <summary>
    /// 计算元素在画布中的旋转后边界框（以左上角为旋转中心）
    /// </summary>
    private (double MinX, double MinY, double MaxX, double MaxY) GetRotatedBounds(CanvasElement element)
    {
        var topLeft = GetElementTopLeftInCanvas(element);
        if (topLeft == null)
        {
            return (element.X, element.Y, element.X + element.Width, element.Y + element.Height);
        }

        double originX = topLeft.Value.X;
        double originY = topLeft.Value.Y;
        double width = element.Width;
        double height = element.Height;

        if (!ControlCreator!.ElementControlMap.TryGetValue(element, out var control) || control == null)
            return (originX, originY, originX + width, originY + height);

        double totalRotation = GetTotalRotationFromControlChain(control);

        if (Math.Abs(totalRotation) < 0.01)
            return (originX, originY, originX + width, originY + height);

        double rad = totalRotation * Math.PI / 180;
        double cos = Math.Cos(rad), sin = Math.Sin(rad);

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var (ox, oy) in new[] { (0.0, 0.0), (width, 0.0), (width, height), (0.0, height) })
        {
            double rx = originX + ox * cos - oy * sin;
            double ry = originY + ox * sin + oy * cos;
            minX = Math.Min(minX, rx); minY = Math.Min(minY, ry);
            maxX = Math.Max(maxX, rx); maxY = Math.Max(maxY, ry);
        }

        return (minX, minY, maxX, maxY);
    }

    public void AddElementToNewContainer(Control childControl, CanvasElement element, Panel parentPanel, CanvasElement? parentElement, int? insertedIndex, int? gridRow = null, int? gridColumn = null)
    {
        if (childControl == null)
        {
            return;
        }

        Debug.Assert(!(element == parentElement));

        // 只有当父容器是 Canvas 时才强制设置 Left/Top 对齐
        // 其他容器（StackPanel、Panel 等）应保持 Stretch 对齐
        if (parentPanel is Canvas)
        {
            if (childControl.HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                childControl.HorizontalAlignment = HorizontalAlignment.Left;
            }
            if (childControl.VerticalAlignment == VerticalAlignment.Stretch)
            {
                childControl.VerticalAlignment = VerticalAlignment.Top;
            }
        }
        else
        {
            // 非 Canvas 容器处理
            // Shape 控件（Path）在 Stretch 对齐下会塌陷为 0 宽高，需要特殊处理
            if (childControl is yuyiAvalonia.UI.ElementControls.Controls.ShapeFlowControlBase shapeControl)
            {
                // Shape 控件在非 Canvas 容器中使用 Left 对齐，保持其原始位置
                if (childControl.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    childControl.HorizontalAlignment = HorizontalAlignment.Left;
                }
                if (childControl.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    childControl.VerticalAlignment = VerticalAlignment.Top;
                }
            }
        }

        // 检查父容器是否是 Grid
        if (parentPanel is Grid grid && gridRow.HasValue && gridColumn.HasValue)
        {
            if (grid.Parent!.Parent is GridControl gridControl)
            {
                if (childControl != null && childControl is BorderControlBase)
                {
                    gridControl.AddChildToCell((BorderControlBase)childControl, gridRow.Value, gridColumn.Value);
                }
            }
            else if (grid.Parent is Panel contentPanel
                     && contentPanel.Parent is Border innerBorder
                     && innerBorder.Parent is Canvas canvasBackground
                     && canvasBackground.Parent is RoundedRectangleGridControl rrectGridCtrl)
            {
                // 圆角矩形 + Grid 容器：直接通过 RoundedRectangleGridControl.AddChildToCell，
                // 它内部处理 Element.Parent / Grid.SetRow / Grid.SetColumn / Stretch 对齐等。
                rrectGridCtrl.AddChildToCell(childControl, gridRow.Value, gridColumn.Value);
            }
            else
            {
                if (!grid.Children.Contains(childControl))
                {
                    ((Panel?)childControl.Parent)?.Children.Remove(childControl);
                    grid.Children.Add(childControl);
                }
                // 将控件添加到 Grid 的指定单元格
                Grid.SetRow(childControl, gridRow.Value);
                Grid.SetColumn(childControl, gridColumn.Value);
                element.GridRow = gridRow.Value;
                element.GridColumn = gridColumn.Value;
            }
        }
        else
        {
            // 非 Grid 容器的处理
            if (!parentPanel.Children.Contains(childControl))
            {
                IElementContainer? parentContainer = parentPanel.Parent!.Parent as IElementContainer;

                ((Panel?)childControl.Parent)?.Children.Remove(childControl);

                if (parentContainer != null)
                {
                    if (insertedIndex.HasValue && insertedIndex.Value > -1)
                    {
                        if (childControl != null && childControl is IElementControlBase)
                        {
                            parentContainer.Insert(insertedIndex.Value, childControl);
                        }
                    }
                    else if (parentPanel != null && childControl is IElementControlBase && !parentPanel.Children.Contains(childControl!))
                    {
                        parentContainer.Add(childControl);
                    }
                }

                if (parentElement != null && parentElement is RoundedRectangleStackPanelElement)
                {
                    if (childControl != null)
                    {
                        parentPanel.Children.Add(childControl);
                    }
                }
                else if (parentElement != null && parentElement is RoundedRectangleCanvasElement)
                {
                    if (childControl != null)
                    {
                        parentPanel.Children.Add(childControl);
                    }
                }
                else if (parentElement != null && parentElement is RoundedRectangleGridElement)
                {
                    if (childControl != null)
                    {
                        parentPanel.Children.Add(childControl);
                    }
                }
            }


            // 如果父容器是 Canvas，设置 Left/Top 位置
            if (childControl != null && childControl.Parent != null && childControl.Parent is Canvas parentCanvas)
            {
                var parentPos = parentCanvas.TranslatePoint(new Point(0, 0), this._workCanvas!);
                var posX = element.X - parentPos!.Value.X;
                var posY = element.Y - parentPos.Value.Y;
                Canvas.SetLeft(childControl, posX);
                Canvas.SetTop(childControl, posY);

                element.Left = posX;
                element.Top = posY;
            }
        }

        if (ControlCreator.ElementControlMap.ContainsKey(element) == false)
        {
            ControlCreator.ElementControlMap[element] = childControl;
        }

        ObservableCollection<CanvasElement> canvasElements = null;

        if (parentElement != null)
        {
            canvasElements = parentElement.Children;
        }
        else
        {
            canvasElements = this.CanvasElements;
        }

        if (!canvasElements.Contains(element))
        {
            if (element.Parent == null)
            {
                if (this.CanvasElements.Contains(element))
                {
                    this.CanvasElements.Remove(element);
                }
            }

            element.Parent = parentElement;
            canvasElements.Add(element);
        }

        parentPanel!.UpdateLayout();
        _selectionOverlay!.RefreshSelectionOverlay();
    }

    private CanvasElement? FindContainerElementAtPosition(Point position, ObservableCollection<CanvasElement>? canvasElements, CanvasElement findElement)
    {
        if (canvasElements == null)
            return null;

        var orderList = canvasElements.OrderBy(e => e.ZIndex).ToList();

        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            var containerElement = orderList[i];

            // 使用精确命中测试（支持 Shape 图形）
            if (CheckIsPointInElement(position, containerElement))
            {
                if (containerElement.IsContainer && findElement != containerElement)
                {
                    if (containerElement.Children.Count > 0)
                    {
                        var childContainerElement = FindContainerElementAtPosition(position, containerElement.Children, findElement);

                        if (childContainerElement != null && containerElement.IsContainer)
                        {
                            return childContainerElement;
                        }
                    }

                    return containerElement;
                }

            }
        }

        return null;
    }

    private CanvasElement? FindContainerElementAtPosition(Point position, ObservableCollection<CanvasElement>? canvasElements, CanvasElement[] findElements)
    {
        if (canvasElements == null)
            return null;

        var orderList = canvasElements.OrderBy(e => e.ZIndex).ToList();

        for (int i = orderList.Count - 1; i >= 0; i--)
        {
            var containerElement = orderList[i];

            // 使用精确命中测试（支持 Shape 图形）
            if (CheckIsPointInElement(position, containerElement))
            {
                if (containerElement.IsContainer && !findElements.Contains(containerElement))
                {
                    if (containerElement.Children.Count > 0)
                    {
                        var childContainerElement = FindContainerElementAtPosition(position, containerElement.Children, findElements);

                        if (childContainerElement != null && containerElement.IsContainer)
                        {
                            if (CheckIsParentElement(childContainerElement, findElements))
                            {
                                return null;
                            }
                            return childContainerElement;
                        }
                    }

                    if (CheckIsParentElement(containerElement, findElements))
                    {
                        return null;
                    }
                    return containerElement;
                }

            }
        }

        return null;
    }

    /// <summary>
    /// 用于查找容器的类型: Grid StackPanel WrapPanel
    /// </summary>
    /// <param name="position"></param>
    /// <param name="container"></param>
    /// <returns></returns>
    private int? FindIndexInContainer(Point position, CanvasElement container)
    {
        if (container == null)
            return -1;


        if (ControlCreator.TryGetControl(container, out Control? control) && control != null)
        {
            if (control is BorderControlBase borderControl)
            {
                Panel panel = borderControl.Control as Panel;
                if (panel != null)
                {
                    for (int i = 0; i < panel.Children.Count; i++)
                    {
                        if (panel.Children[i].Tag != null && panel.Children[i].Tag is CanvasElement childElement)
                        {
                            Rect rect = new Rect(childElement.X, childElement.Y, childElement.Width, childElement.Height);
                            if (rect.Contains(position))
                            {
                                return i;
                            }
                        }
                    }
                }
            }
            // 检查是否是 ShapeFlowControlBase（也是容器）
            else if (control is ShapeFlowControlBase shapeContainer)
            {
                for (int i = 0; i < shapeContainer.Children.Count; i++)
                {
                    if (shapeContainer.Children[i].Tag != null && shapeContainer.Children[i].Tag is CanvasElement childElement)
                    {
                        Rect rect = new Rect(childElement.X, childElement.Y, childElement.Width, childElement.Height);
                        if (rect.Contains(position))
                        {
                            return i;
                        }
                    }
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// 检测点是否在元素内
    /// 根据元素类型选择精确的命中测试方法
    /// </summary>
    private bool CheckIsPointInElement(Point point, CanvasElement element)
    {
        // 如果是 Shape 类型的元素，使用图形几何进行精确命中测试
        if (element is ShapeElement shapeElement)
        {
            return CheckIsPointInShape(point, shapeElement);
        }

        // 其他元素使用矩形命中测试
        return CheckPointInRect(point, element);
    }

    /// <summary>
    /// 检测点是否在 Shape 图形内（使用 PathGeometry 进行精确命中测试）
    /// </summary>
    private bool CheckIsPointInShape(Point point, ShapeElement shapeElement)
    {
        if (!_controlCreator.ElementControlMap.TryGetValue(shapeElement, out var control) || control == null)
            return CheckPointInRect(point, shapeElement);

        if (control is Avalonia.Controls.Shapes.Path pathControl && pathControl.Data != null)
        {
            // 获取 Path 中心点在 Canvas 中的位置
            var pathCenter = new Point(pathControl.Bounds.Width / 2, pathControl.Bounds.Height / 2);
            var centerInCanvas = pathControl.TranslatePoint(pathCenter, _workCanvas);
            if (centerInCanvas == null)
                return CheckPointInRect(point, shapeElement);

            // 计算点击点相对于 Path 中心的偏移
            double localX = point.X - centerInCanvas.Value.X;
            double localY = point.Y - centerInCanvas.Value.Y;

            // 如果有旋转，应用旋转变换的逆变换
            double totalRotation = GetTotalRotationFromControlChain(control);
            if (Math.Abs(totalRotation) > 0.01)
            {
                var angle = -totalRotation * Math.PI / 180;
                var cos = Math.Cos(angle);
                var sin = Math.Sin(angle);
                var rx = localX * cos - localY * sin;
                var ry = localX * sin + localY * cos;
                localX = rx;
                localY = ry;
            }

            // 转换到 Path 局部坐标（相对于 Bounds 左上角）
            var localPoint = new Point(localX + pathControl.Bounds.Width / 2, localY + pathControl.Bounds.Height / 2);

            // 使用 Geometry.FillContains 检测点是否在几何图形填充区域内
            if (pathControl.Data.FillContains(localPoint))
                return true;

            // 对于有边框的图形，检查边框
            if (pathControl.StrokeThickness > 0 && pathControl.Stroke != null)
            {
                var pen = new Pen(pathControl.Stroke, pathControl.StrokeThickness);
                if (pathControl.Data.StrokeContains(pen, localPoint))
                    return true;
            }

            return false;
        }

        // 回退到矩形命中测试
        return CheckPointInRect(point, shapeElement);
    }

    /// <summary>
    /// 通过 CanvasHelper 进行矩形命中测试
    /// </summary>
    private bool CheckPointInRect(Point point, CanvasElement element)
    {
        if (_workCanvas == null || element == null)
            return false;
        if (!ControlCreator.ElementControlMap.TryGetValue(element, out var control) || control == null)
            return false;
        return _canvasHelper.CheckPointInRotatedRect(point, control, _workCanvas);
    }

    public bool CheckIsParentElement(CanvasElement checkedElement, CanvasElement[] findElements)
    {
        if (findElements == null || findElements.Length == 0)
        {
            return false;
        }

        if (findElements.Contains(checkedElement.Parent))
        {
            return true;
        }

        bool isAvail = false;
        var parent = checkedElement.Parent;

        if (parent != null)
        {
            isAvail = CheckIsParentElement(parent, findElements);
        }

        return isAvail;
    }

    #endregion

    #region 元素操作

    public void UpdateElementControl(CanvasElement element, string? propertyName)
    {
        if (!ControlCreator.TryGetControl(element, out var control))
            return;

        // 处理普通控件（继承自 ElementControlBase / Border）
        if (control is not IElementControlBase contentBorder)
            return;

        Task.Run(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                switch (propertyName)
                {
                    case nameof(CanvasElement.X):
                    case nameof(CanvasElement.Y):
                        if (this._workCanvas != control.Parent)
                        {
                            return;
                        }
                        break;
                }

                _renderControl!.UpdateElementControl(element, control, propertyName);

                switch (propertyName )
                {
                    case nameof(CanvasElement.Width):
                    case nameof(CanvasElement.Height):
                    case nameof(CanvasElement.X):
                    case nameof(CanvasElement.Y):
                    case nameof(CanvasElement.Top):
                    case nameof(CanvasElement.Right):
                    case nameof(CanvasElement.Bottom):
                    case nameof(CanvasElement.Left):
                    case nameof(CanvasElement.Rotation):
                        if (!_selectionOverlay!.IsRotating)
                    {
                        this._workCanvas!.UpdateLayout();
                        _selectionOverlay.RefreshSelectionOverlay();
                    }
                        break;
                    case nameof(CanvasElement.Margin):
                        _selectionOverlay!.RefreshSelectionOverlay();
                        break;
                    case nameof(CanvasElement.IsVisible):
                        _selectionOverlay!.RemoveSelectedElement(element);
                        break;
                }
            });
        });
    }

    public void ChangeControlSizeWhenCanvasSizeChanged(double canvasWidth, double canvasHeight)
    {
        foreach (CanvasElement element in this.CanvasElements)
        {
            if (ControlCreator.TryGetControl(element, out var control))
            {
                if (control.Parent != null && control.Parent is Canvas canvas)
                {
                    if (element.FixLeft)
                    {
                        if (element.FixRight && element.Right != double.NaN)
                        {
                            element.Width = Math.Max(0, canvasWidth - element.Left - element.Right);
                            control.Width = element.Width;
                        }
                    }
                    else
                    {
                        if (element.FixRight && element.Right != double.NaN)
                        {
                            element.Left = canvasWidth - element.Right - element.Width;
                            Canvas.SetLeft(control, element.Left);
                            if(control.Parent == this._workCanvas)
                            {
                                element.X = element.Left;
                            }
                        }
                    }
                    if (element.FixTop)
                    {
                        if (element.FixBottom && element.Bottom != double.NaN)
                        {
                            element.Height = Math.Max(0, canvasHeight - element.Top - element.Bottom);
                            control.Height = element.Height;
                        }
                    }
                    else
                    {
                        if (element.FixBottom && element.Bottom != double.NaN)
                        {
                            element.Top = canvasHeight - element.Bottom - element.Height;
                            Canvas.SetTop(control, element.Top);
                            if (control.Parent == this._workCanvas)
                            {
                                element.Y = element.Top;
                            }
                        }
                    }
                }
            }
        }
    }

    public void DeleteSelectedElements()
    {
        if (_selectionOverlay!.SelectedElements.Count > 0)
        {
            for (int i = _selectionOverlay.SelectedElements.Count - 1; i >= 0; i--)
            {
                var deletedElement = _selectionOverlay.SelectedElements[i];
                DeleteElement(deletedElement);
            }
            _selectionOverlay.RefreshSelectionOverlay();
        }

        SetSelectedElement();
    }

    public void SetSelectedElement()
    {
        if (_selectionOverlay!.SelectedElements.Count == 1)
        {
            if (_selectionOverlay.SelectedElements[0] == SelectedElement)
            {
                return;
            }
            SelectedElement = _selectionOverlay.SelectedElements[0];
        }
        else
        {
            SelectedElement = null;
        }

        SelectedElementChanged?.Invoke(this, SelectedElement);
    }

    private void DeleteElement(CanvasElement deletedElement)
    {
        if (deletedElement == null)
            return;

        if (CanvasElements.Contains(deletedElement))
        {
            CanvasElements.Remove(deletedElement);
        }
        if(_selectionOverlay!.SelectedElements.Contains(deletedElement))
        {
            _selectionOverlay.SelectedElements.Remove(deletedElement);
        }

        ControlCreator!.DeleteElement(deletedElement);

        _selectionOverlay.RefreshSelectionOverlay();
    }

    public Control? CreateElementControl(ComponentDefinition definition,out CanvasElement element)
    {
        var control = ControlCreator!.CreateElementControl(definition, out element);

        return control;
    }

    /// <summary>
    /// 更新所有元素的Id标签显示
    /// </summary>
    public void UpdateAllElementsShowIdLabel(bool showAllIds)
    {
        foreach (var element in CanvasElements)
        {
            if (ControlCreator!.TryGetControl(element, out var control))
            {
                if (control is IElementControlBase baseControl)
                {
                    element.ShowIdLabel = showAllIds;
                    baseControl.UpdateIdLabel(element);
                }
            }
        }
    }

    /// <summary>
    /// 根据Id查找元素
    /// </summary>
    public CanvasElement? FindElementById(string id)
    {
        return FindElementByIdRecursive(CanvasElements, id);
    }

    //递归按ID查找元素
    private CanvasElement? FindElementByIdRecursive(ObservableCollection<CanvasElement> elements, string id)
    {
        foreach (var element in elements)
        {
            if (element.Id == id)
                return element;

            if (element.Children != null && element.Children.Count > 0)
            {
                var found = FindElementByIdRecursive(element.Children, id);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据Id列表选中元素
    /// </summary>
    public void SelectElementsByIds(IEnumerable<string> ids)
    {
        _selectionOverlay?.ClearAllSelection();

        foreach (var id in ids)
        {
            var element = FindElementById(id);
            if (element != null)
            {
                _selectionOverlay?.AddSelectedElement(element);
            }
        }
        _selectionOverlay!.RefreshSelectionOverlay();
        SetSelectedElement();
    }

    /// <summary>
    /// 从外部（如图层树）选择一个元素
    /// </summary>
    public void SelectElement(CanvasElement? element)
    {
        if (element == null)
        {
            _selectionOverlay?.ClearAllSelection();
            SetSelectedElement();
            return;
        }

        _selectionOverlay?.ClearAllSelection();
        _selectionOverlay?.AddSingleSelectedElement(element);
    }

    /// <summary>
    /// 从外部（如图层树）选择一个元素
    /// </summary>
    public void SelectElements(CanvasElement[] elements)
    {
        if (elements == null || elements.Length == 0)
        {
            _selectionOverlay?.ClearAllSelection();
            SetSelectedElement();
            return;
        }

        _selectionOverlay?.ClearAllSelection();
        _selectionOverlay?.AddSelectedElements(elements);

    }

    #endregion

    #region 元素图层操作

    /// <summary>
    /// 检测两个元素是否相交（基于轴对齐边界框）
    /// </summary>
    private bool CheckElementsAreIntersecting(CanvasElement a, CanvasElement b)
    {
        var boundsA = GetRotatedBounds(a);
        var boundsB = GetRotatedBounds(b);

        return !(boundsA.MaxX <= boundsB.MinX ||
                 boundsA.MinX >= boundsB.MaxX ||
                 boundsA.MaxY <= boundsB.MinY ||
                 boundsA.MinY >= boundsB.MaxY);
    }

    public void MoveElementUp()
    {
        foreach (CanvasElement element in _selectionOverlay!.SelectedElements)
        {
            MoveElementUp(element);
        }

        //历史记录
        _historyManager.RecordElements(this.CanvasElements, this._selectionOverlay!.SelectedElements.Select(x => x.Id).ToArray(), HistoryActionType.ElementModified);
    }

    public void MoveElementUp(CanvasElement? element)
    {
        if (element == null)
        {
            return;
        }

        var canvasElements = CanvasElements;
        if (element.Parent != null)
        {
            canvasElements = element.Parent.Children;
        }

        var elementZIndex = element.ZIndex;

        var intersectingElements = canvasElements
            .Where(e => e != element && CheckElementsAreIntersecting(element, e))
            .ToList();

        if (intersectingElements.Count == 0)
        {
            return;
        }

        int maxIntersectingZ = intersectingElements.Max(e => e.ZIndex);
        int minIntersectingZ = intersectingElements.Min(e => e.ZIndex);

        if (elementZIndex <= maxIntersectingZ)
        {
            element.ZIndex = maxIntersectingZ + 1;
        }

        UpdateControlZIndex(element);
        foreach (var intersecting in intersectingElements)
        {
            UpdateControlZIndex(intersecting);
        }
    }

    public void MoveElementDown()
    {
        foreach (CanvasElement element in _selectionOverlay!.SelectedElements)
        {
            MoveElementDown(element);
        }
        //历史记录
        _historyManager.RecordElements(this.CanvasElements, this._selectionOverlay!.SelectedElements.Select(x => x.Id).ToArray(), HistoryActionType.ElementModified);
    }

    public void MoveElementDown(CanvasElement? element)
    {
        if (element == null)
        {
            return;
        }

        var canvasElements = CanvasElements;
        if (element.Parent != null)
        {
            canvasElements = element.Parent.Children;
        }

        var elementZIndex = element.ZIndex;

        var intersectingElements = canvasElements
            .Where(e => e != element && CheckElementsAreIntersecting(element, e))
            .ToList();

        if (intersectingElements.Count == 0)
        {
            return;
        }

        int minIntersectingZ = intersectingElements.Min(e => e.ZIndex);

        if (elementZIndex >= minIntersectingZ)
        {
            element.ZIndex = minIntersectingZ - 1;
        }

        UpdateControlZIndex(element);
        foreach (var intersecting in intersectingElements)
        {
            UpdateControlZIndex(intersecting);
        }
    }

    /// <summary>
    /// 更新控件的ZIndex以匹配元素
    /// </summary>
    private void UpdateControlZIndex(CanvasElement element)
    {
        if (ControlCreator.TryGetControl(element, out var control))
        {
            control.ZIndex = element.ZIndex;
        }
    }

    #endregion

    public void ClearAllStatus()
    {
        ClearDragPreviewIcon();
        _isDraggingFromComponent = false;
        _draggingComponent = null;
    }
}