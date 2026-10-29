using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.ComponentModel.Design;
using yuyiAvalonia.ColorSelector.ViewModels;

namespace yuyiAvalonia.ColorSelector.Views;

public partial class ColorSelectorView : UserControl
{
    public ColorSelectorView()
    {
        InitializeComponent();
    }

    private void ColorSwatch_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is string propertyName && DataContext is ColorSelectorViewModel vm)
        {
            vm.OpenColorPickerCommand.Execute(propertyName);
        }
    }

    private void ColorPickerOverlay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source == sender && DataContext is ColorSelectorViewModel vm)
        {
            vm.CloseColorPickerCommand.Execute(null);
        }
    }

    private void PresetColor_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is string color && DataContext is ColorSelectorViewModel vm)
        {
            vm.SelectPresetColor(color);
        }
    }
}
