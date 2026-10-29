using System.Globalization;
using yuyiAvalonia.Base.Localization;

namespace yuyiAvalonia.Java.Plugin;

/// <summary>
/// Java 插件本地化资源贡献者
/// </summary>
public class LocalizationResourceContributor : ILocalizaitonResourceContributor
{
    public Dictionary<string, string>? GetResource(CultureInfo culture)
    {
        if (culture.Name == "zh-Hans")
        {
            var resources = new Dictionary<string, string>();
            resources.Add("Menu.Toolbar.Java", "Java");
            resources.Add("Java.Title", "Java 工具");
            resources.Add("Java.Description", "Java 环境管理工具");
            resources.Add("Java.InstalledVersions", "已安装版本");
            resources.Add("Java.AvailableVersions", "下载安装");
            resources.Add("Java.Settings", "环境配置");
            return resources;
        }
        return null;
    }
}
