using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using yuyiAvalonia.Base.Helpers;

namespace yuyiAvalonia.UI.Controls;

public partial class PositionEditor : UserControl
{
    private Button? _btnLeftTop;
    private Button? _btnTop;
    private Button? _btnTopRight;
    private Button? _btnLeft;
    private Button? _btnCenter;
    private Button? _btnRight;
    private Button? _btnLeftBottom;
    private Button? _btnBottom;
    private Button? _btnRightBottom;

    private PathIcon? _iconLeft;
    private PathIcon? _iconTop;
    private PathIcon? _iconRight;
    private PathIcon? _iconBottom;

    private bool _isUpdating;

    IBrush? selectedBrush = null;
    public PositionEditor()
    {
        InitializeComponent();

        _btnLeftTop = this.FindControl<Button>("BtnLeftTop");
        _btnTop = this.FindControl<Button>("BtnTop");
        _btnTopRight = this.FindControl<Button>("BtnTopRight");
        _btnLeft = this.FindControl<Button>("BtnLeft");
        _btnCenter = this.FindControl<Button>("BtnCenter");
        _btnRight = this.FindControl<Button>("BtnRight");
        _btnLeftBottom = this.FindControl<Button>("BtnLeftBottom");
        _btnBottom = this.FindControl<Button>("BtnBottom");
        _btnRightBottom = this.FindControl<Button>("BtnRightBottom");

        _iconLeft = this.FindControl<PathIcon>("IconLeft");
        _iconTop = this.FindControl<PathIcon>("IconTop");
        _iconRight = this.FindControl<PathIcon>("IconRight");
        _iconBottom = this.FindControl<PathIcon>("IconBottom");

        FixLeftProperty.Changed.AddClassHandler<PositionEditor>((control, e) => control.UpdateButtonStyles());
        FixTopProperty.Changed.AddClassHandler<PositionEditor>((control, e) => control.UpdateButtonStyles());
        FixBottomProperty.Changed.AddClassHandler<PositionEditor>((control, e) => control.UpdateButtonStyles());
        FixRightProperty.Changed.AddClassHandler<PositionEditor>((control, e) => control.UpdateButtonStyles());

        Loaded += (s, e) =>
        {
            UpdateButtonStyles();
        };
    }

    // Left 属性
    public static readonly StyledProperty<double> LeftProperty =
        AvaloniaProperty.Register<PositionEditor, double>(nameof(Left),
            defaultValue: -1,
            defaultBindingMode: BindingMode.TwoWay);

