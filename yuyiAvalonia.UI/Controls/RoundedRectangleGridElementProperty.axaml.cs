using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Elements;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 圆角矩形容器（圆角矩形 + Grid）属性面板。
/// 包含原 RoundedRectangleElement 的所有属性，
/// 额外新增：内边距（Padding）、Grid 行数 / 列数 / 行间距 / 列间距 / 行定义 / 列定义。
/// </summary>
public partial class RoundedRectangleGridElementProperty : UserControl
{
    public static readonly StyledProperty<RoundedRectangleGridElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<RoundedRectangleGridElementProperty, RoundedRectangleGridElement?>(
            nameof(ElementProperty),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

    public RoundedRectangleGridElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    private RoundedRectangleGridElement? _currentElement;

    #region 事件

    public event PropertyChangedEventHandler? ElementPropertyChanged;

    /// <summary>
    /// 请求行定义事件 - 当需要获取当前的行定义时触发
    /// </summary>
    public event EventHandler<string>? RequestRowDefinitions;

    /// <summary>
    /// 请求列定义事件
    /// </summary>
    public event EventHandler<string>? RequestColumnDefinitions;

    /// <summary>
    /// 应用行定义事件 - 当用户点击行定义的应用按钮时触发
    /// </summary>
    public event EventHandler<string>? ApplyRowDefinitions;

    /// <summary>
    /// 应用列定义事件
    /// </summary>
    public event EventHandler<string>? ApplyColumnDefinitions;

    #endregion

    #region 控件引用

    private GridDefinitionEditor? _rowDefinitionEditor;
    private GridDefinitionEditor? _columnDefinitionEditor;

    #endregion

    public RoundedRectangleGridElementProperty()
    {
        InitializeComponent();

        ElementPropertyProperty.Changed.AddClassHandler<RoundedRectangleGridElementProperty>((sender, e) =>
        {
            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged -= sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged -= sender.OnBackFillPropertyChanged;
            }

            sender._currentElement = e.NewValue as RoundedRectangleGridElement;

            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged += sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged += sender.OnBackFillPropertyChanged;
                sender.SyncFillEditor();
                sender.SyncStrokeEditor();
            }
        });

        ShapeEditor.ElementPropertyChanged += OnShapeEditorElementPropertyChanged;

