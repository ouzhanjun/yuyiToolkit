using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using yuyiAvalonia.Base.Controls;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public class ColorToBrushConverter : IValueConverter
{
    public static readonly ColorToBrushConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is string colorStr && Color.TryParse(colorStr, out var color))
            return new SolidColorBrush(color);
        return Brushes.Transparent;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
}


public class GradientKindRadialConverter : IValueConverter
{
    public static readonly GradientKindRadialConverter Instance = new();
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is GradientKind kind && kind == GradientKind.Radial;
    }
    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
}

public partial class GradientFillPropertyEditor : UserControl
{
    #region Styled Properties

    public static readonly StyledProperty<double> GradientAngleProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, double>(nameof(GradientAngle), defaultValue: 0.0, defaultBindingMode: BindingMode.TwoWay);
    public double GradientAngle { get => GetValue(GradientAngleProperty); set => SetValue(GradientAngleProperty, value); }

    public static readonly StyledProperty<double> RadialCenterXProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, double>(nameof(RadialCenterX), defaultValue: 0.5, defaultBindingMode: BindingMode.TwoWay);
    public double RadialCenterX { get => GetValue(RadialCenterXProperty); set => SetValue(RadialCenterXProperty, value); }

    public static readonly StyledProperty<double> RadialCenterYProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, double>(nameof(RadialCenterY), defaultValue: 0.5, defaultBindingMode: BindingMode.TwoWay);
    public double RadialCenterY { get => GetValue(RadialCenterYProperty); set => SetValue(RadialCenterYProperty, value); }

    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, double>(nameof(Opacity), defaultValue: 1.0, defaultBindingMode: BindingMode.TwoWay);
    public double Opacity { get => GetValue(OpacityProperty); set => SetValue(OpacityProperty, value); }

    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, double>(nameof(OpacityPercent), defaultValue: 100.0, defaultBindingMode: BindingMode.TwoWay);
    public double OpacityPercent { get => GetValue(OpacityPercentProperty); set => SetValue(OpacityPercentProperty, value); }

    public static readonly StyledProperty<ObservableCollection<GradientStopInfo>?> GradientStopsProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, ObservableCollection<GradientStopInfo>?>(nameof(GradientStops), defaultBindingMode: BindingMode.TwoWay);
    public ObservableCollection<GradientStopInfo>? GradientStops { get => GetValue(GradientStopsProperty); set => SetValue(GradientStopsProperty, value); }

    public static readonly StyledProperty<GradientKind> GradientKindEnumProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, GradientKind>(nameof(GradientKindEnum), defaultValue: GradientKind.Linear, defaultBindingMode: BindingMode.TwoWay);
    public GradientKind GradientKindEnum { get => GetValue(GradientKindEnumProperty); set => SetValue(GradientKindEnumProperty, value); }

    public bool IsGradientLinearOrAngle => GradientKindEnum == GradientKind.Linear || GradientKindEnum == GradientKind.Angle;

    public static readonly StyledProperty<int> GradientKindIndexProperty =
        AvaloniaProperty.Register<GradientFillPropertyEditor, int>(nameof(GradientKindIndex), defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);
    public int GradientKindIndex { get => GetValue(GradientKindIndexProperty); set => SetValue(GradientKindIndexProperty, value); }

    #endregion

    private GradientStopInfo? _activeStopColor;

    public event Action? StopOffsetChanged;

    /// <summary>
    /// 标记点（停止点）控件与对应 GradientStopInfo 的映射表
    /// </summary>
    private readonly Dictionary<GradientStopInfo, GradientStopMarker> _stopMarkers = new();

    public static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<ColorInput, bool>(nameof(IsPickerOpen), false);

