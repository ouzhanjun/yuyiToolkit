using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace yuyiAvalonia.ImageEdit.Models;

/// <summary>
/// 文字图层数据模型
/// </summary>
public class TextLine
{
    public string Text { get; set; } = string.Empty;
    public Point Position { get; set; }
}

public class TextLayerData
{
    public Point InsertPoint { get; set; }
    public List<TextLine> Lines { get; set; } = new();
    public double FontSize { get; set; }
    public FontWeight FontWeight { get; set; } = FontWeight.Normal;
    public FontStyle FontStyle { get; set; } = FontStyle.Normal;
    public FontFamily FontFamily { get; set; } = FontFamily.Default;
    public string FontColor { get; set; } = "#000000";
    
    /// <summary>
    /// 行高（根据字号计算）
    /// </summary>
    public double LineHeight => FontSize * 1.2;
}
