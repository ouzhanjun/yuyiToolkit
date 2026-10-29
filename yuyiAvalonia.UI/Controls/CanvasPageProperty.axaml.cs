using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Controls;

public partial class CanvasPageProperty : UserControl
{
    private ObservableCollection<CanvasSize>? _canvasSizesSource;

    public CanvasPageProperty()
    {
        InitializeComponent();

        IsCanvasSizePanelExpanded = false;

        // 监听 CanvasSizes 属性变化
        CanvasSizesProperty.Changed.AddClassHandler<CanvasPageProperty>((sender, e) =>
        {
            if (e.NewValue is ObservableCollection<CanvasSize> newCollection)
            {
                // 取消旧集合的监听
                if (_canvasSizesSource != null)
                {
                    _canvasSizesSource.CollectionChanged -= sender.OnCanvasSizesChanged;
                }

                // 监听新集合的变化
                _canvasSizesSource = newCollection;
                _canvasSizesSource.CollectionChanged += sender.OnCanvasSizesChanged;

                // 初始更新筛选列表
                sender.UpdateFilteredCanvasSizes();
            }
        });

        // 监听 SelectedCanvasSize 变化
        SelectedCanvasSizeProperty.Changed.AddClassHandler<CanvasPageProperty>((sender, e) =>
        {
            if (e.OldValue != null)
            {
                (e.OldValue as CanvasSize)!.IsSelected = false;
            }
            if (e.NewValue != null)
            {
                sender.SelectedCanvasSize = (CanvasSize)e.NewValue;
                sender.SelectedCanvasSize!.IsSelected = true;
            }
        });

        // 初始化筛选状态 - 默认显示全部
        _currentCategoryFilter = null;
    }

