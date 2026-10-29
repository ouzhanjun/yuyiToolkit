using yuyiAvalonia.ColorSelector.Models;
using System.Text;

namespace yuyiAvalonia.ColorSelector.Services;

public static class HtmlPreviewGenerator
{
    public static string GenerateHtmlPreview(ColorTheme theme)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"zh-CN\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>{EscapeHtml(theme.Name)} - 颜色主题预览</title>");
        sb.AppendLine("    <style>");
        
        sb.Append(GenerateCssVariables(theme));
        sb.Append(GenerateBaseStyles());
        sb.Append(GenerateComponentStyles());
        
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        
        sb.AppendLine($"        <h1 class=\"page-title\">{EscapeHtml(theme.Name)}</h1>");
        sb.AppendLine($"        <p class=\"page-subtitle\">{EscapeHtml(theme.Description)}</p>");
        
        sb.Append(GenerateSemanticColorsSection(theme));
        sb.Append(GenerateButtonSection(theme));
        sb.Append(GenerateInputSection(theme));
        sb.Append(GeneratePanelSection(theme));
        sb.Append(GenerateTableSection(theme));
        sb.Append(GenerateNeutralColorsSection(theme));
        sb.Append(GenerateGrayScaleSection(theme));
        
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        
        return sb.ToString();
    }
    
    private static string GenerateCssVariables(ColorTheme theme)
    {
        var sb = new StringBuilder();
        sb.AppendLine("        :root {");
        
        sb.AppendLine($"            --primary-color: {theme.PrimaryColor};");
        sb.AppendLine($"            --primary-light: {theme.PrimaryLight};");
        sb.AppendLine($"            --primary-dark: {theme.PrimaryDark};");
        sb.AppendLine($"            --secondary-color: {theme.SecondaryColor};");
        sb.AppendLine($"            --success-color: {theme.SuccessColor};");
        sb.AppendLine($"            --warning-color: {theme.WarningColor};");
        sb.AppendLine($"            --error-color: {theme.ErrorColor};");
        sb.AppendLine($"            --info-color: {theme.InfoColor};");
        sb.AppendLine($"            --accent-color: {theme.AccentColor};");
        sb.AppendLine();
        sb.AppendLine($"            --background-color: {theme.BackgroundColor};");
        sb.AppendLine($"            --background-secondary: {theme.BackgroundSecondary};");
        sb.AppendLine($"            --background-tertiary: {theme.BackgroundTertiary};");
        sb.AppendLine($"            --foreground-color: {theme.ForegroundColor};");
        sb.AppendLine($"            --foreground-secondary: {theme.ForegroundSecondary};");
        sb.AppendLine($"            --foreground-tertiary: {theme.ForegroundTertiary};");
        sb.AppendLine($"            --border-color: {theme.BorderColor};");
        sb.AppendLine($"            --border-secondary: {theme.BorderSecondary};");
        sb.AppendLine($"            --border-light: {theme.BorderLight};");
        sb.AppendLine($"            --divider-color: {theme.DividerColor};");
        sb.AppendLine($"            --placeholder-color: {theme.PlaceholderColor};");
        sb.AppendLine($"            --disabled-color: {theme.DisabledColor};");
        sb.AppendLine($"            --hover-color: {theme.HoverColor};");
        sb.AppendLine();
        sb.AppendLine($"            --gray-1: {theme.Gray1};");
        sb.AppendLine($"            --gray-2: {theme.Gray2};");
        sb.AppendLine($"            --gray-3: {theme.Gray3};");
        sb.AppendLine($"            --gray-4: {theme.Gray4};");
        sb.AppendLine($"            --gray-5: {theme.Gray5};");
        sb.AppendLine($"            --gray-6: {theme.Gray6};");
        sb.AppendLine($"            --gray-7: {theme.Gray7};");
        sb.AppendLine($"            --gray-8: {theme.Gray8};");
        sb.AppendLine($"            --gray-9: {theme.Gray9};");
        sb.AppendLine($"            --gray-10: {theme.Gray10};");
        sb.AppendLine($"            --gray-11: {theme.Gray11};");
        sb.AppendLine($"            --gray-12: {theme.Gray12};");
        sb.AppendLine($"            --gray-13: {theme.Gray13};");
        sb.AppendLine();
        
        var formStyle = theme.FormElementStyle;
        sb.AppendLine($"            --btn-primary-bg: {formStyle.PrimaryBackgroundColor};");
        sb.AppendLine($"            --btn-primary-fg: {formStyle.PrimaryForegroundColor};");
        sb.AppendLine($"            --btn-primary-hover-bg: {formStyle.PrimaryHoverBackgroundColor};");
        sb.AppendLine($"            --btn-primary-active-bg: {formStyle.PrimaryActiveBackgroundColor};");
        sb.AppendLine($"            --btn-primary-disabled-bg: {formStyle.PrimaryDisabledBackgroundColor};");
        sb.AppendLine($"            --btn-secondary-bg: {formStyle.SecondaryBackgroundColor};");
        sb.AppendLine($"            --btn-secondary-fg: {formStyle.SecondaryForegroundColor};");
        sb.AppendLine($"            --btn-secondary-border: {formStyle.SecondaryBorderColor};");
        sb.AppendLine($"            --input-bg: {formStyle.InputBackgroundColor};");
        sb.AppendLine($"            --input-border: {formStyle.InputBorderColor};");
        sb.AppendLine($"            --input-focus-border: {formStyle.InputFocusBorderColor};");
        
        var pageStyle = theme.PagePanelStyle;
        sb.AppendLine($"            --panel-bg: {pageStyle.PanelBackgroundColor};");
        sb.AppendLine($"            --panel-border: {pageStyle.PanelBorderColor};");
        sb.AppendLine($"            --panel-hover-border: {pageStyle.PanelHoverBorderColor};");
        sb.AppendLine($"            --panel-selected-bg: {pageStyle.PanelSelectedBackgroundColor};");
        sb.AppendLine($"            --tab-selected-fg: {pageStyle.TabSelectedForegroundColor};");
        sb.AppendLine($"            --tab-underline: {pageStyle.TabSelectedUnderlineColor};");
        sb.AppendLine($"            --list-item-hover-bg: {pageStyle.ListItemHoverBackgroundColor};");
        sb.AppendLine($"            --list-item-selected-bg: {pageStyle.ListItemSelectedBackgroundColor};");
        sb.AppendLine($"            --table-header-bg: {pageStyle.TableHeaderBackgroundColor};");
        sb.AppendLine($"            --table-row-hover-bg: {pageStyle.TableRowHoverBackgroundColor};");
        
        sb.AppendLine($"            --border-radius: {formStyle.BorderRadius}px;");
        sb.AppendLine($"            --border-radius-lg: 8px;");
        
        sb.AppendLine("        }");
        return sb.ToString();
    }
    
    private static string GenerateBaseStyles()
    {
        return @"
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: var(--background-tertiary);
            color: var(--foreground-color);
            line-height: 1.6;
        }
        
        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 24px;
        }
        
        .section {
            background: var(--background-color);
            border-radius: var(--border-radius-lg);
            padding: 20px;
            margin-bottom: 24px;
            border: 1px solid var(--border-secondary);
        }
        
        .section-title {
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 8px;
            color: var(--foreground-color);
        }
        
        .section-desc {
            font-size: 12px;
            color: var(--foreground-tertiary);
            margin-bottom: 16px;
        }
        
        .page-title {
            font-size: 28px;
            font-weight: bold;
            margin-bottom: 8px;
        }
        
        .page-subtitle {
            font-size: 14px;
            color: var(--foreground-tertiary);
            margin-bottom: 24px;
        }
        
        .color-grid {
            display: grid;
            grid-template-columns: repeat(6, 1fr);
            gap: 16px;
            margin-top: 16px;
        }
        
        .color-item {
            text-align: center;
        }
        
        .color-swatch {
            height: 40px;
            border-radius: var(--border-radius);
            margin-bottom: 8px;
        }
        
        .color-name {
            font-size: 10px;
            font-weight: 500;
        }
        
        .color-value {
            font-size: 9px;
            color: var(--foreground-tertiary);
        }
        
        .btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 8px 16px;
            border-radius: var(--border-radius);
            font-size: 14px;
            font-weight: 500;
            cursor: pointer;
            border: none;
            transition: all 0.2s;
        }
        
        .btn-primary {
            background: var(--btn-primary-bg);
            color: var(--btn-primary-fg);
        }
        
        .btn-primary:hover {
            background: var(--btn-primary-hover-bg);
        }
        
        .btn-primary:active {
            background: var(--btn-primary-active-bg);
        }
        
        .btn-primary:disabled {
            background: var(--btn-primary-disabled-bg);
            cursor: not-allowed;
        }
        
        .btn-secondary {
            background: var(--btn-secondary-bg);
            color: var(--btn-secondary-fg);
            border: 1px solid var(--btn-secondary-border);
        }
        
        .btn-secondary:hover {
            background: var(--hover-color);
            border-color: var(--primary-color);
        }
        
        .btn-success {
            background: var(--success-color);
            color: white;
        }
        
        .btn-warning {
            background: var(--warning-color);
            color: white;
        }
        
        .btn-error {
            background: var(--error-color);
            color: white;
        }
        
        .btn-group {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
            margin-top: 12px;
        }
        
        .btn-label {
            font-size: 12px;
            font-weight: 500;
            margin-bottom: 8px;
        }
        
        .input {
            padding: 8px 12px;
            border: 1px solid var(--input-border);
            border-radius: var(--border-radius);
            font-size: 14px;
            width: 150px;
            transition: all 0.2s;
        }
        
        .input:focus {
            outline: none;
            border-color: var(--input-focus-border);
        }
        
        .input:disabled {
            background: var(--disabled-color);
            cursor: not-allowed;
        }
        
        .input::placeholder {
            color: var(--placeholder-color);
        }
        
        .checkbox-group {
            display: flex;
            gap: 24px;
            align-items: center;
            margin-top: 12px;
        }
        
        .checkbox-label {
            display: flex;
            align-items: center;
            gap: 8px;
            cursor: pointer;
        }
        
        .checkbox {
            width: 16px;
            height: 16px;
            accent-color: var(--primary-color);
        }
        
        .switch {
            position: relative;
            width: 44px;
            height: 22px;
        }
        
        .switch input {
            opacity: 0;
            width: 0;
            height: 0;
        }
        
        .switch-slider {
            position: absolute;
            cursor: pointer;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: var(--border-color);
            transition: 0.3s;
            border-radius: 22px;
        }
        
        .switch-slider:before {
            position: absolute;
            content: "";
            height: 18px;
            width: 18px;
            left: 2px;
            bottom: 2px;
            background-color: white;
            transition: 0.3s;
            border-radius: 50%;
        }
        
        .switch input:checked + .switch-slider {
            background-color: var(--primary-color);
        }
        
        .switch input:checked + .switch-slider:before {
            transform: translateX(22px);
        }
        
        .card-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 16px;
            margin-top: 12px;
        }
        
        .card {
            background: var(--panel-bg);
            border: 1px solid var(--panel-border);
            border-radius: var(--border-radius-lg);
            padding: 16px;
            transition: all 0.2s;
        }
        
        .card:hover {
            border-color: var(--panel-hover-border);
        }
        
        .card.selected {
            background: var(--panel-selected-bg);
            border-color: var(--primary-color);
        }
        
        .card-title {
            font-weight: 500;
            margin-bottom: 4px;
        }
        
        .card-desc {
            font-size: 12px;
            color: var(--foreground-tertiary);
        }
        
        .tabs {
            display: flex;
            border-bottom: 1px solid var(--border-secondary);
            margin-top: 12px;
        }
        
        .tab {
            padding: 12px 16px;
            cursor: pointer;
            border-bottom: 2px solid transparent;
            transition: all 0.2s;
            color: var(--foreground-secondary);
        }
        
        .tab:hover {
            color: var(--primary-color);
        }
        
        .tab.active {
            color: var(--tab-selected-fg);
            border-bottom-color: var(--tab-underline);
            font-weight: 500;
        }
        
        .list {
            background: white;
            border: 1px solid var(--border-secondary);
            border-radius: var(--border-radius);
            overflow: hidden;
            margin-top: 12px;
        }
        
        .list-item {
            padding: 12px 16px;
            border-bottom: 1px solid var(--border-secondary);
            transition: all 0.2s;
        }
        
        .list-item:last-child {
            border-bottom: none;
        }
        
        .list-item:hover {
            background: var(--list-item-hover-bg);
        }
        
        .list-item.selected {
            background: var(--list-item-selected-bg);
            color: var(--primary-color);
        }
        
        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 12px;
            background: white;
            border: 1px solid var(--border-secondary);
            border-radius: var(--border-radius);
            overflow: hidden;
        }
        
        .table th {
            background: var(--table-header-bg);
            padding: 12px 16px;
            text-align: left;
            font-weight: 600;
            border-bottom: 1px solid var(--border-secondary);
        }
        
        .table td {
            padding: 12px 16px;
            border-bottom: 1px solid var(--border-secondary);
        }
        
        .table tr:last-child td {
            border-bottom: none;
        }
        
        .table tr:hover td {
            background: var(--table-row-hover-bg);
        }
        
        .gray-scale {
            display: flex;
            gap: 2px;
            margin-top: 12px;
        }
        
        .gray-item {
            width: 50px;
            height: 40px;
            display: flex;
            align-items: flex-end;
            justify-content: center;
            padding-bottom: 4px;
        }
        
        .gray-item:first-child {
            border-radius: var(--border-radius) 0 0 var(--border-radius);
        }
        
        .gray-item:last-child {
            border-radius: 0 var(--border-radius) var(--border-radius) 0;
        }
        
        .gray-label {
            font-size: 8px;
            color: white;
            text-shadow: 0 1px 2px rgba(0,0,0,0.5);
        }
        
        .code-block {
            background: #1E1E1E;
            border-radius: var(--border-radius);
            padding: 12px;
            margin-top: 16px;
            overflow-x: auto;
        }
        
        .code {
            font-family: 'Consolas', 'Monaco', monospace;
            font-size: 11px;
            color: #D4D4D4;
            white-space: pre-wrap;
            line-height: 1.5;
        }
