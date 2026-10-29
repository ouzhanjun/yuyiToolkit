using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 组件完整样式 - 包含组件的所有样式属性
/// </summary>
public partial class ComponentStyle : ObservableObject
{
    #region 基础属性

    /// <summary>
    /// 圆角
    /// </summary>
    [ObservableProperty] private double _borderRadius = 6;

    /// <summary>
    /// 边框宽度
    /// </summary>
    [ObservableProperty] private double _borderWidth = 1;

    /// <summary>
    /// 阴影颜色
    /// </summary>
    [ObservableProperty] private string _shadowColor = "#00000015";

    /// <summary>
    /// 阴影模糊半径
    /// </summary>
    [ObservableProperty] private double _shadowBlur = 8;

    /// <summary>
    /// 阴影偏移X
    /// </summary>
    [ObservableProperty] private double _shadowOffsetX = 0;

    /// <summary>
    /// 阴影偏移Y
    /// </summary>
    [ObservableProperty] private double _shadowOffsetY = 2;

    /// <summary>
    /// 内边距
    /// </summary>
    [ObservableProperty] private double _padding = 12;

    #endregion

    #region 默认状态

    /// <summary>
    /// 默认-背景色
    /// </summary>
    [ObservableProperty] private string _backgroundColor = "#FFFFFF";

    /// <summary>
    /// 默认-前景色（文字颜色）
    /// </summary>
    [ObservableProperty] private string _foregroundColor = "#333333";

    /// <summary>
    /// 默认-边框颜色
    /// </summary>
    [ObservableProperty] private string _borderColor = "#D9D9D9";

    /// <summary>
    /// 默认-不透明度
    /// </summary>
    [ObservableProperty] private double _opacity = 1.0;

    #endregion

    #region 悬停状态

    /// <summary>
    /// 悬停-背景色
    /// </summary>
    [ObservableProperty] private string _hoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 悬停-前景色
    /// </summary>
    [ObservableProperty] private string _hoverForegroundColor = "#333333";

    /// <summary>
    /// 悬停-边框颜色
    /// </summary>
    [ObservableProperty] private string _hoverBorderColor = "#1890FF";

    /// <summary>
    /// 悬停-不透明度
    /// </summary>
    [ObservableProperty] private double _hoverOpacity = 1.0;

    #endregion

    #region 按下/激活状态

    /// <summary>
    /// 按下-背景色
    /// </summary>
    [ObservableProperty] private string _activeBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 按下-前景色
    /// </summary>
    [ObservableProperty] private string _activeForegroundColor = "#1890FF";

    /// <summary>
    /// 按下-边框颜色
    /// </summary>
    [ObservableProperty] private string _activeBorderColor = "#1890FF";

    /// <summary>
    /// 按下-不透明度
    /// </summary>
    [ObservableProperty] private double _activeOpacity = 1.0;

    #endregion

    #region 选中状态

    /// <summary>
    /// 选中-背景色
    /// </summary>
    [ObservableProperty] private string _selectedBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 选中-前景色
    /// </summary>
    [ObservableProperty] private string _selectedForegroundColor = "#1890FF";

    /// <summary>
    /// 选中-边框颜色
    /// </summary>
    [ObservableProperty] private string _selectedBorderColor = "#1890FF";

    /// <summary>
    /// 选中-不透明度
    /// </summary>
    [ObservableProperty] private double _selectedOpacity = 1.0;

    #endregion

    #region 禁用状态

    /// <summary>
    /// 禁用-背景色
    /// </summary>
    [ObservableProperty] private string _disabledBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 禁用-前景色
    /// </summary>
    [ObservableProperty] private string _disabledForegroundColor = "#BFBFBF";

    /// <summary>
    /// 禁用-边框颜色
    /// </summary>
    [ObservableProperty] private string _disabledBorderColor = "#D9D9D9";

    /// <summary>
    /// 禁用-不透明度
    /// </summary>
    [ObservableProperty] private double _disabledOpacity = 0.5;

    #endregion

    /// <summary>
    /// 克隆样式
    /// </summary>
    public ComponentStyle Clone()
    {
        return new ComponentStyle
        {
            BorderRadius = BorderRadius,
            BorderWidth = BorderWidth,
            ShadowColor = ShadowColor,
            ShadowBlur = ShadowBlur,
            ShadowOffsetX = ShadowOffsetX,
            ShadowOffsetY = ShadowOffsetY,
            Padding = Padding,
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            BorderColor = BorderColor,
            Opacity = Opacity,
            HoverBackgroundColor = HoverBackgroundColor,
            HoverForegroundColor = HoverForegroundColor,
            HoverBorderColor = HoverBorderColor,
            HoverOpacity = HoverOpacity,
            ActiveBackgroundColor = ActiveBackgroundColor,
            ActiveForegroundColor = ActiveForegroundColor,
            ActiveBorderColor = ActiveBorderColor,
            ActiveOpacity = ActiveOpacity,
            SelectedBackgroundColor = SelectedBackgroundColor,
            SelectedForegroundColor = SelectedForegroundColor,
            SelectedBorderColor = SelectedBorderColor,
            SelectedOpacity = SelectedOpacity,
            DisabledBackgroundColor = DisabledBackgroundColor,
            DisabledForegroundColor = DisabledForegroundColor,
            DisabledBorderColor = DisabledBorderColor,
            DisabledOpacity = DisabledOpacity
        };
    }

    /// <summary>
    /// 生成 CSS
    /// </summary>
    public string ToCss(string selector)
    {
        var shadow = ShadowOffsetX != 0 || ShadowOffsetY != 0
            ? $"box-shadow: {ShadowOffsetX}px {ShadowOffsetY}px {ShadowBlur}px {ShadowColor};"
            : "";

        return $@"{selector} {{
    background-color: {BackgroundColor};
    color: {ForegroundColor};
    border: {BorderWidth}px solid {BorderColor};
    border-radius: {BorderRadius}px;
    padding: {Padding}px;
    opacity: {Opacity};
    {(string.IsNullOrEmpty(shadow) ? "" : shadow)}
}}
{selector}:hover {{
    background-color: {HoverBackgroundColor};
    color: {HoverForegroundColor};
    border-color: {HoverBorderColor};
    opacity: {HoverOpacity};
}}
{selector}:active {{
    background-color: {ActiveBackgroundColor};
    color: {ActiveForegroundColor};
    border-color: {ActiveBorderColor};
    opacity: {ActiveOpacity};
}}
{selector}:selected {{
    background-color: {SelectedBackgroundColor};
    color: {SelectedForegroundColor};
    border-color: {SelectedBorderColor};
    opacity: {SelectedOpacity};
}}
{selector}:disabled {{
    background-color: {DisabledBackgroundColor};
    color: {DisabledForegroundColor};
    border-color: {DisabledBorderColor};
    opacity: {DisabledOpacity};
}}";
    }
}
