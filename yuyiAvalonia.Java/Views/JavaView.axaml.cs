using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using APath = Avalonia.Controls.Shapes.Path;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.Java.Views;

public partial class JavaView : JavaView2
{
    // ============ 星形 ============
    private readonly RoundedStarElement _star = new()
    {
        Width = 300, Height = 300,
        OuterRatio = 0.46, InnerRatio = 0.42, RotDeg = -90,
        R0 = 12, R1 = 6, R2 = 12, R3 = 6, R4 = 12,
        R5 = 6, R6 = 12, R7 = 6, R8 = 12, R9 = 6
    };
    private RoundedStarControl? _starCtrl;

    // ============ 多边形 ============
    private readonly RoundedPolygonElement _poly = new()
    {
        Width = 260, Height = 260,
        PointCount = 3, RotDeg = 0,
        R0 = 0, R1 = 0, R2 = 0
    };
    private RoundedPolygonControl? _polyCtrl;

    // ============ 矩形 ============
    private readonly RoundedRectangleElement _rect = new()
    {
        Width = 280, Height = 180,
        R0 = 20, R1 = 20, R2 = 20, R3 = 20
    };
    private RoundedRectangleControl? _rectCtrl;

    // ============ 扇形 ============
    private readonly SectorElement _sector = new()
    {
        Width = 320, Height = 220,
        StartAngle = 0, SweepAngle = 360, InnerRadius = 0
    };
    private SectorFlowControl? _sectorCtrl;

    public JavaView()
    {
    }

    #region Tab Switching

    private void SelectTab(string tab)
    {
        PanelStar.IsVisible = tab == "star";
        PanelPolygon.IsVisible = tab == "polygon";
        PanelRect.IsVisible = tab == "rect";
        PanelSector.IsVisible = tab == "sector";
        PanelBezier.IsVisible = tab == "bezier";

        // Update button styles
        var defFont = FontWeight.Normal;
        var activeFont = FontWeight.Bold;

        BtnTabStar.FontWeight = tab == "star" ? activeFont : defFont;
        BtnTabPolygon.FontWeight = tab == "polygon" ? activeFont : defFont;
        BtnTabRect.FontWeight = tab == "rect" ? activeFont : defFont;
        BtnTabSector.FontWeight = tab == "sector" ? activeFont : defFont;
        BtnTabBezier.FontWeight = tab == "bezier" ? activeFont : defFont;
    }

    private void BtnShowStar_Click(object? s, RoutedEventArgs e) => SelectTab("star");
    private void BtnShowPolygon_Click(object? s, RoutedEventArgs e) => SelectTab("polygon");
    private void BtnShowRect_Click(object? s, RoutedEventArgs e) => SelectTab("rect");
    private void BtnShowSector_Click(object? s, RoutedEventArgs e) => SelectTab("sector");
    private void BtnShowBezier_Click(object? s, RoutedEventArgs e) => SelectTab("bezier");

    #endregion

    #region Build Controls

