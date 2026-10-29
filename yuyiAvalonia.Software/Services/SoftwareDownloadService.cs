using System.IO;
using System.Net.Http;
using System.Text.Json;
using yuyiAvalonia.Software.Models;

namespace yuyiAvalonia.Software.Services;

/// <summary>
/// 软件下载服务 - 负责软件的下载和管理
/// </summary>
public class SoftwareDownloadService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _downloadFolder;
    private CancellationTokenSource? _currentDownloadCts;

    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;
    public event EventHandler<DownloadCompletedEventArgs>? DownloadCompleted;

    public SoftwareDownloadService()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(30);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

        _downloadFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads",
            "Software");

        if (!Directory.Exists(_downloadFolder))
            Directory.CreateDirectory(_downloadFolder);
    }

    /// <summary>
    /// 获取下载保存路径
    /// </summary>
    public string GetDownloadPath(string softwareName, string version, string extension = ".exe")
    {
        var fileName = $"{softwareName}_{version}{extension}";
        var invalidChars = Path.GetInvalidFileNameChars();
        fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return Path.Combine(_downloadFolder, fileName);
    }

    /// <summary>
    /// 下载软件
    /// </summary>
    public async Task<bool> DownloadSoftwareAsync(SoftwareItem software, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(software.DownloadUrl))
            return false;

        try
        {
            _currentDownloadCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            using var response = await _httpClient.GetAsync(software.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, _currentDownloadCts.Token);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var extension = GetFileExtension(response.Content.Headers.ContentType?.MediaType);
            var savePath = GetDownloadPath(software.Name, software.Version, extension);

            using var contentStream = await response.Content.ReadAsStreamAsync(_currentDownloadCts.Token);
            using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long bytesRead = 0;
            int read;

            while ((read = await contentStream.ReadAsync(buffer, _currentDownloadCts.Token)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), _currentDownloadCts.Token);
                bytesRead += read;

                if (totalBytes > 0)
                {
                    var percentage = (double)bytesRead / totalBytes * 100;
                    progress?.Report(percentage);
                    DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs(percentage, bytesRead, totalBytes));
                }
            }

            software.LocalPath = savePath;
            software.Status = SoftwareStatus.Downloaded;
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(true, savePath));
            return true;
        }
        catch (OperationCanceledException)
        {
            software.Status = SoftwareStatus.NotDownloaded;
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(false, null, "下载已取消"));
            return false;
        }
        catch (Exception ex)
        {
            software.Status = SoftwareStatus.Error;
            DownloadCompleted?.Invoke(this, new DownloadCompletedEventArgs(false, null, ex.Message));
            return false;
        }
        finally
        {
            _currentDownloadCts?.Dispose();
            _currentDownloadCts = null;
        }
    }

    /// <summary>
    /// 取消当前下载
    /// </summary>
    public void CancelDownload()
    {
        _currentDownloadCts?.Cancel();
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    public async Task<long> GetFileSizeAsync(string url)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentLength ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// 检查软件是否已下载
    /// </summary>
    public bool IsDownloaded(SoftwareItem software)
    {
        if (string.IsNullOrEmpty(software.LocalPath))
            return false;

        return File.Exists(software.LocalPath);
    }

    /// <summary>
    /// 打开下载文件夹
    /// </summary>
    public void OpenDownloadFolder()
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = _downloadFolder,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// 打开文件位置
    /// </summary>
    public void OpenFileLocation(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{filePath}\"",
            UseShellExecute = true
        });
    }

    /// <summary>
    /// 运行已下载的软件
    /// </summary>
    public void RunSoftware(SoftwareItem software)
    {
        if (!IsDownloaded(software))
            return;

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = software.LocalPath,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    private string GetFileExtension(string? mediaType)
    {
        return mediaType?.ToLower() switch
        {
            "application/x-msdownload" or "application/octet-stream" => ".exe",
            "application/zip" => ".zip",
            "application/x-zip-compressed" => ".zip",
            "application/x-rar-compressed" => ".rar",
            "application/vnd.microsoft.portable-executable" => ".exe",
            "application/x-internet-explorer" => ".exe",
            _ => ".exe"
        };
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _currentDownloadCts?.Dispose();
    }
}

/// <summary>
/// 下载进度事件参数
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    public double Progress { get; }
    public long BytesDownloaded { get; }
    public long TotalBytes { get; }

    public DownloadProgressEventArgs(double progress, long bytesDownloaded, long totalBytes)
    {
        Progress = progress;
        BytesDownloaded = bytesDownloaded;
        TotalBytes = totalBytes;
    }
}

/// <summary>
/// 下载完成事件参数
/// </summary>
public class DownloadCompletedEventArgs : EventArgs
{
    public bool Success { get; }
    public string? FilePath { get; }
    public string? ErrorMessage { get; }

    public DownloadCompletedEventArgs(bool success, string? filePath, string? errorMessage = null)
    {
        Success = success;
        FilePath = filePath;
        ErrorMessage = errorMessage;
    }
}
