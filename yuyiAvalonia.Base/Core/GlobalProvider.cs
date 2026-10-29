using yuyiAvalonia.Base.ControlStyles;

namespace yuyiAvalonia.Base.Core
{
    public class GlobalProvider
    {
        public static SystemContext? SystemContext { get; set; }

        public static SystemTheme SystemTheme { get; set; } = new SystemTheme();
    }
}
