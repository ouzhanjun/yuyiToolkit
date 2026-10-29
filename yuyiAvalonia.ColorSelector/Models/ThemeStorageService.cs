using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;
using yuyiAvalonia.Base.Core;

namespace yuyiAvalonia.ColorSelector.Models;

/// <summary>
/// 主题存储服务
/// </summary>
public class ThemeStorageService
{
    private readonly string _storagePath;
    private readonly string _userThemesFolder;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ThemeStorageService()
    {
        // 获取插件存储路径
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        _storagePath = Path.Combine(basePath, "themes");
        _userThemesFolder = Path.Combine(_storagePath, "user");

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        if (!Directory.Exists(_storagePath))
            Directory.CreateDirectory(_storagePath);
        if (!Directory.Exists(_userThemesFolder))
            Directory.CreateDirectory(_userThemesFolder);
    }

    /// <summary>
    /// 保存主题到文件
    /// </summary>
    public async Task SaveThemeAsync(ColorTheme theme)
    {
        var fileName = SanitizeFileName(theme.Name) + "_" + theme.Id + ".json";
        var filePath = Path.Combine(_userThemesFolder, fileName);

        var json = JsonSerializer.Serialize(theme, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 加载所有用户主题
    /// </summary>
    public async Task<List<ColorTheme>> LoadUserThemesAsync()
    {
        var themes = new List<ColorTheme>();

        if (!Directory.Exists(_userThemesFolder))
            return themes;

        var files = Directory.GetFiles(_userThemesFolder, "*.json");
        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var theme = JsonSerializer.Deserialize<ColorTheme>(json, _jsonOptions);
                if (theme != null)
                {
                    theme.IsBuiltIn = false;
                    themes.Add(theme);
                }
            }
            catch
            {
                // 忽略无效文件
            }
        }

        return themes;
    }

    /// <summary>
    /// 删除主题文件
    /// </summary>
    public async Task DeleteThemeAsync(ColorTheme theme)
    {
        if (theme.IsBuiltIn)
            return;

        var files = Directory.GetFiles(_userThemesFolder, $"*_{theme.Id}.json");
        foreach (var file in files)
        {
            await Task.Run(() => File.Delete(file));
        }
    }

    /// <summary>
    /// 导出主题为JSON
    /// </summary>
    public async Task ExportThemeAsync(ColorTheme theme, string filePath)
    {
        var json = JsonSerializer.Serialize(theme, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 从JSON导入主题
    /// </summary>
    public async Task<ColorTheme?> ImportThemeAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var theme = JsonSerializer.Deserialize<ColorTheme>(json, _jsonOptions);
            if (theme != null)
            {
                theme.Id = Guid.NewGuid().ToString();
                theme.IsBuiltIn = false;
                theme.CreatedAt = DateTime.Now;
                theme.UpdatedAt = DateTime.Now;
            }
            return theme;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 导出主题为CSS文件
    /// </summary>
    public async Task ExportThemeToCssAsync(ColorTheme theme, string filePath)
    {
        var css = theme.ToFullCss();
        await File.WriteAllTextAsync(filePath, css);
    }

    /// <summary>
    /// 导出主题为SCSS变量文件
    /// </summary>
    public async Task ExportThemeToScssAsync(ColorTheme theme, string filePath)
    {
        var scss = theme.ToScssVariables();
        await File.WriteAllTextAsync(filePath, scss);
    }

    /// <summary>
    /// 导出主题为微信小程序配置
    /// </summary>
    public async Task ExportThemeToWxAppAsync(ColorTheme theme, string folderPath)
    {
        EnsureDirectoriesExist(folderPath);

        var appJson = theme.ToWxAppJson();
        var indexPath = Path.Combine(folderPath, "app.json");
        var wxssPath = Path.Combine(folderPath, "app.wxss");

        await File.WriteAllTextAsync(indexPath, appJson);
        await File.WriteAllTextAsync(wxssPath, theme.ToWxss());
    }

    private void EnsureDirectoriesExist(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
    }

    private string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
    }
}

/// <summary>
/// 主题导出格式
/// </summary>
public enum ExportFormat
{
    Json,
    Css
}

/// <summary>
/// 导出格式项（用于下拉框显示）
/// </summary>
public class ExportFormatItem
{
    public ExportFormat Format { get; set; }
    public string DisplayName { get; set; } = "";

    public override string ToString() => DisplayName;
}

/// <summary>
/// ColorTheme 扩展方法
/// </summary>
public static class ColorThemeExtensions
{
    /// <summary>
    /// 生成SCSS变量
    /// </summary>
    public static string ToScssVariables(this ColorTheme theme)
    {
        return $@"// Theme: {theme.Name}
// Generated by ColorSelector

// Color Palette
$primary-color: {theme.PrimaryColor};
$secondary-color: {theme.SecondaryColor};
$accent-color: {theme.AccentColor};
$success-color: {theme.SuccessColor};
$warning-color: {theme.WarningColor};
$error-color: {theme.ErrorColor};
$info-color: {theme.InfoColor};

// Background & Foreground
$background-color: {theme.BackgroundColor};
$foreground-color: {theme.ForegroundColor};

// Border & Divider
$border-color: {theme.BorderColor};
$divider-color: {theme.DividerColor};

// State Colors
$placeholder-color: {theme.PlaceholderColor};
$disabled-color: {theme.DisabledColor};
$hover-color: {theme.HoverColor};
$selected-color: {theme.SelectedColor};
";
    }

