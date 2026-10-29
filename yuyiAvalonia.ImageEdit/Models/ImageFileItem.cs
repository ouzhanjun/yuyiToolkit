using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace yuyiAvalonia.ImageEdit.Models;

public partial class ImageFileItem : ObservableObject
{
    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private Bitmap? _thumbnail;

    [ObservableProperty]
    private bool _isSelected;

    public string FileSize
    {
        get
        {
            try
            {
                if (System.IO.File.Exists(FilePath))
                {
                    var fileInfo = new System.IO.FileInfo(FilePath);
                    var sizeInBytes = fileInfo.Length;
                    if (sizeInBytes < 1024)
                        return $"{sizeInBytes} B";
                    else if (sizeInBytes < 1024 * 1024)
                        return $"{Math.Round(sizeInBytes / 1024.0, 2)} KB";
                    else
                        return $"{Math.Round(sizeInBytes / (1024.0 * 1024.0), 2)} MB";
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }

    public ImageFileItem()
    {
    }

    public ImageFileItem(string filePath, Bitmap? thumbnail = null)
    {
        FilePath = filePath;
        FileName = System.IO.Path.GetFileName(filePath);
        Thumbnail = thumbnail;
    }
}
