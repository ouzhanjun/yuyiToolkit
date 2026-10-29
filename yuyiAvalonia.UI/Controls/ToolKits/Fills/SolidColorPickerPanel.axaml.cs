using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.ComponentModel;
using yuyiAvalonia.Base.Controls;
using BaseColorHexInput = yuyiAvalonia.Base.Controls.ColorHexInput;
using BaseColorPickerPanel = yuyiAvalonia.Base.Controls.ColorPickerPanel;

namespace yuyiAvalonia.UI.Controls;

public partial class SolidColorPickerPanel : UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isDraggingSaturation;
    private bool _isDraggingHue;
    private bool _isDraggingBrightness;
    private bool _isDraggingAlpha;
    private bool _isDraggingHslSaturation;
    private bool _isDraggingHslHue;
    private bool _isDraggingLightness;
    private bool _isDraggingHslSatSlider;
    private bool _isDraggingHsbSatSlider;

    public SolidColorPickerPanel()
    {
        InitializeComponent();
        Loaded += SolidColorPickerPanel_OnLoaded;
        Unloaded += SolidColorPickerPanel_Unloaded;
        SetupInteractions();

        // 当 ActiveFormat 变化（HSB/HSL 切换）时重新装配面板
        BaseColorPickerPanel.ActiveFormatProperty.Changed.AddClassHandler<SolidColorPickerPanel>((panel, e) =>
        {
            panel.ApplyFormatMode();
            panel.UpdateSelectors();
        });

        // Alpha 变化时刷新 alpha 渐变、滑块位置以及相关文本。
        AlphaProperty.Changed.AddClassHandler<SolidColorPickerPanel>((panel, e) =>
        {
            if (Equals(e.OldValue, e.NewValue)) return;
            panel.UpdateAlphaGradient();
            panel.UpdateSelectors();
            panel.UpdateHsbText();
            panel.UpdateHslText();
            panel.UpdateCssText();
            panel.OnPropertyChanged(nameof(RgbText));
            // 实时通知订阅者（例如 GradientFillPropertyEditor 监听 stop 颜色变化）
            panel.RaiseColorSelected();
        });
        // 当外部对 Color 进行双向绑定写入时，把 RGB / HSB / HSL 状态同步过来。
        // 重入由 SyncFromColor 内部的 _isUpdatingDepth 自增守卫处理，因此这里不再额外检查。
        ColorProperty.Changed.AddClassHandler<SolidColorPickerPanel>((panel, e) =>
        {
            if (Equals(e.OldValue, e.NewValue)) return;
            if (e.NewValue is Color color)
            {
                panel.SyncFromColor(color);
                // 颜色变化时实时通知（拖动 / 选择时立即同步到 GradientStopInfo）
                panel.RaiseColorSelected();
            }
        });

        // HSL 文本变化时解析并更新颜色。
        HslTextProperty.Changed.AddClassHandler<SolidColorPickerPanel>((panel, e) =>
        {
            if (panel._isUpdatingDepth > 0) return;
            if (Equals(e.OldValue, e.NewValue)) return;
            if (e.NewValue is string text)
            {
                if (!panel.EnterUpdateScope()) return;
                try { panel.TryParseHslText(text); }
                finally { panel.ExitUpdateScope(); }
            }
        });

        // HSB 文本变化时解析并更新颜色。
        HsbTextProperty.Changed.AddClassHandler<SolidColorPickerPanel>((panel, e) =>
        {
            if (panel._isUpdatingDepth > 0) return;
            if (Equals(e.OldValue, e.NewValue)) return;
            if (e.NewValue is string text)
            {
                if (!panel.EnterUpdateScope()) return;
                try { panel.TryParseHsbText(text); }
                finally { panel.ExitUpdateScope(); }
            }
        });

        // CSS rgba 文本变化时解析并更新颜色。
        CssTextProperty.Changed.AddClassHandler<SolidColorPickerPanel>((panel, e) =>
        {
            if (panel._isUpdatingDepth > 0) return;
            if (Equals(e.OldValue, e.NewValue)) return;
            if (e.NewValue is string text)
            {
                if (!panel.EnterUpdateScope()) return;
                try { panel.TryParseCssText(text); }
                finally { panel.ExitUpdateScope(); }
            }
        });

    }

    #region Color (TwoWay)
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, Color>(
            nameof(Color),
            defaultValue: Color.FromRgb(24, 144, 255),
            defaultBindingMode: BindingMode.TwoWay);

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
    #endregion

    #region Alpha (TwoWay, 0-100)
    public static readonly StyledProperty<double> AlphaProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, double>(
            nameof(Alpha),
            defaultValue: 100,
            defaultBindingMode: BindingMode.TwoWay);

    public double Alpha
    {
        get => GetValue(AlphaProperty);
        set => SetValue(AlphaProperty, Math.Clamp(value, 0, 100));
    }
    #endregion

    /// <summary>
    /// 当前在文本编辑器中激活的颜色格式（继承自基类 ColorPickerPanel.ColorFormatTab）。
    /// </summary>
    public new BaseColorPickerPanel.ColorFormatTab ActiveFormat
    {
        get => (BaseColorPickerPanel.ColorFormatTab)GetValue(BaseColorPickerPanel.ActiveFormatProperty);
        set => SetValue(BaseColorPickerPanel.ActiveFormatProperty, value);
    }

    /// <summary>
    /// 用于 XAML 中以 TwoWay 方式绑定 Hex 输入框的字符串属性（#RRGGBB）。
    /// </summary>
    public static readonly StyledProperty<string> HexColorProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, string>(nameof(HexColor), defaultValue: "#1890FF");

    public string HexColor
    {
        get => GetValue(HexColorProperty);
        set => SetValue(HexColorProperty, value);
    }

    public event EventHandler<(string Color, double Alpha)>? ColorSelected;
    public event EventHandler? CloseRequested;

    public static readonly string[] PresetColors = new[]
    {
        "#FF4D4F", "#FA8C16", "#FFC53D", "#52C41A", "#1890FF",
        "#722ED1", "#EB2F96", "#000000", "#333333", "#666666"
    };

    private void SolidColorPickerPanel_OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _isDraggingAlpha = false;

        BuildFormatComboItems();

        UpdateAllVisuals();
        ApplyFormatMode();
        LayoutUpdated += OnLayoutUpdated;
        Dispatcher.UIThread.Post(ApplyWhiteBorder, DispatcherPriority.Loaded);

        HexInput_ModeChanged(null, ColorHexInput.ColorInputMode.Hex);
    }

    private void SolidColorPickerPanel_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LayoutUpdated -= OnLayoutUpdated;
    }

    private void BuildFormatComboItems()
    {
        if (FormatComboItems == null) return;
        FormatComboItems.Children.Clear();
        foreach (var tab in new[] {BaseColorPickerPanel.ColorFormatTab.RGB, BaseColorPickerPanel.ColorFormatTab.Hsb, BaseColorPickerPanel.ColorFormatTab.Hsl })
        {
            var item = new Border
            {
                Background = new SolidColorBrush(Colors.Transparent),
                CornerRadius = new CornerRadius(2),
                Padding = new Thickness(8, 4),
                Cursor = new Cursor(StandardCursorType.Hand),
                Tag = tab
            };
            IBrush fgBrush = Brushes.Black;
            if (Application.Current?.Resources.TryGetResource("TextPrimary", Application.Current.ActualThemeVariant, out var res) == true && res is IBrush b)
                fgBrush = b;
            var text = new TextBlock
            {
                Text = tab.ToString().ToUpperInvariant(),
                FontSize = 10,
                Foreground = fgBrush
            };
            item.Child = text;
            item.PointerPressed += FormatComboItem_PointerPressed;
            FormatComboItems.Children.Add(item);
        }
    }

    private void FormatComboTrigger_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (FormatComboPopup == null) return;
        FormatComboPopup.Open();
        e.Handled = true;
    }

    private void FormatComboItem_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border b && b.Tag is BaseColorPickerPanel.ColorFormatTab tab)
        {
            ActiveFormat = tab;
            FormatComboPopup?.Close();
        }
        e.Handled = true;
    }

    private void ApplyWhiteBorder()
    {
        if (PresetColorItemsControl == null) return;
        int index = 0;
        foreach (var item in PresetColorItemsControl.ItemsSource)
        {
            if (item?.ToString() == "#FFFFFF")
            {
                var container = PresetColorItemsControl.ItemContainerGenerator.ContainerFromIndex(index);
                if (container is ContentPresenter presenter)
                {
                    var border = presenter.FindDescendantOfType<Border>();
                    if (border != null)
                    {
                        border.BorderBrush = new SolidColorBrush(Color.Parse("#DDDDDD"));
                        border.BorderThickness = new Thickness(1);
                    }
                }
                break;
            }
            index++;
        }
    }

    private void OnLayoutUpdated(object? sender, EventArgs e) => UpdateSelectors();

    /// <summary>
    /// 兼容旧调用方式：根据 hex 字符串（含/不含 alpha）设置颜色。
    /// </summary>
    public void SetColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return;

        try
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 6)
            {
                var newColor = Color.FromRgb(
                    Convert.ToByte(hex[..2], 16),
                    Convert.ToByte(hex[2..4], 16),
                    Convert.ToByte(hex[4..6], 16));
                if (Color != newColor) Color = newColor;
            }
            else if (hex.Length == 8)
            {
                var newAlpha = Convert.ToByte(hex[..2], 16) / 255.0 * 100;
                if (Math.Abs(Alpha - newAlpha) > 0.001) Alpha = newAlpha;
                var newColor = Color.FromRgb(
                    Convert.ToByte(hex[2..4], 16),
                    Convert.ToByte(hex[4..6], 16),
                    Convert.ToByte(hex[6..8], 16));
                if (Color != newColor) Color = newColor;
            }
        }
        catch { }
    }

    public string GetColor() => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

    /// <summary>
    /// 主动触发一次 ColorSelected 事件，把当前 Color/Alpha 推给订阅者。
    /// 用于属性 setter 内部，在用户拖动滑块或外部 setter 写入时实时同步。
    /// </summary>
    public void RaiseColorSelected()
    {
        try
        {
            ColorSelected?.Invoke(this, (GetColor(), Alpha));
        }
        catch
        {
            // 订阅者抛出的异常不应阻断颜色选择流程
        }
    }

    #region RGB / HSB / HSL 内部状态（便于内部控件绑定）
    private double _red = 24;
    public double Red
    {
        get => _red;
        set
        {
            var clamped = Math.Clamp(value, 0, 255);
            if (Math.Abs(_red - clamped) < 0.001) return;
            _red = clamped;
            OnPropertyChanged(nameof(Red));
            UpdateFromRgb();
        }
    }

    private double _green = 144;
    public double Green
    {
        get => _green;
        set
        {
            var clamped = Math.Clamp(value, 0, 255);
            if (Math.Abs(_green - clamped) < 0.001) return;
            _green = clamped;
            OnPropertyChanged(nameof(Green));
            UpdateFromRgb();
        }
    }

    private double _blue = 255;
    public double Blue
    {
        get => _blue;
        set
        {
            var clamped = Math.Clamp(value, 0, 255);
            if (Math.Abs(_blue - clamped) < 0.001) return;
            _blue = clamped;
            OnPropertyChanged(nameof(Blue));
            UpdateFromRgb();
        }
    }

    // HSB 状态：Hue 0-360, Saturation 0-100, Brightness 0-100
    private double _hsbH = HsbFromRgb(24, 144, 255).H;
    private double _hsbS = HsbFromRgb(24, 144, 255).S;
    private double _hsbB = HsbFromRgb(24, 144, 255).B;

    // HSL 状态：Hue 0-360, Saturation 0-100, Lightness 0-100
    private double _hslH = HslFromRgb(24, 144, 255).H;
    private double _hslS = HslFromRgb(24, 144, 255).S;
    private double _hslL = HslFromRgb(24, 144, 255).L;

    /// <summary>HSB 色相 0-360，对应色相条与拾色面板定位。</summary>
    public double Hue
    {
        get => _hsbH;
        set
        {
            var clamped = Math.Clamp(value, 0, 360);
            if (Math.Abs(_hsbH - clamped) < 0.001) return;
            _hsbH = clamped;
            OnPropertyChanged(nameof(Hue));
            UpdateFromHsb();
        }
    }

    /// <summary>HSB 饱和度 0-100，即 SVG 拾色面板水平轴。</summary>
    public double Saturation
    {
        get => _hsbS;
        set
        {
            var clamped = Math.Clamp(value, 0, 100);
            if (Math.Abs(_hsbS - clamped) < 0.001) return;
            _hsbS = clamped;
            OnPropertyChanged(nameof(Saturation));
            UpdateFromHsb();
        }
    }

    /// <summary>HSB 亮度 0-100，即 SVG 拾色面板垂直轴的"反向"。</summary>
    public double Brightness
    {
        get => _hsbB;
        set
        {
            var clamped = Math.Clamp(value, 0, 100);
            if (Math.Abs(_hsbB - clamped) < 0.001) return;
            _hsbB = clamped;
            OnPropertyChanged(nameof(Brightness));
            UpdateFromHsb();
        }
    }

    /// <summary>HSL 亮度 0-100。</summary>
    public double Lightness
    {
        get => _hslL;
        set
        {
            var clamped = Math.Clamp(value, 0, 100);
            if (Math.Abs(_hslL - clamped) < 0.001) return;
            _hslL = clamped;
            OnPropertyChanged(nameof(Lightness));
            UpdateFromHsl();
        }
    }

    /// <summary>HSL 饱和度 0-100（注意：与 HSB 饱和度是不同数学定义）。</summary>
    public double HslSaturation
    {
        get => _hslS;
        set
        {
            var clamped = Math.Clamp(value, 0, 100);
            if (Math.Abs(_hslS - clamped) < 0.001) return;
            _hslS = clamped;
            OnPropertyChanged(nameof(HslSaturation));
            UpdateFromHsl();
        }
    }

    /// <summary>HSL 色相 0-360，独立于 <see cref="Hue"/>。</summary>
    public double HslHue
    {
        get => _hslH;
        set
        {
            var clamped = Math.Clamp(value, 0, 360);
            if (Math.Abs(_hslH - clamped) < 0.001) return;
            _hslH = clamped;
            OnPropertyChanged(nameof(HslHue));
            UpdateFromHsl();
        }
    }

    public string RgbText => $"RGB({Color.R:F0}, {Color.G:F0}, {Color.B:F0}) | A: {Alpha:F0}";

    /// <summary>
    /// HSL 格式文本，用于 TwoWay 绑定解析。
    /// 格式：hsl(H, S%, L%)
    /// </summary>
    public static readonly StyledProperty<string> HslTextProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, string>(nameof(HslText), "hsl(0, 0%, 0%)");

    public string HslText
    {
        get => GetValue(HslTextProperty);
        set => SetValue(HslTextProperty, value);
    }

    /// <summary>
    /// HSB (HSV) 格式文本，用于 TwoWay 绑定解析。
    /// 格式：hsb(H, S%, B%)
    /// </summary>
    public static readonly StyledProperty<string> HsbTextProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, string>(nameof(HsbText), "hsb(0, 0%, 0%)");

    public string HsbText
    {
        get => GetValue(HsbTextProperty);
        set => SetValue(HsbTextProperty, value);
    }

    /// <summary>
    /// CSS rgba 格式文本，用于 TwoWay 绑定解析。
    /// 格式：rgba(R, G, B, A)
    /// </summary>
    public static readonly StyledProperty<string> CssTextProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, string>(nameof(CssText), "rgba(0, 0, 0, 1)");

    public string CssText
    {
        get => GetValue(CssTextProperty);
        set => SetValue(CssTextProperty, value);
    }

    public static readonly StyledProperty<BaseColorHexInput.ColorInputMode> HexCssModeProperty =
        AvaloniaProperty.Register<SolidColorPickerPanel, BaseColorHexInput.ColorInputMode>(nameof(HexCssMode), BaseColorHexInput.ColorInputMode.Hex);

    public BaseColorHexInput.ColorInputMode HexCssMode
    {
        get => GetValue(HexCssModeProperty);
        set => SetValue(HexCssModeProperty, value);
    }
    #endregion

    /// <summary>
    /// 重入深度计数器：用于在嵌套调用 SyncFromColor / Update* / TryParse* 时抑制回环。
    /// 取值 &gt; 0 表示当前正在某个更新过程中；只有最外层（depth 由 0 增至 1 后又减回 0）才真正广播次级事件。
    /// </summary>
    private int _isUpdatingDepth;

    /// <summary>
    /// 进入一段需要抑制回环广播的更新过程。返回 true 表示这是最外层进入（depth 从 0 → 1），
    /// 此时所有 StyledProperty.Changed 处理器会放行；返回 false 表示已经是嵌套层，所有
    /// Update*/SyncFromColor 调用应当短路返回以避免重入。
    /// </summary>
    private bool EnterUpdateScope()
    {
        if (_isUpdatingDepth > 0) return false;
        _isUpdatingDepth = 1;
        return true;
    }

    /// <summary>
    /// 退出更新过程。仅当 depth 从 1 减回 0 时返回 true，调用方此时可以执行收尾的广播/UI 更新。
    /// </summary>
    private bool ExitUpdateScope()
    {
        if (_isUpdatingDepth <= 0) return true;
        _isUpdatingDepth--;
        return _isUpdatingDepth == 0;
    }

    private void SyncFromColor(Color color)
    {
        SyncFromColor(color, force: false);
    }

    /// <summary>
    /// 把 RGB / HSB / HSL 状态同步自给定的 Color。
    /// <paramref name="force"/> 为 true 时跳过重入守卫（用于 <see cref="TryParseHslText"/> 等
    /// 已经处于更新作用域内、但需要真正落地状态的场景）。
    /// </summary>
    private void SyncFromColor(Color color, bool force)
    {
        // 重入守卫：通过深度计数器，嵌套调用 SyncFromColor 时直接返回，避免重复重算与回环广播
        if (!force && _isUpdatingDepth > 0) return;
        _isUpdatingDepth = 1;
        try
        {
            if (Math.Abs(_red - color.R) > 0.001) { _red = color.R; OnPropertyChanged(nameof(Red)); }
            if (Math.Abs(_green - color.G) > 0.001) { _green = color.G; OnPropertyChanged(nameof(Green)); }
            if (Math.Abs(_blue - color.B) > 0.001) { _blue = color.B; OnPropertyChanged(nameof(Blue)); }

            SetHsbFromRgb(color.R, color.G, color.B);
            SetHslFromRgb(color.R, color.G, color.B);

            var newHex = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            if (HexColor != newHex) HexColor = newHex;
            if (HexInput != null && HexInput.Text != newHex) HexInput.Text = newHex;

            UpdateHsbText();
            UpdateHslText();
            UpdateCssText();

            OnPropertyChanged(nameof(RgbText));
            UpdateAllVisuals();
        }
        finally
        {
            _isUpdatingDepth = 0;
        }
    }

    private void UpdateFromRgb()
    {
        if (!EnterUpdateScope()) return;
        try
        {
            SetHsbFromRgb((byte)Red, (byte)Green, (byte)Blue);
            SetHslFromRgb((byte)Red, (byte)Green, (byte)Blue);

            var newHex = $"#{(byte)Red:X2}{(byte)Green:X2}{(byte)Blue:X2}";
            if (HexColor != newHex) HexColor = newHex;

            var newColor = Color.FromRgb((byte)Red, (byte)Green, (byte)Blue);
            if (Color != newColor) Color = newColor;

            if (HexInput != null && HexInput.Text != newHex) HexInput.Text = newHex;

            UpdateHsbText();
            UpdateHslText();
            UpdateCssText();

            OnPropertyChanged(nameof(RgbText));
            UpdateAllVisuals();
        }
        finally
        {
            ExitUpdateScope();
        }
    }

    private void UpdateFromHsb()
    {
        if (!EnterUpdateScope()) return;
        try
        {
            var (r, g, b) = HsbToRgb(_hsbH, _hsbS, _hsbB);
            if (Math.Abs(_red - r) > 0.001) { _red = r; OnPropertyChanged(nameof(Red)); }
            if (Math.Abs(_green - g) > 0.001) { _green = g; OnPropertyChanged(nameof(Green)); }
            if (Math.Abs(_blue - b) > 0.001) { _blue = b; OnPropertyChanged(nameof(Blue)); }

            SetHslFromRgb((byte)Red, (byte)Green, (byte)Blue);

            var newHex = $"#{(byte)Red:X2}{(byte)Green:X2}{(byte)Blue:X2}";
            if (HexColor != newHex) HexColor = newHex;

            var newColor = Color.FromRgb((byte)Red, (byte)Green, (byte)Blue);
            if (Color != newColor) Color = newColor;

            if (HexInput != null && HexInput.Text != newHex) HexInput.Text = newHex;

            UpdateHsbText();
            UpdateHslText();
            UpdateCssText();

            OnPropertyChanged(nameof(RgbText));
            UpdateAllVisuals();
        }
        finally
        {
            ExitUpdateScope();
        }
    }

    private void UpdateFromHsl()
    {
        if (!EnterUpdateScope()) return;
        try
        {
            var (r, g, b) = HslToRgb(_hslH, _hslS, _hslL);
            if (Math.Abs(_red - r) > 0.001) { _red = r; OnPropertyChanged(nameof(Red)); }
            if (Math.Abs(_green - g) > 0.001) { _green = g; OnPropertyChanged(nameof(Green)); }
            if (Math.Abs(_blue - b) > 0.001) { _blue = b; OnPropertyChanged(nameof(Blue)); }

            SetHsbFromRgb((byte)Red, (byte)Green, (byte)Blue);

            var newHex = $"#{(byte)Red:X2}{(byte)Green:X2}{(byte)Blue:X2}";
            if (HexColor != newHex) HexColor = newHex;

            var newColor = Color.FromRgb((byte)Red, (byte)Green, (byte)Blue);
            if (Color != newColor) Color = newColor;

            if (HexInput != null && HexInput.Text != newHex) HexInput.Text = newHex;

            UpdateHsbText();
            UpdateHslText();
            UpdateCssText();

            OnPropertyChanged(nameof(RgbText));
            UpdateAllVisuals();
        }
        finally
        {
            ExitUpdateScope();
        }
    }

    /// <summary>从 RGB 重算并写入 HSB 状态（仅当差异超过容差时触发 PropertyChanged）。</summary>
    private void SetHsbFromRgb(byte r, byte g, byte b)
    {
        var next = HsbFromRgb(r, g, b);
        var prevH = _hsbH; var prevS = _hsbS; var prevB = _hsbB;
        _hsbH = next.H; _hsbS = next.S; _hsbB = next.B;
        if (HasSignificantDelta(prevH, next.H)) OnPropertyChanged(nameof(Hue));
        if (HasSignificantDelta(prevS, next.S)) OnPropertyChanged(nameof(Saturation));
        if (HasSignificantDelta(prevB, next.B)) OnPropertyChanged(nameof(Brightness));
    }

    /// <summary>从 RGB 重算并写入 HSL 状态（仅当差异超过容差时触发 PropertyChanged）。</summary>
    private void SetHslFromRgb(byte r, byte g, byte b)
    {
        var next = HslFromRgb(r, g, b);
        var prevH = _hslH; var prevS = _hslS; var prevL = _hslL;
        _hslH = next.H; _hslS = next.S; _hslL = next.L;
        if (HasSignificantDelta(prevH, next.H)) OnPropertyChanged(nameof(HslHue));
        if (HasSignificantDelta(prevS, next.S)) OnPropertyChanged(nameof(HslSaturation));
        if (HasSignificantDelta(prevL, next.L)) OnPropertyChanged(nameof(Lightness));
    }

    /// <summary>
    /// 判断两个 double 是否"显著不同"：使用严格 ==（NaN != NaN），
    /// 当任一为 NaN 或差值超过容差时认为不同，强制广播以恢复 XAML 显示。
    /// </summary>
    private static bool HasSignificantDelta(double a, double b)
    {
        if (double.IsNaN(a) || double.IsNaN(b)) return true;
        return Math.Abs(a - b) > 0.001;
    }

    /// <summary>HSB → (R,G,B) 0-255。</summary>
    private static (double R, double G, double B) HsbToRgb(double h, double s, double b)
    {
        h = ((h % 360) + 360) % 360;
        s = Math.Clamp(s, 0, 100) / 100.0;
        b = Math.Clamp(b, 0, 100) / 100.0;

        double c = b * s;
        double x = c * (1 - Math.Abs(((h / 60.0) % 2) - 1));
        double m = b - c;
        double r, g, bl;
        if (h < 60) { r = c; g = x; bl = 0; }
        else if (h < 120) { r = x; g = c; bl = 0; }
        else if (h < 180) { r = 0; g = c; bl = x; }
        else if (h < 240) { r = 0; g = x; bl = c; }
        else if (h < 300) { r = x; g = 0; bl = c; }
        else { r = c; g = 0; bl = x; }
        return ((r + m) * 255, (g + m) * 255, (bl + m) * 255);
    }

    /// <summary>RGB → HSB (Hue 0-360, S/B 0-100)。</summary>
    private static (double H, double S, double B) HsbFromRgb(byte r, byte g, byte b)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;
        double hue = 0, sat, bri = max;

        if (delta > 0.00001)
        {
            if (max == rd) hue = 60 * (((gd - bd) / delta) % 6);
            else if (max == gd) hue = 60 * (((bd - rd) / delta) + 2);
            else hue = 60 * (((rd - gd) / delta) + 4);
            if (hue < 0) hue += 360;
            sat = delta / max;
        }
        else
        {
            sat = 0;
        }
        return (hue, sat * 100, bri * 100);
    }

    /// <summary>HSL → (R,G,B) 0-255。</summary>
    private static (double R, double G, double B) HslToRgb(double h, double s, double l)
    {
        h = ((h % 360) + 360) % 360;
        s = Math.Clamp(s, 0, 100) / 100.0;
        l = Math.Clamp(l, 0, 100) / 100.0;

        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs(((h / 60.0) % 2) - 1));
        double m = l - c / 2.0;
        double r, g, bl;
        if (h < 60) { r = c; g = x; bl = 0; }
        else if (h < 120) { r = x; g = c; bl = 0; }
        else if (h < 180) { r = 0; g = c; bl = x; }
        else if (h < 240) { r = 0; g = x; bl = c; }
        else if (h < 300) { r = x; g = 0; bl = c; }
        else { r = c; g = 0; bl = x; }
        return ((r + m) * 255, (g + m) * 255, (bl + m) * 255);
    }

    /// <summary>RGB → HSL (Hue 0-360, S/L 0-100)。</summary>
    private static (double H, double S, double L) HslFromRgb(byte r, byte g, byte b)
    {
        double rd = r / 255.0, gd = g / 255.0, bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;
        double hue = 0, sat = 0, lt = (max + min) / 2.0;

        if (delta > 0.00001)
        {
            if (max == rd) hue = 60 * (((gd - bd) / delta) % 6);
            else if (max == gd) hue = 60 * (((bd - rd) / delta) + 2);
            else hue = 60 * (((rd - gd) / delta) + 4);
            if (hue < 0) hue += 360;
            sat = delta / (1 - Math.Abs(2 * lt - 1));
        }

        return (hue, sat * 100, lt * 100);
    }

    /// <summary>
    /// 解析 HSB 文本字符串，更新内部 HSB 状态。
    /// 支持格式：hsb(H, S%, B%) 或 hsv(H, S%, B%)
    /// </summary>
    private bool TryParseHsbText(string text)
    {
        try
        {
            var trimmed = text.Trim();
            if (!trimmed.EndsWith(")")) return false;

            int openParen = trimmed.IndexOf('(');
            if (openParen < 0) return false;
            var prefix = trimmed[..openParen].ToLowerInvariant();
            if (prefix != "hsb" && prefix != "hsv") return false;

            var inner = trimmed[(openParen + 1)..^1];
            var parts = inner.Split(',');
            if (parts.Length != 3) return false;

            if (!double.TryParse(parts[0].Trim(), out double h)) return false;
            var sStr = parts[1].Trim().TrimEnd('%');
            if (!double.TryParse(sStr, out double s)) return false;
            var bStr = parts[2].Trim().TrimEnd('%');
            if (!double.TryParse(bStr, out double b)) return false;

            h = Math.Clamp(h, 0, 360);
            s = Math.Clamp(s, 0, 100);
            b = Math.Clamp(b, 0, 100);

            var (r, g, bl) = HsbToRgb(h, s, b);
            var newColor = Color.FromRgb((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(bl, 0, 255));
            // 直接调用 SyncFromColor(force: true) 而非通过 Color 赋值，避免被 _isUpdatingDepth 守卫短路
            SyncFromColor(newColor, force: true);
            if (Color != newColor) Color = newColor;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 解析 HSL 文本字符串，更新 Color。
    /// 支持格式：hsl(H, S%, L%)
    /// </summary>
    private bool TryParseHslText(string text)
    {
        try
        {
            var trimmed = text.Trim();
            if (!trimmed.EndsWith(")")) return false;

            int openParen = trimmed.IndexOf('(');
            if (openParen < 0) return false;
            var prefix = trimmed[..openParen].ToLowerInvariant();
            if (prefix != "hsl") return false;

            var inner = trimmed[(openParen + 1)..^1];
            var parts = inner.Split(',');
            if (parts.Length != 3) return false;

            if (!double.TryParse(parts[0].Trim(), out double h)) return false;
            var sStr = parts[1].Trim().TrimEnd('%');
            if (!double.TryParse(sStr, out double s)) return false;
            var lStr = parts[2].Trim().TrimEnd('%');
            if (!double.TryParse(lStr, out double l)) return false;

            h = Math.Clamp(h, 0, 360);
            s = Math.Clamp(s, 0, 100);
            l = Math.Clamp(l, 0, 100);

            var (r, g, b) = HslToRgb(h, s, l);
            var newColor = Color.FromRgb((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
            // 直接调用 SyncFromColor(force: true) 而非通过 Color 赋值，避免被 _isUpdatingDepth 守卫短路
            SyncFromColor(newColor, force: true);
            if (Color != newColor) Color = newColor;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 解析 CSS rgba 文本字符串，更新 Color 和 Alpha。
    /// 支持格式：rgba(R, G, B, A) 或 rgb(R, G, B)
    /// </summary>
    private bool TryParseCssText(string text)
    {
        try
        {
            var trimmed = text.Trim();
            if (!trimmed.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                return false;

            var inner = trimmed[trimmed.IndexOf('(')..];
            if (!inner.EndsWith(")")) return false;
            inner = inner[1..^1];

            var parts = inner.Split(',');
            if (parts.Length != 3 && parts.Length != 4) return false;

            if (!double.TryParse(parts[0].Trim(), out double r)) return false;
            if (!double.TryParse(parts[1].Trim(), out double g)) return false;
            if (!double.TryParse(parts[2].Trim(), out double b)) return false;

            r = Math.Clamp(r, 0, 255);
            g = Math.Clamp(g, 0, 255);
            b = Math.Clamp(b, 0, 255);

            var newColor = Color.FromRgb((byte)r, (byte)g, (byte)b);
            // 直接调用 SyncFromColor(force: true) 而非通过 Color 赋值，避免被 _isUpdatingDepth 守卫短路
            SyncFromColor(newColor, force: true);
            if (Color != newColor) Color = newColor;

            if (parts.Length == 4)
            {
                if (!double.TryParse(parts[3].Trim(), out double a)) return false;
                a = Math.Clamp(a, 0, 1);
                var newAlpha = a * 100;
                if (Math.Abs(Alpha - newAlpha) > 0.001) Alpha = newAlpha;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 更新 HslText StyledProperty。
    /// </summary>
    private void UpdateHslText()
    {
        var text = $"hsl({_hslH:F0},{_hslS:F0}%,{_hslL:F0}%)";
        if (HslText != text) HslText = text;
    }

    /// <summary>
    /// 更新 HsbText StyledProperty。
    /// </summary>
    private void UpdateHsbText()
    {
        var text = $"hsb({_hsbH:F0},{_hsbS:F0}%,{_hsbB:F0}%)";
        if (HsbText != text) HsbText = text;
    }

    /// <summary>
    /// 更新 CssText StyledProperty。
    /// </summary>
    private void UpdateCssText()
    {
        double a = Math.Round(Alpha / 100.0, 2);
        var text = $"rgba({Color.R:F0},{Color.G:F0},{Color.B:F0},{a:0.##})";
        if (CssText != text) CssText = text;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void UpdateAllVisuals()
    {
        UpdateSaturationGradient();
        UpdateBrightnessGradient();
        UpdateHueGradient();
        UpdateAlphaGradient();
        UpdateSelectors();
        ApplyFormatMode();
    }

    private void UpdateSaturationGradient()
    {
        if (SaturationGradient == null) return;
        var (r, g, b) = HsbToRgb(_hsbH, 100, 100);
        var hueColor = Color.FromRgb((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
        SaturationGradient.Fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(hueColor, 1)
            }
        };
    }

    private void UpdateBrightnessGradient()
    {
        if (BrightnessGradient == null) return;
        switch(ActiveFormat)
        {
            case BaseColorPickerPanel.ColorFormatTab.Hsl:
                // HSL 模式：第三分量 = Lightness
                var (r0, g0, b0) = HslToRgb(_hslH, 100, 0);
                var (r1, g1, b1) = HslToRgb(_hslH, 100, 100);
                var colorL0 = Color.FromRgb((byte)Math.Clamp(r0, 0, 255), (byte)Math.Clamp(g0, 0, 255), (byte)Math.Clamp(b0, 0, 255));
                var colorL100 = Color.FromRgb((byte)Math.Clamp(r1, 0, 255), (byte)Math.Clamp(g1, 0, 255), (byte)Math.Clamp(b1, 0, 255));
                BrightnessGradient.Fill = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                {
                    new GradientStop(colorL0, 0),
                    new GradientStop(colorL100, 1)
                }
                };
                break;
            case BaseColorPickerPanel.ColorFormatTab.Hsb:
                // HSB 模式：第三分量 = Brightness
                var (r, g, b) = HsbToRgb(_hsbH, _hsbS, 100);
                var hueColor = Color.FromRgb((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
                BrightnessGradient.Fill = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(hueColor, 0),
                        new GradientStop(Colors.Transparent, 1)
                    }
                };
                break;
            default:
                // RGB / Css 模式：以当前颜色为右端，黑色为左端，构成亮度条
                var picked = Color.FromRgb(
                    Color.R,
                    Color.G,
                    Color.B);
                BrightnessGradient.Fill = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Colors.Black, 0),
                        new GradientStop(picked, 1)
                    }
                };
                break;
        }
    }

    private void UpdateHueGradient()
    {
        if (HueGradient == null) return;
        HueGradient.Fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Color.FromRgb(255, 0, 0), 0),
                new GradientStop(Color.FromRgb(255, 255, 0), 0.167),
                new GradientStop(Color.FromRgb(0, 255, 0), 0.333),
                new GradientStop(Color.FromRgb(0, 255, 255), 0.5),
                new GradientStop(Color.FromRgb(0, 0, 255), 0.667),
                new GradientStop(Color.FromRgb(255, 0, 255), 0.833),
                new GradientStop(Color.FromRgb(255, 0, 0), 1)
            }
        };
    }

    private void UpdateAlphaGradient()
    {
        if (AlphaGradient == null) return;
        var color = Color.FromArgb(255, Color.R, Color.G, Color.B);

        AlphaGradient.Fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(color, 1)
            }
        };
    }

    private void UpdateSelectors()
    {
        Dispatcher.UIThread.Post(() =>
        {
            // 共享拾色面板：根据当前模式选不同 selector
            if (PickerHostBorder != null && PickerHostBorder.Bounds.Width > 0)
            {
                if (ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl)
                {
                    if (HslPickerSelector != null)
                    {
                        var x = HslSaturation / 100.0 * PickerHostBorder.Bounds.Width - 8;
                        var y = (1 - Lightness / 100.0) * PickerHostBorder.Bounds.Height - 8;
                        HslPickerSelector.Margin = new Thickness(x, y, 0, 0);
                    }
                }
                else
                {
                    if (SaturationSelector != null)
                    {
                        var x = Saturation / 100.0 * PickerHostBorder.Bounds.Width - 8;
                        var y = (1 - Brightness / 100.0) * PickerHostBorder.Bounds.Height - 8;
                        SaturationSelector.Margin = new Thickness(x, y, 0, 0);
                    }
                }
            }

            // 共享色相条 thumb：根据模式显示不同
            if (HueTrack != null && HueTrack.Bounds.Width > 0)
            {
                if (ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl)
                {
                    if (HslHueThumb != null)
                    {
                        var x = (HslHue / 360.0) * HueTrack.Bounds.Width - 4;
                        HslHueThumb.Margin = new Thickness(x, 0, 0, 0);
                    }
                }
                else
                {
                    if (HueThumb != null)
                    {
                        var x = (Hue / 360.0) * HueTrack.Bounds.Width - 4;
                        HueThumb.Margin = new Thickness(x, 0, 0, 0);
                    }
                }
            }

            // 共享"第三分量" track thumb：根据模式显示不同
            if (BrightnessTrack != null && BrightnessTrack.Bounds.Width > 0)
            {
                if (ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl)
                {
                    if (LightnessThumb != null)
                    {
                        var x = (Lightness / 100.0) * BrightnessTrack.Bounds.Width - 4;
                        LightnessThumb.Margin = new Thickness(x, 0, 0, 0);
                    }
                }
                else
                {
                    if (BrightnessThumb != null)
                    {
                        var x = (Brightness / 100.0) * BrightnessTrack.Bounds.Width - 4;
                        BrightnessThumb.Margin = new Thickness(x, 0, 0, 0);
                    }
                }
            }

            // HSL 饱和度 slider（仅 HSL 模式显示）
            if (HslSaturationTrack != null && HslSaturationTrack.Bounds.Width > 0 && HslSaturationThumb != null)
            {
                var x = (HslSaturation / 100.0) * HslSaturationTrack.Bounds.Width - 4;
                HslSaturationThumb.Margin = new Thickness(x, 0, 0, 0);
            }

            // HSB 饱和度 slider（仅 HSB 模式显示）
            if (HsbSaturationTrack != null && HsbSaturationTrack.Bounds.Width > 0 && HsbSaturationThumb != null)
            {
                var x = (Saturation / 100.0) * HsbSaturationTrack.Bounds.Width - 4;
                HsbSaturationThumb.Margin = new Thickness(x, 0, 0, 0);
            }

            if (AlphaThumb != null && AlphaTrack != null && AlphaTrack.Bounds.Width > 0)
            {
                var x = ((Alpha - 1) / 99.0) * AlphaTrack.Bounds.Width - 4;
                AlphaThumb.Margin = new Thickness(x, 0, 0, 0);
            }
        });
    }

    /// <summary>HSL 拾色面板：底层纯色 + 白→透明 + 黑→透明。</summary>
    private void UpdateHslSaturationGradient()
    {
        if (HslPickerBase == null) return;
        var (rr, gg, bb) = HslToRgb(_hslH, 100, 50);
        HslPickerBase.Fill = new SolidColorBrush(
            Color.FromRgb((byte)Math.Clamp(rr, 0, 255), (byte)Math.Clamp(gg, 0, 255), (byte)Math.Clamp(bb, 0, 255)));
    }

    private void UpdateHslSaturationTrackGradient()
    {
        if (HslSaturationTrackGradient == null) return;
        var (rr, gg, bb) = HslToRgb(_hslH, 100, 50);
        var colorRight = Color.FromRgb((byte)Math.Clamp(rr, 0, 255), (byte)Math.Clamp(gg, 0, 255), (byte)Math.Clamp(bb, 0, 255));
        HslSaturationTrackGradient.Fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                new GradientStop(Color.FromRgb(128, 128, 128), 0),
                new GradientStop(colorRight, 1)
            }
        };
    }

    private void UpdateHsbSaturationTrackGradient()
    {
        if (HsbSaturationTrackGradient == null) return;
        var (rr, gg, bb) = HsbToRgb(_hsbH, 100, _hsbB);
        var colorRight = Color.FromRgb((byte)Math.Clamp(rr, 0, 255), (byte)Math.Clamp(gg, 0, 255), (byte)Math.Clamp(bb, 0, 255));
        HsbSaturationTrackGradient.Fill = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops = new GradientStops
            {
                // S=0 时取当前 H/B 的灰阶（等亮去饱和），S=100 时为当前 H/B 满饱和色
                new GradientStop(Color.FromRgb((byte)Math.Clamp(_hsbB * 2.55, 0, 255), (byte)Math.Clamp(_hsbB * 2.55, 0, 255), (byte)Math.Clamp(_hsbB * 2.55, 0, 255)), 0),
                new GradientStop(colorRight, 1)
            }
        };
    }

    /// <summary>根据当前 ActiveFormat 切换 UI 显隐（共享控件的可见性、数值列显隐、面板底色、文本标签等）。</summary>
    private void ApplyFormatMode()
    {
        var isHsl = ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl;

        if (HsbPickerPanel != null) HsbPickerPanel.IsVisible = !isHsl;
        if (HslPickerPanel != null) HslPickerPanel.IsVisible = isHsl;

        if (HslHueThumb != null) HslHueThumb.IsVisible = isHsl;
        if (LightnessThumb != null) LightnessThumb.IsVisible = isHsl;

        if (HueThumb != null) HueThumb.IsVisible = !isHsl;
        if (BrightnessThumb != null) BrightnessThumb.IsVisible = !isHsl;

        //if (HsbNumberRow != null) HsbNumberRow.IsVisible = !isHsl;
        //if (HslNumberRow != null) HslNumberRow.IsVisible = isHsl;
        //if (HslSaturationRow != null) HslSaturationRow.IsVisible = isHsl;
        //if (HslSaturationRow != null)
        //{
        //    HslSaturationRow.IsEnabled = isHsl;
        //    PART_HSLMask.IsVisible = !isHsl;
        //}

        //if (BrightnessLabel != null) BrightnessLabel.Text = isHsl ? "亮度 (HSL)" : "亮度";
        //if (BrightnessValueText != null)
        //    BrightnessValueText.Text = isHsl
        //        ? $"{Lightness:F0}%"
        //        : $"{Brightness:F0}%";
        //if (HueValueText != null)
        //    HueValueText.Text = isHsl
        //        ? $"{HslHue:F0}°"
        //        : $"{Hue:F0}°";

        if (FormatComboLabel != null)
        {
            switch (ActiveFormat)
            {
                case BaseColorPickerPanel.ColorFormatTab.Hsl:
                    if (HsbNumberRow != null) HsbNumberRow.IsVisible = false;
                    if (HslNumberRow != null) HslNumberRow.IsVisible = true;
                    if (RGBNumberRow != null) RGBNumberRow.IsVisible = false;
                    if (PART_HSLMask != null) PART_HSLMask.IsVisible = false;
                    if (HslSaturationRow != null)
                    {
                        HslSaturationRow.IsVisible = true;
                        HslSaturationRow.IsEnabled = true;
                    }
                    if (HsbSaturationRow != null)
                    {
                        HsbSaturationRow.IsVisible = false;
                        HsbSaturationRow.IsEnabled = false;
                    }

                    if (BrightnessLabel != null) BrightnessLabel.Text = "亮度(Lightness)";
                    if (BrightnessValueText != null) BrightnessValueText.Text = $"{Lightness:F0}%";
                    if (HueValueText != null) HueValueText.Text = $"{HslHue:F0}°";

                    if (LightnessThumb != null) LightnessThumb.IsVisible = true ;
                    if (BrightnessThumb != null) BrightnessThumb.IsVisible = false;
                    FormatComboLabel.Text = "HSL";
                    break;

                case BaseColorPickerPanel.ColorFormatTab.RGB:
                    if (HsbNumberRow != null) HsbNumberRow.IsVisible = false;
                    if (HslNumberRow != null) HslNumberRow.IsVisible = false;
                    if (RGBNumberRow != null) RGBNumberRow.IsVisible = true;
                    if (PART_HSLMask != null) PART_HSLMask.IsVisible = true;
                    if (HslSaturationRow != null)
                    {
                        HslSaturationRow.IsVisible = false;
                        HslSaturationRow.IsEnabled = false;
                    }
                    if (HsbSaturationRow != null)
                    {
                        HsbSaturationRow.IsVisible = false;
                        HsbSaturationRow.IsEnabled = false;
                    }

                    if (BrightnessLabel != null) BrightnessLabel.Text = "亮度";
                    if (BrightnessValueText != null) BrightnessValueText.Text = $"{Brightness:F0}%";
                    if (HueValueText != null) HueValueText.Text = $"{Hue:F0}°";

                    if (LightnessThumb != null) LightnessThumb.IsVisible = false ;
                    if (BrightnessThumb != null) BrightnessThumb.IsVisible = true;
                    FormatComboLabel.Text = "RGB";
                    break;
                case BaseColorPickerPanel.ColorFormatTab.Hsb:
                    if (HsbNumberRow != null) HsbNumberRow.IsVisible = true;
                    if (HslNumberRow != null) HslNumberRow.IsVisible = false;
                    if (RGBNumberRow != null) RGBNumberRow.IsVisible = false;
                    if (PART_HSLMask != null) PART_HSLMask.IsVisible = true;
                    if (HslSaturationRow != null)
                    {
                        HslSaturationRow.IsVisible = false;
                        HslSaturationRow.IsEnabled = false;
                    }
                    if (HsbSaturationRow != null)
                    {
                        HsbSaturationRow.IsVisible = true;
                        HsbSaturationRow.IsEnabled = true;
                    }

                    if (BrightnessLabel != null) BrightnessLabel.Text = "亮度(Brightness)";
                    if (BrightnessValueText != null) BrightnessValueText.Text = $"{Brightness:F0}%";
                    if (HueValueText != null) HueValueText.Text = $"{Hue:F0}°";

                    if (LightnessThumb != null) LightnessThumb.IsVisible = false;
                    if (BrightnessThumb != null) BrightnessThumb.IsVisible = true;
                    FormatComboLabel.Text = "HSB";
                    break;
            }
        }

        // 重绘背景渐变（取色面板 + 滑块）
        UpdateSaturationGradient();
        UpdateBrightnessGradient();
        UpdateHslSaturationGradient();
        UpdateHslSaturationTrackGradient();
        UpdateHsbSaturationTrackGradient();
    }

    #region Interactions

    private void SetupInteractions()
    {
        if (HueTrack != null)
        {
            // 单一 track 接收两种模式的拖拽，根据当前 ActiveFormat 分派目标属性
            HueTrack.PointerPressed += (s, e) =>
            {
                _isDraggingHue = true;
                e.Pointer.Capture(HueTrack);
                UpdateHueFromPointer(e.GetPosition(HueTrack).X);
                e.Handled = true;
            };
            HueTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingHue) UpdateHueFromPointer(e.GetPosition(HueTrack).X);
            };
            HueTrack.PointerReleased += (s, e) => { _isDraggingHue = false; e.Pointer.Capture(null); };
        }

        if (BrightnessTrack != null)
        {
            // 单一 track 接收 HSB 亮度 / HSL 亮度的拖拽，根据当前 ActiveFormat 分派目标属性
            BrightnessTrack.PointerPressed += (s, e) =>
            {
                _isDraggingBrightness = true;
                e.Pointer.Capture(BrightnessTrack);
                UpdateBrightnessFromPointer(e.GetPosition(BrightnessTrack).X);
                e.Handled = true;
            };
            BrightnessTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingBrightness) UpdateBrightnessFromPointer(e.GetPosition(BrightnessTrack).X);
            };
            BrightnessTrack.PointerReleased += (s, e) => { _isDraggingBrightness = false; e.Pointer.Capture(null); };
        }

        if (PickerHostBorder != null)
        {
            // 共享拾色面板 host：无论 HSB / HSL 子面板是否可见，host 都接收指针事件
            PickerHostBorder.PointerPressed += (s, e) =>
            {
                _isDraggingSaturation = true;
                e.Pointer.Capture(PickerHostBorder);
                UpdatePickerFromPointer(e.GetPosition(PickerHostBorder));
                e.Handled = true;
            };
            PickerHostBorder.PointerMoved += (s, e) =>
            {
                if (_isDraggingSaturation && e.Pointer.Captured == PickerHostBorder)
                    UpdatePickerFromPointer(e.GetPosition(PickerHostBorder));
            };
            PickerHostBorder.PointerReleased += (s, e) => { _isDraggingSaturation = false; e.Pointer.Capture(null); };
        }

        if (HslSaturationTrack != null)
        {
            HslSaturationTrack.PointerPressed += (s, e) =>
            {
                _isDraggingHslSatSlider = true;
                e.Pointer.Capture(HslSaturationTrack);
                UpdateHslSaturationSliderFromPointer(e.GetPosition(HslSaturationTrack).X);
                e.Handled = true;
            };
            HslSaturationTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingHslSatSlider) UpdateHslSaturationSliderFromPointer(e.GetPosition(HslSaturationTrack).X);
            };
            HslSaturationTrack.PointerReleased += (s, e) => { _isDraggingHslSatSlider = false; e.Pointer.Capture(null); };
        }

        if (HsbSaturationTrack != null)
        {
            HsbSaturationTrack.PointerPressed += (s, e) =>
            {
                _isDraggingHsbSatSlider = true;
                e.Pointer.Capture(HsbSaturationTrack);
                UpdateHsbSaturationSliderFromPointer(e.GetPosition(HsbSaturationTrack).X);
                e.Handled = true;
            };
            HsbSaturationTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingHsbSatSlider) UpdateHsbSaturationSliderFromPointer(e.GetPosition(HsbSaturationTrack).X);
            };
            HsbSaturationTrack.PointerReleased += (s, e) => { _isDraggingHsbSatSlider = false; e.Pointer.Capture(null); };
        }

        if (AlphaTrack != null)
        {
            AlphaTrack.PointerPressed += (s, e) =>
            {
                _isDraggingAlpha = true;
                e.Pointer.Capture(AlphaTrack);
                UpdateAlphaFromPointer(e.GetPosition(AlphaTrack).X);
                e.Handled = true;
            };
            AlphaTrack.PointerMoved += (s, e) =>
            {
                if (_isDraggingAlpha) UpdateAlphaFromPointer(e.GetPosition(AlphaTrack).X);
            };
            AlphaTrack.PointerReleased += (s, e) =>
            {
                _isDraggingAlpha = false;
                e.Pointer.Capture(null);
            };
        }

        if (HexInput != null)
        {
            // 双向绑定 HexColor 字符串与 Hex 输入框，避免重复 TextChanged 处理逻辑
            HexInput.Text = HexColor;
            HexInput.TextChanged += (s, e) =>
            {
                // 防止 Update* / SyncFromColor 内部触发的 HexInput.Text 写入再次进入 SetColor 形成重入
                if (_isUpdatingDepth > 0) return;
                var text = HexInput.Text;
                if (string.IsNullOrEmpty(text)) return;
                text = text.TrimStart('#');
                if ((text.Length == 6 || text.Length == 8) && text.All(c => "0123456789ABCDEFabcdef".Contains(c)))
                {
                    SetColor(text);
                }
            };
        }
    }

    private void UpdatePickerFromPointer(Point position)
    {
        var host = PickerHostBorder;
        if (host == null) return;
        var bounds = host.Bounds;
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        if (ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl)
        {
            HslSaturation = Math.Round(Math.Clamp(position.X / bounds.Width, 0, 1) * 100);
            Lightness = Math.Round((1 - Math.Clamp(position.Y / bounds.Height, 0, 1)) * 100);
        }
        else
        {
            Saturation = Math.Round(Math.Clamp(position.X / bounds.Width, 0, 1) * 100);
            Brightness = Math.Round((1 - Math.Clamp(position.Y / bounds.Height, 0, 1)) * 100);
        }
    }

    private void UpdateHueFromPointer(double x)
    {
        if (HueTrack == null) return;
        var bounds = HueTrack.Bounds;
        if (bounds.Width <= 0) return;
        var value = Math.Round(Math.Clamp(x / bounds.Width, 0, 1) * 360);
        if (ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl)
            HslHue = value;
        else
            Hue = value;
    }

    private void UpdateBrightnessFromPointer(double x)
    {
        if (BrightnessTrack == null) return;
        var bounds = BrightnessTrack.Bounds;
        if (bounds.Width <= 0) return;
        var value = Math.Round(Math.Clamp(x / bounds.Width, 0, 1) * 100);
        if (ActiveFormat == BaseColorPickerPanel.ColorFormatTab.Hsl)
            Lightness = value;
        else
            Brightness = value;
    }

    private void UpdateAlphaFromPointer(double x)
    {
        if (AlphaTrack == null) return;
        var bounds = AlphaTrack.Bounds;
        if (bounds.Width <= 0) return;
        Alpha = Math.Round(Math.Clamp(x / bounds.Width, 0, 1) * 99 + 1);
    }

    private void UpdateHslSaturationSliderFromPointer(double x)
    {
        if (HslSaturationTrack == null) return;
        var bounds = HslSaturationTrack.Bounds;
        if (bounds.Width <= 0) return;
        HslSaturation = Math.Round(Math.Clamp(x / bounds.Width, 0, 1) * 100);
    }

    private void UpdateHsbSaturationSliderFromPointer(double x)
    {
        if (HsbSaturationTrack == null) return;
        var bounds = HsbSaturationTrack.Bounds;
        if (bounds.Width <= 0) return;
        Saturation = Math.Round(Math.Clamp(x / bounds.Width, 0, 1) * 100);
    }

    #endregion

    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void PresetColor_Click(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Tag is string colorHex)
        {
            SetColor(colorHex);
        }
    }

    private void HexInput_ModeChanged(object? sender, yuyiAvalonia.Base.Controls.ColorHexInput.ColorInputMode e)
    {
        switch (e)
        {
            case ColorHexInput.ColorInputMode.Hex:
                PART_ColorModeTitle.Text = "Hex";
                break;
            case ColorHexInput.ColorInputMode.Css:
                PART_ColorModeTitle.Text = "CSS";
                break;
            default:
                PART_ColorModeTitle.Text = "";
                break;
        }
    }
}
