using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using yuyiAvalonia.Base.Controls;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;


public partial class StrokePropertyEditor : UserControl
{
    #region EnableAdvancedStroke
    public static readonly StyledProperty<bool> EnableAdvancedStrokeProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, bool>(
            nameof(EnableAdvancedStroke),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool EnableAdvancedStroke
    {
        get => GetValue(EnableAdvancedStrokeProperty);
        set
        {
            SetValue(EnableAdvancedStrokeProperty, value);
            OnEnableAdvancedStrokeChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// 回调：当 EnableAdvancedStroke 变化时通知外部（用于形状控件绑定）
    /// </summary>
    public event Action<bool>? OnEnableAdvancedStrokeChanged;

    #endregion

    #region IsToggleVisible（用于隐藏启用开关）
    public static readonly StyledProperty<bool> IsToggleVisibleProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, bool>(
            nameof(IsToggleVisible),
            defaultValue: true,
            defaultBindingMode: BindingMode.OneWay);

    public bool IsToggleVisible
    {
        get => GetValue(IsToggleVisibleProperty);
        set => SetValue(IsToggleVisibleProperty, value);
    }
    #endregion

    #region StrokeStyleIndex
    public static readonly StyledProperty<int> StrokeStyleIndexProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, int>(
            nameof(StrokeStyleIndex),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public int StrokeStyleIndex
    {
        get => GetValue(StrokeStyleIndexProperty);
        set => SetValue(StrokeStyleIndexProperty, value);
    }
    #endregion

    #region StrokeWidth
    public static readonly StyledProperty<double> StrokeWidthProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, double>(
            nameof(StrokeWidth),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double StrokeWidth
    {
        get => GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }
    #endregion

    #region StrokeFillType
    public static readonly StyledProperty<Models.StrokeFillType> StrokeFillTypeProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, Models.StrokeFillType>(
            nameof(StrokeFillType),
            defaultValue: Models.StrokeFillType.Solid,
            defaultBindingMode: BindingMode.TwoWay);

    public Models.StrokeFillType StrokeFillType
    {
        get => GetValue(StrokeFillTypeProperty);
        set => SetValue(StrokeFillTypeProperty, value);
    }
    #endregion

    #region StrokeColor
    public static readonly StyledProperty<string> StrokeColorProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, string>(
            nameof(StrokeColor),
            defaultValue: "#333333",
            defaultBindingMode: BindingMode.TwoWay);

    public string StrokeColor
    {
        get => GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }
    #endregion

    #region StrokeOpacity
    public static readonly StyledProperty<double> StrokeOpacityProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, double>(
            nameof(StrokeOpacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double StrokeOpacity
    {
        get => GetValue(StrokeOpacityProperty);
        set => SetValue(StrokeOpacityProperty, value);
    }
    #endregion

    #region StrokeGradientStartColor
    public static readonly StyledProperty<string> StrokeGradientStartColorProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, string>(
            nameof(StrokeGradientStartColor),
            defaultValue: "#333333",
            defaultBindingMode: BindingMode.TwoWay);

    public string StrokeGradientStartColor
    {
        get => GetValue(StrokeGradientStartColorProperty);
        set => SetValue(StrokeGradientStartColorProperty, value);
    }
    #endregion

    #region StrokeGradientEndColor
    public static readonly StyledProperty<string> StrokeGradientEndColorProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, string>(
            nameof(StrokeGradientEndColor),
            defaultValue: "#666666",
            defaultBindingMode: BindingMode.TwoWay);

    public string StrokeGradientEndColor
    {
        get => GetValue(StrokeGradientEndColorProperty);
        set => SetValue(StrokeGradientEndColorProperty, value);
    }
    #endregion

    #region StrokeGradientAngle
    public static readonly StyledProperty<double> StrokeGradientAngleProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, double>(
            nameof(StrokeGradientAngle),
            defaultValue: 0.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double StrokeGradientAngle
    {
        get => GetValue(StrokeGradientAngleProperty);
        set => SetValue(StrokeGradientAngleProperty, value);
    }
    #endregion

    #region StrokeImage
    public static readonly StyledProperty<string> StrokeImageProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, string>(
            nameof(StrokeImage),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string StrokeImage
    {
        get => GetValue(StrokeImageProperty);
        set => SetValue(StrokeImageProperty, value);
    }
    #endregion

    #region StrokeImageStretch
    public static readonly StyledProperty<Stretch> StrokeImageStretchProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, Stretch>(
            nameof(StrokeImageStretch),
            defaultValue: Stretch.UniformToFill,
            defaultBindingMode: BindingMode.TwoWay);

    public Stretch StrokeImageStretch
    {
        get => GetValue(StrokeImageStretchProperty);
        set => SetValue(StrokeImageStretchProperty, value);
    }
    #endregion

    #region StrokeImageStretchIndex (for backward compat)
    public static readonly StyledProperty<int> StrokeImageStretchIndexProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, int>(
            nameof(StrokeImageStretchIndex),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public int StrokeImageStretchIndex
    {
        get => GetValue(StrokeImageStretchIndexProperty);
        set => SetValue(StrokeImageStretchIndexProperty, value);
    }
    #endregion

    #region StrokeImageTileMode
    public static readonly StyledProperty<TileMode> StrokeImageTileModeProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, TileMode>(
            nameof(StrokeImageTileMode),
            defaultValue: TileMode.None,
            defaultBindingMode: BindingMode.TwoWay);

    public TileMode StrokeImageTileMode
    {
        get => GetValue(StrokeImageTileModeProperty);
        set => SetValue(StrokeImageTileModeProperty, value);
    }
    #endregion

    #region StrokeImageTileModeIndex (for backward compat)
    public static readonly StyledProperty<int> StrokeImageTileModeIndexProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, int>(
            nameof(StrokeImageTileModeIndex),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public int StrokeImageTileModeIndex
    {
        get => GetValue(StrokeImageTileModeIndexProperty);
        set => SetValue(StrokeImageTileModeIndexProperty, value);
    }
    #endregion

    #region StrokeImageAlignmentX
    public static readonly StyledProperty<AlignmentX> StrokeImageAlignmentXProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, AlignmentX>(
            nameof(StrokeImageAlignmentX),
            defaultValue: AlignmentX.Left,
            defaultBindingMode: BindingMode.TwoWay);

    public AlignmentX StrokeImageAlignmentX
    {
        get => GetValue(StrokeImageAlignmentXProperty);
        set => SetValue(StrokeImageAlignmentXProperty, value);
    }
    #endregion

    #region StrokeImageAlignmentY
    public static readonly StyledProperty<AlignmentY> StrokeImageAlignmentYProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, AlignmentY>(
            nameof(StrokeImageAlignmentY),
            defaultValue: AlignmentY.Top,
            defaultBindingMode: BindingMode.TwoWay);

    public AlignmentY StrokeImageAlignmentY
    {
        get => GetValue(StrokeImageAlignmentYProperty);
        set => SetValue(StrokeImageAlignmentYProperty, value);
    }
    #endregion

    #region StrokeDashOffset
    public static readonly StyledProperty<double> StrokeDashOffsetProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, double>(
            nameof(StrokeDashOffset),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double StrokeDashOffset
    {
        get => GetValue(StrokeDashOffsetProperty);
        set => SetValue(StrokeDashOffsetProperty, value);
    }
    #endregion

    #region StrokeDashArrayDefinition (虚线数组定义字符串)
    public static readonly StyledProperty<string> StrokeDashArrayDefinitionProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, string>(
            nameof(StrokeDashArrayDefinition),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string StrokeDashArrayDefinition
    {
        get => GetValue(StrokeDashArrayDefinitionProperty);
        set => SetValue(StrokeDashArrayDefinitionProperty, value);
    }
    #endregion

    #region CornerRadius
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, double>(
            nameof(CornerRadius),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    #endregion

    #region IsStrokeGradientAnglePickerOpen
    public static readonly StyledProperty<bool> IsStrokeGradientAnglePickerOpenProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, bool>(
            nameof(IsStrokeGradientAnglePickerOpen),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsStrokeGradientAnglePickerOpen
    {
        get => GetValue(IsStrokeGradientAnglePickerOpenProperty);
        set => SetValue(IsStrokeGradientAnglePickerOpenProperty, value);
    }
    #endregion

    #region StrokeStyleItemsSource
    public static readonly StyledProperty<IList<StrokeStyleItem>> StrokeStyleItemsSourceProperty =
        AvaloniaProperty.Register<StrokePropertyEditor, IList<StrokeStyleItem>>(
            nameof(StrokeStyleItemsSource),
            defaultValue: PathHelper.GetStrokeStyleItems());

    public IList<StrokeStyleItem> StrokeStyleItemsSource
    {
        get => GetValue(StrokeStyleItemsSourceProperty);
        set => SetValue(StrokeStyleItemsSourceProperty, value);
    }
    #endregion

    private EnumComboBox? _strokeStretchComboBox;
    private EnumComboBox? _strokeTileModeComboBox;
    private EnumComboBox? _strokeAlignmentXComboBox;
    private EnumComboBox? _strokeAlignmentYComboBox;
    private EnumComboBox? _strokeFillTypeComboBox;
    private StackPanel? _panelSolid;
    private StackPanel? _panelGradient;
    private StackPanel? _panelImage;

    // 标志位，防止循环同步
    private bool _isSyncingStrokeFillType = false;

    public StrokePropertyEditor()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            _strokeStretchComboBox = this.FindControl<EnumComboBox>("PART_StrokeStretchComboBox");
            _strokeTileModeComboBox = this.FindControl<EnumComboBox>("PART_StrokeTileModeComboBox");
            _strokeAlignmentXComboBox = this.FindControl<EnumComboBox>("PART_StrokeAlignmentXComboBox");
            _strokeAlignmentYComboBox = this.FindControl<EnumComboBox>("PART_StrokeAlignmentYComboBox");
            _strokeFillTypeComboBox = this.FindControl<EnumComboBox>("PART_StrokeFillTypeComboBox");
            _panelSolid = this.FindControl<StackPanel>("PART_PanelSolid");
            _panelGradient = this.FindControl<StackPanel>("PART_PanelGradient");
            _panelImage = this.FindControl<StackPanel>("PART_PanelImage");

            if (_strokeFillTypeComboBox != null)
            {
                _strokeFillTypeComboBox.EnumType = typeof(Models.StrokeFillType);
                _strokeFillTypeComboBox.SelectedValue = StrokeFillType.ToString();
                _strokeFillTypeComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<Models.StrokeFillType>(args.NewValue, out var fillType))
                    {
                        StrokeFillType = fillType;
                    }
                };
            }
            SyncFillTypePanels((int)StrokeFillType);

            if (_strokeStretchComboBox != null)
            {
                _strokeStretchComboBox.EnumType = typeof(Stretch);
                _strokeStretchComboBox.SelectedValue = StrokeImageStretch.ToString();
                _strokeStretchComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<Stretch>(args.NewValue, out var stretch))
                    {
                        StrokeImageStretch = stretch;
                    }
                };
            }

            if (_strokeTileModeComboBox != null)
            {
                _strokeTileModeComboBox.EnumType = typeof(TileMode);
                _strokeTileModeComboBox.SelectedValue = StrokeImageTileMode.ToString();
                _strokeTileModeComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<TileMode>(args.NewValue, out var tileMode))
                    {
                        StrokeImageTileMode = tileMode;
                    }
                };
            }

            if (_strokeAlignmentXComboBox != null)
            {
                _strokeAlignmentXComboBox.EnumType = typeof(AlignmentX);
                _strokeAlignmentXComboBox.SelectedValue = StrokeImageAlignmentX.ToString();
                _strokeAlignmentXComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<AlignmentX>(args.NewValue, out var alignmentX))
                    {
                        StrokeImageAlignmentX = alignmentX;
                    }
                };
            }

            if (_strokeAlignmentYComboBox != null)
            {
                _strokeAlignmentYComboBox.EnumType = typeof(AlignmentY);
                _strokeAlignmentYComboBox.SelectedValue = StrokeImageAlignmentY.ToString();
                _strokeAlignmentYComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<AlignmentY>(args.NewValue, out var alignmentY))
                    {
                        StrokeImageAlignmentY = alignmentY;
                    }
                };
            }
        };

        //StrokeFillTypeProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        //{
        //    if (e.NewValue is Models.StrokeFillType fillType && !sender._isSyncingStrokeFillType)
        //    {
        //        int index = (int)fillType;
        //        //var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        //        //timer.Tick += (s, args) =>
        //        //{
        //        //    timer.Stop();
        //        //    if (_strokeFillTypeComboBox == null) return;
        //        //    SyncFillTypePanels(index);
        //        //    if (_strokeFillTypeComboBox.ItemCount >= index)
        //        //    {
        //        //        sender._isSyncingStrokeFillType = true;
        //        //        _strokeFillTypeComboBox.SelectedIndex = index;
        //        //        sender._isSyncingStrokeFillType = false;
        //        //    }
        //        //};
        //        //timer.Start();


        //        if (_strokeFillTypeComboBox == null) return;
        //        SyncFillTypePanels(index);
        //        if (_strokeFillTypeComboBox.ItemCount >= index)
        //        {
        //            sender._isSyncingStrokeFillType = true;
        //            _strokeFillTypeComboBox.SelectedIndex = index;
        //            sender._isSyncingStrokeFillType = false;
        //        }
        //    }
        //});

        StrokeImageStretchProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        {
            if (_strokeStretchComboBox != null && e.NewValue is Stretch stretch)
            {
                _strokeStretchComboBox.SelectedValue = stretch.ToString();
            }
        });

        StrokeImageTileModeProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        {
            if (_strokeTileModeComboBox != null && e.NewValue is TileMode tileMode)
            {
                _strokeTileModeComboBox.SelectedValue = tileMode.ToString();
            }
        });

        StrokeImageAlignmentXProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        {
            if (_strokeAlignmentXComboBox != null && e.NewValue is AlignmentX alignmentX)
            {
                _strokeAlignmentXComboBox.SelectedValue = alignmentX.ToString();
            }
        });

        StrokeImageAlignmentYProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        {
            if (_strokeAlignmentYComboBox != null && e.NewValue is AlignmentY alignmentY)
            {
                _strokeAlignmentYComboBox.SelectedValue = alignmentY.ToString();
            }
        });

