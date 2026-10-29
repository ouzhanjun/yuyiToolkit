using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia;
using Avalonia.Collections;

namespace yuyiAvalonia.Java.Views;

/// <summary>
/// 圆角星形可绑定元素（任意"外尖数" PointCount，默认 5）。
///
/// 顶点 = 2 × PointCount 个，按 (外尖→内凹) 交替排列、绕中心闭合。
/// 偶数下标 (0,2,4,…) = 外尖；奇数下标 (1,3,5,…) = 内凹。
/// 角度步进 = 2π / (2·PointCount)；首顶点由 RotDeg 决定（默认 -90° 即朝上）。
///
/// 半径通过 Radii 集合传入，长度必须 = 2 · PointCount。
/// 为兼容旧用法，预留 R0..R9 便利属性：PointCount=5 时 R0..R9 全部生效；
/// PointCount=6 时 R0..R11 全部生效，更高则忽略。修改 R0..R9 会自动写回 Radii。
///
/// 算法与 RoundedStarGeometry 一致：对任意 N 个相邻顶点都自洽。
/// </summary>
public partial class RoundedStarElement : ObservableObject
{
    [ObservableProperty] private double _width = 240;
    [ObservableProperty] private double _height = 240;

    [ObservableProperty] private string _fillColor = "#E6B800";
    [ObservableProperty] private string _strokeColor = "#222222";
    [ObservableProperty] private double _strokeWidth = 2;

    [ObservableProperty] private double _outerRatio = 0.46;   // 外尖相对 min 边的比例
    [ObservableProperty] private double _innerRatio = 0.42;   // 内凹占外尖的比例
    [ObservableProperty] private double _rotDeg = -90;        // 起始角度（°）

    /// <summary>外尖数量（决定顶点总数 = 2 × PointCount）。最小 2，最大 64。</summary>
    [ObservableProperty] private int _pointCount = 5;

    // —— 逐顶点圆角半径 —— 长度 = 2 × PointCount ——
    private readonly AvaloniaList<double> _radii = new();
    public AvaloniaList<double> Radii => _radii;

    // —— 兼容旧字段：R0..R9 ——
    // 修改这些属性时若 Radii 还没到对应索引，自动扩容；否则写回 Radii[i]。
    // PointCount > 5 时这些字段最多覆盖前 10 个；其余通过 Radii.Add / SetRadiusAt 维护。
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

    public RoundedStarElement()
    {
        // 默认 5 角星：R0=12, R1=6, R2=12, R3=6, ...
        for (int i = 0; i < 10; i++) _radii.Add(i % 2 == 0 ? 12 : 6);
        _r = new double[] { 12, 6, 12, 6, 12, 6, 12, 6, 12, 6 };

        _radii.CollectionChanged += (_, _) =>
        {
            for (int i = 0; i < Math.Min(_radii.Count, _r.Length); i++)
                _r[i] = _radii[i];
        };
    }

    /// <summary>完整或部分修改 PointCount，自动扩缩 Radii；
    /// 把已有半径值保留，新增位用"外尖 12 / 内凹 6"补齐；删除位截断。</summary>
    public void ResizeToPointCount(int newPointCount)
    {
        newPointCount = Math.Clamp(newPointCount, 2, 64);
        if (newPointCount == PointCount && _radii.Count == 2 * PointCount) return;

        int targetN = 2 * newPointCount;

        // 先扩缩 _radii，最后再公开 PointCount，避免 Rebuild 看到中间状态
        var keep = new double[targetN];
        for (int i = 0; i < targetN; i++)
        {
            if (i < _radii.Count) keep[i] = _radii[i];
            else                 keep[i] = (i % 2 == 0) ? 12 : 6;
        }
        _radii.Clear();
        for (int i = 0; i < targetN; i++) 
            _radii.Add(Math.Max(0, keep[i]));

        // 同步 _r 数组（在公开 PointCount 之前，否则 _r 会与 Radii 不一致）
        if (_r.Length < targetN) Array.Resize(ref _r, Math.Max(targetN, 64));
        for (int i = 0; i < targetN; i++) _r[i] = keep[i];

        // 最后一步：发布 PointCount 变更通知 → 触发 Rebuild，此时 _radii 已同步
        PointCount = newPointCount;
    }

    /// <summary>设置 i 位置的圆角（i ∈ [0, 2·PointCount)）。</summary>
    public void SetRadiusAt(int i, double r)
    {
        if (i < 0 || i >= _radii.Count)
            throw new ArgumentOutOfRangeException(nameof(i));
        _radii[i] = Math.Max(0, r);
    }

    /// <summary>
    /// 返回 2 × PointCount 个相邻顶点（外尖→内凹→…交替，闭合）。
    /// 步进 = π / PointCount；i 增 → 屏幕顺时针。
    /// </summary>
    public Point[] GetPts()
    {
        int n = 2 * PointCount;
        var pts = new Point[n];
        double cx = Width / 2.0, cy = Height / 2.0;
        double outerR = OuterRatio * Math.Min(Width, Height);
        double innerR = outerR * Math.Max(0, InnerRatio);
        double start = RotDeg * Math.PI / 180.0;
        for (int i = 0; i < n; i++)
        {
            double a = start + i * Math.PI / PointCount;
            double r = (i % 2 == 0) ? outerR : innerR;
            pts[i] = new Point(cx + r * Math.Cos(a), cy + r * Math.Sin(a));
        }
        return pts;
    }

    /// <summary>返回逐顶点半径序列，长度 = 2 × PointCount。
    /// 如果 _radii 与 PointCount 不一致（中间状态），直接按 _radii 当前内容返回，避免触发修复造成无限循环。</summary>
    public double[] GetRadii() => _radii.ToArray();

    public RoundedStarElement Clone()
    {
        var c = new RoundedStarElement
        {
            Width = Width, Height = Height,
            FillColor = FillColor, StrokeColor = StrokeColor, StrokeWidth = StrokeWidth,
            OuterRatio = OuterRatio, InnerRatio = InnerRatio, RotDeg = RotDeg,
            PointCount = PointCount
        };
        // 同步 Radii
        c._radii.Clear();
        for (int i = 0; i < _radii.Count; i++) c._radii.Add(_radii[i]);
        // 同步 _r 数组 + R0..R9
        if (c._r.Length < _r.Length) Array.Resize(ref c._r, _r.Length);
        Array.Copy(_r, c._r, _r.Length);
        c.R0 = R0; c.R1 = R1; c.R2 = R2; c.R3 = R3; c.R4 = R4;
        c.R5 = R5; c.R6 = R6; c.R7 = R7; c.R8 = R8; c.R9 = R9;
        return c;
    }

    // —— 内部 R0..R9 <-> Radii 双向 ——
    private double ReadR(int i)
    {
        if (i < _r.Length) return _r[i];
        return (i % 2 == 0) ? 12 : 6;
    }
    private void WriteR(int i, double v)
    {
        if (i >= _r.Length) Array.Resize(ref _r, Math.Max(i + 1, 64));
        _r[i] = Math.Max(0, v);
        if (i < _radii.Count) _radii[i] = _r[i];
        // Radii 不够长时不主动扩容，避免 PointCount 调整前的副作用；用户改 PointCount 后会同步
    }
}