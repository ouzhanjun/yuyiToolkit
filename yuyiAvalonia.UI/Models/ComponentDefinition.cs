using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Themes;

namespace yuyiAvalonia.UI.Models;

/// <summary>
/// 组件定义 - 描述可添加到画布的组件模板
/// </summary>
public partial class ComponentDefinition : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ComponentType _type;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _category = "基础";

    [ObservableProperty]
    private ComponentDefaults? _defaults;

    /// <summary>
    /// 关联的主题ID（可选）
    /// </summary>
    [ObservableProperty]
    private string? _themeId;

    /// <summary>
    /// 获取此组件应该使用的主题
    /// </summary>
    public IComponentTheme Theme => ThemeProvider.Instance.GetTheme(ThemeId ?? "default")
                                    ?? ThemeProvider.Instance.CurrentTheme;

    /// <summary>
    /// 快速获取组件的基础样式
    /// </summary>
    public ComponentThemeStyle BaseStyle => Theme.GetStyle(Type, ComponentState.Normal);

    /// <summary>
    /// 从主题获取指定状态的样式
    /// </summary>
    public ComponentThemeStyle GetStateStyle(ComponentState state)
    {
        return Theme.GetStyle(Type, state);
    }
}

/// <summary>
/// 组件默认值配置 - 与 CanvasElement 属性保持一致
/// </summary>
public partial class ComponentDefaults : ObservableObject
{
    #region 尺寸属性

    [ObservableProperty]
    private double _width = 100;

    [ObservableProperty]
    private double _height = 50;

    [ObservableProperty]
    private double _x = 20;

    [ObservableProperty]
    private double _y = 20;

    [ObservableProperty]
    private double _rotation = 0;

    [ObservableProperty]
    private int _zIndex = 0;

    #endregion

    #region 盒子属性

    [ObservableProperty]
    private double _padding = 3;

    [ObservableProperty]
    private double _margin = 0;

    [ObservableProperty]
    private double _paddingLeft = 3;

    [ObservableProperty]
    private double _paddingTop = 3;

    [ObservableProperty]
    private double _paddingRight = 3;

    [ObservableProperty]
    private double _paddingBottom = 3;

    [ObservableProperty]
    private double _marginLeft = 0;

    [ObservableProperty]
    private double _marginTop = 0;

    [ObservableProperty]
    private double _marginRight = 0;

    [ObservableProperty]
    private double _marginBottom = 0;

    [ObservableProperty]
    private bool _paddingIsUniform = true;

    [ObservableProperty]
    private bool _marginIsUniform = true;

    #endregion

    #region 背景属性

    [ObservableProperty]
    private string _backgroundColor = "#FFFFFF";

    [ObservableProperty]
    private double _opacity = 1.0;

    [ObservableProperty]
    private string _backgroundImage = string.Empty;

    [ObservableProperty]
    private Stretch _backgroundImageStretch = Stretch.UniformToFill;

    [ObservableProperty]
    private TileMode _backgroundImageTileMode = TileMode.None;

    [ObservableProperty]
    private AlignmentX _backgroundImageAlignmentX = AlignmentX.Left;

    [ObservableProperty]
    private AlignmentY _backgroundImageAlignmentY = AlignmentY.Top;

    #endregion

    #region 边框属性

    [ObservableProperty]
    private string _borderColor = "#E5E5E5";

    [ObservableProperty]
    private double _borderWidth = 0;

    [ObservableProperty]
    private double _cornerRadius = 0;

    #endregion

    #region 阴影属性

    [ObservableProperty]
    private bool _hasShadow = false;

    [ObservableProperty]
    private string _shadowColor = "#000000";

    [ObservableProperty]
    private double _shadowBlur = 0;

    [ObservableProperty]
    private double _shadowOffsetX = 0;

    [ObservableProperty]
    private double _shadowOffsetY = 2;

    [ObservableProperty]
    private double _shadowSpread = 0;

