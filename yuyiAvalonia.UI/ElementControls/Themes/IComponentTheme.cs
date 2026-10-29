using Avalonia.Media;
using Avalonia.Media.Imaging;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Themes;

/// <summary>
/// 组件主题样式定义 - 覆盖 CanvasElement 的所有视觉相关属性
/// </summary>
public class ComponentThemeStyle
{
    #region 盒子属性

    /// <summary>
    /// 内边距
    /// </summary>
    public double Padding { get; set; }

    /// <summary>
    /// 外边距
    /// </summary>
    public double Margin { get; set; }

    #endregion

    #region 背景属性

    /// <summary>
    /// 背景色
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// 透明度
    /// </summary>
    public double Opacity { get; set; } = 1.0;

    /// <summary>
    /// 背景图片路径
    /// </summary>
    public string? BackgroundImage { get; set; }

    /// <summary>
    /// 背景图片拉伸模式
    /// </summary>
    public Stretch BackgroundImageStretch { get; set; } = Stretch.UniformToFill;

    /// <summary>
    /// 背景图片平铺模式
    /// </summary>
    public TileMode BackgroundImageTileMode { get; set; } = TileMode.None;

    /// <summary>
    /// 背景图水平对齐方式
    /// </summary>
    public AlignmentX BackgroundImageAlignmentX { get; set; } = AlignmentX.Left;

    /// <summary>
    /// 背景图垂直对齐方式
    /// </summary>
    public AlignmentY BackgroundImageAlignmentY { get; set; } = AlignmentY.Top;

    #endregion

    #region 边框属性

    /// <summary>
    /// 边框颜色
    /// </summary>
    public string? BorderColor { get; set; }

    /// <summary>
    /// 边框宽度
    /// </summary>
    public double BorderWidth { get; set; }

    /// <summary>
    /// 圆角
    /// </summary>
    public double CornerRadius { get; set; }

    #endregion

    #region 阴影属性

    /// <summary>
    /// 是否有阴影
    /// </summary>
    public bool HasShadow { get; set; }

    /// <summary>
    /// 阴影颜色
    /// </summary>
    public string? ShadowColor { get; set; }

    /// <summary>
    /// 阴影模糊半径
    /// </summary>
    public double ShadowBlur { get; set; }

    /// <summary>
    /// 阴影X偏移
    /// </summary>
    public double ShadowOffsetX { get; set; }

    /// <summary>
    /// 阴影Y偏移
    /// </summary>
    public double ShadowOffsetY { get; set; }

    /// <summary>
    /// 阴影扩散
    /// </summary>
    public double ShadowSpread { get; set; }

    /// <summary>
    /// 是否为内阴影
    /// </summary>
    public bool IsInset { get; set; }

    #endregion

    #region 发光效果属性

    /// <summary>
    /// 是否有发光效果
    /// </summary>
    public bool HasGlow { get; set; }

    /// <summary>
    /// 发光颜色
    /// </summary>
    public string? GlowColor { get; set; }

    /// <summary>
    /// 发光模糊半径
    /// </summary>
    public double GlowBlurRadius { get; set; } = 15;

    /// <summary>
    /// 发光X偏移
    /// </summary>
    public double GlowOffsetX { get; set; }

    /// <summary>
    /// 发光Y偏移
    /// </summary>
    public double GlowOffsetY { get; set; }

    /// <summary>
    /// 发光透明度
    /// </summary>
    public double GlowOpacity { get; set; } = 0.9;

    #endregion

    #region 渐变属性

    /// <summary>
    /// 填充类型
    /// </summary>
    public FillType FillType { get; set; } = FillType.Solid;

    /// <summary>
    /// 渐变起始颜色
    /// </summary>
    public string? GradientStartColor { get; set; }

    /// <summary>
    /// 渐变结束颜色
    /// </summary>
    public string? GradientEndColor { get; set; }

    /// <summary>
    /// 渐变角度
    /// </summary>
    public double GradientAngle { get; set; }

    #endregion

    #region 文字样式属性

    /// <summary>
    /// 前景色（文字颜色）
    /// </summary>
    public string? ForeColor { get; set; }

    /// <summary>
    /// 字体大小
    /// </summary>
    public double FontSize { get; set; }

