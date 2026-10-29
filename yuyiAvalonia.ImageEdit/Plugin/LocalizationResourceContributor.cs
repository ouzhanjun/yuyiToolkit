using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Localization;

namespace yuyiAvalonia.ImageEdit.Plugin
{
    public class LocalizationResourceContributor : ILocalizaitonResourceContributor
    {
        private readonly AppSettings _appSettings;

        public LocalizationResourceContributor(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        public Dictionary<string, string>? GetResource(CultureInfo culture)
        {
            var resource = JsonSerializer.Deserialize<Dictionary<string, string>>(
                File.ReadAllBytes(Path.Combine(_appSettings.PluginPath,
                "First",
                "i18n",
                $"{culture.IetfLanguageTag}.json")));
            return resource;
        }
    }
}
