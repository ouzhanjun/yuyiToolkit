using System.Collections.ObjectModel;

namespace yuyiAvalonia.UI.ElementControls.Themes;

/// <summary>
/// 主题提供器 - 管理所有可用的主题
/// </summary>
public class ThemeProvider
{
    private static ThemeProvider? _instance;
    private static readonly object _lock = new();

    /// <summary>
    /// 单例实例
    /// </summary>
    public static ThemeProvider Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ThemeProvider();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 所有已注册的主题
    /// </summary>
    public ReadOnlyCollection<IComponentTheme> Themes => _themes.AsReadOnly();

    private readonly List<IComponentTheme> _themes = new();

    /// <summary>
    /// 当前激活的主题
    /// </summary>
    private IComponentTheme? _currentTheme;

    public IComponentTheme CurrentTheme
    {
        get => _currentTheme ?? GetDefaultTheme();
        set
        {
            if (_currentTheme != value)
            {
                var oldTheme = _currentTheme;
                _currentTheme = value;
                OnThemeChanged(oldTheme, value);
            }
        }
    }

    /// <summary>
    /// 主题变更事件
    /// </summary>
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    private ThemeProvider()
    {
        RegisterDefaultThemes();
    }

    /// <summary>
    /// 注册默认主题
    /// </summary>
    private void RegisterDefaultThemes()
    {
        // 按优先级注册主题
        RegisterTheme(new DefaultComponentTheme());
        RegisterTheme(new DarkComponentTheme());
        RegisterTheme(new WeChatComponentTheme());
    }

    /// <summary>
    /// 注册主题
    /// </summary>
    public void RegisterTheme(IComponentTheme theme)
    {
        if (theme == null) throw new ArgumentNullException(nameof(theme));

        var existing = _themes.FirstOrDefault(t => t.Id == theme.Id);
        if (existing != null)
        {
            _themes.Remove(existing);
        }

        // 按优先级插入
        var index = _themes.BinarySearch(theme, Comparer<IComponentTheme>.Create((a, b) => a.Priority.CompareTo(b.Priority)));
        if (index < 0) index = ~index;
        _themes.Insert(index, theme);

        // 如果没有设置当前主题，自动设置
        if (_currentTheme == null)
        {
            _currentTheme = theme;
        }
    }

    /// <summary>
    /// 注销主题
    /// </summary>
    public bool UnregisterTheme(string themeId)
    {
        var theme = _themes.FirstOrDefault(t => t.Id == themeId);
        if (theme != null)
        {
            _themes.Remove(theme);
            if (_currentTheme?.Id == themeId)
            {
                _currentTheme = GetDefaultTheme();
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 根据ID获取主题
    /// </summary>
    public IComponentTheme? GetTheme(string themeId)
    {
        return _themes.FirstOrDefault(t => t.Id == themeId);
    }

    /// <summary>
    /// 获取默认主题（优先级最低的）
    /// </summary>
    public IComponentTheme GetDefaultTheme()
    {
        return _themes.FirstOrDefault() ?? new DefaultComponentTheme();
    }

    /// <summary>
    /// 获取所有深色主题
    /// </summary>
    public IEnumerable<IComponentTheme> GetDarkThemes()
    {
        return _themes.Where(t => t.IsDarkTheme);
    }

    /// <summary>
    /// 获取所有浅色主题
    /// </summary>
    public IEnumerable<IComponentTheme> GetLightThemes()
    {
        return _themes.Where(t => !t.IsDarkTheme);
    }

    /// <summary>
    /// 获取组件在当前主题和指定状态下的样式
    /// </summary>
    public ComponentThemeStyle GetStyle(Models.ComponentType componentType, ComponentState state)
    {
        return CurrentTheme.GetStyle(componentType, state);
    }

    /// <summary>
    /// 获取组件的基础样式（Normal状态）
    /// </summary>
    public ComponentThemeStyle GetBaseStyle(Models.ComponentType componentType)
    {
        return CurrentTheme.GetStyle(componentType, ComponentState.Normal);
    }

    /// <summary>
    /// 切换到指定主题
    /// </summary>
    public void SwitchTo(string themeId)
    {
        var theme = GetTheme(themeId);
        if (theme != null)
        {
            CurrentTheme = theme;
        }
    }

    /// <summary>
    /// 切换到深色/浅色模式
    /// </summary>
    public void SwitchToDarkMode(bool isDark)
    {
        var theme = isDark
            ? _themes.FirstOrDefault(t => t.IsDarkTheme && t.Id != "default")
            : _themes.FirstOrDefault(t => !t.IsDarkTheme);
        if (theme != null)
        {
            CurrentTheme = theme;
        }
    }

    /// <summary>
    /// 通知主题已变更
    /// </summary>
    protected virtual void OnThemeChanged(IComponentTheme? oldTheme, IComponentTheme? newTheme)
    {
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(oldTheme, newTheme));
    }
}

/// <summary>
/// 主题变更事件参数
/// </summary>
public class ThemeChangedEventArgs : EventArgs
{
    /// <summary>
    /// 旧主题
    /// </summary>
    public IComponentTheme? OldTheme { get; }

    /// <summary>
    /// 新主题
    /// </summary>
    public IComponentTheme? NewTheme { get; }

    public ThemeChangedEventArgs(IComponentTheme? oldTheme, IComponentTheme? newTheme)
    {
        OldTheme = oldTheme;
        NewTheme = newTheme;
    }
}
