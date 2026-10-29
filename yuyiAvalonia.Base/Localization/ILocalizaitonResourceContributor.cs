using System.Globalization;

namespace yuyiAvalonia.Base.Localization
{
    public interface ILocalizaitonResourceContributor
    {
        Dictionary<string, string>? GetResource(CultureInfo culture);
    }
}
