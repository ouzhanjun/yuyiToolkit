using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace yuyiAvalonia.UI.Controls;

public partial class ZoomControl : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<double> ZoomFactorProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(ZoomFactor), 1.0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinZoomProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(MinZoom), 0.25);

    public static readonly StyledProperty<double> MaxZoomProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(MaxZoom), 4.0);

    public static readonly StyledProperty<double> ZoomStepProperty =
        AvaloniaProperty.Register<ZoomControl, double>(nameof(ZoomStep), 0.1);

    private TextBlock? _zoomPercentText;

    public double ZoomFactor
    {
        get => GetValue(ZoomFactorProperty);
        set
        {
            var clampedValue = Math.Max(MinZoom, Math.Min(MaxZoom, value));
            if (Math.Abs(GetValue(ZoomFactorProperty) - clampedValue) < 0.0001) return;
            SetValue(ZoomFactorProperty, clampedValue);
            UpdateZoomPercentText();
        }
    }

    public double MinZoom
    {
        get => GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public double MaxZoom
    {
        get => GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    public double ZoomStep
    {
        get => GetValue(ZoomStepProperty);
        set => SetValue(ZoomStepProperty, value);
    }

    public string ZoomPercent => $"{(int)(ZoomFactor * 100)}%";

    public event PropertyChangedEventHandler? PropertyChanged;

    public ZoomControl()
    {
        InitializeComponent();

        ZoomFactorProperty.Changed.AddClassHandler<ZoomControl>((sender, args) =>
        {
            sender.UpdateZoomPercentText();
        });
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _zoomPercentText = this.FindControl<TextBlock>("ZoomPercentText");
        UpdateZoomPercentText();
    }

    private void UpdateZoomPercentText()
    {
        if (_zoomPercentText != null)
        {
            _zoomPercentText.Text = ZoomPercent;
        }
    }

    private void OnZoomInClick(object? sender, RoutedEventArgs e)
    {
        ZoomFactor = Math.Min(MaxZoom, ZoomFactor + ZoomStep);
    }

    private void OnZoomOutClick(object? sender, RoutedEventArgs e)
    {
        ZoomFactor = Math.Max(MinZoom, ZoomFactor - ZoomStep);
    }

    private void OnResetZoomClick(object? sender, RoutedEventArgs e)
    {
        ZoomFactor = 1.0;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
