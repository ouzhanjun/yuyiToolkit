using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace yuyiAvalonia.Base.ViewModels.Menu;

/// <summary>
/// 菜单项视图模型
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 按钮文本
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// 本地化键
    /// </summary>
    public string LocaleKey { get; set; } = string.Empty;

    /// <summary>
    /// 图标
    /// </summary>
    public StreamGeometry? Icon { get; set; }

    /// <summary>
    /// 子菜单
    /// </summary>
    public ICollection<MenuItemViewModel>? Items { get; set; }

    /// <summary>
    /// 菜单是否选中
    /// </summary>
    [ObservableProperty]
    private bool _isActive;

    /// <summary>
    /// 菜单点击命令
    /// </summary>
    public ICommand? MenuClickCommand { get; set; }

    /// <summary>
    /// 命令参数
    /// </summary>
    public string? CommandType { get; set; }

    /// <summary>
    /// 命令参数
    /// </summary>
    public string? CommandParameter { get; set; }

    /// <summary>
    /// 菜单分组
    /// </summary>
    public string? Group { get; set; } = string.Empty;

    /// <summary>
    /// 是否为分隔线
    /// </summary>
    public bool IsSeparator { get; set; }

    /// <summary>
    /// 是否为复杂控件（如缩放选择器）
    /// </summary>
    public bool IsComplex { get; set; }

    /// <summary>
    /// 复杂控件内容（如缩放百分比按钮）
    /// </summary>
    public Control? ComplexContent { get; set; }

    public MenuItemViewModel(string localeKey)
    {
        LocaleKey = localeKey;
    }

    public void AddItem(MenuItemViewModel item)
    {
        Items ??= [];
        Items?.Add(item);
    }
}