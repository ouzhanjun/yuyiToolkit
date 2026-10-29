using yuyiAvalonia.Base.Core.Abstractions.Navigation;

namespace yuyiAvalonia.Base.Models.Navigation;

public class NavigationCommandContext
{
    /// <summary>
    /// 视图导航器对象
    /// </summary>
    public IViewNavigator? Navigator { get; set; }

    public NavigationCommandContext(IViewNavigator? navigator)
    {
        Navigator = navigator;
    }
}

