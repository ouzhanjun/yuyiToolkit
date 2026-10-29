using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using yuyiAvalonia.Base.Controls;

namespace yuyiAvalonia.UI.Controls;

public partial class ImageFillPropertyEditor : UserControl
{
    #region IsEnabled
    public static readonly StyledProperty<bool> IsEnabledProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, bool>(
            nameof(IsEnabled),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsEnabled
    {
        get => GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }
    #endregion

    #region FillName
    public static readonly StyledProperty<string> FillNameProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, string>(
            nameof(FillName),
            defaultValue: "",
            defaultBindingMode: BindingMode.TwoWay);

    public string FillName
    {
        get => GetValue(FillNameProperty);
        set => SetValue(FillNameProperty, value);
    }
    #endregion

    #region ImagePath
    public static readonly StyledProperty<string> ImagePathProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, string>(
            nameof(ImagePath),
            defaultValue: "",
            defaultBindingMode: BindingMode.TwoWay);

    public string ImagePath
    {
        get => GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }
    #endregion

    #region ImageStretch
    public static readonly StyledProperty<Stretch> ImageStretchProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, Stretch>(
            nameof(ImageStretch),
            defaultValue: Stretch.UniformToFill,
            defaultBindingMode: BindingMode.TwoWay);

    public Stretch ImageStretch
    {
        get => GetValue(ImageStretchProperty);
        set => SetValue(ImageStretchProperty, value);
    }
    #endregion

    #region ImageTileMode
    public static readonly StyledProperty<TileMode> ImageTileModeProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, TileMode>(
            nameof(ImageTileMode),
            defaultValue: TileMode.None,
            defaultBindingMode: BindingMode.TwoWay);

    public TileMode ImageTileMode
    {
        get => GetValue(ImageTileModeProperty);
        set => SetValue(ImageTileModeProperty, value);
    }
    #endregion

    #region ImageAlignmentX
    public static readonly StyledProperty<AlignmentX> ImageAlignmentXProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, AlignmentX>(
            nameof(ImageAlignmentX),
            defaultValue: AlignmentX.Left,
            defaultBindingMode: BindingMode.TwoWay);

    public AlignmentX ImageAlignmentX
    {
        get => GetValue(ImageAlignmentXProperty);
        set => SetValue(ImageAlignmentXProperty, value);
    }
    #endregion

    #region ImageAlignmentY
    public static readonly StyledProperty<AlignmentY> ImageAlignmentYProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, AlignmentY>(
            nameof(ImageAlignmentY),
            defaultValue: AlignmentY.Top,
            defaultBindingMode: BindingMode.TwoWay);

    public AlignmentY ImageAlignmentY
    {
        get => GetValue(ImageAlignmentYProperty);
        set => SetValue(ImageAlignmentYProperty, value);
    }
    #endregion

    #region Opacity
    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, double>(
            nameof(Opacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public new double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }


    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<ImageFillPropertyEditor, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }


    #endregion

    private EnumComboBox? _stretchComboBox;
    private EnumComboBox? _tileModeComboBox;
    private EnumComboBox? _alignmentXComboBox;
    private EnumComboBox? _alignmentYComboBox;

    public ImageFillPropertyEditor()
    {
        InitializeComponent();


        Loaded += (s, e) =>
        {
            _stretchComboBox = this.FindControl<EnumComboBox>("PART_StretchComboBox");
            _tileModeComboBox = this.FindControl<EnumComboBox>("PART_TileModeComboBox");
            _alignmentXComboBox = this.FindControl<EnumComboBox>("PART_AlignmentXComboBox");
            _alignmentYComboBox = this.FindControl<EnumComboBox>("PART_AlignmentYComboBox");

            SetupComboBox(_stretchComboBox, typeof(Stretch), () => ImageStretch, v => ImageStretch = (Stretch)v);
            SetupComboBox(_tileModeComboBox, typeof(TileMode), () => ImageTileMode, v => ImageTileMode = (TileMode)v);
            SetupComboBox(_alignmentXComboBox, typeof(AlignmentX), () => ImageAlignmentX, v => ImageAlignmentX = (AlignmentX)v);
            SetupComboBox(_alignmentYComboBox, typeof(AlignmentY), () => ImageAlignmentY, v => ImageAlignmentY = (AlignmentY)v);

        };

        OpacityProperty.Changed.AddClassHandler<ImageFillPropertyEditor>((control, args) =>
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

        OpacityPercentProperty.Changed.AddClassHandler<ImageFillPropertyEditor>((control, args) =>
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

    private void SetupComboBox(EnumComboBox? comboBox, Type enumType, Func<object> getter, Action<object> setter)
    {
        if (comboBox == null) return;
        comboBox.EnumType = enumType;
        comboBox.SelectedValue = getter().ToString();
        comboBox.SelectedValueChanged += (ctrl, args) =>
        {
            if (Enum.TryParse(enumType, args.NewValue, out var result))
            {
                setter(result);
            }
        };

        comboBox.PointerPressed += (ctrl, e) =>
        {
            e.Handled = true;   
        };
    }

    private async void SelectImage_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null)
            return;
        var popup = FindParentPopup(this);
        if (popup != null)
        {
            bool oldValue = popup.IsLightDismissEnabled;
            popup.IsLightDismissEnabled = false;

            try
            {
                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "选择图片",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("图片文件")
                        {
                            Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.gif", "*.bmp", "*.webp" }
                        },
                        new FilePickerFileType("所有文件")
                        {
                            Patterns = new[] { "*.*" }
                        }
                    }
                });

                if (files.Count >= 1)
                {
                    ImagePath = files[0].Path.LocalPath;
                }
            }
            finally
            {
                popup.IsLightDismissEnabled = oldValue;
            }
        }
    }

    // 自定义查找上级Popup方法
    private Popup? FindParentPopup(StyledElement control)
    {
        StyledElement? cur = control;

        while (cur != null)
        {
            if (cur is Popup popup)
                return popup as Popup;
            cur = cur.Parent;
        }

        return null;
    }

    private void ClearImage_Click(object? sender, RoutedEventArgs e)
    {
        ImagePath = "";
    }
}
