using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Globalization;

namespace yuyiAvalonia.UI.Controls
{
    /// <summary>
    /// 刻度尺方向
    /// </summary>
    public enum RulerOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// 刻度尺单位
    /// </summary>
    public enum RulerUnit
    {
        Pixel,
        Point,
        Centimeter,
        Millimeter,
        Inch
    }

    /// <summary>
    /// 位置刻度尺控件 - 绑定到目标控件的X或Y坐标
    /// 支持不同尺寸单位
    /// </summary>
    public class PositionRuler : Control
    {
        #region 依赖属性

        public static readonly StyledProperty<Control?> TargetControlProperty =
            AvaloniaProperty.Register<PositionRuler, Control?>(nameof(TargetControl), null);

        public Control? TargetControl
        {
            get => GetValue(TargetControlProperty);
            set => SetValue(TargetControlProperty, value);
        }

        public static readonly StyledProperty<RulerOrientation> OrientationProperty =
            AvaloniaProperty.Register<PositionRuler, RulerOrientation>(nameof(Orientation), RulerOrientation.Horizontal);

        public RulerOrientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly StyledProperty<double> RulerSizeProperty =
            AvaloniaProperty.Register<PositionRuler, double>(nameof(RulerSize), 24);

        public double RulerSize
        {
            get => GetValue(RulerSizeProperty);
            set => SetValue(RulerSizeProperty, value);
        }

        public static readonly StyledProperty<IBrush> RulerBrushProperty =
            AvaloniaProperty.Register<PositionRuler, IBrush>(nameof(RulerBrush));