    /// <summary>
    /// 生成微信小程序 app.json 片段
    /// </summary>
    public static string ToWxAppJson(this ColorTheme theme)
    {
        return $@"{{
  ""usingComponents"": {{}},
  ""window"": {{
    ""navigationBarBackgroundColor"": ""{theme.PrimaryColor}"",
    ""navigationBarTextStyle"": ""white"",
    ""backgroundColor"": ""{theme.BackgroundColor}"",
    ""backgroundTextStyle"": ""dark""
  }}
}}";
    }

    /// <summary>
    /// 生成微信小程序 WXSS 变量
    /// </summary>
    public static string ToWxss(this ColorTheme theme)
    {
        return $@"/* Theme: {theme.Name} */
/* Generated by ColorSelector */

page {{
  --primary-color: {theme.PrimaryColor};
  --secondary-color: {theme.SecondaryColor};
  --accent-color: {theme.AccentColor};
  --success-color: {theme.SuccessColor};
  --warning-color: {theme.WarningColor};
  --error-color: {theme.ErrorColor};
  --info-color: {theme.InfoColor};
  --background-color: {theme.BackgroundColor};
  --foreground-color: {theme.ForegroundColor};
  --border-color: {theme.BorderColor};
  --divider-color: {theme.DividerColor};
  --placeholder-color: {theme.PlaceholderColor};
  --disabled-color: {theme.DisabledColor};
  --hover-color: {theme.HoverColor};
  --selected-color: {theme.SelectedColor};
}}";
    }

    /// <summary>
    /// 生成 React Native 样式表
    /// </summary>
    public static string ToReactNativeStyles(this ColorTheme theme)
    {
        return $@"// Theme: {theme.Name}
// Generated by ColorSelector
// For React Native projects

export const colors = {{
  primary: '{theme.PrimaryColor}',
  secondary: '{theme.SecondaryColor}',
  accent: '{theme.AccentColor}',
  success: '{theme.SuccessColor}',
  warning: '{theme.WarningColor}',
  error: '{theme.ErrorColor}',
  info: '{theme.InfoColor}',
  background: '{theme.BackgroundColor}',
  foreground: '{theme.ForegroundColor}',
  border: '{theme.BorderColor}',
  divider: '{theme.DividerColor}',
  placeholder: '{theme.PlaceholderColor}',
  disabled: '{theme.DisabledColor}',
  hover: '{theme.HoverColor}',
  selected: '{theme.SelectedColor}',
}};

export const spacing = {{
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
}};

export const typography = {{
  h1: {{ fontSize: 24, fontWeight: 'bold', color: colors.foreground }},
  h2: {{ fontSize: 20, fontWeight: '600', color: colors.foreground }},
  h3: {{ fontSize: 18, fontWeight: '600', color: colors.foreground }},
  body: {{ fontSize: 14, color: colors.foreground, lineHeight: 22 }},
  caption: {{ fontSize: 12, color: colors.placeholder }},
}};
";
    }

    /// <summary>
    /// 生成 Flutter 主题数据
    /// </summary>
    public static string ToFlutterTheme(this ColorTheme theme)
    {
        return $@"// Theme: {theme.Name}
// Generated by ColorSelector
// For Flutter projects

import 'package:flutter/material.dart';

class AppTheme {{
  static const Color primaryColor = Color(0x{theme.PrimaryColor.Replace("#", "")});
  static const Color secondaryColor = Color(0x{theme.SecondaryColor.Replace("#", "")});
  static const Color accentColor = Color(0x{theme.AccentColor.Replace("#", "")});
  static const Color successColor = Color(0x{theme.SuccessColor.Replace("#", "")});
  static const Color warningColor = Color(0x{theme.WarningColor.Replace("#", "")});
  static const Color errorColor = Color(0x{theme.ErrorColor.Replace("#", "")});
  static const Color infoColor = Color(0x{theme.InfoColor.Replace("#", "")});
  static const Color backgroundColor = Color(0x{theme.BackgroundColor.Replace("#", "")});
  static const Color foregroundColor = Color(0x{theme.ForegroundColor.Replace("#", "")});
  static const Color borderColor = Color(0x{theme.BorderColor.Replace("#", "")});
  static const Color dividerColor = Color(0x{theme.DividerColor.Replace("#", "")});

  static ThemeData get theme => ThemeData(
    primaryColor: primaryColor,
    scaffoldBackgroundColor: backgroundColor,
    colorScheme: ColorScheme.light(
      primary: primaryColor,
      secondary: secondaryColor,
      surface: backgroundColor,
      error: errorColor,
    ),
    appBarTheme: AppBarTheme(
      backgroundColor: primaryColor,
      foregroundColor: Colors.white,
    ),
  );
}}
";
    }

    /// <summary>
    /// 生成 Tailwind CSS 配置
    /// </summary>
    public static string ToTailwindConfig(this ColorTheme theme)
    {
        return $@"// Theme: {theme.Name}
// Generated by ColorSelector
// For Tailwind CSS projects

module.exports = {{
  theme: {{
    extend: {{
      colors: {{
        primary: '{theme.PrimaryColor}',
        secondary: '{theme.SecondaryColor}',
        accent: '{theme.AccentColor}',
        success: '{theme.SuccessColor}',
        warning: '{theme.WarningColor}',
        error: '{theme.ErrorColor}',
        info: '{theme.InfoColor}',
        background: '{theme.BackgroundColor}',
        foreground: '{theme.ForegroundColor}',
        border: '{theme.BorderColor}',
        divider: '{theme.DividerColor}',
        placeholder: '{theme.PlaceholderColor}',
        disabled: '{theme.DisabledColor}',
        hover: '{theme.HoverColor}',
        selected: '{theme.SelectedColor}',
      }},
    }},
  }},
}}
";
    }
}
