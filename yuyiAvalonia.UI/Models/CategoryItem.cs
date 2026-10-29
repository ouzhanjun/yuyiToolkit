using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.UI.Models
{

    /// <summary>
    /// 分类项
    /// </summary>
    public partial class CategoryItem : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _icon = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _count;

        [ObservableProperty]
        private bool _isExpanded = true;
    }
}
