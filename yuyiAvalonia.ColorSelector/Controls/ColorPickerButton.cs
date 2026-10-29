using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace yuyiAvalonia.ColorSelector.Controls;

public class ColorPickerButton : TemplatedControl
{
    public static readonly StyledProperty<string> ColorProperty =
        AvaloniaProperty.Register<ColorPickerButton, string>(nameof(Color), "#1890FF");

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<ColorPickerButton, string>(nameof(Label), "");

    public static readonly DirectProperty<ColorPickerButton, Popup?> PopupProperty =
        AvaloniaProperty.RegisterDirect<ColorPickerButton, Popup?>(nameof(Popup), o => o.Popup);

    private Popup? _popup;
    private bool _isPopupOpen = false;

    public string Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public Popup? Popup => _popup;

    private Views.ColorPickerPopup? _colorPicker;

    public ColorPickerButton()
    {
        PointerPressed += OnPointerPressed;
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        TogglePopup();
    }

    public void TogglePopup()
    {
        if (_popup == null)
        {
            CreatePopup();
        }

        if (_popup != null)
        {
            _isPopupOpen = !_isPopupOpen;
            _popup.IsOpen = _isPopupOpen;

            if (_isPopupOpen && _colorPicker != null)
            {
                _colorPicker.SetColor(Color);
                _colorPicker.ColorSelected += OnColorSelected;
            }
        }
    }

    public void OpenPopup()
    {
        if (_popup == null)
        {
            CreatePopup();
        }

        if (_popup != null && !_isPopupOpen)
        {
            _isPopupOpen = true;
            _popup.IsOpen = true;

            if (_colorPicker != null)
            {
                _colorPicker.SetColor(Color);
                _colorPicker.ColorSelected += OnColorSelected;
            }
        }
    }

    public void ClosePopup()
    {
        if (_popup != null && _isPopupOpen)
        {
            _isPopupOpen = false;
            _popup.IsOpen = false;

            if (_colorPicker != null)
            {
                _colorPicker.ColorSelected -= OnColorSelected;
            }
        }
    }

    private void CreatePopup()
    {
        _colorPicker = new Views.ColorPickerPopup();
        _colorPicker.Width = 280;

        _popup = new Popup
        {
            Placement = PlacementMode.Bottom,
            PlacementTarget = this,
            Child = _colorPicker,
            IsLightDismissEnabled = true
        };

        _popup.Closed += (s, e) =>
        {
            _isPopupOpen = false;
            if (_colorPicker != null)
            {
                _colorPicker.ColorSelected -= OnColorSelected;
            }
        };

        ((Panel)this.Parent)?.Children.Add(_popup);
    }

    private void OnColorSelected(object? sender, string color)
    {
        Color = color;
        ClosePopup();
        ColorChanged?.Invoke(this, color);
    }

    public event EventHandler<string>? ColorChanged;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        ClosePopup();

        if (_popup != null && this.Parent is Panel panel)
        {
            panel.Children.Remove(_popup);
            _popup = null;
        }
    }
}
