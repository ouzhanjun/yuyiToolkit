using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Elements;

namespace yuyiAvalonia.UI.Controls;

public partial class RoundEllipsePropertyEditor : UserControl
{
    /// <summary>属性变更事件，供父容器订阅。</summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    private RoundEllipseElement? _currentElement;
    private bool _isSyncingFromElement;

    public RoundEllipsePropertyEditor()
    {
        InitializeComponent();

        PropertyChanged += RoundEllipsePropertyEditor_PropertyChanged;

        Loaded += (_, _) =>
        {
            PART_InnerRadiusBorder?.Attach(PART_Slider_InnerRadius);
            PART_CornerRadiusBorder?.Attach(PART_Slider_CornerRadius);
            SyncFromElement();
        };
    }

    #region Element 双向同步

    private void RoundEllipsePropertyEditor_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(ElementProperty):
                _currentElement = e.NewValue as RoundEllipseElement;
                SyncFromElement();
                break;
            case nameof(StartAngle):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.StartAngle = StartAngle;
                UpdateInfo();
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartAngle)));
                break;
            case nameof(SweepAngle):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.SweepAngle = SweepAngle;
                UpdateInfo();
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SweepAngle)));
                break;
            case nameof(InnerRadius):
                PART_InnerRadius.Value = InnerRadius;
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.InnerRadius = InnerRadius;
                UpdateInfo();
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerRadius)));
                break;
            case nameof(CornerRadius):
                PART_CornerRadius.Value = CornerRadius;
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.CornerRadius = CornerRadius;
                UpdateInfo();
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

        StartAngle = _currentElement.StartAngle;
        SweepAngle = _currentElement.SweepAngle;
        InnerRadius = _currentElement.InnerRadius;
        CornerRadius = _currentElement.CornerRadius;

        // 同步调试点属性
        DrawPoint = _currentElement.DrawPoint;
        DrawVertex = _currentElement.DrawVertex;
        DrawTangent = _currentElement.DrawTangent;
        DrawCenter = _currentElement.DrawCenter;
        DrawVertexLine = _currentElement.DrawVertexLine;

        UpdateInfo();

        _isSyncingFromElement = false;
    }

    private void UpdateInfo()
    {
        if (_currentElement == null || SectorInfoText == null) return;

        double s = _currentElement.SweepAngle, i = _currentElement.InnerRadius;
        string shape = i > 0.0001
            ? (Math.Abs(s) >= 360 ? "椭圆环 (donut)" : "扇环 (ring sector)")
            : (Math.Abs(s) >= 360 ? "椭圆" : "扇形 / 弧");
        SectorInfoText.Text =
            $"形态：{shape}\n" +
            $"起始角 = {_currentElement.StartAngle:0.0}°\n" +
            $"扫描角 = {_currentElement.SweepAngle:0.0}°\n" +
            $"内圈占比 = {_currentElement.InnerRadius:0.00}\n" +
            $"圆角半径 = {_currentElement.CornerRadius:0.1} px";
    }

    private void SetSectorPreset(double start, double sweep, double inner)
    {
        if (_currentElement == null) return;
        _currentElement.StartAngle = start;
        _currentElement.SweepAngle = sweep;
        _currentElement.InnerRadius = inner;
        SyncFromElement();
        ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartAngle)));
    }

    private void BtnSectorFull_Click(object? s, RoutedEventArgs e)       => SetSectorPreset(0, 360, 0);
    private void BtnSectorHalf_Click(object? s, RoutedEventArgs e)       => SetSectorPreset(0, 180, 0);
    private void BtnSectorPie_Click(object? s, RoutedEventArgs e)        => SetSectorPreset(-90, 90, 0);
    private void BtnSectorArc_Click(object? s, RoutedEventArgs e)        => SetSectorPreset(-30, 120, 0);
    private void BtnSectorRing_Click(object? s, RoutedEventArgs e)       => SetSectorPreset(0, 360, 0.5);
    private void BtnSectorRingSector_Click(object? s, RoutedEventArgs e) => SetSectorPreset(-90, 120, 0.5);
    private void BtnSectorReset_Click(object? s, RoutedEventArgs e)      => SetSectorPreset(0, 360, 0);

    #endregion

    #region Styled Properties

    public static readonly StyledProperty<RoundEllipseElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, RoundEllipseElement?>(
            nameof(ElementProperty), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);

    public RoundEllipseElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    /// <summary>起始角（°）。</summary>
    public static readonly StyledProperty<double> StartAngleProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, double>(nameof(StartAngle),
            defaultValue: 0.0, defaultBindingMode: BindingMode.TwoWay);

    public double StartAngle
    {
        get => GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>扫描角（°）。</summary>
    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, double>(nameof(SweepAngle),
            defaultValue: 360.0, defaultBindingMode: BindingMode.TwoWay);

    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    /// <summary>内圈占比（0 = 实心 / 弧，1 = 退化）。</summary>
    public static readonly StyledProperty<double> InnerRadiusProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, double>(nameof(InnerRadius),
            defaultValue: 0.0, defaultBindingMode: BindingMode.TwoWay);

    public double InnerRadius
    {
        get => GetValue(InnerRadiusProperty);
        set => SetValue(InnerRadiusProperty, value);
    }

    /// <summary>圆角半径（像素）。</summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, double>(nameof(CornerRadius),
            defaultValue: 0.0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    #region 调试点显示

    /// <summary>是否显示调试点（总开关）</summary>
    public static readonly StyledProperty<bool> DrawPointProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, bool>(nameof(DrawPoint),
            defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawPoint
    {
        get => GetValue(DrawPointProperty);
        set => SetValue(DrawPointProperty, value);
    }

    /// <summary>是否显示图形点（顶点）</summary>
    public static readonly StyledProperty<bool> DrawVertexProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, bool>(nameof(DrawVertex),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawVertex
    {
        get => GetValue(DrawVertexProperty);
        set => SetValue(DrawVertexProperty, value);
    }

    /// <summary>是否显示控制点（切点）</summary>
    public static readonly StyledProperty<bool> DrawTangentProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, bool>(nameof(DrawTangent),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawTangent
    {
        get => GetValue(DrawTangentProperty);
        set => SetValue(DrawTangentProperty, value);
    }

    /// <summary>是否显示圆心</summary>
    public static readonly StyledProperty<bool> DrawCenterProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, bool>(nameof(DrawCenter),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawCenter
    {
        get => GetValue(DrawCenterProperty);
        set => SetValue(DrawCenterProperty, value);
    }

    /// <summary>是否用虚线连接图形点</summary>
    public static readonly StyledProperty<bool> DrawVertexLineProperty =
        AvaloniaProperty.Register<RoundEllipsePropertyEditor, bool>(nameof(DrawVertexLine),
            defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawVertexLine
    {
        get => GetValue(DrawVertexLineProperty);
        set => SetValue(DrawVertexLineProperty, value);
    }

    #endregion
}
