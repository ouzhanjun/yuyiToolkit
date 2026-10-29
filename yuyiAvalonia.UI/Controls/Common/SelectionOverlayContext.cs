using Avalonia;
using Avalonia.Controls;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.Renders;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 元素调整大小事件参数
/// </summary>
public class ElementResizedEventArgs : EventArgs
{
    public IReadOnlyList<CanvasElement> ResizedElements { get; }
    public ElementResizedEventArgs(IReadOnlyList<CanvasElement> elements) => ResizedElements = elements;
}

/// <summary>
/// 元素旋转事件参数
/// </summary>
public class ElementRotatedEventArgs : EventArgs
{
    public IReadOnlyList<CanvasElement> RotatedElements { get; }
    public ElementRotatedEventArgs(IReadOnlyList<CanvasElement> elements) => RotatedElements = elements;
}

/// <summary>
/// 元素对齐事件参数
/// </summary>
public class ElementAlignedEventArgs : EventArgs
{
    public IReadOnlyList<CanvasElement> AlignedElements { get; }
    public AlignmentType AlignmentType { get; }
    public ElementAlignedEventArgs(IReadOnlyList<CanvasElement> elements, AlignmentType alignmentType)
    {
        AlignedElements = elements;
        AlignmentType = alignmentType;
    }
}

/// <summary>
/// 单选覆盖层与宿主之间的上下文接口
/// 用于解耦 SingleSelectionOverlay 和 MultiSelectionOverlay
/// </summary>
public interface ISelectionOverlayContext
{
    /// <summary>
    /// 获取元素对应的控件映射
    /// </summary>
    Dictionary<CanvasElement, Control>? ElementControlMap { get; set; }

    /// <summary>
    /// 获取 RenderControl 用于更新控件
    /// </summary>
    RenderControl? RenderControl { get; set; }

    /// <summary>
    /// 获取当前 Canvas 尺寸
    /// </summary>
    Size CanvasSize { get; }

    /// <summary>
    /// 获取/设置父容器 Canvas
    /// </summary>
    Canvas? ParentCanvas { get; set; }

    /// <summary>
    /// 元素调整大小事件
    /// </summary>
    event EventHandler<ElementResizedEventArgs>? ElementResized;

    /// <summary>
    /// 元素旋转事件
    /// </summary>
    event EventHandler<ElementRotatedEventArgs>? ElementRotated;

    /// <summary>
    /// 获取元素在画布坐标系下的全局边界
    /// </summary>
    (double MinX, double MinY, double MaxX, double MaxY) GetGlobalBounds(CanvasElement element, Canvas? canvas = null);

    /// <summary>
    /// 更新单个元素的控件位置和尺寸
    /// </summary>
    void UpdateElementControl(CanvasElement element, Control control);

    /// <summary>
    /// 请求父级刷新选择遮罩
    /// </summary>
    void RequestRefreshSelectionMask();

    /// <summary>
    /// 请求父级清除选择
    /// </summary>
    void RequestClearSelection();
}
