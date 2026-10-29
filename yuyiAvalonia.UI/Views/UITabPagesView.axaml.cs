using Avalonia.Controls;
using Avalonia.Input;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Views;

public partial class UITabPagesView : UserControl
{
    private UITabPagesViewModel? _viewModel;

    public UITabPagesView()
    {
        InitializeComponent();

        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        _viewModel = DataContext as UITabPagesViewModel;
    }

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is TabItemViewModel tabItem)
        {
            _viewModel?.CloseTab(tabItem);
        }
    }
}
