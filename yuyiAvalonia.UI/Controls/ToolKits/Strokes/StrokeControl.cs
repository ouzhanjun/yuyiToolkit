using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Helpers;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.Controls.ToolKits
{
    /// <summary>
    /// 几何图形描边控件，支持 Inside/Center/Outside 三种描边位置，
    /// 以及实线/虚线/自定义虚线三种描边类型，并可挂载多层 FillItem 背景。
    /// </summary>
    public class StrokeControl : Panel
    {

        #region Styled Properties（描边参数）

        /// <summary>描边依附的几何路径（主路径）</summary>
        public static readonly StyledProperty<Geometry?> GeometryProperty =
            AvaloniaProperty.Register<StrokeControl, Geometry?>(nameof(Geometry));

        public Geometry? Geometry
        {
            get => GetValue(GeometryProperty);
            set => SetValue(GeometryProperty, value);
        }

        /// <summary>描边线宽（像素）</summary>
        public static readonly StyledProperty<double> StrokeWidthProperty =
            AvaloniaProperty.Register<StrokeControl, double>(nameof(StrokeWidth), 1.0);

        public double StrokeWidth
        {
            get => GetValue(StrokeWidthProperty);
            set => SetValue(StrokeWidthProperty, value);
        }

        /// <summary>虚线线段与间隔的像素值序列（由 DashArrayDefinition 解析得到）</summary>
        public static readonly StyledProperty<AvaloniaList<double>?> StrokeDashArrayProperty =
            AvaloniaProperty.Register<StrokeControl, AvaloniaList<double>?>(nameof(StrokeDashArray));

        public AvaloniaList<double>? StrokeDashArray
        {
            get => GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        /// <summary>
        /// 虚线数组定义值（格式如 "8,4" 或 "4,2,1,2"）
        /// </summary>
        public static readonly StyledProperty<string> DashArrayDefinitionProperty =
            AvaloniaProperty.Register<StrokeControl, string>(nameof(DashArrayDefinition));

        public string DashArrayDefinition
        {
            get => GetValue(DashArrayDefinitionProperty);
            set => SetValue(DashArrayDefinitionProperty, value);
        }

        /// <summary>虚线相位偏移（像素）</summary>
        public static readonly StyledProperty<double> StrokeDashOffsetProperty =
            AvaloniaProperty.Register<StrokeControl, double>(nameof(StrokeDashOffset));

        public double StrokeDashOffset
        {
            get => GetValue(StrokeDashOffsetProperty);
            set => SetValue(StrokeDashOffsetProperty, value);
        }

        /// <summary>描边线段端点连接方式（Miter/Round/Bevel）</summary>
        public static readonly StyledProperty<StrokeJoin> StrokeJoinProperty =
            AvaloniaProperty.Register<StrokeControl, StrokeJoin>(nameof(StrokeJoin), StrokeJoin.Miter);

        public StrokeJoin StrokeJoin
        {
            get => GetValue(StrokeJoinProperty);
            set => SetValue(StrokeJoinProperty, value);
        }

        /// <summary>描边线段端点样式（Butt/Round/Square）</summary>
        public static readonly StyledProperty<StrokeEndpoints?> StrokeEndpointProperty =
            AvaloniaProperty.Register<StrokeControl, StrokeEndpoints?>(
                nameof(StrokeEndpoint),
                defaultValue: Models.StrokeEndpoints.Butt,
                defaultBindingMode: BindingMode.TwoWay);

        public StrokeEndpoints? StrokeEndpoint
        {
            get => GetValue(StrokeEndpointProperty);
            set => SetValue(StrokeEndpointProperty, value);
        }

        /// <summary>描边尖角延伸上限（Miter模式下超过此值自动切换为Bevel）</summary>
        public static readonly StyledProperty<double> StrokeMiterLimitProperty =
            AvaloniaProperty.Register<StrokeControl, double>(nameof(StrokeMiterLimit), 4.0);

        public double StrokeMiterLimit
        {
            get => GetValue(StrokeMiterLimitProperty);
            set => SetValue(StrokeMiterLimitProperty, value);
        }

        /// <summary>描边尖角延伸上限（Miter模式下超过此值自动切换为Bevel）</summary>
        public static readonly StyledProperty<FillItem?> StrokeFillProperty =
            AvaloniaProperty.Register<StrokeControl, FillItem?>(nameof(StrokeFill), null);

        public FillItem? StrokeFill
        {
            get => GetValue(StrokeFillProperty);
            set => SetValue(StrokeFillProperty, value);
        }

        /// <summary>多层填充项集合（支持渐变/图片/纯色混合），由 UpdateStrokeBackground() 渲染</summary>
        public static readonly StyledProperty<ObservableCollection<FillItem>> StrokeFillsProperty =
            AvaloniaProperty.Register<StrokeControl, ObservableCollection<FillItem>>(
                nameof(StrokeFills),
                defaultValue: new ObservableCollection<FillItem>());

        public ObservableCollection<FillItem> StrokeFills
        {
            get => GetValue(StrokeFillsProperty);
            set => SetValue(StrokeFillsProperty, value);
        }

        /// <summary>描边类型（Solid=实线/Dashed=标准虚线/Custom=自定义虚线）</summary>
        public static readonly StyledProperty<StrokeType?> StrokeTypeProperty =
            AvaloniaProperty.Register<StrokeControl, StrokeType?>(
                nameof(StrokeType),
                defaultValue: Models.StrokeType.Solid,
                defaultBindingMode: BindingMode.TwoWay);

        public StrokeType? StrokeType
        {
            get => GetValue(StrokeTypeProperty);
            set => SetValue(StrokeTypeProperty, value);
        }

        /// <summary>
        /// 描边相对于路径的位置：Inside=描边向内、Centre=描边居中、Outside=描边向外。
        /// 不同位置通过 CombinedGeometry 的 Intersect/Exclude 运算生成不同的裁剪形状。
        /// </summary>
        public static readonly StyledProperty<StrokePosition?> StrokePositionProperty =
            AvaloniaProperty.Register<StrokeControl, StrokePosition?>(
                nameof(StrokePosition),
                defaultValue: Models.StrokePosition.Center,
                defaultBindingMode: BindingMode.TwoWay);

        public StrokePosition? StrokePosition
        {
            get => GetValue(StrokePositionProperty);
            set => SetValue(StrokePositionProperty, value);
        }

        /// <summary>描边填充模式（Solid=纯色填充/Gradient=渐变填充/Image=图片填充）</summary>
        public static readonly StyledProperty<Models.StrokeFillType> StrokeFillTypeProperty =
            AvaloniaProperty.Register<StrokeControl, Models.StrokeFillType>(
                nameof(StrokeFillType),
                defaultValue: Models.StrokeFillType.Solid,
                defaultBindingMode: BindingMode.TwoWay);

        public Models.StrokeFillType StrokeFillType
        {
            get => GetValue(StrokeFillTypeProperty);
            set => SetValue(StrokeFillTypeProperty, value);
        }

        /// <summary>描边颜色（#RRGGBB 格式字符串）</summary>
        public static readonly StyledProperty<string> StrokeColorProperty =
            AvaloniaProperty.Register<StrokeControl, string>(
                nameof(StrokeColor),
                defaultValue: "#333333",
                defaultBindingMode: BindingMode.TwoWay);

        public string StrokeColor
        {
            get => GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }

        /// <summary>
        /// 描边颜色不透明度（0.0~1.0）
        /// </summary>
        public static readonly StyledProperty<double> StrokeOpacityProperty =
            AvaloniaProperty.Register<StrokeControl, double>(
                nameof(StrokeOpacity),
                defaultValue: 1.0,
                defaultBindingMode: BindingMode.TwoWay);

        public double StrokeOpacity
        {
            get => GetValue(StrokeOpacityProperty);
            set => SetValue(StrokeOpacityProperty, value);
        }

        /// <summary>描边底层背景色（用于 StrokePosition=Outside 时描边区域下方的底色填充）</summary>
        public static readonly StyledProperty<Color> BackStrokeColorProperty =
            AvaloniaProperty.Register<StrokeControl, Color>(
                nameof(BackStrokeColor),
                defaultValue: Colors.Transparent,
                defaultBindingMode: BindingMode.TwoWay);

        public Color BackStrokeColor
        {
            get => GetValue(BackStrokeColorProperty);
            set => SetValue(BackStrokeColorProperty, value);
        }

        #endregion

        // 描边背景 Border：Clip 为路径加宽区域，Background 由 StrokeFills 或 StrokeColor 填充
        protected Border _imageBackground;
        // 当前 Pen 实例（延迟创建，包含Thickness/LineJoin/LineCap/MiterLimit）
        protected Pen? strokePen = null;

        /// <summary>
        /// 构造函数：创建背景 Border，订阅所有 StyledProperty 变更事件，
        /// 在对应属性变化时自动解析/更新描边画笔、裁剪形状和背景填充。
        /// </summary>
        public StrokeControl()
        {
            _imageBackground = new();

            this.Children.Add(_imageBackground);

            _imageBackground.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            _imageBackground.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

            // 属性变更统一回调：根据属性名执行对应的解析/更新逻辑
            PropertyChanged += (object? sender, AvaloniaPropertyChangedEventArgs e) =>
            {
                switch (e.Property.Name)
                {
                    case nameof(DashArrayDefinition):
                        var parsed = ParseDashArray(DashArrayDefinition);
                        StrokeDashArray = new AvaloniaList<double>(parsed);
                        break;
                    case nameof(StrokeColor):
                    case nameof(StrokeFills):
                        UpdateStrokeBackground();
                        break;
                    case nameof(BackStrokeColor):
                    case nameof(StrokeOpacity):
                        this.Background = new SolidColorBrush(BackStrokeColor, StrokeOpacity);
                        break;
                    case nameof(StrokeJoin):
                    case nameof(StrokeEndpoint):
                    case nameof(StrokeDashOffset):
                    case nameof(StrokeMiterLimit):
                    case nameof(StrokeWidth):
                    case nameof(StrokeType):
                    case nameof(Geometry):
                    case nameof(StrokeDashArray):
                    case nameof(StrokePosition):
                        RebuildStroke();
                        break;
                }
            };
        }

        private void RebuildStroke()
        {
            if (Geometry == null || StrokeWidth == 0)
                return;

            PenLineJoin penLineJoin = StrokeJoin switch
            {
                StrokeJoin.Miter => PenLineJoin.Miter,
                StrokeJoin.Round => PenLineJoin.Round,
                StrokeJoin.Bevel => PenLineJoin.Bevel,
                _ => PenLineJoin.Miter
            };

            PenLineCap penLineCap = (StrokeEndpoint ?? Models.StrokeEndpoints.Butt) switch
            {
                StrokeEndpoints.Butt => PenLineCap.Flat,
                StrokeEndpoints.Round => PenLineCap.Round,
                StrokeEndpoints.Square => PenLineCap.Square,
                _ => PenLineCap.Flat
            };

            strokePen = new Pen(Brushes.Black)
            {
                Thickness = StrokeWidth,
                LineJoin = penLineJoin,
                LineCap = penLineCap,
                MiterLimit = StrokeMiterLimit,
            };

            Geometry? workGeometry = Geometry.Clone().GetWidenedGeometry(strokePen);

            strokePen.Thickness = StrokeWidth;

            Geometry originGeo = Geometry.Clone();
            Geometry fullRing = originGeo.GetWidenedGeometry(strokePen);
            Geometry innerRing = new CombinedGeometry(GeometryCombineMode.Intersect, fullRing, originGeo);
            Geometry LargeGeometry = new CombinedGeometry(GeometryCombineMode.Union, originGeo, innerRing);

            Geometry outerRing = new CombinedGeometry(GeometryCombineMode.Exclude, fullRing, originGeo);
            Geometry outerStrokeBlock = new CombinedGeometry(GeometryCombineMode.Union, originGeo, outerRing);

            switch (StrokePosition)
            {
                case Models.StrokePosition.Outside:
                    workGeometry = outerStrokeBlock.Clone();
                    break;
                case Models.StrokePosition.Inside:
                    workGeometry = new CombinedGeometry
                    {
                        GeometryCombineMode = GeometryCombineMode.Exclude,
                        Geometry1 = originGeo,
                        Geometry2 = fullRing
                    };
                    break;
                case Models.StrokePosition.Center:
                    workGeometry = originGeo;
                    break;
            }

            strokePen.Thickness = StrokeWidth;

            switch (StrokeType)
            {
                case Models.StrokeType.Dashed:
                case Models.StrokeType.Custom:
                    AvaloniaList<double> dashArray = new AvaloniaList<double>();

                    if (StrokeDashArray != null && StrokeDashArray.Count() > 0)
                    {
                        dashArray.AddRange(StrokeDashArray.Select(x => x / strokePen.Thickness).ToArray());
                    }

                    strokePen.DashStyle = new DashStyle(dashArray, StrokeDashOffset);
                    break;
            }

            var workGeometryClone = workGeometry.Clone();
            if (workGeometry != null)
            {
                var backPen = new Pen(Brushes.Black)
                {
                    Thickness = StrokeWidth,
                    LineJoin = penLineJoin,
                    LineCap = penLineCap,
                    MiterLimit = StrokeMiterLimit,
                };

                this.Clip = workGeometryClone.GetWidenedGeometry(backPen);
            }

            if (workGeometry != null)
            {
                _imageBackground.Clip = workGeometryClone.GetWidenedGeometry(strokePen);
            }
        }

        private void RebuildStroke2()
        {
            if (Geometry == null || StrokeWidth <= 0)
                return;

            double baseThickness = StrokeWidth;
            double halfThick = baseThickness / 2;
            // 仅克隆一次原始几何体，全程复用
            Geometry originGeo = Geometry.Clone();

            #region 统一构建完整参数Pen（所有属性一次性配置完成，不再中途修改）
            PenLineJoin penLineJoin = StrokeJoin switch
            {
                StrokeJoin.Miter => PenLineJoin.Miter,
                StrokeJoin.Round => PenLineJoin.Round,
                StrokeJoin.Bevel => PenLineJoin.Bevel,
                _ => PenLineJoin.Miter
            };

            PenLineCap penLineCap = (StrokeEndpoint ?? Models.StrokeEndpoints.Butt) switch
            {
                StrokeEndpoints.Butt => PenLineCap.Flat,
                StrokeEndpoints.Round => PenLineCap.Round,
                StrokeEndpoints.Square => PenLineCap.Square,
                _ => PenLineCap.Flat
            };

            strokePen = new Pen(Brushes.Black)
            {
                Thickness = baseThickness,
                LineJoin = penLineJoin,
                LineCap = penLineCap,
                MiterLimit = StrokeMiterLimit,
            };

            // 虚线提前配置，所有加宽操作共用一套完整Pen
            if (StrokeType is Models.StrokeType.Dashed or Models.StrokeType.Custom && StrokeDashArray != null)
            {
                AvaloniaList<double> dashArray = new AvaloniaList<double>();
                dashArray.AddRange(StrokeDashArray.Select(x => x / strokePen.Thickness).ToArray());
                strokePen.DashStyle = new DashStyle(dashArray, StrokeDashOffset);
            }
            #endregion

            Geometry? strokeClipGeo = null;
            Pen offsetPen = strokePen;

            switch (StrokePosition)
            {
                case Models.StrokePosition.Outside:
                    // 外描边：原始向外加宽半厚，环形作为Clip
                    offsetPen.Thickness = baseThickness;
                    Geometry outerWide = originGeo.GetWidenedGeometry(offsetPen);
                    strokeClipGeo = new CombinedGeometry(GeometryCombineMode.Exclude, outerWide, originGeo);
                    break;

                case Models.StrokePosition.Inside:
                    // 内描边：原始向内收缩半厚，原图减收缩图形得到内侧环形
                    offsetPen.Thickness = baseThickness;
                    Geometry innerWide = originGeo.GetWidenedGeometry(offsetPen);
                    // 收缩：原图 Exclude 外扩区域，得到内部镂空小图形
                    Geometry innerShrink = new CombinedGeometry(GeometryCombineMode.Exclude, originGeo, innerWide);
                    strokeClipGeo = new CombinedGeometry(GeometryCombineMode.Exclude, originGeo, innerShrink);
                    break;

                case Models.StrokePosition.Center:
                    // 居中描边：直接标准加宽环形
                    offsetPen.Thickness = baseThickness;
                    strokeClipGeo = originGeo.GetWidenedGeometry(offsetPen);
                    break;
            }

            // 直接赋值Clip，禁止二次加宽叠加
            this.Clip = strokeClipGeo;
        }
        /// <summary>
        /// 解析虚线数组字符串
        /// </summary>
        public static double[] ParseDashArray(string? definition)
        {
            if (string.IsNullOrWhiteSpace(definition))
                return Array.Empty<double>();

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var result = new List<double>();

            foreach (var part in parts)
            {
                if (double.TryParse(part.Trim(), out double value) && value >= 0)
                {
                    result.Add(value);
                }
            }

            return result.ToArray();
        }

        #region 更新方法

        /// <summary>
        /// 重建背景填充：根据 StrokeFills（多层填充）或 StrokeColor 生成背景 Brush，
        /// 赋值给 _imageBackground.Background。
        /// </summary>
        public void UpdateStrokeBackground()
        {
            if (StrokeFills != null && StrokeFills.Count > 0)
            {
                List<FillItem> fillItems = new();
                if (StrokeFill != null && StrokeFill.Opacity > 0)
                {
                    fillItems.AddRange(StrokeFill);
                }
                fillItems.AddRange(StrokeFills);
                _imageBackground.Background = PathHelper.BuildMultiFillBrush(fillItems.ToArray(), this.Width, this.Height);
            }
            else if (StrokeFill != null)
            {
                _imageBackground.Background = PathHelper.BuildMultiFillBrush(new FillItem[] { StrokeFill }, this.Width, this.Height);
            }
            else
            {
                _imageBackground.Background = Brushes.Transparent;
            }
        }

        /// <summary>
        /// 设置描边依附的几何路径，触发 PropertyChanged → 重新构建画笔和裁剪形状。
        /// </summary>
        public void UpdateGeometry(Geometry geometry)
        {
            Geometry = geometry;
        }

        #endregion
    }
}
