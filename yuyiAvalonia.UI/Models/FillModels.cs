using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace yuyiAvalonia.UI.Models;

/// <summary>
/// 填充类型
/// </summary>
public enum FillType
{
    None,
    Solid,
    Gradient,
    Image
}

/// <summary>
/// 渐变类型
/// </summary>
public enum GradientKind
{
    Linear = 0,
    Radial = 1,
    Angle = 2,
    Diamond = 3
}

/// <summary>
/// 渐变停止点
/// </summary>
[ObservableObject]
public partial class GradientStopInfo
{
    /// <summary>
    /// 位置 (0-1)
    /// </summary>
    [ObservableProperty]
    private double _offset = 0;

    /// <summary>
    /// 颜色
    /// </summary>
    [ObservableProperty]
    private string _color = "#FFFFFF";

    /// <summary>
    /// 透明度 (0-1)
    /// </summary>
    [ObservableProperty]
    private double _opacity = 1.0;

    public GradientStopInfo() { }

    public GradientStopInfo(double offset, string color, double opacity = 1.0)
    {
        Offset = offset;
        Color = color;
        Opacity = opacity;
    }

    public GradientStopInfo Clone()
    {
        return new GradientStopInfo { Offset = Offset, Color = Color, Opacity = Opacity };
    }
}

/// <summary>
/// 单个填充项的数据模型
/// </summary>
[ObservableObject]
public partial class FillItem
{
    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private FillType _fillType = FillType.Solid;

    [ObservableProperty]
    private string _name = string.Empty;

    #region 纯色属性

    [ObservableProperty]
    private string _backgroundColor = "#FFFFFF";

    [ObservableProperty]
    private double _opacity = 1.0;

    #endregion

    #region 渐变属性

    [ObservableProperty]
    private GradientKind _gradientKind = GradientKind.Linear;

    /// <summary>
    /// 渐变停止点集合（支持多个停止点）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<GradientStopInfo> _gradientStops = new()
    {
        new GradientStopInfo(0.0, "#FFFFFF"),
        new GradientStopInfo(1.0, "#000000")
    };

    [ObservableProperty]
    private double _gradientAngle = 0;

    [ObservableProperty]
    private double _radialCenterX = 0.5;

    [ObservableProperty]
    private double _radialCenterY = 0.5;

    [ObservableProperty]
    private double _radialRadius = 0.5;

    #endregion

    #region 图片属性

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private int _imageStretchIndex = 3;

    [ObservableProperty]
    private int _imageTileModeIndex = 0;

    [ObservableProperty]
    private AlignmentX _imageAlignmentX = AlignmentX.Center;

    [ObservableProperty]
    private AlignmentY _imageAlignmentY = AlignmentY.Center;

    [ObservableProperty]
    private ImageRenderEffect _imageEffect = new();

    /// <summary>
    /// 图片拉伸模式（映射到 ImageStretchIndex，枚举值 0-3）
    /// </summary>
    public Stretch ImageStretch
    {
        get => _imageStretchIndex switch
        {
            0 => Stretch.None,
            1 => Stretch.Fill,
            2 => Stretch.Uniform,
            3 => Stretch.UniformToFill,
            _ => Stretch.UniformToFill
        };
        set => _imageStretchIndex = value switch
        {
            Stretch.None => 0,
            Stretch.Fill => 1,
            Stretch.Uniform => 2,
            Stretch.UniformToFill => 3,
            _ => 3
        };
    }

    /// <summary>
    /// 图片平铺模式（映射到 ImageTileModeIndex，枚举值 0-3）
    /// </summary>
    public TileMode ImageTileMode
    {
        get => _imageTileModeIndex switch
        {
            0 => TileMode.None,
            1 => TileMode.FlipX,
            2 => TileMode.FlipY,
            3 => TileMode.FlipXY,
            _ => TileMode.None
        };
        set => _imageTileModeIndex = value switch
        {
            TileMode.None => 0,
            TileMode.FlipX => 1,
            TileMode.FlipY => 2,
            TileMode.FlipXY => 3,
            _ => 0
        };
    }

    #endregion

    #region 兼容旧属性（保留用于迁移阶段）

    /// <summary>
    /// 渐变起始颜色（兼容属性，自动同步到第一个停止点）
    /// </summary>
    [ObservableProperty]
    private string _gradientStartColor = "#FFFFFF";

