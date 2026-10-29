using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Localization;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.Plugin;
using yuyiAvalonia.Base.Services;
using yuyiAvalonia.ColorSelector.Commands;
using yuyiAvalonia.ColorSelector.ViewModels;

namespace yuyiAvalonia.ColorSelector.Plugin;

/// <summary>
/// ColorSelector 插件 - 颜色主题管理
/// </summary>
public class ColorSelectorPlugin : PluginBase
{
    public override string PluginName => "ColorSelector";

    public ColorSelectorPlugin(IOptions<AppSettings> appSettings) : base(appSettings)
    {
        // 1. 获取当前应用
        var app = Application.Current;
        if (app == null) return;

        // 2. 加载样式
        var styleInclude = new StyleInclude(new Uri("avares://yuyiAvalonia.ColorSelector/Assets/Styles.axaml"))
        {
            Source = new Uri("avares://yuyiAvalonia.ColorSelector/Assets/Styles.axaml")
        };
        // 3. 加到全局样式集合
        if (!app.Styles.Contains(styleInclude))
        {
            app.Styles.Add(styleInclude);
        }
    }

    public override void ConfigureMenu(SystemContext context)
    {
        var item = context.MenuItems.FirstOrDefault(x => x.Key == "ColorSelector");
        if (item != null)
        {
            return;
        }

        List<CustomMenuItem> items = new List<CustomMenuItem>();

        // 主菜单项 - 颜色主题选择器
        CustomMenuItem mainItem = new CustomMenuItem
        {
            Key = "ColorSelector",
            LocaleKey = "Menu.Toolbar.ColorSelector",
            Group = "Toolbar",
            CommandType = nameof(MenuClickCommandType.Active),
            CommandParameter = "ColorSelector",
            Icon = "M672 752a331.2 331.2 0 0 1-166.4 44.8h-70.4a83.2 83.2 0 0 1-83.2-84.8 62.4 62.4 0 0 0-62.4-62.4 99.2 99.2 0 0 1-99.2-99.2A321.6 321.6 0 0 1 512 228.8h91.2A235.2 235.2 0 0 1 836.8 464a331.2 331.2 0 0 1-80 216 16 16 0 0 0 24 20.8A363.2 363.2 0 0 0 868.8 464a267.2 267.2 0 0 0-265.6-267.2H512A353.6 353.6 0 0 0 158.4 550.4a131.2 131.2 0 0 0 131.2 131.2 30.4 30.4 0 0 1 30.4 30.4 115.2 115.2 0 0 0 115.2 115.2h70.4a363.2 363.2 0 0 0 182.4-48 16 16 0 1 0-16-27.2zM252.8 526.4m-41.6 0a41.6 41.6 0 1 0 83.2 0 41.6 41.6 0 1 0-83.2 0ZM324.8 416m-48 0a48 48 0 1 0 96 0 48 48 0 1 0-96 0ZM475.2 398.4a62.4 62.4 0 1 0-6.4 0zM644.8 331.2m-68.8 0a68.8 68.8 0 1 0 137.6 0 68.8 68.8 0 1 0-137.6 0ZM393.6 716.8a62.4 62.4 0 1 0 62.4-62.4 62.4 62.4 0 0 0-62.4 62.4z m62.4-30.4a30.4 30.4 0 1 1-30.4 30.4 30.4 30.4 0 0 1 28.8-30.4zM584 550.4l-16-41.6-6.4-8-56-40a25.6 25.6 0 0 0-36.8 33.6l30.4 52.8 6.4 6.4 35.2 20.8 296 296a24 24 0 0 0 33.6-33.6z"
        };
        items.Add(mainItem);

        context.AddMenus(items);
    }

    public override R? Execute<T, R>(string methodName, T? parameters)
        where T : default
        where R : class
    {
        return methodName switch
        {
            "GetTheme" => GetCurrentTheme() as R,
            "SetTheme" => SetTheme(parameters as string) as R,
            "ExportTheme" => ExportTheme(parameters as string) as R,
            _ => throw new Exception("插件方法不存在")
        };
    }

    private PluginResult GetCurrentTheme()
    {
        return new PluginResult(true);
    }

    private PluginResult SetTheme(string? themeId)
    {
        return new PluginResult(true);
    }

    private PluginResult ExportTheme(string? format)
    {
        return new PluginResult(true);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IPlugin, ColorSelectorPlugin>();
        services.AddTransient<ColorSelectorViewModel>();
        services.AddCommand(new ColorSelectorCommand(GlobalProvider.SystemContext));
        services.AddSingleton<ILocalizaitonResourceContributor, LocalizationResourceContributor>();
    }
}
