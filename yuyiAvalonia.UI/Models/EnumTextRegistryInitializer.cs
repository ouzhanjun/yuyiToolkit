using System.Collections.Generic;
using yuyiAvalonia.Base.Controls.Models;

namespace yuyiAvalonia.UI.Models
{
    /// <summary>
    /// 在模块加载时自动注册 UI 项目中自定义枚举的显示文本映射。
    /// 由静态构造函数触发，所有映射在首次访问任何枚举文本前完成。
    /// </summary>
    public static class EnumTextRegistryInitializer
    {
        static EnumTextRegistryInitializer()
        {
            // ===== yuyiAvalonia.UI.Models 枚举 =====

            EnumTextRegistry.Register(typeof(FillType), new Dictionary<string, string>
            {
                { "None", "无填充" },
                { "Solid", "纯色" },
                { "Gradient", "渐变" },
                { "Image", "图片" },
            });

            EnumTextRegistry.Register(typeof(GradientKind), new Dictionary<string, string>
            {
                { "Linear", "线性" },
                { "Radial", "径向" },
                { "Angle", "角度" },
                { "Diamond", "菱形" },
            });

            EnumTextRegistry.Register(typeof(StrokePosition), new Dictionary<string, string>
            {
                { "Inside", "内侧" },
                { "Outside", "外侧" },
                { "Center", "居中" },
            });

            EnumTextRegistry.Register(typeof(StrokeJoin), new Dictionary<string, string>
            {
                { "Miter", "尖角" },
                { "Round", "圆角" },
                { "Bevel", "斜角" },
            });

            EnumTextRegistry.Register(typeof(StrokeEndpoints), new Dictionary<string, string>
            {
                { "Butt", "平头" },
                { "Round", "圆头" },
                { "Square", "方头" },
            });

            EnumTextRegistry.Register(typeof(StrokeStyleType), new Dictionary<string, string>
            {
                { "Solid", "实线" },
                { "Dashed", "虚线" },
                { "Dot", "点线" },
                { "DashDot", "点划线" },
                { "DashDotDot", "双点划线" },
            });

            EnumTextRegistry.Register(typeof(StrokeType), new Dictionary<string, string>
            {
                { "Solid", "实线" },
                { "Dashed", "虚线" },
                { "Custom", "自定义" },
            });

            EnumTextRegistry.Register(typeof(StrokeWidthMode), new Dictionary<string, string>
            {
                { "Uniform", "统一宽度" },
                { "Individual", "独立宽度" },
            });

            EnumTextRegistry.Register(typeof(StrokeFillType), new Dictionary<string, string>
            {
                { "None", "无" },
                { "Solid", "纯色" },
                { "Gradient", "渐变" },
                { "Image", "图片" },
            });

            EnumTextRegistry.Register(typeof(BackgroundType), new Dictionary<string, string>
            {
                { "None", "无" },
                { "SolidColor", "纯色" },
                { "Gradient", "渐变" },
            });

            EnumTextRegistry.Register(typeof(GlowType), new Dictionary<string, string>
            {
                { "None", "无" },
                { "Solid", "纯色" },
                { "Gradient", "渐变" },
            });
        }

        /// <summary>
        /// 在首次访问此类的任何成员前，触发静态构造函数完成注册。
        /// 在程序入口处调用一次即可。
        /// </summary>
        public static void EnsureRegistered() { }
    }
}
