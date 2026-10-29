namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 预设颜色方案集合
/// </summary>
public static class PresetColorSchemes
{
    public static List<ColorTheme> GetAll()
    {
        return new List<ColorTheme>
        {
            // === 基础主题 ===
            CreateLightTheme(),
            CreateDarkTheme(),

            // === 行业主题 ===
            CreateBlueTheme(),
            CreateGreenTheme(),
            CreatePurpleTheme(),
            CreateOrangeTheme(),
            CreatePinkTheme(),
            CreateRedTheme(),
            CreateTealTheme(),
            CreateIndigoTheme(),

            // === 风格主题 ===
            CreateMinimalTheme(),
            CreateGradientTheme(),
            CreateRetroTheme(),
            CreateNeonTheme(),
            CreatePastelTheme(),
            CreateEarthToneTheme(),

            // === 专业主题 ===
            CreateCorporateTheme(),
            CreateStartupTheme(),
            CreateElegantTheme(),
            CreatePlayfulTheme(),
            CreateNatureTheme()
        };
    }

    #region 基础主题

    public static ColorTheme CreateLightTheme()
    {
        return new ColorTheme
        {
            Id = "preset-light", Name = "浅色主题", Description = "明亮清新的浅色配色方案，适合大多数应用场景", Author = "系统", Category = "基础", IsBuiltIn = true, IsDefault = true,
            PrimaryColor = "#1890FF", PrimaryLight = "#40A9FF", PrimaryDark = "#096DD9", PrimaryLighter = "#E6F7FF", PrimaryBg = "#BAE7FF",
            SecondaryColor = "#722ED1", SecondaryLight = "#9254DE", SecondaryDark = "#531DAB",
            AccentColor = "#FA8C16", AccentLight = "#FFBB00", AccentDark = "#D46B00",
            SuccessColor = "#52C41A", SuccessLight = "#73D13D", SuccessDark = "#389E0D", SuccessBg = "#F6FFED", SuccessBorder = "#B7EB8F",
            WarningColor = "#FAAD14", WarningLight = "#FFC53D", WarningDark = "#D48806", WarningBg = "#FFFBE6", WarningBorder = "#FFE58F",
            ErrorColor = "#FF4D4F", ErrorLight = "#FF7875", ErrorDark = "#CF1322", ErrorBg = "#FFF2F0", ErrorBorder = "#FFCCC7",
            InfoColor = "#1890FF", InfoLight = "#40A9FF", InfoDark = "#096DD9", InfoBg = "#E6F7FF", InfoBorder = "#91D5FF",
            BackgroundColor = "#FFFFFF", BackgroundSecondary = "#FAFAFA", BackgroundTertiary = "#F5F5F5",
            ForegroundColor = "rgba(0,0,0,0.85)", ForegroundSecondary = "rgba(0,0,0,0.65)", ForegroundTertiary = "rgba(0,0,0,0.45)",
            BorderColor = "#D9D9D9", BorderSecondary = "#E8E8E8", BorderLight = "#F0F0F0",
            DividerColor = "#E8E8E8", DividerDark = "#303030",
            PlaceholderColor = "#BFBFBF", DisabledColor = "#D9D9D9", DisabledBg = "#F5F5F5",
            HoverColor = "rgba(0,0,0,0.04)", SelectedColor = "#E6F7FF", ActiveColor = "rgba(0,0,0,0.08)",
            Gray1 = "#FFFFFF", Gray2 = "#FAFAFA", Gray3 = "#F5F5F5", Gray4 = "#E8E8E8", Gray5 = "#D9D9D9",
            Gray6 = "#BFBFBF", Gray7 = "#8C8C8C", Gray8 = "#595959", Gray9 = "#434343", Gray10 = "#262626", Gray11 = "#1F1F1F", Gray12 = "#141414", Gray13 = "#000000",
            ShadowColor = "rgba(0,0,0,0.12)", ShadowColorLight = "rgba(0,0,0,0.08)", ShadowColorDark = "rgba(0,0,0,0.25)",
            FormElementStyle = new FormElementStyle
            {
                PrimaryBackgroundColor = "#1890FF", PrimaryForegroundColor = "#FFFFFF", PrimaryBorderColor = "#1890FF",
                PrimaryHoverBackgroundColor = "#096DD9", PrimaryHoverForegroundColor = "#FFFFFF", PrimaryHoverBorderColor = "#096DD9",
                PrimaryActiveBackgroundColor = "#0050B3", PrimaryActiveForegroundColor = "#FFFFFF", PrimaryActiveBorderColor = "#0050B3",
                PrimaryDisabledBackgroundColor = "#D9D9D9", PrimaryDisabledForegroundColor = "#FFFFFF", PrimaryDisabledBorderColor = "#D9D9D9",
                SecondaryBackgroundColor = "#FFFFFF", SecondaryForegroundColor = "#333333", SecondaryBorderColor = "#D9D9D9",
                SecondaryHoverBackgroundColor = "#F5F5F5", SecondaryHoverForegroundColor = "#333333", SecondaryHoverBorderColor = "#1890FF",
                InputBackgroundColor = "#FFFFFF", InputForegroundColor = "#333333", InputBorderColor = "#D9D9D9",
                InputHoverBorderColor = "#40A9FF", InputFocusBorderColor = "#1890FF", InputFocusShadowColor = "#1890FF33",
                InputPlaceholderColor = "#BFBFBF"
            },
            PagePanelStyle = new PagePanelStyle
            {
                PageBackgroundColor = "#F0F2F5", PageForegroundColor = "#333333", PageBorderColor = "#E8E8E8",
                PanelBackgroundColor = "#FFFFFF", PanelForegroundColor = "#333333", PanelBorderColor = "#E8E8E8",
                PanelHoverBorderColor = "#40A9FF", PanelSelectedBackgroundColor = "#E6F7FF", PanelSelectedBorderColor = "#1890FF",
                DividerDefaultColor = "#E8E8E8", DividerDisabledColor = "#F0F0F0",
                TabBackgroundColor = "#FFFFFF", TabForegroundColor = "#666666", TabHoverBackgroundColor = "#FFFFFF", TabHoverForegroundColor = "#333333",
                TabSelectedBackgroundColor = "#FFFFFF", TabSelectedForegroundColor = "#1890FF", TabSelectedUnderlineColor = "#1890FF",
                ListItemBackgroundColor = "transparent", ListItemForegroundColor = "#333333", ListItemHoverBackgroundColor = "#F5F5F5",
                ListItemSelectedBackgroundColor = "#E6F7FF", ListItemSelectedForegroundColor = "#1890FF",
                TableHeaderBackgroundColor = "#FAFAFA", TableHeaderForegroundColor = "#333333", TableCellBackgroundColor = "#FFFFFF"
            }
        };
    }

