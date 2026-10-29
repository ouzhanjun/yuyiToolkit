using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角矩形的可绑定元素。
/// 4 个顶点顺时针：左上 → 右上 → 右下 → 左下（闭合）。
/// 每个顶点有独立的圆角半径 R0..R3，0 表示尖角。
///   R0 = 左上   R1 = 右上
///   R2 = 右下   R3 = 左下
/// </summary>
public partial class RoundedRectangleElement : ObservableObject
{
    [ObservableProperty] private double _width = 240;
    [ObservableProperty] private double _height = 160;

    [ObservableProperty] private string _fillColor = "#4F8DD3";
    [ObservableProperty] private string _strokeColor = "#222222";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private double _r0 = 14;   // 左上
    [ObservableProperty] private double _r1 = 14;   // 右上
    [ObservableProperty] private double _r2 = 14;   // 右下
    [ObservableProperty] private double _r3 = 14;   // 左下

    /// <summary>
    /// 顺时针顶点序列（左上 → 右上 → 右下 → 左下）。
    /// Avalonia y 朝下：顶点 y 越小越靠屏幕上方。
    ///   左上 = (0,    0)
    ///   右上 = (W,    0)
    ///   右下 = (W,    H)
    ///   左下 = (0,    H)
    /// </summary>
    public Point[] GetPts()
    {
        double w = Width, h = Height;
        return new[]
        {
            new Point(0, 0),   // 左上  R0
            new Point(w, 0),   // 右上  R1
            new Point(w, h),   // 右下  R2
            new Point(0, h),   // 左下  R3
        };
    }

    public double[] GetRadii() => new[] { R0, R1, R2, R3 };

    public RoundedRectangleElement Clone() => new()
    {
        Width = Width, Height = Height,
        FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
        R0 = R0, R1 = R1, R2 = R2, R3 = R3
    };
}