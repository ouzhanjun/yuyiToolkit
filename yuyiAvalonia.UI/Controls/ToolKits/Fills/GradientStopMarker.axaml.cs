using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 渐变停止点标记控件（位于色带上方）
/// 形状：矩形 + 底部向下小尖角（五边形），内部小矩形显示停止点的颜色
/// 行为：可水平拖动改变偏移、双击删除、左键选中
/// </summary>
public partial class GradientStopMarker : UserControl
{
    public static readonly StyledProperty<double> OffsetValueProperty =
        AvaloniaProperty.Register<GradientStopMarker, double>(nameof(OffsetValue), 0.0);

    public static readonly StyledProperty<string> StopColorProperty =
        AvaloniaProperty.Register<GradientStopMarker, string>(nameof(StopColor), "#FFFFFFFF");

    public static readonly StyledProperty<double> StopOpacityProperty =
        AvaloniaProperty.Register<GradientStopMarker, double>(nameof(StopOpacity), 1.0);

    public static readonly StyledProperty<bool> IsStopSelectedProperty =
        AvaloniaProperty.Register<GradientStopMarker, bool>(nameof(IsStopSelected), false);

    public double OffsetValue
    {
        get => GetValue(OffsetValueProperty);
        set => SetValue(OffsetValueProperty, value);
    }

    public string StopColor
    {
        get => GetValue(StopColorProperty);
        set => SetValue(StopColorProperty, value);
    }

    public double StopOpacity
    {
        get => GetValue(StopOpacityProperty);
        set => SetValue(StopOpacityProperty, value);
    }

    public bool IsStopSelected
    {
        get => GetValue(IsStopSelectedProperty);
        set => SetValue(IsStopSelectedProperty, value);
    }

    public event EventHandler<double>? OffsetDragging;
    public event EventHandler? DeleteRequested;
    public event EventHandler? MarkerSelected;

    private bool _isDragging;
    private Canvas? _parentCanvas;

    public GradientStopMarker()
    {
        InitializeComponent();

        OffsetValueProperty.Changed.AddClassHandler<GradientStopMarker>((c, _) => c.UpdatePosition());
        StopColorProperty.Changed.AddClassHandler<GradientStopMarker>((c, _) => c.UpdateColor());
        StopOpacityProperty.Changed.AddClassHandler<GradientStopMarker>((c, _) => c.UpdateColor());
        IsStopSelectedProperty.Changed.AddClassHandler<GradientStopMarker>((c, _) => c.UpdateSelection());

        AttachedToVisualTree += (s, e) =>
        {
            HookParentCanvas();
            UpdatePosition();
        };
        DetachedFromVisualTree += (s, e) => UnhookParentCanvas();

        // 布局完成后主动重新定位（兜底：解决父 Canvas 初始尺寸为 0 / NaN 时
        // SizeChanged 不触发的问题，确保重新打开编辑器后标记点能正确停放）
        LayoutUpdated += (s, e) => UpdatePosition();

        UpdateColor();
        UpdateSelection();
    }

    #region 父 Canvas 追踪（用于根据父容器宽度计算位置）

    private void HookParentCanvas()
    {
        UnhookParentCanvas();
        _parentCanvas = FindParentCanvas();
        if (_parentCanvas != null)
        {
            _parentCanvas.SizeChanged += OnParentSizeChanged;
        }
    }

    private void UnhookParentCanvas()
    {
        if (_parentCanvas != null)
        {
            _parentCanvas.SizeChanged -= OnParentSizeChanged;
            _parentCanvas = null;
        }
    }

    private Canvas? FindParentCanvas()
    {
        Visual? v = this.GetVisualParent();
        while (v != null)
        {
            if (v is Canvas c) return c;
            v = v.GetVisualParent();
        }
        return null;
    }

    private void OnParentSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdatePosition();
    }

    #endregion

    private void UpdatePosition()
    {
        if (_parentCanvas == null)
        {
            HookParentCanvas();
            if (_parentCanvas == null) return;
        }

        double w = _parentCanvas.Bounds.Width;
        if (w <= 0 || double.IsNaN(w)) return;

        double markerW = Bounds.Width > 0 ? Bounds.Width : Width;
        double markerH = Bounds.Height > 0 ? Bounds.Height : Height;

        // 水平居中于对应 offset 位置；垂直对齐到父容器底部，使尖角指向色带
        double x = OffsetValue * w - markerW / 2.0;
        double y = Math.Max(0, _parentCanvas.Bounds.Height - markerH);

        Canvas.SetLeft(this, x);
        Canvas.SetTop(this, y);
    }

    /// <summary>
    /// 兜底：让 marker 在没有 SizeChanged 触发时也能重新定位（AttachedToVisualTree / Loaded 后调用）。
    /// 即使父 Canvas 宽度为 0，也会激活订阅，确保后续 LayoutUpdated 触发定位。
    /// </summary>
    public void InvalidatePosition()
    {
        if (_parentCanvas == null) HookParentCanvas();
        UpdatePosition();
    }

    /// <summary>
    /// 强制立即重算并应用 Canvas.Left / Canvas.Top。常用于 SyncStopMarkers 兜底阶段。
    /// </summary>
    public void UpdatePositionNow()
    {
        UpdatePosition();
    }

    private void UpdateColor()
    {
        if (PART_ColorRect == null) return;

        try
        {
            if (Color.TryParse(StopColor, out Color color))
            {
                byte a = (byte)Math.Clamp((int)(color.A * StopOpacity), 0, 255);
                PART_ColorRect.Background = new SolidColorBrush(
                    Color.FromArgb(a, color.R, color.G, color.B));
            }
            else
            {
                PART_ColorRect.Background = new SolidColorBrush(Colors.White);
            }
        }
        catch
        {
            PART_ColorRect.Background = new SolidColorBrush(Colors.White);
        }
    }

    private void UpdateSelection()
    {
        if (PART_OuterPath == null) return;

        bool highlight = IsStopSelected || _isDragging;
        if (highlight)
        {
            PART_OuterPath.Stroke = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            PART_OuterPath.StrokeThickness = 1.5;
        }
        else
        {
            if (this.TryFindResource("BorderColor", ActualThemeVariant, out var resource) && resource is IBrush brush)
                PART_OuterPath.Stroke = brush;
            else
                PART_OuterPath.Stroke = new SolidColorBrush(Color.FromRgb(140, 140, 140));
            PART_OuterPath.StrokeThickness = 1;
        }
    }

    #region 指针事件（拖拽、删除、选中）

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(this);
        if (point.Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            UpdateSelection();
            MarkerSelected?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || _parentCanvas == null) return;

        var pos = e.GetPosition(_parentCanvas);
        double w = _parentCanvas.Bounds.Width;
        if (w <= 0) return;

        double newOffset = Math.Clamp(pos.X / w, 0, 1);
        if (Math.Abs(newOffset - OffsetValue) > 0.0005)
        {
            OffsetValue = newOffset;
            OffsetDragging?.Invoke(this, newOffset);
        }
        e.Handled = true;
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            UpdateSelection();
        }
    }

    private void OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        DeleteRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    #endregion
}
