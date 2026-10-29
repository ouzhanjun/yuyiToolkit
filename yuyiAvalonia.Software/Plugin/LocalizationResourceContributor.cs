using yuyiAvalonia.Base.Localization;
using System.Globalization;
using System.Collections.Generic;

namespace yuyiAvalonia.Software.Plugin;

/// <summary>
/// 本地化资源贡献者
/// </summary>
public class LocalizationResourceContributor : ILocalizaitonResourceContributor
{
    public Dictionary<string, string>? GetResource(CultureInfo culture)
    {
        return null;
    }
}