using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using yuyiAvalonia.Base.Controls;
using yuyiAvalonia.ImageEdit.Controls;
using yuyiAvalonia.ImageEdit.ViewModels;

namespace yuyiAvalonia.ImageEdit.Views;

public partial class ImageEditTextPanel : UserControl
{
    public ImageEditTextPanel()
    {
        InitializeComponent();

        Loaded += ImageEditTextPanel_Loaded;
    }

    private void ImageEditTextPanel_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(ColorSelector != null)
        {
            ColorSelector.ColorSelected += ColorSelector_ColorSelected;
        }
    }

    private void ColorSelector_ColorSelected(object? sender, Color e)
    {
        if (DataContext is not ImageEditViewModel vm) return;
        vm.SelectTextColor(e);
    }

    private void ColorPicker_ColorSelected(object? sender, (string Color, double Alpha) e)
    {
        if (DataContext is not ImageEditViewModel vm) return;

        try
        {
            var colorHex = e.Color.TrimStart('#');
            if (colorHex.Length >= 6)
            {
                var r = Convert.ToByte(colorHex.Substring(0, 2), 16);
                var g = Convert.ToByte(colorHex.Substring(2, 2), 16);
                var b = Convert.ToByte(colorHex.Substring(4, 2), 16);
                var alpha = (byte)(e.Alpha / 100.0 * 255);
                var color = Color.FromArgb(alpha, r, g, b);
                vm.SelectTextColorCommand.Execute(color);
            }
        }
        catch { }
    }

    private void ColorPicker_CloseRequested(object? sender, EventArgs e)
    {
        // 关闭拾色器面板或执行其他关闭逻辑
    }

    private async void ShowColorPickerBtn_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm) return;

        var currentColor = vm.CurrentTextSettings.FontColor;
        var hexColor = $"#{currentColor.R:X2}{currentColor.G:X2}{currentColor.B:X2}";

        var dialog = new ColorPickerDialog();
        dialog.SetInitialColor(hexColor);

        var result = await dialog.ShowDialog<(string Color, double Alpha)?>(this.VisualRoot as Window);

        if (result.HasValue)
        {
            try
            {
                var colorHex = result.Value.Color.TrimStart('#');
                if (colorHex.Length >= 6)
                {
                    var r = Convert.ToByte(colorHex.Substring(0, 2), 16);
                    var g = Convert.ToByte(colorHex.Substring(2, 2), 16);
                    var b = Convert.ToByte(colorHex.Substring(4, 2), 16);
                    var alpha = (byte)(result.Value.Alpha / 100.0 * 255);
                    var color = Color.FromArgb(alpha, r, g, b);
                    vm.SelectTextColorCommand.Execute(color);
                }
            }
            catch { }
        }
    }
}
