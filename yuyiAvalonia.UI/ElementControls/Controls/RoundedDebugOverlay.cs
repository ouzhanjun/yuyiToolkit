using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 在 Canvas（_debugOverlay）上叠加"圆角控件调试标记"。
    /// 支持分类显示不同类型的点：
    ///   - 图形点 (Vertices)：顶点位置
    ///   - 控制点 (Tangents)：弧入/出切点
    ///   - 圆心 (Corner centers)：圆角圆心（仅在 r > 0 时显示）
    /// 
    /// 颜色和大小配置：
    ///   - 图形点：蓝色实心圆
    ///   - 控制点：黄色实心圆
    ///   - 圆心：橙色实心圆
    /// 
    /// 当 Element.DrawPoint = true 时由 Rounded*Control 调用本类绘制标记，否则清空。
    /// </summary>
    internal static class RoundedDebugOverlay
    {
        // 图形点（顶点）
        private static readonly IBrush _vertexFill = new SolidColorBrush(Color.FromRgb(30, 136, 229));    // 蓝色
        private static readonly IBrush _vertexStroke = new SolidColorBrush(Color.FromRgb(21, 101, 192));
        
        // 控制点（切点）
        private static readonly IBrush _tangentFill = new SolidColorBrush(Color.FromRgb(255, 235, 59));   // 黄色
        private static readonly IBrush _tangentStroke = new SolidColorBrush(Color.FromRgb(245, 127, 23));
        
        // 圆心
        private static readonly IBrush _centerFill = new SolidColorBrush(Color.FromRgb(255, 152, 0));    // 橙色
        private static readonly IBrush _centerStroke = new SolidColorBrush(Color.FromRgb(230, 81, 0));

        // 点的大小配置
        private const double _vertexSize = 10;       // 图形点（顶点）
        private const double _tangentSize = 8;       // 控制点（切点）
        private const double _centerSize = 8;         // 圆心

        /// <summary>
        /// 显示选项标志位
        /// </summary>
        [Flags]
        public enum ShowFlags
        {
            None = 0,
            Vertex = 1,      // 图形点（顶点）
            Tangent = 2,      // 控制点（切点）
            Center = 4,       // 圆心
            ConnectVertices = 8,  // 用虚线连接图形点
            All = Vertex | Tangent | Center
        }

        // 虚线连接颜色
        private static readonly IBrush _connectStroke = new SolidColorBrush(Color.FromRgb(30, 136, 229));

        /// <summary>
        /// 用最新几何数据重绘 overlay。
        /// </summary>
        /// <param name="overlay">绘制目标 Canvas</param>
        /// <param name="actualPoints">实际顶点</param>
        /// <param name="radii">圆角半径</param>
        /// <param name="cornerCenters">圆心位置</param>
        /// <param name="arcStarts">弧起点（切点）</param>
        /// <param name="arcEnds">弧终点（切点）</param>
        /// <param name="show">是否显示（总开关）</param>
        /// <param name="showVertex">是否显示图形点（顶点）</param>
        /// <param name="showTangent">是否显示控制点（切点）</param>
        /// <param name="showCenter">是否显示圆心</param>
        /// <param name="drawVertexLines">是否用虚线连接图形点</param>
        public static void Redraw(
            Canvas overlay,
            IReadOnlyList<Point> actualPoints,
            IReadOnlyList<double> radii,
            Avalonia.Point[] cornerCenters,
            Avalonia.Point[] arcStarts,
            Avalonia.Point[] arcEnds,
            bool show,
            bool showVertex = true,
            bool showTangent = true,
            bool showCenter = true,
            bool drawVertexLines = false)
        {
            overlay.Children.Clear();

            if (!show || actualPoints == null || actualPoints.Count == 0)
                return;

            int n = actualPoints.Count;
            int rn = radii?.Count ?? 0;

            // 0) 用虚线连接图形点
            if (drawVertexLines && n >= 2)
            {
                for (int i = 0; i < n - 1; i++)
                {
                    AddDashedLine(overlay, actualPoints[i], actualPoints[i + 1]);
                }
                // 连接最后一个点到第一个点形成闭合
                AddDashedLine(overlay, actualPoints[n - 1], actualPoints[0]);
            }

            // 1) 图形点（顶点）：蓝色实心圆
            if (showVertex)
            {
                for (int i = 0; i < n; i++)
                {
                    var p = actualPoints[i];
                    overlay.Children.Add(new Ellipse
                    {
                        Width = _vertexSize,
                        Height = _vertexSize,
                        Fill = _vertexFill,
                        Stroke = _vertexStroke,
                        StrokeThickness = 1.5,
                        [Canvas.LeftProperty] = p.X - _vertexSize * 0.5,
                        [Canvas.TopProperty] = p.Y - _vertexSize * 0.5
                    });
                }
            }

            // 2) 控制点（切点）：黄色实心圆
            if (showTangent)
            {
                if (arcStarts != null)
                {
                    int len = Math.Min(arcStarts.Length, n);
                    for (int i = 0; i < len; i++)
                        AddTangentCircle(overlay, arcStarts[i]);
                }
                if (arcEnds != null)
                {
                    int len = Math.Min(arcEnds.Length, n);
                    for (int i = 0; i < len; i++)
                        AddTangentCircle(overlay, arcEnds[i]);
                }
            }

            // 3) 圆心：橙色实心圆（仅当 r > 0 时）
            if (showCenter && cornerCenters != null && radii != null)
            {
                int len = Math.Min(cornerCenters.Length, rn);
                for (int i = 0; i < len; i++)
                {
                    if (radii[i] > 0.0001)
                        AddCenterCircle(overlay, cornerCenters[i]);
                }
            }
        }

        /// <summary>
        /// 向后兼容的重载方法：使用默认显示选项（全部显示）
        /// </summary>
        public static void Redraw(
            Canvas overlay,
            IReadOnlyList<Point> actualPoints,
            IReadOnlyList<double> radii,
            Avalonia.Point[] cornerCenters,
            Avalonia.Point[] arcStarts,
            Avalonia.Point[] arcEnds,
            bool show)
        {
            Redraw(overlay, actualPoints, radii, cornerCenters, arcStarts, arcEnds, show, true, true, true);
        }

        private static void AddTangentCircle(Canvas overlay, Point p)
        {
            double size = _tangentSize;
            overlay.Children.Add(new Ellipse
            {
                Width = size,
                Height = size,
                Fill = _tangentFill,
                Stroke = _tangentStroke,
                StrokeThickness = 1.5,
                [Canvas.LeftProperty] = p.X - size * 0.5,
                [Canvas.TopProperty] = p.Y - size * 0.5
            });
        }

        private static void AddCenterCircle(Canvas overlay, Point p)
        {
            double size = _centerSize;
            overlay.Children.Add(new Ellipse
            {
                Width = size,
                Height = size,
                Fill = _centerFill,
                Stroke = _centerStroke,
                StrokeThickness = 1.5,
                [Canvas.LeftProperty] = p.X - size * 0.5,
                [Canvas.TopProperty] = p.Y - size * 0.5
            });
        }

        private static void AddDashedLine(Canvas overlay, Point start, Point end)
        {
            var line = new Line
            {
                StartPoint = start,
                EndPoint = end,
                Stroke = _connectStroke,
                StrokeThickness = 1,
                StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 3 }
            };
            overlay.Children.Add(line);
        }
    }
}