    /// <summary>
    /// 是否加粗
    /// </summary>
    public bool IsBold { get; set; }

    /// <summary>
    /// 是否斜体
    /// </summary>
    public bool IsItalic { get; set; }

    #endregion

    #region 变换属性

    /// <summary>
    /// 缩放变换
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>
    /// 过渡动画时长（毫秒）
    /// </summary>
    public int TransitionDuration { get; set; } = 150;

    #endregion

    #region 工厂方法和转换

    /// <summary>
    /// 创建默认样式
    /// </summary>
    public static ComponentThemeStyle CreateDefault()
    {
        return new ComponentThemeStyle
        {
            // 盒子
            Padding = 3,
            Margin = 0,

            // 背景
            BackgroundColor = "#FFFFFF",
            Opacity = 1.0,
            BackgroundImage = null,
            BackgroundImageStretch = Stretch.UniformToFill,
            BackgroundImageTileMode = TileMode.None,
            BackgroundImageAlignmentX = AlignmentX.Left,
            BackgroundImageAlignmentY = AlignmentY.Top,

            // 边框
            BorderColor = "#E5E5E5",
            BorderWidth = 0,
            CornerRadius = 0,

            // 阴影
            HasShadow = false,
            ShadowColor = "#000000",
            ShadowBlur = 0,
            ShadowOffsetX = 0,
            ShadowOffsetY = 2,
            ShadowSpread = 0,
            IsInset = false,

            // 发光
            HasGlow = false,
            GlowColor = "#0099FF",
            GlowBlurRadius = 15,
            GlowOffsetX = 0,
            GlowOffsetY = 0,
            GlowOpacity = 0.9,

            // 渐变
            FillType = FillType.Solid,
            GradientStartColor = "#FFFFFF",
            GradientEndColor = "#07C160",
            GradientAngle = 0,

            // 文字
            ForeColor = "#333333",
            FontSize = 14,
            IsBold = false,
            IsItalic = false,

            // 变换
            Scale = 1.0,
            TransitionDuration = 150
        };
    }

    /// <summary>
    /// 从 ComponentDefaults 创建样式
    /// </summary>
    public static ComponentThemeStyle FromDefaults(ComponentDefaults defaults)
    {
        return new ComponentThemeStyle
        {
            Padding = defaults.Padding,
            Margin = defaults.Margin,
            BackgroundColor = defaults.BackgroundColor,
            Opacity = defaults.Opacity,
            BorderColor = defaults.BorderColor,
            BorderWidth = defaults.BorderWidth,
            CornerRadius = defaults.CornerRadius,
            ShadowColor = defaults.ShadowColor,
            ShadowBlur = defaults.ShadowBlur,
            ShadowOffsetX = defaults.ShadowOffsetX,
            ShadowOffsetY = defaults.ShadowOffsetY,
            ShadowSpread = defaults.ShadowSpread,
            IsInset = defaults.IsInset,
            HasShadow = defaults.HasShadow,
            HasGlow = defaults.HasGlow,
            GlowColor = defaults.GlowColor,
            GlowBlurRadius = defaults.GlowBlurRadius,
            GlowOffsetX = defaults.GlowOffsetX,
            GlowOffsetY = defaults.GlowOffsetY,
            GlowOpacity = defaults.GlowOpacity,
            FillType = defaults.FillType,
            GradientStartColor = defaults.GradientStartColor,
            GradientEndColor = defaults.GradientEndColor,
            GradientAngle = defaults.GradientAngle,
            BackgroundImage = defaults.BackgroundImage,
            BackgroundImageStretch = defaults.BackgroundImageStretch,
            BackgroundImageTileMode = defaults.BackgroundImageTileMode,
            BackgroundImageAlignmentX = defaults.BackgroundImageAlignmentX,
            BackgroundImageAlignmentY = defaults.BackgroundImageAlignmentY,
            ForeColor = defaults.ForeColor,
            FontSize = defaults.FontSize,
            IsBold = defaults.IsBold,
            IsItalic = defaults.IsItalic
        };
    }

    #endregion

    #region 样式合并

