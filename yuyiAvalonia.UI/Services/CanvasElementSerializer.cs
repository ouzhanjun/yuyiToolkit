using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Layout;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Services;

/// <summary>
/// 画布元素 JSON 序列化器
/// </summary>
public static class CanvasElementSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        Converters = { new CanvasElementJsonConverter() }
    };

    /// <summary>
    /// 反序列化选项（供外部使用）
    /// </summary>
    public static JsonSerializerOptions DeserializationOptions => _options;

    /// <summary>
    /// 保存画布到 JSON 文件
    /// </summary>
    public static bool SaveToFile(string filePath, IEnumerable<CanvasElement> elements, CanvasProperty canvasProperty, out string errorMessage)
    {
        errorMessage = string.Empty;

        try
        {
            var saveData = new CanvasSaveData
            {
                CanvasProperty = canvasProperty,
                Elements = new List<CanvasElement>(elements)
            };

            var json = JsonSerializer.Serialize(saveData, _options);
            System.IO.File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 从 JSON 文件加载画布
    /// </summary>
    public static CanvasSaveData? LoadFromFile(string filePath, out string errorMessage)
    {
        errorMessage = string.Empty;

        try
        {
            var json = System.IO.File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<CanvasSaveData>(json, _options);
            return data;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return null;
        }
    }
}

/// <summary>
/// 画布保存数据结构
/// </summary>
public class CanvasSaveData
{
    public CanvasProperty CanvasProperty { get; set; } = new();
    public List<CanvasElement> Elements { get; set; } = new();
}

/// <summary>
/// CanvasElement JSON 转换器
/// </summary>
public class CanvasElementJsonConverter : JsonConverter<CanvasElement>
{
    public override CanvasElement? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        if (!root.TryGetProperty("ElementType", out var typeProp))
            return null;

        ComponentType componentType;

        // 支持字符串和数字两种格式的 ElementType
        if (typeProp.ValueKind == JsonValueKind.String)
        {
            var typeValue = typeProp.GetString();
            if (string.IsNullOrEmpty(typeValue) || !Enum.TryParse<ComponentType>(typeValue, out componentType))
                return null;
        }
        else if (typeProp.ValueKind == JsonValueKind.Number)
        {
            if (!typeProp.TryGetInt32(out var typeInt) || !Enum.IsDefined(typeof(ComponentType), typeInt))
                return null;
            componentType = (ComponentType)typeInt;
        }
        else
        {
            return null;
        }

        CanvasElement element = componentType switch
        {
            ComponentType.Text => new TextElement(),
            ComponentType.Button => new ButtonElement(),
            ComponentType.Image => new ImageElement(),
            ComponentType.Card => new CardElement(),
            ComponentType.Grid => new GridElement(),
            ComponentType.VerticalStackPanel or ComponentType.HorizontalStackPanel => new StackPanelElement(),
            ComponentType.Panel => new PanelElement(),
            ComponentType.Canvas => new CanvasPanelElement(),
            ComponentType.Triangle => new TriangleContainerElement(),
            ComponentType.Rectangle => new RectangleContainerElement(),
            ComponentType.RoundedRectangle => new RoundedRectangleElement(),
            ComponentType.Ellipse => new EllipseContainerElement(),
            ComponentType.Parallelogram => new ParallelogramContainerElement(),
            ComponentType.Star => new RoundedStarElement(),
            ComponentType.RoundedPolygon => new RoundedPolygonElement(),
            ComponentType.Sector => new RoundEllipseElement(),
            _ => new CanvasElement()
        };

        // 基础属性
        element.SetId(GetStringOrDefault(root, "Id") ?? string.Empty);
        element.Name = GetStringOrDefault(root, "Name") ?? string.Empty;
        element.X = GetDoubleOrDefault(root, "X");
        element.Y = GetDoubleOrDefault(root, "Y");
        element.Width = GetDoubleOrDefault(root, "Width", 100);
        element.Height = GetDoubleOrDefault(root, "Height", 50);
        element.Rotation = GetDoubleOrDefault(root, "Rotation");
        element.ZIndex = GetIntOrDefault(root, "ZIndex");

        element.IsVisible = GetBoolOrDefault(root, "IsVisible", true);
        element.ShowIdLabel = GetBoolOrDefault(root, "ShowIdLabel");

        element.HorizontalAlignment = ParseHorizontalAlignment(GetStringOrDefault(root, "HorizontalAlignment"));
        element.VerticalAlignment = ParseVerticalAlignment(GetStringOrDefault(root, "VerticalAlignment"));
        element.Padding = GetDoubleOrDefault(root, "Padding", 8);
        element.Margin = GetDoubleOrDefault(root, "Margin");
        element.PaddingLeft = GetDoubleOrDefault(root, "PaddingLeft", 8);
        element.PaddingTop = GetDoubleOrDefault(root, "PaddingTop", 8);
        element.PaddingRight = GetDoubleOrDefault(root, "PaddingRight", 8);
        element.PaddingBottom = GetDoubleOrDefault(root, "PaddingBottom", 8);
        element.MarginLeft = GetDoubleOrDefault(root, "MarginLeft");
        element.MarginTop = GetDoubleOrDefault(root, "MarginTop");
        element.MarginRight = GetDoubleOrDefault(root, "MarginRight");
        element.MarginBottom = GetDoubleOrDefault(root, "MarginBottom");
        element.PaddingIsUniform = GetBoolOrDefault(root, "PaddingIsUniform", true);
        element.MarginIsUniform = GetBoolOrDefault(root, "MarginIsUniform", true);

        element.BackgroundFill.BackgroundColor = GetStringOrDefault(root, "BackgroundColor") ?? "#FFFFFF";
        element.Opacity = GetDoubleOrDefault(root, "Opacity", 1);
        element.BackgroundFill.ImagePath = GetStringOrDefault(root, "BackgroundImage") ?? string.Empty;
        element.BorderColor = (GetStringOrDefault(root, "BorderColor") is { } bc && !string.IsNullOrEmpty(bc) ? bc : null) ?? "#E5E5E5";
        element.BorderWidth = GetDoubleOrDefault(root, "BorderWidth");
        element.CornerRadius = GetDoubleOrDefault(root, "CornerRadius");

        element.BackgroundFill.FillType = ParseFillType(GetStringOrDefault(root, "FillType"));
        element.BackgroundFill.GradientStartColor = GetStringOrDefault(root, "GradientStartColor") ?? "#FFFFFF";
        element.BackgroundFill.GradientEndColor = GetStringOrDefault(root, "GradientEndColor") ?? "#07C160";
        element.BackgroundFill.GradientAngle = GetDoubleOrDefault(root, "GradientAngle");
        element.BackgroundFill.ImageStretch = ParseStretch(GetStringOrDefault(root, "BackgroundImageStretch"));
        element.BackgroundFill.ImageTileMode = ParseTileMode(GetStringOrDefault(root, "BackgroundImageTileMode"));
        element.BackgroundFill.ImageAlignmentX = ParseAlignmentX(GetStringOrDefault(root, "BackgroundImageAlignmentX"));
        element.BackgroundFill.ImageAlignmentY = ParseAlignmentY(GetStringOrDefault(root, "BackgroundImageAlignmentY"));

        element.Top = GetDoubleOrDefault(root, "Top", -1);
        element.Left = GetDoubleOrDefault(root, "Left", -1);
        element.Bottom = GetDoubleOrDefault(root, "Bottom", -1);
        element.Right = GetDoubleOrDefault(root, "Right", -1);
        element.FixLeft = GetBoolOrDefault(root, "FixLeft", true);
        element.FixTop = GetBoolOrDefault(root, "FixTop", true);
        element.FixBottom = GetBoolOrDefault(root, "FixBottom");
        element.FixRight = GetBoolOrDefault(root, "FixRight");

        element.GridRow = GetIntOrDefault(root, "GridRow", -1);
        element.GridColumn = GetIntOrDefault(root, "GridColumn", -1);

        // 加载子元素
        if (root.TryGetProperty("Children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array)
        {
            element.Children.Clear();
            foreach (var child in childrenProp.EnumerateArray())
            {
                var childJson = child.GetRawText();
                var childElement = JsonSerializer.Deserialize<CanvasElement>(childJson, options);
                if (childElement != null)
                {
                    childElement.Parent = element;
                    element.Children.Add(childElement);
                }
            }
        }

        // 特定元素属性 - 注意：ButtonElement 继承自 TextElement，需要先检查
        if (element is ButtonElement buttonElement)
        {
            // 加载 TextElement 的属性
            if (root.TryGetProperty("Text", out var buttonTextProp))
                buttonElement.Text = buttonTextProp.GetString() ?? "按钮";
            buttonElement.FontSize = GetDoubleOrDefault(root, "FontSize", 14);
            buttonElement.FontFamily = GetStringOrDefault(root, "FontFamily") ?? "Microsoft YaHei";
            buttonElement.IsBold = GetBoolOrDefault(root, "IsBold");
            buttonElement.IsItalic = GetBoolOrDefault(root, "IsItalic");
            buttonElement.IsUnderline = GetBoolOrDefault(root, "IsUnderline");
            buttonElement.IsStrikethrough = GetBoolOrDefault(root, "IsStrikethrough");
            buttonElement.ForeColor = GetStringOrDefault(root, "ForeColor") ?? "#FFFFFF";
            buttonElement.TextAlignment = ParseTextAlignment(GetStringOrDefault(root, "TextAlignment"));
            buttonElement.LineHeight = GetDoubleOrDefault(root, "LineHeight", double.NaN);
            buttonElement.LetterSpacing = GetDoubleOrDefault(root, "LetterSpacing");
            buttonElement.HasTextGradient = GetBoolOrDefault(root, "HasTextGradient");
            buttonElement.TextGradientStartColor = GetStringOrDefault(root, "TextGradientStartColor") ?? "#00f5d4";
            buttonElement.TextGradientEndColor = GetStringOrDefault(root, "TextGradientEndColor") ?? "#7b2fff";
            buttonElement.TextGradientAngle = GetDoubleOrDefault(root, "TextGradientAngle");

            // 加载 ButtonElement 的属性
            if (root.TryGetProperty("ButtonStyle", out var buttonStyleProp))
                buttonElement.ButtonStyle = ParseButtonStyle(buttonStyleProp.GetString());
        }
        else if (element is TextElement textElement)
        {
            if (root.TryGetProperty("Text", out var textProp))
                textElement.Text = textProp.GetString() ?? "双击编辑文本";
            textElement.FontSize = GetDoubleOrDefault(root, "FontSize", 14);
            textElement.FontFamily = GetStringOrDefault(root, "FontFamily") ?? "Microsoft YaHei";
            textElement.IsBold = GetBoolOrDefault(root, "IsBold");
            textElement.IsItalic = GetBoolOrDefault(root, "IsItalic");
            textElement.IsUnderline = GetBoolOrDefault(root, "IsUnderline");
            textElement.IsStrikethrough = GetBoolOrDefault(root, "IsStrikethrough");
            textElement.ForeColor = GetStringOrDefault(root, "ForeColor") ?? "#000000";
            textElement.TextAlignment = ParseTextAlignment(GetStringOrDefault(root, "TextAlignment"));
            textElement.LineHeight = GetDoubleOrDefault(root, "LineHeight", double.NaN);
            textElement.LetterSpacing = GetDoubleOrDefault(root, "LetterSpacing");
            textElement.HasTextGradient = GetBoolOrDefault(root, "HasTextGradient");
            textElement.TextGradientStartColor = GetStringOrDefault(root, "TextGradientStartColor") ?? "#00f5d4";
            textElement.TextGradientEndColor = GetStringOrDefault(root, "TextGradientEndColor") ?? "#7b2fff";
            textElement.TextGradientAngle = GetDoubleOrDefault(root, "TextGradientAngle");
        }

        if (element is ImageElement imageElement && root.TryGetProperty("ImagePath", out var imgPathProp))
        {
            imageElement.ImagePath = imgPathProp.GetString() ?? string.Empty;
            imageElement.ImageStretch = ParseStretch(GetStringOrDefault(root, "ImageStretch"));
            imageElement.ImageStretchDirection = ParseStretchDirection(GetStringOrDefault(root, "ImageStretchDirection"));
        }

        if (element is GridElement gridElement)
        {
            gridElement.Rows = GetIntOrDefault(root, "Rows", 3);
            gridElement.Columns = GetIntOrDefault(root, "Columns", 3);
            gridElement.RowSpacing = GetDoubleOrDefault(root, "RowSpacing", 5);
            gridElement.ColumnSpacing = GetDoubleOrDefault(root, "ColumnSpacing", 5);
            gridElement.RowDefinitions = GetStringOrDefault(root, "RowDefinitions") ?? string.Empty;
            gridElement.ColumnDefinitions = GetStringOrDefault(root, "ColumnDefinitions") ?? string.Empty;
        }

        if (element is StackPanelElement stackPanelElement)
        {
            stackPanelElement.Orientation = ParseStackPanelOrientation(GetStringOrDefault(root, "Orientation"));
            stackPanelElement.Spacing = GetDoubleOrDefault(root, "Spacing");
        }

        if (element is CardElement cardElement)
        {
            cardElement.Spacing = GetDoubleOrDefault(root, "Spacing");
        }

        if (element is RoundEllipseElement sectorElement)
        {
            sectorElement.StartAngle = GetDoubleOrDefault(root, "StartAngle");
            sectorElement.SweepAngle = GetDoubleOrDefault(root, "SweepAngle", 360);
            sectorElement.InnerRadius = GetDoubleOrDefault(root, "InnerRadius");
            sectorElement.CornerRadius = GetDoubleOrDefault(root, "CornerRadius");
        }

        return element;
    }

    public override void Write(Utf8JsonWriter writer, CanvasElement value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("ElementType", value.ElementType.ToString());
        writer.WriteString("Id", value.Id);
        writer.WriteString("Name", value.Name);
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Width", value.Width);
        writer.WriteNumber("Height", value.Height);
        writer.WriteNumber("Rotation", value.Rotation);
        writer.WriteNumber("ZIndex", value.ZIndex);

        writer.WriteBoolean("IsVisible", value.IsVisible);
        writer.WriteBoolean("ShowIdLabel", value.ShowIdLabel);

        writer.WriteString("HorizontalAlignment", value.HorizontalAlignment.ToString());
        writer.WriteString("VerticalAlignment", value.VerticalAlignment.ToString());
        writer.WriteNumber("Padding", value.Padding);
        writer.WriteNumber("Margin", value.Margin);
        writer.WriteNumber("PaddingLeft", value.PaddingLeft);
        writer.WriteNumber("PaddingTop", value.PaddingTop);
        writer.WriteNumber("PaddingRight", value.PaddingRight);
        writer.WriteNumber("PaddingBottom", value.PaddingBottom);
        writer.WriteNumber("MarginLeft", value.MarginLeft);
        writer.WriteNumber("MarginTop", value.MarginTop);
        writer.WriteNumber("MarginRight", value.MarginRight);
        writer.WriteNumber("MarginBottom", value.MarginBottom);
        writer.WriteBoolean("PaddingIsUniform", value.PaddingIsUniform);
        writer.WriteBoolean("MarginIsUniform", value.MarginIsUniform);

        writer.WriteString("BackgroundColor", value.BackgroundFill.BackgroundColor);
        writer.WriteNumber("Opacity", value.Opacity);
        writer.WriteString("BackgroundImage", value.BackgroundFill.ImagePath);
        if (!string.IsNullOrEmpty(value.BorderColor))
            writer.WriteString("BorderColor", value.BorderColor);
        if (value.BorderWidth > 0)
            writer.WriteNumber("BorderWidth", value.BorderWidth);
        writer.WriteNumber("CornerRadius", value.CornerRadius);

        writer.WriteString("FillType", value.BackgroundFill.FillType.ToString());
        writer.WriteString("GradientStartColor", value.BackgroundFill.GradientStartColor);
        writer.WriteString("GradientEndColor", value.BackgroundFill.GradientEndColor);
        writer.WriteNumber("GradientAngle", value.BackgroundFill.GradientAngle);
        writer.WriteString("BackgroundImageStretch", value.BackgroundFill.ImageStretch.ToString());
        writer.WriteString("BackgroundImageTileMode", value.BackgroundFill.ImageTileMode.ToString());
        writer.WriteString("BackgroundImageAlignmentX", value.BackgroundFill.ImageAlignmentX.ToString());
        writer.WriteString("BackgroundImageAlignmentY", value.BackgroundFill.ImageAlignmentY.ToString());

        writer.WriteNumber("Top", value.Top);
        writer.WriteNumber("Left", value.Left);
        writer.WriteNumber("Bottom", value.Bottom);
        writer.WriteNumber("Right", value.Right);
        writer.WriteBoolean("FixLeft", value.FixLeft);
        writer.WriteBoolean("FixTop", value.FixTop);
        writer.WriteBoolean("FixBottom", value.FixBottom);
        writer.WriteBoolean("FixRight", value.FixRight);

        writer.WriteNumber("GridRow", value.GridRow);
        writer.WriteNumber("GridColumn", value.GridColumn);

        // 子元素
        if (value.Children != null && value.Children.Count > 0)
        {
            writer.WritePropertyName("Children");
            writer.WriteStartArray();
            foreach (var child in value.Children)
            {
                JsonSerializer.Serialize(writer, child, options);
            }
            writer.WriteEndArray();
        }

        // 特定元素属性
        switch (value)
        {
            case ButtonElement button:
                writer.WriteString("Text", button.Text);
                writer.WriteNumber("FontSize", button.FontSize);
                writer.WriteString("FontFamily", button.FontFamily);
                writer.WriteBoolean("IsBold", button.IsBold);
                writer.WriteBoolean("IsItalic", button.IsItalic);
                writer.WriteBoolean("IsUnderline", button.IsUnderline);
                writer.WriteBoolean("IsStrikethrough", button.IsStrikethrough);
                writer.WriteString("ForeColor", button.ForeColor);
                writer.WriteString("TextAlignment", button.TextAlignment.ToString());
                if (!double.IsNaN(button.LineHeight))
                    writer.WriteNumber("LineHeight", button.LineHeight);
                writer.WriteNumber("LetterSpacing", button.LetterSpacing);
                writer.WriteBoolean("HasTextGradient", button.HasTextGradient);
                writer.WriteString("TextGradientStartColor", button.TextGradientStartColor);
                writer.WriteString("TextGradientEndColor", button.TextGradientEndColor);
                writer.WriteNumber("TextGradientAngle", button.TextGradientAngle);
                writer.WriteString("ButtonStyle", button.ButtonStyle.ToString());
                break;

            case TextElement text:
                writer.WriteString("Text", text.Text);
                writer.WriteNumber("FontSize", text.FontSize);
                writer.WriteString("FontFamily", text.FontFamily);
                writer.WriteBoolean("IsBold", text.IsBold);
                writer.WriteBoolean("IsItalic", text.IsItalic);
                writer.WriteBoolean("IsUnderline", text.IsUnderline);
                writer.WriteBoolean("IsStrikethrough", text.IsStrikethrough);
                writer.WriteString("ForeColor", text.ForeColor);
                writer.WriteString("TextAlignment", text.TextAlignment.ToString());
                if (!double.IsNaN(text.LineHeight))
                    writer.WriteNumber("LineHeight", text.LineHeight);
                writer.WriteNumber("LetterSpacing", text.LetterSpacing);
                writer.WriteBoolean("HasTextGradient", text.HasTextGradient);
                writer.WriteString("TextGradientStartColor", text.TextGradientStartColor);
                writer.WriteString("TextGradientEndColor", text.TextGradientEndColor);
                writer.WriteNumber("TextGradientAngle", text.TextGradientAngle);
                break;

            case ImageElement image:
                writer.WriteString("ImagePath", image.ImagePath);
                writer.WriteString("ImageStretch", image.ImageStretch.ToString());
                writer.WriteString("ImageStretchDirection", image.ImageStretchDirection.ToString());
                break;

            case GridElement grid:
                writer.WriteNumber("Rows", grid.Rows);
                writer.WriteNumber("Columns", grid.Columns);
                writer.WriteNumber("RowSpacing", grid.RowSpacing);
                writer.WriteNumber("ColumnSpacing", grid.ColumnSpacing);
                writer.WriteString("RowDefinitions", grid.RowDefinitions);
                writer.WriteString("ColumnDefinitions", grid.ColumnDefinitions);
                break;

            case StackPanelElement stackPanel:
                writer.WriteString("Orientation", stackPanel.Orientation.ToString());
                writer.WriteNumber("Spacing", stackPanel.Spacing);
                break;

            case CardElement card:
                writer.WriteNumber("Spacing", card.Spacing);
                break;

            case RoundEllipseElement sector:
                writer.WriteNumber("StartAngle", sector.StartAngle);
                writer.WriteNumber("SweepAngle", sector.SweepAngle);
                writer.WriteNumber("InnerRadius", sector.InnerRadius);
                writer.WriteNumber("CornerRadius", sector.CornerRadius);
                break;
        }

        writer.WriteEndObject();
    }

    // 辅助解析方法
    private static HorizontalAlignment ParseHorizontalAlignment(string? value) => value?.ToLower() switch
    {
        "left" => HorizontalAlignment.Left,
        "center" => HorizontalAlignment.Center,
        "right" => HorizontalAlignment.Right,
        "stretch" => HorizontalAlignment.Stretch,
        _ => HorizontalAlignment.Left
    };

    private static VerticalAlignment ParseVerticalAlignment(string? value) => value?.ToLower() switch
    {
        "top" => VerticalAlignment.Top,
        "center" => VerticalAlignment.Center,
        "bottom" => VerticalAlignment.Bottom,
        "stretch" => VerticalAlignment.Stretch,
        _ => VerticalAlignment.Top
    };

    private static FillType ParseFillType(string? value) => value?.ToLower() switch
    {
        "none" => FillType.None,
        "solid" => FillType.Solid,
        "gradient" => FillType.Gradient,
        "image" => FillType.Image,
        _ => FillType.Solid
    };

    private static Avalonia.Media.Stretch ParseStretch(string? value) => value?.ToLower() switch
    {
        "none" => Avalonia.Media.Stretch.None,
        "fill" => Avalonia.Media.Stretch.Fill,
        "uniform" => Avalonia.Media.Stretch.Uniform,
        "uniformtofill" => Avalonia.Media.Stretch.UniformToFill,
        _ => Avalonia.Media.Stretch.Uniform
    };

    private static Avalonia.Media.TileMode ParseTileMode(string? value) => value?.ToLower() switch
    {
        "none" => Avalonia.Media.TileMode.None,
        "flipx" => Avalonia.Media.TileMode.FlipX,
        "flipy" => Avalonia.Media.TileMode.FlipY,
        "flipxy" => Avalonia.Media.TileMode.FlipXY,
        "tile" => Avalonia.Media.TileMode.Tile,
        _ => Avalonia.Media.TileMode.None
    };

    private static AlignmentX ParseAlignmentX(string? value) => value?.ToLower() switch
    {
        "left" => AlignmentX.Left,
        "center" => AlignmentX.Center,
        "right" => AlignmentX.Right,
        _ => AlignmentX.Left
    };

    private static AlignmentY ParseAlignmentY(string? value) => value?.ToLower() switch
    {
        "top" => AlignmentY.Top,
        "center" => AlignmentY.Center,
        "bottom" => AlignmentY.Bottom,
        _ => AlignmentY.Top
    };

    private static TextAlignment ParseTextAlignment(string? value) => value?.ToLower() switch
    {
        "left" => TextAlignment.Left,
        "center" => TextAlignment.Center,
        "right" => TextAlignment.Right,
        "justify" => TextAlignment.Justify,
        _ => TextAlignment.Center
    };

    private static ButtonStyle ParseButtonStyle(string? value) => value?.ToLower() switch
    {
        "primary" => ButtonStyle.Primary,
        "secondary" => ButtonStyle.Secondary,
        "outline" => ButtonStyle.Outline,
        "text" => ButtonStyle.Text,
        _ => ButtonStyle.Primary
    };

    private static Avalonia.Media.StretchDirection ParseStretchDirection(string? value) => value?.ToLower() switch
    {
        "uponly" => Avalonia.Media.StretchDirection.UpOnly,
        "downonly" => Avalonia.Media.StretchDirection.DownOnly,
        "both" => Avalonia.Media.StretchDirection.Both,
        _ => Avalonia.Media.StretchDirection.Both
    };

    private static StackPanelOrientation ParseStackPanelOrientation(string? value) => value?.ToLower() switch
    {
        "vertical" => StackPanelOrientation.Vertical,
        "horizontal" => StackPanelOrientation.Horizontal,
        _ => StackPanelOrientation.Vertical
    };

    // 辅助方法 - 使用 JsonElement 内置方法，无需反射
    private static double GetDoubleOrDefault(JsonElement element, string propertyName, double defaultValue = 0)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.TryGetDouble(out var value)
            ? value
            : defaultValue;
    }

    private static int GetIntOrDefault(JsonElement element, string propertyName, int defaultValue = 0)
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var value)
            ? value
            : defaultValue;
    }

    private static bool GetBoolOrDefault(JsonElement element, string propertyName, bool defaultValue = false)
    {
        return element.TryGetProperty(propertyName, out var prop) && (prop.ValueKind == JsonValueKind.True || prop.ValueKind == JsonValueKind.False)
            ? prop.GetBoolean()
            : defaultValue;
    }

    private static string? GetStringOrDefault(JsonElement element, string propertyName, string defaultValue = "")
    {
        return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() ?? defaultValue
            : defaultValue;
    }
}