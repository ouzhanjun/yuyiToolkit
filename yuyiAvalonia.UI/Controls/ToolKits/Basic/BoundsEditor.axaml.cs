using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;
using yuyiAvalonia.Base.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class BoundsEditor : UserControl
{
    public BoundsEditor()
    {
        InitializeComponent();
        //DataContext = this; // 关键！让控件绑定自己的属性

        // FillOpacityProperty 变化时同步 OpacityPercent（用于外部绑定如 ElementProperty.FillOpacity）
        OpacityProperty.Changed.AddClassHandler<BoundsEditor>((control, args) =>
        {
            if (args.NewValue is double opacity)
            {
                var newPercent = opacity * 100;
                // 避免循环更新
                if (Math.Abs(newPercent - control.OpacityPercent) > 0.01)
                {
                    control.SetValue(OpacityPercentProperty, newPercent);
                }
            }
        });

        // OpacityPercent 变化时同步 FillOpacity
        OpacityPercentProperty.Changed.AddClassHandler<BoundsEditor>((control, args) =>
        {
            if (args.NewValue is double percent)
            {
                var newOpacity = percent / 100.0;
                if (Math.Abs(newOpacity - control.Opacity) > 0.001)
                {
                    control.SetValue(OpacityProperty, newOpacity);
                }
            }
        });
    }

    // X 属性
    public static readonly StyledProperty<double> XProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(nameof(X),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double X
    {
        get => GetValue(XProperty);
        set => SetValue(XProperty, value);
    }

    // Y 属性
    public static readonly StyledProperty<double> YProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(nameof(Y),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Y
    {
        get => GetValue(YProperty);
        set => SetValue(YProperty, value);
    }

    // 宽度
    public static readonly StyledProperty<double> WidthProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(nameof(Width),
            defaultValue: 100,
            defaultBindingMode: BindingMode.TwoWay);

    public double Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    // 高度
    public static readonly StyledProperty<double> HeightProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(nameof(Height),
            defaultValue: 100,
            defaultBindingMode: BindingMode.TwoWay);

    public double Height
    {
        get => GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }
    
    // Rotation 属性
    public static readonly StyledProperty<double> RotationProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(nameof(Rotation),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(
            nameof(Opacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<BoundsEditor, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }

}