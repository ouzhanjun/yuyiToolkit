using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using yuyiAvalonia.Base.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class BorderPropertyEditor : UserControl
{
    #region BorderWidth Property
    public static readonly StyledProperty<double> BorderWidthProperty =
        AvaloniaProperty.Register<BorderPropertyEditor, double>(
            nameof(BorderWidth),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double BorderWidth
    {
        get => GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }
    #endregion

    #region BorderColor Property
    public static readonly StyledProperty<string> BorderColorProperty =
        AvaloniaProperty.Register<BorderPropertyEditor, string>(
            nameof(BorderColor),
            defaultValue: "#E5E5E5",
            defaultBindingMode: BindingMode.TwoWay);

    public string BorderColor
    {
        get => GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
    #endregion

    #region CornerRadius Property
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<BorderPropertyEditor, double>(
            nameof(CornerRadius),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion

    //private ColorInput? _borderColorInput;

    public BorderPropertyEditor()
    {
        InitializeComponent();

        //// 查找边框颜色输入控件并建立双向绑定
        //Loaded += (s, e) =>
        //{
        //    _borderColorInput = this.PART_BorderColorInput;

        //    if (_borderColorInput != null)
        //    {
        //        // 监听 ColorInput 变化
        //        _borderColorInput.TextChanged += (sender, args) =>
        //        {
        //            if (_borderColorInput != null)
        //            {
        //                BorderColor = _borderColorInput.Text;
        //            }
        //        };

        //        // 同步初始值
        //        if (!string.IsNullOrEmpty(BorderColor))
        //        {
        //            _borderColorInput.Text = BorderColor;
        //        }
        //    }
        //};

    }
}