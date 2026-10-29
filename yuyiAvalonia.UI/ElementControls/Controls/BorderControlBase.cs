using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using yuyiAvalonia.UI.Controls.ToolKits;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;
using Path = Avalonia.Controls.Shapes.Path;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public abstract partial class BorderControlBase : Border, IElementControlBase
    {
        protected Control? control;
        protected bool _isShowOverlay = false;

        protected Panel? _contentPanel;

        /// <summary>
        /// 边线绘制层集合（每条线对应一个 Path，支持多层独立线条）
        /// </summary>
        private readonly List<Avalonia.Controls.Shapes.Path> _strokePaths = new();

        /// <summary>
        /// StrokeControl 实例，用于多层填充模式的描边渲染
        /// </summary>
        protected StrokeControl _stroke;

        /// <summary>
        /// 右下角Id标签
        /// </summary>
        private Border? _idLabel;

        public virtual Control Control
        {
            get
            {
                return control;
            }
        }

        public BorderControlBase(CanvasElement element)
        {
            // 创建内容（承载子元素和覆盖层）
            _contentPanel = new Panel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };

            // 多层线条 Path 在 ApplyStrokePath 时按需创建

            this.ApplyMultiFill(element);
            this.Opacity = element.Opacity;
            this.Width = element.Width;
            this.Height = element.Height;

            this.BorderBrush = PropertyValidator.CreateColorBrush(element.BorderColor);
            this.BorderThickness = new Thickness(PropertyValidator.ValidateSpacing(element.BorderWidth, min: 0));
            this.CornerRadius = new CornerRadius(PropertyValidator.ValidateSpacing(element.CornerRadius));

            // 处理 Padding：根据 IsUniform 决定使用统一值还是分开的值
            this.UpdatePadding(element);

            // 处理 Margin：根据 IsUniform 决定使用统一值还是分开的值
            this.UpdateMargin(element);

            this.UpdateHorizontalAlignment(element);
            this.UpdateVerticalAlignment(element);

            this.UpdateZIndex(element.ZIndex);

            // 设置旋转角度
            this.UpdateRotation(element.Rotation);

            // 设置可见性
            this.IsVisible = element.IsVisible;

            this.Tag = element;

            // 创建右下角Id标签（不添加到任何容器，子类通过 AddIdLabelToContainer 方法添加）
            _idLabel = new Border
            {
                IsHitTestVisible = false,
                IsVisible = element.ShowIdLabel && !string.IsNullOrEmpty(element.Id),
                Background = new Avalonia.Media.SolidColorBrush(Color.Parse("#E0603030")),
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
                Foreground = Avalonia.Media.Brushes.White,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            };

            _idLabel.Child = idText;

            // 初始化高级边线样式
            UpdateStroke(element);

            // 添加Id标签
            AddIdLabelToContainer(_contentPanel!);

            this.SizeChanged += (object? sender, SizeChangedEventArgs e) =>
            {
                RefreshMultiFill();
            };

        }

        #region 浮动层
        public bool IsShowOverlay
        {
            get => _isShowOverlay;
        }

        public abstract void ShowWorkspaceOverlay();

        public abstract void HideWorkspaceOverlay();

        #endregion

        #region 控件属性

        public void ParentSizeChanged()
        {
            if (this.Tag != null && this.Tag is CanvasElement element)
            {
                if (this.Parent != null && this.Parent is Canvas canvas)
                {
                    if (element.FixLeft)
                    {
                        if (element.FixRight && element.Right != double.NaN)
                        {
                            element.Width = Math.Max(0, canvas.Bounds.Width - element.Left - element.Right);
                            this.Width = element.Width;
                        }
                    }
                    else
                    {
                        if (element.FixRight && element.Right != double.NaN)
                        {
                            element.Left = canvas.Bounds.Width - element.Right - element.Width;
                            Canvas.SetLeft(this, element.Left);
                        }
                    }
                    if (element.FixTop)
                    {
                        if (element.FixBottom && element.Bottom != double.NaN)
                        {
                            element.Height = Math.Max(0, canvas.Bounds.Height - element.Top - element.Bottom);
                            this.Height = element.Height;
                        }
                    }
                    else
                    {
                        if (element.FixBottom && element.Bottom != double.NaN)
                        {
                            element.Top = canvas.Bounds.Height - element.Bottom - element.Height;
                            Canvas.SetTop(this, element.Top);
                        }
                    }
                }
            }
        }

        public virtual void UpdateElement(CanvasElement element)
        {
            this.UpdateOpacity(element.Opacity);
            this.UpdateIsVisible(element.IsVisible);

            this.UpdateZIndex(element.ZIndex);

            this.ApplyMultiFill(element);

            this.UpdateWidth(element.Width);
            this.UpdateHeight(element.Height);

            this.UpdateHorizontalAlignment(element);
            this.UpdateVerticalAlignment(element);

            // 设置旋转角度
            this.UpdateRotation(element.Rotation);

            if (element.Rotation != 0)
            {
                this.RenderTransform = new RotateTransform(element.Rotation);
                this.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            }

            // 根据是否启用高级边框决定边框处理方式
            //bool useAdvancedStroke = element.HasAdvancedStroke && element.StrokeWidth > 0
            //    && (element.StrokeFillType != StrokeFillType.None
            //        || (element.StrokeFills != null && element.StrokeFills.Count > 0));

            //if (useAdvancedStroke)
            //{
            //    // 高级边框启用时，直接应用高级边框（会在 UpdateStroke 中设置 BorderThickness=0）
            //    // 先设置圆角，因为高级边框需要使用
            //    this.CornerRadius = new CornerRadius(PropertyValidator.ValidateSpacing(element.CornerRadius));
            //}
            //else
            //{
                // 普通边框模式
                this.UpdateBorderColor(element.BorderColor);
                this.UpdateBorderWidth(element.BorderWidth);
                this.UpdateCornerRadius(element.CornerRadius);
            //}

            // 处理 Padding：根据 IsUniform 决定使用统一值还是分开的值
            this.UpdatePadding(element);

            // 处理 Margin：根据 IsUniform 决定使用统一值还是分开的值
            this.UpdateMargin(element);

            // 应用高级边线样式（包含边框优先级判断）
            UpdateStroke(element);

            // 更新Id标签
            UpdateIdLabel(element);
        }

        #endregion

        #region 通用属性

        protected void AddIdLabelToContainer(Panel container)
        {
            if (_idLabel != null && !container.Children.Contains(_idLabel))
            {
                container.Children.Add(_idLabel);
            }
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
        }

        public void UpdateOpacity(double opacity)
        {
            this.Opacity = opacity;
        }

        public void UpdateIsVisible(bool isVisible)
        {
            this.IsVisible = isVisible;
        }

        public void UpdateMargin(CanvasElement element)
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

        public void UpdatePadding(CanvasElement element)
        {
            if (element.PaddingIsUniform)
            {
                this.Padding = new Thickness(PropertyValidator.ValidateSpacing(element.Padding));
            }
            else
            {
                this.Padding = new Thickness(
                    PropertyValidator.ValidateSpacing(element.PaddingLeft),
                    PropertyValidator.ValidateSpacing(element.PaddingTop),
                    PropertyValidator.ValidateSpacing(element.PaddingRight),
                    PropertyValidator.ValidateSpacing(element.PaddingBottom));
            }
        }

        public void UpdateWidth(double width)
        {
            if (this.Parent is Panel && this.Parent is not Canvas)
            {
                if (this.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    if (this.Width != double.NaN)
                        this.Width = double.NaN;
                    return;
                }
            }
            this.Width = PropertyValidator.ValidateSize(width);

            if (this.Tag is CanvasElement element && element.Parent != null && this.Parent is Canvas canvas)
            {
                element.Right = Math.Max(0, canvas.Bounds.Width - element.Left - element.Width);
                UpdateStroke(element);
            }

            // 更新边线尺寸
            UpdateStrokePath();

        }

        public void UpdateHeight(double height)
        {
            if (this.Parent is Panel && this.Parent is not Canvas)
            {
                if (this.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    if (this.Height != double.NaN)
                        this.Height = double.NaN;
                    return;
                }
            }
            this.Height = PropertyValidator.ValidateSize(height);

            if (this.Tag is CanvasElement element && element.Parent != null && this.Parent is Canvas canvas)
            {
                element.Bottom = Math.Max(0, canvas.Bounds.Height - element.Top - element.Height);
                UpdateStroke(element);
            }

            // 更新边线尺寸
            UpdateStrokePath();
        }

        public virtual void UpdateHorizontalAlignment(CanvasElement element)
        {
            if (element.HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                if (this.Parent != null && (this.Parent is Border || this.Parent is Panel) && this.Parent is not Canvas)
                {
                    this.Width = double.NaN;
                    this.HorizontalAlignment = element.HorizontalAlignment;
                }
                return;
            }

            this.HorizontalAlignment = element.HorizontalAlignment;
            this.Width = element.Width;
        }

        public virtual void UpdateVerticalAlignment(CanvasElement element)
        {
            if (element.VerticalAlignment == VerticalAlignment.Stretch)
            {
                if (this.Parent != null && (this.Parent is Border || this.Parent is Panel) && this.Parent is not Canvas)
                {
                    this.Height = double.NaN; // 设置为自动宽度
                    this.VerticalAlignment = element.VerticalAlignment;
                }
                return;
            }

            this.VerticalAlignment = element.VerticalAlignment;
            this.Height = element.Height;
        }

        #endregion

        #region 位置

        public virtual void UpdateLeft(double? value)
        {
            if (this.Parent != null && this.Parent is Canvas canvas)
            {
                Canvas.SetRight(this, double.NaN);
                Canvas.SetLeft(this, value ?? double.NaN);

                if (this.Tag is CanvasElement element && element.Parent != null)
                {
                    element.Right = Math.Max(0, canvas.Bounds.Width - element.Left - this.Width);
                }
            }
        }

        public virtual void UpdateTop(double? value)
        {
            if (this.Parent != null && this.Parent is Canvas canvas)
            {
                Canvas.SetBottom(this, double.NaN);
                Canvas.SetTop(this, value ?? double.NaN);

                if (this.Tag is CanvasElement element && element.Parent != null)
                {
                    element.Bottom = Math.Max(0, canvas.Bounds.Height - element.Top - this.Height);
                }
            }
        }

        public virtual void UpdateRight(double? value)
        {
            if (this.Parent != null && this.Parent is Canvas)
            {
                Canvas.SetLeft(this, double.NaN);
                Canvas.SetRight(this, value ?? double.NaN);
            }
        }

        public virtual void UpdateBottom(double? value)
        {
            if (this.Parent != null && this.Parent is Canvas)
            {
                Canvas.SetTop(this, double.NaN);
                Canvas.SetBottom(this, value ?? double.NaN);
            }
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
            }
        }

        #endregion

        #region 背景填充

        /// <summary>
        /// 应用多层填充到 Border 背景
        /// </summary>
        public void ApplyMultiFill(CanvasElement element)
        {
            if (element.BackgroundFills == null || element.BackgroundFills.Count == 0)
            {
                ApplyFillType(element.BackgroundFill);
                return;
            }

            double width = this.Bounds.Width > 0 ? this.Bounds.Width : (element.Width > 0 ? element.Width : 100);
            double height = this.Bounds.Height > 0 ? this.Bounds.Height : (element.Height > 0 ? element.Height : 100);

            var drawingBrush = PathHelper.BuildMultiFillBrush(element.BackgroundFills.ToArray(), width, height);

            if (drawingBrush != null)
            {
                var effectFill = element.BackgroundFills.FirstOrDefault(f => f.IsEnabled && f.FillType == FillType.Image && f.ImageEffect?.NeedEffect == true);
                if (effectFill != null)
                {
                    var bounds = new Avalonia.Rect(0, 0, width, height);
                    var effectGroup = new DrawingGroup();
                    using (var effectCtx = effectGroup.Open())
                    {
                        effectCtx.DrawRectangle(drawingBrush, null, bounds);
                    }
                    var finalBrush = new DrawingBrush(effectGroup);
                    if (Math.Abs(effectFill.ImageEffect.Opacity - 1.0) > 0.001)
                        finalBrush.Opacity = effectFill.ImageEffect.Opacity;
                    this.Background = finalBrush;
                    return;
                }
            }

            this.Background = (IBrush?)drawingBrush ?? Brushes.Transparent;
        }

        /// <summary>
        /// 刷新多层填充（尺寸变化或属性变化时调用）
        /// </summary>
        public void RefreshMultiFill()
        {
            if (this.Tag is CanvasElement element)
            {
                ApplyMultiFill(element);
            }
        }

        public void ApplyFillType(FillItem? fillItem)
        {
            if (fillItem == null) return;

            // 获取底色画刷
            IBrush? fillBrush = null;
            switch (fillItem.FillType)
            {
                case FillType.None:
                    fillBrush = Brushes.Transparent;
                    break;
                case FillType.Solid:
                    fillBrush = PropertyValidator.CreateColorBrush(fillItem.BackgroundColor);
                    break;
                case FillType.Gradient:
                    fillBrush = CreateGradientBrush(fillItem.GradientStartColor, fillItem.GradientEndColor, fillItem.GradientAngle);
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
                            DrawBackgroundImageToContext(context, bounds, fillItem.ImagePath);
                        }
                        var drawingBrush = new DrawingBrush(drawingGroup)
                        {
                            Stretch = Stretch.None
                        };
                        this.Background = drawingBrush;
                        return;
                    }
                    else
                    {
                        this.Background = Brushes.Transparent;
                        return;
                    }
            }

            this.Background = fillBrush;
        }

        /// <summary>
        /// 绘制背景图片到 DrawingContext
        /// </summary>
        private void DrawBackgroundImageToContext(DrawingContext context, Avalonia.Rect bounds, string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return;

            try
            {
                var bitmap = new Bitmap(imagePath);
                var element = Tag as CanvasElement;
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
            catch
            {
                // 图片加载失败，忽略
            }
        }

        private static IBrush CreateGradientBrush(string startColor, string endColor, double angle)
        {
            // 与 FillItemToGradientBrushConverter（预览）保持一致：
            // 以 (0.5, 0.5) 为对称中心，沿角度方向偏移 0.5，
            // 不 clamp，保证任意角度下向量长度恒为 1，渐变覆盖整个形状。
            var angleRad = angle * Math.PI / 180;
            var dx = Math.Cos(angleRad);
            var dy = Math.Sin(angleRad);

            PropertyValidator.TryParseColor(startColor, out var start, Colors.White);
            PropertyValidator.TryParseColor(endColor, out var end, Colors.Black);

            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new Avalonia.Media.GradientStop(start, 0),
                    new Avalonia.Media.GradientStop(end, 1)
                }
            };
        }


        #endregion

        #region 描边

        /// <summary>
        /// 初始化描边控件，在子类将 Child 赋值为 _contentPanel 后调用
        /// </summary>
        protected void InitStrokes()
        {
            if (_stroke == null)
            {
                _stroke = new StrokeControl();
                _stroke.IsVisible = false;
            }
            _contentPanel?.Children.Add(_stroke);
        }

        public void UpdateBorderColor(string color)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateBorderColor] called: color={color}");
            // 如果启用高级边框，只更新高级边框颜色，不更新普通边框
            if (Tag is CanvasElement element)
            {
                UpdateStroke(element);
                return;
            }
            this.BorderBrush = PropertyValidator.CreateColorBrush(color);
        }

        public void UpdateBorderWidth(double width)
        {
            System.Diagnostics.Debug.WriteLine($"[UpdateBorderWidth] called: width={width}");
            // 如果启用高级边框，只更新高级边框宽度，不更新普通边框
            if (Tag is CanvasElement element )
            {
                UpdateStroke(element);
                return;
            }
            this.BorderThickness = new Thickness(PropertyValidator.ValidateSpacing(width, min: 0));
        }

        public void UpdateCornerRadius(double radius)
        {
            var validatedRadius = PropertyValidator.ValidateSpacing(radius);
            this.CornerRadius = new CornerRadius(validatedRadius);

            // 圆角变化时也要刷新高级边框
            if (Tag is CanvasElement element)
            {
                UpdateStroke(element);
            }
        }

        /// <summary>
        /// 应用高级边线
        /// </summary>
        public void UpdateStroke(CanvasElement element)
        {
            var actualWidth = this.Bounds.Width > 0 ? this.Bounds.Width : (element.Width > 0 ? element.Width : 100);
            var actualHeight = this.Bounds.Height > 0 ? this.Bounds.Height : (element.Height > 0 ? element.Height : 100);

            if (element.StrokeFills != null && element.StrokeFills.Count > 0)
            {
                ApplyMultiFillsStroke(element, actualWidth, actualHeight);
            }
            else
            {
                ApplyStrokePath(element);
            }
        }

        /// <summary>
        /// 多层填充模式（<c>StrokeFills</c> 有内容）：统一走 <see cref="StrokeControl"/>。
        /// </summary>
        public void ApplyMultiFillsStroke(CanvasElement element, double actualWidth, double actualHeight)
        {
            var baseGeometry = new RectangleGeometry(new Rect(0, 0, actualWidth, actualHeight), element.CornerRadius, element.CornerRadius);
            var strokeWidth = element.StrokeWidth > 0 ? element.StrokeWidth : 1;

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

            _strokePaths.ForEach(p => p.IsVisible = false);
            _stroke.IsVisible = true;
            _stroke.Geometry = baseGeometry;
            _stroke.StrokeType = element.StrokeType;
            _stroke.StrokeWidth = strokeWidth;
            _stroke.DashArrayDefinition = element.StrokeDashArrayString;
            _stroke.StrokeDashArray = dashArray;
            _stroke.StrokeDashOffset = dashOffset;
            _stroke.StrokeJoin = element.StrokeJoin;
            _stroke.StrokeEndpoint = element.StrokeEndpoint;
            _stroke.StrokeMiterLimit = element.StrokeMiterLimit;
            _stroke.StrokeFills = element.StrokeFills;
            _stroke.StrokePosition = element.StrokePosition;
            _stroke.UpdateStrokeBackground();

            this.BorderThickness = new Thickness(0);
        }

        /// <summary>
        /// 单填充模式（<c>StrokeFills</c> 为空）：用 <c>_strokePath</c> 配 <c>Path.Stroke</c>。
        /// </summary>
        public void ApplyStrokePath(CanvasElement element)
        {
            var strokeWidth = PathHelper.GetStrokeWidth(element.StrokeWidthMode,
                element.StrokeWidth, element.StrokeWidthLeft, element.StrokeWidthTop,
                element.StrokeWidthRight, element.StrokeWidthBottom);

            if (strokeWidth <= 0 || (element.StrokeFillType == StrokeFillType.None && (element.StrokeFills == null || element.StrokeFills.Count == 0)))
            {
                RestoreOriginalBorder(element);
                return;
            }

            if(_stroke!=null)
                _stroke.IsVisible = false;

            EnsureStrokePaths(1);
            var sp = _strokePaths[0];
            sp.IsVisible = true;
            sp.Fill = Brushes.Transparent;
            sp.StrokeThickness = strokeWidth;
            sp.StrokeJoin = ConvertLineJoin(element.StrokeJoin);
            sp.StrokeLineCap = ConvertLineCap(element.StrokeEndpoint);
            sp.StrokeMiterLimit = element.StrokeMiterLimit;

            var actualWidth = this.Bounds.Width > 0 ? this.Bounds.Width : (element.Width > 0 ? element.Width : 100);
            var actualHeight = this.Bounds.Height > 0 ? this.Bounds.Height : (element.Height > 0 ? element.Height : 100);
            PathHelper.ConfigurePathGeometry(sp, actualWidth, actualHeight, strokeWidth, element.CornerRadius);

            sp.StrokeDashArray = null;
            sp.StrokeDashOffset = 0;
            if (element.StrokeStyleIndex > 0)
            {
                var rawDash = PathHelper.GetStrokeDashArray(element.StrokeDashArrayString);
                if (rawDash != null && rawDash.Count > 0)
                {
                    var arr = new Avalonia.Collections.AvaloniaList<double>();
                    foreach (var v in rawDash) arr.Add(v / strokeWidth);
                    sp.StrokeDashArray = arr;
                    sp.StrokeDashOffset = element.StrokeDashOffset / strokeWidth;
                }
            }

            switch (element.StrokeFillType)
            {
                case Models.StrokeFillType.None:
                    sp.Stroke = null;
                    break;
                case Models.StrokeFillType.Solid:
                    if (PropertyValidator.TryParseColor(element.StrokeColor, out var sc))
                        sp.Stroke = new SolidColorBrush(sc) { Opacity = Math.Clamp(element.StrokeOpacity, 0, 1) };
                    break;
                case Models.StrokeFillType.Gradient:
                    //var gs = PropertyValidator.TryParseColor(element.StrokeGradientStartColor, out var gsc) ? gsc : Colors.Gray;
                    //var ge = PropertyValidator.TryParseColor(element.StrokeGradientEndColor, out var gec) ? gec : Colors.Gray;
                    //sp.Stroke = PathHelper.CreateSimpleGradientBrush(gs, ge, element.StrokeGradientAngle, element.StrokeOpacity);
                    break;
                case Models.StrokeFillType.Image:
                    //if (!string.IsNullOrEmpty(element.StrokeImage))
                    //{
                    //    sp.Stroke = PathHelper.CreateImageBrush(element.StrokeImage, element.StrokeImageStretch,
                    //        element.StrokeImageTileMode, element.BackgroundFill.ImageAlignmentX, element.BackgroundFill.ImageAlignmentY, element.StrokeOpacity);
                    //}
                    //else
                    //    sp.Stroke = PropertyValidator.CreateColorBrush(element.StrokeColor);
                    break;
            }

            this.BorderThickness = new Thickness(0);
        }

        private void EnsureStrokePaths(int count)
        {
            while (_strokePaths.Count < count)
            {
                var sp = new Path();
                if (_contentPanel != null)
                    _contentPanel.Children.Add(sp);
                else if (this.Child is Panel panel)
                    panel.Children.Add(sp);
                sp.IsVisible = false;
                _strokePaths.Add(sp);
            }
            for (int i = count; i < _strokePaths.Count; i++)
                _strokePaths[i].IsVisible = false;
        }

        private void RestoreOriginalBorder(CanvasElement element)
        {
            if (element.BorderWidth > 0 && !string.IsNullOrEmpty(element.BorderColor))
            {
                this.BorderBrush = PropertyValidator.CreateColorBrush(element.BorderColor);
                this.BorderThickness = new Thickness(element.BorderWidth);
            }
            else
            {
                this.BorderThickness = new Thickness(0);
            }
            this.CornerRadius = new CornerRadius(element.CornerRadius);
        }

        private void UpdateStrokePath()
        {
            if (this.Tag is not CanvasElement strokeElement)
                return;

            if (strokeElement.StrokeFills != null && strokeElement.StrokeFills.Count > 0)
            {
                var w = this.Bounds.Width > 0 ? this.Bounds.Width : (strokeElement.Width > 0 ? strokeElement.Width : 100);
                var h = this.Bounds.Height > 0 ? this.Bounds.Height : (strokeElement.Height > 0 ? strokeElement.Height : 100);
                ApplyMultiFillsStroke(strokeElement, w, h);
                return;
            }

            if (_strokePaths.Count == 0 || !_strokePaths[0].IsVisible)
                return;

            var strokeWidth = PathHelper.GetStrokeWidth(strokeElement.StrokeWidthMode,
                strokeElement.StrokeWidth, strokeElement.StrokeWidthLeft, strokeElement.StrokeWidthTop,
                strokeElement.StrokeWidthRight, strokeElement.StrokeWidthBottom);
            PathHelper.ConfigurePathGeometry(_strokePaths[0], this.Width, this.Height, strokeWidth, strokeElement.CornerRadius);
        }

        private static Avalonia.Media.PenLineJoin ConvertLineJoin(StrokeJoin join) => join switch
        {
            StrokeJoin.Miter => Avalonia.Media.PenLineJoin.Miter,
            StrokeJoin.Round => Avalonia.Media.PenLineJoin.Round,
            StrokeJoin.Bevel => Avalonia.Media.PenLineJoin.Bevel,
            _ => Avalonia.Media.PenLineJoin.Miter
        };

        private static Avalonia.Media.PenLineCap ConvertLineCap(StrokeEndpoints cap) => cap switch
        {
            StrokeEndpoints.Butt => Avalonia.Media.PenLineCap.Flat,
            StrokeEndpoints.Round => Avalonia.Media.PenLineCap.Round,
            StrokeEndpoints.Square => Avalonia.Media.PenLineCap.Square,
            _ => Avalonia.Media.PenLineCap.Flat
        };

        #endregion

        #region 阴影

        #endregion

    }
}
