using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace yuyiAvalonia.Base.Helpers
{
    public class AppHelper
    {
        public static void AppExit()
        {

            // 在 ViewModel 或代码中获取 ApplicationLifetime 并调用 Shutdown()
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            desktop?.Shutdown();
        }

        public static List<T> FindControlsOfType<T>(Visual parent) where T : Visual?
        {
            var result = new List<T>();
            if (parent == null) return result;

            var children = parent.GetVisualChildren();
            foreach (var child in children)
            {
                if (child is T t)
                    result.Add(t);

                if (child == null)
                    continue;
                // 递归查找子控件
                result.AddRange(FindControlsOfType<T>(child));
            }

            return result;
        }

        public static async Task<string[]> OpenFile()
        {
            List<string> lstFiles = new List<string>();
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;
            // 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
            var topLevel = TopLevel.GetTopLevel(window);

            // 启动异步操作以打开对话框。
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Text File",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                //// 打开第一个文件的读取流。
                //await using var stream = await files[0].OpenReadAsync();
                //using var streamReader = new StreamReader(stream);
                //// 将文件的所有内容作为文本读取。
                //var fileContent = await streamReader.ReadToEndAsync();
                foreach (var file in files)
                {
                    lstFiles.Add(file.Name);
                }
            }

            return lstFiles.ToArray();
        }

        public static async Task<string[]> OpenDirectory()
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            // 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
            var topLevel = TopLevel.GetTopLevel(window);

            // 启动异步操作以打开对话框。
            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Directory",
                AllowMultiple = false
            });

            return folders.Select(folder => folder.Name).ToArray();
        }

        public static async void SaveAsFile()
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            // 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
            var topLevel = TopLevel.GetTopLevel(window);

            // 启动异步操作以打开对话框。
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Text File"
            });

            if (file is not null)
            {
                // 打开文件的写入流。
                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);
                // 将一些内容写入文件。
                await streamWriter.WriteLineAsync("Hello World!");
            }
        }

        public static void OpenWithBrowser(string url)
        {
            try
            {
                // .NET Core 3.0+ 版本
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static void ChangeTheme()
        {
            if (Application.Current is null)
            {
                return;
            }

            Application.Current.RequestedThemeVariant =
                Application.Current.RequestedThemeVariant == Avalonia.Styling.ThemeVariant.Default ?
                ThemeVariant.Dark : ThemeVariant.Default;
        }

        public static async void ShowAboutDialog()
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            if (window == null) return;

            // 根据主题选择颜色
            var isDark = Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;
            var textColor = isDark ? new SolidColorBrush(Color.Parse("#F1F5F9")) : new SolidColorBrush(Color.Parse("#111827"));
            var bgColor = isDark ? new SolidColorBrush(Color.Parse("#1E293B")) : new SolidColorBrush(Color.Parse("#FFFFFF"));

            var dialog = new Window
            {
                Title = "关于",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = bgColor
            };

            var panel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            // 添加图标
            var icon = new Avalonia.Controls.Image
            {
                Width = 68,
                Height = 68,
                Margin = new Avalonia.Thickness(0, 0, 0, 15)
            };

            // 尝试加载应用图标，如果没有则使用默认图标
            try
            {
                icon.Source = new Bitmap("Assets/images/file.png");
            }
            catch
            {
                // 忽略图标加载错误
            }

            // 应用名称
            var titleText = new TextBlock
            {
                Text = "宇毅画图工具箱",
                FontSize = 22,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 6),
                Foreground = textColor
            };

            // 作者
            var authorText = new TextBlock
            {
                Text = "作者：欧詹军",
                FontSize = 13,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
                Foreground = textColor
            };

            // 版本号
            var versionText = new TextBlock
            {
                Text = "版本: 1.0",
                FontSize = 13,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
                Foreground = textColor
            };

            // 描述
            var descText = new TextBlock
            {
                Text = "一个强大的图像处理工具集合，支持多种图像编辑功能，包括液化变形、画笔、文字添加等。",
                FontSize = 12,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
                Foreground = textColor
            };

            // 关闭按钮
            var closeButton = new Button
            {
                Content = "确定",
                Width = 100,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            closeButton.Click += (s, e) => dialog.Close();

            panel.Children.Add(icon); 
            panel.Children.Add(authorText);
            panel.Children.Add(titleText);
            panel.Children.Add(versionText);
            panel.Children.Add(descText);
            panel.Children.Add(closeButton);

            dialog.Content = panel;

            await dialog.ShowDialog(window);
        }
    }
}
