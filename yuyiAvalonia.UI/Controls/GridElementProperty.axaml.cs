using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// Grid 元素属性面板
/// </summary>
public partial class GridElementProperty : UserControl
{
    #region 依赖属性

    /// <summary>
    /// 选中的元素属性
    /// </summary>
    public static readonly StyledProperty<CanvasElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<GridElementProperty, CanvasElement?>(
            nameof(ElementProperty),
            defaultValue: null);

    public CanvasElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    private CanvasElement? _currentElement;

    #region FillEditor

    private void SyncFillEditor()
    {
        var fillEditor = this.FillEditorCtrl;
        if (fillEditor == null) return;

        if (_currentElement != null)
        {
            fillEditor.Fills = _currentElement.BackgroundFills;
            fillEditor.RefreshControl -= OnFillEditorRefresh;
            fillEditor.RefreshControl += OnFillEditorRefresh;
        }
    }

    private void OnFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.BackgroundFills = this.FillEditorCtrl?.Fills ?? _currentElement.BackgroundFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFills)));
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

    private void OnStrokeFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.StrokeFills = this.StrokeEditorCtrl?.StrokeFills ?? _currentElement.StrokeFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.StrokeFills)));
    }

    #endregion

    #region 事件

    /// <summary>
    /// 请求行定义事件 - 当需要获取当前的行定义时触发
    /// 返回: 当前行定义字符串
    /// </summary>
    public event EventHandler<string>? RequestRowDefinitions;

    /// <summary>
    /// 请求列定义事件 - 当需要获取当前的列定义时触发
    /// 返回: 当前列定义字符串
    /// </summary>
    public event EventHandler<string>? RequestColumnDefinitions;

    /// <summary>
    /// 应用行定义事件 - 当用户点击行定义的应用按钮时触发
    /// 参数: 新定义字符串
    /// </summary>
    public event EventHandler<string>? ApplyRowDefinitions;

    /// <summary>
    /// 应用列定义事件 - 当用户点击列定义的应用按钮时触发
    /// 参数: 新定义字符串
    /// </summary>
    public event EventHandler<string>? ApplyColumnDefinitions;

    /// <summary>
    /// 属性变更事件
    /// </summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    #endregion

    #region 控件引用

    private GridDefinitionEditor? _rowDefinitionEditor;
    private GridDefinitionEditor? _columnDefinitionEditor;

    #endregion

    public GridElementProperty()
    {
        InitializeComponent();

        // 监听 ElementProperty 变化
        ElementPropertyProperty.Changed.AddClassHandler<GridElementProperty>((sender, e) =>
        {
            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged -= sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged -= sender.OnBackFillPropertyChanged;
            }

            sender._currentElement = e.NewValue as CanvasElement;

            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged += sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged += sender.OnBackFillPropertyChanged;
                sender.SyncFillEditor();
                sender.SyncStrokeEditor();
            }
        });

        // 控件初始化后获取引用
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
        // 获取 GridDefinitionEditor 的引用
        _rowDefinitionEditor = this.FindControl<GridDefinitionEditor>("PART_RowDefinitionEditor");
        _columnDefinitionEditor = this.FindControl<GridDefinitionEditor>("PART_ColumnDefinitionEditor");

        // 订阅应用事件
        if (_rowDefinitionEditor != null)
        {
            _rowDefinitionEditor.Applied += OnRowDefinitionApplied;
        }

        if (_columnDefinitionEditor != null)
        {
            _columnDefinitionEditor.Applied += OnColumnDefinitionApplied;
        }

    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null)
        {
            // 触发属性面板更新
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
        // 更新 GridElement 的属性
        if (ElementProperty is GridElement gridElement)
        {
            gridElement.RowDefinitions = definition;
        }

        // 触发应用事件，让上层处理 GridControl 的更新
        ApplyRowDefinitions?.Invoke(this, definition);
    }

    /// <summary>
    /// 列定义应用事件处理
    /// </summary>
    private void OnColumnDefinitionApplied(object? sender, string definition)
    {
        // 更新 GridElement 的属性
        if (ElementProperty is GridElement gridElement)
        {
            gridElement.ColumnDefinitions = definition;
        }

        // 触发应用事件，让上层处理 GridControl 的更新
        ApplyColumnDefinitions?.Invoke(this, definition);
    }
}
#endregion