using Avalonia.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class EditorTemplatePanel : UserControl
{
    public event EventHandler? WhenLandingPageTemplateRequested;
    public event EventHandler? WhenFormTemplateRequested;
    public event EventHandler? WhenDashboardTemplateRequested;

    public EditorTemplatePanel()
    {
        InitializeComponent();
    }

    private void OnLandingPagePressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        WhenLandingPageTemplateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnFormPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        WhenFormTemplateRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnDashboardPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        WhenDashboardTemplateRequested?.Invoke(this, EventArgs.Empty);
    }
}
