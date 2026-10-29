using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 带网格背景的 Canvas，支持显示/隐藏网格、调整网格间距
/// </summary>
public class GridCanvas : Canvas
{
    public static readonly StyledProperty<double> GridTickIntervalProperty =
        AvaloniaProperty.Register<GridCanvas, double>(nameof(GridTickInterval), 10);

    public double GridTickInterval
    {
        get => GetValue(GridTickIntervalProperty);
        set => SetValue(GridTickIntervalProperty, value);
    }

    public static readonly StyledProperty<int> GridMajorTickIntervalProperty =
        AvaloniaProperty.Register<GridCanvas, int>(nameof(GridMajorTickInterval), 5);

    public int GridMajorTickInterval
    {
        get => GetValue(GridMajorTickIntervalProperty);
        set => SetValue(GridMajorTickIntervalProperty, value);
    }

    public static readonly StyledProperty<bool> ShowGridProperty =
        AvaloniaProperty.Register<GridCanvas, bool>(nameof(ShowGrid), false);

    public bool ShowGrid
    {
        get => GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    private static readonly SolidColorBrush MinorBrush = new(Color.Parse("#14000000"));
    private static readonly SolidColorBrush MajorBrush = new(Color.Parse("#30000000"));

    private Image _gridImage;

    static GridCanvas()
    {
        AffectsRender<GridCanvas>(GridTickIntervalProperty, GridMajorTickIntervalProperty, ShowGridProperty);
    }

    public GridCanvas()
    {
        _gridImage = new Image();
        Children.Insert(0, _gridImage);
        _gridImage.Margin = new Thickness(0);
        Canvas.SetLeft(_gridImage, 0);
        Canvas.SetTop(_gridImage, 0);
        _gridImage.IsHitTestVisible = false;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateGridImage();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ShowGridProperty
            || change.Property == GridTickIntervalProperty
            || change.Property == GridMajorTickIntervalProperty)
        {
            UpdateGridImage();
        }
    }

    private void UpdateGridImage()
    {
        if (!ShowGrid)
        {
            _gridImage.IsVisible = false;
            return;
        }

        double interval = GridTickInterval;

        double w = this.Width + interval;
        double h = this.Height + interval;

        if (interval < 1 || w <= 0 || h <= 0)
        {
            _gridImage.IsVisible = false;
            return;
        }

        int majorEvery = GridMajorTickInterval;

        var drawing = new DrawingGroup();

        int vCount = (int)Math.Ceiling(w / interval);
        for (int i = 0; i <= vCount; i++)
        {
            double x = i * interval;
            var pen = (i % majorEvery) == 0
                ? new Pen(MajorBrush, 1.0)
                : new Pen(MinorBrush, 0.5);
            drawing.Children.Add(new GeometryDrawing
            {
                Brush = pen.Brush,
                Pen = pen,
                Geometry = new LineGeometry(new Point(x, 0), new Point(x, h))
            });
        }

        int hCount = (int)Math.Ceiling(h / interval);
        for (int i = 0; i <= hCount; i++)
        {
            double y = i * interval;
            var pen = (i % majorEvery) == 0
                ? new Pen(MajorBrush, 1.0)
                : new Pen(MinorBrush, 0.5);
            drawing.Children.Add(new GeometryDrawing
            {
                Brush = pen.Brush,
                Pen = pen,
                Geometry = new LineGeometry(new Point(0, y), new Point(w, y))
            });
        }

        _gridImage.IsVisible = true;
        _gridImage.Width = w;
        _gridImage.Height = h;
        Canvas.SetLeft(_gridImage, 0);
        Canvas.SetTop(_gridImage, 0);
        _gridImage.Source = new DrawingImage { Drawing = drawing };
    }
}