    public bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    public GradientFillPropertyEditor()
    {
        InitializeComponent();

        GradientKindEnumProperty.Changed.AddClassHandler<GradientFillPropertyEditor>((sender, e) =>
        {
            sender.UpdateGradientPreview();
            sender.UpdateGradientKindState();
            // 同步 EnumComboBox
            if (sender.PART_GradientKindComboBox is EnumComboBox kindCombo && e.NewValue is GradientKind kind)
            {
                kindCombo.SelectedValue = kind.ToString();
            }
        });

        GradientKindIndexProperty.Changed.AddClassHandler<GradientFillPropertyEditor>((sender, e) =>
        {
            if (e.NewValue is int index)
                sender.GradientKindEnum = (GradientKind)index;
        });

        OpacityProperty.Changed.AddClassHandler<GradientFillPropertyEditor>((control, args) =>
        {
            if (args.NewValue is double opacity)
            {
                var newPercent = opacity * 100;
                if (Math.Abs(newPercent - control.OpacityPercent) > 0.01)
                    control.SetValue(OpacityPercentProperty, newPercent);
            }
        });

        OpacityPercentProperty.Changed.AddClassHandler<GradientFillPropertyEditor>((control, args) =>
        {
            if (args.NewValue is double percent)
            {
                var newOpacity = percent / 100.0;
                if (Math.Abs(newOpacity - control.Opacity) > 0.001)
                    control.SetValue(OpacityProperty, newOpacity);
            }
        });

        GradientAngleProperty.Changed.AddClassHandler<GradientFillPropertyEditor>((sender, e) =>
        {
            sender.UpdateGradientPreview();
        });

        // 渐变停止点集合变化时同步标记点控件
        GradientStopsProperty.Changed.AddClassHandler<GradientFillPropertyEditor>((sender, e) =>
        {
            if (e.OldValue is ObservableCollection<GradientStopInfo> oldCollection)
                oldCollection.CollectionChanged -= sender.OnGradientStopsCollectionChanged;
            if (e.NewValue is ObservableCollection<GradientStopInfo> newCollection)
                newCollection.CollectionChanged += sender.OnGradientStopsCollectionChanged;
            sender.SyncStopMarkers();
        });

        PART_GradientKindComboBox.PointerPressed += (sender, e) =>
        {
            e.Handled = true;
        };

        Loaded += (s, e) =>
        {
            if (PART_GradientKindComboBox is EnumComboBox kindCombo)
            {
                kindCombo.EnumType = typeof(GradientKind);
                kindCombo.SelectedValue = GradientKindEnum.ToString();
                kindCombo.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<GradientKind>(args.NewValue, out var kind))
                        GradientKindEnum = kind;
                };
            }

            // 初始同步一次标记点
            SyncStopMarkers();

            // 兜底：Loaded 时父 Canvas 可能尚未完成布局（Bounrd.Width 仍为 0 / NaN），
            // 这里在 Loaded 完成后再延迟一次同步，等布局真正稳定后再算定位。
            Dispatcher.UIThread.Post(() =>
            {
                SyncStopMarkers();
                RepositionAllMarkers();
            }, DispatcherPriority.Loaded);

            UpdateGradientPreview();

            PART_ColorPicker.ColorSelected += OnColorSelected;

