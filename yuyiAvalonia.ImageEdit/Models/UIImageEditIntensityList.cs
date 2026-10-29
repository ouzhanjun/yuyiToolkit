using yuyiAvalonia.ImageEdit.Helpers;
using yuyiAvalonia.ImageEdit.Models;

namespace yuyiAvalonia.ImageEdit.Models
{
    public class UIImageEditIntensityList : ImageIntensityCollection, ICloneable
    {
        public UIImageEditIntensityList()
        {
            // 基础调整
            this.AddRange(new ImageIntensity[]{
                new ImageIntensity("亮度", ImageEditAction.Brightness, ImageIntensityCategory.Basic, -100, 100),
                new ImageIntensity("对比度", ImageEditAction.Contrast, ImageIntensityCategory.Basic, -100, 100),
                new ImageIntensity("曝光", ImageEditAction.Exposure, ImageIntensityCategory.Basic, -100, 100)
            });

            // 色彩调整
            this.AddRange(new ImageIntensity[]{
                new ImageIntensity("饱和度", ImageEditAction.Saturation, ImageIntensityCategory.Color, -100, 100),
                new ImageIntensity("鲜艳度", ImageEditAction.Vibrance, ImageIntensityCategory.Color, -100, 100),
                new ImageIntensity("色温", ImageEditAction.Temperature, ImageIntensityCategory.Color, -100, 100),
                new ImageIntensity("色调", ImageEditAction.Hue, ImageIntensityCategory.Color, -100, 100),
                new ImageIntensity("色彩平衡(红)", ImageEditAction.ColorBalance, ImageIntensityCategory.Color, -100, 100, ImageHelper.ColorChannel.Red),
                new ImageIntensity("色彩平衡(绿)", ImageEditAction.ColorBalance, ImageIntensityCategory.Color, -100, 100, ImageHelper.ColorChannel.Green),
                new ImageIntensity("色彩平衡(蓝)", ImageEditAction.ColorBalance, ImageIntensityCategory.Color, -100, 100, ImageHelper.ColorChannel.Blue)
            });

            // 清晰度调整
            this.AddRange(new ImageIntensity[]{
                new ImageIntensity("模糊", ImageEditAction.Blur, ImageIntensityCategory.Effect, 0, 100),
                new ImageIntensity("锐化", ImageEditAction.Sharpen, ImageIntensityCategory.Effect, 0, 100),
                new ImageIntensity("降噪", ImageEditAction.Denoise, ImageIntensityCategory.Effect, 0, 100),
                new ImageIntensity("景深", ImageEditAction.DepthOfField, ImageIntensityCategory.Effect, 0, 100)
            });

            // 滤镜
            this.AddRange(new ImageIntensity[]{
                // 艺术滤镜
                new ImageIntensity("油画", ImageEditAction.OilPaint, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("水彩", ImageEditAction.Watercolor, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("素描", ImageEditAction.Sketch, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("漫画", ImageEditAction.Comic, ImageIntensityCategory.Stylize, 0, 100),
                // 风格滤镜
                new ImageIntensity("灰度", ImageEditAction.GrayScale, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("棕褐色", ImageEditAction.Sepia, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("反相", ImageEditAction.Invert, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("浮雕", ImageEditAction.Emboss, ImageIntensityCategory.Stylize, 0, 100),
                // 特效滤镜
                new ImageIntensity("边缘检测", ImageEditAction.EdgeDetection, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("马赛克", ImageEditAction.Mosaic, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("鱼眼", ImageEditAction.FishEye, ImageIntensityCategory.Stylize, 0, 100),
                // 新增滤镜
                new ImageIntensity("暗角", ImageEditAction.Vignette, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("颗粒感", ImageEditAction.Grain, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("去雾", ImageEditAction.Defog, ImageIntensityCategory.Stylize, 0, 100),
                new ImageIntensity("高光", ImageEditAction.Highlights, ImageIntensityCategory.Color, -100, 100),
                new ImageIntensity("阴影", ImageEditAction.Shadows, ImageIntensityCategory.Color, -100, 100)
            });
        }

        public object Clone()
        {
            var clone = new UIImageEditIntensityList();
            clone.Clear();
            foreach (var item in this)
            {
                clone.Add((ImageIntensity)item.Clone());
            }
            return clone;
        }

        /// <summary>
        /// 按分类获取项目
        /// </summary>
        public IEnumerable<ImageIntensity> GetByCategory(ImageIntensityCategory category)
        {
            return this.Where(x => x.Category == category);
        }
    }
}
