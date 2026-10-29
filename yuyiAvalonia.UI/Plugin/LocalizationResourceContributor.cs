using System.Globalization;
using System.IO;
using System.Text.Json;
using yuyiAvalonia.Base.Localization;

namespace yuyiAvalonia.UI.Plugin
{
    public class LocalizationResourceContributor : ILocalizaitonResourceContributor
    {
        public Dictionary<string, string>? GetResource(CultureInfo culture)
        {
            var filePath = Path.Combine("i18n", $"{culture.Name}.json");
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            return null;
        }
    }
}