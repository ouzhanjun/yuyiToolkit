using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角多边形可绑定元素（任意边数 PointCount，默认 3 = 三角形）。
/// PointCount 个顶点等距分布在外接圆上，每个顶点有独立的圆角半径。
/// 角度步进 = 2π / PointCount；首顶点由 RotDeg 决定（默认 0° = 正右，顺时针）。
/// </summary>
public partial class RoundedPolygonElement : ObservableObject
{
    [ObservableProperty] private double _width = 120;
    [ObservableProperty] private double _height = 120;

    [ObservableProperty] private string _fillColor = "#4FC3F7";
    [ObservableProperty] private string _strokeColor = "#222222";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private int _pointCount = 3;
    [ObservableProperty] private double _rotDeg = 0;

    [ObservableProperty] private bool _drawPoint = false;

    // 逐顶点圆角半径，长度 = PointCount
    private readonly Avalonia.Collections.AvaloniaList<double> _radii = new();
    public Avalonia.Collections.AvaloniaList<double> Radii => _radii;

    private double[] _r = new double[64];

    public double R0 { get => ReadR(0); set => WriteR(0, value); }
    public double R1 { get => ReadR(1); set => WriteR(1, value); }
    public double R2 { get => ReadR(2); set => WriteR(2, value); }
    public double R3 { get => ReadR(3); set => WriteR(3, value); }
    public double R4 { get => ReadR(4); set => WriteR(4, value); }
    public double R5 { get => ReadR(5); set => WriteR(5, value); }
    public double R6 { get => ReadR(6); set => WriteR(6, value); }
    public double R7 { get => ReadR(7); set => WriteR(7, value); }
    public double R8 { get => ReadR(8); set => WriteR(8, value); }
    public double R9 { get => ReadR(9); set => WriteR(9, value); }

    public RoundedPolygonElement()
    {
        for (int i = 0; i < 32; i++) _radii.Add(0);
        _r = new double[32];
        for (int i = 0; i < 32; i++) _r[i] = 0;

        _radii.CollectionChanged += (_, _) =>
        {
            for (int i = 0; i < Math.Min(_radii.Count, _r.Length); i++)
                _r[i] = _radii[i];
        };
    }

    public void ResizeToPointCount(int newPointCount)
    {
        newPointCount = Math.Clamp(newPointCount, 3, 32);
        if (newPointCount == PointCount && _radii.Count == PointCount) return;

        var keep = new double[newPointCount];
        for (int i = 0; i < newPointCount; i++)
        {
            if (i < _radii.Count) keep[i] = _radii[i];
            else keep[i] = 0;
        }
        _radii.Clear();
        for (int i = 0; i < newPointCount; i++)
            _radii.Add(Math.Max(0, keep[i]));

        if (_r.Length < newPointCount) System.Array.Resize(ref _r, Math.Max(newPointCount, 64));
        for (int i = 0; i < newPointCount; i++) _r[i] = keep[i];

        PointCount = newPointCount;
    }

    public void SetRadiusAt(int i, double r)
    {
        if (i < 0 || i >= _radii.Count)
            throw new System.ArgumentOutOfRangeException(nameof(i));
        _radii[i] = Math.Max(0, r);
    }

    public Avalonia.Point[] GetPts()
    {
        int n = PointCount;
        var pts = new Avalonia.Point[n];
        double cx = Width / 2.0, cy = Height / 2.0;
        double r = Math.Min(Width, Height) / 2.0;
        double start = RotDeg * Math.PI / 180.0;
        for (int i = 0; i < n; i++)
        {
            double a = start + i * 2.0 * Math.PI / n;
            pts[i] = new Avalonia.Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }
        return pts;
    }

    public double[] GetRadii() => _radii.ToArray();

    public RoundedPolygonElement Clone()
    {
        var c = new RoundedPolygonElement
        {
            Width = Width, Height = Height,
            FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
            PointCount = PointCount, RotDeg = RotDeg, DrawPoint = DrawPoint
        };
        c._radii.Clear();
        for (int i = 0; i < _radii.Count; i++) c._radii.Add(_radii[i]);
        if (c._r.Length < _r.Length) System.Array.Resize(ref c._r, _r.Length);
        System.Array.Copy(_r, c._r, _r.Length);
        return c;
    }

    private double ReadR(int i)
    {
        if (i < _r.Length) return _r[i];
        return 0;
    }
    private void WriteR(int i, double v)
    {
        if (i >= _r.Length) System.Array.Resize(ref _r, Math.Max(i + 1, 64));
        _r[i] = Math.Max(0, v);
        if (i < _radii.Count) _radii[i] = _r[i];
    }
}
