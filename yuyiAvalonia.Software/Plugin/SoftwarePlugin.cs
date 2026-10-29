using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Localization;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.Plugin;
using yuyiAvalonia.Base.Services;
using yuyiAvalonia.Software.Commands;
using yuyiAvalonia.Software.ViewModels;

namespace yuyiAvalonia.Software.Plugin;

/// <summary>
/// Software 插件 - 软件下载管理
/// </summary>
public class SoftwarePlugin : PluginBase
{
    public override string PluginName => "Software";

    public SoftwarePlugin(IOptions<AppSettings> appSettings) : base(appSettings)
    {
    }

    public override void ConfigureMenu(SystemContext context)
    {
        var item = context.MenuItems.FirstOrDefault(x => x.Key == "Software");
        if (item != null)
        {
            return;
        }

        List<CustomMenuItem> items = new List<CustomMenuItem>();

        // 主菜单项 - 软件下载器
        CustomMenuItem mainItem = new CustomMenuItem
        {
            Key = "Software",
            LocaleKey = "Menu.Toolbar.Software",
            Group = "Toolbar",
            CommandType = nameof(MenuClickCommandType.Active),
            CommandParameter = "Software",
            Icon = "M994.816 473.7024c-6.8608-95.232-44.032-185.7536-95.1296-248.4224-16.1792-20.8896-44.032-50.9952-67.2768-67.2768-20.8896-13.9264-44.1344-20.8896-69.632-20.8896-6.9632 0-11.5712 2.3552-18.5344 2.3552-60.3136 9.3184-104.448 60.416-104.448 120.7296 0 4.608 2.3552 11.5712 2.3552 18.5344 6.9632 34.816 25.6 62.6688 51.0976 83.5584 46.4896 37.0688 236.8512 155.5456 266.9568 310.9888 0 2.3552 2.3552 6.9632 2.3552 9.3184 6.9632-13.9264 34.816-85.8112 34.6112-176.4352C997.1712 494.592 994.816 485.2736 994.816 473.7024zM245.248 397.1072c13.9264 6.9632 27.8528 9.3184 44.1344 9.3184 4.608 0 11.5712-2.3552 18.5344-2.3552 34.816-4.7104 62.6688-25.6 83.5584-51.0976 4.608-4.608 9.3184-11.5712 13.9264-18.5344C451.7888 267.0592 563.3024 104.6528 709.632 81.3056c-13.9264-6.9632-83.5584-32.4608-169.472-32.4608l-2.3552 0c-11.6736 0-23.2448 0-37.1712 2.3552-95.232 6.9632-183.3984 46.3872-246.0672 97.5872-18.6368 16.1792-51.0976 44.032-65.024 67.2768-13.9264 18.5344-20.8896 41.7792-20.8896 67.2768 0 6.9632 0 13.9264 2.3552 20.8896C177.9712 348.3648 208.0768 380.8256 245.248 397.1072zM428.6464 759.1936c0-6.9632 0-13.9264-2.3552-20.8896-6.9632-34.816-25.6-60.3136-51.0976-81.2032-48.7424-37.1712-243.712-153.1904-266.8544-311.0912-7.0656 14.0288-32.5632 81.3056-32.5632 164.864 0 13.9264 0 25.4976 2.3552 39.424 6.9632 95.1296 46.3872 181.0432 97.4848 243.712 16.1792 20.8896 44.032 51.0976 67.2768 65.024 18.6368 13.9264 41.7792 20.8896 65.024 20.8896 6.9632 0 13.9264 0 20.8896-2.3552C386.8672 868.352 428.6464 817.2544 428.6464 759.1936zM783.6672 615.2192c-4.608 0-11.5712 2.3552-18.5344 2.3552-25.4976 2.3552-48.7424 16.2816-67.2768 32.5632l-13.9264 13.9264c-27.8528 34.816-99.7376 148.5824-194.9696 220.3648-39.5264 27.8528-81.3056 48.7424-127.6928 55.7056 13.824 6.9632 88.1664 34.816 178.688 34.816 9.3184 0 20.8896-2.3552 30.208-2.3552 97.4848-6.9632 185.6512-44.032 250.6752-95.1296 20.8896-16.1792 50.9952-44.032 67.2768-67.2768 13.9264-20.8896 20.8896-44.1344 20.8896-69.632l0-2.3552c0-6.9632-2.3552-11.5712-2.3552-18.5344C895.0784 659.3536 844.0832 615.2192 783.6672 615.2192z"
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
            "GetSoftwareList" => GetSoftwareList() as R,
            "DownloadSoftware" => DownloadSoftware(parameters as string) as R,
            _ => throw new Exception("插件方法不存在")
        };
    }

    private PluginResult GetSoftwareList()
    {
        return new PluginResult(true);
    }

    private PluginResult DownloadSoftware(string? softwareId)
    {
        return new PluginResult(true);
    }

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IPlugin, SoftwarePlugin>();
        services.AddTransient<SoftwareViewModel>();
        services.AddCommand(new SoftwareCommand(GlobalProvider.SystemContext));
        services.AddSingleton<ILocalizaitonResourceContributor, LocalizationResourceContributor>();
    }
}