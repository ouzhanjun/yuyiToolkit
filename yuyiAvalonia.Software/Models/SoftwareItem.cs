using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.Software.Models;

/// <summary>
/// 软件项 - 表示一个可下载的软件
/// </summary>
public partial class SoftwareItem : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _downloadUrl = string.Empty;

    [ObservableProperty]
    private string _iconUrl = string.Empty;

    [ObservableProperty]
    private string _website = string.Empty;

    [ObservableProperty]
    private string _category = "其他";

    [ObservableProperty]
    private string _size = string.Empty;

    [ObservableProperty]
    private DateTime _lastUpdated = DateTime.Now;

    [ObservableProperty]
    private bool _isBuiltIn = false;

    [ObservableProperty]
    private bool _isFavorite = false;

    [ObservableProperty]
    private int _downloadCount = 0;

    [ObservableProperty]
    private SoftwareStatus _status = SoftwareStatus.NotDownloaded;

    [ObservableProperty]
    private double _downloadProgress = 0;

    [ObservableProperty]
    private string _localPath = string.Empty;

    [ObservableProperty]
    private string _publisher = string.Empty;

    [ObservableProperty]
    private long _fileSizeBytes = 0;

    [ObservableProperty]
    private string _checksum = string.Empty;

    [ObservableProperty]
    private List<string> _tags = new();

    [ObservableProperty]
    private string _license = string.Empty;

    [ObservableProperty]
    private int _rating = 0;

    [ObservableProperty]
    private string _operatingSystem = string.Empty;

    [ObservableProperty]
    private string _architecture = string.Empty;

    [ObservableProperty]
    private DateTime? _lastDownloaded;

    /// <summary>
    /// 创建副本
    /// </summary>
    public SoftwareItem Clone()
    {
        return new SoftwareItem
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Version = Version,
            DownloadUrl = DownloadUrl,
            IconUrl = IconUrl,
            Website = Website,
            Category = Category,
            Size = Size,
            LastUpdated = LastUpdated,
            IsBuiltIn = IsBuiltIn,
            IsFavorite = IsFavorite,
            DownloadCount = DownloadCount,
            Status = Status,
            DownloadProgress = DownloadProgress,
            LocalPath = LocalPath,
            Publisher = Publisher,
            FileSizeBytes = FileSizeBytes,
            Checksum = Checksum,
            Tags = new List<string>(Tags),
            License = License,
            Rating = Rating,
            OperatingSystem = OperatingSystem,
            Architecture = Architecture,
            LastDownloaded = LastDownloaded
        };
    }
}

/// <summary>
/// 软件状态
/// </summary>
public enum SoftwareStatus
{
    NotDownloaded,
    Downloading,
    Downloaded,
    UpdateAvailable,
    Error
}

/// <summary>
/// 软件分类统计
/// </summary>
public class SoftwareCategoryStat
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<SoftwareItem> Items { get; set; } = new();
}

/// <summary>
/// 软件列表配置
/// </summary>
public class SoftwareConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "我的软件列表";
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public List<SoftwareItem> Items { get; set; } = new();
    public List<string> FavoriteIds { get; set; } = new();
}

/// <summary>
/// 软件分类枚举
/// </summary>
public static class SoftwareCategories
{
    public const string Development = "开发工具";
    public const string Browser = "浏览器";
    public const string Office = "办公软件";
    public const string Compression = "压缩工具";
    public const string Graphics = "图像处理";
    public const string Media = "音视频";
    public const string Communication = "通讯工具";
    public const string System = "系统工具";
    public const string Security = "安全软件";
    public const string Game = "游戏平台";
    public const string Other = "其他";

    public static readonly string[] All =
    {
        Development, Browser, Office, Compression, Graphics,
        Media, Communication, System, Security, Game, Other
    };
}

/// <summary>
/// 下载任务信息
/// </summary>
public class DownloadTaskInfo
{
    public string SoftwareId { get; set; } = string.Empty;
    public string SoftwareName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string SavePath { get; set; } = string.Empty;
    public long TotalBytes { get; set; }
    public long DownloadedBytes { get; set; }
    public double Progress => TotalBytes > 0 ? (double)DownloadedBytes / TotalBytes * 100 : 0;
    public DownloadTaskStatus Status { get; set; } = DownloadTaskStatus.Pending;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 下载任务状态
/// </summary>
public enum DownloadTaskStatus
{
    Pending,
    Downloading,
    Paused,
    Completed,
    Failed,
    Cancelled
}
