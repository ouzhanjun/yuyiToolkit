using Avalonia; 
using Avalonia.Controls; 
using Avalonia.Input; 
using Avalonia.Media; 
using System.Collections.ObjectModel; 
using System.ComponentModel;

namespace yuyiAvalonia.Base.Controls;

public partial class ColorSelectorControl : UserControl
{
    // 1. 定义依赖属性 - SelectedColor
    public static readonly StyledProperty<Color> SelectedColorProperty = 
        AvaloniaProperty.Register<ColorSelectorControl, Color>(
            nameof(SelectedColor), 
            Colors.Black);

    // 2. CLR 包装属性 - SelectedColor
    public Color SelectedColor
    { 
        get => GetValue(SelectedColorProperty); 
        set => SetValue(SelectedColorProperty, value); 
    }

    // 1. 定义依赖属性 - ColorItems
    public static readonly StyledProperty<ObservableCollection<ColorItem>> ColorItemsProperty = 
        AvaloniaProperty.Register<ColorSelectorControl, ObservableCollection<ColorItem>>(
            nameof(ColorItems));

    // 2. CLR 包装属性 - ColorItems
    public ObservableCollection<ColorItem> ColorItems
    { 
        get => GetValue(ColorItemsProperty); 
        set => SetValue(ColorItemsProperty, value); 
    }
    
    public event EventHandler<Color>? ColorSelected;
    
    public ColorSelectorControl()
    {
        InitializeComponent();
        
        // 初始化ColorItems
        SetValue(ColorItemsProperty, new ObservableCollection<ColorItem>());
        
        DataContext = this;
        InitializeColors();
        
        // 监听SelectedColor变化
        SelectedColorProperty.Changed.AddClassHandler<ColorSelectorControl>((control, e) =>
        {
            control.OnSelectedColorChanged();
        });
    }
    
    private void InitializeColors()
    {
        // 初始化颜色列表，按照用户提供的顺序
        var colorList = new List<Color>
        {
            Avalonia.Media.Colors.Yellow,       // 黄色
            Avalonia.Media.Colors.Orange,       // 橙色
            Avalonia.Media.Colors.OrangeRed,    // 橙红色
            Avalonia.Media.Colors.Red,          // 红色
            Avalonia.Media.Colors.Purple,       // 紫色
            Avalonia.Media.Colors.Violet,       // 紫罗兰色
            Avalonia.Media.Colors.Fuchsia,      // 紫红色
            Avalonia.Media.Colors.DeepPink,     // 深粉色
            Avalonia.Media.Colors.DodgerBlue,   // 道奇蓝
            Avalonia.Media.Colors.Blue,         // 蓝色
            Avalonia.Media.Colors.DarkBlue,     // 深蓝色
            Avalonia.Media.Colors.Cyan,          // 青色
            Avalonia.Media.Colors.Magenta,       // 品红色
            Avalonia.Media.Colors.Green,        // 绿色
            Avalonia.Media.Colors.DarkGreen,    // 深绿色
            Avalonia.Media.Colors.White,        // 白色
            Avalonia.Media.Colors.Gray,         // 灰色
            Avalonia.Media.Colors.Black         // 黑色
        };
        
        foreach (var color in colorList)
        {
            ColorItems.Add(new ColorItem { Color = color, IsSelected = color == SelectedColor });
        }
    }
    
    private void OnSelectedColorChanged()
    {
        // 更新选中状态
        foreach (var item in ColorItems)
        {
            item.IsSelected = item.Color == SelectedColor;
        }
    }
    
    private void ColorButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is Color color)
        {
            SelectedColor = color;
            
            // 触发颜色选择事件
            ColorSelected?.Invoke(this, color);
        }
    }
}

public class ColorItem : INotifyPropertyChanged
{
    private Color _color;
    private bool _isSelected;
    
    public Color Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(Brush));
            }
        }
    }
    
    public Brush Brush => new SolidColorBrush(_color);
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
