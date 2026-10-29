using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 描边线条样式下拉控件 <see cref="StrokeStyleDropDown"/> 的单个选项。
/// 每个选项独立持有 <see cref="DashArray"/> 字符串（用于绘制虚线预览，
/// 与 <see cref="Converters.StrokeStyleDashArrayConverter"/> 一同使用）以及
/// 鼠标悬停时显示的 <see cref="ToolTip"/>。
/// </summary>
public sealed class StrokeStyleDropDownItem
{
    public StrokeStyleDropDownItem(
        StrokeStyleType value,
        string dashArray,
        string toolTip)
    {
        Value = value;
        DashArray = dashArray;
        ToolTip = toolTip;
    }

    public StrokeStyleType Value { get; }

    /// <summary>
    /// Avalonia 兼容的虚线字符串（"4,2" / "6,2,1,2,1,2"），空串表示实线。
    /// 与 <see cref="Converters.StrokeStyleDashArrayConverter"/> 一起绑定到 <c>StrokeDashArray</c>。
    /// </summary>
    public string DashArray { get; }

    /// <summary>鼠标悬停在下拉项 / 触发器时显示的提示文本。</summary>
    public string ToolTip { get; }

    public bool HasToolTip => !string.IsNullOrWhiteSpace(ToolTip);
}