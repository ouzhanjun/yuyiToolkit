using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.Globalization;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Converters
{

    public class FillTypeBoolConverter : IValueConverter
    {
        private readonly FillType _kind;
        public FillTypeBoolConverter(FillType kind) => _kind = kind;
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is FillType k) return k == _kind;
            return false;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is true) return _kind;
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// 将 FillItem 的 GradientStops 集合转换为 Avalonia 渐变画刷（支持 Linear/Radial/Angle/Diamond）。
    /// XAML 中用于预览渐变效果。
    /// </summary>
    public class FillItemToGradientBrushConverter : IMultiValueConverter
    {
        public static readonly FillItemToGradientBrushConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2) return null;
            if (values[0] is not FillItem fill) return null;
            if (values[1] is not double opacity) opacity = 1.0;

            if (fill.GradientStops == null || fill.GradientStops.Count < 2)
            {
                var start = TryParseColor(fill.GradientStartColor, out var sc) ? sc : Colors.White;
                var end = TryParseColor(fill.GradientEndColor, out var ec) ? ec : Colors.Black;
                return new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb((byte)(opacity * 255), start.R, start.G, start.B), 0),
                        new GradientStop(Color.FromArgb((byte)(opacity * 255), end.R, end.G, end.B), 1),
                    }
                };
            }

            var radians = fill.GradientAngle * Math.PI / 180.0;
            var dx = Math.Cos(radians);
            var dy = Math.Sin(radians);

            var ordered = fill.GradientStops.OrderBy(s => s.Offset).ToList();

            switch (fill.GradientKind)
            {
                case GradientKind.Linear:
                    return new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                        GradientStops = BuildStops(ordered, opacity)
                    };

                case GradientKind.Radial:
                    return new RadialGradientBrush
                    {
                        Center = new RelativePoint(fill.RadialCenterX, fill.RadialCenterY, RelativeUnit.Relative),
                        GradientOrigin = new RelativePoint(fill.RadialCenterX, fill.RadialCenterY, RelativeUnit.Relative),
                        Radius = fill.RadialRadius,
                        GradientStops = BuildStops(ordered, opacity)
                    };

                case GradientKind.Angle:
                case GradientKind.Diamond:
                    var first = ordered.First();
                    var last = ordered.Last();
                    var fc = TryParseColor(first.Color, out var fsc) ? fsc : Colors.White;
                    var lc = TryParseColor(last.Color, out var lec) ? lec : Colors.Black;
                    return new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(0.5 - dx * 0.5, 0.5 - dy * 0.5, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(0.5 + dx * 0.5, 0.5 + dy * 0.5, RelativeUnit.Relative),
                        GradientStops = new GradientStops
                        {
                            new GradientStop(Color.FromArgb((byte)(first.Opacity * opacity * 255), fc.R, fc.G, fc.B), 0),
                            new GradientStop(Color.FromArgb((byte)(last.Opacity * opacity * 255), lc.R, lc.G, lc.B), 1),
                        }
                    };

                default:
                    return null;
            }
        }

        private static GradientStops BuildStops(IList<GradientStopInfo> ordered, double baseOpacity)
        {
            var stops = new GradientStops();
            foreach (var stop in ordered)
            {
                var color = TryParseColor(stop.Color, out var c) ? c : Colors.White;
                var a = (byte)(stop.Opacity * baseOpacity * 255);
                stops.Add(new GradientStop(Color.FromArgb(a, color.R, color.G, color.B), stop.Offset));
            }
            return stops;
        }

        private static bool TryParseColor(string? colorString, out Color color)
        {
            if (!string.IsNullOrWhiteSpace(colorString) && Color.TryParse(colorString, out color))
                return true;
            color = Colors.White;
            return false;
        }
    }

    /// <summary>
    /// 把 FillItem 的渐变首末颜色合成为一个 LinearGradientBrush：
    /// 首色 80% 透明度、末色 20% 透明度，沿对角线（左上 → 右下）渲染。
    /// 用于"渐变类型"按钮的图标预览，结构与纯色按钮的实心圆保持一致。
    /// </summary>
    public class GradientStopsToPreviewBrushConverter : IValueConverter
    {
        public static readonly GradientStopsToPreviewBrushConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var fill = value as FillItem;
            var hasStops = fill?.GradientStops != null && fill.GradientStops.Count >= 2;

            if (!hasStops)
            {
                // 兜底：缺停止点时使用旧的起止色字段，仍输出一个带 80/20 不透明度的渐变
                var bs = TryParseColor(fill?.BackgroundColor, out var bsc) ? bsc : Colors.White;
                var be = TryParseColor(fill?.BackgroundColor, out var bec) ? bec : Colors.Black;
                return new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(Color.FromArgb((byte)(0.9 * 255), bs.R, bs.G, bs.B), 0),
                        new GradientStop(Color.FromArgb((byte)(0.1 * 255), be.R, be.G, be.B), 1),
                    }
                };
            }

            var ordered = fill!.GradientStops.OrderBy(s => s.Offset).ToList();
            var first = ordered.First();
            var last = ordered.Last();
            var fc = TryParseColor(first.Color, out var fsc) ? fsc : Colors.White;
            var lc = TryParseColor(last.Color, out var lec) ? lec : Colors.Black;

            return new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.FromArgb((byte)(0.9 * 255), fc.R, fc.G, fc.B), 0),
                    new GradientStop(Color.FromArgb((byte)(0.1 * 255), lc.R, lc.G, lc.B), 1),
                }
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Avalonia.Data.BindingOperations.DoNothing;

        private static bool TryParseColor(string? colorString, out Color color)
        {
            if (!string.IsNullOrWhiteSpace(colorString) && Color.TryParse(colorString, out color))
                return true;
            color = Colors.White;
            return false;
        }
    }

}
