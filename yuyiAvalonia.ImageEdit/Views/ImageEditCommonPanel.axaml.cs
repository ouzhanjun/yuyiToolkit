using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.ImageEdit.Models;
using yuyiAvalonia.ImageEdit.ViewModels;

namespace yuyiAvalonia.ImageEdit.Views;

public partial class ImageEditCommonPanel : UserControl
{
    private readonly Debounce _debounce;
    // Title 属性
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ImageEditCommonPanel, string>(nameof(Title), defaultValue: "基础调整");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ImageEditCommonPanel()
    {
        InitializeComponent();
        _debounce = new Debounce(150);
    }

    // 1. 定义依赖属性
    public static readonly StyledProperty<ObservableCollection<ImageIntensity>> IntensityBasicProperty =
        AvaloniaProperty.Register<ImageEditCommonPanel, ObservableCollection<ImageIntensity>>(
            nameof(IntensityBasic));

    // 2. CLR 包装属性
    public ObservableCollection<ImageIntensity> IntensityBasic
    {
        get => GetValue(IntensityBasicProperty);
        set => SetValue(IntensityBasicProperty, value);
    }


    private void Slider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm || !vm.HasImage || vm.ImageSource == null)
            return;

        _debounce.Trigger(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                vm.PreviewImageAdjust();
            });
        });
    }

    private void Slider_PointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (DataContext is ImageEditViewModel vm)
        {
            if (!vm.HasImage || vm.ImageSource == null)
                return;

            Slider slider = (Slider)sender;
            ImageIntensity intensity = slider.Tag as ImageIntensity;

            if (intensity != null)
            {
                vm.ApplyImageAdjust(intensity);
            }
        }
    }
}