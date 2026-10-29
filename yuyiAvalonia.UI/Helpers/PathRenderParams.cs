using Avalonia;
using Avalonia.Media;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Helpers;

/// <summary>
/// Path 渲染参数，包含线条样式和几何属性
/// </summary>
public class PathRenderParams
{
    /// <summary>
    /// 线条宽度
    /// </summary>
    public double StrokeWidth { get; set; } = 1;

    /// <summary>
    /// 拐角连接类型
    /// </summary>
    public StrokeJoin StrokeJoin { get; set; } = StrokeJoin.Miter;

    /// <summary>
    /// 端点样式
    /// </summary>
    public StrokeEndpoints StrokeEndpoints { get; set; } = StrokeEndpoints.Butt;

    /// <summary>
    /// 线条样式类型（实线/虚线/点线等）
    /// </summary>
    public StrokeStyleType StrokeStyle { get; set; } = StrokeStyleType.Solid;

    /// <summary>
    /// 虚线偏移
    /// </summary>
    public double StrokeDashOffset { get; set; } = 0;

    /// <summary>
    /// Path 是否可见
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Path 宽度
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Path 高度
    /// </summary>
    public double Height { get; set; }

    /// <summary>
    /// Path 外边距（用于偏移 Path 位置）
    /// </summary>
    public Thickness Margin { get; set; } = new Thickness(0);

    /// <summary>
    /// Path 几何数据
    /// </summary>
    public Geometry? Data { get; set; }

    /// <summary>
    /// 线条背景填充画刷（填充 Path 内部区域，支持透明）
    /// </summary>
    public IBrush? Fill { get; set; }

    /// <summary>
    /// 填充透明度（0~1）
    /// </summary>
    public double FillOpacity { get; set; } = 1.0;

    /// <summary>
    /// MiterLimit：尖角最大长度限制（miterLength / lineWidth，超过则截断为 bevel），默认 10
    /// </summary>
    public double MiterLimit { get; set; } = 10.0;
}