    public static ColorTheme CreateDarkTheme()
    {
        return new ColorTheme
        {
            Id = "preset-dark", Name = "深色主题", Description = "护眼的深色配色方案，适合夜间使用和开发者工具", Author = "系统", Category = "基础", IsBuiltIn = true,
            PrimaryColor = "#177DDC", PrimaryLight = "#4C9AE8", PrimaryDark = "#1465B4", PrimaryLighter = "#112A45", PrimaryBg = "#0D1F35",
            SecondaryColor = "#9254DE", SecondaryLight = "#AD82E6", SecondaryDark = "#7338BA",
            AccentColor = "#D89614", AccentLight = "#E8AD4D", AccentDark = "#B87A0A",
            SuccessColor = "#49AA19", SuccessLight = "#6CC038", SuccessDark = "#368B11", SuccessBg = "#0D1A0A", SuccessBorder = "#2D541C",
            WarningColor = "#D89614", WarningLight = "#E8AD4D", WarningDark = "#B87A0A", WarningBg = "#1A140A", WarningBorder = "#45341A",
            ErrorColor = "#DC3546", ErrorLight = "#E46070", ErrorDark = "#B8273B", ErrorBg = "#1A0A0D", ErrorBorder = "#451A22",
            InfoColor = "#177DDC", InfoLight = "#4C9AE8", InfoDark = "#1465B4", InfoBg = "#0D1A2E", InfoBorder = "#1A3560",
            BackgroundColor = "#141414", BackgroundSecondary = "#1F1F1F", BackgroundTertiary = "#262626",
            ForegroundColor = "#E8E8E8", ForegroundSecondary = "#B8B8B8", ForegroundTertiary = "#808080",
            BorderColor = "#434343", BorderSecondary = "#303030", BorderLight = "#525252",
            DividerColor = "#303030", DividerDark = "#1F1F1F",
            PlaceholderColor = "#666666", DisabledColor = "#525252", DisabledBg = "#1F1F1F",
            HoverColor = "rgba(255,255,255,0.08)", SelectedColor = "#0D1F35", ActiveColor = "rgba(255,255,255,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#F5F5F5", Gray3 = "#E8E8E8", Gray4 = "#D9D9D9", Gray5 = "#D0D0D0",
            Gray6 = "#8C8C8C", Gray7 = "#737373", Gray8 = "#595959", Gray9 = "#404040", Gray10 = "#2D2D2D", Gray11 = "#262626", Gray12 = "#1F1F1F", Gray13 = "#141414",
            ShadowColor = "rgba(0,0,0,0.4)", ShadowColorLight = "rgba(0,0,0,0.3)", ShadowColorDark = "rgba(0,0,0,0.6)",
            FormElementStyle = new FormElementStyle
            {
                PrimaryBackgroundColor = "#177DDC", PrimaryForegroundColor = "#FFFFFF", PrimaryBorderColor = "#177DDC",
                PrimaryHoverBackgroundColor = "#1465B4", PrimaryHoverForegroundColor = "#FFFFFF", PrimaryHoverBorderColor = "#1465B4",
                PrimaryActiveBackgroundColor = "#0D4A80", PrimaryActiveForegroundColor = "#FFFFFF", PrimaryActiveBorderColor = "#0D4A80",
                PrimaryDisabledBackgroundColor = "#434343", PrimaryDisabledForegroundColor = "#666666", PrimaryDisabledBorderColor = "#434343",
                SecondaryBackgroundColor = "#1F1F1F", SecondaryForegroundColor = "#E8E8E8", SecondaryBorderColor = "#434343",
                SecondaryHoverBackgroundColor = "#262626", SecondaryHoverForegroundColor = "#FFFFFF", SecondaryHoverBorderColor = "#177DDC",
                InputBackgroundColor = "#1F1F1F", InputForegroundColor = "#E8E8E8", InputBorderColor = "#434343",
                InputHoverBorderColor = "#4C9AE8", InputFocusBorderColor = "#177DDC", InputFocusShadowColor = "#177DDC33",
                InputPlaceholderColor = "#666666"
            },
            PagePanelStyle = new PagePanelStyle
            {
                PageBackgroundColor = "#0D0D0D", PageForegroundColor = "#E8E8E8", PageBorderColor = "#303030",
                PanelBackgroundColor = "#1F1F1F", PanelForegroundColor = "#E8E8E8", PanelBorderColor = "#303030",
                PanelHoverBorderColor = "#4C9AE8", PanelSelectedBackgroundColor = "#0D1F35", PanelSelectedBorderColor = "#177DDC",
                DividerDefaultColor = "#303030", DividerDisabledColor = "#262626",
                TabBackgroundColor = "#1F1F1F", TabForegroundColor = "#B8B8B8", TabHoverBackgroundColor = "#262626", TabHoverForegroundColor = "#E8E8E8",
                TabSelectedBackgroundColor = "#1F1F1F", TabSelectedForegroundColor = "#177DDC", TabSelectedUnderlineColor = "#177DDC",
                ListItemBackgroundColor = "transparent", ListItemForegroundColor = "#E8E8E8", ListItemHoverBackgroundColor = "rgba(255,255,255,0.08)",
                ListItemSelectedBackgroundColor = "#0D1F35", ListItemSelectedForegroundColor = "#177DDC",
                TableHeaderBackgroundColor = "#1F1F1F", TableHeaderForegroundColor = "#E8E8E8", TableCellBackgroundColor = "#1F1F1F"
            }
        };
    }

    #endregion

    #region 行业主题

    public static ColorTheme CreateBlueTheme()
    {
        return new ColorTheme
        {
            Id = "preset-blue", Name = "科技蓝", Description = "科技感十足的蓝色配色方案，适合技术类应用和SaaS产品", Author = "系统", Category = "科技", IsBuiltIn = true,
            PrimaryColor = "#0066CC", PrimaryLight = "#3399FF", PrimaryDark = "#004D99", PrimaryLighter = "#E6F2FF", PrimaryBg = "#CCE0FF",
            SecondaryColor = "#00A3E0", SecondaryLight = "#33C4FF", SecondaryDark = "#007AA8",
            AccentColor = "#00D4FF", AccentLight = "#66E5FF", AccentDark = "#00A8CC",
            SuccessColor = "#00CC66", SuccessLight = "#33DB80", SuccessDark = "#00994D", SuccessBg = "#E6FFF0", SuccessBorder = "#99E6B3",
            WarningColor = "#FFB800", WarningLight = "#FFCF33", WarningDark = "#CC9200", WarningBg = "#FFFAE6", WarningBorder = "#FFE699",
            ErrorColor = "#FF3333", ErrorLight = "#FF6666", ErrorDark = "#CC1A1A", ErrorBg = "#FFE6E6", ErrorBorder = "#FFB3B3",
            InfoColor = "#0099FF", InfoLight = "#33B3FF", InfoDark = "#0077CC", InfoBg = "#E6F5FF", InfoBorder = "#99CCFF",
            BackgroundColor = "#F0F8FF", BackgroundSecondary = "#E6F2FF", BackgroundTertiary = "#D9EBFF",
            ForegroundColor = "#1A2A3A", ForegroundSecondary = "#3A4A5A", ForegroundTertiary = "#5A6A7A",
            BorderColor = "#B3D4F0", BorderSecondary = "#C9E0F5", BorderLight = "#E0F0FF",
            DividerColor = "#C9E0F5", DividerDark = "#1A3344",
            PlaceholderColor = "#80A8CC", DisabledColor = "#B3D4F0", DisabledBg = "#D9EBFF",
            HoverColor = "rgba(0,102,204,0.08)", SelectedColor = "#E6F2FF", ActiveColor = "rgba(0,102,204,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#F5FAFF", Gray3 = "#EBF5FF", Gray4 = "#D4E8F7", Gray5 = "#B3D0E8",
            Gray6 = "#80A8CC", Gray7 = "#4D80A8", Gray8 = "#336080", Gray9 = "#1A3344", Gray10 = "#0D1A22", Gray11 = "#0A141D", Gray12 = "#080F16", Gray13 = "#050A10",
            ShadowColor = "rgba(0,102,204,0.15)", ShadowColorLight = "rgba(0,102,204,0.1)", ShadowColorDark = "rgba(0,102,204,0.25)"
        };
    }

    public static ColorTheme CreateGreenTheme()
    {
        return new ColorTheme
        {
            Id = "preset-green", Name = "自然绿", Description = "清新自然的绿色配色方案，适合环保、健康和生鲜类应用", Author = "系统", Category = "行业", IsBuiltIn = true,
            PrimaryColor = "#2E7D32", PrimaryLight = "#60AD5E", PrimaryDark = "#005500", PrimaryLighter = "#E8F5E9", PrimaryBg = "#C8E6C9",
            SecondaryColor = "#00ACC1", SecondaryLight = "#4DD0E1", SecondaryDark = "#00838F",
            AccentColor = "#81C784", AccentLight = "#A5D6A7", AccentDark = "#66BB6A",
            SuccessColor = "#4CAF50", SuccessLight = "#81C784", SuccessDark = "#388E3C", SuccessBg = "#E8F5E9", SuccessBorder = "#A5D6A7",
            WarningColor = "#FFA726", WarningLight = "#FFB74D", WarningDark = "#F57C00", WarningBg = "#FFF3E0", WarningBorder = "#FFCC80",
            ErrorColor = "#EF5350", ErrorLight = "#E57373", ErrorDark = "#D32F2F", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#42A5F5", InfoLight = "#64B5F6", InfoDark = "#1976D2", InfoBg = "#E3F2FD", InfoBorder = "#90CAF9",
            BackgroundColor = "#F1F8E9", BackgroundSecondary = "#E8F5E9", BackgroundTertiary = "#DCEDC8",
            ForegroundColor = "#1B5E20", ForegroundSecondary = "#2E7D32", ForegroundTertiary = "#388E3C",
            BorderColor = "#A5D6A7", BorderSecondary = "#C8E6C9", BorderLight = "#E8F5E9",
            DividerColor = "#C8E6C9", DividerDark = "#1B5E20",
            PlaceholderColor = "#81C784", DisabledColor = "#A5D6A7", DisabledBg = "#DCEDC8",
            HoverColor = "rgba(46,125,50,0.08)", SelectedColor = "#E8F5E9", ActiveColor = "rgba(46,125,50,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#F1F8E9", Gray3 = "#E8F5E9", Gray4 = "#C8E6C9", Gray5 = "#A5D6A7",
            Gray6 = "#81C784", Gray7 = "#66BB6A", Gray8 = "#4CAF50", Gray9 = "#388E3C", Gray10 = "#2E7D32", Gray11 = "#1B5E20", Gray12 = "#145018", Gray13 = "#0D330F",
            ShadowColor = "rgba(46,125,50,0.15)", ShadowColorLight = "rgba(46,125,50,0.1)", ShadowColorDark = "rgba(46,125,50,0.25)"
        };
    }

