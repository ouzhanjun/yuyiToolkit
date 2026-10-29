using Avalonia;
using Avalonia.Controls;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class CardElementProperty : UserControl
{
    public static readonly StyledProperty<CanvasElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<CardElementProperty, CanvasElement?>(
            nameof(ElementProperty));

    public CanvasElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    private CanvasElement? _currentElement;

    public CardElementProperty()
    {
        InitializeComponent();

        ElementPropertyProperty.Changed.AddClassHandler<CardElementProperty>((sender, e) =>
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
        //if (_currentElement != null)
        //{
        //    _currentElement.StrokeFills = this.FillEditorCtrl.StrokeFills ?? _currentElement.StrokeFills; ;
        //}

        ElementPropertyChanged?.Invoke(_currentElement, new PropertyChangedEventArgs(nameof(_currentElement.BackgroundFills)));
    }

    private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is CanvasElement && e.PropertyName != null)
        {
            ElementPropertyChanged?.Invoke(sender, e);
        }
    }

    public event PropertyChangedEventHandler? ElementPropertyChanged;
}