        // 监听 StrokeFillType 变化，动态切换下方面板的可见性
        StrokeFillTypeProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        {
            if (e.NewValue is Models.StrokeFillType fillType)
            {
                sender.SyncFillTypePanels((int)fillType);
                if (sender._strokeFillTypeComboBox != null)
                    sender._strokeFillTypeComboBox.SelectedValue = fillType.ToString();
            }
        });

        //// CornerRadius 变化时触发属性更新，刷新高级边框
        //CornerRadiusProperty.Changed.AddClassHandler<StrokePropertyEditor>((sender, e) =>
        //{
        //    // 属性绑定会自动同步到 ElementProperty，ElementProperty 变化会触发 UpdateStroke
        //});
    }

    private void SyncFillTypePanels(int index)
    {
        if (_panelSolid != null && _panelSolid.GetVisualRoot() != null) _panelSolid.IsVisible = index == 1;
        if (_panelGradient != null && _panelGradient.GetVisualRoot() != null) _panelGradient.IsVisible = index == 2;
        if (_panelImage != null && _panelImage.GetVisualRoot() != null) _panelImage.IsVisible = index == 3;
    }

    private async void OnStrokeSelectImageClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "选择边框图片",
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
            StrokeImage = files[0].Path.LocalPath;
        }
    }

    private void OnStrokeClearImageClick(object? sender, RoutedEventArgs e)
    {
        StrokeImage = string.Empty;
    }
}

