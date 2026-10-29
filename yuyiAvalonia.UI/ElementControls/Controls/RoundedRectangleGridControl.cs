using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 圆角矩形 + Grid 容器控件。
    /// 继承自 <see cref="RoundedRactangleControlBase"/>，复用 _innerBorder 布局更新、CornerRadius 同步等基类能力。
    /// 在此基础上叠加 Grid 容器：内部以一个 Border（按圆角裁剪，由基类持有）包裹 _contentPanel（承载 _grid），
    /// 子元素通过 <see cref="IElementContainer"/> 接口添加，按 Row/Column 落入对应单元格。
    /// 与 <see cref="RoundedRectangleStackPanelControl"/>、<see cref="RoundedRectangleCanvasControl"/> 平级：
    /// StackPanel 按 Orientation 排列，Canvas 按 Left/Top 自由定位，Grid 按行列规则布局。
    /// </summary>
    public class RoundedRectangleGridControl : RoundedRactangleControlBase
    {
        // 内部 Grid：按行/列定义布局子元素。
        private readonly Grid _grid = new()
        {
            Background = Brushes.Transparent,
            RowSpacing = 0,
            ColumnSpacing = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        /// <summary>
        /// 内部 Panel 容器（子元素将被添加到其中）。
        /// 用于 FlowContainerManager / 其他依赖容器面板位置的代码。
        /// </summary>
        public override Panel? Container => _grid;

        public RoundedRectangleGridControl(RoundedRectangleGridElement element)
            : base(element)
        {
            // 初始化 Grid 行列 / 间距
            _grid.RowSpacing = element.RowSpacing;
            _grid.ColumnSpacing = element.ColumnSpacing;

            // 把 _grid 放到基类 _contentPanel 中（基类已创建 _contentPanel 并置入 _innerBorder）
            _contentPanel.Children.Add(_grid);

            // 订阅元素属性变化（Padding 由基类自动处理）
            element.PropertyChanged += OnElementPropertyChanged;

            // 初始同步网格布局
            UpdateGridLayout(element);
        }

        public override void UpdateShape(CanvasElement? shapeElement)
        {
            base.UpdateShape(shapeElement);
            if (shapeElement is RoundedRectangleGridElement element)
            {
                UpdateGridLayout(element);
            }
        }

        private void OnElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not RoundedRectangleGridElement element)
                return;

            switch (e.PropertyName)
            {
                case nameof(RoundedRectangleGridElement.Rows):
                    UpdateGridLayout(element);
                    break;
                case nameof(RoundedRectangleGridElement.Columns):
                    UpdateGridLayout(element);
                    break;
                case nameof(RoundedRectangleGridElement.RowSpacing):
                    UpdateGridLayout(element);
                    break;
                case nameof(RoundedRectangleGridElement.ColumnSpacing):
                    UpdateGridLayout(element);
                    break;
                case nameof(RoundedRectangleGridElement.RowDefinitions):
                    UpdateRowDefinitions(element);
                    break;
                case nameof(RoundedRectangleGridElement.ColumnDefinitions):
                    UpdateColumnDefinitions(element);
                    break;
            }
        }

        #region 网格布局

        /// <summary>
        /// 完整更新网格布局（行数 / 列数 / 间距）。
        /// </summary>
        private void UpdateGridLayout(RoundedRectangleGridElement element)
        {
            UpdateGridLayout(element.Rows, element.Columns, element.RowSpacing, element.ColumnSpacing);
        }

        private void UpdateGridLayout(int rows, int columns, double rowSpacing, double columnSpacing)
        {
            rows = Math.Max(1, rows);
            columns = Math.Max(1, columns);

            _grid.RowSpacing = rowSpacing;
            _grid.ColumnSpacing = columnSpacing;

            // 行：扩列
            if (rows > _grid.RowDefinitions.Count)
            {
                for (int i = _grid.RowDefinitions.Count; i < rows; i++)
                {
                    _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                }
            }
            else if (rows < _grid.RowDefinitions.Count)
            {
                while (_grid.RowDefinitions.Count > rows)
                {
                    var controls = _grid.Children.Where(c => c.GetValue(Grid.RowProperty) == _grid.RowDefinitions.Count - 1).ToArray();
                    if (controls.Length > 0)
                    {
                        foreach (var child in controls)
                            _grid.Children.Remove(child);
                    }
                    _grid.RowDefinitions.RemoveAt(_grid.RowDefinitions.Count - 1);
                }
            }

            // 列：扩列
            if (columns > _grid.ColumnDefinitions.Count)
            {
                for (int i = _grid.ColumnDefinitions.Count; i < columns; i++)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                }
            }
            else if (columns < _grid.ColumnDefinitions.Count)
            {
                while (_grid.ColumnDefinitions.Count > columns)
                {
                    var controls = _grid.Children.Where(c => c.GetValue(Grid.ColumnProperty) == _grid.ColumnDefinitions.Count - 1).ToArray();
                    if (controls.Length > 0)
                    {
                        foreach (var child in controls)
                            _grid.Children.Remove(child);
                    }
                    _grid.ColumnDefinitions.RemoveAt(_grid.ColumnDefinitions.Count - 1);
                }
            }

            // 同步回 Element
            if (_currentElement is RoundedRectangleGridElement gridElement)
            {
                gridElement.Rows = rows;
                gridElement.Columns = columns;
                gridElement.RowDefinitions = GetRowDefinitionsString();
                gridElement.ColumnDefinitions = GetColumnDefinitionsString();
            }

            // 重新绘制网格线覆盖层
            if (WorkspaceOverlayBorder?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 更新行定义（当行定义字符串变化时调用）。
        /// </summary>
        public void UpdateRowDefinitions(RoundedRectangleGridElement element)
        {
            _grid.RowDefinitions.Clear();
            ParseRowDefinitions(element);

            element.RowDefinitions = this.GetRowDefinitionsString();

            _grid.InvalidateMeasure();
            _grid.InvalidateArrange();

            if (WorkspaceOverlayBorder?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 更新列定义（当列定义字符串变化时调用）。
        /// </summary>
        public void UpdateColumnDefinitions(RoundedRectangleGridElement element)
        {
            _grid.ColumnDefinitions.Clear();
            ParseColumnDefinitions(element);

            element.ColumnDefinitions = this.GetColumnDefinitionsString();

            _grid.InvalidateMeasure();
            _grid.InvalidateArrange();

            if (WorkspaceOverlayBorder?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        private void ParseRowDefinitions(RoundedRectangleGridElement element)
        {
            string rowDef = element.RowDefinitions;

            if (string.IsNullOrWhiteSpace(rowDef))
            {
                for (int i = 0; i < element.Rows; i++)
                {
                    _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                }
                return;
            }

            var parts = rowDef.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                _grid.RowDefinitions.Add(ParseRowDefinition(part));
            }
            while (_grid.RowDefinitions.Count < element.Rows)
            {
                _grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            }
        }

        private void ParseColumnDefinitions(RoundedRectangleGridElement element)
        {
            string colDef = element.ColumnDefinitions;

            if (string.IsNullOrWhiteSpace(colDef))
            {
                for (int i = 0; i < element.Columns; i++)
                {
                    _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                }
                return;
            }

            var parts = colDef.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                _grid.ColumnDefinitions.Add(ParseColumnDefinition(part));
            }
            while (_grid.ColumnDefinitions.Count < element.Columns)
            {
                _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            }
        }

        private static RowDefinition ParseRowDefinition(string definition)
        {
            definition = definition.Trim();

            if (string.IsNullOrEmpty(definition) || definition == "*")
                return new RowDefinition(1, GridUnitType.Star);

            if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return new RowDefinition(GridLength.Auto);

            if (definition.EndsWith("*"))
            {
                var numPart = definition.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                    return new RowDefinition(1, GridUnitType.Star);
                if (double.TryParse(numPart, out double value))
                    return new RowDefinition(value, GridUnitType.Star);
                return new RowDefinition(1, GridUnitType.Star);
            }

            if (double.TryParse(definition, out double pixelValue))
                return new RowDefinition(pixelValue, GridUnitType.Pixel);

            return new RowDefinition(1, GridUnitType.Star);
        }

        private static ColumnDefinition ParseColumnDefinition(string definition)
        {
            definition = definition.Trim();

            if (string.IsNullOrEmpty(definition) || definition == "*")
                return new ColumnDefinition(1, GridUnitType.Star);

            if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return new ColumnDefinition(GridLength.Auto);

            if (definition.EndsWith("*"))
            {
                var numPart = definition.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                    return new ColumnDefinition(1, GridUnitType.Star);
                if (double.TryParse(numPart, out double value))
                    return new ColumnDefinition(value, GridUnitType.Star);
                return new ColumnDefinition(1, GridUnitType.Star);
            }

            if (double.TryParse(definition, out double pixelValue))
                return new ColumnDefinition(pixelValue, GridUnitType.Pixel);

            return new ColumnDefinition(1, GridUnitType.Star);
        }

        /// <summary>
        /// 解析字符串为 GridLength。
        /// </summary>
        private GridLength ParseGridLength(string definition)
        {
            definition = definition.Trim();

            if (string.IsNullOrEmpty(definition) || definition == "*")
                return new GridLength(1, GridUnitType.Star);

            if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
                return GridLength.Auto;

            if (definition.EndsWith("*"))
            {
                var numPart = definition.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                    return new GridLength(1, GridUnitType.Star);
                if (double.TryParse(numPart, out double value))
                    return new GridLength(value, GridUnitType.Star);
                return new GridLength(1, GridUnitType.Star);
            }

            if (double.TryParse(definition, out double pixelValue))
                return new GridLength(pixelValue, GridUnitType.Pixel);

            return new GridLength(1, GridUnitType.Star);
        }

        /// <summary>
        /// 应用行定义字符串（直接修改现有定义，不清空重建）。
        /// </summary>
        public void ApplyRowDefinitions(string definition)
        {
            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < parts.Length && i < _grid.RowDefinitions.Count; i++)
            {
                var rowDef = _grid.RowDefinitions[i];
                var newLength = ParseGridLength(parts[i]);
                rowDef.SetCurrentValue(RowDefinition.HeightProperty, newLength);
            }

            _grid.InvalidateMeasure();
            _grid.InvalidateArrange();

            if (WorkspaceOverlayBorder?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 应用列定义字符串（直接修改现有定义，不清空重建）。
        /// </summary>
        public void ApplyColumnDefinitions(string definition)
        {
            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            for (int i = 0; i < parts.Length && i < _grid.ColumnDefinitions.Count; i++)
            {
                var colDef = _grid.ColumnDefinitions[i];
                var newLength = ParseGridLength(parts[i]);
                colDef.SetCurrentValue(ColumnDefinition.WidthProperty, newLength);
            }

            _grid.InvalidateMeasure();
            _grid.InvalidateArrange();

            if (WorkspaceOverlayBorder?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 获取当前行的定义字符串（从现有的 Grid.RowDefinitions 读取）。
        /// </summary>
        public string GetRowDefinitionsString()
        {
            var parts = new List<string>();
            foreach (var row in _grid.RowDefinitions)
            {
                parts.Add(GridLengthToString(row.Height));
            }
            return string.Join(",", parts);
        }

        /// <summary>
        /// 获取当前列的定义字符串。
        /// </summary>
        public string GetColumnDefinitionsString()
        {
            var parts = new List<string>();
            foreach (var col in _grid.ColumnDefinitions)
            {
                parts.Add(GridLengthToString(col.Width));
            }
            return string.Join(",", parts);
        }

        private static string GridLengthToString(GridLength length)
        {
            if (length.IsAuto) return "Auto";
            if (length.IsStar)
                return length.Value == 1 ? "*" : $"{length.Value}*";
            return length.Value.ToString("0.##");
        }

        #endregion

        #region 覆盖层

        protected override void DrawWorkspaceOverlayContent()
        {
            DrawGridLines();
        }

        /// <summary>
        /// 绘制网格线（基于 GridLength 计算精确尺寸）。
        /// </summary>
        private void DrawGridLines()
        {
            if (WorkspaceOverlayBorder == null || _currentElement is not RoundedRectangleGridElement gridElement) return;

            var rows = gridElement.Rows;
            var cols = gridElement.Columns;
            var rowSpacing = gridElement.RowSpacing;
            var colSpacing = gridElement.ColumnSpacing;

            double width = _grid.Bounds.Width;
            double height = _grid.Bounds.Height;

            if (width <= 0 || height <= 0) return;

            var canvas = new Canvas
            {
                Width = width,
                Height = height,
            };

            var lineBrush = GridLineBrush;
            var fillBrush = CellFillBrush;
            var lineThickness = GridLineThickness;
            var cornerRadius = CellCornerRadius;

            double availableWidth = width - colSpacing * (cols - 1);
            double availableHeight = height - rowSpacing * (rows - 1);

            if (availableWidth <= 0 || availableHeight <= 0) return;

            var rowLengths = ParseGridLengths(gridElement.RowDefinitions, rows);
            var colLengths = ParseGridLengths(gridElement.ColumnDefinitions, cols);

            double[] rowHeights = CalculateSizes(rowLengths, availableHeight);
            double[] colWidths = CalculateSizes(colLengths, availableWidth);

            double y = 0;
            for (int row = 0; row < rows; row++)
            {
                double cellHeight = rowHeights[row];
                if (cellHeight <= 0) continue;

                double x = 0;
                for (int col = 0; col < cols; col++)
                {
                    double cellWidth = colWidths[col];
                    if (cellWidth <= 0) continue;

                    var cellBorder = new Border
                    {
                        Width = cellWidth,
                        Height = cellHeight,
                        Background = fillBrush,
                        BorderBrush = lineBrush,
                        BorderThickness = new Thickness(lineThickness),
                        CornerRadius = cornerRadius,
                    };

                    Canvas.SetLeft(cellBorder, x);
                    Canvas.SetTop(cellBorder, y);
                    canvas.Children.Add(cellBorder);

                    x += cellWidth + colSpacing;
                }
                y += cellHeight + rowSpacing;
            }

            WorkspaceOverlayBorder!.Child = canvas;
        }

        /// <summary>
        /// 根据 GridLength 数组计算实际尺寸。
        /// </summary>
        private static double[] CalculateSizes(GridLength[] lengths, double availableSize)
        {
            var result = new double[lengths.Length];

            double starTotalValue = 0;
            double pixelTotal = 0;
            int autoCount = 0;
            var starIndices = new List<int>();

            for (int i = 0; i < lengths.Length; i++)
            {
                var length = lengths[i];
                switch (length.GridUnitType)
                {
                    case GridUnitType.Star:
                        starTotalValue += length.Value;
                        starIndices.Add(i);
                        break;
                    case GridUnitType.Pixel:
                        pixelTotal += length.Value;
                        break;
                    case GridUnitType.Auto:
                        autoCount++;
                        break;
                }
            }

            double starAvailableSize = availableSize - pixelTotal;

            double autoSize = autoCount > 0 ? Math.Max(0, starAvailableSize / (starIndices.Count + autoCount)) : 0;
            double starUnitSize = starIndices.Count > 0 && starTotalValue > 0
                ? Math.Max(0, starAvailableSize - autoSize * autoCount) / starTotalValue
                : 0;

            for (int i = 0; i < lengths.Length; i++)
            {
                var length = lengths[i];
                switch (length.GridUnitType)
                {
                    case GridUnitType.Star:
                        result[i] = starUnitSize * length.Value;
                        break;
                    case GridUnitType.Pixel:
                        result[i] = length.Value;
                        break;
                    case GridUnitType.Auto:
                        result[i] = autoSize;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 解析行/列定义字符串为 GridLength 数组。
        /// </summary>
        private GridLength[] ParseGridLengths(string definition, int expectedCount)
        {
            var result = new List<GridLength>();

            if (string.IsNullOrWhiteSpace(definition))
            {
                for (int i = 0; i < expectedCount; i++)
                {
                    result.Add(new GridLength(1, GridUnitType.Star));
                }
                return result.ToArray();
            }

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                result.Add(ParseGridLength(part));
            }

            while (result.Count < expectedCount)
            {
                result.Add(new GridLength(1, GridUnitType.Star));
            }

            return result.ToArray();
        }

        #endregion

        #region 命中检测

        /// <summary>
        /// 获取指定位置的单元格索引（基于 GridLength 计算）。
        /// position 是相对于 Grid 控件本身的坐标（已考虑 Padding）。
        /// </summary>
        public (int Row, int Column)? GetCellAtPosition(Point position)
        {
            if (_currentElement is not RoundedRectangleGridElement gridElement) return null;

            var rows = gridElement.Rows;
            var cols = gridElement.Columns;
            var rowSpacing = gridElement.RowSpacing;
            var colSpacing = gridElement.ColumnSpacing;

            double paddingLeft = gridElement.PaddingLeft;
            double paddingTop = gridElement.PaddingTop;
            double paddingRight = gridElement.PaddingRight;
            double paddingBottom = gridElement.PaddingBottom;

            double width = _grid.Bounds.Width;
            double height = _grid.Bounds.Height;

            if (width <= 0 || height <= 0) return null;

            double contentWidth = width - paddingLeft - paddingRight;
            double contentHeight = height - paddingTop - paddingBottom;

            if (contentWidth <= 0 || contentHeight <= 0) return null;

            double availableWidth = contentWidth - colSpacing * (cols - 1);
            double availableHeight = contentHeight - rowSpacing * (rows - 1);

            if (availableWidth <= 0 || availableHeight <= 0) return null;

            var rowLengths = ParseGridLengths(gridElement.RowDefinitions, rows);
            var colLengths = ParseGridLengths(gridElement.ColumnDefinitions, cols);

            double[] rowHeights = CalculateSizes(rowLengths, availableHeight);
            double[] colWidths = CalculateSizes(colLengths, availableWidth);

            double localX = position.X - paddingLeft;
            double localY = position.Y - paddingTop;

            if (localX < 0 || localY < 0) return null;

            int targetRow = 0;
            double y = 0;
            for (int row = 0; row < rows; row++)
            {
                double cellHeight = rowHeights[row];
                if (localY >= y && localY < y + cellHeight)
                {
                    targetRow = row;
                    break;
                }
                y += cellHeight + rowSpacing;
            }

            int targetCol = 0;
            double x = 0;
            for (int col = 0; col < cols; col++)
            {
                double cellWidth = colWidths[col];
                if (localX >= x && localX < x + cellWidth)
                {
                    targetCol = col;
                    break;
                }
                x += cellWidth + colSpacing;
            }

            if (targetRow < 0) targetRow = 0;
            if (targetRow >= rows) targetRow = rows - 1;
            if (targetCol < 0) targetCol = 0;
            if (targetCol >= cols) targetCol = cols - 1;

            return (targetRow, targetCol);
        }

        /// <summary>
        /// 获取相对于 Grid 内部坐标（考虑 _innerBorder.Padding / _contentPanel 偏移）。
        /// </summary>
        public Point GetLocalPosition(Point canvasPosition)
        {
            var gridPosition = this.Bounds.Position;
            double innerX = _contentPanel.Bounds.X;
            double innerY = _contentPanel.Bounds.Y;

            double localX = canvasPosition.X - gridPosition.X - innerX;
            double localY = canvasPosition.Y - gridPosition.Y - innerY;

            return new Point(localX, localY);
        }

        #endregion

        #region IElementContainer

        public void AddChildToCell(Control? child, int row, int column)
        {
            if (child == null || this == child)
                return;

            if (child.Tag is CanvasElement element && _currentElement is RoundedRectangleGridElement parentElement)
            {
                if (!_grid.Children.Contains(child))
                {
                    ((Panel?)child.Parent)?.Children.Remove(child);
                    _grid.Children.Add(child);

                    Grid.SetRow(child, row);
                    Grid.SetColumn(child, column);

                    element.Parent = parentElement;
                    if (!parentElement.Children.Contains(element))
                    {
                        parentElement.Children.Add(element);
                    }

                    element.GridRow = row;
                    element.GridColumn = column;

                    // Grid 子元素：水平和垂直都 Stretch 填满单元格
                    element.HorizontalAlignment = HorizontalAlignment.Stretch;
                    element.VerticalAlignment = VerticalAlignment.Stretch;

                    if (child is IElementControlBase elementControl)
                    {
                        elementControl.UpdateHorizontalAlignment(element);
                        elementControl.UpdateVerticalAlignment(element);
                    }
                }
            }
        }

        public override void RemoveChild(Control? childControl)
        {
            if (childControl == null)
                return;

            if (_grid.Children.Contains(childControl))
            {
                _grid.Children.Remove(childControl);

                if (this.Tag is CanvasElement element)
                {
                    if (childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = null;

                        if (element.Children.Contains(childElement))
                        {
                            element.Children.Remove(childElement);
                        }
                    }
                }
            }
        }

        public override void Insert(int insertedIndex, Control? childControl)
        {
            if (childControl == null || insertedIndex < 0)
                return;

            // 超出 Grid 子元素数量上限则截断
            if (insertedIndex >= _grid.Children.Count)
            {
                Add(childControl);
                return;
            }

            // 找到目标位置上的当前子元素，作为参考单元格
            var existing = _grid.Children[insertedIndex];
            int targetRow = Grid.GetRow(existing);
            int targetCol = Grid.GetColumn(existing);

            _grid.Children.Insert(insertedIndex, childControl);

            Grid.SetRow(childControl, targetRow);
            Grid.SetColumn(childControl, targetCol);

            if (this.Tag is CanvasElement element)
            {
                if (childControl.Tag is CanvasElement childElement)
                {
                    childElement.Parent = element;

                    if (!element.Children.Contains(childElement))
                    {
                        element.Children.Insert(insertedIndex, childElement);
                    }

                    childElement.GridRow = targetRow;
                    childElement.GridColumn = targetCol;
                    childElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    childElement.VerticalAlignment = VerticalAlignment.Stretch;

                    if (childControl is IElementControlBase elementControl)
                    {
                        elementControl.UpdateHorizontalAlignment(childElement);
                        elementControl.UpdateVerticalAlignment(childElement);
                    }
                }
            }
        }

        public override void Add(Control? childControl)
        {
            if (childControl == null)
                return;

            if (!_grid.Children.Contains(childControl))
            {
                // 默认放入 (0, 0) 单元格
                _grid.Children.Add(childControl);
                Grid.SetRow(childControl, 0);
                Grid.SetColumn(childControl, 0);

                if (this.Tag is CanvasElement element)
                {
                    if (childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = element;
                        if (!element.Children.Contains(childElement))
                        {
                            element.Children.Add(childElement);
                        }
                        childElement.GridRow = 0;
                        childElement.GridColumn = 0;
                        childElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                        childElement.VerticalAlignment = VerticalAlignment.Stretch;

                        if (childControl is IElementControlBase elementControl)
                        {
                            elementControl.UpdateHorizontalAlignment(childElement);
                            elementControl.UpdateVerticalAlignment(childElement);
                        }
                    }
                }
            }
        }

        #endregion

        #region 样式化属性

        public static readonly StyledProperty<IBrush> GridLineBrushProperty =
            AvaloniaProperty.Register<RoundedRectangleGridControl, IBrush>(nameof(GridLineBrush),
                new ImmutableSolidColorBrush(Color.Parse("#6699CC"), 0.5));

        public IBrush GridLineBrush
        {
            get => GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> CellFillBrushProperty =
            AvaloniaProperty.Register<RoundedRectangleGridControl, IBrush>(nameof(CellFillBrush),
                new ImmutableSolidColorBrush(Color.Parse("#6699CC"), 0.1));

        public IBrush CellFillBrush
        {
            get => GetValue(CellFillBrushProperty);
            set => SetValue(CellFillBrushProperty, value);
        }

        public static readonly StyledProperty<double> GridLineThicknessProperty =
            AvaloniaProperty.Register<RoundedRectangleGridControl, double>(nameof(GridLineThickness), 1.0);

        public double GridLineThickness
        {
            get => GetValue(GridLineThicknessProperty);
            set => SetValue(GridLineThicknessProperty, value);
        }

        public static readonly StyledProperty<CornerRadius> CellCornerRadiusProperty =
            AvaloniaProperty.Register<RoundedRectangleGridControl, CornerRadius>(nameof(CellCornerRadius), new CornerRadius(3));

        public CornerRadius CellCornerRadius
        {
            get => GetValue(CellCornerRadiusProperty);
            set => SetValue(CellCornerRadiusProperty, value);
        }

        #endregion
    }
}
