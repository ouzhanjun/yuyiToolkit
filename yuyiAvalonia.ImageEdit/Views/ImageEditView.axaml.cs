using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using yuyiAvalonia.ImageEdit.ViewModels;
using yuyiAvalonia.ImageEdit.Common;

namespace yuyiAvalonia.ImageEdit.Views;

public partial class ImageEditView : UserControl
{
    private Point? _lastPointerPosition;
    private bool _isPanning;
    private Polyline? _currentPolyline;
    private Point? _pendingTextPosition;

    public ImageEditView()
    {
        InitializeComponent();

        // 确保控件可以接收焦点以处理键盘事件
        Focusable = true;

        // 启用拖放功能（使用附加属性方式）
        DragDrop.SetAllowDrop(this, true);
        DragDrop.SetAllowDrop(ImageContainer, true);

        // 注册拖放事件
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        AddHandler(DragDrop.DropEvent, OnDrop);
        ImageContainer.AddHandler(DragDrop.DragOverEvent, OnDragOver);
        ImageContainer.AddHandler(DragDrop.DropEvent, OnDrop);

        TextInputBox.MyLostFocusEvent += TextInputBox_MyLostFocusEvent;
    }

    /// <summary>
    /// 处理文件拖入事件
    /// </summary>
    private void OnDragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null && files.Any(f => IsImageFile(f.Path.LocalPath)))
            {
                e.DragEffects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        e.DragEffects = DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// 处理文件拖动悬停事件
    /// </summary>
    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null && files.Any(f => IsImageFile(f.Path.LocalPath)))
            {
                e.DragEffects = DragDropEffects.Copy;
                e.Handled = true;
                return;
            }
        }
        e.DragEffects = DragDropEffects.None;
        e.Handled = true;
    }

    /// <summary>
    /// 处理文件放下事件
    /// </summary>
    private async void OnDrop(object? sender, DragEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm)
            return;

        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files != null)
            {
                // 查找第一个图片文件
                foreach (var file in files)
                {
                    if (IsImageFile(file.Path.LocalPath))
                    {
                        await vm.LoadImageFromPath(file.Path.LocalPath);
                        e.Handled = true;
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查文件是否为支持的图片格式
    /// </summary>
    private static bool IsImageFile(string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath??"").ToLower();
        return extension is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".webp" or ".tiff" or ".tif";
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        // DataContext 设置后订阅选择框事件
        if (DataContext is ImageEditViewModel vm)
        {
            // 延迟到控件加载完成后初始化
            Loaded += OnViewLoaded;
        }
    }

    private void OnViewLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is ImageEditViewModel vm)
        {
            // 订阅选择框调整事件
            SelectionOverlay.SelectionChanged += OnSelectionChanged;

            // 订阅滚轮事件用于缩放
            SelectionOverlay.WheelChanged += e =>
            {
                OnPointerWheelChanged(SelectionOverlay, e);
            };

            // 订阅 ViewModel 的选择框变化事件
            vm.SelectionRectChanged += newRect =>
            {
                // ViewModel 更新了选择框，UI 已经通过绑定自动更新
                // 这里不需要额外操作
            };

            // 订阅图片列表选中项变化事件
            vm.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(vm.SelectedImageItem) && vm.SelectedImageItem != null)
                {
                    _ = vm.SelectImageFromListAsync(vm.SelectedImageItem);
                }
            };
        }
    }

    /// <summary>
    /// 选择框调整回调
    /// </summary>
    private void OnSelectionChanged(Rect newRect)
    {
        if (DataContext is ImageEditViewModel vm)
        {
            // SelectionOverlay 返回的是图像坐标，直接使用
            vm.SelectionStart = newRect.TopLeft;
            vm.SelectionEnd = newRect.BottomRight;
            vm.UpdateSelectionRect();
        }
    }

    private void TextInputBox_MyLostFocusEvent(object? sender, EventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm || !vm.HasImage)
            return;

        // 获取输入的文字
        var text = TextInputBox.Text;
        if (!string.IsNullOrWhiteSpace(text) && _pendingTextPosition.HasValue)
        {
            // 使用 RichTextInputControl 的绘制方法将文字应用到图像
            ApplyTextToImageWithControl(text, vm);
        }
        else
        {
            vm.CancelAddText();
        }

        // 隐藏输入框
        BrushCanvas.IsVisible = false;
        _pendingTextPosition = null;
    }

    /// <summary>
    /// 使用 RichTextInputControl 的绘制方法将文字应用到图像
    /// </summary>
    private void ApplyTextToImageWithControl(string text, ImageEditViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(text) || !vm.HasImage)
            return;

        try
        {
            var sourceBitmap = vm.CurrentImageEditItem?.Image;
            if (sourceBitmap == null)
                return;

            var srcBitmap = sourceBitmap.Copy();

            var width = srcBitmap.Width;
            var height = srcBitmap.Height;

            // 创建目标 SkiaSharp 位图
            var destBitmap = new SkiaSharp.SKBitmap(width, height);
            using (var canvas = new SkiaSharp.SKCanvas(destBitmap))
            {
                // 绘制原始图像
                canvas.DrawBitmap(srcBitmap, 0, 0);

                // 使用 RichTextInputControl 的绘制方法
                var textColor = new SkiaSharp.SKColor(
                    vm.CurrentTextSettings.FontColor.R,
                    vm.CurrentTextSettings.FontColor.G,
                    vm.CurrentTextSettings.FontColor.B,
                    vm.CurrentTextSettings.FontColor.A);

                // 设置 RichTextInputControl 的属性与输入框一致
                // 注意：FontSize 使用原始值（非 Display），绘制时 scale = 1.0
                TextInputBox.FontSize = vm.CurrentTextSettings.FontSize;
                TextInputBox.FontFamily = new Avalonia.Media.FontFamily(vm.CurrentTextSettings.FontFamilyName ?? "Arial");
                TextInputBox.FontWeight = vm.CurrentTextSettings.IsBold ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
                TextInputBox.FontStyle = vm.CurrentTextSettings.IsItalic ? Avalonia.Media.FontStyle.Italic : Avalonia.Media.FontStyle.Normal;

                // 绘制位置：显示坐标需要转换为原始图片坐标（除以 ZoomLevel）
                var displayPosition = new Avalonia.Point(
                    vm.TextInsertPointDisplay.X,
                    vm.TextInsertPointDisplay.Y);
                var imagePosition = new SkiaSharp.SKPoint(
                    (float)((displayPosition.X-6) / vm.ZoomLevel),
                    (float)((displayPosition.Y-8) / vm.ZoomLevel));

                // 使用原始图片坐标绘制（已除以 ZoomLevel 转换）
                TextInputBox.DrawTextToCanvas(canvas, text, imagePosition, 1.0, textColor);
            }

            // 释放源位图副本
            srcBitmap.Dispose();

            // 使用 ViewModel 的方法来应用文字
            vm.EndAddText(text, destBitmap);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying text to image: {ex.Message}");
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm || !vm.HasImage)
            return;

        // 计算缩放增量（使用对数缩放使缩放更平滑）
        double zoomDelta = e.Delta.Y > 0 ? 1.1 : 0.9;
        double newZoom = vm.ZoomLevel * zoomDelta;
        newZoom = System.Math.Clamp(newZoom, vm.MinZoom, vm.MaxZoom);

        if (System.Math.Abs(newZoom - vm.ZoomLevel) > 0.001)
        {
            vm.ZoomLevel = newZoom;
        }

        e.Handled = true;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {

        if (DataContext is not ImageEditViewModel vm || !vm.HasImage)
            return;

        var point = e.GetCurrentPoint(this);

        // 画笔模式：开始绘制
        if (point.Properties.IsLeftButtonPressed &&
            (e.Source is Image || e.Source is Canvas) &&
            vm.CurrentToolMode == EditToolMode.Brush)
        {
            TextInputBox.IsVisible = false;
            var imagePoint = e.GetPosition(DisplayImage);
            vm.StartBrushStroke(imagePoint);

            // 创建 Polyline 来显示当前笔触
            Dispatcher.UIThread.Post(() =>
            {
                if (BrushCanvas != null)
                {

                    // 确保画布可见
                    BrushCanvas.IsVisible = true;

                    // 线宽随缩放调整
                    var scaledStrokeWidth = vm.CurrentBrushSettings.StrokeWidthDisplay;
                    _currentPolyline = new Polyline
                    {
                        Stroke = new SolidColorBrush(vm.CurrentBrushSettings.StrokeColor),
                        StrokeThickness = scaledStrokeWidth,
                        Opacity = vm.CurrentBrushSettings.Opacity,
                        IsHitTestVisible = false
                    };
                    _currentPolyline.Points.Add(imagePoint);
                    BrushCanvas.Children.Add(_currentPolyline);

                    lastPoint = imagePoint;
                }
            });

            e.Handled = true;
            return;
        }

        // 文字模式：点击输入文字
        if (point.Properties.IsLeftButtonPressed && e.Source is Image &&
            vm.CurrentToolMode == EditToolMode.Text)
        {

            var imagePoint = e.GetPosition(DisplayImage);
            _pendingTextPosition = imagePoint;

            // 调用 ViewModel 开始添加文字（设置插入点）
            vm.StartAddText(imagePoint);

            // 设置文字输入框位置
            Dispatcher.UIThread.Post(() =>
            {
                // 将 TextBox 添加到 BrushCanvas 中
                if (!BrushCanvas.Children.Contains(TextInputBox))
                {
                    BrushCanvas.Children.Add(TextInputBox);
                }

                // 同步文字设置到输入框
                TextInputBox.FontSize = vm.CurrentTextSettings.FontSize;
                TextInputBox.FontFamily = new Avalonia.Media.FontFamily(vm.CurrentTextSettings.FontFamilyName ?? "Arial");
                TextInputBox.FontWeight = vm.CurrentTextSettings.IsBold ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
                TextInputBox.FontStyle = vm.CurrentTextSettings.IsItalic ? Avalonia.Media.FontStyle.Italic : Avalonia.Media.FontStyle.Normal;

                TextInputBox.Text = string.Empty;
                TextInputBox.IsVisible = true;
                BrushCanvas.IsVisible = true;
                TextInputBox.Focus();
            });

            e.Handled = true;
            return;
        }
        else
        {
            BrushCanvas.IsVisible = false;
        }

        // 中键或 Ctrl+左键：平移模式
        if (point.Properties.IsMiddleButtonPressed ||
            (point.Properties.IsLeftButtonPressed && e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            _isPanning = true;
            _lastPointerPosition = e.GetPosition(this);
            e.Handled = true;
            return;
        }

        // 左键：在选择/裁剪模式下开始框选
        vm.UseSelection = false;
        if (point.Properties.IsLeftButtonPressed &&
            (vm.CurrentToolMode == EditToolMode.Select
            //|| vm.CurrentToolMode == EditToolMode.Crop
            ))
        {
            var imagePoint = e.GetPosition(DisplayImage);
            vm.StartSelection(imagePoint);
            e.Handled = true;
            return;
        }
    }

    Point lastPoint;
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm)
            return;

        // 画笔模式：绘制路径
        if (vm.IsDrawingBrush)
        {
            var imagePoint = e.GetPosition(DisplayImage);
            if (lastPoint != default && lastPoint.Equals(imagePoint))
            {
                e.Handled = true;
                return;
            }
            vm.ContinueBrushStroke(imagePoint);

            // 仅更新 Polyline 预览显示，不实时应用到图像（提高性能）
            Dispatcher.UIThread.Post(() =>
            {
                if (_currentPolyline != null && vm.IsDrawingBrush)
                {
                    _currentPolyline.Points.Add(imagePoint);
                }
            });
            lastPoint = imagePoint;
            e.Handled = true;
            return;
        }

        // 正在平移（ScrollViewer 会自动处理滚动，这里不需要额外操作）
        if (_isPanning && _lastPointerPosition != null)
        {
            _lastPointerPosition = e.GetPosition(this);
            e.Handled = true;
            return;
        }

        // 正在绘制选择框
        if (vm.IsSelecting)
        {
            var imagePoint = e.GetPosition(DisplayImage);
            vm.UpdateSelection(imagePoint);
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm)
            return;

        // 画笔模式：结束绘制
        if (vm.IsDrawingBrush)
        {
            vm.EndBrushStroke();

            // 清除当前的 Polyline
            Dispatcher.UIThread.Post(() =>
            {
                if (BrushCanvas != null)
                {
                    BrushCanvas.Children.Clear();
                }
                _currentPolyline = null;
            });

            e.Handled = true;
        }

        if (_isPanning)
        {
            _isPanning = false;
            _lastPointerPosition = null;
            e.Handled = true;
        }

        if (vm.IsSelecting)
        {
            vm.EndSelection();
            e.Handled = true;
        }

    }

    private void ImageContainer_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is ImageEditViewModel vm)
        {
            vm.ContainerSize = ImageContainer.Bounds.Size;
            vm.RefreshComputedProperties();
        }
    }

    private void OnTextInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm)
            return;

        if (e.Key == Key.Escape)
        {
            // 取消输入
            vm.CancelAddText();
            BrushCanvas.IsVisible = false;
            _pendingTextPosition = null;
            e.Handled = true;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not ImageEditViewModel vm)
            return;

        // 在文字模式下按 Escape 取消
        if (e.Key == Key.Escape && vm.CurrentToolMode == EditToolMode.Text)
        {
            vm.CancelAddText();
            BrushCanvas.IsVisible = false;
            _pendingTextPosition = null;
            e.Handled = true;
        }
    }

}
