using Avalonia;
using Avalonia.Controls;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class StackPanelElementProperty : UserControl
{
    public static readonly StyledProperty<CanvasElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<StackPanelElementProperty, CanvasElement?>(
            nameof(ElementProperty));

    public CanvasElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    private CanvasElement? _currentElement;

    #region 转发属性（用于与 StrokePropertyEditor 绑定）

    public int StrokeStyleIndex
    {
        get => ElementProperty?.StrokeStyleIndex ?? 0;
        set
        {
            if (ElementProperty != null)
                ElementProperty.StrokeStyleIndex = value;
        }
    }

    public double StrokeWidth
    {
        get => ElementProperty?.StrokeWidth ?? 1;
        set
        {
            if (ElementProperty != null)
                ElementProperty.StrokeWidth = value;
        }
    }

    public Models.StrokeFillType StrokeFillType
    {
        get => ElementProperty?.StrokeFillType ?? Models.StrokeFillType.Solid;
        set
        {
            if (ElementProperty != null)
                ElementProperty.StrokeFillType = value;
        }
    }

    public string StrokeColor
    {
        get => ElementProperty?.StrokeColor ?? "#333333";
        set
        {
            if (ElementProperty != null)
                ElementProperty.StrokeColor = value;
        }
    }

    public double StrokeOpacity
    {
        get => ElementProperty?.StrokeOpacity ?? 1;
        set
        {
            if (ElementProperty != null)
                ElementProperty.StrokeOpacity = value;
        }
    }

    //public string StrokeGradientStartColor
    //{
    //    get => ElementProperty?.StrokeGradientStartColor ?? "#333333";
    //    set
    //    {
    //        if (ElementProperty != null)
    //            ElementProperty.StrokeGradientStartColor = value;
    //    }
    //}

    //public string StrokeGradientEndColor
    //{
    //    get => ElementProperty?.StrokeGradientEndColor ?? "#666666";
    //    set
    //    {
    //        if (ElementProperty != null)
    //            ElementProperty.StrokeGradientEndColor = value;
    //    }
    //}

    //public double StrokeGradientAngle
    //{
    //    get => ElementProperty?.StrokeGradientAngle ?? 0;
    //    set
    //    {
    //        if (ElementProperty != null)
    //            ElementProperty.StrokeGradientAngle = value;
    //    }
    //}

    //public string StrokeImage
    //{
    //    get => ElementProperty?.StrokeImage ?? string.Empty;
    //    set
    //    {
    //        if (ElementProperty != null)
    //            ElementProperty.StrokeImage = value;
    //    }
    //}

    //public int StrokeImageStretchIndex
    //{
    //    get => ElementProperty?.StrokeImageStretchIndex ?? 3;
    //    set
    //    {
    //        if (ElementProperty != null)
    //            ElementProperty.StrokeImageStretchIndex = value;
    //    }
    //}

    //public int StrokeImageTileModeIndex
    //{
    //    get => ElementProperty?.StrokeImageTileModeIndex ?? 0;
    //    set
    //    {
    //        if (ElementProperty != null)
    //            ElementProperty.StrokeImageTileModeIndex = value;
    //    }
    //}

    public double StrokeDashOffset
    {
        get => ElementProperty?.StrokeDashOffset ?? 0;
        set
        {
            if (ElementProperty != null)
                ElementProperty.StrokeDashOffset = value;
        }
    }

    #endregion

    public StackPanelElementProperty()
    {
        InitializeComponent();

        ElementPropertyProperty.Changed.AddClassHandler<StackPanelElementProperty>((sender, e) =>
        {
            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged -= sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged -= sender.OnBackFillPropertyChanged;
            }

            sender._currentElement = e.NewValue as CanvasElement;

            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged += sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged += sender.OnBackFillPropertyChanged;
            }

            sender.SyncFillEditor();
            sender.SyncStrokeEditor();
        });
    }

    private void OnBackFillPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_currentElement != null && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(_currentElement, new System.ComponentModel.PropertyChangedEventArgs(e.PropertyName));
        }
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

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is CanvasElement && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(sender, e);
        }
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

    public event PropertyChangedEventHandler? ElementPropertyChanged;
}
