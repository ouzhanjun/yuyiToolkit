using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Collections.ObjectModel;
using System.Globalization;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class ShadowEditor : UserControl
{
    #region StyledProperties

    public static readonly StyledProperty<ObservableCollection<ShadowItem>> ShadowsProperty =
        AvaloniaProperty.Register<ShadowEditor, ObservableCollection<ShadowItem>>(
            nameof(Shadows),
            defaultValue: new ObservableCollection<ShadowItem>());

    public ObservableCollection<ShadowItem> Shadows
    {
        get => GetValue(ShadowsProperty);
        set => SetValue(ShadowsProperty, value);
    }

    public static readonly StyledProperty<bool> HasShadowsProperty =
        AvaloniaProperty.Register<ShadowEditor, bool>(nameof(HasShadows));

    public bool HasShadows
    {
        get => GetValue(HasShadowsProperty);
        set => SetValue(HasShadowsProperty, value);
    }

    public static readonly StyledProperty<ShadowItem?> SelectedShadowProperty =
        AvaloniaProperty.Register<ShadowEditor, ShadowItem?>(nameof(SelectedShadow));

    public ShadowItem? SelectedShadow
    {
        get => GetValue(SelectedShadowProperty);
        set => SetValue(SelectedShadowProperty, value);
    }

    public static readonly StyledProperty<bool> IsShadowEditorOpenProperty =
        AvaloniaProperty.Register<ShadowEditor, bool>(nameof(IsShadowEditorOpen));

    public bool IsShadowEditorOpen
    {
        get => GetValue(IsShadowEditorOpenProperty);
        set => SetValue(IsShadowEditorOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsAddShadowPopupOpenProperty =
        AvaloniaProperty.Register<ShadowEditor, bool>(nameof(IsAddShadowPopupOpen));

    public bool IsAddShadowPopupOpen
    {
        get => GetValue(IsAddShadowPopupOpenProperty);
        set => SetValue(IsAddShadowPopupOpenProperty, value);
    }

    public double OpacityPercent
    {
        get => GetValue(OpacityPercentProperty);
        set => SetValue(OpacityPercentProperty, value);
    }

    public static readonly StyledProperty<double> OpacityPercentProperty =
        AvaloniaProperty.Register<StrokeEditor, double>(
            nameof(OpacityPercent),
            defaultValue: 100.0,
            defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region ShadowType Converters

    public static readonly IValueConverter ShadowTypeOuterConverter = new ShadowTypeBoolConverter(ShadowType.Outer);
    public static readonly IValueConverter ShadowTypeInnerConverter = new ShadowTypeBoolConverter(ShadowType.Inner);

    private class ShadowTypeBoolConverter : IValueConverter
    {
        private readonly ShadowType _kind;
        public ShadowTypeBoolConverter(ShadowType kind) => _kind = kind;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ShadowType k) return k == _kind;
            return false;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is true) return _kind;
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

    #endregion

    public event Action? RefreshControl;

    public ShadowEditor()
    {
        InitializeComponent();

        HasShadows = false;
        IsShadowEditorOpen = false;
        IsAddShadowPopupOpen = false;

        IsShadowEditorOpenProperty.Changed.AddClassHandler<ShadowEditor>((sender, e) =>
        {
            if (e.NewValue is false)
                sender.SelectedShadow = null;
        });

        ShadowsProperty.Changed.AddClassHandler<ShadowEditor>((sender, e) =>
        {
            if (Shadows == null)
                return;
            HasShadows = Shadows.Count > 0;
            Shadows.CollectionChanged += (s, e) =>
            {
                HasShadows = Shadows.Count > 0;
                if (SelectedShadow != null && !Shadows.Contains(SelectedShadow))
                    SelectedShadow = null;
            };
        });

        SelectedShadowProperty.Changed.AddClassHandler<ShadowEditor>((sender, e) =>
        {
            if (e.OldValue is ShadowItem oldShadow)
            {
                oldShadow.PropertyChanged -= sender.OnSelectedShadowPropertyChanged;
            }
            if (e.NewValue is ShadowItem newShadow)
            {
                newShadow.PropertyChanged += sender.OnSelectedShadowPropertyChanged;

                var newPercent = newShadow.Opacity * 100;

                if (Math.Abs(newPercent - sender.OpacityPercent) > 0.01)
                {
                    sender.SetValue(OpacityPercentProperty, newPercent);
                }
            }
        });

        OpacityPercentProperty.Changed.AddClassHandler<ShadowEditor>((sender, e) =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (e.NewValue is double percent)
                {
                    var newOpacity = percent / 100.0;
                    if (Math.Abs(newOpacity - sender.Opacity) > 0.001)
                    {
                        if(SelectedShadow != null)
                        {
                            SelectedShadow.Opacity = newOpacity;
                        }
                    }
                }
            });
        });

        PropertyChanged += (object? sender, AvaloniaPropertyChangedEventArgs e) =>
        {
            RefreshControl?.Invoke();
        };

        DragHost.PointerPressed += OnPointerPressed;
        DragHost.PointerMoved += OnPointerMoved;
        DragHost.PointerReleased += OnPointerReleased;

        ShadowEditorPopup.Opened += ShadowEditorPopup_Opened;
    }

    #region 拖动弹窗

    private bool _isDragging;
    private Point _lastPos;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            _isDragging = true;
            _lastPos = e.GetPosition(this);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastPos;
            ShadowEditorPopup.HorizontalOffset += delta.X;
            ShadowEditorPopup.VerticalOffset += delta.Y;
            _lastPos = pos;
            e.Handled = true;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    private void ShadowEditorPopup_Opened(object? sender, EventArgs e)
    {
        ShadowEditorPopup.HorizontalOffset = 0;
        ShadowEditorPopup.VerticalOffset = 0;
    }

    #endregion

    private void OnSelectedShadowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        RefreshControl?.Invoke();
    }

    #region Event Handlers

    private void AddButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsAddShadowPopupOpen = !IsAddShadowPopupOpen;
    }

    private void AddOuterShadow_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsAddShadowPopupOpen = false;
        AddShadow(ShadowType.Outer);
    }

    private void AddInnerShadow_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsAddShadowPopupOpen = false;
        AddShadow(ShadowType.Inner);
    }

    private void AddShadow(ShadowType shadowType = ShadowType.Outer)
    {
        if (Shadows == null) return;

        var count = Shadows.Count + 1;
        var shadow = new ShadowItem
        {
            IsEnabled = true,
            Name = $"阴影 {count}",
            ShadowType = shadowType,
            OffsetX = 0,
            OffsetY = shadowType == ShadowType.Inner ? -4 : 4,
            Spread = 0,
            Blur = 8,
            Color = "#000000",
            Opacity = 0.5
        };

        Shadows.Add(shadow);
        SelectedShadow = shadow;
        RefreshControl?.Invoke();
    }

    private void RemoveShadow_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ShadowItem shadow && Shadows != null)
        {
            var index = Shadows.IndexOf(shadow);
            Shadows.Remove(shadow);
            if (SelectedShadow == shadow)
                SelectedShadow = Shadows.Count > 0 ? Shadows[System.Math.Min(index, Shadows.Count - 1)] : null;
            RefreshControl?.Invoke();
        }
    }

    private void MoveUpShadow_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is ShadowItem shadow && Shadows != null)
        {
            var index = Shadows.IndexOf(shadow);
            if (index > 0)
                Shadows.Move(index, index - 1);
        }
    }

    private void ShadowItem_Click(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is ShadowItem shadow)
        {
            ShadowEditorPopup.PlacementTarget = border;
            SelectedShadow = shadow;
            IsShadowEditorOpen = true;
        }
    }

    private void SelectShadowType_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is ShadowType kind)
        {
            if (SelectedShadow != null)
                SelectedShadow.ShadowType = kind;
        }
    }

    private void CloseShadowEditor_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsShadowEditorOpen = false;
    }

    private void VisibilityToggle_Toggled(object? sender, RoutedEventArgs e)
    {
        RefreshControl?.Invoke();
    }
    #endregion
}
