namespace yuyiAvalonia.UI.ElementControls.Themes;

/// <summary>
/// 组件状态枚举
/// </summary>
public enum ComponentState
{
    /// <summary>
    /// 正常状态
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 悬停状态
    /// </summary>
    Hover = 1,

    /// <summary>
    /// 按下/激活状态
    /// </summary>
    Pressed = 2,

    /// <summary>
    /// 禁用状态
    /// </summary>
    Disabled = 3,

    /// <summary>
    /// 选中状态
    /// </summary>
    Selected = 4,

    /// <summary>
    /// 聚焦状态
    /// </summary>
    Focused = 5,

    /// <summary>
    /// 加载状态
    /// </summary>
    Loading = 6,

    /// <summary>
    /// 错误状态
    /// </summary>
    Error = 7,

    /// <summary>
    /// 成功状态
    /// </summary>
    Success = 8,

    /// <summary>
    /// 警告状态
    /// </summary>
    Warning = 9
}

/// <summary>
/// 组件状态扩展方法
/// </summary>
public static class ComponentStateExtensions
{
    /// <summary>
    /// 判断是否为可交互状态
    /// </summary>
    public static bool IsInteractive(this ComponentState state)
    {
        return state != ComponentState.Disabled && state != ComponentState.Loading;
    }

    /// <summary>
    /// 判断是否显示特殊样式
    /// </summary>
    public static bool HasVisualStyle(this ComponentState state)
    {
        return state is ComponentState.Hover
            or ComponentState.Pressed
            or ComponentState.Selected
            or ComponentState.Focused
            or ComponentState.Disabled
            or ComponentState.Error
            or ComponentState.Success
            or ComponentState.Warning;
    }

    /// <summary>
    /// 获取状态对应的样式优先级
    /// </summary>
    public static int GetPriority(this ComponentState state)
    {
        return state switch
        {
            ComponentState.Normal => 0,
            ComponentState.Hover => 1,
            ComponentState.Pressed => 2,
            ComponentState.Selected => 2,
            ComponentState.Focused => 2,
            ComponentState.Disabled => 3,
            ComponentState.Loading => 3,
            ComponentState.Error => 4,
            ComponentState.Success => 4,
            ComponentState.Warning => 4,
            _ => 0
        };
    }
}
