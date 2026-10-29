using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Views;

namespace yuyiAvalonia.UI.ViewModels;

public partial class UITabPagesViewModel : PageViewModelBase
{
    SystemContext? _systemContext;

    [ObservableProperty]
    private ObservableCollection<TabItemViewModel> _tabs = new();

    [ObservableProperty]
    private TabItemViewModel? _selectedTab;

    private UIMainViewModel? _mainViewModel;

    public override void Initialize(SystemContext? context)
    {
        _systemContext = context;

        _mainViewModel = new UIMainViewModel();
        _mainViewModel.Initialize(context);
        _mainViewModel.TemplateSelected += OnTemplateSelected;

        var homeTab = new TabItemViewModel
        {
            Id = "home",
            Title = "首页",
            Icon = "🏠",
            CanClose = false,
            Content = new Views.UIMainView { DataContext = _mainViewModel }
        };

        Tabs.Add(homeTab);
        SelectedTab = homeTab;
    }

    private void OnTemplateSelected(TemplateItem template)
    {
        OpenFlowEditorTab(template);
    }

    public void OpenFlowEditorTab(TemplateItem template)
    {
        var existingTab = Tabs.FirstOrDefault(t => t.TemplateId == template.Id);
        if (existingTab != null)
        {
            SelectedTab = existingTab;
            return;
        }

        var editorVm = new FlowEditorViewModel() { TemplateItem = template };
        var editorContent = new FlowEditorView { DataContext = editorVm };
        editorVm.Initialize(_systemContext);

        var editorTab = new TabItemViewModel
        {
            Id = Guid.NewGuid().ToString(),
            TemplateId = template.Id,
            Title = template.Name ?? "流程图设计",
            Icon = template.Icon,
            CanClose = true,
            Content = editorContent
        };

        Tabs.Add(editorTab);
        SelectedTab = editorTab;
    }

    public void CloseTab(TabItemViewModel tab)
    {
        if (!tab.CanClose) return;

        var index = Tabs.IndexOf(tab);
        Tabs.Remove(tab);

        if (Tabs.Count > 0)
        {
            if (index >= Tabs.Count)
                SelectedTab = Tabs[^1];
            else
                SelectedTab = Tabs[index];
        }
    }

    public override void UpdateLanguageTexts()
    {
    }

    public override void Dispose()
    {
        if (_mainViewModel != null)
        {
            _mainViewModel.TemplateSelected -= OnTemplateSelected;
            _mainViewModel.Dispose();
        }

        foreach (var tab in Tabs)
        {
            if (tab.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
        Tabs.Clear();
    }
}

public partial class TabItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string? _templateId;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _icon;

    [ObservableProperty]
    private bool _canClose;

    [ObservableProperty]
    private object? _content;
}
