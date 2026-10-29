using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Renders
{
    /// <summary>
    /// 画布预览对话框 - 根据层次结构渲染画布元素
    /// 渲染逻辑：使用 CreateElementControlForPreview 创建完整元素树（已递归包含所有子元素），
    /// 只将顶级元素添加到预览画布（子元素已在控件内部渲染）
    /// </summary>
    public class CanvasPreviewDialog : Window
    {
        private readonly Canvas _previewCanvas;
        private readonly Border _canvasBackgroundBorder;
        private readonly Border _canvasBackgroundImageBorder;
        private readonly ScrollViewer _scrollViewer;
        private readonly Border _canvasContainer;

        /// <summary>
        /// 元素控件创建委托，用于复用 ContainerManager 的控件创建逻辑
        /// 返回 (控件, 内部容器Panel)
        /// </summary>
        public Func<CanvasElement, (Control?, Panel?)>? CreateElementControl { get; set; }

        /// <summary>
        /// 画布属性（背景色等）
        /// </summary>
        public CanvasProperty? CanvasProperty
        {
            get => _canvasProperty;
            set
            {
                if (_canvasProperty != null)
                    _canvasProperty.WhenBackgroundChanged -= OnCanvasBackgroundChanged;

                _canvasProperty = value;

                if (_canvasProperty != null)
                {
                    _canvasProperty.WhenBackgroundChanged += OnCanvasBackgroundChanged;
                    ApplyCanvasBackground();
                    ApplyBackgroundImage();
                }
            }
        }

        private CanvasProperty? _canvasProperty;

        public CanvasPreviewDialog()
        {
            Title = "画布预览";
            Width = 800;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = true;
            MinWidth = 400;
            MinHeight = 300;

            // 根据主题选择颜色
            var isDark = Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;
            Background = isDark
                ? new SolidColorBrush(Color.Parse("#1E293B"))
                : new SolidColorBrush(Color.Parse("#F8FAFC"));

            // 顶部工具栏
            var toolbar = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Thickness(10),
                Spacing = 10
            };

            var zoomLabel = new TextBlock
            {
                Text = "缩放:",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var zoomSlider = new Slider
            {
                Minimum = 0.1,
                Maximum = 3.0,
                Value = 1.0,
                Width = 200,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var zoomText = new TextBlock
            {
                Text = "100%",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Width = 50
            };

            zoomSlider.ValueChanged += (s, e) =>
            {
                zoomText.Text = $"{(int)(e.NewValue * 100)}%";
                _previewCanvas.RenderTransform = new ScaleTransform(e.NewValue, e.NewValue);
            };

            var exportButton = new Button
            {
                Content = "导出图片",
                Padding = new Thickness(15, 5)
            };
            exportButton.Click += OnExportClick;

            var closeButton = new Button
            {
                Content = "关闭",
                Padding = new Thickness(15, 5)
            };
            closeButton.Click += (s, e) => Close();

            toolbar.Children.Add(zoomLabel);
            toolbar.Children.Add(zoomSlider);
            toolbar.Children.Add(zoomText);
            toolbar.Children.Add(exportButton);
            toolbar.Children.Add(closeButton);

            // 画布容器 - 铺满整个区域，画布居中显示
            _canvasContainer = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#E2E8F0")),
                BorderBrush = new SolidColorBrush(Color.Parse("#CBD5E1")),
                BorderThickness = new Thickness(1),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                Padding = new Thickness(2)
            };

            // 背景画布面板（包含背景层和背景图层）
            var backgroundPanel = new Panel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            // 预览画布背景层
            _canvasBackgroundBorder = new Border
            {
                Background = Brushes.White
            };

            // 预览画布背景图层
            _canvasBackgroundImageBorder = new Border
            {
                IsHitTestVisible = false
            };

            // 预览画布
            _previewCanvas = new Canvas
            {
                Background = Brushes.Transparent,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            backgroundPanel.Children.Add(_canvasBackgroundBorder);
            backgroundPanel.Children.Add(_canvasBackgroundImageBorder);
            backgroundPanel.Children.Add(_previewCanvas);

            _canvasContainer.Child = backgroundPanel;

            // 滚动查看器
            _scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Content = _canvasContainer
            };

            // 布局
            var mainPanel = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                Children = { toolbar, _scrollViewer }
            };
            Grid.SetRow(_scrollViewer, 1);

            Content = mainPanel;
        }

        /// <summary>
        /// 设置画布尺寸
        /// </summary>
        public void SetCanvasSize(double width, double height)
        {
            _previewCanvas.Width = width;
            _previewCanvas.Height = height;
            _canvasBackgroundBorder.Width = width;
            _canvasBackgroundBorder.Height = height;
            _canvasBackgroundImageBorder.Width = width;
            _canvasBackgroundImageBorder.Height = height;
        }

        /// <summary>
        /// 渲染画布元素集合
        /// 逻辑：使用 CreateElementControlForPreview 创建完整元素树（已包含所有子元素），
        /// 只渲染顶级元素（不在任何容器 Children 中的元素），然后添加到画布
        /// </summary>
        public void RenderElements(ObservableCollection<CanvasElement> elements, CanvasProperty? canvasProperty)
        {
            CanvasProperty = canvasProperty;

            _previewCanvas.Children.Clear();
            SetCanvasSize(CanvasProperty?.CanvasWidth ?? 375, CanvasProperty?.CanvasHeight ?? 667);

            ApplyCanvasBackground();
            ApplyBackgroundImage();

            if (CreateElementControl == null)
                return;

            // 构建所有子元素的 ID 集合（用于判断哪些是顶级元素）
            var childElementIds = new HashSet<string>();
            CollectAllChildIds(elements, childElementIds);

            // 只渲染顶级元素（不在任何容器 Children 中的元素）
            foreach (var element in elements)
            {
                if (!childElementIds.Contains(element.Id))
                {
                    // CreateElementControlForPreview 已经递归创建了完整元素树（包括所有子元素）
                    var (control, _) = CreateElementControl(element);
                    if (control != null)
                    {
                        // 设置位置（与 RefreshCanvasElements 保持一致）
                        element.Left = element.X;
                        element.Top = element.Y;

                        Canvas.SetLeft(control, element.Left);
                        Canvas.SetTop(control, element.Top);

                        _previewCanvas.Children.Add(control);
                    }
                }
            }
        }

        /// <summary>
        /// 应用画布背景属性（填充，不包含背景图）
        /// </summary>
        private void ApplyCanvasBackground()
        {
            var canvasProperty = CanvasProperty;
            if (canvasProperty == null)
            {
                _canvasBackgroundBorder.Background = Brushes.White;
                return;
            }

            switch (canvasProperty.FillType)
            {
                case BackgroundType.SolidColor:
                    _canvasBackgroundBorder.Background = CreateColorBrush(canvasProperty.CanvasBackgroundColor);
                    break;

                case BackgroundType.Gradient:
                    _canvasBackgroundBorder.Background = CreateGradientBrush(
                        canvasProperty.GradientStartColor,
                        canvasProperty.GradientEndColor,
                        canvasProperty.GradientAngle);
                    break;

                case BackgroundType.None:
                    _canvasBackgroundBorder.Background = Brushes.Transparent;
                    break;

                default:
                    _canvasBackgroundBorder.Background = Brushes.White;
                    break;
            }
        }

        /// <summary>
        /// 应用背景图叠加
        /// </summary>
        private void ApplyBackgroundImage()
        {
            var canvasProperty = CanvasProperty;
            if (canvasProperty == null)
            {
                _canvasBackgroundImageBorder.Background = Brushes.Transparent;
                return;
            }

            if (!canvasProperty.EnableBackgroundImage)
            {
                _canvasBackgroundImageBorder.Background = Brushes.Transparent;
                return;
            }

            if (string.IsNullOrEmpty(canvasProperty.CanvasBackgroundImage) ||
                !File.Exists(canvasProperty.CanvasBackgroundImage))
            {
                // 图片不存在或未设置时保持背景透明
                return;
            }

            try
            {
                var bitmap = new Bitmap(canvasProperty.CanvasBackgroundImage);
                var imageBrush = new ImageBrush(bitmap)
                {
                    TileMode = canvasProperty.BackgroundImageTileMode,
                    AlignmentX = canvasProperty.BackgroundImageAlignmentX,
                    AlignmentY = canvasProperty.BackgroundImageAlignmentY
                };

                // TileMode 需要 Stretch=None 才生效
                if (canvasProperty.BackgroundImageTileMode != TileMode.None)
                {
                    imageBrush.Stretch = Stretch.None;
                    // 设置 DestinationRect 为图片原始尺寸，实现真正的平铺
                    imageBrush.DestinationRect = new RelativeRect(
                        0, 0,
                        (double)bitmap.Size.Width,
                        (double)bitmap.Size.Height,
                        RelativeUnit.Absolute);
                }
                else
                {
                    imageBrush.Stretch = canvasProperty.BackgroundImageStretch;
                }

                _canvasBackgroundImageBorder.Background = imageBrush;
            }
            catch
            {
                // 加载失败时保持背景透明
            }
        }

        /// <summary>
        /// 画布背景属性变化时自动更新预览
        /// </summary>
        private void OnCanvasBackgroundChanged(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ApplyCanvasBackground();
                ApplyBackgroundImage();
            });
        }

        private IBrush CreateColorBrush(string? color)
        {
            if (string.IsNullOrEmpty(color))
                return Brushes.White;

            try
            {
                return new SolidColorBrush(Color.Parse(color));
            }
            catch
            {
                return Brushes.White;
            }
        }

        private IBrush CreateGradientBrush(string startColor, string endColor, double angle)
        {
            // 与 FillItemToGradientBrushConverter（预览）保持一致：以 (0.5, 0.5) 为对称中心
            // 沿角度方向对称偏移 0.5，不 clamp，保证任意角度下向量长度恒为 1，
            // 渐变覆盖整个形状而不是被截到一半。
            var angleRad = angle * Math.PI / 180;
            var dx = Math.Cos(angleRad);
            var dy = Math.Sin(angleRad);

            Color start, end;
            try { start = Color.Parse(startColor); } catch { start = Colors.White; }
            try { end = Color.Parse(endColor); } catch { end = Colors.Black; }

            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                EndPoint   = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(start, 0),
                    new GradientStop(end, 1)
                }
            };
        }

        /// <summary>
        /// 递归收集所有子元素的 ID
        /// </summary>
        private void CollectAllChildIds(ObservableCollection<CanvasElement> elements, HashSet<string> childIds)
        {
            foreach (var element in elements)
            {
                if (element.Children != null)
                {
                    foreach (var child in element.Children)
                    {
                        childIds.Add(child.Id);
                        // 递归收集更深层的子元素
                        CollectChildIdsRecursive(child, childIds);
                    }
                }
            }
        }

        /// <summary>
        /// 递归收集单个元素的子元素 ID
        /// </summary>
        private void CollectChildIdsRecursive(CanvasElement element, HashSet<string> childIds)
        {
            if (element.Children != null)
            {
                foreach (var child in element.Children)
                {
                    childIds.Add(child.Id);
                    CollectChildIdsRecursive(child, childIds);
                }
            }
        }

        private async void OnExportClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "导出画布图片",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PNG 图片")
                    {
                        Patterns = new[] { "*.png" }
                    }
                },
                SuggestedFileName = $"画布预览_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            });

            if (file != null)
            {
                try
                {
                    // 渲染包含背景的完整画布
                    var backgroundPanel = _previewCanvas.Parent as Panel;
                    if (backgroundPanel == null)
                    {
                        throw new Exception("无法获取画布容器");
                    }

                    using var renderBitmap = new RenderTargetBitmap(
                        new PixelSize((int)backgroundPanel.Bounds.Width, (int)backgroundPanel.Bounds.Height),
                        new Vector(96, 96));
                    renderBitmap.Render(backgroundPanel);

                    using var stream = File.OpenWrite(file.Path.LocalPath);
                    renderBitmap.Save(stream);

                    await new Window
                    {
                        Title = "导出成功",
                        Content = new TextBlock
                        {
                            Text = $"图片已导出到:\n{file.Path.LocalPath}",
                            Margin = new Thickness(20),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        Width = 400,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    await new Window
                    {
                        Title = "导出失败",
                        Content = new TextBlock
                        {
                            Text = $"导出失败:\n{ex.Message}",
                            Margin = new Thickness(20),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap
                        },
                        Width = 400,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    }.ShowDialog(this);
                }
            }
        }
    }
}
