using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using SkiaSharp;
using yuyiAvalonia.ImageEdit.Models;

namespace yuyiAvalonia.ImageEdit.Controls;

/// <summary>
/// 自定义富文本输入控件
/// 使用隐藏的 TextBox 捕获 IME 输入，自定义渲染显示
/// 支持按行高渲染，光标位置基于行高计算
/// </summary>
public class RichTextInputControl : Decorator
{
    // 公开的 Text 属性，用于数据绑定
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<RichTextInputControl, string>(nameof(Text), string.Empty);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<RichTextInputControl, double>(nameof(FontSize), 24);

    /// <summary>
    /// 行高属性，根据字号自动计算或手动设置
    /// </summary>
    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<RichTextInputControl, double>(nameof(LineHeight), 0);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<RichTextInputControl, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<RichTextInputControl, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<RichTextInputControl, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    public static readonly StyledProperty<FontStretch> FontStretchProperty =
        AvaloniaProperty.Register<RichTextInputControl, FontStretch>(nameof(FontStretch), FontStretch.Normal);

    public static readonly StyledProperty<SolidColorBrush> ForegroundProperty =
        AvaloniaProperty.Register<RichTextInputControl, SolidColorBrush>(nameof(Foreground), new SolidColorBrush(ColorValues.Black));

    public static readonly StyledProperty<IImmutableSolidColorBrush> BorderBrushProperty =
        AvaloniaProperty.Register<RichTextInputControl, IImmutableSolidColorBrush>(nameof(BorderBrush),
            new ImmutableSolidColorBrush(Color.Parse("#111111")));

    public static readonly StyledProperty<double> BorderThicknessProperty =
        AvaloniaProperty.Register<RichTextInputControl, double>(nameof(BorderThickness), 2);

    /// <summary>
    /// 缩放级别，用于渲染时调整尺寸
    /// </summary>
    public static readonly StyledProperty<double> ZoomLevelProperty =
        AvaloniaProperty.Register<RichTextInputControl, double>(nameof(ZoomLevel), 1.0);

    /// <summary>
    /// 行高乘数，用于计算行高（字号 * LineHeightMultiplier）
    /// </summary>
    public static readonly StyledProperty<double> LineHeightMultiplierProperty =
        AvaloniaProperty.Register<RichTextInputControl, double>(nameof(LineHeightMultiplier), 1.4);

    public double ZoomLevel
    {
        get => GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set
        {
            SetValue(FontSizeProperty, value);
            // 当字号变化时，自动更新行高（如果行高是自动模式）
            if (LineHeight <= 0)
            {
                UpdateLineHeight();
            }
            //ozj:添加字体尺寸
            _hiddenInput.FontSize = FontSize * ZoomLevel;
        }
    }

    /// <summary>
    /// 行高，如果为0则自动根据字号计算
    /// </summary>
    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set
        {
            SetValue(LineHeightProperty, value);
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 行高乘数
    /// </summary>
    public double LineHeightMultiplier
    {
        get => GetValue(LineHeightMultiplierProperty);
        set
        {
            SetValue(LineHeightMultiplierProperty, value);
            UpdateLineHeight();
        }
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set
        {
            SetValue(FontFamilyProperty, value);
            // ozj:同步字体到隐藏输入框
            if (_hiddenInput != null)
            {
                _hiddenInput.FontFamily = value;
            }
        }
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set
        {
            SetValue(FontWeightProperty, value);
            // ozj:同步字重到隐藏输入框
            if (_hiddenInput != null)
            {
                _hiddenInput.FontWeight = value;
            }
        }
    }

        public FontStyle FontStyle
        {
            get => GetValue(FontStyleProperty);
            set
            {
                SetValue(FontStyleProperty, value);
                // ozj:同步字体样式到隐藏输入框
                if (_hiddenInput != null)
                {
                    _hiddenInput.FontStyle = value;
                }
            }
        }

        public FontStretch FontStretch
        {
            get => GetValue(FontStretchProperty);
            set
            {
                SetValue(FontStretchProperty, value);
                // ozj:同步字体拉伸到隐藏输入框
                if (_hiddenInput != null)
                {
                    _hiddenInput.FontStretch = value;
                }
            }
        }

    public SolidColorBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set
        {
            SetValue(ForegroundProperty, value);
            // ozj:同步字体拉伸到隐藏输入框
            if (_hiddenInput != null)
            {
                _hiddenInput.Foreground = value;
            }
        }
    }

