using yuyiAvalonia.Base.Models.Navigation;

namespace yuyiAvalonia.Base.Core.Abstractions.Navigation;

public interface INavigationCommand
{
    /// <summary>
    /// 页面的唯一标识符
    /// </summary>
    string Key { get; }

    /// <summary>
    /// 执行导航命令的方法
    /// </summary>
    /// <param name="context">执行导航命令时传递的上下文信息</param>
    void Execute(NavigationCommandContext context);
}