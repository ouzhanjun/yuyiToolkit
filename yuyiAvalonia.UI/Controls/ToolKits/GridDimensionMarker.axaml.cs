using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using yuyiAvalonia.Base.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// Grid 尺寸标注控件 - 显示行列尺寸和比例信息
/// 支持浮动定位，当选中 Grid 时自动定位到Grid上方和左边
/// </summary>
public partial class GridDimensionMarker : UserControl
{
    #region 依赖属性

    /// <summary>
    /// 锁定状态（用于XAML绑定）
    /// </summary>
    public static readonly StyledProperty<bool> IsLockCheckedProperty =
        AvaloniaProperty.Register<GridDimensionMarker, bool>(
            nameof(IsLockChecked),
            defaultValue: false);

    public bool IsLockChecked
    {
        get => GetValue(IsLockCheckedProperty);
        set => SetValue(IsLockCheckedProperty, value);
    }

    /// <summary>
    /// 比例模式状态（用于XAML绑定）
    /// </summary>
    public static readonly StyledProperty<bool> IsRatioCheckedProperty =
        AvaloniaProperty.Register<GridDimensionMarker, bool>(
            nameof(IsRatioChecked),
            defaultValue: false);

    public bool IsRatioChecked
    {
        get => GetValue(IsRatioCheckedProperty);
        set => SetValue(IsRatioCheckedProperty, value);
    }

    /// <summary>
    /// 行数
    /// </summary>
    public static readonly StyledProperty<int> RowCountProperty =
        AvaloniaProperty.Register<GridDimensionMarker, int>(
            nameof(RowCount),
            defaultValue: 3,
            defaultBindingMode: BindingMode.TwoWay);

    public int RowCount
    {
        get => GetValue(RowCountProperty);
        set => SetValue(RowCountProperty, value);
    }

    /// <summary>
    /// 列数
    /// </summary>
    public static readonly StyledProperty<int> ColumnCountProperty =
        AvaloniaProperty.Register<GridDimensionMarker, int>(
            nameof(ColumnCount),
            defaultValue: 3,
            defaultBindingMode: BindingMode.TwoWay);

    public int ColumnCount
    {
        get => GetValue(ColumnCountProperty);
        set => SetValue(ColumnCountProperty, value);
    }

    /// <summary>
    /// 行定义（格式如 "*,*,*" 或 "2*,*,Auto"）
    /// </summary>
    public static readonly StyledProperty<string> RowDefinitionsProperty =
        AvaloniaProperty.Register<GridDimensionMarker, string>(
            nameof(RowDefinitions),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string RowDefinitions
    {
        get => GetValue(RowDefinitionsProperty);
        set => SetValue(RowDefinitionsProperty, value);
    }

    /// <summary>
    /// 列定义（格式如 "*,*,*" 或 "2*,*,Auto"）
    /// </summary>
    public static readonly StyledProperty<string> ColumnDefinitionsProperty =
        AvaloniaProperty.Register<GridDimensionMarker, string>(
            nameof(ColumnDefinitions),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string ColumnDefinitions
    {
        get => GetValue(ColumnDefinitionsProperty);
        set => SetValue(ColumnDefinitionsProperty, value);
    }

    /// <summary>
    /// 总宽度（用于计算比例显示）
    /// </summary>
    public static readonly StyledProperty<double> TotalWidthProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(TotalWidth),
            defaultValue: 300,
            defaultBindingMode: BindingMode.TwoWay);

    public double TotalWidth
    {
        get => GetValue(TotalWidthProperty);
        set => SetValue(TotalWidthProperty, value);
    }

    /// <summary>
    /// 总高度（用于计算比例显示）
    /// </summary>
    public static readonly StyledProperty<double> TotalHeightProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(TotalHeight),
            defaultValue: 200,
            defaultBindingMode: BindingMode.TwoWay);

    public double TotalHeight
    {
        get => GetValue(TotalHeightProperty);
        set => SetValue(TotalHeightProperty, value);
    }

