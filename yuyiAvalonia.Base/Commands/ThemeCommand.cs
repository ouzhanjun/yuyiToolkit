using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using yuyiAvalonia.Base.Core;

namespace yuyiAvalonia.Base.Commands
{
    public class ThemeCommand : IRelayCommand
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
                if (Application.Current is null)
                {
                    return;
                }

                ThemeVariant? theme = parameter switch
                {
                    nameof(ThemeVariant.Default) => ThemeVariant.Default,
                    nameof(ThemeVariant.Dark) => ThemeVariant.Dark,
                    nameof(ThemeVariant.Light) => ThemeVariant.Light,
                    _ => ThemeVariant.Default
                };

                Application.Current.RequestedThemeVariant = theme;
                GlobalProvider.SystemTheme.ChangeTheme(theme);
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