    public static ColorTheme CreatePurpleTheme()
    {
        return new ColorTheme
        {
            Id = "preset-purple", Name = "优雅紫", Description = "高贵典雅的紫色配色方案，适合创意、设计和时尚类应用", Author = "系统", Category = "创意", IsBuiltIn = true,
            PrimaryColor = "#7B1FA2", PrimaryLight = "#AE52D4", PrimaryDark = "#4A0072", PrimaryLighter = "#F3E5F5", PrimaryBg = "#E1BEE7",
            SecondaryColor = "#EC407A", SecondaryLight = "#F48FB1", SecondaryDark = "#C2185B",
            AccentColor = "#CE93D8", AccentLight = "#E1BEE7", AccentDark = "#AB47BC",
            SuccessColor = "#66BB6A", SuccessLight = "#81C784", SuccessDark = "#4CAF50", SuccessBg = "#F3E5F5", SuccessBorder = "#CE93D8",
            WarningColor = "#FFB74D", WarningLight = "#FFCC80", WarningDark = "#FFA726", WarningBg = "#FFF3E0", WarningBorder = "#FFE0B2",
            ErrorColor = "#EF5350", ErrorLight = "#E57373", ErrorDark = "#D32F2F", ErrorBg = "#FCE4EC", ErrorBorder = "#F8BBD0",
            InfoColor = "#42A5F5", InfoLight = "#64B5F6", InfoDark = "#1E88E5", InfoBg = "#E3F2FD", InfoBorder = "#BBDEFB",
            BackgroundColor = "#F3E5F5", BackgroundSecondary = "#FCE4EC", BackgroundTertiary = "#F8BBD0",
            ForegroundColor = "#4A148C", ForegroundSecondary = "#6A1B9A", ForegroundTertiary = "#7B1FA2",
            BorderColor = "#E1BEE7", BorderSecondary = "#CE93D8", BorderLight = "#F3E5F5",
            DividerColor = "#CE93D8", DividerDark = "#4A148C",
            PlaceholderColor = "#AB47BC", DisabledColor = "#E1BEE7", DisabledBg = "#F8BBD0",
            HoverColor = "rgba(123,31,162,0.08)", SelectedColor = "#F3E5F5", ActiveColor = "rgba(123,31,162,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#FCE4EC", Gray3 = "#F8BBD0", Gray4 = "#F48FB1", Gray5 = "#EC407A",
            Gray6 = "#CE93D8", Gray7 = "#AB47BC", Gray8 = "#8E24AA", Gray9 = "#7B1FA2", Gray10 = "#6A1B9A", Gray11 = "#4A148C", Gray12 = "#311B92", Gray13 = "#1A0A2E",
            ShadowColor = "rgba(123,31,162,0.2)", ShadowColorLight = "rgba(123,31,162,0.12)", ShadowColorDark = "rgba(123,31,162,0.35)"
        };
    }

    public static ColorTheme CreateOrangeTheme()
    {
        return new ColorTheme
        {
            Id = "preset-orange", Name = "活力橙", Description = "充满活力的橙色配色方案，适合餐饮、电商和促销类应用", Author = "系统", Category = "电商", IsBuiltIn = true,
            PrimaryColor = "#E65100", PrimaryLight = "#FF833A", PrimaryDark = "#AC1900", PrimaryLighter = "#FFF3E0", PrimaryBg = "#FFE0B2",
            SecondaryColor = "#FF6D00", SecondaryLight = "#FF9E40", SecondaryDark = "#C43E00",
            AccentColor = "#FFB74D", AccentLight = "#FFD180", AccentDark = "#FF8F00",
            SuccessColor = "#4CAF50", SuccessLight = "#81C784", SuccessDark = "#388E3C", SuccessBg = "#FFF8E1", SuccessBorder = "#FFE082",
            WarningColor = "#FFA000", WarningLight = "#FFC107", WarningDark = "#FF8F00", WarningBg = "#FFFDE7", WarningBorder = "#FFF59D",
            ErrorColor = "#F44336", ErrorLight = "#E57373", ErrorDark = "#D32F2F", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#2196F3", InfoLight = "#64B5F6", InfoDark = "#1976D2", InfoBg = "#E3F2FD", InfoBorder = "#90CAF9",
            BackgroundColor = "#FFF8E1", BackgroundSecondary = "#FFECB3", BackgroundTertiary = "#FFE082",
            ForegroundColor = "#BF360C", ForegroundSecondary = "#D84315", ForegroundTertiary = "#E65100",
            BorderColor = "#FFCC80", BorderSecondary = "#FFE0B2", BorderLight = "#FFF3E0",
            DividerColor = "#FFE0B2", DividerDark = "#BF360C",
            PlaceholderColor = "#FF8F00", DisabledColor = "#FFCC80", DisabledBg = "#FFE082",
            HoverColor = "rgba(230,81,0,0.08)", SelectedColor = "#FFF3E0", ActiveColor = "rgba(230,81,0,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#FFF8E1", Gray3 = "#FFECB3", Gray4 = "#FFE082", Gray5 = "#FFD54F",
            Gray6 = "#FFCA28", Gray7 = "#FFA000", Gray8 = "#FF8F00", Gray9 = "#F57C00", Gray10 = "#EF6C00", Gray11 = "#E65100", Gray12 = "#BF360C", Gray13 = "#870000",
            ShadowColor = "rgba(230,81,0,0.2)", ShadowColorLight = "rgba(230,81,0,0.12)", ShadowColorDark = "rgba(230,81,0,0.35)"
        };
    }

    public static ColorTheme CreatePinkTheme()
    {
        return new ColorTheme
        {
            Id = "preset-pink", Name = "少女粉", Description = "甜美可爱的粉色配色方案，适合女性、时尚和美妆类应用", Author = "系统", Category = "时尚", IsBuiltIn = true,
            PrimaryColor = "#C2185B", PrimaryLight = "#E91E63", PrimaryDark = "#880E4F", PrimaryLighter = "#FCE4EC", PrimaryBg = "#F8BBD0",
            SecondaryColor = "#FF4081", SecondaryLight = "#FF80AB", SecondaryDark = "#F50057",
            AccentColor = "#F48FB1", AccentLight = "#F8BBD0", AccentDark = "#EC407A",
            SuccessColor = "#4CAF50", SuccessLight = "#81C784", SuccessDark = "#388E3C", SuccessBg = "#FCE4EC", SuccessBorder = "#C8E6C9",
            WarningColor = "#FF9800", WarningLight = "#FFB74D", WarningDark = "#F57C00", WarningBg = "#FFF3E0", WarningBorder = "#FFE0B2",
            ErrorColor = "#F44336", ErrorLight = "#E57373", ErrorDark = "#D32F2F", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#2196F3", InfoLight = "#64B5F6", InfoDark = "#1976D2", InfoBg = "#E3F2FD", InfoBorder = "#90CAF9",
            BackgroundColor = "#FFF0F5", BackgroundSecondary = "#FCE4EC", BackgroundTertiary = "#F8BBD0",
            ForegroundColor = "#880E4F", ForegroundSecondary = "#AD1457", ForegroundTertiary = "#C2185B",
            BorderColor = "#F8BBD0", BorderSecondary = "#F48FB1", BorderLight = "#FCE4EC",
            DividerColor = "#F48FB1", DividerDark = "#880E4F",
            PlaceholderColor = "#EC407A", DisabledColor = "#F8BBD0", DisabledBg = "#F48FB1",
            HoverColor = "rgba(194,24,91,0.08)", SelectedColor = "#FCE4EC", ActiveColor = "rgba(194,24,91,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#FFF0F5", Gray3 = "#FCE4EC", Gray4 = "#F8BBD0", Gray5 = "#F48FB1",
            Gray6 = "#EC407A", Gray7 = "#E91E63", Gray8 = "#D81B60", Gray9 = "#C2185B", Gray10 = "#AD1457", Gray11 = "#880E4F", Gray12 = "#560D28", Gray13 = "#2D0614",
            ShadowColor = "rgba(194,24,91,0.2)", ShadowColorLight = "rgba(194,24,91,0.12)", ShadowColorDark = "rgba(194,24,91,0.35)"
        };
    }

