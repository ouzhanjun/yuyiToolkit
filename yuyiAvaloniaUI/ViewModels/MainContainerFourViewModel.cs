using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class MainContainerFourViewModel : PageViewModelBase
    {
        private readonly IViewNavigator _viewNavigator;
        private readonly SystemContext _context;
        private readonly Debounce _debounce;
        private readonly LocalizationService _localizationService;
        private IDictionary<string, ObservableCollection<MenuItemViewModel>>? _originalSidebarMenus;
        
        [ObservableProperty]
        private PageViewModelBase? _currentPage;

        public string T_SearchText => _localizationService?["Search.Placeholder"];

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

        /// <summary>
        /// 设置子菜单集合
        /// </summary>
        [ObservableProperty]
        private IDictionary<string, ObservableCollection<MenuItemViewModel>>? _leftSidebarMenus;

        /// <summary>
        /// 顶部工具栏
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<ChildToolbar>? _topToolbars;

        /// <summary>
        /// 顶部工具栏
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Avalonia.Controls.MenuItem>? _topMenuItems;

        public MainContainerFourViewModel( DefaultViewNavigator viewNavigator)
        {
            _debounce = new Debounce(500);
            _context = GlobalProvider.SystemContext;
            _localizationService = _context.Localization;
            _viewNavigator = viewNavigator;
            _viewNavigator.PropertyChanged += _viewNavigator_PropertyChanged;
            _localizationService.LanguageChanged += OnLanguageChanged;
            _context.PropertyChanged += _context_PropertyChanged;

            LoadToolbarMenus();
        }

        private void _viewNavigator_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IViewNavigator.CurrentPage))
            {
                //CurrentPage = _viewNavigator.CurrentPage;
            }
        }
        
        private void _context_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TopToolbar" || string.IsNullOrEmpty(e.PropertyName))
            {
                // 重新加载工具栏内容
                if (_context.TopToolbar?.ChildTools != null)
                {
                    TopToolbars = new ObservableCollection<ChildToolbar>(_context.TopToolbar.ChildTools);
                }
            }
            LoadToolbarMenus();
        }


        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            UpdateLanguageTexts();
        }

        private void LoadToolbarMenus()
        {
            //先记录已经选中的项目
            MenuItemViewModel activeLeftTopButton = _leftTopToolBar?.FirstOrDefault(x => x.IsActive);
            MenuItemViewModel activeLeftBottomButton = _leftBottomToolBar?.FirstOrDefault(x => x.IsActive);

            var leftTopToolbar = _context.LeftTopToolItems.Select(ConvertMenuItemToViewModel());
            LeftTopToolBar = new ObservableCollection<MenuItemViewModel>(leftTopToolbar);

            var leftBottomToolbar = _context.LeftBottomToolItems.Select(ConvertMenuItemToViewModel());
            LeftBottomToolBar = new ObservableCollection<MenuItemViewModel>(leftBottomToolbar);

            TopToolbars = new ObservableCollection<ChildToolbar>(_context.TopToolbar.ChildTools);

            TopMenuItems = new ObservableCollection<Avalonia.Controls.MenuItem>(_context.TopMenus.MenuItems.ToArray());
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
                    LoadSidebarMenus(menuItem);
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
                    LoadSidebarMenus(menuItem);
                }),
                MenuClickCommandType.Navigate => new RelayCommand<MenuItemViewModel>((MenuItemViewModel? menuItem) =>
                {
                    SetActiveMenu(menuItem, () => LeftSidebarMenus?.Values.SelectMany(x => x)?.FirstOrDefault(x => x.IsActive));
                    if (menuItem?.CommandParameter is null)
                    {
                        return;
                    }
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

        public void OnSearchMenu(string? text)
        {
            _debounce.Trigger(() =>
            {
                var filterMenus = _originalSidebarMenus?.Values
                .SelectMany(x => x)
                .Where(x => x.Text.Contains(text ?? string.Empty,
                StringComparison.InvariantCultureIgnoreCase));

                LeftSidebarMenus = filterMenus?.GroupBy(x => x.Group).ToDictionary(
                    x => x.Key ?? "UNGROUPED",
                    x => new ObservableCollection<MenuItemViewModel>(x));
            });
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

        private void LoadSidebarMenus(MenuItemViewModel? menuItem)
        {
            var subItems = _context.MenuItems.FirstOrDefault(x => x.Key == menuItem?.Key)?.Items;
            _originalSidebarMenus = LeftSidebarMenus = subItems?.Select(ConvertMenuItemToViewModel()).GroupBy(x => x.Group).ToDictionary(
                 x => x.Key ?? "UNGROUPED",
                 x => new ObservableCollection<MenuItemViewModel>(x));
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
