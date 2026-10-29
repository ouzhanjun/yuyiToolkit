using System.ComponentModel;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Navigation;
using yuyiAvalonia.Base.ViewModels.Pages;

namespace yuyiAvalonia.Base.Core
{
    public class DefaultViewNavigator : IViewNavigator
    {
        /// <summary>
        /// 导航属性更改事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<string, INavigationCommand> _commands;

        private readonly LocalizationService _l;

        public DefaultViewNavigator(LocalizationService localizer, Dictionary<string, INavigationCommand> commands)
        {
            _l = localizer;
            _commands = commands;
        }

        /// <summary>
        /// 当前页视图模型
        /// </summary>
        public PageViewModelBase? CurrentPage { get; private set; }

        /// <summary>
        /// 导航到指定页面视图
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="NotSupportedException"></exception>
        public void NavigateTo(string? key)
        {
            BeforeNavigate(key);

            if (string.IsNullOrEmpty(key))
            {
                SetCurrentPage(null);
                return;
            }
            if (_commands.ContainsKey(key))
            {
                var command = _commands[key];
                command.Execute(new NavigationCommandContext(this));
            }

            AfterNavigate(key);
        }

        private void BeforeNavigate(string? key)
        {
            // 可以在这里添加导航前的逻辑，例如权限检查、日志记录等
            if (CurrentPage != null)
            {
                CurrentPage.Dispose();
            }
        }

        private void AfterNavigate(string? key)
        {
            // 可以在这里添加导航前的逻辑，例如权限检查、日志记录等
        }

        /// <summary>
        /// 设置当前页面视图
        /// </summary>
        /// <param name="page"></param>
        public void SetCurrentPage(PageViewModelBase page)
        {
            CurrentPage = page;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPage)));
        }
    }
}
