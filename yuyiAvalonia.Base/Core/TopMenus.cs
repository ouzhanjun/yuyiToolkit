using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using yuyiAvalonia.Base.Commands;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Views;

namespace yuyiAvalonia.Base.Core
{
    public class TopMenus
    {
        private LocalizationService _localization;
        private List<MenuItem> _menuItems;
        private MenuItem _menuFile;
        private MenuItem _menuTool;
        private MenuItem _menuView;
        private MenuItem _menuHelp;
        private MenuItem _menuOpenFile;
        private MenuItem _menuOpenDirectory;
        private MenuItem _menuSaveAs;
        private MenuItem _menuOptions;
        public event EventHandler<object> OnOpenFile;
        public event EventHandler<object> OnOpenDirectory;

        public TopMenus(LocalizationService localization)
        {
            _localization = localization;

            _menuItems = new List<MenuItem>();

            FileMenuBind();
            ToolMenuBind();
            HelpMenuBind();
        }

        public List<MenuItem> MenuItems
        {
            get { return _menuItems; }
        }

        public MenuItem MenuFile { get => _menuFile; }
        public MenuItem MenuTool { get => _menuTool; }
        public MenuItem MenuView { get => _menuView; }
        public MenuItem MenuHelp { get => _menuHelp; }
        public MenuItem MenuOpenFile { get => _menuOpenFile; }
        public MenuItem MenuOpenDirectory { get => _menuOpenDirectory; }
        public MenuItem MenuSaveAs { get => _menuSaveAs; }
        public MenuItem MenuOptions { get => _menuOptions; }

        protected void FileMenuBind()
        {
            _menuFile = new MenuItem();
            MenuFile.Header = _localization["Menu.File"];
            MenuFile.Classes[0] = "top";
            MenuFile.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                AppHelper.AppExit();
            });

            _menuItems.Add(MenuFile);

            MenuItem menuOpen = new MenuItem();
            menuOpen.Header = _localization["Menu.Open"];
            menuOpen.Icon = new Image
            {
                Source = new Bitmap("Assets/images/file.png"),
                Width = 16,
                Height = 16
            };

            MenuFile.Items.Add(menuOpen);

            _menuOpenFile = new MenuItem();
            _menuOpenFile.Header = _localization["Menu.OpenFile"];
            _menuOpenFile.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                string[] files = AppHelper.OpenFile().Result;
                if (OnOpenFile != null)
                {
                    OnOpenFile(_menuOpenFile, files);
                }
            });

            menuOpen.Items.Add(_menuOpenFile);

            _menuOpenDirectory = new MenuItem();
            _menuOpenDirectory.Header = _localization["Menu.OpenDirectory"];
            _menuOpenDirectory.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                string[] folders = AppHelper.OpenDirectory().Result;
                if (OnOpenDirectory != null)
                {
                    OnOpenDirectory(_menuOpenDirectory, folders);
                }
            });

            menuOpen.Items.Add(_menuOpenDirectory);

            _menuSaveAs = new MenuItem();
            _menuSaveAs.Header = _localization["Menu.FileSaveAs"];
            _menuSaveAs.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                AppHelper.SaveAsFile();
            });

            MenuFile.Items.Add(_menuSaveAs);

            MenuItem menuSeperator = new MenuItem();
            menuSeperator.Header = "-";
            MenuFile.Items.Add(menuSeperator);

            MenuItem menuExit = new MenuItem();
            menuExit.Header = _localization["Menu.Exit"];
            menuExit.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                AppHelper.AppExit();
            });

            MenuFile.Items.Add(menuExit);
        }

        protected void ToolMenuBind()
        {
            _menuTool = new MenuItem();
            MenuTool.Header = _localization["Menu.Tools"];
            MenuTool.Classes[0] = "top";
            _menuItems.Add(MenuTool);

            MenuItem menuTheme = new MenuItem();
            menuTheme.Header = _localization["Menu.Theme"];
            MenuTool.Items.Add(menuTheme);

            MenuItem menuThemeDefault = new MenuItem();
            menuThemeDefault.Header = _localization["Menu.Theme.DefaultTheme"];
            menuThemeDefault.Command = new ThemeCommand();
            menuThemeDefault.CommandParameter = "Default";
            menuTheme.Items.Add(menuThemeDefault);

            MenuItem menuThemeDark = new MenuItem();
            menuThemeDark.Header = _localization["Menu.Theme.DarkTheme"];
            menuThemeDark.Command = new ThemeCommand();
            menuThemeDark.CommandParameter = "Dark";
            menuTheme.Items.Add(menuThemeDark);

            MenuItem menuSeperator = new MenuItem();
            menuSeperator.Header = "-";
            MenuTool.Items.Add(menuSeperator);

            _menuOptions = new MenuItem();
            _menuOptions.Header = _localization["Menu.Options"];
            _menuOptions.Command = new RelayCommand<MenuItem>(async (MenuItem? menuItem) =>
            {
                await OptionsDialog.ShowAsync();
            });
            MenuTool.Items.Add(_menuOptions);
        }

        protected void HelpMenuBind()
        {
            _menuHelp = new MenuItem();
            MenuHelp.Header = _localization["Menu.Help"];
            MenuHelp.Classes[0] = "top";

            _menuItems.Add(MenuHelp);

            MenuItem menuHelpDocument = new MenuItem();
            menuHelpDocument.Header = _localization["Menu.HelpDocument"];
            menuHelpDocument.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                AppHelper.OpenWithBrowser(".\\docs\\Help.md");
            });
            MenuHelp.Items.Add(menuHelpDocument);

            MenuItem menuAbout = new MenuItem();
            menuAbout.Header = _localization["Menu.About"];
            menuAbout.Command = new RelayCommand<MenuItem>((MenuItem? menuItem) =>
            {
                AppHelper.ShowAboutDialog();
            });

            MenuHelp.Items.Add(menuAbout);
        }
    }
}