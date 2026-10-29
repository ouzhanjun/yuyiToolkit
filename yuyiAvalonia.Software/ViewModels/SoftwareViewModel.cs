using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvalonia.Software.Models;
using yuyiAvalonia.Software.Services;

namespace yuyiAvalonia.Software.ViewModels;

/// <summary>
/// 软件管理器视图模型
/// </summary>
public partial class SoftwareViewModel : PageViewModelBase, INotifyPropertyChanged
{
    private SystemContext? _systemContext;
    private readonly SoftwareStorageService _storageService;
    private readonly SoftwareDownloadService _downloadService;
    private const string ToolbarSoftware = "toolbar_Software";

    [ObservableProperty]
    private ObservableCollection<SoftwareItem> _allSoftware = new();

    [ObservableProperty]
    private ObservableCollection<SoftwareItem> _filteredSoftware = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private SoftwareItem? _selectedSoftware;

    [ObservableProperty]
    private string _searchKeyword = "";

    [ObservableProperty]
    private string _currentCategory = "全部";

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private string _downloadStatus = "";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private SoftwareItem? _editingSoftware;

    [ObservableProperty]
    private bool _showOnlyFavorites;

    [ObservableProperty]
    private bool _showOnlyDownloaded;

    [ObservableProperty]
    private string _sortBy = "名称";

    [ObservableProperty]
    private int _currentTabIndex;

    public string[] SortOptions { get; } = { "名称", "分类", "更新时间", "大小" };

    // 静态实例供 XAML 绑定使用
    public static SoftwareViewModel? Instance { get; private set; }

    public SoftwareViewModel()
    {
        Instance = this;
        _storageService = new SoftwareStorageService();
        _downloadService = new SoftwareDownloadService();
        InitializeSoftwareList();
    }

    public override void Initialize(SystemContext? context)
    {
        if (context == null)
            return;
        _systemContext = context;

        InitializeToolbar();
    }

    private void InitializeToolbar()
    {
        MenuItemViewModel itemSeparator = new MenuItemViewModel("Separator");
        itemSeparator.Text = "-";
        itemSeparator.IsSeparator = true;

        MenuItemViewModel itemAdd = new MenuItemViewModel("Add");
        itemAdd.Icon = StreamGeometry.Parse("M412.584229 227.9424 279.1424 326.802286 279.1424 725.313829 571.743086 725.313829 571.743086 783.828114 220.628114 783.828114 220.628114 297.311086 393.245257 169.428114 686.314057 169.428114 686.314057 434.5856 636.372114 433.9712 636.079543 226.655086ZM279.1424 326.802286 425.428114 326.685257 425.428114 198.685257ZM513.2288 599.742171 630.257371 599.742171 630.257371 481.514057 686.314057 481.514057 686.314057 599.742171 803.342629 599.742171 803.342629 659.485257 686.314057 659.485257 686.314057 771.627886 629.028571 771.627886 629.028571 659.485257 513.2288 659.485257Z");
        itemAdd.Text = "添加软件";
        itemAdd.MenuClickCommand = this.AddSoftwareCommand;

        MenuItemViewModel itemBatchDownload = new MenuItemViewModel("BatchDownload");
        itemBatchDownload.Icon = StreamGeometry.Parse("M736 832c-19.2 0-32-12.8-32-32s12.8-32 32-32c108.8 0 192-83.2 192-192 0-96-70.4-179.2-166.4-192-12.8 0-25.6-12.8-25.6-25.6C723.2 243.2 627.2 160 512 160 390.4 160 288 262.4 288 384c0 19.2-12.8 32-32 32C166.4 416 96 486.4 96 576c0 89.6 70.4 160 160 160 19.2 0 32 12.8 32 32S275.2 800 256 800c-121.6 0-224-102.4-224-224 0-115.2 83.2-204.8 192-224 12.8-147.2 140.8-256 288-256 140.8 0 256 96 281.6 230.4 115.2 25.6 198.4 128 198.4 249.6C992 716.8 876.8 832 736 832zM512 928c-6.4 0-19.2-6.4-25.6-6.4l-96-96c-12.8-12.8-12.8-32 0-44.8s32-12.8 44.8 0l38.4 38.4L473.6 576c0-140.8 115.2-256 256-256s256 115.2 256 256c0 140.8-115.2 256-256 256-19.2 0-32-12.8-32-32s12.8-32 32-32c108.8 0 192-83.2 192-192s-83.2-192-192-192-192 83.2-192 192l0 320c0 12.8-6.4 25.6-19.2 32C518.4 928 518.4 928 512 928z");
        itemBatchDownload.Text = "批量下载";
        itemBatchDownload.MenuClickCommand = this.BatchDownloadCommand;

        MenuItemViewModel itemRefresh = new MenuItemViewModel("Refresh");
        itemRefresh.Icon = Icon.Refresh;
        itemRefresh.Text = "刷新";
        itemRefresh.MenuClickCommand = this.RefreshCommand;

        MenuItemViewModel itemOpenFolder = new MenuItemViewModel("OpenFolder");
        itemOpenFolder.Icon = Icon.OpenFolder;
        itemOpenFolder.Text = "打开下载目录";
        itemOpenFolder.MenuClickCommand = this.OpenDownloadFolderCommand;

        ChildToolbar childToolbar = new ChildToolbar(ToolbarSoftware,
        [
            itemAdd,
            itemBatchDownload,
            itemRefresh,
            itemOpenFolder
        ]);

        var existingTool = _systemContext.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == childToolbar.Name);
        if (existingTool != null)
        {
            _systemContext.TopToolbar.ChildTools.Remove(existingTool);
        }