    public static ColorTheme CreateRedTheme()
    {
        return new ColorTheme
        {
            Id = "preset-red", Name = "中国红", Description = "庄重热烈的红色配色方案，适合节庆、电商和品牌类应用", Author = "系统", Category = "节庆", IsBuiltIn = true,
            PrimaryColor = "#C62828", PrimaryLight = "#EF5350", PrimaryDark = "#8E0000", PrimaryLighter = "#FFEBEE", PrimaryBg = "#FFCDD2",
            SecondaryColor = "#D32F2F", SecondaryLight = "#E57373", SecondaryDark = "#B71C1C",
            AccentColor = "#FF8A80", AccentLight = "#FFCDD2", AccentDark = "#FF5252",
            SuccessColor = "#4CAF50", SuccessLight = "#81C784", SuccessDark = "#388E3C", SuccessBg = "#E8F5E9", SuccessBorder = "#A5D6A7",
            WarningColor = "#FFA726", WarningLight = "#FFB74D", WarningDark = "#F57C00", WarningBg = "#FFF3E0", WarningBorder = "#FFE0B2",
            ErrorColor = "#D32F2F", ErrorLight = "#EF5350", ErrorDark = "#B71C1C", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#1976D2", InfoLight = "#42A5F5", InfoDark = "#1565C0", InfoBg = "#E3F2FD", InfoBorder = "#90CAF9",
            BackgroundColor = "#FFEBEE", BackgroundSecondary = "#FFCDD2", BackgroundTertiary = "#EF9A9A",
            ForegroundColor = "#B71C1C", ForegroundSecondary = "#C62828", ForegroundTertiary = "#D32F2F",
            BorderColor = "#EF9A9A", BorderSecondary = "#FFCDD2", BorderLight = "#FFEBEE",
            DividerColor = "#FFCDD2", DividerDark = "#8E0000",
            PlaceholderColor = "#D32F2F", DisabledColor = "#EF9A9A", DisabledBg = "#FFCDD2",
            HoverColor = "rgba(198,40,40,0.08)", SelectedColor = "#FFEBEE", ActiveColor = "rgba(198,40,40,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#FFEBEE", Gray3 = "#EF9A9A", Gray4 = "#E57373", Gray5 = "#EF5350",
            Gray6 = "#F44336", Gray7 = "#E53935", Gray8 = "#D32F2F", Gray9 = "#C62828", Gray10 = "#B71C1C", Gray11 = "#8E0000", Gray12 = "#560000", Gray13 = "#2D0000",
            ShadowColor = "rgba(198,40,40,0.2)", ShadowColorLight = "rgba(198,40,40,0.12)", ShadowColorDark = "rgba(198,40,40,0.35)"
        };
    }

    public static ColorTheme CreateTealTheme()
    {
        return new ColorTheme
        {
            Id = "preset-teal", Name = "清新青", Description = "清新淡雅的青绿色配色方案，适合金融和健康类应用", Author = "系统", Category = "金融", IsBuiltIn = true,
            PrimaryColor = "#00796B", PrimaryLight = "#26A69A", PrimaryDark = "#004D40", PrimaryLighter = "#E0F2F1", PrimaryBg = "#B2DFDB",
            SecondaryColor = "#00897B", SecondaryLight = "#4DB6AC", SecondaryDark = "#00695C",
            AccentColor = "#4DB6AC", AccentLight = "#80CBC4", AccentDark = "#26A69A",
            SuccessColor = "#43A047", SuccessLight = "#66BB6A", SuccessDark = "#2E7D32", SuccessBg = "#E8F5E9", SuccessBorder = "#A5D6A7",
            WarningColor = "#FFA000", WarningLight = "#FFB74D", WarningDark = "#FF8F00", WarningBg = "#FFFDE7", WarningBorder = "#FFF59D",
            ErrorColor = "#E53935", ErrorLight = "#EF5350", ErrorDark = "#C62828", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#039BE5", InfoLight = "#29B6F6", InfoDark = "#0277BD", InfoBg = "#E1F5FE", InfoBorder = "#81D4FA",
            BackgroundColor = "#E0F2F1", BackgroundSecondary = "#B2DFDB", BackgroundTertiary = "#80CBC4",
            ForegroundColor = "#004D40", ForegroundSecondary = "#00695C", ForegroundTertiary = "#00796B",
            BorderColor = "#B2DFDB", BorderSecondary = "#80CBC4", BorderLight = "#E0F2F1",
            DividerColor = "#80CBC4", DividerDark = "#004D40",
            PlaceholderColor = "#26A69A", DisabledColor = "#B2DFDB", DisabledBg = "#80CBC4",
            HoverColor = "rgba(0,121,107,0.08)", SelectedColor = "#E0F2F1", ActiveColor = "rgba(0,121,107,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#E0F2F1", Gray3 = "#B2DFDB", Gray4 = "#80CBC4", Gray5 = "#4DB6AC",
            Gray6 = "#26A69A", Gray7 = "#00897B", Gray8 = "#00796B", Gray9 = "#00695C", Gray10 = "#004D40", Gray11 = "#003D33", Gray12 = "#002B26", Gray13 = "#001A19",
            ShadowColor = "rgba(0,121,107,0.15)", ShadowColorLight = "rgba(0,121,107,0.1)", ShadowColorDark = "rgba(0,121,107,0.25)"
        };
    }

    public static ColorTheme CreateIndigoTheme()
    {
        return new ColorTheme
        {
            Id = "preset-indigo", Name = "靛蓝", Description = "深邃沉稳的靛蓝配色方案，适合企业级应用和仪表盘", Author = "系统", Category = "企业", IsBuiltIn = true,
            PrimaryColor = "#3949AB", PrimaryLight = "#5C6BC0", PrimaryDark = "#283593", PrimaryLighter = "#E8EAF6", PrimaryBg = "#C5CAE9",
            SecondaryColor = "#303F9F", SecondaryLight = "#7986CB", SecondaryDark = "#1A237E",
            AccentColor = "#7986CB", AccentLight = "#9FA8DA", AccentDark = "#5C6BC0",
            SuccessColor = "#43A047", SuccessLight = "#66BB6A", SuccessDark = "#2E7D32", SuccessBg = "#E8EAF6", SuccessBorder = "#C5CAE9",
            WarningColor = "#FFA000", WarningLight = "#FFB74D", WarningDark = "#FF8F00", WarningBg = "#FFF8E1", WarningBorder = "#FFE082",
            ErrorColor = "#E53935", ErrorLight = "#EF5350", ErrorDark = "#C62828", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#039BE5", InfoLight = "#29B6F6", InfoDark = "#0277BD", InfoBg = "#E8EAF6", InfoBorder = "#9FA8DA",
            BackgroundColor = "#E8EAF6", BackgroundSecondary = "#C5CAE9", BackgroundTertiary = "#9FA8DA",
            ForegroundColor = "#1A237E", ForegroundSecondary = "#283593", ForegroundTertiary = "#3949AB",
            BorderColor = "#C5CAE9", BorderSecondary = "#9FA8DA", BorderLight = "#E8EAF6",
            DividerColor = "#9FA8DA", DividerDark = "#1A237E",
            PlaceholderColor = "#5C6BC0", DisabledColor = "#C5CAE9", DisabledBg = "#9FA8DA",
            HoverColor = "rgba(57,73,171,0.08)", SelectedColor = "#E8EAF6", ActiveColor = "rgba(57,73,171,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#E8EAF6", Gray3 = "#C5CAE9", Gray4 = "#9FA8DA", Gray5 = "#7986CB",
            Gray6 = "#5C6BC0", Gray7 = "#3F51B5", Gray8 = "#3949AB", Gray9 = "#303F9F", Gray10 = "#283593", Gray11 = "#1A237E", Gray12 = "#0D1257", Gray13 = "#050A3A",
            ShadowColor = "rgba(57,73,171,0.15)", ShadowColorLight = "rgba(57,73,171,0.1)", ShadowColorDark = "rgba(57,73,171,0.25)"
        };
    }

