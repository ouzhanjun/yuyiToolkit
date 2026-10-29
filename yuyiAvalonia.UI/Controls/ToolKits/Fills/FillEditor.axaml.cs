using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Collections.ObjectModel;
using System.Globalization;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class FillEditor : UserControl
{
    #region StyledProperties

    public static readonly StyledProperty<ObservableCollection<FillItem>> FillsProperty =
        AvaloniaProperty.Register<FillEditor, ObservableCollection<FillItem>>(
            nameof(Fills),
            defaultValue: new ObservableCollection<FillItem>());

    public ObservableCollection<FillItem> Fills
    {
        get => GetValue(FillsProperty);
        set => SetValue(FillsProperty, value);
    }

    public static readonly StyledProperty<FillItem?> BackgroundFillProperty =
        AvaloniaProperty.Register<FillEditor, FillItem?>(nameof(BackgroundFill));

    public FillItem? BackgroundFill
    {
        get => GetValue(BackgroundFillProperty);
        set => SetValue(BackgroundFillProperty, value);
    }

    public static readonly StyledProperty<FillItem?> SelectedFillProperty =
        AvaloniaProperty.Register<FillEditor, FillItem?>(nameof(SelectedFill));

    public FillItem? SelectedFill
    {
        get => GetValue(SelectedFillProperty);
        set => SetValue(SelectedFillProperty, value);
    }

    public static readonly StyledProperty<bool> IsAddPopupOpenProperty =
        AvaloniaProperty.Register<FillEditor, bool>(nameof(IsAddPopupOpen));

    public bool IsAddPopupOpen
    {
        get => GetValue(IsAddPopupOpenProperty);
        set => SetValue(IsAddPopupOpenProperty, value);
    }

    public static readonly StyledProperty<bool> IsFillEditorOpenProperty =
        AvaloniaProperty.Register<FillEditor, bool>(nameof(IsFillEditorOpen));

    public bool IsFillEditorOpen
    {
        get => GetValue(IsFillEditorOpenProperty);
        set => SetValue(IsFillEditorOpenProperty, value);
    }

    #endregion

    #region FillType Converters

    public static readonly IValueConverter FillTypeSolidConverter = new FillTypeBoolConverter(FillType.Solid);
    public static readonly IValueConverter FillTypeGradientConverter = new FillTypeBoolConverter(FillType.Gradient);
    public static readonly IValueConverter FillTypeImageConverter = new FillTypeBoolConverter(FillType.Image);
    public static readonly IValueConverter FillTypeNoneConverter = new FillTypeBoolConverter(FillType.None);

    private class FillTypeBoolConverter : IValueConverter
    {
        private readonly FillType _kind;
        public FillTypeBoolConverter(FillType kind) => _kind = kind;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FillType k) return k == _kind;
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

    public FillEditor()
    {
        InitializeComponent();

        IsAddPopupOpen = false;
        IsFillEditorOpen = false;

        IsFillEditorOpenProperty.Changed.AddClassHandler<FillEditor>((sender, e) =>
        {
            if (e.NewValue is false)
                sender.SelectedFill = null;
        });

        FillsProperty.Changed.AddClassHandler<FillEditor>((sender, e) =>
        {
            if (Fills == null)
                return;

            Fills.CollectionChanged += (s, e) =>
            {
                if (SelectedFill != null && !Fills.Contains(SelectedFill))
                    SelectedFill = null;
            };

            if (FillItemsCtrl != null)
            {
                FillItemsCtrl.FillItemClicked -= OnFillItemsCtrlItemClicked;
                FillItemsCtrl.FillItemClicked += OnFillItemsCtrlItemClicked;
                FillItemsCtrl.RemoveRequested -= OnFillItemsCtrlRemoveRequested;
                FillItemsCtrl.RemoveRequested += OnFillItemsCtrlRemoveRequested;
                FillItemsCtrl.MoveUpRequested -= OnFillItemsCtrlMoveUpRequested;
                FillItemsCtrl.MoveUpRequested += OnFillItemsCtrlMoveUpRequested;
            }
        });

        SelectedFillProperty.Changed.AddClassHandler<FillEditor>((sender, e) =>
        {
            if (e.OldValue is FillItem oldFill)
            {
                oldFill.PropertyChanged -= sender.OnSelectedFillPropertyChanged;
                oldFill.GradientStops.CollectionChanged -= GradientStops_CollectionChanged;
            }

            if (e.NewValue is FillItem newFill)
            {
                newFill.PropertyChanged += sender.OnSelectedFillPropertyChanged;
                newFill.GradientStops.CollectionChanged += GradientStops_CollectionChanged;
            }
        });

        GradientFillPropertyEditor.StopOffsetChanged += () =>
        {
            RefreshControl?.Invoke();
        };

        DragHost.PointerPressed += OnPointerPressed;
        DragHost.PointerMoved += OnPointerMoved;
        DragHost.PointerReleased += OnPointerReleased;

        FillEditorPopup.Opened += FillEditorPopup_Opened;
    }


    private void OnSelectedFillPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
       
        RefreshControl?.Invoke();
    }

    private void GradientStops_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshControl?.Invoke();
    }

    #region 拖动弹窗

    private bool _isDragging;
    private Point _lastPos;

    // 鼠标按下：开始拖动
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Pointer.Type == PointerType.Mouse)
        {
            if (e.GetPosition((Visual)sender!).Y < 40)
            {
                _isDragging = true;
                _lastPos = e.GetPosition(this);
                e.Handled = true;
            }
        }
    }

    // 鼠标移动：更新 Popup 位置
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var pos = e.GetPosition(this);
            var delta = pos - _lastPos;

            FillEditorPopup.HorizontalOffset += delta.X;
            FillEditorPopup.VerticalOffset += delta.Y;

            _lastPos = pos;
            e.Handled = true;
        }
    }

    // 鼠标松开：结束拖动
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    private void FillEditorPopup_Opened(object? sender, EventArgs e)
    {
        FillEditorPopup.HorizontalOffset = 0;
        FillEditorPopup.VerticalOffset = 0;
    }

    #endregion

    #region Event Handlers

    private void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = !IsAddPopupOpen;
    }

    private void AddSolidFill_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = false;
        AddFill(FillType.Solid);
    }

    private void AddGradientFill_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = false;
        AddFill(FillType.Gradient);
    }

    private void AddImageFill_Click(object? sender, RoutedEventArgs e)
    {
        IsAddPopupOpen = false;
        AddFill(FillType.Image);
    }

    private void AddFill(FillType kind)
    {
        if (Fills == null) return;

        var fill = new FillItem
        {
            IsEnabled = true,
            FillType = kind,
            Name = GetDefaultFillName(kind)
        };

        if (kind == FillType.Solid)
        {
            fill.BackgroundColor = "#3DCFEF";
            fill.Opacity = 1.0;
        }
        else if (kind == FillType.Gradient)
        {
            fill.GradientKind = GradientKind.Linear;
            fill.GradientStartColor = "#FF6B6B";
            fill.GradientEndColor = "#4ECDC4";
            fill.GradientAngle = 0;
            fill.Opacity = 1.0;
            fill.GradientStops = new ObservableCollection<GradientStopInfo>
            {
                new GradientStopInfo(0.0, "#FF6B6B"),
                new GradientStopInfo(1.0, "#4ECDC4")
            };
        }
        else if (kind == FillType.Image)
        {
            fill.ImageStretchIndex = 3;
            fill.Opacity = 1.0;
        }

        Fills.Add(fill);
        SelectedFill = fill;
        IsFillEditorOpen = false;
        RefreshControl?.Invoke();
    }

    private string GetDefaultFillName(FillType kind)
    {
        var baseName = kind switch
        {
            FillType.Solid => "纯色",
            FillType.Gradient => "渐变",
            FillType.Image => "图片",
            _ => "填充"
        };

        if (Fills == null) 
            return baseName;
        var count = Fills.Count(f => f.FillType == kind) + 1;
        return $"{baseName} {count}";
    }

    // 列表项内部事件路由处理（FillItemsControl 抛上来的）
    private void OnFillItemsCtrlRemoveRequested(FillItem fill)
    {
        if (Fills == null) return;
        var index = Fills.IndexOf(fill);
        var wasSelected = SelectedFill == fill;

        Fills.Remove(fill);

        if (wasSelected)
        {
            SelectedFill = Fills.Count > 0 ? Fills[Math.Min(index, Fills.Count - 1)] : null;
            // 删除的是当前正在编辑的项时关闭编辑器弹窗（避免弹窗仍指向一个已不存在的项）
            IsFillEditorOpen = false;
        }
        RefreshControl?.Invoke();
    }

    private void OnFillItemsCtrlMoveUpRequested(FillItem fill)
    {
        if (Fills == null) return;
        var index = Fills.IndexOf(fill);
        if (index > 0)
            Fills.Move(index, index - 1);
    }

    private void OnFillItemsCtrlItemClicked(FillItem fill)
    {
        FillEditorPopup.PlacementTarget = FillItemsCtrl;
        SelectedFill = fill;
        IsAddPopupOpen = false;
        IsFillEditorOpen = true;
    }

    private void SelectFillType_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && btn.Tag is FillType kind)
        {
            if (SelectedFill != null)
            {
                SelectedFill.FillType = kind;
            }
        }
    }

    private void CloseFillEditor_Click(object? sender, RoutedEventArgs e)
    {
        IsFillEditorOpen = false;
    }

    #endregion
}
