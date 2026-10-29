using Avalonia;
using Avalonia.Media.Imaging;
using SkiaSharp;
using yuyiAvalonia.ImageEdit.Models;

namespace yuyiAvalonia.ImageEdit.Helpers
{
    /// <summary>
    /// 画笔绘制辅助类 - 提供高性能的画笔绘制功能
    /// 支持多种画笔类型：钢笔、铅笔、油画、马克笔、蜡笔、水彩、喷雾、气笔
    /// </summary>
    public static class DrawingHelper
    {
        /// <summary>
        /// 根据画笔类型获取 StrokeCap（线帽样式）
        /// </summary>
        /// <param name="brushType">画笔类型</param>
        /// <returns>SkiaSharp 线帽样式</returns>
        public static SKStrokeCap GetStrokeCap(BrushType brushType)
        {
            return brushType switch
            {
                BrushType.Pen => SKStrokeCap.Round,          // 钢笔：圆头，平滑
                BrushType.Pencil => SKStrokeCap.Round,       // 铅笔：圆头
                BrushType.Oil => SKStrokeCap.Round,          // 油画笔：圆头，模拟笔触
                BrushType.Marker => SKStrokeCap.Square,     // 马克笔：方头，边缘清晰
                BrushType.Crayon => SKStrokeCap.Round,      // 蜡笔：圆头
                BrushType.Watercolor => SKStrokeCap.Round,   // 水彩笔：圆头
                BrushType.Spray => SKStrokeCap.Round,        // 喷雾：圆头
                BrushType.Airbrush => SKStrokeCap.Round,     // 气笔：圆头
                _ => SKStrokeCap.Round
            };
        }

        /// <summary>
        /// 根据画笔类型获取 StrokeJoin（线段连接样式）
        /// </summary>
        public static SKStrokeJoin GetStrokeJoin(BrushType brushType)
        {
            return brushType switch
            {
                BrushType.Marker => SKStrokeJoin.Miter,      // 马克笔：尖锐连接
                BrushType.Crayon => SKStrokeJoin.Round,     // 蜡笔：圆滑连接
                _ => SKStrokeJoin.Round
            };
        }

        /// <summary>
        /// 根据画笔类型获取推荐的混合模式
        /// </summary>
        /// <param name="settings">画笔设置</param>
        /// <returns>推荐使用的混合模式</returns>
        public static SKBlendMode GetBrushBlendMode(BrushSettings settings)
        {
            // 水彩需要特殊的乘法混合来模拟颜料叠加
            if (settings.BrushType == BrushType.Watercolor)
            {
                return SKBlendMode.Multiply;
            }

            // 油画使用 Plus 混合模式实现颜料叠加变厚效果
            if (settings.BrushType == BrushType.Oil)
            {
                return SKBlendMode.Plus;
            }

            // 其他画笔使用默认 SrcOver
            return SKBlendMode.SrcOver;
        }

        /// <summary>
        /// 根据画笔类型获取推荐的透明度系数
        /// </summary>
        public static float GetBrushAlphaMultiplier(BrushType brushType)
        {
            return brushType switch
            {
                BrushType.Pen => 1.0f,           // 钢笔：不透明
                BrushType.Pencil => 0.7f,        // 铅笔：略透明，模拟石墨
                BrushType.Oil => 0.9f,           // 油画：较不透明
                BrushType.Marker => 0.6f,         // 马克笔：半透明
                BrushType.Crayon => 0.75f,       // 蜡笔：略带纹理
                BrushType.Watercolor => 0.4f,    // 水彩：非常透明
                BrushType.Spray => 0.5f,         // 喷雾：稀疏
                BrushType.Airbrush => 0.55f,     // 气笔：柔和
                _ => 1.0f
            };
        }

        /// <summary>
        /// 根据画笔类型获取推荐的线宽系数（相对于设置宽度）
        /// </summary>
        public static float GetBrushWidthMultiplier(BrushType brushType)
        {
            return brushType switch
            {
                BrushType.Pen => 1.0f,           // 钢笔：标准宽度
                BrushType.Pencil => 0.8f,        // 铅笔：略细
                BrushType.Oil => 1.2f,           // 油画：较粗，模拟笔触
                BrushType.Marker => 1.1f,        // 马克笔：略宽
                BrushType.Crayon => 1.15f,      // 蜡笔：略宽，模拟蜡笔质感
                BrushType.Watercolor => 1.3f,   // 水彩：较宽，模拟水彩晕染
                BrushType.Spray => 1.5f,        // 喷雾：更宽
                BrushType.Airbrush => 1.4f,      // 气笔：柔和扩散
                _ => 1.0f
            };
        }

