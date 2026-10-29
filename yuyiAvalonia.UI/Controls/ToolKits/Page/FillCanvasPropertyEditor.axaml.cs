using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using yuyiAvalonia.Base.Controls;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Controls;

public partial class FillCanvasPropertyEditor : UserControl
{
    #region FillType
    public static readonly StyledProperty<FillType> FillTypeProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, FillType>(
            nameof(FillType),
            defaultValue: FillType.Solid,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public FillType FillType
    {
        get => GetValue(FillTypeProperty);
        set => SetValue(FillTypeProperty, value);
    }
    #endregion

    #region BackgroundType (与 FillType 同步，用于 CanvasProperty)
    public static readonly StyledProperty<BackgroundType> BackgroundTypeProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, BackgroundType>(
            nameof(BackgroundType),
            defaultValue: BackgroundType.None,
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public BackgroundType BackgroundType
    {
        get => GetValue(BackgroundTypeProperty);
        set => SetValue(BackgroundTypeProperty, value);
    }
    #endregion

    #region FillColor
    public static readonly StyledProperty<string> FillColorProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, string>(
            nameof(FillColor),
            defaultValue: "#FFFFFF",
            defaultBindingMode: BindingMode.TwoWay);

    public string FillColor
    {
        get => GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }
    #endregion

    #region GradientStartColor
    public static readonly StyledProperty<string> GradientStartColorProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, string>(
            nameof(GradientStartColor),
            defaultValue: "#FFFFFF",
            defaultBindingMode: BindingMode.TwoWay);

    public string GradientStartColor
    {
        get => GetValue(GradientStartColorProperty);
        set => SetValue(GradientStartColorProperty, value);
    }
    #endregion

    #region GradientEndColor
    public static readonly StyledProperty<string> GradientEndColorProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, string>(
            nameof(GradientEndColor),
            defaultValue: "#e5e5e5",
            defaultBindingMode: BindingMode.TwoWay);

    public string GradientEndColor
    {
        get => GetValue(GradientEndColorProperty);
        set => SetValue(GradientEndColorProperty, value);
    }
    #endregion

    #region GradientAngle
    public static readonly StyledProperty<double> GradientAngleProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, double>(
            nameof(GradientAngle),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double GradientAngle
    {
        get => GetValue(GradientAngleProperty);
        set => SetValue(GradientAngleProperty, value);
    }
    #endregion

    #region BackgroundImage
    public static readonly StyledProperty<string> BackgroundImageProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, string>(
            nameof(BackgroundImage),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string BackgroundImage
    {
        get => GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }
    #endregion

    #region BackgroundImageStretch
    public static readonly StyledProperty<Stretch> BackgroundImageStretchProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, Stretch>(
            nameof(BackgroundImageStretch),
            defaultValue: Stretch.UniformToFill,
            defaultBindingMode: BindingMode.TwoWay);

    public Stretch BackgroundImageStretch
    {
        get => GetValue(BackgroundImageStretchProperty);
        set => SetValue(BackgroundImageStretchProperty, value);
    }
    #endregion

    #region BackgroundImageTileMode
    public static readonly StyledProperty<TileMode> BackgroundImageTileModeProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, TileMode>(
            nameof(BackgroundImageTileMode),
            defaultValue: TileMode.None,
            defaultBindingMode: BindingMode.TwoWay);

    public TileMode BackgroundImageTileMode
    {
        get => GetValue(BackgroundImageTileModeProperty);
        set => SetValue(BackgroundImageTileModeProperty, value);
    }
    #endregion

    #region BackgroundImageAlignmentX
    public static readonly StyledProperty<AlignmentX> BackgroundImageAlignmentXProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, AlignmentX>(
            nameof(BackgroundImageAlignmentX),
            defaultValue: AlignmentX.Left,
            defaultBindingMode: BindingMode.TwoWay);

    public AlignmentX BackgroundImageAlignmentX
    {
        get => GetValue(BackgroundImageAlignmentXProperty);
        set => SetValue(BackgroundImageAlignmentXProperty, value);
    }
    #endregion

    #region BackgroundImageAlignmentY
    public static readonly StyledProperty<AlignmentY> BackgroundImageAlignmentYProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, AlignmentY>(
            nameof(BackgroundImageAlignmentY),
            defaultValue: AlignmentY.Top,
            defaultBindingMode: BindingMode.TwoWay);

    public AlignmentY BackgroundImageAlignmentY
    {
        get => GetValue(BackgroundImageAlignmentYProperty);
        set => SetValue(BackgroundImageAlignmentYProperty, value);
    }
    #endregion

    #region BackgroundColor
    public static readonly StyledProperty<string> BackgroundColorProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, string>(
            nameof(BackgroundColor),
            defaultBindingMode: BindingMode.TwoWay);

    public string BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }
    #endregion

    #region Opacity
    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, double>(
            nameof(Opacity),
            defaultValue: 1,
            defaultBindingMode: BindingMode.TwoWay);

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }
    #endregion

    #region EnableBackgroundImage
    public static readonly StyledProperty<bool> EnableBackgroundImageProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, bool>(
            nameof(EnableBackgroundImage),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool EnableBackgroundImage
    {
        get => GetValue(EnableBackgroundImageProperty);
        set => SetValue(EnableBackgroundImageProperty, value);
    }
    #endregion

    #region IsGradientAnglePickerOpen
    public static readonly StyledProperty<bool> IsGradientAnglePickerOpenProperty =
        AvaloniaProperty.Register<FillCanvasPropertyEditor, bool>(
            nameof(IsGradientAnglePickerOpen),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsGradientAnglePickerOpen
    {
        get => GetValue(IsGradientAnglePickerOpenProperty);
        set => SetValue(IsGradientAnglePickerOpenProperty, value);
    }
    #endregion

    private EnumComboBox? _stretchComboBox;
    private EnumComboBox? _tileModeComboBox;
    private EnumComboBox? _alignmentXComboBox;
    private EnumComboBox? _alignmentYComboBox;

    // 标志位，防止 FillType 和 BackgroundType 之间的循环同步
    private bool _isSyncingFillType = false;
    private bool _isSyncingBackgroundType = false;

    public FillCanvasPropertyEditor()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            _stretchComboBox = this.FindControl<EnumComboBox>("PART_StretchComboBox");
            _tileModeComboBox = this.FindControl<EnumComboBox>("PART_TileModeComboBox");
            _alignmentXComboBox = this.FindControl<EnumComboBox>("PART_AlignmentXComboBox");
            _alignmentYComboBox = this.FindControl<EnumComboBox>("PART_AlignmentYComboBox");

            if (_stretchComboBox != null)
            {
                _stretchComboBox.EnumType = typeof(Stretch);
                _stretchComboBox.SelectedValue = BackgroundImageStretch.ToString();
                _stretchComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<Stretch>(args.NewValue, out var stretch))
                    {
                        BackgroundImageStretch = stretch;
                    }
                };
            }

            if (_tileModeComboBox != null)
            {
                _tileModeComboBox.EnumType = typeof(TileMode);
                _tileModeComboBox.SelectedValue = BackgroundImageTileMode.ToString();
                _tileModeComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<TileMode>(args.NewValue, out var tileMode))
                    {
                        BackgroundImageTileMode = tileMode;
                    }
                };
            }

