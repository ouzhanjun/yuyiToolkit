using Avalonia.Controls;
using yuyiAvaloniaUI.ViewModels;

namespace yuyiAvaloniaUI;

public partial class LeftMenubar : UserControl
{

    public LeftMenubar()
    {
        InitializeComponent();
    }
   
    private void SearchTextBox_OnKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        ((MainContainerFourViewModel?)this.DataContext)?.OnSearchMenu(((TextBox?)sender)?.Text);
    }
}