        /// <summary>
        /// 创建基础 SKPaint（根据画笔类型设置初始属性）
        /// </summary>
        /// <param name="settings">画笔设置</param>
        /// <param name="scaledStrokeWidth">缩放后的线宽</param>
        /// <returns>配置好的 SKPaint 实例</returns>
        public static SKPaint CreatePaint(BrushSettings settings, float scaledStrokeWidth)
        {
            // 计算最终透明度：基础透明度 × 画笔类型系数 × 用户透明度
            float brushAlphaMultiplier = GetBrushAlphaMultiplier(settings.BrushType);
            byte finalAlpha = (byte)(settings.StrokeColor.A * settings.Opacity * brushAlphaMultiplier);
            finalAlpha = Math.Clamp(finalAlpha, (byte)0, (byte)255);

            // 根据画笔类型计算实际线宽
            float widthMultiplier = GetBrushWidthMultiplier(settings.BrushType);
            float actualWidth = scaledStrokeWidth * widthMultiplier;

            var paint = new SKPaint
            {
                Color = new SKColor(
                    settings.StrokeColor.R,
                    settings.StrokeColor.G,
                    settings.StrokeColor.B,
                    finalAlpha),
                StrokeWidth = actualWidth,
                IsAntialias = settings.IsSmooth,
                Style = SKPaintStyle.Stroke,
                StrokeCap = GetStrokeCap(settings.BrushType),
                StrokeJoin = GetStrokeJoin(settings.BrushType),
                BlendMode = GetBrushBlendMode(settings)
            };

            // 为油画笔添加特殊效果
            if (settings.BrushType == BrushType.Oil)
            {
                // 油画笔使用实心填充来模拟颜料厚度
                paint.Style = SKPaintStyle.Fill;
                // 创建柔和边缘
                paint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, actualWidth * 0.15f);
            }

            // 为马克笔添加不透明效果
            if (settings.BrushType == BrushType.Marker)
            {
                // 确保高不透明度
                paint.Color = paint.Color.WithAlpha(Math.Max(paint.Color.Alpha, (byte)180));
            }

            return paint;
        }

        /// <summary>
        /// 复制 SKPaint（手动复制所有属性）
        /// </summary>
        private static SKPaint ClonePaint(SKPaint source)
        {
            return new SKPaint
            {
                Color = source.Color,
                StrokeWidth = source.StrokeWidth,
                IsAntialias = source.IsAntialias,
                Style = source.Style,
                StrokeCap = source.StrokeCap,
                StrokeJoin = source.StrokeJoin,
                BlendMode = source.BlendMode,
                PathEffect = source.PathEffect,
                MaskFilter = source.MaskFilter,
                ImageFilter = source.ImageFilter,
                ColorFilter = source.ColorFilter,
                Shader = source.Shader
            };
        }

        /// <summary>
        /// 应用画笔效果并绘制路径（核心绘制方法）
        /// </summary>
        /// <param name="canvas">SkiaSharp 画布</param>
        /// <param name="stroke">笔画点列表</param>
        /// <param name="paint">基础画笔</param>
        /// <param name="settings">画笔设置</param>
        /// <param name="scaledStrokeWidth">缩放后的线宽</param>
        public static void ApplyBrushEffectAndDraw(SKCanvas canvas, List<Point> stroke, SKPaint paint, BrushSettings settings, float scaledStrokeWidth)
        {
            if (stroke == null || stroke.Count < 2)
                return;

            // 将点转换为 SkiaSharp 坐标
            var skPoints = stroke.Select(p => new SKPoint((float)p.X, (float)p.Y)).ToArray();

            // 根据画笔类型应用特殊效果
            switch (settings.BrushType)
            {
                case BrushType.Spray:
                    // 喷雾效果：使用独立方法绘制
                    DrawSprayEffect(canvas, stroke, paint, scaledStrokeWidth);
                    return;

                case BrushType.Watercolor:
                    // 水彩效果：使用路径效果创建不规则边缘
                    ApplyWatercolorEffect(canvas, skPoints, paint, scaledStrokeWidth);
                    return;

                case BrushType.Crayon:
                    // 蜡笔效果：使用离散路径效果创建粗糙纹理
                    ApplyCrayonEffect(canvas, skPoints, paint, scaledStrokeWidth);
                    return;

                case BrushType.Airbrush:
                    // 气笔效果：使用高斯模糊
                    ApplyAirbrushEffect(canvas, skPoints, paint, scaledStrokeWidth);
                    return;

                case BrushType.Pencil:
                    // 铅笔效果：创建渐变效果的路径
                    ApplyPencilEffect(canvas, skPoints, paint, scaledStrokeWidth);
                    return;

                case BrushType.Oil:
                    // 油画效果：绘制多层模拟颜料厚度
                    ApplyOilEffect(canvas, skPoints, paint, scaledStrokeWidth);
                    return;

                case BrushType.Marker:
                case BrushType.Pen:
                default:
                    // 马克笔和钢笔：直接绘制
                    DrawBasicPath(canvas, skPoints, paint);
                    break;
            }
        }