    /// <summary>
    /// 渐变结束颜色（兼容属性，自动同步到最后一个停止点）
    /// </summary>
    [ObservableProperty]
    private string _gradientEndColor = "#000000";

    #endregion

    public FillItem()
    {
        // 初始化时同步兼容属性到停止点
        SyncCompatibilityProperties();
    }

    private void SyncCompatibilityProperties()
    {
        if (GradientStops.Count >= 2)
        {
            GradientStops[0].Color = GradientStartColor;
            GradientStops[^1].Color = GradientEndColor;
        }
        else
        {
            GradientStops.Clear();
            GradientStops.Add(new GradientStopInfo(0.0, GradientStartColor));
            GradientStops.Add(new GradientStopInfo(1.0, GradientEndColor));
        }
    }

    /// <summary>
    /// 根据 FillType 创建对应的 IBrush
    /// </summary>
    public IBrush? CreateBrush(double width, double height)
    {
        if (!IsEnabled)
            return null;

        return FillType switch
        {
            FillType.None => Brushes.Transparent,
            FillType.Solid => CreateSolidBrush(),
            FillType.Gradient => CreateGradientBrush(width, height),
            FillType.Image => CreateImageBrush(width, height),
            _ => null
        };
    }

    private IBrush CreateSolidBrush()
    {
        if (!Color.TryParse(BackgroundColor, out var color))
            color = Colors.White;

        return new SolidColorBrush(Color.FromArgb((byte)(Opacity * 255), color.R, color.G, color.B));
    }

    private IBrush CreateGradientBrush(double width, double height)
    {
        var stops = BuildGradientStops();
        if (stops.Count < 2)
            stops = new GradientStops { new GradientStop(Colors.White, 0), new GradientStop(Colors.Black, 1) };

        return GradientKind switch
        {
            GradientKind.Radial => CreateRadialGradientBrush(stops),
            // Angle is handled separately via DrawingContext (扇形锥形渐变)
            _ => CreateLinearGradientBrush(stops)
        };
    }

    private GradientStops BuildGradientStops()
    {
        var result = new GradientStops();
        foreach (var stop in GradientStops.OrderBy(s => s.Offset))
        {
            if (!Color.TryParse(stop.Color, out var color))
                color = Colors.White;
            var a = (byte)(stop.Opacity * Opacity * 255);
            result.Add(new GradientStop(Color.FromArgb(a, color.R, color.G, color.B), stop.Offset));
        }
        return result;
    }

    private IBrush CreateLinearGradientBrush(GradientStops stops)
    {
        var radians = GradientAngle * Math.PI / 180.0;
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);

