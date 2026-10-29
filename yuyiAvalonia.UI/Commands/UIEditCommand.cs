using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Models.Navigation;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Commands;

public class UIEditCommand : INavigationCommand
{
    public string Key => "UIEdit";
    private SystemContext? _context;

    public UIEditCommand(SystemContext? context)
    {
        _context = context;
    }

    public void Execute(NavigationCommandContext context)
    {
        UITabPagesViewModel viewModel = new UITabPagesViewModel();
        viewModel.Initialize(_context);
        context.Navigator?.SetCurrentPage(viewModel);
    }
}
