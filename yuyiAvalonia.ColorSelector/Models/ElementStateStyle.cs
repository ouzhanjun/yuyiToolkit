using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 元素状态样式 - 包含各状态下的背景色、前景色、边框色
/// </summary>
public partial class ElementStateStyle : ObservableObject
{
    /// <summary>
    /// 默认状态
    /// </summary>
    [ObservableProperty] private string _backgroundColor = "transparent";
    [ObservableProperty] private string _foregroundColor = "#333333";
    [ObservableProperty] private string _borderColor = "#D9D9D9";
    [ObservableProperty] private string _borderWidth = "1px";
    [ObservableProperty] private double _opacity = 1.0;

    /// <summary>
    /// 悬停状态
    /// </summary>
    [ObservableProperty] private string _hoverBackgroundColor = "#F5F5F5";
    [ObservableProperty] private string _hoverForegroundColor = "#333333";
    [ObservableProperty] private string _hoverBorderColor = "#1890FF";
    [ObservableProperty] private double _hoverOpacity = 1.0;

    /// <summary>
    /// 按下/激活状态
    /// </summary>
    [ObservableProperty] private string _activeBackgroundColor = "#E6F7FF";
    [ObservableProperty] private string _activeForegroundColor = "#1890FF";
    [ObservableProperty] private string _activeBorderColor = "#1890FF";
    [ObservableProperty] private double _activeOpacity = 1.0;

    /// <summary>
    /// 选中状态
    /// </summary>
    [ObservableProperty] private string _selectedBackgroundColor = "#E6F7FF";
    [ObservableProperty] private string _selectedForegroundColor = "#1890FF";
    [ObservableProperty] private string _selectedBorderColor = "#1890FF";
    [ObservableProperty] private double _selectedOpacity = 1.0;

    /// <summary>
    /// 禁用状态
    /// </summary>
    [ObservableProperty] private string _disabledBackgroundColor = "#F5F5F5";
    [ObservableProperty] private string _disabledForegroundColor = "#BFBFBF";
    [ObservableProperty] private string _disabledBorderColor = "#D9D9D9";
    [ObservableProperty] private double _disabledOpacity = 0.5;

    /// <summary>
    /// 获取指定状态的样式值
    /// </summary>
    public (string bg, string fg, string border, double opacity) GetStateStyle(ElementState state)
    {
        return state switch
        {
            ElementState.Default => (BackgroundColor, ForegroundColor, BorderColor, Opacity),
            ElementState.Hover => (HoverBackgroundColor, HoverForegroundColor, HoverBorderColor, HoverOpacity),
            ElementState.Active => (ActiveBackgroundColor, ActiveForegroundColor, ActiveBorderColor, ActiveOpacity),
            ElementState.Selected => (SelectedBackgroundColor, SelectedForegroundColor, SelectedBorderColor, SelectedOpacity),
            ElementState.Disabled => (DisabledBackgroundColor, DisabledForegroundColor, DisabledBorderColor, DisabledOpacity),
            _ => (BackgroundColor, ForegroundColor, BorderColor, Opacity)
        };
    }

    /// <summary>
    /// 克隆样式
    /// </summary>
    public ElementStateStyle Clone()
    {
        return new ElementStateStyle
        {
            BackgroundColor = BackgroundColor,
            ForegroundColor = ForegroundColor,
            BorderColor = BorderColor,
            BorderWidth = BorderWidth,
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
    /// 生成 CSS 状态样式
    /// </summary>
    public string ToCss(string selector, string baseStyles = "")
    {
        return $@"{selector} {{
    {baseStyles}
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
{selector}:disabled {{
    background-color: {DisabledBackgroundColor};
    color: {DisabledForegroundColor};
    border-color: {DisabledBorderColor};
    opacity: {DisabledOpacity};
}}";
    }
}

/// <summary>
/// 元素状态枚举
/// </summary>
public enum ElementState
{
    Default,
    Hover,
    Active,
    Selected,
    Disabled
}
