using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.UI.Controls;

/// <summary>
/// 富文本片段，表示一段带有样式的文本
/// </summary>
public partial class RichTextRun : ObservableObject
{
    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private string _foregroundColor = "#000000";

    [ObservableProperty]
    private double _fontSize = 14;

    [ObservableProperty]
    private bool _isBold = false;

    [ObservableProperty]
    private bool _isItalic = false;

    [ObservableProperty]
    private bool _isUnderline = false;

    [ObservableProperty]
    private bool _isStrikethrough = false;

    public FontWeight FontWeight => IsBold ? FontWeight.Bold : FontWeight.Normal;

    public FontStyle FontStyle => IsItalic ? FontStyle.Italic : FontStyle.Normal;

    /// <summary>
    /// 获取文本装饰（如下划线、删除线）
    /// </summary>
    public TextDecorationCollection? TextDecorations
    {
        get
        {
            if (!IsUnderline && !IsStrikethrough)
                return null;

            var collection = new TextDecorationCollection();
            if (IsUnderline)
                collection.Add(new TextDecoration { Location = TextDecorationLocation.Underline });
            if (IsStrikethrough)
                collection.Add(new TextDecoration { Location = TextDecorationLocation.Strikethrough });
            return collection;
        }
    }

    public RichTextRun()
    {
    }

    public RichTextRun(string text)
    {
        Text = text;
    }

    public RichTextRun(string text, string foregroundColor, double fontSize = 14)
    {
        Text = text;
        ForegroundColor = foregroundColor;
        FontSize = fontSize;
    }

    public RichTextRun(string text, string foregroundColor, double fontSize, bool isBold, bool isItalic = false)
    {
        Text = text;
        ForegroundColor = foregroundColor;
        FontSize = fontSize;
        IsBold = isBold;
        IsItalic = isItalic;
    }

    public RichTextRun Clone()
    {
        return new RichTextRun
        {
            Text = Text,
            ForegroundColor = ForegroundColor,
            FontSize = FontSize,
            IsBold = IsBold,
            IsItalic = IsItalic,
            IsUnderline = IsUnderline,
            IsStrikethrough = IsStrikethrough
        };
    }
}