            PART_ColorPicker.CloseRequested += (s, e) =>
            {
                IsPickerOpen = false;
            };
        };

        DragHost.PointerPressed += OnPointerPressed;
        DragHost.PointerMoved += OnPointerMoved;
        DragHost.PointerReleased += OnPointerReleased;
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
            PART_ColorPopup.HorizontalOffset += delta.X;
            PART_ColorPopup.VerticalOffset += delta.Y;

            _lastPos = pos;
            e.Handled = true;
        }
    }

    // 鼠标松开：结束拖动
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    #endregion

    private void UpdateGradientKindState()
    {
        GradientKindIndex = (int)GradientKindEnum;
    }

    private void Border_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border input || input.Tag is not GradientStopInfo stop)
            return;

        _activeStopColor = stop;

        if (PART_ColorPicker == null || PART_ColorPopup == null)
            return;

        PART_ColorPicker.SetColor(stop.Color);
        PART_ColorPicker.Alpha = stop.Opacity * 100;

        PART_ColorPopup.HorizontalOffset = 0;
        PART_ColorPopup.VerticalOffset = 0;
        PART_ColorPopup.PlacementTarget = input;
        PART_ColorPopup.IsOpen = true;
    }

    private void ColorInput_TextChanged(object? sender, string e)
    {
        if (sender is not ColorInput input || input.Tag is not GradientStopInfo stop)
            return;

        if (Color.TryParse(input.Text, out Color color))
        {
            if (stop.Color != input.Text)
                stop.Color = input.Text;

            if (PART_PreviewCanvas != null)
            {
                PART_PreviewCanvas.InvalidateVisual();
                StopOffsetChanged?.Invoke();
            }
        }
    }

    private void OnColorSelected(object? sender, (string Color, double Alpha) e)
    {
        bool isChanged = false;

        if (_activeStopColor != null)
        {
            if (_activeStopColor.Color != e.Color && Color.TryParse(e.Color, out Color color))
            {
                _activeStopColor.Color = e.Color;
                isChanged = true;
            }

            double opecityValue = e.Alpha / 100;
            if (_activeStopColor.Opacity != opecityValue)
            {
                _activeStopColor.Opacity = opecityValue;
                isChanged = true;
            }
        }

        if (PART_PreviewCanvas != null && isChanged)
        {
            PART_PreviewCanvas.InvalidateVisual();
            StopOffsetChanged?.Invoke();
        }
    }

    private void GradientBar_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var bar = this.FindControl<Border>("PART_GradientBar");
        if (bar == null) return;

        var pos = e.GetPosition(bar);
        double newOffset = Math.Clamp(pos.X / bar.Bounds.Width, 0, 1);
        var stops = GradientStops;
        if (stops == null) return;

        // 双击：删除最近的停止点
        if (e.ClickCount == 1 && e.Properties.IsRightButtonPressed)
        {
            var nearest = stops.OrderBy(s => Math.Abs(s.Offset - newOffset)).FirstOrDefault();
            if (nearest != null && stops.Count > 2)
            {
                stops.Remove(nearest);
                UpdateGradientPreview();
            }
            return;
        }

        // 单击：在空白处添加停止点
        if (!stops.Any(s => Math.Abs(s.Offset - newOffset) < 0.02))
        {
            var newStop = new GradientStopInfo(newOffset, BlendColorAtOffset(newOffset));
            stops.Add(newStop);
            UpdateGradientPreview();
        }
    }

    private string BlendColorAtOffset(double offset)
    {
        var stops = GradientStops;
        if (stops == null || stops.Count == 0) return "#FFFFFF";

        var sorted = stops.OrderBy(s => s.Offset).ToList();
        if (offset <= sorted[0].Offset) return sorted[0].Color;
        if (offset >= sorted[^1].Offset) return sorted[^1].Color;

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (offset >= sorted[i].Offset && offset <= sorted[i + 1].Offset)
            {
                var t = (offset - sorted[i].Offset) / (sorted[i + 1].Offset - sorted[i].Offset);
                return BlendTwoColors(sorted[i].Color, sorted[i + 1].Color, t);
            }
        }
        return "#FFFFFF";
    }

    private string BlendTwoColors(string c1, string c2, double t)
    {
        if (!Color.TryParse(c1, out var color1)) color1 = Colors.White;
        if (!Color.TryParse(c2, out var color2)) color2 = Colors.Black;
        var r = (byte)(color1.R + (color2.R - color1.R) * t);
        var g = (byte)(color1.G + (color2.G - color1.G) * t);
        var b = (byte)(color1.B + (color2.B - color1.B) * t);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private void UpdateGradientPreview()
    {
        var preview = this.FindControl<GradientPreviewCanvas>("PART_PreviewCanvas");
        var stops = GradientStops;
        if (preview == null)
            return;

        preview.SetGradient(stops);
    }

    #region 标记点（GradientStopMarker）同步管理

    private void OnGradientStopsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 必须在 UI 线程同步画布
        if (Dispatcher.UIThread.CheckAccess())
            SyncStopMarkers();
        else
            Dispatcher.UIThread.Post(SyncStopMarkers, DispatcherPriority.Background);
    }

    private void SyncStopMarkers()
    {
        if (PART_MarkersCanvas == null) return;
        var stops = GradientStops;

        // 移除已不存在的停止点对应的标记
        if (stops == null)
        {
            foreach (var marker in _stopMarkers.Values)
            {
                UnhookMarker(marker);
                PART_MarkersCanvas.Children.Remove(marker);
            }
            _stopMarkers.Clear();
            return;
        }

        var currentSet = new HashSet<GradientStopInfo>(stops);
        var toRemove = _stopMarkers.Where(kvp => !currentSet.Contains(kvp.Key)).Select(kvp => kvp.Key).ToList();
        foreach (var stop in toRemove)
        {
            if (_stopMarkers.TryGetValue(stop, out var marker))
            {
                UnhookMarker(marker);
                stop.PropertyChanged -= OnStopPropertyChanged;
                PART_MarkersCanvas.Children.Remove(marker);
                _stopMarkers.Remove(stop);
            }
        }

        // 添加新停止点对应的标记，刷新已有标记的数据
        foreach (var stop in stops)
        {
            if (!_stopMarkers.TryGetValue(stop, out var marker))
            {
                marker = new GradientStopMarker
                {
                    StopColor = stop.Color,
                    StopOpacity = stop.Opacity,
                    OffsetValue = stop.Offset,
                };
                HookMarker(marker);
                stop.PropertyChanged += OnStopPropertyChanged;
                PART_MarkersCanvas.Children.Add(marker);
                _stopMarkers[stop] = marker;
            }
            else
            {
                if (!string.Equals(marker.StopColor, stop.Color, StringComparison.Ordinal))
                    marker.StopColor = stop.Color;
                if (Math.Abs(marker.StopOpacity - stop.Opacity) > 0.0001)
                    marker.StopOpacity = stop.Opacity;
                if (Math.Abs(marker.OffsetValue - stop.Offset) > 0.0001)
                    marker.OffsetValue = stop.Offset;
            }
        }
    }

    /// <summary>
    /// 强制让所有 marker 重新计算位置。解决 Loaded / 首次打开时父 Canvas 宽度尚未确定的场景。
    /// </summary>
    private void RepositionAllMarkers()
    {
        if (PART_MarkersCanvas == null) return;
        double w = PART_MarkersCanvas.Bounds.Width;
        // 即使宽度暂时为 0，也通过反射调用 marker 的 UpdatePosition，让 LayoutUpdated / SizeChanged 后续兜底
        foreach (var marker in _stopMarkers.Values)
        {
            marker.InvalidatePosition();
        }
        // 假如宽度 OK 则同步一次即可
        if (w > 0)
        {
            foreach (var marker in _stopMarkers.Values)
            {
                marker.UpdatePositionNow();
            }
        }
    }

    private void HookMarker(GradientStopMarker marker)
    {
        marker.OffsetDragging += OnMarkerOffsetDragging;
        marker.DeleteRequested += OnMarkerDeleteRequested;
        marker.MarkerSelected += OnMarkerSelected;
    }

    private void UnhookMarker(GradientStopMarker marker)
    {
        marker.OffsetDragging -= OnMarkerOffsetDragging;
        marker.DeleteRequested -= OnMarkerDeleteRequested;
        marker.MarkerSelected -= OnMarkerSelected;
    }

    /// <summary>
    /// 监听对应 GradientStopInfo 的属性变化，实时同步到标记点（颜色 / 透明度 / 位置）。
    /// </summary>
    private void OnStopPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not GradientStopInfo stop) return;
        if (!_stopMarkers.TryGetValue(stop, out var marker)) return;

        if (e.PropertyName == nameof(GradientStopInfo.Color))
        {
            marker.StopColor = stop.Color;
            UpdateGradientPreview();
        }
        else if (e.PropertyName == nameof(GradientStopInfo.Opacity))
        {
            marker.StopOpacity = stop.Opacity;
            UpdateGradientPreview();
        }
        else if (e.PropertyName == nameof(GradientStopInfo.Offset))
        {
            marker.OffsetValue = stop.Offset;
            UpdateGradientPreview();
        }
    }

    private void OnMarkerOffsetDragging(object? sender, double offset)
    {
        if (sender is not GradientStopMarker marker) return;

        // 反向查找对应的停止点
        GradientStopInfo? stop = null;
        foreach (var kvp in _stopMarkers)
        {
            if (ReferenceEquals(kvp.Value, marker))
            {
                stop = kvp.Key;
                break;
            }
        }
        if (stop == null) return;

        stop.Offset = offset;
        UpdateGradientPreview();
        StopOffsetChanged?.Invoke();
    }

    private void OnMarkerDeleteRequested(object? sender, EventArgs e)
    {
        if (sender is not GradientStopMarker marker) return;
        GradientStopInfo? stop = null;
        foreach (var kvp in _stopMarkers)
        {
            if (ReferenceEquals(kvp.Value, marker))
            {
                stop = kvp.Key;
                break;
            }
        }
        if (stop == null) return;

        var stops = GradientStops;
        if (stops == null || stops.Count <= 2) return;

        stops.Remove(stop);
        UpdateGradientPreview();
        StopOffsetChanged?.Invoke();
    }

    private void OnMarkerSelected(object? sender, EventArgs e)
    {
        if (sender is not GradientStopMarker marker) return;
        foreach (var kvp in _stopMarkers)
        {
            kvp.Value.IsStopSelected = ReferenceEquals(kvp.Value, marker);
        }
    }

    #endregion

    private void NumericUpDownEx_ValueChanged(object? sender, double e)
    {
        NumericPercentUpDownEx numericUpDownEx = sender as NumericPercentUpDownEx;
        if (numericUpDownEx?.Tag is GradientStopInfo stop)
        {
            var preview = this.FindControl<GradientPreviewCanvas>("PART_PreviewCanvas");
            preview?.InvalidateVisual();
            StopOffsetChanged?.Invoke();
        }
    }

    private void AddStop_Click(object? sender, TappedEventArgs e)
    {
        var stops = GradientStops;
        if (stops == null) return;

        double newOffset = 0.5;
        var sorted = stops.OrderBy(s => s.Offset).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
        {
            double gap = sorted[i + 1].Offset - sorted[i].Offset;
            if (gap > 0.05)
            {
                newOffset = sorted[i].Offset + gap / 2;
                break;
            }
        }

        var newStop = new GradientStopInfo(newOffset, BlendColorAtOffset(newOffset));
        stops.Add(newStop);
        UpdateGradientPreview();
    }

    private void RemoveStop_Click(object? sender, TappedEventArgs e)
    {
        if (sender is not TextBlock btn || btn.Tag == null || btn.Tag is not GradientStopInfo stop)
            return;

        var stops = GradientStops;
        if (stops == null || stops.Count <= 2)
            return;

        stops.Remove(stop);
        UpdateGradientPreview();
    }

    private void SimpleColorInput_ColorPickerClicked(object? sender, System.EventArgs e)
    {
        if (sender is not SimpleColorInput input || input.Tag == null || input.Tag is not GradientStopInfo stop)
            return;

        _activeStopColor = stop;

        if (PART_ColorPicker == null || PART_ColorPopup == null)
            return;

        PART_ColorPicker.SetColor(stop.Color);
        PART_ColorPicker.Alpha = stop.Opacity * 100;

        PART_ColorPopup.HorizontalOffset = 0;
        PART_ColorPopup.VerticalOffset = 0;
        PART_ColorPopup.PlacementTarget = input.Parent as Control;
        PART_ColorPopup.IsOpen = true;
    }
}

