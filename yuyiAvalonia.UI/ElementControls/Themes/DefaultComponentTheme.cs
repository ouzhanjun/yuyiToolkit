using Avalonia.Media;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Themes;

/// <summary>
/// 默认主题 - 提供通用的组件样式
/// </summary>
public class DefaultComponentTheme : IComponentTheme
{
    public string Name => "默认主题";

    public string Id => "default";

    public string Description => "系统默认主题，提供通用的组件样式和状态变化效果";

    public bool IsDarkTheme => false;

    public int Priority => 0;

    private readonly Dictionary<ComponentType, ComponentTypeTheme> _componentThemes;

    public DefaultComponentTheme()
    {
        _componentThemes = InitializeThemes();
    }

    public ComponentTypeTheme? GetComponentTheme(ComponentType componentType)
    {
        return _componentThemes.TryGetValue(componentType, out var theme) ? theme : null;
    }

    public ComponentThemeStyle GetStyle(ComponentType componentType, ComponentState state)
    {
        var typeTheme = GetComponentTheme(componentType);
        return typeTheme?.GetStyle(state) ?? ComponentThemeStyle.CreateDefault();
    }

    public IEnumerable<ComponentTypeTheme> GetAllComponentThemes()
    {
        return _componentThemes.Values;
    }

    private static Dictionary<ComponentType, ComponentTypeTheme> InitializeThemes()
    {
        // 创建基础默认样式
        var defaultStyle = ComponentThemeStyle.CreateDefault();

        return new Dictionary<ComponentType, ComponentTypeTheme>
        {
            #region 文本组件

            [ComponentType.Text] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Text,
                BaseStyle = new ComponentThemeStyle
                {
                    ForeColor = "#333333",
                    FontSize = 14,
                    IsBold = false,
                    IsItalic = false,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { ForeColor = "#0078D4" },
                    [ComponentState.Disabled] = new ComponentThemeStyle { ForeColor = "#999999", Opacity = 0.6 }
                }
            },

            #endregion

            #region 按钮组件

            [ComponentType.Button] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Button,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#007BFF",
                    ForeColor = "#FFFFFF",
                    BorderColor = "#0056B3",
                    BorderWidth = 1,
                    CornerRadius = 4,
                    FontSize = 14,
                    IsBold = false,
                    HasShadow = false,
                    HasGlow = false,
                    ShadowBlur = 0,
                    ShadowColor = "#000000",
                    ShadowOffsetY = 2,
                    Padding = 16,
                    Opacity = 1.0,
                    Scale = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#0056B3",
                        BorderColor = "#004494",
                        Scale = 1.02,
                        TransitionDuration = 100
                    },
                    [ComponentState.Pressed] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#004494",
                        BorderColor = "#003366",
                        Scale = 0.98,
                        TransitionDuration = 50
                    },
                    [ComponentState.Disabled] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#CCCCCC",
                        ForeColor = "#888888",
                        BorderColor = "#BBBBBB",
                        Opacity = 0.7
                    },
                    [ComponentState.Focused] = new ComponentThemeStyle
                    {
                        BorderColor = "#80BDFF",
                        BorderWidth = 2
                    }
                }
            },

            #endregion

            #region 卡片组件

            [ComponentType.Card] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Card,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#DEE2E6",
                    BorderWidth = 1,
                    CornerRadius = 8,
                    Padding = 16,
                    HasShadow = true,
                    HasGlow = false,
                    ShadowBlur = 8,
                    ShadowColor = "#000000",
                    ShadowOffsetY = 2,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle
                    {
                        BorderColor = "#ADB5BD",
                        ShadowBlur = 12,
                        ShadowOffsetY = 4,
                        TransitionDuration = 150
                    },
                    [ComponentState.Selected] = new ComponentThemeStyle
                    {
                        BorderColor = "#007BFF",
                        BorderWidth = 2,
                        ShadowBlur = 12
                    },
                    [ComponentState.Disabled] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#F8F9FA",
                        Opacity = 0.7
                    }
                }
            },

            #endregion

            #region Panel容器

            [ComponentType.Panel] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Panel,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#E0E0E0",
                    BorderWidth = 1,
                    CornerRadius = 4,
                    Padding = 10,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#CCCCCC" },
                    [ComponentState.Disabled] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#F5F5F5",
                        Opacity = 0.7
                    }
                }
            },

            #endregion

            #region 垂直堆叠容器

            [ComponentType.VerticalStackPanel] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.VerticalStackPanel,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FAFAFA",
                    BorderColor = "#E0E0E0",
                    BorderWidth = 1,
                    CornerRadius = 4,
                    Padding = 10,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#CCCCCC" }
                }
            },

            #endregion

            #region 水平堆叠容器

            [ComponentType.HorizontalStackPanel] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.HorizontalStackPanel,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FAFAFA",
                    BorderColor = "#E0E0E0",
                    BorderWidth = 1,
                    CornerRadius = 4,
                    Padding = 10,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#CCCCCC" }
                }
            },

            #endregion

            #region 网格容器

            [ComponentType.Grid] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Grid,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#E0E0E0",
                    BorderWidth = 1,
                    CornerRadius = 4,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#CCCCCC" },
                    [ComponentState.Disabled] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#F5F5F5",
                        Opacity = 0.7
                    }
                }
            },

            #endregion

            #region 画布容器

            [ComponentType.Canvas] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Canvas,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#E0E0E0",
                    BorderWidth = 1,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#CCCCCC" }
                }
            },

            #endregion

            #region 图片组件

            [ComponentType.Image] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Image,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#F0F0F0",
                    BorderColor = "#DDDDDD",
                    BorderWidth = 1,
                    CornerRadius = 0,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle
                    {
                        BorderColor = "#007BFF",
                        Scale = 1.02
                    },
                    [ComponentState.Disabled] = new ComponentThemeStyle { Opacity = 0.5 },
                    [ComponentState.Loading] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#E8E8E8"
                    },
                    [ComponentState.Error] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#FFE4E4",
                        BorderColor = "#FFCCCC"
                    }
                }
            }

            #endregion
        };
    }
}

