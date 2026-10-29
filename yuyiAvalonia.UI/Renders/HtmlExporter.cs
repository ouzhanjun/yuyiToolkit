using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Text;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Renders;

/// <summary>
/// HTML导出结果
/// </summary>
public class HtmlExportResult
{
    public bool Success { get; set; }
    public string HtmlFilePath { get; set; } = string.Empty;
    public string ImagesFolder { get; set; } = string.Empty;
    public List<string> ExportedImages { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// HTML导出器 - 将画布元素转换为可运行的HTML页面
/// </summary>
public class HtmlExporter
{
    private readonly ObservableCollection<CanvasElement> _elements;
    private readonly double _canvasWidth;
    private readonly double _canvasHeight;
    private readonly string _exportFolder;
    private readonly string _imagesFolder;
    private readonly Dictionary<string, string> _imagePathMapping = new();

    public HtmlExporter(ObservableCollection<CanvasElement> elements, double canvasWidth, double canvasHeight, string exportFolder)
    {
        _elements = elements;
        _canvasWidth = canvasWidth;
        _canvasHeight = canvasHeight;
        _exportFolder = exportFolder;
        _imagesFolder = Path.Combine(_exportFolder, "images");

        // 确保目录存在
        Directory.CreateDirectory(_exportFolder);
        Directory.CreateDirectory(_imagesFolder);
    }

    /// <summary>
    /// 执行导出
    /// </summary>
    public HtmlExportResult Export()
    {
        var result = new HtmlExportResult
        {
            ImagesFolder = _imagesFolder
        };

        try
        {
            // 1. 处理图片 - 导出所有图片到images文件夹
            ExportImages();

            // 2. 生成HTML
            var html = GenerateHtml();
            var htmlPath = Path.Combine(_exportFolder, "index.html");
            File.WriteAllText(htmlPath, html, Encoding.UTF8);

            result.Success = true;
            result.HtmlFilePath = htmlPath;
            result.ExportedImages = _imagePathMapping.Values.Distinct().ToList();
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add(ex.Message);
        }

        return result;
    }

    /// <summary>
    /// 导出所有图片
    /// </summary>
    private void ExportImages()
    {
        var imageElements = GetAllElementsOfType<ImageElement>(_elements);
        foreach (var img in imageElements)
        {
            if (!string.IsNullOrEmpty(img.ImagePath) && File.Exists(img.ImagePath))
            {
                ExportSingleImage(img.ImagePath, img.Id);
            }
        }

        // 也检查背景图片
        var elementsWithBg = _elements.Concat(GetAllChildren(_elements))
            .Where(e => !string.IsNullOrEmpty(e.BackgroundFill.ImagePath) && File.Exists(e.BackgroundFill.ImagePath));
        foreach (var elem in elementsWithBg)
        {
            if (!_imagePathMapping.ContainsKey(elem.BackgroundFill.ImagePath))
            {
                ExportSingleImage(elem.BackgroundFill.ImagePath, $"{elem.Id}_bg");
            }
        }
    }

    /// <summary>
    /// 导出单张图片
    /// </summary>
    private void ExportSingleImage(string sourcePath, string elementId)
    {
        try
        {
            var extension = Path.GetExtension(sourcePath);
            var newFileName = $"{elementId}{extension}";
            var destPath = Path.Combine(_imagesFolder, newFileName);

            File.Copy(sourcePath, destPath, true);
            _imagePathMapping[sourcePath] = $"./images/{newFileName}";
        }
        catch (Exception)
        {
            // 图片导出失败不影响主流程
        }
    }

    /// <summary>
    /// 生成HTML内容
    /// </summary>
    private string GenerateHtml()
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"zh-CN\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>微信小程序页面</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        * { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; }");
        sb.AppendLine($"        .page-container {{ width: {_canvasWidth}px; min-height: {_canvasHeight}px; position: relative; overflow: hidden; margin: 0 auto; }}");
        sb.AppendLine("        .element { position: absolute; }");
        sb.AppendLine("        .text-element { display: flex; align-items: center; justify-content: center; }");
        sb.AppendLine("        .button-element { display: flex; align-items: center; justify-content: center; cursor: pointer; }");
        sb.AppendLine("        .image-element { overflow: hidden; }");
        sb.AppendLine("        .image-element img { width: 100%; height: 100%; object-fit: inherit; }");
        sb.AppendLine("        .container { display: flex; }");
        sb.AppendLine("        .container-vertical { flex-direction: column; }");
        sb.AppendLine("        .container-grid { display: grid; }");
        sb.AppendLine("        .container-canvas { position: relative; }");
        sb.AppendLine("        .container-panel { position: relative; }");
        sb.AppendLine("        .card { border-radius: 8px; overflow: hidden; }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"page-container\">");

        // 渲染所有顶层元素及其子元素
        foreach (var element in _elements)
        {
            RenderElement(sb, element);
        }

        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// 渲染单个元素
    /// </summary>
    private void RenderElement(StringBuilder sb, CanvasElement element, int depth = 0)
    {
        var isContainer = element.IsContainer;
        var tag = element.ElementType switch
        {
            ComponentType.Text => "div",
            ComponentType.Button => "div",
            ComponentType.Card => "div",
            ComponentType.Panel => "div",
            ComponentType.VerticalStackPanel => "div",
            ComponentType.HorizontalStackPanel => "div",
            ComponentType.Grid => "div",
            ComponentType.Canvas => "div",
            ComponentType.Image => "div",
            _ => "div"
        };

        // 计算位置和样式
        var styles = new List<string>();
        var classes = new List<string> { "element" };

        // 基础定位样式
        styles.Add($"left: {element.X}px");
        styles.Add($"top: {element.Y}px");
        styles.Add($"width: {element.Width}px");
        styles.Add($"height: {element.Height}px");

        // Z-Value
        if (element.ZIndex > 0)
            styles.Add($"z-index: {element.ZIndex}");

        // 透明度
        if (element.Opacity < 1)
            styles.Add($"opacity: {element.Opacity}");

        // 背景色
        if (element.BackgroundFill.FillType == FillType.Solid && !string.IsNullOrEmpty(element.BackgroundFill.BackgroundColor))
        {
            styles.Add($"background-color: {element.BackgroundFill.BackgroundColor}");
        }
        // 渐变背景
        else if (element.BackgroundFill.FillType == FillType.Gradient)
        {
            var gradient = GenerateGradientCss(element.BackgroundFill.GradientStartColor, element.BackgroundFill.GradientEndColor, element.BackgroundFill.GradientAngle);
            styles.Add($"background: {gradient}");
        }
        // 图片背景
        else if (element.BackgroundFill.FillType == FillType.Image && !string.IsNullOrEmpty(element.BackgroundFill.ImagePath))
        {
            var imgUrl = GetImageUrl(element.BackgroundFill.ImagePath);
            styles.Add($"background-image: url('{imgUrl}')");
            styles.Add($"background-size: {(element.BackgroundFill.ImageStretch == Stretch.UniformToFill ? "cover" : "contain")}");
            styles.Add($"background-repeat: no-repeat");
        }

        // 边框
        if (element.BorderWidth > 0 && !string.IsNullOrEmpty(element.BorderColor))
        {
            styles.Add($"border: {element.BorderWidth}px solid {element.BorderColor}");
        }

        // 圆角
        if (element.CornerRadius > 0)
        {
            styles.Add($"border-radius: {element.CornerRadius}px");
        }

        // 内边距
        if (element.Padding > 0)
            styles.Add($"padding: {element.Padding}px");
        else if (!element.PaddingIsUniform && element.PaddingLeft > 0)
            styles.Add($"padding: {element.PaddingTop}px {element.PaddingRight}px {element.PaddingBottom}px {element.PaddingLeft}px");

        // 根据类型添加特定样式
        switch (element.ElementType)
        {
            case ComponentType.Text:
                RenderTextElement(sb, element as TextElement, tag, styles, classes);
                return;

            case ComponentType.Button:
                RenderButtonElement(sb, element as ButtonElement, tag, styles, classes);
                return;

            case ComponentType.Image:
                RenderImageElement(sb, element as ImageElement, tag, styles, classes);
                return;

            case ComponentType.Card:
                classes.Add("card");
                classes.Add("container");
                break;

            case ComponentType.Panel:
                classes.Add("container-panel");
                break;

            case ComponentType.VerticalStackPanel:
                classes.Add("container");
                classes.Add("container-vertical");
                var vStack = element as StackPanelElement;
                if (vStack != null && vStack.Spacing > 0)
                    styles.Add($"gap: {vStack.Spacing}px");
                break;

            case ComponentType.HorizontalStackPanel:
                classes.Add("container");
                var hStack = element as StackPanelElement;
                if (hStack != null && hStack.Spacing > 0)
                    styles.Add($"gap: {hStack.Spacing}px");
                break;

            case ComponentType.Grid:
                RenderGridElement(sb, element as GridElement, tag, styles, classes);
                return;

            case ComponentType.Canvas:
                classes.Add("container-canvas");
                break;
        }

        // 输出元素
        var styleStr = string.Join("; ", styles);
        var classStr = string.Join(" ", classes);

        sb.AppendLine($"<{tag} class=\"{classStr}\" style=\"{styleStr}\">");

        // 渲染子元素
        if (element.Children != null && element.Children.Count > 0)
        {
            foreach (var child in element.Children)
            {
                RenderElement(sb, child, depth + 1);
            }
        }

        sb.AppendLine($"</{tag}>");
    }

    /// <summary>
    /// 渲染文本元素
    /// </summary>
    private void RenderTextElement(StringBuilder sb, TextElement? element, string tag, List<string> styles, List<string> classes)
    {
        if (element == null) return;

        classes.Add("text-element");

        var fontStyles = new List<string>();
        fontStyles.Add($"font-size: {element.FontSize}px");
        fontStyles.Add($"font-family: '{element.FontFamily}', sans-serif");
        if (element.IsBold) fontStyles.Add("font-weight: bold");
        if (element.IsItalic) fontStyles.Add("font-style: italic");
        if (element.IsUnderline) fontStyles.Add("text-decoration: underline");
        if (element.IsStrikethrough) fontStyles.Add("text-decoration: line-through");
        fontStyles.Add($"color: {element.ForeColor}");
        fontStyles.Add($"text-align: {element.TextAlignment.ToString().ToLower()}");
        if (element.LineHeight > 0)
            fontStyles.Add($"line-height: {element.LineHeight}");

        var styleStr = string.Join("; ", styles.Concat(fontStyles));
        var classStr = string.Join(" ", classes);

        var text = System.Web.HttpUtility.HtmlEncode(element.Text ?? "");
        sb.AppendLine($"<{tag} class=\"{classStr}\" style=\"{styleStr}\">{text}</{tag}>");
    }

    /// <summary>
    /// 渲染按钮元素
    /// </summary>
    private void RenderButtonElement(StringBuilder sb, ButtonElement? element, string tag, List<string> styles, List<string> classes)
    {
        if (element == null) return;

        classes.Add("button-element");

        // 按钮样式
        var buttonStyles = new List<string>();
        switch (element.ButtonStyle)
        {
            case ButtonStyle.Primary:
                styles.Add("background-color: #07C160");
                styles.Add("color: #FFFFFF");
                break;
            case ButtonStyle.Secondary:
                styles.Add("background-color: #F5F5F5");
                styles.Add("color: #333333");
                break;
            case ButtonStyle.Outline:
                styles.Add("background-color: transparent");
                styles.Add("border: 1px solid #07C160");
                styles.Add("color: #07C160");
                break;
            case ButtonStyle.Text:
                styles.Add("background-color: transparent");
                styles.Add("color: #07C160");
                break;
        }

        var fontStyles = new List<string>();
        fontStyles.Add($"font-size: {element.FontSize}px");
        fontStyles.Add($"font-family: '{element.FontFamily}', sans-serif");
        if (element.IsBold) fontStyles.Add("font-weight: bold");
        fontStyles.Add($"color: {element.ForeColor}");
        fontStyles.Add($"text-align: {element.TextAlignment.ToString().ToLower()}");

        var styleStr = string.Join("; ", styles.Concat(fontStyles));
        var classStr = string.Join(" ", classes);

        var text = System.Web.HttpUtility.HtmlEncode(element.Text ?? "");
        sb.AppendLine($"<{tag} class=\"{classStr}\" style=\"{styleStr}\">{text}</{tag}>");
    }

    /// <summary>
    /// 渲染图片元素
    /// </summary>
    private void RenderImageElement(StringBuilder sb, ImageElement? element, string tag, List<string> styles, List<string> classes)
    {
        if (element == null) return;

        classes.Add("image-element");

        // 图片拉伸模式
        var stretchMode = element.ImageStretch switch
        {
            Avalonia.Media.Stretch.Uniform => "contain",
            Avalonia.Media.Stretch.UniformToFill => "cover",
            Avalonia.Media.Stretch.Fill => "100% 100%",
            Avalonia.Media.Stretch.None => "auto",
            _ => "contain"
        };
        styles.Add($"object-fit: {stretchMode}");

        var styleStr = string.Join("; ", styles);
        var classStr = string.Join(" ", classes);

        var imgUrl = GetImageUrl(element.ImagePath);
        sb.AppendLine($"<{tag} class=\"{classStr}\" style=\"{styleStr}\">");
        sb.AppendLine($"    <img src=\"{imgUrl}\" alt=\"图片\" style=\"width:100%;height:100%;object-fit:{stretchMode};\"/>");
        sb.AppendLine($"</{tag}>");
    }

    /// <summary>
    /// 渲染Grid容器
    /// </summary>
    private void RenderGridElement(StringBuilder sb, GridElement? element, string tag, List<string> styles, List<string> classes)
    {
        if (element == null) return;

        classes.Add("container-grid");

        // 解析行定义
        var rowDefs = ParseGridDefinitions(element.RowDefinitions, element.Rows);
        var colDefs = ParseGridDefinitions(element.ColumnDefinitions, element.Columns);

        styles.Add($"grid-template-rows: {rowDefs}");
        styles.Add($"grid-template-columns: {colDefs}");
        if (element.RowSpacing > 0)
            styles.Add($"grid-row-gap: {element.RowSpacing}px");
        if (element.ColumnSpacing > 0)
            styles.Add($"grid-column-gap: {element.ColumnSpacing}px");

        var styleStr = string.Join("; ", styles);
        var classStr = string.Join(" ", classes);

        sb.AppendLine($"<{tag} class=\"{classStr}\" style=\"{styleStr}\">");

        // 渲染子元素
        if (element.Children != null && element.Children.Count > 0)
        {
            foreach (var child in element.Children)
            {
                // Grid子元素需要设置grid-row和grid-column
                var childStyles = new List<string>();
                if (child.GridRow >= 0)
                    childStyles.Add($"grid-row: {child.GridRow + 1}");
                if (child.GridColumn >= 0)
                    childStyles.Add($"grid-column: {child.GridColumn + 1}");

                var childStyleStr = string.Join("; ", childStyles);
                sb.AppendLine($"    <div class=\"element\" style=\"{childStyleStr}\">");
                RenderElement(sb, child, 2);
                sb.AppendLine($"    </div>");
            }
        }

        sb.AppendLine($"</{tag}>");
    }

    /// <summary>
    /// 解析Grid定义字符串
    /// </summary>
    private string ParseGridDefinitions(string definitions, int defaultCount)
    {
        if (string.IsNullOrEmpty(definitions))
        {
            // 默认平均分配
            return string.Join(" ", Enumerable.Repeat("1fr", defaultCount));
        }

        var parts = definitions.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<string>();

        foreach (var part in parts)
        {
            var p = part.Trim();
            if (p.EndsWith("*"))
            {
                var numStr = p.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numStr))
                    result.Add("1fr");
                else if (double.TryParse(numStr, out var num))
                    result.Add($"{num}fr");
                else
                    result.Add("1fr");
            }
            else if (p.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                result.Add("auto");
            }
            else if (double.TryParse(p, out _))
            {
                result.Add($"{p}px");
            }
            else
            {
                result.Add("1fr");
            }
        }

