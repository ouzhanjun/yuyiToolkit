using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 表单元素样式 - 包含按钮、输入框等表单元素的完整样式
/// </summary>
public partial class FormElementStyle : ObservableObject
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
    /// 内边距
    /// </summary>
    [ObservableProperty] private double _paddingHorizontal = 16;

    /// <summary>
    /// 内边距（垂直）
    /// </summary>
    [ObservableProperty] private double _paddingVertical = 8;

    /// <summary>
    /// 字体大小
    /// </summary>
    [ObservableProperty] private double _fontSize = 14;

    /// <summary>
    /// 字体粗细
    /// </summary>
    [ObservableProperty] private string _fontWeight = "400";

    #endregion

    #region 主色调按钮样式

    /// <summary>
    /// 主按钮-默认背景色
    /// </summary>
    [ObservableProperty] private string _primaryBackgroundColor = "#1890FF";

    /// <summary>
    /// 主按钮-默认前景色
    /// </summary>
    [ObservableProperty] private string _primaryForegroundColor = "#FFFFFF";

    /// <summary>
    /// 主按钮-默认边框颜色
    /// </summary>
    [ObservableProperty] private string _primaryBorderColor = "#1890FF";

    /// <summary>
    /// 主按钮-悬停背景色
    /// </summary>
    [ObservableProperty] private string _primaryHoverBackgroundColor = "#096DD9";

    /// <summary>
    /// 主按钮-悬停前景色
    /// </summary>
    [ObservableProperty] private string _primaryHoverForegroundColor = "#FFFFFF";

    /// <summary>
    /// 主按钮-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _primaryHoverBorderColor = "#096DD9";

    /// <summary>
    /// 主按钮-按下背景色
    /// </summary>
    [ObservableProperty] private string _primaryActiveBackgroundColor = "#0050B3";

    /// <summary>
    /// 主按钮-按下前景色
    /// </summary>
    [ObservableProperty] private string _primaryActiveForegroundColor = "#FFFFFF";

    /// <summary>
    /// 主按钮-按下边框颜色
    /// </summary>
    [ObservableProperty] private string _primaryActiveBorderColor = "#0050B3";

    /// <summary>
    /// 主按钮-禁用背景色
    /// </summary>
    [ObservableProperty] private string _primaryDisabledBackgroundColor = "#D9D9D9";

    /// <summary>
    /// 主按钮-禁用前景色
    /// </summary>
    [ObservableProperty] private string _primaryDisabledForegroundColor = "#FFFFFF";

    /// <summary>
    /// 主按钮-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _primaryDisabledBorderColor = "#D9D9D9";

    #endregion

    #region 次要按钮样式

    /// <summary>
    /// 次按钮-默认背景色
    /// </summary>
    [ObservableProperty] private string _secondaryBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 次按钮-默认前景色
    /// </summary>
    [ObservableProperty] private string _secondaryForegroundColor = "#333333";

    /// <summary>
    /// 次按钮-默认边框颜色
    /// </summary>
    [ObservableProperty] private string _secondaryBorderColor = "#D9D9D9";

    /// <summary>
    /// 次按钮-悬停背景色
    /// </summary>
    [ObservableProperty] private string _secondaryHoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 次按钮-悬停前景色
    /// </summary>
    [ObservableProperty] private string _secondaryHoverForegroundColor = "#333333";

    /// <summary>
    /// 次按钮-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _secondaryHoverBorderColor = "#1890FF";

    /// <summary>
    /// 次按钮-按下背景色
    /// </summary>
    [ObservableProperty] private string _secondaryActiveBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 次按钮-按下前景色
    /// </summary>
    [ObservableProperty] private string _secondaryActiveForegroundColor = "#1890FF";

    /// <summary>
    /// 次按钮-按下边框颜色
    /// </summary>
    [ObservableProperty] private string _secondaryActiveBorderColor = "#1890FF";

    /// <summary>
    /// 次按钮-禁用背景色
    /// </summary>
    [ObservableProperty] private string _secondaryDisabledBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 次按钮-禁用前景色
    /// </summary>
    [ObservableProperty] private string _secondaryDisabledForegroundColor = "#BFBFBF";

    /// <summary>
    /// 次按钮-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _secondaryDisabledBorderColor = "#D9D9D9";

    #endregion

    #region 虚线按钮/文字按钮样式

    /// <summary>
    /// 虚线按钮-默认背景色
    /// </summary>
    [ObservableProperty] private string _dashedBackgroundColor = "transparent";

    /// <summary>
    /// 虚线按钮-默认前景色
    /// </summary>
    [ObservableProperty] private string _dashedForegroundColor = "#333333";

    /// <summary>
    /// 虚线按钮-默认边框颜色
    /// </summary>
    [ObservableProperty] private string _dashedBorderColor = "#D9D9D9";

    /// <summary>
    /// 虚线按钮-悬停背景色
    /// </summary>
    [ObservableProperty] private string _dashedHoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 虚线按钮-悬停前景色
    /// </summary>
    [ObservableProperty] private string _dashedHoverForegroundColor = "#1890FF";

    /// <summary>
    /// 虚线按钮-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _dashedHoverBorderColor = "#1890FF";

    /// <summary>
    /// 虚线按钮-按下背景色
    /// </summary>
    [ObservableProperty] private string _dashedActiveBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 虚线按钮-按下前景色
    /// </summary>
    [ObservableProperty] private string _dashedActiveForegroundColor = "#1890FF";

    /// <summary>
    /// 虚线按钮-按下边框颜色
    /// </summary>
    [ObservableProperty] private string _dashedActiveBorderColor = "#1890FF";

    /// <summary>
    /// 虚线按钮-禁用背景色
    /// </summary>
    [ObservableProperty] private string _dashedDisabledBackgroundColor = "transparent";

    /// <summary>
    /// 虚线按钮-禁用前景色
    /// </summary>
    [ObservableProperty] private string _dashedDisabledForegroundColor = "#BFBFBF";

    /// <summary>
    /// 虚线按钮-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _dashedDisabledBorderColor = "#D9D9D9";

    #endregion

    #region 文本输入框样式

    /// <summary>
    /// 输入框-默认背景色
    /// </summary>
    [ObservableProperty] private string _inputBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 输入框-默认前景色
    /// </summary>
    [ObservableProperty] private string _inputForegroundColor = "#333333";

    /// <summary>
    /// 输入框-默认边框颜色
    /// </summary>
    [ObservableProperty] private string _inputBorderColor = "#D9D9D9";

    /// <summary>
    /// 输入框-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _inputHoverBorderColor = "#40A9FF";

    /// <summary>
    /// 输入框-聚焦边框颜色
    /// </summary>
    [ObservableProperty] private string _inputFocusBorderColor = "#1890FF";

    /// <summary>
    /// 输入框-聚焦阴影颜色
    /// </summary>
    [ObservableProperty] private string _inputFocusShadowColor = "#1890FF33";

    /// <summary>
    /// 输入框-禁用背景色
    /// </summary>
    [ObservableProperty] private string _inputDisabledBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 输入框-禁用前景色
    /// </summary>
    [ObservableProperty] private string _inputDisabledForegroundColor = "#BFBFBF";

    /// <summary>
    /// 输入框-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _inputDisabledBorderColor = "#D9D9D9";

    /// <summary>
    /// 输入框-占位符颜色
    /// </summary>
    [ObservableProperty] private string _inputPlaceholderColor = "#BFBFBF";

    #endregion

    #region 下拉选择框样式

    /// <summary>
    /// 下拉框-默认背景色
    /// </summary>
    [ObservableProperty] private string _selectBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 下拉框-默认前景色
    /// </summary>
    [ObservableProperty] private string _selectForegroundColor = "#333333";

    /// <summary>
    /// 下拉框-默认边框颜色
    /// </summary>
    [ObservableProperty] private string _selectBorderColor = "#D9D9D9";

    /// <summary>
    /// 下拉框-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _selectHoverBorderColor = "#40A9FF";

    /// <summary>
    /// 下拉框-选中背景色
    /// </summary>
    [ObservableProperty] private string _selectSelectedBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 下拉框-选中前景色
    /// </summary>
    [ObservableProperty] private string _selectSelectedForegroundColor = "#1890FF";

    /// <summary>
    /// 下拉框-禁用背景色
    /// </summary>
    [ObservableProperty] private string _selectDisabledBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 下拉框-禁用前景色
    /// </summary>
    [ObservableProperty] private string _selectDisabledForegroundColor = "#BFBFBF";

    /// <summary>
    /// 下拉框-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _selectDisabledBorderColor = "#D9D9D9";

    /// <summary>
    /// 下拉框-下拉箭头颜色
    /// </summary>
    [ObservableProperty] private string _selectArrowColor = "#666666";

    /// <summary>
    /// 下拉框-下拉箭头禁用颜色
    /// </summary>
    [ObservableProperty] private string _selectArrowDisabledColor = "#BFBFBF";

    #endregion

    #region 复选框样式

    /// <summary>
    /// 复选框-未选中背景色
    /// </summary>
    [ObservableProperty] private string _checkboxUncheckedBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 复选框-未选中边框颜色
    /// </summary>
    [ObservableProperty] private string _checkboxUncheckedBorderColor = "#D9D9D9";

    /// <summary>
    /// 复选框-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _checkboxHoverBorderColor = "#40A9FF";

    /// <summary>
    /// 复选框-选中背景色
    /// </summary>
    [ObservableProperty] private string _checkboxCheckedBackgroundColor = "#1890FF";

    /// <summary>
    /// 复选框-选中边框颜色
    /// </summary>
    [ObservableProperty] private string _checkboxCheckedBorderColor = "#1890FF";

    /// <summary>
    /// 复选框-选中图标颜色
    /// </summary>
    [ObservableProperty] private string _checkboxCheckedIconColor = "#FFFFFF";

    /// <summary>
    /// 复选框-禁用背景色
    /// </summary>
    [ObservableProperty] private string _checkboxDisabledBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 复选框-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _checkboxDisabledBorderColor = "#D9D9D9";

    #endregion

    #region 单选框样式

    /// <summary>
    /// 单选框-未选中背景色
    /// </summary>
    [ObservableProperty] private string _radioUncheckedBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 单选框-未选中边框颜色
    /// </summary>
    [ObservableProperty] private string _radioUncheckedBorderColor = "#D9D9D9";

    /// <summary>
    /// 单选框-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _radioHoverBorderColor = "#40A9FF";

    /// <summary>
    /// 单选框-选中背景色
    /// </summary>
    [ObservableProperty] private string _radioCheckedBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 单选框-选中边框颜色
    /// </summary>
    [ObservableProperty] private string _radioCheckedBorderColor = "#1890FF";

    /// <summary>
    /// 单选框-选中圆点颜色
    /// </summary>
    [ObservableProperty] private string _radioCheckedDotColor = "#1890FF";

    /// <summary>
    /// 单选框-禁用背景色
    /// </summary>
    [ObservableProperty] private string _radioDisabledBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 单选框-禁用边框颜色
    /// </summary>
    [ObservableProperty] private string _radioDisabledBorderColor = "#D9D9D9";

    #endregion

    #region 开关样式

    /// <summary>
    /// 开关-关闭背景色
    /// </summary>
    [ObservableProperty] private string _switchOffBackgroundColor = "#D9D9D9";

    /// <summary>
    /// 开关-关闭滑块颜色
    /// </summary>
    [ObservableProperty] private string _switchOffThumbColor = "#FFFFFF";

    /// <summary>
    /// 开关-打开背景色
    /// </summary>
    [ObservableProperty] private string _switchOnBackgroundColor = "#1890FF";

    /// <summary>
    /// 开关-打开滑块颜色
    /// </summary>
    [ObservableProperty] private string _switchOnThumbColor = "#FFFFFF";

    /// <summary>
    /// 开关-禁用关闭背景色
    /// </summary>
    [ObservableProperty] private string _switchDisabledOffBackgroundColor = "#E0E0E0";

    /// <summary>
    /// 开关-禁用关闭滑块颜色
    /// </summary>
    [ObservableProperty] private string _switchDisabledOffThumbColor = "#BFBFBF";

    /// <summary>
    /// 开关-禁用打开背景色
    /// </summary>
    [ObservableProperty] private string _switchDisabledOnBackgroundColor = "#BFBFBF";

    #endregion

    /// <summary>
    /// 克隆样式
    /// </summary>
    public FormElementStyle Clone()
    {
        return new FormElementStyle
        {
            BorderRadius = BorderRadius,
            BorderWidth = BorderWidth,
            PaddingHorizontal = PaddingHorizontal,
            PaddingVertical = PaddingVertical,
            FontSize = FontSize,
            FontWeight = FontWeight,
            PrimaryBackgroundColor = PrimaryBackgroundColor,
            PrimaryForegroundColor = PrimaryForegroundColor,
            PrimaryBorderColor = PrimaryBorderColor,
            PrimaryHoverBackgroundColor = PrimaryHoverBackgroundColor,
            PrimaryHoverForegroundColor = PrimaryHoverForegroundColor,
            PrimaryHoverBorderColor = PrimaryHoverBorderColor,
            PrimaryActiveBackgroundColor = PrimaryActiveBackgroundColor,
            PrimaryActiveForegroundColor = PrimaryActiveForegroundColor,
            PrimaryActiveBorderColor = PrimaryActiveBorderColor,
            PrimaryDisabledBackgroundColor = PrimaryDisabledBackgroundColor,
            PrimaryDisabledForegroundColor = PrimaryDisabledForegroundColor,
            PrimaryDisabledBorderColor = PrimaryDisabledBorderColor,
            SecondaryBackgroundColor = SecondaryBackgroundColor,
            SecondaryForegroundColor = SecondaryForegroundColor,
            SecondaryBorderColor = SecondaryBorderColor,
            SecondaryHoverBackgroundColor = SecondaryHoverBackgroundColor,
            SecondaryHoverForegroundColor = SecondaryHoverForegroundColor,
            SecondaryHoverBorderColor = SecondaryHoverBorderColor,
            SecondaryActiveBackgroundColor = SecondaryActiveBackgroundColor,
            SecondaryActiveForegroundColor = SecondaryActiveForegroundColor,
            SecondaryActiveBorderColor = SecondaryActiveBorderColor,
            SecondaryDisabledBackgroundColor = SecondaryDisabledBackgroundColor,
            SecondaryDisabledForegroundColor = SecondaryDisabledForegroundColor,
            SecondaryDisabledBorderColor = SecondaryDisabledBorderColor,
            DashedBackgroundColor = DashedBackgroundColor,
            DashedForegroundColor = DashedForegroundColor,
            DashedBorderColor = DashedBorderColor,
            DashedHoverBackgroundColor = DashedHoverBackgroundColor,
            DashedHoverForegroundColor = DashedHoverForegroundColor,
            DashedHoverBorderColor = DashedHoverBorderColor,
            DashedActiveBackgroundColor = DashedActiveBackgroundColor,
            DashedActiveForegroundColor = DashedActiveForegroundColor,
            DashedActiveBorderColor = DashedActiveBorderColor,
            DashedDisabledBackgroundColor = DashedDisabledBackgroundColor,
            DashedDisabledForegroundColor = DashedDisabledForegroundColor,
            DashedDisabledBorderColor = DashedDisabledBorderColor,
            InputBackgroundColor = InputBackgroundColor,
            InputForegroundColor = InputForegroundColor,
            InputBorderColor = InputBorderColor,
            InputHoverBorderColor = InputHoverBorderColor,
            InputFocusBorderColor = InputFocusBorderColor,
            InputFocusShadowColor = InputFocusShadowColor,
            InputDisabledBackgroundColor = InputDisabledBackgroundColor,
            InputDisabledForegroundColor = InputDisabledForegroundColor,
            InputDisabledBorderColor = InputDisabledBorderColor,
            InputPlaceholderColor = InputPlaceholderColor,
            SelectBackgroundColor = SelectBackgroundColor,
            SelectForegroundColor = SelectForegroundColor,
            SelectBorderColor = SelectBorderColor,
            SelectHoverBorderColor = SelectHoverBorderColor,
            SelectSelectedBackgroundColor = SelectSelectedBackgroundColor,
            SelectSelectedForegroundColor = SelectSelectedForegroundColor,
            SelectDisabledBackgroundColor = SelectDisabledBackgroundColor,
            SelectDisabledForegroundColor = SelectDisabledForegroundColor,
            SelectDisabledBorderColor = SelectDisabledBorderColor,
            SelectArrowColor = SelectArrowColor,
            SelectArrowDisabledColor = SelectArrowDisabledColor,
            CheckboxUncheckedBackgroundColor = CheckboxUncheckedBackgroundColor,
            CheckboxUncheckedBorderColor = CheckboxUncheckedBorderColor,
            CheckboxHoverBorderColor = CheckboxHoverBorderColor,
            CheckboxCheckedBackgroundColor = CheckboxCheckedBackgroundColor,
            CheckboxCheckedBorderColor = CheckboxCheckedBorderColor,
            CheckboxCheckedIconColor = CheckboxCheckedIconColor,
            CheckboxDisabledBackgroundColor = CheckboxDisabledBackgroundColor,
            CheckboxDisabledBorderColor = CheckboxDisabledBorderColor,
            RadioUncheckedBackgroundColor = RadioUncheckedBackgroundColor,
            RadioUncheckedBorderColor = RadioUncheckedBorderColor,
            RadioHoverBorderColor = RadioHoverBorderColor,
            RadioCheckedBackgroundColor = RadioCheckedBackgroundColor,
            RadioCheckedBorderColor = RadioCheckedBorderColor,
            RadioCheckedDotColor = RadioCheckedDotColor,
            RadioDisabledBackgroundColor = RadioDisabledBackgroundColor,
            RadioDisabledBorderColor = RadioDisabledBorderColor,
            SwitchOffBackgroundColor = SwitchOffBackgroundColor,
            SwitchOffThumbColor = SwitchOffThumbColor,
            SwitchOnBackgroundColor = SwitchOnBackgroundColor,
            SwitchOnThumbColor = SwitchOnThumbColor,
            SwitchDisabledOffBackgroundColor = SwitchDisabledOffBackgroundColor,
            SwitchDisabledOffThumbColor = SwitchDisabledOffThumbColor,
            SwitchDisabledOnBackgroundColor = SwitchDisabledOnBackgroundColor
        };
    }

    /// <summary>
    /// 生成按钮 CSS
    /// </summary>
    public string ToButtonCss(string selector, string bg, string fg, string border, string hoverBg, string hoverFg, string hoverBorder, string activeBg, string activeFg, string activeBorder, string disabledBg, string disabledFg, string disabledBorder)
    {
        return $@"{selector} {{
    background-color: {bg};
    color: {fg};
    border: {BorderWidth}px solid {border};
    border-radius: {BorderRadius}px;
    padding: {PaddingVertical}px {PaddingHorizontal}px;
    font-size: {FontSize}px;
    font-weight: {FontWeight};
    cursor: pointer;
}}
{selector}:hover {{
    background-color: {hoverBg};
    color: {hoverFg};
    border-color: {hoverBorder};
}}
{selector}:active {{
    background-color: {activeBg};
    color: {activeFg};
    border-color: {activeBorder};
}}
{selector}:disabled {{
    background-color: {disabledBg};
    color: {disabledFg};
    border-color: {disabledBorder};
    cursor: not-allowed;
    opacity: 0.5;
}}";
    }
}
