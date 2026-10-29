using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 图片元素
    /// </summary>
    public partial class ImageElement : CanvasElement
    {
        public ImageElement() : base()
        {
            ElementType = ComponentType.Image;
            IsContainer = false;
        }

        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private Stretch _imageStretch = Avalonia.Media.Stretch.Uniform;

        [ObservableProperty]
        private StretchDirection _imageStretchDirection = StretchDirection.Both;

        public override CanvasElement Clone()
        {
            var element = (ImageElement)base.Clone();
            element.ImagePath = ImagePath;
            element.ImageStretch = ImageStretch;
            element.ImageStretchDirection = ImageStretchDirection;
            return element;
        }
    }
}
