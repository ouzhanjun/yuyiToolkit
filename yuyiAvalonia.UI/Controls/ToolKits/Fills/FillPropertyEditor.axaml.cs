using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using yuyiAvalonia.UI.Converters;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class FillPropertyEditor : UserControl
{
    #region FillType Converters

    public static readonly IValueConverter FillTypeSolidConverter = new FillTypeBoolConverter(FillType.Solid);
    public static readonly IValueConverter FillTypeGradientConverter = new FillTypeBoolConverter(FillType.Gradient);
    public static readonly IValueConverter FillTypeImageConverter = new FillTypeBoolConverter(FillType.Image);
    public static readonly IValueConverter FillTypeNoneConverter = new FillTypeBoolConverter(FillType.None);

    /// <summary>
    /// 第一行（详情）：纯色填充时显示 BackgroundColor 颜色值（例如 "#FF0000"），其它类型显示空。
    /// </summary>
    public static readonly IMultiValueConverter FillTypeSolidTextConverter = new FillTypeMultiTextConverter(FillType.Solid, fi => fi?.BackgroundColor ?? string.Empty);

    /// <summary>
    /// 第一行（详情）：渐变填充时显示渐变类型名称（线性/径向/角度/菱形），其它类型显示空。
    /// </summary>
    public static readonly IMultiValueConverter FillTypeGradientTextConverter = new FillTypeMultiTextConverter(FillType.Gradient, fi =>
    {
        if (fi == null) return string.Empty;
        return fi.GradientKind switch
        {
            GradientKind.Linear => "线性",
            GradientKind.Radial => "径向",
            GradientKind.Angle => "角度",
            GradientKind.Diamond => "菱形",
            _ => string.Empty
        };
    });

    #endregion

    #region StyledProperties

    /// <summary>
    /// 当前编辑的单个 FillItem（元素属性面板绑定 ElementProperty.BackgroundFill 使用）。
    /// </summary>
    public static readonly StyledProperty<FillItem?> FillProperty =
        AvaloniaProperty.Register<FillPropertyEditor, FillItem?>(nameof(Fill));

    public FillItem? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly StyledProperty<bool> IsFillEditorOpenProperty =
        AvaloniaProperty.Register<FillPropertyEditor, bool>(nameof(IsFillEditorOpen));

    public bool IsFillEditorOpen
    {
        get => GetValue(IsFillEditorOpenProperty);
        set => SetValue(IsFillEditorOpenProperty, value);
    }

    /// <summary>
    /// 当前编辑的单个 FillItem（元素属性面板绑定 ElementProperty.BackgroundFill 使用）。
    /// </summary>
    public static readonly StyledProperty<int> OpacityPercentProperty =
        AvaloniaProperty.Register<FillPropertyEditor, int>(nameof(OpacityPercent));

    public int OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }
    #endregion

    public event Action? RefreshControl;

    private FillItem? _subscribedFill;

    public FillPropertyEditor()
    {
        InitializeComponent();

        IsFillEditorOpen = false;

        // 拖动填充编辑器弹窗
        DragHost.PointerPressed += OnPointerPressed;
        DragHost.PointerMoved += OnPointerMoved;
        DragHost.PointerReleased += OnPointerReleased;

        OpacityPercentProperty.Changed.AddClassHandler<FillPropertyEditor>((sender, e) =>
        {
            if (sender is not FillPropertyEditor editor)
                return;

            if (e.NewValue != null)
            {
                double opacity = (int)e.NewValue / 100.0;
                if (Math.Abs(opacity - editor.Fill!.Opacity) > 0.001)
                {
                    editor.Fill!.Opacity = opacity;
                }
            }
        });

        FillProperty.Changed.AddClassHandler<FillPropertyEditor>((sender, e) =>
            sender.OnFillChanged(e.OldValue as FillItem, e.NewValue as FillItem));

        Loaded += (_, _) =>
        {
            // 让子编辑器（Solid/Gradient/Image）的 StopOffsetChanged 等通知也能向上抛
            if (GradientFillPropertyEditor != null)
            {
                GradientFillPropertyEditor.StopOffsetChanged += () => RefreshControl?.Invoke();
            }
        };
    }

    private void OnFillChanged(FillItem? oldFill, FillItem? newFill)
    {
        if (oldFill != null)
        {
            oldFill.PropertyChanged -= OnFillPropertyChanged;
            UnsubscribeFromStops(oldFill.GradientStops);
        }

        _subscribedFill = newFill;

        if (newFill != null)
        {
            this.OpacityPercent = (int)(newFill!.Opacity * 100);
            newFill.PropertyChanged += OnFillPropertyChanged;
            SubscribeToStops(newFill.GradientStops);
        }
    }

    private void OnFillPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var name = e.PropertyName;
        switch (name)
        {
            case nameof(FillItem.GradientStops):
                UnsubscribeFromStops(_subscribedFill?.GradientStops);
                SubscribeToStops(_subscribedFill?.GradientStops);
                break;
        }
        RefreshControl?.Invoke();
    }

    private void SubscribeToStops(System.Collections.ObjectModel.ObservableCollection<GradientStopInfo>? stops)
    {
        if (stops == null) return;
        stops.CollectionChanged += OnStopsCollectionChanged;
        // 已经存在的停止点也要监听其属性变化
        foreach (var stop in stops)
        {
            stop.PropertyChanged += OnStopPropertyChanged;
        }
    }

    private void UnsubscribeFromStops(System.Collections.ObjectModel.ObservableCollection<GradientStopInfo>? stops)
    {
        if (stops == null) return;
        stops.CollectionChanged -= OnStopsCollectionChanged;
        foreach (var stop in stops)
        {
            stop.PropertyChanged -= OnStopPropertyChanged;
        }
    }

    private void OnStopsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is GradientStopInfo stop)
                    stop.PropertyChanged -= OnStopPropertyChanged;
            }
        }

        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is GradientStopInfo stop)
                    stop.PropertyChanged += OnStopPropertyChanged;
            }
        }

        RefreshControl?.Invoke();
    }

    private void OnStopPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshControl?.Invoke();
    }

    #region 拖动弹窗

    private bool _isDragging;
    private Point _lastPos;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            _isDragging = true;
            _lastPos = e.GetPosition(this);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastPos;

            FillEditorPopup.HorizontalOffset += delta.X;
            FillEditorPopup.VerticalOffset += delta.Y;

            _lastPos = pos;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    #endregion

    #region 事件处理

    private void FillItem_Click(object? sender, PointerPressedEventArgs e)
    {
        if (Fill == null) return;

        if (sender is Border border)
        {
            FillEditorPopup.HorizontalOffset = 0;
            FillEditorPopup.VerticalOffset = 0;
            FillEditorPopup.PlacementTarget = ((Control?)border.Parent)!.Parent as Control;
            IsFillEditorOpen = true;
        }
    }

    private void SelectFillType_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is FillType kind)
        {
            if (Fill != null)
            {
                Fill.FillType = kind;
            }
        }
    }

    private void CloseFillEditor_Click(object? sender, RoutedEventArgs e)
    {
        IsFillEditorOpen = false;
    }

    #endregion
}

/// <summary>
/// 多值转换器：根据 <see cref="FillItem.FillType"/> 输出不同的文本内容（颜色值 / 渐变类型 等）。
/// XAML 用法：MultiBinding 第一项为 FillType，第二项为 FillItem。
/// 只能匹配指定 <see cref="FillType"/>，否则输出空字符串，方便在 XAML 中与可见性绑定配合使用。
/// </summary>
internal class FillTypeMultiTextConverter : IMultiValueConverter
{
    private readonly FillType _kind;
    private readonly Func<FillItem?, string> _textSelector;

    public FillTypeMultiTextConverter(FillType kind, Func<FillItem?, string> textSelector)
    {
        _kind = kind;
        _textSelector = textSelector;
    }

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && values[0] is FillType k && k == _kind && values[1] is FillItem fi)
            return _textSelector(fi);
        return string.Empty;
    }
}
