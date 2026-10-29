using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;
using yuyiAvalonia.Base.ViewModels.Pages;

namespace yuyiAvalonia.Java.ViewModels;

/// <summary>
/// Java 工具视图模型
/// </summary>
public partial class JavaViewModel : PageViewModelBase
{
    SystemContext? _systemContext;
    private const string ToolbarJava = "toolbar_Java";

    #region 属性

    [ObservableProperty]
    private ObservableCollection<JavaInstallation> _installedJavaVersions = new();

    [ObservableProperty]
    private ObservableCollection<JavaVersionInfo> _availableVersions = new();

    [ObservableProperty]
    private JavaInstallation? _selectedInstalledVersion;

    [ObservableProperty]
    private JavaVersionInfo? _selectedAvailableVersion;

    [ObservableProperty]
    private string _javaHome = "";

    [ObservableProperty]
    private string _searchKeyword = "";

    [ObservableProperty]
    private int _currentTabIndex;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private string _currentJavaVersion = "";

    [ObservableProperty]
    private string _currentJavacVersion = "";

    public static JavaViewModel? Instance { get; private set; }

    #endregion

    #region 构造函数

    public JavaViewModel()
    {
        Instance = this;
    }

    #endregion

    #region 生命周期

    public override void Initialize(SystemContext? context)
    {
        if (context == null)
            return;

        _systemContext = context;
        InitializeToolbar();
        LoadJavaInfo();
        LoadInstalledVersions();
        LoadAvailableVersions();
    }

    private void InitializeToolbar()
    {
        MenuItemViewModel itemRefresh = new MenuItemViewModel("Refresh");
        itemRefresh.Icon = Icon.Refresh;
        itemRefresh.Text = "刷新";
        itemRefresh.MenuClickCommand = this.RefreshCommand;

        MenuItemViewModel itemOpenFolder = new MenuItemViewModel("OpenFolder");
        itemOpenFolder.Icon = Icon.OpenFolder;
        itemOpenFolder.Text = "打开 Java 目录";
        itemOpenFolder.MenuClickCommand = this.OpenJavaHomeCommand;

        ChildToolbar childToolbar = new ChildToolbar(ToolbarJava,
        [
            itemRefresh,
            itemOpenFolder
        ]);

        var existingTool = _systemContext?.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == childToolbar.Name);
        if (existingTool != null)
        {
            _systemContext?.TopToolbar.ChildTools.Remove(existingTool);
        }

        _systemContext?.TopToolbar.ChildTools.Add(childToolbar);
        _systemContext?.NotifyPropertyChange(true, "TopToolbar");
    }

    public override void UpdateLanguageTexts()
    {
    }

    public override void Dispose()
    {
        Instance = null;

        var childTool = _systemContext?.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == ToolbarJava);
        if (childTool != null)
        {
            _systemContext?.TopToolbar.ChildTools.Remove(childTool);
            _systemContext?.NotifyPropertyChange(true, "TopToolbar");
        }
    }

    #endregion

    #region 命令

    [RelayCommand]
    private void Refresh()
    {
        LoadJavaInfo();
        LoadInstalledVersions();
        LoadAvailableVersions();
    }

    [RelayCommand]
    private void LoadJavaInfo()
    {
        try
        {
            //// 获取 Java 版本
            //var javaVersion = CommandHelper.ExecuteCommand("java -version");
            //CurrentJavaVersion = ParseJavaVersion(javaVersion);

            //// 获取 Javac 版本
            //var javacVersion = CommandHelper.ExecuteCommand("javac -version");
            //CurrentJavacVersion = javacVersion.Trim();

            // 获取 JAVA_HOME
            JavaHome = Environment.GetEnvironmentVariable("JAVA_HOME") ?? "";
            StatusMessage = "Java 环境正常";
        }
        catch
        {
            CurrentJavaVersion = "未安装";
            CurrentJavacVersion = "未安装";
            JavaHome = "";
            StatusMessage = "未检测到 Java 环境";
        }
    }

    [RelayCommand]
    private void OpenJavaHome()
    {
        if (!string.IsNullOrEmpty(JavaHome))
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = JavaHome,
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }

    [RelayCommand]
    private void SetAsDefault()
    {
        if (SelectedInstalledVersion == null)
            return;

        // TODO: 实现设置默认 Java 版本
        StatusMessage = $"已将 {SelectedInstalledVersion.Version} 设置为默认版本";
    }

    [RelayCommand]
    private void Uninstall()
    {
        if (SelectedInstalledVersion == null)
            return;

        // TODO: 实现卸载 Java
        StatusMessage = $"正在卸载 {SelectedInstalledVersion.Version}...";
    }

    #endregion

    #region 私有方法

    private string ParseJavaVersion(string versionOutput)
    {
        if (string.IsNullOrEmpty(versionOutput))
            return "未知";

        // 解析 java -version 输出
        var lines = versionOutput.Split('\n');
        foreach (var line in lines)
        {
            if (line.Contains("version"))
            {
                // 提取版本号
                var start = line.IndexOf("\"");
                var end = line.LastIndexOf("\"");
                if (start >= 0 && end > start)
                {
                    return line.Substring(start + 1, end - start - 1);
                }
            }
        }
        return versionOutput.Split('\n').FirstOrDefault()?.Trim() ?? "未知";
    }

    private void LoadInstalledVersions()
    {
        InstalledJavaVersions.Clear();

        // TODO: 实际检测系统已安装的 Java 版本
        // 目前添加示例数据
        InstalledJavaVersions.Add(new JavaInstallation
        {
            Version = "Java 21",
            Vendor = "Oracle",
            Path = @"C:\Program Files\Java\jdk-21",
            IsDefault = true
        });

        InstalledJavaVersions.Add(new JavaInstallation
        {
            Version = "Java 17",
            Vendor = "Eclipse Temurin",
            Path = @"C:\Program Files\Eclipse Adoptium\jdk-17.0.9",
            IsDefault = false
        });
    }

    private void LoadAvailableVersions()
    {
        AvailableVersions.Clear();

        // TODO: 从网络获取可用的 Java 版本
        // 目前添加示例数据
        AvailableVersions.Add(new JavaVersionInfo
        {
            Version = "Java 21 LTS",
            Vendor = "Oracle",
            Size = "156 MB",
            ReleaseDate = DateTime.Now.AddDays(-30)
        });

        AvailableVersions.Add(new JavaVersionInfo
        {
            Version = "Java 17 LTS",
            Vendor = "Eclipse Temurin",
            Size = "148 MB",
            ReleaseDate = DateTime.Now.AddDays(-60)
        });

        AvailableVersions.Add(new JavaVersionInfo
        {
            Version = "Java 11",
            Vendor = "Amazon Corretto",
            Size = "189 MB",
            ReleaseDate = DateTime.Now.AddDays(-90)
        });
    }

    #endregion
}

#region 模型类

public class JavaInstallation
{
    public string Version { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Path { get; set; } = "";
    public bool IsDefault { get; set; }
    public string DisplayName => $"{Version} ({Vendor})";
}

public class JavaVersionInfo
{
    public string Version { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Size { get; set; } = "";
    public DateTime ReleaseDate { get; set; }
}

#endregion