    public IImmutableSolidColorBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    // 隐藏的 TextBox 用于捕获 IME 输入
    private TextBox? _hiddenInput;

    private bool _showCursor = true;
    private DispatcherTimer? _cursorTimer;

    // 缓存当前行高
    private double _currentLineHeight = 0;

    public event EventHandler MyLostFocusEvent;

    public RichTextInputControl()
    {
        Focusable = true;
        MinWidth = 80;
        MinHeight = 10;
      

        // 监听属性变化，触发重新测量和渲染
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == FontSizeProperty || e.Property == LineHeightMultiplierProperty)
            {
                UpdateLineHeight();
                InvalidateMeasure();
                InvalidateVisual();
            }
        };

        // 创建隐藏的 TextBox 来捕获 IME 输入
        _hiddenInput = new TextBox
        {
            // 不能 IsVisible=false：不可见会脱离布局/无法进入 IME 管线
            IsVisible = true,
            Opacity = 0.01, // 接近完全透明但 IME 仍能识别 //2026.03.22
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = Brushes.Transparent,
            Foreground = Brushes.Black,           //2026.03.22
            //Foreground = Brushes.Transparent, //2026.03.22
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            AcceptsReturn = true, // 启用回车换行
            // ozj:同步外部字体设置
            FontFamily = FontFamily,
            FontSize = FontSize * ZoomLevel,
            FontWeight = FontWeight,
            FontStyle = FontStyle,
            FontStretch = FontStretch,
        };
        

        // 必须把 TextBox 放到视觉树里，IME 才能工作
        Child = _hiddenInput;
        _hiddenInput.LostFocus += _hiddenInput_LostFocus;
        _hiddenInput.KeyDown += OnHiddenKeyDown;

        // 订阅隐藏 TextBox 的文本变化事件
        _hiddenInput.TextChanged += OnHiddenTextChanged;

        // 使用属性变化监听光标位置
        _hiddenInput.PropertyChanged += (s, e) =>
        {
            if (e.Property == TextBox.SelectionStartProperty ||
                e.Property == TextBox.SelectionEndProperty)
            {
                InvalidateVisual();
            }
        };


        // 初始化行高
        UpdateLineHeight();
    }

    /// <summary>
    /// 更新行高
    /// </summary>
    private void UpdateLineHeight()
    {
        if (LineHeight > 0)
        {
            _currentLineHeight = LineHeight;
        }
        else
        {
            // 根据字号和行高乘数自动计算行高
            _currentLineHeight = FontSize * LineHeightMultiplier;
        }
    }

    /// <summary>
    /// 获取当前行高
    /// </summary>
    public double GetLineHeight()
    {
        return _currentLineHeight;
    }

    private int GetCaretIndexFromHiddenInput()
    {
        // 获取隐藏 TextBox 中的实际光标位置
        if (_hiddenInput != null)
        {
            return _hiddenInput.SelectionStart;
        }
        return Text?.Length ?? 0;
    }

    private void OnHiddenKeyDown(object? sender, KeyEventArgs e)
    {
        // 回车键需要在 TextBox 中被正确处理
        if (e.Key == Key.Enter && _hiddenInput != null)
        {
            // 使用延迟确保在 TextBox 处理完回车后重新获取焦点
            Dispatcher.UIThread.Post(() =>
            {
                if (_hiddenInput != null && !_hiddenInput.IsFocused)
                {
                    _hiddenInput.Focus();
                }
            }, DispatcherPriority.Loaded);
        }
    }

    private void _hiddenInput_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        MyLostFocusEvent?.Invoke(this, e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty && _hiddenInput != null)
        {
            var newText = change.GetNewValue<string>() ?? string.Empty;
            if (_hiddenInput.Text != newText)
                _hiddenInput.Text = newText;

            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    private void OnHiddenTextChanged(object? sender, TextChangedEventArgs e)
    {
        // 将隐藏 TextBox 的文本同步到当前控件
        if (_hiddenInput != null && _hiddenInput.Text != Text)
        {
            Text = _hiddenInput.Text ?? string.Empty;
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        StartCursorTimer();

        // 控件附加到视觉树后，如果已有焦点则立即转移到隐藏 TextBox
        // 否则等待焦点事件触发
        if (IsFocused)
        {
            FocusHiddenInput();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        StopCursorTimer();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        // 将焦点转移到隐藏的 TextBox
        FocusHiddenInput();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // 确保按键时隐藏 TextBox 有焦点
        if (_hiddenInput != null && !_hiddenInput.IsFocused)
        {
            _hiddenInput.Focus();
        }

        base.OnKeyDown(e);

        // 回车键后确保焦点保持在隐藏 TextBox
        if (e.Key == Key.Enter)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_hiddenInput != null)
                {
                    _hiddenInput.Focus();
                }
            }, DispatcherPriority.Loaded);
        }
    }

    private void FocusHiddenInput()
    {
        // 使用延迟确保 TextBox 已完全初始化并准备好接收 IME
        Dispatcher.UIThread.Post(() =>
        {
            if (_hiddenInput != null)
            {
                if (!_hiddenInput.IsFocused)
                {
                    _hiddenInput.Focus();
                }
                // 将光标移动到末尾
                _hiddenInput.CaretIndex = _hiddenInput.Text?.Length ?? 0;
            }
        }, DispatcherPriority.Loaded);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        Focus();
        e.Handled = true;
    }

    private void StartCursorTimer()
    {
        _cursorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _cursorTimer.Tick += (s, e) =>
        {
            _showCursor = !_showCursor;
            InvalidateVisual();
        };
        _cursorTimer.Start();
    }

    private void StopCursorTimer()
    {
        _cursorTimer?.Stop();
        _cursorTimer = null;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // 确保行高是最新的
        UpdateLineHeight();

        // 获取缩放级别
        double scale = ZoomLevel > 0 ? ZoomLevel : 1.0;

        // 确保行高能容纳字号（使用原始字号）
        double effectiveLineHeight = _currentLineHeight;
        if (effectiveLineHeight < FontSize)
        {
            effectiveLineHeight = FontSize * 1.2;
        }

        // 让子 TextBox 使用父控件的全部可用尺寸
        if (_hiddenInput != null)
        {
            _hiddenInput.Measure(availableSize);
        }

        //var padding = 5.0;
        var padding = 0.0;
        var textWidth = MinWidth;
        double textHeight;

        if (!string.IsNullOrEmpty(Text))
        {
            // 使用可用宽度计算文本换行
            var maxWidth = availableSize.Width > 0 && availableSize.Width < double.PositiveInfinity
                ? availableSize.Width - padding * scale
                : double.PositiveInfinity;

            // 获取所有行
            var lines = Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineCount = lines.Length;

            // 计算最大行宽（使用缩放后的字号）
            double maxLineWidth = MinWidth;
            double scaledFontSize = FontSize * scale;
            foreach (var line in lines)
            {
                var formattedText = new FormattedText(
                    line,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyle, FontWeight),
                    scaledFontSize,
                    Foreground ?? new SolidColorBrush(ColorValues.Black));

                maxLineWidth = Math.Max(maxLineWidth, formattedText.Width);
            }

            textWidth = maxLineWidth + padding * scale;

            // 使用行高计算总高度（使用缩放后的行高）
            double scaledLineHeight = effectiveLineHeight * scale;
            textHeight = lineCount * scaledLineHeight;
        }
        else
        {
            // 空文本时至少显示一行
            textHeight = effectiveLineHeight * scale;
        }

        var width = Math.Max(MinWidth, textWidth);
        var height = Math.Max(MinHeight, Math.Max(textHeight, effectiveLineHeight * scale + 4 * scale));

        Width = width+10;
        Height = height;

        return new Size(width, height);
    }

    public override void Render(DrawingContext context)
    {
        // 确保行高是最新的
        UpdateLineHeight();

        double scale = ZoomLevel > 0 ? ZoomLevel : 1.0;
        double scaledPadding = 0 * scale;
        double scaledBorderThickness = BorderThickness * scale;
        double scaledFontSize = FontSize * scale;  // 字号需要缩放

        double scaledLineHeight = _hiddenInput.DesiredSize.Height / _hiddenInput.GetLineCount();

        // 计算边界
        var bounds = new Rect(0, 0, Bounds.Size.Width, Bounds.Size.Height);

        // 绘制边框
        if (BorderThickness > 0 && BorderBrush is ImmutableSolidColorBrush solidColorBrush)
        {
            var pen = new ImmutablePen(
                solidColorBrush.Color.ToUInt32(),
                scaledBorderThickness,
                new ImmutableDashStyle(new double[] { 4 * scale, 4 * scale }, 0),
                PenLineCap.Round);
            context.DrawRectangle(null, pen, bounds);
        }

        // 绘制文字 - 按行高渲染
        if (!string.IsNullOrEmpty(Text))
        {
            var lines = Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var foregroundBrush = Foreground ?? new SolidColorBrush(ColorValues.Black);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;

                //var formattedText = new FormattedText(
                //    line,
                //    System.Globalization.CultureInfo.CurrentCulture,
                //    FlowDirection.LeftToRight,
                //    new Typeface(FontFamily, FontStyle, FontWeight),
                //    scaledFontSize,
                //    foregroundBrush);

                // Y位置按行高计算，确保每行文字垂直居中于行高范围内
                //double yPos = scaledPadding + i * scaledLineHeight + (scaledLineHeight - scaledFontSize) / 2;
                double yPos = 0 + i * scaledLineHeight;

                //context.DrawText(formattedText, new Point(scaledPadding, yPos));

                //---------
                var textLayout = new TextLayout(
                    text: line,
                    typeface: new Typeface(FontFamily, FontStyle, FontWeight),
                    fontSize: FontSize * ZoomLevel,
                    foreground: foregroundBrush,
                    textAlignment: TextAlignment.Left,
                    textWrapping: TextWrapping.NoWrap,
                    textTrimming: TextTrimming.None,
                    textDecorations: null,
                    flowDirection: FlowDirection.LeftToRight,
                    lineHeight: 0,
                    letterSpacing: 0, 
                    maxWidth: double.PositiveInfinity,
                    maxHeight: double.PositiveInfinity
                  
                );

                //在指定位置绘制
                textLayout.Draw(context, new Point(scaledPadding, yPos));


            }
        }

        // 绘制光标
        if (_showCursor && Bounds.Width > 0 && Bounds.Height > 0)
        {
            var cursorX = scaledPadding;
            var cursorY = scaledPadding;

            // 获取实际的光标位置
            int caretIndex = GetCaretIndexFromHiddenInput();
            var text = Text ?? string.Empty;

            if (caretIndex >= 0)
            {
                // 使用行高计算光标位置
                var (cursorPosX, cursorPosY) = CalculateCaretPositionWithLineHeight(text, caretIndex, scaledPadding, scaledFontSize, scaledLineHeight);
                cursorX = cursorPosX;
                cursorY = cursorPosY;
            }

            int offsetY = 2;

            var cursorBrush = new ImmutableSolidColorBrush(Colors.Black);
            var cursorPen = new ImmutablePen(cursorBrush.Color.ToUInt32(), scale);

            // 光标高度使用行高，确保光标贯穿整行
            double cursorHeight = scaledLineHeight - offsetY * 2;
            context.DrawLine(cursorPen,
                new Point(cursorX, cursorY - offsetY),
                new Point(cursorX, cursorY - offsetY + cursorHeight));
        }
    }


    /// <summary>
    /// 计算指定文本在给定缩放级别下的渲染尺寸
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="scale">缩放级别</param>
    /// <returns>渲染尺寸</returns>
    public Size CalculateRenderedSize(string text, double scale)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new Size(0, 0);
        }

        double scaledFontSize = FontSize * scale;
        double scaledLineHeight = _currentLineHeight * scale;
        if (scaledLineHeight < scaledFontSize)
        {
            scaledLineHeight = scaledFontSize * 1.2;
        }

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        double maxWidth = 0;
        double scaledPadding = 5 * scale;

        foreach (var line in lines)
        {
            var formattedText = new FormattedText(
                line,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight),
                scaledFontSize,
                Foreground ?? new SolidColorBrush(ColorValues.Black));
            maxWidth = Math.Max(maxWidth, formattedText.Width);
        }

        double width = maxWidth + scaledPadding * 2;
        double height = lines.Length * scaledLineHeight;

        return new Size(width, height);
    }

    /// <summary>
    /// 获取当前控件在指定缩放级别下的行高
    /// </summary>
    public double GetScaledLineHeight(double scale)
    {
        double scaledFontSize = FontSize * scale;
        double scaledLineHeight = _currentLineHeight * scale;
        if (scaledLineHeight < scaledFontSize)
        {
            scaledLineHeight = scaledFontSize * 1.2;
        }
        return scaledLineHeight;
    }

    /// <summary>
    /// 使用与渲染相同的方式绘制文本到 SkiaSharp Canvas
    /// </summary>
    /// <param name="canvas">SkiaSharp 画布</param>
    /// <param name="text">文本内容</param>
    /// <param name="position">绘制位置（相对于图片的坐标）</param>
    /// <param name="scale">缩放级别</param>
    /// <param name="textColor">文字颜色</param>
    public void DrawTextToCanvas(SKCanvas canvas, string text, SKPoint position, double scale, SKColor textColor)
    {
        if (string.IsNullOrEmpty(text))
            return;

        double scaledFontSize = FontSize * scale;
        double scaledLineHeight = _currentLineHeight * scale;
        if (scaledLineHeight < scaledFontSize)
        {
            scaledLineHeight = scaledFontSize * 1.2;
        }

        double scaledPadding = 5 * scale;

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        // 创建 SKTypeface
        var skFontStyle = SKFontStyle.Normal;
        if (FontWeight == FontWeight.Bold && FontStyle == FontStyle.Italic)
            skFontStyle = SKFontStyle.BoldItalic;
        else if (FontWeight == FontWeight.Bold)
            skFontStyle = SKFontStyle.Bold;
        else if (FontStyle == FontStyle.Italic)
            skFontStyle = SKFontStyle.Italic;

        using var typeface = SKTypeface.FromFamilyName(FontFamily.Name, skFontStyle);

        using var font = new SKFont
        {
            Size = (float)scaledFontSize,
            Typeface = typeface
        };

        using var paint = new SKPaint
        {
            Color = textColor,
            IsAntialias = true,
            TextSize = (float)scaledFontSize,
            Typeface = typeface,
            Style = SKPaintStyle.Fill
        };

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line)) continue;

            // Y位置按行高计算，确保每行文字垂直居中于行高范围内
            double yPos = position.Y + scaledPadding + i * scaledLineHeight + (scaledLineHeight - scaledFontSize) / 2 + scaledFontSize;

            canvas.DrawText(line, (float)(position.X + scaledPadding), (float)yPos, font, paint);
        }
    }
    private (double x, double y) CalculateCaretPositionWithLineHeight(string text, int caretIndex, double padding, double fontSize, double lineHeight)
    {
        if (string.IsNullOrEmpty(text) || caretIndex <= 0)
        {
            return (padding, padding);
        }

        // 确保 caretIndex 在有效范围内
        caretIndex = Math.Max(0, Math.Min(caretIndex, text.Length));

        // 找到当前行的起始位置和行号
        int currentLineIndex = 0;
        int lineStartIndex = 0;

        for (int i = 0; i < caretIndex; i++)
        {
            if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    // \r\n 组合，换行到下一行
                    currentLineIndex++;
                    lineStartIndex = i + 2;
                    i++; // 跳过 \n
                }
                else
                {
                    // 单个 \r
                    currentLineIndex++;
                    lineStartIndex = i + 1;
                }
            }
            else if (text[i] == '\n')
            {
                // 单个 \n
                currentLineIndex++;
                lineStartIndex = i + 1;
            }
        }

        // 找到当前行的结束位置
        int lineEndIndex = caretIndex;
        while (lineEndIndex < text.Length)
        {
            if (text[lineEndIndex] == '\r' || text[lineEndIndex] == '\n')
            {
                break;
            }
            lineEndIndex++;
        }

        // 提取当前行中光标前的文本
        int charsInLine = caretIndex - lineStartIndex;
        string textBeforeCaret = "";
        if (charsInLine > 0 && lineStartIndex < text.Length)
        {
            int extractLength = Math.Min(charsInLine, lineEndIndex - lineStartIndex);
            textBeforeCaret = text.Substring(lineStartIndex, Math.Max(0, extractLength));
        }

        // 计算光标X位置
        var formattedTextForLine = new FormattedText(
            textBeforeCaret,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight),
            fontSize,
            Foreground ?? new SolidColorBrush(ColorValues.Black));

        double cursorX = padding + formattedTextForLine.Width;

        // 使用行高计算 Y 位置，并加上文字垂直居中的偏移
        // 文字在行高范围内是垂直居中的，所以光标也要居中
        double cursorY = padding + currentLineIndex * lineHeight + (lineHeight - fontSize) / 2;

        return (cursorX, cursorY);
    }

    /// <summary>
    /// 兼容旧版本的方法
    /// </summary>
    private (double x, double y) CalculateCaretPosition(string text, int caretIndex, double padding, double fontSize)
    {
        return CalculateCaretPositionWithLineHeight(text, caretIndex, padding, fontSize, _currentLineHeight);
    }
}
