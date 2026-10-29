using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using yuyiAvalonia.Base.Models;

namespace yuyiAvalonia.Base.Views
{
    public class OptionsDialog : Window
    {
        private readonly AppSettings _settings;
        private readonly List<CheckBox> _checkBoxes = new();
        private readonly List<ComboBox> _comboBoxes = new();

        private static bool IsDarkTheme => Application.Current?.RequestedThemeVariant == ThemeVariant.Dark;

        public OptionsDialog()
        {
            Title = "选项";
            Width = 500;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CanResize = false;
            Padding = new Thickness(20);
            Background = Avalonia.Media.Brushes.Transparent;

            _settings = AppSettings.Instance;

            var mainPanel = new StackPanel
            {
                Spacing = 15
            };

            // 标题
            var titleText = new TextBlock
            {
                Text = "应用程序选项",
                FontSize = 18,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                Foreground = IsDarkTheme ? new SolidColorBrush(Color.Parse("#F1F5F9")) : new SolidColorBrush(Color.Parse("#111827"))
            };

            // 创建 TabControl
            var tabControl = new TabControl
            {
                Margin = new Thickness(0, 10)
            };

            // 常规选项卡
            var generalTab = new TabItem
            {
                Header = "常规"
            };

            var generalContent = new ScrollViewer
            {
                Content = GenerateGeneralSettingsPanel(),
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Padding = new Thickness(10)
            };
            generalTab.Content = generalContent;

            // 外观选项卡
            var appearanceTab = new TabItem
            {
                Header = "外观"
            };

            var appearanceContent = new ScrollViewer
            {
                Content = GenerateAppearanceSettingsPanel(),
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                Padding = new Thickness(10)
            };
            appearanceTab.Content = appearanceContent;

            tabControl.Items.Add(generalTab);
            tabControl.Items.Add(appearanceTab);

            // 按钮面板
            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 0)
            };

            var okButton = new Button
            {
                Content = "确定",
                MinWidth = 80
            };
            okButton.Click += OnOkClick;

            var cancelButton = new Button
            {
                Content = "取消",
                MinWidth = 80
            };
            cancelButton.Click += OnCancelClick;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            mainPanel.Children.Add(titleText);
            mainPanel.Children.Add(tabControl);
            mainPanel.Children.Add(buttonPanel);

            Content = mainPanel;
        }

        private StackPanel GenerateGeneralSettingsPanel()
        {
            var panel = new StackPanel
            {
                Spacing = 10
            };

            foreach (var setting in _settings.GeneralSettings)
            {
                if (setting is BooleanSettingItem boolSetting)
                {
                    var container = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 10,
                        Margin = new Thickness(0, 5)
                    };

                    var checkBox = new CheckBox
                    {
                        Content = boolSetting.DisplayName,
                        IsChecked = boolSetting.Value,
                        Tag = boolSetting
                    };

                    checkBox.IsCheckedChanged += (s, e) =>
                    {
                        if (s is CheckBox cb && cb.Tag is BooleanSettingItem bs)
                        {
                            bs.Value = cb.IsChecked == true;
                        }
                    };

                    _checkBoxes.Add(checkBox);
                    container.Children.Add(checkBox);
                    panel.Children.Add(container);
                }
            }

            return panel;
        }

        private StackPanel GenerateAppearanceSettingsPanel()
        {
            var panel = new StackPanel
            {
                Spacing = 15
            };

            foreach (var setting in _settings.AppearanceSettings)
            {
                if (setting is OptionSettingItem optionSetting)
                {
                    var container = new StackPanel
                    {
                        Spacing = 8,
                        Margin = new Thickness(0, 5)
                    };

                    var label = new TextBlock
                    {
                        Text = optionSetting.DisplayName,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    container.Children.Add(label);

                    var comboBox = new ComboBox
                    {
                        Width = 200,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                        Tag = optionSetting
                    };

                    // 使用字典存储选项值和显示文本的映射
                    var optionItems = new Dictionary<string, ComboBoxItem>();

                    foreach (var option in optionSetting.Options)
                    {
                        var displayText = GetThemeDisplayName(option);
                        var item = new ComboBoxItem
                        {
                            Content = displayText,
                            Tag = option
                        };
                        optionItems[option] = item;
                        comboBox.Items.Add(item);
                    }

                    // 直接匹配并设置 SelectedItem
                    if (optionItems.TryGetValue(optionSetting.Value, out var targetItem))
                    {
                        comboBox.SelectedItem = targetItem;
                    }

                    comboBox.SelectionChanged += (s, e) =>
                    {
                        if (s is ComboBox cb && cb.SelectedItem is ComboBoxItem selectedItem)
                        {
                            optionSetting.Value = selectedItem.Tag?.ToString() ?? optionSetting.Value;
                        }
                    };

                    _comboBoxes.Add(comboBox);
                    container.Children.Add(comboBox);
                    panel.Children.Add(container);
                }
            }

            return panel;
        }

        private string GetThemeDisplayName(string theme)
        {
            return theme switch
            {
                "Default" => "默认主题",
                "Dark" => "深色主题",
                _ => theme
            };
        }

        private async void OnOkClick(object? sender, RoutedEventArgs e)
        {
            _settings.Save();
            Close(true);
        }

        private async void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            _settings.Load();
            UpdateUIFromSettings();
            Close(false);
        }

        private void UpdateUIFromSettings()
        {
            // 更新复选框
            foreach (var checkBox in _checkBoxes)
            {
                if (checkBox.Tag is BooleanSettingItem boolSetting)
                {
                    checkBox.IsChecked = boolSetting.Value;
                }
            }

            // 更新下拉框
            foreach (var comboBox in _comboBoxes)
            {
                if (comboBox.Tag is OptionSettingItem optionSetting)
                {
                    // 直接遍历 Items 集合查找匹配的项
                    foreach (var item in comboBox.Items)
                    {
                        if (item is ComboBoxItem comboItem &&
                            comboItem.Tag?.ToString() == optionSetting.Value)
                        {
                            comboBox.SelectedItem = comboItem;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 显示选项对话框
        /// </summary>
        public static async Task<bool> ShowAsync()
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                        ? desktop.MainWindow
                        : null;

            if (window == null)
                return false;

            var dialog = new OptionsDialog();
            var result = await dialog.ShowDialog<bool?>(window);
            return result == true;
        }
    }
}
