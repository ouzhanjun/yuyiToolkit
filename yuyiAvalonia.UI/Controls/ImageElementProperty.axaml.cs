using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System.ComponentModel;
using yuyiAvalonia.Base.Controls;
using yuyiAvalonia.UI.ElementControls.Elements;

namespace yuyiAvalonia.UI.Controls;

public partial class ImageElementProperty : UserControl
{
    #region ElementProperty
    public static readonly StyledProperty<ImageElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<ImageElementProperty, ImageElement?>(
            nameof(ElementProperty),
            defaultValue: null);

    public ImageElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }
    #endregion

    #region ImagePath
    public static readonly StyledProperty<string> ImagePathProperty =
        AvaloniaProperty.Register<ImageElementProperty, string>(
            nameof(ImagePath),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.TwoWay);

    public string ImagePath
    {
        get => GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }
    #endregion

    #region ImageStretch
    public static readonly StyledProperty<Stretch> ImageStretchProperty =
        AvaloniaProperty.Register<ImageElementProperty, Stretch>(
            nameof(ImageStretch),
            defaultValue: Stretch.Uniform,
            defaultBindingMode: BindingMode.TwoWay);

    public Stretch ImageStretch
    {
        get => GetValue(ImageStretchProperty);
        set => SetValue(ImageStretchProperty, value);
    }
    #endregion

    #region ImageStretchDirection
    public static readonly StyledProperty<StretchDirection> ImageStretchDirectionProperty =
        AvaloniaProperty.Register<ImageElementProperty, StretchDirection>(
            nameof(ImageStretchDirection),
            defaultValue: StretchDirection.Both,
            defaultBindingMode: BindingMode.TwoWay);

    public StretchDirection ImageStretchDirection
    {
        get => GetValue(ImageStretchDirectionProperty);
        set => SetValue(ImageStretchDirectionProperty, value);
    }
    #endregion

    private EnumComboBox? _stretchComboBox;
    private EnumComboBox? _stretchDirectionComboBox;
    private ImageElement? _currentElement;

    public ImageElementProperty()
    {
        InitializeComponent();

        // 监听 ElementProperty 变化
        ElementPropertyProperty.Changed.AddClassHandler<ImageElementProperty>((sender, e) =>
        {
            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged -= sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged -= sender.OnBackFillPropertyChanged;
            }

            sender._currentElement = e.NewValue as ImageElement;

            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged += sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged += sender.OnBackFillPropertyChanged;
                sender.SyncAllToControl();
                sender.SyncFillEditor();
                sender.SyncStrokeEditor();
            }
            else
            {
                // 清空属性
                sender.ImagePath = string.Empty;
                sender.ImageStretch = Stretch.Uniform;
                sender.ImageStretchDirection = StretchDirection.Both;
            }
        });

        Loaded += (s, e) =>
        {
            _stretchComboBox = this.FindControl<EnumComboBox>("PART_StretchComboBox");
            _stretchDirectionComboBox = this.FindControl<EnumComboBox>("PART_StretchDirectionComboBox");

            if (_stretchComboBox != null)
            {
                _stretchComboBox.EnumType = typeof(Stretch);
                _stretchComboBox.SelectedValue = ImageStretch.ToString();
                _stretchComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<Stretch>(args.NewValue, out var stretch))
                    {
                        if (ElementProperty != null)
                            ElementProperty.ImageStretch = stretch;
                    }
                };
            }

            if (_stretchDirectionComboBox != null)
            {
                _stretchDirectionComboBox.EnumType = typeof(StretchDirection);
                _stretchDirectionComboBox.SelectedValue = ImageStretchDirection.ToString();
                _stretchDirectionComboBox.SelectedValueChanged += (ctrl, args) =>
                {
                    if (Enum.TryParse<StretchDirection>(args.NewValue, out var direction))
                    {
                        if (ElementProperty != null)
                            ElementProperty.ImageStretchDirection = direction;
                    }
                };
            }
        };

        // 监听 ImagePath 变化，同步到元素
        ImagePathProperty.Changed.AddClassHandler<ImageElementProperty>((sender, e) =>
        {
            if (sender.ElementProperty != null && e.NewValue is string path)
            {
                sender.ElementProperty.ImagePath = path;
            }
        });
    }

    /// <summary>
    /// 同步元素的所有属性到控件
    /// </summary>
    private void SyncAllToControl()
    {
        if (ElementProperty == null) return;

        ImagePath = ElementProperty.ImagePath ?? string.Empty;
        ImageStretch = ElementProperty.ImageStretch;
        ImageStretchDirection = ElementProperty.ImageStretchDirection;

        // 同步 EnumComboBox
        if (_stretchComboBox != null)
            _stretchComboBox.SelectedValue = ImageStretch.ToString();
        if (_stretchDirectionComboBox != null)
            _stretchDirectionComboBox.SelectedValue = ImageStretchDirection.ToString();
    }

    private void SyncFillEditor()
    {
        var fillEditor = this.FillEditorCtrl;
        if (fillEditor == null) return;

        if (_currentElement != null)
        {
            fillEditor.Fills = _currentElement.BackgroundFills;
            fillEditor.RefreshControl -= OnFillEditorRefresh;
            fillEditor.RefreshControl += OnFillEditorRefresh;
        }
    }

    private void OnFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.BackgroundFills = this.FillEditorCtrl?.Fills ?? _currentElement.BackgroundFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFills)));
    }

    private void SyncStrokeEditor()
    {
        var strokeEditor = this.StrokeEditorCtrl;
        if (strokeEditor == null) return;

        if (_currentElement != null)
        {
            strokeEditor.StrokeFills = _currentElement.StrokeFills;
            strokeEditor.RefreshControl -= OnStrokeFillEditorRefresh;
            strokeEditor.RefreshControl += OnStrokeFillEditorRefresh;
        }
    }

    private void OnStrokeFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.StrokeFills = this.StrokeEditorCtrl?.StrokeFills ?? _currentElement.StrokeFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.StrokeFills)));
    }

    /// <summary>
    /// 属性变更事件 - 当元素属性变化时触发，用于通知外部更新
    /// </summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    private void OnBackFillPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_currentElement != null && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(_currentElement, new System.ComponentModel.PropertyChangedEventArgs(e.PropertyName));
        }
    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ImageElement element && e.PropertyName != null)
        {
            // 同步属性变化到控件
            switch (e.PropertyName)
            {
                case nameof(ImageElement.ImagePath):
                    ImagePath = element.ImagePath;
                    break;
                case nameof(ImageElement.ImageStretch):
                    ImageStretch = element.ImageStretch;
                    break;
                case nameof(ImageElement.ImageStretchDirection):
                    ImageStretchDirection = element.ImageStretchDirection;
                    break;
            }

            ElementPropertyChanged?.Invoke(sender, e);
        }
    }

    private async void OnSelectImageClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

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

    private void OnClearImageClick(object? sender, RoutedEventArgs e)
    {
        ImagePath = string.Empty;
    }
}
