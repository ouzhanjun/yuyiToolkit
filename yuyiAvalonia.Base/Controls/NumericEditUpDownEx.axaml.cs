using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace yuyiAvalonia.Base.Controls;

public partial class NumericEditUpDownEx : UserControl
{
    // ── 属性定义 ──────────────────────────────────────────────

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, double>(nameof(Value), 0,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, double>(nameof(Minimum), 0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, double>(nameof(Maximum), 255);

    public static readonly StyledProperty<double> StepProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, double>(nameof(Step), 1);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, string>(nameof(Text), "0");

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<StreamGeometry?> IconProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, StreamGeometry?>(nameof(Icon));

    public static readonly StyledProperty<StreamGeometry?> ButtonIconProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, StreamGeometry?>(nameof(ButtonIcon));

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, double>(nameof(FontSize), 11);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, FontFamily>(nameof(FontFamily),
            new FontFamily("Consolas"));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, CornerRadius>(nameof(CornerRadius),
            new CornerRadius(4));

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, string>(nameof(Suffix), string.Empty);

    public static readonly StyledProperty<bool> UseSliderProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, bool>(nameof(UseSlider), false,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<bool> IsSliderPickerOpenProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, bool>(nameof(IsSliderPickerOpen), false,
            defaultBindingMode: BindingMode.TwoWay);

    // 供 Slider Popup 绑定用：Slider 绑定到此属性（代表当前选中段的数值）
    public static readonly StyledProperty<double> SelectedPartValueProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, double>(nameof(SelectedPartValue), 0,
            defaultBindingMode: BindingMode.TwoWay);

    // ── CLR 属性 ──────────────────────────────────────────────

    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Step { get => GetValue(StepProperty); set => SetValue(StepProperty, value); }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public StreamGeometry? Icon { get => GetValue(IconProperty); set => SetValue(IconProperty, value); }
    public StreamGeometry? ButtonIcon { get => GetValue(ButtonIconProperty); set => SetValue(ButtonIconProperty, value); }
    public double FontSize { get => GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
    public FontFamily FontFamily { get => GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
    public CornerRadius CornerRadius { get => GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }

    public static readonly StyledProperty<bool> NeedUpDownProperty =
        AvaloniaProperty.Register<NumericEditUpDownEx, bool>(nameof(NeedUpDown), true);

    public bool NeedUpDown
    {
        get => GetValue(NeedUpDownProperty);
        set => SetValue(NeedUpDownProperty, value);
    }

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public bool UseSlider { get => GetValue(UseSliderProperty); set => SetValue(UseSliderProperty, value); }

    public bool IsSliderPickerOpen
    {
        get => GetValue(IsSliderPickerOpenProperty);
        set => SetValue(IsSliderPickerOpenProperty, value);
    }

    /// <summary>
    /// 当前选中逗号分段的数值，Slider Popup 绑定到此属性。
    /// </summary>
    public double SelectedPartValue
    {
        get => GetValue(SelectedPartValueProperty);
        set => SetValue(SelectedPartValueProperty, value);
    }

    // ── 事件 ─────────────────────────────────────────────────

    public event EventHandler<double>? ValueChanged;
    public event EventHandler? PressedButton;

    // ── 内部字段 ──────────────────────────────────────────────

    private bool _isSyncingText;
    private bool _isSyncingSlider;

    // 逗号分隔段数据缓存（持久化，避免每次从 TextBox 光标状态重新解析）
    private List<double> _cachedParts = new();
    private int _cachedPartIndex;

    // 重复按键定时器
    private DispatcherTimer? _repeatTimer;
    private bool _isRepeating;
    private int _repeatDirection; // 1=增加, -1=减少
    private int _repeatCount;

    // 拖拽字段
    private bool _isDragging;
    private Point _dragStartPoint;
    private double _dragStartValue;
    private const double DragSensitivity = 0.85;

    // ── 初始化 ────────────────────────────────────────────────

    public NumericEditUpDownEx()
    {
        InitializeComponent();

        PART_PropertyBorder.Attach(this);

        // Text → TextBox 同步（同时初始化缓存）
        TextProperty.Changed.AddClassHandler<NumericEditUpDownEx>((control, e) =>
        {
            if (control._isSyncingText) return;
            if (control.PART_TextBox == null) return;
            var newText = e.NewValue?.ToString() ?? "0";
            if (control.PART_TextBox.Text != newText)
                control.PART_TextBox.Text = newText;
            // 外部修改 Text 时同步缓存
            control.SyncCachedPartsFromText();
        });

        // SelectedPartValue → Text 同步（Slider 拖动时实时更新对应段）
        SelectedPartValueProperty.Changed.AddClassHandler<NumericEditUpDownEx>((control, _) =>
        {
            if (control._isSyncingSlider) return;
            control.ApplySliderToSelectedPart();
        });

        IconProperty.Changed.AddClassHandler<NumericEditUpDownEx>((_, _) => UpdateHeaderDisplay());
        TitleProperty.Changed.AddClassHandler<NumericEditUpDownEx>((_, _) => UpdateHeaderDisplay());
        ButtonIconProperty.Changed.AddClassHandler<NumericEditUpDownEx>((_, _) => UpdateButtonDisplay());

        Loaded += (s, e) =>
        {
            UpdateHeaderDisplay();
            UpdateButtonDisplay();

            // TextBox 事件
            if (PART_TextBox != null)
            {
                PART_TextBox.LostFocus += PART_TextBox_LostFocus;
                PART_TextBox.KeyDown += PART_TextBox_KeyDown;
            }

            if (PART_UpDownButtons != null)
                PART_UpDownButtons.IsVisible = NeedUpDown;

            // Button Click
            if (PART_Button != null)
                PART_Button.Click += PARTButton_Click;

            PART_UpButton.AddHandler(InputElement.PointerPressedEvent,
                UpButton_PointerPressed, RoutingStrategies.Tunnel);
            PART_UpButton.AddHandler(InputElement.PointerReleasedEvent,
                UpButton_PointerReleased, RoutingStrategies.Tunnel);
            PART_UpButton.AddHandler(InputElement.PointerExitedEvent,
                UpButton_PointerExited, RoutingStrategies.Tunnel);

            PART_DownButton.AddHandler(InputElement.PointerPressedEvent,
                DownButton_PointerPressed, RoutingStrategies.Tunnel);
            PART_DownButton.AddHandler(InputElement.PointerReleasedEvent,
                DownButton_PointerReleased, RoutingStrategies.Tunnel);
            PART_DownButton.AddHandler(InputElement.PointerExitedEvent,
                DownButton_PointerExited, RoutingStrategies.Tunnel);
        };

        SliderPickerPopup.Closed += (sender, _) => IsSliderPickerOpen = false;
    }

    // ── Header / Button 显示控制 ─────────────────────────────

    private void UpdateHeaderDisplay()
    {
        if (PART_Title == null || PART_Icon == null || PART_GridContent == null) return;
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

    private void UpdateButtonDisplay()
    {
        if (PART_Button == null) return;
        var hasSlider = UseSlider;
        PART_Button.IsVisible = hasSlider;
        if (PART_ButtonIcon != null && ButtonIcon != null)
            PART_ButtonIcon.Data = ButtonIcon;
    }

    // ── TextBox 事件 ─────────────────────────────────────────

    private void PART_TextBox_LostFocus(object? sender, RoutedEventArgs e)
    {
        SyncPartsFromText();
    }

    private void PART_TextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SyncPartsFromText();
            this.Focus();
        }
        else if (e.Key == Key.Up)
        {
            IncrementSelectedPart(1);
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            IncrementSelectedPart(-1);
            e.Handled = true;
        }
    }

    // ── 核心：逗号分隔字符串解析 ──────────────────────────────

    /// <summary>
    /// 从 Text 解析段列表，存入 _cachedParts，并更新 _cachedPartIndex
    /// </summary>
    private void SyncCachedPartsFromText()
    {
        _cachedParts.Clear();
        if (string.IsNullOrWhiteSpace(Text))
        {
            _cachedPartIndex = -1;
            return;
        }
        var segments = Text.Split(',');
        foreach (var seg in segments)
        {
            var trimmed = seg.Trim();
            if (double.TryParse(trimmed, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                _cachedParts.Add(Math.Clamp(v, Minimum, Maximum));
            else
                _cachedParts.Add(Minimum);
        }
        // 根据 TextBox 光标更新段下标（仅作为初始值，实际操作以缓存为准）
        _cachedPartIndex = GetSelectedPartIndexFromCursor();
    }

    /// <summary>
    /// 将 _cachedParts 写回 Text
    /// </summary>
    private void FlushCachedPartsToText()
    {
        _isSyncingText = true;
        Text = string.Join(",", _cachedParts.ConvertAll(v => Math.Round(v).ToString()));
        _isSyncingText = false;
    }

    /// <summary>
    /// 根据 TextBox 光标获取段下标（作为缓存的初始值，不在 Slider 操作中调用）
    /// </summary>
    private int GetSelectedPartIndexFromCursor()
    {
        if (PART_TextBox == null) return 0;
        var text = PART_TextBox.Text ?? "";
        var selStart = PART_TextBox.SelectionStart;
        var selEnd = PART_TextBox.SelectionEnd;

        // 有选中文本（且选中内容非空）→ 解析为数字后找段
        if (selStart < selEnd)
        {
            var selected = text.Substring(selStart, selEnd - selStart).Trim();
            if (double.TryParse(selected, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                int pos = selStart;
                int partIdx = 0;
                int segStart = 0;
                foreach (var seg in text.Split(','))
                {
                    int segLen = seg.Length;
                    int segEnd = segStart + segLen;
                    if (pos >= segStart && pos <= segEnd)
                        return partIdx;
                    segStart = segEnd + 1;
                    partIdx++;
                }
                return partIdx - 1;
            }
        }

        // 无选中文本：光标落在哪段就是哪段
        int cur = 0;
        int start = 0;
        foreach (var seg in text.Split(','))
        {
            int segEnd = start + seg.Length;
            if (cur == 0 && selStart <= segEnd)
                return 0;
            if (selStart >= start && selStart <= segEnd)
                return cur;
            start = segEnd + 1;
            cur++;
        }
        return Math.Max(0, cur - 1);
    }

    /// <summary>
    /// 获取缓存中当前选中段的值
    /// </summary>
    private double GetSelectedPartValue()
    {
        if (_cachedPartIndex < 0 || _cachedPartIndex >= _cachedParts.Count) return Minimum;
        return _cachedParts[_cachedPartIndex];
    }

    /// <summary>
    /// 更新缓存中指定下标的段值，并刷新 Text
    /// </summary>
    private void SetPartValue(int partIdx, double newValue)
    {
        if (partIdx < 0 || partIdx >= _cachedParts.Count) return;
        newValue = Math.Clamp(newValue, Minimum, Maximum);
        if (Math.Abs(_cachedParts[partIdx] - newValue) < 0.001) return;
        _cachedParts[partIdx] = newValue;
        FlushCachedPartsToText();
        RaiseValueChanged();
    }

    /// <summary>
    /// TextBox 失焦或回车时：将所有段的数值合并到 Value
    /// </summary>
    private void SyncPartsFromText()
    {
        SyncCachedPartsFromText();
        if (_cachedParts.Count == 0) return;
        double sum = 0;
        foreach (var p in _cachedParts) sum += p;
        _isSyncingText = true;
        Value = sum;
        _isSyncingText = false;
    }

    // ── UpButton / DownButton ─────────────────────────────────

    private void IncrementSelectedPart(int direction)
    {
        SyncCachedPartsFromText();

        if (_cachedPartIndex < 0 || _cachedPartIndex >= _cachedParts.Count) return;
        var newVal = Math.Clamp(_cachedParts[_cachedPartIndex] + direction * Step, Minimum, Maximum);
        _cachedParts[_cachedPartIndex] = Math.Round(newVal / Step) * Step;
        FlushCachedPartsToText();
        RaiseValueChanged();
    }

    private void UpButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        IncrementSelectedPart(1);
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
        IncrementSelectedPart(-1);
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

    // ── 重复计时器 ────────────────────────────────────────────

    public void StartRepeating(bool isUp)
    {
        if (_isRepeating) return;
        _isRepeating = true;
        _repeatDirection = isUp ? 1 : -1;
        _repeatCount = 0;
        _repeatTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
        _repeatTimer.Tick += RepeatTimer_Tick;
        _repeatTimer.Start();
    }

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
        if (_repeatCount < 3) return TimeSpan.FromMilliseconds(200);
        if (_repeatCount < 6) return TimeSpan.FromMilliseconds(100);
        if (_repeatCount < 10) return TimeSpan.FromMilliseconds(50);
        return TimeSpan.FromMilliseconds(30);
    }

    private void RepeatTimer_Tick(object? sender, EventArgs e)
    {
        if (!_isRepeating) return;
        double step = Step;
        if (_repeatCount >= 6) step = Step * 2;
        if (_repeatCount >= 10) step = Step * 5;
        IncrementSelectedPart(_repeatDirection * (int)(step / Step));
        _repeatCount++;
        if (_repeatTimer != null)
            _repeatTimer.Interval = GetNextInterval();
    }

    // ── Slider ───────────────────────────────────────────────

    private void PARTButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!UseSlider) return;
        // 打开弹窗前，先更新缓存（确保 _cachedParts 和 _cachedPartIndex 是最新的）
        SyncCachedPartsFromText();
        // Popup 弹出前把 SelectedPartValue 设为当前选中段的值
        SelectedPartValue = GetSelectedPartValue();
        PART_Slider.Width = this.Bounds.Width > 0 ? this.Bounds.Width : 100;
        SliderPickerPopup.PlacementTarget = PART_Button;
        IsSliderPickerOpen = true;
        PressedButton?.Invoke(this, e);
    }

    /// <summary>
    /// Slider 的 SelectedPartValue 改变后，将新值写回缓存对应段（使用缓存索引，不依赖 TextBox 焦点状态）
    /// </summary>
    private void ApplySliderToSelectedPart()
    {
        if (!_isSyncingSlider)
        {
            _isSyncingSlider = true;
            SetPartValue(_cachedPartIndex, SelectedPartValue);
            _isSyncingSlider = false;
        }
    }

    // ── 拖拽（PanelIcon）────────────────────────────────────

    private void PanelIcon_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (PART_PanelIcon == null) return;
        var props = e.GetCurrentPoint(this).Properties;
        if (!props.IsLeftButtonPressed) return;
        SyncCachedPartsFromText();
        _isDragging = true;
        _dragStartPoint = e.GetPosition(this);
        _dragStartValue = GetSelectedPartValue();
        e.Pointer.Capture(PART_PanelIcon);
        e.Handled = true;
    }

    private void PanelIcon_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging) return;
        var deltaX = e.GetPosition(this).X - _dragStartPoint.X;
        var newValue = Math.Clamp(_dragStartValue + deltaX * DragSensitivity, Minimum, Maximum);
        newValue = Math.Round(newValue / Step) * Step;
        SetPartValue(_cachedPartIndex, newValue);
        e.Handled = true;
    }

    private void PanelIcon_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        e.Pointer.Capture(null);
        RaiseValueChanged();
        e.Handled = true;
    }

    // ── 事件触发 ─────────────────────────────────────────────

    private void RaiseValueChanged()
    {
        SyncPartsFromText();
        ValueChanged?.Invoke(this, Value);
    }
}