        /// <summary>
        /// 绘制基本路径（用于普通画笔）
        /// </summary>
        private static void DrawBasicPath(SKCanvas canvas, SKPoint[] skPoints, SKPaint paint)
        {
            using var path = new SKPath();
            path.MoveTo(skPoints[0]);
            for (int i = 1; i < skPoints.Length; i++)
            {
                path.LineTo(skPoints[i]);
            }
            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// 喷雾效果实现 - 在笔画路径周围生成随机分布的颗粒
        /// </summary>
        /// <param name="canvas">画布</param>
        /// <param name="stroke">笔画点列表</param>
        /// <param name="paint">基础画笔</param>
        /// <param name="scaledStrokeWidth">线宽</param>
        public static void DrawSprayEffect(SKCanvas canvas, List<Point> stroke, SKPaint paint, float scaledStrokeWidth)
        {
            // 使用线程局部随机数生成器以提高性能
            var density = scaledStrokeWidth * 4;
            var radius = scaledStrokeWidth * 1.5f;
            var random = new Random();

            // 预创建画笔以提高性能
            using var sprayPaint = new SKPaint
            {
                Color = paint.Color,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            foreach (var point in stroke)
            {
                // 减少每个点的喷雾密度以提高性能
                int particleCount = Math.Max(5, (int)(density / 3));

                for (int i = 0; i < particleCount; i++)
                {
                    // 使用高斯分布模拟更自然的喷雾效果
                    float angle = (float)(random.NextDouble() * Math.PI * 2);
                    float distance = (float)(random.NextDouble() * radius);

                    float x = (float)point.X + (float)Math.Cos(angle) * distance;
                    float y = (float)point.Y + (float)Math.Sin(angle) * distance;

                    // 随机大小和透明度
                    float particleSize = 0.5f + (float)random.NextDouble() * 1.5f;
                    byte alpha = (byte)(paint.Color.Alpha * (0.3f + random.NextDouble() * 0.7f));

                    sprayPaint.Color = paint.Color.WithAlpha(alpha);
                    canvas.DrawCircle(x, y, particleSize, sprayPaint);
                }
            }
        }

        /// <summary>
        /// 水彩效果 - 使用路径效果创建不规则边缘并多层叠加
        /// </summary>
        private static void ApplyWatercolorEffect(SKCanvas canvas, SKPoint[] skPoints, SKPaint paint, float scaledStrokeWidth)
        {
            // 水彩需要多层绘制以创建渐变和扩散效果
            int layers = 3;

            for (int layer = 0; layer < layers; layer++)
            {
                // 每一层使用略微不同的参数
                float layerScale = 1f + layer * 0.15f;
                float alphaDecay = 1f - layer * 0.25f;

                using var layerPaint = ClonePaint(paint);
                layerPaint.Color = layerPaint.Color.WithAlpha((byte)(layerPaint.Color.Alpha * alphaDecay));
                layerPaint.StrokeWidth = scaledStrokeWidth * layerScale;

                // 使用离散路径效果创建不规则边缘
                float roughness = scaledStrokeWidth * 0.3f;
                layerPaint.PathEffect = SKPathEffect.CreateDiscrete(roughness, roughness / 2);

                // 添加轻微模糊模拟水彩扩散
                layerPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, scaledStrokeWidth * 0.1f * layer);

                // 绘制路径
                DrawBasicPath(canvas, skPoints, layerPaint);
            }
        }

        /// <summary>
        /// 蜡笔效果 - 粗糙纹理和颜色变化
        /// </summary>
        private static void ApplyCrayonEffect(SKCanvas canvas, SKPoint[] skPoints, SKPaint paint, float scaledStrokeWidth)
        {
            // 蜡笔需要粗糙纹理效果
            using var crayonPaint = ClonePaint(paint);

            // 使用离散效果创建粗糙边缘
            float roughness = scaledStrokeWidth * 0.4f;
            crayonPaint.PathEffect = SKPathEffect.CreateDiscrete(roughness, roughness * 0.5f);

            // 绘制第一层
            DrawBasicPath(canvas, skPoints, crayonPaint);

            // 绘制第二层略微偏移的路径以增加纹理感
            var random = new Random(42); // 固定种子以保持一致性

            using var texturePaint = ClonePaint(paint);
            texturePaint.Color = texturePaint.Color.WithAlpha((byte)(texturePaint.Color.Alpha * 0.5f));
            texturePaint.PathEffect = SKPathEffect.CreateDiscrete(roughness * 0.8f, roughness * 0.3f);

            // 绘制纹理层
            DrawBasicPath(canvas, skPoints, texturePaint);
        }

        /// <summary>
        /// 气笔效果 - 柔和模糊的边缘
        /// </summary>
        private static void ApplyAirbrushEffect(SKCanvas canvas, SKPoint[] skPoints, SKPaint paint, float scaledStrokeWidth)
        {
            // 气笔需要柔和的模糊效果
            using var airbrushPaint = ClonePaint(paint);

            // 高斯模糊创建柔和边缘
            float blurRadius = scaledStrokeWidth / 3f;
            airbrushPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius);

            // 绘制
            DrawBasicPath(canvas, skPoints, airbrushPaint);
        }

