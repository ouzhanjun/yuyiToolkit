using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;

namespace yuyiAvalonia.ImageEdit.Models
{
    /// <summary>
    /// 画笔类型枚举
    /// </summary>
    public enum BrushType
    {
        Oil,           // 油画笔
        Marker,        // 记号笔
        Crayon,        // 蜡笔
        Watercolor,    // 水彩笔
        Pen,           // 钢笔
        Pencil,        // 铅笔
        Spray,         // 喷雾罐
        Airbrush       // 气笔
    }

    /// <summary>
    /// 画笔属性模型
    /// </summary>
    public partial class BrushSettings : ObservableObject
    {
        [ObservableProperty]
        private int _strokeWidth = 5;

        /// <summary>
        /// 显示用线宽（乘以 ZoomLevel，由外部设置）
        /// </summary>
        public double StrokeWidthDisplay { get; set; } = 5;

        /// <summary>
        /// 通知显示线宽已更新
        /// </summary>
        public void NotifyStrokeWidthDisplayChanged() => OnPropertyChanged(nameof(StrokeWidthDisplay));

        [ObservableProperty]
        private Color _strokeColor = Colors.Black;

        partial void OnStrokeColorChanged(Color value)
        {
            OnPropertyChanged(nameof(StrokeColorDisplay));
        }

        [ObservableProperty]
        private double _opacity = 1.0;

        [ObservableProperty]
        private BrushType _brushType = BrushType.Pen;

        [ObservableProperty]
        private bool _isSmooth = true;

        [ObservableProperty]
        private double _glossiness = 0.0;

        [ObservableProperty]
        private bool _hasColor = true;

        public int BrushTypeIndex
        {
            get => (int)BrushType;
            set => BrushType = (BrushType)value;
        }

        public IBrush StrokeColorDisplay => new SolidColorBrush(StrokeColor);

        public string BrushTypeName => BrushType switch
        {
            BrushType.Oil => "油画笔",
            BrushType.Marker => "记号笔",
            BrushType.Crayon => "蜡笔",
            BrushType.Watercolor => "水彩笔",
            BrushType.Pen => "钢笔",
            BrushType.Pencil => "铅笔",
            BrushType.Spray => "喷雾罐",
            BrushType.Airbrush => "气笔",
            _ => "钢笔"
        };
    }

    /// <summary>
    /// 预设调色板颜色
    /// </summary>
    public static class PaletteColors
    {
        // 基本颜色
        public static readonly Color[] Basic = new[]
        {
            Color.Parse("#000000"), // 黑
            Color.Parse("#FFFFFF"), // 白
            Color.Parse("#808080"), // 灰
            Color.Parse("#FF0000"), // 红
            Color.Parse("#00FF00"), // 绿
            Color.Parse("#0000FF"), // 蓝
            Color.Parse("#FFFF00"), // 黄
            Color.Parse("#FF00FF"), // 品红
            Color.Parse("#00FFFF"), // 青
        };

        // 柔和颜色
        public static readonly Color[] Soft = new[]
        {
            Color.Parse("#FFC0CB"), // 粉红
            Color.Parse("#FFE4E1"), // 雾玫瑰红
            Color.Parse("#E6E6FA"), // 薰衣草淡紫
            Color.Parse("#E0FFFF"), // 淡青色
            Color.Parse("#F0FFF0"), // 蜜露绿
            Color.Parse("#FFFACD"), // 柠檬绸黄
            Color.Parse("#F5DEB3"), // 小麦色
            Color.Parse("#D3D3D3"), // 浅灰
        };

        // 鲜艳颜色
        public static readonly Color[] Vivid = new[]
        {
            Color.Parse("#FF4500"), // 橙红
            Color.Parse("#FF8C00"), // 暗橙
            Color.Parse("#FFD700"), // 金色
            Color.Parse("#32CD32"), // 酸橙绿
            Color.Parse("#00FA9A"), // 中春绿色
            Color.Parse("#00CED1"), // 深绿松石色
            Color.Parse("#1E90FF"), // 道奇蓝
            Color.Parse("#9370DB"), // 中紫色
        };

        // 大自然颜色
        public static readonly Color[] Nature = new[]
        {
            Color.Parse("#8B4513"), // 马鞍棕色
            Color.Parse("#A0522D"), // 赭色
            Color.Parse("#CD853F"), // 秘鲁色
            Color.Parse("#DEB887"), // 实木色
            Color.Parse("#556B2F"), // 暗橄榄绿
            Color.Parse("#6B8E23"), // 橄榄褐色
            Color.Parse("#2F4F4F"), // 暗石板灰
            Color.Parse("#708090"), // 板岩灰
        };

        // 渐变预设
        public static readonly Color[][] AllCategories = new[] { Basic, Soft, Vivid, Nature };
        public static readonly string[] CategoryNames = new[] { "基本色", "柔和色", "鲜艳色", "自然色" };
    }
}
