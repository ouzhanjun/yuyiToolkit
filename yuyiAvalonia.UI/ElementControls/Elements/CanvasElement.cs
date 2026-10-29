using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace yuyiAvalonia.UI.Models;

/// <summary>
/// 画布元素基类
/// </summary>
public partial class CanvasElement : ObservableObject
{
    public CanvasElement()
    {
    }

    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// 设置Id（初始化时调用）
    /// </summary>
    public void SetId(string value)
    {
        Id = value;
    }

    [ObservableProperty]
    private ComponentType _elementType;

    #region 位置尺寸

    [ObservableProperty]
    private double _x;

    [ObservableProperty]
    private double _y;

    [ObservableProperty]
    private double _width = 100;

    [ObservableProperty]
    private double _height = 50;

    [ObservableProperty]
    private double _rotation;

    [ObservableProperty]
    private int _zIndex;

    #endregion

    #region 状态

    [ObservableProperty]
    private bool _isContainer = false;

    [ObservableProperty]
    private bool _isVisible = true;

    /// <summary>
    /// 是否在控件右下角显示Id标识
    /// </summary>
    [ObservableProperty]
    private bool _showIdLabel = false;

    #endregion

    #region 对齐方式

    [ObservableProperty]
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;

    [ObservableProperty]
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;

    #endregion

    #region 盒子

    [ObservableProperty]
    private double _padding = 3;

    [ObservableProperty]
    private double _margin;

    // 四边分别设置
    [ObservableProperty]
    private double _paddingLeft = 3;

    [ObservableProperty]
    private double _paddingTop = 3;

    [ObservableProperty]
    private double _paddingRight = 3;

    [ObservableProperty]
    private double _paddingBottom = 3;

    [ObservableProperty]
    private double _marginLeft;

    [ObservableProperty]
    private double _marginTop;

    [ObservableProperty]
    private double _marginRight;

    [ObservableProperty]
    private double _marginBottom;


    // 是否使用统一模式
    [ObservableProperty]
    private bool _paddingIsUniform = true;

    [ObservableProperty]
    private bool _marginIsUniform = true;

    #endregion

    #region 阴影
    /// <summary>
    /// 内外阴影列表（可叠加多层）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<ShadowItem> _shadows = new();

    #endregion

    #region 背景填充

    /// <summary>
    /// 多层填充集合（按顺序层叠，索引越小越在底层）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FillItem> _backgroundFills = new();

    /// <summary>
    /// 背景填充（统一管理所有背景相关属性：填充类型/纯色/渐变/图片）。
    /// 原 CanvasElement 上的 BackgroundColor / FillType / GradientStartColor / GradientEndColor /
    /// GradientAngle / BackgroundImage / BackgroundImageStretch / BackgroundImageTileMode /
    /// BackgroundImageAlignmentX / BackgroundImageAlignmentY 全部迁移到 BackgroundFill 上的对应属性。
    /// </summary>
    [ObservableProperty]
    private FillItem _backgroundFill = new();

    partial void OnBackgroundFillChanged(FillItem? oldValue, FillItem newValue)
    {
        if (oldValue != null)
            oldValue.PropertyChanged -= OnBackFillPropertyChanged;
        if (newValue != null)
            newValue.PropertyChanged += OnBackFillPropertyChanged;
    }

