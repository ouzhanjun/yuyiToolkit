using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Elements;

namespace yuyiAvalonia.UI.Controls;

public partial class RoundedRectangleShapeEditor : UserControl
{
    /// <summary>属性变更事件，供父容器订阅。</summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    private const string MixedText = "Mixed";
    private bool _isSyncingFromElement;
    private bool _cornerPartChanged;

    public RoundedRectangleShapeEditor()
    {
        InitializeComponent();

        PropertyChanged += RoundedRectangleShapeEditor_PropertyChanged;

        Loaded += (_, _) =>
        {
            PART_PropertyBorder?.Attach(PART_Slider_Unified);
            PART_UnifiedInput.ValueChanged += PART_UnifiedInput_ValueChanged;
            SyncFromElement();
        };
    }

    private void PART_UnifiedInput_ValueChanged(object? sender, double e)
    {
        if (double.TryParse(PART_UnifiedInput.Text, out double radius) && !_cornerPartChanged)
        {
            // 统一值
            CornerRadius = radius;

            // 四角同步为同一值
            CornerRadiusLT = radius;
            CornerRadiusRT = radius;
            CornerRadiusRB = radius;
            CornerRadiusLB = radius;
        }
    }

    #region Element 双向同步

    private RoundedRectangleElement? _currentElement;

