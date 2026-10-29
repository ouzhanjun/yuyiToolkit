using System.ComponentModel;
using yuyiAvalonia.Base.ViewModels.Pages;

namespace yuyiAvalonia.Base.Core.Abstractions.Navigation;

public interface IViewNavigator
{
    event PropertyChangedEventHandler? PropertyChanged;

    PageViewModelBase? CurrentPage { get; }

    void SetCurrentPage(PageViewModelBase page);

    void NavigateTo(string? key);
}

