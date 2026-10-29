using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 页面面板样式 - 用于定义页面、面板、卡片、标签页等容器元素的样式
/// </summary>
public partial class PagePanelStyle : ObservableObject
{
    #region 基础属性

    /// <summary>
    /// 圆角
    /// </summary>
    [ObservableProperty] private double _borderRadius = 8;

    /// <summary>
    /// 边框宽度
    /// </summary>
    [ObservableProperty] private double _borderWidth = 1;

    /// <summary>
    /// 内边距
    /// </summary>
    [ObservableProperty] private double _padding = 16;

    /// <summary>
    /// 阴影颜色
    /// </summary>
    [ObservableProperty] private string _shadowColor = "#00000010";

    /// <summary>
    /// 阴影模糊半径
    /// </summary>
    [ObservableProperty] private double _shadowBlur = 10;

    /// <summary>
    /// 阴影偏移X
    /// </summary>
    [ObservableProperty] private double _shadowOffsetX = 0;

    /// <summary>
    /// 阴影偏移Y
    /// </summary>
    [ObservableProperty] private double _shadowOffsetY = 2;

    #endregion

    #region 页面容器样式

    /// <summary>
    /// 页面-背景色
    /// </summary>
    [ObservableProperty] private string _pageBackgroundColor = "#F0F2F5";

    /// <summary>
    /// 页面-前景色
    /// </summary>
    [ObservableProperty] private string _pageForegroundColor = "#333333";

    /// <summary>
    /// 页面-边框颜色
    /// </summary>
    [ObservableProperty] private string _pageBorderColor = "#E8E8E8";

    #endregion

    #region 面板/卡片样式

    /// <summary>
    /// 面板-默认背景色
    /// </summary>
    [ObservableProperty] private string _panelBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 面板-默认前景色
    /// </summary>
    [ObservableProperty] private string _panelForegroundColor = "#333333";

    /// <summary>
    /// 面板-默认边框颜色
    /// </summary>
    [ObservableProperty] private string _panelBorderColor = "#E8E8E8";

    /// <summary>
    /// 面板-悬停背景色
    /// </summary>
    [ObservableProperty] private string _panelHoverBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 面板-悬停边框颜色
    /// </summary>
    [ObservableProperty] private string _panelHoverBorderColor = "#40A9FF";

    /// <summary>
    /// 面板-选中背景色
    /// </summary>
    [ObservableProperty] private string _panelSelectedBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 面板-选中边框颜色
    /// </summary>
    [ObservableProperty] private string _panelSelectedBorderColor = "#1890FF";

    #endregion

    #region 分割线样式

    /// <summary>
    /// 分割线-默认颜色
    /// </summary>
    [ObservableProperty] private string _dividerDefaultColor = "#E8E8E8";

    /// <summary>
    /// 分割线-禁用/淡化颜色
    /// </summary>
    [ObservableProperty] private string _dividerDisabledColor = "#F0F0F0";

    /// <summary>
    /// 分割线-垂直颜色
    /// </summary>
    [ObservableProperty] private string _dividerVerticalColor = "#E8E8E8";

    #endregion

    #region 标签页样式

    /// <summary>
    /// 标签页-默认背景色
    /// </summary>
    [ObservableProperty] private string _tabBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 标签页-默认前景色
    /// </summary>
    [ObservableProperty] private string _tabForegroundColor = "#666666";

    /// <summary>
    /// 标签页-悬停背景色
    /// </summary>
    [ObservableProperty] private string _tabHoverBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 标签页-悬停前景色
    /// </summary>
    [ObservableProperty] private string _tabHoverForegroundColor = "#333333";

    /// <summary>
    /// 标签页-选中背景色
    /// </summary>
    [ObservableProperty] private string _tabSelectedBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 标签页-选中前景色
    /// </summary>
    [ObservableProperty] private string _tabSelectedForegroundColor = "#1890FF";

    /// <summary>
    /// 标签页-选中下划线颜色
    /// </summary>
    [ObservableProperty] private string _tabSelectedUnderlineColor = "#1890FF";

    /// <summary>
    /// 标签页-禁用前景色
    /// </summary>
    [ObservableProperty] private string _tabDisabledForegroundColor = "#BFBFBF";

    #endregion

    #region 工具栏样式

    /// <summary>
    /// 工具栏-背景色
    /// </summary>
    [ObservableProperty] private string _toolbarBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 工具栏-前景色
    /// </summary>
    [ObservableProperty] private string _toolbarForegroundColor = "#333333";

