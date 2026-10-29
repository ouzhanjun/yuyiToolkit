using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.ImageEdit.ViewModels;
using yuyiAvalonia.ImageEdit.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ImageEdit.Views;

public partial class ImageEditControlPanel : UserControl
{
    private readonly Debounce _debounce;

    public ImageEditControlPanel()
    {
        InitializeComponent();
        
        // 初始化防抖器，延迟100毫秒
        _debounce = new Debounce(150);
        this.Loaded += ImageEditControlPanel_Loaded;
    }

    private void ImageEditControlPanel_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //if (DataContext is ImageEditViewModel vm)
        //{
        //    BasicPanel.IntensityBasic = vm.ImageEditIntensityBasic;
        //}
    }

    private void Slider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm || !vm.HasImage || vm.ImageSource == null)
            return;
        
        // 使用防抖来优化性能，避免频繁调用
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