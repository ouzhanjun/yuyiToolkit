using Avalonia.Controls;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.Base.Models.Menu;

namespace yuyiAvalonia.Base.Core
{
    /// <summary>
    /// 菜单配置上下文对象
    /// </summary>
    public class SystemContext
    {
        /// <summary>
        /// 导航属性更改事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name = Guid.NewGuid().ToString(); 

        /// <summary>
        /// 菜单项列表
        /// </summary>
        protected List<CustomMenuItem>? _menuItems;

        protected LocalizationService _localization;

        protected AppSettings _appSettings;

        protected TopMenus _topMenus;

        protected TopToolbar _topToolbar;

        protected List<CustomMenuItem>? _leftTopToolItems;

        protected List<CustomMenuItem>? _leftBottomToolItems;

        private ObservableCollection<StatusItem> statusItems = new ObservableCollection<StatusItem>();

        /// <summary>
        /// 传入配置文件中的菜单
        /// </summary>
        /// <param name="menuItems">菜单项</param>
        public SystemContext(IOptions<List<CustomMenuItem>> menuItems, LocalizationService localization, IOptions<AppSettings> appSettings)
        {
            _menuItems = menuItems.Value;
            _localization = localization;
            _appSettings = appSettings.Value;

            _topMenus = new TopMenus(localization);
            _topToolbar = new TopToolbar(localization);

        }

        public TopToolbar TopToolbar
        {
            get
            {
                return _topToolbar;
            }
        }

        public TopMenus TopMenus
        {
            get
            {
                return _topMenus;
            }
        }

        public List<CustomMenuItem> MenuItems
        {
            get
            {
                if (_menuItems == null)
                {
                    _menuItems = new List<CustomMenuItem>();
                }

                return _menuItems;
            }
        }

        public List<CustomMenuItem> LeftTopToolItems
        {
            get
            {
                _leftTopToolItems = MenuItems.Where(x => x.Group == "Toolbar").ToList();
                return _leftTopToolItems;
            }
        }

        public List<CustomMenuItem> LeftBottomToolItems
        {
            get
            {
                _leftBottomToolItems = MenuItems.Where(x => x.Group == "Settings").ToList();
                return _leftBottomToolItems;
            }
        }

        public AppSettings AppSettings
        {
            get
            {
                return _appSettings;
            }
        }

        public LocalizationService Localization
        {
            get
            {
                return _localization;
            }
        }

        public ObservableCollection<StatusItem> StatusItems { get => statusItems;  }


        public void AddStatusItem(StatusItem item)
        {
            bool isChanged = false;

            StatusItem? statusItem = StatusItems.FirstOrDefault(x => x.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

            if (statusItem == null)
            {
                StatusItems.Add(item);
                isChanged = true;

                NotifyPropertyChange(isChanged, "StatusItems");
            }
            else
            {
                statusItem.Text = item.Text;
                isChanged = true;
                NotifyPropertyChange(isChanged, "StatusItems");
            }
        }

        public void AddMenus(List<CustomMenuItem> menuItems)
        {
            bool isChanged = false;

            foreach (CustomMenuItem item in menuItems)
            {
                if (!MenuItems.Any(x => x.Key.Equals(item.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    MenuItems.Add(item);
                    isChanged = true;
                }
            }

            NotifyPropertyChange(isChanged, "MenuItems");
        }

        public void NotifyPropertyChange(bool isChanged, string eventArgs)
        {
            if (isChanged && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(eventArgs));
            }
        }

        public void AddLocalization(Dictionary<string, string> keyValues)
        {
            bool isChanged = false;

            foreach (var key in keyValues)
            {
                _localization.Add(key.Key, key.Value);
                isChanged = true;
            }

            if (isChanged && PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Localization"));
            }
        }

        public void AddTopMenus(MenuItem menuItem)
        {
            if (menuItem != null)
            {
                TopMenus.MenuItems.Add(menuItem);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TopMenus"));
                }
            }
        }
    }
}