    private void BuildStar()
    {
        _starCtrl = new RoundedStarControl
        {
            Element = _star,
            Width = _star.Width,
            Height = _star.Height,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        StarHost.Children.Add(_starCtrl);
    }

    private void BuildPolygon()
    {
        _polyCtrl = new RoundedPolygonControl
        {
            Element = _poly,
            Width = _poly.Width,
            Height = _poly.Height,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        PolyHost.Children.Add(_polyCtrl);
    }

    private void BuildRectangle()
    {
        _rectCtrl = new RoundedRectangleControl
        {
            Element = _rect,
            Width = _rect.Width,
            Height = _rect.Height,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        RectHost.Children.Add(_rectCtrl);
    }

    private void BuildSector()
    {
        _sectorCtrl = new SectorFlowControl
        {
            Element = _sector,
            Width = _sector.Width,
            Height = _sector.Height,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        SectorHost.Children.Add(_sectorCtrl);
    }

    #endregion

    #region Wire

    private void Wire()
    {
        void Bind(Slider s, Action<double> set, double min, double max, double init)
        {
            s.Minimum = min; s.Maximum = max; s.Value = init;
            s.PropertyChanged += (_, e) =>
            {
                if (e.Property == Slider.ValueProperty)
                {
                    set(s.Value);
                }
            };
        }

        void Refresh(Action a)
        {
            a();
        }

        // === 星形 ===
        Bind(StarWSlider, v => { _star.Width = v; _starCtrl!.Width = v; _starCtrl.InvalidateArrange(); _starCtrl.Refresh(); UpdateStarInfo(); }, 80, 500, 300);
        Bind(StarHSlider, v => { _star.Height = v; _starCtrl!.Height = v; _starCtrl.InvalidateArrange(); _starCtrl.Refresh(); UpdateStarInfo(); }, 80, 500, 300);
        Bind(StarStrokeSlider, v => { _star.StrokeWidth = v; _starCtrl!.Refresh(); }, 0, 10, 2);
        Bind(StarInnerRatioSlider, v => { _star.InnerRatio = v; _starCtrl!.Refresh(); UpdateStarInfo(); }, 0, 0.9, 0.42);

        StarPointSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == Slider.ValueProperty)
            {
                int n = (int)StarPointSlider.Value;
                _star.ResizeToPointCount(n);
                StarPointText.Text = n.ToString();
                StarSyncRadiiSliders();
                UpdateStarInfo();
            }
        };

        var starSliders = new (Slider s, int idx)[]
        {
            (SR0,0),(SR1,1),(SR2,2),(SR3,3),(SR4,4),(SR5,5),(SR6,6),(SR7,7),
            (SR8,8),(SR9,9),(SR10,10),(SR11,11),(SR12,12),(SR13,13),(SR14,14),(SR15,15)
        };
        foreach (var (sl, i) in starSliders)
        {
            int idx = i;
            double init = (i % 2 == 0) ? 12 : 6;
            Bind(sl, v =>
            {
                if (idx < _star.Radii.Count) _star.SetRadiusAt(idx, v);
                else if (idx < 2 * _star.PointCount) _star.Radii.Add(v);
                _starCtrl!.Refresh();
                UpdateStarInfo();
            }, 0, 60, init);
        }

        // === 多边形 ===
        Bind(PolyWSlider, v => { _poly.Width = v; _polyCtrl!.Width = v; _polyCtrl.InvalidateArrange(); _polyCtrl.Refresh(); UpdatePolyInfo(); }, 80, 500, 260);
        Bind(PolyHSlider, v => { _poly.Height = v; _polyCtrl!.Height = v; _polyCtrl.InvalidateArrange(); _polyCtrl.Refresh(); UpdatePolyInfo(); }, 80, 500, 260);
        Bind(PolyStrokeSlider, v => { _poly.StrokeWidth = v; _polyCtrl!.Refresh(); }, 0, 10, 2);

        PolyPointSlider.PropertyChanged += (_, e) =>
        {
            if (e.Property == Slider.ValueProperty)
            {
                int n = (int)PolyPointSlider.Value;
                _poly.ResizeToPointCount(n);
                PolyPointText.Text = n.ToString();
                PolySyncRadiiSliders();
                UpdatePolyInfo();
            }
        };

        var polySliders = new (Slider s, int idx)[]
        {
            (PR0,0),(PR1,1),(PR2,2),(PR3,3),(PR4,4),(PR5,5),(PR6,6),(PR7,7),
            (PR8,8),(PR9,9),(PR10,10),(PR11,11),(PR12,12),(PR13,13),(PR14,14),(PR15,15)
        };
        foreach (var (sl, i) in polySliders)
        {
            int idx = i;
            Bind(sl, v =>
            {
                if (idx < _poly.Radii.Count) _poly.SetRadiusAt(idx, v);
                else if (idx < _poly.PointCount) _poly.Radii.Add(v);
                _polyCtrl!.Refresh();
                UpdatePolyInfo();
            }, 0, 80, 0);
        }

        // === 矩形 ===
        Bind(RectWSlider, v => { _rect.Width = v; _rectCtrl!.Width = v; _rectCtrl.InvalidateArrange(); _rectCtrl.Refresh(); UpdateRectInfo(); }, 80, 500, 280);
        Bind(RectHSlider, v => { _rect.Height = v; _rectCtrl!.Height = v; _rectCtrl.InvalidateArrange(); _rectCtrl.Refresh(); UpdateRectInfo(); }, 80, 500, 180);
        Bind(RectStrokeSlider, v => { _rect.StrokeWidth = v; _rectCtrl!.Refresh(); }, 0, 10, 2);
        Bind(RR0, v => { _rect.R0 = v; _rectCtrl!.Refresh(); UpdateRectInfo(); }, 0, 100, 20);
        Bind(RR1, v => { _rect.R1 = v; _rectCtrl!.Refresh(); UpdateRectInfo(); }, 0, 100, 20);
        Bind(RR2, v => { _rect.R2 = v; _rectCtrl!.Refresh(); UpdateRectInfo(); }, 0, 100, 20);
        Bind(RR3, v => { _rect.R3 = v; _rectCtrl!.Refresh(); UpdateRectInfo(); }, 0, 100, 20);

        // === 扇形 ===
        Bind(SectorWSlider, v => { _sector.Width = v; _sectorCtrl!.Width = v; _sectorCtrl.InvalidateArrange(); _sectorCtrl.Refresh(); UpdateSectorInfo(); }, 60, 600, 320);
        Bind(SectorHSlider, v => { _sector.Height = v; _sectorCtrl!.Height = v; _sectorCtrl.InvalidateArrange(); _sectorCtrl.Refresh(); UpdateSectorInfo(); }, 60, 600, 220);
        Bind(SectorStrokeSlider, v => { _sector.StrokeWidth = v; _sectorCtrl!.Refresh(); }, 0, 10, 2);
        Bind(SectorStartSlider, v => { _sector.StartAngle = v; _sectorCtrl!.Refresh(); UpdateSectorInfo(); }, -360, 720, 0);
        Bind(SectorSweepSlider, v => { _sector.SweepAngle = v; _sectorCtrl!.Refresh(); UpdateSectorInfo(); }, -360, 360, 360);
        Bind(SectorInnerSlider, v => { _sector.InnerRadius = v; _sectorCtrl!.Refresh(); UpdateSectorInfo(); }, 0, 0.95, 0);
        Bind(SectorCornerSlider, v => { _sector.CornerRadius = v; _sectorCtrl!.Refresh(); UpdateSectorInfo(); }, 0, 64, 0);
    }

    #endregion

    #region Star Buttons

    private Slider[] StarSliders => new[] { SR0, SR1, SR2, SR3, SR4, SR5, SR6, SR7, SR8, SR9, SR10, SR11, SR12, SR13, SR14, SR15 };

    private void StarSyncRadiiSliders()
    {
        int n = _star.Radii.Count;
        for (int i = 0; i < StarSliders.Length; i++)
        {
            var s = StarSliders[i];
            s.Value = (i < n) ? _star.Radii[i] : 0;
            s.IsEnabled = i < n;
        }
    }

    private void StarSetPointCount(int n)
    {
        n = Math.Clamp(n, 2, 32);
        _star.ResizeToPointCount(n);
        StarPointSlider.Value = n;
        StarPointText.Text = n.ToString();
        StarSyncRadiiSliders();
        UpdateStarInfo();
    }

    private void SetStarR(double v)
    {
        for (int i = 0; i < _star.Radii.Count; i++) _star.SetRadiusAt(i, v);
        StarSyncRadiiSliders();
        _starCtrl!.Refresh();
        UpdateStarInfo();
    }

    private void SetStarOuterOnly(double vOuter, double vInner)
    {
        for (int i = 0; i < _star.Radii.Count; i++)
            _star.SetRadiusAt(i, i % 2 == 0 ? vOuter : vInner);
        StarSyncRadiiSliders();
        _starCtrl!.Refresh();
        UpdateStarInfo();
    }

    private void BtnStarPreset2_Click(object? s, RoutedEventArgs e) => StarSetPointCount(2);
    private void BtnStarPreset3_Click(object? s, RoutedEventArgs e) => StarSetPointCount(3);
    private void BtnStarPreset4_Click(object? s, RoutedEventArgs e) => StarSetPointCount(4);
    private void BtnStarPreset5_Click(object? s, RoutedEventArgs e) => StarSetPointCount(5);
    private void BtnStarPreset6_Click(object? s, RoutedEventArgs e) => StarSetPointCount(6);
    private void BtnStarPreset8_Click(object? s, RoutedEventArgs e) => StarSetPointCount(8);
    private void BtnStarPreset10_Click(object? s, RoutedEventArgs e) => StarSetPointCount(10);
    private void BtnStarPreset12_Click(object? s, RoutedEventArgs e) => StarSetPointCount(12);
    private void BtnStarPreset16_Click(object? s, RoutedEventArgs e) => StarSetPointCount(16);
    private void BtnStarSharp_Click(object? s, RoutedEventArgs e) => SetStarR(0);
    private void BtnStarOuterRound_Click(object? s, RoutedEventArgs e) => SetStarOuterOnly(20, 4);
    private void BtnStarAllRound_Click(object? s, RoutedEventArgs e) => SetStarOuterOnly(18, 14);
    private void BtnStarReset_Click(object? s, RoutedEventArgs e)
    {
        StarSetPointCount(5);
        _star.InnerRatio = 0.42;
        StarInnerRatioSlider.Value = 0.42;
        SetStarOuterOnly(12, 6);
    }

    #endregion

    #region Polygon Buttons

    private Slider[] PolySliders => new[] { PR0, PR1, PR2, PR3, PR4, PR5, PR6, PR7, PR8, PR9, PR10, PR11, PR12, PR13, PR14, PR15 };

    private void PolySyncRadiiSliders()
    {
        int n = _poly.Radii.Count;
        for (int i = 0; i < PolySliders.Length; i++)
        {
            var s = PolySliders[i];
            s.Value = (i < n) ? _poly.Radii[i] : 0;
            s.IsEnabled = i < n;
        }
    }

    private void PolySetPointCount(int n)
    {
        n = Math.Clamp(n, 3, 32);
        _poly.ResizeToPointCount(n);
        PolyPointSlider.Value = n;
        PolyPointText.Text = n.ToString();
        PolySyncRadiiSliders();
        UpdatePolyInfo();
    }

    private void SetPolyR(double v)
    {
        for (int i = 0; i < _poly.Radii.Count; i++) _poly.SetRadiusAt(i, v);
        PolySyncRadiiSliders();
        _polyCtrl!.Refresh();
        UpdatePolyInfo();
    }

    private void BtnPoly3_Click(object? s, RoutedEventArgs e) => PolySetPointCount(3);
    private void BtnPoly4_Click(object? s, RoutedEventArgs e) => PolySetPointCount(4);
    private void BtnPoly5_Click(object? s, RoutedEventArgs e) => PolySetPointCount(5);
    private void BtnPoly6_Click(object? s, RoutedEventArgs e) => PolySetPointCount(6);
    private void BtnPoly8_Click(object? s, RoutedEventArgs e) => PolySetPointCount(8);
    private void BtnPoly10_Click(object? s, RoutedEventArgs e) => PolySetPointCount(10);
    private void BtnPoly12_Click(object? s, RoutedEventArgs e) => PolySetPointCount(12);
    private void BtnPoly16_Click(object? s, RoutedEventArgs e) => PolySetPointCount(16);
    private void BtnPolySharp_Click(object? s, RoutedEventArgs e) => SetPolyR(0);
    private void BtnPolyAllRound_Click(object? s, RoutedEventArgs e)
    {
        double maxR = Math.Min(_poly.Width, _poly.Height) * 0.3;
        SetPolyR(maxR);
    }
    private void BtnPolyReset_Click(object? s, RoutedEventArgs e)
    {
        PolySetPointCount(3);
        SetPolyR(0);
    }

    #endregion

    #region Rectangle Buttons

    private void SetRectR(double r0, double r1, double r2, double r3)
    {
        RR0.Value = r0; RR1.Value = r1; RR2.Value = r2; RR3.Value = r3;
        _rect.R0 = r0; _rect.R1 = r1; _rect.R2 = r2; _rect.R3 = r3;
        _rectCtrl!.Refresh();
        UpdateRectInfo();
    }

    private void BtnRectSharp_Click(object? s, RoutedEventArgs e) => SetRectR(0, 0, 0, 0);
    private void BtnRectAllRound_Click(object? s, RoutedEventArgs e)
    {
        double r = Math.Min(_rect.Width, _rect.Height) * 0.3;
        SetRectR(r, r, r, r);
    }
    private void BtnRectTopRound_Click(object? s, RoutedEventArgs e)
    {
        double r = Math.Min(_rect.Width, _rect.Height) * 0.3;
        SetRectR(r, r, 0, 0);
    }
    private void BtnRectLeftRound_Click(object? s, RoutedEventArgs e)
    {
        double r = Math.Min(_rect.Width, _rect.Height) * 0.3;
        SetRectR(r, 0, 0, r);
    }
    private void BtnRectDiagRound_Click(object? s, RoutedEventArgs e)
    {
        double r = Math.Min(_rect.Width, _rect.Height) * 0.3;
        SetRectR(r, 0, r, 0);
    }
    private void BtnRectReset_Click(object? s, RoutedEventArgs e)
    {
        RectWSlider.Value = 280; RectHSlider.Value = 180;
        SetRectR(20, 20, 20, 20);
    }

    #endregion

    #region Sector Buttons

    private void SetSectorPreset(double start, double sweep, double inner)
    {
        SectorStartSlider.Value = start;
        SectorSweepSlider.Value = sweep;
        SectorInnerSlider.Value = inner;
        _sector.StartAngle = start;
        _sector.SweepAngle = sweep;
        _sector.InnerRadius = inner;
        _sectorCtrl!.Refresh();
        UpdateSectorInfo();
    }

    private void BtnSectorFull_Click(object? s, RoutedEventArgs e) => SetSectorPreset(0, 360, 0);
    private void BtnSectorHalf_Click(object? s, RoutedEventArgs e) => SetSectorPreset(0, 180, 0);
    private void BtnSectorPie_Click(object? s, RoutedEventArgs e) => SetSectorPreset(-90, 90, 0);
    private void BtnSectorArc_Click(object? s, RoutedEventArgs e) => SetSectorPreset(-30, 120, 0);
    private void BtnSectorRing_Click(object? s, RoutedEventArgs e) => SetSectorPreset(0, 360, 0.5);
    private void BtnSectorRingSector_Click(object? s, RoutedEventArgs e) => SetSectorPreset(-90, 120, 0.5);
    private void BtnSectorQuarter_Click(object? s, RoutedEventArgs e) => SetSectorPreset(-90, 90, 0);
    private void BtnSectorReset_Click(object? s, RoutedEventArgs e)
    {
        SectorWSlider.Value = 320; SectorHSlider.Value = 220;
        SetSectorPreset(0, 360, 0);
    }

    #endregion

    #region Info Text

    private void UpdateStarInfo()
    {
        int n = _star.PointCount;
        int pts = 2 * n;
        string radiiStr = "";
        if (_star.Radii.Count > 0)
        {
            var r = _star.Radii.Take(Math.Min(8, _star.Radii.Count)).Select(x => $"{x:0.0}").ToArray();
            radiiStr = "R=" + string.Join(",", r);
        }
        StarInfoText.Text = $"顶点数={pts} | 内凹={_star.InnerRatio:0.00} | {radiiStr}";
    }

    private void UpdatePolyInfo()
    {
        int n = _poly.PointCount;
        string radiiStr = "";
        if (_poly.Radii.Count > 0)
        {
            var r = _poly.Radii.Take(Math.Min(8, _poly.Radii.Count)).Select(x => $"{x:0.0}").ToArray();
            radiiStr = "R=" + string.Join(",", r);
        }
        PolyInfoText.Text = $"边数={n} | {radiiStr}";
    }

    private void UpdateRectInfo()
    {
        RectInfoText.Text = $"Size=({_rect.Width:0}x{_rect.Height:0}) R0={_rect.R0:0.0} R1={_rect.R1:0.0} R2={_rect.R2:0.0} R3={_rect.R3:0.0}";
    }

    private void UpdateSectorInfo()
    {
        double s = _sector.SweepAngle, i = _sector.InnerRadius;
        string shape = i > 0.0001
            ? (Math.Abs(s) >= 360 ? "椭圆环" : "扇环")
            : (Math.Abs(s) >= 360 ? "椭圆" : "扇形/弧");
        SectorInfoText.Text =
            $"形态：{shape}\n" +
            $"起始角 = {_sector.StartAngle:0.0}°\n" +
            $"扫描角 = {_sector.SweepAngle:0.0}°\n" +
            $"内圈占比 = {_sector.InnerRadius:0.00}\n" +
            $"尖角圆角 = {_sector.CornerRadius:0}px";
    }

    #endregion

    #region Bezier Demo (纯代码构建，不依赖 XAML binding)

    /// <summary>
    /// 一个 Bezier 演示面板：
    ///   - 一个 Canvas 绘制区域
    ///   - 多个可拖动圆点
    ///   - 拖动点后根据曲线类型实时重绘曲线
    /// </summary>
    private sealed class BezierPanel
    {
        public Border Root;                   // 外层卡片
        public Canvas Surface = null!;        // 绘图区
        public Border CanvasBorder = null!;   // 包含 Surface 的带边框 Border
        public APath PathShape = null!;        // 路径图形
        public List<Ellipse> Handles = new(); // 控制点圆点
        public List<Line> Guides = new();     // 贝塞尔切线
        public List<Point> Points = new();    // 当前控制点位置
        public int HoverIndex = -1;           // 当前正在拖动的点
        public Point LastMousePos;            // 拖动增量基准
        public string Title = "";
        public string PathColor = "#4FC3F7";

        public void RebuildPath()
        {
            var geo = new PathGeometry();
            if (Points.Count == 0) { PathShape.Data = geo; return; }
            var fig = new PathFigure { StartPoint = Points[0], IsClosed = false };
            geo.Figures!.Add(fig);

            switch (Points.Count)
            {
                case 2: // 直线 L
                    fig.Segments!.Add(new Avalonia.Media.LineSegment { Point = Points[1] });
                    break;
                case 3: // 二次贝塞尔 Q
                    fig.Segments!.Add(new QuadraticBezierSegment { Point1 = Points[1], Point2 = Points[2] });
                    break;
                case 4: // 三次贝塞尔 C
                    fig.Segments!.Add(new BezierSegment { Point1 = Points[1], Point2 = Points[2], Point3 = Points[3] });
                    break;
            }

            PathShape.Data = geo;

            // 切线（贝塞尔）
            Guides.Clear();
            if (Points.Count == 3)
            {
                Guides.Add(MakeGuide(Points[0], Points[1], "#5581C784"));
                Guides.Add(MakeGuide(Points[1], Points[2], "#5581C784"));
            }
            else if (Points.Count == 4)
            {
                Guides.Add(MakeGuide(Points[0], Points[1], "#55BA68C8"));
                Guides.Add(MakeGuide(Points[2], Points[3], "#55BA68C8"));
            }

            // 重画 control points
            for (int i = 0; i < Handles.Count && i < Points.Count; i++)
            {
                Canvas.SetLeft(Handles[i], Points[i].X - 8);
                Canvas.SetTop(Handles[i], Points[i].Y - 8);
            }
        }

        private Line MakeGuide(Point a, Point b, string color)
        {
            var line = new Line
            {
                StartPoint = a,
                EndPoint = b,
                Stroke = new SolidColorBrush(Color.Parse(color)),
                StrokeThickness = 1,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 3, 3 },
            };
            Surface.Children.Add(line);
            return line;
        }
    }

    /// <summary>构造 Bezier 演示面板并添加到 BezierContainer。</summary>
    private void BuildBezier()
    {
        // 标题
        var title = new TextBlock
        {
            Text = "Path 命令演示 —— 拖动控制点实时观察形状变化",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF)),
            Margin = new Thickness(0, 0, 0, 8)
        };
        BezierContainer.Children.Add(title);

