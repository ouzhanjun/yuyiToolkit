using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class ImageControl : BorderControlBase
    {
        private Image? _imageControl;
        private Border? _workspaceOverlay;

        public override Control? Control => _imageControl;

        public ImageControl(ImageElement element) : base(element)
        {
            _imageControl = new Image
            {
                Stretch = element.ImageStretch,
                StretchDirection = element.ImageStretchDirection,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            // 加载图片
            LoadImage(element.ImagePath);

            _contentPanel!.Children.Add(_imageControl);

            this.Child = _contentPanel;

            // 初始化描边控件
            InitStrokes();
        }

        private void LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                _imageControl.Source = null;
                return;
            }

            try
            {
                _imageControl.Source = new Bitmap(path);
            }
            catch
            {
                _imageControl.Source = null;
            }
        }

        public override void UpdateElement(CanvasElement element)
        {
            ImageElement imageElement = (ImageElement)element;
            base.UpdateElement(imageElement);

            this.UpdateImageStretch(imageElement.ImageStretch);
            this.UpdateImagePath(imageElement.ImagePath);
        }

        public void UpdateImagePath(string path)
        {
            LoadImage(path);
        }

        public void UpdateImageStretch(Stretch stretch)
        {
            if (_imageControl != null)
            {
                _imageControl.Stretch = stretch;
            }
        }

        public void UpdateImageStretchDirection(StretchDirection stretchDirection)
        {
            if (_imageControl != null)
            {
                _imageControl.StretchDirection = stretchDirection;
            }
        }

        public void UpdateImageTileMode(TileMode tileMode)
        {
            if (_imageControl != null)
            {
                _imageControl.Stretch = tileMode != TileMode.None ? Avalonia.Media.Stretch.Fill : Avalonia.Media.Stretch.Uniform;
            }
        }

        public void UpdateImageAlignmentX(AlignmentX alignmentX)
        {
        }

        public void UpdateImageAlignmentY(AlignmentY alignmentY)
        {
        }

        /// <summary>
        /// 显示工作区覆盖层
        /// </summary>
        public override void ShowWorkspaceOverlay()
        {
            _isShowOverlay = true;
            // ImageControl 不需要覆盖层
        }

        /// <summary>
        /// 隐藏工作区覆盖层
        /// </summary>
        public override void HideWorkspaceOverlay()
        {
            _isShowOverlay = false;
        }
    }
}