using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.ImageEdit.Helpers;

namespace yuyiAvalonia.ImageEdit.Models
{
    /// <summary>
    /// 图像调整参数分类
    /// </summary>
    public enum ImageIntensityCategory
    {
        Basic,      // 基础调整
        Color,      // 颜色调整
        Effect,     // 效果
        Stylize     // 风格化
    }

    public partial class ImageIntensity : ObservableObject, ICloneable
    {
        public ImageIntensity(string name, ImageEditAction mode, ImageIntensityCategory category = ImageIntensityCategory.Basic, double min = 0, double max = 100, ImageHelper.ColorChannel? channel = null)
        {
            _value = 0;
            Name = name;
            _mode = mode;
            _category = category;
            Minimum = min;
            Maximum = max;
            _channel = channel;
        }

        [ObservableProperty]
        private int _value = 0;

        private string _name;

        private ImageEditAction _mode;

        private ImageIntensityCategory _category;

        private double _minimum = 0;
        private double _maximum = 100;

        private ImageHelper.ColorChannel? _channel;

        public string Name { get => _name; set => _name = value; }

        public ImageEditAction Mode { get => _mode; set => _mode = value; }

        public ImageIntensityCategory Category { get => _category; set => _category = value; }

        public double Minimum { get => _minimum; set => _minimum = value; }

        public double Maximum { get => _maximum; set => _maximum = value; }

        public ImageHelper.ColorChannel? Channel { get => _channel; set => _channel = value; }

        public object Clone()
        {
            ImageIntensity intensity = new ImageIntensity(_name, _mode, _category, Minimum, Maximum, _channel);
            intensity.Value = Value;
            intensity.Minimum = Minimum;
            intensity.Maximum = Maximum;
            return intensity;
        }
    }
}