        // 控件初始化后获取 GridDefinitionEditor 引用
        Loaded += OnLoaded;
    }

    private void OnBackFillPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_currentElement != null && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(_currentElement, new System.ComponentModel.PropertyChangedEventArgs(e.PropertyName));
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _rowDefinitionEditor = this.FindControl<GridDefinitionEditor>("PART_RowDefinitionEditor");
        _columnDefinitionEditor = this.FindControl<GridDefinitionEditor>("PART_ColumnDefinitionEditor");

        if (_rowDefinitionEditor != null)
        {
            _rowDefinitionEditor.Applied += OnRowDefinitionApplied;
        }

        if (_columnDefinitionEditor != null)
        {
            _columnDefinitionEditor.Applied += OnColumnDefinitionApplied;
        }
    }

    private void OnShapeEditorElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ElementPropertyChanged?.Invoke(sender, e);
    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is RoundedRectangleGridElement && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(sender, e);
        }
    }

    /// <summary>
    /// 设置当前的行定义到编辑器
    /// </summary>
    public void SetRowDefinitions(string definition)
    {
        if (_rowDefinitionEditor != null)
        {
            _rowDefinitionEditor.Definition = definition;
        }
    }

    /// <summary>
    /// 设置当前的列定义到编辑器
    /// </summary>
    public void SetColumnDefinitions(string definition)
    {
        if (_columnDefinitionEditor != null)
        {
            _columnDefinitionEditor.Definition = definition;
        }
    }

    /// <summary>
    /// 行定义应用事件处理
    /// </summary>
    private void OnRowDefinitionApplied(object? sender, string definition)
    {
        if (ElementProperty is RoundedRectangleGridElement gridElement)
        {
            gridElement.RowDefinitions = definition;
        }
        ApplyRowDefinitions?.Invoke(this, definition);
    }

    /// <summary>
    /// 列定义应用事件处理
    /// </summary>
    private void OnColumnDefinitionApplied(object? sender, string definition)
    {
        if (ElementProperty is RoundedRectangleGridElement gridElement)
        {
            gridElement.ColumnDefinitions = definition;
        }
        ApplyColumnDefinitions?.Invoke(this, definition);
    }

    #region FillEditor

    private void SyncFillEditor()
    {
        var fillEditor = this.FillEditorCtrl;
        if (fillEditor == null) return;

        if (_currentElement != null)
        {
            fillEditor.Fills = _currentElement.BackgroundFills;
            fillEditor.BackgroundFill = _currentElement.BackgroundFill;
            fillEditor.RefreshControl -= OnFillEditorRefresh;
            fillEditor.RefreshControl += OnFillEditorRefresh;
            fillEditor.FillPropertyEditorCtrl.RefreshControl -= FillEditorCtrl_FillPropertyEditorCtrl_RefreshControl;
            fillEditor.FillPropertyEditorCtrl.RefreshControl += FillEditorCtrl_FillPropertyEditorCtrl_RefreshControl;

            StrokeEditorCtrl.StrokeFills = _currentElement.StrokeFills;
            StrokeEditorCtrl.StrokeFill = _currentElement.StrokeFill;
            StrokeEditorCtrl.RefreshControl -= OnStrokeFillEditorRefresh;
            StrokeEditorCtrl.RefreshControl += OnStrokeFillEditorRefresh;
            StrokeEditorCtrl.FillPropertyEditorCtrl.RefreshControl -= StrokeEditorCtrl_FillPropertyEditorCtrl_RefreshControl;
            StrokeEditorCtrl.FillPropertyEditorCtrl.RefreshControl += StrokeEditorCtrl_FillPropertyEditorCtrl_RefreshControl;

            ShadowEditorCtrl.Shadows = _currentElement.Shadows;
            ShadowEditorCtrl.RefreshControl -= OnShadowEditorRefreshControl;
            ShadowEditorCtrl.RefreshControl += OnShadowEditorRefreshControl;
        }
    }

    private void FillEditorCtrl_FillPropertyEditorCtrl_RefreshControl()
    {
        if (_currentElement != null)
            _currentElement.BackgroundFill = this.FillEditorCtrl.FillPropertyEditorCtrl.Fill ?? _currentElement.BackgroundFill;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFill)));
    }

    private void StrokeEditorCtrl_FillPropertyEditorCtrl_RefreshControl()
    {
        if (_currentElement != null)
            _currentElement.StrokeFill = this.StrokeEditorCtrl.FillPropertyEditorCtrl.Fill ?? _currentElement.StrokeFill;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.StrokeFill)));
    }

    private void OnShadowEditorRefreshControl()
    {
        if (_currentElement != null)
            _currentElement.Shadows = this.ShadowEditorCtrl?.Shadows ?? _currentElement.Shadows;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs("Shadows"));
    }

    private void OnFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.BackgroundFills = this.FillEditorCtrl?.Fills ?? _currentElement.BackgroundFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFills)));
    }

    private void OnStrokeFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.StrokeFills = this.StrokeEditorCtrl?.StrokeFills ?? _currentElement.StrokeFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.StrokeFills)));
    }

    private void SyncStrokeEditor()
    {
        var strokeEditor = this.StrokeEditorCtrl;
        if (strokeEditor == null) return;

        if (_currentElement != null)
        {
            strokeEditor.StrokeFills = _currentElement.StrokeFills;
            strokeEditor.RefreshControl -= OnStrokeFillEditorRefresh;
            strokeEditor.RefreshControl += OnStrokeFillEditorRefresh;
        }
    }

    #endregion
}