/// <summary>
/// 深色主题
/// </summary>
public class DarkComponentTheme : IComponentTheme
{
    private readonly DefaultComponentTheme _baseTheme = new();

    public string Name => "深色主题";
    public string Id => "dark";
    public string Description => "深色模式主题";
    public bool IsDarkTheme => true;
    public int Priority => 1;

    public ComponentTypeTheme? GetComponentTheme(ComponentType componentType)
    {
        return _baseTheme.GetComponentTheme(componentType);
    }

    public ComponentThemeStyle GetStyle(ComponentType componentType, ComponentState state)
    {
        var style = _baseTheme.GetStyle(componentType, state);

        // 颜色反转处理
        if (style.BackgroundColor == "#FFFFFF")
            style.BackgroundColor = "#2D2D30";
        else if (style.BackgroundColor == "#FAFAFA")
            style.BackgroundColor = "#3C3C3F";
        else if (style.BackgroundColor == "#F0F0F0")
            style.BackgroundColor = "#3C3C3F";
        else if (style.BackgroundColor == "#F8F9FA")
            style.BackgroundColor = "#3C3C3F";

        if (style.ForeColor == "#333333")
            style.ForeColor = "#E0E0E0";
        else if (style.ForeColor == "#666666")
            style.ForeColor = "#AAAAAA";
        else if (style.ForeColor == "#999999")
            style.ForeColor = "#666666";

        if (style.BorderColor == "#E0E0E0")
            style.BorderColor = "#555555";
        else if (style.BorderColor == "#DEE2E6")
            style.BorderColor = "#444444";
        else if (style.BorderColor == "#DDDDDD")
            style.BorderColor = "#444444";

        return style;
    }

    public IEnumerable<ComponentTypeTheme> GetAllComponentThemes()
    {
        return _baseTheme.GetAllComponentThemes();
    }
}

/// <summary>
/// 微信风格主题
/// </summary>
public class WeChatComponentTheme : IComponentTheme
{
    public string Name => "微信风格";
    public string Id => "wechat";
    public string Description => "仿微信小程序风格主题";
    public bool IsDarkTheme => false;
    public int Priority => 10;

