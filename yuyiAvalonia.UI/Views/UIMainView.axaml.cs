using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Views;

public partial class UIMainView : UserControl
{
    public UIMainView()
    {
        InitializeComponent();
    }

    #region 分类选择

    private void OnCategoryPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is CategoryItem category)
        {
            if (DataContext is UIMainViewModel vm)
            {
                vm.SelectedCategory = category;
            }
        }
    }

    #endregion

    #region 模板操作

    private void OnTemplatePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is TemplateItem template)
        {
            if (DataContext is UIMainViewModel vm)
            {
                vm.SelectTemplate(template);
            }
        }
    }

    private void OnQuickCreatePressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;

        if (sender is Border border && border.DataContext is TemplateItem template)
        {
            if (DataContext is UIMainViewModel vm)
            {
                vm.QuickCreate(template);
                ShowQuickCreateDialog(template);
            }
        }
        else if (sender is Button button && button.Tag is string templateId)
        {
            if (DataContext is UIMainViewModel vm)
            {
                var newTemplate = vm.Templates.FirstOrDefault(t => t.Id == templateId);
                if (newTemplate != null)
                {
                    vm.QuickCreate(newTemplate);
                    ShowQuickCreateDialog(newTemplate);
                }
            }
        }
    }

    #endregion

    #region 原型设计

    private void OnNewPrototypePressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is UIMainViewModel vm)
        {
            vm.OpenPrototypeDesign();
        }
    }

    private void OnQuickTemplatePressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is TemplateItem template)
        {
            if (DataContext is UIMainViewModel vm)
            {
                vm.SelectTemplate(template);
            }
        }
    }

    private void OnOpenPrototypeDesign(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is UIMainViewModel vm)
        {
            vm.OpenPrototypeDesign();
        }
    }

    #endregion

    #region 对话框

    private void ShowQuickCreateDialog(TemplateItem template)
    {
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            var dialog = new Window
            {
                Title = "创建项目",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var grid = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto,Auto"),
                Margin = new Thickness(24)
            };

            var contentPanel = new StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 16
            };

            var iconBorder = new Border
            {
                Width = 72,
                Height = 72,
                CornerRadius = new CornerRadius(16),
                Background = new SolidColorBrush(Avalonia.Media.Color.Parse(template.BackgroundColor ?? "#1890FF")),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var iconText = new TextBlock
            {
                Text = template.Icon,
                FontSize = 28,
                FontWeight = FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            iconBorder.Child = iconText;

            var titleBlock = new TextBlock
            {
                Text = $"创建「{template.Name}」项目",
                FontSize = 18,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var descBlock = new TextBlock
            {
                Text = template.Description,
                FontSize = 13,
                Opacity = 0.7,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 320
            };

            contentPanel.Children.Add(iconBorder);
            contentPanel.Children.Add(titleBlock);
            contentPanel.Children.Add(descBlock);

            Grid.SetRow(contentPanel, 0);

            var inputPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 16, 0, 24)
            };

            var nameLabel = new TextBlock
            {
                Text = "项目名称:",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };

            var nameInput = new TextBox
            {
                Width = 200,
                Watermark = $"My{template.Name?.Replace(" ", "")}",
                Text = $"My{template.Name?.Replace(" ", "")}"
            };

            inputPanel.Children.Add(nameLabel);
            inputPanel.Children.Add(nameInput);

            Grid.SetRow(inputPanel, 1);

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                Spacing = 12
            };

            var cancelButton = new Button
            {
                Content = "取消",
                Padding = new Thickness(24, 10)
            };
            cancelButton.Click += (s, args) => dialog.Close();

            var createButton = new Button
            {
                Content = "创建",
                Padding = new Thickness(24, 10)
            };
            createButton.Classes.Add("Accent");

            createButton.Click += (s, args) =>
            {
                var projectName = string.IsNullOrWhiteSpace(nameInput.Text)
                    ? $"My{template.Name?.Replace(" ", "")}"
                    : nameInput.Text;

                OnProjectCreated(template, projectName);
                dialog.Close();
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);

            Grid.SetRow(buttonPanel, 2);

            grid.Children.Add(contentPanel);
            grid.Children.Add(inputPanel);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;
            dialog.ShowDialog(window);
        }
    }

    private void OnProjectCreated(TemplateItem template, string projectName)
    {
        if (DataContext is UIMainViewModel vm)
        {
            vm.SelectTemplate(template);
        }

        System.Diagnostics.Debug.WriteLine($"项目已创建: {projectName} (模板: {template.Name})");
    }

    #endregion
}
