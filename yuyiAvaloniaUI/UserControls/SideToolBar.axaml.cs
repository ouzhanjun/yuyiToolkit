using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace yuyiAvaloniaUI;

public partial class SideToolBar : UserControl
{
    public SideToolBar()
    {
        InitializeComponent();
    }

    private void MenuFlyout_Opened(object? sender, System.EventArgs e)
    {
        MenuFlyout? flyout = sender as MenuFlyout;

        if (flyout != null)
        {
            if (flyout.Items.Count == 0)
            {
                flyout.Hide();
            }
        }
    }

}