        return result.Count > 0 ? string.Join(" ", result) : string.Join(" ", Enumerable.Repeat("1fr", defaultCount));
    }

    /// <summary>
    /// 生成渐变CSS
    /// </summary>
    private string GenerateGradientCss(string startColor, string endColor, double angle)
    {
        // 角度转换为CSS方向
        var direction = angle switch
        {
            0 => "to right",
            90 => "to bottom",
            180 => "to left",
            270 => "to top",
            _ => $"to right"
        };

        return $"linear-gradient({direction}, {startColor}, {endColor})";
    }

    /// <summary>
    /// 获取图片URL（处理路径映射）
    /// </summary>
    private string GetImageUrl(string originalPath)
    {
        if (string.IsNullOrEmpty(originalPath))
            return "";

        if (_imagePathMapping.TryGetValue(originalPath, out var mappedPath))
            return mappedPath;

        // 如果没有映射，假设是网络URL或相对路径
        if (originalPath.StartsWith("http://") || originalPath.StartsWith("https://"))
            return originalPath;

        // 相对路径
        return originalPath;
    }

    /// <summary>
    /// 获取所有指定类型的元素
    /// </summary>
    private IEnumerable<T> GetAllElementsOfType<T>(ObservableCollection<CanvasElement> elements) where T : CanvasElement
    {
        foreach (var element in elements)
        {
            if (element is T typed)
                yield return typed;

            if (element.Children != null)
            {
                foreach (var child in GetAllElementsOfType<T>(element.Children))
                {
                    yield return child;
                }
            }
        }
    }

    /// <summary>
    /// 获取所有子元素
    /// </summary>
    private IEnumerable<CanvasElement> GetAllChildren(ObservableCollection<CanvasElement> elements)
    {
        foreach (var element in elements)
        {
            if (element.Children != null)
            {
                foreach (var child in element.Children)
                {
                    yield return child;
                    foreach (var grandChild in GetAllChildren(new ObservableCollection<CanvasElement> { child }))
                    {
                        yield return grandChild;
                    }
                }
            }
        }
    }
}
