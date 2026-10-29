using Avalonia;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.ElementControls.Themes;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls.ElementFlow;

public class FlowUIControlCreator: IUIControlCreator
{
    private Panel _topLevelContainer;

    private bool _showElementId = false;

    private Dictionary<CanvasElement, Control> _elementControlMap;

    private ObservableCollection<CanvasElement> _canvasElements = null;

    public Dictionary<CanvasElement, Control> ElementControlMap
    {
        get
        {
            if (_elementControlMap == null)
            {
                _elementControlMap = new Dictionary<CanvasElement, Control>();
            }
            return _elementControlMap;
        }
    }

    /// <summary>
    /// 生成唯一的名称和ID
    /// </summary>
    public static string GenerateUniqueName(ComponentType type)
    {
        if (!_counters.TryGetValue(type, out var counter))
        {
            counter = 1;
        }
        else
        {
            counter++;
        }
        _counters[type] = counter;

        var typeName = GetTypeName(type);
        return $"{typeName}_{counter}";
    }

    public bool ShowElementId { get => _showElementId; set => _showElementId = value; }

    public bool TryGetControl(CanvasElement element, out Control? control)
    {
        if (ElementControlMap.TryGetValue(element, out control))
        {
            return true;
        }

        control = null;
        return false;
    }

    // 静态计数器字典，为每种组件类型维护独立的计数器
    private static readonly Dictionary<ComponentType, int> _counters = new();

    // 组件类型的中文名称映射
    private static readonly Dictionary<ComponentType, string> _typeNames = new()
        {
            { ComponentType.Text, "Text" },
            { ComponentType.Card, "Card" },
            { ComponentType.Button, "Button" },
            { ComponentType.VerticalStackPanel, "VerticalStackPanel" },
            { ComponentType.HorizontalStackPanel, "HorizontalStackPanel" },
            { ComponentType.Panel, "Panel" },
            { ComponentType.Grid, "Grid" },
            { ComponentType.Canvas, "Canvas" },
            { ComponentType.Image, "Image" },
            { ComponentType.Triangle, "Triangle" },
            { ComponentType.Rectangle, "BasicRectangle" },
            { ComponentType.RoundedRectangle, "RoundedRectangle" },
            { ComponentType.RoundedRectangleStackPanel, "RoundedRectangleStackPanel" },
            { ComponentType.RoundedRectangleCanvas, "RoundedRectangleCanvas" },
            { ComponentType.RoundedRectangleGrid, "RoundedRectangleGrid" },
            { ComponentType.Ellipse, "Ellipse" },
            { ComponentType.Parallelogram, "Parallelogram" },
            { ComponentType.Star, "SixStar" },
            { ComponentType.RoundedPolygon, "RoundedPolygon" },
            { ComponentType.Sector, "Sector" },
        };

    public FlowUIControlCreator(Panel topPanel, ObservableCollection<CanvasElement> canvasElements)
    {
        _topLevelContainer = topPanel;
        _canvasElements = canvasElements;
    }

    /// <summary>
    /// 获取组件类型的中文名称
    /// </summary>
    public static string GetTypeName(ComponentType type)
    {
        return _typeNames.TryGetValue(type, out var name) ? name : type.ToString();
    }

    /// <summary>
    /// 重置所有计数器（用于清空画布后）
    /// </summary>
    public static void ResetAllCounters()
    {
        _counters.Clear();
    }

