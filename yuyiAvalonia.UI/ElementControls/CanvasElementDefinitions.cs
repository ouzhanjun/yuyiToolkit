namespace yuyiAvalonia.UI.ElementControls;

public enum ButtonStyle
{
    Primary,
    Secondary,
    Outline,
    Text
}

/// <summary>
/// StackPanel容器方向枚举
/// </summary>
public enum StackPanelOrientation
{
    Vertical,
    Horizontal,
    FreeLayout,
    GridLayout
}


/// <summary>
/// 对齐类型
/// </summary>
public enum AlignmentType
{
    Left,
    HorizontalCenter,
    Right,
    Top,
    VerticalCenter,
    Bottom,
    HorizontalSpacing,
    VerticalSpacing,
    SameWidth,
    SameHeight,
    SameSize
}

/// <summary>
/// 对齐请求事件参数
/// </summary>
public class AlignRequestedEventArgs : EventArgs
{
    public AlignmentType Alignment { get; }

    public AlignRequestedEventArgs(AlignmentType alignment)
    {
        Alignment = alignment;
    }
}