        public IBrush RulerBrush
        {
            get => GetValue(RulerBrushProperty);
            set => SetValue(RulerBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> LineBrushProperty =
            AvaloniaProperty.Register<PositionRuler, IBrush>(nameof(LineBrush));

        public IBrush LineBrush
        {
            get => GetValue(LineBrushProperty);
            set => SetValue(LineBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> BackgroundBrushProperty =
            AvaloniaProperty.Register<PositionRuler, IBrush>(nameof(BackgroundBrush));

        public IBrush BackgroundBrush
        {
            get => GetValue(BackgroundBrushProperty);
            set => SetValue(BackgroundBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> MinorTickBrushProperty =
            AvaloniaProperty.Register<PositionRuler, IBrush>(nameof(MinorTickBrush));

        public IBrush MinorTickBrush
        {
            get => GetValue(MinorTickBrushProperty);
            set => SetValue(MinorTickBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> NumberBrushProperty =
            AvaloniaProperty.Register<PositionRuler, IBrush>(nameof(NumberBrush));

        public IBrush NumberBrush
        {
            get => GetValue(NumberBrushProperty);
            set => SetValue(NumberBrushProperty, value);
        }

        public static readonly StyledProperty<bool> ShowNumbersProperty =
            AvaloniaProperty.Register<PositionRuler, bool>(nameof(ShowNumbers), true);

        public bool ShowNumbers
        {
            get => GetValue(ShowNumbersProperty);
            set => SetValue(ShowNumbersProperty, value);
        }

        public static readonly StyledProperty<double> TickIntervalProperty =
            AvaloniaProperty.Register<PositionRuler, double>(nameof(TickInterval), 10);

        public double TickInterval
        {
            get => GetValue(TickIntervalProperty);
            set => SetValue(TickIntervalProperty, value);
        }

        public static readonly StyledProperty<int> MajorTickIntervalProperty =
            AvaloniaProperty.Register<PositionRuler, int>(nameof(MajorTickInterval), 10);

        public int MajorTickInterval
        {
            get => GetValue(MajorTickIntervalProperty);
            set => SetValue(MajorTickIntervalProperty, value);
        }

        /// <summary>
        /// 起始偏移量 - 控制刻度0点相对于控件起始位置的实际像素偏移
        /// 例如：ElementsCanvas在(50,50)，则StartOffset设为50，刻度从0开始
        /// </summary>
        public static readonly StyledProperty<double> StartOffsetProperty =
            AvaloniaProperty.Register<PositionRuler, double>(nameof(StartOffset), 0);

        public double StartOffset
        {
            get => GetValue(StartOffsetProperty);
            set => SetValue(StartOffsetProperty, value);
        }

        public static readonly StyledProperty<bool> ShowPositionLineProperty =
            AvaloniaProperty.Register<PositionRuler, bool>(nameof(ShowPositionLine), true);

        public bool ShowPositionLine
        {
            get => GetValue(ShowPositionLineProperty);
            set => SetValue(ShowPositionLineProperty, value);
        }

        /// <summary>
        /// 单位类型：Pixel, Point, Cm, Mm, Inch
        /// </summary>
        public static readonly StyledProperty<RulerUnit> UnitProperty =
            AvaloniaProperty.Register<PositionRuler, RulerUnit>(nameof(Unit), RulerUnit.Pixel);

        public RulerUnit Unit
        {
            get => GetValue(UnitProperty);
            set => SetValue(UnitProperty, value);
        }

        /// <summary>
        /// 自身坐标
        /// </summary>
        public static readonly StyledProperty<Point> LocationProperty =
            AvaloniaProperty.Register<PositionRuler, Point>(nameof(Location), new Point(0,0));

        public Point Location
        {
            get => GetValue(LocationProperty);
            set => SetValue(LocationProperty, value);
        }

        /// <summary>
        /// 自身坐标
        /// </summary>
        public static readonly StyledProperty<double> ZoomProperty =
            AvaloniaProperty.Register<PositionRuler, double>(nameof(Zoom), 1.0);

        public double Zoom
        {
            get => GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }

        /// <summary>
        /// 自身坐标
        /// </summary>
        public static readonly StyledProperty<Size> SizeProperty =
            AvaloniaProperty.Register<PositionRuler, Size>(nameof(Size), new Size(100,100));

        public Size Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        #endregion

        #region 私有字段

        private double _targetPosition;

        #endregion

        #region 构造方法

        public PositionRuler()
        {
            ClipToBounds = true;
            Focusable = false;
        }

        #endregion

        #region 属性变化处理

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TargetControlProperty)
            {
                OnTargetControlChanged(change.NewValue as Control);
            }
            else if (change.Property == OrientationProperty ||
                     change.Property == RulerSizeProperty ||
                     change.Property == ShowNumbersProperty ||
                     change.Property == TickIntervalProperty ||
                     change.Property == MajorTickIntervalProperty ||
                     change.Property == ShowPositionLineProperty ||
                     change.Property == UnitProperty ||
                     change.Property == StartOffsetProperty ||
                     change.Property == RulerBrushProperty ||
                     change.Property == LineBrushProperty ||
                     change.Property == BackgroundBrushProperty ||
                     change.Property == MinorTickBrushProperty ||
                     change.Property == NumberBrushProperty)
            {
                InvalidateVisual();
            }
        }

        private void OnTargetControlChanged(Control? control)
        {
            // StartOffset 现在由外部直接设置，这里只处理 ShowPositionLine 的位置更新
            InvalidateVisual();
        }

        #endregion

        #region 布局

        protected override Size MeasureOverride(Size availableSize)
        {
            var width = double.IsInfinity( availableSize.Width)? 0 : availableSize.Width;
            var height = double.IsInfinity( availableSize.Height)? 0 : availableSize.Height;

            double desiredWidth = Orientation == RulerOrientation.Horizontal
                ? Math.Max(100, width)
                : RulerSize;

            double desiredHeight = Orientation == RulerOrientation.Horizontal
                ? RulerSize
                : Math.Max(100, height);

            return new Size(desiredWidth, desiredHeight);
        }

        public override void Render(DrawingContext context)
        {
            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);

            var bgBrush = BackgroundBrush ?? new SolidColorBrush(Color.Parse("#2D2D2D"));
            context.FillRectangle(bgBrush, bounds);
            DrawTicks(context);

            if (ShowPositionLine)
            {
                DrawPositionLine(context);
            }
        }

        private void DrawTicks(DrawingContext context)
        {
            double size = Orientation == RulerOrientation.Horizontal ? Size.Width : Size.Height;
            double location = Orientation == RulerOrientation.Horizontal ? Location.X : Location.Y;
            double tickInterval = (TickInterval * Zoom);
            int majorInterval = MajorTickInterval;

            // 零刻度位置
            double zeroPosition = StartOffset - location;

            // 计算刻度数量（基于实际尺寸）
            double endPosition = size ;
            int displayValue = 0;

            for (double pos = zeroPosition; pos >= 0; pos -= tickInterval)
            {
                DrawTick(context, majorInterval, displayValue, pos);

                displayValue -= (int)TickInterval;
            }

            displayValue = 0;

            for (double pos = zeroPosition; pos <= endPosition; pos += tickInterval)
            {
                DrawTick(context, majorInterval, displayValue, pos);

                displayValue += (int)TickInterval;
            }
        }

        private void DrawTick(DrawingContext context, int majorInterval, int displayValue, double pos)
        {
            double displayPos = pos;
            int tickIndex = (int)(displayValue / TickInterval);
            bool isMajor = tickIndex % majorInterval == 0;
            double tickLength = isMajor ? RulerSize * 0.4 : RulerSize * 0.2;
            Point start, end;
            if (Orientation == RulerOrientation.Horizontal)
            {
                start = new Point(displayPos, 0);
                end = new Point(displayPos, tickLength);
            }
            else
            {
                start = new Point(0, displayPos);
                end = new Point(tickLength, displayPos);
            }

            var pen = new Pen(isMajor ? LineBrush : (MinorTickBrush ?? new SolidColorBrush(Color.Parse("#707070"))), isMajor ? 1.3 : 0.5);

            context.DrawLine(pen, start, end);

            if (isMajor && ShowNumbers)
            {
                DrawTickNumber(context, displayPos, tickLength, (int)displayValue);
            }
        }

        private void DrawTickNumber(DrawingContext context, double displayPosition, double tickLength,int displayValue)
        {
            double size = Orientation == RulerOrientation.Horizontal ? Size.Width : Size.Height;
            // 显示实际的刻度值（不是显示位置）
            double actualValue = displayPosition- StartOffset ;
            string text = displayValue.ToString();

            // Avalonia 11 FormattedText - 使用完整构造函数
            var formattedText = new FormattedText(
                text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                9,
                NumberBrush ?? new SolidColorBrush(Color.Parse("#AAAAAA")));


            double textWidth = formattedText.Width;
            double textHeight = formattedText.Height;

            Point textPos;
            if (Orientation == RulerOrientation.Horizontal)
            {
                textPos = new Point(displayPosition - textWidth / 2, tickLength + 2);
            }
            else
            {
                textPos = new Point(tickLength + 2, displayPosition - textHeight / 2);
            }

            //// 边界检查
            //if (textPos.X >= 0 && textPos.X + textWidth <= Bounds.Width &&
            //    textPos.Y >= 0 && textPos.Y + textHeight <= Bounds.Height)
            //{
            //    context.DrawText(formattedText, textPos);
            //}
            // 旋转 -90 度（逆时针），实现竖排
            if(Orientation == RulerOrientation.Vertical)
            {
                using (context.PushTransform(Matrix.CreateTranslation(textPos)))
                using (context.PushTransform(Matrix.CreateRotation(-Math.PI / 2)))
                {
                    // 绘制文字（原点已修正）
                    context.DrawText(formattedText, new Point(0, 0));
                }
            }
            else
            {
                context.DrawText(formattedText, textPos);
            }
        }

        private void DrawPositionLine(DrawingContext context)
        {
            if (TargetControl == null) return;

            double position = Orientation == RulerOrientation.Horizontal
                ? TargetControl.Bounds.X
                : TargetControl.Bounds.Y;

            Point start, end;
            if (Orientation == RulerOrientation.Horizontal)
            {
                start = new Point(position, 0);
                end = new Point(position, Bounds.Height);
            }
            else
            {
                start = new Point(0, position);
                end = new Point(Bounds.Width, position);
            }

            var pen = new Pen(RulerBrush, 1.5);
            context.DrawLine(pen, start, end);

            DrawPositionLabel(context, position);
        }

        private void DrawPositionLabel(DrawingContext context, double position)
        {
            double displayValue = ConvertToUnit(position);
            string unitSuffix = GetUnitSuffix();
            string labelText = displayValue.ToString("F1") + unitSuffix;

            // Avalonia 11 FormattedText - 使用完整构造函数
            var formattedText = new FormattedText(
                labelText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                10,
                Brushes.White);

            double labelWidth = Math.Max(formattedText.Width + 4, 40);
            double labelHeight = Math.Max(formattedText.Height + 2, 14);

            Point labelPos;
            if (Orientation == RulerOrientation.Horizontal)
            {
                double x = position + 4;
                if (x + labelWidth > Bounds.Width) x = position - labelWidth - 4;
                labelPos = new Point(x, (Bounds.Height - labelHeight) / 2);
            }
            else
            {
                double y = position + 4;
                if (y + labelHeight > Bounds.Height) y = position - labelHeight - 4;
                labelPos = new Point((Bounds.Width - labelWidth) / 2, y);
            }

            var bgBrush = new SolidColorBrush(Color.Parse("#4A8FD9"));
            context.FillRectangle(bgBrush, new Rect(labelPos.X - 2, labelPos.Y - 1, labelWidth, labelHeight));
            context.DrawText(formattedText, labelPos);
        }

        #endregion

        #region 单位转换辅助方法

        /// <summary>
        /// 将像素值转换为当前单位
        /// </summary>
        private double ConvertToUnit(double pixelValue)
        {
            return Unit switch
            {
                RulerUnit.Pixel => pixelValue,
                RulerUnit.Point => pixelValue * 0.75, // 1pt = 0.75px (96dpi下)
                RulerUnit.Centimeter => pixelValue / 37.795275591, // 1cm ≈ 37.8px (96dpi下)
                RulerUnit.Millimeter => pixelValue / 3.7795275591, // 1mm ≈ 3.78px
                RulerUnit.Inch => pixelValue / 96, // 1in = 96px (96dpi下)
                _ => pixelValue
            };
        }

        /// <summary>
        /// 格式化数字显示
        /// </summary>
        private string FormatNumber(double pixelValue)
        {
            double displayValue = ConvertToUnit(pixelValue);
            return Unit switch
            {
                RulerUnit.Pixel => ((int)pixelValue).ToString(),
                RulerUnit.Point => displayValue.ToString("F1") + "pt",
                RulerUnit.Centimeter => displayValue.ToString("F2") + "cm",
                RulerUnit.Millimeter => displayValue.ToString("F1") + "mm",
                RulerUnit.Inch => displayValue.ToString("F2") + "in",
                _ => ((int)pixelValue).ToString()
            };
        }

        /// <summary>
        /// 获取单位后缀
        /// </summary>
        private string GetUnitSuffix()
        {
            return Unit switch
            {
                RulerUnit.Pixel => "px",
                RulerUnit.Point => "pt",
                RulerUnit.Centimeter => "cm",
                RulerUnit.Millimeter => "mm",
                RulerUnit.Inch => "in",
                _ => "px"
            };
        }

        #endregion

        #region 公开方法

        public void SetTargetPosition(double position)
        {
            _targetPosition = position;
            InvalidateVisual();
        }

        public double GetTargetPosition() => _targetPosition;

        #endregion
    }
}
