using System.ComponentModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 颜色主题 - 包含完整的网页/小程序配色方案
/// 支持组件样式与基础色自动关联
/// </summary>
public partial class ColorTheme : ObservableObject
{
    private bool _isSyncingStyles = false;

    private string _id = Guid.NewGuid().ToString();
    private string _name = "新主题";
    private string _description = "";
    private string _author = "";
    private DateTime _createdAt = DateTime.Now;
    private DateTime _updatedAt = DateTime.Now;
    private bool _isBuiltIn = false;
    private bool _isDefault = false;
    private string _category = "自定义";

    /// <summary>
    /// 是否自动同步组件样式
    /// </summary>
    [ObservableProperty] private bool _autoSyncStyles = true;

    public string Id { get => _id; set => SetProperty(ref _id, value); }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                UpdatedAt = DateTime.Now;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
    }

    public string Description { get => _description; set => SetProperty(ref _description, value); }
    public string Author { get => _author; set => SetProperty(ref _author, value); }
    public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }
    public DateTime UpdatedAt { get => _updatedAt; set => SetProperty(ref _updatedAt, value); }
    public bool IsBuiltIn { get => _isBuiltIn; set => SetProperty(ref _isBuiltIn, value); }
    public bool IsDefault { get => _isDefault; set => SetProperty(ref _isDefault, value); }
    public string Category { get => _category; set => SetProperty(ref _category, value); }

    [JsonIgnore]
    public string DisplayName => IsDefault ? $"{Name} (默认)" : Name;

    #region 自动同步机制

    /// <summary>
    /// 当主色变化时，自动更新相关组件样式
    /// </summary>
    partial void OnPrimaryColorChanged(string value)
    {
        if (_isSyncingStyles || !AutoSyncStyles) return;
        SyncComponentStylesFromBase();
    }

    /// <summary>
    /// 当背景色变化时，自动更新页面面板样式
    /// </summary>
    partial void OnBackgroundColorChanged(string value)
    {
        if (_isSyncingStyles || !AutoSyncStyles) return;
        SyncPagePanelStylesFromBase();
    }

    /// <summary>
    /// 当边框色变化时，自动更新相关组件样式
    /// </summary>
    partial void OnBorderColorChanged(string value)
    {
        if (_isSyncingStyles || !AutoSyncStyles) return;
        SyncComponentStylesFromBase();
    }

    /// <summary>
    /// 当前景色变化时，自动更新页面面板样式
    /// </summary>
    partial void OnForegroundColorChanged(string value)
    {
        if (_isSyncingStyles || !AutoSyncStyles) return;
        SyncPagePanelStylesFromBase();
    }

    /// <summary>
    /// 当主色变化时，同步更新组件样式
    /// </summary>
    private void SyncComponentStylesFromBase()
    {
        _isSyncingStyles = true;
        try
        {
            var isDark = ColorCalculator.IsDarkTheme(BorderColor);

            // 主按钮样式 - 使用主色
            FormElementStyle.PrimaryBackgroundColor = PrimaryColor;
            FormElementStyle.PrimaryForegroundColor = ColorCalculator.GetContrastColor(PrimaryColor);
            FormElementStyle.PrimaryBorderColor = PrimaryColor;
            FormElementStyle.PrimaryHoverBackgroundColor = PrimaryDark;
            FormElementStyle.PrimaryHoverForegroundColor = ColorCalculator.GetContrastColor(PrimaryDark);
            FormElementStyle.PrimaryHoverBorderColor = PrimaryDark;
            FormElementStyle.PrimaryActiveBackgroundColor = ColorCalculator.Darken(PrimaryDark, 0.1);
            FormElementStyle.PrimaryActiveForegroundColor = ColorCalculator.GetContrastColor(PrimaryDark);
            FormElementStyle.PrimaryActiveBorderColor = PrimaryDark;
            FormElementStyle.PrimaryDisabledBackgroundColor = isDark ? "#434343" : "#D9D9D9";
            FormElementStyle.PrimaryDisabledForegroundColor = isDark ? "#666666" : "#BFBFBF";
            FormElementStyle.PrimaryDisabledBorderColor = isDark ? "#434343" : "#D9D9D9";

            // 次要按钮样式 - 使用次色
            FormElementStyle.SecondaryBackgroundColor = BackgroundColor;
            FormElementStyle.SecondaryForegroundColor = ForegroundColor;
            FormElementStyle.SecondaryBorderColor = BorderColor;
            FormElementStyle.SecondaryHoverBackgroundColor = BackgroundSecondary;
            FormElementStyle.SecondaryHoverForegroundColor = ForegroundColor;
            FormElementStyle.SecondaryHoverBorderColor = PrimaryColor;
            FormElementStyle.SecondaryActiveBackgroundColor = PrimaryBg;
            FormElementStyle.SecondaryActiveForegroundColor = PrimaryDark;
            FormElementStyle.SecondaryActiveBorderColor = PrimaryColor;
            FormElementStyle.SecondaryDisabledBackgroundColor = DisabledBg;
            FormElementStyle.SecondaryDisabledForegroundColor = DisabledColor;
            FormElementStyle.SecondaryDisabledBorderColor = BorderColor;

            // 输入框样式
            FormElementStyle.InputBackgroundColor = BackgroundColor;
            FormElementStyle.InputForegroundColor = ForegroundColor;
            FormElementStyle.InputBorderColor = BorderColor;
            FormElementStyle.InputHoverBorderColor = ColorCalculator.Lighten(BorderColor, 0.2);
            FormElementStyle.InputFocusBorderColor = PrimaryColor;
            FormElementStyle.InputFocusShadowColor = $"{PrimaryColor}33";
            FormElementStyle.InputPlaceholderColor = PlaceholderColor;

            // 选中颜色
            SelectedColor = ColorCalculator.GetSelectedBg(PrimaryColor);
            // 悬停颜色
            HoverColor = $"{BorderColor}0A";
            // 激活颜色
            ActiveColor = $"{BorderColor}14";
        }
        finally
        {
            _isSyncingStyles = false;
        }
    }

    /// <summary>
    /// 当背景色变化时，同步更新页面面板样式
    /// </summary>
    private void SyncPagePanelStylesFromBase()
    {
        _isSyncingStyles = true;
        try
        {
            var isDark = ColorCalculator.IsDarkTheme(BackgroundColor);

            // 页面容器样式
            PagePanelStyle.PageBackgroundColor = BackgroundColor;
            PagePanelStyle.PageForegroundColor = ForegroundColor;
            PagePanelStyle.PageBorderColor = BorderColor;

            // 面板样式
            PagePanelStyle.PanelBackgroundColor = BackgroundColor;
            PagePanelStyle.PanelForegroundColor = ForegroundColor;
            PagePanelStyle.PanelBorderColor = BorderColor;
            PagePanelStyle.PanelHoverBorderColor = PrimaryColor;
            PagePanelStyle.PanelSelectedBackgroundColor = SelectedColor;
            PagePanelStyle.PanelSelectedBorderColor = PrimaryColor;

            // 分割线样式
            PagePanelStyle.DividerDefaultColor = DividerColor;
            PagePanelStyle.DividerDisabledColor = isDark ? "#303030" : "#F0F0F0";
            PagePanelStyle.DividerVerticalColor = DividerColor;

            // 标签页样式
            PagePanelStyle.TabBackgroundColor = BackgroundColor;
            PagePanelStyle.TabForegroundColor = ForegroundSecondary;
            PagePanelStyle.TabHoverBackgroundColor = BackgroundSecondary;
            PagePanelStyle.TabHoverForegroundColor = ForegroundColor;
            PagePanelStyle.TabSelectedBackgroundColor = BackgroundColor;
            PagePanelStyle.TabSelectedForegroundColor = PrimaryColor;
            PagePanelStyle.TabSelectedUnderlineColor = PrimaryColor;
            PagePanelStyle.TabDisabledForegroundColor = isDark ? "#666666" : "#BFBFBF";

            // 列表项样式
            PagePanelStyle.ListItemBackgroundColor = "transparent";
            PagePanelStyle.ListItemForegroundColor = ForegroundColor;
            PagePanelStyle.ListItemHoverBackgroundColor = HoverColor;
            PagePanelStyle.ListItemSelectedBackgroundColor = SelectedColor;
            PagePanelStyle.ListItemSelectedForegroundColor = PrimaryColor;
            PagePanelStyle.ListItemActiveBackgroundColor = ActiveColor;
            PagePanelStyle.ListItemActiveForegroundColor = PrimaryColor;
            PagePanelStyle.ListItemDisabledForegroundColor = DisabledColor;

            // 表格样式
            PagePanelStyle.TableHeaderBackgroundColor = BackgroundSecondary;
            PagePanelStyle.TableHeaderForegroundColor = ForegroundColor;
            PagePanelStyle.TableHeaderBorderColor = BorderColor;
            PagePanelStyle.TableCellBackgroundColor = BackgroundColor;
            PagePanelStyle.TableCellForegroundColor = ForegroundColor;
            PagePanelStyle.TableCellBorderColor = BorderColor;
            PagePanelStyle.TableRowHoverBackgroundColor = HoverColor;
            PagePanelStyle.TableRowSelectedBackgroundColor = SelectedColor;
        }
        finally
        {
            _isSyncingStyles = false;
        }
    }

    /// <summary>
    /// 强制同步所有组件样式
    /// </summary>
    public void ForceSyncAllStyles()
    {
        SyncComponentStylesFromBase();
        SyncPagePanelStylesFromBase();
    }

    #endregion

    #region 语义色 Semantic Colors

    [ObservableProperty] private string _primaryColor = "#1890FF";
    [ObservableProperty] private string _primaryLight = "#40A9FF";
    [ObservableProperty] private string _primaryDark = "#096DD9";
    [ObservableProperty] private string _primaryLighter = "#E6F7FF";
    [ObservableProperty] private string _primaryBg = "#BAE7FF";

    [ObservableProperty] private string _secondaryColor = "#722ED1";
    [ObservableProperty] private string _secondaryLight = "#9254DE";
    [ObservableProperty] private string _secondaryDark = "#531DAB";
    [ObservableProperty] private string _secondaryLighter = "#F3E5F5";
    [ObservableProperty] private string _secondaryBg = "#E1BEE7";

    [ObservableProperty] private string _accentColor = "#FA8C16";
    [ObservableProperty] private string _accentLight = "#FFBB00";
    [ObservableProperty] private string _accentDark = "#D46B00";

    [ObservableProperty] private string _successColor = "#52C41A";
    [ObservableProperty] private string _successLight = "#73D13D";
    [ObservableProperty] private string _successDark = "#389E0D";
    [ObservableProperty] private string _successBg = "#F6FFED";
    [ObservableProperty] private string _successBorder = "#B7EB8F";

    [ObservableProperty] private string _warningColor = "#FAAD14";
    [ObservableProperty] private string _warningLight = "#FFC53D";
    [ObservableProperty] private string _warningDark = "#D48806";
    [ObservableProperty] private string _warningBg = "#FFFBE6";
    [ObservableProperty] private string _warningBorder = "#FFE58F";

    [ObservableProperty] private string _errorColor = "#FF4D4F";
    [ObservableProperty] private string _errorLight = "#FF7875";
    [ObservableProperty] private string _errorDark = "#CF1322";
    [ObservableProperty] private string _errorBg = "#FFF2F0";
    [ObservableProperty] private string _errorBorder = "#FFCCC7";

    [ObservableProperty] private string _infoColor = "#1890FF";
    [ObservableProperty] private string _infoLight = "#40A9FF";
    [ObservableProperty] private string _infoDark = "#096DD9";
    [ObservableProperty] private string _infoBg = "#E6F7FF";
    [ObservableProperty] private string _infoBorder = "#91D5FF";

    #endregion

    #region 中性色 Neutral Colors

    [ObservableProperty] private string _backgroundColor = "#FFFFFF";
    [ObservableProperty] private string _backgroundSecondary = "#FAFAFA";
    [ObservableProperty] private string _backgroundTertiary = "#F5F5F5";
    [ObservableProperty] private string _backgroundDark = "#000000";
    [ObservableProperty] private string _backgroundDarkSecondary = "#141414";
    [ObservableProperty] private string _backgroundDarkTertiary = "#1F1F1F";

    [ObservableProperty] private string _foregroundColor = "rgba(0, 0, 0, 0.85)";
    [ObservableProperty] private string _foregroundSecondary = "rgba(0, 0, 0, 0.65)";
    [ObservableProperty] private string _foregroundTertiary = "rgba(0, 0, 0, 0.45)";
    [ObservableProperty] private string _foregroundQuaternary = "rgba(0, 0, 0, 0.25)";
    [ObservableProperty] private string _foregroundDisabled = "rgba(0, 0, 0, 0.25)";

    [ObservableProperty] private string _borderColor = "#D9D9D9";
    [ObservableProperty] private string _borderSecondary = "#E8E8E8";
    [ObservableProperty] private string _borderLight = "#F0F0F0";
    [ObservableProperty] private string _borderDark = "#434343";

    [ObservableProperty] private string _dividerColor = "#E8E8E8";
    [ObservableProperty] private string _dividerDark = "#303030";

    [ObservableProperty] private string _placeholderColor = "#BFBFBF";
    [ObservableProperty] private string _disabledColor = "#D9D9D9";
    [ObservableProperty] private string _disabledBg = "#F5F5F5";

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ColorTheme()
    {
        // 强制同步所有样式，确保主题初始化时所有样式都正确设置
        ForceSyncAllStyles();
    }

    [ObservableProperty] private string _hoverColor = "rgba(0, 0, 0, 0.04)";
    [ObservableProperty] private string _selectedColor = "#E6F7FF";
    [ObservableProperty] private string _activeColor = "rgba(0, 0, 0, 0.08)";

    #endregion

    #region 灰度色 Gray Scale

    [ObservableProperty] private string _gray1 = "#FFFFFF";
    [ObservableProperty] private string _gray2 = "#FAFAFA";
    [ObservableProperty] private string _gray3 = "#F5F5F5";
    [ObservableProperty] private string _gray4 = "#E8E8E8";
    [ObservableProperty] private string _gray5 = "#D9D9D9";
    [ObservableProperty] private string _gray6 = "#BFBFBF";
    [ObservableProperty] private string _gray7 = "#8C8C8C";
    [ObservableProperty] private string _gray8 = "#595959";
    [ObservableProperty] private string _gray9 = "#434343";
    [ObservableProperty] private string _gray10 = "#262626";
    [ObservableProperty] private string _gray11 = "#1F1F1F";
    [ObservableProperty] private string _gray12 = "#141414";
    [ObservableProperty] private string _gray13 = "#000000";

    #endregion

    #region 阴影色 Shadow Colors

    [ObservableProperty] private string _shadowColor = "rgba(0, 0, 0, 0.12)";
    [ObservableProperty] private string _shadowColorLight = "rgba(0, 0, 0, 0.08)";
    [ObservableProperty] private string _shadowColorDark = "rgba(0, 0, 0, 0.25)";

    #endregion

    #region 渐变色 Gradients

    [ObservableProperty] private string _gradientPrimary = "linear-gradient(135deg, #667eea 0%, #764ba2 100%)";
    [ObservableProperty] private string _gradientSuccess = "linear-gradient(135deg, #11998e 0%, #38ef7d 100%)";
    [ObservableProperty] private string _gradientWarning = "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)";
    [ObservableProperty] private string _gradientInfo = "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)";

    #endregion

    #region 容器样式

    [ObservableProperty] private ContainerStyle _mainContainerStyle = new();
    [ObservableProperty] private ContainerStyle _cardContainerStyle = new();
    [ObservableProperty] private ContainerStyle _buttonContainerStyle = new();
    [ObservableProperty] private ContainerStyle _inputContainerStyle = new();

    #endregion

    #region 表单元素样式

    [ObservableProperty] private FormElementStyle _formElementStyle = new();

    #endregion

    #region 页面面板样式

    [ObservableProperty] private PagePanelStyle _pagePanelStyle = new();

    #endregion

    #region 文字样式

    [ObservableProperty] private TextStyle _headingTextStyle = new() { FontSize = 24, FontWeight = FontWeight.Bold, Color = "#333333" };
    [ObservableProperty] private TextStyle _subheadingTextStyle = new() { FontSize = 18, FontWeight = FontWeight.SemiBold, Color = "#333333" };
    [ObservableProperty] private TextStyle _bodyTextStyle = new() { FontSize = 14, Color = "#333333", LineHeight = 1.6 };
    [ObservableProperty] private TextStyle _captionTextStyle = new() { FontSize = 12, Color = "#999999" };
    [ObservableProperty] private TextStyle _linkTextStyle = new() { Color = "#1890FF", TextDecoration = TextDecoration.Underline };
    [ObservableProperty] private TextStyle _buttonTextStyle = new() { FontSize = 14, FontWeight = FontWeight.Medium };

    #endregion

    /// <summary>
    /// 创建主题副本
    /// </summary>
    public ColorTheme Clone()
    {
        return new ColorTheme
        {
            Id = Guid.NewGuid().ToString(),
            Name = $"{Name} (副本)",
            Description = Description,
            Author = Author,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsBuiltIn = false,
            IsDefault = false,
            Category = Category,
            PrimaryColor = PrimaryColor, PrimaryLight = PrimaryLight, PrimaryDark = PrimaryDark, PrimaryLighter = PrimaryLighter, PrimaryBg = PrimaryBg,
            SecondaryColor = SecondaryColor, SecondaryLight = SecondaryLight, SecondaryDark = SecondaryDark, SecondaryLighter = SecondaryLighter, SecondaryBg = SecondaryBg,
            AccentColor = AccentColor, AccentLight = AccentLight, AccentDark = AccentDark,
            SuccessColor = SuccessColor, SuccessLight = SuccessLight, SuccessDark = SuccessDark, SuccessBg = SuccessBg, SuccessBorder = SuccessBorder,
            WarningColor = WarningColor, WarningLight = WarningLight, WarningDark = WarningDark, WarningBg = WarningBg, WarningBorder = WarningBorder,
            ErrorColor = ErrorColor, ErrorLight = ErrorLight, ErrorDark = ErrorDark, ErrorBg = ErrorBg, ErrorBorder = ErrorBorder,
            InfoColor = InfoColor, InfoLight = InfoLight, InfoDark = InfoDark, InfoBg = InfoBg, InfoBorder = InfoBorder,
            BackgroundColor = BackgroundColor, BackgroundSecondary = BackgroundSecondary, BackgroundTertiary = BackgroundTertiary,
            BackgroundDark = BackgroundDark, BackgroundDarkSecondary = BackgroundDarkSecondary, BackgroundDarkTertiary = BackgroundDarkTertiary,
            ForegroundColor = ForegroundColor, ForegroundSecondary = ForegroundSecondary, ForegroundTertiary = ForegroundTertiary,
            ForegroundQuaternary = ForegroundQuaternary, ForegroundDisabled = ForegroundDisabled,
            BorderColor = BorderColor, BorderSecondary = BorderSecondary, BorderLight = BorderLight, BorderDark = BorderDark,
            DividerColor = DividerColor, DividerDark = DividerDark,
            PlaceholderColor = PlaceholderColor, DisabledColor = DisabledColor, DisabledBg = DisabledBg,
            HoverColor = HoverColor, SelectedColor = SelectedColor, ActiveColor = ActiveColor,
            Gray1 = Gray1, Gray2 = Gray2, Gray3 = Gray3, Gray4 = Gray4, Gray5 = Gray5, Gray6 = Gray6,
            Gray7 = Gray7, Gray8 = Gray8, Gray9 = Gray9, Gray10 = Gray10, Gray11 = Gray11, Gray12 = Gray12, Gray13 = Gray13,
            ShadowColor = ShadowColor, ShadowColorLight = ShadowColorLight, ShadowColorDark = ShadowColorDark,
            GradientPrimary = GradientPrimary, GradientSuccess = GradientSuccess, GradientWarning = GradientWarning, GradientInfo = GradientInfo,
            MainContainerStyle = MainContainerStyle.Clone(),
            CardContainerStyle = CardContainerStyle.Clone(),
            ButtonContainerStyle = ButtonContainerStyle.Clone(),
            InputContainerStyle = InputContainerStyle.Clone(),
            FormElementStyle = FormElementStyle.Clone(),
            PagePanelStyle = PagePanelStyle.Clone(),
            HeadingTextStyle = HeadingTextStyle.Clone(),
            SubheadingTextStyle = SubheadingTextStyle.Clone(),
            BodyTextStyle = BodyTextStyle.Clone(),
            CaptionTextStyle = CaptionTextStyle.Clone(),
            LinkTextStyle = LinkTextStyle.Clone(),
            ButtonTextStyle = ButtonTextStyle.Clone()
        };
    }

    /// <summary>
    /// 生成完整CSS变量
    /// </summary>
    public string ToCssVariables()
    {
        return $@"/* Theme: {Name} | Author: {Author} */
/* Generated by ColorSelector */
:root {{
    /* === Primary Colors === */
    --primary-color: {PrimaryColor};
    --primary-light: {PrimaryLight};
    --primary-dark: {PrimaryDark};
    --primary-lighter: {PrimaryLighter};
    --primary-bg: {PrimaryBg};

    /* === Secondary Colors === */
    --secondary-color: {SecondaryColor};
    --secondary-light: {SecondaryLight};
    --secondary-dark: {SecondaryDark};
    --secondary-lighter: {SecondaryLighter};
    --secondary-bg: {SecondaryBg};

    /* === Accent Colors === */
    --accent-color: {AccentColor};
    --accent-light: {AccentLight};
    --accent-dark: {AccentDark};

    /* === Semantic Colors === */
    --success-color: {SuccessColor};
    --success-light: {SuccessLight};
    --success-dark: {SuccessDark};
    --success-bg: {SuccessBg};
    --success-border: {SuccessBorder};

    --warning-color: {WarningColor};
    --warning-light: {WarningLight};
    --warning-dark: {WarningDark};
    --warning-bg: {WarningBg};
    --warning-border: {WarningBorder};

    --error-color: {ErrorColor};
    --error-light: {ErrorLight};
    --error-dark: {ErrorDark};
    --error-bg: {ErrorBg};
    --error-border: {ErrorBorder};

    --info-color: {InfoColor};
    --info-light: {InfoLight};
    --info-dark: {InfoDark};
    --info-bg: {InfoBg};
    --info-border: {InfoBorder};

    /* === Background Colors === */
    --bg-color: {BackgroundColor};
    --bg-secondary: {BackgroundSecondary};
    --bg-tertiary: {BackgroundTertiary};
    --bg-dark: {BackgroundDark};
    --bg-dark-secondary: {BackgroundDarkSecondary};
    --bg-dark-tertiary: {BackgroundDarkTertiary};

    /* === Foreground Colors === */
    --text-primary: {ForegroundColor};
    --text-secondary: {ForegroundSecondary};
    --text-tertiary: {ForegroundTertiary};
    --text-quaternary: {ForegroundQuaternary};
    --text-disabled: {ForegroundDisabled};

    /* === Border Colors === */
    --border-color: {BorderColor};
    --border-secondary: {BorderSecondary};
    --border-light: {BorderLight};
    --border-dark: {BorderDark};

    /* === Divider Colors === */
    --divider-color: {DividerColor};
    --divider-dark: {DividerDark};

    /* === State Colors === */
    --placeholder-color: {PlaceholderColor};
    --disabled-color: {DisabledColor};
    --disabled-bg: {DisabledBg};
    --hover-color: {HoverColor};
    --selected-color: {SelectedColor};
    --active-color: {ActiveColor};

    /* === Gray Scale === */
    --gray-1: {Gray1};
    --gray-2: {Gray2};
    --gray-3: {Gray3};
    --gray-4: {Gray4};
    --gray-5: {Gray5};
    --gray-6: {Gray6};
    --gray-7: {Gray7};
    --gray-8: {Gray8};
    --gray-9: {Gray9};
    --gray-10: {Gray10};
    --gray-11: {Gray11};
    --gray-12: {Gray12};
    --gray-13: {Gray13};

    /* === Shadow Colors === */
    --shadow-color: {ShadowColor};
    --shadow-color-light: {ShadowColorLight};
    --shadow-color-dark: {ShadowColorDark};

    /* === Button Styles === */
    /* Primary Button */
    --btn-primary-bg: {FormElementStyle.PrimaryBackgroundColor};
    --btn-primary-color: {FormElementStyle.PrimaryForegroundColor};
    --btn-primary-border: {FormElementStyle.PrimaryBorderColor};
    --btn-primary-hover-bg: {FormElementStyle.PrimaryHoverBackgroundColor};
    --btn-primary-hover-color: {FormElementStyle.PrimaryHoverForegroundColor};
    --btn-primary-hover-border: {FormElementStyle.PrimaryHoverBorderColor};
    --btn-primary-active-bg: {FormElementStyle.PrimaryActiveBackgroundColor};
    --btn-primary-active-color: {FormElementStyle.PrimaryActiveForegroundColor};
    --btn-primary-active-border: {FormElementStyle.PrimaryActiveBorderColor};
    --btn-primary-disabled-bg: {FormElementStyle.PrimaryDisabledBackgroundColor};
    --btn-primary-disabled-color: {FormElementStyle.PrimaryDisabledForegroundColor};
    --btn-primary-disabled-border: {FormElementStyle.PrimaryDisabledBorderColor};

    /* Secondary Button */
    --btn-secondary-bg: {FormElementStyle.SecondaryBackgroundColor};
    --btn-secondary-color: {FormElementStyle.SecondaryForegroundColor};
    --btn-secondary-border: {FormElementStyle.SecondaryBorderColor};
    --btn-secondary-hover-bg: {FormElementStyle.SecondaryHoverBackgroundColor};
    --btn-secondary-hover-color: {FormElementStyle.SecondaryHoverForegroundColor};
    --btn-secondary-hover-border: {FormElementStyle.SecondaryHoverBorderColor};
    --btn-secondary-active-bg: {FormElementStyle.SecondaryActiveBackgroundColor};
    --btn-secondary-active-color: {FormElementStyle.SecondaryActiveForegroundColor};
    --btn-secondary-active-border: {FormElementStyle.SecondaryActiveBorderColor};
    --btn-secondary-disabled-bg: {FormElementStyle.SecondaryDisabledBackgroundColor};
    --btn-secondary-disabled-color: {FormElementStyle.SecondaryDisabledForegroundColor};
    --btn-secondary-disabled-border: {FormElementStyle.SecondaryDisabledBorderColor};

    /* Input Styles */
    --input-bg: {FormElementStyle.InputBackgroundColor};
    --input-color: {FormElementStyle.InputForegroundColor};
    --input-border: {FormElementStyle.InputBorderColor};
    --input-hover-border: {FormElementStyle.InputHoverBorderColor};
    --input-focus-border: {FormElementStyle.InputFocusBorderColor};
    --input-focus-shadow: {FormElementStyle.InputFocusShadowColor};
    --input-disabled-bg: {FormElementStyle.InputDisabledBackgroundColor};
    --input-disabled-color: {FormElementStyle.InputDisabledForegroundColor};
    --input-disabled-border: {FormElementStyle.InputDisabledBorderColor};
    --input-placeholder: {FormElementStyle.InputPlaceholderColor};

    /* Panel Styles */
    --panel-bg: {PagePanelStyle.PanelBackgroundColor};
    --panel-color: {PagePanelStyle.PanelForegroundColor};
    --panel-border: {PagePanelStyle.PanelBorderColor};
    --panel-hover-border: {PagePanelStyle.PanelHoverBorderColor};
    --panel-selected-bg: {PagePanelStyle.PanelSelectedBackgroundColor};
    --panel-selected-border: {PagePanelStyle.PanelSelectedBorderColor};

    /* Tab Styles */
    --tab-bg: {PagePanelStyle.TabBackgroundColor};
    --tab-color: {PagePanelStyle.TabForegroundColor};
    --tab-hover-bg: {PagePanelStyle.TabHoverBackgroundColor};
    --tab-hover-color: {PagePanelStyle.TabHoverForegroundColor};
    --tab-selected-bg: {PagePanelStyle.TabSelectedBackgroundColor};
    --tab-selected-color: {PagePanelStyle.TabSelectedForegroundColor};
    --tab-selected-underline: {PagePanelStyle.TabSelectedUnderlineColor};
    --tab-disabled-color: {PagePanelStyle.TabDisabledForegroundColor};

    /* Divider Styles */
    --divider-default: {PagePanelStyle.DividerDefaultColor};
    --divider-disabled: {PagePanelStyle.DividerDisabledColor};
    --divider-vertical: {PagePanelStyle.DividerVerticalColor};

    /* List Item Styles */
    --list-item-bg: {PagePanelStyle.ListItemBackgroundColor};
    --list-item-color: {PagePanelStyle.ListItemForegroundColor};
    --list-item-hover-bg: {PagePanelStyle.ListItemHoverBackgroundColor};
    --list-item-selected-bg: {PagePanelStyle.ListItemSelectedBackgroundColor};
    --list-item-selected-color: {PagePanelStyle.ListItemSelectedForegroundColor};
    --list-item-active-bg: {PagePanelStyle.ListItemActiveBackgroundColor};
    --list-item-active-color: {PagePanelStyle.ListItemActiveForegroundColor};
    --list-item-disabled-color: {PagePanelStyle.ListItemDisabledForegroundColor};

    /* Table Styles */
    --table-header-bg: {PagePanelStyle.TableHeaderBackgroundColor};
    --table-header-color: {PagePanelStyle.TableHeaderForegroundColor};
    --table-header-border: {PagePanelStyle.TableHeaderBorderColor};
    --table-cell-bg: {PagePanelStyle.TableCellBackgroundColor};
    --table-cell-color: {PagePanelStyle.TableCellForegroundColor};
    --table-cell-border: {PagePanelStyle.TableCellBorderColor};
    --table-row-hover-bg: {PagePanelStyle.TableRowHoverBackgroundColor};
    --table-row-selected-bg: {PagePanelStyle.TableRowSelectedBackgroundColor};
}}";
    }

    /// <summary>
    /// 生成完整CSS
    /// </summary>
    public string ToFullCss()
    {
        var panelShadow = PagePanelStyle.ShadowOffsetX != 0 || PagePanelStyle.ShadowOffsetY != 0
            ? $"box-shadow: {PagePanelStyle.ShadowOffsetX}px {PagePanelStyle.ShadowOffsetY}px {PagePanelStyle.ShadowBlur}px {PagePanelStyle.ShadowColor};"
            : "";

        return $@"{ToCssVariables()}

/* ========================================
   UTILITY CLASSES
   ======================================== */

/* === Page & Container Styles === */
.page-container {{
    background-color: {PagePanelStyle.PageBackgroundColor};
    color: {PagePanelStyle.PageForegroundColor};
}}

.panel {{
    background-color: var(--panel-bg);
    color: var(--panel-color);
    border: 1px solid var(--panel-border);
    border-radius: {PagePanelStyle.BorderRadius}px;
    padding: {PagePanelStyle.Padding}px;
    {(string.IsNullOrEmpty(panelShadow) ? "" : panelShadow)}
}}
.panel:hover {{
    border-color: var(--panel-hover-border);
}}

/* === Divider Styles === */
.divider {{
    border: none;
    border-top: 1px solid var(--divider-default);
    margin: 16px 0;
}}
.divider-vertical {{
    display: inline-block;
    width: 1px;
    height: 100%;
    background-color: var(--divider-vertical);
    vertical-align: middle;
}}
.divider-disabled {{
    border-top-color: var(--divider-disabled);
    opacity: 0.5;
}}

/* === Button Styles === */
/* Primary Button */
.btn-primary {{
    background-color: var(--btn-primary-bg);
    color: var(--btn-primary-color);
    border: 1px solid var(--btn-primary-border);
    border-radius: {FormElementStyle.BorderRadius}px;
    padding: {FormElementStyle.PaddingVertical}px {FormElementStyle.PaddingHorizontal}px;
    font-size: {FormElementStyle.FontSize}px;
    cursor: pointer;
    transition: all 0.2s ease;
}}
.btn-primary:hover {{
    background-color: var(--btn-primary-hover-bg);
    color: var(--btn-primary-hover-color);
    border-color: var(--btn-primary-hover-border);
}}
.btn-primary:active {{
    background-color: var(--btn-primary-active-bg);
    color: var(--btn-primary-active-color);
    border-color: var(--btn-primary-active-border);
}}
.btn-primary:disabled {{
    background-color: var(--btn-primary-disabled-bg);
    color: var(--btn-primary-disabled-color);
    border-color: var(--btn-primary-disabled-border);
    opacity: 0.5;
    cursor: not-allowed;
}}

/* Secondary Button */
.btn-secondary {{
    background-color: var(--btn-secondary-bg);
    color: var(--btn-secondary-color);
    border: 1px solid var(--btn-secondary-border);
    border-radius: {FormElementStyle.BorderRadius}px;
    padding: {FormElementStyle.PaddingVertical}px {FormElementStyle.PaddingHorizontal}px;
    font-size: {FormElementStyle.FontSize}px;
    cursor: pointer;
    transition: all 0.2s ease;
}}
.btn-secondary:hover {{
    background-color: var(--btn-secondary-hover-bg);
    color: var(--btn-secondary-hover-color);
    border-color: var(--btn-secondary-hover-border);
}}
.btn-secondary:active {{
    background-color: var(--btn-secondary-active-bg);
    color: var(--btn-secondary-active-color);
    border-color: var(--btn-secondary-active-border);
}}
.btn-secondary:disabled {{
    background-color: var(--btn-secondary-disabled-bg);
    color: var(--btn-secondary-disabled-color);
    border-color: var(--btn-secondary-disabled-border);
    opacity: 0.5;
    cursor: not-allowed;
}}

/* Dashed Button */
.btn-dashed {{
    background-color: transparent;
    color: var(--text-primary);
    border: 1px dashed var(--border-color);
    border-radius: {FormElementStyle.BorderRadius}px;
    padding: {FormElementStyle.PaddingVertical}px {FormElementStyle.PaddingHorizontal}px;
    font-size: {FormElementStyle.FontSize}px;
    cursor: pointer;
}}
.btn-dashed:hover {{
    color: var(--primary-color);
    border-color: var(--primary-color);
}}

/* === Input Styles === */
.input {{
    background-color: var(--input-bg);
    color: var(--input-color);
    border: 1px solid var(--input-border);
    border-radius: {FormElementStyle.BorderRadius}px;
    padding: {FormElementStyle.PaddingVertical}px {FormElementStyle.PaddingHorizontal}px;
    font-size: {FormElementStyle.FontSize}px;
    outline: none;
    transition: all 0.2s ease;
}}
.input:hover {{
    border-color: var(--input-hover-border);
}}
.input:focus {{
    border-color: var(--input-focus-border);
    box-shadow: 0 0 0 2px var(--input-focus-shadow);
}}
.input::placeholder {{
    color: var(--input-placeholder);
}}
.input:disabled {{
    background-color: var(--input-disabled-bg);
    color: var(--input-disabled-color);
    border-color: var(--input-disabled-border);
    cursor: not-allowed;
}}

/* === Tab Styles === */
.tabs {{
    display: flex;
    border-bottom: 1px solid var(--border-color);
}}
.tab {{
    padding: 12px 16px;
    color: var(--tab-color);
    background: transparent;
    border: none;
    cursor: pointer;
    position: relative;
    transition: all 0.2s ease;
}}
.tab:hover {{
    color: var(--tab-hover-color);
    background: var(--tab-hover-bg);
}}
.tab.active {{
    color: var(--tab-selected-color);
    background: var(--tab-selected-bg);
}}
.tab.active::after {{
    content: '';
    position: absolute;
    bottom: -1px;
    left: 0;
    right: 0;
    height: 2px;
    background-color: var(--tab-selected-underline);
}}
.tab:disabled {{
    color: var(--tab-disabled-color);
    cursor: not-allowed;
}}

/* === List Item Styles === */
.list-item {{
    padding: 12px 16px;
    color: var(--list-item-color);
    background: var(--list-item-bg);
    cursor: pointer;
    transition: all 0.2s ease;
}}
.list-item:hover {{
    background: var(--list-item-hover-bg);
}}
.list-item.selected {{
    background: var(--list-item-selected-bg);
    color: var(--list-item-selected-color);
}}
.list-item.active {{
    background: var(--list-item-active-bg);
    color: var(--list-item-active-color);
}}
.list-item:disabled {{
    color: var(--list-item-disabled-color);
    cursor: not-allowed;
}}

/* === Table Styles === */
.table {{
    width: 100%;
    border-collapse: collapse;
}}
.table th {{
    background-color: var(--table-header-bg);
    color: var(--table-header-color);
    border: 1px solid var(--table-header-border);
    padding: 12px;
    text-align: left;
    font-weight: 600;
}}
.table td {{
    background-color: var(--table-cell-bg);
    color: var(--table-cell-color);
    border: 1px solid var(--table-cell-border);
    padding: 12px;
}}
.table tr:hover td {{
    background-color: var(--table-row-hover-bg);
}}
.table tr.selected td {{
    background-color: var(--table-row-selected-bg);
}}

/* === Alert Styles === */
.alert-success {{
    background-color: var(--success-bg);
    border-color: var(--success-border);
    color: var(--success-dark);
    padding: 12px 16px;
    border-radius: {FormElementStyle.BorderRadius}px;
    border-left: 4px solid var(--success-color);
}}

.alert-warning {{
    background-color: var(--warning-bg);
    border-color: var(--warning-border);
    color: var(--warning-dark);
    padding: 12px 16px;
    border-radius: {FormElementStyle.BorderRadius}px;
    border-left: 4px solid var(--warning-color);
}}

.alert-error {{
    background-color: var(--error-bg);
    border-color: var(--error-border);
    color: var(--error-dark);
    padding: 12px 16px;
    border-radius: {FormElementStyle.BorderRadius}px;
    border-left: 4px solid var(--error-color);
}}

.alert-info {{
    background-color: var(--info-bg);
    border-color: var(--info-border);
    color: var(--info-dark);
    padding: 12px 16px;
    border-radius: {FormElementStyle.BorderRadius}px;
    border-left: 4px solid var(--info-color);
}}

/* === Badge Styles === */
.badge {{
    display: inline-block;
    padding: 2px 8px;
    font-size: 12px;
    border-radius: 10px;
    font-weight: 500;
}}
.badge-primary {{
    background-color: var(--primary-bg);
    color: var(--primary-dark);
}}
.badge-success {{
    background-color: var(--success-bg);
    color: var(--success-dark);
}}
.badge-warning {{
    background-color: var(--warning-bg);
    color: var(--warning-dark);
}}
.badge-error {{
    background-color: var(--error-bg);
    color: var(--error-dark);
}}
.badge-info {{
    background-color: var(--info-bg);
    color: var(--info-dark);
}}

/* === Links === */
a {{
    color: var(--primary-color);
    text-decoration: none;
}}
a:hover {{
    color: var(--primary-dark);
    text-decoration: underline;
}}
a:disabled {{
    color: var(--text-disabled);
    cursor: not-allowed;
}}";
    }

    public string ToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public static ColorTheme? FromJson(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<ColorTheme>(json);
        }
        catch { return null; }
    }
}

public static class ContainerStyleExtensions
{
    public static string ToCssString(this ContainerStyle style, string selector)
    {
        var lines = new List<string>
        {
            $".{selector} {{",
            $"    background-color: {style.BackgroundColor};",
            $"    border: {style.BorderWidth}px {style.BorderStyle.ToString().ToLower()} {style.BorderColor};",
            $"    border-radius: {style.BorderRadius}px;",
            $"    padding: {style.Padding.Top}px {style.Padding.Right}px {style.Padding.Bottom}px {style.Padding.Left}px;"
        };
        if (!string.IsNullOrEmpty(style.Width) && style.Width != "auto")
            lines.Add($"    width: {style.Width};");
        if (style.BoxShadow != null)
            lines.Add($"    box-shadow: {style.BoxShadow};");
        if (style.Opacity < 1.0)
            lines.Add($"    opacity: {style.Opacity};");
        lines.Add("}");
        return string.Join("\n", lines);
    }
}
