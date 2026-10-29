using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.ComponentModel;
using yuyiAvalonia.UI.Controls.ToolKits;
using yuyiAvalonia.UI.Controls.ToolKits.Effects;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;
using Color = Avalonia.Media.Color;
using Colors = Avalonia.Media.Colors;
using File = System.IO.File;
using Point = Avalonia.Point;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public abstract class ShapeFlowControlBase : Canvas, IElementControlBase
    {
        protected CanvasElement? _currentElement;

        protected bool _isShowOverlay = false;
        protected bool _ignoreUpdate = false;

        public bool IgnoreUpdate
        {
            get
            {
                return _ignoreUpdate;
            }
            set
            {
                _ignoreUpdate = value;
            }
        }

        // 原始顶点
        private List<Avalonia.Point> originalPoints = new();
        // 实际顶点
        private List<Avalonia.Point> actualPoints = new();
        // 原始顶点
        protected List<Point> OriginalPoints { get => originalPoints; }
        protected List<Point> ActualPoints { get => actualPoints; }

        // 圆角半径
        private List<double> radii = new List<double>();
        protected List<double> Radii { get => radii; }

        // 内部相对原点位置
        protected Point _originPosition = new Point(0, 0);
        protected Point OriginPosition { get => _originPosition; }

        // 初始尺寸
        protected Size? _originRect = null;
        protected Size? OriginRect { get => _originRect; }

        // 背景
        protected Canvas _canvasBackground;

        public event EventHandler<Control?> OnShapeClicked;

        // 边线
        protected Avalonia.Controls.Shapes.Path? _strokePath;
        protected StrokeControl? _fillStroke;
        protected double _fillStrokeMargin = 40;

        // 阴影
        protected Dictionary<ShadowItem, EffectShadow> _effectShadows;

        // Id标签
        private Border? _idLabel;

        public virtual Control? Control { get; }

        public ShapeFlowControlBase()
        {
            _effectShadows = new();

            this.IsHitTestVisible = true;
            this.Background = Brushes.Transparent;

            // 用于存放内部阴影和背景填充渲染
            _canvasBackground = new Canvas();
            _canvasBackground.Tapped += _canvasBackground_Tapped;
            this.Children.Add(_canvasBackground);

            InitStrokes();
        }

        private void _canvasBackground_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            if (OnShapeClicked != null)
            {
                OnShapeClicked.Invoke(this, Control);
            }
        }

        #region Overlay

        public bool IsShowOverlay
        {
            get => _isShowOverlay;
        }

        public abstract void ShowWorkspaceOverlay();

        public abstract void HideWorkspaceOverlay();

        #endregion

        #region 形状

        public abstract void InitialPoints();

        public abstract Geometry? BuildGeometry();

        public virtual Geometry? MoveOffsetGeometry(double offsetX = 0, double offsetY = 0)
        {
            return BuildGeometry();
        }

        public void ResizeActualPoints(Avalonia.Point[] originPoints)
        {
            // 把按"基准尺寸"初始化的顶点等比例缩放到当前控件尺寸。
            // 基准尺寸用当前元素（_currentElement）的 Width/Height，
            // 这样：编辑器改 Width/Height 或 OnSizeChanged 触发时，点都跟着同步。
            if (originPoints.Length == 0 || _currentElement == null || this.Bounds.Size == OriginRect)
                return;

            double baseW = OriginRect!.Value.Width;
            double baseH = OriginRect!.Value.Height;

            if (baseW <= 0 || baseH <= 0)
                return;

            double newW = this.Bounds.Width;
            double newH = this.Bounds.Height;
            if (newW <= 0 || newH <= 0)
                return;

            double scaleX = newW / baseW;
            double scaleY = newH / baseH;
            var actualPoints = new Point[originPoints.Length];

            for (int i = 0; i < originPoints.Length; i++)
            {
                var p = originPoints[i];
                actualPoints[i] = new Point(p.X * scaleX, p.Y * scaleY);
            }

            ActualPoints.Clear();
            ActualPoints.AddRange(actualPoints);
        }

        private void SetShapeClip(Geometry? clipGeometry)
        {
            if (clipGeometry != null)
            {
                this._canvasBackground.Clip = clipGeometry;
            }
        }

        public virtual void UpdateShape(CanvasElement? shapeElement)
        {
            if (shapeElement is not ShapeElement element)
                return;

            // 更新裁剪区域
            var clipGeometry = BuildGeometry();
            SetShapeClip(clipGeometry);

            if (_currentElement != null)
            {
                UpdateStroke(_currentElement);
            }

            UpdateShadows();
        }

        public void UpdatePositionAndSize()
        {
            if (_canvasBackground != null)
            {
                _canvasBackground.Height = this.Bounds.Height;
                _canvasBackground.Width = this.Bounds.Width;
                Canvas.SetLeft(_canvasBackground, _originPosition.X);
                Canvas.SetTop(_canvasBackground, _originPosition.Y);
            }

            if (_fillStroke != null)
            {
                _fillStroke.Height = this.Bounds.Height + _fillStrokeMargin * 2;
                _fillStroke.Width = this.Bounds.Width + _fillStrokeMargin * 2;
                Canvas.SetLeft(_fillStroke, _originPosition.X - _fillStrokeMargin);
                Canvas.SetTop(_fillStroke, _originPosition.Y - _fillStrokeMargin);
            }

            if (_strokePath != null)
            {
                Canvas.SetLeft(_strokePath, _originPosition.X);
                Canvas.SetTop(_strokePath, _originPosition.Y);
            }
        }

        #endregion

        #region 控件属性

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);
            ResizeActualPoints(OriginalPoints.ToArray());
            UpdatePositionAndSize();
            UpdateShape(_currentElement);
            RefreshMultiFill();
        }

        /// <summary>
        /// 父容器尺寸变化时的处理
        /// </summary>
        public void ParentSizeChanged()
        {
            if (_currentElement != null && this.Parent is Canvas canvas)
            {
                bool widthChanged = false;
                bool heightChanged = false;
                IgnoreUpdate = true;

                if (_currentElement.FixLeft)
                {
                    if (_currentElement.FixRight && _currentElement.Right != double.NaN)
                    {
                        double newWidth = Math.Max(0, canvas.Bounds.Width - _currentElement.Left - _currentElement.Right);
                        if (Math.Abs(_currentElement.Width - newWidth) > 0.001)
                        {
                            _currentElement.Width = newWidth;
                            widthChanged = true;
                        }
                    }
                }
                else
                {
                    if (_currentElement.FixRight && _currentElement.Right != double.NaN)
                    {
                        _currentElement.Left = canvas.Bounds.Width - _currentElement.Right - _currentElement.Width;
                        Canvas.SetLeft(this, _currentElement.Left);
                    }
                }

                if (_currentElement.FixTop)
                {
                    if (_currentElement.FixBottom && _currentElement.Bottom != double.NaN)
                    {
                        double newHeight = Math.Max(0, canvas.Bounds.Height - _currentElement.Top - _currentElement.Bottom);
                        if (Math.Abs(_currentElement.Height - newHeight) > 0.001)
                        {
                            _currentElement.Height = newHeight;
                            heightChanged = true;
                        }
                    }
                }
                else
                {
                    if (_currentElement.FixBottom && _currentElement.Bottom != double.NaN)
                    {
                        _currentElement.Top = canvas.Bounds.Height - _currentElement.Bottom - _currentElement.Height;
                        Canvas.SetTop(this, _currentElement.Top);
                    }
                }

                IgnoreUpdate = false;

                if (widthChanged || heightChanged)
                {
                    this.Width = _currentElement.Width;
                    this.Height = _currentElement.Height;
                    UpdateShape(_currentElement);
                    UpdateIdLabelPosition();
                }
            }
        }

        /// <summary>
        /// 应用元素属性
        /// </summary>
        public void UpdateElement(CanvasElement newElement)
        {
            if (_currentElement != newElement)
            {
                _currentElement = newElement;
                this.Tag = newElement;
            }

            if (newElement is not ShapeElement element)
                return;

            // 更新裁剪区域
            var clipGeometry = BuildGeometry();
            SetShapeClip(clipGeometry);

            this.UpdateIsVisible(element.IsVisible);
            this.UpdateOpacity(element.Opacity);
            this.UpdateZIndex(element.ZIndex);

            // 应用对齐方式
            this.UpdateHorizontalAlignment(element);
            this.UpdateVerticalAlignment(element);

            // 设置元素内外边距
            this.UpdateMargin(element);
            this.UpdatePadding(element);

            // 设置旋转角度
            this.UpdateRotation(element.Rotation);

            // 设置宽度和高度
            this.UpdateWidth(element.Width);
            this.UpdateHeight(element.Height);

            // 背景填充
            this.ApplyMultiFill(element);

            // 线条填充
            this.UpdateStroke(element);

            // 阴影
            this.UpdateShadows();

            // 标签
            this.CreateIdLabel(element);
        }

        #endregion

        #region Id 标签

        private void CreateIdLabel(CanvasElement element)
        {
            _idLabel = new Border
            {
                IsHitTestVisible = false,
                IsVisible = element.ShowIdLabel && !string.IsNullOrEmpty(element.Id),
                Background = new SolidColorBrush(Color.Parse("#E0603030")),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(4, 2),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 2, 2),
            };

            var idText = new TextBlock
            {
                Text = element.Id,
                FontSize = 9,
                Foreground = Brushes.White,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            };
            _idLabel.Child = idText;
            AddIdLabel();
        }

        private void AddIdLabel()
        {
            if (_idLabel != null && this.Children.Contains(_idLabel!))
            {
                this.Children.Remove(_idLabel);
            }
            else if (_idLabel == null)
            {
                CreateIdLabel(_currentElement!);
            }
            this.Children.Add(_idLabel!);
            UpdateIdLabelPosition();
        }

        protected void UpdateIdLabelPosition()
        {
            if (_idLabel == null || _currentElement == null)
                return;

            Canvas.SetLeft(_idLabel, this.Bounds.Width - _idLabel.Bounds.Width);
            Canvas.SetTop(_idLabel, this.Bounds.Height + 2);
        }

        public void UpdateIdLabel(CanvasElement element)
        {
            if (_idLabel == null)
                return;

            _idLabel.IsVisible = element.ShowIdLabel && !string.IsNullOrEmpty(element.Id);

            if (_idLabel.Child is TextBlock textBlock)
            {
                textBlock.Text = element.Id;
            }
            UpdateIdLabelPosition();
        }

        #endregion

        #region 通用属性
        public void UpdateOpacity(double opacity) => this.Opacity = opacity;

        public void UpdateIsVisible(bool isVisible) => this.IsVisible = isVisible;

        public void UpdateWidth(double width)
        {
            if (_currentElement == null)
                return;

            this.Width = PropertyValidator.ValidateSize(width);

            // 更新 ID 标签位置
            UpdateIdLabelPosition();
        }

        public void UpdateHeight(double height)
        {
            if (_currentElement == null)
                return;

            this.Height = PropertyValidator.ValidateSize(height);

            // 更新 ID 标签位置
            UpdateIdLabelPosition();
        }

        public void UpdateZIndex(int zIndex)
        {
            this.ZIndex = zIndex;
        }

        public void UpdateRotation(double rotation)
        {
            if (rotation == 0)
            {
                this.RenderTransform = null;
            }
            else
            {
                this.RenderTransform = new RotateTransform(rotation);
                this.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }
        }

        public void UpdateLeft(double? value)
        {
            if (value.HasValue && _currentElement != null)
            {
                Canvas.SetLeft(this, value.Value);
                UpdateIdLabelPosition();
            }
        }

        public void UpdateTop(double? value)
        {
            if (value.HasValue && _currentElement != null)
            {
                Canvas.SetTop(this, value.Value);
                UpdateIdLabelPosition();
            }
        }

        public void UpdateRight(double? value)
        {
            if (value.HasValue && _currentElement != null && this.Parent is Canvas canvas)
            {
                IgnoreUpdate = true;
                _currentElement.Right = value.Value;
                _currentElement.Left = canvas.Bounds.Width - _currentElement.Right - this.Width;

                IgnoreUpdate = false;
                Canvas.SetLeft(this, _currentElement.Left);

                UpdateIdLabelPosition();
            }
        }

        public void UpdateBottom(double? value)
        {
            if (value.HasValue && _currentElement != null && this.Parent is Canvas canvas)
            {
                IgnoreUpdate = true;

                _currentElement.Bottom = value.Value;
                _currentElement.Top = canvas.Bounds.Height - _currentElement.Bottom - this.Height;

                IgnoreUpdate = false;

                Canvas.SetTop(this, _currentElement.Top);

                UpdateIdLabelPosition();
            }
        }

        #endregion

        #region 容器

        public void UpdateHorizontalAlignment(CanvasElement element)
        {
            this.HorizontalAlignment = element.HorizontalAlignment;
        }

        public void UpdateVerticalAlignment(CanvasElement element)
        {
            this.VerticalAlignment = element.VerticalAlignment;
        }

        public virtual void UpdateMargin(CanvasElement element)
        {
            if (element.MarginIsUniform)
            {
                this.Margin = new Thickness(PropertyValidator.ValidateSpacing(element.Margin));
            }
            else
            {
                this.Margin = new Thickness(
                    PropertyValidator.ValidateSpacing(element.MarginLeft),
                    PropertyValidator.ValidateSpacing(element.MarginTop),
                    PropertyValidator.ValidateSpacing(element.MarginRight),
                    PropertyValidator.ValidateSpacing(element.MarginBottom));
            }
        }

        public virtual void UpdatePadding(CanvasElement element)
        {
            // 图形容器使用固定 padding
        }

        #endregion

        #region 背景填充

        public void ApplyFillType(FillItem? fillItem)
        {
            if (fillItem == null)
            {
                return;
            }

            if (!fillItem.IsEnabled)
            {
                this._canvasBackground.Background = Brushes.Transparent;
                return;
            }

            IBrush? fillBrush = null;
            double opacity = Math.Clamp(fillItem.Opacity, 0.0, 1.0);

            switch (fillItem.FillType)
            {
                case FillType.None:
                    fillBrush = Brushes.Transparent;
                    break;
                case FillType.Solid:
                    fillBrush = PropertyValidator.CreateColorBrush(fillItem.BackgroundColor);
                    if (fillBrush is Brush solidBrush)
                        solidBrush.Opacity = opacity;
                    break;
                case FillType.Gradient:
                    fillBrush = CreateGradientBrush(fillItem);
                    if (fillBrush is Brush gradientBrush)
                        gradientBrush.Opacity = opacity;
                    break;
                case FillType.Image:
                    if (!string.IsNullOrEmpty(fillItem.ImagePath) && File.Exists(fillItem.ImagePath))
                    {
                        double width = this.Width > 0 ? this.Width : 100;
                        double height = this.Height > 0 ? this.Height : 100;
                        var bounds = new Avalonia.Rect(0, 0, width, height);

                        var drawingGroup = new DrawingGroup();
                        using (var context = drawingGroup.Open())
                        {
                            context.DrawRectangle(fillBrush, null, bounds);
                            DrawBackgroundImageToContext(context, bounds, fillItem.ImagePath);
                        }
                        var drawingBrush = new DrawingBrush(drawingGroup)
                        {
                            Stretch = Stretch.None,
                            Opacity = opacity
                        };
                        fillBrush = drawingBrush;
                    }
                    break;
                default:
                    fillBrush = Brushes.Transparent;
                    break;
            }

            this._canvasBackground.Background = fillBrush;
        }

        private void DrawBackgroundImageToContext(DrawingContext context, Avalonia.Rect bounds, string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return;

            try
            {
                var bitmap = new Bitmap(imagePath);
                var element = _currentElement;
                var stretch = element?.BackgroundFill.ImageStretch ?? Avalonia.Media.Stretch.UniformToFill;
                var tileMode = element?.BackgroundFill.ImageTileMode ?? Avalonia.Media.TileMode.None;
                var alignmentX = element?.BackgroundFill.ImageAlignmentX ?? Avalonia.Media.AlignmentX.Left;
                var alignmentY = element?.BackgroundFill.ImageAlignmentY ?? Avalonia.Media.AlignmentY.Top;

                var imageBrush = new Avalonia.Media.ImageBrush(bitmap)
                {
                    Stretch = stretch,
                    TileMode = tileMode,
                    AlignmentX = alignmentX,
                    AlignmentY = alignmentY
                };

                if (tileMode != Avalonia.Media.TileMode.None)
                {
                    imageBrush.Stretch = Avalonia.Media.Stretch.None;
                    imageBrush.DestinationRect = new RelativeRect(0, 0, (double)bitmap.Size.Width, (double)bitmap.Size.Height, RelativeUnit.Absolute);
                }
                else
                {
                    imageBrush.Stretch = stretch;
                }

                context.DrawRectangle(imageBrush, null, bounds);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 根据 FillItem 创建渐变画刷。优先使用 <see cref="FillItem.GradientStops"/>
        /// （支持任意数量的停止点 + 每个停止点的透明度），并根据 <see cref="FillItem.GradientKind"/>
        /// 派生出 Linear/Radial/Angle/Diamond。
        /// 旧字段 GradientStartColor/GradientEndColor 仅在 GradientStops 不足两个停止点时作为兜底使用。
        /// </summary>
        private IBrush CreateGradientBrush(FillItem fillItem)
        {
            var avStops = BuildAvaloniaGradientStops(fillItem);

            switch (fillItem.GradientKind)
            {
                case GradientKind.Radial:
                {
                    var centerX = double.IsNaN(fillItem.RadialCenterX) ? 0.5 : fillItem.RadialCenterX;
                    var centerY = double.IsNaN(fillItem.RadialCenterY) ? 0.5 : fillItem.RadialCenterY;
                    var radius = fillItem.RadialRadius <= 0 ? 0.5 : fillItem.RadialRadius;
                    return new RadialGradientBrush
                    {
                        Center = new RelativePoint(centerX, centerY, RelativeUnit.Relative),
                        GradientOrigin = new RelativePoint(centerX, centerY, RelativeUnit.Relative),
                        Radius = radius,
                        GradientStops = avStops
                    };
                }
                case GradientKind.Diamond:
                {
                    // 菱形渐变：通过 DrawingContext 单独绘制为多边形路径
                    double width = this.Width > 0 ? this.Width : 100;
                    double height = this.Height > 0 ? this.Height : 100;
                    var bounds = new Avalonia.Rect(0, 0, width, height);
                    var drawingGroup = new DrawingGroup();
                    using (var context = drawingGroup.Open())
                    {
                        PathHelper.DrawDiamondGradientFill(context, bounds, fillItem);
                    }
                    return new DrawingBrush(drawingGroup)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center,
                        DestinationRect = new RelativeRect(new Avalonia.Rect(0, 0, width, height), RelativeUnit.Absolute)
                    };
                }
                case GradientKind.Angle:
                {
                    // 角度（锥形）渐变：通过 DrawingContext 绘制为扇形
                    double width = this.Width > 0 ? this.Width : 100;
                    double height = this.Height > 0 ? this.Height : 100;
                    var bounds = new Avalonia.Rect(0, 0, width, height);
                    var drawingGroup = new DrawingGroup();
                    using (var context = drawingGroup.Open())
                    {
                        PathHelper.DrawAngleGradientFill(context, bounds, fillItem);
                    }
                    return new DrawingBrush(drawingGroup)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Center,
                        AlignmentY = AlignmentY.Center,
                        DestinationRect = new RelativeRect(new Avalonia.Rect(0, 0, width, height), RelativeUnit.Absolute)
                    };
                }
                case GradientKind.Linear:
                default:
                {
                    // 与 FillItemToGradientBrushConverter（预览）保持一致：
                    // 渐变起点/终点都基于形状中心 (0.5, 0.5) 对称偏移 0.5，
                    // 不 clamp，保证任意角度下向量的长度恒为 1，
                    // 渐变覆盖整个形状而不是被截到一半。
                    var angleRad = fillItem.GradientAngle * Math.PI / 180;
                    var dx = Math.Cos(angleRad);
                    var dy = Math.Sin(angleRad);

                    return new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                        EndPoint   = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                        GradientStops = avStops
                    };
                }
            }
        }

        /// <summary>
        /// 把 <see cref="FillItem.GradientStops"/> 转换成 Avalonia 的 <see cref="GradientStops"/>，
        /// 包含颜色解析、偏移排序、透明度合成。
        /// </summary>
        private static GradientStops BuildAvaloniaGradientStops(FillItem fillItem)
        {
            var result = new GradientStops();
            var stops = fillItem.GradientStops;
            double globalOpacity = Math.Clamp(fillItem.Opacity, 0.0, 1.0);
            if (stops != null && stops.Count > 0)
            {
                foreach (var s in stops.OrderBy(x => x.Offset))
                {
                    Color.TryParse(s.Color, out var c);
                    // 综合每个停止点自身的 Opacity 和 FillItem.Opacity
                    byte a = (byte)(c.A * Math.Clamp(s.Opacity, 0.0, 1.0) * globalOpacity);
                    result.Add(new Avalonia.Media.GradientStop(
                        Color.FromArgb(a, c.R, c.G, c.B), Math.Clamp(s.Offset, 0.0, 1.0)));
                }
            }

            // 兜底：少于两个停止点时使用旧字段 GradientStartColor/GradientEndColor
            if (result.Count < 2)
            {
                result.Clear();
                PropertyValidator.TryParseColor(fillItem.GradientStartColor, out var start, Colors.White);
                PropertyValidator.TryParseColor(fillItem.GradientEndColor, out var end, Colors.Black);
                byte sa = (byte)(start.A * globalOpacity);
                byte ea = (byte)(end.A * globalOpacity);
                result.Add(new Avalonia.Media.GradientStop(Color.FromArgb(sa, start.R, start.G, start.B), 0));
                result.Add(new Avalonia.Media.GradientStop(Color.FromArgb(ea, end.R, end.G, end.B), 1));
            }

            return result;
        }

        /// <summary>
        /// 刷新多层填充
        /// </summary>
        /// 
        public void RefreshMultiFill()
        {
            if (this.Tag is CanvasElement element)
            {
                ApplyMultiFill(element);
            }
        }

        /// <summary>
        /// 应用多层填充到 Canvas 背景
        /// </summary>
        public void ApplyMultiFill(CanvasElement element)
        {
            ApplyFillType(element.BackgroundFill);

            if (element.BackgroundFills != null && element.BackgroundFills.Count > 0 && element.BackgroundFills.Any(x => x.IsEnabled))
            {
                double width = this.Width > 0 ? this.Width : 100;
                double height = this.Height > 0 ? this.Height : 100;

                List<FillItem> fillItems = new();

                if (element.BackgroundFill != null && element.BackgroundFill.Opacity > 0)
                {
                    fillItems.Add(element.BackgroundFill);
                }

                fillItems.AddRange(element.BackgroundFills);

                var drawingBrush = PathHelper.BuildMultiFillBrush(fillItems.ToArray(), width, height);
                this._canvasBackground.Background = (IBrush?)drawingBrush ?? Brushes.Transparent;
            }

        }

        #endregion

        #region 描边

        public void InitStrokes()
        {
            if (_fillStroke == null)
            {
                _fillStroke = new StrokeControl();
                _fillStroke.IsVisible = false;
            }

            if (!this.Children.Contains(_fillStroke))
            {
                this.Children.Add(_fillStroke);
            }

            if (_strokePath == null)
            {
                _strokePath = new Avalonia.Controls.Shapes.Path();
                _strokePath.IsVisible = false;
            }

            if (!this.Children.Contains(_strokePath))
            {
                this.Children.Add(_strokePath);
            }
        }

        public void UpdateStroke(CanvasElement element)
        {
            if (_currentElement == null)
                return;

            if (element.StrokeFills != null && element.StrokeFills.Count > 0)
            {
                ApplyMultiFillsStroke(element);
            }
            else
            {
                ApplySingleFillStroke(element);
            }
        }

        public void UpdateBorderColor(string color)
        {
            if (_currentElement != null)
            {
                UpdateStroke(_currentElement);
            }
        }

        public void UpdateBorderWidth(double width)
        {
            if (_currentElement != null)
            {
                UpdateStroke(_currentElement);

            }
        }

        public void UpdateCornerRadius(double radius)
        {
            if (_currentElement != null)
            {
                UpdateStroke(_currentElement);
            }
        }

        public void ApplyMultiFillsStroke(CanvasElement element, double actualWidth = 0, double actualHeight = 0)
        {
            var strokeWidth = element.StrokeWidth;

            if (_strokePath != null)
            {
                _strokePath.IsVisible = false;
            }

            if (_fillStroke != null)
            {
                var isVisible = strokeWidth > 0;
                _fillStroke.IsVisible = isVisible;

                if (!isVisible)
                {
                    return;
                }
            }

            // 虚线参数（原始像素值，交给 StrokeControl 内部处理）
            Avalonia.Collections.AvaloniaList<double>? dashArray = null;
            double dashOffset = element.StrokeDashOffset;
            bool hasDash = element.StrokeStyleIndex > 0;
            if (hasDash)
            {
                var rawDash = PathHelper.GetStrokeDashArray(element.StrokeDashArrayString);
                if (rawDash != null && rawDash.Count > 0)
                {
                    dashArray = new Avalonia.Collections.AvaloniaList<double>(rawDash);
                }
                else
                {
                    hasDash = false;
                }
            }

            var baseGeometry = MoveOffsetGeometry(_fillStrokeMargin, _fillStrokeMargin);
            StrokeType strokeType = element.StrokeType != StrokeType.Custom ? element.StrokeType : StrokeType.Dashed;

            if (_fillStroke != null)
            {
                _fillStroke.Geometry = baseGeometry;
                _fillStroke.StrokeType = strokeType;
                _fillStroke.StrokeWidth = strokeWidth;
                _fillStroke.DashArrayDefinition = element.StrokeDashArrayString;
                _fillStroke.StrokeDashArray = dashArray;
                _fillStroke.StrokeDashOffset = dashOffset;
                _fillStroke.StrokeJoin = element.StrokeJoin;
                _fillStroke.StrokeEndpoint = element.StrokeEndpoint;
                _fillStroke.StrokeMiterLimit = element.StrokeMiterLimit;
                _fillStroke.StrokeFills = element.StrokeFills;
                _fillStroke.StrokePosition = element.StrokePosition;
                _fillStroke.StrokeOpacity = element.StrokeOpacity;

                if (PropertyValidator.TryParseColor(element.StrokeColor, out var sc))
                {
                    _fillStroke.BackStrokeColor = sc;
                }
                else
                {
                    _fillStroke.BackStrokeColor = Colors.Transparent;
                }

                _fillStroke.UpdateStrokeBackground();
            }
        }

        /// <summary>
        /// 根据CanvasElement的属性创建一个独立渲染代替之渲染Path
        /// </summary>
        /// <param name="element"></param>
        private void ApplySingleFillStroke(CanvasElement element)
        {
            var strokeWidth = element.StrokeWidth;

            _strokePath.IsVisible = false;
            _fillStroke.IsVisible = strokeWidth > 0;

            var baseGeometry = MoveOffsetGeometry(_fillStrokeMargin, _fillStrokeMargin);

            // 虚线参数（原始像素值，交给 StrokeControl 内部处理）
            Avalonia.Collections.AvaloniaList<double>? dashArray = null;
            double dashOffset = element.StrokeDashOffset;
            bool hasDash = element.StrokeStyleIndex > 0;
            if (hasDash)
            {
                var rawDash = PathHelper.GetStrokeDashArray(element.StrokeDashArrayString);
                if (rawDash != null && rawDash.Count > 0)
                {
                    dashArray = new Avalonia.Collections.AvaloniaList<double>(rawDash);
                }
            }

            StrokeType strokeType = element.StrokeType != StrokeType.Custom ? element.StrokeType : StrokeType.Dashed;

            _fillStroke.StrokeType = strokeType;
            _fillStroke.Geometry = baseGeometry;
            _fillStroke.StrokeWidth = strokeWidth;
            _fillStroke.DashArrayDefinition = element.StrokeDashArrayString;
            _fillStroke.StrokeDashArray = dashArray;
            _fillStroke.StrokeDashOffset = dashOffset;
            _fillStroke.StrokeJoin = element.StrokeJoin;
            _fillStroke.StrokeEndpoint = element.StrokeEndpoint;
            _fillStroke.StrokeMiterLimit = element.StrokeMiterLimit;
            _fillStroke.StrokePosition = element.StrokePosition;
            _fillStroke.StrokeFill = element.StrokeFill;

            switch (strokeType)
            {
                case StrokeType.Dashed:
                case StrokeType.Custom:
                    if (PropertyValidator.TryParseColor(element.StrokeColor, out var sc))
                    {
                        _fillStroke.BackStrokeColor = sc;
                        _fillStroke.StrokeOpacity = element.StrokeOpacity;
                    }
                    else
                    {
                        _fillStroke.BackStrokeColor = Colors.Transparent;
                    }
                    break;
                default:
                    _fillStroke.BackStrokeColor = Colors.Transparent;
                    break;
            }

            _fillStroke.UpdateStrokeBackground();

        }

        public void ApplyStrokePath(CanvasElement element)
        {
            if (_fillStroke != null)
            {
                _fillStroke.IsVisible = false;
            }

            if (_strokePath != null)
            {
                _strokePath.IsVisible = true;
            }

            if (_strokePath != null)
            {
                _strokePath.Data = BuildGeometry();
                _strokePath.Fill = Brushes.Transparent;
                _strokePath.StrokeThickness = element.StrokeWidth > 0 ? element.StrokeWidth : 0;
                _strokePath.StrokeJoin = ConvertLineJoin(element.StrokeJoin);
                _strokePath.StrokeLineCap = ConvertLineCap(element.StrokeEndpoint);
                _strokePath.StrokeMiterLimit = element.StrokeMiterLimit;
                _strokePath.StrokeDashArray = null;
                _strokePath.StrokeDashOffset = 0;
                if (element.StrokeStyleIndex >= 0)
                {
                    var rawDash = PathHelper.GetStrokeDashArray(element.StrokeDashArrayString);
                    if (rawDash != null && rawDash.Count > 0)
                    {
                        var sw = Math.Max(element.StrokeWidth, 1);
                        var arr = new Avalonia.Collections.AvaloniaList<double>();
                        foreach (var v in rawDash) arr.Add(v / sw);
                        _strokePath.StrokeDashArray = arr;
                        _strokePath.StrokeDashOffset = element.StrokeDashOffset / sw;
                    }
                }

                switch (element.StrokeFillType)
                {
                    case Models.StrokeFillType.None:
                        _strokePath.Stroke = null;
                        break;
                    case Models.StrokeFillType.Solid:
                        if (PropertyValidator.TryParseColor(element.StrokeColor, out var sc))
                            _strokePath.Stroke = new SolidColorBrush(sc) { Opacity = Math.Clamp(element.StrokeOpacity, 0, 1) };
                        break;
                    case Models.StrokeFillType.Gradient:
                        //var gs = PropertyValidator.TryParseColor(element.StrokeGradientStartColor, out var gsc) ? gsc : Colors.Gray;
                        //var ge = PropertyValidator.TryParseColor(element.StrokeGradientEndColor, out var gec) ? gec : Colors.Gray;
                        //_strokePath.Stroke = PathHelper.CreateSimpleGradientBrush(gs, ge, element.StrokeGradientAngle, element.StrokeOpacity);
                        break;
                    case Models.StrokeFillType.Image:
                        //if (!string.IsNullOrEmpty(element.StrokeImage))
                        //{
                        //    _strokePath.Stroke = PathHelper.CreateImageBrush(element.StrokeImage,
                        //        element.StrokeImageStretch,
                        //        element.StrokeImageTileMode,
                        //        AlignmentX.Center,
                        //        AlignmentY.Center,
                        //        element.StrokeOpacity);
                        //}
                        //else
                        //{
                        //    _strokePath.Stroke = PropertyValidator.CreateColorBrush(element.StrokeColor);
                        //}
                        break;
                }
            }
        }

        private Avalonia.Media.PenLineJoin ConvertLineJoin(StrokeJoin join) => join switch
        {
            StrokeJoin.Miter => Avalonia.Media.PenLineJoin.Miter,
            StrokeJoin.Round => Avalonia.Media.PenLineJoin.Round,
            StrokeJoin.Bevel => Avalonia.Media.PenLineJoin.Bevel,
            _ => Avalonia.Media.PenLineJoin.Miter
        };

        private Avalonia.Media.PenLineCap ConvertLineCap(StrokeEndpoints cap) => cap switch
        {
            StrokeEndpoints.Butt => Avalonia.Media.PenLineCap.Flat,
            StrokeEndpoints.Round => Avalonia.Media.PenLineCap.Round,
            StrokeEndpoints.Square => Avalonia.Media.PenLineCap.Square,
            _ => Avalonia.Media.PenLineCap.Flat
        };

        #endregion

        #region 阴影

        public void UpdateShadows(List<ShadowItem> shadowItems)
        {
            EffectShadow? effectControl = null;

            ShadowItem[] removingShadowItems = _effectShadows.Where(x => !shadowItems.Contains(x.Key)).Select(x => x.Key).ToArray();
            foreach (var item in removingShadowItems)
            {
                if (_effectShadows.ContainsKey(item))
                {
                    effectControl = _effectShadows[item];
                    switch (item.ShadowType)
                    {
                        case ShadowType.Inner:
                            if (this._canvasBackground.Children.Contains(effectControl))
                            {
                                this._canvasBackground.Children.Remove(effectControl);
                            }
                            break;
                        case ShadowType.Outer:
                            if (this.Children.Contains(effectControl))
                            {
                                this.Children.Remove(effectControl);
                            }
                            break;
                    }
                    _effectShadows.Remove(item);
                }
            }

            var shadows = _canvasBackground.Children.Where(x => x is EffectShadow).Select(x => x).ToArray();

            foreach(var shadow in shadows)
            {
                _canvasBackground.Children.Remove(shadow);
            }

            shadows = this.Children.Where(x => x is EffectShadow).Select(x => x).ToArray();

            foreach (var shadow in shadows)
            {
                this.Children.Remove(shadow);
            }

            if (shadowItems!.Count() >= 0)
            {
                int outerIndex = 0;
                foreach (ShadowItem item in shadowItems)
                {
                    if (_effectShadows.ContainsKey(item))
                    {
                        effectControl = _effectShadows[item];
                    }
                    else
                    {
                        effectControl = new();
                        _effectShadows.Add(item, effectControl);
                    }

                    switch (item.ShadowType)
                    {
                        case ShadowType.Inner:
                            effectControl.IsInner = true;
                            effectControl.OffsetGeometry = MoveOffsetGeometry(item.OffsetX, item.OffsetY);
                            this._canvasBackground.Children.Add(effectControl);
                            break;
                        case ShadowType.Outer:
                            effectControl.IsInner = false;
                            this.Children.Insert(outerIndex++, effectControl);
                            break;
                    }

                    effectControl.IsEnabled = item.IsEnabled;
                    effectControl.IsVisible = item.IsEnabled;

                    Color color = Color.Parse(item.Color);
                    effectControl.Geometry = BuildGeometry();
                    effectControl.OffsetX = item.OffsetX;
                    effectControl.OffsetY = item.OffsetY;
                    effectControl.FillColor = color;
                    effectControl.FillOpacity = item.Opacity;
                    effectControl.Blur = item.Blur;
                    effectControl.Spread = item.Spread;
                }
            }
        }

        public void UpdateShadows()
        {
            if (_effectShadows == null)
            {
                return;
            }

            var geo = BuildGeometry();
            EffectShadow? shadow = null;
            var shadowItems = _effectShadows.Select(x => x.Key).ToArray();

            foreach (ShadowItem item in shadowItems)
            {
                shadow = _effectShadows[item];
                shadow.IsEnabled = shadow.IsVisible = item.IsEnabled;

                if (!shadow.IsEnabled)
                    continue;

                shadow.UpdateGeometry(geo!);
                switch (item.ShadowType)
                {
                    case ShadowType.Inner:
                        shadow.OffsetInnerShadow(MoveOffsetGeometry(item.OffsetX, item.OffsetY)!);
                        break;
                    case ShadowType.Outer:
                        break;
                }
            }
        }

        #endregion

    }
}