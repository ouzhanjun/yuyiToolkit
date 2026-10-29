using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using yuyiAvalonia.UI.Helpers;
using APath = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.Java.Views;

public partial class JavaView2 : UserControl
{
    private const double HandleRadius = 8;
    private static readonly Color EndPointColor = Color.FromRgb(0x6E, 0xC8, 0xFF);
    private static readonly Color QuadraticCurveColor = Color.FromRgb(0x65, 0xD1, 0xA6);
    private static readonly Color QuadraticControlColor = Color.FromRgb(0xFF, 0xB8, 0x5C);
    private static readonly Color QuadraticGuideColor = Color.FromArgb(150, 0xFF, 0xB8, 0x5C);
    private static readonly Color CubicCurveColor = Color.FromRgb(0xC7, 0x99, 0xFF);
    private static readonly Color CubicControlColor = Color.FromRgb(0xFF, 0x82, 0xB4);
    private static readonly Color CubicGuideColor = Color.FromArgb(150, 0xC7, 0x99, 0xFF);
    private static readonly SolidColorBrush QuadraticStatusBrush = new(QuadraticCurveColor);
    private static readonly SolidColorBrush CubicStatusBrush = new(CubicCurveColor);
    private static readonly SolidColorBrush LineStatusBrush = new(EndPointColor);

    public JavaView2()
    {
        InitializeComponent();
        Loaded += JavaView2_Loaded;
        SizeChanged += JavaView2_SizeChanged;
    }

    private void JavaView2_Loaded(object? sender, RoutedEventArgs e)
    {
        InitializeQuadraticPanel();
        InitializeCubicPanel();
        RedrawQuadratic();
        RedrawCubic();
    }