    #endregion

    #region 风格主题

    public static ColorTheme CreateMinimalTheme()
    {
        return new ColorTheme
        {
            Id = "preset-minimal", Name = "极简白", Description = "简约纯净的黑白配色方案，适合追求极简风格的应用", Author = "系统", Category = "风格", IsBuiltIn = true,
            PrimaryColor = "#000000", PrimaryLight = "#333333", PrimaryDark = "#000000", PrimaryLighter = "#F5F5F5", PrimaryBg = "#E0E0E0",
            SecondaryColor = "#666666", SecondaryLight = "#999999", SecondaryDark = "#333333",
            AccentColor = "#999999", AccentLight = "#CCCCCC", AccentDark = "#666666",
            SuccessColor = "#333333", SuccessLight = "#666666", SuccessDark = "#000000", SuccessBg = "#FFFFFF", SuccessBorder = "#E0E0E0",
            WarningColor = "#888888", WarningLight = "#AAAAAA", WarningDark = "#555555", WarningBg = "#FFFFFF", WarningBorder = "#E0E0E0",
            ErrorColor = "#000000", ErrorLight = "#333333", ErrorDark = "#000000", ErrorBg = "#FFFFFF", ErrorBorder = "#E0E0E0",
            InfoColor = "#333333", InfoLight = "#666666", InfoDark = "#000000", InfoBg = "#FFFFFF", InfoBorder = "#E0E0E0",
            BackgroundColor = "#FFFFFF", BackgroundSecondary = "#FAFAFA", BackgroundTertiary = "#F5F5F5",
            ForegroundColor = "#000000", ForegroundSecondary = "#333333", ForegroundTertiary = "#666666",
            BorderColor = "#E0E0E0", BorderSecondary = "#F0F0F0", BorderLight = "#F5F5F5",
            DividerColor = "#F0F0F0", DividerDark = "#1A1A1A",
            PlaceholderColor = "#999999", DisabledColor = "#E0E0E0", DisabledBg = "#F5F5F5",
            HoverColor = "rgba(0,0,0,0.04)", SelectedColor = "#F5F5F5", ActiveColor = "rgba(0,0,0,0.08)",
            Gray1 = "#FFFFFF", Gray2 = "#FAFAFA", Gray3 = "#F5F5F5", Gray4 = "#E8E8E8", Gray5 = "#E0E0E0",
            Gray6 = "#CCCCCC", Gray7 = "#999999", Gray8 = "#666666", Gray9 = "#333333", Gray10 = "#1A1A1A", Gray11 = "#0D0D0D", Gray12 = "#050505", Gray13 = "#000000",
            ShadowColor = "rgba(0,0,0,0.08)", ShadowColorLight = "rgba(0,0,0,0.04)", ShadowColorDark = "rgba(0,0,0,0.15)"
        };
    }

    public static ColorTheme CreateGradientTheme()
    {
        return new ColorTheme
        {
            Id = "preset-gradient", Name = "渐变彩", Description = "色彩丰富的渐变配色方案，适合年轻、创意和社交类应用", Author = "系统", Category = "风格", IsBuiltIn = true,
            PrimaryColor = "#667EEA", PrimaryLight = "#8BA4F4", PrimaryDark = "#4C5FD5", PrimaryLighter = "#EEF1FA", PrimaryBg = "#D0DAF9",
            SecondaryColor = "#764BA2", SecondaryLight = "#9370BD", SecondaryDark = "#5E3585",
            AccentColor = "#F093FB", AccentLight = "#F5C3F7", AccentDark = "#E06BC3",
            SuccessColor = "#4ADE80", SuccessLight = "#6EE7A0", SuccessDark = "#22C55E", SuccessBg = "#F0FDF4", SuccessBorder = "#BBF7D0",
            WarningColor = "#FBBF24", WarningLight = "#FCD34D", WarningDark = "#F59E0B", WarningBg = "#FFFBEB", WarningBorder = "#FDE68A",
            ErrorColor = "#F87171", ErrorLight = "#FCA5A5", ErrorDark = "#EF4444", ErrorBg = "#FEF2F2", ErrorBorder = "#FECACA",
            InfoColor = "#38BDF8", InfoLight = "#7DD3FC", InfoDark = "#0EA5E9", InfoBg = "#F0F9FF", InfoBorder = "#BAE6FD",
            BackgroundColor = "#FAFBFF", BackgroundSecondary = "#F0F4FF", BackgroundTertiary = "#E8EDFF",
            ForegroundColor = "#1E293B", ForegroundSecondary = "#334155", ForegroundTertiary = "#475569",
            BorderColor = "#E0E7FF", BorderSecondary = "#E8EDFF", BorderLight = "#F0F4FF",
            DividerColor = "#E8EDFF", DividerDark = "#1E293B",
            PlaceholderColor = "#64748B", DisabledColor = "#E0E7FF", DisabledBg = "#E8EDFF",
            HoverColor = "rgba(102,126,234,0.08)", SelectedColor = "#EEF1FA", ActiveColor = "rgba(102,126,234,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#F8FAFC", Gray3 = "#F1F5F9", Gray4 = "#E2E8F0", Gray5 = "#CBD5E1",
            Gray6 = "#94A3B8", Gray7 = "#64748B", Gray8 = "#475569", Gray9 = "#334155", Gray10 = "#1E293B", Gray11 = "#0F172A", Gray12 = "#020617", Gray13 = "#000000",
            ShadowColor = "rgba(102,126,234,0.2)", ShadowColorLight = "rgba(102,126,234,0.12)", ShadowColorDark = "rgba(102,126,234,0.35)",
            GradientPrimary = "linear-gradient(135deg, #667EEA 0%, #764BA2 100%)",
            GradientSuccess = "linear-gradient(135deg, #11998E 0%, #38EF7D 100%)",
            GradientWarning = "linear-gradient(135deg, #F093FB 0%, #F5576C 100%)",
            GradientInfo = "linear-gradient(135deg, #4FACFE 0%, #00F2FE 100%)"
        };
    }

    public static ColorTheme CreateRetroTheme()
    {
        return new ColorTheme
        {
            Id = "preset-retro", Name = "复古风", Description = "怀旧复古的配色方案，适合文创和手工类应用", Author = "系统", Category = "风格", IsBuiltIn = true,
            PrimaryColor = "#8B4513", PrimaryLight = "#A0522D", PrimaryDark = "#6B3410", PrimaryLighter = "#FAF0E6", PrimaryBg = "#DEB887",
            SecondaryColor = "#D2691E", SecondaryLight = "#E9967A", SecondaryDark = "#8B4513",
            AccentColor = "#CD853F", AccentLight = "#DEB887", AccentDark = "#A0522D",
            SuccessColor = "#556B2F", SuccessLight = "#6B8E23", SuccessDark = "#3D4F22", SuccessBg = "#FAF0E6", SuccessBorder = "#C4B896",
            WarningColor = "#DAA520", WarningLight = "#EDC143", WarningDark = "#B8860B", WarningBg = "#FFF8DC", WarningBorder = "#F0D78C",
            ErrorColor = "#B22222", ErrorLight = "#CD5C5C", ErrorDark = "#8B0000", ErrorBg = "#FFF0F0", ErrorBorder = "#E8B4B4",
            InfoColor = "#4682B4", InfoLight = "#6A9EC0", InfoDark = "#2F6A9C", InfoBg = "#F0F8FF", InfoBorder = "#B0D4E8",
            BackgroundColor = "#FFF8DC", BackgroundSecondary = "#FAEBD7", BackgroundTertiary = "#DEB887",
            ForegroundColor = "#3D2314", ForegroundSecondary = "#5C3A24", ForegroundTertiary = "#7A4D35",
            BorderColor = "#D2B48C", BorderSecondary = "#DEB887", BorderLight = "#F5DEB3",
            DividerColor = "#DEB887", DividerDark = "#3D2314",
            PlaceholderColor = "#A0826D", DisabledColor = "#D2B48C", DisabledBg = "#DEB887",
            HoverColor = "rgba(139,69,19,0.08)", SelectedColor = "#FAF0E6", ActiveColor = "rgba(139,69,19,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#FFF8DC", Gray3 = "#FAEBD7", Gray4 = "#F5DEB3", Gray5 = "#DEB887",
            Gray6 = "#C4A87C", Gray7 = "#A0826D", Gray8 = "#8B7355", Gray9 = "#6B5344", Gray10 = "#5C4333", Gray11 = "#4D3629", Gray12 = "#3E2B1F", Gray13 = "#2F2015",
            ShadowColor = "rgba(139,69,19,0.15)", ShadowColorLight = "rgba(139,69,19,0.1)", ShadowColorDark = "rgba(139,69,19,0.25)"
        };
    }

