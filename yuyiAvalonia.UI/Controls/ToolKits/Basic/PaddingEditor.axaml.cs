using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class PaddingEditor : UserControl
{
    /// <summary>属性变更事件，供父容器订阅。</summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    private const string MixedText = "Mixed";
    private bool _isSyncingFromElement;
    private bool _paddingPartChanged;

    public PaddingEditor()
    {
        InitializeComponent();

        PropertyChanged += PaddingEditor_PropertyChanged;

        Loaded += (_, _) =>
        {
            PART_PropertyBorder?.Attach(PART_Slider_Padding);
            PART_Padding.ValueChanged += PART_Padding_ValueChanged;
            SyncFromElement();
        };
    }

    private void PART_Padding_ValueChanged(object? sender, double e)
    {
        if (double.TryParse(PART_Padding.Text, out double padding) && !_paddingPartChanged)
        {
            Padding = padding;
            PaddingLeft = padding;
            PaddingTop = padding;
            PaddingRight = padding;
            PaddingBottom = padding;
        }
    }

    #region Element 双向同步

    private CanvasElement? _currentElement;

    private void PaddingEditor_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(ElementProperty):
                _currentElement = e.NewValue as CanvasElement;
                SyncFromElement();
                break;
            case nameof(PaddingLeft):
            case nameof(PaddingTop):
            case nameof(PaddingRight):
            case nameof(PaddingBottom):
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.PaddingIsUniform = false;
                    switch (e.Property.Name)
                    {
                        case nameof(PaddingLeft): _currentElement.PaddingLeft = PaddingLeft; break;
                        case nameof(PaddingTop): _currentElement.PaddingTop = PaddingTop; break;
                        case nameof(PaddingRight): _currentElement.PaddingRight = PaddingRight; break;
                        case nameof(PaddingBottom): _currentElement.PaddingBottom = PaddingBottom; break;
                    }
                }
                _paddingPartChanged = true;
                SyncFromElement();
                _paddingPartChanged = false;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name ?? string.Empty));
                break;
            case nameof(Padding):
                PART_Padding.Value = Padding;
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.PaddingIsUniform = true;
                    _currentElement.Padding = Padding;
                }
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Padding)));
                break;
        }
    }

    private void SyncFromElement()
    {
        if (_currentElement == null || _isSyncingFromElement) return;
        _isSyncingFromElement = true;

        if (_currentElement.PaddingIsUniform)
        {
            if (double.IsNaN(_currentElement.Padding))
            {
                PART_Padding.Value = 0;
                PART_Padding.Text = MixedText;
                Padding = 0;
                PART_Slider_Padding.Value = Padding;
            }
            else
            {
                Padding = _currentElement.Padding;
            }
        }
        else
        {
            PaddingLeft = _currentElement.PaddingLeft;
            PaddingTop = _currentElement.PaddingTop;
            PaddingRight = _currentElement.PaddingRight;
            PaddingBottom = _currentElement.PaddingBottom;

            double value = PaddingLeft;
            if (value == PaddingTop && value == PaddingRight && value == PaddingBottom)
            {
                Padding = value;
                PART_Padding.Text = Padding.ToString();
            }
            else
            {
                PART_Padding.Value = 0;
                PART_Padding.Text = MixedText;
                Padding = 0;
                PART_Slider_Padding.Value = Padding;
            }
        }

        _isSyncingFromElement = false;
    }

    #endregion

    #region Styled Properties

    /// <summary>绑定到 CanvasElement，用于双向同步。</summary>
    public static readonly StyledProperty<CanvasElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<PaddingEditor, CanvasElement?>(
            nameof(ElementProperty), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);

    public CanvasElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    /// <summary>UI 显示用中间属性，控制切换按钮和分开模式 Grid 的可见性。</summary>
    public static readonly StyledProperty<bool> IsShowPaddingIsUniformProperty =
        AvaloniaProperty.Register<PaddingEditor, bool>(nameof(IsShowPaddingIsUniform), false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsShowPaddingIsUniform
    {
        get => GetValue(IsShowPaddingIsUniformProperty);
        set => SetValue(IsShowPaddingIsUniformProperty, value);
    }

    /// <summary>统一内边距（Slider 绑定目标）。</summary>
    public static readonly StyledProperty<double> PaddingProperty =
        AvaloniaProperty.Register<PaddingEditor, double>(nameof(Padding),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    #endregion

    #region 分开模式四边独立属性

    public static readonly StyledProperty<double> PaddingLeftProperty =
        AvaloniaProperty.Register<PaddingEditor, double>(nameof(PaddingLeft),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double PaddingLeft
    {
        get => GetValue(PaddingLeftProperty);
        set => SetValue(PaddingLeftProperty, value);
    }

    public static readonly StyledProperty<double> PaddingTopProperty =
        AvaloniaProperty.Register<PaddingEditor, double>(nameof(PaddingTop),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double PaddingTop
    {
        get => GetValue(PaddingTopProperty);
        set => SetValue(PaddingTopProperty, value);
    }

    public static readonly StyledProperty<double> PaddingRightProperty =
        AvaloniaProperty.Register<PaddingEditor, double>(nameof(PaddingRight),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double PaddingRight
    {
        get => GetValue(PaddingRightProperty);
        set => SetValue(PaddingRightProperty, value);
    }

    public static readonly StyledProperty<double> PaddingBottomProperty =
        AvaloniaProperty.Register<PaddingEditor, double>(nameof(PaddingBottom),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double PaddingBottom
    {
        get => GetValue(PaddingBottomProperty);
        set => SetValue(PaddingBottomProperty, value);
    }

    #endregion
}
