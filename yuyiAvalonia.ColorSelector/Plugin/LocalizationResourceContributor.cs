using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Localization;

namespace yuyiAvalonia.ColorSelector.Plugin;

/// <summary>
/// 国际化资源提供者
/// </summary>
public class LocalizationResourceContributor : ILocalizaitonResourceContributor
{
    private readonly AppSettings _appSettings;

    public LocalizationResourceContributor(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;
    }

    public Dictionary<string, string>? GetResource(CultureInfo culture)
    {
        try
        {
            var resourcePath = System.IO.Path.Combine(
                _appSettings.PluginPath,
                "ColorSelector",
                "i18n",
                $"{culture.IetfLanguageTag}.json");

            if (System.IO.File.Exists(resourcePath))
            {
                var json = System.IO.File.ReadAllText(resourcePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }

            // 回退到英文
            var fallbackPath = System.IO.Path.Combine(
                _appSettings.PluginPath,
                "ColorSelector",
                "i18n",
                "en.json");

            if (System.IO.File.Exists(fallbackPath))
            {
                var json = System.IO.File.ReadAllText(fallbackPath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
        }
        catch
        {
            // 忽略错误
        }

        return null;
    }
}