    public static ColorTheme CreateNeonTheme()
    {
        return new ColorTheme
        {
            Id = "preset-neon", Name = "霓虹灯", Description = "赛博朋克风格的霓虹配色方案，适合游戏和娱乐类应用", Author = "系统", Category = "风格", IsBuiltIn = true,
            PrimaryColor = "#00FFFF", PrimaryLight = "#66FFFF", PrimaryDark = "#00CCCC", PrimaryLighter = "#E0FFFF", PrimaryBg = "#B3FFFF",
            SecondaryColor = "#FF00FF", SecondaryLight = "#FF66FF", SecondaryDark = "#CC00CC", SecondaryLighter = "#FFE0FF", SecondaryBg = "#FFB3FF",
            AccentColor = "#00FF00", AccentLight = "#66FF66", AccentDark = "#00CC00",
            SuccessColor = "#39FF14", SuccessLight = "#7FFF5C", SuccessDark = "#2FCC10", SuccessBg = "#E0FFE0", SuccessBorder = "#99FF99",
            WarningColor = "#FFFF00", WarningLight = "#FFFF66", WarningDark = "#CCCC00", WarningBg = "#FFFFF0", WarningBorder = "#FFFFB3",
            ErrorColor = "#FF3131", ErrorLight = "#FF6B6B", ErrorDark = "#CC1A1A", ErrorBg = "#FFE0E0", ErrorBorder = "#FFB3B3",
            InfoColor = "#00D4FF", InfoLight = "#66E5FF", InfoDark = "#00A8CC", InfoBg = "#E0F8FF", InfoBorder = "#B3ECFF",
            BackgroundColor = "#0D0D0D", BackgroundSecondary = "#1A1A1A", BackgroundTertiary = "#262626",
            ForegroundColor = "#FFFFFF", ForegroundSecondary = "#E0E0E0", ForegroundTertiary = "#CCCCCC",
            BorderColor = "#333333", BorderSecondary = "#262626", BorderLight = "#404040",
            DividerColor = "#262626", DividerDark = "#0D0D0D",
            PlaceholderColor = "#707070", DisabledColor = "#404040", DisabledBg = "#262626",
            HoverColor = "rgba(255,255,255,0.08)", SelectedColor = "#1A1A1A", ActiveColor = "rgba(255,255,255,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#F0F0F0", Gray3 = "#D0D0D0", Gray4 = "#B0B0B0", Gray5 = "#909090",
            Gray6 = "#707070", Gray7 = "#505050", Gray8 = "#404040", Gray9 = "#333333", Gray10 = "#262626", Gray11 = "#1A1A1A", Gray12 = "#0D0D0D", Gray13 = "#000000",
            ShadowColor = "rgba(0,255,255,0.3)", ShadowColorLight = "rgba(255,0,255,0.2)", ShadowColorDark = "rgba(0,255,255,0.5)",
            GradientPrimary = "linear-gradient(135deg, #00FFFF 0%, #FF00FF 100%)",
            GradientSuccess = "linear-gradient(135deg, #39FF14 0%, #00FFFF 100%)",
            GradientWarning = "linear-gradient(135deg, #FFFF00 0%, #FF00FF 100%)",
            GradientInfo = "linear-gradient(135deg, #00D4FF 0%, #00FF00 100%)"
        };
    }

    public static ColorTheme CreatePastelTheme()
    {
        return new ColorTheme
        {
            Id = "preset-pastel", Name = "马卡龙", Description = "柔和甜美的马卡龙配色方案，适合烘焙和儿童类应用", Author = "系统", Category = "风格", IsBuiltIn = true,
            PrimaryColor = "#A8D8EA", PrimaryLight = "#C8E8F4", PrimaryDark = "#7BBFDB", PrimaryLighter = "#E8F4F8", PrimaryBg = "#D8EEF6",
            SecondaryColor = "#FFAAA5", SecondaryLight = "#FFCCC8", SecondaryDark = "#F08B85", SecondaryLighter = "#FFE5E2", SecondaryBg = "#FFE0DD",
            AccentColor = "#FFD3B6", AccentLight = "#FFE4CC", AccentDark = "#F5C099",
            SuccessColor = "#98DDCA", SuccessLight = "#B5E7D8", SuccessDark = "#7BCEB8", SuccessBg = "#E8F8F5", SuccessBorder = "#C8F0E6",
            WarningColor = "#FFECC2", WarningLight = "#FFF5DB", WarningDark = "#F5DFA5", WarningBg = "#FFFEF5", WarningBorder = "#FFF9E8",
            ErrorColor = "#FFAAA5", ErrorLight = "#FFC8C3", ErrorDark = "#F08B85", ErrorBg = "#FFF5F4", ErrorBorder = "#FFE8E5",
            InfoColor = "#D5AAFF", InfoLight = "#E8C8FF", InfoDark = "#C68BE8", InfoBg = "#F8F0FF", InfoBorder = "#E8DCFF",
            BackgroundColor = "#FFFEF9", BackgroundSecondary = "#FFF9F0", BackgroundTertiary = "#FFF5E8",
            ForegroundColor = "#5A5A5A", ForegroundSecondary = "#6E6E6E", ForegroundTertiary = "#828282",
            BorderColor = "#E8E0D8", BorderSecondary = "#F0E8E0", BorderLight = "#F8F2EC",
            DividerColor = "#F0E8E0", DividerDark = "#5A5A5A",
            PlaceholderColor = "#A69B8C", DisabledColor = "#E8E0D8", DisabledBg = "#F0E8E0",
            HoverColor = "rgba(168,216,234,0.15)", SelectedColor = "#FFF9F0", ActiveColor = "rgba(168,216,234,0.25)",
            Gray1 = "#FFFFFF", Gray2 = "#FFFEF9", Gray3 = "#FFF9F0", Gray4 = "#F5EEE5", Gray5 = "#EDE3DA",
            Gray6 = "#DDD3C8", Gray7 = "#C4B8AA", Gray8 = "#A69B8C", Gray9 = "#8A7E70", Gray10 = "#6E6358", Gray11 = "#544C42", Gray12 = "#3A352F", Gray13 = "#26221D",
            ShadowColor = "rgba(168,216,234,0.2)", ShadowColorLight = "rgba(168,216,234,0.12)", ShadowColorDark = "rgba(168,216,234,0.3)"
        };
    }

