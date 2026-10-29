using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using yuyiAvalonia.Base.Models;
using yuyiAvalonia.UI.ElementControls.Controls;

namespace yuyiAvaloniaUI.Views
{
    public partial class MainWindow : Window
    {
        private TrayIcon? _trayIcon;
        MainContainerFourView? _mainContent;
        private List<string> _openedImages = new();

        public MainWindow()
        {
            InitializeComponent();

            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            if (AppSettings.Instance.MinimizeToTray.Value)
            {
                e.Cancel = true;
                Hide();
                EnsureTrayIcon();
            }
            else
            {
                _trayIcon?.Dispose();
            }
        }

        private void EnsureTrayIcon()
        {
            if (_trayIcon != null) return;

            var trayMenu = CreateTrayMenu();

            _trayIcon = new TrayIcon
            {
                ToolTipText = "绘图工具箱",
                Menu = trayMenu,
                Icon = Icon
            };

            _trayIcon.Clicked += (s, e) => ShowMainWindow();
        }

        private NativeMenu CreateTrayMenu()
        {
            var trayMenu = new NativeMenu
            {
                new NativeMenuItem
                {
                    Header = "显示主窗口",
                    Command = new RelayCommand(ShowMainWindow)
                }
            };

            if (AppSettings.Instance.AddImageToSystemMenu.Value && _openedImages.Count > 0)
            {
                trayMenu.Add(new NativeMenuItemSeparator());

                var imagesMenu = new NativeMenuItem { Header = "已打开的图片" };

                foreach (var imagePath in _openedImages.Take(10))
                {
                    var imageItem = new NativeMenuItem
                    {
                        Header = System.IO.Path.GetFileName(imagePath),
                        Command = new RelayCommand(() => OpenImageFile(imagePath))
                    };
                    imagesMenu.Menu?.Items.Add(imageItem);
                }

                if (_openedImages.Count > 10)
                {
                    var moreItem = new NativeMenuItem { Header = $"...还有 {_openedImages.Count - 10} 个" };
                    imagesMenu.Menu?.Items.Add(moreItem);
                }

                trayMenu.Add(imagesMenu);
            }

            trayMenu.Add(new NativeMenuItemSeparator());
            trayMenu.Add(new NativeMenuItem
            {
                Header = "关闭",
                Command = new RelayCommand(CloseApplication)
            });

            return trayMenu;
        }

        public void AddImageToSystemMenu(string imagePath)
        {
            if (!AppSettings.Instance.AddImageToSystemMenu.Value)
                return;

            if (!_openedImages.Contains(imagePath))
            {
                _openedImages.Insert(0, imagePath);
                if (_openedImages.Count > 50)
                {
                    _openedImages.RemoveAt(_openedImages.Count - 1);
                }
            }

            if (_trayIcon != null)
            {
                _trayIcon.Menu = CreateTrayMenu();
            }
        }

        private void OpenImageFile(string path)
        {
            // 触发打开图片事件
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void CloseApplication()
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
            var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            desktop?.Shutdown();
        }

    }
}
