using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace yuyiAvalonia.Base.Controls;

public partial class NumericUpDownEx : UserControl
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<NumericUpDownEx, double>(nameof(Value), 0,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<NumericUpDownEx, double>(nameof(Minimum), 0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<NumericUpDownEx, double>(nameof(Maximum), 255);

    public static readonly StyledProperty<double> StepProperty =
        AvaloniaProperty.Register<NumericUpDownEx, double>(nameof(Step), 1);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Text), "0");

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<NumericUpDownEx, string>(nameof(Suffix), string.Empty);

    public static readonly StyledProperty<StreamGeometry?> IconProperty =
        AvaloniaProperty.Register<NumericUpDownEx, StreamGeometry?>(nameof(Icon));

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<NumericUpDownEx, double>(nameof(FontSize), 11);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<NumericUpDownEx, FontFamily>(nameof(FontFamily), new FontFamily("Consolas"));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<NumericUpDownEx, CornerRadius>(nameof(CornerRadius), new CornerRadius(4));

    public static readonly StyledProperty<bool> NeedUpDownProperty =
        AvaloniaProperty.Register<NumericUpDownEx, bool>(nameof(NeedUpDown), true);

    /// <summary>
    /// 显示的小数位数。默认 0（整数）；设为 2 则显示 0.00、1.23 等。
    /// </summary>
    public static readonly StyledProperty<int> DecimalPlacesProperty =
        AvaloniaProperty.Register<NumericUpDownEx, int>(nameof(DecimalPlaces), 0);

    public bool NeedUpDown
    {
        get => GetValue(NeedUpDownProperty);
        set => SetValue(NeedUpDownProperty, value);
    }

    public int DecimalPlaces
    {
        get => GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Step
    {
        get => GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }
    
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public StreamGeometry? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public int Frequency { get; set; } = 4;

    public event EventHandler<double>? ValueChanged;

    private bool _isSyncing = false;
    private bool _isLoaded = false;

    // 持续变化相关字段
    private DispatcherTimer? _repeatTimer;
    private bool _isRepeating = false;
    private int _repeatDirection = 0; // 1=增加, -1=减少
    private int _repeatCount = 0; // 用于加速计算

    public NumericUpDownEx()
    {
        InitializeComponent();

        PART_PropertyBorder.Attach(this);

        ValueProperty.Changed.AddClassHandler<NumericUpDownEx>((control, e) =>
        {
            if (control._isSyncing) return;

            control._isSyncing = true;

            // 同步 Text 显示
            control.SetValue(TextProperty, control.FormatValue(control.Value));

            control._isSyncing = false;

            control.ValueChanged?.Invoke(control, control.Value);
        });

        TextProperty.Changed.AddClassHandler<NumericUpDownEx>((control, e) =>
        {
            if (control.PART_TextBox == null) return;

            var newText = e.NewValue?.ToString() ?? "0";
            if (control.PART_TextBox.Text != newText)
            {
                control.PART_TextBox.Text = newText;
            }
        });

        IconProperty.Changed.AddClassHandler<NumericUpDownEx>((control, _) => control.UpdateHeaderDisplay());
        TitleProperty.Changed.AddClassHandler<NumericUpDownEx>((control, _) => control.UpdateHeaderDisplay());
        DecimalPlacesProperty.Changed.AddClassHandler<NumericUpDownEx>((control, _) =>
        {
            if (control.PART_TextBox != null)
                control.PART_TextBox.Text = control.FormatValue(control.Value);
        });

        SuffixProperty.Changed.AddClassHandler<NumericUpDownEx>((control, _) =>
        {
            control.PART_Suffix.IsVisible = !String.IsNullOrEmpty(control.Suffix) && !String.IsNullOrWhiteSpace(control.Suffix);
            //control.PART_Suffix.Margin = control.PART_Suffix.IsVisible ? new Thickness(2, 0) : new Thickness(0);
            string columnDefinition = "Auto,*";
            NewColumnDefitions(columnDefinition);
        });

        if (PART_TextBox != null)
        {
            PART_TextBox.LostFocus += (s, e) => SyncValueFromText();
            PART_TextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    SyncValueFromText();
                    this.Focus();
                }
            };
        }

        NeedUpDownProperty.Changed.AddClassHandler<NumericUpDownEx>((control, e) =>
        {
            if (bool.TryParse(e.NewValue!.ToString(), out bool need))
            {
                PART_UpDownButtons.IsVisible = need;

                string columnDefinition = "Auto,*";
                NewColumnDefitions(columnDefinition);
            }

        });

        Loaded += (s, e) =>
        {
            _isLoaded = true;
            UpdateHeaderDisplay();

            if (PART_UpDownButtons != null)
                PART_UpDownButtons.IsVisible = NeedUpDown;
            string columnDefinition = "Auto,*";
            NewColumnDefitions(columnDefinition);

            if (PART_TextBox != null)
                PART_TextBox.Text = FormatValue(Value);
            // 注册按钮的指针事件，使用 AddHandler 确保事件能触发
            if (PART_UpButton != null)
            {
                PART_UpButton.AddHandler(InputElement.PointerPressedEvent, UpButton_PointerPressed, RoutingStrategies.Tunnel);
                PART_UpButton.AddHandler(InputElement.PointerReleasedEvent, UpButton_PointerReleased, RoutingStrategies.Tunnel);
                PART_UpButton.AddHandler(InputElement.PointerExitedEvent, UpButton_PointerExited, RoutingStrategies.Tunnel);
            }

            if (PART_DownButton != null)
            {
                PART_DownButton.AddHandler(InputElement.PointerPressedEvent, DownButton_PointerPressed, RoutingStrategies.Tunnel);
                PART_DownButton.AddHandler(InputElement.PointerReleasedEvent, DownButton_PointerReleased, RoutingStrategies.Tunnel);
                PART_DownButton.AddHandler(InputElement.PointerExitedEvent, DownButton_PointerExited, RoutingStrategies.Tunnel);
            }
        };
    }

    void NewColumnDefitions(string columnDefinition)
    {
        if (PART_UpDownButtons.IsVisible)
        {
            columnDefinition += ",Auto";
        }
        if (PART_Suffix.IsVisible)
        {
            columnDefinition += ",Auto";
        }

        PART_GridContent.ColumnDefinitions = new ColumnDefinitions(columnDefinition);
    }

    private void UpdateHeaderDisplay()
    {
        if (PART_Title == null || PART_Icon == null || PART_GridContent == null)
            return;

        var hasIcon = Icon != null;
        var hasTitle = !string.IsNullOrWhiteSpace(Title);
        var hasHeader = hasIcon || hasTitle;

        PART_Icon.IsVisible = hasIcon;
        PART_Title.IsVisible = !hasIcon && hasTitle;

        if (hasIcon)
            PART_Icon.Data = Icon;
        else
            PART_Title.Text = Title;

        PART_GridContent.ColumnDefinitions[0].Width = hasHeader ? GridLength.Auto : new GridLength(0);
    }

    private void SyncValueFromText()
    {
        if (PART_TextBox == null || _isSyncing) return;
        if (string.IsNullOrWhiteSpace(PART_TextBox.Text)) return;

        if (double.TryParse(PART_TextBox.Text, out var newValue))
        {
            newValue = Math.Clamp(newValue, Minimum, Maximum);
            double tolerance = Math.Pow(10, -DecimalPlaces) / 2.0;
            if (Math.Abs(newValue - Value) > tolerance)
            {
                _isSyncing = true;
                SetValue(ValueProperty, newValue);
                _isSyncing = false;
            }
        }

        PART_TextBox.Text = FormatValue(Value);
    }

    private void UpButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // 立即触发一次变化
        Value = Math.Min(Maximum, Value + Step);
        // 开始持续变化
        StartRepeating(true);
        e.Handled = true;
    }

    private void UpButton_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        StopRepeating();
        e.Handled = true;
    }

    private void UpButton_PointerExited(object? sender, PointerEventArgs e)
    {
        StopRepeating();
        e.Handled = true;
    }

    private void DownButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // 立即触发一次变化
        Value = Math.Max(Minimum, Value - Step);
        // 开始持续变化
        StartRepeating(false);
        e.Handled = true;
    }

    private void DownButton_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        StopRepeating();
        e.Handled = true;
    }

    private void DownButton_PointerExited(object? sender, PointerEventArgs e)
    {
        StopRepeating();
        e.Handled = true;
    }

    /// <summary>
    /// 开始持续变化（鼠标按住时调用）
    /// </summary>
    /// <param name="isUp">true=增加，false=减少</param>
    public void StartRepeating(bool isUp)
    {
        if (_isRepeating) return;

        _isRepeating = true;
        _repeatDirection = isUp ? 1 : -1;
        _repeatCount = 0;

        // 初始化定时器，先延迟后开始快速变化
        _repeatTimer = new DispatcherTimer
        {
            Interval = GetNextInterval()
        };

        _repeatTimer.Tick += RepeatTimer_Tick;
        _repeatTimer.Start();
    }

    /// <summary>
    /// 停止持续变化（鼠标释放时调用）
    /// </summary>
    public void StopRepeating()
    {
        if (!_isRepeating) return;

        _isRepeating = false;
        _repeatDirection = 0;
        _repeatCount = 0;

        _repeatTimer?.Stop();
        _repeatTimer = null;
    }

    private TimeSpan GetNextInterval()
    {
        // 初始延迟较慢，逐渐加速
        if (_repeatCount < 3)
            return TimeSpan.FromMilliseconds(200); // 初始延迟
        else if (_repeatCount < 6)
            return TimeSpan.FromMilliseconds(100);
        else if (_repeatCount < 10)
            return TimeSpan.FromMilliseconds(50);
        else
            return TimeSpan.FromMilliseconds(30); // 最快速度
    }

    private void RepeatTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isRepeating) return;

        // 计算要变化的量（支持加速）
        double step = Step;
        if (_repeatCount >= 6)
            step = Step * 2;
        if (_repeatCount >= 10)
            step = Step * 5;

        if (_repeatDirection > 0) // 增加
        {
            Value = Math.Min(Maximum, Value + step);
        }
        else // 减少
        {
            Value = Math.Max(Minimum, Value - step);
        }

        _repeatCount++;

        // 更新定时器间隔（逐渐加速）
        if (_repeatTimer != null)
        {
            _repeatTimer.Interval = GetNextInterval();
        }
    }

    // 拖拽相关字段
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private double _dragStartValue;
    //private const double DragSensitivity = 0.85; // 每像素变化量

    /// <summary>
    /// 把当前 Value 格式化为 Text 字符串，根据 DecimalPlaces 决定小数位数。
    /// </summary>
    private string FormatValue(double v)
    {
        if (DecimalPlaces <= 0)
            return ((long)Math.Round(v)).ToString();
        double rounded = Math.Round(v, DecimalPlaces);
        return rounded.ToString("F" + DecimalPlaces, System.Globalization.CultureInfo.InvariantCulture);
    }

    private void PanelIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (PART_PanelIcon == null) return;

        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            _dragStartValue = Value;

            // 显示拖拽符号
            //if (PART_DragArrows != null)
            //    PART_DragArrows.IsVisible = true;

            e.Pointer.Capture(PART_PanelIcon);
            e.Handled = true;
        }
    }

    private void PanelIcon_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;

        var currentPoint = e.GetPosition(this);
        var deltaX = currentPoint.X - _dragStartPoint.X;

        // 计算新值：往右拖动增加，往左拖动减少
        //var deltaValue = deltaX * DragSensitivity;
        //var newValue = _dragStartValue + deltaValue;

        //// 限制在范围内
        //newValue = Math.Clamp(newValue, Minimum, Maximum);

        //// 按步长取整
        //newValue = Math.Round(newValue / Step) * Step;

        var newValue = _dragStartValue + Math.Round(deltaX / Frequency) * Step;
        // 限制在范围内
        newValue = Math.Clamp(newValue, Minimum, Maximum);

        if (Math.Abs(newValue - Value) > 0.001)
        {
            Value = newValue;
        }

        e.Handled = true;
    }

    private void PanelIcon_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;

            // 隐藏拖拽符号
            //if (PART_DragArrows != null)
            //    PART_DragArrows.IsVisible = false;

            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }
}