    private void RoundedRectangleShapeEditor_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(ElementProperty):
                _currentElement = e.NewValue as RoundedRectangleElement;
                SyncFromElement();
                break;
            case nameof(CornerRadiusLT):
            case nameof(CornerRadiusRT):
            case nameof(CornerRadiusRB):
            case nameof(CornerRadiusLB):
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.CornerRadiusIsUniform = false;
                    switch (e.Property.Name)
                    {
                        case nameof(CornerRadiusLT): _currentElement.R0 = CornerRadiusLT; break;
                        case nameof(CornerRadiusRT): _currentElement.R1 = CornerRadiusRT; break;
                        case nameof(CornerRadiusRB): _currentElement.R2 = CornerRadiusRB; break;
                        case nameof(CornerRadiusLB): _currentElement.R3 = CornerRadiusLB; break;
                    }
                }
                _cornerPartChanged = true;
                SyncFromElement();
                _cornerPartChanged = false;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name ?? string.Empty));
                break;
            case nameof(CornerRadius):
                PART_UnifiedInput.Value = CornerRadius;
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.CornerRadiusIsUniform = true;
                    _currentElement.CornerRadius = CornerRadius;
                }
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CornerRadius)));
                break;
            case nameof(DrawPoint):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.DrawPoint = DrawPoint;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawPoint)));
                break;
            case nameof(DrawVertex):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.DrawVertex = DrawVertex;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawVertex)));
                break;
            case nameof(DrawTangent):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.DrawTangent = DrawTangent;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawTangent)));
                break;
            case nameof(DrawCenter):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.DrawCenter = DrawCenter;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawCenter)));
                break;
            case nameof(DrawVertexLine):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.DrawVertexLine = DrawVertexLine;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawVertexLine)));
                break;
        }
    }

    private void SyncFromElement()
    {
        if (_currentElement == null || _isSyncingFromElement) return;
        _isSyncingFromElement = true;

        if (_currentElement.CornerRadiusIsUniform)
        {
            if (double.IsNaN(_currentElement.CornerRadius))
            {
                PART_UnifiedInput.Value = 0;
                PART_UnifiedInput.Text = MixedText;
                CornerRadius = 0;
                PART_Slider_Unified.Value = CornerRadius;
            }
            else
            {
                CornerRadius = _currentElement.CornerRadius;
            }
        }
        else
        {
            CornerRadiusLT = _currentElement.R0;
            CornerRadiusRT = _currentElement.R1;
            CornerRadiusRB = _currentElement.R2;
            CornerRadiusLB = _currentElement.R3;

            double lt = CornerRadiusLT;
            if (lt == CornerRadiusRT && lt == CornerRadiusRB && lt == CornerRadiusLB)
            {
                CornerRadius = lt;
                PART_UnifiedInput.Text = CornerRadius.ToString();
            }
            else
            {
                PART_UnifiedInput.Value = 0;
                PART_UnifiedInput.Text = MixedText;
                CornerRadius = 0;
                PART_Slider_Unified.Value = CornerRadius;
            }
        }

        // 同步调试点属性
        DrawPoint = _currentElement.DrawPoint;
        DrawVertex = _currentElement.DrawVertex;
        DrawTangent = _currentElement.DrawTangent;
        DrawCenter = _currentElement.DrawCenter;
        DrawVertexLine = _currentElement.DrawVertexLine;

        _isSyncingFromElement = false;
    }

    #endregion

    #region Styled Properties

    public static readonly StyledProperty<RoundedRectangleElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, RoundedRectangleElement?>(
            nameof(ElementProperty), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);

    public RoundedRectangleElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    /// <summary>UI 显示用中间属性，控制切换按钮和分开模式 Grid 的可见性。</summary>
    public static readonly StyledProperty<bool> IsShowCornerRadiusIsUniformProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, bool>(nameof(IsShowCornerRadiusIsUniform), true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsShowCornerRadiusIsUniform
    {
        get => GetValue(IsShowCornerRadiusIsUniformProperty);
        set => SetValue(IsShowCornerRadiusIsUniformProperty, value);
    }

    /// <summary>统一圆角（Slider 绑定目标）。</summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, double>(nameof(CornerRadius),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region 分开模式四角独立属性

    public static readonly StyledProperty<double> CornerRadiusLTProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, double>(nameof(CornerRadiusLT),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadiusLT
    {
        get => GetValue(CornerRadiusLTProperty);
        set => SetValue(CornerRadiusLTProperty, value);
    }

    public static readonly StyledProperty<double> CornerRadiusRTProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, double>(nameof(CornerRadiusRT),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadiusRT
    {
        get => GetValue(CornerRadiusRTProperty);
        set => SetValue(CornerRadiusRTProperty, value);
    }

    public static readonly StyledProperty<double> CornerRadiusRBProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, double>(nameof(CornerRadiusRB),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadiusRB
    {
        get => GetValue(CornerRadiusRBProperty);
        set => SetValue(CornerRadiusRBProperty, value);
    }

    public static readonly StyledProperty<double> CornerRadiusLBProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, double>(nameof(CornerRadiusLB),
            defaultValue: 0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadiusLB
    {
        get => GetValue(CornerRadiusLBProperty);
        set => SetValue(CornerRadiusLBProperty, value);
    }

    #endregion

    #region 调试点显示

    /// <summary>是否显示调试点（总开关）</summary>
    public static readonly StyledProperty<bool> DrawPointProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, bool>(nameof(DrawPoint),
            defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawPoint
    {
        get => GetValue(DrawPointProperty);
        set => SetValue(DrawPointProperty, value);
    }

    /// <summary>是否显示图形点（顶点）</summary>
    public static readonly StyledProperty<bool> DrawVertexProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, bool>(nameof(DrawVertex),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawVertex
    {
        get => GetValue(DrawVertexProperty);
        set => SetValue(DrawVertexProperty, value);
    }

    /// <summary>是否显示控制点（切点）</summary>
    public static readonly StyledProperty<bool> DrawTangentProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, bool>(nameof(DrawTangent),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawTangent
    {
        get => GetValue(DrawTangentProperty);
        set => SetValue(DrawTangentProperty, value);
    }

    /// <summary>是否显示圆心</summary>
    public static readonly StyledProperty<bool> DrawCenterProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, bool>(nameof(DrawCenter),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawCenter
    {
        get => GetValue(DrawCenterProperty);
        set => SetValue(DrawCenterProperty, value);
    }

    /// <summary>是否用虚线连接图形点</summary>
    public static readonly StyledProperty<bool> DrawVertexLineProperty =
        AvaloniaProperty.Register<RoundedRectangleShapeEditor, bool>(nameof(DrawVertexLine),
            defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawVertexLine
    {
        get => GetValue(DrawVertexLineProperty);
        set => SetValue(DrawVertexLineProperty, value);
    }

    #endregion
}
