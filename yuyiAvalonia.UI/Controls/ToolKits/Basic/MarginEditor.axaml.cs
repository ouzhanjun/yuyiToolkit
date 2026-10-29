using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls;

public partial class MarginEditor : UserControl
{
    /// <summary>属性变更事件，供父容器订阅。</summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    private const string MixedText = "Mixed";
    private bool _isSyncingFromElement;
    private bool _marginPartChanged;

    public MarginEditor()
    {
        InitializeComponent();

        PropertyChanged += MarginEditor_PropertyChanged;

        Loaded += (_, _) =>
        {
            PART_PropertyBorder?.Attach(PART_Slider_Margin);
            PART_Margin.ValueChanged += PART_Margin_ValueChanged;
            SyncFromElement();
        };
    }

    private void PART_Margin_ValueChanged(object? sender, double e)
    {
        if (double.TryParse(PART_Margin.Text, out double margin) && !_marginPartChanged)
        {
            Margin = margin;
            MarginLeft = margin;
            MarginTop = margin;
            MarginRight = margin;
            MarginBottom = margin;
        }
    }

    #region Element 双向同步

    private CanvasElement? _currentElement;

    private void MarginEditor_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(ElementProperty):
                _currentElement = e.NewValue as CanvasElement;
                SyncFromElement();
                break;
            case nameof(MarginLeft):
            case nameof(MarginTop):
            case nameof(MarginRight):
            case nameof(MarginBottom):
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.MarginIsUniform = false;
                    switch (e.Property.Name)
                    {
                        case nameof(MarginLeft): _currentElement.MarginLeft = MarginLeft; break;
                        case nameof(MarginTop): _currentElement.MarginTop = MarginTop; break;
                        case nameof(MarginRight): _currentElement.MarginRight = MarginRight; break;
                        case nameof(MarginBottom): _currentElement.MarginBottom = MarginBottom; break;
                    }
                }
                _marginPartChanged = true;
                //double value = MarginLeft;
                //if (value == MarginTop && value == MarginRight && value == MarginBottom)
                //{
                //    Margin = value;
                //    PART_Margin.Text = Margin.ToString();
                //}
                //else
                //{
                //    PART_Margin.Value = 0;
                //    PART_Margin.Text = MixedText;
                //    Margin = 0;
                //    PART_Slider_Margin.Value = Margin;
                //}
                SyncFromElement();
                _marginPartChanged = false;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name ?? string.Empty));
                break;
            case nameof(Margin):
                PART_Margin.Value = Margin;
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.MarginIsUniform = true;
                    _currentElement.Margin = Margin;
                }
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Margin)));
                break;
        }
    }

    private void SyncFromElement()
    {
        if (_currentElement == null || _isSyncingFromElement) return;
        _isSyncingFromElement = true;

        if (_currentElement.MarginIsUniform)
        {
            if (double.IsNaN(_currentElement.Margin))
            {
                PART_Margin.Value = 0;
                PART_Margin.Text = MixedText;
                Margin = 0;
                PART_Slider_Margin.Value= Margin;
            }
            else
            {
                Margin = _currentElement.Margin;
            }
        }
        else
        {
            MarginLeft = _currentElement.MarginLeft;
            MarginTop = _currentElement.MarginTop;
            MarginRight = _currentElement.MarginRight;
            MarginBottom = _currentElement.MarginBottom;

            double value = MarginLeft;
            if (value == MarginTop && value == MarginRight && value == MarginBottom)
            {
                Margin = value;
                PART_Margin.Text = Margin.ToString();
            }
            else
            {
                PART_Margin.Value = 0;
                PART_Margin.Text = MixedText;
                Margin = 0;
                PART_Slider_Margin.Value = Margin;
            }
        }

        _isSyncingFromElement = false;
    }

    #endregion

    #region Styled Properties

    /// <summary>绑定到 CanvasElement，用于双向同步。</summary>
    public static readonly StyledProperty<CanvasElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<MarginEditor, CanvasElement?>(
            nameof(ElementProperty), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);

    public CanvasElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    /// <summary>UI 显示用中间属性，控制切换按钮和分开模式 Grid 的可见性。</summary>
    public static readonly StyledProperty<bool> IsShowMarginIsUniformProperty =
        AvaloniaProperty.Register<MarginEditor, bool>(nameof(IsShowMarginIsUniform), false,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsShowMarginIsUniform
    {
        get => GetValue(IsShowMarginIsUniformProperty);
        set => SetValue(IsShowMarginIsUniformProperty, value);
    }

    /// <summary>统一外边距（Slider 绑定目标）。</summary>
    public static readonly StyledProperty<double> MarginProperty =
        AvaloniaProperty.Register<MarginEditor, double>(nameof(Margin),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double Margin
    {
        get => GetValue(MarginProperty);
        set => SetValue(MarginProperty, value);
    }

    #endregion

    #region 分开模式四边独立属性

    public static readonly StyledProperty<double> MarginLeftProperty =
        AvaloniaProperty.Register<MarginEditor, double>(nameof(MarginLeft),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double MarginLeft
    {
        get => GetValue(MarginLeftProperty);
        set => SetValue(MarginLeftProperty, value);
    }

    public static readonly StyledProperty<double> MarginTopProperty =
        AvaloniaProperty.Register<MarginEditor, double>(nameof(MarginTop),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double MarginTop
    {
        get => GetValue(MarginTopProperty);
        set => SetValue(MarginTopProperty, value);
    }

    public static readonly StyledProperty<double> MarginRightProperty =
        AvaloniaProperty.Register<MarginEditor, double>(nameof(MarginRight),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double MarginRight
    {
        get => GetValue(MarginRightProperty);
        set => SetValue(MarginRightProperty, value);
    }

    public static readonly StyledProperty<double> MarginBottomProperty =
        AvaloniaProperty.Register<MarginEditor, double>(nameof(MarginBottom),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double MarginBottom
    {
        get => GetValue(MarginBottomProperty);
        set => SetValue(MarginBottomProperty, value);
    }

    #endregion
}
