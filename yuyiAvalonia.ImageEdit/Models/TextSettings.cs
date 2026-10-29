using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;
using System;

namespace yuyiAvalonia.ImageEdit.Models
{
    /// <summary>
    /// 文字属性模型
    /// </summary>
    public partial class TextSettings : ObservableObject
    {
        [ObservableProperty]
        private double _fontSize = 18;

        /// <summary>
        /// 显示用字号（乘以 ZoomLevel，由外部设置）
        /// </summary>
        public double FontSizeDisplay { get; set; } = 18;

        /// <summary>
        /// 通知显示字号已更新
        /// </summary>
        public void NotifyFontSizeDisplayChanged() => OnPropertyChanged(nameof(FontSizeDisplay));

        /// <summary>
        /// 字号变化时通知（用于更新输入框尺寸）
        /// </summary>
        public event Action? FontSizeChanged;

        partial void OnFontSizeChanged(double value)
        {
            FontSizeDisplay = value;
            OnPropertyChanged(nameof(FontSizeDisplay));
            FontSizeChanged?.Invoke();
        }

        [ObservableProperty]
        private Color _fontColor = Colors.Black;

        partial void OnFontColorChanged(Color value)
        {
            FontColorDisplay = new SolidColorBrush(value);
        }

        [ObservableProperty]
        private string _fontFamilyName = "Microsoft YaHei";

        partial void OnFontFamilyNameChanged(string value)
        {
            // 当字体名称改变时，更新 FontFamily 属性
            FontFamily = new FontFamily(value);
        }

        [ObservableProperty]
        private FontFamily _fontFamily = new FontFamily("Microsoft YaHei");

        [ObservableProperty]
        private bool _isBold;

        [ObservableProperty]
        private bool _isItalic;

        [ObservableProperty]
        private bool _isUnderline;

        [ObservableProperty]
        private bool _hasColor = true;

        [ObservableProperty]
        private IBrush _fontColorDisplay = new SolidColorBrush(Colors.Black);
    }

    /// <summary>
    /// 常用字体名称列表
    /// </summary>
    public static class FontNames
    {
        public static readonly string[] Common = new[]
        {
            "Microsoft YaHei",
            "SimSun",
            "SimHei",
            "KaiTi",
            "FangSong",
            "Arial",
            "Arial Black",
            "Times New Roman",
            "Courier New",
            "Verdana",
            "Tahoma",
            "Georgia",
            "Comic Sans MS",
            "Trebuchet MS"
        };
    }
}
