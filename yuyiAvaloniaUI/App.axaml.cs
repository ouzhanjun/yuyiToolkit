using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using yuyiAvalonia.Base.Models;
using yuyiAvalonia.Base.Services;
using yuyiAvalonia.UI.Models;
using yuyiAvaloniaUI.ViewModels;
using yuyiAvaloniaUI.Views;

namespace yuyiAvaloniaUI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ApplyDefaultTheme();

            var services = new ServiceCollection();

            services.AddServices();
            services.AddTransient<MainWindowViewModel>();

            var provider = services.BuildServiceProvider();
            var vm = provider.GetRequiredService<MainWindowViewModel>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                var mainWindow = new MainWindow
                {
                    DataContext = vm
                };

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ApplyDefaultTheme()
        {
            var settings = AppSettings.Instance;
            var theme = settings.DefaultTheme?.Value ?? "Default";

            RequestedThemeVariant = theme switch
            {
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}
