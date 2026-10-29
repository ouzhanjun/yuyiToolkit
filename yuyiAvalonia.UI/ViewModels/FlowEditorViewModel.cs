using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.UI.Controls.ElementFlow;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;
using yuyiAvalonia.UI.Services;

namespace yuyiAvalonia.UI.ViewModels;

/// <summary>
/// 网页设计器 ViewModel
/// </summary>
public partial class FlowEditorViewModel : ElementEditViewModelBase
{
    SystemContext? _systemContext;

    [ObservableProperty]
    private TemplateItem? _templateItem;

    public event EventHandler OnRefreshCanvasElements;

    public CanvasHistoryManager HistoryManager { get; } = new();


    [RelayCommand]
    private void Undo()
    {
        // 撤销由 View 层处理，这里只触发通知
        //WhenUndoRequested?.Invoke(this, EventArgs.Empty);
        HistoryManager.Undo();
    }


    [ObservableProperty]
    private bool _canUndo = false;

    [RelayCommand]
    private void Redo()
    {
        // 重做由 View 层处理，这里只触发通知
        //WhenRedoRequested?.Invoke(this, EventArgs.Empty);
        HistoryManager.Redo();
    }

    [ObservableProperty]
    private bool _canRedo = false;

    public event EventHandler? WhenUndoRequested;
    public event EventHandler? WhenRedoRequested;


    public FlowEditorViewModel() : base()
    {
        _canvasProperty = new CanvasProperty();

        _canvasProperty.WhenCanvasWidthChanged += (s, e) =>
        {
            if (SelectedCanvasSize != null && Math.Abs(SelectedCanvasSize.Width - e) > 1)
            {
                SelectedCanvasSize.IsSelected = false;
                SelectedCanvasSize = null;
            }
        };

        _canvasProperty.WhenCanvasHeightChanged += (s, e) =>
        {
            if (SelectedCanvasSize != null && Math.Abs(SelectedCanvasSize.Height - e) > 1)
            {
                SelectedCanvasSize.IsSelected = false;
                SelectedCanvasSize = null;
            }
        };

        _canvasProperty.WhenBackgroundChanged += (s, e) =>
        {
            WhenCanvasPropertyChanged?.Invoke(_canvasProperty, new PropertyChangedEventArgs("BackgroundChanged"));
        };

        _canvasProperty.WhenShowAllElementIdsChanged += (s, showAllIds) =>
        {
            WhenShowAllElementIdsChanged?.Invoke(this, showAllIds);
        };

        _canvasProperty.WhenShowRulersChanged += (s, showRulers) =>
        {
            WhenShowRulersChanged?.Invoke(this, showRulers);
        };

        _canvasProperty.WhenShowGridChanged += (s, showGrid) =>
        {
            WhenShowGridChanged?.Invoke(this, showGrid);
        };

        _canvasProperty.PropertyChanged += _canvasProperty_PropertyChanged;

        HistoryManager.PropertyChanged += HistoryManager_PropertyChanged;
    }

