using System.Collections.Generic;

namespace yuyiAvalonia.Base.Models.Menu;

public class CustomMenuItem
{
    public string Key { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string LocaleKey { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string GroupLocaleKey { get; set; } = string.Empty;

    public string? Group { get; set; }

    public List<CustomMenuItem> Items { get; set; } = new List<CustomMenuItem>();

    public string? CommandType { get; set; }
    
    public string? CommandParameter { get; set; }
}