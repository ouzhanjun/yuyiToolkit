using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;

namespace yuyiAvalonia.Base.Core
{
    public class TopToolbar
    {
        private LocalizationService _localization;
        private List<ChildToolbar> _childTools;


        public TopToolbar(LocalizationService localization)
        {
            _localization = localization;

            ChildTools = new List<ChildToolbar>();

            MenuItemViewModel item = new MenuItemViewModel("Home");
            item.Icon = Icon.Home;

            ChildToolbar childToolbar = new ChildToolbar("toolbar_1", [item, ]);
            ChildTools.Add(childToolbar);

        }

        public List<ChildToolbar> ChildTools { get => _childTools; private set => _childTools = value; }
    }
}
