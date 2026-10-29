using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Collections.ObjectModel;

namespace yuyiAvalonia.Base.Controls;

public partial class FontComboBox : UserControl
{
    public static readonly StyledProperty<string> SelectedFontFamilyProperty =
        AvaloniaProperty.Register<FontComboBox, string>(
            nameof(SelectedFontFamily),
            defaultValue: "Microsoft YaHei",
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<FontComboBox, double>(nameof(Height), 28);

    public string SelectedFontFamily
    {
        get => GetValue(SelectedFontFamilyProperty);
        set => SetValue(SelectedFontFamilyProperty, value);
    }

    public new double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<FontComboBox, double>(nameof(FontSize), 10);

    public new double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    private ComboBox? _comboBox;
    private bool _isUpdating;

    public FontComboBox()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            _comboBox = this.FindControl<ComboBox>("PART_FontComboBox");
            UpdateSelectedItem();
        };

        SelectedFontFamilyProperty.Changed.AddClassHandler<FontComboBox>((control, e) =>
        {
            control.UpdateSelectedItem();
        });
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdating && _comboBox?.SelectedItem is FontItem item)
        {
            SelectedFontFamily = item.FontName;
        }
    }

    private void UpdateSelectedItem()
    {
        if (_comboBox != null && FontItem.Fonts.Count > 0)
        {
            _isUpdating = true;
            try
            {
                for (int i = 0; i < FontItem.Fonts.Count; i++)
                {
                    if (FontItem.Fonts[i].FontName == SelectedFontFamily)
                    {
                        _comboBox.SelectedIndex = i;
                        return;
                    }
                }
                if (_comboBox.SelectedIndex < 0 && FontItem.Fonts.Count > 0)
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

public class FontItem
{
    public static ObservableCollection<FontItem> Fonts { get; } = new();

    public string FontName { get; set; } = "";
    public string DisplayName { get; set; } = "";

    public FontItem(string fontName, string displayName)
    {
        FontName = fontName;
        DisplayName = displayName;
    }

    static FontItem()
    {
        // 中文字体
        Fonts.Add(new FontItem("Microsoft YaHei", "微软雅黑"));
        Fonts.Add(new FontItem("SimSun", "宋体"));
        Fonts.Add(new FontItem("SimHei", "黑体"));
        Fonts.Add(new FontItem("KaiTi", "楷体"));
        Fonts.Add(new FontItem("FangSong", "仿宋"));
        Fonts.Add(new FontItem("STSong", "华文宋体"));
        Fonts.Add(new FontItem("STKaiti", "华文楷体"));
        Fonts.Add(new FontItem("STZhongsong", "华文中宋"));
        Fonts.Add(new FontItem("STXihei", "华文细黑"));
        Fonts.Add(new FontItem("STHeiti", "华文黑体"));

        // 英文字体
        Fonts.Add(new FontItem("Arial", "Arial"));
        Fonts.Add(new FontItem("Times New Roman", "Times New Roman"));
        Fonts.Add(new FontItem("Calibri", "Calibri"));
        Fonts.Add(new FontItem("Verdana", "Verdana"));
        Fonts.Add(new FontItem("Tahoma", "Tahoma"));
        Fonts.Add(new FontItem("Georgia", "Georgia"));
        Fonts.Add(new FontItem("Consolas", "Consolas"));
        Fonts.Add(new FontItem("Courier New", "Courier New"));
        Fonts.Add(new FontItem("Impact", "Impact"));
        Fonts.Add(new FontItem("Comic Sans MS", "Comic Sans MS"));
    }
}