using Avalonia.Media.Imaging;
using SkiaSharp;
using yuyiAvalonia.ImageEdit.Models;

namespace yuyiAvalonia.ImageEdit.Helpers
{
    public enum ImageEditAction
    {
        None,
        RotateLeft,
        RotateRight,
        FlipHorizontal,
        FlipVertical,
        // 基础调整
        Brightness,
        Contrast,
        Exposure,
        // 颜色调整
        Saturation,
        Vibrance,
        Temperature,
        Hue,
        // 效果
        Blur,
        Sharpen,
        Denoise,
        DepthOfField,
        // 风格化
        GrayScale,
        Sepia,
        Invert,
        Emboss,
        Comic,
        // 新效果
        EdgeDetection,
        OilPaint,
        Watercolor,
        Sketch,
        Mosaic,
        FishEye,
        ColorBalance,
        Crop,
        Brush,
        AddText,
        // 新增效果
        Vignette,      // 暗角
        Grain,         // 颗粒感
        Defog,         // 去雾
        Highlights,    // 高光
        Shadows,       // 阴影
        Liquify        // 液化
    }

    public enum FlipMode
    {
        Horizontal,
        Vertical
    }

    public class ImageEditHistoryItem
    {
        public ImageEditHistoryItem()
        {
            Properties = new HistoryProperty();
        }

        public SKBitmap? Image { get; set; } = null!;

        private HistoryProperty _properties;

        private HistoryAction _action;

        public HistoryProperty Properties { get => _properties; set => _properties = value; }

        public HistoryAction Action { get => _action; set => _action = value; }
    }

    public class HistoryAction
    {
        public ImageEditAction Edit { get; set; }

        public double RotationAngle { get; set; }

        public FlipMode FlipMode { get; set; }
    }


    public class HistoryProperty
    {
        public HistoryProperty()
        {
            _properties = new UIImageEditIntensityList();
        }

        private UIImageEditIntensityList _properties;

        public UIImageEditIntensityList IntensityList
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }
    }
}