    /// <summary>
    /// 合并样式（用于状态样式覆盖基础样式）
    /// </summary>
    public void ApplyOverrides(ComponentThemeStyle overrides)
    {
        // 盒子
        if (overrides.Padding >= 0) Padding = overrides.Padding;
        if (overrides.Margin >= 0) Margin = overrides.Margin;

        // 背景
        if (overrides.BackgroundColor != null) BackgroundColor = overrides.BackgroundColor;
        if (overrides.Opacity >= 0 && overrides.Opacity <= 1) Opacity = overrides.Opacity;
        if (overrides.BackgroundImage != null) BackgroundImage = overrides.BackgroundImage;
        BackgroundImageStretch = overrides.BackgroundImageStretch;
        BackgroundImageTileMode = overrides.BackgroundImageTileMode;
        BackgroundImageAlignmentX = overrides.BackgroundImageAlignmentX;
        BackgroundImageAlignmentY = overrides.BackgroundImageAlignmentY;

        // 边框
        if (overrides.BorderColor != null) BorderColor = overrides.BorderColor;
        if (overrides.BorderWidth >= 0) BorderWidth = overrides.BorderWidth;
        if (overrides.CornerRadius >= 0) CornerRadius = overrides.CornerRadius;

        // 阴影
        HasShadow = overrides.HasShadow;
        if (overrides.ShadowColor != null) ShadowColor = overrides.ShadowColor;
        if (overrides.ShadowBlur >= 0) ShadowBlur = overrides.ShadowBlur;
        if (overrides.ShadowOffsetX >= 0) ShadowOffsetX = overrides.ShadowOffsetX;
        if (overrides.ShadowOffsetY >= 0) ShadowOffsetY = overrides.ShadowOffsetY;
        if (overrides.ShadowSpread >= 0) ShadowSpread = overrides.ShadowSpread;
        IsInset = overrides.IsInset;

        // 发光
        HasGlow = overrides.HasGlow;
        if (overrides.GlowColor != null) GlowColor = overrides.GlowColor;
        if (overrides.GlowBlurRadius >= 0) GlowBlurRadius = overrides.GlowBlurRadius;
        if (overrides.GlowOffsetX >= 0) GlowOffsetX = overrides.GlowOffsetX;
        if (overrides.GlowOffsetY >= 0) GlowOffsetY = overrides.GlowOffsetY;
        if (overrides.GlowOpacity >= 0 && overrides.GlowOpacity <= 1) GlowOpacity = overrides.GlowOpacity;

        // 渐变
        FillType = overrides.FillType;
        if (overrides.GradientStartColor != null) GradientStartColor = overrides.GradientStartColor;
        if (overrides.GradientEndColor != null) GradientEndColor = overrides.GradientEndColor;
        if (overrides.GradientAngle >= 0) GradientAngle = overrides.GradientAngle;

        // 文字
        if (overrides.ForeColor != null) ForeColor = overrides.ForeColor;
        if (overrides.FontSize > 0) FontSize = overrides.FontSize;
        IsBold = overrides.IsBold;
        IsItalic = overrides.IsItalic;

        // 变换
        if (overrides.Scale > 0) Scale = overrides.Scale;
        if (overrides.TransitionDuration >= 0) TransitionDuration = overrides.TransitionDuration;
    }

    #endregion
}

/// <summary>
/// 单个组件类型的主题配置
/// </summary>
public class ComponentTypeTheme
{
    /// <summary>
    /// 组件类型
    /// </summary>
    public ComponentType ComponentType { get; set; }

    /// <summary>
    /// 基础样式（Normal状态）
    /// </summary>
    public ComponentThemeStyle BaseStyle { get; set; } = new();

    /// <summary>
    /// 各状态的样式覆盖
    /// </summary>
    public Dictionary<ComponentState, ComponentThemeStyle> StateStyles { get; set; } = new();

    /// <summary>
    /// 获取指定状态的样式
    /// </summary>
    public ComponentThemeStyle GetStyle(ComponentState state)
    {
        var style = new ComponentThemeStyle();
        style.ApplyOverrides(BaseStyle);

        if (StateStyles.TryGetValue(state, out var stateStyle))
        {
            style.ApplyOverrides(stateStyle);
        }

        return style;
    }
}

