using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.Plugin;

namespace yuyiAvalonia.Base.Services;

public static class ServiceCollectionExtensions
{
    private static LocalizationService _localization = LocalizationService.Instance;

    private static Dictionary<string, INavigationCommand> _commands = new Dictionary<string, INavigationCommand>();

    private static SystemContext _systemContext;

    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<Dictionary<string, INavigationCommand>>(_commands);

        services.AddTransient<DefaultViewNavigator>();

        services.AddSettings();

        services.AddLocalization();

        services.AddMenus();

        services.AddSingleton<SystemContext>();

        services.AddPlugins();

        return services;
    }


    private static IServiceCollection AddSettings(this IServiceCollection services)
    {
        var appSettings = Options.Create(new AppSettings
        {
            OutputPath = Path.Combine(AppContext.BaseDirectory, "Output"),
            PluginPath = Path.Combine(AppContext.BaseDirectory, "Plugins")
        });

        services.AddSingleton(appSettings);
        return services;
    }

    public static IServiceCollection AddCommand(this IServiceCollection services, INavigationCommand command)
    {
        if(command != null)
        {
            _commands[command.Key] = command;
        }
        return services;
    }

    public static IServiceCollection RemoveCommand(this IServiceCollection services, string key)
    {
        Debug.Assert(string.IsNullOrEmpty(key));
        if (_commands.ContainsKey(key))
        {
            _commands.Remove(key);
        }
        return services;
    }

    private static IServiceCollection AddLocalization(this IServiceCollection services)
    {
        services.AddSingleton<LocalizationService>(_localization);
        return services;
    }

    private static IServiceCollection AddMenus(this IServiceCollection services)
    {
        // 从配置文件读取菜单注入到 DI 容器
        string jsonFile = Path.Combine(AppContext.BaseDirectory, "Configs", "menus.json");
        var menuItems = JsonSerializer.Deserialize<List<CustomMenuItem>>(File.ReadAllBytes(jsonFile));
        services.AddSingleton(Options.Create(menuItems!));
        return services;
    }

    /// <summary>
    /// 注册插件服务
    /// </summary>
    /// <param name= "services" ></param>
    /// <returns ></returns>
    private static IServiceCollection AddPlugins(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var appSettings = serviceProvider?.GetService<IOptions<AppSettings>>();
        GlobalProvider.SystemContext = serviceProvider?.GetService<SystemContext>()!;
        var pluginPath = appSettings?.Value.PluginPath;

        if (!Directory.Exists(pluginPath))
        {
            return services;
        }

        var files = Directory.GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                var assembly = System.Reflection.Assembly.LoadFrom(file);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(PluginBase).IsAssignableFrom(t) && !t.IsAbstract)
                    .Select(t => (IPlugin)Activator.CreateInstance(t, appSettings)!);

                foreach (var plugin in pluginTypes)
                {
                    plugin.RegisterServices(services);
                    plugin.ConfigureMenu(GlobalProvider.SystemContext);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading plugin from {file}: {ex.Message}");
            }
        }

        return services;
    }
}