    /// <summary>
    /// 从组件定义创建元素（使用主题）
    /// </summary>
    public CanvasElement CreateElement(ComponentDefinition definition)
    {
        // 使用主题获取基础样式
        var theme = ThemeProvider.Instance;
        var themeStyle = theme.GetStyle(definition.Type, ComponentState.Normal);

        // 如果定义中有默认值，优先使用；否则使用主题样式
        var defaults = definition.Defaults!;

        // 生成唯一的名称和ID
        var uniqueName = GenerateUniqueName(definition.Type);

        CanvasElement element = definition.Type switch
        {
            ComponentType.Text => CreateTextElement(defaults, themeStyle, definition.Name, uniqueName),
            ComponentType.Card => CreateCardElement(defaults, themeStyle, uniqueName),
            ComponentType.Button => CreateButtonElement(defaults, themeStyle, definition.Name, uniqueName),
            ComponentType.VerticalStackPanel => CreateVerticalStackPanelElement(defaults, themeStyle, uniqueName),
            ComponentType.HorizontalStackPanel => CreateHorizontalStackPanelElement(defaults, themeStyle, uniqueName),
            ComponentType.Panel => CreatePanelElement(defaults, themeStyle, uniqueName),
            ComponentType.Grid => CreateGridElementFromDefinition(definition, themeStyle),
            ComponentType.Canvas => CreateCanvasElement(defaults, themeStyle, uniqueName),
            ComponentType.Image => CreateImageElement(defaults, themeStyle, uniqueName),
            ComponentType.Triangle => CreateTriangleContainerElement(defaults, uniqueName),
            ComponentType.Rectangle => CreateRectangleContainerElement(defaults, uniqueName),
            ComponentType.RoundedRectangle => CreateRoundedRectangleContainerElement(defaults, uniqueName),
            ComponentType.RoundedRectangleStackPanel => CreateRoundedRectangleStackPanelContainerElement(defaults, uniqueName),
            ComponentType.RoundedRectangleCanvas => CreateRoundedRectangleCanvasContainerElement(defaults, uniqueName),
            ComponentType.RoundedRectangleGrid => CreateRoundedRectangleGridContainerElement(defaults, uniqueName),
            ComponentType.Ellipse => CreateEllipseContainerElement(defaults, uniqueName),
            ComponentType.Parallelogram => CreateParallelogramContainerElement(defaults, uniqueName),
            ComponentType.Star => CreateStarContainerElement(defaults, uniqueName),
            ComponentType.RoundedPolygon => CreateRoundedPolygonContainerElement(defaults, uniqueName),
            ComponentType.Sector => CreateSectorContainerElement(defaults, uniqueName),
            _ => new CanvasElement { Width = defaults.Width, Height = defaults.Height }
        };

        // 设置唯一的Id
        element.SetId(uniqueName);
        return element;
    }

    #region 元素创建方法

