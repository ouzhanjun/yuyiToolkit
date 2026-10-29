namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 文字样式 - 用于定义网页/小程序的文字样式
/// </summary>
public class TextStyle
{
    /// <summary>
    /// 字体族
    /// </summary>
    public string FontFamily { get; set; } = "system-ui, -apple-system, sans-serif";

    /// <summary>
    /// 字体大小
    /// </summary>
    public double FontSize { get; set; } = 14;

    /// <summary>
    /// 字体大小单位
    /// </summary>
    public FontSizeUnit FontSizeUnit { get; set; } = FontSizeUnit.Px;

    /// <summary>
    /// 字体颜色
    /// </summary>
    public string Color { get; set; } = "#333333";

    /// <summary>
    /// 字体粗细
    /// </summary>
    public FontWeight FontWeight { get; set; } = FontWeight.Normal;

    /// <summary>
    /// 字体样式
    /// </summary>
    public FontStyle FontStyle { get; set; } = FontStyle.Normal;

    /// <summary>
    /// 行高
    /// </summary>
    public double LineHeight { get; set; } = 1.5;

    /// <summary>
    /// 文字间距
    /// </summary>
    public double LetterSpacing { get; set; } = 0;

    /// <summary>
    /// 文字对齐方式
    /// </summary>
    public TextAlign TextAlign { get; set; } = TextAlign.Left;

    /// <summary>
    /// 文字装饰线
    /// </summary>
    public TextDecoration TextDecoration { get; set; } = TextDecoration.None;

    /// <summary>
    /// 首行缩进
    /// </summary>
    public double TextIndent { get; set; } = 0;

    /// <summary>
    /// 单词换行
    /// </summary>
    public bool WordWrap { get; set; } = true;

    /// <summary>
    /// 文字溢出处理
    /// </summary>
    public TextOverflow TextOverflow { get; set; } = TextOverflow.Normal;

    /// <summary>
    /// 转换文本
    /// </summary>
    public TextTransform TextTransform { get; set; } = TextTransform.None;

    public TextStyle Clone()
    {
        return new TextStyle
        {
            FontFamily = FontFamily,
            FontSize = FontSize,
            FontSizeUnit = FontSizeUnit,
            Color = Color,
            FontWeight = FontWeight,
            FontStyle = FontStyle,
            LineHeight = LineHeight,
            LetterSpacing = LetterSpacing,
            TextAlign = TextAlign,
            TextDecoration = TextDecoration,
            TextIndent = TextIndent,
            WordWrap = WordWrap,
            TextOverflow = TextOverflow,
            TextTransform = TextTransform
        };
    }

    /// <summary>
    /// 生成CSS代码
    /// </summary>
    public string ToCss()
    {
        var unit = FontSizeUnit == FontSizeUnit.Px ? "px" : "rem";
        var lines = new List<string>
        {
            $"font-family: {FontFamily};",
            $"font-size: {FontSize}{unit};",
            $"color: {Color};",
            $"font-weight: {(int)FontWeight};",
            $"line-height: {LineHeight};",
            $"text-align: {TextAlign.ToString().ToLower()};"
        };

        if (LetterSpacing != 0)
            lines.Add($"letter-spacing: {LetterSpacing}{unit};");
        if (TextDecoration != TextDecoration.None)
            lines.Add($"text-decoration: {TextDecoration.ToString().ToLower()};");
        if (TextIndent != 0)
            lines.Add($"text-indent: {TextIndent}{unit};");
        if (!WordWrap)
            lines.Add($"white-space: nowrap;");
        if (TextTransform != TextTransform.None)
            lines.Add($"text-transform: {TextTransform.ToString().ToLower()};");

        return string.Join("\n", lines);
    }
}

public enum FontSizeUnit
{
    Px,
    Rem,
    Em,
    Percent
}

public enum FontWeight
{
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    Normal = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    Black = 900
}

public enum FontStyle
{
    Normal,
    Italic,
    Oblique
}

public enum TextAlign
{
    Left,
    Center,
    Right,
    Justify
}

public enum TextDecoration
{
    None,
    Underline,
    Overline,
    LineThrough
}

public enum TextOverflow
{
    Normal,
    Clip,
    Ellipsis
}

public enum TextTransform
{
    None,
    Uppercase,
    Lowercase,
    Capitalize
}