    public static ColorTheme CreateEarthToneTheme()
    {
        return new ColorTheme
        {
            Id = "preset-earth", Name = "大地色", Description = "沉稳自然的大地色配色方案，适合户外和旅行类应用", Author = "系统", Category = "风格", IsBuiltIn = true,
            PrimaryColor = "#8B7355", PrimaryLight = "#A69076", PrimaryDark = "#6B5642", PrimaryLighter = "#F5F0EB", PrimaryBg = "#D4C4B0",
            SecondaryColor = "#6B8E23", SecondaryLight = "#8FBC3F", SecondaryDark = "#556B2F",
            AccentColor = "#CD853F", AccentLight = "#DDA065", AccentDark = "#B8722E",
            SuccessColor = "#556B2F", SuccessLight = "#6B8E23", SuccessDark = "#3D4F22", SuccessBg = "#F5F0EB", SuccessBorder = "#B8C49A",
            WarningColor = "#DAA520", WarningLight = "#E8C547", WarningDark = "#B8860B", WarningBg = "#FDF8E8", WarningBorder = "#E8D890",
            ErrorColor = "#A0522D", ErrorLight = "#BC7A5B", ErrorDark = "#7A3D22", ErrorBg = "#FAF0EB", ErrorBorder = "#D4B4A0",
            InfoColor = "#708090", InfoLight = "#8FA0B0", InfoDark = "#556070", InfoBg = "#F0F2F4", InfoBorder = "#C0CCD4",
            BackgroundColor = "#F5F0EB", BackgroundSecondary = "#EBE3DA", BackgroundTertiary = "#D4C4B0",
            ForegroundColor = "#3D3225", ForegroundSecondary = "#5C4A38", ForegroundTertiary = "#7A6350",
            BorderColor = "#D4C4B0", BorderSecondary = "#C4B4A0", BorderLight = "#E5DCD0",
            DividerColor = "#C4B4A0", DividerDark = "#3D3225",
            PlaceholderColor = "#8B7355", DisabledColor = "#D4C4B0", DisabledBg = "#C4B4A0",
            HoverColor = "rgba(139,115,85,0.08)", SelectedColor = "#EBE3DA", ActiveColor = "rgba(139,115,85,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#F5F0EB", Gray3 = "#EBE3DA", Gray4 = "#D4C4B0", Gray5 = "#B8A890",
            Gray6 = "#9C8A70", Gray7 = "#8B7355", Gray8 = "#6B5642", Gray9 = "#5C4A38", Gray10 = "#4A3C2E", Gray11 = "#3D3225", Gray12 = "#2E261D", Gray13 = "#1F1A14",
            ShadowColor = "rgba(139,115,85,0.15)", ShadowColorLight = "rgba(139,115,85,0.1)", ShadowColorDark = "rgba(139,115,85,0.25)"
        };
    }

    #endregion

    #region 专业主题

    public static ColorTheme CreateCorporateTheme()
    {
        return new ColorTheme
        {
            Id = "preset-corporate", Name = "企业蓝", Description = "专业稳重的企业配色方案，适合B2B和企业管理系统", Author = "系统", Category = "企业", IsBuiltIn = true,
            PrimaryColor = "#1565C0", PrimaryLight = "#1E88E5", PrimaryDark = "#0D47A1", PrimaryLighter = "#E3F2FD", PrimaryBg = "#BBDEFB",
            SecondaryColor = "#455A64", SecondaryLight = "#607D8B", SecondaryDark = "#37474F",
            AccentColor = "#00ACC1", AccentLight = "#26C6DA", AccentDark = "#00838F",
            SuccessColor = "#2E7D32", SuccessLight = "#43A047", SuccessDark = "#1B5E20", SuccessBg = "#E8F5E9", SuccessBorder = "#A5D6A7",
            WarningColor = "#F9A825", WarningLight = "#FBC02D", WarningDark = "#F57F17", WarningBg = "#FFFDE7", WarningBorder = "#FFF59D",
            ErrorColor = "#C62828", ErrorLight = "#EF5350", ErrorDark = "#8E0000", ErrorBg = "#FFEBEE", ErrorBorder = "#FFCDD2",
            InfoColor = "#0277BD", InfoLight = "#039BE5", InfoDark = "#01579B", InfoBg = "#E1F5FE", InfoBorder = "#81D4FA",
            BackgroundColor = "#F5F7FA", BackgroundSecondary = "#ECEFF1", BackgroundTertiary = "#E0E7ED",
            ForegroundColor = "#1A2530", ForegroundSecondary = "#37474F", ForegroundTertiary = "#546E7A",
            BorderColor = "#CFD8DC", BorderSecondary = "#B0BEC5", BorderLight = "#ECEFF1",
            DividerColor = "#ECEFF1", DividerDark = "#1A2530",
            PlaceholderColor = "#78909C", DisabledColor = "#CFD8DC", DisabledBg = "#ECEFF1",
            HoverColor = "rgba(21,101,192,0.06)", SelectedColor = "#E3F2FD", ActiveColor = "rgba(21,101,192,0.1)",
            Gray1 = "#FFFFFF", Gray2 = "#F5F7FA", Gray3 = "#ECEFF1", Gray4 = "#CFD8DC", Gray5 = "#B0BEC5",
            Gray6 = "#90A4AE", Gray7 = "#78909C", Gray8 = "#607D8B", Gray9 = "#546E7A", Gray10 = "#455A64", Gray11 = "#37474F", Gray12 = "#263238", Gray13 = "#1A2530",
            ShadowColor = "rgba(21,101,192,0.1)", ShadowColorLight = "rgba(21,101,192,0.06)", ShadowColorDark = "rgba(21,101,192,0.18)"
        };
    }

    public static ColorTheme CreateStartupTheme()
    {
        return new ColorTheme
        {
            Id = "preset-startup", Name = "创业蓝", Description = "现代活力的创业配色方案，适合创业公司和SaaS产品", Author = "系统", Category = "科技", IsBuiltIn = true,
            PrimaryColor = "#4F46E5", PrimaryLight = "#6366F1", PrimaryDark = "#3730A3", PrimaryLighter = "#EEF2FF", PrimaryBg = "#C7D2FE",
            SecondaryColor = "#EC4899", SecondaryLight = "#F472B6", SecondaryDark = "#DB2777",
            AccentColor = "#10B981", AccentLight = "#34D399", AccentDark = "#059669",
            SuccessColor = "#10B981", SuccessLight = "#34D399", SuccessDark = "#059669", SuccessBg = "#ECFDF5", SuccessBorder = "#A7F3D0",
            WarningColor = "#F59E0B", WarningLight = "#FBBF24", WarningDark = "#D97706", WarningBg = "#FFFBEB", WarningBorder = "#FDE68A",
            ErrorColor = "#EF4444", ErrorLight = "#F87171", ErrorDark = "#DC2626", ErrorBg = "#FEF2F2", ErrorBorder = "#FECACA",
            InfoColor = "#3B82F6", InfoLight = "#60A5FA", InfoDark = "#2563EB", InfoBg = "#EFF6FF", InfoBorder = "#BFDBFE",
            BackgroundColor = "#FAFAFA", BackgroundSecondary = "#F3F4F6", BackgroundTertiary = "#E5E7EB",
            ForegroundColor = "#111827", ForegroundSecondary = "#374151", ForegroundTertiary = "#6B7280",
            BorderColor = "#E5E7EB", BorderSecondary = "#D1D5DB", BorderLight = "#F3F4F6",
            DividerColor = "#E5E7EB", DividerDark = "#111827",
            PlaceholderColor = "#9CA3AF", DisabledColor = "#D1D5DB", DisabledBg = "#E5E7EB",
            HoverColor = "rgba(79,70,229,0.06)", SelectedColor = "#EEF2FF", ActiveColor = "rgba(79,70,229,0.1)",
            Gray1 = "#FFFFFF", Gray2 = "#FAFAFA", Gray3 = "#F3F4F6", Gray4 = "#E5E7EB", Gray5 = "#D1D5DB",
            Gray6 = "#9CA3AF", Gray7 = "#6B7280", Gray8 = "#4B5563", Gray9 = "#374151", Gray10 = "#1F2937", Gray11 = "#111827", Gray12 = "#0A0F1A", Gray13 = "#050708",
            ShadowColor = "rgba(79,70,229,0.1)", ShadowColorLight = "rgba(79,70,229,0.05)", ShadowColorDark = "rgba(79,70,229,0.15)"
        };
    }

