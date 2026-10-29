using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using yuyiAvalonia.Base.Helpers;

namespace yuyiAvalonia.Base.Controls;

public partial class MarginTypeButtonGroup : UserControl
{
    // 四边相同图标
    public static readonly StyledProperty<StreamGeometry> UniformIconProperty =
        AvaloniaProperty.Register<MarginTypeButtonGroup, StreamGeometry>(nameof(UniformIcon), Icon.Padding);

    // 是否统一模式
    public static readonly StyledProperty<bool> IsUniformProperty =
        AvaloniaProperty.Register<MarginTypeButtonGroup, bool>(nameof(IsUniform), true,
            defaultBindingMode: BindingMode.TwoWay);

    public StreamGeometry UniformIcon
    {
        get => GetValue(UniformIconProperty);
        set => SetValue(UniformIconProperty, value);
    }

    public bool IsUniform
    {
        get => GetValue(IsUniformProperty);
        set => SetValue(IsUniformProperty, value);
    }

    private Border? _buttonUniform;

    IBrush? selectedBrush = null;
    IBrush? normalBrush = null;

    public MarginTypeButtonGroup()
    {
        InitializeComponent();

        _buttonUniform = this.FindControl<Border>("PART_ButtonUniform");

        normalBrush = Brushes.Transparent;

        IsUniformProperty.Changed.AddClassHandler<MarginTypeButtonGroup>((control, e) =>
        {
            control.UpdateButtonStyles();
        });

        Loaded += (s, e) =>
        {
            UpdateButtonStyles();
        };
    }

    private void UpdateButtonStyles()
    {
        object? hoverBrush = null;

        if (selectedBrush == null)
        {
            if (this.TryFindResource("ButtonBackgroundPointerOver", this.ActualThemeVariant, out hoverBrush))
            {
                selectedBrush = hoverBrush as IBrush;
            }
        }

        if (_buttonUniform != null)
        {
            _buttonUniform.Background = IsUniform ? selectedBrush : normalBrush;
        }
    }

    private void ButtonUniform_Click(object? sender, TappedEventArgs e)
    {
        IsUniform = !IsUniform;
        UpdateButtonStyles();
    }

}
