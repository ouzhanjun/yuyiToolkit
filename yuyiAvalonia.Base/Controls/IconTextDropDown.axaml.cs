using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Collections.Generic;

namespace yuyiAvalonia.Base.Controls;

public partial class IconTextDropDown : UserControl
{
    public static readonly StyledProperty<IList<IconTextDropDownItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<IconTextDropDown, IList<IconTextDropDownItem>?>(
            nameof(ItemsSource));

    public static readonly StyledProperty<IconTextDropDownItem?> SelectedItemProperty =
        AvaloniaProperty.Register<IconTextDropDown, IconTextDropDownItem?>(
            nameof(SelectedItem),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<object?> SelectedValueProperty =
        AvaloniaProperty.Register<IconTextDropDown, object?>(
            nameof(SelectedValue),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<IconTextDropDown, bool>(nameof(IsOpen));

    public static readonly StyledProperty<string?> ToolTipTextProperty =
        AvaloniaProperty.Register<IconTextDropDown, string?>(nameof(ToolTipText));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<IconTextDropDown, CornerRadius>(
            nameof(CornerRadius),
            new CornerRadius(4));

    public IList<IconTextDropDownItem>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IconTextDropDownItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string? ToolTipText
    {
        get => GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public event EventHandler<IconTextDropDownItem?>? SelectionChanged;

    public IconTextDropDown()
    {
        InitializeComponent();
        PART_PropertyBorder.Attach(this);

        SelectedItemProperty.Changed.AddClassHandler<IconTextDropDown>((control, e) =>
        {
            if (e.NewValue is IconTextDropDownItem item && !Equals(control.SelectedValue, item.Value))
                control.SelectedValue = item.Value;
            else if (e.NewValue is null && control.SelectedValue is not null)
                control.SelectedValue = null;

            if (!Equals(e.OldValue, e.NewValue))
                control.SelectionChanged?.Invoke(control, control.SelectedItem);
        });

        ItemsSourceProperty.Changed.AddClassHandler<IconTextDropDown>((control, _) =>
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

        SelectedValueProperty.Changed.AddClassHandler<IconTextDropDown>((control, e) =>
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

public sealed class IconTextDropDownItem
{
    public IconTextDropDownItem(
        object value,
        string text,
        StreamGeometry? icon = null,
        string? description = null)
    {
        Value = value;
        Text = text;
        Icon = icon;
        Description = description;
    }

    public object Value { get; }

    public string Text { get; }

    public StreamGeometry? Icon { get; }

    public string? Description { get; }

    public bool HasIcon => Icon != null;

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
}