    /// <summary>
    /// 工具栏-边框颜色
    /// </summary>
    [ObservableProperty] private string _toolbarBorderColor = "#E8E8E8";

    /// <summary>
    /// 工具栏-悬停项背景色
    /// </summary>
    [ObservableProperty] private string _toolbarItemHoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 工具栏-选中项背景色
    /// </summary>
    [ObservableProperty] private string _toolbarItemSelectedBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 工具栏-选中项前景色
    /// </summary>
    [ObservableProperty] private string _toolbarItemSelectedForegroundColor = "#1890FF";

    #endregion

    #region 侧边栏样式

    /// <summary>
    /// 侧边栏-背景色
    /// </summary>
    [ObservableProperty] private string _sidebarBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 侧边栏-前景色
    /// </summary>
    [ObservableProperty] private string _sidebarForegroundColor = "#333333";

    /// <summary>
    /// 侧边栏-边框颜色
    /// </summary>
    [ObservableProperty] private string _sidebarBorderColor = "#E8E8E8";

    /// <summary>
    /// 侧边栏-项目悬停背景色
    /// </summary>
    [ObservableProperty] private string _sidebarItemHoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 侧边栏-项目选中背景色
    /// </summary>
    [ObservableProperty] private string _sidebarItemSelectedBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 侧边栏-项目选中前景色
    /// </summary>
    [ObservableProperty] private string _sidebarItemSelectedForegroundColor = "#1890FF";

    /// <summary>
    /// 侧边栏-项目禁用前景色
    /// </summary>
    [ObservableProperty] private string _sidebarItemDisabledForegroundColor = "#BFBFBF";

    #endregion

    #region 列表项样式

    /// <summary>
    /// 列表项-默认背景色
    /// </summary>
    [ObservableProperty] private string _listItemBackgroundColor = "transparent";

    /// <summary>
    /// 列表项-默认前景色
    /// </summary>
    [ObservableProperty] private string _listItemForegroundColor = "#333333";

    /// <summary>
    /// 列表项-悬停背景色
    /// </summary>
    [ObservableProperty] private string _listItemHoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 列表项-选中背景色
    /// </summary>
    [ObservableProperty] private string _listItemSelectedBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 列表项-选中前景色
    /// </summary>
    [ObservableProperty] private string _listItemSelectedForegroundColor = "#1890FF";

    /// <summary>
    /// 列表项-激活/按下背景色
    /// </summary>
    [ObservableProperty] private string _listItemActiveBackgroundColor = "#E6F7FF";

    /// <summary>
    /// 列表项-激活/按下前景色
    /// </summary>
    [ObservableProperty] private string _listItemActiveForegroundColor = "#1890FF";

    /// <summary>
    /// 列表项-禁用前景色
    /// </summary>
    [ObservableProperty] private string _listItemDisabledForegroundColor = "#BFBFBF";

    #endregion

    #region 表格样式

    /// <summary>
    /// 表格-表头背景色
    /// </summary>
    [ObservableProperty] private string _tableHeaderBackgroundColor = "#FAFAFA";

    /// <summary>
    /// 表格-表头前景色
    /// </summary>
    [ObservableProperty] private string _tableHeaderForegroundColor = "#333333";

    /// <summary>
    /// 表格-表头边框颜色
    /// </summary>
    [ObservableProperty] private string _tableHeaderBorderColor = "#E8E8E8";

    /// <summary>
    /// 表格-单元格背景色
    /// </summary>
    [ObservableProperty] private string _tableCellBackgroundColor = "#FFFFFF";

    /// <summary>
    /// 表格-单元格前景色
    /// </summary>
    [ObservableProperty] private string _tableCellForegroundColor = "#333333";

    /// <summary>
    /// 表格-单元格边框颜色
    /// </summary>
    [ObservableProperty] private string _tableCellBorderColor = "#E8E8E8";

    /// <summary>
    /// 表格-行悬停背景色
    /// </summary>
    [ObservableProperty] private string _tableRowHoverBackgroundColor = "#F5F5F5";

    /// <summary>
    /// 表格-行选中背景色
    /// </summary>
    [ObservableProperty] private string _tableRowSelectedBackgroundColor = "#E6F7FF";

    #endregion

