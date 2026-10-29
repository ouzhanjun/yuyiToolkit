using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using yuyiAvalonia.Base.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace yuyiAvalonia.UI.Controls;

public partial class IdPropertyEditor : UserControl
{
    public static readonly StyledProperty<string> IdProperty =
        AvaloniaProperty.Register<IdPropertyEditor, string>(
            nameof(Id),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    public static readonly StyledProperty<bool> ShowIdLabelProperty =
        AvaloniaProperty.Register<IdPropertyEditor, bool>(
            nameof(ShowIdLabel),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Text), "0");


    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<StreamGeometry?> IconProperty =
        AvaloniaProperty.Register<NumericUpDownEx, StreamGeometry?>(nameof(Icon));

    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<IdPropertyEditor, double>(
            nameof(Opacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsVisibleProperty =
        AvaloniaProperty.Register<IdPropertyEditor, bool>(
            nameof(IsVisible),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsElementVisibleProperty =
        AvaloniaProperty.Register<IdPropertyEditor, bool>(
            nameof(IsElementVisible),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<IdPropertyEditor, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    public bool ShowIdLabel
    {
        get => GetValue(ShowIdLabelProperty);
        set => SetValue(ShowIdLabelProperty, value);
    }

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    public bool IsElementVisible
    {
        get => GetValue(IsElementVisibleProperty);
        set => SetValue(IsElementVisibleProperty, value);
    }

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public StreamGeometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IdPropertyEditor()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            UpdateHeaderDisplay();
        };

        IconProperty.Changed.AddClassHandler<IdPropertyEditor>((control, _) => control.UpdateHeaderDisplay());
        TitleProperty.Changed.AddClassHandler<IdPropertyEditor>((control, _) => control.UpdateHeaderDisplay());

        // FillOpacityProperty 变化时同步 OpacityPercent（用于外部绑定如 ElementProperty.FillOpacity）
        OpacityProperty.Changed.AddClassHandler<IdPropertyEditor>((control, args) =>
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
        OpacityPercentProperty.Changed.AddClassHandler<IdPropertyEditor>((control, args) =>
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

    private void UpdateHeaderDisplay()
    {
        if (PART_Title == null || PART_Icon == null || PART_GridContent == null)
            return;

        var hasIcon = Icon != null;
        var hasTitle = !string.IsNullOrWhiteSpace(Title);
        var hasHeader = hasIcon || hasTitle;

        PART_Icon.IsVisible = hasIcon;
        PART_Title.IsVisible = !hasIcon && hasTitle;

        if (hasIcon)
            PART_Icon.Data = Icon;
        else
            PART_Title.Text = Title;

        PART_GridContent.ColumnDefinitions[0].Width = hasHeader ? GridLength.Auto : new GridLength(0);
    }

}