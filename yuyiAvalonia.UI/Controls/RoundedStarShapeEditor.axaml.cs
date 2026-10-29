using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using yuyiAvalonia.UI.ElementControls.Elements;

namespace yuyiAvalonia.UI.Controls;

public partial class RoundedStarShapeEditor : UserControl
{
    /// <summary>属性变更事件，供父容器订阅。</summary>
    public event PropertyChangedEventHandler? ElementPropertyChanged;

    private const string MixedText = "Mixed";
    private RoundedStarElement? _currentElement;
    private bool _isSyncingFromElement;
    private bool _cornerPartChanged;
    private bool _cornerUnifiedChanged;
    private ObservableCollection<RadiiItem> RadiiItems { get; } = new();

    public RoundedStarShapeEditor()
    {
        InitializeComponent();

        PropertyChanged += RoundedStarShapeEditor_PropertyChanged;

        Loaded += (_, _) =>
        {
            PART_PropertyBorder?.Attach(PART_Slider_UnifiedCornerRadius);
            PART_UnifiedCornerRadius.ValueChanged += PART_UnifiedCornerRadius_ValueChanged;
            SyncFromElement();
        };
    }

    private void PART_UnifiedCornerRadius_ValueChanged(object? sender, double e)
    {
        if (double.TryParse(PART_UnifiedCornerRadius.Text, out double radius)
            && !_isSyncingFromElement
            && !_cornerPartChanged)
        {
            _cornerUnifiedChanged = true;
            CornerRadius = radius;
            if (_currentElement != null)
            {
                _currentElement.CornerRadius = radius;
                var radii = _currentElement.Radii;

                for (int i = 0; i < radii.Count; i++)
                {
                    RadiiItems[i].Radius = radius;
                }
            }
            _cornerUnifiedChanged = false;
        }
    }

    #region Element 双向同步

