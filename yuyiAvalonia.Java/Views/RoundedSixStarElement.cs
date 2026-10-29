using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角六星形的可绑定元素。
/// 外尖→内凹→外尖…共 12 个顶点。下标 0,2,4,6,8,10 是外尖；1,3,5,7,9,11 是内凹。
/// 每个顶点有独立的圆角半径 R0..R11，0 表示尖角。
/// 步进角 = 360°/12 = 30°。
/// </summary>
public partial class RoundedSixStarElement : ObservableObject
{
    [ObservableProperty] private double _width = 240;
    [ObservableProperty] private double _height = 240;

    [ObservableProperty] private string _fillColor = "#9C27B0";
    [ObservableProperty] private string _strokeColor = "#222222";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private double _outerRatio = 0.46;   // 外尖相对 min 边的比例
    [ObservableProperty] private double _innerRatio = 0.42;   // 内凹占外尖的比例
    [ObservableProperty] private double _rotDeg = -90;        // 起始角度（°）

    [ObservableProperty] private double _r0  = 12;
    [ObservableProperty] private double _r1  = 6;
    [ObservableProperty] private double _r2  = 12;
    [ObservableProperty] private double _r3  = 6;
    [ObservableProperty] private double _r4  = 12;
    [ObservableProperty] private double _r5  = 6;
    [ObservableProperty] private double _r6  = 12;
    [ObservableProperty] private double _r7  = 6;
    [ObservableProperty] private double _r8  = 12;
    [ObservableProperty] private double _r9  = 6;
    [ObservableProperty] private double _r10 = 12;
    [ObservableProperty] private double _r11 = 6;

    /// <summary>
    /// 返回 12 个相邻顶点（外尖→内凹→…交替，闭合）。
    /// RotDeg = -90 起点在正上方；按 30° 步进、i 增 → 屏幕顺时针排列。
    /// </summary>
    public Point[] GetPts()
    {
        var pts = new Point[12];
        double cx = Width / 2.0, cy = Height / 2.0;
        double outerR = OuterRatio * Math.Min(Width, Height);
        double innerR = outerR * Math.Max(0, InnerRatio);
        double start = RotDeg * Math.PI / 180.0;
        for (int i = 0; i < 12; i++)
        {
            double a = start + i * Math.PI / 6.0;
            double r = (i % 2 == 0) ? outerR : innerR;
            pts[i] = new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }
        return pts;
    }

    public double[] GetRadii()
        => new[] { R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11 };

    public RoundedSixStarElement Clone() => new()
    {
        Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
        OuterRatio = OuterRatio, InnerRatio = InnerRatio, RotDeg = RotDeg,
        R0 = R0, R1 = R1, R2 = R2, R3 = R3, R4 = R4, R5 = R5,
        R6 = R6, R7 = R7, R8 = R8, R9 = R9, R10 = R10, R11 = R11
    };
}