        /// <summary>
        /// 铅笔效果 - 渐变透明度和轻微纹理
        /// </summary>
        private static void ApplyPencilEffect(SKCanvas canvas, SKPoint[] skPoints, SKPaint paint, float scaledStrokeWidth)
        {
            // 铅笔效果：创建沿路径的渐变效果
            using var pencilPaint = ClonePaint(paint);

            // 轻微离散效果模拟石墨颗粒
            pencilPaint.PathEffect = SKPathEffect.CreateDiscrete(scaledStrokeWidth * 0.2f, scaledStrokeWidth * 0.1f);

            // 绘制
            DrawBasicPath(canvas, skPoints, pencilPaint);
        }

        /// <summary>
        /// 油画效果 - 多层叠加模拟颜料厚度
        /// </summary>
        private static void ApplyOilEffect(SKCanvas canvas, SKPoint[] skPoints, SKPaint paint, float scaledStrokeWidth)
        {
            // 油画效果：使用多层绘制
            int layers = 2;

            for (int layer = 0; layer < layers; layer++)
            {
                using var layerPaint = ClonePaint(paint);

                // 每一层略微偏移并使用不同的模糊
                float offset = layer * scaledStrokeWidth * 0.1f;
                layerPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, scaledStrokeWidth * 0.1f * (layer + 1));

                // 绘制路径
                DrawBasicPath(canvas, skPoints, layerPaint);
            }
        }

        /// <summary>
        /// 应用画笔到图像（核心绘制方法）
        /// </summary>
        /// <param name="sourceBitmap">源 Avalonia Bitmap</param>
        /// <param name="brushStrokes">画笔路径列表</param>
        /// <param name="settings">画笔设置</param>
        /// <param name="displayStrokeWidth">显示线宽</param>
        /// <param name="cachedSkBitmap">可选的缓存 SKBitmap，用于避免重复转换</param>
        /// <param name="startStrokeIndex">开始绘制的笔画索引（用于只绘制新笔画）</param>
        /// <param name="zoomLevel">当前缩放级别，用于计算棋格大小</param>
        /// <returns>绘制后的新 Bitmap</returns>
        public static Bitmap? ApplyBrushToImage(SKBitmap sourceBitmap, List<List<Point>> brushStrokes, BrushSettings settings, double displayStrokeWidth, int startStrokeIndex = 0, double zoomLevel = 1.0)
        {
            if (sourceBitmap == null || brushStrokes == null || brushStrokes.Count == 0)
                return null;

            try
            {
                var width = sourceBitmap.Width;
                var height = sourceBitmap.Height;

                // 获取或创建 SKBitmap
                var skBitmap = sourceBitmap;

                if (skBitmap == null)
                    return null;

                // 创建目标 SkiaSharp 位图
                using var destBitmap = new SKBitmap(width, height);
                using var canvas = new SKCanvas(destBitmap);

                // 绘制原始图像作为背景
                canvas.DrawBitmap(skBitmap, 0, 0);

                // 使用显示线宽
                var scaledStrokeWidth = (float)displayStrokeWidth;

                // 绘制新添加的笔画
                for (int i = startStrokeIndex; i < brushStrokes.Count; i++)
                {
                    var stroke = brushStrokes[i];
                    if (stroke.Count < 2) continue;

                    using var paint = CreatePaint(settings, scaledStrokeWidth);
                    ApplyBrushEffectAndDraw(canvas, stroke, paint, settings, scaledStrokeWidth);
                }

                // 将结果转换为 Avalonia Bitmap（使用当前缩放级别绘制棋格）
                return ImageHelper.ConvertToAvaloniaBitmap(destBitmap, true, zoomLevel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyBrushToImage error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 直接在已有的 SKBitmap 上绘制笔画（用于增量绘制）
        /// </summary>
        /// <param name="skBitmap">目标 SKBitmap（将被修改）</param>
        /// <param name="stroke">要绘制的笔画点列表</param>
        /// <param name="settings">画笔设置</param>
        /// <param name="displayStrokeWidth">显示线宽</param>
        /// <returns>是否成功</returns>
        public static bool DrawStrokeOnSkBitmap(SKBitmap skBitmap, List<Point> stroke, BrushSettings settings, float displayStrokeWidth)
        {
            if (skBitmap == null || stroke == null || stroke.Count < 2)
                return false;

            try
            {
                using var canvas = new SKCanvas(skBitmap);
                using var paint = CreatePaint(settings, displayStrokeWidth);
                ApplyBrushEffectAndDraw(canvas, stroke, paint, settings, displayStrokeWidth);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawStrokeOnSkBitmap error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 将 SKBitmap 转换为 Avalonia Bitmap
        /// </summary>
        /// <param name="skBitmap">源 SKBitmap</param>
        /// <param name="zoomLevel">当前缩放级别，用于计算棋格大小</param>
        public static Bitmap? SkBitmapToAvaloniaBitmap(SKBitmap skBitmap, double zoomLevel = 1.0)
        {
            return ImageHelper.ConvertToAvaloniaBitmap(skBitmap, true, zoomLevel);
        }

        /// <summary>
        /// 应用画笔到 SKBitmap（高级方法，返回 SKBitmap）
        /// </summary>
        /// <param name="sourceSkBitmap">源 SKBitmap</param>
        /// <param name="brushStrokes">画笔路径列表</param>
        /// <param name="settings">画笔设置</param>
        /// <returns>绘制后的 SKBitmap</returns>
        public static SKBitmap? ApplyBrushToSkBitmap(SKBitmap sourceSkBitmap, List<List<Point>> brushStrokes, BrushSettings settings)
        {
            if (sourceSkBitmap == null || brushStrokes == null || brushStrokes.Count == 0)
                return null;

            try
            {
                var width = sourceSkBitmap.Width;
                var height = sourceSkBitmap.Height;

                // 创建目标 SkiaSharp 位图
                var destBitmap = new SKBitmap(width, height);
                using var canvas = new SKCanvas(destBitmap);

                // 绘制原始图像
                canvas.DrawBitmap(sourceSkBitmap, 0, 0);

                // 使用设置中的线宽
                var scaledStrokeWidth = (float)settings.StrokeWidth;

                // 绘制所有画笔路径
                foreach (var stroke in brushStrokes)
                {
                    if (stroke.Count < 2) continue;

                    using var paint = CreatePaint(settings, scaledStrokeWidth);
                    ApplyBrushEffectAndDraw(canvas, stroke, paint, settings, scaledStrokeWidth);
                }

                return destBitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyBrushToSkBitmap error: {ex.Message}");
                return null;
            }
        }
    }
}
