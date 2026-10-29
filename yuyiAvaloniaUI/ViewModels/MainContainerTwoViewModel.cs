using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using yuyiAvalonia.Base.Commands;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;
using yuyiAvalonia.Base.ViewModels.Pages;

namespace yuyiAvaloniaUI.ViewModels
{
    public partial class MainContainerTwoViewModel : PageViewModelBase
    {
        private readonly IViewNavigator _viewNavigator;
        private readonly SystemContext _context;
        private readonly Debounce _debounce;
        private readonly LocalizationService _localizationService;
        private IDictionary<string, ObservableCollection<MenuItemViewModel>>? _originalSidebarMenus;

        [ObservableProperty]
        private PageViewModelBase? _currentPage;

        /// <summary>
        /// 左上侧工具栏
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MenuItemViewModel>? _leftTopToolBar;

        /// <summary>
        /// 左下侧工具栏
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<MenuItemViewModel>? _leftBottomToolBar;

        public MainContainerTwoViewModel( DefaultViewNavigator viewNavigator)
        {
            _context = GlobalProvider.SystemContext;
            _localizationService = _context.Localization;
            _debounce = new Debounce(500);
            _viewNavigator = viewNavigator;
            _viewNavigator.PropertyChanged += _viewNavigator_PropertyChanged;
            _context.PropertyChanged += _context_PropertyChanged;
            LoadToolbarMenus();
        }

        private void _context_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            LoadToolbarMenus();
        }

        private void _viewNavigator_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IViewNavigator.CurrentPage))
            {
                CurrentPage = _viewNavigator.CurrentPage;
            }
        }

        private void LoadToolbarMenus()
        {
            var leftTopToolbar = _context.LeftTopToolItems.Select(ConvertMenuItemToViewModel());
            LeftTopToolBar = new ObservableCollection<MenuItemViewModel>(leftTopToolbar);

            var leftBottomToolbar = _context.LeftBottomToolItems.Select(ConvertMenuItemToViewModel());
            LeftBottomToolBar = new ObservableCollection<MenuItemViewModel>(leftBottomToolbar);
        }

        private System.Func<CustomMenuItem, MenuItemViewModel> ConvertMenuItemToViewModel()
        {
            return x =>
            {
                MenuItemViewModel menuItem = new MenuItemViewModel(x.LocaleKey)
                {
                    Text = !string.IsNullOrEmpty(x.LocaleKey) ? _localizationService[x.LocaleKey] : x.Key,
                    Key = x.Key,
                    IsActive = x.IsActive,
                    Icon = string.IsNullOrWhiteSpace(x.Icon) ? null : StreamGeometry.Parse(x.Icon),
                    Group = _localizationService[x.GroupLocaleKey],
                    CommandType = x.CommandType,
                    LocaleKey = x.LocaleKey,
                    CommandParameter = x.CommandParameter,
                    Items = x.Items?.Select(ConvertMenuItemToViewModel()).ToList(),
                    MenuClickCommand = GetRelayCommand(x.CommandType),
                };

                if (menuItem.IsActive)
                {
                    //LoadSidebarMenus(menuItem);
                }

                return menuItem;
            };
        }

        private IRelayCommand? GetRelayCommand(string? commandType)
        {
            if (!Enum.TryParse<MenuClickCommandType>(commandType, out MenuClickCommandType cmdType))
            {
                return null;
            }

            return cmdType switch
            {
                MenuClickCommandType.Active => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                    SetActiveMenu(menuItem, () => LeftTopToolBar.FirstOrDefault(x => x.IsActive));
                    _viewNavigator.NavigateTo(menuItem?.CommandParameter);
                    //LoadSidebarMenus(menuItem);
                }),
                MenuClickCommandType.Navigate => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                    //SetActiveMenu(menuItem, () => LeftSidebarMenus?.Values.SelectMany(x => x)?.FirstOrDefault(x => x.IsActive));
                    //if (menuItem?.CommandParameter is null)
                    //{
                    //    return;
                    //}
                    _viewNavigator.NavigateTo(menuItem?.CommandParameter);
                }),
                MenuClickCommandType.SwitchTheme => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                    new ThemeCommand().Execute(menuItem?.CommandParameter);
                }),
                MenuClickCommandType.SwitchLanguage => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                    string? languageCode = menuItem?.CommandParameter as string;
                    if (!string.IsNullOrEmpty(languageCode))
                    {
                        _localizationService.SetLanguage(languageCode);
                        LoadToolbarMenus();
                    }
                }),
                MenuClickCommandType.Link => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                    string? url = menuItem?.CommandParameter;
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }),
                MenuClickCommandType.PopupMenu => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                }),
                _ => null
            };
        }

        public void SetActiveMenu(MenuItemViewModel? menuItem, Func<MenuItemViewModel> func)
        {
            if (menuItem is null)
            {
                return;
            }

            var beforeActived = func();
            if (beforeActived != null)
            {
                beforeActived.IsActive = false;
            }

            menuItem.IsActive = true;

        }

        public override void UpdateLanguageTexts()
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {

        }

        public override void Initialize(SystemContext? context)
        {
            throw new NotImplementedException();
        }
    }
}
