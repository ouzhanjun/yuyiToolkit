using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;

namespace yuyiAvalonia.Base.Commands
{
    public class OpenFileCommand : IRelayCommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            if (CanExecute(parameter))
            {
                var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;
                // 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
                var topLevel = TopLevel.GetTopLevel(window);

                // 启动异步操作以打开对话框。
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Text File",
                    AllowMultiple = false
                });

                if (files.Count >= 1)
                {
                    // 打开第一个文件的读取流。
                    await using var stream = await files[0].OpenReadAsync();
                    using var streamReader = new StreamReader(stream);
                    // 将文件的所有内容作为文本读取。
                    var fileContent = await streamReader.ReadToEndAsync();
                }
            }
        }

        public void NotifyCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