public class GradientPreviewCanvas : Control
{
    private ObservableCollection<GradientStopInfo>? _gradientStops;

    public GradientPreviewCanvas()
    {
    }

    public void SetGradient(ObservableCollection<GradientStopInfo>? stops)
    {
        _gradientStops = stops;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var w = this.Bounds.Width;
        var h = this.Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var barRect = new Rect(0, 0, w, h);

        // 棋盘格底（透明区域显示）
        DrawCheckerboard(context, barRect);

        // 渐变填充
        if (_gradientStops != null && _gradientStops.Count >= 2)
        {
            var stops = BuildStops();
            var brush = CreateLinearGradient(stops);
            context.FillRectangle(brush, barRect);
        }
    }

    private static void DrawCheckerboard(DrawingContext context, Rect area)
    {
        double cellSize = 8;
        var light = new SolidColorBrush(Color.Parse("#FFF"));
        var dark = new SolidColorBrush(Color.Parse("#CCC"));
        for (double x = area.X; x < area.Right; x += cellSize)
        {
            for (double y = area.Y; y < area.Bottom; y += cellSize)
            {
                bool isLight = (((int)(x / cellSize) + (int)(y / cellSize)) & 1) == 0;
                var rect = new Rect(x, y,
                    Math.Min(cellSize, area.Right - x),
                    Math.Min(cellSize, area.Bottom - y));
                context.FillRectangle(isLight ? light : dark, rect);
            }
        }
    }

    private GradientStops BuildStops()
    {
        var result = new GradientStops();
        if (_gradientStops == null) return result;
        foreach (var s in _gradientStops.OrderBy(x => x.Offset))
        {
            if (!Color.TryParse(s.Color, out var color))
                color = Colors.White;
            byte a = (byte)(color.A * s.Opacity);
            result.Add(new GradientStop(Color.FromArgb(a, color.R, color.G, color.B), s.Offset));
        }
        return result;
    }

    private static IBrush CreateLinearGradient(GradientStops stops)
    {
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
            GradientStops = stops
        };
    }
}
