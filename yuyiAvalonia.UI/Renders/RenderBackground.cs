using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using yuyiAvalonia.UI.Converters;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Renders
{
    public class RenderBackground
    {
        /// <summary>
        /// 根据背景类型更新背景（不包含背景图）
        /// </summary>
        public void UpdateBackground(Border control, CanvasProperty property)
        {
            if (control == null || property == null) return;

            switch (property.FillType)
            {
                case BackgroundType.None:
                    UpdateNoneBackground(control);
                    break;
                case BackgroundType.SolidColor:
                    UpdateSolidBackground(control, property);
                    break;
                case BackgroundType.Gradient:
                    UpdateGradientBackground(control, property);
                    break;
            }

            control.Opacity = property.CanvasOpacity;
        }

        /// <summary>
        /// 无背景（透明）
        /// </summary>
        public void UpdateNoneBackground(Border control)
        {
            control!.Background = Brushes.Transparent;
        }

        /// <summary>
        /// 纯色背景
        /// </summary>
        public void UpdateSolidBackground(Border control, CanvasProperty property)
        {
            if (control == null) return;
            control.Background = new SolidColorBrush(property.CanvasBackgroundColor.ToColor());
        }

        /// <summary>
        /// 渐变色背景
        /// </summary>
        public void UpdateGradientBackground(Border control, CanvasProperty property)
        {
            if (control == null || property == null) return;

            if (property.FillType == BackgroundType.Gradient)
            {
                // 与 FillItemToGradientBrushConverter（预览）保持一致：以 (0.5, 0.5) 为对称中心
                // 沿角度方向对称偏移 0.5，不 clamp，保证任意角度下向量长度恒为 1，
                // 渐变覆盖整个形状而不是被截到一半。
                var angleRad = property.GradientAngle * Math.PI / 180;
                var dx = Math.Cos(angleRad);
                var dy = Math.Sin(angleRad);

                control.Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                    EndPoint   = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new Avalonia.Media.GradientStop(property.GradientStartColor.ToColor(), 0),
                        new Avalonia.Media.GradientStop(property.GradientEndColor.ToColor(), 1)
                    }
                };
            }
            else
            {
                control.Background = null;
            }
        }

        /// <summary>
        /// 背景图（支持多种填充方式，独立于背景填充）
        /// </summary>
        public void UpdateImageBackground(Border control, CanvasProperty property)
        {
            if (control == null || property == null) return;

            // 只有 EnableBackgroundImage 为 true 时才显示背景图
            if (!property.EnableBackgroundImage)
            {
                control.Background = Brushes.Transparent;
                return;
            }

            var imagePath = property.CanvasBackgroundImage;
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                // 图片不存在时保持背景透明，让下面的背景色/渐变显示出来
                return;
            }

            try
            {
                var bitmap = new Bitmap(imagePath);
                var imageBrush = new ImageBrush(bitmap);

                // TileMode 需要 Stretch=None 才生效
                if (property.BackgroundImageTileMode != TileMode.None)
                {
                    imageBrush.Stretch = Stretch.None;
                    // 设置 DestinationRect 为图片原始尺寸，实现真正的平铺
                    imageBrush.DestinationRect = new RelativeRect(
                        0, 0,
                        (double)bitmap.Size.Width,
                        (double)bitmap.Size.Height,
                        RelativeUnit.Absolute);
                }
                else
                {
                    imageBrush.Stretch = property.BackgroundImageStretch;
                }

                imageBrush.TileMode = property.BackgroundImageTileMode;
                imageBrush.AlignmentX = property.BackgroundImageAlignmentX;
                imageBrush.AlignmentY = property.BackgroundImageAlignmentY;

                control.Background = imageBrush;
            }
            catch
            {
                // 加载失败时保持背景透明
            }
        }

        /// <summary>
        /// 更新背景图路径
        /// </summary>
        public void UpdateImageBackgroundPath(Border control, CanvasProperty property)
        {
            if (property.EnableBackgroundImage)
            {
                UpdateImageBackground(control, property);
            }
        }

        /// <summary>
        /// 更新背景图拉伸模式
        /// </summary>
        public void UpdateImageBackgroundStretch(Border control, CanvasProperty property)
        {
            if (property.EnableBackgroundImage)
            {
                UpdateImageBackground(control, property);
            }
        }

        /// <summary>
        /// 更新背景图平铺模式
        /// </summary>
        public void UpdateImageBackgroundTileMode(Border control, CanvasProperty property)
        {
            if (property.EnableBackgroundImage)
            {
                UpdateImageBackground(control, property);
            }
        }

        /// <summary>
        /// 更新背景图水平对齐
        /// </summary>
        public void UpdateImageBackgroundAlignmentX(Border control, CanvasProperty property)
        {
            if (property.EnableBackgroundImage && control?.Background is ImageBrush imageBrush)
            {
                imageBrush.AlignmentX = property.BackgroundImageAlignmentX;
            }
        }

        /// <summary>
        /// 更新背景图垂直对齐
        /// </summary>
        public void UpdateImageBackgroundAlignmentY(Border control, CanvasProperty property)
        {
            if (property.EnableBackgroundImage && control?.Background is ImageBrush imageBrush)
            {
                imageBrush.AlignmentY = property.BackgroundImageAlignmentY;
            }
        }

        /// <summary>
        /// 更新背景图透明度
        /// </summary>
        public void UpdateImageBackgroundOpacity(Border control, CanvasProperty property)
        {
            if (property.EnableBackgroundImage && control?.Background is ImageBrush imageBrush)
            {
                imageBrush.Opacity = property.CanvasOpacity;
            }
        }

        /// <summary>
        /// 清除背景图
        /// </summary>
        public void ClearImageBackground(Border control)
        {
            control!.Background = Brushes.Transparent;
        }
    }
}
