using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvalonia.ColorSelector.Models;

namespace yuyiAvalonia.ColorSelector.ViewModels;

/// <summary>
/// 颜色选择器视图模型
/// </summary>
public partial class ColorSelectorViewModel : PageViewModelBase, INotifyPropertyChanged
{
    private SystemContext? _systemContext;
    private ThemeStorageService _storageService;
    private const string ToolbarTheme = "toolbar_Theme";

    /// <summary>
    /// 所有主题列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColorTheme> _allThemes = new();

    /// <summary>
    /// 预设主题列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColorTheme> _presetThemes = new();

    /// <summary>
    /// 用户主题列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColorTheme> _userThemes = new();

    /// <summary>
    /// 当前选中的主题
    /// </summary>
    [ObservableProperty]
    private ColorTheme? _selectedTheme;

    /// <summary>
    /// 当前编辑的主题
    /// </summary>
    [ObservableProperty]
    private ColorTheme? _editingTheme;

    /// <summary>
    /// 预览文本
    /// </summary>
    [ObservableProperty]
    private string _previewText = "预览文本示例";

    /// <summary>
    /// 是否正在编辑
    /// </summary>
    [ObservableProperty]
    private bool _isEditing;

    /// <summary>
    /// 是否显示新建对话框
    /// </summary>
    [ObservableProperty]
    private bool _showNewThemeDialog;

    /// <summary>
    /// 新主题名称
    /// </summary>
    [ObservableProperty]
    private string _newThemeName = "";

    /// <summary>
    /// 新主题描述
    /// </summary>
    [ObservableProperty]
    private string _newThemeDescription = "";

    /// <summary>
    /// 搜索关键词
    /// </summary>
    [ObservableProperty]
    private string _searchKeyword = "";

    /// <summary>
    /// 当前分类
    /// </summary>
    [ObservableProperty]
    private string _currentCategory = "全部";

    /// <summary>
    /// 可用分类列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    /// <summary>
    /// 过滤后的主题列表
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ColorTheme> _filteredThemes = new();

    /// <summary>
    /// 当前编辑的标签页
    /// </summary>
    [ObservableProperty]
    private int _currentTabIndex;

    /// <summary>
    /// CSS 代码预览
    /// </summary>
    [ObservableProperty]
    private string _cssPreview = "";

    /// <summary>
    /// 选中的导出格式项
    /// </summary>
    [ObservableProperty]
    private ExportFormatItem? _selectedExportFormatItem;

    /// <summary>
    /// 选中的导出格式（用于逻辑判断）
    /// </summary>
    public ExportFormat SelectedExportFormat => SelectedExportFormatItem?.Format ?? ExportFormat.Css;

    /// <summary>
    /// 导出格式列表（用于下拉框显示）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ExportFormatItem> _exportFormats = new()
    {
        new ExportFormatItem { Format = ExportFormat.Json, DisplayName = "JSON" },
        new ExportFormatItem { Format = ExportFormat.Css, DisplayName = "CSS" }
    };

    /// <summary>
    /// 颜色面板是否可见
    /// </summary>
    [ObservableProperty]
    private bool _showColorPicker;

    /// <summary>
    /// 当前编辑的颜色属性名
    /// </summary>
    [ObservableProperty]
    private string _editingColorProperty = "";

    /// <summary>
    /// 拾色器预览颜色
    /// </summary>
    [ObservableProperty]
    private string _editingColorPreview = "#1890FF";

    /// <summary>
    /// 拾色器红色值
    /// </summary>
    [ObservableProperty]
    private int _colorPickerRed = 24;

    /// <summary>
    /// 拾色器绿色值
    /// </summary>
    [ObservableProperty]
    private int _colorPickerGreen = 144;

    /// <summary>
    /// 拾色器蓝色值
    /// </summary>
    [ObservableProperty]
    private int _colorPickerBlue = 255;

    /// <summary>
    /// 拾色器HEX值
    /// </summary>
    [ObservableProperty]
    private string _colorPickerHex = "#1890FF";

    public ColorSelectorViewModel()
    {
        _storageService = new ThemeStorageService();
        _selectedExportFormatItem = _exportFormats.FirstOrDefault(x => x.Format == ExportFormat.Css);
        InitializeAsync();
    }

    public override void Initialize(SystemContext? context)
    {
        _systemContext = context;

        // 初始化工具栏
        InitializeToolbar();

        // 加载主题
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        // 加载预设主题
        var presets = PresetColorSchemes.GetAll();
        PresetThemes = new ObservableCollection<ColorTheme>(presets);
        AllThemes = new ObservableCollection<ColorTheme>(presets);

        // 加载用户主题
        await LoadUserThemesAsync();

        // 设置默认选中
        if (AllThemes.Count > 0 && SelectedTheme == null)
        {
            SelectedTheme = AllThemes.FirstOrDefault(x => x.IsDefault) ?? AllThemes.First();
        }

        // 更新分类
        UpdateCategories();

        // 更新过滤后的主题列表
        UpdateFilteredThemes();

        // 更新预览
        UpdateCssPreview();
    }

    partial void OnSearchKeywordChanged(string value)
    {
        UpdateFilteredThemes();
    }

    partial void OnCurrentCategoryChanged(string value)
    {
        UpdateFilteredThemes();
    }

    private void UpdateFilteredThemes()
    {
        var filtered = AllThemes.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.ToLower();
            filtered = filtered.Where(t =>
                t.Name.ToLower().Contains(keyword) ||
                t.Description.ToLower().Contains(keyword) ||
                t.Category.ToLower().Contains(keyword));
        }

