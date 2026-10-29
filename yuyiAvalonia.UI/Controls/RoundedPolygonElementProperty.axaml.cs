using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Elements;

namespace yuyiAvalonia.UI.Controls;

public partial class RoundedPolygonElementProperty : UserControl
{
    public static readonly StyledProperty<RoundedPolygonElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<RoundedPolygonElementProperty, RoundedPolygonElement?>(
            nameof(ElementProperty),
            defaultValue: null,
            defaultBindingMode: BindingMode.TwoWay);

    public RoundedPolygonElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    private RoundedPolygonElement? _currentElement;

    public RoundedPolygonElementProperty()
    {
        InitializeComponent();

        ElementPropertyProperty.Changed.AddClassHandler<RoundedPolygonElementProperty>((sender, e) =>
        {
            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged -= sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged -= sender.OnBackFillPropertyChanged;
            }

            sender._currentElement = e.NewValue as RoundedPolygonElement;

            if (sender._currentElement != null)
            {
                sender._currentElement.PropertyChanged += sender.OnElementPropertyChanged;
                sender._currentElement.BackgroundFill.PropertyChanged += sender.OnBackFillPropertyChanged;
            }

            sender.SyncFillEditor();
        });

        ShapeEditor.ElementPropertyChanged += OnShapeEditorElementPropertyChanged;
    }

    private void OnShapeEditorElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        ElementPropertyChanged?.Invoke(sender, e);
    }

    private void OnBackFillPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_currentElement != null && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(_currentElement, new PropertyChangedEventArgs(e.PropertyName));
        }
    }

    private void SyncFillEditor()
    {
        if (FillEditorCtrl == null) return;

        if (_currentElement != null)
        {
            FillEditorCtrl.Fills = _currentElement.BackgroundFills;
            FillEditorCtrl.BackgroundFill = _currentElement.BackgroundFill;
            FillEditorCtrl.RefreshControl -= OnFillEditorRefresh;
            FillEditorCtrl.RefreshControl += OnFillEditorRefresh;
            FillEditorCtrl.FillPropertyEditorCtrl.RefreshControl -= FillEditorCtrl_FillPropertyEditorCtrl_RefreshControl;
            FillEditorCtrl.FillPropertyEditorCtrl.RefreshControl += FillEditorCtrl_FillPropertyEditorCtrl_RefreshControl;

            StrokeEditorCtrl.StrokeFills = _currentElement.StrokeFills;
            StrokeEditorCtrl.StrokeFill = _currentElement.StrokeFill;
            StrokeEditorCtrl.RefreshControl -= OnStrokeFillEditorRefresh;
            StrokeEditorCtrl.RefreshControl += OnStrokeFillEditorRefresh;
            StrokeEditorCtrl.FillPropertyEditorCtrl.RefreshControl -= StrokeEditorCtrl_FillPropertyEditorCtrl_RefreshControl;
            StrokeEditorCtrl.FillPropertyEditorCtrl.RefreshControl += StrokeEditorCtrl_FillPropertyEditorCtrl_RefreshControl;

            ShadowEditorCtrl.Shadows = _currentElement.Shadows;
            ShadowEditorCtrl.RefreshControl -= OnShadowEditorRefreshControl;
            ShadowEditorCtrl.RefreshControl += OnShadowEditorRefreshControl;
        }
    }

    private void StrokeEditorCtrl_FillPropertyEditorCtrl_RefreshControl()
    {
        if (_currentElement != null)
            _currentElement.StrokeFill = this.StrokeEditorCtrl.FillPropertyEditorCtrl.Fill ?? _currentElement.StrokeFill;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.StrokeFill)));
    }

    private void FillEditorCtrl_FillPropertyEditorCtrl_RefreshControl()
    {
        if (_currentElement != null)
            _currentElement.BackgroundFill = this.FillEditorCtrl.FillPropertyEditorCtrl.Fill ?? _currentElement.BackgroundFill;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFill)));
    }

    private void OnShadowEditorRefreshControl()
    {
        if (_currentElement != null)
            _currentElement.Shadows = this.ShadowEditorCtrl?.Shadows ?? _currentElement.Shadows;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs("Shadows"));
    }

    private void OnFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.BackgroundFills = this.FillEditorCtrl?.Fills ?? _currentElement.BackgroundFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFills)));
    }

    private void OnStrokeFillEditorRefresh()
    {
        if (_currentElement != null)
            _currentElement.StrokeFills = this.StrokeEditorCtrl?.StrokeFills ?? _currentElement.StrokeFills;
        ElementPropertyChanged?.Invoke(_currentElement!, new PropertyChangedEventArgs(nameof(_currentElement.StrokeFills)));
    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is RoundedPolygonElement && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(sender, e);
        }
    }

    public event PropertyChangedEventHandler? ElementPropertyChanged;
}