using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Navigation;
using yuyiAvalonia.Java.ViewModels;

namespace yuyiAvalonia.Java.Commands
{
    public class JavaCommand : INavigationCommand
    {
        public string Key => "Java";
        private SystemContext? _systemContext;

        public JavaCommand(SystemContext? context)
        {
            _systemContext = context;
        }

        public void Execute(NavigationCommandContext context)
        {
            JavaViewModel viewModel = new JavaViewModel();
            viewModel.Initialize(_systemContext);
            // 调用视图导航器设置当前页面
            context.Navigator?.SetCurrentPage(viewModel);
        }
    }
}