            if (_alignmentXComboBox != null)
            {
                _alignmentXComboBox.EnumType = typeof(AlignmentX);
                _alignmentXComboBox.SelectedValue = BackgroundImageAlignmentX.ToString();
                _alignmentXComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<AlignmentX>(args.NewValue, out var alignmentX))
                    {
                        BackgroundImageAlignmentX = alignmentX;
                    }
                };
            }

            if (_alignmentYComboBox != null)
            {
                _alignmentYComboBox.EnumType = typeof(AlignmentY);
                _alignmentYComboBox.SelectedValue = BackgroundImageAlignmentY.ToString();
                _alignmentYComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<AlignmentY>(args.NewValue, out var alignmentY))
                    {
                        BackgroundImageAlignmentY = alignmentY;
                    }
                };
            }

        };

        BackgroundImageStretchProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (_stretchComboBox != null && e.NewValue is Stretch stretch)
            {
                _stretchComboBox.SelectedValue = stretch.ToString();
            }
        });

        BackgroundImageTileModeProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (_tileModeComboBox != null && e.NewValue is TileMode tileMode)
            {
                _tileModeComboBox.SelectedValue = tileMode.ToString();
            }
        });

        BackgroundImageAlignmentXProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (_alignmentXComboBox != null && e.NewValue is AlignmentX alignmentX)
            {
                _alignmentXComboBox.SelectedValue = alignmentX.ToString();
            }
        });

        BackgroundImageAlignmentYProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (_alignmentYComboBox != null && e.NewValue is AlignmentY alignmentY)
            {
                _alignmentYComboBox.SelectedValue = alignmentY.ToString();
            }
        });

        FillTypeProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (_isSyncingBackgroundType) return;
            _isSyncingFillType = true;
            try
            {

                if (e.NewValue is FillType fillType)
                {
                    var bgType = fillType switch
                    {
                        FillType.None => BackgroundType.None,
                        FillType.Solid => BackgroundType.SolidColor,
                        FillType.Gradient => BackgroundType.Gradient,
                        _ => BackgroundType.SolidColor
                    };
                    if (!Equals(sender.BackgroundType, bgType))
                    {
                        sender.BackgroundType = bgType;
                    }
                }
            }
            finally
            {
                _isSyncingFillType = false;
            }
        });

        BackgroundTypeProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (_isSyncingFillType) return;
            _isSyncingBackgroundType = true;
            try
            {
                if (e.NewValue is BackgroundType bgType)
                {
                    var fillType = bgType switch
                    {
                        BackgroundType.None => FillType.None,
                        BackgroundType.SolidColor => FillType.Solid,
                        BackgroundType.Gradient => FillType.Gradient,
                        _ => FillType.Solid
                    };
                    if (!Equals(sender.FillType, fillType))
                    {
                        sender.FillType = fillType;
                    }
                }
            }
            finally
            {
                _isSyncingBackgroundType = false;
            }
        });

        GradientStartColorProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
        });

        GradientEndColorProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
        });

        GradientAngleProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
        });

        BackgroundImageProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (!Equals(e.OldValue, e.NewValue))
            {
                sender.SyncToCanvasPropertyFromBinding(e.NewValue, "CanvasBackgroundImage");
            }
        });

        OpacityProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
        });

        BackgroundColorProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
        });

        EnableBackgroundImageProperty.Changed.AddClassHandler<FillCanvasPropertyEditor>((sender, e) =>
        {
            if (!Equals(e.OldValue, e.NewValue))
            {
                sender.SyncToCanvasPropertyFromBinding(e.NewValue, "EnableBackgroundImage");
            }
        });

        if (this.FillType != null)
        {
            var bgType = this.FillType switch
            {
                FillType.None => BackgroundType.None,
                FillType.Solid => BackgroundType.SolidColor,
                FillType.Gradient => BackgroundType.Gradient,
                _ => BackgroundType.SolidColor
            };
            if (!Equals(this.BackgroundType, bgType))
            {
                this.BackgroundType = bgType;
            }
        }
    }


    private object? GetCurrentCanvasPropertyValue(CanvasProperty canvasProperty, string propertyName)
    {
        var property = canvasProperty.GetType().GetProperty(propertyName);
        return property?.GetValue(canvasProperty);
    }

    private bool IsCanvasPropertyAttribute(string propertyName)
    {
        return propertyName is "CanvasBackgroundColor" or "CanvasBackgroundImage" or
               "GradientStartColor" or "GradientEndColor" or "GradientAngle" or
               "CanvasOpacity" or "IsGradientBackground" or "FillType" or
               "BackgroundImageStretch" or "BackgroundImageTileMode" or
               "BackgroundImageAlignmentX" or "BackgroundImageAlignmentY";
    }

    private void SyncToCanvasProperty(CanvasProperty canvasProperty, string propertyName, object? value)
    {
        var property = canvasProperty.GetType().GetProperty(propertyName);
        if (property != null && value != null)
        {
            try
            {
                if (property.PropertyType.IsEnum)
                {
                    var enumValue = Enum.Parse(property.PropertyType, value.ToString()!);
                    property.SetValue(canvasProperty, enumValue);
                }
                else
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(canvasProperty, convertedValue);
                }
            }
            catch
            {
                property.SetValue(canvasProperty, value);
            }
        }
    }

    /// <summary>
    /// 同步属性值到绑定目标的 CanvasProperty
    /// </summary>
    private void SyncToCanvasPropertyFromBinding(object? value, string propertyName)
    {
        CanvasProperty? canvasProperty = null;

        // 尝试从 DataContext 获取 CanvasProperty
        if (DataContext != null)
        {
            // 1. 直接是 CanvasProperty
            if (DataContext is CanvasProperty cp)
                canvasProperty = cp;
            // 2. 有 CanvasProperty 属性
            else
            {
                var cpProp = DataContext.GetType().GetProperty("CanvasProperty");
                if (cpProp != null)
                    canvasProperty = cpProp.GetValue(DataContext) as CanvasProperty;
            }
        }

        if (canvasProperty != null)
        {
            SyncToCanvasProperty(canvasProperty, propertyName, value);
        }
    }

    private void SyncPropertyToObject(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName);
        if (property != null && value != null)
        {
            try
            {
                if (property.PropertyType.IsEnum)
                {
                    var enumValue = Enum.Parse(property.PropertyType, value.ToString()!);
                    property.SetValue(target, enumValue);
                }
                else
                {
                    var convertedValue = Convert.ChangeType(value, property.PropertyType);
                    property.SetValue(target, convertedValue);
                }
            }
            catch
            {
                property.SetValue(target, value);
            }
        }
    }

    private async void OnSelectImageClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择背景图片",
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
            var file = files[0];
            BackgroundImage = file.Path.LocalPath;
        }
    }

    private void OnClearImageClick(object? sender, RoutedEventArgs e)
    {
        BackgroundImage = string.Empty;
    }

}
