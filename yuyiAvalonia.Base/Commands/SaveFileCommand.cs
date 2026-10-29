using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;

namespace yuyiAvalonia.Base.Commands
{
    public class SaveFileCommand : IRelayCommand
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
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Text File"
                });

                if (file is not null)
                {
                    // 打开文件的写入流。
                    await using var stream = await file.OpenWriteAsync();
                    using var streamWriter = new StreamWriter(stream);
                    // 将一些内容写入文件。
                    await streamWriter.WriteLineAsync("Hello World!");
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