";
    }
    
    private static string GenerateComponentStyles()
    {
        return @"";
    }
    
    private static string GenerateSemanticColorsSection(ColorTheme theme)
    {
        return $@"
        <!-- 基础颜色 -->
        <div class=""section"">
            <h2 class=""section-title"">基础颜色 (Semantic Colors)</h2>
            <p class=""section-desc"">用于定义应用的核心语义色彩。</p>
            
            <div class=""color-grid"">
                <div class=""color-item"">
                    <div class=""color-swatch"" style=""background: {theme.PrimaryColor};""></div>
                    <div class=""color-name"">PrimaryColor</div>
                    <div class=""color-value"">{theme.PrimaryColor}</div>
                </div>
                <div class=""color-item"">
                    <div class=""color-swatch"" style=""background: {theme.SecondaryColor};""></div>
                    <div class=""color-name"">SecondaryColor</div>
                    <div class=""color-value"">{theme.SecondaryColor}</div>
                </div>
                <div class=""color-item"">
                    <div class=""color-swatch"" style=""background: {theme.SuccessColor};""></div>
                    <div class=""color-name"">SuccessColor</div>
                    <div class=""color-value"">{theme.SuccessColor}</div>
                </div>
                <div class=""color-item"">
                    <div class=""color-swatch"" style=""background: {theme.WarningColor};""></div>
                    <div class=""color-name"">WarningColor</div>
                    <div class=""color-value"">{theme.WarningColor}</div>
                </div>
                <div class=""color-item"">
                    <div class=""color-swatch"" style=""background: {theme.ErrorColor};""></div>
                    <div class=""color-name"">ErrorColor</div>
                    <div class=""color-value"">{theme.ErrorColor}</div>
                </div>
                <div class=""color-item"">
                    <div class=""color-swatch"" style=""background: {theme.InfoColor};""></div>
                    <div class=""color-name"">InfoColor</div>
                    <div class=""color-value"">{theme.InfoColor}</div>
                </div>
            </div>
        </div>
";
    }
    
    private static string GenerateButtonSection(ColorTheme theme)
    {
        return $@"
        <!-- 按钮样式 -->
        <div class=""section"">
            <h2 class=""section-title"">按钮样式 (Buttons)</h2>
            <p class=""section-desc"">支持主按钮、次要按钮、语义按钮等多种类型。</p>
            
            <div class=""btn-label"">主按钮 (Primary Button)</div>
            <div class=""btn-group"">
                <button class=""btn btn-primary"">默认状态</button>
                <button class=""btn btn-primary"" style=""background: {theme.FormElementStyle.PrimaryHoverBackgroundColor};"">悬停状态</button>
                <button class=""btn btn-primary"" style=""background: {theme.FormElementStyle.PrimaryActiveBackgroundColor};"">按下状态</button>
                <button class=""btn btn-primary"" disabled>禁用状态</button>
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">次要按钮 (Secondary Button)</div>
            <div class=""btn-group"">
                <button class=""btn btn-secondary"">默认状态</button>
                <button class=""btn btn-secondary"" style=""background: {theme.HoverColor}; border-color: {theme.PrimaryColor};"">悬停状态</button>
                <button class=""btn btn-secondary"" disabled>禁用状态</button>
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">语义按钮 (Semantic Buttons)</div>
            <div class=""btn-group"">
                <button class=""btn btn-primary"">主要</button>
                <button class=""btn btn-secondary"">次要</button>
                <button class=""btn btn-success"">成功</button>
                <button class=""btn btn-warning"">警告</button>
                <button class=""btn btn-error"">错误</button>
            </div>
        </div>
";
    }
    
    private static string GenerateInputSection(ColorTheme theme)
    {
        return $@"
        <!-- 输入框样式 -->
        <div class=""section"">
            <h2 class=""section-title"">输入框样式 (Inputs)</h2>
            <p class=""section-desc"">包含文本输入框、复选框、开关等表单元素。</p>
            
            <div class=""btn-label"">文本输入框</div>
            <div class=""btn-group"">
                <input type=""text"" class=""input"" value=""默认状态"" />
                <input type=""text"" class=""input"" value=""聚焦状态"" style=""border-color: {theme.FormElementStyle.InputFocusBorderColor};"" />
                <input type=""text"" class=""input"" value=""禁用状态"" disabled />
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">复选框 &amp; 开关</div>
            <div class=""checkbox-group"">
                <label class=""checkbox-label"">
                    <input type=""checkbox"" class=""checkbox"" checked />
                    复选框
                </label>
                <label class=""checkbox-label"">
                    <input type=""radio"" class=""checkbox"" name=""radio"" checked />
                    单选框
                </label>
                <label class=""switch"">
                    <input type=""checkbox"" checked />
                    <span class=""switch-slider""></span>
                </label>
            </div>
        </div>
";
    }
    
    private static string GeneratePanelSection(ColorTheme theme)
    {
        return $@"
        <!-- 面板样式 -->
        <div class=""section"">
            <h2 class=""section-title"">面板样式 (Panels)</h2>
            <p class=""section-desc"">用于页面容器、卡片、标签页、列表等容器元素。</p>
            
            <div class=""btn-label"">卡片面板</div>
            <div class=""card-grid"">
                <div class=""card"">
                    <div class=""card-title"">默认面板</div>
                    <div class=""card-desc"">这是面板内容示例</div>
                </div>
                <div class=""card"" style=""border-color: {theme.PagePanelStyle.PanelHoverBorderColor};"">
                    <div class=""card-title"">悬停面板</div>
                    <div class=""card-desc"">边框变为主色</div>
                </div>
                <div class=""card selected"">
                    <div class=""card-title"" style=""color: {theme.PrimaryColor};"">选中面板</div>
                    <div class=""card-desc"" style=""color: {theme.PrimaryColor};"">背景和边框都变为主色</div>
                </div>
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">标签页</div>
            <div class=""tabs"">
                <div class=""tab active"">选中标签</div>
                <div class=""tab"">未选中标签</div>
                <div class=""tab"">禁用标签</div>
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">列表项</div>
            <div class=""list"">
                <div class=""list-item"">默认列表项</div>
                <div class=""list-item"" style=""background: {theme.PagePanelStyle.ListItemHoverBackgroundColor};"">悬停列表项</div>
                <div class=""list-item selected"">选中列表项</div>
            </div>
        </div>
";
    }
    
    private static string GenerateTableSection(ColorTheme theme)
    {
        return $@"
        <!-- 表格样式 -->
        <div class=""section"">
            <h2 class=""section-title"">表格样式 (Table)</h2>
            <p class=""section-desc"">用于数据表格的表头、单元格、行状态等样式定义。</p>
            
            <table class=""table"">
                <thead>
                    <tr>
                        <th>姓名</th>
                        <th>年龄</th>
                        <th>城市</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>张三</td>
                        <td>25</td>
                        <td>北京</td>
                    </tr>
                    <tr style=""background: {theme.PagePanelStyle.TableRowHoverBackgroundColor};"">
                        <td>李四</td>
                        <td>30</td>
                        <td>上海</td>
                    </tr>
                    <tr style=""background: {theme.PagePanelStyle.TableRowSelectedBackgroundColor};"">
                        <td style=""font-weight: 600;"">王五</td>
                        <td style=""font-weight: 600;"">28</td>
                        <td style=""font-weight: 600;"">广州</td>
                    </tr>
                </tbody>
            </table>
        </div>
";
    }
    
    private static string GenerateNeutralColorsSection(ColorTheme theme)
    {
        return $@"
        <!-- 中性色 -->
        <div class=""section"">
            <h2 class=""section-title"">中性色 (Neutral Colors)</h2>
            <p class=""section-desc"">用于背景、前景、边框等基础元素。</p>
            
            <div class=""btn-label"">背景色</div>
            <div style=""display: flex; gap: 4px; margin-top: 8px;"">
                <div style=""width: 60px; height: 30px; border-radius: 4px; background: {theme.BackgroundColor}; border: 1px solid {theme.BorderSecondary};""></div>
                <div style=""width: 60px; height: 30px; border-radius: 4px; background: {theme.BackgroundSecondary};""></div>
                <div style=""width: 60px; height: 30px; border-radius: 4px; background: {theme.BackgroundTertiary};""></div>
                <div style=""width: 60px; height: 30px; border-radius: 4px; background: {theme.Gray13};""></div>
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">前景色</div>
            <div style=""display: flex; gap: 16px; margin-top: 8px;"">
                <span style=""color: {theme.ForegroundColor};"">主要文字</span>
                <span style=""color: {theme.ForegroundSecondary};"">次要文字</span>
                <span style=""color: {theme.ForegroundTertiary};"">辅助文字</span>
                <span style=""color: {theme.PlaceholderColor};"">禁用文字</span>
            </div>
            
            <div class=""btn-label"" style=""margin-top: 16px;"">边框色</div>
            <div style=""display: flex; gap: 4px; margin-top: 8px;"">
                <div style=""width: 60px; height: 20px; border-radius: 4px; background: {theme.BorderColor};""></div>
                <div style=""width: 60px; height: 20px; border-radius: 4px; background: {theme.BorderSecondary};""></div>
                <div style=""width: 60px; height: 20px; border-radius: 4px; background: {theme.BorderLight};""></div>
                <div style=""width: 60px; height: 20px; border-radius: 4px; background: {theme.Gray9};""></div>
            </div>
        </div>
";
    }
    
    private static string GenerateGrayScaleSection(ColorTheme theme)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"
        <!-- 灰度色板 -->
        <div class=""section"">
            <h2 class=""section-title"">灰度色板 (Gray Scale)</h2>
            <p class=""section-desc"">从白到黑的13级灰度色板。</p>
            
            <div class=""gray-scale"">");
        
        for (int i = 1; i <= 13; i++)
        {
            var grayProp = typeof(ColorTheme).GetProperty($"Gray{i}");
            var grayValue = grayProp?.GetValue(theme)?.ToString() ?? "#FFFFFF";
            var textColor = i <= 6 ? $"color: {theme.Gray13}; text-shadow: none;" : "";
            sb.AppendLine($"                <div class=\"gray-item\" style=\"background: {grayValue};{(i == 1 ? $" border: 1px solid {theme.BorderSecondary};" : "")}\"><span class=\"gray-label\" style=\"{textColor}\">Gray{i}</span></div>");
        }
        
        sb.AppendLine(@"            </div>
        </div>
");
        return sb.ToString();
    }
    
    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
