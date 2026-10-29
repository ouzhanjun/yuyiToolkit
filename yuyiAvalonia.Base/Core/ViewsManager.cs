using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using yuyiAvalonia.Base.Core.Abstractions.Navigation;

namespace yuyiAvalonia.Base.Core
{
    public class ViewsManager
    {
        private List<IViewNavigator> _viewNavigators = new List<IViewNavigator>();
         
        public List<IViewNavigator> ViewNavigators
        {
            get
            {
                return _viewNavigators;
            }
        }

        public ViewsManager()
        {
        }
    }
}
