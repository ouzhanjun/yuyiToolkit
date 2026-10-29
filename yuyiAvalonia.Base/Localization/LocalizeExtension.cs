using Avalonia.Markup.Xaml;
using System.Globalization;

// 对应你的 Ke.Bee.Localization.Extensions 命名空间
namespace yuyiAvalonia.Base.Localization
{
    /// <summary>
    /// 自定义本地化标记扩展（对应 XAML 中的 i18n:Localize）
    /// </summary>
    public class LocalizeExtension : MarkupExtension
    {
        // 资源键（必选）
        public string Key { get; set; } = string.Empty;

        // 构造函数：接收 XAML 中传入的资源键（如 AppName）
        public LocalizeExtension(string key)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key), "本地化资源键不能为空");
        }

        // 模拟多语言资源库（实际项目中可替换为读取 resx/json 文件）
        private static readonly Dictionary<string, Dictionary<string, string>> _languageResources;

        // 当前应用语言（默认使用系统语言，可手动切换）
        public static string CurrentCulture { get; set; } = CultureInfo.CurrentUICulture.Name;

        /// <summary>
        /// 标记扩展核心方法：返回资源键对应的本地化文本
        /// </summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // 容错：如果当前语言不存在，回退到中文
            if (!_languageResources.ContainsKey(CurrentCulture))
            {
                CurrentCulture = "zh-CN";
            }

            // 容错：如果资源键不存在，返回键本身（避免界面空白）
            if (_languageResources[CurrentCulture].TryGetValue(Key, out var text))
            {
                return text;
            }
            else
            {
                return $"[{Key}]"; // 未找到资源时显示 [资源键]，便于调试
            }
        }

        /// <summary>
        /// 切换语言（静态方法，全局调用）
        /// </summary>
        /// <param name="cultureName">语言代码（如 zh-CN、en-US）</param>
        public static void SwitchLanguage(string cultureName)
        {
            if (_languageResources.ContainsKey(cultureName))
            {
                CurrentCulture = cultureName;
            }
        }

        public string this[string key]
        {
            get
            {
                Key = key;
                return ProvideValue(null)?.ToString() ?? string.Empty;
            }
        }
    }
}