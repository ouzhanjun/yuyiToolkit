using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace yuyiAvalonia.UI.Models;

/// <summary>
/// 组件分组（包含名称、展开状态和组件列表）
/// </summary>
public partial class ComponentGroup : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private double _contentOpacity = 1.0;

    [ObservableProperty]
    private ObservableCollection<ComponentDefinition> _components = new();
}

/// <summary>
/// 用于 XAML 绑定的分组数据源（静态单例）
/// </summary>
public class ComponentGroupSource
{
    private static ComponentGroupSource? _instance;
    public static ComponentGroupSource Instance => _instance ??= new ComponentGroupSource();

    public ObservableCollection<ComponentGroup> Groups { get; } = new();
}
