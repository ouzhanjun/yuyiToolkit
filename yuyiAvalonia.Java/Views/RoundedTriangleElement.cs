using CommunityToolkit.Mvvm.ComponentModel;
using Point = Avalonia.Point;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角三角形元素：三个顶点 + 三个独立圆角半径。
/// 顶点坐标为相对于自身尺寸的比例（0-1），圆角半径为像素绝对值。
/// </summary>
public partial class RoundedTriangleElement : ObservableObject
{
    [ObservableProperty] private double _width = 240;
    [ObservableProperty] private double _height = 220;
    [ObservableProperty] private string _fillColor = "#FF4F8EF7";
    [ObservableProperty] private string _strokeColor = "#FF1F5DC9";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private double _p1X = 0.5;
    [ObservableProperty] private double _p1Y = 0.0;
    [ObservableProperty] private double _p2X = 0.0;
    [ObservableProperty] private double _p2Y = 1.0;
    [ObservableProperty] private double _p3X = 1.0;
    [ObservableProperty] private double _p3Y = 1.0;

    [ObservableProperty] private double _r1 = 0;
    [ObservableProperty] private double _r2 = 0;
    [ObservableProperty] private double _r3 = 0;

    public Point GetP1() => new Point(P1X * Width, P1Y * Height);
    public Point GetP2() => new Point(P2X * Width, P2Y * Height);
    public Point GetP3() => new Point(P3X * Width, P3Y * Height);

    public RoundedTriangleElement Clone() => new()
    {
        Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
        P1X = P1X, P1Y = P1Y, P2X = P2X, P2Y = P2Y, P3X = P3X, P3Y = P3Y,
        R1 = R1, R2 = R2, R3 = R3
    };
}