/// <summary>
/// 组件主题接口
/// </summary>
public interface IComponentTheme
{
    /// <summary>
    /// 主题名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 主题ID
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 主题描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 是否为深色主题
    /// </summary>
    bool IsDarkTheme { get; }

    /// <summary>
    /// 获取组件类型的主题配置
    /// </summary>
    ComponentTypeTheme? GetComponentTheme(ComponentType componentType);

    /// <summary>
    /// 获取组件在指定状态下的样式
    /// </summary>
    ComponentThemeStyle GetStyle(ComponentType componentType, ComponentState state);

    /// <summary>
    /// 获取所有组件类型的主题
    /// </summary>
    IEnumerable<ComponentTypeTheme> GetAllComponentThemes();

    /// <summary>
    /// 获取主题优先级（用于主题叠加）
    /// </summary>
    int Priority { get; }
}

/// <summary>
/// 主题扩展方法
/// </summary>
public static class ComponentThemeExtensions
{
    /// <summary>
    /// 获取悬停状态样式
    /// </summary>
    public static ComponentThemeStyle GetHoverStyle(this IComponentTheme theme, ComponentType type)
    {
        return theme.GetStyle(type, ComponentState.Hover);
    }

    /// <summary>
    /// 获取按下状态样式
    /// </summary>
    public static ComponentThemeStyle GetPressedStyle(this IComponentTheme theme, ComponentType type)
    {
        return theme.GetStyle(type, ComponentState.Pressed);
    }

    /// <summary>
    /// 获取禁用状态样式
    /// </summary>
    public static ComponentThemeStyle GetDisabledStyle(this IComponentTheme theme, ComponentType type)
    {
        return theme.GetStyle(type, ComponentState.Disabled);
    }

    /// <summary>
    /// 批量应用样式到元素
    /// </summary>
    public static void ApplyStyleToElement(this ComponentThemeStyle style, Models.CanvasElement element)
    {
        // 盒子
        if (style.Padding >= 0) element.Padding = style.Padding;
        if (style.Margin >= 0) element.Margin = style.Margin;

        // 背景（通过 BackgroundFill 统一管理）
        if (style.BackgroundColor != null) element.BackgroundFill.BackgroundColor = style.BackgroundColor;
        if (style.Opacity >= 0 && style.Opacity <= 1) element.Opacity = style.Opacity;
        if (style.BackgroundImage != null) element.BackgroundFill.ImagePath = style.BackgroundImage;
        element.BackgroundFill.ImageStretch = style.BackgroundImageStretch;
        element.BackgroundFill.ImageTileMode = style.BackgroundImageTileMode;
        element.BackgroundFill.ImageAlignmentX = style.BackgroundImageAlignmentX;
        element.BackgroundFill.ImageAlignmentY = style.BackgroundImageAlignmentY;

        // 边框
        if (style.BorderColor != null) element.BorderColor = style.BorderColor;
        if (style.BorderWidth >= 0) element.BorderWidth = style.BorderWidth;
        if (style.CornerRadius >= 0) element.CornerRadius = style.CornerRadius;

        // 填充
        element.BackgroundFill.FillType = style.FillType;
        if (style.GradientStartColor != null) element.BackgroundFill.GradientStartColor = style.GradientStartColor;
        if (style.GradientEndColor != null) element.BackgroundFill.GradientEndColor = style.GradientEndColor;
        if (style.GradientAngle >= 0) element.BackgroundFill.GradientAngle = style.GradientAngle;

        // 文字相关属性（仅对支持这些属性的元素类型）
        if (element is TextElement textElement)
        {
            if (style.ForeColor != null) textElement.ForeColor = style.ForeColor;
            if (style.FontSize > 0) textElement.FontSize = style.FontSize;
            textElement.IsBold = style.IsBold;
            textElement.IsItalic = style.IsItalic;
        }
        else if (element is ButtonElement buttonElement)
        {
            if (style.ForeColor != null) buttonElement.ForeColor = style.ForeColor;
            if (style.FontSize > 0) buttonElement.FontSize = style.FontSize;
            buttonElement.IsBold = style.IsBold;
            buttonElement.IsItalic = style.IsItalic;
        }
    }
}