    private static TextElement CreateTextElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string name, string uniqueName)
    {
        return new TextElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : themeStyle.FontSize * 10,
            Height = defaults.Height > 0 ? defaults.Height : themeStyle.FontSize * 2,
            Text = name,
            FontSize = defaults.FontSize > 0 ? defaults.FontSize : themeStyle.FontSize,
            ForeColor = !string.IsNullOrEmpty(defaults.ForeColor) && defaults.ForeColor != "#000000" ? defaults.ForeColor : themeStyle.ForeColor ?? "#333333",
            IsBold = defaults.IsBold || themeStyle.IsBold,
            IsItalic = defaults.IsItalic || themeStyle.IsItalic,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
        };
    }

    private static CardElement CreateCardElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string uniqueName)
    {
        return new CardElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 200,
            Height = defaults.Height > 0 ? defaults.Height : 100,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) && defaults.BackgroundColor != "#FFFFFF" ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#FFFFFF" },
            BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) && defaults.BorderColor != "#E5E5E5" ? defaults.BorderColor : themeStyle.BorderColor ?? "#DEE2E6",
            BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
            CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
        };
    }

    private static ButtonElement CreateButtonElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string name, string uniqueName)
    {
        return new ButtonElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 40,
            Text = name,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) && defaults.BackgroundColor != "#FFFFFF" ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#007BFF" },
            ForeColor = !string.IsNullOrEmpty(defaults.ForeColor) && defaults.ForeColor != "#000000" ? defaults.ForeColor : themeStyle.ForeColor ?? "#FFFFFF",
            BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) ? defaults.BorderColor : themeStyle.BorderColor,
            BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
            CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
            FontSize = defaults.FontSize > 0 ? defaults.FontSize : themeStyle.FontSize,
            IsBold = defaults.IsBold || themeStyle.IsBold,
            IsItalic = defaults.IsItalic || themeStyle.IsItalic,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
        };
    }

    private static StackPanelElement CreateVerticalStackPanelElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string uniqueName)
    {
        return new StackPanelElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 300,
            Height = defaults.Height > 0 ? defaults.Height : 200,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) && defaults.BackgroundColor != "#FFFFFF" ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#FAFAFA" },
            BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) && defaults.BorderColor != "#E5E5E5" ? defaults.BorderColor : themeStyle.BorderColor ?? "#E0E0E0",
            BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
            CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
            Orientation = StackPanelOrientation.Vertical,
        };
    }

    private static StackPanelElement CreateHorizontalStackPanelElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string uniqueName)
    {
        return new StackPanelElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 300,
            Height = defaults.Height > 0 ? defaults.Height : 50,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) && defaults.BackgroundColor != "#FFFFFF" ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#FAFAFA" },
            BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) && defaults.BorderColor != "#E5E5E5" ? defaults.BorderColor : themeStyle.BorderColor ?? "#E0E0E0",
            BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
            CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
            Orientation = StackPanelOrientation.Horizontal,
        };
    }

    private static PanelElement CreatePanelElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string uniqueName)
    {
        return new PanelElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 300,
            Height = defaults.Height > 0 ? defaults.Height : 200,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) && defaults.BackgroundColor != "#FFFFFF" ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#FFFFFF" },
            BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) && defaults.BorderColor != "#E5E5E5" ? defaults.BorderColor : themeStyle.BorderColor ?? "#E0E0E0",
            BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
            CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
        };
    }

    private static CanvasPanelElement CreateCanvasElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string uniqueName)
    {
        return new CanvasPanelElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 300,
            Height = defaults.Height > 0 ? defaults.Height : 200,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) && defaults.BackgroundColor != "#FFFFFF" ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#FFFFFF" },
            BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) && defaults.BorderColor != "#E5E5E5" ? defaults.BorderColor : themeStyle.BorderColor ?? "#E0E0E0",
            BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
            CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
            IsContainer = true,
            ElementType = ComponentType.Canvas,
        };
    }

    private static ImageElement CreateImageElement(ComponentDefaults defaults, ComponentThemeStyle themeStyle, string uniqueName)
    {
        return new ImageElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 120,
            ImageStretch = defaults.ImageStretch,
            Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
            Margin = defaults.Margin,
        };
    }

    private static TriangleContainerElement CreateTriangleContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new TriangleContainerElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 100,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#FF6B6B" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            //StrokeThickness = defaults.StrokeThickness >= 0 ? defaults.StrokeThickness : 2,
            IsContainer = true
        };
    }

    private static RectangleContainerElement CreateRectangleContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RectangleContainerElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 80,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#4ECDC4" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            //StrokeThickness = defaults.StrokeThickness >= 0 ? defaults.StrokeThickness : 2,
            IsContainer = true
        };
    }

    private static RoundedRectangleElement CreateRoundedRectangleContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundedRectangleElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 80,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#4ECDC4" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            //StrokeThickness = defaults.StrokeThickness >= 0 ? defaults.StrokeThickness : 2,
            IsContainer = true
        };
    }

    private static RoundedRectangleStackPanelElement CreateRoundedRectangleStackPanelContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundedRectangleStackPanelElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 320,
            Height = defaults.Height > 0 ? defaults.Height : 200,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#4ECDC4" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            IsContainer = true,
            Orientation = StackPanelOrientation.Vertical,
            Spacing = defaults.Padding,
        };
    }

    private static RoundedRectangleCanvasElement CreateRoundedRectangleCanvasContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundedRectangleCanvasElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 320,
            Height = defaults.Height > 0 ? defaults.Height : 200,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#4ECDC4" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            IsContainer = true,
        };
    }

    private static RoundedRectangleGridElement CreateRoundedRectangleGridContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundedRectangleGridElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 320,
            Height = defaults.Height > 0 ? defaults.Height : 200,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#4ECDC4" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            IsContainer = true,
        };
    }

    private static EllipseContainerElement CreateEllipseContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new EllipseContainerElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 100,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#45B7D1" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            //StrokeThickness = defaults.StrokeThickness >= 0 ? defaults.StrokeThickness : 2,
            IsContainer = true
        };
    }

    private static ParallelogramContainerElement CreateParallelogramContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new ParallelogramContainerElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 140,
            Height = defaults.Height > 0 ? defaults.Height : 80,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#96CEB4" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            //StrokeThickness = defaults.StrokeThickness >= 0 ? defaults.StrokeThickness : 2,
            IsContainer = true
        };
    }

    private static RoundedStarElement CreateStarContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundedStarElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 140,
            Height = defaults.Height > 0 ? defaults.Height : 140,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#9C27B0" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            //StrokeThickness = defaults.StrokeThickness >= 0 ? defaults.StrokeThickness : 2,
            IsContainer = true
        };
    }

    private static RoundedPolygonElement CreateRoundedPolygonContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundedPolygonElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 120,
            Height = defaults.Height > 0 ? defaults.Height : 100,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#FF9F43" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            IsContainer = true
        };
    }

    private static RoundEllipseElement CreateSectorContainerElement(ComponentDefaults defaults, string uniqueName)
    {
        return new RoundEllipseElement
        {
            Id = uniqueName,
            Name = uniqueName,
            Width = defaults.Width > 0 ? defaults.Width : 140,
            Height = defaults.Height > 0 ? defaults.Height : 140,
            BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : "#7B61FF" },
            StrokeColor = !string.IsNullOrEmpty(defaults.StrokeColor) ? defaults.StrokeColor : "#333333",
            IsContainer = true
        };
    }

    /// <summary>
    /// 根据组件定义创建 Grid 元素（支持布局模板）
    /// </summary>
    private static GridElement CreateGridElementFromDefinition(ComponentDefinition definition, ComponentThemeStyle themeStyle)
    {
        var defaults = definition.Defaults!;
        var name = definition.Name;

        GridElement element = name switch
        {
            // 默认 Grid 容器
            _ => new GridElement
            {
                BackgroundFill = new FillItem { BackgroundColor = !string.IsNullOrEmpty(defaults.BackgroundColor) ? defaults.BackgroundColor : themeStyle.BackgroundColor ?? "#FFFFFF" },
                BorderColor = !string.IsNullOrEmpty(defaults.BorderColor) ? defaults.BorderColor : themeStyle.BorderColor ?? "#E0E0E0",
                BorderWidth = defaults.BorderWidth >= 0 ? defaults.BorderWidth : themeStyle.BorderWidth,
                CornerRadius = defaults.CornerRadius >= 0 ? defaults.CornerRadius : themeStyle.CornerRadius,
                Padding = defaults.Padding >= 0 ? defaults.Padding : themeStyle.Padding,
                Margin = defaults.Margin,
                IsContainer = true,
                ElementType = ComponentType.Grid,
            }
        };

        // 更新基础属性
        element.Width = defaults.Width > 0 ? defaults.Width : 300;
        element.Height = defaults.Height > 0 ? defaults.Height : 200;

        // 自动生成唯一的Id
        element.SetId(GenerateUniqueName(ComponentType.Grid));

        return element;
    }

    #endregion

    public Control? CreateElementControl(ComponentDefinition definition, out CanvasElement element, Panel? container = null)
    {
        element = CreateElement(definition);
        return CreateElementControl(element, container);
    }

    /// <summary>
    /// 从CanvasElement创建控件
    /// </summary>
    public Control? CreateElementControl(CanvasElement element, Panel? container = null)
    {
        Control? control = null;

        if (element == null)
            return control;

        switch (element.ElementType)
        {
            case ComponentType.Text:
                TextElement textElement = (TextElement)element;
                control = new TextControl(textElement);
                break;

            case ComponentType.Card:
                CardElement cardElement = (CardElement)element;
                control = new CardControl(cardElement);
                break;

            case ComponentType.Button:
                ButtonElement buttonElement = (ButtonElement)element;
                control = new ButtonControl(buttonElement);
                break;

            case ComponentType.VerticalStackPanel:
            case ComponentType.HorizontalStackPanel:
                StackPanelElement stackPanelElement = (StackPanelElement)element;
                control = CreateStackPanelElementControl(stackPanelElement);
                break;

            case ComponentType.Panel:
                PanelElement panelElement = (PanelElement)element;
                control = CreatePanelElementControl(panelElement);
                break;

            case ComponentType.Grid:
                GridElement gridElement = (GridElement)element;
                control = CreateGridElementControl(gridElement);
                break;

            case ComponentType.Image:
                ImageElement imageElement = (ImageElement)element;
                control = new ImageControl(imageElement);
                break;

            case ComponentType.Triangle:
                TriangleContainerElement triangleContainerElement = (TriangleContainerElement)element;
                control = new TriangleFlowControl(triangleContainerElement);
                break;

            case ComponentType.Rectangle:
                RectangleContainerElement rectContainerElement = (RectangleContainerElement)element;
                control = new RectangleFlowControl(rectContainerElement);
                break;

            case ComponentType.RoundedRectangle:
                RoundedRectangleElement roundedRectContainerElement = (RoundedRectangleElement)element;
                control = new RoundedRectangleControl(roundedRectContainerElement);
                break;

            case ComponentType.RoundedRectangleStackPanel:
                RoundedRectangleStackPanelElement roundedRectStackElement = (RoundedRectangleStackPanelElement)element;
                control = new RoundedRectangleStackPanelControl(roundedRectStackElement);
                break;

            case ComponentType.RoundedRectangleCanvas:
                RoundedRectangleCanvasElement roundedRectCanvasElement = (RoundedRectangleCanvasElement)element;
                control = new RoundedRectangleCanvasControl(roundedRectCanvasElement);
                break;

            case ComponentType.RoundedRectangleGrid:
                RoundedRectangleGridElement roundedRectGridElement = (RoundedRectangleGridElement)element;
                control = new RoundedRectangleGridControl(roundedRectGridElement);
                break;

            case ComponentType.Ellipse:
                EllipseContainerElement ellipseContainerElement = (EllipseContainerElement)element;
                control = new EllipseFlowControl(ellipseContainerElement);
                break;

            case ComponentType.Parallelogram:
                ParallelogramContainerElement paraContainerElement = (ParallelogramContainerElement)element;
                control = new ParallelogramFlowControl(paraContainerElement);
                break;

            case ComponentType.Star:
                RoundedStarElement sixStarContainerElement = (RoundedStarElement)element;
                control = new RoundedStarControl(sixStarContainerElement);
                break;

            case ComponentType.RoundedPolygon:
                RoundedPolygonElement roundedPolygonContainerElement = (RoundedPolygonElement)element;
                control = new RoundedPolygonControl(roundedPolygonContainerElement);
                break;

            case ComponentType.Sector:
                RoundEllipseElement sectorContainerElement = (RoundEllipseElement)element;
                control = new RoundEllipseControl(sectorContainerElement);
                break;

            case ComponentType.Canvas:
                control = CreateCanvasElementControl(element);
                break;
        }

        if (control != null)
        {
            control.Tag = element;
            ElementControlMap.Add(element, control);
        }

        if (container is Panel panel && control != null)
        {
            panel.Children.Add(control);
            if (panel == _topLevelContainer)
            {
                // 如果是直接添加到画布上的元素，设置初始位置
                Canvas.SetLeft(control, element.X);
                Canvas.SetTop(control, element.Y);
            }
            else if (panel is Canvas)
            {
                Canvas.SetLeft(control, element.Left);
                Canvas.SetTop(control, element.Top);
            }
            else if (panel is Grid)
            {
                // 将控件添加到 Grid 的指定单元格
                Grid.SetRow(control, element.GridRow);
                Grid.SetColumn(control, element.GridColumn);
            }
        }

        if (control != null)
        {
            // 只对容器控件订阅 LayoutUpdated，Canvas 子控件不需要
            if (IsContainer(control))
            {
                control.LayoutUpdated += Control_LayoutUpdated;
                control.SizeChanged += Control_SizeChanged;
            }
            element.ShowIdLabel = this.ShowElementId;
            ((IElementControlBase?)control)?.UpdateIdLabel(element);
        }

        return control;
    }

    private void Control_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is BorderControlBase elementControl)
        {
            if (elementControl.IsShowOverlay)
            {
                elementControl.ShowWorkspaceOverlay();
            }
            if (elementControl.Control is Canvas panel)
            {
                var lst = panel.Children.OrderBy(c => c.ZIndex).ToList();

                HandleSizeChild(lst);

            }
        }
    }

    public void DeleteElement(CanvasElement element)
    {
        if (element == null)
            return;

        // 从父容器删除控件
        if (this.ElementControlMap.TryGetValue(element, out var control))
        {
            control.Tag = null;

            if (IsContainer(control))
            {
                control.LayoutUpdated -= Control_LayoutUpdated;
                control.SizeChanged -= Control_SizeChanged;
            }

            if (control.Parent is Panel parent)
            {
                parent.Children.Remove(control);
            }
        }

        // 从父元素中移除父子关系
        if (this.ElementControlMap.ContainsKey(element))
        {
            this.ElementControlMap.Remove(element);
        }

        ObservableCollection<CanvasElement>? parentCanvasElements = null;
        if (element.Parent != null)
        {
            if (element.Parent!.Children != null && element.Parent.Children!.Contains(element))
            {
                element.Parent.Children!.Remove(element);
            }
            element.Parent = null;
        }

        if (element.Children != null)
        {
            List<CanvasElement> list = element.Children.ToList();
            // 删除子元素和对应的控件
            foreach (var childElement in list)
            {
                DeleteElement(childElement);
            }
        }
    }

    private static bool IsContainer(Control control)
    {
        return control is StackPanelControl || control is PanelControl || control is CardControl || control is GridControl || control is CanvasContainerControl
            || control is TriangleFlowControl || control is RectangleFlowControl || control is RoundedRectangleControl || control is EllipseFlowControl || control is ParallelogramFlowControl
            || control is RoundedStarControl || control is RoundedPolygonControl || control is RoundEllipseControl;
    }

    private static void HandleSizeChild(List<Control> lst)
    {
        for (int i = 0; i < lst.Count; i++)
        {
            BorderControlBase grandChild = lst[i] as BorderControlBase;

            if (grandChild != null && grandChild.Tag is CanvasElement childElement)
            {
                grandChild.ParentSizeChanged();
                if (grandChild.Control is Canvas panel)
                {
                    var lst2 = panel.Children.OrderBy(c => c.ZIndex).ToList();

                    HandleSizeChild(lst2);

                }
            }
        }
    }

    private void Control_LayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is BorderControlBase elementControl)
        {
            if (elementControl.Control is Panel panel)
            {
                var lst = panel.Children.OrderBy(c => c.ZIndex).ToList();
                for (int i = 0; i < lst.Count; i++)
                {
                    var grandChild = lst[i];

                    if (grandChild.Tag is CanvasElement childElement)
                    {
                        LayoutControl(grandChild, childElement);
                    }
                }

            }
        }
    }

    /// <summary>
    /// 完成容器内部控件位置的更新，更新排序号ZIndex，更新元素的X,Y坐标等。由于控件的Bounds是在布局完成后才有值，所以需要在控件的LayoutUpdated事件中进行更新。
    /// </summary>
    /// <param name="child"></param>
    /// <param name="element"></param>
    private void LayoutControl(Control child, CanvasElement element)
    {

        var pos = ((Panel)child.Parent!).TranslatePoint(new Point(0, 0), this._topLevelContainer);
        element.X = pos!.Value.X + child.Bounds.X;
        element.Y = pos.Value.Y + child.Bounds.Y;

        //var pos = child.TranslatePoint(new Point(0, 0), this._topLevelContainer);
        //element.X = pos!.Value.X ;
        //element.Y = pos.Value.Y ;

        element.Width = child.Bounds.Width;
        element.Height = child.Bounds.Height;

        if (element.IsContainer && element.Children.Count > 0)
        {
            if (((BorderControlBase)child).Control is Panel panel)
            {
                var lst = panel.Children.OrderBy(c => c.ZIndex).ToList();
                for (int i = 0; i < lst.Count; i++)
                {
                    var grandChild = lst[i];
                    if (grandChild.Tag is CanvasElement childElement)
                    {
                        LayoutControl(grandChild, childElement);
                    }
                }
            }
        }
    }

    private StackPanelControl CreateStackPanelElementControl(StackPanelElement element)
    {
        var control = new StackPanelControl(element);

        // 如果有子元素，递归创建
        foreach (var childElement in element.Children)
        {
            CreateElementControl(childElement, control.Control as Panel);
        }

        return control;
    }

    private PanelControl CreatePanelElementControl(PanelElement element)
    {
        var control = new PanelControl(element);

        // 如果有子元素，递归创建
        foreach (var childElement in element.Children)
        {
            CreateElementControl(childElement, control.Control as Panel);
        }

        return control;
    }

    private GridControl CreateGridElementControl(GridElement element)
    {
        var control = new GridControl(element);

        // 如果有子元素，递归创建
        foreach (var childElement in element.Children)
        {
            CreateElementControl(childElement, control.Control as Panel);
        }

        return control;
    }

    /// <summary>
    /// 创建控件用于预览（递归创建子元素，但不订阅事件）
    /// 返回 (控件, 内部容器Panel)
    /// </summary>
    public (Control?, Panel?) CreateElementControlForPreview(CanvasElement element)
    {
        Control? control = null;
        Panel? innerContainer = null;

        if (element is TextElement textElement)
        {
            control = new TextControl(textElement);
        }
        else if (element is CardElement cardElement)
        {
            control = new CardControl(cardElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is ButtonElement buttonElement)
        {
            control = new ButtonControl(buttonElement);
        }
        else if (element is StackPanelElement stackPanelElement)
        {
            control = new StackPanelControl(stackPanelElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is PanelElement panelElement)
        {
            control = new PanelControl(panelElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is GridElement gridElement)
        {
            control = new GridControl(gridElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is CanvasPanelElement canvasPanelElement)
        {
            control = new CanvasContainerControl(canvasPanelElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is ImageElement imageElement)
        {
            control = new ImageControl(imageElement);
        }
        else if (element is TriangleContainerElement triElement)
        {
            control = new TriangleFlowControl(triElement);
        }
        else if (element is RectangleContainerElement basicRectElement)
        {
            control = new RectangleFlowControl(basicRectElement);
        }
        else if (element is RoundedRectangleStackPanelElement roundedRectStackElement)
        {
            control = new RoundedRectangleStackPanelControl(roundedRectStackElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is RoundedRectangleCanvasElement roundedRectCanvasElement)
        {
            control = new RoundedRectangleCanvasControl(roundedRectCanvasElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is RoundedRectangleGridElement roundedRectGridElement)
        {
            control = new RoundedRectangleGridControl(roundedRectGridElement);
            innerContainer = GetInnerContainer(control);
        }
        else if (element is RoundedRectangleElement roundedRectElement)
        {
            control = new RoundedRectangleControl(roundedRectElement);
        }
        else if (element is EllipseContainerElement ellipseElement2)
        {
            control = new EllipseFlowControl(ellipseElement2);
        }
        else if (element is ParallelogramContainerElement paraElement)
        {
            control = new ParallelogramFlowControl(paraElement);
        }
        else if (element is RoundedStarElement sixStarElement)
        {
            control = new RoundedStarControl(sixStarElement);
        }
        else if (element is RoundedPolygonElement roundedPolygonElement)
        {
            control = new RoundedPolygonControl(roundedPolygonElement);
        }
        else if (element is RoundEllipseElement sectorElement)
        {
            control = new RoundEllipseControl(sectorElement);
        }
        else if (element is CanvasElement)
        {
            control = new CanvasContainerControl(element);
            innerContainer = GetInnerContainer(control);
        }

        // 递归为容器创建子元素（与 CreateElementControl 中的 Create*ElementControl 方法保持一致）
        if (innerContainer != null)
        {
            if (element is CardElement)
            {
                CreateCardElementChildrenForPreview(element, innerContainer);
            }
            else if (element is StackPanelElement)
            {
                CreateStackPanelElementChildrenForPreview(element, innerContainer);
            }
            else if (element is PanelElement)
            {
                CreatePanelElementChildrenForPreview(element, innerContainer);
            }
            else if (element is GridElement)
            {
                CreateGridElementChildrenForPreview(element, innerContainer);
            }
            else if (element is CanvasPanelElement)
            {
                CreateCanvasElementChildrenForPreview(element, innerContainer);
            }
            else if (element is RoundedRectangleStackPanelElement)
            {
                CreateStackPanelElementChildrenForPreview(element, innerContainer);
            }
            else if (element is RoundedRectangleCanvasElement)
            {
                CreateCanvasElementChildrenForPreview(element, innerContainer);
            }
            else if (element is RoundedRectangleGridElement)
            {
                CreateGridElementChildrenForPreview(element, innerContainer);
            }
            else if (control is CanvasContainerControl)
            {
                CreateCanvasElementChildrenForPreview(element, innerContainer);
            }
        }

        if (control != null)
        {
            control.Tag = element;
        }

        return (control, innerContainer);
    }

    private void CreateCardElementChildrenForPreview(CanvasElement element, Panel innerContainer)
    {
        foreach (var childElement in element.Children)
        {
            var (childControl, childInner) = CreateElementControlForPreview(childElement);
            if (childControl != null)
            {
                childControl.Tag = childElement;
                ApplyChildPositionForPreview(childControl, childElement, innerContainer);
                innerContainer.Children.Add(childControl);
            }
        }
    }

    private void CreateStackPanelElementChildrenForPreview(CanvasElement element, Panel innerContainer)
    {
        foreach (var childElement in element.Children)
        {
            var (childControl, childInner) = CreateElementControlForPreview(childElement);
            if (childControl != null)
            {
                childControl.Tag = childElement;
                ApplyChildPositionForPreview(childControl, childElement, innerContainer);
                innerContainer.Children.Add(childControl);
            }
        }
    }

    private void CreatePanelElementChildrenForPreview(CanvasElement element, Panel innerContainer)
    {
        foreach (var childElement in element.Children)
        {
            var (childControl, childInner) = CreateElementControlForPreview(childElement);
            if (childControl != null)
            {
                childControl.Tag = childElement;
                ApplyChildPositionForPreview(childControl, childElement, innerContainer);
                innerContainer.Children.Add(childControl);
            }
        }
    }

    private void CreateGridElementChildrenForPreview(CanvasElement element, Panel innerContainer)
    {
        if (innerContainer is Grid gridPanel)
        {
            foreach (var childElement in element.Children)
            {
                var (childControl, childInner) = CreateElementControlForPreview(childElement);
                if (childControl != null)
                {
                    childControl.Tag = childElement;
                    Grid.SetRow(childControl, childElement.GridRow);
                    Grid.SetColumn(childControl, childElement.GridColumn);
                    innerContainer.Children.Add(childControl);
                }
            }
        }
    }

    private void CreateCanvasElementChildrenForPreview(CanvasElement element, Panel innerContainer)
    {
        foreach (var childElement in element.Children)
        {
            var (childControl, childInner) = CreateElementControlForPreview(childElement);
            if (childControl != null)
            {
                childControl.Tag = childElement;
                ApplyChildPositionForPreview(childControl, childElement, innerContainer);
                innerContainer.Children.Add(childControl);
            }
        }
    }

    /// <summary>
    /// 根据父容器类型设置子元素的位置
    /// </summary>
    private void ApplyChildPositionForPreview(Control childControl, CanvasElement child, Panel parentContainer)
    {
        if (parentContainer is Canvas canvasParent)
        {
            Canvas.SetLeft(childControl, child.Left);
            Canvas.SetTop(childControl, child.Top);
        }
        else
        {
            // 非 Canvas 容器处理
            // Shape 控件需要设置 Margin 来模拟位置
            if (childControl is yuyiAvalonia.UI.ElementControls.Controls.ShapeFlowControlBase)
            {
                childControl.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                childControl.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            }
        }
    }

    /// <summary>
    /// 从 ElementControlBase 获取内部容器 Panel
    /// </summary>
    private Panel? GetInnerContainer(Control control)
    {
        if (control is IElementControlBase elementControl)
        {
            return elementControl.Control as Panel;
        }
        return control as Panel;
    }

    private CanvasContainerControl CreateCanvasElementControl(CanvasElement element)
    {
        CanvasContainerControl control = new CanvasContainerControl(element);

        // 如果有子元素，递归创建
        foreach (var childElement in element.Children)
        {
            CreateElementControl(childElement, control.Control as Panel);
        }

        return control;
    }

}
