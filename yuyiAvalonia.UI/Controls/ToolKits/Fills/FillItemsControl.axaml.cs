using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 填充项列表控件：从 <see cref="FillEditor"/> 抽离出来的列表项展示 + 添加/删除/上移/可见性交互。
/// 渲染 <see cref="Fills"/> 集合，每个列表项支持：
///   - 点击预览卡 → 抛出 <see cref="FillItemClicked"/>
///   - 可见性切换 → 通过 <see cref="FillItem"/> 自身的 <see cref="FillItem.IsEnabled"/>（TwoWay），并抛出 <see cref="VisibilityToggled"/>
///   - 上移按钮 → 抛出 <see cref="MoveUpRequested"/>
///   - 删除按钮 → 抛出 <see cref="RemoveRequested"/>
/// 父容器（通常是 <see cref="FillEditor"/>）订阅以上事件即可保持原有行为。
/// </summary>
public partial class FillItemsControl : UserControl
{
    #region StyledProperties

    public static readonly StyledProperty<ObservableCollection<FillItem>?> FillsProperty =
        AvaloniaProperty.Register<FillItemsControl, ObservableCollection<FillItem>?>(nameof(Fills));

    public ObservableCollection<FillItem>? Fills
    {
        get => GetValue(FillsProperty);
        set => SetValue(FillsProperty, value);
    }

    public static readonly StyledProperty<bool> HasFillsProperty =
        AvaloniaProperty.Register<FillItemsControl, bool>(nameof(HasFills));

    public bool HasFills
    {
        get => GetValue(HasFillsProperty);
        set => SetValue(HasFillsProperty, value);
    }

    #endregion

    /// <summary>用户点击某个列表项的预览卡（PointerPressed）。参数：被点击的 FillItem。</summary>
    public event Action<FillItem>? FillItemClicked;

    /// <summary>用户点击某个列表项的"删除"按钮。参数：待删除的 FillItem。</summary>
    public event Action<FillItem>? RemoveRequested;

    /// <summary>用户点击某个列表项的"上移"按钮。参数：待上移的 FillItem。</summary>
    public event Action<FillItem>? MoveUpRequested;

    /// <summary>用户点击某个列表项的"可见性"切换按钮。参数：被切换的 FillItem。</summary>
    public event Action<FillItem>? VisibilityToggled;

    public FillItemsControl()
    {
        InitializeComponent();

        HasFills = false;

        FillsProperty.Changed.AddClassHandler<FillItemsControl>((sender, e) =>
        {
            if (sender.Fills == null)
                return;

            sender.HasFills = sender.Fills.Count > 0;

            sender.Fills.CollectionChanged += (_, _) => sender.HasFills = sender.Fills.Count > 0;
        });
    }

    private void RemoveFill_Click(object? sender, RoutedEventArgs e)
    {
        // 优先从 Tag 取 FillItem，避免在最后一项容器被 ItemsControl 回收时
        // DataContext 短暂为 null 而导致 remove 事件丢失。
        if (sender is Control ctrl)
        {
            var fill = ctrl.Tag as FillItem
                       ?? (ctrl.DataContext as FillItem);
            if (fill != null)
                RemoveRequested?.Invoke(fill);
        }
    }

    private void MoveUpFill_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control ctrl)
        {
            var fill = ctrl.Tag as FillItem
                       ?? (ctrl.DataContext as FillItem);
            if (fill != null)
                MoveUpRequested?.Invoke(fill);
        }
    }

    private void FillItem_Click(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border)
        {
            var fill = border.Tag as FillItem
                       ?? (border.DataContext as FillItem);
            if (fill != null)
                FillItemClicked?.Invoke(fill);
        }
    }

    private void VisibilityToggle_Toggled(object? sender, RoutedEventArgs e)
    {
        if (sender is Control ctrl)
        {
            var fill = ctrl.Tag as FillItem
                       ?? (ctrl.DataContext as FillItem);
            if (fill != null)
                VisibilityToggled?.Invoke(fill);
        }
    }
}
