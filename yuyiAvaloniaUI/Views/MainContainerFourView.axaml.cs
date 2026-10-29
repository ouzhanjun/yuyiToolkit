using Avalonia.Controls;
using yuyiAvaloniaUI.ViewModels;

namespace yuyiAvaloniaUI.Views;

public partial class MainContainerFourView : UserControl
{
    TextBox Search;

    public MainContainerFourView()
    {
        InitializeComponent();
        Search = leftMenubar.Search;
    }

    private void SearchTextBox_OnKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        ((MainContainerFourViewModel?)this.DataContext)?.OnSearchMenu(((TextBox?)sender)?.Text);
    }

    private bool isLeftExpanded = false;
    private GridLength oldLeftLength = new GridLength(0);
    private bool isRightExpanded = false;
    private GridLength oldRightLength = new GridLength(0);
    private GridLength SplitLength = new GridLength(5);

    public bool IsLeftExpanded { get => isLeftExpanded; set => isLeftExpanded = value; }
    public bool IsRightExpanded { get => isRightExpanded; set => isRightExpanded = value; }

    public void CollapseLeftContent()
    {
        oldLeftLength = this.mainGrid.ColumnDefinitions[1].Width;
        this.mainGrid.ColumnDefinitions[1].Width = new GridLength(0);
        this.mainGrid.ColumnDefinitions[2].Width = new GridLength(0);
        Search.IsVisible = false;
        //btnShowLeft.IsChecked = false;
    }

    public void ExpandLeftContent()
    {
        if (oldLeftLength == new GridLength(0))
        {
            oldLeftLength = new GridLength(10);
        }
        this.mainGrid.ColumnDefinitions[1].Width = oldLeftLength;
        this.mainGrid.ColumnDefinitions[2].Width = SplitLength;
        Search.IsVisible = true;
        isLeftExpanded = true;
        //btnShowLeft.IsChecked = true;
    }

    public void CollapseRightContent()
    {
        oldRightLength = this.mainGrid.ColumnDefinitions[5].Width;
        this.mainGrid.ColumnDefinitions[4].Width = new GridLength(0);
        this.mainGrid.ColumnDefinitions[5].Width = new GridLength(0);
        RightSplitter.IsEnabled = false;
        isLeftExpanded = false;
        //btnShowLeft.IsChecked = false;
    }

    public void ExpandRightContent()
    {
        if (oldRightLength == new GridLength(0))
        {
            oldRightLength = new GridLength(10);
        }
        this.mainGrid.ColumnDefinitions[5].Width = oldRightLength;
        this.mainGrid.ColumnDefinitions[4].Width = SplitLength;
        RightSplitter.IsEnabled = true;
    }
}