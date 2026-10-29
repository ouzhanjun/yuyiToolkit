using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 椭圆 / 弧 / 扇形 元素（Figma 风格参数）。
///   - StartAngle   (deg, 0° = 正右，逆时针为正；90° = 正上)
///   - SweepAngle   (deg, [0, 360]；360 = 完整闭合椭圆 / 完整圆环)
///   - InnerRadius  (0..1；相对外接椭圆的比例；0 = 实心 / 弧；1 = 退化；中间 = 甜甜圈 / 扇环)
///   - CornerRadius (外圈两端圆角, 像素)
///
/// 派生形态：
///   InnerRadius = 0, Sweep = 360  → 椭圆（实心）
///   InnerRadius = 0, Sweep < 360  → 扇形（pie）
///   InnerRadius > 0, Sweep = 360  → 圆环 / 椭圆环（donut）
///   InnerRadius > 0, Sweep < 360  → 扇环（ring sector）
/// </summary>
public partial class EllipseElement : ObservableObject
{
    [ObservableProperty] private double _width = 320;
    [ObservableProperty] private double _height = 220;
    [ObservableProperty] private string _fillColor = "#7B61FF";
    [ObservableProperty] private string _strokeColor = "#222222";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private double _startAngle = 0;       // deg, 0 = +x, CCW
    [ObservableProperty] private double _sweepAngle = 360;      // deg
    [ObservableProperty] private double _innerRadius = 0;       // 0..1
    [ObservableProperty] private double _cornerRadius = 0;      // px, 仅对两端切点圆角（简化：先保留接口）

    public EllipseElement Clone() => new()
    {
        Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
        StartAngle = StartAngle, SweepAngle = SweepAngle,
        InnerRadius = InnerRadius, CornerRadius = CornerRadius
    };
}