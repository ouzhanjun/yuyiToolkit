using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Navigation;
using yuyiAvalonia.Software.ViewModels;

namespace yuyiAvalonia.Software.Commands;

/// <summary>
/// 软件下载器命令
/// </summary>
public class SoftwareCommand : INavigationCommand
{
    private readonly SystemContext? _systemContext;

    public SoftwareCommand(SystemContext? systemContext)
    {
        _systemContext = systemContext;
    }

    public string Key => "Software";

    /// <summary>
    /// 执行导航命令的方法
    /// </summary>
    /// <param name="context"></param>
    public void Execute(NavigationCommandContext context)
    {
        SoftwareViewModel viewModel = new SoftwareViewModel();
        viewModel.Initialize(_systemContext);
        // 调用视图导航器设置当前页面
        context.Navigator?.SetCurrentPage(viewModel);
    }
}