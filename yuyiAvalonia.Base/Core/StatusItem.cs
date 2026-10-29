using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.Base.Core
{
    public partial class StatusItem : ObservableObject
    {
        private string _name;

        public StatusItem(string name)
        {
            _name = name;
        }

        [ObservableProperty]
        private string _text = string.Empty;

        public string Name { get => _name;  }
    }
}