/// <summary>
/// 虚线数组编辑器（适用于 StrokePropertyEditor）
/// 根据 StrokeStyleIndex 自动生成默认虚线数组
/// </summary>
//public class StrokeDashArrayEditor : DashArrayDefinitionEditor
//{
//    public static readonly StyledProperty<int> StrokeStyleIndexProperty =
//        AvaloniaProperty.Register<StrokeDashArrayEditor, int>(
//            nameof(StrokeStyleIndex),
//            defaultValue: 0,
//            defaultBindingMode: BindingMode.OneWay);

//    public int StrokeStyleIndex
//    {
//        get => GetValue(StrokeStyleIndexProperty);
//        set => SetValue(StrokeStyleIndexProperty, value);
//    }

//    protected override string GetDefaultDashArrayForCurrentStyle()
//    {
//        return GetDefaultDashArrayForStyle(StrokeStyleIndex);
//    }

//    public StrokeDashArrayEditor()
//    {
//        StrokeStyleIndexProperty.Changed.AddClassHandler<StrokeDashArrayEditor>((sender, e) =>
//        {
//            // 当样式变化时，自动生成新的默认值
//            if (sender.StrokeStyleIndex > 0)
//            {
//                sender.DashArrayDefinition = GetDefaultDashArrayForStyle(sender.StrokeStyleIndex);
//            }
//        });
//    }
//}
