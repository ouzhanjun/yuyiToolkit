using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Collections.ObjectModel;

namespace yuyiAvalonia.Base.Controls;

public partial class LineSpacingComboBox : UserControl
{
    public static readonly StyledProperty<double> SelectedValueProperty =
        AvaloniaProperty.Register<LineSpacingComboBox, double>(
            nameof(SelectedValue),
            defaultValue: double.NaN,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<LineSpacingComboBox, double>(nameof(FontSize), 10);

    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<LineSpacingComboBox, double>(nameof(Height), 28);

    public double SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public new double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public new double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    private ComboBox? _comboBox;
    private bool _isUpdating;

    public LineSpacingComboBox()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            _comboBox = this.FindControl<ComboBox>("PART_ComboBox");
            UpdateSelectedItem();
        };

        SelectedValueProperty.Changed.AddClassHandler<LineSpacingComboBox>((control, e) =>
        {
            control.UpdateSelectedItem();
        });
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdating && _comboBox?.SelectedItem is LineSpacingItem item)
        {
            _isUpdating = true;
            SelectedValue = item.Value;
            _isUpdating = false;
        }
    }

    private void UpdateSelectedItem()
    {
        if (_comboBox != null && LineSpacingItem.Items.Count > 0)
        {
            _isUpdating = true;
            try
            {
                var val = SelectedValue;
                bool found = false;
                for (int i = 0; i < LineSpacingItem.Items.Count; i++)
                {
                    if (LineSpacingItem.Items[i].Value == val)
                    {
                        _comboBox.SelectedIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    _comboBox.SelectedIndex = 0;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}

public class LineSpacingItem
{
    public static ObservableCollection<LineSpacingItem> Items { get; } = new();

    public double Value { get; set; }
    public string DisplayName { get; set; } = "";

    public LineSpacingItem(double value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    static LineSpacingItem()
    {
        Items.Add(new LineSpacingItem(double.NaN, "默认"));
        Items.Add(new LineSpacingItem(0.8, "0.8"));
        Items.Add(new LineSpacingItem(0.9, "0.9"));
        Items.Add(new LineSpacingItem(1.0, "1.0"));
        Items.Add(new LineSpacingItem(1.1, "1.1"));
        Items.Add(new LineSpacingItem(1.2, "1.2"));
        Items.Add(new LineSpacingItem(1.3, "1.3"));
        Items.Add(new LineSpacingItem(1.5, "1.5"));
        Items.Add(new LineSpacingItem(1.8, "1.8"));
        Items.Add(new LineSpacingItem(2.0, "2.0"));
        Items.Add(new LineSpacingItem(2.5, "2.5"));
        Items.Add(new LineSpacingItem(3.0, "3.0"));
    }
}