        _systemContext.TopToolbar.ChildTools.Add(childToolbar);
        _systemContext.NotifyPropertyChange(true, "TopToolbar");
    }

    partial void OnSearchKeywordChanged(string value) => UpdateFilteredSoftware();
    partial void OnCurrentCategoryChanged(string value) => UpdateFilteredSoftware();
    partial void OnShowOnlyFavoritesChanged(bool value) => UpdateFilteredSoftware();
    partial void OnShowOnlyDownloadedChanged(bool value) => UpdateFilteredSoftware();
    partial void OnSortByChanged(string value) => UpdateFilteredSoftware();

    /// <summary>
    /// 初始化软件列表
    /// </summary>
    private void InitializeSoftwareList()
    {
        var (presets, userItems) = _storageService.LoadAllSoftware();

        AllSoftware.Clear();
        foreach (var item in presets)
        {
            AllSoftware.Add(item);
        }
        foreach (var item in userItems)
        {
            if (!AllSoftware.Any(x => x.Id == item.Id))
            {
                AllSoftware.Add(item);
            }
        }

        UpdateCategories();
        UpdateFilteredSoftware();

        if (AllSoftware.Count > 0 && SelectedSoftware == null)
        {
            SelectedSoftware = AllSoftware.First();
        }
    }

    /// <summary>
    /// 更新分类列表
    /// </summary>
    private void UpdateCategories()
    {
        var cats = new List<string> { "全部" };
        cats.AddRange(AllSoftware.Select(x => x.Category).Distinct().OrderBy(x => x));
        Categories = new ObservableCollection<string>(cats);
    }

    /// <summary>
    /// 更新过滤后的软件列表
    /// </summary>
    private void UpdateFilteredSoftware()
    {
        var filtered = AllSoftware.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.ToLower();
            filtered = filtered.Where(x =>
                x.Name.ToLower().Contains(keyword) ||
                x.Description.ToLower().Contains(keyword) ||
                x.Category.ToLower().Contains(keyword));
        }

        if (CurrentCategory != "全部")
        {
            filtered = filtered.Where(x => x.Category == CurrentCategory);
        }

        if (ShowOnlyFavorites)
        {
            filtered = filtered.Where(x => x.IsFavorite);
        }

        if (ShowOnlyDownloaded)
        {
            filtered = filtered.Where(x => x.Status == SoftwareStatus.Downloaded || _downloadService.IsDownloaded(x));
        }

        filtered = SortBy switch
        {
            "名称" => filtered.OrderBy(x => x.Name),
            "分类" => filtered.OrderBy(x => x.Category).ThenBy(x => x.Name),
            "更新时间" => filtered.OrderByDescending(x => x.LastUpdated),
            "大小" => filtered.OrderBy(x => x.Size),
            _ => filtered.OrderBy(x => x.Name)
        };

        FilteredSoftware = new ObservableCollection<SoftwareItem>(filtered);
    }

    /// <summary>
    /// 添加新软件
    /// </summary>
    [RelayCommand]
    private void AddSoftware()
    {
        EditingSoftware = new SoftwareItem
        {
            Id = Guid.NewGuid().ToString(),
            LastUpdated = DateTime.Now,
            IsBuiltIn = false
        };
        IsEditing = true;
    }

    /// <summary>
    /// 开始编辑
    /// </summary>
    [RelayCommand]
    private void StartEdit(SoftwareItem? item)
    {
        if (item == null) return;
        EditingSoftware = item.Clone();
        IsEditing = true;
    }

    /// <summary>
    /// 保存编辑
    /// </summary>
    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (EditingSoftware == null) return;

        var existing = AllSoftware.FirstOrDefault(x => x.Id == EditingSoftware.Id);
        if (existing != null)
        {
            existing.Name = EditingSoftware.Name;
            existing.Description = EditingSoftware.Description;
            existing.Version = EditingSoftware.Version;
            existing.DownloadUrl = EditingSoftware.DownloadUrl;
            existing.IconUrl = EditingSoftware.IconUrl;
            existing.Website = EditingSoftware.Website;
            existing.Category = EditingSoftware.Category;
            existing.Size = EditingSoftware.Size;
            existing.Publisher = EditingSoftware.Publisher;
            existing.LastUpdated = DateTime.Now;
        }
        else
        {
            AllSoftware.Add(EditingSoftware);
        }

        await SaveUserSoftwareAsync();
        UpdateCategories();
        UpdateFilteredSoftware();
        IsEditing = false;
        EditingSoftware = null;
    }

    /// <summary>
    /// 取消编辑
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingSoftware = null;
    }

    /// <summary>
    /// 删除软件
    /// </summary>
    [RelayCommand]
    private async Task DeleteSoftwareAsync(SoftwareItem? item)
    {
        if (item == null || item.IsBuiltIn) return;

        AllSoftware.Remove(item);
        await SaveUserSoftwareAsync();
        UpdateCategories();
        UpdateFilteredSoftware();

        if (SelectedSoftware == item)
        {
            SelectedSoftware = FilteredSoftware.FirstOrDefault();
        }
    }

    /// <summary>
    /// 收藏/取消收藏
    /// </summary>
    [RelayCommand]
    private async Task ToggleFavoriteAsync(SoftwareItem? item)
    {
        if (item == null) return;
        item.IsFavorite = !item.IsFavorite;
        await SaveUserSoftwareAsync();
        UpdateFilteredSoftware();
    }

    /// <summary>
    /// 下载软件
    /// </summary>
    [RelayCommand]
    private async Task DownloadSoftwareAsync(SoftwareItem? item)
    {
        var target = item ?? SelectedSoftware;
        if (target == null || string.IsNullOrEmpty(target.DownloadUrl))
            return;

        try
        {
            IsDownloading = true;
            DownloadProgress = 0;
            DownloadStatus = "准备下载...";
            target.Status = SoftwareStatus.Downloading;

            var progress = new Progress<double>(value =>
            {
                DownloadProgress = value;
                target.DownloadProgress = value;
                DownloadStatus = $"下载中... {value:F1}%";
            });

            var success = await _downloadService.DownloadSoftwareAsync(target, progress);

            if (success)
            {
                DownloadStatus = "下载完成！";
                target.DownloadCount++;
                target.LastDownloaded = DateTime.Now;
                await SaveUserSoftwareAsync();
            }
            else
            {
                DownloadStatus = "下载失败";
                target.Status = SoftwareStatus.Error;
            }
        }
        catch (Exception ex)
        {
            DownloadStatus = $"错误: {ex.Message}";
            target.Status = SoftwareStatus.Error;
        }
        finally
        {
            IsDownloading = false;
        }
    }

    /// <summary>
    /// 批量下载
    /// </summary>
    [RelayCommand]
    private async Task BatchDownloadAsync()
    {
        var notDownloaded = AllSoftware.Where(x =>
            x.Status != SoftwareStatus.Downloaded &&
            !string.IsNullOrEmpty(x.DownloadUrl) &&
            _downloadService.IsDownloaded(x) == false).ToList();

        foreach (var software in notDownloaded)
        {
            await DownloadSoftwareAsync(software);
        }
    }

    /// <summary>
    /// 取消下载
    /// </summary>
    [RelayCommand]
    private void CancelDownload()
    {
        _downloadService.CancelDownload();
        if (SelectedSoftware != null)
        {
            SelectedSoftware.Status = SoftwareStatus.NotDownloaded;
            SelectedSoftware.DownloadProgress = 0;
        }
        IsDownloading = false;
        DownloadStatus = "已取消";
    }

    /// <summary>
    /// 打开官网
    /// </summary>
    [RelayCommand]
    private void OpenWebsite()
    {
        if (SelectedSoftware == null || string.IsNullOrEmpty(SelectedSoftware.Website))
            return;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = SelectedSoftware.Website,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// 打开下载文件夹
    /// </summary>
    [RelayCommand]
    private void OpenDownloadFolder()
    {
        _downloadService.OpenDownloadFolder();
    }

    /// <summary>
    /// 打开文件位置
    /// </summary>
    [RelayCommand]
    private void OpenFileLocation()
    {
        if (SelectedSoftware == null || string.IsNullOrEmpty(SelectedSoftware.LocalPath))
            return;

        _downloadService.OpenFileLocation(SelectedSoftware.LocalPath);
    }

    /// <summary>
    /// 运行软件
    /// </summary>
    [RelayCommand]
    private void RunSoftware()
    {
        if (SelectedSoftware == null)
            return;

        if (_downloadService.IsDownloaded(SelectedSoftware))
        {
            _downloadService.RunSoftware(SelectedSoftware);
        }
    }

    /// <summary>
    /// 复制下载地址
    /// </summary>
    [RelayCommand]
    private async Task CopyDownloadUrlAsync()
    {
        if (SelectedSoftware == null || string.IsNullOrEmpty(SelectedSoftware.DownloadUrl))
            return;

        try
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window != null)
            {
                var topLevel = TopLevel.GetTopLevel(window);
                if (topLevel?.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(SelectedSoftware.DownloadUrl);
                }
            }
        }
        catch
        {
            // 忽略剪贴板操作错误
        }
    }

    /// <summary>
    /// 打开下载管理器
    /// </summary>
    [RelayCommand]
    private void OpenDownloadManager()
    {
        // TODO: 实现下载管理器窗口
    }

    /// <summary>
    /// 保存用户软件列表
    /// </summary>
    private async Task SaveUserSoftwareAsync()
    {
        var userItems = AllSoftware.Where(x => !x.IsBuiltIn).ToList();
        await _storageService.SaveUserSoftwareAsync(userItems);
    }

    /// <summary>
    /// 刷新列表
    /// </summary>
    [RelayCommand]
    private void Refresh()
    {
        InitializeSoftwareList();
    }

    public override void UpdateLanguageTexts() { }

    public override void Dispose()
    {
        _downloadService.Dispose();
        Instance = null;

        var childTool = _systemContext.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == ToolbarSoftware);
        if (childTool != null)
        {
            _systemContext.TopToolbar.ChildTools.Remove(childTool);
            _systemContext.NotifyPropertyChange(true, "TopToolbar");
        }
    }
}
