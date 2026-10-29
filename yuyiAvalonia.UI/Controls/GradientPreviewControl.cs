using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 渐变预览控件，直接渲染 FillItem 的渐变效果（支持 Linear/Radial/Angle/Diamond）。
/// 订阅 FillItem.PropertyChanged，属性变化时自动重绘，无需 converter。
/// </summary>
public class GradientPreviewControl : Control
{
    public static readonly StyledProperty<FillItem?> FillItemProperty =
        AvaloniaProperty.Register<GradientPreviewControl, FillItem?>(nameof(FillItem));

    public FillItem? FillItem
    {
        get => GetValue(FillItemProperty);
        set => SetValue(FillItemProperty, value);
    }

    private FillItem? _subscribedFill;
    private ObservableCollection<GradientStopInfo>? _subscribedStops;

    public GradientPreviewControl()
    {
        AffectsRender<GradientPreviewControl>(FillItemProperty);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FillItemProperty)
        {
            UnsubscribeFromFill(_subscribedFill);

            _subscribedFill = change.NewValue as FillItem;

            SubscribeToFill(_subscribedFill);

            InvalidateVisual();
        }
    }

    private void SubscribeToFill(FillItem? fill)
    {
        if (fill == null) return;
        fill.PropertyChanged += OnFillItemPropertyChanged;
        SubscribeToStops(fill.GradientStops);
    }

    private void UnsubscribeFromFill(FillItem? fill)
    {
        if (fill == null) return;
        fill.PropertyChanged -= OnFillItemPropertyChanged;
        UnsubscribeFromStops(fill.GradientStops);
    }

    private void SubscribeToStops(ObservableCollection<GradientStopInfo>? stops)
    {
        if (stops == null) return;
        _subscribedStops = stops;
        stops.CollectionChanged += OnGradientStopsCollectionChanged;
        foreach (var stop in stops)
            stop.PropertyChanged += OnStopPropertyChanged;
    }

    private void UnsubscribeFromStops(ObservableCollection<GradientStopInfo>? stops)
    {
        if (stops == null) return;
        if (ReferenceEquals(stops, _subscribedStops))
            _subscribedStops = null;
        stops.CollectionChanged -= OnGradientStopsCollectionChanged;
        foreach (var stop in stops)
            stop.PropertyChanged -= OnStopPropertyChanged;
    }

    private void OnGradientStopsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
            foreach (GradientStopInfo oldStop in e.OldItems)
                oldStop.PropertyChanged -= OnStopPropertyChanged;
        if (e.NewItems != null)
            foreach (GradientStopInfo newStop in e.NewItems)
                newStop.PropertyChanged += OnStopPropertyChanged;

        InvalidateVisual();
    }

    private void OnStopPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void OnFillItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var name = e.PropertyName;
        if (name == nameof(FillItem.GradientStops))
        {
            UnsubscribeFromStops(_subscribedFill?.GradientStops);
            SubscribeToStops(_subscribedFill?.GradientStops);
            InvalidateVisual();
            return;
        }

        if (name == nameof(FillItem.GradientAngle)
            || name == nameof(FillItem.GradientKind)
            || name == nameof(FillItem.RadialCenterX)
            || name == nameof(FillItem.RadialCenterY)
            || name == nameof(FillItem.RadialRadius)
            || name == nameof(FillItem.GradientStartColor)
            || name == nameof(FillItem.GradientEndColor)
            || name == nameof(FillItem.Opacity))
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        var w = this.Bounds.Width;
        var h = this.Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var fill = FillItem;
        if (fill == null) return;

        double opacity = fill.Opacity;

        DrawCheckerboard(context, w, h);

        var brush = BuildBrush(fill, w, h, opacity);
        if (brush != null)
            context.FillRectangle(brush, new Rect(0, 0, w, h));
    }

    private IBrush? BuildBrush(FillItem fill, double w, double h, double opacity)
    {
        if (fill.GradientStops == null || fill.GradientStops.Count < 2)
        {
            var start = TryParseColor(fill.GradientStartColor, out var sc) ? sc : Colors.White;
            var end = TryParseColor(fill.GradientEndColor, out var ec) ? ec : Colors.Black;
            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.FromArgb((byte)(opacity * 255), start.R, start.G, start.B), 0),
                    new GradientStop(Color.FromArgb((byte)(opacity * 255), end.R, end.G, end.B), 1),
                }
            };
        }

        var radians = fill.GradientAngle * Math.PI / 180.0;
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);
        var ordered = fill.GradientStops.OrderBy(s => s.Offset).ToList();

        switch (fill.GradientKind)
        {
            case GradientKind.Linear:
                return new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                    GradientStops = BuildStops(ordered, opacity)
                };

            case GradientKind.Radial:
                return new RadialGradientBrush
                {
                    Center = new RelativePoint(fill.RadialCenterX, fill.RadialCenterY, RelativeUnit.Relative),
                    GradientOrigin = new RelativePoint(fill.RadialCenterX, fill.RadialCenterY, RelativeUnit.Relative),
                    Radius = fill.RadialRadius,
                    GradientStops = BuildStops(ordered, opacity)
                };

            case GradientKind.Angle:
            case GradientKind.Diamond:
                var fc = TryParseColor(ordered[0].Color, out var fsc) ? fsc : Colors.White;
                var lc = TryParseColor(ordered[^1].Color, out var lec) ? lec : Colors.Black;
                return new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb((byte)(ordered[0].Opacity * opacity * 255), fc.R, fc.G, fc.B), 0),
                        new GradientStop(Color.FromArgb((byte)(ordered[^1].Opacity * opacity * 255), lc.R, lc.G, lc.B), 1),
                    }
                };

            default:
                return null;
        }
    }

    private static GradientStops BuildStops(IList<GradientStopInfo> ordered, double baseOpacity)
    {
        var stops = new GradientStops();
        foreach (var stop in ordered)
        {
            var color = TryParseColor(stop.Color, out var c) ? c : Colors.White;
            var a = (byte)(stop.Opacity * baseOpacity * 255);
            stops.Add(new GradientStop(Color.FromArgb(a, color.R, color.G, color.B), stop.Offset));
        }
        return stops;
    }

    private static bool TryParseColor(string? colorString, out Color color)
    {
        if (!string.IsNullOrWhiteSpace(colorString) && Color.TryParse(colorString, out color))
            return true;
        color = Colors.White;
        return false;
    }

    private static void DrawCheckerboard(DrawingContext context, double w, double h)
    {
        double cellSize = 8;
        var light = new SolidColorBrush(Color.Parse("#FFF"));
        var dark = new SolidColorBrush(Color.Parse("#CCC"));
        for (double x = 0; x < w; x += cellSize)
        {
            for (double y = 0; y < h; y += cellSize)
            {
                bool isLight = (((int)(x / cellSize) + (int)(y / cellSize)) & 1) == 0;
                var rect = new Rect(x, y, Math.Min(cellSize, w - x), Math.Min(cellSize, h - y));
                context.FillRectangle(isLight ? light : dark, rect);
            }
        }
    }
}
