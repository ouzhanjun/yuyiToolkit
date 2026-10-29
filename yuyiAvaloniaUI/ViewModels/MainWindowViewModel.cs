using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;
using yuyiAvalonia.Base.ViewModels.Pages;

namespace yuyiAvaloniaUI.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly LocalizationService _localizationService;

        private IDictionary<string, ObservableCollection<MenuItemViewModel>>? _originalSidebarMenus;

        private readonly Debounce _debounce;

        private readonly IViewNavigator _viewNavigator;

        private readonly SystemContext _context;

        #region 监控变量

        [ObservableProperty]
        private PageViewModelBase? _currentPage;

        [ObservableProperty]
        private string _windowTitle = string.Empty;

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

        /// <summary>
        /// 状态栏
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<StatusItem>? _statusItems;

        #endregion

        #region 构造函数

        public MainWindowViewModel(DefaultViewNavigator viewNavigator)
        {
            _context = GlobalProvider.SystemContext;
            _localizationService = _context.Localization;
            _debounce = new Debounce(500);
            _viewNavigator = viewNavigator;

            _localizationService.LanguageChanged += OnLanguageChanged;
            _viewNavigator.PropertyChanged += _viewNavigator_PropertyChanged;
            _context.PropertyChanged += _context_PropertyChanged;

            UpdateWindowTitle();
            LoadViewContent();

            CurrentPage = new MainContainerTwoViewModel(viewNavigator);
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
            if(e.PropertyName == "StatusItems" || string.IsNullOrEmpty(e.PropertyName))
            {
                if (_context.StatusItems != null)
                {
                    StatusItems = new ObservableCollection<StatusItem>(_context.StatusItems);
                }
            }
            LoadViewContent();
        }

        private void _viewNavigator_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IViewNavigator.CurrentPage))
            {
                //CurrentPage = _viewNavigator.CurrentPage;
            }
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            UpdateWindowTitle();
            UpdateLanguageTexts();
        }

        #endregion

        private void LoadViewContent()
        {
            TopToolbars = new ObservableCollection<ChildToolbar>(_context.TopToolbar.ChildTools);
            TopMenuItems = new ObservableCollection<Avalonia.Controls.MenuItem>(_context.TopMenus.MenuItems.ToArray());
        }

        private void UpdateWindowTitle()
        {
            WindowTitle = _localizationService["AppName"];
        }

        public override void UpdateLanguageTexts()
        {
            foreach (var menu in TopMenuItems)
            {
                UpdateLanguageText(menu);
            }

        }

        private void UpdateLanguageText(MenuItem? menu)
        {
            if (menu == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(menu.Name))
            {
                menu.Header = _localizationService[menu.Name];
            }

            if (menu.Items != null)
            {
                foreach (var subMenu in menu.Items)
                {
                    UpdateLanguageText((MenuItem?)subMenu);
                }
            }
        }
    }
}