    private void HistoryManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "CanUndo":
                this.CanUndo = HistoryManager.CanUndo;
                break;
            case "CanRedo":
                this.CanRedo = HistoryManager.CanRedo;
                break;
        }
    }

    public event EventHandler<bool>? WhenShowAllElementIdsChanged;
    public event EventHandler<bool>? WhenShowRulersChanged;
    public event EventHandler<bool>? WhenShowGridChanged;

    #region 属性修改

    public override void SelectedElementChangedHandle(CanvasElement? oldValue, CanvasElement? newValue)
    {
        if (oldValue != null)
        {
            oldValue.PropertyChanged -= NewValue_PropertyChanged;
            oldValue.BackgroundFills.CollectionChanged -= Fills_CollectionChanged;
            foreach (var fill in oldValue.BackgroundFills)
                fill.PropertyChanged -= OnFillItemPropertyChanged;
        }

        if (newValue != null)
        {
            newValue.PropertyChanged += NewValue_PropertyChanged;
            newValue.BackgroundFills.CollectionChanged += Fills_CollectionChanged;
            foreach (var fill in newValue.BackgroundFills)
                fill.PropertyChanged += OnFillItemPropertyChanged;
        }

        HasSelectedElement = newValue != null;
        RightPanelIndex = HasSelectedElement ? 1 : 0;

        OnPropertyChanged(nameof(CanvasPanelWidth));
        OnPropertyChanged(nameof(CanvasPanelHeight));
    }

    private void Fills_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (System.ComponentModel.INotifyPropertyChanged item in e.NewItems)
                item.PropertyChanged += OnFillItemPropertyChanged;
        }
        if (e.OldItems != null)
        {
            foreach (System.ComponentModel.INotifyPropertyChanged item in e.OldItems)
                item.PropertyChanged -= OnFillItemPropertyChanged;
        }
        WhenSelectedElementPropertyChanged?.Invoke(SelectedElement, new PropertyChangedEventArgs("BackgroundFills"));
    }

    private void OnFillItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        WhenSelectedElementPropertyChanged?.Invoke(SelectedElement, new PropertyChangedEventArgs("BackgroundFills"));
    }

    public event EventHandler<PropertyChangedEventArgs>? WhenSelectedElementPropertyChanged;

    private void NewValue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        WhenSelectedElementPropertyChanged?.Invoke(sender, e);
    }

    public event EventHandler<PropertyChangedEventArgs>? WhenCanvasPropertyChanged;

    private void _canvasProperty_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        WhenCanvasPropertyChanged?.Invoke(sender, e);
    }

    #endregion

    #region 组件列表

    [ObservableProperty]
    private ObservableCollection<ComponentDefinition> _componentDefinitions = UIComponentDefinitions.LoadWebComponentDefinitions();

    [ObservableProperty]
    private ObservableCollection<CategoryItem> _categories = new();

    [ObservableProperty]
    private CategoryItem? _selectedCategory;

    public ObservableCollection<ComponentDefinition> FilteredComponents { get; } = new();

    // 用于 XAML 绑定的静态分组集合
    public static ComponentGroupSource GroupsSource => ComponentGroupSource.Instance;

    #endregion

    #region 画布属性

    [ObservableProperty]
    private CanvasProperty _canvasProperty;

    [ObservableProperty]
    private ObservableCollection<CanvasSize> _canvasSizes = new();

    [ObservableProperty]
    private CanvasSize? _selectedCanvasSize;

    #endregion

    #region 画布缩放

    [ObservableProperty]
    private double _zoomFactor = 1.0;

    private const double MinZoom = 0.25;
    private const double MaxZoom = 4.0;
    private const double ZoomStep = 0.1;
    private const double CanvasMargin = 40;

    [ObservableProperty]
    private double _contentPanelWidth = 0;
    [ObservableProperty]
    private double _contentPanelHeight = 0;

    public double CanvasPanelWidth
    {
        get
        {
            return Math.Max(ContentPanelWidth- CanvasMargin*2, CanvasProperty.CanvasWidth * ZoomFactor + CanvasMargin * 2);
        }
    }

    public double CanvasPanelHeight
    {
        get
        {
            return Math.Max(_contentPanelHeight - CanvasMargin * 2, CanvasProperty.CanvasHeight * ZoomFactor + CanvasMargin * 2);
        }
    }

    public event EventHandler? WhenZoomFactorChanged;

    partial void OnZoomFactorChanged(double value)
    {
        ZoomFactor = Math.Max(MinZoom, Math.Min(MaxZoom, value));

        OnPropertyChanged(nameof(CanvasPanelWidth));
        OnPropertyChanged(nameof(CanvasPanelHeight));
        WhenZoomFactorChanged?.Invoke(this, EventArgs.Empty);
    }

    [ObservableProperty]
    private int _zoomPercent = 100;

    [RelayCommand]
    private void ZoomIn() => ZoomFactor = Math.Min(MaxZoom, ZoomFactor + ZoomStep);

    [RelayCommand]
    private void ZoomOut() => ZoomFactor = Math.Max(MinZoom, ZoomFactor - ZoomStep);

    [RelayCommand]
    private void ZoomReset() => ZoomFactor = 1.0;

    [RelayCommand]
    private void ZoomFit() => ZoomFactor = 1.0;

    #endregion

    #region 右侧面板

    [ObservableProperty]
    private int _rightPanelIndex;

    #endregion

    #region 生命周期

    public override void Initialize(SystemContext? context)
    {
        _systemContext = context;
        LoadWebCanvasSizes();
        LoadCategories();
        //LoadDefaultPage();

        //历史记录
        this.HistoryManager.RecordElements(this.CanvasElements, null, HistoryActionType.ElementModified);
    }

    public override void UpdateLanguageTexts() { }
    public override void Dispose() { }

    #endregion

    #region 画布尺寸

    partial void OnSelectedCanvasSizeChanged(CanvasSize? value)
    {
        if (value != null)
        {
            if (CanvasProperty.CanvasWidth != value.Width)
            {
                CanvasProperty.CanvasWidth = value.Width;
                OnPropertyChanged(nameof(CanvasPanelWidth));
            }
            if (CanvasProperty.CanvasHeight != value.Height)
            {
                CanvasProperty.CanvasHeight = value.Height;
                OnPropertyChanged(nameof(CanvasPanelHeight));
            }
        }
    }

    private void LoadWebCanvasSizes()
    {
        CanvasSizes.Clear();
        CanvasSizes.Add(new CanvasSize { Name = "PC标准", Width = 1920, Height = 1080, Description = "标准全高清显示器 (1920×1080)", Category = CanvasSizeCategory.Other });
        CanvasSizes.Add(new CanvasSize { Name = "笔记本1366", Width = 1366, Height = 768, Description = "标准笔记本 (1366×768)", Category = CanvasSizeCategory.Other });
        CanvasSizes.Add(new CanvasSize { Name = "笔记本1440", Width = 1440, Height = 900, Description = "笔记本 (1440×900)", Category = CanvasSizeCategory.Other });
        CanvasSizes.Add(new CanvasSize { Name = "iPad横屏", Width = 1024, Height = 768, Description = "iPad横向 (1024×768)", Category = CanvasSizeCategory.Tablet });
        CanvasSizes.Add(new CanvasSize { Name = "iPad Pro横屏", Width = 1366, Height = 1024, Description = "iPad Pro横向 (1366×1024)", Category = CanvasSizeCategory.Tablet });
        CanvasSizes.Add(new CanvasSize { Name = "手机标准", Width = 375, Height = 812, Description = "手机 (375×812)", Category = CanvasSizeCategory.Mobile });
        CanvasSizes.Add(new CanvasSize { Name = "手机Plus", Width = 414, Height = 896, Description = "大屏手机 (414×896)", Category = CanvasSizeCategory.Mobile });
        CanvasSizes.Add(new CanvasSize { Name = "自定义", Width = 800, Height = 600, Description = "自定义尺寸", Category = CanvasSizeCategory.Other });

        SelectedCanvasSize = CanvasSizes.FirstOrDefault(x => x.Name == "自定义");
    }

    #endregion

    #region 分类加载

    private void LoadCategories()
    {
        // 加载分类按钮（用于筛选）
        var types = ComponentDefinitions.Select(c => c.Category).Distinct().ToList();
        var cats = new List<CategoryItem> { new CategoryItem { Name = "全部", IsSelected = true, Count = ComponentDefinitions.Count } };

        foreach (var type in types)
        {
            cats.Add(new CategoryItem
            {
                Name = type,
                IsSelected = false,
                Count = ComponentDefinitions.Count(c => c.Category == type)
            });
        }

        Categories = new ObservableCollection<CategoryItem>(cats);
        UpdateFilteredComponents("全部");

        // 加载分组（用于折叠面板）
        LoadComponentGroups();
    }

    private void LoadComponentGroups()
    {
        GroupsSource.Groups.Clear();
        var types = ComponentDefinitions.Select(c => c.Category).Distinct().ToList();

        foreach (var type in types)
        {
            var group = new ComponentGroup
            {
                Name = type,
                IsExpanded = true
            };

            var components = ComponentDefinitions.Where(c => c.Category == type);
            foreach (var c in components)
            {
                group.Components.Add(c);
            }

            GroupsSource.Groups.Add(group);
        }
    }

    partial void OnSelectedCategoryChanged(CategoryItem? value)
    {
        if (value != null)
        {
            foreach (var cat in Categories)
                cat.IsSelected = cat.Name == value.Name;
            UpdateFilteredComponents(value.Name);
        }
    }

    private void UpdateFilteredComponents(string category)
    {
        FilteredComponents.Clear();
        var filtered = string.IsNullOrEmpty(category) || category == "全部"
            ? ComponentDefinitions
            : ComponentDefinitions.Where(c => c.Category == category);

        foreach (var c in filtered)
            FilteredComponents.Add(c);
    }

    #endregion

    #region 元素操作

    [RelayCommand]
    private void SelectElement(CanvasElement? element)
    {
        SelectedElement = element;

        if (SelectedElement != element)
        {
            SelectedElement = element;
        }
    }

    [RelayCommand]
    private void ClearCanvas()
    {
        CanvasElements.Clear();
        SelectElement(null);

        //历史记录
        HistoryManager.RecordElements(this.CanvasElements,null,HistoryActionType.ElementDeleted);
    }

    [RelayCommand]
    private void PreviewCanvas() => WhenPreviewCanvasRequested?.Invoke(this, EventArgs.Empty);

    public event EventHandler? WhenPreviewCanvasRequested;

    #endregion

    #region 对齐操作

    [RelayCommand] private void AlignLeft() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.Left));
    [RelayCommand] private void AlignHorizontalCenter() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.HorizontalCenter));
    [RelayCommand] private void AlignRight() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.Right));
    [RelayCommand] private void AlignTop() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.Top));
    [RelayCommand] private void AlignVerticalCenter() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.VerticalCenter));
    [RelayCommand] private void AlignBottom() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.Bottom));
    [RelayCommand] private void DistributeHorizontalSpacing() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.HorizontalSpacing));
    [RelayCommand] private void DistributeVerticalSpacing() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.VerticalSpacing));
    [RelayCommand] private void MakeSameWidth() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.SameWidth));
    [RelayCommand] private void MakeSameHeight() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.SameHeight));
    [RelayCommand] private void MakeSameSize() => WhenAlignRequested?.Invoke(this, new AlignRequestedEventArgs(AlignmentType.SameSize));

    public event EventHandler<AlignRequestedEventArgs>? WhenAlignRequested;

    #endregion

    #region 状态栏

    internal void SetStatusMousePosition(Point position)
    {
        _systemContext?.AddStatusItem(new StatusItem("mouse position") { Text = $"鼠标位置 x:{position.X:F0} y:{position.Y:F0}" });
    }

    #endregion

    #region 默认页面

    private void LoadDefaultPage()
    {
        var container = CreateCardFromDefinition("Panel容器");
        container.X = 50;
        container.Y = 50;
        container.Width = CanvasProperty.CanvasWidth - 100;
        container.Height = CanvasProperty.CanvasHeight - 100;
        container.BackgroundFill.BackgroundColor = "#FFFFFF";
        CanvasElements.Add(container);

        var header = CreateCardFromDefinition("Panel容器");
        header.X = 50;
        header.Y = 50;
        header.Width = CanvasProperty.CanvasWidth - 100;
        header.Height = 60;
        header.BackgroundFill.BackgroundColor = "#007BFF";
        CanvasElements.Add(header);

        var title = CreateTextFromDefinition("文本");
        title.X = 100;
        title.Y = 60;
        title.Text = "网页标题";
        title.FontSize = 20;
        title.IsBold = true;
        title.ForeColor = "#FFFFFF";
        CanvasElements.Add(title);

        var content = CreateCardFromDefinition("Panel容器");
        content.X = 50;
        content.Y = 130;
        content.Width = CanvasProperty.CanvasWidth - 100;
        content.Height = CanvasProperty.CanvasHeight - 230;
        content.BackgroundFill.BackgroundColor = "#F8F9FA";
        CanvasElements.Add(content);

        var button = CreateButtonFromDefinition("按钮");
        button.X = 100;
        button.Y = CanvasProperty.CanvasHeight - 120;
        button.Text = "提交按钮";
        CanvasElements.Add(button);

        OnRefreshCanvasElements?.Invoke(this, EventArgs.Empty);
    }

    private TextElement CreateTextFromDefinition(string name)
    {
        var definition = ComponentDefinitions.FirstOrDefault(d => d.Name == name);
        if (definition?.Defaults == null) return new TextElement();

        var uniqueName = FlowUIControlCreator.GenerateUniqueName(ComponentType.Text);
        var element = new TextElement
        {
            Width = definition.Defaults.Width,
            Height = definition.Defaults.Height,
            Text = definition.Name,
            FontSize = definition.Defaults.FontSize,
            ForeColor = definition.Defaults.ForeColor,
            IsBold = definition.Defaults.IsBold,
            Padding = definition.Defaults.Padding,
            Margin = definition.Defaults.Margin
        };
        element.SetId(uniqueName);
        return element;
    }

    private CardElement CreateCardFromDefinition(string name)
    {
        var definition = ComponentDefinitions.FirstOrDefault(d => d.Name == name);
        if (definition?.Defaults == null) return new CardElement();

        var uniqueName = FlowUIControlCreator.GenerateUniqueName(ComponentType.Card);
        var element = new CardElement
        {
            Width = definition.Defaults.Width,
            Height = definition.Defaults.Height,
            BackgroundFill = new FillItem { BackgroundColor = definition.Defaults.BackgroundColor },
            BorderColor = definition.Defaults.BorderColor,
            BorderWidth = definition.Defaults.BorderWidth,
            CornerRadius = definition.Defaults.CornerRadius,
            Padding = definition.Defaults.Padding,
            Margin = definition.Defaults.Margin
        };
        element.SetId(uniqueName);
        return element;
    }

    private ButtonElement CreateButtonFromDefinition(string name)
    {
        var definition = ComponentDefinitions.FirstOrDefault(d => d.Name == name);
        if (definition?.Defaults == null) return new ButtonElement();

        var uniqueName = FlowUIControlCreator.GenerateUniqueName(ComponentType.Button);
        var element = new ButtonElement
        {
            Width = definition.Defaults.Width,
            Height = definition.Defaults.Height,
            Text = definition.Name,
            BackgroundFill = new FillItem { BackgroundColor = definition.Defaults.BackgroundColor },
            ForeColor = definition.Defaults.ForeColor,
            BorderColor = definition.Defaults.BorderColor,
            BorderWidth = definition.Defaults.BorderWidth,
            CornerRadius = definition.Defaults.CornerRadius,
            Padding = definition.Defaults.Padding,
            Margin = definition.Defaults.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        element.SetId(uniqueName);
        return element;
    }

    #endregion

    #region 页面模板

    [RelayCommand]
    private void LoadLandingPageTemplate()
    {
        ClearCanvas();
        CanvasProperty.CanvasWidth = 1920;
        CanvasProperty.CanvasHeight = 1080;
        CanvasProperty.CanvasBackgroundColor = "#F0F2F5";

        var heroSection = CreateCardFromDefinition("Panel容器");
        heroSection.X = 0;
        heroSection.Y = 0;
        heroSection.Width = 1920;
        heroSection.Height = 600;
        heroSection.BackgroundFill.BackgroundColor = "#1E90FF";
        CanvasElements.Add(heroSection);

        var heroTitle = CreateTextFromDefinition("文本");
        heroTitle.X = 760;
        heroTitle.Y = 200;
        heroTitle.Width = 400;
        heroTitle.Text = "欢迎来到我们的网站";
        heroTitle.FontSize = 48;
        heroTitle.IsBold = true;
        heroTitle.ForeColor = "#FFFFFF";
        CanvasElements.Add(heroTitle);

        var heroSubtitle = CreateTextFromDefinition("文本");
        heroSubtitle.X = 800;
        heroSubtitle.Y = 280;
        heroSubtitle.Width = 320;
        heroSubtitle.Text = "这是副标题描述文本";
        heroSubtitle.FontSize = 24;
        heroSubtitle.ForeColor = "#FFFFFF";
        CanvasElements.Add(heroSubtitle);

        var ctaButton = CreateButtonFromDefinition("按钮");
        ctaButton.X = 900;
        ctaButton.Y = 350;
        ctaButton.Width = 120;
        ctaButton.Height = 48;
        ctaButton.Text = "开始使用";
        CanvasElements.Add(ctaButton);

        OnRefreshCanvasElements?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void LoadFormTemplate()
    {
        ClearCanvas();
        CanvasProperty.CanvasWidth = 600;
        CanvasProperty.CanvasHeight = 800;
        CanvasProperty.CanvasBackgroundColor = "#F5F5F5";

        var formCard = CreateCardFromDefinition("卡片");
        formCard.X = 100;
        formCard.Y = 100;
        formCard.Width = 400;
        formCard.Height = 600;
        formCard.BackgroundFill.BackgroundColor = "#FFFFFF";
        CanvasElements.Add(formCard);

        var formTitle = CreateTextFromDefinition("文本");
        formTitle.X = 130;
        formTitle.Y = 130;
        formTitle.Width = 340;
        formTitle.Text = "用户注册表单";
        formTitle.FontSize = 24;
        formTitle.IsBold = true;
        CanvasElements.Add(formTitle);

        var label1 = CreateTextFromDefinition("文本");
        label1.X = 130;
        label1.Y = 200;
        label1.Width = 100;
        label1.Text = "用户名:";
        label1.FontSize = 14;
        CanvasElements.Add(label1);

        var input1 = CreateCardFromDefinition("卡片");
        input1.X = 130;
        input1.Y = 230;
        input1.Width = 340;
        input1.Height = 40;
        input1.BackgroundFill.BackgroundColor = "#FFFFFF";
        input1.BorderColor = "#CCCCCC";
        input1.BorderWidth = 1;
        CanvasElements.Add(input1);

        var label2 = CreateTextFromDefinition("文本");
        label2.X = 130;
        label2.Y = 290;
        label2.Width = 100;
        label2.Text = "邮箱:";
        label2.FontSize = 14;
        CanvasElements.Add(label2);

        var input2 = CreateCardFromDefinition("卡片");
        input2.X = 130;
        input2.Y = 320;
        input2.Width = 340;
        input2.Height = 40;
        input2.BackgroundFill.BackgroundColor = "#FFFFFF";
        input2.BorderColor = "#CCCCCC";
        input2.BorderWidth = 1;
        CanvasElements.Add(input2);

        var submitBtn = CreateButtonFromDefinition("按钮");
        submitBtn.X = 130;
        submitBtn.Y = 400;
        submitBtn.Width = 340;
        submitBtn.Text = "提交";
        CanvasElements.Add(submitBtn);

        OnRefreshCanvasElements?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void LoadDashboardTemplate()
    {
        ClearCanvas();
        CanvasProperty.CanvasWidth = 1200;
        CanvasProperty.CanvasHeight = 800;
        CanvasProperty.CanvasBackgroundColor = "#F0F2F5";

        var sidebar = CreateCardFromDefinition("Panel容器");
        sidebar.X = 0;
        sidebar.Y = 0;
        sidebar.Width = 250;
        sidebar.Height = 800;
        sidebar.BackgroundFill.BackgroundColor = "#2C3E50";
        CanvasElements.Add(sidebar);

        var mainContent = CreateCardFromDefinition("Panel容器");
        mainContent.X = 250;
        mainContent.Y = 0;
        mainContent.Width = 950;
        mainContent.Height = 80;
        mainContent.BackgroundFill.BackgroundColor = "#FFFFFF";
        CanvasElements.Add(mainContent);

        var card1 = CreateCardFromDefinition("卡片");
        card1.X = 270;
        card1.Y = 100;
        card1.Width = 280;
        card1.Height = 150;
        CanvasElements.Add(card1);

        var card2 = CreateCardFromDefinition("卡片");
        card2.X = 570;
        card2.Y = 100;
        card2.Width = 280;
        card2.Height = 150;
        CanvasElements.Add(card2);

        var card3 = CreateCardFromDefinition("卡片");
        card3.X = 870;
        card3.Y = 100;
        card3.Width = 280;
        card3.Height = 150;
        CanvasElements.Add(card3);

        var tableCard = CreateCardFromDefinition("卡片");
        tableCard.X = 270;
        tableCard.Y = 280;
        tableCard.Width = 880;
        tableCard.Height = 480;
        CanvasElements.Add(tableCard);

        OnRefreshCanvasElements?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region 导出功能

    [ObservableProperty]
    private string _exportStatus = string.Empty;

    [ObservableProperty]
    private bool _isExporting = false;

    [RelayCommand]
    private async Task ExportHtmlAsync()
    {
        if (IsExporting) return;

        try
        {
            IsExporting = true;
            ExportStatus = "正在导出...";

            var topLevel = GetTopLevel();
            if (topLevel == null)
            {
                ExportStatus = "无法获取窗口";
                return;
            }

            var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "选择导出目录" });

            if (folder.Count == 0)
            {
                ExportStatus = "已取消导出";
                return;
            }

            var exportPath = folder[0].Path.LocalPath;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var exportFolder = Path.Combine(exportPath, $"web_export_{timestamp}");

            var exporter = new HtmlExporter(CanvasElements, CanvasProperty.CanvasWidth, CanvasProperty.CanvasHeight, exportFolder);
            var result = exporter.Export();

            if (result.Success)
            {
                ExportStatus = $"导出成功！文件位置：{result.HtmlFilePath}";
                try { System.Diagnostics.Process.Start("explorer.exe", exportFolder); } catch { }
            }
            else
            {
                ExportStatus = $"导出失败：{string.Join(", ", result.Errors)}";
            }
        }
        catch (Exception ex)
        {
            ExportStatus = $"导出异常：{ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    [RelayCommand]
    private async Task SaveCanvasAsync()
    {
        try
        {
            ExportStatus = "正在保存...";
            var topLevel = GetTopLevel();
            if (topLevel == null) { ExportStatus = "无法获取窗口"; return; }

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存画布",
                SuggestedFileName = $"web_canvas_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                FileTypeChoices = new[] { new FilePickerFileType("JSON 文件") { Patterns = new[] { "*.json" } } }
            });

            if (file == null) { ExportStatus = "已取消保存"; return; }

            var filePath = file.Path.LocalPath;
            if (CanvasElementSerializer.SaveToFile(filePath, CanvasElements, CanvasProperty, out var errorMessage))
                ExportStatus = "保存成功！";
            else
                ExportStatus = $"保存失败：{errorMessage}";
        }
        catch (Exception ex)
        {
            ExportStatus = $"保存异常：{ex.Message}";
        }
    }

    [RelayCommand]
    private async Task LoadCanvasAsync()
    {
        try
        {
            ExportStatus = "正在加载...";
            var topLevel = GetTopLevel();
            if (topLevel == null) { ExportStatus = "无法获取窗口"; return; }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "加载画布",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("JSON 文件") { Patterns = new[] { "*.json" } } }
            });

            if (files.Count == 0) { ExportStatus = "已取消加载"; return; }

            var filePath = files[0].Path.LocalPath;
            var saveData = CanvasElementSerializer.LoadFromFile(filePath, out var errorMessage);

            if (saveData == null) { ExportStatus = $"加载失败：{errorMessage}"; return; }

            CanvasElements.Clear();
            if (saveData.CanvasProperty != null)
                CanvasProperty.CopyFrom(saveData.CanvasProperty);

            foreach (var element in saveData.Elements)
                CanvasElements.Add(element);

            OnRefreshCanvasElements?.Invoke(this, EventArgs.Empty);
            ExportStatus = $"加载成功！共 {saveData.Elements.Count} 个元素";
        }
        catch (Exception ex)
        {
            ExportStatus = $"加载异常：{ex.Message}";
        }
    }

    private TopLevel? GetTopLevel()
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow : null;
        return TopLevel.GetTopLevel(window);
    }

    #endregion
}