    public double Left
    {
        get => GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    // Top 属性
    public static readonly StyledProperty<double> TopProperty =
        AvaloniaProperty.Register<PositionEditor, double>(nameof(Top),
            defaultValue: -1,
            defaultBindingMode: BindingMode.TwoWay);

    public double Top
    {
        get => GetValue(TopProperty);
        set => SetValue(TopProperty, value);
    }

    // Right 属性
    public static readonly StyledProperty<double> RightProperty =
        AvaloniaProperty.Register<PositionEditor, double>(nameof(Right),
            defaultValue: -1,
            defaultBindingMode: BindingMode.TwoWay);

    public double Right
    {
        get => GetValue(RightProperty);
        set => SetValue(RightProperty, value);
    }

    // Bottom 属性
    public static readonly StyledProperty<double> BottomProperty =
        AvaloniaProperty.Register<PositionEditor, double>(nameof(Bottom),
            defaultValue: -1,
            defaultBindingMode: BindingMode.TwoWay);

    public double Bottom
    {
        get => GetValue(BottomProperty);
        set => SetValue(BottomProperty, value);
    }

    // FixLeft 属性 - 默认true表示固定左边
    public static readonly StyledProperty<bool> FixLeftProperty =
        AvaloniaProperty.Register<PositionEditor, bool>(nameof(FixLeft),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool FixLeft
    {
        get => GetValue(FixLeftProperty);
        set => SetValue(FixLeftProperty, value);
    }

    // FixTop 属性 - 默认true表示固定上边
    public static readonly StyledProperty<bool> FixTopProperty =
        AvaloniaProperty.Register<PositionEditor, bool>(nameof(FixTop),
            defaultValue: true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool FixTop
    {
        get => GetValue(FixTopProperty);
        set => SetValue(FixTopProperty, value);
    }

    // FixBottom 属性 - 默认false表示不固定下边
    public static readonly StyledProperty<bool> FixBottomProperty =
        AvaloniaProperty.Register<PositionEditor, bool>(nameof(FixBottom),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool FixBottom
    {
        get => GetValue(FixBottomProperty);
        set => SetValue(FixBottomProperty, value);
    }

    // FixRight 属性 - 默认false表示不固定右边
    public static readonly StyledProperty<bool> FixRightProperty =
        AvaloniaProperty.Register<PositionEditor, bool>(nameof(FixRight),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool FixRight
    {
        get => GetValue(FixRightProperty);
        set => SetValue(FixRightProperty, value);
    }

    // IsDockPanelOpen 属性
    public static readonly StyledProperty<bool> IsDockPanelOpenProperty =
        AvaloniaProperty.Register<PositionEditor, bool>(nameof(IsDockPanelOpen),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsDockPanelOpen
    {
        get => GetValue(IsDockPanelOpenProperty);
        set => SetValue(IsDockPanelOpenProperty, value);
    }

    private void DockButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        IsDockPanelOpen = !IsDockPanelOpen;
    }

    private void OnDockClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tagStr && int.TryParse(tagStr, out int tag))
        {
            _isUpdating = true;
            UpdateFixedState(tag);
            UpdateButtonStyles();
            _isUpdating = false;
        }
    }

    private void UpdateFixedState(int tag)
    {
        switch (tag)
        {
            case 0: // 都不固定
                break;
            case 1: // Left + Top
                break;
            case 2: // Top
                FixTop = !FixTop;
                break;
            case 3: // Top + Right
                break;
            case 4: // Left
                FixLeft = !FixLeft;
                break;
            case 5: // 居中（都不固定）
                break;
            case 6: // Right
                FixRight = !FixRight;
                break;
            case 7: // Left + Bottom
                break;
            case 8: // Bottom
                FixBottom = !FixBottom;
                break;
            case 9: // Right + Bottom
                break;
        }
    }

    private void UpdateButtonStyles()
    {
        var buttons = new (Button? btn, int tag)[] {
            (_btnLeftTop, 1),
            (_btnTop, 2),
            (_btnTopRight, 3),
            (_btnLeft, 4),
            (_btnCenter, 5),
            (_btnRight, 6),
            (_btnLeftBottom, 7),
            (_btnBottom, 8),
            (_btnRightBottom, 9)
        };

        object? hoverBrush = null;

        if (selectedBrush == null)
        {
            if (this.TryFindResource("ButtonBackgroundPointerOver", this.ActualThemeVariant, out hoverBrush))
            {
                selectedBrush = hoverBrush as IBrush;
            }
        }

        var normalBrush = Brushes.Transparent;

        foreach (var (btn, _) in buttons)
        {
            if (btn != null)
            {
                btn.Background = normalBrush;
            }
        }
        
        BtnLeft.Background = FixLeft ? selectedBrush : normalBrush;
        BtnRight.Background = FixRight ? selectedBrush : normalBrush;
        BtnTop.Background = FixTop ? selectedBrush : normalBrush;
        BtnBottom.Background = FixBottom ? selectedBrush : normalBrush;

        // 更新上下左右4个按钮的图标：选中(固定)显示Lock，未选中显示Unlock
        if (_iconLeft != null)
            _iconLeft.Data = FixLeft ? Icon.Lock : Icon.Unlock;
        if (_iconRight != null)
            _iconRight.Data = FixRight ? Icon.Lock : Icon.Unlock;
        if (_iconTop != null)
            _iconTop.Data = FixTop ? Icon.Lock : Icon.Unlock;
        if (_iconBottom != null)
            _iconBottom.Data = FixBottom ? Icon.Lock : Icon.Unlock;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        UpdateButtonStyles();
    }
}
