using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.Base.ViewModels.Menu;

namespace yuyiAvalonia.Base.Models.Menu
{
    public class ChildToolbar : Dictionary<string, List<MenuItemViewModel>>
    {
        private string? _name;

        private List<MenuItemViewModel>? _menuItems;

        public ChildToolbar()
        {

        }

        public ChildToolbar(string name, List<MenuItemViewModel>? menuItems)
        {
            _name = name;
            _menuItems = menuItems;
        }

        public List<MenuItemViewModel>? MenuItems
        {
            get
            {
                if (_menuItems == null)
                {
                    _menuItems = new List<MenuItemViewModel>();
                }

                return _menuItems;
            }
        }

        public string? Name { get => _name; protected set => _name = value; }
    }
}