using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ViewModels;

public class UIMainViewModel : PageViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private SystemContext? _systemContext;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }

    #region 搜索和视图

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            OnPropertyChanged(nameof(FilteredTemplates));
        }
    }

    private bool _isGridView = true;
    public bool IsGridView
    {
        get => _isGridView;
        set
        {
            SetProperty(ref _isGridView, value);
            if (value) _isListView = false;
            OnPropertyChanged(nameof(IsListView));
        }
    }

    private bool _isListView = false;
    public bool IsListView
    {
        get => _isListView;
        set
        {
            SetProperty(ref _isListView, value);
            if (value) _isGridView = false;
            OnPropertyChanged(nameof(IsGridView));
        }
    }

    #endregion

    #region 模板数据

    public ObservableCollection<TemplateItem> Templates { get; } = new ObservableCollection<TemplateItem>
    {
        new TemplateItem
        {
            Id = "WeChatApp",
            Name = "微信小程序",
            Description = "创建微信小程序应用，支持快速生成项目结构",
            Icon = "💬",
            BackgroundColor = "#07C160",
            Category = "应用开发"
        },
        new TemplateItem
        {
            Id = "IoTDataPlatform",
            Name = "物联网数据平台",
            Description = "构建物联网数据采集、存储和分析平台",
            Icon = "📊",
            BackgroundColor = "#13C2C2",
            Category = "物联网开发"
        },
        new TemplateItem
        {
            Id = "WebDesign",
            Name = "网页设计",
            Description = "创建响应式网页设计，支持多种屏幕尺寸",
            Icon = "🌐",
            BackgroundColor = "#1890FF",
            Category = "设计"
        },
        new TemplateItem
        {
            Id = "Flowchart",
            Name = "流程图",
            Description = "创建流程图和工作流，可视化业务流程",
            Icon = "📊",
            BackgroundColor = "#FAAD14",
            Category = "图表"
        },
        new TemplateItem
        {
            Id = "MindMap",
            Name = "思维导图",
            Description = "创建思维导图和概念图，激发创意灵感",
            Icon = "🧠",
            BackgroundColor = "#722ED1",
            Category = "图表"
        },
        new TemplateItem
        {
            Id = "DesktopApp",
            Name = "桌面应用原型",
            Description = "创建桌面应用原型设计，支持多窗口布局",
            Icon = "🖥️",
            BackgroundColor = "#13C2C2",
            Category = "应用开发"
        },
        new TemplateItem
        {
            Id = "MobileDesign",
            Name = "移动设计",
            Description = "创建移动应用界面设计，支持 iOS/Android",
            Icon = "📱",
            BackgroundColor = "#EB2F96",
            Category = "设计"
        },
        new TemplateItem
        {
            Id = "Dashboard",
            Name = "数据仪表盘",
            Description = "创建数据可视化仪表盘，实时展示数据",
            Icon = "📈",
            BackgroundColor = "#FA541C",
            Category = "图表"
        },
        new TemplateItem
        {
            Id = "ECommerce",
            Name = "电商应用",
            Description = "创建电商应用界面，包含商品展示和购物车",
            Icon = "🛒",
            BackgroundColor = "#FA8C16",
            Category = "应用开发"
        },
        new TemplateItem
        {
            Id = "PrototypeDesign",
            Name = "原型设计",
            Description = "创建交互式原型设计，快速验证产品概念",
            Icon = "🎨",
            BackgroundColor = "#722ED1",
            Category = "设计"
        }
    };

    #endregion

    #region 分类数据

    public ObservableCollection<CategoryItem> Categories { get; } = new ObservableCollection<CategoryItem>
    {
        new CategoryItem { Name = "全部", Icon = "📦", IsSelected = true },
        new CategoryItem { Name = "物联网开发", Icon = "🏭", IsSelected = false },
        new CategoryItem { Name = "设计", Icon = "🎨", IsSelected = false },
        new CategoryItem { Name = "图表", Icon = "📊", IsSelected = false }
    };

    private CategoryItem? _selectedCategory;
    public CategoryItem? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            SetProperty(ref _selectedCategory, value);
            UpdateCategorySelection();
            OnPropertyChanged(nameof(FilteredTemplates));
        }
    }

    private void UpdateCategorySelection()
    {
        foreach (var cat in Categories)
        {
            cat.IsSelected = cat == SelectedCategory;
        }
    }

    #endregion

    #region 快速模板

    public ObservableCollection<TemplateItem> QuickTemplates { get; } = new ObservableCollection<TemplateItem>
    {
        new TemplateItem
        {
            Id = "WeChatApp",
            Name = "微信小程序",
            Icon = "💬",
            BackgroundColor = "#07C160",
            Category = "应用开发"
        },
        new TemplateItem
        {
            Id = "WebDesign",
            Name = "网页设计",
            Icon = "🌐",
            BackgroundColor = "#1890FF",
            Category = "设计"
        },
        new TemplateItem
        {
            Id = "DesktopApp",
            Name = "桌面应用",
            Icon = "🖥️",
            BackgroundColor = "#13C2C2",
            Category = "应用开发"
        },
        new TemplateItem
        {
            Id = "PrototypeDesign",
            Name = "原型设计",
            Icon = "🎨",
            BackgroundColor = "#722ED1",
            Category = "设计"
        }
    };

    public ObservableCollection<RecentPrototypeItem> RecentTemplates { get; } = new ObservableCollection<RecentPrototypeItem>();

    #endregion

    #region 过滤模板

    public ObservableCollection<TemplateItem> FilteredTemplates
    {
        get
        {
            var filtered = Templates.AsEnumerable();

            // 按分类过滤
            if (SelectedCategory != null && SelectedCategory.Name != "全部")
            {
                filtered = filtered.Where(t => t.Category == SelectedCategory.Name);
            }

            // 按搜索文本过滤
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLower();
                filtered = filtered.Where(t =>
                    (t.Name?.ToLower().Contains(search) ?? false) ||
                    (t.Description?.ToLower().Contains(search) ?? false) ||
                    (t.Category?.ToLower().Contains(search) ?? false));
            }

            return new ObservableCollection<TemplateItem>(filtered);
        }
    }

    #endregion

    #region 分类计数

    public void UpdateCategoryCounts()
    {
        foreach (var cat in Categories)
        {
            if (cat.Name == "全部")
            {
                cat.Count = Templates.Count;
            }
            else
            {
                cat.Count = Templates.Count(t => t.Category == cat.Name);
            }
        }
    }

    #endregion

    #region 模板操作

    public event Action<TemplateItem>? TemplateSelected;
    public event Action<TemplateItem>? QuickCreateRequested;
    public event Action? OpenPrototypeDesignRequested;
    public event Action? OpenWebDesignRequested;

    public void SelectTemplate(TemplateItem? template)
    {
        if (template == null) return;
        TemplateSelected?.Invoke(template);
    }

    public void QuickCreate(TemplateItem? template)
    {
        if (template == null) return;
        QuickCreateRequested?.Invoke(template);
    }

    public void OpenPrototypeDesign()
    {
        OpenPrototypeDesignRequested?.Invoke();
    }

    public void OpenWebDesign()
    {
        OpenWebDesignRequested?.Invoke();
    }

    #endregion

    #region 生命周期

    public override void UpdateLanguageTexts()
    {
        OnPropertyChanged(nameof(FilteredTemplates));
    }

    public override void Dispose()
    {
        PropertyChanged = null;
    }

    public override void Initialize(SystemContext? context)
    {
        _systemContext = context;
        UpdateCategoryCounts();
        SelectedCategory = Categories.FirstOrDefault();
    }

    #endregion
}

public class RecentPrototypeItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? LastModified { get; set; }
}