    public static ColorTheme CreateElegantTheme()
    {
        return new ColorTheme
        {
            Id = "preset-elegant", Name = "优雅金", Description = "低调奢华的优雅配色方案，适合高端品牌和奢侈品应用", Author = "系统", Category = "品牌", IsBuiltIn = true,
            PrimaryColor = "#1C1C1C", PrimaryLight = "#2D2D2D", PrimaryDark = "#0A0A0A", PrimaryLighter = "#F7F7F7", PrimaryBg = "#E5E5E5",
            SecondaryColor = "#C9A962", SecondaryLight = "#D4BC7D", SecondaryDark = "#A68B4B",
            AccentColor = "#8B7355", AccentLight = "#A69076", AccentDark = "#6B5642",
            SuccessColor = "#4A5D23", SuccessLight = "#6B8E23", SuccessDark = "#354515", SuccessBg = "#F7F6F0", SuccessBorder = "#D4D5C0",
            WarningColor = "#C9A962", WarningLight = "#D4BC7D", WarningDark = "#A68B4B", WarningBg = "#F9F7F2", WarningBorder = "#E8DFC7",
            ErrorColor = "#8B0000", ErrorLight = "#A52A2A", ErrorDark = "#5C0000", ErrorBg = "#FAF5F5", ErrorBorder = "#E8D4D4",
            InfoColor = "#2F4F4F", InfoLight = "#3D5A5A", InfoDark = "#1F3F3F", InfoBg = "#F0F5F5", InfoBorder = "#C8D8D8",
            BackgroundColor = "#FAFAF8", BackgroundSecondary = "#F5F5F0", BackgroundTertiary = "#EBEBE6",
            ForegroundColor = "#1A1A1A", ForegroundSecondary = "#3D3D3D", ForegroundTertiary = "#5C5C5C",
            BorderColor = "#D4D0C8", BorderSecondary = "#E5E0D8", BorderLight = "#F0EDE8",
            DividerColor = "#E5E0D8", DividerDark = "#1A1A1A",
            PlaceholderColor = "#A8A8A0", DisabledColor = "#D4D0C8", DisabledBg = "#E5E0D8",
            HoverColor = "rgba(0,0,0,0.04)", SelectedColor = "#F5F5F0", ActiveColor = "rgba(0,0,0,0.08)",
            Gray1 = "#FFFFFF", Gray2 = "#FAFAF8", Gray3 = "#F5F5F0", Gray4 = "#E5E0D8", Gray5 = "#D4D0C8",
            Gray6 = "#A8A8A0", Gray7 = "#787870", Gray8 = "#585848", Gray9 = "#3D3D35", Gray10 = "#2D2D25", Gray11 = "#1A1A15", Gray12 = "#0D0D08", Gray13 = "#000000",
            ShadowColor = "rgba(0,0,0,0.08)", ShadowColorLight = "rgba(0,0,0,0.04)", ShadowColorDark = "rgba(201,169,98,0.15)"
        };
    }

    public static ColorTheme CreatePlayfulTheme()
    {
        return new ColorTheme
        {
            Id = "preset-playful", Name = "活泼橙", Description = "活泼可爱的配色方案，适合游戏和娱乐类应用", Author = "系统", Category = "娱乐", IsBuiltIn = true,
            PrimaryColor = "#FF6B6B", PrimaryLight = "#FF8A8A", PrimaryDark = "#EE5A5A", PrimaryLighter = "#FFF5F5", PrimaryBg = "#FFD8D8",
            SecondaryColor = "#4ECDC4", SecondaryLight = "#7EDDD6", SecondaryDark = "#3DBDB4",
            AccentColor = "#FFE66D", AccentLight = "#FFF08D", AccentDark = "#FFD93D",
            SuccessColor = "#6BCB77", SuccessLight = "#95D89A", SuccessDark = "#52B45A", SuccessBg = "#F0FFF0", SuccessBorder = "#C8F5C8",
            WarningColor = "#FFD93D", WarningLight = "#FFE66D", WarningDark = "#E6C235", WarningBg = "#FFFFF5", WarningBorder = "#FFF5C8",
            ErrorColor = "#FF6B6B", ErrorLight = "#FF8A8A", ErrorDark = "#EE5A5A", ErrorBg = "#FFF5F5", ErrorBorder = "#FFD8D8",
            InfoColor = "#4DABF7", InfoLight = "#74C0FC", InfoDark = "#339AF0", InfoBg = "#F0F8FF", InfoBorder = "#C5E3FF",
            BackgroundColor = "#FFFFFF", BackgroundSecondary = "#FFF9F0", BackgroundTertiary = "#FFF0E5",
            ForegroundColor = "#2D3436", ForegroundSecondary = "#4A5568", ForegroundTertiary = "#718096",
            BorderColor = "#FFD8D8", BorderSecondary = "#FFE8E8", BorderLight = "#FFF5F5",
            DividerColor = "#FFE8E8", DividerDark = "#2D3436",
            PlaceholderColor = "#A0AEC0", DisabledColor = "#FFD8D8", DisabledBg = "#FFE8E8",
            HoverColor = "rgba(255,107,107,0.08)", SelectedColor = "#FFF5F5", ActiveColor = "rgba(255,107,107,0.12)",
            Gray1 = "#FFFFFF", Gray2 = "#FAFAFA", Gray3 = "#F5F5F5", Gray4 = "#E8E8E8", Gray5 = "#D8D8D8",
            Gray6 = "#A0AEC0", Gray7 = "#718096", Gray8 = "#4A5568", Gray9 = "#2D3748", Gray10 = "#1A202C", Gray11 = "#171923", Gray12 = "#0D1117", Gray13 = "#000000",
            ShadowColor = "rgba(255,107,107,0.15)", ShadowColorLight = "rgba(255,107,107,0.08)", ShadowColorDark = "rgba(255,107,107,0.25)"
        };
    }

    public static ColorTheme CreateNatureTheme()
    {
        return new ColorTheme
        {
            Id = "preset-nature", Name = "森林绿", Description = "清新自然的森林配色方案，适合户外和自然类应用", Author = "系统", Category = "自然", IsBuiltIn = true,
            PrimaryColor = "#2D5016", PrimaryLight = "#4A7C23", PrimaryDark = "#1E360D", PrimaryLighter = "#E8F0DE", PrimaryBg = "#C5E0A4",
            SecondaryColor = "#8B4513", SecondaryLight = "#A0522D", SecondaryDark = "#6B3410",
            AccentColor = "#DAA520", AccentLight = "#E8C547", AccentDark = "#B8860B",
            SuccessColor = "#228B22", SuccessLight = "#3CB371", SuccessDark = "#1A6B1A", SuccessBg = "#E8F4E8", SuccessBorder = "#C5E0C5",
            WarningColor = "#DAA520", WarningLight = "#E8C547", WarningDark = "#B8860B", WarningBg = "#FFFEF0", WarningBorder = "#F5EDB8",
            ErrorColor = "#8B0000", ErrorLight = "#A52A2A", ErrorDark = "#5C0000", ErrorBg = "#FFF5F0", ErrorBorder = "#E8D0C0",
            InfoColor = "#4682B4", InfoLight = "#6A9EC0", InfoDark = "#2F6A9C", InfoBg = "#F0F8FF", InfoBorder = "#B0D4E8",
            BackgroundColor = "#F5F8F2", BackgroundSecondary = "#E8F0E5", BackgroundTertiary = "#D8E8D0",
            ForegroundColor = "#1A3009", ForegroundSecondary = "#2D5016", ForegroundTertiary = "#3D6820",
            BorderColor = "#A8C89A", BorderSecondary = "#C5E0A4", BorderLight = "#E0F0D0",
            DividerColor = "#C5E0A4", DividerDark = "#1A3009",
            PlaceholderColor = "#708C50", DisabledColor = "#A8C89A", DisabledBg = "#C5E0A4",
            HoverColor = "rgba(45,80,22,0.06)", SelectedColor = "#E8F0DE", ActiveColor = "rgba(45,80,22,0.1)",
            Gray1 = "#FFFFFF", Gray2 = "#F5F8F2", Gray3 = "#E8F0E5", Gray4 = "#D8E8D0", Gray5 = "#C5E0A4",
            Gray6 = "#9AB878", Gray7 = "#708C50", Gray8 = "#506830", Gray9 = "#3D5025", Gray10 = "#2D3A1A", Gray11 = "#1E280F", Gray12 = "#0F1408", Gray13 = "#000000",
            ShadowColor = "rgba(45,80,22,0.12)", ShadowColorLight = "rgba(45,80,22,0.06)", ShadowColorDark = "rgba(45,80,22,0.2)"
        };
    }

    #endregion
}