    private void OnBackFillPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(BackgroundFill));
    }

    /// <summary>
    /// 元素整体透明度（区别于 BackgroundFill.Opacity 仅作用于纯色填充层）。
    /// </summary>
    [ObservableProperty]
    private double _opacity = 1;

    #endregion

    #region 边线样式

    [ObservableProperty]
    private string _borderColor = "#E5E5E5";

    [ObservableProperty]
    private double _borderWidth;

    [ObservableProperty]
    private double _cornerRadius;

    /// <summary>
    /// 线条类型（实线、虚线、自定义）
    /// </summary>
    [ObservableProperty]
    private StrokeType _strokeType = StrokeType.Solid;

    /// <summary>
    /// 线条位置
    /// </summary>
    [ObservableProperty]
    private StrokePosition _strokePosition = Models.StrokePosition.Center;

    /// <summary>
    /// 边线样式类型（实线、虚线、点线等）
    /// </summary>
    [ObservableProperty]
    private StrokeStyleType _strokeStyle = StrokeStyleType.Solid;

    /// <summary>MiterLimit：尖角最大长度限制（miterLength / lineWidth，超过则截断为 bevel），默认 10</summary>
    [ObservableProperty]
    private double _strokeMiterLimit = 4.0;

    /// <summary>
    /// 线条宽度（统一）
    /// </summary>
    [ObservableProperty]
    private double _strokeWidth = 1;

    /// <summary>
    /// 线条连接头
    /// </summary>
    [ObservableProperty]
    private StrokeEndpoints _strokeEndpoint = StrokeEndpoints.Butt;

    /// <summary>
    /// 线条连接方式
    /// </summary>
    [ObservableProperty]
    private StrokeJoin _strokeJoin = StrokeJoin.Miter;

    /// <summary>
    /// 边线虚线偏移
    /// </summary>
    [ObservableProperty]
    private double _strokeDashOffset = 0;

    /// <summary>
    /// 虚线数组字符串（用于序列化，格式如 "8,4" 或 "4,2,1,2"）
    /// </summary>
    [ObservableProperty]
    private string _strokeDashArrayString;

    /// <summary>
    /// 边线透明度
    /// </summary>
    [ObservableProperty]
    private double _strokeOpacity = 1;

    /// <summary>
    /// 多层填充集合（按顺序层叠，索引越小越在底层）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FillItem> _strokeFills = new();

    [ObservableProperty]
    private FillItem _strokeFill = new();

    /******************************
     线条样式 结束
     *****************************/

    /// <summary>
    /// 边线样式索引（用于 UI 选择）
    /// </summary>
    public int StrokeStyleIndex
    {
        get => StrokeStyle switch
        {
            StrokeStyleType.Solid => 0,
            StrokeStyleType.Dashed => 1,
            StrokeStyleType.Dot => 2,
            StrokeStyleType.DashDot => 3,
            StrokeStyleType.DashDotDot => 4,
            _ => 0
        };
        set
        {
            StrokeStyle = value switch
            {
                0 => StrokeStyleType.Solid,
                1 => StrokeStyleType.Dashed,
                2 => StrokeStyleType.Dot,
                3 => StrokeStyleType.DashDot,
                4 => StrokeStyleType.DashDotDot,
                _ => StrokeStyleType.Solid
            };
            OnPropertyChanged(nameof(StrokeStyle));
        }
    }

    /// <summary>
    /// 边线宽度模式（统一或四边分别）
    /// </summary>
    [ObservableProperty]
    private StrokeWidthMode _strokeWidthMode = StrokeWidthMode.Uniform;

    /// <summary>
    /// 左边线宽度
    /// </summary>
    [ObservableProperty]
    private double _strokeWidthLeft = 1;

    /// <summary>
    /// 上边线宽度
    /// </summary>
    [ObservableProperty]
    private double _strokeWidthTop = 1;

    /// <summary>
    /// 右边线宽度
    /// </summary>
    [ObservableProperty]
    private double _strokeWidthRight = 1;

    /// <summary>
    /// 下边线宽度
    /// </summary>
    [ObservableProperty]
    private double _strokeWidthBottom = 1;

    /// <summary>
    /// 边线填充类型（纯色、渐变、图片）
    /// </summary>
    [ObservableProperty]
    private StrokeFillType _strokeFillType = StrokeFillType.Solid;

    /// <summary>
    /// 边线颜色（用于纯色填充）
    /// </summary>
    [ObservableProperty]
    private string _strokeColor = "#333333";

    #endregion

    #region 元素关系

    [ObservableProperty]
    private CanvasElement? _parent;

    [ObservableProperty]
    private ObservableCollection<CanvasElement> _children = new();

    public event EventHandler<CanvasElement?>? ParentChanged;

    partial void OnParentChanged(CanvasElement? value)
    {
        ParentChanged?.Invoke(this, value);
    }

    #endregion

    #region Grid属性

    /// <summary>
    /// Grid容器中的行位置（-1表示未设置）
    /// </summary>
    [ObservableProperty]
    private int _gridRow = -1;

    /// <summary>
    /// Grid容器中的列位置（-1表示未设置）
    /// </summary>
    [ObservableProperty]
    private int _gridColumn = -1;

    #endregion

    #region Canvas 定位

    [ObservableProperty]
    private double _top = -1;

    [ObservableProperty]
    private double _left = -1;

    [ObservableProperty]
    private double _bottom = -1;

    [ObservableProperty]
    private double _right = -1;

    /// <summary>
    /// 使用Left定位而不是Right定位（默认true）
    /// </summary>
    [ObservableProperty]
    private bool _fixLeft = true;

    /// <summary>
    /// 固定上边定位（默认true）
    /// </summary>
    [ObservableProperty]
    private bool _fixTop = true;

    /// <summary>
    /// 固定下边定位（默认false）
    /// </summary>
    [ObservableProperty]
    private bool _fixBottom = false;

    /// <summary>
    /// 固定右边定位（默认false）
    /// </summary>
    [ObservableProperty]
    private bool _fixRight = false;

    #endregion

    #region 容器

    public bool CanAddToContainer()
    {
        return IsContainer;
    }
    #endregion

    public virtual CanvasElement Clone()
    {
        return new CanvasElement
        {
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Opacity = Opacity,
            Rotation = Rotation,
            ZIndex = ZIndex,
            Padding = Padding,
            Margin = Margin,
            PaddingLeft = PaddingLeft,
            PaddingTop = PaddingTop,
            PaddingRight = PaddingRight,
            PaddingBottom = PaddingBottom,
            MarginLeft = MarginLeft,
            MarginTop = MarginTop,
            MarginRight = MarginRight,
            MarginBottom = MarginBottom,
            PaddingIsUniform = PaddingIsUniform,
            MarginIsUniform = MarginIsUniform,
            HorizontalAlignment = HorizontalAlignment,
            VerticalAlignment = VerticalAlignment,
            IsContainer = IsContainer,
            IsVisible = IsVisible,
            Parent = Parent,
            Children = Children,
            BackgroundFill = BackgroundFill.Clone(),
            BackgroundFills = new ObservableCollection<FillItem>(BackgroundFills.Select(f => f.Clone())),
            BorderColor = BorderColor,
            BorderWidth = BorderWidth,
            CornerRadius = CornerRadius,
            Shadows = new ObservableCollection<ShadowItem>(Shadows.Select(s => s.Clone())),
            Top = Top,
            Left = Left,
            Bottom = Bottom,
            Right = Right,
            FixLeft = FixLeft,
            FixTop = FixTop,
            FixBottom = FixBottom,
            FixRight = FixRight,
            GridRow = GridRow,
            GridColumn = GridColumn,
            ShowIdLabel = ShowIdLabel,
            StrokeType = StrokeType,
        };
    }
}
