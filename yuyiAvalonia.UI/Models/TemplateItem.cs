using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.UI.Models
{

    /// <summary>
    /// 模板项
    /// </summary>
    public partial class TemplateItem : ObservableObject
    {
        [ObservableProperty]
        private string? _id;

        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _description;

        [ObservableProperty]
        private string? _icon;

        [ObservableProperty]
        private string? _backgroundColor;

        [ObservableProperty]
        private string? _category;

    }
}
