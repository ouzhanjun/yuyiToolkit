using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 容器样式 - 用于定义网页/小程序的容器（盒子）样式
/// </summary>
public class ContainerStyle
{
    /// <summary>
    /// 背景色
    /// </summary>
    public string BackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// 背景图片URL
    /// </summary>
    public string? BackgroundImage { get; set; }

    /// <summary>
    /// 背景图片平铺方式
    /// </summary>
    public BackgroundRepeat BackgroundRepeat { get; set; } = BackgroundRepeat.NoRepeat;

    /// <summary>
    /// 背景图片位置
    /// </summary>
    public BackgroundPosition BackgroundPosition { get; set; } = BackgroundPosition.Center;

    /// <summary>
    /// 背景图片尺寸
    /// </summary>
    public BackgroundSize BackgroundSize { get; set; } = BackgroundSize.Cover;

    /// <summary>
    /// 边框颜色
    /// </summary>
    public string BorderColor { get; set; } = "#E0E0E0";

    /// <summary>
    /// 边框宽度
    /// </summary>
    public double BorderWidth { get; set; } = 1;

    /// <summary>
    /// 边框样式
    /// </summary>
    public BorderStyle BorderStyle { get; set; } = BorderStyle.Solid;

    /// <summary>
    /// 圆角
    /// </summary>
    public double BorderRadius { get; set; } = 8;

    /// <summary>
    /// 内边距
    /// </summary>
    public Thickness Padding { get; set; } = new Thickness(16);

    /// <summary>
    /// 外边距
    /// </summary>
    public Thickness Margin { get; set; } = new Thickness(0);

    /// <summary>
    /// 阴影
    /// </summary>
    public BoxShadow? BoxShadow { get; set; }

    /// <summary>
    /// 宽度
    /// </summary>
    public string Width { get; set; } = "auto";

    /// <summary>
    /// 高度
    /// </summary>
    public string Height { get; set; } = "auto";

    /// <summary>
    /// 最小宽度
    /// </summary>
    public string MinWidth { get; set; } = "0";

    /// <summary>
    /// 最大宽度
    /// </summary>
    public string MaxWidth { get; set; } = "none";

    /// <summary>
    /// 显示方式
    /// </summary>
    public DisplayMode Display { get; set; } = DisplayMode.Block;

    /// <summary>
    /// 定位方式
    /// </summary>
    public PositionMode Position { get; set; } = PositionMode.Relative;

    /// <summary>
    /// 透明度
    /// </summary>
    public double Opacity { get; set; } = 1.0;

    public ContainerStyle Clone()
    {
        return new ContainerStyle
        {
            BackgroundColor = BackgroundColor,
            BackgroundImage = BackgroundImage,
            BackgroundRepeat = BackgroundRepeat,
            BackgroundPosition = BackgroundPosition,
            BackgroundSize = BackgroundSize,
            BorderColor = BorderColor,
            BorderWidth = BorderWidth,
            BorderStyle = BorderStyle,
            BorderRadius = BorderRadius,
            Padding = Padding,
            Margin = Margin,
            BoxShadow = BoxShadow?.Clone(),
            Width = Width,
            Height = Height,
            MinWidth = MinWidth,
            MaxWidth = MaxWidth,
            Display = Display,
            Position = Position,
            Opacity = Opacity
        };
    }
}

/// <summary>
/// 背景图片平铺方式
/// </summary>
public enum BackgroundRepeat
{
    NoRepeat,
    Repeat,
    RepeatX,
    RepeatY,
    Round,
    Space
}

/// <summary>
/// 背景图片位置
/// </summary>
public enum BackgroundPosition
{
    Top,
    Bottom,
    Left,
    Right,
    Center,
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom,
    Custom
}

/// <summary>
/// 背景图片尺寸
/// </summary>
public enum BackgroundSize
{
    Auto,
    Cover,
    Contain,
    Custom
}

/// <summary>
/// 边框样式
/// </summary>
public enum BorderStyle
{
    None,
    Solid,
    Dashed,
    Dotted,
    Double,
    Groove,
    Ridge,
    Inset,
    Outset
}

/// <summary>
/// 显示方式
/// </summary>
public enum DisplayMode
{
    Block,
    Inline,
    InlineBlock,
    Flex,
    Grid,
    None
}

/// <summary>
/// 定位方式
/// </summary>
public enum PositionMode
{
    Static,
    Relative,
    Absolute,
    Fixed,
    Sticky
}

/// <summary>
/// 阴影样式
/// </summary>
public class BoxShadow
{
    public double OffsetX { get; set; } = 0;
    public double OffsetY { get; set; } = 2;
    public double BlurRadius { get; set; } = 4;
    public double SpreadRadius { get; set; } = 0;
    public string Color { get; set; } = "#00000020";
    public bool Inset { get; set; } = false;

    public BoxShadow Clone()
    {
        return new BoxShadow
        {
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            BlurRadius = BlurRadius,
            SpreadRadius = SpreadRadius,
            Color = Color,
            Inset = Inset
        };
    }

    public override string ToString()
    {
        var insetStr = Inset ? "inset " : "";
        return $"{insetStr}{OffsetX}px {OffsetY}px {BlurRadius}px {SpreadRadius}px {Color}";
    }
}
