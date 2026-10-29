using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System.Text.Json;
using yuyiAvalonia.UI.Controls;
using yuyiAvalonia.UI.Controls.ElementFlow;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;
using yuyiAvalonia.UI.Services;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Views;

public partial class FlowEditorView : UserControl
{
    private FlowContainerManager _containerManager = null!;
    private Border? _canvasBackgroundBorder;
    private Border? _canvasBackgroundImageBorder;
    private readonly RenderControl _renderControl = new();

    private IUIControlCreator? _controlCreator = null;

    /// <summary>
    /// 控件创建器（用于预览等场景）
    /// </summary>
    public IUIControlCreator? ControlCreator => _controlCreator;

    public FlowEditorView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;

        ElementsCanvas.PointerPressed += MainCanvas_PointerPressed;
        MainCanvas.PointerMoved += MainCanvas_PointerMoved;
        MainCanvas.PointerReleased += MainCanvas_PointerReleased;
        MainCanvas.KeyDown += MainCanvas_KeyDown;

        CanvasScrollViewer.ScrollChanged += CanvasScrollViewer_ScrollChanged;
        CardElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        TextElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        FlowShapeElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundedPolygonElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundedStarElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundedRectangleElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundedRectangleStackPanelElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundedRectangleGridElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundedRectangleCanvasElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;
        RoundEllipseElementControl.ElementPropertyChanged += SelectedElementPropertyChanged;

        Loaded += OnLoaded;

