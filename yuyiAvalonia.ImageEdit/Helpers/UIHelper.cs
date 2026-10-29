using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

namespace yuyiAvalonia.ImageEdit.Helpers
{
    public class UIHelper
    {
        public static string ShowSaveFilePicker(string filePath)
        {
            string _currentFilePath = string.Empty;

            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;
            var topLevel = TopLevel.GetTopLevel(window);
            if (topLevel == null)
            {
                return _currentFilePath;
            }

            // 否则弹出保存对话框
            var saveFileType = new FilePickerFileType("图片文件")
            {
                MimeTypes = new List<string> { ".png", ".jpg", ".jpeg", ".bmp" }
            };

            FileInfo fileInfo = new FileInfo(filePath);

            var saveOptions = new FilePickerSaveOptions
            {
                Title = "保存图片",
                FileTypeChoices = new List<FilePickerFileType> { saveFileType },
                SuggestedFileName = fileInfo.Name,
                DefaultExtension = fileInfo.Extension,
                SuggestedFileType = new FilePickerFileType(fileInfo.Extension)
            };

            var storage = topLevel.StorageProvider.SaveFilePickerAsync(saveOptions).ConfigureAwait(true);
            var file = storage.GetAwaiter().GetResult();
            if (file != null)
            {
                _currentFilePath = file.Path.LocalPath;
            }

            return _currentFilePath;
        }

        public static async Task PrintImage(Bitmap ImageSource)
        {
            if (ImageSource == null)
            {
                return;
            }

            try
            {
                // 创建临时文件用于打印
                var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"print_temp_{Guid.NewGuid()}.png");

                // 保存图片到临时文件
                using (var stream = System.IO.File.Create(tempPath))
                {
                    ImageSource.Save(stream);
                }

                // 调用系统打印
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    Verb = "print",
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);

                // 延迟后删除临时文件（给系统时间启动打印）
                await Task.Delay(3000);
                try { System.IO.File.Delete(tempPath); } catch { /* 忽略删除失败 */ }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打印失败: {ex.Message}");
            }
        }
    }
}