        var startX = 0.5 - dx * 0.5;
        var startY = 0.5 - dy * 0.5;
        var endX = 0.5 + dx * 0.5;
        var endY = 0.5 + dy * 0.5;

        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(startX, startY, RelativeUnit.Relative),
            EndPoint = new RelativePoint(endX, endY, RelativeUnit.Relative),
            GradientStops = stops
        };
    }

    private IBrush CreateRadialGradientBrush(GradientStops stops)
    {
        return new RadialGradientBrush
        {
            Center = new RelativePoint(RadialCenterX, RadialCenterY, RelativeUnit.Relative),
            GradientOrigin = new RelativePoint(RadialCenterX, RadialCenterY, RelativeUnit.Relative),
            Radius = RadialRadius,
            GradientStops = stops
        };
    }

    private IBrush? CreateImageBrush(double width, double height)
    {
        if (string.IsNullOrEmpty(ImagePath) || !System.IO.File.Exists(ImagePath))
            return null;

        try
        {
            var bitmap = new Avalonia.Media.Imaging.Bitmap(ImagePath);

            var stretch = ImageStretchIndex switch
            {
                0 => Stretch.None,
                1 => Stretch.Fill,
                2 => Stretch.Uniform,
                3 => Stretch.UniformToFill,
                _ => Stretch.UniformToFill
            };

            var tileMode = ImageTileModeIndex switch
            {
                0 => TileMode.None,
                1 => TileMode.FlipX,
                2 => TileMode.FlipY,
                3 => TileMode.FlipXY,
                4 => TileMode.Tile,
                _ => TileMode.None
            };

            var brush = new ImageBrush(bitmap)
            {
                Stretch= stretch,
                AlignmentX = ImageAlignmentX,
                AlignmentY = ImageAlignmentY,
                TileMode = tileMode,
                Opacity = Opacity
            };

            if (tileMode != TileMode.None)
            {
                brush.DestinationRect = new RelativeRect(
                    0, 0,
                    (double)bitmap.Size.Width,
                    (double)bitmap.Size.Height,
                    RelativeUnit.Absolute);
            }

            return brush;
        }
        catch
        {
            return null;
        }
    }

    public FillItem Clone()
    {
        return new FillItem
        {
            IsEnabled = IsEnabled,
            FillType = FillType,
            Name = Name,
            BackgroundColor = BackgroundColor,
            Opacity = Opacity,
            GradientKind = GradientKind,
            GradientAngle = GradientAngle,
            RadialCenterX = RadialCenterX,
            RadialCenterY = RadialCenterY,
            RadialRadius = RadialRadius,
            ImagePath = ImagePath,
            ImageStretchIndex = ImageStretchIndex,
            ImageTileModeIndex = ImageTileModeIndex,
            ImageAlignmentX = ImageAlignmentX,
            ImageAlignmentY = ImageAlignmentY,
            ImageEffect = ImageEffect?.Clone(),
            GradientStartColor = GradientStartColor,
            GradientEndColor = GradientEndColor,
            GradientStops = new ObservableCollection<GradientStopInfo>(GradientStops.Select(s => s.Clone()))
        };
    }
}

/// <summary>
/// 图片渲染效果
/// </summary>
public class ImageRenderEffect
{
    public bool IsEnabled { get; set; } = false;
    public double BlurRadius { get; set; } = 0;
    public double Grayscale { get; set; } = 0;
    public double Brightness { get; set; } = 1.0;
    public double Contrast { get; set; } = 1.0;
    public double Opacity { get; set; } = 1.0;

    public bool NeedEffect => IsEnabled && (BlurRadius > 0 || Grayscale > 0 || Math.Abs(Brightness - 1.0) > 0.001 || Math.Abs(Contrast - 1.0) > 0.001 || Math.Abs(Opacity - 1.0) > 0.001);

    public ImageRenderEffect Clone()
    {
        return new ImageRenderEffect
        {
            IsEnabled = IsEnabled,
            BlurRadius = BlurRadius,
            Grayscale = Grayscale,
            Brightness = Brightness,
            Contrast = Contrast,
            Opacity = Opacity
        };
    }
}

/// <summary>
/// 单个阴影项的数据模型（内外阴影通用）
/// </summary>
[ObservableObject]
public partial class ShadowItem
{
    /// <summary>
    /// 是否启用
    /// </summary>
    [ObservableProperty]
    private bool _isEnabled = true;

    /// <summary>
    /// 阴影名称（UI显示用）
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// 阴影类型（外阴影/内阴影）
    /// </summary>
    [ObservableProperty]
    private ShadowType _shadowType = ShadowType.Outer;

    /// <summary>
    /// 水平偏移（负数向左，正数向右）
    /// </summary>
    [ObservableProperty]
    private double _offsetX = 0;

    /// <summary>
    /// 垂直偏移（负数向上，正数向下）
    /// </summary>
    [ObservableProperty]
    private double _offsetY = 4;

    /// <summary>
    /// 阴影扩散/收缩（正数扩散，负数收缩）
    /// </summary>
    [ObservableProperty]
    private double _spread = 0;

    /// <summary>
    /// 模糊半径
    /// </summary>
    [ObservableProperty]
    private double _blur = 8;

    /// <summary>
    /// 阴影颜色
    /// </summary>
    [ObservableProperty]
    private string _color = "#000000";

    /// <summary>
    /// 透明度（0-1）
    /// </summary>
    [ObservableProperty]
    private double _opacity = 1.0;

    public ShadowItem() { }

    public ShadowItem Clone()
    {
        return new ShadowItem
        {
            IsEnabled = IsEnabled,
            Name = Name,
            ShadowType = ShadowType,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            Spread = Spread,
            Blur = Blur,
            Color = Color,
            Opacity = Opacity
        };
    }
}