    private void OnCanvasSizesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateFilteredCanvasSizes();
    }

    #region 画布尺寸面板展开状态

    public static readonly StyledProperty<bool> IsCanvasSizePanelExpandedProperty =
        AvaloniaProperty.Register<CanvasPageProperty, bool>(
            nameof(IsCanvasSizePanelExpanded),
            defaultValue: true);

    public bool IsCanvasSizePanelExpanded
    {
        get => GetValue(IsCanvasSizePanelExpandedProperty);
        set => SetValue(IsCanvasSizePanelExpandedProperty, value);
    }

    #endregion

    #region 筛选后的画布尺寸列表

    public static readonly StyledProperty<ObservableCollection<CanvasSize>> FilteredCanvasSizesProperty =
        AvaloniaProperty.Register<CanvasPageProperty, ObservableCollection<CanvasSize>>(
            nameof(FilteredCanvasSizes),
            defaultValue: new ObservableCollection<CanvasSize>());

    public ObservableCollection<CanvasSize> FilteredCanvasSizes
    {
        get => GetValue(FilteredCanvasSizesProperty);
        set => SetValue(FilteredCanvasSizesProperty, value);
    }

    // 当前分类筛选状态
    public static readonly StyledProperty<string> CurrentCategoryFilterProperty =
        AvaloniaProperty.Register<CanvasPageProperty, string>(
            nameof(CurrentCategoryFilter),
            defaultValue: "All");

    public string CurrentCategoryFilter
    {
        get => GetValue(CurrentCategoryFilterProperty);
        set
        {
            SetValue(CurrentCategoryFilterProperty, value);
            _currentCategoryFilter = value switch
            {
                "Mobile" => CanvasSizeCategory.Mobile,
                "Tablet" => CanvasSizeCategory.Tablet,
                "Other" => CanvasSizeCategory.Other,
                _ => null
            };
            UpdateFilteredCanvasSizes();
        }
    }

    private CanvasSizeCategory? _currentCategoryFilter = null;

    private void UpdateFilteredCanvasSizes()
    {
        if (CanvasSizes == null)
        {
            FilteredCanvasSizes = new ObservableCollection<CanvasSize>();
            return;
        }

        var filtered = _currentCategoryFilter switch
        {
            CanvasSizeCategory.Mobile => CanvasSizes.Where(x => x.Category == CanvasSizeCategory.Mobile).ToList(),
            CanvasSizeCategory.Tablet => CanvasSizes.Where(x => x.Category == CanvasSizeCategory.Tablet).ToList(),
            CanvasSizeCategory.Other => CanvasSizes.Where(x => x.Category == CanvasSizeCategory.Other).ToList(),
            _ => CanvasSizes.ToList()
        };

        FilteredCanvasSizes = new ObservableCollection<CanvasSize>(filtered);
    }

    #endregion

    // 1. 定义 StyledProperty（命名建议：Property后缀表示这是属性定义）
    public static readonly StyledProperty<ObservableCollection<CanvasSize>> CanvasSizesProperty =
        AvaloniaProperty.Register<CanvasPageProperty, ObservableCollection<CanvasSize>>(
            nameof(CanvasSizes));

    public ObservableCollection<CanvasSize> CanvasSizes
    {
        get => GetValue(CanvasSizesProperty);
        set
        {
            SetValue(CanvasSizesProperty, value);
            UpdateFilteredCanvasSizes();
        }
    }

    // 2. 定义 StyledProperty（命名建议：Property后缀表示这是属性定义）
    public static readonly StyledProperty<CanvasSize> SelectedCanvasSizeProperty =
        AvaloniaProperty.Register<CanvasPageProperty, CanvasSize>(
            nameof(SelectedCanvasSize));

    public CanvasSize SelectedCanvasSize
    {
        get => GetValue(SelectedCanvasSizeProperty);
        set => SetValue(SelectedCanvasSizeProperty, value);
    }

    // 3. 定义 StyledProperty（命名建议：Property后缀表示这是属性定义）
    public static readonly StyledProperty<CanvasProperty> CanvasPropertyProperty =
        AvaloniaProperty.Register<CanvasPageProperty, CanvasProperty>(
            nameof(CanvasProperty));

    public CanvasProperty CanvasProperty
    {
        get => GetValue(CanvasPropertyProperty);
        set => SetValue(CanvasPropertyProperty, value);
    }

    private CanvasProperty? _canvasProperty = null;

    #region 画布尺寸面板折叠/展开

    private void ToggleCanvasSizePanel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsCanvasSizePanelExpanded = !IsCanvasSizePanelExpanded;
    }

    private void CloseCanvasSizePopup_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CanvasSizePopup.Close();
    }

    private void FilterByCategory_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string tag)
        {
            // 更新 Active 类
            BtnMobile.Classes.Remove("Active");
            BtnTablet.Classes.Remove("Active");
            BtnOther.Classes.Remove("Active");
            BtnAll.Classes.Remove("Active");

            switch (tag)
            {
                case "Mobile":
                    BtnMobile.Classes.Add("Active");
                    break;
                case "Tablet":
                    BtnTablet.Classes.Add("Active");
                    break;
                case "Other":
                    BtnOther.Classes.Add("Active");
                    break;
                case "All":
                    BtnAll.Classes.Add("Active");
                    break;
            }

            // 更新筛选状态
            CurrentCategoryFilter = tag;
        }
    }

    #endregion

    #region 画布尺寸选择

    public event EventHandler<CanvasSize>? WhenCanvasSizePressed;

    private void OnCanvasSizePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is CanvasSize size)
        {
            if (SelectedCanvasSize != null)
            {
                SelectedCanvasSize.IsSelected = false;
            }

            size.IsSelected = true;
            SelectedCanvasSize = size;
            WhenCanvasSizePressed?.Invoke(sender, size);

            // 选择后自动关闭弹窗
            CanvasSizePopup.Close();

            e.Handled = true;
        }
    }

    #endregion

    #region 颜色预设

    public event EventHandler<PointerPressedEventArgs>? WhenColorPresetPressed;

    private void OnColorPresetPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is string color)
        {
            CanvasProperty.CanvasBackgroundColor = color;
            WhenColorPresetPressed?.Invoke(sender, e);
            e.Handled = true;
        }
    }

    private void CanvasSizePopup_Closed(object? sender, System.EventArgs e)
    {
        IsCanvasSizePanelExpanded = false;
    }

    #endregion
}