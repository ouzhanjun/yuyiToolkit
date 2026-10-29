using System.IO;
using System.Text.Json;
using yuyiAvalonia.Software.Models;

namespace yuyiAvalonia.Software.Services;

/// <summary>
/// 软件存储服务 - 负责软件列表的持久化存储
/// </summary>
public class SoftwareStorageService
{
    private readonly string _dataPath;
    private readonly string _presetsPath;
    private readonly string _userDataPath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SoftwareStorageService()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var dataFolder = Path.Combine(basePath, "data");

        if (!Directory.Exists(dataFolder))
            Directory.CreateDirectory(dataFolder);

        _presetsPath = Path.Combine(dataFolder, "software_presets.json");
        _userDataPath = Path.Combine(dataFolder, "software_user.json");

        if (!File.Exists(_presetsPath))
            CreatePresetSoftwareList();
    }

    /// <summary>
    /// 加载所有软件（预设 + 用户）
    /// </summary>
    public (List<SoftwareItem> presets, List<SoftwareItem> user) LoadAllSoftware()
    {
        var presets = LoadPresets();
        var userItems = LoadUserSoftware();
        return (presets, userItems);
    }

    /// <summary>
    /// 加载预设软件列表
    /// </summary>
    public List<SoftwareItem> LoadPresets()
    {
        try
        {
            if (File.Exists(_presetsPath))
            {
                var json = File.ReadAllText(_presetsPath);
                var config = JsonSerializer.Deserialize<SoftwareConfig>(json, _jsonOptions);
                return config?.Items ?? new List<SoftwareItem>();
            }
        }
        catch { }
        return new List<SoftwareItem>();
    }

    /// <summary>
    /// 加载用户软件列表
    /// </summary>
    public List<SoftwareItem> LoadUserSoftware()
    {
        try
        {
            if (File.Exists(_userDataPath))
            {
                var json = File.ReadAllText(_userDataPath);
                var config = JsonSerializer.Deserialize<SoftwareConfig>(json, _jsonOptions);
                return config?.Items ?? new List<SoftwareItem>();
            }
        }
        catch { }
        return new List<SoftwareItem>();
    }

    /// <summary>
    /// 保存用户软件列表
    /// </summary>
    public async Task SaveUserSoftwareAsync(List<SoftwareItem> items)
    {
        try
        {
            var config = new SoftwareConfig
            {
                Items = items,
                UpdatedAt = DateTime.Now
            };
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            await File.WriteAllTextAsync(_userDataPath, json);
        }
        catch { }
    }

    /// <summary>
    /// 保存用户软件列表（同步）
    /// </summary>
    public void SaveUserSoftware(List<SoftwareItem> items)
    {
        try
        {
            var config = new SoftwareConfig
            {
                Items = items,
                UpdatedAt = DateTime.Now
            };
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            File.WriteAllText(_userDataPath, json);
        }
        catch { }
    }

    /// <summary>
    /// 创建预设软件列表
    /// </summary>
    private void CreatePresetSoftwareList()
    {
        var presets = new SoftwareConfig
        {
            Id = "default",
            Name = "常用软件",
            Description = "预设的常用软件列表",
            Items = new List<SoftwareItem>
            {
                new() { Id = "vscode", Name = "Visual Studio Code", Description = "微软开源的现代化代码编辑器，支持多种编程语言", Version = "1.90", Category = "开发工具", DownloadUrl = "https://code.visualstudio.com/sha/download?build=stable&os=win32-x64-user", IconUrl = "", Website = "https://code.visualstudio.com/", Size = "~80MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "chrome", Name = "Google Chrome", Description = "Google开发的快速、安全的网页浏览器", Version = "125", Category = "浏览器", DownloadUrl = "https://dl.google.com/chrome/install/standalone/ChromeStandaloneSetup64.exe", IconUrl = "", Website = "https://www.google.com/chrome/", Size = "~100MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "firefox", Name = "Mozilla Firefox", Description = "Mozilla基金会开发的自由开源网页浏览器", Version = "126", Category = "浏览器", DownloadUrl = "https://download.mozilla.org/?product=firefox-latest&os=win64&lang=zh-CN", IconUrl = "", Website = "https://www.mozilla.org/", Size = "~80MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "edge", Name = "Microsoft Edge", Description = "微软基于Chromium开发的网页浏览器", Version = "125", Category = "浏览器", DownloadUrl = "https://go.microsoft.com/fwlink/?linkid=2108834&Channel=Stable&language=zh-cn&brand=M102", IconUrl = "", Website = "https://www.microsoft.com/edge", Size = "~120MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "notepadpp", Name = "Notepad++", Description = "功能强大的免费源代码编辑器和记事本替代品", Version = "8.6", Category = "开发工具", DownloadUrl = "https://github.com/notepad-plus-plus/notepad-plus-plus/releases/download/v8.6.4/npp.8.6.4.Installer.x64.exe", IconUrl = "", Website = "https://notepad-plus-plus.org/", Size = "~4MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "git", Name = "Git", Description = "分布式版本控制系统，用于跟踪代码变更", Version = "2.45", Category = "开发工具", DownloadUrl = "https://github.com/git-for-windows/git/releases/download/v2.45.1.windows.1/Git-2.45.1-64-bit.exe", IconUrl = "", Website = "https://git-scm.com/", Size = "~50MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "nodejs", Name = "Node.js LTS", Description = "基于Chrome V8引擎的JavaScript运行时环境", Version = "20", Category = "开发工具", DownloadUrl = "https://nodejs.org/dist/v20.13.1/node-v20.13.1-x64.msi", IconUrl = "", Website = "https://nodejs.org/", Size = "~30MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "7zip", Name = "7-Zip", Description = "开源免费的高压缩比文件压缩工具", Version = "24.07", Category = "压缩工具", DownloadUrl = "https://www.7-zip.org/a/7z2407-x64.exe", IconUrl = "", Website = "https://www.7-zip.org/", Size = "~2MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "bandizip", Name = "Bandizip", Description = "轻量级、快速、支持多格式的压缩软件", Version = "7", Category = "压缩工具", DownloadUrl = "https://en.bandisoft.com/bandizip/dl.php?win64", IconUrl = "", Website = "https://www.bandisoft.com/bandizip/", Size = "~5MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "winrar", Name = "WinRAR", Description = "功能强大的压缩文件管理工具", Version = "7", Category = "压缩工具", DownloadUrl = "https://www.rarlab.com/rar/winrar-x64-701scp.exe", IconUrl = "", Website = "https://www.win-rar.com/", Size = "~4MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "wps", Name = "WPS Office", Description = "金山办公出品的国产办公软件套装", Version = "12", Category = "办公软件", DownloadUrl = "https://official-package.wpscdn.cn/wps/download/WPSOffice_12.8.2.11258.exe", IconUrl = "", Website = "https://www.wps.cn/", Size = "~250MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "office", Name = "Microsoft Office", Description = "微软办公软件套装（需购买许可证）", Version = "2021", Category = "办公软件", DownloadUrl = "", IconUrl = "", Website = "https://www.microsoft.com/microsoft-365/microsoft-365", Size = "", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "foxit", Name = "Foxit PDF Reader", Description = "轻量级PDF阅读器，功能丰富", Version = "13", Category = "办公软件", DownloadUrl = "https://www.foxit.com/downloads/latest.html?product=Foxit-Reader-CN&platform=Windows&package=Exe&version=13.2.2&language=Chinese", IconUrl = "", Website = "https://www.foxit.com/", Size = "~180MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "snipaste", Name = "Snipaste", Description = "简单强大的截图贴图工具", Version = "2.8", Category = "系统工具", DownloadUrl = "https://download.snipaste.com/releases/Snipaste-2.8.5-Beta-x64.zip", IconUrl = "", Website = "https://www.snipaste.com/", Size = "~20MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "listary", Name = "Listary", Description = "强大的文件搜索和快速启动工具", Version = "6", Category = "系统工具", DownloadUrl = "https://www.listary.com/download/listary6.exe", IconUrl = "", Website = "https://www.listary.com/", Size = "~10MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "everything", Name = "Everything", Description = "超快速文件名搜索工具", Version = "1.4", Category = "系统工具", DownloadUrl = "https://www.voidtools.com/Everything-1.4.1.1026.x64-Setup.exe", IconUrl = "", Website = "https://www.voidtools.com/", Size = "~2MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "dish", Name = "Ditto", Description = "Windows剪贴板增强工具，支持历史记录", Version = "3", Category = "系统工具", DownloadUrl = "https://github.com/sabrogden/Ditto/releases/download/3.24.246.0/DittoSetup_3_24_246_0_x64.exe", IconUrl = "", Website = "https://ditto-cp.sourceforge.io/", Size = "~5MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "qbittorrent", Name = "qBittorrent", Description = "开源免费的BitTorrent下载客户端", Version = "4.6", Category = "系统工具", DownloadUrl = "https://sourceforge.net/projects/qbittorrent/files/qbittorrent-win32/qbittorrent-4.6.5/qbittorrent-4.6.5_x64_setup.exe/download", IconUrl = "", Website = "https://www.qbittorrent.org/", Size = "~45MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "potplayer", Name = "PotPlayer", Description = "功能强大的多媒体播放器", Version = "6", Category = "音视频", DownloadUrl = "https://daumpotplayer.com/download/", IconUrl = "", Website = "https://daumpotplayer.com/", Size = "~40MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "vlc", Name = "VLC Media Player", Description = "开源跨平台多媒体播放器", Version = "3", Category = "音视频", DownloadUrl = "https://get.videolan.org/vlc/last/win64/VLC-3.0.21-win64.exe", IconUrl = "", Website = "https://www.videolan.org/", Size = "~45MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "steam", Name = "Steam", Description = "全球最大的数字游戏发行平台", Version = "", Category = "游戏平台", DownloadUrl = "https://cdn.akamai.steamstatic.com/client/installer/SteamSetup.exe", IconUrl = "", Website = "https://store.steampowered.com/", Size = "~5MB", IsBuiltIn = true, LastUpdated = DateTime.Now },
                new() { Id = "epic", Name = "Epic Games", Description = "Epic游戏平台，提供免费游戏", Version = "", Category = "游戏平台", DownloadUrl = "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/installer/download/EpicGamesLauncherInstaller.msi", IconUrl = "", Website = "https://www.epicgames.com/", Size = "~70MB", IsBuiltIn = true, LastUpdated = DateTime.Now }
            }
        };

        var json = JsonSerializer.Serialize(presets, _jsonOptions);
        File.WriteAllText(_presetsPath, json);
    }
}