    private readonly Dictionary<ComponentType, ComponentTypeTheme> _componentThemes;

    public WeChatComponentTheme()
    {
        _componentThemes = InitializeWeChatThemes();
    }

    public ComponentTypeTheme? GetComponentTheme(ComponentType componentType)
    {
        return _componentThemes.TryGetValue(componentType, out var theme) ? theme : null;
    }

    public ComponentThemeStyle GetStyle(ComponentType componentType, ComponentState state)
    {
        var typeTheme = GetComponentTheme(componentType);
        return typeTheme?.GetStyle(state) ?? ComponentThemeStyle.CreateDefault();
    }

    public IEnumerable<ComponentTypeTheme> GetAllComponentThemes()
    {
        return _componentThemes.Values;
    }

    private static Dictionary<ComponentType, ComponentTypeTheme> InitializeWeChatThemes()
    {
        return new Dictionary<ComponentType, ComponentTypeTheme>
        {
            [ComponentType.Button] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Button,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#07C160",
                    ForeColor = "#FFFFFF",
                    BorderColor = "#06A050",
                    BorderWidth = 1,
                    CornerRadius = 4,
                    FontSize = 16,
                    HasShadow = false,
                    HasGlow = false,
                    Padding = 12,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BackgroundColor = "#06A050" },
                    [ComponentState.Disabled] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#CCCCCC",
                        ForeColor = "#888888",
                        BorderColor = "#BBBBBB",
                        Opacity = 0.7
                    }
                }
            },

            [ComponentType.Card] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Card,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 1,
                    CornerRadius = 8,
                    Padding = 12,
                    HasShadow = true,
                    HasGlow = false,
                    ShadowBlur = 4,
                    ShadowColor = "#10000000",
                    ShadowOffsetY = 1,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#DDDDDD" }
                }
            },

            [ComponentType.Text] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Text,
                BaseStyle = new ComponentThemeStyle
                {
                    ForeColor = "#333333",
                    FontSize = 14,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { ForeColor = "#666666" },
                    [ComponentState.Disabled] = new ComponentThemeStyle { ForeColor = "#999999", Opacity = 0.6 }
                }
            },

            [ComponentType.Panel] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Panel,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 1,
                    CornerRadius = 8,
                    Padding = 12,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#DDDDDD" }
                }
            },

            [ComponentType.VerticalStackPanel] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.VerticalStackPanel,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 1,
                    CornerRadius = 8,
                    Padding = 12,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#DDDDDD" }
                }
            },

            [ComponentType.HorizontalStackPanel] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.HorizontalStackPanel,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 1,
                    CornerRadius = 8,
                    Padding = 12,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#DDDDDD" }
                }
            },

            [ComponentType.Grid] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Grid,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 1,
                    CornerRadius = 8,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#DDDDDD" }
                }
            },

            [ComponentType.Canvas] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Canvas,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#FFFFFF",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 1,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle { BorderColor = "#DDDDDD" }
                }
            },

            [ComponentType.Image] = new ComponentTypeTheme
            {
                ComponentType = ComponentType.Image,
                BaseStyle = new ComponentThemeStyle
                {
                    BackgroundColor = "#F5F5F5",
                    BorderColor = "#EEEEEE",
                    BorderWidth = 0,
                    CornerRadius = 4,
                    HasShadow = false,
                    HasGlow = false,
                    Opacity = 1.0
                },
                StateStyles = new Dictionary<ComponentState, ComponentThemeStyle>
                {
                    [ComponentState.Hover] = new ComponentThemeStyle
                    {
                        BorderColor = "#07C160",
                        Scale = 1.02
                    },
                    [ComponentState.Disabled] = new ComponentThemeStyle { Opacity = 0.5 },
                    [ComponentState.Error] = new ComponentThemeStyle
                    {
                        BackgroundColor = "#FFF0F0",
                        BorderColor = "#FFCCCC"
                    }
                }
            }
        };
    }
}
