using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using yuyiAvalonia.Base.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class RotationEditor : UserControl
{
    // Rotation 属性
    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<RotationEditor, double>(nameof(Rotation),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    // Rotation 属性
    public static readonly StyledProperty<bool> IsShowTitleProperty =
        AvaloniaProperty.Register<RotationEditor, bool>(nameof(IsShowTitle),
            defaultValue: true,
            defaultBindingMode: BindingMode.OneWay);

    public bool IsShowTitle
    {
        get => GetValue(IsShowTitleProperty);
        set => SetValue(IsShowTitleProperty, value);
    }

    public RotationEditor()
    {
        InitializeComponent();

    }

}
