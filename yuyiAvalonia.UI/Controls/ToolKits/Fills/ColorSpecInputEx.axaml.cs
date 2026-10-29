using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace yuyiAvalonia.UI.Controls;

public partial class ColorSpecUpDownEx : UserControl
{

    #region FillColor
    public static readonly StyledProperty<string> FillColorProperty =
        AvaloniaProperty.Register<ColorSpecUpDownEx, string>(
            nameof(FillColor),
            defaultValue: "#FFFFFF",
            defaultBindingMode: BindingMode.TwoWay);

    public string FillColor
    {
        get => GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }
    #endregion

    #region Opacity
    public static readonly StyledProperty<double> FillOpacityProperty =
        AvaloniaProperty.Register<ColorSpecUpDownEx, double>(
            nameof(FillOpacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double FillOpacity
    {
        get => GetValue(FillOpacityProperty);
        set => SetValue(FillOpacityProperty, value);
    }

    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<ColorSpecUpDownEx, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }

    #endregion


    public ColorSpecUpDownEx()
    {
        InitializeComponent();

        PART_PropertyBorder.Attach(this);

        FillOpacityProperty.Changed.AddClassHandler<ColorSpecUpDownEx>((control, args) =>
        {
            if (args.NewValue is double opacity)
            {
                var newPercent = opacity * 100;
                if (Math.Abs(newPercent - control.OpacityPercent) > 0.01)
                {
                    control.SetValue(OpacityPercentProperty, newPercent);
                }
            }
        });

        OpacityPercentProperty.Changed.AddClassHandler<ColorSpecUpDownEx>((control, args) =>
        {
            if (args.NewValue is double percent)
            {
                var newOpacity = percent / 100.0;
                if (Math.Abs(newOpacity - control.FillOpacity) > 0.001)
                {
                    control.SetValue(FillOpacityProperty, newOpacity);
                }
            }
        });
    }
}