    /// <summary>
    /// 行间距
    /// </summary>
    public static readonly StyledProperty<double> RowSpacingProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(RowSpacing),
            defaultValue: 5,
            defaultBindingMode: BindingMode.TwoWay);

    public double RowSpacing
    {
        get => GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    /// <summary>
    /// 列间距
    /// </summary>
    public static readonly StyledProperty<double> ColumnSpacingProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(ColumnSpacing),
            defaultValue: 5,
            defaultBindingMode: BindingMode.TwoWay);

    public double ColumnSpacing
    {
        get => GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    /// <summary>
    /// 是否显示编辑浮层
    /// </summary>
    public static readonly StyledProperty<bool> IsEditingProperty =
        AvaloniaProperty.Register<GridDimensionMarker, bool>(
            nameof(IsEditing),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsEditing
    {
        get => GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    /// <summary>
    /// 是否启用浮动模式（跟随目标位置）
    /// </summary>
    public static readonly StyledProperty<bool> IsFloatingProperty =
        AvaloniaProperty.Register<GridDimensionMarker, bool>(
            nameof(IsFloating),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsFloating
    {
        get => GetValue(IsFloatingProperty);
        set => SetValue(IsFloatingProperty, value);
    }

    /// <summary>
    /// 目标Grid的X位置（浮动定位用）
    /// </summary>
    public static readonly StyledProperty<double> TargetLeftProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(TargetLeft),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double TargetLeft
    {
        get => GetValue(TargetLeftProperty);
        set => SetValue(TargetLeftProperty, value);
    }

    /// <summary>
    /// 目标Grid的Y位置（浮动定位用）
    /// </summary>
    public static readonly StyledProperty<double> TargetTopProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(TargetTop),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double TargetTop
    {
        get => GetValue(TargetTopProperty);
        set => SetValue(TargetTopProperty, value);
    }

    /// <summary>
    /// 目标Grid的宽度（浮动定位用）
    /// </summary>
    public static readonly StyledProperty<double> TargetWidthProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(TargetWidth),
            defaultValue: 300,
            defaultBindingMode: BindingMode.TwoWay);

    public double TargetWidth
    {
        get => GetValue(TargetWidthProperty);
        set => SetValue(TargetWidthProperty, value);
    }

    /// <summary>
    /// 目标Grid的高度（浮动定位用）
    /// </summary>
    public static readonly StyledProperty<double> TargetHeightProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(TargetHeight),
            defaultValue: 200,
            defaultBindingMode: BindingMode.TwoWay);

    public double TargetHeight
    {
        get => GetValue(TargetHeightProperty);
        set => SetValue(TargetHeightProperty, value);
    }

    /// <summary>
    /// 浮动偏移量（标注与目标Grid的间距）
    /// </summary>
    public static readonly StyledProperty<double> FloatingOffsetProperty =
        AvaloniaProperty.Register<GridDimensionMarker, double>(
            nameof(FloatingOffset),
            defaultValue: 8,
            defaultBindingMode: BindingMode.TwoWay);

    public double FloatingOffset
    {
        get => GetValue(FloatingOffsetProperty);
        set => SetValue(FloatingOffsetProperty, value);
    }

    #endregion

    #region 控件引用

    private ItemsControl? _rowMarkers;
    private ItemsControl? _columnMarkers;
    private Border? _editOverlay;
    private NumericUpDownEx? _valueInput;
    private ToggleButton? _lockButton;
    private ToggleButton? _ratioButton;
    private TranslateTransform? _translateTransform;

    #endregion

    #region 事件

    /// <summary>
    /// 行定义变更事件
    /// </summary>
    public event EventHandler<string>? RowDefinitionsChanged;

    /// <summary>
    /// 列定义变更事件
    /// </summary>
    public event EventHandler<string>? ColumnDefinitionsChanged;

    /// <summary>
    /// 选中索引变更事件（行或列）
    /// </summary>
    public event EventHandler<(bool IsRow, int Index)>? IndexSelected;

    #endregion

    #region 静态属性

    /// <summary>
    /// 静态锁状态引用（用于XAML绑定）
    /// </summary>
    public static GridDimensionMarker? StaticLockState { get; private set; }

    #endregion

    #region 静态转换器

    /// <summary>
    /// 布尔值反转转换器
    /// </summary>
    public static IValueConverter InverseBoolConverter { get; } =
        new FuncValueConverter<bool, bool>(value => !value);

    /// <summary>
    /// 布尔值转比例模式前景色
    /// </summary>
    public static IMultiValueConverter BoolToRatioForegroundConverter { get; } =
        new FuncMultiValueConverter<object?, IBrush>(values =>
        {
            var valueList = values.ToList();
            if (valueList.Count > 0 && valueList[0] is bool isChecked)
            {
                return isChecked
                    ? new SolidColorBrush(Color.Parse("#0078D4"))
                    : new SolidColorBrush(Color.Parse("#666666"));
            }
            return new SolidColorBrush(Color.Parse("#666666"));
        });

    /// <summary>
    /// 布尔值转可见性（反转）
    /// </summary>
    public static IValueConverter InverseBoolToVisibilityConverter { get; } =
        new FuncValueConverter<bool, bool>(value => !value);

    #endregion

    public GridDimensionMarker()
    {
        InitializeComponent();

        // 设置静态引用（用于XAML绑定）
        StaticLockState = this;

        // 获取控件引用
        _rowMarkers = this.FindControl<ItemsControl>("PART_RowMarkers");
        _columnMarkers = this.FindControl<ItemsControl>("PART_ColumnMarkers");
        _editOverlay = this.FindControl<Border>("PART_EditOverlay");
        _valueInput = this.FindControl<NumericUpDownEx>("PART_ValueInput");
        _lockButton = this.FindControl<ToggleButton>("PART_LockButton");
        _ratioButton = this.FindControl<ToggleButton>("PART_RatioButton");

        // 获取TranslateTransform引用
        var rootGrid = this.FindControl<Grid>("RootGrid");
        if (rootGrid?.RenderTransform is TranslateTransform transform)
        {
            _translateTransform = transform;
        }

        // 监听属性变化
        RowCountProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateMarkers());
        ColumnCountProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateMarkers());
        RowDefinitionsProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateMarkers());
        ColumnDefinitionsProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateMarkers());
        TotalWidthProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateMarkers());
        TotalHeightProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateMarkers());
        IsEditingProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) =>
        {
            if (sender._editOverlay != null && e.NewValue is bool isVisible)
                sender._editOverlay.IsVisible = isVisible;
        });

        // 浮动定位属性变化监听
        IsFloatingProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateFloatingPosition());
        TargetLeftProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateFloatingPosition());
        TargetTopProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateFloatingPosition());
        TargetWidthProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateFloatingPosition());
        TargetHeightProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateFloatingPosition());
        FloatingOffsetProperty.Changed.AddClassHandler<GridDimensionMarker>((sender, e) => sender.UpdateFloatingPosition());

        // 按钮事件
        if (_ratioButton != null)
        {
            _ratioButton.Click += OnRatioButtonClick;
            _ratioButton.Checked += OnRatioButtonChecked;
            _ratioButton.Unchecked += OnRatioButtonUnchecked;
        }

        if (_lockButton != null)
        {
            _lockButton.Checked += OnLockButtonChecked;
            _lockButton.Unchecked += OnLockButtonUnchecked;
        }

        if (_valueInput != null)
        {
            _valueInput.ValueChanged += OnValueChanged;
        }

        // 初始更新
        UpdateMarkers();
    }

    /// <summary>
    /// 锁定按钮图标点击（切换图标显示）
    /// </summary>
    private void OnLockButtonClick(object? sender, RoutedEventArgs e)
    {
        UpdateLockButtonIcons();
    }

    /// <summary>
    /// 锁定按钮选中状态变更
    /// </summary>
    private void OnLockButtonChecked(object? sender, RoutedEventArgs e)
    {
        IsLockChecked = true;
        UpdateLockButtonIcons();
    }

    /// <summary>
    /// 锁定按钮取消选中状态变更
    /// </summary>
    private void OnLockButtonUnchecked(object? sender, RoutedEventArgs e)
    {
        IsLockChecked = false;
        UpdateLockButtonIcons();
    }

    /// <summary>
    /// 更新锁定按钮图标显示
    /// </summary>
    private void UpdateLockButtonIcons()
    {
        if (_lockButton != null)
        {
            var panel = _lockButton.Content as Panel;
            if (panel != null)
            {
                var unlockedIcon = panel.Children.OfType<Avalonia.Controls.Shapes.Path>().FirstOrDefault(p => p.Name == "UnlockedIcon");
                var lockedIcon = panel.Children.OfType<Avalonia.Controls.Shapes.Path>().FirstOrDefault(p => p.Name == "LockedIcon");

                if (unlockedIcon != null)
                    unlockedIcon.IsVisible = !IsLockChecked;
                if (lockedIcon != null)
                    lockedIcon.IsVisible = IsLockChecked;
            }
        }
    }

    /// <summary>
    /// 比例按钮选中状态变更
    /// </summary>
    private void OnRatioButtonChecked(object? sender, RoutedEventArgs e)
    {
        IsRatioChecked = true;
        UpdateRatioButtonStyle();
    }

    /// <summary>
    /// 比例按钮取消选中状态变更
    /// </summary>
    private void OnRatioButtonUnchecked(object? sender, RoutedEventArgs e)
    {
        IsRatioChecked = false;
        UpdateRatioButtonStyle();
    }

    /// <summary>
    /// 更新比例按钮样式
    /// </summary>
    private void UpdateRatioButtonStyle()
    {
        if (_ratioButton != null && _ratioButton.Content is TextBlock textBlock)
        {
            textBlock.Foreground = IsRatioChecked
                ? new SolidColorBrush(Color.Parse("#0078D4"))
                : new SolidColorBrush(Color.Parse("#666666"));
        }
    }

    /// <summary>
    /// 更新标注显示
    /// </summary>
    private void UpdateMarkers()
    {
        UpdateRowMarkers();
        UpdateColumnMarkers();
        UpdateFloatingPosition();
    }

    /// <summary>
    /// 更新浮动定位
    /// </summary>
    private void UpdateFloatingPosition()
    {
        if (!IsFloating || _translateTransform == null)
            return;

        // 计算位置：将标注控件定位到目标Grid的左上方
        // 标注的左上角(24, 24)要与目标Grid的左上角(0, 0)对齐
        // 所以需要向左偏移24像素，向上偏移24像素
        double offsetX = -6 - PART_ColumnMarkers.Padding.Left - PART_ColumnMarkers.Bounds.Width + TargetLeft;
        double offsetY = 2 + -PART_ColumnMarkers.Padding.Top - PART_RowMarkers.Bounds.Height + TargetTop - FloatingOffset;

        _translateTransform.X = offsetX;
        _translateTransform.Y = offsetY;
    }

    /// <summary>
    /// 设置浮动定位到目标位置
    /// </summary>
    /// <param name="left">目标Grid的左边距</param>
    /// <param name="top">目标Grid的顶边距</param>
    /// <param name="width">目标Grid的宽度</param>
    /// <param name="height">目标Grid的高度</param>
    public void SetFloatingPosition(double left, double top, double width, double height)
    {
        TargetLeft = left;
        TargetTop = top;
        TargetWidth = width;
        TargetHeight = height;
        IsFloating = true;
        UpdateFloatingPosition();
    }

    /// <summary>
    /// 清除浮动定位
    /// </summary>
    public void ClearFloatingPosition()
    {
        IsFloating = false;
        if (_translateTransform != null)
        {
            _translateTransform.X = 0;
            _translateTransform.Y = 0;
        }
    }

    /// <summary>
    /// 更新行标注（顶部）
    /// </summary>
    private void UpdateRowMarkers()
    {
        if (_rowMarkers == null) return;

        var items = new List<DimensionMarkerItem>();
        var definitions = ParseDefinitions(RowDefinitions, RowCount);

        for (int i = 0; i < ColumnCount; i++)
        {
            var def = i < definitions.Count ? definitions[i] : "*";
            var width = CalculateSize(def, TotalWidth, ColumnCount);

            items.Add(new DimensionMarkerItem
            {
                Index = i,
                IsRow = false,
                Value = width,
                Definition = def,
                DisplayText = FormatDisplayText(def, width),
                IsRatio = def.EndsWith("*") && !def.TrimEnd('*').EndsWith("1"),
                IsLocked = def.Equals("Auto", StringComparison.OrdinalIgnoreCase) || double.TryParse(def, out _)
            });
        }

        _rowMarkers.ItemsSource = items;
    }

    /// <summary>
    /// 更新列标注（左侧）
    /// </summary>
    private void UpdateColumnMarkers()
    {
        if (_columnMarkers == null) return;

        var items = new List<DimensionMarkerItem>();
        var definitions = ParseDefinitions(ColumnDefinitions, ColumnCount);

        for (int i = 0; i < RowCount; i++)
        {
            var def = i < definitions.Count ? definitions[i] : "*";
            var height = CalculateSize(def, TotalHeight, RowCount);

            items.Add(new DimensionMarkerItem
            {
                Index = i,
                IsRow = true,
                Value = height,
                Definition = def,
                DisplayText = FormatDisplayText(def, height),
                IsRatio = def.EndsWith("*") && !def.TrimEnd('*').EndsWith("1"),
                IsLocked = def.Equals("Auto", StringComparison.OrdinalIgnoreCase) || double.TryParse(def, out _)
            });
        }

        _columnMarkers.ItemsSource = items;
    }

    /// <summary>
    /// 解析定义字符串
    /// </summary>
    private List<string> ParseDefinitions(string definition, int expectedCount)
    {
        var result = new List<string>();

        if (string.IsNullOrWhiteSpace(definition))
        {
            for (int i = 0; i < expectedCount; i++)
                result.Add("*");
            return result;
        }

        var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            result.Add(part.Trim());
        }

        while (result.Count < expectedCount)
            result.Add("*");
        while (result.Count > expectedCount)
            result.RemoveAt(result.Count - 1);

        return result;
    }

    /// <summary>
    /// 计算单个尺寸
    /// </summary>
    private double CalculateSize(string definition, double totalSize, int count)
    {
        if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
        {
            return 50;
        }

        if (double.TryParse(definition, out double pixelValue))
        {
            return pixelValue;
        }

        if (definition.EndsWith("*"))
        {
            var ratioStr = definition.TrimEnd('*').Trim();
            double ratio = string.IsNullOrEmpty(ratioStr) ? 1 : double.TryParse(ratioStr, out double r) ? r : 1;

            var allDefs = definition.Contains(",") ? RowDefinitions : ColumnDefinitions;
            double totalRatio = 0;
            var defs = ParseDefinitions(allDefs, count);
            foreach (var d in defs)
            {
                if (d.EndsWith("*"))
                {
                    var rStr = d.TrimEnd('*').Trim();
                    totalRatio += string.IsNullOrEmpty(rStr) ? 1 : double.TryParse(rStr, out double r2) ? r2 : 1;
                }
                else if (!double.TryParse(d, out _) && !d.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                {
                    totalRatio += 1;
                }
            }

            if (totalRatio > 0)
            {
                return (ratio / totalRatio) * totalSize;
            }
        }

        return totalSize / count;
    }

    /// <summary>
    /// 格式化显示文本
    /// </summary>
    private string FormatDisplayText(string definition, double pixelValue)
    {
        if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            return "Auto";

        if (double.TryParse(definition, out _))
            return $"{pixelValue:F0}";

        if (definition.EndsWith("*"))
        {
            var ratioStr = definition.TrimEnd('*').Trim();
            if (string.IsNullOrEmpty(ratioStr))
                return $"* ({pixelValue:F0})";
            return $"{definition} ({pixelValue:F0})";
        }

        return $"{pixelValue:F0}";
    }

    /// <summary>
    /// 比例按钮点击
    /// </summary>
    private void OnRatioButtonClick(object? sender, RoutedEventArgs e)
    {
        if (_ratioButton != null)
        {
        }
    }

    /// <summary>
    /// 数值变更
    /// </summary>
    private void OnValueChanged(object? sender, double e)
    {
        if (_ratioButton?.IsChecked == true)
        {
        }
        else
        {
        }
    }

    /// <summary>
    /// 更新特定行的定义
    /// </summary>
    public void UpdateRowDefinition(int index, string definition)
    {
        var defs = ParseDefinitions(RowDefinitions, ColumnCount);
        if (index >= 0 && index < defs.Count)
        {
            defs[index] = definition;
            RowDefinitions = string.Join(",", defs);
        }
    }

    /// <summary>
    /// 更新特定列的定义
    /// </summary>
    public void UpdateColumnDefinition(int index, string definition)
    {
        var defs = ParseDefinitions(ColumnDefinitions, RowCount);
        if (index >= 0 && index < defs.Count)
        {
            defs[index] = definition;
            ColumnDefinitions = string.Join(",", defs);
        }
    }

    /// <summary>
    /// 选中指定索引
    /// </summary>
    public void SelectIndex(bool isRow, int index)
    {
        IndexSelected?.Invoke(this, (isRow, index));
    }
}

/// <summary>
/// 单个尺寸标注项
/// </summary>
public class DimensionMarkerItem
{
    public int Index { get; set; }
    public bool IsRow { get; set; }
    public double Value { get; set; }
    public string Definition { get; set; } = "*";
    public string DisplayText { get; set; } = "*";
    public bool IsRatio { get; set; }
    public bool IsLocked { get; set; }
}