        // 面板 1：直线 L
        AddLinePanel();

        // 面板 2：椭圆弧 A —— 用 LinePanel 类似的结构，但额外几个 Slider 来控制 Rx/Ry/Rotation/Large/Sweep
        AddArcPanel();

        // 面板 3：二次贝塞尔 Q
        AddBezierPanel("二次贝塞尔 (Q)",
            "#81C784",
            new[] { new Point(60, 200), new Point(280, 20), new Point(440, 220) });

        // 面板 4：三次贝塞尔 C
        AddBezierPanel("三次贝塞尔 (C)",
            "#BA68C8",
            new[] { new Point(60, 220), new Point(60, 20), new Point(440, 20), new Point(440, 220) });

        // ========== 互转演示 ==========
        AddConversionDemo();
    }

    /// <summary>路径段互转演示面板：直线 ↔ 二次/三次贝塞尔、圆弧 ↔ 贝塞尔。</summary>
    private void AddConversionDemo()
    {
        const double panelHeight = 320;
        const string bgDark = "#252525";
        const string bgCard = "#2D2D2D";
        const string borderClr = "#444";

        // ---- 标题 ----
        var sectionTitle = new TextBlock
        {
            Text = "路径段互转演示（PathSegmentConversion）",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x80)),
            Margin = new Thickness(0, 16, 0, 12)
        };
        BezierContainer.Children.Add(sectionTitle);

        // ========== 第一行：Line ↔ Bezier ==========
        var row1 = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*,*"), Margin = new Thickness(0, 0, 0, 12) };

        // --- 直线 ↔ 二次贝塞尔（单一画布，互斥切换 + 拖动控制点）---
        var card1 = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(4),
        };
        BuildLineQuadraticCard(card1, panelHeight);
        Grid.SetColumn(card1, 0);
        row1.Children.Add(card1);

        // --- 二次 → 三次贝塞尔 ---
        var card2 = MakeConversionCard("二次 → 三次贝塞尔\n(等效提升公式)", "#81C784", "#BA68C8");
        var p2 = new ConversionContext { CurveType = CurveType.Quadratic, Title = "源：二次贝塞尔（拖动三个控制点）", ConvertLabel = "提升为三次贝塞尔" };
        BuildConversionPath(card2, p2, panelHeight);
        Grid.SetColumn(card2, 1);
        row1.Children.Add(card2);

        // --- 三次贝塞尔 → 近似二次 ---
        var card3 = MakeConversionCard("三次 → 近似二次\n(中点切线交点)", "#BA68C8", "#FFB74D");
        var p3 = new ConversionContext { CurveType = CurveType.Cubic, Title = "源：三次贝塞尔（拖动四个控制点）", ConvertLabel = "转换为近似二次" };
        BuildConversionPath(card3, p3, panelHeight);
        Grid.SetColumn(card3, 2);
        row1.Children.Add(card3);

        BezierContainer.Children.Add(row1);

        // ========== 第二行：圆弧互转 ==========
        var row2 = new Grid { ColumnDefinitions = new ColumnDefinitions("*,*"), Margin = new Thickness(0, 0, 0, 12) };

        // --- 圆弧 → 二次贝塞尔 ---
        var card4 = MakeConversionCard("圆弧 → 二次贝塞尔\n(采样中点作控制点)", "#FF8A65", "#81C784");
        var p4 = new ConversionContext { CurveType = CurveType.Arc, Title = "源：圆弧（拖动圆心或半径点）", ConvertLabel = "转换为二次贝塞尔" };
        BuildConversionPath(card4, p4, panelHeight);
        Grid.SetColumn(card4, 0);
        row2.Children.Add(card4);

        // --- 圆弧 → 三次贝塞尔 ---
        var card5 = MakeConversionCard("圆弧 → 三次贝塞尔\n(三等分点作控制点)", "#FF8A65", "#BA68C8");
        var p5 = new ConversionContext { CurveType = CurveType.Arc, Title = "源：圆弧（拖动圆心或半径点）", ConvertLabel = "转换为三次贝塞尔" };
        BuildConversionPath(card5, p5, panelHeight);
        Grid.SetColumn(card5, 1);
        row2.Children.Add(card5);

        BezierContainer.Children.Add(row2);

        // ========== 说明文字 ==========
        var desc = new TextBlock
        {
            Text =
                "互转说明：\n" +
                "• 直线→二次贝塞尔：控制点 = lerp(起点, 终点, 0.5)\n" +
                "• 直线→三次贝塞尔：C1=lerp(起点,终点,1/3)，C2=lerp(起点,终点,2/3)\n" +
                "• 二次→三次贝塞尔：C1=lerp(control,a,2/3)，C2=lerp(control,b,2/3)\n" +
                "• 三次→近似二次：control = 曲线中点处两边切线的交点\n" +
                "• 圆弧→贝塞尔：采样弧上 t=0.5 或 t=1/3,2/3 处的点作为控制点",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 20)
        };
        BezierContainer.Children.Add(desc);
    }

    /// <summary>直线 ↔ 二次贝塞尔 互斥演示状态：单一画布，可来回切换 + 自由拖动控制点。</summary>
    private sealed class LineQuadraticState
    {
        public enum Mode { Line, Quadratic }
        public Mode Current = Mode.Line;
        public Canvas Surface = null!;
        public List<Point> Points = new();
        public List<string> HandleColors = new();
        public List<Ellipse> Handles = new();
        public TextBlock StatusText = null!;
        public Button ToggleBtn = null!;
        public int HoverIndex = -1;
        public Point LastMousePos;
    }

    /// <summary>构造"直线 ↔ 二次贝塞尔"演示卡片：单一画布，按钮切换形态，互斥显示。</summary>
    private void BuildLineQuadraticCard(Border card, double height)
    {
        var root = new StackPanel { Spacing = 6 };
        card.Child = root;

        // 状态标题
        var statusText = new TextBlock
        {
            Text = "当前：直线 (L) —— 2 个端点",
            FontWeight = FontWeight.Bold,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0x4F, 0xC3, 0xF7)),
        };
        root.Children.Add(statusText);

        // 画布（单一显示区域）
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x25)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Width = 500,
            Height = height,
            ClipToBounds = true,
        };
        var canvas = new Canvas { Background = Brushes.Transparent, Width = 500, Height = height };
        border.Child = canvas;
        root.Children.Add(border);

        // 转换按钮
        var btn = new Button
        {
            Content = "▶ 提升为二次贝塞尔 (在两端点中点正上方插入控制点)",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Thickness(14, 6),
            Margin = new Thickness(0, 4),
            Background = new SolidColorBrush(Color.FromRgb(0x4A, 0x4A, 0x4A)),
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x80)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x4D)),
            BorderThickness = new Thickness(1),
        };
        root.Children.Add(btn);

        // 提示
        var hint = new TextBlock
        {
            Text = "• 拖动任意圆点移动端点 / 控制点\n" +
                   "• 互斥切换：直线 ↔ 二次贝塞尔 同时只存在一种\n" +
                   "• 转换时插入的控制点位于中点正上方（明显偏移，避免曲线退化为直线）",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };
        root.Children.Add(hint);

        // 状态
        var st = new LineQuadraticState
        {
            Current = LineQuadraticState.Mode.Line,
            Surface = canvas,
            StatusText = statusText,
            ToggleBtn = btn,
        };
        InitLinePoints(st);

        // 按钮事件
        btn.Click += (_, _) => ToggleLineQuadratic(st);

        // 拖动
        WireLineQuadraticDrag(st);

        // 首次绘制
        RedrawLineQuadratic(st);
    }

    /// <summary>初始化直线 2 个端点位置。</summary>
    private void InitLinePoints(LineQuadraticState st)
    {
        double y = st.Surface.Height / 2;
        st.Points.Clear();
        st.HandleColors.Clear();
        st.Points.Add(new Point(80, y));
        st.Points.Add(new Point(420, y));
        st.HandleColors.Add("#4FC3F7");
        st.HandleColors.Add("#4FC3F7");
    }

    /// <summary>切换 Line ↔ Quadratic 形态（互斥）。</summary>
    private void ToggleLineQuadratic(LineQuadraticState st)
    {
        if (st.Current == LineQuadraticState.Mode.Line)
        {
            // 直线 → 二次贝塞尔：在两端点中点正上方插入控制点（明显偏移，避免曲线退化为直线）
            var a = st.Points[0];
            var b = st.Points[1];
            double offset = 80;
            var ctrl = new Point((a.X + b.X) / 2, Math.Max(10, (a.Y + b.Y) / 2 - offset));
            st.Points = new List<Point> { a, ctrl, b };
            st.HandleColors = new List<string> { "#4FC3F7", "#FFB74D", "#4FC3F7" };
            st.Current = LineQuadraticState.Mode.Quadratic;
            st.StatusText.Text = "当前：二次贝塞尔 (Q) —— 2 个端点 + 1 个控制点";
            st.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0xC7, 0x84));
            st.ToggleBtn.Content = "◀ 还原为直线 (删除中间控制点)";
        }
        else
        {
            // 二次贝塞尔 → 直线：删除中间控制点，保留首尾
            if (st.Points.Count >= 2)
            {
                var a = st.Points[0];
                var b = st.Points[st.Points.Count - 1];
                st.Points = new List<Point> { a, b };
                st.HandleColors = new List<string> { "#4FC3F7", "#4FC3F7" };
            }
            st.Current = LineQuadraticState.Mode.Line;
            st.StatusText.Text = "当前：直线 (L) —— 2 个端点";
            st.StatusText.Foreground = new SolidColorBrush(Color.FromRgb(0x4F, 0xC3, 0xF7));
            st.ToggleBtn.Content = "▶ 提升为二次贝塞尔 (在两端点中点正上方插入控制点)";
        }
        RedrawLineQuadratic(st);
    }

    /// <summary>重绘当前形态（直线 或 二次贝塞尔），重建控制点 ellipses。</summary>
    private void RedrawLineQuadratic(LineQuadraticState st)
    {
        var c = st.Surface;
        c.Children.Clear();
        st.Handles.Clear();

        var fromColor = Color.FromRgb(0x4F, 0xC3, 0xF7);

        if (st.Current == LineQuadraticState.Mode.Line)
        {
            // 直线
            c.Children.Add(MakeLine(st.Points[0], st.Points[1], fromColor, 2.5, null));
        }
        else
        {
            // 二次贝塞尔
            c.Children.Add(MakeQuadraticPath(st.Points[0], st.Points[1], st.Points[2], fromColor, 2.5));
            // 切线引导线
            c.Children.Add(MakeLine(st.Points[0], st.Points[1], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
            c.Children.Add(MakeLine(st.Points[1], st.Points[2], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
        }

        // 控制点
        for (int i = 0; i < st.Points.Count; i++)
        {
            var h = MakeHandleEllipse(st.Points[i], st.HandleColors[i], i);
            c.Children.Add(h);
            st.Handles.Add(h);
        }
    }

    /// <summary>为单个画布安装拖动事件：拖动任何控制点后实时重绘。</summary>
    private void WireLineQuadraticDrag(LineQuadraticState st)
    {
        st.Surface.PointerPressed += (_, e) =>
        {
            var pos = e.GetPosition(st.Surface);
            for (int i = 0; i < st.Handles.Count; i++)
            {
                var h = st.Handles[i];
                var left = Canvas.GetLeft(h);
                var top = Canvas.GetTop(h);
                if (new Rect(left, top, h.Width, h.Height).Contains(pos))
                {
                    st.HoverIndex = i;
                    st.LastMousePos = pos;
                    e.Pointer.Capture(st.Surface);
                    return;
                }
            }
        };

        st.Surface.PointerMoved += (_, e) =>
        {
            if (st.HoverIndex < 0) return;
            var pos = e.GetPosition(st.Surface);
            double dx = pos.X - st.LastMousePos.X;
            double dy = pos.Y - st.LastMousePos.Y;
            st.LastMousePos = pos;
            int idx = st.HoverIndex;
            double nx = Math.Clamp(st.Points[idx].X + dx, 0, st.Surface.Width);
            double ny = Math.Clamp(st.Points[idx].Y + dy, 0, st.Surface.Height);
            st.Points[idx] = new Point(nx, ny);
            RedrawLineQuadratic(st);
        };

        st.Surface.PointerReleased += (_, e) =>
        {
            st.HoverIndex = -1;
            e.Pointer.Capture(null);
        };
    }

    private enum CurveType { Line, Quadratic, Cubic, Arc, ArcToCubic }

    /// <summary>转换上下文：源曲线控制点 + 目标曲线控制点 + UI 引用。</summary>
    private class ConversionContext
    {
        public CurveType CurveType;
        public string Title = "";
        public string ConvertLabel = "转换";
        public string RevertLabel = "还原";
        public bool IsToggleable = false; // 是否支持"转换 ↔ 还原"来回切换
        public bool IsConverted = false;  // 当前是否处于"已转换"状态
        // UI 引用：在 BuildConversionPath 里赋值
        public DemoPanel SourcePanel = null!;
        public DemoPanel TargetPanel = null!;
        public Button ConvertButton = null!;
        // 缓存：还原时回到这些点
        public List<Point> OriginalPoints = new();
        public List<string> OriginalHandleColors = new();
    }

    /// <summary>演示用的小面板：Surface + 控制点 + 鼠标状态。</summary>
    private sealed class DemoPanel
    {
        public Canvas Surface = null!;
        public List<Ellipse> Handles = new();
        public List<Point> Points = new();
        public List<string> HandleColors = new();
        public int HoverIndex = -1;
        public Point LastMousePos;
        public double Width;
        public double Height;
    }

    private Border MakeConversionCard(string title, string fromColor, string toColor)
    {
        return new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Margin = new Thickness(4),
        };
    }

    private void BuildConversionPath(Border card, ConversionContext ctx, double height)
    {
        var root = new StackPanel { Spacing = 6 };
        card.Child = root;

        // ===== 标题行（曲线类型 + 公式说明）=====
        root.Children.Add(new TextBlock
        {
            Text = ctx.Title,
            FontWeight = FontWeight.Bold,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
        });

        // ===== 源曲线 Canvas（可拖动控制点）=====
        var srcCanvas = MakeDemoCanvas(height);
        var srcPanel = new DemoPanel { Surface = srcCanvas, Width = srcCanvas.Width, Height = srcCanvas.Height };
        ctx.SourcePanel = srcPanel;
        InitSourceHandles(ctx);
        root.Children.Add(srcCanvas.Parent as Border);

        // ===== 转换按钮 =====
        var convertBtn = new Button
        {
            Content = "▶ " + ctx.ConvertLabel,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Thickness(14, 4),
            Margin = new Thickness(0, 2),
            Background = new SolidColorBrush(Color.FromRgb(0x4A, 0x4A, 0x4A)),
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x80)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x4D)),
            BorderThickness = new Thickness(1),
        };
        ctx.ConvertButton = convertBtn;
        convertBtn.Click += (_, _) => ApplyConversion(ctx);
        root.Children.Add(convertBtn);

        // ===== 目标曲线 Canvas（转换后显示）=====
        var dstCanvas = MakeDemoCanvas(height);
        var dstPanel = new DemoPanel { Surface = dstCanvas, Width = dstCanvas.Width, Height = dstCanvas.Height };
        ctx.TargetPanel = dstPanel;
        root.Children.Add(dstCanvas.Parent as Border);

        // 初始绘制源 + 给目标画一个灰色的 placeholder
        RedrawSource(ctx);
        DrawTargetPlaceholder(ctx);
        // 拖动源控制点时，刷新源；如果是直线→二次这种交互模式，会同步刷新目标
        WireDemoDrag(srcPanel, () =>
        {
            RedrawSource(ctx);
            if (ctx.IsConverted) SyncTargetToSource(ctx);
        });
    }

    /// <summary>创建一个带 Border 包裹的透明 Canvas。返回 Canvas（但 Parent 是 Border）。</summary>
    private Canvas MakeDemoCanvas(double height)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x25, 0x25, 0x25)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Width = 500,
            Height = height,
            ClipToBounds = true,
        };
        var canvas = new Canvas { Background = Brushes.Transparent, Width = 500, Height = height };
        border.Child = canvas;
        return canvas;
    }

    /// <summary>初始化源曲线控制点（根据 CurveType 给 sourcePoints 填初值）。</summary>
    private void InitSourceHandles(ConversionContext ctx)
    {
        var src = ctx.SourcePanel;
        src.Handles.Clear();
        src.Points.Clear();
        src.HandleColors.Clear();

        switch (ctx.CurveType)
        {
            case CurveType.Line:
                src.Points.Add(new Point(60, height_center(ctx)));
                src.Points.Add(new Point(440, height_center(ctx)));
                src.HandleColors.Add("#4FC3F7");
                src.HandleColors.Add("#4FC3F7");
                break;
            case CurveType.Quadratic:
                src.Points.Add(new Point(60, height_center(ctx)));
                src.Points.Add(new Point(250, 40));
                src.Points.Add(new Point(440, height_center(ctx)));
                src.HandleColors.Add("#4FC3F7");
                src.HandleColors.Add("#FFB74D");
                src.HandleColors.Add("#4FC3F7");
                break;
            case CurveType.Cubic:
                src.Points.Add(new Point(60, height_center(ctx)));
                src.Points.Add(new Point(120, 40));
                src.Points.Add(new Point(380, 40));
                src.Points.Add(new Point(440, height_center(ctx)));
                src.HandleColors.Add("#4FC3F7");
                src.HandleColors.Add("#FFB74D");
                src.HandleColors.Add("#FFB74D");
                src.HandleColors.Add("#4FC3F7");
                break;
            case CurveType.Arc:
                src.Points.Add(new Point(230, 140)); // 圆心
                src.Points.Add(new Point(330, 140)); // 弧上点（决定半径）
                src.HandleColors.Add("#90A4AE");
                src.HandleColors.Add("#FF8A65");
                break;
        }

        // 缓存原始控制点（用于还原）
        ctx.OriginalPoints = new List<Point>(src.Points);
        ctx.OriginalHandleColors = new List<string>(src.HandleColors);
    }

    private static double height_center(ConversionContext ctx)
        => ctx.SourcePanel != null ? ctx.SourcePanel.Height / 2 : 150;

    /// <summary>重绘源曲线（源是 Path + Handles）。</summary>
    private void RedrawSource(ConversionContext ctx)
    {
        var src = ctx.SourcePanel;
        src.Surface.Children.Clear();
        DrawSourceShape(ctx);
        // 重新生成椭圆控制点（每次重画以保持 Z 顺序在 Path 之上）
        src.Handles.Clear();
        for (int i = 0; i < src.Points.Count; i++)
        {
            var h = MakeHandleEllipse(src.Points[i], src.HandleColors[i], i);
            src.Surface.Children.Add(h);
            src.Handles.Add(h);
        }
    }

    /// <summary>绘制源曲线形状（不含控制点）。</summary>
    private void DrawSourceShape(ConversionContext ctx)
    {
        var c = ctx.SourcePanel.Surface;
        var pts = ctx.SourcePanel.Points;
        var fromColor = Color.FromRgb(0x4F, 0xC3, 0xF7);

        switch (ctx.CurveType)
        {
            case CurveType.Line:
                c.Children.Add(MakeLine(pts[0], pts[1], fromColor, 2.5, null));
                break;
            case CurveType.Quadratic:
                c.Children.Add(MakeQuadraticPath(pts[0], pts[1], pts[2], fromColor, 2.5));
                // 切线引导线
                c.Children.Add(MakeLine(pts[0], pts[1], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
                c.Children.Add(MakeLine(pts[1], pts[2], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
                break;
            case CurveType.Cubic:
                c.Children.Add(MakeCubicPath(pts[0], pts[1], pts[2], pts[3], fromColor, 2.5));
                c.Children.Add(MakeLine(pts[0], pts[1], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
                c.Children.Add(MakeLine(pts[2], pts[3], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
                break;
            case CurveType.Arc:
                {
                    var center = pts[0];
                    var edge = pts[1];
                    double r = Math.Sqrt((edge.X - center.X) * (edge.X - center.X) + (edge.Y - center.Y) * (edge.Y - center.Y));
                    if (r < 5) r = 5;
                    c.Children.Add(MakeArcPath(center.X, center.Y, r, 0, 360, fromColor, 1.5));
                    // 标注圆心 vs 边缘点
                    c.Children.Add(MakeLine(center, edge, Color.FromArgb(0x60, 0xFF, 0x8A, 0x65), 1, new double[] { 3, 3 }));
                    break;
                }
        }
    }

    /// <summary>目标 Canvas 灰色 placeholder。 </summary>
    private void DrawTargetPlaceholder(ConversionContext ctx)
    {
        var c = ctx.TargetPanel.Surface;
        c.Children.Clear();
        c.Children.Add(new TextBlock
        {
            Text = "目标曲线：点击上方按钮应用公式",
            Foreground = new SolidColorBrush(Color.FromRgb(0x77, 0x77, 0x77)),
            FontSize = 12,
        });
        Canvas.SetLeft(c.Children[0], 100);
        Canvas.SetTop(c.Children[0], ctx.TargetPanel.Height / 2 - 10);
    }

    /// <summary>创建可拖动椭圆。Tag = 索引。</summary>
    private Ellipse MakeHandleEllipse(Point pt, string colorHex, int index)
    {
        var e = new Ellipse
        {
            Width = 16,
            Height = 16,
            Fill = new SolidColorBrush(Color.Parse(colorHex)),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1.5,
            Tag = index,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),
        };
        Canvas.SetLeft(e, pt.X - 8);
        Canvas.SetTop(e, pt.Y - 8);
        return e;
    }

    /// <summary>把 demo panel 的 Surface 接受拖动。</summary>
    private void WireDemoDrag(DemoPanel panel, Action onChanged)
    {
        panel.Surface.PointerPressed += (_, e) =>
        {
            var pos = e.GetPosition(panel.Surface);
            for (int i = 0; i < panel.Handles.Count; i++)
            {
                var h = panel.Handles[i];
                var left = Canvas.GetLeft(h);
                var top = Canvas.GetTop(h);
                if (new Rect(left, top, h.Width, h.Height).Contains(pos))
                {
                    panel.HoverIndex = i;
                    panel.LastMousePos = pos;
                    e.Pointer.Capture(panel.Surface);
                    return;
                }
            }
        };

        panel.Surface.PointerMoved += (_, e) =>
        {
            if (panel.HoverIndex < 0) return;
            var pos = e.GetPosition(panel.Surface);
            double dx = pos.X - panel.LastMousePos.X;
            double dy = pos.Y - panel.LastMousePos.Y;
            panel.LastMousePos = pos;
            int idx = panel.HoverIndex;
            double nx = Math.Clamp(panel.Points[idx].X + dx, 0, panel.Width);
            double ny = Math.Clamp(panel.Points[idx].Y + dy, 0, panel.Height);
            panel.Points[idx] = new Point(nx, ny);
            onChanged();
        };

        panel.Surface.PointerReleased += (_, e) =>
        {
            panel.HoverIndex = -1;
            e.Pointer.Capture(null);
        };
    }

    /// <summary>核心：根据 ctx.CurveType 把源控制点按公式转换为目标控制点并绘制。</summary>
    private void ApplyConversion(ConversionContext ctx)
    {
        var dst = ctx.TargetPanel;
        dst.Surface.Children.Clear();
        dst.Points.Clear();
        dst.HandleColors.Clear();

        var src = ctx.SourcePanel.Points;
        var targetColor = Color.FromRgb(0x81, 0xC7, 0x84);
        var handleColor = Color.FromRgb(0xFF, 0xB7, 0x4D);

        switch (ctx.CurveType)
        {
            case CurveType.Quadratic:
            {
                // Quadratic → Cubic：C1 = lerp(a, ctrl, 2/3)，C2 = lerp(b, ctrl, 2/3)
                var a = src[0]; var q = src[1]; var b = src[2];
                var c1 = new Point(a.X + (q.X - a.X) * 2 / 3, a.Y + (q.Y - a.Y) * 2 / 3);
                var c2 = new Point(b.X + (q.X - b.X) * 2 / 3, b.Y + (q.Y - b.Y) * 2 / 3);
                dst.Points.Add(a); dst.Points.Add(c1); dst.Points.Add(c2); dst.Points.Add(b);
                dst.HandleColors.Add("#4FC3F7"); dst.HandleColors.Add("#BA68C8");
                dst.HandleColors.Add("#BA68C8"); dst.HandleColors.Add("#4FC3F7");

                dst.Surface.Children.Add(MakeCubicPath(a, c1, c2, b, targetColor, 2.5));
                AddTargetLabel(dst, "C1", c1, "#BA68C8");
                AddTargetLabel(dst, "C2", c2, "#BA68C8");
                break;
            }
            case CurveType.Cubic:
            {
                // Cubic → 近似 Quadratic：取曲线 t=0.5 处切线交点
                var p0 = src[0]; var p1 = src[1]; var p2 = src[2]; var p3 = src[3];
                double u = 0.5;
                double midX = u * u * p0.X + 2 * u * (1 - u) * p1.X + 2 * u * (1 - u) * p2.X + (1 - u) * (1 - u) * p3.X;
                double midY = u * u * p0.Y + 2 * u * (1 - u) * p1.Y + 2 * u * (1 - u) * p2.Y + (1 - u) * (1 - u) * p3.Y;
                var mid = new Point(midX, midY);
                var ctrl = new Point(2 * mid.X - 0.5 * p0.X - 0.5 * p3.X, 2 * mid.Y - 0.5 * p0.Y - 0.5 * p3.Y);
                dst.Points.Add(p0); dst.Points.Add(ctrl); dst.Points.Add(p3);
                dst.HandleColors.Add("#4FC3F7"); dst.HandleColors.Add("#FFB74D"); dst.HandleColors.Add("#4FC3F7");

                dst.Surface.Children.Add(MakeQuadraticPath(p0, ctrl, p3, targetColor, 2.5));
                AddTargetLabel(dst, "Q控制点（切线交点）", ctrl, "#FFB74D");
                AddTargetLabel(dst, "中点", mid, "#80DEEA");
                dst.Surface.Children.Add(MakePoint(mid, Color.FromRgb(0x80, 0xDE, 0xEA), 5));
                break;
            }
            case CurveType.Arc:
            {
                // 圆弧 → Quadratic 或 Cubic，根据 ConvertLabel 区分
                var center = src[0]; var edge = src[1];
                double r = Math.Sqrt((edge.X - center.X) * (edge.X - center.X) + (edge.Y - center.Y) * (edge.Y - center.Y));
                if (r < 5) r = 5;
                double a0 = 0, a1 = Math.PI * 2;
                var p0 = new Point(center.X + r * Math.Cos(a0), center.Y + r * Math.Sin(a0));
                var p2 = new Point(center.X + r * Math.Cos(a1), center.Y + r * Math.Sin(a1));

                bool toCubic = ctx.ConvertLabel.Contains("三次");
                dst.Surface.Children.Add(MakeArcPath(center.X, center.Y, r, 0, 360,
                    Color.FromArgb(0x60, 0x4F, 0xC3, 0xF7), 1));

                if (toCubic)
                {
                    // 三等分点作 C1/C2
                    double aM1 = a0 + (a1 - a0) * (1.0 / 3.0);
                    double aM2 = a0 + (a1 - a0) * (2.0 / 3.0);
                    var c1 = new Point(center.X + r * Math.Cos(aM1), center.Y + r * Math.Sin(aM1));
                    var c2 = new Point(center.X + r * Math.Cos(aM2), center.Y + r * Math.Sin(aM2));
                    dst.Points.Add(p0); dst.Points.Add(c1); dst.Points.Add(c2); dst.Points.Add(p2);
                    dst.HandleColors.Add("#4FC3F7"); dst.HandleColors.Add("#BA68C8");
                    dst.HandleColors.Add("#BA68C8"); dst.HandleColors.Add("#4FC3F7");

                    dst.Surface.Children.Add(MakeCubicPath(p0, c1, c2, p2, targetColor, 2.5));
                    dst.Surface.Children.Add(MakePoint(c1, Color.FromRgb(0xBA, 0x68, 0xC8), 7));
                    dst.Surface.Children.Add(MakePoint(c2, Color.FromRgb(0xBA, 0x68, 0xC8), 7));
                    AddTargetLabel(dst, "C1 = t=1/3", c1, "#BA68C8");
                    AddTargetLabel(dst, "C2 = t=2/3", c2, "#BA68C8");
                }
                else
                {
                    // 中点作控制点
                    var p1 = new Point(center.X + r * Math.Cos((a0 + a1) * 0.5), center.Y + r * Math.Sin((a0 + a1) * 0.5));
                    dst.Points.Add(p0); dst.Points.Add(p1); dst.Points.Add(p2);
                    dst.HandleColors.Add("#4FC3F7"); dst.HandleColors.Add("#FFB74D"); dst.HandleColors.Add("#4FC3F7");

                    dst.Surface.Children.Add(MakeQuadraticPath(p0, p1, p2, targetColor, 2.5));
                    dst.Surface.Children.Add(MakePoint(p1, Color.FromRgb(0xFF, 0xB7, 0x4D), 7));
                    AddTargetLabel(dst, "Q控制点 = t=0.5", p1, "#FFB74D");
                }
                break;
            }
        }
        // 画目标控制点（draw last so they are on top）
        for (int i = 0; i < dst.Points.Count; i++)
        {
            var h = MakeHandleEllipse(dst.Points[i], dst.HandleColors[i], i);
            dst.Surface.Children.Add(h);
            dst.Handles.Add(h);
        }
    }

    /// <summary>把目标同步成源的镜像（用于"已转换"状态下的拖动实时反馈）。</summary>
    private void SyncTargetToSource(ConversionContext ctx)
    {
        var dst = ctx.TargetPanel;
        var src = ctx.SourcePanel;
        dst.Surface.Children.Clear();
        dst.Handles.Clear();
        dst.Points.Clear();
        dst.HandleColors.Clear();

        var pts = src.Points;
        var cols = src.HandleColors;
        var targetColor = Color.FromRgb(0x81, 0xC7, 0x84);

        // 复制控制点
        for (int i = 0; i < pts.Count; i++)
        {
            dst.Points.Add(pts[i]);
            dst.HandleColors.Add(cols[i]);
        }

        // 画曲线（绿色）
        switch (pts.Count)
        {
            case 3: // Quadratic
                dst.Surface.Children.Add(MakeQuadraticPath(pts[0], pts[1], pts[2], targetColor, 2.5));
                dst.Surface.Children.Add(MakeLine(pts[0], pts[1], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
                dst.Surface.Children.Add(MakeLine(pts[1], pts[2], Color.FromArgb(0x60, 0xFF, 0xB7, 0x4D), 1, new double[] { 3, 3 }));
                AddTargetLabel(dst, "Q", pts[1], "#81C784");
                break;
            case 4: // Cubic
                dst.Surface.Children.Add(MakeCubicPath(pts[0], pts[1], pts[2], pts[3], targetColor, 2.5));
                break;
            case 2: // Line
                dst.Surface.Children.Add(MakeLine(pts[0], pts[1], targetColor, 2.5, null));
                break;
        }

        // 画目标控制点
        for (int i = 0; i < dst.Points.Count; i++)
        {
            var h = MakeHandleEllipse(dst.Points[i], dst.HandleColors[i], i);
            dst.Surface.Children.Add(h);
            dst.Handles.Add(h);
        }
    }

    /// <summary>在目标 Canvas 上添加文字标签。</summary>
    private static void AddTargetLabel(DemoPanel panel, string text, Point pt, string colorHex)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.Parse(colorHex)),
            Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x20, 0x20, 0x20)),
            Padding = new Thickness(3, 1),
        };
        Canvas.SetLeft(label, pt.X + 9);
        Canvas.SetTop(label, pt.Y - 14);
        panel.Surface.Children.Add(label);
    }

    // ==================== 绘图工具函数 ====================

    private static Line MakeLine(Point a, Point b, Color color, double thickness, double[]? dash)
    {
        var line = new Line
        {
            StartPoint = a,
            EndPoint = b,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
        };
        if (dash != null) line.StrokeDashArray = new Avalonia.Collections.AvaloniaList<double>(dash);
        return line;
    }

    private static Path MakeQuadraticPath(Point a, Point ctrl, Point b, Color color, double thickness)
    {
        var geo = new PathGeometry();
        var fig = new PathFigure { StartPoint = a };
        fig.Segments!.Add(new QuadraticBezierSegment { Point1 = ctrl, Point2 = b });
        geo.Figures!.Add(fig);
        return new Path
        {
            Data = geo,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
            StrokeLineCap = PenLineCap.Round,
        };
    }

    private static Path MakeCubicPath(Point p0, Point p1, Point p2, Point p3, Color color, double thickness)
    {
        var geo = new PathGeometry();
        var fig = new PathFigure { StartPoint = p0 };
        fig.Segments!.Add(new BezierSegment { Point1 = p1, Point2 = p2, Point3 = p3 });
        geo.Figures!.Add(fig);
        return new Path
        {
            Data = geo,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
            StrokeLineCap = PenLineCap.Round,
        };
    }

    private static Path MakeArcPath(double cx, double cy, double r, double angStart, double angEnd, Color color, double thickness)
    {
        var geo = new PathGeometry();
        double toRad = Math.PI / 180.0;
        double a0 = angStart * toRad;
        double a1 = angEnd * toRad;
        var fig = new PathFigure { StartPoint = new Point(cx + r * Math.Cos(a0), cy + r * Math.Sin(a0)) };
        fig.Segments!.Add(new ArcSegment
        {
            Point = new Point(cx + r * Math.Cos(a1), cy + r * Math.Sin(a1)),
            Size = new Size(r, r),
            RotationAngle = 0,
            IsLargeArc = Math.Abs(angEnd - angStart) > 180,
            SweepDirection = SweepDirection.Clockwise,
        });
        geo.Figures!.Add(fig);
        return new Path
        {
            Data = geo,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = thickness,
            StrokeLineCap = PenLineCap.Round,
        };
    }

    private static Ellipse MakePoint(Point pt, Color color, double radius)
    {
        var ellipse = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Fill = new SolidColorBrush(color),
            Stroke = new SolidColorBrush(Colors.White),
            StrokeThickness = 1,
        };
        Canvas.SetLeft(ellipse, pt.X - radius);
        Canvas.SetTop(ellipse, pt.Y - radius);
        return ellipse;
    }

    private static void AddLabel(Canvas canvas, string text, Point pt, string color)
    {
        var label = new TextBlock
        {
            Text = text,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse(color)),
            Background = new SolidColorBrush(Color.FromArgb(0xAA, 0x20, 0x20, 0x20)),
            Padding = new Thickness(3, 1),
        };
        Canvas.SetLeft(label, pt.X + 10);
        Canvas.SetTop(label, pt.Y - 16);
        canvas.Children.Add(label);
    }

    /// <summary>直线面板（2 个控制点）。</summary>
    private void AddLinePanel()
    {
        var panel = CreateBasePanel("直线 (L)", "#4FC3F7", new[] { new Point(60, 220), new Point(440, 60) });
        BezierContainer.Children.Add(panel.Root);
    }

    /// <summary>通用：二次/三次贝塞尔面板（3 或 4 个控制点）。</summary>
    private void AddBezierPanel(string title, string pathColor, params Point[] pts)
    {
        var colors = pts.Length == 3
            ? new[] { "#4FC3F7", "#81C784", "#FFB74D" }
            : new[] { "#4FC3F7", "#BA68C8", "#BA68C8", "#FFB74D" };
        var panel = CreateBasePanel(title, pathColor, pts, colors);
        BezierContainer.Children.Add(panel.Root);
    }

    /// <summary>
    /// 椭圆弧面板：起止两个控制点 + 5 个 Slider 来控制 Rx / Ry / Rotation / 大弧 / 顺时针。
    /// </summary>
    private void AddArcPanel()
    {
        const double surfaceHeight = 280;

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
        };
        var root = new StackPanel { Spacing = 6 };
        border.Child = root;

        root.Children.Add(new TextBlock
        {
            Text = "椭圆弧 (A)",
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xB7, 0x4D)),
            FontSize = 16,
        });

        var canvasBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Height = surfaceHeight,
        };
        var surface = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true,
            Width = 500,
            Height = surfaceHeight,
        };
        canvasBorder.Child = surface;
        root.Children.Add(canvasBorder);

        var paramPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 4, 0, 0)
        };
        root.Children.Add(paramPanel);

        // 现在创建 panel —— 之后所有的 lambda 都能捕获它
        var panel = new BezierPanel
        {
            Root = border,
            CanvasBorder = canvasBorder,
            Surface = surface,
            Title = "椭圆弧 (A)",
            PathColor = "#FFB74D",
            Points = new List<Point> { new(80, 200), new(540, 80) }
        };
        InitializeHandlesAndPath(panel, new[] { "#FFB74D", "#4FC3F7" });

        double rx = 180, ry = 100, rot = 0;
        bool isLarge = false, isCw = false;

        void Rebuild()
        {
            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint = panel.Points[0], IsClosed = false };
            geo.Figures!.Add(fig);
            fig.Segments!.Add(new ArcSegment
            {
                Point = panel.Points[1],
                Size = new Size(Math.Max(1, rx), Math.Max(1, ry)),
                RotationAngle = rot,
                IsLargeArc = isLarge,
                SweepDirection = isCw ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
            });
            panel.PathShape.Data = geo;

            for (int i = 0; i < panel.Handles.Count; i++)
            {
                Canvas.SetLeft(panel.Handles[i], panel.Points[i].X - 8);
                Canvas.SetTop(panel.Handles[i], panel.Points[i].Y - 8);
            }
        }

        paramPanel.Children.Add(MakeSliderParam("Rx", 20, 300, rx, v => { rx = v; Rebuild(); }));
        paramPanel.Children.Add(MakeSliderParam("Ry", 20, 300, ry, v => { ry = v; Rebuild(); }));
        paramPanel.Children.Add(MakeSliderParam("旋转°", -180, 180, rot, v => { rot = v; Rebuild(); }));
        paramPanel.Children.Add(MakeCheckParam("大弧", isLarge, v => { isLarge = v; Rebuild(); }));
        paramPanel.Children.Add(MakeCheckParam("顺时针", isCw, v => { isCw = v; Rebuild(); }));

        WireDrag(panel);
        Rebuild();
    }

    /// <summary>基础面板工厂：title + 颜色 + 控制点 + 颜色数组（每个控制点的填充色）。</summary>
    private BezierPanel CreateBasePanel(string title, string pathColor, Point[] pts, string[]? colors = null)
    {
        const double surfaceHeight = 280;

        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
        };
        var root = new StackPanel { Spacing = 6 };
        border.Child = root;

        root.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse(pathColor)),
            FontSize = 16,
        });

        var canvasBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x3A, 0x3A, 0x3A)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Height = surfaceHeight,
        };
        var surface = new Canvas
        {
            Background = Brushes.Transparent,
            ClipToBounds = true,
            Width = 500,
            Height = surfaceHeight,
        };
        canvasBorder.Child = surface;
        root.Children.Add(canvasBorder);

        var panel = new BezierPanel
        {
            Root = border,
            CanvasBorder = canvasBorder,
            Surface = surface,
            Title = title,
            PathColor = pathColor,
            Points = pts.ToList(),
        };
        InitializeHandlesAndPath(panel, colors);

        return panel;
    }

    /// <summary>初始化 path 形状 + 控制点 Ellipse + 注册拖动事件。</summary>
    private void InitializeHandlesAndPath(BezierPanel panel, string[]? colors)
    {
        var path = new APath
        {
            Stroke = new SolidColorBrush(Color.Parse(panel.PathColor)),
            StrokeThickness = 3,
            StrokeLineCap = PenLineCap.Round,
        };
        panel.PathShape = path;
        panel.Surface.Children.Add(path);

        for (int i = 0; i < panel.Points.Count; i++)
        {
            var fill = colors != null && i < colors.Length
                ? Color.Parse(colors[i])
                : Colors.Yellow;

            var handle = new Ellipse
            {
                Width = 16,
                Height = 16,
                Fill = new SolidColorBrush(fill),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1.5,
                Tag = i,   // 用 Tag 记录控制点索引
            };
            Canvas.SetLeft(handle, panel.Points[i].X - 8);
            Canvas.SetTop(handle, panel.Points[i].Y - 8);
            panel.Surface.Children.Add(handle);
            panel.Handles.Add(handle);
        }

        WireDrag(panel);
        panel.RebuildPath();
    }

    /// <summary>
    /// 让 panel 上的 Surface 接受 PointerPressed / Moved / Released 实现拖动控制点。
    /// </summary>
    private void WireDrag(BezierPanel panel)
    {
        panel.Surface.PointerPressed += (_, e) =>
        {
            var pos = e.GetPosition(panel.Surface);
            for (int i = 0; i < panel.Handles.Count; i++)
            {
                var h = panel.Handles[i];
                var left = Canvas.GetLeft(h);
                var top = Canvas.GetTop(h);
                var rect = new Rect(left, top, h.Width, h.Height);
                if (rect.Contains(pos))
                {
                    panel.HoverIndex = i;
                    panel.LastMousePos = pos;
                    e.Pointer.Capture(panel.Surface);
                    return;
                }
            }
        };

        panel.Surface.PointerMoved += (_, e) =>
        {
            if (panel.HoverIndex < 0) return;
            var pos = e.GetPosition(panel.Surface);
            double dx = pos.X - panel.LastMousePos.X;
            double dy = pos.Y - panel.LastMousePos.Y;
            panel.LastMousePos = pos;
            var w = panel.Surface.Bounds.Width;
            var h = panel.Surface.Bounds.Height;
            int idx = panel.HoverIndex;
            double nx = Math.Clamp(panel.Points[idx].X + dx, 0, Math.Max(0, w));
            double ny = Math.Clamp(panel.Points[idx].Y + dy, 0, Math.Max(0, h));
            panel.Points[idx] = new Point(nx, ny);

            // ArcPanel 自己处理
            if (panel.Title == "椭圆弧 (A)")
            {
                // 重新画路径但不包括 guides
                var geo = new PathGeometry();
                var fig = new PathFigure { StartPoint = panel.Points[0], IsClosed = false };
                geo.Figures!.Add(fig);

                // 取出 Slider 当前值 —— 通过重建"等量半径"参数来维持弧形
                // 为简化：固定使用与初始同样的弧形参数
                fig.Segments!.Add(new ArcSegment
                {
                    Point = panel.Points[1],
                    Size = new Size(180, 100),
                    RotationAngle = 0,
                    IsLargeArc = false,
                    SweepDirection = SweepDirection.Clockwise,
                });
                panel.PathShape.Data = geo;
            }
            else
            {
                panel.RebuildPath();
            }

            // 更新 Ellipse 位置
            Canvas.SetLeft(panel.Handles[idx], nx - 8);
            Canvas.SetTop(panel.Handles[idx], ny - 8);
        };

        panel.Surface.PointerReleased += (_, e) =>
        {
            panel.HoverIndex = -1;
            e.Pointer.Capture(null);
        };
    }

    /// <summary>辅助：构造一个 "Rx: [slider] [value]" 小面板。</summary>
    private StackPanel MakeSliderParam(string label, double min, double max, double initValue, Action<double> onChange)
    {
        var sp = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 4, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        sp.Children.Add(new TextBlock { Text = label, Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)), Width = 50 });
        var slider = new Slider { Minimum = min, Maximum = max, Value = initValue, Width = 160 };
        var valueText = new TextBlock { Text = $"{initValue:0}", Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)), Width = 40 };
        slider.PropertyChanged += (_, ev) =>
        {
            if (ev.Property == Slider.ValueProperty)
            {
                var v = slider.Value;
                valueText.Text = $"{v:0}";
                onChange(v);
            }
        };
        sp.Children.Add(slider);
        sp.Children.Add(valueText);
        return sp;
    }

    /// <summary>辅助：构造一个 "[checkbox]" 小面板。</summary>
    private StackPanel MakeCheckParam(string label, bool initValue, Action<bool> onChange)
    {
        var sp = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal, Spacing = 4, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
        var cb = new CheckBox { Content = label, IsChecked = initValue, Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)) };
        cb.PropertyChanged += (_, ev) =>
        {
            if (ev.Property == CheckBox.IsCheckedProperty)
                onChange(cb.IsChecked == true);
        };
        sp.Children.Add(cb);
        return sp;
    }

    #endregion
}
