using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using yuyiAvalonia.ColorSelector.ViewModels;

namespace yuyiAvalonia.ColorSelector.Views;

public partial class ColorPickerPopup : UserControl
{
    private ColorPickerPopupViewModel? _viewModel;
    private bool _isDraggingHue = false;
    private bool _isDraggingAlpha = false;
    private bool _isDraggingSaturation = false;

    public event EventHandler<string>? ColorSelected;

    public ColorPickerPopup()
    {
        InitializeComponent();

        _viewModel = new ColorPickerPopupViewModel();
        DataContext = _viewModel;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SetupInteractions();
        UpdateColorPreview();
        UpdateSliderPositions();
    }

    public void SetColor(string hex)
    {
        if (_viewModel != null)
        {
            _viewModel.SetColor(hex);
            UpdateColorPreview();
            UpdateSliderPositions();
        }
    }

    public string GetColor()
    {
        return _viewModel?.HexColor ?? "#1890FF";
    }

    private void SetupInteractions()
    {
        if (HueSliderTrack != null)
        {
            HueSliderTrack.PointerPressed += (s, e) =>
            {
                _isDraggingHue = true;
                UpdateHueFromPointer(e.GetPosition(HueSliderTrack).X, HueSliderTrack.Bounds.Width);
            };
            HueSliderTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingHue)
                {
                    UpdateHueFromPointer(e.GetPosition(HueSliderTrack).X, HueSliderTrack.Bounds.Width);
                }
            };
            HueSliderTrack.PointerReleased += (s, e) => _isDraggingHue = false;
            HueSliderTrack.PointerExited += (s, e) => _isDraggingHue = false;
        }

        if (AlphaSliderTrack != null)
        {
            AlphaSliderTrack.PointerPressed += (s, e) =>
            {
                _isDraggingAlpha = true;
                UpdateAlphaFromPointer(e.GetPosition(AlphaSliderTrack).X, AlphaSliderTrack.Bounds.Width);
            };
            AlphaSliderTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingAlpha)
                {
                    UpdateAlphaFromPointer(e.GetPosition(AlphaSliderTrack).X, AlphaSliderTrack.Bounds.Width);
                }
            };
            AlphaSliderTrack.PointerReleased += (s, e) => _isDraggingAlpha = false;
            AlphaSliderTrack.PointerExited += (s, e) => _isDraggingAlpha = false;
        }

        if (HueSaturationCanvas != null)
        {
            HueSaturationCanvas.PointerPressed += (s, e) =>
            {
                _isDraggingSaturation = true;
                UpdateSaturationFromPointer(e.GetPosition(HueSaturationCanvas));
            };
            HueSaturationCanvas.PointerMoved += (s, e) =>
            {
                if (_isDraggingSaturation)
                {
                    UpdateSaturationFromPointer(e.GetPosition(HueSaturationCanvas));
                }
            };
            HueSaturationCanvas.PointerReleased += (s, e) => _isDraggingSaturation = false;
            HueSaturationCanvas.PointerExited += (s, e) => _isDraggingSaturation = false;
        }

        if (ConfirmButton != null)
        {
            ConfirmButton.Click += (s, e) =>
            {
                ColorSelected?.Invoke(this, GetColor());
            };
        }

        // 预设颜色点击
        var presetColors = this.FindControl<WrapPanel>("PresetColors");
        if (presetColors != null)
        {
            foreach (var child in presetColors.Children)
            {
                if (child is Border border && border.Tag is string colorHex)
                {
                    border.PointerPressed += (s, e) =>
                    {
                        SetColor(colorHex);
                    };
                }
            }
        }

        // 监听视图模型变化
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ColorPickerPopupViewModel.HexColor) ||
                    e.PropertyName == nameof(ColorPickerPopupViewModel.Hue) ||
                    e.PropertyName == nameof(ColorPickerPopupViewModel.Saturation) ||
                    e.PropertyName == nameof(ColorPickerPopupViewModel.Brightness))
                {
                    UpdateColorPreview();
                    UpdateSliderPositions();
                }
            };
        }
    }

    private void UpdateHueFromPointer(double x, double width)
    {
        if (_viewModel == null || width <= 0) return;
        var ratio = Math.Max(0, Math.Min(1, x / width));
        _viewModel.Hue = ratio * 360;
    }

    private void UpdateAlphaFromPointer(double x, double width)
    {
        if (_viewModel == null || width <= 0) return;
        var ratio = Math.Max(0, Math.Min(1, x / width));
        _viewModel.Alpha = (byte)(ratio * 255);
    }

    private void UpdateSaturationFromPointer(Avalonia.Point position)
    {
        if (_viewModel == null) return;
        var bounds = HueSaturationCanvas.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        var x = position.X / bounds.Width * 100;
        var y = position.Y / bounds.Height * 100;

        _viewModel.Saturation = Math.Max(0, Math.Min(100, x));
        _viewModel.Brightness = Math.Max(0, Math.Min(100, 100 - y));
    }

    private void UpdateColorPreview()
    {
        if (_viewModel == null || ColorPreview == null) return;

        var color = new SolidColorBrush(Color.FromArgb(
            _viewModel.Alpha,
            _viewModel.Red,
            _viewModel.Green,
            _viewModel.Blue));

        ColorPreview.Fill = color;

        // 暂时注释掉 AlphaEndColor 相关代码，因为在代码中无法直接访问 XAML 中定义的渐变停止点
        // if (AlphaEndColor != null)
        // {
        //     AlphaEndColor.Color = Color.FromArgb(255, _viewModel.Red, _viewModel.Green, _viewModel.Blue);
        // }
    }

    private void UpdateSliderPositions()
    {
        if (_viewModel == null) return;

        if (HueSliderThumb != null && HueSliderTrack != null)
        {
            var hueRatio = _viewModel.Hue / 360.0;
            var hueThumbX = hueRatio * HueSliderTrack.Bounds.Width;
            HueSliderThumb.Width = 6;
            HueSliderThumb.IsVisible = true;
        }

        if (AlphaSliderThumb != null && AlphaSliderTrack != null)
        {
            var alphaRatio = _viewModel.Alpha / 255.0;
            var alphaThumbX = alphaRatio * AlphaSliderTrack.Bounds.Width;
            AlphaSliderThumb.Width = 6;
            AlphaSliderThumb.IsVisible = true;
        }

        if (SelectorCircle != null && HueSaturationCanvas != null)
        {
            var satX = _viewModel.Saturation / 100.0 * HueSaturationCanvas.Bounds.Width - 8;
            var brightY = (100 - _viewModel.Brightness) / 100.0 * HueSaturationCanvas.Bounds.Height - 8;
            SelectorCircle.Margin = new Thickness(satX, brightY, 0, 0);
        }
    }
}