    /// <summary>
    /// 克隆样式
    /// </summary>
    public PagePanelStyle Clone()
    {
        return new PagePanelStyle
        {
            BorderRadius = BorderRadius,
            BorderWidth = BorderWidth,
            Padding = Padding,
            ShadowColor = ShadowColor,
            ShadowBlur = ShadowBlur,
            ShadowOffsetX = ShadowOffsetX,
            ShadowOffsetY = ShadowOffsetY,
            PageBackgroundColor = PageBackgroundColor,
            PageForegroundColor = PageForegroundColor,
            PageBorderColor = PageBorderColor,
            PanelBackgroundColor = PanelBackgroundColor,
            PanelForegroundColor = PanelForegroundColor,
            PanelBorderColor = PanelBorderColor,
            PanelHoverBackgroundColor = PanelHoverBackgroundColor,
            PanelHoverBorderColor = PanelHoverBorderColor,
            PanelSelectedBackgroundColor = PanelSelectedBackgroundColor,
            PanelSelectedBorderColor = PanelSelectedBorderColor,
            DividerDefaultColor = DividerDefaultColor,
            DividerDisabledColor = DividerDisabledColor,
            DividerVerticalColor = DividerVerticalColor,
            TabBackgroundColor = TabBackgroundColor,
            TabForegroundColor = TabForegroundColor,
            TabHoverBackgroundColor = TabHoverBackgroundColor,
            TabHoverForegroundColor = TabHoverForegroundColor,
            TabSelectedBackgroundColor = TabSelectedBackgroundColor,
            TabSelectedForegroundColor = TabSelectedForegroundColor,
            TabSelectedUnderlineColor = TabSelectedUnderlineColor,
            TabDisabledForegroundColor = TabDisabledForegroundColor,
            ToolbarBackgroundColor = ToolbarBackgroundColor,
            ToolbarForegroundColor = ToolbarForegroundColor,
            ToolbarBorderColor = ToolbarBorderColor,
            ToolbarItemHoverBackgroundColor = ToolbarItemHoverBackgroundColor,
            ToolbarItemSelectedBackgroundColor = ToolbarItemSelectedBackgroundColor,
            ToolbarItemSelectedForegroundColor = ToolbarItemSelectedForegroundColor,
            SidebarBackgroundColor = SidebarBackgroundColor,
            SidebarForegroundColor = SidebarForegroundColor,
            SidebarBorderColor = SidebarBorderColor,
            SidebarItemHoverBackgroundColor = SidebarItemHoverBackgroundColor,
            SidebarItemSelectedBackgroundColor = SidebarItemSelectedBackgroundColor,
            SidebarItemSelectedForegroundColor = SidebarItemSelectedForegroundColor,
            SidebarItemDisabledForegroundColor = SidebarItemDisabledForegroundColor,
            ListItemBackgroundColor = ListItemBackgroundColor,
            ListItemForegroundColor = ListItemForegroundColor,
            ListItemHoverBackgroundColor = ListItemHoverBackgroundColor,
            ListItemSelectedBackgroundColor = ListItemSelectedBackgroundColor,
            ListItemSelectedForegroundColor = ListItemSelectedForegroundColor,
            ListItemActiveBackgroundColor = ListItemActiveBackgroundColor,
            ListItemActiveForegroundColor = ListItemActiveForegroundColor,
            ListItemDisabledForegroundColor = ListItemDisabledForegroundColor,
            TableHeaderBackgroundColor = TableHeaderBackgroundColor,
            TableHeaderForegroundColor = TableHeaderForegroundColor,
            TableHeaderBorderColor = TableHeaderBorderColor,
            TableCellBackgroundColor = TableCellBackgroundColor,
            TableCellForegroundColor = TableCellForegroundColor,
            TableCellBorderColor = TableCellBorderColor,
            TableRowHoverBackgroundColor = TableRowHoverBackgroundColor,
            TableRowSelectedBackgroundColor = TableRowSelectedBackgroundColor
        };
    }

    /// <summary>
    /// 生成面板 CSS
    /// </summary>
    public string ToPanelCss(string selector)
    {
        var shadow = ShadowOffsetX != 0 || ShadowOffsetY != 0
            ? $"box-shadow: {ShadowOffsetX}px {ShadowOffsetY}px {ShadowBlur}px {ShadowColor};"
            : "";

        return $@"{selector} {{
    background-color: {PanelBackgroundColor};
    color: {PanelForegroundColor};
    border: {BorderWidth}px solid {PanelBorderColor};
    border-radius: {BorderRadius}px;
    padding: {Padding}px;
    {(string.IsNullOrEmpty(shadow) ? "" : shadow)}
}}
{selector}:hover {{
    border-color: {PanelHoverBorderColor};
}}";
    }
}
