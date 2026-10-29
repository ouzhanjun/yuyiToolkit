using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using yuyiAvalonia.Base.Controls.Models;

namespace yuyiAvalonia.Base.Controls;

public class EnumValueChangedEventArgs : EventArgs
{
    public string OldValue { get; }
    public string NewValue { get; }
    public EnumValueChangedEventArgs(string oldValue, string newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public partial class EnumComboBox : UserControl
{
    public event EventHandler<EnumValueChangedEventArgs>? SelectedValueChanged;

    public static readonly StyledProperty<string> SelectedValueProperty =
        AvaloniaProperty.Register<EnumComboBox, string>(
            nameof(SelectedValue),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Type?> EnumTypeProperty =
        AvaloniaProperty.Register<EnumComboBox, Type?>(
            nameof(EnumType),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<ObservableCollection<EnumItem>?> EnumItemsProperty =
        AvaloniaProperty.Register<EnumComboBox, ObservableCollection<EnumItem>?>(nameof(EnumItems), null);

    public ObservableCollection<EnumItem>? EnumItems
    {
        get => GetValue(EnumItemsProperty);
        set => SetValue(EnumItemsProperty, value);
    }

    public string SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public Type? EnumType
    {
        get => GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }

    private ComboBox? _comboBox;

    public EnumComboBox()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            _comboBox = this.FindControl<ComboBox>("PART_EnumComboBox");
            RefreshItems();
        };

        EnumTypeProperty.Changed.AddClassHandler<EnumComboBox>((control, _) =>
        {
            control.RefreshItems();
        });

        EnumItemsProperty.Changed.AddClassHandler<EnumComboBox>((control, e) =>
        {
            _comboBox = this.FindControl<ComboBox>("PART_EnumComboBox");
            if (_comboBox != null && e.NewValue != null)
            {
                _comboBox.ItemsSource = e.NewValue as ObservableCollection<EnumItem>;
            }
        });

        SelectedValueProperty.Changed.AddClassHandler<EnumComboBox>((control, _) =>
        {
            control.UpdateSelectedItem();
        });
    }

    private void RefreshItems()
    {
        if (_comboBox == null)
            return;

        _comboBox.ItemsSource = null;

        if (EnumType == null || !EnumType.IsEnum)
        {
            return;
        }

        var items = new ObservableCollection<EnumItem>();
        foreach (var item in EnumTextRegistry.GetEnumItems(EnumType))
        {
            items.Add(item);
        }

        _comboBox.ItemsSource = items;
        UpdateSelectedItem();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_comboBox?.SelectedItem is EnumItem item)
        {
            var oldValue = SelectedValue;
            SelectedValue = item.Value;
            if (oldValue != item.Value)
            {
                SelectedValueChanged?.Invoke(this, new EnumValueChangedEventArgs(oldValue, item.Value));
            }
        }
    }

    private void UpdateSelectedItem()
    {
        if (_comboBox == null || _comboBox.ItemCount == 0)
            return;

        var items = _comboBox.Items.Cast<EnumItem>().ToList();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Value == SelectedValue)
            {
                _comboBox.SelectedIndex = i;
                return;
            }
        }
    }
}