    [ObservableProperty]
    private bool _isInset = false;

    #endregion

    #region 发光属性

    [ObservableProperty]
    private bool _hasGlow = false;

    [ObservableProperty]
    private string _glowColor = "#0099FF";

    [ObservableProperty]
    private double _glowBlurRadius = 15;

    [ObservableProperty]
    private double _glowOffsetX = 0;

    [ObservableProperty]
    private double _glowOffsetY = 0;

    [ObservableProperty]
    private double _glowOpacity = 0.9;

    #endregion

    #region 渐变属性

    [ObservableProperty]
    private FillType _fillType = FillType.Solid;

    [ObservableProperty]
    private string _gradientStartColor = "#FFFFFF";

    [ObservableProperty]
    private string _gradientEndColor = "#07C160";

    [ObservableProperty]
    private double _gradientAngle = 0;

    #endregion

    #region 文字属性

    [ObservableProperty]
    private string _foreColor = "#333333";

    [ObservableProperty]
    private double _fontSize = 14;

    [ObservableProperty]
    private bool _isBold = false;

    [ObservableProperty]
    private bool _isItalic = false;

    #endregion

    #region 媒体属性

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private Stretch _imageStretch = Stretch.Uniform;

    [ObservableProperty]
    private StretchDirection _imageStretchDirection = StretchDirection.Both;

    [ObservableProperty]
    private BitmapBlendingMode _blendMode = BitmapBlendingMode.Unspecified;

    #endregion

    #region 图形属性

    [ObservableProperty]
    private string _strokeColor = "#333333";

    [ObservableProperty]
    private double _strokeThickness = 2;

    #endregion

    #region 转换方法

    /// <summary>
    /// 从主题样式创建默认值
    /// </summary>
    public static ComponentDefaults FromThemeStyle(ComponentThemeStyle style)
    {
        return new ComponentDefaults
        {
            Padding = style.Padding,
            Margin = style.Margin,
            BackgroundColor = style.BackgroundColor ?? "#FFFFFF",
            Opacity = style.Opacity,
            BackgroundImage = style.BackgroundImage,
            BackgroundImageStretch = style.BackgroundImageStretch,
            BackgroundImageTileMode = style.BackgroundImageTileMode,
            BackgroundImageAlignmentX = style.BackgroundImageAlignmentX,
            BackgroundImageAlignmentY = style.BackgroundImageAlignmentY,
            BorderColor = style.BorderColor ?? "#E5E5E5",
            BorderWidth = style.BorderWidth,
            CornerRadius = style.CornerRadius,
            HasShadow = style.HasShadow,
            ShadowColor = style.ShadowColor ?? "#000000",
            ShadowBlur = style.ShadowBlur,
            ShadowOffsetX = style.ShadowOffsetX,
            ShadowOffsetY = style.ShadowOffsetY,
            ShadowSpread = style.ShadowSpread,
            IsInset = style.IsInset,
            HasGlow = style.HasGlow,
            GlowColor = style.GlowColor ?? "#0099FF",
            GlowBlurRadius = style.GlowBlurRadius,
            GlowOffsetX = style.GlowOffsetX,
            GlowOffsetY = style.GlowOffsetY,
            GlowOpacity = style.GlowOpacity,
            FillType = style.FillType,
            GradientStartColor = style.GradientStartColor ?? "#FFFFFF",
            GradientEndColor = style.GradientEndColor ?? "#07C160",
            GradientAngle = style.GradientAngle,
            ForeColor = style.ForeColor ?? "#333333",
            FontSize = style.FontSize,
            IsBold = style.IsBold,
            IsItalic = style.IsItalic
        };
    }

    /// <summary>
    /// 转换为组件主题样式
    /// </summary>
    public ComponentThemeStyle ToThemeStyle()
    {
        return ComponentThemeStyle.FromDefaults(this);
    }

    #endregion
}
