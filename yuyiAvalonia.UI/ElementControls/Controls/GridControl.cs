using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class GridControl : BorderControlBase
    {
        private Border? _workspaceOverlay;

        public override Grid? Control
        {
            get
            {
                return control as Grid;
            }
        }

        public GridElement? GridElement => this.Tag as GridElement;

        public GridControl(GridElement element) : base(element)
        {
            // 创建内部 Grid（承载子元素）
            control = new Grid
            {
                Background = Brushes.Transparent,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                ColumnSpacing = element.ColumnSpacing,
                RowSpacing = element.RowSpacing,
                //ShowGridLines = true,
            };

            // 创建网格线覆盖层
            _workspaceOverlay = new Border
            {
                IsHitTestVisible = false,
                IsVisible = false,
            };

            // 将内部 Grid 添加到内容 Grid
            _contentPanel!.Children.Add(control);

            // 将覆盖层添加到内容 Grid
            _contentPanel.Children.Add(_workspaceOverlay);

            // 添加Id标签
            AddIdLabelToContainer(_contentPanel);

            // 将内容 Grid 设置为 Border 的 Child
            this.Child = _contentPanel;

            // 初始化描边控件
            InitStrokes();

            // 设置行列定义
            UpdateGridLayout(element);

        }

        public override void UpdateElement(CanvasElement element)
        {
            GridElement gridElement = (GridElement)element;
            base.UpdateElement(gridElement);

            this.UpdateRowSpacing(gridElement.RowSpacing);
            this.UpdateColumnSpacing(gridElement.ColumnSpacing);
            this.UpdateGridLayout(gridElement);
        }

        /// <summary>
        /// 更新 Grid 布局（行列定义）
        /// </summary>
        private void UpdateGridLayout(GridElement element)
        {
            if (Control == null) return;


            Control.RowDefinitions.Clear();
            Control.ColumnDefinitions.Clear();

            // 解析行定义字符串
            ParseRowDefinitions(element);

            // 解析列定义字符串
            ParseColumnDefinitions(element);

            // 同步回 GridElement（确保属性面板能显示正确的值）
            element.RowDefinitions = GetRowDefinitionsString();
            element.ColumnDefinitions = GetColumnDefinitionsString();
        }

        /// <summary>
        /// 解析行定义字符串并应用到 Grid
        /// 格式如 "*,*,*" 表示平均分配
        /// "2*,*" 表示第一行是第二行的2倍
        /// "Auto" 表示自动尺寸
        /// "100" 表示固定值100像素
        /// </summary>
        private void ParseRowDefinitions(GridElement element)
        {
            if (Control == null) return;

            string rowDef = element.RowDefinitions;

            // 如果为空或空字符串，使用平均分配
            if (string.IsNullOrWhiteSpace(rowDef))
            {
                for (int i = 0; i < element.Rows; i++)
                {
                    Control.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                }
                return;
            }

            // 分割并解析每个定义
            var parts = rowDef.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                var def = ParseRowDefinition(part);
                Control.RowDefinitions.Add(def);
            }

            // 如果定义数量少于行数，补充平均分配的剩余行
            while (Control.RowDefinitions.Count < element.Rows)
            {
                Control.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            }
        }

        /// <summary>
        /// 解析列定义字符串并应用到 Grid
        /// 格式如 "*,*,*" 表示平均分配
        /// "2*,*" 表示第一列是第二列的2倍
        /// "Auto" 表示自动尺寸
        /// "100" 表示固定值100像素
        /// </summary>
        private void ParseColumnDefinitions(GridElement element)
        {
            if (Control == null) return;

            string colDef = element.ColumnDefinitions;

            // 如果为空或空字符串，使用平均分配
            if (string.IsNullOrWhiteSpace(colDef))
            {
                for (int i = 0; i < element.Columns; i++)
                {
                    Control.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                }
                return;
            }

            // 分割并解析每个定义
            var parts = colDef.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                var def = ParseColumnDefinition(part);
                Control.ColumnDefinitions.Add(def);
            }

            // 如果定义数量少于列数，补充平均分配的剩余列
            while (Control.ColumnDefinitions.Count < element.Columns)
            {
                Control.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            }
        }

        /// <summary>
        /// 解析单个行定义
        /// 支持的格式：
        /// - "*" 或 "1*" - 平均分配（1倍）
        /// - "2*" - 2倍比例
        /// - "Auto" - 自动尺寸
        /// - "100" - 固定像素值
        /// </summary>
        private static RowDefinition ParseRowDefinition(string definition)
        {
            definition = definition.Trim();

            // 空字符串默认为 Star
            if (string.IsNullOrEmpty(definition) || definition == "*")
            {
                return new RowDefinition(1, GridUnitType.Star);
            }

            // 自动尺寸
            if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return new RowDefinition(GridLength.Auto);
            }

            // 以 * 结尾的 Star 类型（比例）
            if (definition.EndsWith("*"))
            {
                var numPart = definition.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                {
                    return new RowDefinition(1, GridUnitType.Star);
                }
                if (double.TryParse(numPart, out double value))
                {
                    return new RowDefinition(value, GridUnitType.Star);
                }
                return new RowDefinition(1, GridUnitType.Star);
            }

            // 纯数字 - 固定像素值
            if (double.TryParse(definition, out double pixelValue))
            {
                return new RowDefinition(pixelValue, GridUnitType.Pixel);
            }

            // 未知格式，默认为 Star
            return new RowDefinition(1, GridUnitType.Star);
        }

        /// <summary>
        /// 解析单个列定义
        /// 支持的格式：
        /// - "*" 或 "1*" - 平均分配（1倍）
        /// - "2*" - 2倍比例
        /// - "Auto" - 自动尺寸
        /// - "100" - 固定像素值
        /// </summary>
        private static ColumnDefinition ParseColumnDefinition(string definition)
        {
            definition = definition.Trim();

            // 空字符串默认为 Star
            if (string.IsNullOrEmpty(definition) || definition == "*")
            {
                return new ColumnDefinition(1, GridUnitType.Star);
            }

            // 自动尺寸
            if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return new ColumnDefinition(GridLength.Auto);
            }

            // 以 * 结尾的 Star 类型（比例）
            if (definition.EndsWith("*"))
            {
                var numPart = definition.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                {
                    return new ColumnDefinition(1, GridUnitType.Star);
                }
                if (double.TryParse(numPart, out double value))
                {
                    return new ColumnDefinition(value, GridUnitType.Star);
                }
                return new ColumnDefinition(1, GridUnitType.Star);
            }

            // 纯数字 - 固定像素值
            if (double.TryParse(definition, out double pixelValue))
            {
                return new ColumnDefinition(pixelValue, GridUnitType.Pixel);
            }

            // 未知格式，默认为 Star
            return new ColumnDefinition(1, GridUnitType.Star);
        }

        /// <summary>
        /// 显示网格线覆盖层
        /// </summary>
        public override void ShowWorkspaceOverlay()
        {
            if (_workspaceOverlay == null || GridElement == null || _contentPanel == null) return;

            // 确保覆盖层在最上层
            _contentPanel.Children.Remove(_workspaceOverlay);
            _contentPanel.Children.Add(_workspaceOverlay);

            // 强制触发布局更新后再绘制网格
            _contentPanel.InvalidateArrange();

            // 使用 Dispatcher 确保布局完成后再绘制
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                DrawGridLines();
            });

            _workspaceOverlay.IsVisible = true;
            _isShowOverlay = _workspaceOverlay.IsVisible;
        }

        /// <summary>
        /// 隐藏网格线覆盖层
        /// </summary>
        public override void HideWorkspaceOverlay()
        {
            if (_workspaceOverlay != null)
            {
                _workspaceOverlay.IsVisible = false;
                _isShowOverlay = _workspaceOverlay.IsVisible;
            }
        }

        /// <summary>
        /// 重新绘制网格线（用于尺寸变化时）
        /// </summary>
        public void RedrawGridOverlay()
        {
            if (_workspaceOverlay?.IsVisible == true && GridElement != null && _contentPanel != null)
            {
                // 强制触发布局更新后再绘制网格
                _contentPanel.InvalidateArrange();

                // 使用 Dispatcher 确保布局完成后再绘制
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    DrawGridLines();
                });
            }
        }

        /// <summary>
        /// 绘制网格线（基于 GridLength 计算精确尺寸）
        /// RowSpacing/ColumnSpacing 是行/列分割线之间的距离，有 (rows-1) 和 (cols-1) 个间距
        /// 单元格从位置 (0,0) 开始绘制
        /// </summary>
        private void DrawGridLines()
        {
            if (_workspaceOverlay == null || GridElement == null || Control == null) return;

            var rows = GridElement.Rows;
            var cols = GridElement.Columns;
            var rowSpacing = GridElement.RowSpacing;
            var colSpacing = GridElement.ColumnSpacing;

            // 使用 _innerGrid 的实际尺寸
            double width = Control.Bounds.Width;
            double height = Control.Bounds.Height;

            if (width <= 0 || height <= 0) return;

            // 创建画布绘制网格线
            var canvas = new Canvas
            {
                Width = width,
                Height = height,
            };

            // 获取样式化的画刷（半透明效果）
            var lineBrush = GridLineBrush;
            var fillBrush = CellFillBrush;
            var lineThickness = GridLineThickness;
            var cornerRadius = CellCornerRadius;

            // 计算去掉所有间距后的可用空间
            // cols 列有 (cols-1) 个列间距，rows 行有 (rows-1) 个行间距
            double availableWidth = width - colSpacing * (cols - 1);
            double availableHeight = height - rowSpacing * (rows - 1);

            if (availableWidth <= 0 || availableHeight <= 0) return;

            // 从 GridElement 获取行/列定义字符串并解析
            var rowLengths = ParseGridLengths(GridElement.RowDefinitions, rows);
            var colLengths = ParseGridLengths(GridElement.ColumnDefinitions, cols);

            // 计算单元格尺寸
            double[] rowHeights = CalculateSizes(rowLengths, availableHeight, true);
            double[] colWidths = CalculateSizes(colLengths, availableWidth, true);

            // 绘制单元格（从 (0,0) 开始）
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

            _workspaceOverlay.Child = canvas;
        }

        /// <summary>
        /// 根据 GridLength 数组计算实际尺寸
        /// </summary>
        private double[] CalculateSizes(GridLength[] lengths, double availableSize, bool isRow)
        {
            var result = new double[lengths.Length];

            // 分离 Star、Auto 和 Pixel 类型
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

            // 计算 Star 类型的总可用空间
            double starAvailableSize = availableSize - pixelTotal;

            // Auto 暂时用平均分配，最后再调整
            double autoSize = autoCount > 0 ? Math.Max(0, starAvailableSize / (starIndices.Count + autoCount)) : 0;
            double starUnitSize = starIndices.Count > 0 && starTotalValue > 0
                ? Math.Max(0, starAvailableSize - autoSize * autoCount) / starTotalValue
                : 0;

            // 计算每个单元格的实际尺寸
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
                        // Auto 使用平均分配（因为无法预知内容尺寸）
                        result[i] = autoSize;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 解析行/列定义字符串为 GridLength 数组
        /// </summary>
        private GridLength[] ParseGridLengths(string definition, int expectedCount)
        {
            var result = new List<GridLength>();

            if (string.IsNullOrWhiteSpace(definition))
            {
                // 空定义，返回默认的平均分配
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

            // 如果定义数量少于预期数量，补充平均分配的剩余部分
            while (result.Count < expectedCount)
            {
                result.Add(new GridLength(1, GridUnitType.Star));
            }

            return result.ToArray();
        }

        /// <summary>
        /// 获取指定位置的单元格索引（基于 GridLength 计算）
        /// RowSpacing/ColumnSpacing 是行/列分割线之间的距离，有 (rows-1) 和 (cols-1) 个间距
        /// 单元格从位置 (0,0) 开始
        /// position 是相对于 Grid 控件本身的坐标（已考虑 Padding）
        /// </summary>
        public (int Row, int Column)? GetCellAtPosition(Point position)
        {
            if (GridElement == null || Control == null) return null;

            var rows = GridElement.Rows;
            var cols = GridElement.Columns;
            var rowSpacing = GridElement.RowSpacing;
            var colSpacing = GridElement.ColumnSpacing;

            // 获取控件的 Padding（从基础元素获取）
            double paddingLeft = GridElement.PaddingLeft;
            double paddingTop = GridElement.PaddingTop;
            double paddingRight = GridElement.PaddingRight;
            double paddingBottom = GridElement.PaddingBottom;

            double width = Control.Bounds.Width;
            double height = Control.Bounds.Height;

            if (width <= 0 || height <= 0) return null;

            // 考虑 Padding 后的实际内容区域尺寸
            double contentWidth = width - paddingLeft - paddingRight;
            double contentHeight = height - paddingTop - paddingBottom;

            if (contentWidth <= 0 || contentHeight <= 0) return null;

            // 计算去掉所有间距后的可用空间
            double availableWidth = contentWidth - colSpacing * (cols - 1);
            double availableHeight = contentHeight - rowSpacing * (rows - 1);

            if (availableWidth <= 0 || availableHeight <= 0) return null;

            // 从 GridElement 获取行/列定义字符串并解析
            var rowLengths = ParseGridLengths(GridElement.RowDefinitions, rows);
            var colLengths = ParseGridLengths(GridElement.ColumnDefinitions, cols);

            // 计算单元格尺寸
            double[] rowHeights = CalculateSizes(rowLengths, availableHeight, true);
            double[] colWidths = CalculateSizes(colLengths, availableWidth, true);

            // 将 position 转换为相对于内容区域的坐标（减去 Padding）
            double localX = position.X - paddingLeft;
            double localY = position.Y - paddingTop;

            // 边界检查：如果坐标在 Padding 区域内，返回 null
            if (localX < 0 || localY < 0) return null;

            // 查找行（_y 从 0 开始，逐行累加）
            int targetRow = 0;
            double y = 0;

            for (int row = 0; row < rows; row++)
            {
                double cellHeight = rowHeights[row];
                // 检查 _y 坐标是否在此行范围内
                if (localY >= y && localY < y + cellHeight)
                {
                    targetRow = row;
                    break;
                }
                y += cellHeight + rowSpacing;
            }

            // 查找列（_x 从 0 开始，逐列累加）
            int targetCol = 0;
            double x = 0;

            for (int col = 0; col < cols; col++)
            {
                double cellWidth = colWidths[col];
                // 检查 _x 坐标是否在此列范围内
                if (localX >= x && localX < x + cellWidth)
                {
                    targetCol = col;
                    break;
                }
                x += cellWidth + colSpacing;
            }

            // 边界检查
            if (targetRow < 0) targetRow = 0;
            if (targetRow >= rows) targetRow = rows - 1;
            if (targetCol < 0) targetCol = 0;
            if (targetCol >= cols) targetCol = cols - 1;

            return (targetRow, targetCol);
        }

        /// <summary>
        /// 获取相对于 Grid 控件内容区域的坐标（将画布坐标转换为 Grid 内部坐标）
        /// 考虑多层嵌套，正确计算内部 Grid 的起始位置
        /// </summary>
        public Point GetLocalPosition(Point canvasPosition)
        {
            // 获取 Grid 控件（Border）在父容器中的位置
            var gridPosition = this.Bounds.Position;

            // Grid 控件在画布上的绝对位置
            double gridX = gridPosition.X;
            double gridY = gridPosition.Y;

            // 计算内部 Grid (_contentPanel 和 control) 在 Border 内的位置
            // 由于 Border 默认从 (0,0) 开始，_contentPanel 通常也在 (0,0)
            // 但需要考虑可能的 Padding 或其他偏移
            double innerGridX = 0;
            double innerGridY = 0;

            // 获取内容 Grid 相对于 Border 的偏移
            if (_contentPanel != null)
            {
                innerGridX = _contentPanel.Bounds.X;
                innerGridY = _contentPanel.Bounds.Y;
            }

            // 将画布坐标转换为内部 Grid 相对坐标
            double localX = canvasPosition.X - gridX - innerGridX;
            double localY = canvasPosition.Y - gridY - innerGridY;

            return new Point(localX, localY);
        }

        /// <summary>
        /// 更新 Grid 行数
        /// </summary>
        public void UpdateRows(int rows)
        {
            UpdateGridLayout(rows, GridElement.Columns, GridElement.RowSpacing, GridElement.ColumnSpacing);

        }

        /// <summary>
        /// 更新 Grid 列数
        /// </summary>
        public void UpdateColumns(int columns)
        {
            UpdateGridLayout(GridElement.Rows, columns, GridElement.RowSpacing, GridElement.ColumnSpacing);
        }

        /// <summary>
        /// 更新行间距
        /// </summary>
        public void UpdateRowSpacing(double rowSpacing)
        {
            UpdateGridLayout(GridElement.Rows, GridElement.Columns, rowSpacing, GridElement.ColumnSpacing);
        }

        /// <summary>
        /// 更新列间距
        /// </summary>
        public void UpdateColumnSpacing(double columnSpacing)
        {
            UpdateGridLayout(GridElement.Rows, GridElement.Columns, GridElement.RowSpacing, columnSpacing);
        }

        /// <summary>
        /// 更新行定义（当行定义字符串变化时调用）
        /// </summary>
        public void UpdateRowDefinitions(GridElement element)
        {
            if (Control == null) return;

            Control.RowDefinitions.Clear();
            ParseRowDefinitions(element);

            // 同步回 GridElement（确保属性面板显示正确的值）
            element.RowDefinitions = this.GetRowDefinitionsString();

            // 强制 Grid 重新布局，立即显示效果
            Control.InvalidateMeasure();
            Control.InvalidateArrange();

            // 重新绘制网格线覆盖层
            if (_workspaceOverlay?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 更新列定义（当列定义字符串变化时调用）
        /// </summary>
        public void UpdateColumnDefinitions(GridElement element)
        {
            if (Control == null) return;

            Control.ColumnDefinitions.Clear();
            ParseColumnDefinitions(element);

            // 同步回 GridElement（确保属性面板显示正确的值）
            element.ColumnDefinitions = this.GetColumnDefinitionsString();

            // 强制 Grid 重新布局，立即显示效果
            Control.InvalidateMeasure();
            Control.InvalidateArrange();

            // 重新绘制网格线覆盖层
            if (_workspaceOverlay?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        #region 行定义读取和应用

        /// <summary>
        /// 获取当前行的定义字符串（从现有的 Grid.RowDefinitions 读取）
        /// </summary>
        public string GetRowDefinitionsString()
        {
            if (Control == null) return string.Empty;

            var parts = new List<string>();
            foreach (var row in Control.RowDefinitions)
            {
                parts.Add(GridLengthToString(row.Height));
            }
            return string.Join(",", parts);
        }

        /// <summary>
        /// 获取当前列的定义字符串（从现有的 Grid.ColumnDefinitions 读取）
        /// </summary>
        public string GetColumnDefinitionsString()
        {
            if (Control == null) return string.Empty;

            var parts = new List<string>();
            foreach (var col in Control.ColumnDefinitions)
            {
                parts.Add(GridLengthToString(col.Width));
            }
            return string.Join(",", parts);
        }

        /// <summary>
        /// 将 GridLength 转换为字符串表示
        /// </summary>
        private string GridLengthToString(GridLength length)
        {
            if (length.IsAuto)
            {
                return "Auto";
            }
            else if (length.IsStar)
            {
                // * = 1*, 2* = 2*, 等等
                if (length.Value == 1)
                {
                    return "*";
                }
                return $"{length.Value}*";
            }
            else
            {
                // 固定像素值
                return length.Value.ToString();
            }
        }

        /// <summary>
        /// 应用行定义字符串（直接修改现有定义，不清空重建）
        /// </summary>
        public void ApplyRowDefinitions(string definition)
        {
            if (Control == null) return;

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // 只更新已有的定义，不改变数量
            for (int i = 0; i < parts.Length && i < Control.RowDefinitions.Count; i++)
            {
                var rowDef = Control.RowDefinitions[i];
                var newLength = ParseGridLength(parts[i]);

                // 直接设置 Width/Height 属性
                rowDef.SetCurrentValue(RowDefinition.HeightProperty, newLength);
            }

            // 强制 Grid 重新布局，立即显示效果
            Control.InvalidateMeasure();
            Control.InvalidateArrange();

            // 重新绘制网格线覆盖层
            if (_workspaceOverlay?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 应用列定义字符串（直接修改现有定义，不清空重建）
        /// </summary>
        public void ApplyColumnDefinitions(string definition)
        {
            if (Control == null) return;

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // 只更新已有的定义，不改变数量
            for (int i = 0; i < parts.Length && i < Control.ColumnDefinitions.Count; i++)
            {
                var colDef = Control.ColumnDefinitions[i];
                var newLength = ParseGridLength(parts[i]);

                // 直接设置 Width 属性
                colDef.SetCurrentValue(ColumnDefinition.WidthProperty, newLength);
            }

            // 强制 Grid 重新布局，立即显示效果
            Control.InvalidateMeasure();
            Control.InvalidateArrange();

            // 重新绘制网格线覆盖层
            if (_workspaceOverlay?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 解析字符串为 GridLength
        /// </summary>
        private GridLength ParseGridLength(string definition)
        {
            definition = definition.Trim();

            // 空字符串或 * 默认为 1*
            if (string.IsNullOrEmpty(definition) || definition == "*")
            {
                return new GridLength(1, GridUnitType.Star);
            }

            // Auto
            if (definition.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return GridLength.Auto;
            }

            // N* 格式（比例）
            if (definition.EndsWith("*"))
            {
                var numPart = definition.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                {
                    return new GridLength(1, GridUnitType.Star);
                }
                if (double.TryParse(numPart, out double value))
                {
                    return new GridLength(value, GridUnitType.Star);
                }
                return new GridLength(1, GridUnitType.Star);
            }

            // 纯数字（固定像素）
            if (double.TryParse(definition, out double pixelValue))
            {
                return new GridLength(pixelValue, GridUnitType.Pixel);
            }

            // 默认返回 1*
            return new GridLength(1, GridUnitType.Star);
        }

        #endregion

        protected int Rows { get; set; }
        protected int Columns { get; set; }
        protected double RowSpacing { get; set; }
        protected double ColumnSpacing { get; set; }

        /// <summary>
        /// 网格线颜色（默认半透明天蓝色）
        /// </summary>
        public static readonly StyledProperty<IBrush> GridLineBrushProperty =
            AvaloniaProperty.Register<GridControl, IBrush>(nameof(GridLineBrush),
                new ImmutableSolidColorBrush(Color.Parse("#6699CC"), 0.5));

        public IBrush GridLineBrush
        {
            get => GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        /// <summary>
        /// 单元格填充颜色（默认半透明填充）
        /// </summary>
        public static readonly StyledProperty<IBrush> CellFillBrushProperty =
            AvaloniaProperty.Register<GridControl, IBrush>(nameof(CellFillBrush),
                new ImmutableSolidColorBrush(Color.Parse("#6699CC"), 0.1));

        public IBrush CellFillBrush
        {
            get => GetValue(CellFillBrushProperty);
            set => SetValue(CellFillBrushProperty, value);
        }

        /// <summary>
        /// 网格线宽度（默认1像素）
        /// </summary>
        public static readonly StyledProperty<double> GridLineThicknessProperty =
            AvaloniaProperty.Register<GridControl, double>(nameof(GridLineThickness), 1.0);

        public double GridLineThickness
        {
            get => GetValue(GridLineThicknessProperty);
            set => SetValue(GridLineThicknessProperty, value);
        }

        /// <summary>
        /// 单元格圆角半径
        /// </summary>
        public static readonly StyledProperty<CornerRadius> CellCornerRadiusProperty =
            AvaloniaProperty.Register<GridControl, CornerRadius>(nameof(CellCornerRadius), new CornerRadius(3));

        public CornerRadius CellCornerRadius
        {
            get => GetValue(CellCornerRadiusProperty);
            set => SetValue(CellCornerRadiusProperty, value);
        }

        /// <summary>
        /// 更新 Grid 布局（行列定义）
        /// </summary>
        private void UpdateGridLayout(int rows, int columns, double rowSpacing, double columnSpacing)
        {
            if (Control == null) return;

            rows = Math.Max(1, rows);
            columns = Math.Max(1, columns);

            Control.RowSpacing = rowSpacing;
            Control.ColumnSpacing = columnSpacing;

            //_contentPanel!.RowSpacing = rowSpacing;
            //_contentPanel.ColumnSpacing = columnSpacing;

            if (rows > Control.RowDefinitions.Count)
            {
                for (int i = Control.RowDefinitions.Count; i < rows; i++)
                {
                    Control.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                }
            }
            else if (rows < Control.RowDefinitions.Count)
            {
                while (Control.RowDefinitions.Count > rows)
                {
                    var controls = Control.Children.Where(c => c.GetValue(Grid.RowProperty) == Control.RowDefinitions.Count - 1).ToArray();

                    if (controls.Length > 0)
                    {
                        for (int j = 0; j < controls.Length; j++)
                        {
                            var Control = controls[j];
                            this.Control.Children.Remove(Control);
                        }
                    }

                    Control.RowDefinitions.RemoveAt(Control.RowDefinitions.Count - 1);
                }
            }

            if (columns > Control.ColumnDefinitions.Count)
            {
                for (int i = Control.ColumnDefinitions.Count; i < columns; i++)
                {
                    Control.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                }
            }
            else if (columns < Control.ColumnDefinitions.Count)
            {

                while (Control.ColumnDefinitions.Count > columns)
                {
                    var controls = Control.Children.Where(c => c.GetValue(Grid.ColumnProperty) == Control.ColumnDefinitions.Count - 1).ToArray();

                    if (controls.Length > 0)
                    {
                        for (int j = 0; j < controls.Length; j++)
                        {
                            var Control = controls[j];
                            this.Control.Children.Remove(Control);
                        }
                    }

                    Control.ColumnDefinitions.RemoveAt(Control.ColumnDefinitions.Count - 1);
                }
            }

            // 减少列时先同步 GridElement，再清理多余的子元素和列定义
            GridElement!.Columns = columns;
            GridElement.ColumnDefinitions = this.GetColumnDefinitionsString();

            // 增加行时同步 GridElement
            GridElement.Rows = rows;
            GridElement.RowDefinitions = this.GetRowDefinitionsString();

            // 更新网格线覆盖层
            if (_workspaceOverlay?.IsVisible == true)
            {
                DrawGridLines();
            }
        }

        /// <summary>
        /// 将控件添加到指定单元格
        /// </summary>
        public void AddChildToCell(BorderControlBase? child, int row, int column)
        {
            if (Control == null || child == null || this == child)
                return;

            if (child.Tag != null && child.Tag is CanvasElement element && this.Tag is CanvasElement parentElement)
            {
                if (!Control.Children.Contains(child))
                {
                    ((Panel?)child.Parent!).Children.Remove(child);
                    Control.Children.Add(child);

                    Grid.SetRow(child, row);
                    Grid.SetColumn(child, column);

                    element.Parent = parentElement;
                    if(!parentElement.Children.Contains(element))
                    {
                        parentElement.Children.Add(element);
                    }

                    element!.GridRow = row;
                    element!.GridColumn = column;

                    element.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                    element.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

                    // 设置子元素填充整个单元格
                    child.UpdateHorizontalAlignment(element);
                    child.UpdateVerticalAlignment(element);
                }
            }
        }

        /// <summary>
        /// 获取 Grid 行定义的字符串，例如："Auto,*,30"
        /// </summary>
        public string GetGridRowDefinitionString()
        {
            List<string> parts = new List<string>();

            foreach (RowDefinition row in Control.RowDefinitions)
            {
                parts.Add(GetGridLengthString(row.Height));
            }

            return string.Join(",", parts);
        }

        /// <summary>
        /// 获取 Grid 行定义的字符串，例如："Auto,*,30"
        /// </summary>
        public string GetGridColumnDefinitionString()
        {
            List<string> parts = new List<string>();

            foreach (ColumnDefinition column in Control.ColumnDefinitions)
            {
                parts.Add(GetGridLengthString(column.Width));
            }

            return string.Join(",", parts);
        }

        private string GetGridLengthString(GridLength gl)
        {
            if (gl.IsAuto)
            {
                return "Auto";
            }
            else if (gl.IsStar)
            {
                double value = gl.Value;
                return value == 1 ? "*" : $"{value}*";
            }
            else
            {
                // 固定像素
                return gl.Value.ToString("0.##");
            }
        }
    }
}