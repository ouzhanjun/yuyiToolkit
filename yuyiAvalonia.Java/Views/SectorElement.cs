using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 扇形 / 弧 / 椭圆 / 圆环 / 扇环 元素。
///   - StartAngle  (deg, 0° = 正右，CCW 为正)
///   - SweepAngle  (deg, [-360, 360]；绝对值 >= 360 视为完整闭合)
///   - InnerRadius (0..1, 相对外圈半径的比例；0 = 实心；>0 = 甜甜圈)
///   - CornerRadius (像素, 尖角圆角半径；0 = 锐利)
/// </summary>
public partial class SectorElement : ObservableObject
{
    [ObservableProperty] private double _width = 320;
    [ObservableProperty] private double _height = 220;

    [ObservableProperty] private string _fillColor = "#7B61FF";
    [ObservableProperty] private string _strokeColor = "#222222";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private double _startAngle = 0;
    [ObservableProperty] private double _sweepAngle = 360;
    [ObservableProperty] private double _innerRadius = 0;
    [ObservableProperty] private double _cornerRadius = 0;

    public SectorElement Clone() => new()
    {
        Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
        StartAngle = StartAngle, SweepAngle = SweepAngle,
        InnerRadius = InnerRadius, CornerRadius = CornerRadius
    };
}
