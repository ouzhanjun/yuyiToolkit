using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class StrokeStyleDropDown : UserControl
{
    /// <summary>
    /// 五个内置描边样式（实线 / 虚线 / 点线 / 点划线 / 双点划线）。
    /// 任意时刻可通过 <see cref="ItemsSource"/> 覆盖。
    /// </summary>
    public static readonly IList<StrokeStyleDropDownItem> DefaultItems = new List<StrokeStyleDropDownItem>
    {
        new(StrokeStyleType.Solid,      "",                "无虚线间隔 (Solid)"),
        new(StrokeStyleType.Dashed,     "4,2",             "4-2 虚线 (4,2)"),
        new(StrokeStyleType.Dot,        "1,3",             "1-3 点线 (1,3)"),
        new(StrokeStyleType.DashDot,    "6,2,1,2",         "6-2-1-2 点划线 (6,2,1,2)"),
        new(StrokeStyleType.DashDotDot, "6,2,1,2,1,2",     "6-2-1-2-1-2 双点划线 (6,2,1,2,1,2)")
    };

    public static readonly StyledProperty<IList<StrokeStyleDropDownItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<StrokeStyleDropDown, IList<StrokeStyleDropDownItem>?>(
            nameof(ItemsSource));

    public static readonly StyledProperty<StrokeStyleDropDownItem?> SelectedItemProperty =
        AvaloniaProperty.Register<StrokeStyleDropDown, StrokeStyleDropDownItem?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<StrokeStyleType?> SelectedValueProperty =
        AvaloniaProperty.Register<StrokeStyleDropDown, StrokeStyleType?>(
            nameof(SelectedValue),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<StrokeStyleDropDown, bool>(nameof(IsOpen));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<StrokeStyleDropDown, CornerRadius>(
            nameof(CornerRadius),
            new CornerRadius(4));

    public IList<StrokeStyleDropDownItem>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public StrokeStyleDropDownItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public StrokeStyleType? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public event EventHandler<StrokeStyleDropDownItem?>? SelectionChanged;

    public StrokeStyleDropDown()
    {
        InitializeComponent();
        PART_PropertyBorder.Attach(this);

        ItemsSource ??= DefaultItems;

        SelectedItemProperty.Changed.AddClassHandler<StrokeStyleDropDown>((control, e) =>
        {
            if (e.NewValue is StrokeStyleDropDownItem item && !Equals(control.SelectedValue, item.Value))
                control.SelectedValue = item.Value;
            else if (e.NewValue is null && control.SelectedValue is not null)
                control.SelectedValue = null;

            if (!Equals(e.OldValue, e.NewValue))
                control.SelectionChanged?.Invoke(control, control.SelectedItem);
        });

        ItemsSourceProperty.Changed.AddClassHandler<StrokeStyleDropDown>((control, _) =>
        {
            if (control.ItemsSource == null)
            {
                control.SelectedItem = null;
                return;
            }

            var item = control.ItemsSource.FirstOrDefault(candidate => Equals(candidate.Value, control.SelectedValue));
            if (!Equals(control.SelectedItem, item))
                control.SelectedItem = item;
        });

        SelectedValueProperty.Changed.AddClassHandler<StrokeStyleDropDown>((control, e) =>
        {
            if (control.ItemsSource == null)
                return;

            var item = control.ItemsSource.FirstOrDefault(candidate => Equals(candidate.Value, e.NewValue));
            if (!Equals(control.SelectedItem, item))
                control.SelectedItem = item;
        });

        PART_Popup.Opened += (_, _) => PART_ItemsListBox.Focus();
    }

    private void Trigger_Click(object? sender, RoutedEventArgs e)
    {
        IsOpen = !IsOpen;
        e.Handled = true;
    }

    private void Trigger_KeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            case Key.Space:
            case Key.Down:
                IsOpen = true;
                PART_ItemsListBox.Focus();
                e.Handled = true;
                break;
            case Key.Escape when IsOpen:
                IsOpen = false;
                e.Handled = true;
                break;
        }
    }

    private void ItemsListBox_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (SelectedItem == null)
            return;

        ClosePopup();
        e.Handled = true;
    }

    private void ItemsListBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && SelectedItem != null)
        {
            ClosePopup();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            ClosePopup();
            e.Handled = true;
        }
    }

    private void ClosePopup()
    {
        IsOpen = false;
        PART_Trigger.Focus();
    }
}