    private void JavaView2_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        bool qReady = !_qInitialized && QDrawingCanvas.Bounds.Width > 0 && QDrawingCanvas.Bounds.Height > 0;
        bool cReady = !_cInitialized && CDrawingCanvas.Bounds.Width > 0 && CDrawingCanvas.Bounds.Height > 0;
        if (qReady) InitializeQuadraticPanel();
        if (cReady) InitializeCubicPanel();
        if (qReady) RedrawQuadratic();
        if (cReady) RedrawCubic();
    }

    // ============================================================
    // 二次贝塞尔面板
    // ============================================================

    private readonly Point[] _qPoints = new Point[3];
    private readonly List<Ellipse> _qHandles = new();
    private bool _isQuadratic;
    private bool _qInitialized;
    private bool _updatingQOffset;
    private int _qDraggedIndex = -1;

    private void InitializeQuadraticPanel()
    {
        double width = Math.Max(QDrawingCanvas.Bounds.Width, 420);
        double height = Math.Max(QDrawingCanvas.Bounds.Height, 420);
        double y = height * 0.56;

        _qPoints[0] = new Point(width * 0.16, y);
        _qPoints[2] = new Point(width * 0.84, y);
        UpdateQuadraticControlFromOffset();
        _qInitialized = true;
    }

    private void QConvertButton_Click(object? sender, RoutedEventArgs e)
    {
        _isQuadratic = !_isQuadratic;
        if (_isQuadratic)
        {
            var converted = PathSegmentConversion.LineToQuadratic(_qPoints[0], _qPoints[2]);
            _qPoints[1] = new Point(converted.ControlPoint.X, converted.ControlPoint.Y + QOffsetSlider.Value);
        }
        RedrawQuadratic();
    }

    private void QResetButton_Click(object? sender, RoutedEventArgs e)
    {
        QOffsetSlider.Value = -110;
        _isQuadratic = false;
        InitializeQuadraticPanel();
        RedrawQuadratic();
    }

    private void QOffsetSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_qInitialized || _updatingQOffset) return;
        UpdateQuadraticControlFromOffset();
        RedrawQuadratic();
    }

    private void QStrokeSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        _qStrokeWidth = e.NewValue;
        if (_qInitialized) RedrawQuadratic();
    }

    private void QGuideCheckBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (_qInitialized) RedrawQuadratic();
    }

    private void UpdateQuadraticControlFromOffset()
    {
        var converted = PathSegmentConversion.LineToQuadratic(_qPoints[0], _qPoints[2]);
        _qPoints[1] = new Point(converted.ControlPoint.X, converted.ControlPoint.Y + QOffsetSlider.Value);
    }

    private void UpdateQuadraticOffsetFromControl()
    {
        var converted = PathSegmentConversion.LineToQuadratic(_qPoints[0], _qPoints[2]);
        double offset = Math.Clamp(_qPoints[1].Y - converted.ControlPoint.Y, QOffsetSlider.Minimum, QOffsetSlider.Maximum);
        _updatingQOffset = true;
        QOffsetSlider.Value = offset;
        _updatingQOffset = false;
    }

    private void RedrawQuadratic()
    {
        if (!_qInitialized) return;
        QDrawingCanvas.Children.Clear();
        _qHandles.Clear();
        AddGrid(QDrawingCanvas);

        ShapeDrawStream stream;
        if (_isQuadratic)
        {
            var segment = PathSegmentConversion.LineToQuadratic(_qPoints[0], _qPoints[2]);
            segment.ControlPoint = _qPoints[1];
            stream = new ShapeDrawStream { ShapeType = ShapeKind.BezierPath }
                .MoveTo(_qPoints[0])
                .QuadraticTo(segment.ControlPoint, segment.EndPoint);

            if (QGuideCheckBox.IsChecked == true)
            {
                AddGuideLine(QDrawingCanvas, _qPoints[0], _qPoints[1], QuadraticGuideColor);
                AddGuideLine(QDrawingCanvas, _qPoints[1], _qPoints[2], QuadraticGuideColor);
            }

            AddCurve(QDrawingCanvas, stream.ToAvaloniaGeometry(), QuadraticCurveColor, _qStrokeWidth);
            AddHandle(QDrawingCanvas, _qHandles, _qPoints[0], 0, EndPointColor);
            AddHandle(QDrawingCanvas, _qHandles, _qPoints[1], 1, QuadraticControlColor);
            AddHandle(QDrawingCanvas, _qHandles, _qPoints[2], 2, EndPointColor);

            QConvertButton.Content = "还原为直线";
            QStatusText.Text = "当前：二次贝塞尔曲线（Q）";
            QStatusText.Foreground = QuadraticStatusBrush;
        }
        else
        {
            stream = new ShapeDrawStream { ShapeType = ShapeKind.Line }
                .MoveTo(_qPoints[0])
                .LineTo(_qPoints[2]);

            AddCurve(QDrawingCanvas, stream.ToAvaloniaGeometry(), EndPointColor, _qStrokeWidth);
            AddHandle(QDrawingCanvas, _qHandles, _qPoints[0], 0, EndPointColor);
            AddHandle(QDrawingCanvas, _qHandles, _qPoints[2], 2, EndPointColor);

            QConvertButton.Content = "转换为二次贝塞尔曲线";
            QStatusText.Text = "当前：直线段（L）";
            QStatusText.Foreground = LineStatusBrush;
        }

        QPathDataText.Text = stream.ToSvgPath();
    }

    private void QDrawingCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        HandlePointerPressed(e, QDrawingCanvas, _qHandles, _qPoints, ref _qDraggedIndex);
    }

    private void QDrawingCanvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_qDraggedIndex < 0) return;
        UpdatePointFromDrag(e, QDrawingCanvas, _qPoints, _qDraggedIndex, QuadraticStatusBrush, updateOffset: true,
            updateSlider: (i) => i == 1,
            updateOffsetFromPoint: () => UpdateQuadraticOffsetFromControl(),
            updateControlFromOffset: () => UpdateQuadraticControlFromOffset());
        RedrawQuadratic();
        e.Handled = true;
    }

    private void QDrawingCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ReleasePointer(e, ref _qDraggedIndex);
    }

    // ============================================================
    // 三次贝塞尔面板
    // ============================================================

    private readonly Point[] _cPoints = new Point[4];
    private readonly List<Ellipse> _cHandles = new();
    private bool _isCubic;
    private bool _cInitialized;
    private bool _updatingC1Offset;
    private bool _updatingC2Offset;
    private int _cDraggedIndex = -1;
    private double _qStrokeWidth = 4;
    private double _cStrokeWidth = 4;

    private void InitializeCubicPanel()
    {
        double width = Math.Max(CDrawingCanvas.Bounds.Width, 420);
        double height = Math.Max(CDrawingCanvas.Bounds.Height, 420);
        double y = height * 0.56;

        _cPoints[0] = new Point(width * 0.16, y);
        _cPoints[3] = new Point(width * 0.84, y);
        UpdateCubicControlsFromOffsets();
        _cInitialized = true;
    }

    private void CConvertButton_Click(object? sender, RoutedEventArgs e)
    {
        _isCubic = !_isCubic;
        if (_isCubic)
        {
            var converted = PathSegmentConversion.LineToCubic(_cPoints[0], _cPoints[3]);
            _cPoints[1] = new Point(converted.Control1.X, converted.Control1.Y + C1OffsetSlider.Value);
            _cPoints[2] = new Point(converted.Control2.X, converted.Control2.Y + C2OffsetSlider.Value);
        }
        RedrawCubic();
    }

    private void CResetButton_Click(object? sender, RoutedEventArgs e)
    {
        C1OffsetSlider.Value = -130;
        C2OffsetSlider.Value = 130;
        _isCubic = false;
        InitializeCubicPanel();
        RedrawCubic();
    }

    private void C1OffsetSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_cInitialized || _updatingC1Offset) return;
        UpdateCubicControlsFromOffsets();
        RedrawCubic();
    }

    private void C2OffsetSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_cInitialized || _updatingC2Offset) return;
        UpdateCubicControlsFromOffsets();
        RedrawCubic();
    }

    private void CStrokeSlider_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        _cStrokeWidth = e.NewValue;
        if (_cInitialized) RedrawCubic();
    }

    private void CGuideCheckBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (_cInitialized) RedrawCubic();
    }

    private void UpdateCubicControlsFromOffsets()
    {
        var converted = PathSegmentConversion.LineToCubic(_cPoints[0], _cPoints[3]);
        _cPoints[1] = new Point(converted.Control1.X, converted.Control1.Y + C1OffsetSlider.Value);
        _cPoints[2] = new Point(converted.Control2.X, converted.Control2.Y + C2OffsetSlider.Value);
    }

    private void UpdateCubicOffsetsFromControls()
    {
        var converted = PathSegmentConversion.LineToCubic(_cPoints[0], _cPoints[3]);
        double c1 = Math.Clamp(_cPoints[1].Y - converted.Control1.Y, C1OffsetSlider.Minimum, C1OffsetSlider.Maximum);
        double c2 = Math.Clamp(_cPoints[2].Y - converted.Control2.Y, C2OffsetSlider.Minimum, C2OffsetSlider.Maximum);
        _updatingC1Offset = true;
        _updatingC2Offset = true;
        C1OffsetSlider.Value = c1;
        C2OffsetSlider.Value = c2;
        _updatingC1Offset = false;
        _updatingC2Offset = false;
    }

    private void RedrawCubic()
    {
        if (!_cInitialized) return;
        CDrawingCanvas.Children.Clear();
        _cHandles.Clear();
        AddGrid(CDrawingCanvas);

        ShapeDrawStream stream;
        if (_isCubic)
        {
            var segment = PathSegmentConversion.LineToCubic(_cPoints[0], _cPoints[3]);
            segment.Control1 = _cPoints[1];
            segment.Control2 = _cPoints[2];
            stream = new ShapeDrawStream { ShapeType = ShapeKind.BezierPath }
                .MoveTo(_cPoints[0])
                .CubicTo(segment.Control1, segment.Control2, segment.EndPoint);

            if (CGuideCheckBox.IsChecked == true)
            {
                AddGuideLine(CDrawingCanvas, _cPoints[0], _cPoints[1], CubicGuideColor);
                AddGuideLine(CDrawingCanvas, _cPoints[2], _cPoints[3], CubicGuideColor);
            }

            AddCurve(CDrawingCanvas, stream.ToAvaloniaGeometry(), CubicCurveColor, _cStrokeWidth);
            AddHandle(CDrawingCanvas, _cHandles, _cPoints[0], 0, EndPointColor);
            AddHandle(CDrawingCanvas, _cHandles, _cPoints[1], 1, CubicControlColor);
            AddHandle(CDrawingCanvas, _cHandles, _cPoints[2], 2, CubicControlColor);
            AddHandle(CDrawingCanvas, _cHandles, _cPoints[3], 3, EndPointColor);

            CConvertButton.Content = "还原为直线";
            CStatusText.Text = "当前：三次贝塞尔曲线（C）";
            CStatusText.Foreground = CubicStatusBrush;
        }
        else
        {
            stream = new ShapeDrawStream { ShapeType = ShapeKind.Line }
                .MoveTo(_cPoints[0])
                .LineTo(_cPoints[3]);

            AddCurve(CDrawingCanvas, stream.ToAvaloniaGeometry(), EndPointColor, _cStrokeWidth);
            AddHandle(CDrawingCanvas, _cHandles, _cPoints[0], 0, EndPointColor);
            AddHandle(CDrawingCanvas, _cHandles, _cPoints[3], 3, EndPointColor);

            CConvertButton.Content = "转换为三次贝塞尔曲线";
            CStatusText.Text = "当前：直线段（L）";
            CStatusText.Foreground = LineStatusBrush;
        }

        CPathDataText.Text = stream.ToSvgPath();
    }

    private void CDrawingCanvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        HandlePointerPressed(e, CDrawingCanvas, _cHandles, _cPoints, ref _cDraggedIndex);
    }

    private void CDrawingCanvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (_cDraggedIndex < 0) return;
        UpdatePointFromDrag(e, CDrawingCanvas, _cPoints, _cDraggedIndex, CubicStatusBrush, updateOffset: false,
            updateSlider: (i) => i == 1 || i == 2,
            updateOffsetFromPoint: () => UpdateCubicOffsetsFromControls(),
            updateControlFromOffset: () => UpdateCubicControlsFromOffsets());
        RedrawCubic();
        e.Handled = true;
    }

    private void CDrawingCanvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ReleasePointer(e, ref _cDraggedIndex);
    }

    // ============================================================
    // 共享绘制/交互辅助
    // ============================================================

    private void AddGrid(Canvas canvas)
    {
        double width = Math.Max(canvas.Bounds.Width, 420);
        double height = Math.Max(canvas.Bounds.Height, 420);
        var brush = new SolidColorBrush(Color.FromArgb(26, 110, 200, 255));
        for (double x = 40; x < width; x += 40)
        {
            canvas.Children.Add(new Line
            {
                StartPoint = new Point(x, 0),
                EndPoint = new Point(x, height),
                Stroke = brush,
                StrokeThickness = 1
            });
        }
        for (double y = 40; y < height; y += 40)
        {
            canvas.Children.Add(new Line
            {
                StartPoint = new Point(0, y),
                EndPoint = new Point(width, y),
                Stroke = brush,
                StrokeThickness = 1
            });
        }
    }

    private void AddCurve(Canvas canvas, Geometry geometry, Color color, double strokeWidth)
    {
        canvas.Children.Add(new APath
        {
            Data = geometry,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = strokeWidth,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round
        });
    }

    private void AddGuideLine(Canvas canvas, Point start, Point end, Color color)
    {
        canvas.Children.Add(new Line
        {
            StartPoint = start,
            EndPoint = end,
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 1.2,
            StrokeDashArray = new AvaloniaList<double> { 5, 4 }
        });
    }

    private void AddHandle(Canvas canvas, List<Ellipse> handles, Point point, int index, Color color)
    {
        var handle = new Ellipse
        {
            Width = HandleRadius * 2,
            Height = HandleRadius * 2,
            Fill = new SolidColorBrush(color),
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Tag = index
        };
        Canvas.SetLeft(handle, point.X - HandleRadius);
        Canvas.SetTop(handle, point.Y - HandleRadius);
        canvas.Children.Add(handle);
        handles.Add(handle);
    }

    private static void HandlePointerPressed(PointerPressedEventArgs e, Canvas canvas, List<Ellipse> handles, Point[] points, ref int draggedIndex)
    {
        if (!e.GetCurrentPoint(canvas).Properties.IsLeftButtonPressed) return;
        var position = e.GetPosition(canvas);
        for (int i = handles.Count - 1; i >= 0; i--)
        {
            if (handles[i].Tag is not int index) continue;
            var point = points[index];
            double dx = position.X - point.X;
            double dy = position.Y - point.Y;
            if (dx * dx + dy * dy <= 18 * 18)
            {
                draggedIndex = index;
                e.Pointer.Capture(canvas);
                e.Handled = true;
                return;
            }
        }
    }

    private static void UpdatePointFromDrag(
        PointerEventArgs e,
        Canvas canvas,
        Point[] points,
        int draggedIndex,
        SolidColorBrush statusBrush,
        bool updateOffset,
        Func<int, bool> updateSlider,
        Action updateOffsetFromPoint,
        Action updateControlFromOffset)
    {
        var position = e.GetPosition(canvas);
        double width = Math.Max(canvas.Bounds.Width, 1);
        double height = Math.Max(canvas.Bounds.Height, 1);
        points[draggedIndex] = new Point(
            Math.Clamp(position.X, HandleRadius, width - HandleRadius),
            Math.Clamp(position.Y, HandleRadius, height - HandleRadius));

        if (updateSlider(draggedIndex))
            updateOffsetFromPoint();
        else if (updateOffset)
            updateControlFromOffset();
    }

    private static void ReleasePointer(PointerReleasedEventArgs e, ref int draggedIndex)
    {
        if (draggedIndex < 0) return;
        draggedIndex = -1;
        e.Pointer.Capture(null);
        e.Handled = true;
    }
}