using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Localization;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.Plugin;
using yuyiAvalonia.Base.Services;
using yuyiAvalonia.UI.Commands;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Plugin;

public class UIEditPlugin : PluginBase
{
    public override string PluginName => "UIEdit";

    public UIEditPlugin(IOptions<AppSettings> appSettings) : base(appSettings)
    {
        // 确保 UI 层自定义枚举的文本映射已注册
        EnumTextRegistryInitializer.EnsureRegistered();
    }

    public override void ConfigureMenu(SystemContext context)
    {
        var item = context.MenuItems.FirstOrDefault(x => x.Key == "UIEdit");
        if (item != null) return;

        List<CustomMenuItem> items = new List<CustomMenuItem>();

        CustomMenuItem mainItem = new CustomMenuItem
        {
            Key = "UIEdit",
            LocaleKey = "Menu.Toolbar.UIEdit",
            Group = "Toolbar",
            CommandType = nameof(MenuClickCommandType.Active),
            CommandParameter = "UIEdit",
            Icon = "M507.164444 116.053333l342.945186 197.973334v395.946666L507.164444 907.946667 164.124444 709.973333V314.026667l343.04-197.973334m0-59.164444a42.097778 42.097778 0 0 0-20.954074 5.594074L128.18963 269.084444a41.908148 41.908148 0 0 0-20.954074 36.314075v413.202962a41.908148 41.908148 0 0 0 20.954074 36.314075l357.925926 206.601481a42.097778 42.097778 0 0 0 42.002963 0l357.925925-206.601481a41.908148 41.908148 0 0 0 20.954075-36.314075V305.493333a42.287407 42.287407 0 0 0-20.954075-36.408889L528.118519 62.482963A42.097778 42.097778 0 0 0 507.164444 56.888889zM420.314074 711.111111a142.222222 142.222222 0 0 1-142.222222-142.222222V341.333333a28.444444 28.444444 0 0 1 56.888889 0v227.555556a85.333333 85.333333 0 0 0 170.666666 0V341.333333a28.444444 28.444444 0 0 1 56.888889 0v227.555556a142.222222 142.222222 0 0 1-142.222222 142.222222zM679.253333 313.742222a28.444444 28.444444 0 0 0-28.444444 28.444445V682.666667a28.444444 28.444444 0 0 0 28.444444 28.444444 28.444444 28.444444 0 0 0 28.444445-28.444444V342.186667a28.444444 28.444444 0 0 0-28.444445-28.444445z"
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
            _ => throw new Exception("插件方法不存在")
        };
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IPlugin, UIEditPlugin>();
        services.AddCommand(new UIEditCommand(GlobalProvider.SystemContext));
        services.AddSingleton<ILocalizaitonResourceContributor, LocalizationResourceContributor>();
    }
}
