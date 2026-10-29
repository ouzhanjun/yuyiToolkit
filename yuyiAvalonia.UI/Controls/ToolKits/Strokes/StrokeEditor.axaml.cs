using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using yuyiAvalonia.Base.Controls;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.UI.Converters;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class StrokeEditor : UserControl
{
    #region 弹窗

    public static readonly StyledProperty<bool> IsAddPopupOpenProperty =
        AvaloniaProperty.Register<StrokeEditor, bool>(nameof(IsAddPopupOpen));

    public bool IsAddPopupOpen
    {
        get => GetValue(IsAddPopupOpenProperty);
        set => SetValue(IsAddPopupOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsFillEditorOpenProperty =
        AvaloniaProperty.Register<StrokeEditor, bool>(nameof(IsFillEditorOpen));

    public bool IsFillEditorOpen
    {
        get => GetValue(IsFillEditorOpenProperty);
        set => SetValue(IsFillEditorOpenProperty, value);
    }

    #endregion

    #region StyledProperties

    public static readonly StyledProperty<ObservableCollection<FillItem>> StrokeFillsProperty =
        AvaloniaProperty.Register<StrokeEditor, ObservableCollection<FillItem>>(nameof(StrokeFills));

    public ObservableCollection<FillItem> StrokeFills
    {
        get => GetValue(StrokeFillsProperty);
        set => SetValue(StrokeFillsProperty, value);
    }

    // 顶层描边填充（与 FillEditor.BackgroundFill 镜像），用于 StrokeEditor 标题行的 FillPropertyEditor 预览卡。
    public static readonly StyledProperty<FillItem?> StrokeFillProperty =
        AvaloniaProperty.Register<StrokeEditor, FillItem?>(nameof(StrokeFill));

    public FillItem? StrokeFill
    {
        get => GetValue(StrokeFillProperty);
        set => SetValue(StrokeFillProperty, value);
    }

    public static readonly StyledProperty<FillItem?> SelectedFillProperty =
        AvaloniaProperty.Register<StrokeEditor, FillItem?>(nameof(SelectedFill));

    public FillItem? SelectedFill
    {
        get => GetValue(SelectedFillProperty);
        set => SetValue(SelectedFillProperty, value);
    }

    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<StrokeEditor, double>(
            nameof(Opacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }

    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<StrokeEditor, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region Stroke 相关属性

    public static readonly StyledProperty<StrokeType?> StrokeTypeProperty =
        AvaloniaProperty.Register<StrokeEditor, StrokeType?>(
            nameof(StrokeType),
            defaultValue: Models.StrokeType.Solid,
            defaultBindingMode: BindingMode.TwoWay);

    public StrokeType? StrokeType
    {
        get => GetValue(StrokeTypeProperty);
        set => SetValue(StrokeTypeProperty, value);
    }

    public static readonly StyledProperty<StrokePosition?> StrokePositionProperty =
        AvaloniaProperty.Register<StrokeEditor, StrokePosition?>(
            nameof(StrokePosition),
            defaultValue: Models.StrokePosition.Center,
            defaultBindingMode: BindingMode.TwoWay);

    public StrokePosition? StrokePosition
    {
        get => GetValue(StrokePositionProperty);
        set => SetValue(StrokePositionProperty, value);
    }

    public static readonly StyledProperty<StrokeStyleType?> StrokeStyleProperty =
        AvaloniaProperty.Register<StrokeEditor, StrokeStyleType?>(
            nameof(StrokeStyle),
            defaultValue: Models.StrokeStyleType.Solid,
            defaultBindingMode: BindingMode.TwoWay);

    public StrokeStyleType? StrokeStyle
    {
        get => GetValue(StrokeStyleProperty);
        set => SetValue(StrokeStyleProperty, value);
    }

    public static readonly StyledProperty<StrokeEndpoints?> StrokeEndpointProperty =
        AvaloniaProperty.Register<StrokeEditor, StrokeEndpoints?>(
            nameof(StrokeEndpoint),
            defaultValue: Models.StrokeEndpoints.Butt,
            defaultBindingMode: BindingMode.TwoWay);

    public StrokeEndpoints? StrokeEndpoint
    {
        get => GetValue(StrokeEndpointProperty);
        set => SetValue(StrokeEndpointProperty, value);
    }

    public static readonly StyledProperty<StrokeJoin?> StrokeJoinProperty =
        AvaloniaProperty.Register<StrokeEditor, StrokeJoin?>(
            nameof(StrokeJoin),
            defaultValue: Models.StrokeJoin.Miter,
            defaultBindingMode: BindingMode.TwoWay);

    public StrokeJoin? StrokeJoin
    {
        get => GetValue(StrokeJoinProperty);
        set => SetValue(StrokeJoinProperty, value);
    }

    public static readonly StyledProperty<double> StrokeMiterLimitProperty =
        AvaloniaProperty.Register<StrokeEditor, double>(
            nameof(StrokeMiterLimit),
            defaultValue: 4.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double StrokeMiterLimit
    {
        get => GetValue(StrokeMiterLimitProperty);
        set => SetValue(StrokeMiterLimitProperty, value);
    }

    public static readonly StyledProperty<string?> StrokeDashArrayStringProperty =
        AvaloniaProperty.Register<StrokeEditor, string?>(
            nameof(StrokeDashArrayString),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string? StrokeDashArrayString
    {
        get => GetValue(StrokeDashArrayStringProperty);
        set => SetValue(StrokeDashArrayStringProperty, value);
    }

    public static readonly StyledProperty<double?> StrokeDashOffsetProperty =
        AvaloniaProperty.Register<StrokeEditor, double?>(
            nameof(StrokeDashOffset),
            defaultValue: 1,
            defaultBindingMode: BindingMode.TwoWay);

    public double? StrokeDashOffset
    {
        get => GetValue(StrokeDashOffsetProperty);
        set => SetValue(StrokeDashOffsetProperty, value);
    }


    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<StrokeEditor, double>(
            nameof(StrokeWidth),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public static readonly StyledProperty<Models.StrokeFillType> StrokeFillTypeProperty =
        AvaloniaProperty.Register<StrokeEditor, Models.StrokeFillType>(
            nameof(StrokeFillType),
            defaultValue: Models.StrokeFillType.Solid,
            defaultBindingMode: BindingMode.TwoWay);

    public Models.StrokeFillType StrokeFillType
    {
        get => GetValue(StrokeFillTypeProperty);
        set => SetValue(StrokeFillTypeProperty, value);
    }

    // StrokeFillType 的 int 索引，供 ComboBox SelectedIndex 绑定使用
    public static readonly StyledProperty<int> StrokeFillTypeIndexProperty =
        AvaloniaProperty.Register<StrokeEditor, int>(
            nameof(StrokeFillTypeIndex),
            defaultValue: 1,
            defaultBindingMode: BindingMode.TwoWay);

    public int StrokeFillTypeIndex
    {
        get => GetValue(StrokeFillTypeIndexProperty);
        set => SetValue(StrokeFillTypeIndexProperty, value);
    }

    public static readonly StyledProperty<string> StrokeColorProperty =
        AvaloniaProperty.Register<StrokeEditor, string>(
            nameof(StrokeColor),
            defaultValue: "#333333",
            defaultBindingMode: BindingMode.TwoWay);

    public string StrokeColor
    {
        get => GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    #endregion

    #region StrokeStyle 映射

    /// <summary>
    /// 描边样式（虚线/点线等）对应的 <c>DashArray</c> 字符串查找表。由
    /// <see cref="PathHelper.GetStrokeStyleItems"/> 在类初始化时构建，
    /// 用于在下拉选中等场景把 <see cref="StrokeStyle"/> 同步到 <see cref="StrokeDashArrayString"/>。
    /// </summary>
    private static readonly Dictionary<StrokeStyleType, string> StrokeStyleDashMap =
        PathHelper.GetStrokeStyleItems().ToDictionary(x => x.Value, x => x.DashArray);

    /// <summary>
    /// 由 <see cref="StrokeStyle"/> 同步 <see cref="StrokeDashArrayString"/>。
    /// 当 <paramref name="onlyFillEmpty"/> 为 true 时仅在 DashArray 空白时才回写，
    /// 避免覆盖用户在"虚线参数"输入的自定义值。
    /// </summary>
    private void SyncDashArrayFromStyle(bool onlyFillEmpty)
    {
        if (StrokeStyle is not StrokeStyleType style)
            return;

        if (!StrokeStyleDashMap.TryGetValue(style, out var dash))
            return;

        if (onlyFillEmpty)
        {
            if (string.IsNullOrWhiteSpace(StrokeDashArrayString))
                StrokeDashArrayString = dash;
        }
        else if (!string.Equals(StrokeDashArrayString, dash, StringComparison.Ordinal))
        {
            StrokeDashArrayString = dash;
        }
    }

    #endregion

    #region StrokeTypeItemsSource

    public static readonly StyledProperty<IList<StrokeType>> StrokeTypeItemsSourceProperty =
        AvaloniaProperty.Register<StrokeEditor, IList<StrokeType>>(
            nameof(StrokeTypeItemsSource),
            defaultValue: new List<StrokeType>
            {
                Models.StrokeType.Solid,
                Models.StrokeType.Dashed,
                Models. StrokeType.Custom
            });

    public IList<StrokeType> StrokeTypeItemsSource
    {
        get => GetValue(StrokeTypeItemsSourceProperty);
        set => SetValue(StrokeTypeItemsSourceProperty, value);
    }

    #endregion

    #region StrokePositionItemsSource

    public static readonly StyledProperty<IList<IconTextDropDownItem>> StrokePositionItemsSourceProperty =
        AvaloniaProperty.Register<StrokeEditor, IList<IconTextDropDownItem>>(
            nameof(StrokePositionItemsSource),
            defaultValue: CreateStrokePositionItems());

    public IList<IconTextDropDownItem> StrokePositionItemsSource
    {
        get => GetValue(StrokePositionItemsSourceProperty);
        set => SetValue(StrokePositionItemsSourceProperty, value);
    }

    private static IList<IconTextDropDownItem> CreateStrokePositionItems() =>
        new List<IconTextDropDownItem>
        {
            new(Models.StrokePosition.Center, "居中", Icon.StrokePositionCenter, "描边沿路径中心展开"),
            new(Models.StrokePosition.Outside, "外部", Icon.StrokePositionOuter, "描边位于路径外侧"),
            new(Models.StrokePosition.Inside, "内部", Icon.StrokePositionInner, "描边位于路径内侧")
        };

    #endregion

    #region StrokeType Converters

    // StrokeType -> bool：true 表示"非实线"（Dashed / Custom），用于控制虚线参数 / 颜色等区块的可见性。
    // 任何 StrokeType 变化（Solid / Dashed / Custom）都会让 IsVisible 重新计算，控件自动显隐。
    public static readonly IValueConverter StrokeTypeNotSolidConverter = new StrokeTypeNotSolidBoolConverter();

    // 仅 StrokeType == Dashed 时为 true（"线条样式"下拉只在虚线时显示）。
    public static readonly IValueConverter StrokeTypeDashedConverter = new StrokeTypeKindBoolConverter(Models.StrokeType.Dashed);

    // 仅 StrokeType == Custom 时为 true（"虚线参数"输入框只在自定义时显示）。
    public static readonly IValueConverter StrokeTypeCustomConverter = new StrokeTypeKindBoolConverter(Models.StrokeType.Custom);

    private class StrokeTypeNotSolidBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is StrokeType k)
                return k == Models.StrokeType.Dashed || k == Models.StrokeType.Custom;
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    private class StrokeTypeKindBoolConverter : IValueConverter
    {
        private readonly StrokeType _kind;
        public StrokeTypeKindBoolConverter(StrokeType kind) => _kind = kind;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is StrokeType k) return k == _kind;
            return false;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    #endregion

    public event Action? RefreshControl;

    public StrokeEditor()
    {
        InitializeComponent();

        IsAddPopupOpen = false;
        IsFillEditorOpen = false;

        //线条类型
        PART_StrokeType.ItemsSource = new List<Models.StrokeType>
        {
            Models. StrokeType.Solid,
            Models.StrokeType.Dashed,
            Models.StrokeType.Custom,
        }.Select(x => (object)x).ToList();

        //线条线帽
        PART_EndCap.ItemsSource = new List<Models.StrokeEndpoints>
        {
            Models.StrokeEndpoints.Butt,
            Models.StrokeEndpoints.Round,
            Models. StrokeEndpoints.Square
        }.Select(x => (object)x).ToList();

        //线条连接
        PART_LineJoin.ItemsSource = new List<Models.StrokeJoin>
        {
            Models.StrokeJoin.Miter,
            Models.StrokeJoin.Round,
            Models. StrokeJoin.Bevel
        }.Select(x => (object)x).ToList();

        // 用户在下拉中选了某个预设 → 同步 DashArrayString。
        StrokeStyleProperty.Changed.AddClassHandler<StrokeEditor>((sender, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (sender is not StrokeEditor ed || ed.StrokeType != Models.StrokeType.Dashed)
                    return;
                ed.SyncDashArrayFromStyle(onlyFillEmpty: false);
            });
        });

        StrokeTypeItemsSourceProperty.Changed.AddClassHandler<StrokeEditor>((sender, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (e.NewValue != null && StrokeTypeItemsSource != null && StrokeTypeItemsSource.Count > 0)
                {
                    StrokeType = StrokeTypeItemsSource[0];
                }
            });
        });

        StrokeTypeProperty.Changed.AddClassHandler<StrokeEditor>((sender, e) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (sender is not StrokeEditor ed) return;
                switch (ed.StrokeType)
                {
                    case Models.StrokeType.Dashed:
                        PART_StrokeStyle.IsEnabled = true;
                        // 仅在 DashArray 空白时按当前 StrokeStyle 强制回写，避免覆盖"虚线参数"输入。
                        ed.SyncDashArrayFromStyle(onlyFillEmpty: true);
                        break;
                    case Models.StrokeType.Solid:

                        PART_StrokeStyle.IsEnabled = false;
                        break;
                    case Models.StrokeType.Custom:
                        if (string.IsNullOrWhiteSpace(ed.StrokeDashArrayString))
                        {
                            ed.StrokeDashArrayString = "1,1";
                        }
                        PART_StrokeStyle.IsEnabled = false;
                        break;
                }
            });
        });


        IsFillEditorOpenProperty.Changed.AddClassHandler<StrokeEditor>((sender, e) =>
        {
            if (e.NewValue is false)
                sender.SelectedFill = null;
        });

        StrokeFillsProperty.Changed.AddClassHandler<StrokeEditor>((sender, e) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (StrokeFills == null)
                    return;

                StrokeFills.CollectionChanged += (s, e) =>
                {
                    if (SelectedFill != null && !StrokeFills.Contains(SelectedFill))
                        SelectedFill = null;
                };

                if (FillItemsCtrl != null)
                {
                    FillItemsCtrl.FillItemClicked -= OnFillItemsCtrlItemClicked;
                    FillItemsCtrl.FillItemClicked += OnFillItemsCtrlItemClicked;
                    FillItemsCtrl.RemoveRequested -= OnFillItemsCtrlRemoveRequested;
                    FillItemsCtrl.RemoveRequested += OnFillItemsCtrlRemoveRequested;
                    FillItemsCtrl.MoveUpRequested -= OnFillItemsCtrlMoveUpRequested;
                    FillItemsCtrl.MoveUpRequested += OnFillItemsCtrlMoveUpRequested;
                    FillItemsCtrl.VisibilityToggled -= OnFillItemsCtrlVisibilityToggled;
                    FillItemsCtrl.VisibilityToggled += OnFillItemsCtrlVisibilityToggled;
                }
            });
        });

        SelectedFillProperty.Changed.AddClassHandler<StrokeEditor>((sender, e) =>
        {
            if (e.OldValue is FillItem oldFill)
            {
                oldFill.PropertyChanged -= sender.OnSelectedFillPropertyChanged;
            }

            if (e.NewValue is FillItem newFill)
            {
                newFill.PropertyChanged += sender.OnSelectedFillPropertyChanged;
            }
        });

        GradientFillPropertyEditor.StopOffsetChanged += () =>
        {
            RefreshControl?.Invoke();
        };

        // FillOpacityProperty 变化时同步 OpacityPercent（用于外部绑定如 ElementProperty.FillOpacity）
        OpacityProperty.Changed.AddClassHandler<StrokeEditor>((control, args) =>
        {
            if (args.NewValue is double opacity)
            {
                var newPercent = opacity * 100;

                if (Math.Abs(newPercent - control.OpacityPercent) > 0.01)
                {
                    control.SetValue(OpacityPercentProperty, newPercent);
                }
            }
        });

        // OpacityPercent 变化时同步 FillOpacity
        OpacityPercentProperty.Changed.AddClassHandler<StrokeEditor>((control, args) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (args.NewValue is double percent)
                {
                    var newOpacity = percent / 100.0;
                    if (Math.Abs(newOpacity - control.Opacity) > 0.001)
                    {
                        control.SetValue(OpacityProperty, newOpacity);
                    }
                }
            });
        });

        this.Loaded += (object? sender, RoutedEventArgs e) =>
        {
            // 面板切换后重新进入：若 DashArray 空白则按当前 StrokeStyle 还原。
            SyncDashArrayFromStyle(onlyFillEmpty: true);
        };

        this.PropertyChanged += (object? sender, AvaloniaPropertyChangedEventArgs e) =>
        {
            RefreshControl?.Invoke();
        };

        DragHost.PointerPressed += OnPointerPressed;
        DragHost.PointerMoved += OnPointerMoved;
        DragHost.PointerReleased += OnPointerReleased;

        FillEditorPopup.Opened += FillEditorPopup_Opened;
    }

    #region 拖动弹窗

    private bool _isDragging;
    private Point _lastPos;

    // 鼠标按下：开始拖动
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            if (e.GetPosition((Visual)sender!).Y < 40)
            {
                _isDragging = true;
                _lastPos = e.GetPosition(this);
                e.Handled = true;
            }
        }
    }

    // 鼠标移动：更新 Popup 位置
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastPos;

            // 让 Popup 跟着鼠标走
            FillEditorPopup.HorizontalOffset += delta.X;
            FillEditorPopup.VerticalOffset += delta.Y;

            _lastPos = pos;
            e.Handled = true;
        }
    }

    // 鼠标松开：结束拖动
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    private void FillEditorPopup_Opened(object? sender, EventArgs e)
    {
        FillEditorPopup.HorizontalOffset = 0;
        FillEditorPopup.VerticalOffset = 0;
    }

    #endregion

    private void OnSelectedFillPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshControl?.Invoke();
    }

    #region Event Handlers

    private void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = !IsAddPopupOpen;
    }

    private void AddSolidFill_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = false;
        AddFill(FillType.Solid);
    }

    private void AddGradientFill_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = false;
        AddFill(FillType.Gradient);
    }

    private void AddImageFill_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = false;
        AddFill(FillType.Image);
    }

    private void AddFill(FillType kind)
    {
        if (StrokeFills == null) return;

        var fill = new FillItem
        {
            IsEnabled = true,
            FillType = kind,
            Name = GetDefaultFillName(kind)
        };

        if (kind == FillType.Solid)
        {
            fill.BackgroundColor = "#3DCFEF";
            fill.Opacity = 1.0;
        }
        else if (kind == FillType.Gradient)
        {
            fill.GradientKind = GradientKind.Linear;
            fill.GradientStartColor = "#FF6B6B";
            fill.GradientEndColor = "#4ECDC4";
            fill.GradientAngle = 0;
            fill.Opacity = 1.0;
            fill.GradientStops = new ObservableCollection<GradientStopInfo>
            {
                new GradientStopInfo(0.0, "#FF6B6B"),
                new GradientStopInfo(1.0, "#4ECDC4")
            };
        }
        else if (kind == FillType.Image)
        {
            fill.ImageStretchIndex = 3;
            fill.Opacity = 1.0;
        }

        StrokeFills.Add(fill);
        SelectedFill = fill;
        IsFillEditorOpen = false;
        RefreshControl?.Invoke();
    }

    private string GetDefaultFillName(FillType kind)
    {
        var baseName = kind switch
        {
            FillType.Solid => "纯色",
            FillType.Gradient => "渐变",
            FillType.Image => "图片",
            _ => "填充"
        };

        if (StrokeFills == null) 
            return baseName;
        var count = StrokeFills.Count(f => f.FillType == kind) + 1;
        return $"{baseName} {count}";
    }

    private void OnFillItemsCtrlRemoveRequested(FillItem fill)
    {
        if (StrokeFills == null) return;
        var index = StrokeFills.IndexOf(fill);
        var wasSelected = SelectedFill == fill;

        StrokeFills.Remove(fill);

        if (wasSelected)
        {
            SelectedFill = StrokeFills.Count > 0 ? StrokeFills[Math.Min(index, StrokeFills.Count - 1)] : null;
            // 删除的是当前正在编辑的项时关闭编辑器弹窗
            IsFillEditorOpen = false;
        }
        RefreshControl?.Invoke();
    }

    private void OnFillItemsCtrlMoveUpRequested(FillItem fill)
    {
        if (StrokeFills == null) return;
        var index = StrokeFills.IndexOf(fill);
        if (index > 0)
            StrokeFills.Move(index, index - 1);
    }

    private void OnFillItemsCtrlItemClicked(FillItem fill)
    {
        FillEditorPopup.PlacementTarget = FillItemsCtrl;
        SelectedFill = fill;
        IsAddPopupOpen = false;
        IsFillEditorOpen = true;
    }

    private void OnFillItemsCtrlVisibilityToggled(FillItem fill)
    {
        RefreshControl?.Invoke();
    }

    private void SelectFillType_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is FillType kind)
        {
            if (SelectedFill != null)
            {
                SelectedFill.FillType = kind;
            }
        }
    }

    private void CloseFillEditor_Click(object? sender, RoutedEventArgs e)
    {
        IsFillEditorOpen = false;
    }

    #endregion
}
