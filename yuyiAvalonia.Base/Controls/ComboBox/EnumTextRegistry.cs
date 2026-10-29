using System.Collections.Generic;

namespace yuyiAvalonia.Base.Controls.Models
{
    /// <summary>
    /// 枚举值到显示文本的映射表，提供中英文等本地化文本。
    /// 使用方式：EnumTextRegistry.GetDisplayText(enumValue) 或 EnumTextRegistry.GetEnumItems(enumType)
    /// 
    /// 静态构造函数自动注册所有 Avalonia 内置枚举。
    /// 子项目（如 yuyiAvalonia.UI）通过 EnumTextRegistryInitializer 注册自定义枚举。
    /// </summary>
    public static partial class EnumTextRegistry
    {
        /// <summary>
        /// 映射表：EnumType.FullName -> Dictionary(EnumName, DisplayText)
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, string>> _registry = new();

        static EnumTextRegistry()
        {
            RegisterAvaloniaEnums();
        }

        private static void RegisterAvaloniaEnums()
        {
            // ===== Avalonia.Media 枚举 =====

            Register(typeof(Avalonia.Media.Stretch), new Dictionary<string, string>
            {
                { "None", "无" },
                { "Fill", "填充" },
                { "Uniform", "均匀" },
                { "UniformToFill", "均匀填充" },
            });

            Register(typeof(Avalonia.Media.StretchDirection), new Dictionary<string, string>
            {
                { "UpOnly", "仅向上" },
                { "DownOnly", "仅向下" },
                { "Both", "双向" },
            });

            Register(typeof(Avalonia.Media.TileMode), new Dictionary<string, string>
            {
                { "None", "无" },
                { "FlipX", "水平翻转" },
                { "FlipY", "垂直翻转" },
                { "FlipXY", "双向翻转" },
                { "Tile", "平铺" },
            });

            Register(typeof(Avalonia.Media.AlignmentX), new Dictionary<string, string>
            {
                { "Left", "左" },
                { "Center", "居中" },
                { "Right", "右" },
            });

            Register(typeof(Avalonia.Media.AlignmentY), new Dictionary<string, string>
            {
                { "Top", "上" },
                { "Center", "居中" },
                { "Bottom", "下" },
            });

            // ===== Avalonia.Layout 枚举 =====

            Register(typeof(Avalonia.Layout.HorizontalAlignment), new Dictionary<string, string>
            {
                { "Left", "左" },
                { "Center", "居中" },
                { "Right", "右" },
                { "Stretch", "拉伸" },
            });

            Register(typeof(Avalonia.Layout.VerticalAlignment), new Dictionary<string, string>
            {
                { "Top", "上" },
                { "Center", "居中" },
                { "Bottom", "下" },
                { "Stretch", "拉伸" },
            });

            Register(typeof(Avalonia.Media.TextAlignment), new Dictionary<string, string>
            {
                { "Left", "左对齐" },
                { "Center", "居中" },
                { "Right", "右对齐" },
                { "Justify", "两端对齐" },
            });
        }

        /// <summary>
        /// 注册一个枚举类型的映射。若已存在则覆盖。
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="mappings">字典，key 为枚举值名称，value 为显示文本</param>
        public static void Register(System.Type enumType, Dictionary<string, string> mappings)
        {
            _registry[enumType.FullName!] = mappings;
        }

        /// <summary>
        /// 获取指定枚举值的显示文本，找不到时返回原始枚举名称。
        /// </summary>
        public static string GetDisplayText<T>(T enumValue) where T : System.Enum
        {
            if (_registry.TryGetValue(enumValue.GetType().FullName!, out var dict) &&
                dict.TryGetValue(enumValue.ToString(), out var text))
            {
                return text;
            }
            return enumValue.ToString();
        }

        /// <summary>
        /// 获取指定枚举值的显示文本（通过类型名和枚举值名称查找）。
        /// </summary>
        public static string GetDisplayText(System.Type enumType, string enumName)
        {
            if (_registry.TryGetValue(enumType.FullName!, out var dict) &&
                dict.TryGetValue(enumName, out var text))
            {
                return text;
            }
            return enumName;
        }

        /// <summary>
        /// 获取指定枚举类型的所有 (EnumName, DisplayText) 对。
        /// </summary>
        public static IEnumerable<(string Name, string DisplayText)> GetItems(System.Type enumType)
        {
            var names = System.Enum.GetNames(enumType);
            foreach (var name in names)
            {
                yield return (name, GetDisplayText(enumType, name));
            }
        }

        /// <summary>
        /// 获取指定枚举类型的所有 EnumItem（含显示文本）。
        /// </summary>
        public static IEnumerable<EnumItem> GetEnumItems(System.Type enumType)
        {
            foreach (var (name, displayText) in GetItems(enumType))
            {
                yield return new EnumItem(name, name) { DisplayText = displayText };
            }
        }

        /// <summary>
        /// 判断指定枚举类型是否已在映射表中注册。
        /// </summary>
        public static bool IsRegistered(System.Type enumType)
        {
            return _registry.ContainsKey(enumType.FullName!);
        }
    }
}