        // 组件面板事件
        ComponentPanel.Groups = ComponentGroupSource.Instance.Groups;
        ComponentPanel.WhenComponentPressed += (s, args) =>
        {
            _containerManager.CreateDraggingFromComponentPanel(args.Definition, args.Args);
        };
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not FlowEditorViewModel vm)
            return;

        _controlCreator = new FlowUIControlCreator(this.ElementsCanvas, vm.CanvasElements);
        _containerManager = new FlowContainerManager(MainCanvas, ElementsCanvas, vm.CanvasElements, _controlCreator, _renderControl, MultiSelectionOverlay, vm.HistoryManager);

        vm.HistoryManager.StateRestored += HistoryManager_StateRestored;

        // 连接模板面板事件
        TemplatePanel.WhenLandingPageTemplateRequested += (s, ev) => vm.LoadLandingPageTemplateCommand.Execute(null);
        TemplatePanel.WhenFormTemplateRequested += (s, ev) => vm.LoadFormTemplateCommand.Execute(null);
        TemplatePanel.WhenDashboardTemplateRequested += (s, ev) => vm.LoadDashboardTemplateCommand.Execute(null);

        // 多选覆盖层删除事件
        MultiSelectionOverlay.DeleteRequested += (s, e) => _containerManager.DeleteSelectedElements();
        MultiSelectionOverlay.RenderControl = _renderControl;

        // 初始化图层树
        ElementTreeViewModel flowElementTreeViewModel = new ElementTreeViewModel(_controlCreator, vm.CanvasElements);
        ElementTree.DataContext = flowElementTreeViewModel;

        // 多选覆盖层选择变化时同步到元素树
        MultiSelectionOverlay!.SelectionChanged += (s, elements) =>
        {
            flowElementTreeViewModel.SelectElements(elements.ToList());
        };

        // 元素树单选/多选变化时，同步到画布
        flowElementTreeViewModel.ElementsSelectionChanged += (s, elements) =>
        {
            _containerManager.SelectElements(elements.ToArray());
        };

        // 连接容器管理器事件
        _containerManager.SelectedElementChanged += (x, selectedElement) =>
        {
            vm.SelectElementCommand.Execute(selectedElement);

            // 更新刻度尺绑定到选中元素
            UpdateRulersTarget(selectedElement);

        };

        // 元素大小调整结束时记录历史
        MultiSelectionOverlay!.ElementResized += (s, args) =>
        {
            vm.HistoryManager.RecordElements(vm.CanvasElements, MultiSelectionOverlay.SelectedElements.Select(x => x.Id).ToArray(), HistoryActionType.ElementResized);
        };

        // 元素旋转结束时记录历史
        MultiSelectionOverlay!.ElementRotated += (s, args) =>
        {
            vm.HistoryManager.RecordElements(vm.CanvasElements, MultiSelectionOverlay.SelectedElements.Select(x => x.Id).ToArray(), HistoryActionType.ElementStyled);
        };

        // 元素对齐操作结束时记录历史
        MultiSelectionOverlay!.ElementAligned += (s, args) =>
        {
            vm.HistoryManager.RecordElements(vm.CanvasElements, MultiSelectionOverlay.SelectedElements.Select(x => x.Id).ToArray(), HistoryActionType.ElementStyled);
        };

        vm.WhenCanvasPropertyChanged += OnCanvasPropertyChanged;
        vm.WhenSelectedElementPropertyChanged += SelectedElementPropertyChanged;

        vm.OnRefreshCanvasElements += (sender, ev) => Dispatcher.UIThread.Post(RefreshCanvasElements);

        vm.WhenPreviewCanvasRequested += (sender, ev) => Dispatcher.UIThread.Post(() =>
        {
            var previewDialog = new CanvasPreviewDialog();
            previewDialog.CreateElementControl = element => ControlCreator!.CreateElementControlForPreview(element);
            previewDialog.RenderElements(vm.CanvasElements, vm.CanvasProperty);
            previewDialog.Show();
        });

        vm.WhenAlignRequested += (sender, ev) =>
        {
            MultiSelectionOverlay.AlignElements(ev.Alignment, this.ElementsCanvas);
        };

        vm.WhenShowAllElementIdsChanged += (s, showAllIds) =>
        {
            Dispatcher.UIThread.Post(() => _containerManager?.UpdateAllElementsShowIdLabel(showAllIds));
        };

        vm.WhenShowRulersChanged += (s, showRulers) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (HorizontalRuler != null) HorizontalRuler.IsVisible = showRulers;
                if (VerticalRuler != null) VerticalRuler.IsVisible = showRulers;
            });
        };

        vm.WhenShowGridChanged += (s, showGrid) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (GridCanvas != null) GridCanvas.ShowGrid = showGrid;
            });
        };

        #region 页面属性

        CanvasPageProperty.CanvasProperty = vm.CanvasProperty;
        CanvasPageProperty.CanvasSizes = vm.CanvasSizes;
        CanvasPageProperty.SelectedCanvasSize = vm.SelectedCanvasSize;
        CanvasPageProperty.WhenCanvasSizePressed += (s, size) =>
        {
            vm.SelectedCanvasSize = size;
            _containerManager.ChangeControlSizeWhenCanvasSizeChanged(vm.CanvasProperty.CanvasWidth, vm.CanvasProperty.CanvasHeight);
        };

        vm.CanvasProperty.WhenBackgroundChanged += (s, ev) => Dispatcher.UIThread.Post(() =>
        {
            _renderControl.RenderBackground(_canvasBackgroundBorder, vm.CanvasProperty);
            _renderControl.RenderBackgroundImage(_canvasBackgroundImageBorder, vm.CanvasProperty);
        });

        Dispatcher.UIThread.Post(() =>
        {
            _renderControl.RenderBackground(_canvasBackgroundBorder, vm.CanvasProperty);
            _renderControl.RenderBackgroundImage(_canvasBackgroundImageBorder, vm.CanvasProperty);
        });

        #endregion

        ContentPanel.SizeChanged += (sender, args) =>
        {
            vm.ContentPanelHeight = ContentPanel.Bounds.Height;
            vm.ContentPanelWidth = ContentPanel.Bounds.Width;
        };

        vm.WhenZoomFactorChanged += (s, zoom) => Dispatcher.UIThread.Post(() => UpdateRulerValues(vm.ZoomFactor));
    }

    private void SelectedElementPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender != null && sender is CanvasElement element)
        {
            _containerManager.UpdateElementControl(element, e.PropertyName);
        }
    }

    private void CanvasScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        UpdateRulerValues();
    }

    #region 主画布事件

    private void MainCanvas_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Delete:
                _containerManager.DeleteSelectedElements();
                break;
        }
    }

    private void MainCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var parentCanvas = ElementsCanvas;
        var point = e.GetCurrentPoint(parentCanvas);

        // 如果右键按下且有多选，尝试多选移动
        if (MultiSelectionOverlay?.IsSelectionActive == true)
        {
            // 检查是否在选中框内右键点击
            if (point.Properties.IsRightButtonPressed && MultiSelectionOverlay.IsPointInSelectionRect(point.Position))
            {
                MultiSelectionOverlay.ControlsMoveStart(point.Position, parentCanvas!);
                _containerManager.SetSelectedElement();
                e.Handled = true;
                return;
            }
        }

        var clickedElement = _containerManager.FindClickedElementAtPosition(point.Position);

        if (clickedElement != null && ControlCreator.TryGetControl(clickedElement, out var control))
        {
            bool isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            if (!isCtrlPressed)
            {
                // 如果点击的元素不在当前多选中，则清除多选并添加该元素到多选中
                MultiSelectionOverlay!.AddSingleSelectedElement(clickedElement);
                _containerManager.SetSelectedElement();

                _containerManager.HistoryManager.RecordSelection(MultiSelectionOverlay.SelectedElements.Select(x => x.Id).ToArray());

                if (point.Properties.IsRightButtonPressed && MultiSelectionOverlay.IsPointInSelectionRect(point.Position))
                {
                    MultiSelectionOverlay.ControlsMoveStart(point.Position, parentCanvas!);
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                MultiSelectionOverlay!.SwitchSelectedElement(clickedElement);

                _containerManager.HistoryManager.RecordSelection(MultiSelectionOverlay.SelectedElements.Select(x => x.Id).ToArray());

                _containerManager.SetSelectedElement();
                e.Handled = true;
                return;
            }
        }
        else
        {
            MultiSelectionOverlay!.ClearAllSelection();

            // 将事件转发给多选覆盖层处理框选
            MultiSelectionOverlay?.SelectionStart(e);
            _containerManager.SetSelectedElement();
        }
    }

    private void MainCanvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        var currentPos = e.GetPosition(this);
        UpdateStatusbarMousePosition(currentPos);

        var canvasPosition = e.GetPosition(this.ElementsCanvas);

        // 把组件从外层进入到画布层
        if (_containerManager.IsDraggingFromComponentPanel() && (canvasPosition.X >= 0 && canvasPosition.Y >= 0))
        {
            _containerManager.MoveDraggingComponentToWorkCanvas(e);
            MultiSelectionOverlay?.RefreshSelectionOverlay();
            return;
        }

        if (_containerManager.IsDraggingFromComponentPanel())
        {
            _containerManager.MoveDraggingFromComponentPanel(currentPos);
            return;
        }

        if (MultiSelectionOverlay.IsMarqueeSelecting)
        {
            MultiSelectionOverlay?.SelectionMoved(e);
        }
        else if (MultiSelectionOverlay.IsMultiMoving)
        {
            // 处理多选移动
            bool isCtrlPressed = e.KeyModifiers.HasFlag(KeyModifiers.Control);
            MultiSelectionOverlay.ControlsMoveTo(canvasPosition, this.ElementsCanvas!, isCtrlPressed);
            MultiSelectionOverlay?.RefreshSelectionOverlay();
        }

        if (_containerManager.IsDraggingFromComponentPanel() || MultiSelectionOverlay!.IsMultiMoving)
        {
            // 检测是否悬停在 Grid 容器上，显示网格线
            _containerManager.UpdateContainerControlOverlay(canvasPosition);
        }
    }

    private void MainCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (MultiSelectionOverlay!.IsMultiMoving)
        {
            _containerManager.MultiSelectionMovedEnd(e);
            _containerManager.HideContainerControlOverlay();

            this.MainCanvas!.Cursor = Cursor.Default;
        }
        else if (MultiSelectionOverlay!.IsMarqueeSelecting)
        {
            MultiSelectionOverlay?.SelectionEnd(e);
            if (DataContext is FlowEditorViewModel vm)
            {
                //历史记录
                vm.HistoryManager.RecordElements(vm.CanvasElements, this.MultiSelectionOverlay!.SelectedElements.Select(x => x.Id).ToArray(), HistoryActionType.ElementModified);
            }
        }
        else if (_containerManager.IsDraggingFromComponentPanel())
        {
            _containerManager.EndDraggingFromComponentPanel(e);
        }

    }

    #endregion

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _canvasBackgroundBorder = CanvasBackgroundBorder;
        _canvasBackgroundImageBorder = CanvasBackgroundImageBorder;

        Dispatcher.UIThread.Post(() =>
        {
            UpdateRulerValues();

            if (DataContext is FlowEditorViewModel vm)
            {
                HorizontalRuler.IsVisible = vm.CanvasProperty.ShowRulers;
                VerticalRuler.IsVisible = vm.CanvasProperty.ShowRulers;
                if (GridCanvas != null && HorizontalRuler != null)
                {
                    GridCanvas.ShowGrid = vm.CanvasProperty.ShowGrid;
                    GridCanvas.GridTickInterval = HorizontalRuler.TickInterval;
                    GridCanvas.GridMajorTickInterval = HorizontalRuler.MajorTickInterval;
                }
            }
        });
    }

    private void HistoryManager_StateRestored(object? sender, Services.HistoryRecord e)
    {
        Dispatcher.UIThread.Post((Action)(() =>
        {
            switch (e.ActionType)
            {
                case HistoryActionType.SelectionChanged:
                    _containerManager.SelectElementsByIds(e.SelectedElementIds);
                    break;

                case HistoryActionType.ElementAdded:
                case HistoryActionType.ElementDeleted:
                case HistoryActionType.ElementModified:
                case HistoryActionType.ElementMoved:
                case HistoryActionType.ElementResized:
                case HistoryActionType.ElementStyled:
                    RestoreCanvasElements(e.CanvasElementsJson);
                    _containerManager.SelectElementsByIds(e.SelectedElementIds);
                    break;

                case HistoryActionType.SingleElementChanged:
                    if (string.IsNullOrEmpty(e.SingleElementJson) || DataContext is not FlowEditorViewModel vm)
                        return;
                    var element = JsonSerializer.Deserialize<CanvasElement>(e.SingleElementJson, CanvasElementSerializer.DeserializationOptions);
                    if (_controlCreator!.TryGetControl(element!, out Control? control) && control is IElementControlBase elemControl)
                    {
                        elemControl.UpdateElement(element!);
                    }
                    break;

                case HistoryActionType.CanvasPropertyChanged:
                    RestoreCanvasProperty(e.CanvasPropertyJson);
                    break;

                case HistoryActionType.Unspecified:
                default:
                    RestoreCanvasElements(e.CanvasElementsJson);
                    RestoreCanvasProperty(e.CanvasPropertyJson);
                    _containerManager.SelectElementsByIds(e.SelectedElementIds);
                    break;
            }

            // 刷新画布
            MultiSelectionOverlay?.RefreshSelectionOverlay();
        }));
    }

    private void RestoreCanvasElements(string? json)
    {
        if (string.IsNullOrEmpty(json) || DataContext is not FlowEditorViewModel vm)
            return;

        try
        {
            var elements = JsonSerializer.Deserialize<List<CanvasElement>>(json, CanvasElementSerializer.DeserializationOptions);
            if (elements == null) return;

            // 逐个移除现有元素（避免触发 Reset action，Reset 会导致 Dispatcher 回调在之后清空画布）
            while (vm.CanvasElements.Count > 0)
            {
                vm.CanvasElements.RemoveAt(vm.CanvasElements.Count - 1);
            }
            MultiSelectionOverlay.ClearAllSelection();
            //SingleSelectionOverlay.ClearOverlay();
            ControlCreator!.ElementControlMap.Clear();

            // 直接在 ElementsCanvas 上重建子元素，避免依赖 RefreshCanvasElements
            ElementsCanvas.Children.Clear();
            foreach (var element in elements)
            {
                vm.CanvasElements.Add(element);

                var control = ControlCreator!.CreateElementControl(element) as BorderControlBase;
                if (control != null)
                {
                    ElementsCanvas.Children.Add(control);
                    element.Left = element.X;
                    element.Top = element.Y;
                    control.UpdateLeft(element.Left);
                    control.UpdateTop(element.Top);
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    private void RestoreCanvasProperty(string? json)
    {
        if (string.IsNullOrEmpty(json) || DataContext is not FlowEditorViewModel vm)
            return;

        try
        {
            var property = JsonSerializer.Deserialize<CanvasProperty>(json);
            if (property == null) return;

            vm.CanvasProperty.CanvasWidth = property.CanvasWidth;
            vm.CanvasProperty.CanvasHeight = property.CanvasHeight;
            vm.CanvasProperty.CanvasBackgroundColor = property.CanvasBackgroundColor;
            vm.CanvasProperty.ShowGrid = property.ShowGrid;
            vm.CanvasProperty.ShowRulers = property.ShowRulers;
        }
        catch (Exception ex)
        {

        }
    }

    private void RefreshCanvasElements()
    {
        if (DataContext is not FlowEditorViewModel vm) 
            return;
        ElementsCanvas.Children.Clear();

        foreach (var element in vm.CanvasElements)
        {
            var control = ControlCreator!.CreateElementControl(element) as BorderControlBase;
            if (control != null)
            {
                ElementsCanvas.Children.Add(control);
                element.Left = element.X;
                element.Top = element.Y;

                control.UpdateLeft(element.Left);
                control.UpdateTop(element.Top);
            }
        }
    }

    private void UpdateStatusbarMousePosition(Point position)
    {
        if (DataContext is FlowEditorViewModel vm)
        {
            vm.SetStatusMousePosition(position);
        }
    }

    #region 标尺

    private void UpdateRulerValues(double zoom)
    {
        if (HorizontalRuler == null || VerticalRuler == null || ElementsCanvas == null || CanvasPanel == null || !HorizontalRuler.IsVisible || !VerticalRuler.IsVisible)
            return;
        VerticalRuler.Zoom = zoom;
        HorizontalRuler.Zoom = zoom;
        GridCanvas!.GridTickInterval = HorizontalRuler.TickInterval;
        GridCanvas!.GridMajorTickInterval = HorizontalRuler.MajorTickInterval;
        UpdateRulerValues();
    }

    private void UpdateRulerValues()
    {
        if (HorizontalRuler == null || VerticalRuler == null || ElementsCanvas == null || CanvasPanel == null || !HorizontalRuler.IsVisible || !VerticalRuler.IsVisible)
            return;

        // 获取 ElementsCanvas 左上角(0,0)相对于 CanvasPanel 的位置
        var point = ElementsCanvas.TranslatePoint(new Point(0, 0), ContentPanel);
        var hLocation = HorizontalRuler.TranslatePoint(new Point(0, 0), ContentPanel);
        var vLocation = VerticalRuler.TranslatePoint(new Point(0, 0), ContentPanel);
        if (point.HasValue)
        {
            HorizontalRuler.StartOffset = point.Value.X;
            VerticalRuler.StartOffset = point.Value.Y;
            HorizontalRuler.Location = hLocation!.Value;
            VerticalRuler.Location = vLocation!.Value;

            HorizontalRuler.Size = new Size(ContentPanel.Bounds.Width, ContentPanel.Bounds.Height);
            VerticalRuler.Size = new Size(ContentPanel.Bounds.Width, ContentPanel.Bounds.Height);
        }
    }

    private void UpdateRulersTarget(CanvasElement? element)
    {
        // 只更新 TargetControl 用于显示位置线，不影响 StartOffset
        if (element != null && ControlCreator.TryGetControl(element, out var elementControl))
        {
            HorizontalRuler.TargetControl = elementControl;
            VerticalRuler.TargetControl = elementControl;
        }
        else
        {
            HorizontalRuler.TargetControl = null;
            VerticalRuler.TargetControl = null;
        }
    }

    #endregion

    #region 画布操作

    private void MainCanvas_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        MainGrid.Width = MainCanvas.Bounds.Width;
        MainGrid.Height = MainCanvas.Bounds.Height;
        Canvas.SetLeft(MainGrid, 0);
        Canvas.SetTop(MainGrid, 0);

        Dispatcher.UIThread.Post(UpdateRulerValues);
    }

    /// <summary>
    /// 画布属性修改后更新视图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (DataContext is FlowEditorViewModel vm)
        {
            _renderControl.OnCanvasPropertyChanged(
                _canvasBackgroundBorder,
                e.PropertyName!,
                vm.CanvasProperty,
                showAllIds => _containerManager.UpdateAllElementsShowIdLabel(showAllIds),
                _controlCreator);

            // 处理 ShowRulers 和 ShowGrid（这些需要在 View 中处理）
            switch (e.PropertyName)
            {
                case nameof(vm.CanvasProperty.ShowRulers):
                    if (HorizontalRuler != null) HorizontalRuler.IsVisible = vm.CanvasProperty.ShowRulers;
                    if (VerticalRuler != null) VerticalRuler.IsVisible = vm.CanvasProperty.ShowRulers;
                    break;
            }
        }
    }

    /// <summary>
    /// 滚动改变画布缩放
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPointerWheel(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is FlowEditorViewModel vm && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            var delta = e.Delta.Y;
            vm.ZoomFactor = Math.Max(0.25, Math.Min(4.0, vm.ZoomFactor + (delta > 0 ? 0.1 : -0.1)));
            AdjustScrollViewerOnZoom(vm.ZoomFactor);
            e.Handled = true;
        }
    }

    private void AdjustScrollViewerOnZoom(double newZoom)
    {
        if (CanvasScrollViewer == null) return;
        var h = CanvasScrollViewer.Offset.X;
        var v = CanvasScrollViewer.Offset.Y;
        var w = CanvasScrollViewer.Bounds.Width;
        var h2 = CanvasScrollViewer.Bounds.Height;

        Dispatcher.UIThread.Post(() =>
        {
            CanvasScrollViewer.Offset = new Point(w / 2 - (w / 2 - h) * newZoom, h2 / 2 - (h2 / 2 - v) * newZoom);
            UpdateRulerValues(newZoom);
        }, DispatcherPriority.Background);
    }

    #endregion

    #region 元素操作

    private void MoveUp_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _containerManager.MoveElementUp();

    private void MoveDown_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _containerManager.MoveElementDown();

    private void DeleteElement_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => _containerManager.DeleteSelectedElements();

    #endregion
}
