using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using yuyiAvalonia.Base.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class TextPropertyEditor : UserControl
{
    #region Text Property
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<TextPropertyEditor, string>(
            nameof(Text),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    #endregion

    #region FontFamily Property
    public static readonly StyledProperty<string> FontFamilyProperty =
        AvaloniaProperty.Register<TextPropertyEditor, string>(
            nameof(FontFamily),
            defaultValue: "Microsoft YaHei",
            defaultBindingMode: BindingMode.TwoWay);

    public string FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }
    #endregion

    #region FontSize Property
    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<TextPropertyEditor, double>(
            nameof(FontSize),
            defaultValue: 14,
            defaultBindingMode: BindingMode.TwoWay);

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }
    #endregion

    #region ForeColor Property
    public static readonly StyledProperty<string> ForeColorProperty =
        AvaloniaProperty.Register<TextPropertyEditor, string>(
            nameof(ForeColor),
            defaultValue: "#000000",
            defaultBindingMode: BindingMode.TwoWay);

    public string ForeColor
    {
        get => GetValue(ForeColorProperty);
        set => SetValue(ForeColorProperty, value);
    }
    #endregion

    #region IsBold Property
    public static readonly StyledProperty<bool> IsBoldProperty =
        AvaloniaProperty.Register<TextPropertyEditor, bool>(
            nameof(IsBold),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsBold
    {
        get => GetValue(IsBoldProperty);
        set => SetValue(IsBoldProperty, value);
    }
    #endregion

    #region IsItalic Property
    public static readonly StyledProperty<bool> IsItalicProperty =
        AvaloniaProperty.Register<TextPropertyEditor, bool>(
            nameof(IsItalic),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsItalic
    {
        get => GetValue(IsItalicProperty);
        set => SetValue(IsItalicProperty, value);
    }
    #endregion

    #region IsUnderline Property
    public static readonly StyledProperty<bool> IsUnderlineProperty =
        AvaloniaProperty.Register<TextPropertyEditor, bool>(
            nameof(IsUnderline),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsUnderline
    {
        get => GetValue(IsUnderlineProperty);
        set => SetValue(IsUnderlineProperty, value);
    }
    #endregion

    #region IsStrikethrough Property
    public static readonly StyledProperty<bool> IsStrikethroughProperty =
        AvaloniaProperty.Register<TextPropertyEditor, bool>(
            nameof(IsStrikethrough),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsStrikethrough
    {
        get => GetValue(IsStrikethroughProperty);
        set => SetValue(IsStrikethroughProperty, value);
    }
    #endregion

    #region LineHeight Property
    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<TextPropertyEditor, double>(
            nameof(LineHeight),
            defaultValue: 1,
            defaultBindingMode: BindingMode.TwoWay);

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }
    #endregion

    #region LetterSpacing Property
    public static readonly StyledProperty<double> LetterSpacingProperty =
        AvaloniaProperty.Register<TextPropertyEditor, double>(
            nameof(LetterSpacing),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double LetterSpacing
    {
        get => GetValue(LetterSpacingProperty);
        set => SetValue(LetterSpacingProperty, value);
    }
    #endregion

    #region TextAlignment Property
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<TextPropertyEditor, TextAlignment>(
            nameof(TextAlignment),
            defaultValue: TextAlignment.Left,
            defaultBindingMode: BindingMode.TwoWay);

    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
    #endregion

    #region TextAlignmentIndex Property (for FlowShapeElement)
    public static readonly StyledProperty<int> TextAlignmentIndexProperty =
        AvaloniaProperty.Register<TextPropertyEditor, int>(
            nameof(TextAlignmentIndex),
            defaultValue: 1,
            defaultBindingMode: BindingMode.TwoWay);

    public int TextAlignmentIndex
    {
        get => GetValue(TextAlignmentIndexProperty);
        set => SetValue(TextAlignmentIndexProperty, value);
    }
    #endregion

    #region VerticalTextAlignmentIndex Property
    /// <summary>
    /// 垂直对齐索引（0=顶部，1=居中，2=底部）
    /// 绑定到 TextElement.VerticalContentAlignmentIndex
    /// </summary>
    public static readonly StyledProperty<int> VerticalTextAlignmentIndexProperty =
        AvaloniaProperty.Register<TextPropertyEditor, int>(
            nameof(VerticalTextAlignmentIndex),
            defaultValue: 1,
            defaultBindingMode: BindingMode.TwoWay);

    public int VerticalTextAlignmentIndex
    {
        get => GetValue(VerticalTextAlignmentIndexProperty);
        set => SetValue(VerticalTextAlignmentIndexProperty, value);
    }
    #endregion

    #region HasTextGradient Property
    public static readonly StyledProperty<bool> HasTextGradientProperty =
        AvaloniaProperty.Register<TextPropertyEditor, bool>(
            nameof(HasTextGradient),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool HasTextGradient
    {
        get => GetValue(HasTextGradientProperty);
        set => SetValue(HasTextGradientProperty, value);
    }
    #endregion

    #region TextGradientStartColor Property
    public static readonly StyledProperty<string> TextGradientStartColorProperty =
        AvaloniaProperty.Register<TextPropertyEditor, string>(
            nameof(TextGradientStartColor),
            defaultValue: "#00f5d4",
            defaultBindingMode: BindingMode.TwoWay);

    public string TextGradientStartColor
    {
        get => GetValue(TextGradientStartColorProperty);
        set => SetValue(TextGradientStartColorProperty, value);
    }
    #endregion

    #region TextGradientEndColor Property
    public static readonly StyledProperty<string> TextGradientEndColorProperty =
        AvaloniaProperty.Register<TextPropertyEditor, string>(
            nameof(TextGradientEndColor),
            defaultValue: "#7b2fff",
            defaultBindingMode: BindingMode.TwoWay);

    public string TextGradientEndColor
    {
        get => GetValue(TextGradientEndColorProperty);
        set => SetValue(TextGradientEndColorProperty, value);
    }
    #endregion

    #region TextGradientAngle Property
    public static readonly StyledProperty<double> TextGradientAngleProperty =
        AvaloniaProperty.Register<TextPropertyEditor, double>(
            nameof(TextGradientAngle),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double TextGradientAngle
    {
        get => GetValue(TextGradientAngleProperty);
        set => SetValue(TextGradientAngleProperty, value);
    }
    #endregion

    private AlignmentButtonGroup? _textAlignmentGroup;
    private AlignmentButtonGroup? _verticalTextAlignmentGroup;
    private bool _isUpdatingHorizontal;
    private bool _isUpdatingVertical;

    public TextPropertyEditor()
    {
        InitializeComponent();

        PART_TextAlignment.ItemsSource = new List<object>() {
                TextAlignment.Left,
                TextAlignment.Center,
                TextAlignment.Right
            };

        PART_TextVerticalAlignment.ItemsSource = new List<object>() {
                Avalonia.Layout.VerticalAlignment.Top,
                Avalonia.Layout.VerticalAlignment.Center,
                Avalonia.Layout.VerticalAlignment.Bottom
            };

        //TextAlignmentProperty.Changed.AddClassHandler<TextPropertyEditor>((control, _) =>
        //{
        //    control.SyncHorizontalButton();
        //});

        //TextAlignmentIndexProperty.Changed.AddClassHandler<TextPropertyEditor>((control, _) =>
        //{
        //    control.SyncTextAlignmentIndexButton();
        //});

        //VerticalTextAlignmentIndexProperty.Changed.AddClassHandler<TextPropertyEditor>((control, _) =>
        //{
        //    control.SyncVerticalButton();
        //});

        Loaded += (s, e) =>
        {
            //_textAlignmentGroup = this.FindControl<AlignmentButtonGroup>("PART_TextAlignmentGroup");
            //_verticalTextAlignmentGroup = this.FindControl<AlignmentButtonGroup>("PART_VerticalTextAlignmentGroup");

            //if (_textAlignmentGroup != null)
            //{
            //    _textAlignmentGroup.SelectionChanged += OnHorizontalAlignmentChanged;
            //    SyncHorizontalButton();
            //    SyncTextAlignmentIndexButton();
            //}

            //if (_verticalTextAlignmentGroup != null)
            //{
            //    _verticalTextAlignmentGroup.SelectionChanged += OnVerticalAlignmentChanged;
            //    SyncVerticalButton();
            //}
        };
    }

    private void OnHorizontalAlignmentChanged(object? sender, int index)
    {
        if (_isUpdatingHorizontal) return;
        _isUpdatingHorizontal = true;
        try
        {
            var alignment = index switch
            {
                0 => TextAlignment.Left,
                1 => TextAlignment.Center,
                2 => TextAlignment.Right,
                _ => TextAlignment.Left
            };
            TextAlignment = alignment;
            TextAlignmentIndex = index;
        }
        finally
        {
            _isUpdatingHorizontal = false;
        }
    }

    private void OnVerticalAlignmentChanged(object? sender, int index)
    {
        if (_isUpdatingVertical) return;
        _isUpdatingVertical = true;
        try
        {
            VerticalTextAlignmentIndex = index;
        }
        finally
        {
            _isUpdatingVertical = false;
        }
    }

    private void SyncHorizontalButton()
    {
        if (_textAlignmentGroup == null || _isUpdatingHorizontal) return;
        _textAlignmentGroup.SelectedIndex = TextAlignment switch
        {
            TextAlignment.Left => 0,
            TextAlignment.Center => 1,
            TextAlignment.Right => 2,
            _ => 0
        };
    }

    private void SyncTextAlignmentIndexButton()
    {
        if (_textAlignmentGroup == null || _isUpdatingHorizontal) return;
        _textAlignmentGroup.SelectedIndex = TextAlignmentIndex;
    }

    private void SyncVerticalButton()
    {
        if (_verticalTextAlignmentGroup == null || _isUpdatingVertical) return;
        _verticalTextAlignmentGroup.SelectedIndex = VerticalTextAlignmentIndex;
    }
}
