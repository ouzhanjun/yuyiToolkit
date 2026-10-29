using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace yuyiAvalonia.Base.Models
{
    /// <summary>
    /// 应用设置基类，所有设置项都应继承此类
    /// </summary>
    public abstract partial class AppSettingsBase : ObservableObject
    {
        /// <summary>
        /// 设置是否已修改，需要保存
        /// </summary>
        [ObservableProperty]
        private bool _isModified;

        /// <summary>
        /// 获取设置项的唯一标识
        /// </summary>
        public abstract string Id { get; }

        /// <summary>
        /// 获取设置项的显示名称
        /// </summary>
        public abstract string DisplayName { get; }
    }

    /// <summary>
    /// 通用布尔值设置项
    /// </summary>
    public partial class BooleanSettingItem : AppSettingsBase
    {
        [ObservableProperty]
        private bool _value;

        public override string Id { get; }
        public override string DisplayName { get; }

        public BooleanSettingItem(string id, string displayName, bool defaultValue = false)
        {
            Id = id;
            DisplayName = displayName;
            _value = defaultValue;
        }

        partial void OnValueChanged(bool value)
        {
            IsModified = true;
        }
    }

    /// <summary>
    /// 通用字符串值设置项
    /// </summary>
    public partial class StringSettingItem : AppSettingsBase
    {
        [ObservableProperty]
        private string _value;

        public override string Id { get; }
        public override string DisplayName { get; }

        public StringSettingItem(string id, string displayName, string defaultValue = "")
        {
            Id = id;
            DisplayName = displayName;
            _value = defaultValue;
        }

        partial void OnValueChanged(string value)
        {
            IsModified = true;
        }
    }

    /// <summary>
    /// 选项列表设置项
    /// </summary>
    public partial class OptionSettingItem : AppSettingsBase
    {
        [ObservableProperty]
        private string _value;

        public List<string> Options { get; }

        public override string Id { get; }
        public override string DisplayName { get; }

        public OptionSettingItem(string id, string displayName, string defaultValue, List<string> options)
        {
            Id = id;
            DisplayName = displayName;
            _value = defaultValue;
            Options = options;
        }

        partial void OnValueChanged(string value)
        {
            IsModified = true;
        }
    }

    /// <summary>
    /// 应用设置管理器
    /// </summary>
    public partial class AppSettings: ObservableObject
    {
        private static AppSettings? _instance;
        public static AppSettings Instance => _instance ??= new AppSettings();

        private readonly string _settingsPath;
        private Dictionary<string, object> _settings = new();

        // 预定义的设置项
        [ObservableProperty]
        private BooleanSettingItem _minimizeToTray = new("MinimizeToTray", "关闭时最小化到托盘", true);

        [ObservableProperty]
        private BooleanSettingItem _addImageToSystemMenu = new("AddImageToSystemMenu", "打开图片时添加到系统菜单", true);

        [ObservableProperty]
        private OptionSettingItem _defaultTheme = new("DefaultTheme", "默认主题", "Default", new List<string> { "Default", "Dark" });

        /// <summary>
        /// 所有设置项列表（用于UI绑定）
        /// </summary>
        public List<AppSettingsBase> AllSettings { get; } = new();

        /// <summary>
        /// 所有设置项列表（用于UI绑定）- 常规选项卡
        /// </summary>
        public List<AppSettingsBase> GeneralSettings { get; } = new();

        /// <summary>
        /// 所有设置项列表（用于UI绑定）- 外观选项卡
        /// </summary>
        public List<AppSettingsBase> AppearanceSettings { get; } = new();

        public AppSettings()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "yuyiToolkits",
                "settings.json"
            );
            
            // 注册常规设置项
            GeneralSettings.Add(MinimizeToTray);
            GeneralSettings.Add(AddImageToSystemMenu);

            // 注册外观设置项
            AppearanceSettings.Add(DefaultTheme);

            // 注册所有设置项（保持兼容）
            AllSettings.AddRange(GeneralSettings);
            AllSettings.AddRange(AppearanceSettings);

            // 加载设置
            Load();
        }

        /// <summary>
        /// 注册新的设置项
        /// </summary>
        public void RegisterSetting(AppSettingsBase setting)
        {
            if (!AllSettings.Contains(setting))
            {
                AllSettings.Add(setting);
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_settingsPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var settingsDict = new Dictionary<string, object>();
                foreach (var setting in AllSettings)
                {
                    if (setting is BooleanSettingItem boolSetting)
                    {
                        settingsDict[setting.Id] = boolSetting.Value;
                    }
                    else if (setting is StringSettingItem stringSetting)
                    {
                        settingsDict[setting.Id] = stringSetting.Value;
                    }
                    else if (setting is OptionSettingItem optionSetting)
                    {
                        settingsDict[setting.Id] = optionSetting.Value;
                    }
                }

                var json = JsonSerializer.Serialize(settingsDict, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_settingsPath, json);
            }
            catch
            {
                // 忽略保存失败
            }
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        public void Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                    return;

                var json = File.ReadAllText(_settingsPath);
                var settingsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (settingsDict == null)
                    return;

                foreach (var setting in AllSettings)
                {
                    if (settingsDict.TryGetValue(setting.Id, out var value))
                    {
                        if (setting is BooleanSettingItem boolSetting)
                        {
                            if (value is System.Text.Json.JsonElement element)
                            {
                                boolSetting.Value = element.GetBoolean();
                            }
                            else if (value is bool boolValue)
                            {
                                boolSetting.Value = boolValue;
                            }
                        }
                        else if (setting is StringSettingItem stringSetting)
                        {
                            if (value is System.Text.Json.JsonElement element)
                            {
                                stringSetting.Value = element.GetString() ?? stringSetting.Value;
                            }
                            else if (value is string strValue)
                            {
                                stringSetting.Value = strValue;
                            }
                        }
                        else if (setting is OptionSettingItem optionSetting)
                        {
                            if (value is System.Text.Json.JsonElement element)
                            {
                                optionSetting.Value = element.GetString() ?? optionSetting.Value;
                            }
                            else if (value is string strValue)
                            {
                                optionSetting.Value = strValue;
                            }
                        }
                    }
                }
            }
            catch
            {
                // 忽略加载失败
            }
        }
    }
}
