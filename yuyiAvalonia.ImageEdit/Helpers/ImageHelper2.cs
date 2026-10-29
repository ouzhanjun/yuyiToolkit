using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace yuyiAvalonia.ImageEdit.Helpers
{
    public class ImageHelper2
    {

        #region 风格化滤镜（Grayscale, Sepia, Invert, Emboss, Comic, Edge Detection）

        /// <summary>
        /// 创建灰度图像
        /// </summary>
        /// <param name="source">源图像</param>
        /// <param name="intensity">灰度强度 (0-100)，0返回原图，100返回完全灰度</param>
        /// <returns>灰度处理后的图像</returns>
        public static Bitmap CreateGrayscaleBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // 如果强度为0，直接返回原图的副本
            if (intensity <= 0)
            {
                return CloneBitmap(source);
            }

            // 如果强度为100，直接使用高效的灰度转换
            if (intensity >= 100)
            {
                return CreateFullGrayscaleBitmap(source);
            }

            // 部分灰度：使用混合方式
            return CreatePartialGrayscaleBitmap(source, intensity);
        }

        /// <summary>
        /// 克隆位图
        /// </summary>
        private static Bitmap CloneBitmap(Bitmap source)
        {
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = srcBitmap.Copy();
            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建完全灰度图像（使用高效的 ColorMatrix）
        /// </summary>
        private static Bitmap CreateFullGrayscaleBitmap(Bitmap source)
        {
            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 创建目标位图
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);
            using var canvas = new SKCanvas(destBitmap);

            // 使用 ColorMatrix 进行灰度转换（高效）
            // 灰度矩阵：将 RGB 通道设为相同的加权值
            var colorMatrix = new float[]
            {
                0.299f, 0.587f, 0.114f, 0, 0,  // R
                0.299f, 0.587f, 0.114f, 0, 0,  // G
                0.299f, 0.587f, 0.114f, 0, 0,  // B
                0,       0,       0,       1, 0   // A
            };

            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(colorMatrix)
            };

            canvas.DrawBitmap(srcBitmap, 0, 0, paint);

            // 转换回 Avalonia Bitmap
            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建部分灰度图像（混合原图和灰度图）- 高性能版本
        /// </summary>
        private static Bitmap CreatePartialGrayscaleBitmap(Bitmap source, int intensity)
        {
            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;
            float grayRatio = intensity / 100.0f;
            float lumR = 0.299f;
            float lumG = 0.587f;
            float lumB = 0.114f;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                // 根据图像尺寸决定是否使用并行处理
                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];

                            float gray = srcR * lumR + srcG * lumG + srcB * lumB;

                            destRow[x4] = (byte)(srcB * (1 - grayRatio) + gray * grayRatio);
                            destRow[x4 + 1] = (byte)(srcG * (1 - grayRatio) + gray * grayRatio);
                            destRow[x4 + 2] = (byte)(srcR * (1 - grayRatio) + gray * grayRatio);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];

                            float gray = srcR * lumR + srcG * lumG + srcB * lumB;

                            destRow[x4] = (byte)(srcB * (1 - grayRatio) + gray * grayRatio);
                            destRow[x4 + 1] = (byte)(srcG * (1 - grayRatio) + gray * grayRatio);
                            destRow[x4 + 2] = (byte)(srcR * (1 - grayRatio) + gray * grayRatio);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 高效地将 Avalonia Bitmap 转换为 SKBitmap（直接复制像素，避免编码/解码）
        /// </summary>
        public static SKBitmap AvaloniaToSkBitmap(Bitmap source)
        {
            int width = source.PixelSize.Width;
            int height = source.PixelSize.Height;

            var skBitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

            unsafe
            {
                var destPtr = (byte*)skBitmap.GetPixels().ToPointer();
                var destRowBytes = skBitmap.RowBytes;

                source.CopyPixels(new PixelRect(0, 0, width, height),
                    (IntPtr)destPtr, destRowBytes * height, destRowBytes);
            }

            return skBitmap;
        }

        /// <summary>
        /// 高效地将 SKBitmap 转换为 Avalonia Bitmap（直接复制像素，避免编码/解码）
        /// </summary>
        public static Bitmap SkBitmapToAvalonia(SKBitmap source)
        {
            int width = source.Width;
            int height = source.Height;
            int stride = (int)source.RowBytes;

            // 创建 Avalonia Bitmap - 使用正确的构造函数
            var bitmap = new Bitmap(
                PixelFormat.Bgra8888,
                AlphaFormat.Premul,
                source.GetPixels(),
                new PixelSize(width, height),
                new Vector(96, 96),
                stride);

            return bitmap;
        }

        /// <summary>
        /// 检测位图是否有透明像素
        /// </summary>
        private static bool HasTransparency(SKBitmap skBitmap)
        {
            unsafe
            {
                var ptr = (byte*)skBitmap.GetPixels().ToPointer();
                int width = skBitmap.Width;
                int height = skBitmap.Height;
                int rowBytes = (int)skBitmap.RowBytes;
                int totalBytes = rowBytes * height;

                // 采样检测：每10个像素检测一次，提高效率
                for (int i = 3; i < totalBytes; i += 40) // BGRA格式，A在每4字节的第4个
                {
                    if (ptr[i] < 255) // Alpha < 255 表示有透明
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 在透明位图上绘制棋格背景（专业图片软件风格）
        /// </summary>
        /// <param name="skBitmap">要绘制的位图</param>
        /// <param name="cellSize">每个棋格的像素大小</param>
        private static void DrawCheckerboardPattern(SKBitmap skBitmap, int cellSize = 16)
        {
            if (cellSize <= 0) cellSize = 1;

            unsafe
            {
                var ptr = (byte*)skBitmap.GetPixels().ToPointer();
                int width = skBitmap.Width;
                int height = skBitmap.Height;
                int rowBytes = (int)skBitmap.RowBytes;

                // 棋格颜色：浅灰 #D3D3D3 和 深灰 #A9A9A9
                var lightGray = new SKColor(211, 211, 211, 255);
                var darkGray = new SKColor(169, 169, 169, 255);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * rowBytes + x * 4;
                        byte alpha = ptr[offset + 3]; // A通道

                        // 只处理透明像素（Alpha < 255）
                        if (alpha < 255)
                        {
                            bool isLightCell = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                            var color = isLightCell ? lightGray : darkGray;

                            ptr[offset] = color.Blue;      // B
                            ptr[offset + 1] = color.Green; // G
                            ptr[offset + 2] = color.Red;   // R
                            ptr[offset + 3] = 255;         // A = 不透明
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 将透明位图恢复为完全透明
        /// </summary>
        private static void RestoreTransparency(SKBitmap skBitmap, SKBitmap originalBitmap)
        {
            if (originalBitmap == null) return;

            unsafe
            {
                var ptr = (byte*)skBitmap.GetPixels().ToPointer();
                var origPtr = (byte*)originalBitmap.GetPixels().ToPointer();
                int width = skBitmap.Width;
                int height = skBitmap.Height;
                int rowBytes = (int)skBitmap.RowBytes;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int offset = y * rowBytes + x * 4;
                        // 从原始位图恢复Alpha通道
                        ptr[offset + 3] = origPtr[offset + 3];
                    }
                }
            }
        }

        /// <summary>
        /// 将 SkiaSharp 位图转换为 Avalonia Bitmap（带棋格背景显示，JPEG高效编码）
        /// 有透明时绘制棋格背景替代，保持高效
        /// </summary>
        private static Bitmap ConvertToAvaloniaBitmap(SKBitmap skBitmap, bool drawCheckerboard = true)
        {
            bool hasTransparency = HasTransparency(skBitmap);

            // 深拷贝一份用于可能需要恢复透明
            SKBitmap? originalForRestore = null;
            if (hasTransparency && !drawCheckerboard)
            {
                originalForRestore = skBitmap.Copy();
            }

            if (hasTransparency && drawCheckerboard)
            {
                // 有透明度：绘制棋格背景 + JPEG高效编码
                DrawCheckerboardPattern(skBitmap);
                using var image = SKImage.FromBitmap(skBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 95);
                using var resultStream = new MemoryStream(data.ToArray());
                return new Bitmap(resultStream);
            }
            else
            {
                // 无透明度或不需要棋格，直接JPEG编码
                using var image = SKImage.FromBitmap(skBitmap);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 95);
                using var resultStream = new MemoryStream(data.ToArray());
                return new Bitmap(resultStream);
            }
        }

        /// <summary>
        /// 将 SkiaSharp 位图转换为 Avalonia Bitmap（保留透明背景，不绘制棋格）
        /// 用于需要精确透明度的场景
        /// </summary>
        private static Bitmap ConvertToAvaloniaBitmapTransparent(SKBitmap skBitmap)
        {
            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var resultStream = new MemoryStream(data.ToArray());
            return new Bitmap(resultStream);
        }


        #endregion

        #region 几何变换（旋转、翻转、裁剪）

        /// <summary>
        /// 创建旋转后的图像
        /// </summary>
        public static Bitmap CreateRotatedBitmap(Bitmap source, double angle)
        {
            // 使用 SkiaSharp 进行真正的图像旋转
            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接从 Avalonia Bitmap 复制像素数据到 SKBitmap，避免编码
            using var srcBitmap = new SKBitmap(srcWidth, srcHeight, SKColorType.Bgra8888, SKAlphaType.Premul);

            unsafe
            {
                var destPtr = (byte*)srcBitmap.GetPixels().ToPointer();
                var destRowBytes = srcBitmap.RowBytes;

                // 使用 CopyPixels 直接读取像素数据
                source.CopyPixels(new PixelRect(0, 0, srcWidth, srcHeight),
                    (IntPtr)destPtr, destRowBytes * srcHeight, destRowBytes);
            }

            // 计算旋转后的尺寸
            float newWidth, newHeight;

            if (Math.Abs(angle) == 90 || Math.Abs(angle) == 270)
            {
                newWidth = srcHeight;
                newHeight = srcWidth;
            }
            else
            {
                newWidth = srcWidth;
                newHeight = srcHeight;
            }

            // 创建目标 SkiaSharp 位图
            using var destBitmap = new SKBitmap((int)newWidth, (int)newHeight);
            using var canvas = new SKCanvas(destBitmap);

            // 执行旋转
            canvas.Translate(newWidth / 2, newHeight / 2);
            canvas.RotateDegrees((float)angle);
            canvas.Translate(-srcWidth / 2, -srcHeight / 2);

            // 绘制原始图像
            canvas.DrawBitmap(srcBitmap, 0, 0);

            return ConvertToAvaloniaBitmap(destBitmap);
        }


        // 创建裁剪后的位图
        public static Bitmap CreateCroppedBitmap(Bitmap source, Rect cropRect)
        {
            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 确保裁剪区域在图像范围内
            var x = (int)Math.Max(0, cropRect.X);
            var y = (int)Math.Max(0, cropRect.Y);
            var width = (int)Math.Min(srcWidth - x, cropRect.Width);
            var height = (int)Math.Min(srcHeight - y, cropRect.Height);

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 创建裁剪区域
            var cropRectSk = new SKRectI(x, y, x + width, y + height);

            var paint = new SKPaint
            {
                IsAntialias = true, // 抗锯齿（关键：避免渲染模糊）
                FilterQuality = SKFilterQuality.High, // 高画质缩放
            };

            // 裁剪
            using var destBitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(destBitmap);

            canvas.DrawBitmap(srcBitmap, cropRectSk, new SKRect(0, 0, width, height), paint);

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        #endregion

        #region 曝光和亮度调整（Exposure, Brightness）

        /// <summary>
        /// 创建翻转后的图像
        /// </summary>
        public static Bitmap CreateFlippedBitmap(Bitmap source, FlipMode mode)
        {
            var width = source.PixelSize.Width;
            var height = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 创建目标 SkiaSharp 位图
            using var destBitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(destBitmap);

            // 执行翻转
            if (mode == FlipMode.Horizontal)
            {
                canvas.Scale(-1, 1, width / 2f, height / 2f);
            }
            else // Vertical
            {
                canvas.Scale(1, -1, width / 2f, height / 2f);
            }

            // 绘制原始图像
            canvas.DrawBitmap(srcBitmap, 0, 0);

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建曝光调整后的图像
        /// 使用平滑曲线（Gamma校正）使变化更自然
        /// </summary>
        /// <param name="source">源图像</param>
        /// <param name="intensity">曝光强度 (-100 到 100)，0为原图</param>
        /// <returns>曝光调整后的图像</returns>
        public static Bitmap CreateExposureBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 使用 Gamma 校正使曝光变化更平滑自然
            // intensity: -100 到 100
            double normalizedIntensity = intensity / 100.0;

            // 计算 Gamma 值：-100 时 gamma > 1 (变暗)，+100 时 gamma < 1 (变亮)
            double gamma = 1.0 - (normalizedIntensity * 0.6);  // gamma 范围: 0.4 到 1.6
            gamma = Math.Max(0.1, Math.Min(3.0, gamma));

            // 预计算查找表以提高性能
            byte[] gammaLut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                double normalized = i / 255.0;
                double corrected = Math.Pow(normalized, gamma);
                gammaLut[i] = (byte)Math.Clamp(corrected * 255, 0, 255);
            }

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                var srcRowBytes = srcBitmap.RowBytes;
                var destRowBytes = destBitmap.RowBytes;

                // 直接使用数组索引，不需要 fixed
                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            destRow[x4] = gammaLut[srcRow[x4]];
                            destRow[x4 + 1] = gammaLut[srcRow[x4 + 1]];
                            destRow[x4 + 2] = gammaLut[srcRow[x4 + 2]];
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            destRow[x4] = gammaLut[srcRow[x4]];
                            destRow[x4 + 1] = gammaLut[srcRow[x4 + 1]];
                            destRow[x4 + 2] = gammaLut[srcRow[x4 + 2]];
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        #endregion

        #region 对比度调整（Contrast）

        /// <summary>
        /// 创建对比度调整后的图像
        /// </summary>
        /// <param name="source">源图像</param>
        /// <param name="intensity">对比度强度 (-100 到 100)，0为原图</param>
        /// <returns>对比度调整后的图像</returns>
        public static Bitmap CreateContrastBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 计算对比度因子：使用指数映射，确保负值时不会完全消失
            // intensity: -100 到 100
            // contrastFactor: 0.5 到 2.0（指数映射）
            float contrastFactor;
            if (intensity > 0)
            {
                // 正值：1.0 到 2.0
                contrastFactor = 1.0f + (intensity / 100.0f);
            }
            else
            {
                // 负值：0.5 到 1.0
                contrastFactor = 1.0f / (1.0f + Math.Abs(intensity) / 100.0f);
            }

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            // 以128为中点进行缩放
                            destRow[x4] = (byte)Math.Clamp(128 + (srcRow[x4] - 128) * contrastFactor, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(128 + (srcRow[x4 + 1] - 128) * contrastFactor, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(128 + (srcRow[x4 + 2] - 128) * contrastFactor, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            // 以128为中点进行缩放
                            destRow[x4] = (byte)Math.Clamp(128 + (srcRow[x4] - 128) * contrastFactor, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(128 + (srcRow[x4 + 1] - 128) * contrastFactor, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(128 + (srcRow[x4 + 2] - 128) * contrastFactor, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        #endregion

        #region 颜色调整（Saturation, Vibrance, Temperature, Hue, Color Balance）

        /// <summary>
        /// 创建饱和度调整后的图像 - 高性能版本
        /// </summary>
        /// <param name="source">源图像</param>
        /// <param name="intensity">饱和度强度 (-100 到 100)，0为原图，100为最鲜艳</param>
        /// <returns>饱和度调整后的图像</returns>
        public static Bitmap CreateSaturationBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 计算饱和度因子：-100 到 100 映射到 0.0 到 2.0
            float saturationFactor = 1.0f + (intensity / 100.0f);

            // 使用 RGB 矩阵乘法进行饱和度调整
            // 公式: newColor = gray * (1 - sat) * lumRGB + originalColor * sat
            // 展开后: newR = originalR * sat + gray * lumR * (1 - sat)
            float sat = saturationFactor;
            float oneMinusSat = 1.0f - saturationFactor;

            // 预计算矩阵系数
            float rCoeff = sat + oneMinusSat * 0.2126f;
            float gCoeff = oneMinusSat * 0.2126f;
            float bCoeff = oneMinusSat * 0.2126f;

            float rCoeffG = oneMinusSat * 0.7152f;
            float gCoeffG = sat + oneMinusSat * 0.7152f;
            float bCoeffG = oneMinusSat * 0.7152f;

            float rCoeffB = oneMinusSat * 0.0722f;
            float gCoeffB = oneMinusSat * 0.0722f;
            float bCoeffB = sat + oneMinusSat * 0.0722f;

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            // 计算亮度 (灰度值)
                            float gray = srcR * 0.2126f + srcG * 0.7152f + srcB * 0.0722f;

                            // 应用饱和度矩阵变换
                            float nr = srcR * rCoeff + gray * gCoeff + gray * bCoeff;
                            float ng = srcR * rCoeffG + srcG * gCoeffG + gray * bCoeffG;
                            float nb = srcR * rCoeffB + srcG * gCoeffB + srcB * bCoeffB;

                            destRow[x4] = (byte)Math.Clamp(nb, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(ng, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(nr, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float gray = srcR * 0.2126f + srcG * 0.7152f + srcB * 0.0722f;

                            float nr = srcR * rCoeff + gray * gCoeff + gray * bCoeff;
                            float ng = srcR * rCoeffG + srcG * gCoeffG + gray * bCoeffG;
                            float nb = srcR * rCoeffB + srcG * gCoeffB + srcB * bCoeffB;

                            destRow[x4] = (byte)Math.Clamp(nb, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(ng, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(nr, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        #endregion

        #region 亮度调整（Brightness）

        /// <summary>
        /// 创建亮度调整后的图像 - 高性能版本
        /// 使用平滑曲线（Sigmoid）使变化更自然
        /// </summary>
        public static Bitmap CreateBrightnessBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 使用平滑曲线（Sigmoid函数）使变化更自然
            // intensity: -100 到 100 映射到 -1.0 到 1.0
            double normalizedIntensity = intensity / 100.0;

            // Sigmoid 平滑因子，使用曲线使变化更平缓
            double smoothFactor = Math.Tanh(normalizedIntensity * 1.5);

            // 转换为亮度偏移值：-255 到 255
            int brightnessOffset = (int)(smoothFactor * 255);

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(srcRow[x4] + brightnessOffset, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(srcRow[x4 + 1] + brightnessOffset, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(srcRow[x4 + 2] + brightnessOffset, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(srcRow[x4] + brightnessOffset, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(srcRow[x4 + 1] + brightnessOffset, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(srcRow[x4 + 2] + brightnessOffset, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        #endregion

        #region 效果处理（Blur, Sharpen, Denoise, Depth of Field）

        /// <summary>
        /// 创建模糊后的图像（高斯模糊）
        /// </summary>
        public static Bitmap CreateBlurBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 使用 SkiaSharp 的高斯模糊
            float sigma = intensity / 5f; // 将 0-100 映射到模糊半径
            sigma = Math.Max(1, Math.Min(sigma, 50));

            using var filter = SKImageFilter.CreateBlur(sigma, sigma);
            using var paint = new SKPaint { ImageFilter = filter };
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);
            using var canvas = new SKCanvas(destBitmap);

            canvas.DrawBitmap(srcBitmap, 0, 0, paint);

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建锐化后的图像
        /// </summary>
        public static Bitmap CreateSharpenBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 锐化强度因子
            float factor = intensity / 100f * 2f;

            // 锐化卷积核
            float[,] kernel = {
                { 0, -factor, 0 },
                { -factor, 1 + 4 * factor, -factor },
                { 0, -factor, 0 }
            };

            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(1, srcHeight - 1, y =>
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        byte* srcRowCurrent = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            float r = 0, g = 0, b = 0;

                            for (int ky = -1; ky <= 1; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -1; kx <= 1; kx++)
                                {
                                    int px4 = (x + kx) * 4;
                                    float k = kernel[ky + 1, kx + 1];
                                    r += neighborRow[px4 + 2] * k;
                                    g += neighborRow[px4 + 1] * k;
                                    b += neighborRow[px4] * k;
                                }
                            }

                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcRowCurrent[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 1; y < srcHeight - 1; y++)
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        byte* srcRowCurrent = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            float r = 0, g = 0, b = 0;

                            for (int ky = -1; ky <= 1; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -1; kx <= 1; kx++)
                                {
                                    int px4 = (x + kx) * 4;
                                    float k = kernel[ky + 1, kx + 1];
                                    r += neighborRow[px4 + 2] * k;
                                    g += neighborRow[px4 + 1] * k;
                                    b += neighborRow[px4] * k;
                                }
                            }

                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcRowCurrent[x4 + 3];
                        }
                    }
                }

                // 边缘像素保持不变 - 使用并行处理
                var destPixels = destBitmap.GetPixels();
                Parallel.For(0, srcHeight, y =>
                {
                    byte* destRow = (byte*)destPixels + (y * destRowBytes);
                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                    // 左边缘
                    destRow[0] = srcRow[0];
                    destRow[1] = srcRow[1];
                    destRow[2] = srcRow[2];
                    destRow[3] = srcRow[3];

                    // 右边缘
                    int rightX = (srcWidth - 1) * 4;
                    destRow[rightX] = srcRow[rightX];
                    destRow[rightX + 1] = srcRow[rightX + 1];
                    destRow[rightX + 2] = srcRow[rightX + 2];
                    destRow[rightX + 3] = srcRow[rightX + 3];
                });

                Parallel.For(0, srcWidth, x =>
                {
                    int x4 = x * 4;
                    // 上边缘
                    byte* srcRowTop = (byte*)srcPixels;
                    byte* destRowTop = (byte*)destPixels;
                    destRowTop[x4] = srcRowTop[x4];
                    destRowTop[x4 + 1] = srcRowTop[x4 + 1];
                    destRowTop[x4 + 2] = srcRowTop[x4 + 2];
                    destRowTop[x4 + 3] = srcRowTop[x4 + 3];

                    // 下边缘
                    byte* srcRowBottom = (byte*)srcPixels + ((srcHeight - 1) * srcRowBytes);
                    byte* destRowBottom = (byte*)destPixels + ((srcHeight - 1) * destRowBytes);
                    destRowBottom[x4] = srcRowBottom[x4];
                    destRowBottom[x4 + 1] = srcRowBottom[x4 + 1];
                    destRowBottom[x4 + 2] = srcRowBottom[x4 + 2];
                    destRowBottom[x4 + 3] = srcRowBottom[x4 + 3];
                });
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建鲜艳度调整后的图像
        /// 智能增强微弱颜色的饱和度，同时保护肤色
        /// </summary>
        public static Bitmap CreateVibranceBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 转换为 HSV 色彩空间进行调整
            float vibranceAmount = intensity / 100f; // -1 到 1

            // 预计算增强因子，避免在循环中重复计算
            float satBoostFactor = 1.0f + vibranceAmount;
            float skinReductionFactor = 1.0f - vibranceAmount * 0.7f; // 肤色保护

            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            // 快速计算饱和度 (使用 RGB 差值估算)
                            int max = Math.Max(srcR, Math.Max(srcG, srcB));
                            int min = Math.Min(srcR, Math.Min(srcG, srcB));
                            int delta = max - min;

                            // 饱和度 (0-1 范围)
                            float s = max == 0 ? 0 : (float)delta / max;

                            // 智能鲜艳度调整：饱和度越低，增强越多
                            float vibranceBoost = (1 - s) * vibranceAmount;

                            // 保护肤色：检测肤色区域并减少处理
                            // 简单肤色检测：较高的红色，绿色适中，蓝色较低
                            bool isSkinTone = srcR > 100 && srcR > srcB && srcG > 50 && srcG < 180 && srcB < 100;
                            if (isSkinTone)
                            {
                                vibranceBoost *= 0.3f; // 减少对肤色的影响
                            }

                            // 应用饱和度调整
                            float newS = s + vibranceBoost * s;
                            newS = Math.Clamp(newS, 0, 1);

                            // 计算亮度 (灰度值)
                            float gray = srcR * 0.2126f + srcG * 0.7152f + srcB * 0.0722f;
                            float v = max / 255f;

                            // 使用简化的 RGB 变换代替完整的 HSV 反向转换
                            // newColor = gray * (1 - newS) + originalColor * newS
                            float oneMinusNewS = 1 - newS;
                            float nr = (gray * oneMinusNewS + srcR * newS) / 255f;
                            float ng = (gray * oneMinusNewS + srcG * newS) / 255f;
                            float nb = (gray * oneMinusNewS + srcB * newS) / 255f;

                            destRow[x4] = (byte)Math.Clamp(nb * 255, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(ng * 255, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(nr * 255, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            int max = Math.Max(srcR, Math.Max(srcG, srcB));
                            int min = Math.Min(srcR, Math.Min(srcG, srcB));
                            int delta = max - min;

                            float s = max == 0 ? 0 : (float)delta / max;

                            float vibranceBoost = (1 - s) * vibranceAmount;

                            bool isSkinTone = srcR > 100 && srcR > srcB && srcG > 50 && srcG < 180 && srcB < 100;
                            if (isSkinTone)
                            {
                                vibranceBoost *= 0.3f;
                            }

                            float newS = s + vibranceBoost * s;
                            newS = Math.Clamp(newS, 0, 1);

                            float gray = srcR * 0.2126f + srcG * 0.7152f + srcB * 0.0722f;
                            float oneMinusNewS = 1 - newS;
                            float nr = (gray * oneMinusNewS + srcR * newS) / 255f;
                            float ng = (gray * oneMinusNewS + srcG * newS) / 255f;
                            float nb = (gray * oneMinusNewS + srcB * newS) / 255f;

                            destRow[x4] = (byte)Math.Clamp(nb * 255, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(ng * 255, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(nr * 255, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建棕褐色（复古色调）图像 - 高性能版本
        /// </summary>
        public static Bitmap CreateSepiaBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float sepiaAmount = intensity / 100f;

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR;
                            float g = srcG;
                            float b = srcB;

                            // 棕褐色转换矩阵
                            float sR = 0.393f * r + 0.769f * g + 0.189f * b;
                            float sG = 0.349f * r + 0.686f * g + 0.168f * b;
                            float sB = 0.272f * r + 0.534f * g + 0.131f * b;

                            // 混合原图和棕褐色
                            r = r * (1 - sepiaAmount) + sR * sepiaAmount;
                            g = g * (1 - sepiaAmount) + sG * sepiaAmount;
                            b = b * (1 - sepiaAmount) + sB * sepiaAmount;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR;
                            float g = srcG;
                            float b = srcB;

                            // 棕褐色转换矩阵
                            float sR = 0.393f * r + 0.769f * g + 0.189f * b;
                            float sG = 0.349f * r + 0.686f * g + 0.168f * b;
                            float sB = 0.272f * r + 0.534f * g + 0.131f * b;

                            // 混合原图和棕褐色
                            r = r * (1 - sepiaAmount) + sR * sepiaAmount;
                            g = g * (1 - sepiaAmount) + sG * sepiaAmount;
                            b = b * (1 - sepiaAmount) + sB * sepiaAmount;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建反相图像（胶片效果） - 高性能版本
        /// </summary>
        public static Bitmap CreateInvertBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float invertAmount = intensity / 100f;

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            byte r = (byte)(255 - srcR);
                            byte g = (byte)(255 - srcG);
                            byte b = (byte)(255 - srcB);

                            // 混合原图和反相
                            r = (byte)(srcR * (1 - invertAmount) + r * invertAmount);
                            g = (byte)(srcG * (1 - invertAmount) + g * invertAmount);
                            b = (byte)(srcB * (1 - invertAmount) + b * invertAmount);

                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            byte r = (byte)(255 - srcR);
                            byte g = (byte)(255 - srcG);
                            byte b = (byte)(255 - srcB);

                            // 混合原图和反相
                            r = (byte)(srcR * (1 - invertAmount) + r * invertAmount);
                            g = (byte)(srcG * (1 - invertAmount) + g * invertAmount);
                            b = (byte)(srcB * (1 - invertAmount) + b * invertAmount);

                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建色温调整后的图像 - 高性能版本
        /// 冷色温偏蓝，暖色温偏橙黄
        /// </summary>
        public static Bitmap CreateTemperatureBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 色温：负值偏冷(蓝)，正值偏暖(橙)
            float temp = intensity / 100f; // -1 到 1
            float rOffset, gOffset, bOffset;

            if (temp > 0)
            {
                // 暖色：增加红色和绿色，减少蓝色
                rOffset = temp * 40;
                gOffset = temp * 15;
                bOffset = -temp * 30;
            }
            else
            {
                // 冷色：增加蓝色，减少红色
                float absTemp = -temp;
                rOffset = -absTemp * 30;
                gOffset = -absTemp * 10;
                bOffset = absTemp * 40;
            }

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float nr = srcR + rOffset;
                            float ng = srcG + gOffset;
                            float nb = srcB + bOffset;

                            // 保持绿色通道相对稳定，只调整红蓝比例
                            float greenOffset = (srcR + srcB) / 2f - srcG;
                            if (temp > 0)
                                ng -= greenOffset * temp * 0.3f;

                            destRow[x4] = (byte)Math.Clamp(nb, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(ng, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(nr, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float nr = srcR + rOffset;
                            float ng = srcG + gOffset;
                            float nb = srcB + bOffset;

                            // 保持绿色通道相对稳定，只调整红蓝比例
                            float greenOffset = (srcR + srcB) / 2f - srcG;
                            if (temp > 0)
                                ng -= greenOffset * temp * 0.3f;

                            destRow[x4] = (byte)Math.Clamp(nb, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(ng, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(nr, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建色调调整后的图像
        /// 色相环旋转，改变整体颜色
        /// </summary>
        public static Bitmap CreateHueBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 使用 SkiaSharp 的 ColorMatrix 进行色调旋转
            // 色调旋转角度
            float degrees = intensity * 3.6f; // -360 到 360 度

            // 创建旋转后的颜色矩阵
            var hueMatrix = CreateHueRotationMatrix(degrees);

            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(hueMatrix)
            };

            using var destBitmap = new SKBitmap(srcWidth, srcHeight);
            using var canvas = new SKCanvas(destBitmap);
            canvas.DrawBitmap(srcBitmap, 0, 0, paint);

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建色调旋转矩阵
        /// </summary>
        private static float[] CreateHueRotationMatrix(float degrees)
        {
            // 将角度转换为弧度
            double radians = degrees * Math.PI / 180.0;

            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);

            //  lumina = 0.213 0.715 0.072
            float lumR = 0.213f;
            float lumG = 0.715f;
            float lumB = 0.072f;

            // 完整的色相旋转矩阵
            float[] matrix = new float[] {
                (float)(lumR + cos * (1 - lumR) + sin * (-lumR)),
                (float)(lumG + cos * (-lumG) + sin * (-lumG)),
                (float)(lumB + cos * (-lumB) + sin * (1 - lumB)),
                0, 0,

                (float)(lumR + cos * (-lumR) + sin * 0.143),
                (float)(lumG + cos * (1 - lumG) + sin * 0.140),
                (float)(lumB + cos * (-lumB) + sin * (-0.283)),
                0, 0,

                (float)(lumR + cos * (-lumR) + sin * (-(1 - lumR))),
                (float)(lumG + cos * (-lumG) + sin * lumG),
                (float)(lumB + cos * (1 - lumB) + sin * lumB),
                0, 0,

                0, 0, 0, 1, 0
            };

            return matrix;
        }

        /// <summary>
        /// 创建降噪后的图像（快速双边滤波，保边降噪）- 优化版本
        /// </summary>
        public static Bitmap CreateDenoiseBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);

            float d = Math.Max(2, intensity / 10f);
            float s = Math.Max(10, intensity);
            int radius = Math.Min(3, (int)Math.Ceiling(d * 2));

            int kernelSize = radius * 2 + 1;
            int kernelArea = kernelSize * kernelSize;

            float d2Inv = 1.0f / (2 * d * d);
            float s2Inv = 1.0f / (2 * s * s);

            float[] spatialWeights = new float[kernelArea];
            for (int ky = -radius; ky <= radius; ky++)
            {
                for (int kx = -radius; kx <= radius; kx++)
                {
                    spatialWeights[(ky + radius) * kernelSize + (kx + radius)] =
                        (float)Math.Exp(-(kx * kx + ky * ky) * d2Inv);
                }
            }

            float[] rangeWeights = new float[256];
            for (int i = 0; i < 256; i++)
            {
                rangeWeights[i] = (float)Math.Exp(-(i * i) * s2Inv);
            }

            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                var srcRowBytes = srcBitmap.RowBytes;
                var destRowBytes = destBitmap.RowBytes;

                Buffer.MemoryCopy((void*)srcPixels, (void*)destPixels,
                    destBitmap.RowBytes * srcHeight, srcRowBytes * srcHeight);

                if (radius == 0)
                {
                    return ConvertToAvaloniaBitmap(destBitmap);
                }

                int grayWidth = srcWidth;
                int grayHeight = srcHeight;
                int* grayMap = (int*)NativeMemory.Alloc((nuint)(grayWidth * grayHeight * sizeof(int)));

                for (int y = 0; y < grayHeight; y++)
                {
                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                    int* grayRow = grayMap + (y * grayWidth);

                    for (int x = 0; x < grayWidth; x++)
                    {
                        int x4 = x * 4;
                        grayRow[x] = (int)(srcRow[x4 + 2] * 0.299f +
                                          srcRow[x4 + 1] * 0.587f +
                                          srcRow[x4] * 0.114f);
                    }
                }

                int processWidth = srcWidth - radius;
                int processHeight = srcHeight - radius;

                // 边界像素使用镜像填充处理
                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        int yStart = Math.Max(0, y - radius);
                        int yEnd = Math.Min(srcHeight - 1, y + radius);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int xStart = Math.Max(0, x - radius);
                            int xEnd = Math.Min(srcWidth - 1, x + radius);

                            int centerGray = grayMap[y * grayWidth + x];
                            int x4 = x * 4;

                            float rSum = 0, gSum = 0, bSum = 0, wSum = 0;

                            for (int ky = yStart; ky <= yEnd; ky++)
                            {
                                int* neighborGrayRow = grayMap + (ky * grayWidth);
                                byte* neighborSrcRow = (byte*)srcPixels + (ky * srcRowBytes);
                                int kyIdx = (ky - y + radius) * kernelSize;

                                for (int kx = xStart; kx <= xEnd; kx++)
                                {
                                    int neighborGray = neighborGrayRow[kx];
                                    int grayDiff = centerGray - neighborGray;
                                    if (grayDiff < 0) grayDiff = -grayDiff;
                                    float rangeWeight = rangeWeights[grayDiff];
                                    float weight = spatialWeights[kyIdx + (kx - x + radius)] * rangeWeight;

                                    int nx4 = kx * 4;
                                    rSum += neighborSrcRow[nx4 + 2] * weight;
                                    gSum += neighborSrcRow[nx4 + 1] * weight;
                                    bSum += neighborSrcRow[nx4] * weight;
                                    wSum += weight;
                                }
                            }

                            float wSumInv = 1.0f / wSum;
                            destRow[x4] = (byte)(bSum * wSumInv + 0.5f);
                            destRow[x4 + 1] = (byte)(gSum * wSumInv + 0.5f);
                            destRow[x4 + 2] = (byte)(rSum * wSumInv + 0.5f);
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        int yStart = Math.Max(0, y - radius);
                        int yEnd = Math.Min(srcHeight - 1, y + radius);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int xStart = Math.Max(0, x - radius);
                            int xEnd = Math.Min(srcWidth - 1, x + radius);

                            int centerGray = grayMap[y * grayWidth + x];
                            int x4 = x * 4;

                            float rSum = 0, gSum = 0, bSum = 0, wSum = 0;

                            for (int ky = yStart; ky <= yEnd; ky++)
                            {
                                int* neighborGrayRow = grayMap + (ky * grayWidth);
                                byte* neighborSrcRow = (byte*)srcPixels + (ky * srcRowBytes);
                                int kyIdx = (ky - y + radius) * kernelSize;

                                for (int kx = xStart; kx <= xEnd; kx++)
                                {
                                    int neighborGray = neighborGrayRow[kx];
                                    int grayDiff = centerGray - neighborGray;
                                    if (grayDiff < 0) grayDiff = -grayDiff;
                                    float rangeWeight = rangeWeights[grayDiff];
                                    float weight = spatialWeights[kyIdx + (kx - x + radius)] * rangeWeight;

                                    int nx4 = kx * 4;
                                    rSum += neighborSrcRow[nx4 + 2] * weight;
                                    gSum += neighborSrcRow[nx4 + 1] * weight;
                                    bSum += neighborSrcRow[nx4] * weight;
                                    wSum += weight;
                                }
                            }

                            float wSumInv = 1.0f / wSum;
                            destRow[x4] = (byte)(bSum * wSumInv + 0.5f);
                            destRow[x4 + 1] = (byte)(gSum * wSumInv + 0.5f);
                            destRow[x4 + 2] = (byte)(rSum * wSumInv + 0.5f);
                        }
                    }
                }

                NativeMemory.Free(grayMap);
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建景深效果的图像（径向模糊，模拟浅景深）- 修复版本
        /// </summary>
        public static Bitmap CreateDepthOfFieldBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 径向模糊中心
            float centerX = srcWidth / 2f;
            float centerY = srcHeight / 2f;
            float strength = intensity / 20f; // 调整强度映射，使效果更明显
            float maxDist = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

            // 预生成随机种子数组以支持并行处理
            var random = new Random(42);
            int maxSamples = 21;
            float[] randomAngles = new float[maxSamples];
            float[] randomRadii = new float[maxSamples];
            for (int i = 0; i < maxSamples; i++)
            {
                randomAngles[i] = (float)(random.NextDouble() * 2 * Math.PI);
                randomRadii[i] = (float)random.NextDouble();
            }

            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                // 先复制源图像到目标
                Buffer.MemoryCopy((void*)srcPixels, (void*)destBitmap.GetPixels(),
                    destBitmap.RowBytes * srcHeight, srcRowBytes * srcHeight);

                var destPixels = destBitmap.GetPixels();

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);
                        byte* srcRowCurrent = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            // 计算到中心的距离
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            // 距离中心越远，模糊越强
                            float blurAmount = (dist / maxDist) * strength;
                            blurAmount = Math.Min(blurAmount, 15); // 增加最大模糊量

                            if (blurAmount < 1.0f) // 降低阈值，使更多区域产生模糊
                            {
                                continue; // 保持原图
                            }

                            // 采样多次取平均
                            int samples = Math.Max(5, (int)blurAmount * 3); // 增加采样数
                            samples = Math.Min(samples, maxSamples);
                            int rSum = 0, gSum = 0, bSum = 0;

                            for (int i = 0; i < samples; i++)
                            {
                                float angle = randomAngles[i];
                                float radius = randomRadii[i] * blurAmount;
                                int sx = (int)(x + (float)Math.Cos(angle) * radius);
                                int sy = (int)(y + (float)Math.Sin(angle) * radius);

                                sx = Math.Clamp(sx, 0, srcWidth - 1);
                                sy = Math.Clamp(sy, 0, srcHeight - 1);

                                byte* sampleRow = (byte*)srcPixels + (sy * srcRowBytes);
                                int sx4 = sx * 4;
                                rSum += sampleRow[sx4 + 2];
                                gSum += sampleRow[sx4 + 1];
                                bSum += sampleRow[sx4];
                            }

                            int x4 = x * 4;
                            destRow[x4] = (byte)(bSum / samples);
                            destRow[x4 + 1] = (byte)(gSum / samples);
                            destRow[x4 + 2] = (byte)(rSum / samples);
                        }
                    });
                }
                else
                {
                    var randomLocal = new Random(42);

                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);
                        byte* srcRowCurrent = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            float blurAmount = (dist / maxDist) * strength;
                            blurAmount = Math.Min(blurAmount, 15);

                            if (blurAmount < 1.0f)
                            {
                                continue;
                            }

                            int samples = Math.Max(5, (int)blurAmount * 3);
                            int rSum = 0, gSum = 0, bSum = 0;

                            for (int i = 0; i < samples; i++)
                            {
                                float angle = (float)(randomLocal.NextDouble() * 2 * Math.PI);
                                float radius = (float)(randomLocal.NextDouble() * blurAmount);
                                int sx = (int)(x + (float)Math.Cos(angle) * radius);
                                int sy = (int)(y + (float)Math.Sin(angle) * radius);

                                sx = Math.Clamp(sx, 0, srcWidth - 1);
                                sy = Math.Clamp(sy, 0, srcHeight - 1);

                                byte* sampleRow = (byte*)srcPixels + (sy * srcRowBytes);
                                int sx4 = sx * 4;
                                rSum += sampleRow[sx4 + 2];
                                gSum += sampleRow[sx4 + 1];
                                bSum += sampleRow[sx4];
                            }

                            int x4 = x * 4;
                            destRow[x4] = (byte)(bSum / samples);
                            destRow[x4 + 1] = (byte)(gSum / samples);
                            destRow[x4 + 2] = (byte)(rSum / samples);
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建浮雕效果的图像
        /// 立体感浮雕，带方向选择
        /// </summary>
        public static Bitmap CreateEmbossBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 参数设置
            float strength = intensity / 100f; // 0-1
            float embossStrength = 1 + strength * 3f; // 浮雕强度

            // 可选的浮雕方向：0=45度, 1=135度, 2=水平, 3=垂直
            // 这里使用45度主浮雕方向
            float angle = (float)(Math.PI / 4); // 45度
            float cosA = (float)Math.Cos(angle);
            float sinA = (float)Math.Sin(angle);

            // 基于角度的浮雕核
            float kernelSize = 2f;

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(1, srcHeight - 1, y =>
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            // 计算偏移像素
                            int x1 = (int)(x - cosA * kernelSize);
                            int y1 = (int)(y - sinA * kernelSize);
                            int x2 = (int)(x + cosA * kernelSize);
                            int y2 = (int)(y + sinA * kernelSize);

                            // 边界检查
                            x1 = Math.Clamp(x1, 0, srcWidth - 1);
                            y1 = Math.Clamp(y1, 0, srcHeight - 1);
                            x2 = Math.Clamp(x2, 0, srcWidth - 1);
                            y2 = Math.Clamp(y2, 0, srcHeight - 1);

                            byte* pixel1Row = (byte*)srcPixels + (y1 * srcRowBytes);
                            byte* pixel2Row = (byte*)srcPixels + (y2 * srcRowBytes);

                            int x1_4 = x1 * 4;
                            int x2_4 = x2 * 4;

                            byte p1b = pixel1Row[x1_4];
                            byte p1g = pixel1Row[x1_4 + 1];
                            byte p1r = pixel1Row[x1_4 + 2];

                            byte p2b = pixel2Row[x2_4];
                            byte p2g = pixel2Row[x2_4 + 1];
                            byte p2r = pixel2Row[x2_4 + 2];

                            // 计算差值
                            float r = (p2r - p1r) * embossStrength;
                            float g = (p2g - p1g) * embossStrength;
                            float b = (p2b - p1b) * embossStrength;

                            // 添加128偏移
                            r += 128;
                            g += 128;
                            b += 128;

                            // 增强对比度使浮雕更明显
                            if (strength > 0.5f)
                            {
                                float contrast = 1 + strength;
                                r = (r - 128) * contrast + 128;
                                g = (g - 128) * contrast + 128;
                                b = (b - 128) * contrast + 128;
                            }

                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 1; y < srcHeight - 1; y++)
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            int x1 = (int)(x - cosA * kernelSize);
                            int y1 = (int)(y - sinA * kernelSize);
                            int x2 = (int)(x + cosA * kernelSize);
                            int y2 = (int)(y + sinA * kernelSize);

                            x1 = Math.Clamp(x1, 0, srcWidth - 1);
                            y1 = Math.Clamp(y1, 0, srcHeight - 1);
                            x2 = Math.Clamp(x2, 0, srcWidth - 1);
                            y2 = Math.Clamp(y2, 0, srcHeight - 1);

                            byte* pixel1Row = (byte*)srcPixels + (y1 * srcRowBytes);
                            byte* pixel2Row = (byte*)srcPixels + (y2 * srcRowBytes);

                            int x1_4 = x1 * 4;
                            int x2_4 = x2 * 4;

                            byte p1b = pixel1Row[x1_4];
                            byte p1g = pixel1Row[x1_4 + 1];
                            byte p1r = pixel1Row[x1_4 + 2];

                            byte p2b = pixel2Row[x2_4];
                            byte p2g = pixel2Row[x2_4 + 1];
                            byte p2r = pixel2Row[x2_4 + 2];

                            float r = (p2r - p1r) * embossStrength;
                            float g = (p2g - p1g) * embossStrength;
                            float b = (p2b - p1b) * embossStrength;

                            r += 128;
                            g += 128;
                            b += 128;

                            if (strength > 0.5f)
                            {
                                float contrast = 1 + strength;
                                r = (r - 128) * contrast + 128;
                                g = (g - 128) * contrast + 128;
                                b = (b - 128) * contrast + 128;
                            }

                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }

                // 边缘保持不变 - 使用并行处理
                var destPixels = destBitmap.GetPixels();

                Parallel.For(0, srcHeight, y =>
                {
                    byte* destRow = (byte*)destPixels + (y * destRowBytes);
                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                    // 左边缘
                    destRow[0] = srcRow[0];
                    destRow[1] = srcRow[1];
                    destRow[2] = srcRow[2];
                    destRow[3] = srcRow[3];

                    // 右边缘
                    int rightX = (srcWidth - 1) * 4;
                    destRow[rightX] = srcRow[rightX];
                    destRow[rightX + 1] = srcRow[rightX + 1];
                    destRow[rightX + 2] = srcRow[rightX + 2];
                    destRow[rightX + 3] = srcRow[rightX + 3];
                });

                Parallel.For(0, srcWidth, x =>
                {
                    int x4 = x * 4;
                    // 上边缘
                    byte* srcRowTop = (byte*)srcPixels;
                    byte* destRowTop = (byte*)destPixels;
                    destRowTop[x4] = srcRowTop[x4];
                    destRowTop[x4 + 1] = srcRowTop[x4 + 1];
                    destRowTop[x4 + 2] = srcRowTop[x4 + 2];
                    destRowTop[x4 + 3] = srcRowTop[x4 + 3];

                    // 下边缘
                    byte* srcRowBottom = (byte*)srcPixels + ((srcHeight - 1) * srcRowBytes);
                    byte* destRowBottom = (byte*)destPixels + ((srcHeight - 1) * destRowBytes);
                    destRowBottom[x4] = srcRowBottom[x4];
                    destRowBottom[x4 + 1] = srcRowBottom[x4 + 1];
                    destRowBottom[x4 + 2] = srcRowBottom[x4 + 2];
                    destRowBottom[x4 + 3] = srcRowBottom[x4 + 3];
                });
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建漫画效果的图像
        /// 特点：高对比度 + 边缘描边 + 色彩量化
        /// </summary>
        public static Bitmap CreateComicBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 预处理：先进行轻微模糊减少噪点
            using var blurredBitmap = new SKBitmap(srcWidth, srcHeight);
            using var blurPaint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateBlur(0.5f, 0.5f)
            };
            using var blurCanvas = new SKCanvas(blurredBitmap);
            blurCanvas.DrawBitmap(srcBitmap, 0, 0, blurPaint);

            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 参数计算
            float strength = intensity / 100f; // 0-1
            int colorLevels = (int)(8 - strength * 6); // 8 -> 2 级
            colorLevels = Math.Max(2, colorLevels);
            float colorStep = 256f / colorLevels;

            // 边缘检测阈值
            float edgeThreshold = 30 + (1 - strength) * 70; // 30-100

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = blurredBitmap.GetPixels();
                int srcRowBytes = blurredBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(1, srcHeight - 1, y =>
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            int x4 = x * 4;

                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            // 1. 边缘检测 (Sobel算子)
                            // 标准Sobel核: X方向 [[-1,0,1],[-2,0,2],[-1,0,1]], Y方向 [[-1,-2,-1],[0,0,0],[1,2,1]]
                            // 获取周围8个像素的灰度值 (按正确顺序填充)
                            float topLeft = GetGrayPixel(srcPixels, x - 1, y - 1, srcRowBytes);
                            float top = GetGrayPixel(srcPixels, x, y - 1, srcRowBytes);
                            float topRight = GetGrayPixel(srcPixels, x + 1, y - 1, srcRowBytes);
                            float left = GetGrayPixel(srcPixels, x - 1, y, srcRowBytes);
                            float right = GetGrayPixel(srcPixels, x + 1, y, srcRowBytes);
                            float bottomLeft = GetGrayPixel(srcPixels, x - 1, y + 1, srcRowBytes);
                            float bottom = GetGrayPixel(srcPixels, x, y + 1, srcRowBytes);
                            float bottomRight = GetGrayPixel(srcPixels, x + 1, y + 1, srcRowBytes);

                            // Sobel X核: [[-1,0,1],[-2,0,2],[-1,0,1]]
                            float gx = topLeft * -1 + topRight * 1 + left * -2 + right * 2 + bottomLeft * -1 + bottomRight * 1;

                            // Sobel Y核: [[-1,-2,-1],[0,0,0],[1,2,1]]
                            float gy = topLeft * -1 + top * -2 + topRight * -1 + bottomLeft * 1 + bottom * 2 + bottomRight * 1;

                            float edge = Math.Abs(gx) + Math.Abs(gy);
                            bool isEdge = edge > edgeThreshold;

                            // 2. 色彩量化
                            byte r = (byte)((int)(srcR / colorStep) * colorStep);
                            byte g = (byte)((int)(srcG / colorStep) * colorStep);
                            byte b = (byte)((int)(srcB / colorStep) * colorStep);

                            // 3. 增加对比度
                            r = (byte)Math.Clamp((r - 128) * (1.2f + strength * 0.5f) + 128, 0, 255);
                            g = (byte)Math.Clamp((g - 128) * (1.2f + strength * 0.5f) + 128, 0, 255);
                            b = (byte)Math.Clamp((b - 128) * (1.2f + strength * 0.5f) + 128, 0, 255);

                            // 4. 如果是边缘，画黑色描边
                            if (isEdge)
                            {
                                r = (byte)(r * 0.3f);
                                g = (byte)(g * 0.3f);
                                b = (byte)(b * 0.3f);
                            }

                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 1; y < srcHeight - 1; y++)
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            int x4 = x * 4;

                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float gx = 0, gy = 0;

                            // Sobel X核
                            gx += GetGrayPixel(srcPixels, x - 1, y - 1, srcRowBytes) * -1;
                            gx += GetGrayPixel(srcPixels, x - 1, y + 1, srcRowBytes) * -1;
                            gx += GetGrayPixel(srcPixels, x + 1, y - 1, srcRowBytes) * 1;
                            gx += GetGrayPixel(srcPixels, x + 1, y + 1, srcRowBytes) * 1;
                            gx += GetGrayPixel(srcPixels, x - 1, y, srcRowBytes) * -2;
                            gx += GetGrayPixel(srcPixels, x + 1, y, srcRowBytes) * 2;
                            // Sobel Y核
                            gy += GetGrayPixel(srcPixels, x - 1, y - 1, srcRowBytes) * -1;
                            gy += GetGrayPixel(srcPixels, x, y - 1, srcRowBytes) * -2;
                            gy += GetGrayPixel(srcPixels, x + 1, y - 1, srcRowBytes) * -1;
                            gy += GetGrayPixel(srcPixels, x - 1, y + 1, srcRowBytes) * 1;
                            gy += GetGrayPixel(srcPixels, x, y + 1, srcRowBytes) * 2;
                            gy += GetGrayPixel(srcPixels, x + 1, y + 1, srcRowBytes) * 1;

                            float edge = (float)Math.Sqrt(gx * gx + gy * gy);
                            bool isEdge = edge > edgeThreshold;

                            byte r = (byte)((int)(srcR / colorStep) * colorStep);
                            byte g = (byte)((int)(srcG / colorStep) * colorStep);
                            byte b = (byte)((int)(srcB / colorStep) * colorStep);

                            r = (byte)Math.Clamp((r - 128) * (1.2f + strength * 0.5f) + 128, 0, 255);
                            g = (byte)Math.Clamp((g - 128) * (1.2f + strength * 0.5f) + 128, 0, 255);
                            b = (byte)Math.Clamp((b - 128) * (1.2f + strength * 0.5f) + 128, 0, 255);

                            if (isEdge)
                            {
                                r = (byte)(r * 0.3f);
                                g = (byte)(g * 0.3f);
                                b = (byte)(b * 0.3f);
                            }

                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }

                // 边缘像素保持原样 - 使用并行处理
                var destPixels = destBitmap.GetPixels();

                Parallel.For(0, srcHeight, y =>
                {
                    byte* destRow = (byte*)destPixels + (y * destRowBytes);
                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                    // 左边缘
                    destRow[0] = srcRow[0];
                    destRow[1] = srcRow[1];
                    destRow[2] = srcRow[2];
                    destRow[3] = srcRow[3];

                    // 右边缘
                    int rightX = (srcWidth - 1) * 4;
                    destRow[rightX] = srcRow[rightX];
                    destRow[rightX + 1] = srcRow[rightX + 1];
                    destRow[rightX + 2] = srcRow[rightX + 2];
                    destRow[rightX + 3] = srcRow[rightX + 3];
                });

                Parallel.For(0, srcWidth, x =>
                {
                    int x4 = x * 4;
                    // 上边缘
                    byte* srcRowTop = (byte*)srcPixels;
                    byte* destRowTop = (byte*)destPixels;
                    destRowTop[x4] = srcRowTop[x4];
                    destRowTop[x4 + 1] = srcRowTop[x4 + 1];
                    destRowTop[x4 + 2] = srcRowTop[x4 + 2];
                    destRowTop[x4 + 3] = srcRowTop[x4 + 3];

                    // 下边缘
                    byte* srcRowBottom = (byte*)srcPixels + ((srcHeight - 1) * srcRowBytes);
                    byte* destRowBottom = (byte*)destPixels + ((srcHeight - 1) * destRowBytes);
                    destRowBottom[x4] = srcRowBottom[x4];
                    destRowBottom[x4 + 1] = srcRowBottom[x4 + 1];
                    destRowBottom[x4 + 2] = srcRowBottom[x4 + 2];
                    destRowBottom[x4 + 3] = srcRowBottom[x4 + 3];
                });
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 辅助方法：获取像素灰度值（用于漫画效果）
        /// </summary>
        private static unsafe float GetGrayPixel(IntPtr pixels, int x, int y, int rowBytes)
        {
            byte* row = (byte*)pixels + (y * rowBytes);
            int x4 = x * 4;
            return row[x4 + 2] * 0.299f + row[x4 + 1] * 0.587f + row[x4] * 0.114f;
        }

        /// <summary>
        /// 获取像素的灰度值
        /// </summary>
        private static float GetGray(SKColor pixel)
        {
            return pixel.Red * 0.299f + pixel.Green * 0.587f + pixel.Blue * 0.114f;
        }

        /// <summary>
        /// 创建边缘检测效果的图像
        /// </summary>
        public static Bitmap CreateEdgeDetectionBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float strength = intensity / 100f * 2f;

            // Sobel 边缘检测卷积核
            float[,] kernelX = {
                {-1, 0, 1},
                {-2, 0, 2},
                {-1, 0, 1}
            };

            float[,] kernelY = {
                {-1, -2, -1},
                {0, 0, 0},
                {1, 2, 1}
            };

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(1, srcHeight - 1, y =>
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            float gx = 0, gy = 0;

                            for (int ky = -1; ky <= 1; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -1; kx <= 1; kx++)
                                {
                                    int nx4 = (x + kx) * 4;
                                    float gray = (neighborRow[nx4 + 2] + neighborRow[nx4 + 1] + neighborRow[nx4]) / 3f;
                                    gx += gray * kernelX[ky + 1, kx + 1];
                                    gy += gray * kernelY[ky + 1, kx + 1];
                                }
                            }

                            float magnitude = (float)Math.Sqrt(gx * gx + gy * gy) * strength;
                            byte value = (byte)Math.Clamp(magnitude, 0, 255);

                            int x4 = x * 4;
                            byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                            destRow[x4] = value;
                            destRow[x4 + 1] = value;
                            destRow[x4 + 2] = value;
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 1; y < srcHeight - 1; y++)
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            float gx = 0, gy = 0;

                            for (int ky = -1; ky <= 1; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -1; kx <= 1; kx++)
                                {
                                    int nx4 = (x + kx) * 4;
                                    float gray = (neighborRow[nx4 + 2] + neighborRow[nx4 + 1] + neighborRow[nx4]) / 3f;
                                    gx += gray * kernelX[ky + 1, kx + 1];
                                    gy += gray * kernelY[ky + 1, kx + 1];
                                }
                            }

                            float magnitude = (float)Math.Sqrt(gx * gx + gy * gy) * strength;
                            byte value = (byte)Math.Clamp(magnitude, 0, 255);

                            int x4 = x * 4;
                            byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                            destRow[x4] = value;
                            destRow[x4 + 1] = value;
                            destRow[x4 + 2] = value;
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }

                // 边缘保持不变 - 使用并行处理
                var destPixels = destBitmap.GetPixels();

                Parallel.For(0, srcHeight, y =>
                {
                    byte* destRow = (byte*)destPixels + (y * destRowBytes);
                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);

                    // 左边缘
                    destRow[0] = srcRow[0];
                    destRow[1] = srcRow[1];
                    destRow[2] = srcRow[2];
                    destRow[3] = srcRow[3];

                    // 右边缘
                    int rightX = (srcWidth - 1) * 4;
                    destRow[rightX] = srcRow[rightX];
                    destRow[rightX + 1] = srcRow[rightX + 1];
                    destRow[rightX + 2] = srcRow[rightX + 2];
                    destRow[rightX + 3] = srcRow[rightX + 3];
                });

                Parallel.For(0, srcWidth, x =>
                {
                    int x4 = x * 4;
                    // 上边缘
                    byte* srcRowTop = (byte*)srcPixels;
                    byte* destRowTop = (byte*)destPixels;
                    destRowTop[x4] = srcRowTop[x4];
                    destRowTop[x4 + 1] = srcRowTop[x4 + 1];
                    destRowTop[x4 + 2] = srcRowTop[x4 + 2];
                    destRowTop[x4 + 3] = srcRowTop[x4 + 3];

                    // 下边缘
                    byte* srcRowBottom = (byte*)srcPixels + ((srcHeight - 1) * srcRowBytes);
                    byte* destRowBottom = (byte*)destPixels + ((srcHeight - 1) * destRowBytes);
                    destRowBottom[x4] = srcRowBottom[x4];
                    destRowBottom[x4 + 1] = srcRowBottom[x4 + 1];
                    destRowBottom[x4 + 2] = srcRowBottom[x4 + 2];
                    destRowBottom[x4 + 3] = srcRowBottom[x4 + 3];
                });
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建油画效果的图像
        /// </summary>
        public static Bitmap CreateOilPaintBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            int radius = Math.Max(1, intensity / 20);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                // 先复制源图像到目标（包括边缘）
                Buffer.MemoryCopy((void*)srcPixels, (void*)destBitmap.GetPixels(), destBitmap.RowBytes * srcHeight, srcRowBytes * srcHeight);

                var destPixels = destBitmap.GetPixels();

                if (srcHeight >= 100)
                {
                    Parallel.For(radius, srcHeight - radius, y =>
                    {
                        int[] histogramR = new int[256];
                        int[] histogramG = new int[256];
                        int[] histogramB = new int[256];
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = radius; x < srcWidth - radius; x++)
                        {
                            // 高效初始化 histogram，避免 Array.Clear 的开销
                            for (int i = 0; i < 256; i++)
                            {
                                histogramR[i] = 0;
                                histogramG[i] = 0;
                                histogramB[i] = 0;
                            }

                            for (int ky = -radius; ky <= radius; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -radius; kx <= radius; kx++)
                                {
                                    int nx4 = (x + kx) * 4;
                                    histogramR[neighborRow[nx4 + 2]]++;
                                    histogramG[neighborRow[nx4 + 1]]++;
                                    histogramB[neighborRow[nx4]]++;
                                }
                            }

                            int maxCountR = 0, maxCountG = 0, maxCountB = 0;
                            byte r = 0, g = 0, b = 0;

                            for (int i = 0; i < 256; i++)
                            {
                                if (histogramR[i] > maxCountR) { maxCountR = histogramR[i]; r = (byte)i; }
                                if (histogramG[i] > maxCountG) { maxCountG = histogramG[i]; g = (byte)i; }
                                if (histogramB[i] > maxCountB) { maxCountB = histogramB[i]; b = (byte)i; }
                            }

                            int x4 = x * 4;
                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                        }
                    });
                }
                else
                {
                    int[] histogramR = new int[256];
                    int[] histogramG = new int[256];
                    int[] histogramB = new int[256];

                    for (int y = radius; y < srcHeight - radius; y++)
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = radius; x < srcWidth - radius; x++)
                        {
                            // 高效初始化 histogram，避免 Array.Clear 的开销
                            for (int i = 0; i < 256; i++)
                            {
                                histogramR[i] = 0;
                                histogramG[i] = 0;
                                histogramB[i] = 0;
                            }

                            for (int ky = -radius; ky <= radius; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -radius; kx <= radius; kx++)
                                {
                                    int nx4 = (x + kx) * 4;
                                    histogramR[neighborRow[nx4 + 2]]++;
                                    histogramG[neighborRow[nx4 + 1]]++;
                                    histogramB[neighborRow[nx4]]++;
                                }
                            }

                            // 优化为单次遍历
                            int maxCountR = 0, maxCountG = 0, maxCountB = 0;
                            byte r = 0, g = 0, b = 0;

                            for (int i = 0; i < 256; i++)
                            {
                                if (histogramR[i] > maxCountR) { maxCountR = histogramR[i]; r = (byte)i; }
                                if (histogramG[i] > maxCountG) { maxCountG = histogramG[i]; g = (byte)i; }
                                if (histogramB[i] > maxCountB) { maxCountB = histogramB[i]; b = (byte)i; }
                            }

                            int x4 = x * 4;
                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建水彩效果的图像
        /// </summary>
        public static Bitmap CreateWatercolorBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            float strength = intensity / 100f;
            int radius = Math.Max(3, (int)(strength * 5)); // 模糊半径 3-8
            float sigma = radius / 3f;

            // 步骤1: 双边滤波 - 边缘保留模糊
            using var blurredBitmap = new SKBitmap(srcWidth, srcHeight);

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                var destPixels = blurredBitmap.GetPixels();
                int destRowBytes = blurredBitmap.RowBytes;

                // 预计算空间高斯权重
                float[] spatialWeight = new float[(radius * 2 + 1) * (radius * 2 + 1)];
                int initIdx = 0;
                for (int ky = -radius; ky <= radius; ky++)
                {
                    for (int kx = -radius; kx <= radius; kx++)
                    {
                        float dist = kx * kx + ky * ky;
                        spatialWeight[initIdx++] = MathF.Exp(-dist / (2 * sigma * sigma));
                    }
                }

                if (srcHeight >= 100)
                {
                    Parallel.For(radius, srcHeight - radius, y =>
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = radius; x < srcWidth - radius; x++)
                        {
                            float sumR = 0, sumG = 0, sumB = 0;
                            float weightSum = 0;

                            // 中心像素颜色（用于范围权重）
                            int x4 = x * 4;
                            byte* centerRow = (byte*)srcPixels + (y * srcRowBytes);
                            float centerR = centerRow[x4 + 2];
                            float centerG = centerRow[x4 + 1];
                            float centerB = centerRow[x4];

                            int spatialIdx = 0;
                            for (int ky = -radius; ky <= radius; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -radius; kx <= radius; kx++)
                                {
                                    int nx4 = (x + kx) * 4;
                                    float neighborR = neighborRow[nx4 + 2];
                                    float neighborG = neighborRow[nx4 + 1];
                                    float neighborB = neighborRow[nx4];

                                    // 颜色差异（范围权重）
                                    float colorDiffR = neighborR - centerR;
                                    float colorDiffG = neighborG - centerG;
                                    float colorDiffB = neighborB - centerB;
                                    float colorDist = MathF.Sqrt(colorDiffR * colorDiffR + colorDiffG * colorDiffG + colorDiffB * colorDiffB);
                                    float rangeWeight = MathF.Exp(-colorDist * colorDist / (2 * 30f * 30f));

                                    float weight = spatialWeight[spatialIdx++] * rangeWeight;

                                    sumR += neighborR * weight;
                                    sumG += neighborG * weight;
                                    sumB += neighborB * weight;
                                    weightSum += weight;
                                }
                            }

                            destRow[x4] = (byte)(sumB / weightSum);
                            destRow[x4 + 1] = (byte)(sumG / weightSum);
                            destRow[x4 + 2] = (byte)(sumR / weightSum);
                            destRow[x4 + 3] = centerRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = radius; y < srcHeight - radius; y++)
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = radius; x < srcWidth - radius; x++)
                        {
                            float sumR = 0, sumG = 0, sumB = 0;
                            float weightSum = 0;

                            int x4 = x * 4;
                            byte* centerRow = (byte*)srcPixels + (y * srcRowBytes);
                            float centerR = centerRow[x4 + 2];
                            float centerG = centerRow[x4 + 1];
                            float centerB = centerRow[x4];

                            int spatialIdx = 0;
                            for (int ky = -radius; ky <= radius; ky++)
                            {
                                byte* neighborRow = (byte*)srcPixels + ((y + ky) * srcRowBytes);
                                for (int kx = -radius; kx <= radius; kx++)
                                {
                                    int nx4 = (x + kx) * 4;
                                    float neighborR = neighborRow[nx4 + 2];
                                    float neighborG = neighborRow[nx4 + 1];
                                    float neighborB = neighborRow[nx4];

                                    float colorDiffR = neighborR - centerR;
                                    float colorDiffG = neighborG - centerG;
                                    float colorDiffB = neighborB - centerB;
                                    float colorDist = MathF.Sqrt(colorDiffR * colorDiffR + colorDiffG * colorDiffG + colorDiffB * colorDiffB);
                                    float rangeWeight = MathF.Exp(-colorDist * colorDist / (2 * 30f * 30f));

                                    float weight = spatialWeight[spatialIdx++] * rangeWeight;

                                    sumR += neighborR * weight;
                                    sumG += neighborG * weight;
                                    sumB += neighborB * weight;
                                    weightSum += weight;
                                }
                            }

                            destRow[x4] = (byte)(sumB / weightSum);
                            destRow[x4 + 1] = (byte)(sumG / weightSum);
                            destRow[x4 + 2] = (byte)(sumR / weightSum);
                            destRow[x4 + 3] = centerRow[x4 + 3];
                        }
                    }
                }
            }

            // 步骤2: 饱和度增强 + 湿边效果 + 轻微对比度
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 饱和度因子 (水彩通常色彩鲜艳)
            float saturationBoost = 1.0f + strength * 0.8f;
            // 对比度微调
            float contrastFactor = 1.0f + strength * 0.15f;
            // 湿边强度
            float wetEdgeStrength = strength * 0.3f;

            unsafe
            {
                var srcPixels = blurredBitmap.GetPixels();
                int srcRowBytes = blurredBitmap.RowBytes;
                var destPixels = destBitmap.GetPixels();
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(1, srcHeight - 1, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);
                        byte* prevRow = (byte*)srcPixels + ((y - 1) * srcRowBytes);
                        byte* nextRow = (byte*)srcPixels + ((y + 1) * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            int x4 = x * 4;

                            float srcB = srcRow[x4];
                            float srcG = srcRow[x4 + 1];
                            float srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            // 计算湿边效果 - 检测边缘，边缘处加深颜色
                            // 使用拉普拉斯算子检测边缘
                            float lapB = prevRow[x4] + nextRow[x4] + srcRow[x4 - 4] + srcRow[x4 + 4] - 4 * srcB;
                            float lapG = prevRow[x4 + 1] + nextRow[x4 + 1] + srcRow[x4 - 3] + srcRow[x4 + 3] - 4 * srcG;
                            float lapR = prevRow[x4 + 2] + nextRow[x4 + 2] + srcRow[x4 - 2] + srcRow[x4 + 2] - 4 * srcR;
                            float edgeStrength = MathF.Sqrt(lapR * lapR + lapG * lapG + lapB * lapB);

                            // 边缘处加深颜色（湿边效果）
                            float wetEdge = Math.Min(1f, edgeStrength / 50f) * wetEdgeStrength;

                            // 饱和度增强 (使用 HSL 变换的简化版本)
                            float gray = srcR * 0.299f + srcG * 0.587f + srcB * 0.114f;
                            float r = gray + (srcR - gray) * saturationBoost;
                            float g = gray + (srcG - gray) * saturationBoost;
                            float b = gray + (srcB - gray) * saturationBoost;

                            // 应用湿边（边缘变暗）
                            r *= (1f - wetEdge);
                            g *= (1f - wetEdge);
                            b *= (1f - wetEdge);

                            // 轻微对比度调整
                            r = (r - 128) * contrastFactor + 128;
                            g = (g - 128) * contrastFactor + 128;
                            b = (b - 128) * contrastFactor + 128;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 1; y < srcHeight - 1; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);
                        byte* prevRow = (byte*)srcPixels + ((y - 1) * srcRowBytes);
                        byte* nextRow = (byte*)srcPixels + ((y + 1) * srcRowBytes);

                        for (int x = 1; x < srcWidth - 1; x++)
                        {
                            int x4 = x * 4;

                            float srcB = srcRow[x4];
                            float srcG = srcRow[x4 + 1];
                            float srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float lapB = prevRow[x4] + nextRow[x4] + srcRow[x4 - 4] + srcRow[x4 + 4] - 4 * srcB;
                            float lapG = prevRow[x4 + 1] + nextRow[x4 + 1] + srcRow[x4 - 3] + srcRow[x4 + 3] - 4 * srcG;
                            float lapR = prevRow[x4 + 2] + nextRow[x4 + 2] + srcRow[x4 - 2] + srcRow[x4 + 2] - 4 * srcR;
                            float edgeStrength = MathF.Sqrt(lapR * lapR + lapG * lapG + lapB * lapB);

                            float wetEdge = Math.Min(1f, edgeStrength / 50f) * wetEdgeStrength;

                            float gray = srcR * 0.299f + srcG * 0.587f + srcB * 0.114f;
                            float r = gray + (srcR - gray) * saturationBoost;
                            float g = gray + (srcG - gray) * saturationBoost;
                            float b = gray + (srcB - gray) * saturationBoost;

                            r *= (1f - wetEdge);
                            g *= (1f - wetEdge);
                            b *= (1f - wetEdge);

                            r = (r - 128) * contrastFactor + 128;
                            g = (g - 128) * contrastFactor + 128;
                            b = (b - 128) * contrastFactor + 128;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建素描效果的图像
        /// </summary>
        public static Bitmap CreateSketchBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);

            // 计算灰度 - 使用并行处理
            using var grayBitmap = new SKBitmap(srcWidth, srcHeight);

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int grayRowBytes = grayBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* grayRow = (byte*)grayBitmap.GetPixels() + (y * grayRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte gray = (byte)((srcRow[x4 + 2] * 0.299 + srcRow[x4 + 1] * 0.587 + srcRow[x4] * 0.114));
                            grayRow[x4] = gray;
                            grayRow[x4 + 1] = gray;
                            grayRow[x4 + 2] = gray;
                            grayRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* grayRow = (byte*)grayBitmap.GetPixels() + (y * grayRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte gray = (byte)((srcRow[x4 + 2] * 0.299 + srcRow[x4 + 1] * 0.587 + srcRow[x4] * 0.114));
                            grayRow[x4] = gray;
                            grayRow[x4 + 1] = gray;
                            grayRow[x4 + 2] = gray;
                            grayRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            // 高斯模糊
            float blurSigma = 2.0f;
            using var blurFilter = SKImageFilter.CreateBlur(blurSigma, blurSigma);
            using var blurPaint = new SKPaint { ImageFilter = blurFilter };
            using var blurredBitmap = new SKBitmap(srcWidth, srcHeight);
            using var blurCanvas = new SKCanvas(blurredBitmap);
            blurCanvas.DrawBitmap(grayBitmap, 0, 0, blurPaint);

            // 计算素描效果（灰度反转 + 混合）- 使用并行处理
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);
            float sketchStrength = intensity / 100f;

            unsafe
            {
                var grayPixels = grayBitmap.GetPixels();
                var blurredPixels = blurredBitmap.GetPixels();
                int grayRowBytes = grayBitmap.RowBytes;
                int blurredRowBytes = blurredBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* grayRow = (byte*)grayPixels + (y * grayRowBytes);
                        byte* blurredRow = (byte*)blurredPixels + (y * blurredRowBytes);
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;

                            // 反转模糊后的灰度（这是素描轮廓线的基础）
                            byte inverted = (byte)(255 - blurredRow[x4]);

                            // 使用颜色减淡 (Color Dodge) 混合模式实现真正的素描效果
                            // result = base / (255 - blend)，如果 blend 为 255 则结果为白色
                            float baseValue = grayRow[x4];
                            float blendValue = inverted;

                            byte value;
                            if (blendValue >= 255)
                            {
                                value = 255; // 防止除零
                            }
                            else
                            {
                                // Color Dodge: 原始灰度除以 (255 - 反转模糊)，产生素描轮廓效果
                                float dodgeResult = baseValue * 255f / (255f - blendValue);
                                // 根据强度混合
                                dodgeResult = Math.Clamp(dodgeResult, 0, 255);
                                value = (byte)(baseValue * (1 - sketchStrength) + dodgeResult * sketchStrength);
                            }

                            destRow[x4] = value;
                            destRow[x4 + 1] = value;
                            destRow[x4 + 2] = value;
                            destRow[x4 + 3] = grayRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* grayRow = (byte*)grayPixels + (y * grayRowBytes);
                        byte* blurredRow = (byte*)blurredPixels + (y * blurredRowBytes);
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;

                            // 反转模糊后的灰度
                            byte inverted = (byte)(255 - blurredRow[x4]);

                            // 使用颜色减淡 (Color Dodge) 混合模式
                            float baseValue = grayRow[x4];
                            float blendValue = inverted;

                            byte value;
                            if (blendValue >= 255)
                            {
                                value = 255;
                            }
                            else
                            {
                                float dodgeResult = baseValue * 255f / (255f - blendValue);
                                dodgeResult = Math.Clamp(dodgeResult, 0, 255);
                                value = (byte)(baseValue * (1 - sketchStrength) + dodgeResult * sketchStrength);
                            }

                            destRow[x4] = value;
                            destRow[x4 + 1] = value;
                            destRow[x4 + 2] = value;
                            destRow[x4 + 3] = grayRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建马赛克效果的图像
        /// </summary>
        public static Bitmap CreateMosaicBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            // 马赛克块大小
            int blockSize = Math.Max(2, intensity / 10);

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                // 预计算所有块的平均颜色
                int blockCountX = (srcWidth + blockSize - 1) / blockSize;
                int blockCountY = (srcHeight + blockSize - 1) / blockSize;
                var blockColors = new (int rSum, int gSum, int bSum, int count)[blockCountX, blockCountY];

                // 先并行计算所有块的平均颜色
                Parallel.For(0, blockCountY, by =>
                {
                    for (int bx = 0; bx < blockCountX; bx++)
                    {
                        int rSum = 0, gSum = 0, bSum = 0, count = 0;

                        int startX = bx * blockSize;
                        int startY = by * blockSize;

                        for (int y = startY; y < startY + blockSize && y < srcHeight; y++)
                        {
                            byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                            for (int x = startX; x < startX + blockSize && x < srcWidth; x++)
                            {
                                int x4 = x * 4;
                                rSum += srcRow[x4 + 2];
                                gSum += srcRow[x4 + 1];
                                bSum += srcRow[x4];
                                count++;
                            }
                        }

                        blockColors[bx, by] = (rSum, gSum, bSum, count);
                    }
                });

                // 然后并行填充马赛克块
                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        int blockY = y / blockSize;

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int blockX = x / blockSize;
                            var (rSum, gSum, bSum, count) = blockColors[blockX, blockY];

                            byte r = count > 0 ? (byte)(rSum / count) : (byte)0;
                            byte g = count > 0 ? (byte)(gSum / count) : (byte)0;
                            byte b = count > 0 ? (byte)(bSum / count) : (byte)0;

                            int x4 = x * 4;
                            byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);
                        int blockY = y / blockSize;

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int blockX = x / blockSize;
                            var (rSum, gSum, bSum, count) = blockColors[blockX, blockY];

                            byte r = count > 0 ? (byte)(rSum / count) : (byte)0;
                            byte g = count > 0 ? (byte)(gSum / count) : (byte)0;
                            byte b = count > 0 ? (byte)(bSum / count) : (byte)0;

                            int x4 = x * 4;
                            byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                            destRow[x4] = b;
                            destRow[x4 + 1] = g;
                            destRow[x4 + 2] = r;
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建鱼眼效果的图像
        /// </summary>
        public static Bitmap CreateFishEyeBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float centerX = srcWidth / 2f;
            float centerY = srcHeight / 2f;
            float radius = Math.Min(centerX, centerY);
            float distortion = intensity / 100f * 2f;

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            // 计算到中心的距离
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (dist < radius)
                            {
                                // 计算鱼眼扭曲后的坐标
                                float normalizedDist = dist / radius;
                                float distortedDist = normalizedDist * (1 + distortion * (1 - normalizedDist * normalizedDist));

                                float newX = centerX + (dx / dist) * distortedDist * radius;
                                float newY = centerY + (dy / dist) * distortedDist * radius;

                                // 确保坐标在范围内
                                if (newX >= 0 && newX < srcWidth && newY >= 0 && newY < srcHeight)
                                {
                                    // 双线性插值
                                    int x1 = (int)Math.Floor(newX);
                                    int y1 = (int)Math.Floor(newY);
                                    int x2 = Math.Min(x1 + 1, srcWidth - 1);
                                    int y2 = Math.Min(y1 + 1, srcHeight - 1);

                                    float tx = newX - x1;
                                    float ty = newY - y1;

                                    byte* p11Row = (byte*)srcPixels + (y1 * srcRowBytes);
                                    byte* p12Row = (byte*)srcPixels + (y2 * srcRowBytes);

                                    int x1_4 = x1 * 4;
                                    int x2_4 = x2 * 4;

                                    byte r = (byte)(p11Row[x1_4 + 2] * (1 - tx) * (1 - ty) + p11Row[x2_4 + 2] * tx * (1 - ty) + p12Row[x1_4 + 2] * (1 - tx) * ty + p12Row[x2_4 + 2] * tx * ty);
                                    byte g = (byte)(p11Row[x1_4 + 1] * (1 - tx) * (1 - ty) + p11Row[x2_4 + 1] * tx * (1 - ty) + p12Row[x1_4 + 1] * (1 - tx) * ty + p12Row[x2_4 + 1] * tx * ty);
                                    byte b = (byte)(p11Row[x1_4] * (1 - tx) * (1 - ty) + p11Row[x2_4] * tx * (1 - ty) + p12Row[x1_4] * (1 - tx) * ty + p12Row[x2_4] * tx * ty);

                                    int x4 = x * 4;
                                    destRow[x4] = b;
                                    destRow[x4 + 1] = g;
                                    destRow[x4 + 2] = r;
                                    destRow[x4 + 3] = p11Row[x1_4 + 3];
                                }
                                else
                                {
                                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                                    int x4 = x * 4;
                                    destRow[x4] = srcRow[x4];
                                    destRow[x4 + 1] = srcRow[x4 + 1];
                                    destRow[x4 + 2] = srcRow[x4 + 2];
                                    destRow[x4 + 3] = srcRow[x4 + 3];
                                }
                            }
                            else
                            {
                                byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                                int x4 = x * 4;
                                destRow[x4] = srcRow[x4];
                                destRow[x4 + 1] = srcRow[x4 + 1];
                                destRow[x4 + 2] = srcRow[x4 + 2];
                                destRow[x4 + 3] = srcRow[x4 + 3];
                            }
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* destRow = (byte*)destBitmap.GetPixels() + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (dist < radius)
                            {
                                float normalizedDist = dist / radius;
                                float distortedDist = normalizedDist * (1 + distortion * (1 - normalizedDist * normalizedDist));

                                float newX = centerX + (dx / dist) * distortedDist * radius;
                                float newY = centerY + (dy / dist) * distortedDist * radius;

                                if (newX >= 0 && newX < srcWidth && newY >= 0 && newY < srcHeight)
                                {
                                    int x1 = (int)Math.Floor(newX);
                                    int y1 = (int)Math.Floor(newY);
                                    int x2 = Math.Min(x1 + 1, srcWidth - 1);
                                    int y2 = Math.Min(y1 + 1, srcHeight - 1);

                                    float tx = newX - x1;
                                    float ty = newY - y1;

                                    byte* p11Row = (byte*)srcPixels + (y1 * srcRowBytes);
                                    byte* p12Row = (byte*)srcPixels + (y2 * srcRowBytes);

                                    int x1_4 = x1 * 4;
                                    int x2_4 = x2 * 4;

                                    byte r = (byte)(p11Row[x1_4 + 2] * (1 - tx) * (1 - ty) + p11Row[x2_4 + 2] * tx * (1 - ty) + p12Row[x1_4 + 2] * (1 - tx) * ty + p12Row[x2_4 + 2] * tx * ty);
                                    byte g = (byte)(p11Row[x1_4 + 1] * (1 - tx) * (1 - ty) + p11Row[x2_4 + 1] * tx * (1 - ty) + p12Row[x1_4 + 1] * (1 - tx) * ty + p12Row[x2_4 + 1] * tx * ty);
                                    byte b = (byte)(p11Row[x1_4] * (1 - tx) * (1 - ty) + p11Row[x2_4] * tx * (1 - ty) + p12Row[x1_4] * (1 - tx) * ty + p12Row[x2_4] * tx * ty);

                                    int x4 = x * 4;
                                    destRow[x4] = b;
                                    destRow[x4 + 1] = g;
                                    destRow[x4 + 2] = r;
                                    destRow[x4 + 3] = p11Row[x1_4 + 3];
                                }
                                else
                                {
                                    byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                                    int x4 = x * 4;
                                    destRow[x4] = srcRow[x4];
                                    destRow[x4 + 1] = srcRow[x4 + 1];
                                    destRow[x4 + 2] = srcRow[x4 + 2];
                                    destRow[x4 + 3] = srcRow[x4 + 3];
                                }
                            }
                            else
                            {
                                byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                                int x4 = x * 4;
                                destRow[x4] = srcRow[x4];
                                destRow[x4 + 1] = srcRow[x4 + 1];
                                destRow[x4 + 2] = srcRow[x4 + 2];
                                destRow[x4 + 3] = srcRow[x4 + 3];
                            }
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建色彩平衡调整后的图像 - 高性能版本
        /// </summary>
        public static Bitmap CreateColorBalanceBitmap(Bitmap source, int intensity, ColorChannel channel)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            // 直接复制像素数据，避免编码/解码
            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float balanceFactor = intensity / 100f * 0.5f;
            float rOffset = 0, gOffset = 0, bOffset = 0;

            switch (channel)
            {
                case ColorChannel.Red:
                    rOffset = balanceFactor * 255;
                    break;
                case ColorChannel.Green:
                    gOffset = balanceFactor * 255;
                    break;
                case ColorChannel.Blue:
                    bOffset = balanceFactor * 255;
                    break;
            }

            // 使用 unsafe 并行处理
            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR + rOffset;
                            float g = srcG + gOffset;
                            float b = srcB + bOffset;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR + rOffset;
                            float g = srcG + gOffset;
                            float b = srcB + bOffset;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 色彩通道枚举
        /// </summary>
        public enum ColorChannel
        {
            Red,
            Green,
            Blue
        }

        #region 暗角、颗粒、去雾、液化、高光阴影

        /// <summary>
        /// 创建暗角（晕影）效果 - 边缘暗化，突出主体
        /// </summary>
        public static Bitmap CreateVignetteBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float strength = intensity / 100f;
            float centerX = srcWidth / 2f;
            float centerY = srcHeight / 2f;
            float maxDist = (float)Math.Sqrt(centerX * centerX + centerY * centerY);
            float radius = maxDist * (1f - strength * 0.5f);
            float softness = maxDist * 0.3f;

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;

                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            float vignette;
                            if (dist < radius)
                            {
                                vignette = 1f;
                            }
                            else if (dist > radius + softness)
                            {
                                vignette = 0f;
                            }
                            else
                            {
                                vignette = 1f - (dist - radius) / softness;
                            }
                            vignette = Math.Clamp(vignette, 0f, 1f);

                            destRow[x4] = (byte)(srcRow[x4] * vignette);
                            destRow[x4 + 1] = (byte)(srcRow[x4 + 1] * vignette);
                            destRow[x4 + 2] = (byte)(srcRow[x4 + 2] * vignette);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;

                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            float vignette;
                            if (dist < radius)
                            {
                                vignette = 1f;
                            }
                            else if (dist > radius + softness)
                            {
                                vignette = 0f;
                            }
                            else
                            {
                                vignette = 1f - (dist - radius) / softness;
                            }
                            vignette = Math.Clamp(vignette, 0f, 1f);

                            destRow[x4] = (byte)(srcRow[x4] * vignette);
                            destRow[x4 + 1] = (byte)(srcRow[x4 + 1] * vignette);
                            destRow[x4 + 2] = (byte)(srcRow[x4 + 2] * vignette);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建颗粒感效果 - 模拟胶片颗粒
        /// </summary>
        public static Bitmap CreateGrainBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float strength = intensity / 100f;
            float grainAmount = strength * 80f;
            var random = new Random(12345);

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        var randomLocal = new Random(y * 10000 + Environment.TickCount);
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;

                            float noise = ((float)randomLocal.NextDouble() - 0.5f) * 2f * grainAmount;

                            float r = srcRow[x4 + 2] + noise;
                            float g = srcRow[x4 + 1] + noise;
                            float b = srcRow[x4] + noise;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        var randomLocal = new Random(y * 10000 + Environment.TickCount);
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;

                            float noise = ((float)randomLocal.NextDouble() - 0.5f) * 2f * grainAmount;

                            float r = srcRow[x4 + 2] + noise;
                            float g = srcRow[x4 + 1] + noise;
                            float b = srcRow[x4] + noise;

                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = srcRow[x4 + 3];
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建去雾效果 - 提升图片通透度
        /// </summary>
        public static Bitmap CreateDefogBitmap(Bitmap source, int intensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float strength = intensity / 100f;

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR / 255f;
                            float g = srcG / 255f;
                            float b = srcB / 255f;

                            float maxChannel = Math.Max(r, Math.Max(g, b));
                            float minChannel = Math.Min(r, Math.Min(g, b));
                            float luminance = (r + g + b) / 3f;

                            float fogAmount = (1f - maxChannel) * 0.5f + (1f - (maxChannel - minChannel)) * 0.3f;
                            fogAmount = Math.Clamp(fogAmount, 0f, 0.95f);

                            float defog = fogAmount * strength;

                            float r2 = (r - defog * 0.85f) / (1f - defog * 0.85f);
                            float g2 = (g - defog * 0.9f) / (1f - defog * 0.9f);
                            float b2 = (b - defog * 0.95f) / (1f - defog * 0.95f);

                            float contrastBoost = 1f + strength * 0.15f;
                            float center = 0.5f;
                            r2 = center + (r2 - center) * contrastBoost;
                            g2 = center + (g2 - center) * contrastBoost;
                            b2 = center + (b2 - center) * contrastBoost;

                            r2 = Math.Clamp(r2, 0f, 1f);
                            g2 = Math.Clamp(g2, 0f, 1f);
                            b2 = Math.Clamp(b2, 0f, 1f);

                            destRow[x4] = (byte)(b2 * 255);
                            destRow[x4 + 1] = (byte)(g2 * 255);
                            destRow[x4 + 2] = (byte)(r2 * 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR / 255f;
                            float g = srcG / 255f;
                            float b = srcB / 255f;

                            float maxChannel = Math.Max(r, Math.Max(g, b));
                            float minChannel = Math.Min(r, Math.Min(g, b));
                            float luminance = (r + g + b) / 3f;

                            float fogAmount = (1f - maxChannel) * 0.5f + (1f - (maxChannel - minChannel)) * 0.3f;
                            fogAmount = Math.Clamp(fogAmount, 0f, 0.95f);

                            float defog = fogAmount * strength;

                            float r2 = (r - defog * 0.85f) / (1f - defog * 0.85f);
                            float g2 = (g - defog * 0.9f) / (1f - defog * 0.9f);
                            float b2 = (b - defog * 0.95f) / (1f - defog * 0.95f);

                            float contrastBoost = 1f + strength * 0.15f;
                            float center = 0.5f;
                            r2 = center + (r2 - center) * contrastBoost;
                            g2 = center + (g2 - center) * contrastBoost;
                            b2 = center + (b2 - center) * contrastBoost;

                            r2 = Math.Clamp(r2, 0f, 1f);
                            g2 = Math.Clamp(g2, 0f, 1f);
                            b2 = Math.Clamp(b2, 0f, 1f);

                            destRow[x4] = (byte)(b2 * 255);
                            destRow[x4 + 1] = (byte)(g2 * 255);
                            destRow[x4 + 2] = (byte)(r2 * 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 创建高光/阴影调整效果
        /// </summary>
        public static Bitmap CreateHighlightsShadowsBitmap(Bitmap source, int highlightIntensity, int shadowIntensity)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (highlightIntensity == 0 && shadowIntensity == 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float highlightAmount = highlightIntensity / 100f;
            float shadowAmount = shadowIntensity / 100f;

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                var destPixels = destBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                int destRowBytes = destBitmap.RowBytes;

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR / 255f;
                            float g = srcG / 255f;
                            float b = srcB / 255f;

                            float luminance = r * 0.2126f + g * 0.7152f + b * 0.0722f;

                            if (luminance > 0.5f)
                            {
                                float factor = (luminance - 0.5f) * 2f;
                                float adjustment = factor * highlightAmount * 0.5f;
                                r = Math.Clamp(r + adjustment, 0, 1);
                                g = Math.Clamp(g + adjustment, 0, 1);
                                b = Math.Clamp(b + adjustment, 0, 1);
                            }
                            else
                            {
                                float factor = (0.5f - luminance) * 2f;
                                float adjustment = factor * shadowAmount * 0.5f;
                                r = Math.Clamp(r - adjustment, 0, 1);
                                g = Math.Clamp(g - adjustment, 0, 1);
                                b = Math.Clamp(b - adjustment, 0, 1);
                            }

                            destRow[x4] = (byte)(b * 255);
                            destRow[x4 + 1] = (byte)(g * 255);
                            destRow[x4 + 2] = (byte)(r * 255);
                            destRow[x4 + 3] = srcA;
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* srcRow = (byte*)srcPixels + (y * srcRowBytes);
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            int x4 = x * 4;
                            byte srcB = srcRow[x4];
                            byte srcG = srcRow[x4 + 1];
                            byte srcR = srcRow[x4 + 2];
                            byte srcA = srcRow[x4 + 3];

                            float r = srcR / 255f;
                            float g = srcG / 255f;
                            float b = srcB / 255f;

                            float luminance = r * 0.2126f + g * 0.7152f + b * 0.0722f;

                            if (luminance > 0.5f)
                            {
                                float factor = (luminance - 0.5f) * 2f;
                                float adjustment = factor * highlightAmount * 0.5f;
                                r = Math.Clamp(r + adjustment, 0, 1);
                                g = Math.Clamp(g + adjustment, 0, 1);
                                b = Math.Clamp(b + adjustment, 0, 1);
                            }
                            else
                            {
                                float factor = (0.5f - luminance) * 2f;
                                float adjustment = factor * shadowAmount * 0.5f;
                                r = Math.Clamp(r - adjustment, 0, 1);
                                g = Math.Clamp(g - adjustment, 0, 1);
                                b = Math.Clamp(b - adjustment, 0, 1);
                            }

                            destRow[x4] = (byte)(b * 255);
                            destRow[x4 + 1] = (byte)(g * 255);
                            destRow[x4 + 2] = (byte)(r * 255);
                            destRow[x4 + 3] = srcA;
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        /// <summary>
        /// 液化变形类型
        /// </summary>
        public enum LiquifyType
        {
            Pinch,      // 收缩/放大
            Stretch,    // 拉伸
            ShiftLeft,  // 左推
            ShiftRight, // 右推
        }

        /// <summary>
        /// 创建液化效果 - 局部变形（人像瘦脸/大眼）
        /// </summary>
        public static Bitmap CreateLiquifyBitmap(Bitmap source, int centerX, int centerY, int radius, int intensity, LiquifyType type)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (intensity == 0 || radius <= 0)
                return CloneBitmap(source);

            var srcWidth = source.PixelSize.Width;
            var srcHeight = source.PixelSize.Height;

            using var srcBitmap = AvaloniaToSkBitmap(source);
            using var destBitmap = new SKBitmap(srcWidth, srcHeight);

            float strength = intensity / 100f;
            float actualRadius = radius;

            unsafe
            {
                var srcPixels = srcBitmap.GetPixels();
                int srcRowBytes = srcBitmap.RowBytes;
                var destPixels = destBitmap.GetPixels();
                int destRowBytes = destBitmap.RowBytes;

                Buffer.MemoryCopy((void*)srcPixels, (void*)destPixels,
                    destBitmap.RowBytes * srcHeight, srcRowBytes * srcHeight);

                if (srcHeight >= 100)
                {
                    Parallel.For(0, srcHeight, y =>
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (dist > actualRadius || dist < 1f)
                                continue;

                            float normalizedDist = dist / actualRadius;
                            float falloff = 1f - normalizedDist;
                            falloff = falloff * falloff * (3f - 2f * falloff);

                            float displacement = strength * falloff * actualRadius * 0.3f;

                            float srcX = x;
                            float srcY = y;

                            switch (type)
                            {
                                case LiquifyType.Pinch:
                                    if (strength > 0)
                                    {
                                        srcX = centerX + dx * (1f - displacement / dist);
                                        srcY = centerY + dy * (1f - displacement / dist);
                                    }
                                    else
                                    {
                                        srcX = centerX + dx * (1f + displacement / dist);
                                        srcY = centerY + dy * (1f + displacement / dist);
                                    }
                                    break;
                                case LiquifyType.Stretch:
                                    srcX = x + displacement * (dx / dist);
                                    srcY = y + displacement * (dy / dist);
                                    break;
                                case LiquifyType.ShiftLeft:
                                    srcX = x - displacement;
                                    break;
                                case LiquifyType.ShiftRight:
                                    srcX = x + displacement;
                                    break;
                            }

                            srcX = Math.Clamp(srcX, 0, srcWidth - 1);
                            srcY = Math.Clamp(srcY, 0, srcHeight - 1);

                            int sx1 = (int)Math.Floor(srcX);
                            int sy1 = (int)Math.Floor(srcY);
                            int sx2 = Math.Min(sx1 + 1, srcWidth - 1);
                            int sy2 = Math.Min(sy1 + 1, srcHeight - 1);

                            float fx = srcX - sx1;
                            float fy = srcY - sy1;

                            byte* row1 = (byte*)srcPixels + (sy1 * srcRowBytes);
                            byte* row2 = (byte*)srcPixels + (sy2 * srcRowBytes);

                            float b = (1 - fx) * (1 - fy) * row1[sx1] + fx * (1 - fy) * row1[sx2] + (1 - fx) * fy * row2[sx1] + fx * fy * row2[sx1];
                            float g = (1 - fx) * (1 - fy) * row1[sx1 + 1] + fx * (1 - fy) * row1[sx2 + 1] + (1 - fx) * fy * row2[sx1 + 1] + fx * fy * row2[sx2 + 1];
                            float r = (1 - fx) * (1 - fy) * row1[sx1 + 2] + fx * (1 - fy) * row1[sx2 + 2] + (1 - fx) * fy * row2[sx1 + 2] + fx * fy * row2[sx2 + 2];
                            float a = (1 - fx) * (1 - fy) * row1[sx1 + 3] + fx * (1 - fy) * row1[sx2 + 3] + (1 - fx) * fy * row2[sx1 + 3] + fx * fy * row2[sx2 + 3];

                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = (byte)Math.Clamp(a, 0, 255);
                        }
                    });
                }
                else
                {
                    for (int y = 0; y < srcHeight; y++)
                    {
                        byte* destRow = (byte*)destPixels + (y * destRowBytes);

                        for (int x = 0; x < srcWidth; x++)
                        {
                            float dx = x - centerX;
                            float dy = y - centerY;
                            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (dist > actualRadius || dist < 1f)
                                continue;

                            float normalizedDist = dist / actualRadius;
                            float falloff = 1f - normalizedDist;
                            falloff = falloff * falloff * (3f - 2f * falloff);

                            float displacement = strength * falloff * actualRadius * 0.3f;

                            float srcX = x;
                            float srcY = y;

                            switch (type)
                            {
                                case LiquifyType.Pinch:
                                    if (strength > 0)
                                    {
                                        srcX = centerX + dx * (1f - displacement / dist);
                                        srcY = centerY + dy * (1f - displacement / dist);
                                    }
                                    else
                                    {
                                        srcX = centerX + dx * (1f + displacement / dist);
                                        srcY = centerY + dy * (1f + displacement / dist);
                                    }
                                    break;
                                case LiquifyType.Stretch:
                                    srcX = x + displacement * (dx / dist);
                                    srcY = y + displacement * (dy / dist);
                                    break;
                                case LiquifyType.ShiftLeft:
                                    srcX = x - displacement;
                                    break;
                                case LiquifyType.ShiftRight:
                                    srcX = x + displacement;
                                    break;
                            }

                            srcX = Math.Clamp(srcX, 0, srcWidth - 1);
                            srcY = Math.Clamp(srcY, 0, srcHeight - 1);

                            int sx1 = (int)Math.Floor(srcX);
                            int sy1 = (int)Math.Floor(srcY);
                            int sx2 = Math.Min(sx1 + 1, srcWidth - 1);
                            int sy2 = Math.Min(sy1 + 1, srcHeight - 1);

                            float fx = srcX - sx1;
                            float fy = srcY - sy1;

                            byte* row1 = (byte*)srcPixels + (sy1 * srcRowBytes);
                            byte* row2 = (byte*)srcPixels + (sy2 * srcRowBytes);

                            float b = (1 - fx) * (1 - fy) * row1[sx1] + fx * (1 - fy) * row1[sx2] + (1 - fx) * fy * row2[sx1] + fx * fy * row2[sx1];
                            float g = (1 - fx) * (1 - fy) * row1[sx1 + 1] + fx * (1 - fy) * row1[sx2 + 1] + (1 - fx) * fy * row2[sx1 + 1] + fx * fy * row2[sx2 + 1];
                            float r = (1 - fx) * (1 - fy) * row1[sx1 + 2] + fx * (1 - fy) * row1[sx2 + 2] + (1 - fx) * fy * row2[sx1 + 2] + fx * fy * row2[sx2 + 2];
                            float a = (1 - fx) * (1 - fy) * row1[sx1 + 3] + fx * (1 - fy) * row1[sx2 + 3] + (1 - fx) * fy * row2[sx1 + 3] + fx * fy * row2[sx2 + 3];

                            int x4 = x * 4;
                            destRow[x4] = (byte)Math.Clamp(b, 0, 255);
                            destRow[x4 + 1] = (byte)Math.Clamp(g, 0, 255);
                            destRow[x4 + 2] = (byte)Math.Clamp(r, 0, 255);
                            destRow[x4 + 3] = (byte)Math.Clamp(a, 0, 255);
                        }
                    }
                }
            }

            return ConvertToAvaloniaBitmap(destBitmap);
        }

        #endregion

        public static async Task SaveToFile(Bitmap imageSource, string filePath)
        {
            if (imageSource == null)
            {
                return;
            }

            try
            {
                using var stream = System.IO.File.Create(filePath);
                imageSource.Save(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存图片失败: {ex.Message}");
            }
        }

        #endregion
    }
}