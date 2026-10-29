using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Navigation;
using yuyiAvalonia.ColorSelector.ViewModels;

namespace yuyiAvalonia.ColorSelector.Commands;

/// <summary>
/// 颜色选择器命令
/// </summary>
public class ColorSelectorCommand : INavigationCommand
{
    private readonly SystemContext? _systemContext;

    public ColorSelectorCommand(SystemContext? systemContext)
    {
        _systemContext = systemContext;
    }

    public string Key => "ColorSelector";

    /// <summary>
    /// 执行导航命令的方法
    /// </summary>
    /// <param name="context"></param>
    public void Execute(NavigationCommandContext context)
    {
        ColorSelectorViewModel viewModel = new ColorSelectorViewModel();
        viewModel.Initialize(_systemContext);
        // 调用视图导航器设置当前页面
        context.Navigator?.SetCurrentPage(viewModel);
    }
}
