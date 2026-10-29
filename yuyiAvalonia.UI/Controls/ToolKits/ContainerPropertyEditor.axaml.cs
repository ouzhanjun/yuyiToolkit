using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using yuyiAvalonia.UI.ElementControls;

namespace yuyiAvalonia.UI.Controls;

public partial class ContainerPropertyEditor : UserControl
{
    #region BackgroundColor
    public static readonly StyledProperty<string> BackgroundColorProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, string>(
            nameof(BackgroundColor),
            defaultValue: "#FFFFFF",
            defaultBindingMode: BindingMode.TwoWay);

    public string BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }
    #endregion

    #region Opacity
    public static readonly StyledProperty<double> OpacityProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, double>(
            nameof(Opacity),
            defaultValue: 1.0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }
    #endregion

    #region BackgroundImage
    public static readonly StyledProperty<string> BackgroundImageProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, string>(
            nameof(BackgroundImage),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string BackgroundImage
    {
        get => GetValue(BackgroundImageProperty);
        set => SetValue(BackgroundImageProperty, value);
    }
    #endregion

    #region Orientation
    public static readonly StyledProperty<StackPanelOrientation> OrientationProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, StackPanelOrientation>(
            nameof(Orientation),
            defaultValue: StackPanelOrientation.Vertical,
            defaultBindingMode: BindingMode.TwoWay);

    public StackPanelOrientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
    #endregion

    #region Spacing
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, double>(
            nameof(Spacing),
            defaultValue: 0,
            defaultBindingMode: BindingMode.TwoWay);

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
    #endregion

    #region ShowLayoutProperties
    public static readonly StyledProperty<bool> ShowLayoutPropertiesProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, bool>(
            nameof(ShowLayoutProperties),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public bool ShowLayoutProperties
    {
        get => GetValue(ShowLayoutPropertiesProperty);
        set => SetValue(ShowLayoutPropertiesProperty, value);
    }
    #endregion

    #region ShowOrientation
    public static readonly StyledProperty<bool> ShowOrientationProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, bool>(
            nameof(ShowOrientation),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public bool ShowOrientation
    {
        get => GetValue(ShowOrientationProperty);
        set => SetValue(ShowOrientationProperty, value);
    }
    #endregion

    #region ShowSpacing
    public static readonly StyledProperty<bool> ShowSpacingProperty =
        AvaloniaProperty.Register<ContainerPropertyEditor, bool>(
            nameof(ShowSpacing),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public bool ShowSpacing
    {
        get => GetValue(ShowSpacingProperty);
        set => SetValue(ShowSpacingProperty, value);
    }
    #endregion

    public bool IsVertical
    {
        get => Orientation == StackPanelOrientation.Vertical;
        set
        {
            if (value) Orientation = StackPanelOrientation.Vertical;
        }
    }

    public bool IsHorizontal
    {
        get => Orientation == StackPanelOrientation.Horizontal;
        set
        {
            if (value) Orientation = StackPanelOrientation.Horizontal;
        }
    }

    private TextBox? _imageTextBox;
    private Button? _selectImageButton;

    public ContainerPropertyEditor()
    {
        InitializeComponent();

        _imageTextBox = this.FindControl<TextBox>("PART_ImageTextBox");
        _selectImageButton = this.FindControl<Button>("PART_SelectImageButton");
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
