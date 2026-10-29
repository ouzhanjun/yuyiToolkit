using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Navigation;
using yuyiAvalonia.ImageEdit.ViewModels;

namespace yuyiAvalonia.ImageEdit.Commands
{
    public class ImageEditCommand : INavigationCommand
    {
        public string Key => "ImageEdit";
        private SystemContext? _systemContext;

        public ImageEditCommand(SystemContext? context)
        {
            _systemContext = context;
        }

        /// <summary>
        /// 执行导航命令的方法
        /// </summary>
        /// <param name="context"></param>
        public void Execute(NavigationCommandContext context)
        {
            ImageEditViewModel viewModel =  new ImageEditViewModel();
            viewModel.Initialize(_systemContext);
            // 调用视图导航器设置当前页面
            context.Navigator?.SetCurrentPage(viewModel);
        }
    }
}