    private void RoundedStarShapeEditor_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        switch (e.Property.Name)
        {
            case nameof(ElementProperty):
                _currentElement = e.NewValue as RoundedStarElement;
                SyncFromElement();
                break;
            case nameof(OuterRatioPercent):
                PART_OuterRatio.Value = OuterRatioPercent;
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.OuterRatio = OuterRatioPercent / 100.0;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OuterRatioPercent)));
                break;
            case nameof(InnerRatioPercent):
                PART_InnerRatio.Value = InnerRatioPercent;
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.InnerRatio = InnerRatioPercent / 100.0;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerRatioPercent)));
                break;
            case nameof(RotDeg):
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.RotDeg = RotDeg;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RotDeg)));
                break;
            case nameof(CornerRadius):
                PART_UnifiedCornerRadius.Value = CornerRadius;
                if (_currentElement != null && !_isSyncingFromElement)
                    _currentElement.CornerRadius = CornerRadius;
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CornerRadius)));
                break;
            case nameof(PointCount):
                if (_currentElement != null && !_isSyncingFromElement)
                {
                    _currentElement.UpdateRadii((int)PointCount);
                    RefreshRadiiItems();
                }
                ElementPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointCount)));
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

        OuterRatioPercent = _currentElement.OuterRatio * 100.0;
        InnerRatioPercent = _currentElement.InnerRatio * 100.0;
        RotDeg = _currentElement.RotDeg;
        CornerRadius = _currentElement.CornerRadius;
        PointCount = _currentElement.PointCount;

        // 同步调试点属性
        DrawPoint = _currentElement.DrawPoint;
        DrawVertex = _currentElement.DrawVertex;
        DrawTangent = _currentElement.DrawTangent;
        DrawCenter = _currentElement.DrawCenter;
        DrawVertexLine = _currentElement.DrawVertexLine;

        RefreshRadiiItems();

        if (RadiiItems.Count > 0)
        {
            double radius = RadiiItems[0].Radius;

            if (RadiiItems.Any(x => x.Radius != radius))
            {
                PART_UnifiedCornerRadius.Value = 0;
                PART_UnifiedCornerRadius.Text = MixedText;
                CornerRadius = 0;
                PART_Slider_UnifiedCornerRadius.Value = CornerRadius;
            }
            else
            {
                CornerRadius = radius;
                PART_UnifiedCornerRadius.Text = CornerRadius.ToString();
            }
        }

        _isSyncingFromElement = false;
    }

    private void RefreshRadiiItems()
    {
        if (_currentElement == null) return;

        //for (int i = 0; i < RadiiItems.Count; i++)
        //{
        //    RadiiItems[i].PropertyChanged -= RoundedStarShapeEditor_PropertyChanged;
        //}

        RadiiItems.Clear();
        var radii = _currentElement.Radii;
        for (int i = 0; i < radii.Count; i++)
        {
            int index = i;
            double radius = radii[i];
            var item = new RadiiItem(index, radius, val =>
            {
                if (_currentElement != null && index >= 0 && index < _currentElement.Radii.Count)
                {
                    _currentElement.SetRadiusAt(index, val);
                    RoundedStarShapeEditor_PropertyChanged();
                }
            });
            //item.PropertyChanged += RoundedStarShapeEditor_PropertyChanged;
            RadiiItems.Add(item);
        }

        if (RadiiItems.Count > 0)
        {
            ElementPropertyChanged?.Invoke(_currentElement, new PropertyChangedEventArgs(nameof(_currentElement.Radii)));
        }

        RadiiItemsControl.ItemsSource = RadiiItems;
    }

    private void RoundedStarShapeEditor_PropertyChanged()
    {
        if (_cornerUnifiedChanged)
            return;

        _cornerPartChanged = true;
        ElementPropertyChanged?.Invoke(_currentElement, new PropertyChangedEventArgs(nameof(_currentElement.Radii)));

        if (RadiiItems.Count > 0)
        {
            double radius =(int) RadiiItems[0].Radius;

            if (RadiiItems.Any(x => x.Radius != radius))
            {
                PART_UnifiedCornerRadius.Value = 0;
                PART_UnifiedCornerRadius.Text = MixedText;
                CornerRadius = 0;
                PART_Slider_UnifiedCornerRadius.Value = CornerRadius;
            }
            else
            {
                CornerRadius = radius;
                PART_UnifiedCornerRadius.Text = CornerRadius.ToString();
            }
        }

        _cornerPartChanged = false;
    }

    #endregion

    #region Styled Properties

    public static readonly StyledProperty<RoundedStarElement?> ElementPropertyProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, RoundedStarElement?>(
            nameof(ElementProperty), defaultValue: null, defaultBindingMode: BindingMode.TwoWay);

    public RoundedStarElement? ElementProperty
    {
        get => GetValue(ElementPropertyProperty);
        set => SetValue(ElementPropertyProperty, value);
    }

    /// <summary>UI 显示用中间属性，控制切换按钮和分开模式 Grid 的可见性。</summary>
    public static readonly StyledProperty<bool> IsShowCornerRadiusIsUniformProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, bool>(nameof(IsShowCornerRadiusIsUniform), true,
            defaultBindingMode: BindingMode.TwoWay);

    public bool IsShowCornerRadiusIsUniform
    {
        get => GetValue(IsShowCornerRadiusIsUniformProperty);
        set => SetValue(IsShowCornerRadiusIsUniformProperty, value);
    }

    /// <summary>外尖半径比例（百分比 0-100）。</summary>
    public static readonly StyledProperty<double> OuterRatioPercentProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, double>(nameof(OuterRatioPercent),
            defaultValue: 50.0, defaultBindingMode: BindingMode.TwoWay);

    public double OuterRatioPercent
    {
        get => GetValue(OuterRatioPercentProperty);
        set => SetValue(OuterRatioPercentProperty, value);
    }

    /// <summary>内凹半径比例（百分比 0-100）。</summary>
    public static readonly StyledProperty<double> InnerRatioPercentProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, double>(nameof(InnerRatioPercent),
            defaultValue: 40.0, defaultBindingMode: BindingMode.TwoWay);

    public double InnerRatioPercent
    {
        get => GetValue(InnerRatioPercentProperty);
        set => SetValue(InnerRatioPercentProperty, value);
    }

    /// <summary>起始角度（度）。</summary>
    public static readonly StyledProperty<double> RotDegProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, double>(nameof(RotDeg),
            defaultValue: -90.0, defaultBindingMode: BindingMode.TwoWay);

    public double RotDeg
    {
        get => GetValue(RotDegProperty);
        set => SetValue(RotDegProperty, value);
    }

    /// <summary>统一圆角（Slider 绑定目标）。</summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, double>(nameof(CornerRadius),
            defaultValue: 0.0, defaultBindingMode: BindingMode.TwoWay);

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>外尖数量。</summary>
    public static readonly StyledProperty<double> PointCountProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, double>(nameof(PointCount),
            defaultValue: 5.0, defaultBindingMode: BindingMode.TwoWay);

    public double PointCount
    {
        get => GetValue(PointCountProperty);
        set => SetValue(PointCountProperty, value);
    }

    #endregion

    #region 调试点显示

    /// <summary>是否显示调试点（总开关）</summary>
    public static readonly StyledProperty<bool> DrawPointProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, bool>(nameof(DrawPoint),
            defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawPoint
    {
        get => GetValue(DrawPointProperty);
        set => SetValue(DrawPointProperty, value);
    }

    /// <summary>是否显示图形点（顶点）</summary>
    public static readonly StyledProperty<bool> DrawVertexProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, bool>(nameof(DrawVertex),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawVertex
    {
        get => GetValue(DrawVertexProperty);
        set => SetValue(DrawVertexProperty, value);
    }

    /// <summary>是否显示控制点（切点）</summary>
    public static readonly StyledProperty<bool> DrawTangentProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, bool>(nameof(DrawTangent),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawTangent
    {
        get => GetValue(DrawTangentProperty);
        set => SetValue(DrawTangentProperty, value);
    }

    /// <summary>是否显示圆心</summary>
    public static readonly StyledProperty<bool> DrawCenterProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, bool>(nameof(DrawCenter),
            defaultValue: true, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawCenter
    {
        get => GetValue(DrawCenterProperty);
        set => SetValue(DrawCenterProperty, value);
    }

    /// <summary>是否用虚线连接图形点</summary>
    public static readonly StyledProperty<bool> DrawVertexLineProperty =
        AvaloniaProperty.Register<RoundedStarShapeEditor, bool>(nameof(DrawVertexLine),
            defaultValue: false, defaultBindingMode: BindingMode.TwoWay);

    public bool DrawVertexLine
    {
        get => GetValue(DrawVertexLineProperty);
        set => SetValue(DrawVertexLineProperty, value);
    }

    #endregion

    #region 逐顶点圆角

    internal class RadiiItem : INotifyPropertyChanged
    {
        private double _radius;
        private readonly int _index;
        private readonly Action<double> _onChanged;

        public int Index => _index;

        public double Radius
        {
            get => _radius;
            set
            {
                if (Math.Abs(_radius - value) > 0.001)
                {
                    _radius = value;
                    OnPropertyChanged();
                    _onChanged?.Invoke(value);
                }
            }
        }

        public RadiiItem(int index, double radius, Action<double> onChanged)
        {
            _index = index;
            _radius = radius;
            _onChanged = onChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    #endregion
}
