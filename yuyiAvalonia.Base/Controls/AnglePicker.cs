using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace yuyiAvalonia.Base.Controls
{
    public class AnglePicker : Control
    {
        public static readonly StyledProperty<double> AngleProperty =
            AvaloniaProperty.Register<AnglePicker, double>(nameof(Angle), 0);

        public double Angle
        {
            get => GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        public static readonly StyledProperty<double> SizeProperty =
            AvaloniaProperty.Register<AnglePicker, double>(nameof(Size), 100);

        public double Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly StyledProperty<IBrush> CircleBrushProperty =
            AvaloniaProperty.Register<AnglePicker, IBrush>(nameof(CircleBrush), new SolidColorBrush(Color.Parse("#333333")));

        public IBrush CircleBrush
        {
            get => GetValue(CircleBrushProperty);
            set => SetValue(CircleBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> LineBrushProperty =
            AvaloniaProperty.Register<AnglePicker, IBrush>(nameof(LineBrush), new SolidColorBrush(Color.Parse("#07C160")));

        public IBrush LineBrush
        {
            get => GetValue(LineBrushProperty);
            set => SetValue(LineBrushProperty, value);
        }

        public static readonly StyledProperty<IBrush> HandleBrushProperty =
            AvaloniaProperty.Register<AnglePicker, IBrush>(nameof(HandleBrush), new SolidColorBrush(Colors.White));

        public IBrush HandleBrush
        {
            get => GetValue(HandleBrushProperty);
            set => SetValue(HandleBrushProperty, value);
        }

        private bool _isDragging;

        static AnglePicker()
        {
            AffectsRender<AnglePicker>(AngleProperty, SizeProperty, CircleBrushProperty, LineBrushProperty, HandleBrushProperty);
        }

        public AnglePicker()
        {
            this.Loaded += AnglePicker_Loaded;
        }

        private void AnglePicker_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            object? hoverBrush = null;

            if (this.TryFindResource("ButtonBackgroundPointerOver", this.ActualThemeVariant, out hoverBrush))
            {
                CircleBrush = hoverBrush as IBrush;
                LineBrush = hoverBrush as IBrush;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            _isDragging = true;
            UpdateAngleFromPointer(e);
            e.Pointer.Capture(this);
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _isDragging = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_isDragging)
            {
                UpdateAngleFromPointer(e);
            }
            e.Handled = true;
        }

        private void UpdateAngleFromPointer(PointerEventArgs e)
        {
            var pos = e.GetPosition(this);
            var center = new Point(Size / 2, Size / 2);
            var dx = pos.X - center.X;
            var dy = pos.Y - center.Y;

            var angle = Math.Atan2(dy, dx) * 180 / Math.PI;
            if (angle < 0) angle += 360;

            //Angle = (Math.Round(angle / 15) * 15 ) % 360;
            Angle = (Math.Round(angle / 5) * 5 ) % 360;
        }

        public override void Render(DrawingContext context)
        {
            var radius = Size / 2;
            var center = new Point(radius, radius);

            // 绘制圆形背景
            var circlePen = new Pen(CircleBrush, 2);
            context.DrawEllipse(null, circlePen, new Rect(2, 2, Size - 4, Size - 4));

            // 绘制刻度线
            var tickPen = new Pen(CircleBrush, 1);
            int tickCount = 5;
            int count = 360 / tickCount;
            for (int i = 0; i < count; i++)
            {
                var tickAngle = i * tickCount;
                //var tickAngle = i * 15;
                var rad = tickAngle * Math.PI / 180;
                var innerRadius = radius - (i % 2 == 0 ? 8 : 5);
                var outerRadius = radius - 2;

                var startX = center.X + Math.Cos(rad) * innerRadius;
                var startY = center.Y + Math.Sin(rad) * innerRadius;
                var endX = center.X + Math.Cos(rad) * outerRadius;
                var endY = center.Y + Math.Sin(rad) * outerRadius;

                context.DrawLine(tickPen, new Point(startX, startY), new Point(endX, endY));
            }

            // 绘制角度线
            var angleRad = Angle * Math.PI / 180;
            var lineEndX = center.X + Math.Cos(angleRad) * (radius - 12);
            var lineEndY = center.Y + Math.Sin(angleRad) * (radius - 12);

            var linePen = new Pen(LineBrush, 2);
            context.DrawLine(linePen, center, new Point(lineEndX, lineEndY));

            // 绘制手柄
            var handlePen = new Pen(LineBrush, 2);
            context.DrawEllipse(HandleBrush, handlePen, new Rect(lineEndX - 3, lineEndY - 3, 6, 6));

            // 绘制中心点
            var centerPen = new Pen(LineBrush, 2);
            context.DrawEllipse(null, centerPen, new Rect(center.X - 3, center.Y - 3, 6, 6));

            // 绘制角度文字
            var text = $"{Angle:F0}°";
            var typeface = new Typeface("Arial");
            var fontSize = 11;

            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                LineBrush);

            var textWidth = formattedText.Width;
            var textX = center.X - textWidth / 2;
            var textY = center.Y + radius * 0.4;
            context.DrawText(formattedText, new Point(textX, textY));
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Size, Size);
        }
    }
}