        if (CurrentCategory != "全部")
        {
            filtered = filtered.Where(t => t.Category == CurrentCategory);
        }

        FilteredThemes = new ObservableCollection<ColorTheme>(filtered);
    }

    private void InitializeToolbar()
    {
        // 可以在这里添加工具栏按钮

        MenuItemViewModel itemSeperator = new MenuItemViewModel("Seperator");
        itemSeperator.Text = "-";
        itemSeperator.IsSeparator = true;

        MenuItemViewModel itemNewTheme = new MenuItemViewModel("NewTheme");
        itemNewTheme.Icon = Icon.NewTheme;
        itemNewTheme.Text = "新建主题";
        itemNewTheme.MenuClickCommand = NewThemeCommand;

        MenuItemViewModel itemImport = new MenuItemViewModel("Import");
        itemImport.Icon = Icon.Import;
        itemImport.Text = "导入";
        itemImport.MenuClickCommand = ImportThemeCommand;

        MenuItemViewModel itemExportJson = new MenuItemViewModel("ExportJson");
        itemExportJson.Icon = Icon.Export;
        itemExportJson.Text = "导出(Json)";
        itemExportJson.MenuClickCommand = ExportThemeByJsonCommand ;

        MenuItemViewModel itemExportCSS = new MenuItemViewModel("ExportCSS");
        itemExportCSS.Icon = Icon.CSS;
        itemExportCSS.Text = "导出(CSS)";
        itemExportCSS.MenuClickCommand = ExportThemeByCssCommand;

        MenuItemViewModel itemHtml = new MenuItemViewModel("html");
        itemHtml.Icon = Icon.Html;
        itemHtml.Text = "预览(html)";
        itemHtml.MenuClickCommand = GenerateHtmlPreviewCommand;

        ChildToolbar childToolbar1 = new ChildToolbar(ToolbarTheme,
            [
                itemNewTheme,
                itemImport,
                itemExportJson,
                itemExportCSS,
                itemHtml,
            ]);

        var childTool = _systemContext.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == childToolbar1.Name);
        if (childTool != null)
        {
            _systemContext.TopToolbar.ChildTools.Remove(childTool);
        }

        _systemContext.TopToolbar.ChildTools.Add(childToolbar1);

        _systemContext.NotifyPropertyChange(true, "TopToolbar");

    }

    private void UpdateCategories()
    {
        var categories = new List<string> { "全部" };
        categories.AddRange(AllThemes.Select(t => t.Category).Distinct().OrderBy(c => c));
        Categories = new ObservableCollection<string>(categories);
    }

    /// <summary>
    /// 加载用户主题
    /// </summary>
    private async Task LoadUserThemesAsync()
    {
        var userThemes = await _storageService.LoadUserThemesAsync();
        UserThemes = new ObservableCollection<ColorTheme>(userThemes);

        foreach (var theme in userThemes)
        {
            if (!AllThemes.Any(t => t.Id == theme.Id))
            {
                AllThemes.Add(theme);
            }
        }
    }

    /// <summary>
    /// 新建主题
    /// </summary>
    [RelayCommand]
    private void NewTheme()
    {
        NewThemeName = "";
        NewThemeDescription = "";
        ShowNewThemeDialog = true;
    }

    /// <summary>
    /// 确认新建主题
    /// </summary>
    [RelayCommand]
    private async Task ConfirmNewTheme()
    {
        if (string.IsNullOrWhiteSpace(NewThemeName))
            return;

        var newTheme = new ColorTheme
        {
            Name = NewThemeName,
            Description = NewThemeDescription,
            Author = "当前用户",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _storageService.SaveThemeAsync(newTheme);

        UserThemes.Add(newTheme);
        AllThemes.Add(newTheme);
        SelectedTheme = newTheme;

        UpdateCategories();
        ShowNewThemeDialog = false;
    }

    /// <summary>
    /// 取消新建
    /// </summary>
    [RelayCommand]
    private void CancelNewTheme()
    {
        ShowNewThemeDialog = false;
    }

    /// <summary>
    /// 保存主题
    /// </summary>
    [RelayCommand]
    private async Task SaveTheme()
    {
        if (EditingTheme == null)
            return;

        EditingTheme.UpdatedAt = DateTime.Now;
        await _storageService.SaveThemeAsync(EditingTheme);
        UpdateCssPreview();
    }

    /// <summary>
    /// 另存为新主题
    /// </summary>
    [RelayCommand]
    private async Task SaveAsNewTheme()
    {
        if (EditingTheme == null)
            return;

        var clone = EditingTheme.Clone();
        clone.Name = EditingTheme.Name + " (副本)";
        clone.IsBuiltIn = false;

        await _storageService.SaveThemeAsync(clone);

        UserThemes.Add(clone);
        AllThemes.Add(clone);
        SelectedTheme = clone;

        UpdateCategories();
    }

    /// <summary>
    /// 删除主题
    /// </summary>
    [RelayCommand]
    private async Task DeleteTheme()
    {
        if (SelectedTheme == null || SelectedTheme.IsBuiltIn)
            return;

        await _storageService.DeleteThemeAsync(SelectedTheme);

        UserThemes.Remove(SelectedTheme);
        AllThemes.Remove(SelectedTheme);

        if (SelectedTheme == EditingTheme)
            EditingTheme = null;

        SelectedTheme = AllThemes.FirstOrDefault();
    }

    /// <summary>
    /// 复制主题
    /// </summary>
    [RelayCommand]
    private async Task DuplicateTheme()
    {
        if (SelectedTheme == null)
            return;

        var clone = SelectedTheme.Clone();
        await _storageService.SaveThemeAsync(clone);

        UserThemes.Add(clone);
        AllThemes.Add(clone);
        SelectedTheme = clone;

        UpdateCategories();
    }

    /// <summary>
    /// 编辑主题
    /// </summary>
    [RelayCommand]
    private void EditTheme()
    {
        if (SelectedTheme == null)
            return;

        EditingTheme = SelectedTheme.Clone();
        IsEditing = true;
    }

    /// <summary>
    /// 取消编辑
    /// </summary>
    [RelayCommand]
    private void CancelEdit()
    {
        EditingTheme = null;
        IsEditing = false;
    }

    /// <summary>
    /// 应用主题
    /// </summary>
    [RelayCommand]
    private void ApplyTheme()
    {
        if (SelectedTheme == null)
            return;

        // 更新应用主题的逻辑
        System.Diagnostics.Debug.WriteLine($"应用主题: {SelectedTheme.Name}");
    }

    /// <summary>
    /// 设置默认主题
    /// </summary>
    [RelayCommand]
    private void SetAsDefault()
    {
        if (SelectedTheme == null)
            return;

        // 取消其他默认
        foreach (var theme in AllThemes)
        {
            theme.IsDefault = false;
        }

        SelectedTheme.IsDefault = true;
    }

    /// <summary>
    /// 重置为预设
    /// </summary>
    [RelayCommand]
    private async Task ResetToPreset()
    {
        if (SelectedTheme == null || !SelectedTheme.IsBuiltIn)
            return;

        // 找到对应的预设并重置
        var preset = PresetColorSchemes.GetAll().FirstOrDefault(p => p.Id == SelectedTheme.Id);
        if (preset != null)
        {
            var index = AllThemes.IndexOf(SelectedTheme);
            AllThemes[index] = preset;
            SelectedTheme = preset;
            EditingTheme = preset.Clone();

            if (SelectedTheme.IsDefault)
            {
                preset.IsDefault = true;
            }
        }
    }

    /// <summary>
    /// 导出主题
    /// </summary>
    [RelayCommand]
    private async Task ExportTheme()
    {
        if (EditingTheme == null)
            return;

        var window = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window == null)
            return;

        var selectedFormat = SelectedExportFormat;

        var extension = selectedFormat switch
        {
            ExportFormat.Json => ".json",
            ExportFormat.Css => ".css",
            _ => ".css"
        };

        var fileName = $"{EditingTheme.Name}{extension}";

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出主题",
            SuggestedFileName = fileName
        });

        if (file != null)
        {
            switch (selectedFormat)
            {
                case ExportFormat.Json:
                    await _storageService.ExportThemeAsync(EditingTheme, file.Path.LocalPath);
                    break;
                case ExportFormat.Css:
                    await _storageService.ExportThemeToCssAsync(EditingTheme, file.Path.LocalPath);
                    break;
            }
        }
    }

    /// <summary>
    /// 导出主题
    /// </summary>
    [RelayCommand]
    private async Task ExportThemeByCss( )
    {
        if (EditingTheme == null)
            return;

        var window = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window == null)
            return;


        var extension = ".css";

        var fileName = $"{EditingTheme.Name}{extension}";

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出主题",
            SuggestedFileName = fileName
        });

        if (file != null)
        {
            await _storageService.ExportThemeToCssAsync(EditingTheme, file.Path.LocalPath);
        }
    }

    /// <summary>
    /// 导出主题
    /// </summary>
    [RelayCommand]
    private async Task ExportThemeByJson()
    {
        if (EditingTheme == null)
            return;

        var window = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window == null)
            return;


        var extension = ".json";

        var fileName = $"{EditingTheme.Name}{extension}";

        var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出主题",
            SuggestedFileName = fileName
        });

        if (file != null)
        {
            await _storageService.ExportThemeAsync(EditingTheme, file.Path.LocalPath);
        }
    }
    /// <summary>
    /// 导入主题
    /// </summary>
    [RelayCommand]
    private async Task ImportTheme()
    {
        var window = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow
            : null;

        if (window == null)
            return;

        var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "导入主题",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON 文件")
                {
                    Patterns = new[] { "*.json" }
                }
            }
        });

        if (files.Count > 0)
        {
            var theme = await _storageService.ImportThemeAsync(files[0].Path.LocalPath);
            if (theme != null)
            {
                await _storageService.SaveThemeAsync(theme);

                UserThemes.Add(theme);
                AllThemes.Add(theme);
                SelectedTheme = theme;

                UpdateCategories();
            }
        }
    }

    /// <summary>
    /// 搜索主题
    /// </summary>
    [RelayCommand]
    private void SearchTheme()
    {
        var filtered = new List<ColorTheme>();

        // 添加预设主题
        var presets = PresetColorSchemes.GetAll();

        // 搜索过滤
        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.ToLower();
            presets = presets.Where(t =>
                t.Name.ToLower().Contains(keyword) ||
                t.Description.ToLower().Contains(keyword) ||
                t.Category.ToLower().Contains(keyword)
            ).ToList();
        }

        // 分类过滤
        if (CurrentCategory != "全部")
        {
            presets = presets.Where(t => t.Category == CurrentCategory).ToList();
        }

        AllThemes = new ObservableCollection<ColorTheme>(presets);

        // 添加用户主题
        var userFiltered = UserThemes.ToList();
        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var keyword = SearchKeyword.ToLower();
            userFiltered = userFiltered.Where(t =>
                t.Name.ToLower().Contains(keyword) ||
                t.Description.ToLower().Contains(keyword) ||
                t.Category.ToLower().Contains(keyword)
            ).ToList();
        }

        foreach (var theme in userFiltered)
        {
            if (!AllThemes.Any(t => t.Id == theme.Id))
            {
                AllThemes.Add(theme);
            }
        }
    }

    /// <summary>
    /// 按分类筛选
    /// </summary>
    [RelayCommand]
    private void FilterByCategory(string category)
    {
        CurrentCategory = category;
        SearchTheme();
    }

    /// <summary>
    /// 选中主题变化
    /// </summary>
    partial void OnSelectedThemeChanged(ColorTheme? value)
    {
        if (value != null)
        {
            EditingTheme = value.Clone();
            UpdateCssPreview();
        }
    }

    /// <summary>
    /// 更新 CSS 预览
    /// </summary>
    private void UpdateCssPreview()
    {
        if (EditingTheme != null)
        {
            CssPreview = EditingTheme.ToFullCss();
        }
        else
        {
            CssPreview = "/* 选择一个主题以查看预览 */";
        }
    }

    /// <summary>
    /// 更新主题颜色
    /// </summary>
    public void UpdateThemeColor(string propertyName, string color)
    {
        if (EditingTheme == null)
            return;

        switch (propertyName)
        {
            case "PrimaryColor":
                EditingTheme.PrimaryColor = color;
                break;
            case "SecondaryColor":
                EditingTheme.SecondaryColor = color;
                break;
            case "AccentColor":
                EditingTheme.AccentColor = color;
                break;
            case "SuccessColor":
                EditingTheme.SuccessColor = color;
                break;
            case "WarningColor":
                EditingTheme.WarningColor = color;
                break;
            case "ErrorColor":
                EditingTheme.ErrorColor = color;
                break;
            case "InfoColor":
                EditingTheme.InfoColor = color;
                break;
            case "BackgroundColor":
                EditingTheme.BackgroundColor = color;
                break;
            case "ForegroundColor":
                EditingTheme.ForegroundColor = color;
                break;
            case "BorderColor":
                EditingTheme.BorderColor = color;
                break;
        }

        UpdateCssPreview();
    }

    /// <summary>
    /// 复制 CSS 到剪贴板
    /// </summary>
    [RelayCommand]
    private async Task CopyCssToClipboard()
    {
        if (string.IsNullOrEmpty(CssPreview))
            return;

        try
        {
            var window = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (window != null)
            {
                var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(window);
                if (topLevel?.Clipboard != null)
                {
                    await topLevel.Clipboard.SetTextAsync(CssPreview);
                }
            }
        }
        catch
        {
            // 忽略剪贴板操作错误
        }
    }

    /// <summary>
    /// 打开拾色器
    /// </summary>
    [RelayCommand]
    private void OpenColorPicker(string propertyName)
    {
        EditingColorProperty = propertyName;
        var currentColor = GetColorValue(propertyName);
        SetColorPickerValues(currentColor);
        ShowColorPicker = true;
    }

    /// <summary>
    /// 关闭拾色器
    /// </summary>
    [RelayCommand]
    private void CloseColorPicker()
    {
        ShowColorPicker = false;
    }

    /// <summary>
    /// 确认拾色器颜色
    /// </summary>
    [RelayCommand]
    private void ConfirmColorPicker()
    {
        var hexColor = $"#{ColorPickerRed:X2}{ColorPickerGreen:X2}{ColorPickerBlue:X2}";
        SetColorValue(EditingColorProperty, hexColor);
        ShowColorPicker = false;
        UpdateCssPreview();
    }

    /// <summary>
    /// 选择预设颜色
    /// </summary>
    public void SelectPresetColor(string color)
    {
        SetColorPickerValues(color);
    }

    /// <summary>
    /// 设置拾色器值
    /// </summary>
    private void SetColorPickerValues(string hexColor)
    {
        try
        {
            if (string.IsNullOrEmpty(hexColor)) return;
            hexColor = hexColor.TrimStart('#');
            if (hexColor.Length == 6)
            {
                ColorPickerRed = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                ColorPickerGreen = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                ColorPickerBlue = Convert.ToInt32(hexColor.Substring(4, 2), 16);
                ColorPickerHex = $"#{hexColor.ToUpper()}";
                EditingColorPreview = ColorPickerHex;
            }
        }
        catch
        {
            ColorPickerRed = 24;
            ColorPickerGreen = 144;
            ColorPickerBlue = 255;
            ColorPickerHex = "#1890FF";
            EditingColorPreview = "#1890FF";
        }
    }

    /// <summary>
    /// 获取颜色值
    /// </summary>
    private string GetColorValue(string propertyName)
    {
        if (EditingTheme == null) return "#1890FF";

        var parts = propertyName.Split('.');
        if (parts.Length == 1)
        {
            return propertyName switch
            {
                "PrimaryColor" => EditingTheme.PrimaryColor,
                "SecondaryColor" => EditingTheme.SecondaryColor,
                "AccentColor" => EditingTheme.AccentColor,
                "SuccessColor" => EditingTheme.SuccessColor,
                "WarningColor" => EditingTheme.WarningColor,
                "ErrorColor" => EditingTheme.ErrorColor,
                "InfoColor" => EditingTheme.InfoColor,
                "BackgroundColor" => EditingTheme.BackgroundColor,
                "ForegroundColor" => EditingTheme.ForegroundColor,
                "BorderColor" => EditingTheme.BorderColor,
                "DividerColor" => EditingTheme.DividerColor,
                "DisabledColor" => EditingTheme.DisabledColor,
                "HoverColor" => EditingTheme.HoverColor,
                _ => "#1890FF"
            };
        }
        else if (parts.Length == 2)
        {
            var objName = parts[0];
            var propName = parts[1];
            if (objName == "FormElementStyle")
            {
                var style = EditingTheme.FormElementStyle;
                return propName switch
                {
                    "PrimaryBackgroundColor" => style.PrimaryBackgroundColor,
                    "PrimaryForegroundColor" => style.PrimaryForegroundColor,
                    "PrimaryBorderColor" => style.PrimaryBorderColor,
                    "PrimaryHoverBackgroundColor" => style.PrimaryHoverBackgroundColor,
                    "PrimaryHoverForegroundColor" => style.PrimaryHoverForegroundColor,
                    "PrimaryHoverBorderColor" => style.PrimaryHoverBorderColor,
                    "PrimaryActiveBackgroundColor" => style.PrimaryActiveBackgroundColor,
                    "PrimaryActiveForegroundColor" => style.PrimaryActiveForegroundColor,
                    "PrimaryActiveBorderColor" => style.PrimaryActiveBorderColor,
                    "PrimaryDisabledBackgroundColor" => style.PrimaryDisabledBackgroundColor,
                    "PrimaryDisabledForegroundColor" => style.PrimaryDisabledForegroundColor,
                    "PrimaryDisabledBorderColor" => style.PrimaryDisabledBorderColor,
                    "SecondaryBackgroundColor" => style.SecondaryBackgroundColor,
                    "SecondaryForegroundColor" => style.SecondaryForegroundColor,
                    "SecondaryBorderColor" => style.SecondaryBorderColor,
                    "SecondaryHoverBackgroundColor" => style.SecondaryHoverBackgroundColor,
                    "SecondaryHoverForegroundColor" => style.SecondaryHoverForegroundColor,
                    "SecondaryHoverBorderColor" => style.SecondaryHoverBorderColor,
                    "SecondaryActiveBackgroundColor" => style.SecondaryActiveBackgroundColor,
                    "SecondaryActiveForegroundColor" => style.SecondaryActiveForegroundColor,
                    "SecondaryActiveBorderColor" => style.SecondaryActiveBorderColor,
                    "SecondaryDisabledBackgroundColor" => style.SecondaryDisabledBackgroundColor,
                    "SecondaryDisabledForegroundColor" => style.SecondaryDisabledForegroundColor,
                    "SecondaryDisabledBorderColor" => style.SecondaryDisabledBorderColor,
                    "InputBackgroundColor" => style.InputBackgroundColor,
                    "InputForegroundColor" => style.InputForegroundColor,
                    "InputBorderColor" => style.InputBorderColor,
                    "InputHoverBorderColor" => style.InputHoverBorderColor,
                    "InputFocusBorderColor" => style.InputFocusBorderColor,
                    "InputFocusShadowColor" => style.InputFocusShadowColor,
                    "InputDisabledBackgroundColor" => style.InputDisabledBackgroundColor,
                    "InputDisabledForegroundColor" => style.InputDisabledForegroundColor,
                    "InputDisabledBorderColor" => style.InputDisabledBorderColor,
                    "InputPlaceholderColor" => style.InputPlaceholderColor,
                    "CheckboxUncheckedBackgroundColor" => style.CheckboxUncheckedBackgroundColor,
                    "CheckboxUncheckedBorderColor" => style.CheckboxUncheckedBorderColor,
                    "CheckboxHoverBorderColor" => style.CheckboxHoverBorderColor,
                    "CheckboxCheckedBackgroundColor" => style.CheckboxCheckedBackgroundColor,
                    "CheckboxCheckedBorderColor" => style.CheckboxCheckedBorderColor,
                    "CheckboxCheckedIconColor" => style.CheckboxCheckedIconColor,
                    "CheckboxDisabledBackgroundColor" => style.CheckboxDisabledBackgroundColor,
                    "CheckboxDisabledBorderColor" => style.CheckboxDisabledBorderColor,
                    _ => "#1890FF"
                };
            }
            else if (objName == "PagePanelStyle")
            {
                var style = EditingTheme.PagePanelStyle;
                return propName switch
                {
                    "PageBackgroundColor" => style.PageBackgroundColor,
                    "PageForegroundColor" => style.PageForegroundColor,
                    "PageBorderColor" => style.PageBorderColor,
                    "PanelBackgroundColor" => style.PanelBackgroundColor,
                    "PanelForegroundColor" => style.PanelForegroundColor,
                    "PanelBorderColor" => style.PanelBorderColor,
                    "PanelHoverBackgroundColor" => style.PanelHoverBackgroundColor,
                    "PanelHoverBorderColor" => style.PanelHoverBorderColor,
                    "PanelSelectedBackgroundColor" => style.PanelSelectedBackgroundColor,
                    "PanelSelectedBorderColor" => style.PanelSelectedBorderColor,
                    "DividerDefaultColor" => style.DividerDefaultColor,
                    "DividerDisabledColor" => style.DividerDisabledColor,
                    "DividerVerticalColor" => style.DividerVerticalColor,
                    "TabBackgroundColor" => style.TabBackgroundColor,
                    "TabForegroundColor" => style.TabForegroundColor,
                    "TabHoverBackgroundColor" => style.TabHoverBackgroundColor,
                    "TabHoverForegroundColor" => style.TabHoverForegroundColor,
                    "TabSelectedBackgroundColor" => style.TabSelectedBackgroundColor,
                    "TabSelectedForegroundColor" => style.TabSelectedForegroundColor,
                    "TabSelectedUnderlineColor" => style.TabSelectedUnderlineColor,
                    "TabDisabledForegroundColor" => style.TabDisabledForegroundColor,
                    "ToolbarBackgroundColor" => style.ToolbarBackgroundColor,
                    "ToolbarForegroundColor" => style.ToolbarForegroundColor,
                    "ToolbarBorderColor" => style.ToolbarBorderColor,
                    "ToolbarItemHoverBackgroundColor" => style.ToolbarItemHoverBackgroundColor,
                    "ToolbarItemSelectedBackgroundColor" => style.ToolbarItemSelectedBackgroundColor,
                    "ToolbarItemSelectedForegroundColor" => style.ToolbarItemSelectedForegroundColor,
                    "SidebarBackgroundColor" => style.SidebarBackgroundColor,
                    "SidebarForegroundColor" => style.SidebarForegroundColor,
                    "SidebarBorderColor" => style.SidebarBorderColor,
                    "SidebarItemHoverBackgroundColor" => style.SidebarItemHoverBackgroundColor,
                    "SidebarItemSelectedBackgroundColor" => style.SidebarItemSelectedBackgroundColor,
                    "SidebarItemSelectedForegroundColor" => style.SidebarItemSelectedForegroundColor,
                    "SidebarItemDisabledForegroundColor" => style.SidebarItemDisabledForegroundColor,
                    "ListItemBackgroundColor" => style.ListItemBackgroundColor,
                    "ListItemForegroundColor" => style.ListItemForegroundColor,
                    "ListItemHoverBackgroundColor" => style.ListItemHoverBackgroundColor,
                    "ListItemSelectedBackgroundColor" => style.ListItemSelectedBackgroundColor,
                    "ListItemSelectedForegroundColor" => style.ListItemSelectedForegroundColor,
                    "ListItemActiveBackgroundColor" => style.ListItemActiveBackgroundColor,
                    "ListItemActiveForegroundColor" => style.ListItemActiveForegroundColor,
                    "ListItemDisabledForegroundColor" => style.ListItemDisabledForegroundColor,
                    "TableHeaderBackgroundColor" => style.TableHeaderBackgroundColor,
                    "TableHeaderForegroundColor" => style.TableHeaderForegroundColor,
                    "TableHeaderBorderColor" => style.TableHeaderBorderColor,
                    "TableCellBackgroundColor" => style.TableCellBackgroundColor,
                    "TableCellForegroundColor" => style.TableCellForegroundColor,
                    "TableCellBorderColor" => style.TableCellBorderColor,
                    "TableRowHoverBackgroundColor" => style.TableRowHoverBackgroundColor,
                    "TableRowSelectedBackgroundColor" => style.TableRowSelectedBackgroundColor,
                    _ => "#1890FF"
                };
            }
        }
        return "#1890FF";
    }

    /// <summary>
    /// 设置颜色值
    /// </summary>
    private void SetColorValue(string propertyName, string color)
    {
        if (EditingTheme == null) return;

        var parts = propertyName.Split('.');
        if (parts.Length == 1)
        {
            switch (propertyName)
            {
                case "PrimaryColor": EditingTheme.PrimaryColor = color; break;
                case "SecondaryColor": EditingTheme.SecondaryColor = color; break;
                case "AccentColor": EditingTheme.AccentColor = color; break;
                case "SuccessColor": EditingTheme.SuccessColor = color; break;
                case "WarningColor": EditingTheme.WarningColor = color; break;
                case "ErrorColor": EditingTheme.ErrorColor = color; break;
                case "InfoColor": EditingTheme.InfoColor = color; break;
                case "BackgroundColor": EditingTheme.BackgroundColor = color; break;
                case "ForegroundColor": EditingTheme.ForegroundColor = color; break;
                case "BorderColor": EditingTheme.BorderColor = color; break;
                case "DividerColor": EditingTheme.DividerColor = color; break;
                case "DisabledColor": EditingTheme.DisabledColor = color; break;
                case "HoverColor": EditingTheme.HoverColor = color; break;
            }
        }
        else if (parts.Length == 2)
        {
            var objName = parts[0];
            var propName = parts[1];
            if (objName == "FormElementStyle")
            {
                var style = EditingTheme.FormElementStyle;
                switch (propName)
                {
                    case "PrimaryBackgroundColor": style.PrimaryBackgroundColor = color; break;
                    case "PrimaryForegroundColor": style.PrimaryForegroundColor = color; break;
                    case "PrimaryBorderColor": style.PrimaryBorderColor = color; break;
                    case "PrimaryHoverBackgroundColor": style.PrimaryHoverBackgroundColor = color; break;
                    case "PrimaryHoverForegroundColor": style.PrimaryHoverForegroundColor = color; break;
                    case "PrimaryHoverBorderColor": style.PrimaryHoverBorderColor = color; break;
                    case "PrimaryActiveBackgroundColor": style.PrimaryActiveBackgroundColor = color; break;
                    case "PrimaryActiveForegroundColor": style.PrimaryActiveForegroundColor = color; break;
                    case "PrimaryActiveBorderColor": style.PrimaryActiveBorderColor = color; break;
                    case "PrimaryDisabledBackgroundColor": style.PrimaryDisabledBackgroundColor = color; break;
                    case "PrimaryDisabledForegroundColor": style.PrimaryDisabledForegroundColor = color; break;
                    case "PrimaryDisabledBorderColor": style.PrimaryDisabledBorderColor = color; break;
                    case "SecondaryBackgroundColor": style.SecondaryBackgroundColor = color; break;
                    case "SecondaryForegroundColor": style.SecondaryForegroundColor = color; break;
                    case "SecondaryBorderColor": style.SecondaryBorderColor = color; break;
                    case "SecondaryHoverBackgroundColor": style.SecondaryHoverBackgroundColor = color; break;
                    case "SecondaryHoverForegroundColor": style.SecondaryHoverForegroundColor = color; break;
                    case "SecondaryHoverBorderColor": style.SecondaryHoverBorderColor = color; break;
                    case "SecondaryActiveBackgroundColor": style.SecondaryActiveBackgroundColor = color; break;
                    case "SecondaryActiveForegroundColor": style.SecondaryActiveForegroundColor = color; break;
                    case "SecondaryActiveBorderColor": style.SecondaryActiveBorderColor = color; break;
                    case "SecondaryDisabledBackgroundColor": style.SecondaryDisabledBackgroundColor = color; break;
                    case "SecondaryDisabledForegroundColor": style.SecondaryDisabledForegroundColor = color; break;
                    case "SecondaryDisabledBorderColor": style.SecondaryDisabledBorderColor = color; break;
                    case "InputBackgroundColor": style.InputBackgroundColor = color; break;
                    case "InputForegroundColor": style.InputForegroundColor = color; break;
                    case "InputBorderColor": style.InputBorderColor = color; break;
                    case "InputHoverBorderColor": style.InputHoverBorderColor = color; break;
                    case "InputFocusBorderColor": style.InputFocusBorderColor = color; break;
                    case "InputFocusShadowColor": style.InputFocusShadowColor = color; break;
                    case "InputDisabledBackgroundColor": style.InputDisabledBackgroundColor = color; break;
                    case "InputDisabledForegroundColor": style.InputDisabledForegroundColor = color; break;
                    case "InputDisabledBorderColor": style.InputDisabledBorderColor = color; break;
                    case "InputPlaceholderColor": style.InputPlaceholderColor = color; break;
                    case "CheckboxUncheckedBackgroundColor": style.CheckboxUncheckedBackgroundColor = color; break;
                    case "CheckboxUncheckedBorderColor": style.CheckboxUncheckedBorderColor = color; break;
                    case "CheckboxHoverBorderColor": style.CheckboxHoverBorderColor = color; break;
                    case "CheckboxCheckedBackgroundColor": style.CheckboxCheckedBackgroundColor = color; break;
                    case "CheckboxCheckedBorderColor": style.CheckboxCheckedBorderColor = color; break;
                    case "CheckboxCheckedIconColor": style.CheckboxCheckedIconColor = color; break;
                    case "CheckboxDisabledBackgroundColor": style.CheckboxDisabledBackgroundColor = color; break;
                    case "CheckboxDisabledBorderColor": style.CheckboxDisabledBorderColor = color; break;
                }
            }
            else if (objName == "PagePanelStyle")
            {
                var style = EditingTheme.PagePanelStyle;
                switch (propName)
                {
                    case "PageBackgroundColor": style.PageBackgroundColor = color; break;
                    case "PageForegroundColor": style.PageForegroundColor = color; break;
                    case "PageBorderColor": style.PageBorderColor = color; break;
                    case "PanelBackgroundColor": style.PanelBackgroundColor = color; break;
                    case "PanelForegroundColor": style.PanelForegroundColor = color; break;
                    case "PanelBorderColor": style.PanelBorderColor = color; break;
                    case "PanelHoverBackgroundColor": style.PanelHoverBackgroundColor = color; break;
                    case "PanelHoverBorderColor": style.PanelHoverBorderColor = color; break;
                    case "PanelSelectedBackgroundColor": style.PanelSelectedBackgroundColor = color; break;
                    case "PanelSelectedBorderColor": style.PanelSelectedBorderColor = color; break;
                    case "DividerDefaultColor": style.DividerDefaultColor = color; break;
                    case "DividerDisabledColor": style.DividerDisabledColor = color; break;
                    case "DividerVerticalColor": style.DividerVerticalColor = color; break;
                    case "TabBackgroundColor": style.TabBackgroundColor = color; break;
                    case "TabForegroundColor": style.TabForegroundColor = color; break;
                    case "TabHoverBackgroundColor": style.TabHoverBackgroundColor = color; break;
                    case "TabHoverForegroundColor": style.TabHoverForegroundColor = color; break;
                    case "TabSelectedBackgroundColor": style.TabSelectedBackgroundColor = color; break;
                    case "TabSelectedForegroundColor": style.TabSelectedForegroundColor = color; break;
                    case "TabSelectedUnderlineColor": style.TabSelectedUnderlineColor = color; break;
                    case "TabDisabledForegroundColor": style.TabDisabledForegroundColor = color; break;
                    case "ToolbarBackgroundColor": style.ToolbarBackgroundColor = color; break;
                    case "ToolbarForegroundColor": style.ToolbarForegroundColor = color; break;
                    case "ToolbarBorderColor": style.ToolbarBorderColor = color; break;
                    case "ToolbarItemHoverBackgroundColor": style.ToolbarItemHoverBackgroundColor = color; break;
                    case "ToolbarItemSelectedBackgroundColor": style.ToolbarItemSelectedBackgroundColor = color; break;
                    case "ToolbarItemSelectedForegroundColor": style.ToolbarItemSelectedForegroundColor = color; break;
                    case "SidebarBackgroundColor": style.SidebarBackgroundColor = color; break;
                    case "SidebarForegroundColor": style.SidebarForegroundColor = color; break;
                    case "SidebarBorderColor": style.SidebarBorderColor = color; break;
                    case "SidebarItemHoverBackgroundColor": style.SidebarItemHoverBackgroundColor = color; break;
                    case "SidebarItemSelectedBackgroundColor": style.SidebarItemSelectedBackgroundColor = color; break;
                    case "SidebarItemSelectedForegroundColor": style.SidebarItemSelectedForegroundColor = color; break;
                    case "SidebarItemDisabledForegroundColor": style.SidebarItemDisabledForegroundColor = color; break;
                    case "ListItemBackgroundColor": style.ListItemBackgroundColor = color; break;
                    case "ListItemForegroundColor": style.ListItemForegroundColor = color; break;
                    case "ListItemHoverBackgroundColor": style.ListItemHoverBackgroundColor = color; break;
                    case "ListItemSelectedBackgroundColor": style.ListItemSelectedBackgroundColor = color; break;
                    case "ListItemSelectedForegroundColor": style.ListItemSelectedForegroundColor = color; break;
                    case "ListItemActiveBackgroundColor": style.ListItemActiveBackgroundColor = color; break;
                    case "ListItemActiveForegroundColor": style.ListItemActiveForegroundColor = color; break;
                    case "ListItemDisabledForegroundColor": style.ListItemDisabledForegroundColor = color; break;
                    case "TableHeaderBackgroundColor": style.TableHeaderBackgroundColor = color; break;
                    case "TableHeaderForegroundColor": style.TableHeaderForegroundColor = color; break;
                    case "TableHeaderBorderColor": style.TableHeaderBorderColor = color; break;
                    case "TableCellBackgroundColor": style.TableCellBackgroundColor = color; break;
                    case "TableCellForegroundColor": style.TableCellForegroundColor = color; break;
                    case "TableCellBorderColor": style.TableCellBorderColor = color; break;
                    case "TableRowHoverBackgroundColor": style.TableRowHoverBackgroundColor = color; break;
                    case "TableRowSelectedBackgroundColor": style.TableRowSelectedBackgroundColor = color; break;
                }
            }
        }
    }

    partial void OnColorPickerRedChanged(int value) => UpdateHexFromRgb();
    partial void OnColorPickerGreenChanged(int value) => UpdateHexFromRgb();
    partial void OnColorPickerBlueChanged(int value) => UpdateHexFromRgb();

    private void UpdateHexFromRgb()
    {
        ColorPickerHex = $"#{ColorPickerRed:X2}{ColorPickerGreen:X2}{ColorPickerBlue:X2}";
        EditingColorPreview = ColorPickerHex;
    }

    /// <summary>
    /// 应用预设颜色到主题
    /// </summary>
    [RelayCommand]
    private void ApplyPresetColors(string presetName)
    {
        if (EditingTheme == null)
            return;

        switch (presetName)
        {
            case "Ant Design":
                ApplyAntDesignColors();
                break;
            case "Material Design":
                ApplyMaterialDesignColors();
                break;
            case "Bootstrap":
                ApplyBootstrapColors();
                break;
        }

        UpdateCssPreview();
    }

    private void ApplyAntDesignColors()
    {
        if (EditingTheme == null) return;
        EditingTheme.PrimaryColor = "#1890FF";
        EditingTheme.SuccessColor = "#52C41A";
        EditingTheme.WarningColor = "#FAAD14";
        EditingTheme.ErrorColor = "#FF4D4F";
        EditingTheme.InfoColor = "#1890FF";
    }

    private void ApplyMaterialDesignColors()
    {
        if (EditingTheme == null) return;
        EditingTheme.PrimaryColor = "#6200EE";
        EditingTheme.SuccessColor = "#00C853";
        EditingTheme.WarningColor = "#FFD600";
        EditingTheme.ErrorColor = "#B00020";
        EditingTheme.InfoColor = "#018786";
    }

    private void ApplyBootstrapColors()
    {
        if (EditingTheme == null) return;
        EditingTheme.PrimaryColor = "#0D6EFD";
        EditingTheme.SuccessColor = "#198754";
        EditingTheme.WarningColor = "#FFC107";
        EditingTheme.ErrorColor = "#DC3545";
        EditingTheme.InfoColor = "#0DCAF0";
    }

    /// <summary>
    /// 生成HTML预览
    /// </summary>
    [RelayCommand]
    private async Task GenerateHtmlPreview()
    {
        if (EditingTheme == null)
            return;

        try
        {
            var html = Services.HtmlPreviewGenerator.GenerateHtmlPreview(EditingTheme);
            
            var tempPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"ColorTheme_{EditingTheme.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            );
            
            await System.IO.File.WriteAllTextAsync(tempPath, html);
            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"生成HTML预览失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 为所有主题生成HTML预览
    /// </summary>
    [RelayCommand]
    private async Task GenerateAllHtmlPreviews()
    {
        try
        {
            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ColorThemePreviews"
            );
            
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
            
            foreach (var theme in AllThemes)
            {
                var html = Services.HtmlPreviewGenerator.GenerateHtmlPreview(theme);
                var fileName = $"{theme.Name}_{DateTime.Now:yyyyMMdd}.html";
                var invalidChars = System.IO.Path.GetInvalidFileNameChars();
                fileName = string.Join("_", fileName.Split(invalidChars));
                
                var filePath = System.IO.Path.Combine(folder, fileName);
                await System.IO.File.WriteAllTextAsync(filePath, html);
            }
            
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = folder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"生成HTML预览失败: {ex.Message}");
        }
    }

    public override void UpdateLanguageTexts()
    {
        // 更新多语言文本
    }

    public override void Dispose()
    {
        // 清理资源
        var childTool = _systemContext.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == ToolbarTheme);
        if (childTool != null)
        {
            _systemContext.TopToolbar.ChildTools.Remove(childTool);
            _systemContext.NotifyPropertyChange(true, "TopToolbar");
        }
    }
}
