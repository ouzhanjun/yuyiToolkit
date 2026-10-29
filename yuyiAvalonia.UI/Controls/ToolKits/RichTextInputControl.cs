using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using SkiaSharp;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Controls.Common;

namespace yuyiAvalonia.UI.Controls;

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
    /// 行高乘数，用于计算行高（字号 * LineHeightMultiplier）
    /// </summary>
    public static readonly StyledProperty<double> LineHeightMultiplierProperty =
        AvaloniaProperty.Register<RichTextInputControl, double>(nameof(LineHeightMultiplier), 1.4);

    /// <summary>
    /// 是否显示边框
    /// </summary>
    public static readonly StyledProperty<bool> ShowBorderProperty =
        AvaloniaProperty.Register<RichTextInputControl, bool>(nameof(ShowBorder), true);

    /// <summary>
    /// 文本对齐方式
    /// </summary>
    public static readonly StyledProperty<TextAlignment> TextAlignmentProperty =
        AvaloniaProperty.Register<RichTextInputControl, TextAlignment>(nameof(TextAlignment), TextAlignment.Left);

    /// <summary>
    /// 下划线
    /// </summary>
    public static readonly StyledProperty<bool> IsUnderlineProperty =
        AvaloniaProperty.Register<RichTextInputControl, bool>(nameof(IsUnderline), false);

    /// <summary>
    /// 删除线
    /// </summary>
    public static readonly StyledProperty<bool> IsStrikethroughProperty =
        AvaloniaProperty.Register<RichTextInputControl, bool>(nameof(IsStrikethrough), false);

    /// <summary>
    /// 垂直内容对齐方式
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<RichTextInputControl, VerticalAlignment>(nameof(VerticalContentAlignment), VerticalAlignment.Top);

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
            // 同步字体尺寸到隐藏输入框
            if (_hiddenInput != null)
            {
                _hiddenInput.FontSize = value;
            }
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

    public bool ShowBorder
    {
        get => GetValue(ShowBorderProperty);
        set
        {
            SetValue(ShowBorderProperty, value);
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 文本对齐方式
    /// </summary>
    public TextAlignment TextAlignment
    {
        get => GetValue(TextAlignmentProperty);
        set
        {
            SetValue(TextAlignmentProperty, value);
            if (_hiddenInput != null)
            {
                _hiddenInput.TextAlignment = value;
            }
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 下划线
    /// </summary>
    public bool IsUnderline
    {
        get => GetValue(IsUnderlineProperty);
        set
        {
            SetValue(IsUnderlineProperty, value);
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 删除线
    /// </summary>
    public bool IsStrikethrough
    {
        get => GetValue(IsStrikethroughProperty);
        set
        {
            SetValue(IsStrikethroughProperty, value);
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 垂直内容对齐方式
    /// </summary>
    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set
        {
            SetValue(VerticalContentAlignmentProperty, value);
            if(this._hiddenInput!=null)
            {
                this._hiddenInput.VerticalContentAlignment = value;
            }
            InvalidateVisual();
        }
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set
        {
            SetValue(FontFamilyProperty, value);
            if (_hiddenInput != null)
            {
                _hiddenInput.FontFamily = value;
            }
            InvalidateVisual();
        }
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set
        {
            SetValue(FontWeightProperty, value);
            if (_hiddenInput != null)
            {
                _hiddenInput.FontWeight = value;
            }
            InvalidateVisual();
        }
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set
        {
            SetValue(FontStyleProperty, value);
            if (_hiddenInput != null)
            {
                _hiddenInput.FontStyle = value;
            }
            InvalidateVisual();
        }
    }

    public FontStretch FontStretch
    {
        get => GetValue(FontStretchProperty);
        set
        {
            SetValue(FontStretchProperty, value);
            if (_hiddenInput != null)
            {
                _hiddenInput.FontStretch = value;
            }
            InvalidateVisual();
        }
    }

    public SolidColorBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set
        {
            SetValue(ForegroundProperty, value);
            if (_hiddenInput != null)
            {
                _hiddenInput.Foreground = value;
            }
            InvalidateVisual();
        }
    }

    public IImmutableSolidColorBrush BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set
        {
            SetValue(BorderBrushProperty, value);
            InvalidateVisual();
        }
    }

    public double BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set
        {
            SetValue(BorderThicknessProperty, value);
            InvalidateVisual();
        }
    }

    // 隐藏的 TextBox 用于捕获 IME 输入
    private TextBox? _hiddenInput;

    private bool _showCursor = true;
    private DispatcherTimer? _cursorTimer;

    /// <summary>
    /// 是否处于编辑状态（影响光标显示）
    /// </summary>
    private bool _isEditing = false;
    public bool IsEditing
    {
        get => _isEditing;
        set
        {
            _isEditing = value;
            _hiddenInput!.IsEnabled = value;
            _hiddenInput.Focusable = value;
            _isEditing = value;
            _showCursor = value; 
            if (!value)
            {
                StopCursorTimer();
            }
            else
            {
                StartCursorTimer();
            }
            InvalidateVisual();
        }
    }

    // 缓存当前行高
    private double _currentLineHeight = 0;

    public event EventHandler LostEditFocusEvent;
    public event EventHandler GotEditFocusEvent;

    public RichTextInputControl()
    {
        Focusable = false;
        MinWidth = 1;
        MinHeight = 1;

        // 监听属性变化，触发重新测量和渲染
        this.PropertyChanged += (s, e) =>
        {
            if (e.Property == FontSizeProperty || e.Property == LineHeightMultiplierProperty)
            {
                UpdateLineHeight();
                InvalidateMeasure();
                InvalidateVisual();
            }
            else if (e.Property == TextAlignmentProperty || e.Property == IsUnderlineProperty || e.Property == IsStrikethroughProperty)
            {
                InvalidateVisual();
            }
        };

        // 创建隐藏的 TextBox 来捕获 IME 输入
        _hiddenInput = new TextBox
        {
            // 不能 IsVisible=false：不可见会脱离布局/无法进入 IME 管线
            IsVisible = true,
            Opacity = 0.01, // 接近完全透明但 IME 仍能识别
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Background = Brushes.Transparent,
            Foreground = Brushes.Black,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            MinWidth = 0,
            MinHeight = 0,
            AcceptsReturn = true,
            TextAlignment = TextAlignment,
            // 同步外部字体设置
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontWeight = FontWeight,
            FontStyle = FontStyle,
            FontStretch = FontStretch,
            // 关闭原生选中高亮，完全由我们自定义渲染
            SelectionBrush = Brushes.Transparent,
        };

        ScrollViewer.SetVerticalScrollBarVisibility(_hiddenInput, ScrollBarVisibility.Disabled);
        ScrollViewer.SetHorizontalScrollBarVisibility(_hiddenInput, ScrollBarVisibility.Disabled);

        // 必须把 TextBox 放到视觉树里，IME 才能工作
        Child = _hiddenInput;

        _hiddenInput.GotFocus += _hiddenInput_GotFocus;
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

        //this.SizeChanged += RichTextInputControl_SizeChanged;

        // 初始化行高
        UpdateLineHeight();
    }


    private void RichTextInputControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (_hiddenInput != null)
        {
            _hiddenInput.Width = this.Bounds.Width;
            _hiddenInput.Height = this.Bounds.Height;
        }
    }

    private void _hiddenInput_GotFocus(object? sender, GotFocusEventArgs e)
    {
        GotEditFocusEvent?.Invoke(this, e);
    }

    private void _hiddenInput_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LostEditFocusEvent?.Invoke(this, e);
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
        if (_hiddenInput != null)
        {
            var newText = _hiddenInput.Text ?? string.Empty;
            if (Text != newText)
            {
                SetValue(TextProperty, newText);
                InvalidateMeasure();
                InvalidateVisual();
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // 只在编辑状态下启动光标定时器
        if (_isEditing)
        {
            StartCursorTimer();
        }

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
    }

    private void StartCursorTimer()
    {
        // 先停止已有的定时器，避免重复启动
        StopCursorTimer();
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

        // 确保行高能容纳字号
        double effectiveLineHeight = _currentLineHeight;
        if (effectiveLineHeight < FontSize)
        {
            effectiveLineHeight = FontSize * 1.2;
        }

        // 使用完整的可用尺寸来测量隐藏的 TextBox
        if (_hiddenInput != null)
        {
            _hiddenInput.Measure(availableSize);
        }

        // 空文本时，使用可用尺寸的宽度（至少要填满容器）
        if (string.IsNullOrEmpty(Text))
        {
            var width = Math.Max(MinWidth, availableSize.Width);
            var height = Math.Max(MinHeight, Math.Max(effectiveLineHeight + 4, availableSize.Height));
            return new Size(width, height);
        }

        // 有文本时，基于文本内容计算
        var lines = Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        int lineCount = lines.Length;

        double maxLineWidth = MinWidth;
        foreach (var line in lines)
        {
            var formattedText = new FormattedText(
                line,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight),
                FontSize,
                Foreground ?? new SolidColorBrush(ColorValues.Black));

            maxLineWidth = Math.Max(maxLineWidth, formattedText.Width);
        }

        double textWidth = maxLineWidth;
        double textHeight = lineCount * effectiveLineHeight;

        var finalWidth = Math.Max(MinWidth, textWidth);
        var finalHeight = Math.Max(MinHeight, Math.Max(textHeight, effectiveLineHeight + 4));

        return new Size(finalWidth, finalHeight);
    }

    public override void Render(DrawingContext context)
    {
        // 确保行高是最新的
        UpdateLineHeight();

        double scaledBorderThickness = ShowBorder ? BorderThickness : 0;

        // 计算边界
        var bounds = new Rect(0, 0, Bounds.Size.Width, Bounds.Size.Height);

        // 绘制边框（可选）
        if (ShowBorder && BorderThickness > 0 && BorderBrush is ImmutableSolidColorBrush solidColorBrush)
        {
            var pen = new ImmutablePen(
                solidColorBrush.Color.ToUInt32(),
                scaledBorderThickness,
                new ImmutableDashStyle(new double[] { 4, 4 }, 0),
                PenLineCap.Round);
            context.DrawRectangle(null, pen, bounds);
        }

        // 获取文本装饰
        TextDecorationCollection? textDecorations = null;
        if (IsUnderline || IsStrikethrough)
        {
            textDecorations = new TextDecorationCollection();
            if (IsUnderline)
            {
                textDecorations.Add(new TextDecoration
                {
                    Location = TextDecorationLocation.Underline
                });
            }
            if (IsStrikethrough)
            {
                textDecorations.Add(new TextDecoration
                {
                    Location = TextDecorationLocation.Strikethrough
                });
            }
        }

        // 计算行高
        double lineHeight = _hiddenInput?.DesiredSize.Height / Math.Max(1, _hiddenInput?.GetLineCount() ?? 1) ?? _currentLineHeight;
        if (lineHeight <= 0)
        {
            lineHeight = _currentLineHeight;
        }

        // 计算垂直对齐偏移
        double verticalAlignmentOffset = 0;
        //if (!string.IsNullOrEmpty(Text))
        {
            var lines = Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineCount = lines.Length;
            double totalTextHeight = lineCount * lineHeight;
            double availableHeight = Bounds.Height;

            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Center:
                    verticalAlignmentOffset = Math.Max(0, (availableHeight - totalTextHeight) / 2);
                    break;
                case VerticalAlignment.Bottom:
                    verticalAlignmentOffset = Math.Max(0, availableHeight - totalTextHeight);
                    break;
                case VerticalAlignment.Stretch:
                    // Stretch 类似于 Top，但会拉伸文本
                    verticalAlignmentOffset = 0;
                    break;
                case VerticalAlignment.Top:
                default:
                    verticalAlignmentOffset = 0;
                    break;
            }
        }

        // 绘制文字 - 按行高渲染
        if (!string.IsNullOrEmpty(Text))
        {
            var lines = Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var foregroundBrush = Foreground ?? new SolidColorBrush(ColorValues.Black);

            var selectionStart = Math.Min(_hiddenInput?.SelectionStart ?? 0, _hiddenInput?.SelectionEnd ?? 0);
            var selectionEnd = Math.Max(_hiddenInput?.SelectionStart ?? 0, _hiddenInput?.SelectionEnd ?? 0);
            bool hasSelection = selectionStart != selectionEnd;

            // 先绘制选区高亮背景
            if (hasSelection)
            {
                RenderSelectionHighlight(context, lines, lineHeight, selectionStart, selectionEnd, verticalAlignmentOffset);
            }

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Y位置按行高计算，加上垂直对齐偏移
                double yPos = verticalAlignmentOffset + i * lineHeight;

                // 计算这一行的全局起始位置
                int lineStart = 0;
                for (int j = 0; j < i; j++)
                {
                    lineStart += lines[j].Length + Environment.NewLine.Length;
                }
                int lineEnd = lineStart + line.Length;

                // 如果选区与当前行有重叠，需要分段绘制
                if (hasSelection && selectionEnd > lineStart && selectionStart < lineEnd)
                {
                    // 计算当前行的水平对齐偏移
                    double lineHorizontalOffset = GetHorizontalOffsetForLine(line);

                    // 分段绘制：未选中部分用原色，选中部分用反差色(白色)
                    int pos = 0;

                    // 未选中的前半部分
                    if (selectionStart > lineStart)
                    {
                        int preLen = Math.Min(selectionStart - lineStart, line.Length);
                        var preText = line.Substring(0, preLen);
                        DrawTextLineAt(context, preText, lineHorizontalOffset, yPos, foregroundBrush, textDecorations);
                        pos = preLen;
                    }

                    // 选中的部分
                    int selStartInLine = Math.Max(0, selectionStart - lineStart);
                    int selEndInLine = Math.Min(line.Length, selectionEnd - lineStart);
                    if (selStartInLine < selEndInLine)
                    {
                        var selText = line.Substring(selStartInLine, selEndInLine - selStartInLine);
                        double selX = lineHorizontalOffset + CalculateTextWidth(line.Substring(0, selStartInLine));
                        DrawTextLineAt(context, selText, selX, yPos, Brushes.White, null);
                        pos = selEndInLine;
                    }

                    // 未选中的后半部分
                    if (pos < line.Length)
                    {
                        var postText = line.Substring(pos);
                        double postX = lineHorizontalOffset + CalculateTextWidth(line.Substring(0, pos));
                        DrawTextLineAt(context, postText, postX, yPos, foregroundBrush, textDecorations);
                    }
                }
                else
                {
                    // 没有选区，正常绘制整行
                    // DrawTextLine 内部会自己加 alignmentOffset，所以传 _x=0 即可
                    DrawTextLine(context, line, 0, yPos, foregroundBrush, textDecorations);
                }
            }
        }
        else if (textDecorations != null)
        {
            // 空文本但有文本装饰时也绘制一行，加上垂直对齐偏移
            // DrawTextLine 内部会自己加 alignmentOffset，所以传 _x=0 即可
            DrawTextLine(context, " ", 0, verticalAlignmentOffset, Foreground ?? new SolidColorBrush(ColorValues.Black), textDecorations);
        }

        // 绘制光标
        if (_showCursor && Bounds.Width > 0 && Bounds.Height > 0)
        {
            int caretIndex = GetCaretIndexFromHiddenInput();
            var text = Text ?? string.Empty;

            if (caretIndex >= 0)
            {
                double cursorX;
                double cursorY;

                // 空文本时，光标位置由 GetHorizontalOffsetForLine 直接计算
                // （GetHorizontalOffsetForLine 对空文本返回基于 Bounds.Width 的对齐偏移）
                if (string.IsNullOrEmpty(text) || caretIndex <= 0)
                {
                    // 对齐方式决定了空文本时光标的水平位置
                    cursorX = GetHorizontalOffsetForLine(string.IsNullOrEmpty(text) ? " " : text);
                    cursorY = 0;
                }
                else
                {
                    // 有文本且光标不在开头：基础位置 + 当前行的水平对齐偏移
                    var (relX, relY) = CalculateCaretPositionWithLineHeight(text, caretIndex, 0, FontSize, lineHeight);

                    int lineStartIndex = 0;
                    for (int i = 0; i < caretIndex; i++)
                    {
                        if (text[i] == '\r')
                        {
                            if (i + 1 < text.Length && text[i + 1] == '\n') { lineStartIndex = i + 2; i++; }
                            else { lineStartIndex = i + 1; }
                        }
                        else if (text[i] == '\n') { lineStartIndex = i + 1; }
                    }

                    int lineEndIndex = caretIndex;
                    while (lineEndIndex < text.Length)
                    {
                        if (text[lineEndIndex] == '\r' || text[lineEndIndex] == '\n') break;
                        lineEndIndex++;
                    }

                    string currentLineText = lineStartIndex < text.Length
                        ? text.Substring(lineStartIndex, Math.Max(0, lineEndIndex - lineStartIndex))
                        : "";

                    cursorX = relX + GetHorizontalOffsetForLine(currentLineText);
                    cursorY = relY;
                }

                cursorY += verticalAlignmentOffset;

                int offsetY = 2;
                var cursorBrush = new ImmutableSolidColorBrush(Colors.Black);
                var cursorPen = new ImmutablePen(cursorBrush.Color.ToUInt32(), 1);
                double cursorHeight = lineHeight - offsetY * 2;
                context.DrawLine(cursorPen,
                    new Point(cursorX, cursorY - offsetY),
                    new Point(cursorX, cursorY - offsetY + cursorHeight));
            }
        }
    }


    /// <summary>
    /// 根据对齐方式计算光标在当前行的水平偏移
    /// </summary>
    private double GetHorizontalOffsetForLine(string lineText)
    {
        if (TextAlignment == TextAlignment.Left)
            return 0;

        double lineWidth;
        if (string.IsNullOrEmpty(lineText))
        {
            // 空文本使用字形宽度作为参考宽度
            lineWidth = FontSize;
        }
        else
        {
            var textLayout = new TextLayout(
                text: lineText,
                typeface: new Typeface(FontFamily, FontStyle, FontWeight),
                fontSize: FontSize,
                foreground: Brushes.Transparent,
                textAlignment: TextAlignment.Left,
                textWrapping: TextWrapping.NoWrap,
                textTrimming: TextTrimming.None,
                flowDirection: FlowDirection.LeftToRight,
                lineHeight: 0,
                letterSpacing: 0,
                maxWidth: double.PositiveInfinity,
                maxHeight: double.PositiveInfinity
            );
            lineWidth = textLayout.Width;
        }
        
        if (lineWidth >= Bounds.Width)
            return 0;

        return TextAlignment == TextAlignment.Center
            ? (Bounds.Width - lineWidth) / 2
            : Bounds.Width - lineWidth;
    }

    /// <summary>
    /// 计算指定文本的渲染尺寸
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <returns>渲染尺寸</returns>
    public Size CalculateRenderedSize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new Size(0, 0);
        }

        double lineHeight = _currentLineHeight;
        if (lineHeight < FontSize)
        {
            lineHeight = FontSize * 1.2;
        }

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        double maxWidth = 0;

        foreach (var line in lines)
        {
            var formattedText = new FormattedText(
                line,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight),
                FontSize,
                Foreground ?? new SolidColorBrush(ColorValues.Black));
            maxWidth = Math.Max(maxWidth, formattedText.Width);
        }

        double width = maxWidth;
        double height = lines.Length * lineHeight;

        return new Size(width, height);
    }

    /// <summary>
    /// 获取当前控件的行高
    /// </summary>
    public double GetCurrentLineHeight()
    {
        double lineHeight = _currentLineHeight;
        if (lineHeight < FontSize)
        {
            lineHeight = FontSize * 1.2;
        }
        return lineHeight;
    }

    /// <summary>
    /// 绘制文本到 SkiaSharp Canvas
    /// </summary>
    /// <param name="canvas">SkiaSharp 画布</param>
    /// <param name="text">文本内容</param>
    /// <param name="position">绘制位置</param>
    /// <param name="textColor">文字颜色</param>
    public void DrawTextToCanvas(SKCanvas canvas, string text, SKPoint position, SKColor textColor)
    {
        if (string.IsNullOrEmpty(text))
            return;

        double lineHeight = _currentLineHeight;
        if (lineHeight < FontSize)
        {
            lineHeight = FontSize * 1.2;
        }

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
            Size = (float)FontSize,
            Typeface = typeface
        };

        using var paint = new SKPaint
        {
            Color = textColor,
            IsAntialias = true,
            TextSize = (float)FontSize,
            Typeface = typeface,
            Style = SKPaintStyle.Fill
        };

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line)) continue;

            // Y位置按行高计算
            double yPos = position.Y + i * lineHeight + FontSize;

            canvas.DrawText(line, (float)position.X, (float)yPos, font, paint);
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

        // 使用TextLayout计算光标X位置（包含尾部空格）
        var textLayout = new TextLayout(
            text: textBeforeCaret,
            typeface: new Typeface(FontFamily, FontStyle, FontWeight),
            fontSize: fontSize,
            foreground: Brushes.Transparent,
            textAlignment: TextAlignment.Left,
            textWrapping: TextWrapping.NoWrap,
            textTrimming: TextTrimming.None,
            flowDirection: FlowDirection.LeftToRight,
            lineHeight: 0,
            letterSpacing: 0,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity
        );

        double cursorX = padding + textLayout.WidthIncludingTrailingWhitespace;

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

    #region 富文本编辑功能

    /// <summary>
    /// 富文本片段集合（用于存储不同样式的文本片段）
    /// </summary>
    public ObservableCollection<RichTextRun> RichTextRuns { get; } = new();

    /// <summary>
    /// 是否启用富文本模式
    /// </summary>
    public bool IsRichTextMode { get; set; } = false;

    /// <summary>
    /// 是否只读
    /// </summary>
    public bool IsReadOnly
    {
        get
        {
            return _hiddenInput!.IsReadOnly;
        }
        set
        {
            _hiddenInput!.IsReadOnly = value;
        }
    }

    /// <summary>
    /// 获取选中的文本
    /// </summary>
    public string SelectedText => _hiddenInput?.SelectedText ?? string.Empty;

    /// <summary>
    /// 获取选区的起始位置
    /// </summary>
    public int SelectionStart => _hiddenInput?.SelectionStart ?? 0;

    /// <summary>
    /// 获取选区的结束位置
    /// </summary>
    public int SelectionEnd => _hiddenInput?.SelectionEnd ?? 0;

    /// <summary>
    /// 获取选区长度
    /// </summary>
    public int SelectionLength => Math.Abs((_hiddenInput?.SelectionEnd ?? 0) - (_hiddenInput?.SelectionStart ?? 0));

    /// <summary>
    /// 当前是否有选区
    /// </summary>
    public bool HasSelection => SelectionLength > 0;

    /// <summary>
    /// 选区变化事件
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// 文本变化事件
    /// </summary>
    public event EventHandler<TextChangedEventArgs>? TextChanged;

    /// <summary>
    /// 全选文本
    /// </summary>
    public void SelectAll()
    {
        if (_hiddenInput != null)
        {
            _hiddenInput.SelectionStart = 0;
            _hiddenInput.SelectionEnd = _hiddenInput.Text?.Length ?? 0;
        }
    }

    /// <summary>
    /// 清除选区
    /// </summary>
    public void ClearSelection()
    {
        if (_hiddenInput != null)
        {
            _hiddenInput.SelectionStart = _hiddenInput.SelectionEnd;
        }
    }

    /// <summary>
    /// 选中指定范围的文本
    /// </summary>
    public void Select(int start, int length)
    {
        if (_hiddenInput != null)
        {
            var textLength = _hiddenInput.Text?.Length ?? 0;
            _hiddenInput.SelectionStart = Math.Max(0, Math.Min(start, textLength));
            _hiddenInput.SelectionEnd = Math.Max(0, Math.Min(start + length, textLength));
        }
    }

    /// <summary>
    /// 在文本末尾追加文本
    /// </summary>
    public void AppendText(string text)
    {
        if (_hiddenInput != null)
        {
            var currentLength = _hiddenInput.Text?.Length ?? 0;
            _hiddenInput.Text = (_hiddenInput.Text ?? string.Empty) + text;
            _hiddenInput.SelectionStart = currentLength;
            _hiddenInput.SelectionEnd = _hiddenInput.Text?.Length ?? 0;
        }
    }

    /// <summary>
    /// 获取当前控件的行数
    /// </summary>
    public int GetLineCount() => _hiddenInput?.GetLineCount() ?? 1;

    #endregion

    #region 富文本格式化方法

    /// <summary>
    /// 设置当前选区的文本颜色
    /// </summary>
    public void SetSelectionColor(string color)
    {
        if (string.IsNullOrEmpty(SelectedText)) return;

        // 在富文本模式下，更新对应片段的颜色
        if (IsRichTextMode)
        {
            UpdateRichTextRunColor(color);
        }

        InvalidateVisual();
    }

    /// <summary>
    /// 设置当前选区的字体大小
    /// </summary>
    public void SetSelectionFontSize(double fontSize)
    {
        if (string.IsNullOrEmpty(SelectedText)) return;

        if (IsRichTextMode)
        {
            UpdateRichTextRunFontSize(fontSize);
        }

        InvalidateVisual();
    }

    /// <summary>
    /// 切换当前选区的加粗状态
    /// </summary>
    public void ToggleBold()
    {
        if (_hiddenInput == null) return;

        var currentWeight = _hiddenInput.FontWeight;
        _hiddenInput.FontWeight = currentWeight == FontWeight.Bold ? FontWeight.Normal : FontWeight.Bold;

        if (IsRichTextMode && !string.IsNullOrEmpty(SelectedText))
        {
            UpdateRichTextRunBold(_hiddenInput.FontWeight == FontWeight.Bold);
        }

        InvalidateVisual();
    }

    /// <summary>
    /// 切换当前选区的斜体状态
    /// </summary>
    public void ToggleItalic()
    {
        if (_hiddenInput == null) return;

        var currentStyle = _hiddenInput.FontStyle;
        _hiddenInput.FontStyle = currentStyle == FontStyle.Italic ? FontStyle.Normal : FontStyle.Italic;

        if (IsRichTextMode && !string.IsNullOrEmpty(SelectedText))
        {
            UpdateRichTextRunItalic(_hiddenInput.FontStyle == FontStyle.Italic);
        }

        InvalidateVisual();
    }

    /// <summary>
    /// 切换当前选区的下划线状态
    /// </summary>
    public void ToggleUnderline()
    {
        if (IsRichTextMode && !string.IsNullOrEmpty(SelectedText))
        {
            UpdateRichTextRunUnderline();
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 切换当前选区的删除线状态
    /// </summary>
    public void ToggleStrikethrough()
    {
        if (IsRichTextMode && !string.IsNullOrEmpty(SelectedText))
        {
            UpdateRichTextRunStrikethrough();
            InvalidateVisual();
        }
    }

    /// <summary>
    /// 应用格式化到选区（加粗、斜体、颜色、字号等）
    /// </summary>
    public void ApplyFormatting(bool? bold = null, bool? italic = null, string? color = null, double? fontSize = null)
    {
        if (!HasSelection) return;

        if (bold.HasValue && _hiddenInput != null)
        {
            _hiddenInput.FontWeight = bold.Value ? FontWeight.Bold : FontWeight.Normal;
        }

        if (italic.HasValue && _hiddenInput != null)
        {
            _hiddenInput.FontStyle = italic.Value ? FontStyle.Italic : FontStyle.Normal;
        }

        if (IsRichTextMode)
        {
            if (color != null)
                UpdateRichTextRunColor(color);
            if (fontSize.HasValue)
                UpdateRichTextRunFontSize(fontSize.Value);
        }

        InvalidateVisual();
    }

    /// <summary>
    /// 更新富文本片段的颜色
    /// </summary>
    private void UpdateRichTextRunColor(string color)
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var length = SelectionLength;
        var text = Text ?? string.Empty;

        if (start >= text.Length) return;

        int pos = 0;
        foreach (var run in RichTextRuns)
        {
            var runEnd = pos + run.Text.Length;

            if (runEnd > start && pos < start + length)
            {
                var overlapStart = Math.Max(pos, start);
                var overlapEnd = Math.Min(runEnd, start + length);

                if (overlapStart < overlapEnd)
                {
                    run.ForegroundColor = color;
                }
            }

            pos = runEnd;
        }
    }

    /// <summary>
    /// 更新富文本片段的字体大小
    /// </summary>
    private void UpdateRichTextRunFontSize(double fontSize)
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var length = SelectionLength;
        var text = Text ?? string.Empty;

        if (start >= text.Length) return;

        int pos = 0;
        foreach (var run in RichTextRuns)
        {
            var runEnd = pos + run.Text.Length;

            if (runEnd > start && pos < start + length)
            {
                var overlapStart = Math.Max(pos, start);
                var overlapEnd = Math.Min(runEnd, start + length);

                if (overlapStart < overlapEnd)
                {
                    run.FontSize = fontSize;
                }
            }

            pos = runEnd;
        }
    }

    /// <summary>
    /// 更新富文本片段的加粗状态
    /// </summary>
    private void UpdateRichTextRunBold(bool isBold)
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var length = SelectionLength;
        var text = Text ?? string.Empty;

        if (start >= text.Length) return;

        int pos = 0;
        foreach (var run in RichTextRuns)
        {
            var runEnd = pos + run.Text.Length;

            if (runEnd > start && pos < start + length)
            {
                var overlapStart = Math.Max(pos, start);
                var overlapEnd = Math.Min(runEnd, start + length);

                if (overlapStart < overlapEnd)
                {
                    run.IsBold = isBold;
                }
            }

            pos = runEnd;
        }
    }

    /// <summary>
    /// 更新富文本片段的斜体状态
    /// </summary>
    private void UpdateRichTextRunItalic(bool isItalic)
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var length = SelectionLength;
        var text = Text ?? string.Empty;

        if (start >= text.Length) return;

        int pos = 0;
        foreach (var run in RichTextRuns)
        {
            var runEnd = pos + run.Text.Length;

            if (runEnd > start && pos < start + length)
            {
                var overlapStart = Math.Max(pos, start);
                var overlapEnd = Math.Min(runEnd, start + length);

                if (overlapStart < overlapEnd)
                {
                    run.IsItalic = isItalic;
                }
            }

            pos = runEnd;
        }
    }

    /// <summary>
    /// 更新富文本片段的下划线状态
    /// </summary>
    private void UpdateRichTextRunUnderline()
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var length = SelectionLength;
        var text = Text ?? string.Empty;

        if (start >= text.Length) return;

        int pos = 0;
        foreach (var run in RichTextRuns)
        {
            var runEnd = pos + run.Text.Length;

            if (runEnd > start && pos < start + length)
            {
                var overlapStart = Math.Max(pos, start);
                var overlapEnd = Math.Min(runEnd, start + length);

                if (overlapStart < overlapEnd)
                {
                    run.IsUnderline = !run.IsUnderline;
                }
            }

            pos = runEnd;
        }
    }

    /// <summary>
    /// 更新富文本片段的删除线状态
    /// </summary>
    private void UpdateRichTextRunStrikethrough()
    {
        var start = Math.Min(SelectionStart, SelectionEnd);
        var length = SelectionLength;
        var text = Text ?? string.Empty;

        if (start >= text.Length) return;

        int pos = 0;
        foreach (var run in RichTextRuns)
        {
            var runEnd = pos + run.Text.Length;

            if (runEnd > start && pos < start + length)
            {
                var overlapStart = Math.Max(pos, start);
                var overlapEnd = Math.Min(runEnd, start + length);

                if (overlapStart < overlapEnd)
                {
                    run.IsStrikethrough = !run.IsStrikethrough;
                }
            }

            pos = runEnd;
        }
    }

    /// <summary>
    /// 添加一个富文本片段
    /// </summary>
    public void AddRichTextRun(RichTextRun run)
    {
        RichTextRuns.Add(run);
        InvalidateVisual();
    }

    /// <summary>
    /// 清空富文本片段
    /// </summary>
    public void ClearRichTextRuns()
    {
        RichTextRuns.Clear();
        InvalidateVisual();
    }

    /// <summary>
    /// 从纯文本创建富文本片段
    /// </summary>
    public void CreateRichTextRunsFromText(string text)
    {
        RichTextRuns.Clear();
        if (!string.IsNullOrEmpty(text))
        {
            RichTextRuns.Add(new RichTextRun
            {
                Text = text,
                ForegroundColor = "#000000",
                FontSize = FontSize,
                IsBold = FontWeight == FontWeight.Bold,
                IsItalic = FontStyle == FontStyle.Italic
            });
        }
        InvalidateVisual();
    }

    /// <summary>
    /// 将当前文本和样式转换为富文本片段
    /// </summary>
    public void SyncTextToRichTextRuns()
    {
        if (string.IsNullOrEmpty(Text))
        {
            RichTextRuns.Clear();
            return;
        }

        // 如果已有片段且文本匹配，只更新样式
        if (RichTextRuns.Count == 1 && RichTextRuns[0].Text == Text)
        {
            RichTextRuns[0].FontSize = FontSize;
            RichTextRuns[0].IsBold = FontWeight == FontWeight.Bold;
            RichTextRuns[0].IsItalic = FontStyle == FontStyle.Italic;
            return;
        }

        // 否则根据当前样式创建新片段
        RichTextRuns.Clear();
        RichTextRuns.Add(new RichTextRun
        {
            Text = Text,
            ForegroundColor = "#000000",
            FontSize = FontSize,
            IsBold = FontWeight == FontWeight.Bold,
            IsItalic = FontStyle == FontStyle.Italic
        });
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 绘制单行文本
    /// </summary>
    private void DrawTextLine(DrawingContext context, string text, double x, double y, IBrush foreground, TextDecorationCollection? textDecorations = null, bool isSelected = false)
    {
        if (string.IsNullOrEmpty(text)) return;

        // 计算对齐偏移
        double alignmentOffset = GetHorizontalOffsetForLine(text);

        var textLayoutForRender = new TextLayout(
            text: text,
            typeface: new Typeface(FontFamily, FontStyle, FontWeight),
            fontSize: FontSize,
            foreground: foreground,
            textAlignment: TextAlignment.Left,
            textWrapping: TextWrapping.NoWrap,
            textTrimming: TextTrimming.None,
            textDecorations: textDecorations,
            flowDirection: FlowDirection.LeftToRight,
            lineHeight: 0,
            letterSpacing: 0,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity
        );

        textLayoutForRender.Draw(context, new Point(x + alignmentOffset, y));
    }

    /// <summary>
    /// 在指定位置绘制文本（不计算对齐偏移）
    /// </summary>
    private void DrawTextLineAt(DrawingContext context, string text, double x, double y, IBrush foreground, TextDecorationCollection? textDecorations = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        var textLayoutForRender = new TextLayout(
            text: text,
            typeface: new Typeface(FontFamily, FontStyle, FontWeight),
            fontSize: FontSize,
            foreground: foreground,
            textAlignment: TextAlignment.Left,
            textWrapping: TextWrapping.NoWrap,
            textTrimming: TextTrimming.None,
            textDecorations: textDecorations,
            flowDirection: FlowDirection.LeftToRight,
            lineHeight: 0,
            letterSpacing: 0,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity
        );

        textLayoutForRender.Draw(context, new Point(x, y));
    }

    /// <summary>
    /// 绘制选区高亮
    /// </summary>
    private void RenderSelectionHighlight(DrawingContext context, string[] lines, double lineHeight, int selectionStart, int selectionEnd, double verticalOffset = 0)
    {
        if (selectionStart == selectionEnd) return;

        int currentPos = 0;
        double highlightY = verticalOffset;

        foreach (var line in lines)
        {
            var lineEnd = currentPos + line.Length;

            if (selectionEnd <= currentPos)
                break;

            if (selectionStart > lineEnd)
            {
                currentPos = lineEnd + Environment.NewLine.Length;
                highlightY += lineHeight;
                continue;
            }

            // 计算选区在这一行的范围
            var lineStartPos = Math.Max(selectionStart, currentPos) - currentPos;
            var lineEndPos = Math.Min(selectionEnd, lineEnd) - currentPos;

            if (lineStartPos < line.Length && lineEndPos > lineStartPos && lineEndPos <= line.Length)
            {
                var selectedText = line.Substring(lineStartPos, lineEndPos - lineStartPos);

                // 计算该行的水平对齐偏移
                double lineHorizontalOffset = GetHorizontalOffsetForLine(line);

                // 计算 X 位置
                double highlightX = lineHorizontalOffset;
                if (lineStartPos > 0)
                {
                    var textBefore = line.Substring(0, lineStartPos);
                    highlightX += CalculateTextWidth(textBefore);
                }

                double highlightWidth = CalculateTextWidth(selectedText);

                // 绘制高亮背景
                var highlightBrush = new ImmutableSolidColorBrush(Color.Parse("#3399FF"));
                context.DrawRectangle(
                    highlightBrush,
                    null,
                    new Rect(highlightX, highlightY + 2, highlightWidth, lineHeight - 4));
            }

            currentPos = lineEnd + Environment.NewLine.Length;
            highlightY += lineHeight;
        }
    }

    /// <summary>
    /// 计算文本宽度（使用TextLayout精确计算，包含尾部空格）
    /// </summary>
    private double CalculateTextWidth(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;

        var textLayout = new TextLayout(
            text: text,
            typeface: new Typeface(FontFamily, FontStyle, FontWeight),
            fontSize: FontSize,
            foreground: Brushes.Transparent,
            textAlignment: TextAlignment.Left,
            textWrapping: TextWrapping.NoWrap,
            textTrimming: TextTrimming.None,
            flowDirection: FlowDirection.LeftToRight,
            lineHeight: 0,
            letterSpacing: 0,
            maxWidth: double.PositiveInfinity,
            maxHeight: double.PositiveInfinity
        );

        // 使用包含尾部空格的宽度
        return textLayout.WidthIncludingTrailingWhitespace;
    }

    #endregion
}
