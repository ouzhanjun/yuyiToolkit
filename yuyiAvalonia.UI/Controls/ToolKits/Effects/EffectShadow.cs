using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls;

namespace yuyiAvalonia.UI.Controls.ToolKits.Effects
{
    /// <summary>
    /// 几何图形阴影控件，通过路径描边加宽产生内外阴影效果。
    /// - IsInner=true：内阴影（CombinedGeometry.Exclude 裁剪路径边缘）
    /// - IsInner=false：外阴影（DropShadowEffect + 平移渲染）
    /// 所有 StyledProperty 均注册为 TwoWay，可在 XAML 直接绑定。
    /// </summary>
    public class EffectShadow : Panel
    {
        #region Styled Properties（阴影参数）

        /// <summary>阴影依附的几何路径（主路径）</summary>
        public static readonly StyledProperty<Geometry?> GeometryProperty =
            AvaloniaProperty.Register<EffectShadow, Geometry?>(
                nameof(Geometry),
                defaultBindingMode: BindingMode.TwoWay);

        public Geometry? Geometry
        {
            get => GetValue(GeometryProperty);
            set => SetValue(GeometryProperty, value);
        }

        /// <summary>
        /// 内阴影偏移后的几何路径（用于排除运算生成内阴影形状）。
        /// 若为 null，则使用 Geometry 自身经 Transform 后的位置进行Exclude运算。
        /// </summary>
        public static readonly StyledProperty<Geometry?> OffsetGeometryProperty =
            AvaloniaProperty.Register<EffectShadow, Geometry?>(
                nameof(OffsetGeometry),
                defaultBindingMode: BindingMode.TwoWay);

        public Geometry? OffsetGeometry
        {
            get => GetValue(OffsetGeometryProperty);
            set => SetValue(OffsetGeometryProperty, value);
        }

        /// <summary>阴影填充颜色</summary>
        public static readonly StyledProperty<Color> FillColorProperty =
            AvaloniaProperty.Register<EffectShadow, Color>(
                nameof(FillColor),
                defaultValue: Colors.Black,
                defaultBindingMode: BindingMode.TwoWay);

        public Color FillColor
        {
            get => GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }

        /// <summary>阴影模糊半径（像素）</summary>
        public static readonly StyledProperty<double> BlurProperty =
            AvaloniaProperty.Register<EffectShadow, double>(
                nameof(Blur),
                defaultValue: 1,
                defaultBindingMode: BindingMode.TwoWay);

        public double Blur
        {
            get => GetValue(BlurProperty);
            set => SetValue(BlurProperty, value);
        }

        /// <summary>阴影描边宽度（向内外各扩展 Spread 像素）</summary>
        public static readonly StyledProperty<double> SpreadProperty =
            AvaloniaProperty.Register<EffectShadow, double>(
                nameof(Spread),
                defaultValue: 0,
                defaultBindingMode: BindingMode.TwoWay);

        public double Spread
        {
            get => GetValue(SpreadProperty);
            set => SetValue(SpreadProperty, value);
        }

        /// <summary>阴影填充不透明度（0.0~1.0）</summary>
        public static readonly StyledProperty<double> FillOpacityProperty =
            AvaloniaProperty.Register<EffectShadow, double>(
                nameof(FillOpacity),
                defaultValue: 1,
                defaultBindingMode: BindingMode.TwoWay);

        public double FillOpacity
        {
            get => GetValue(FillOpacityProperty);
            set => SetValue(FillOpacityProperty, value);
        }

        /// <summary>
        /// 阴影类型：false=外阴影（默认），true=内阴影。
        /// 切换后 UpdateShadow() 会重新生成对应的渲染形状。
        /// </summary>
        public static readonly StyledProperty<bool> IsInnerProperty =
            AvaloniaProperty.Register<EffectShadow, bool>(
                nameof(IsInner),
                defaultValue: false,
                defaultBindingMode: BindingMode.TwoWay);

        public bool IsInner
        {
            get => GetValue(IsInnerProperty);
            set => SetValue(IsInnerProperty, value);
        }

        /// <summary>外阴影 X 方向偏移（像素），0 时内部用 0.001 防止渲染异常</summary>
        public static readonly StyledProperty<double> OffsetXProperty =
            AvaloniaProperty.Register<EffectShadow, double>(
                nameof(OffsetX),
                defaultValue: 3.5355,
                defaultBindingMode: BindingMode.TwoWay);

        public double OffsetX
        {
            get => GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }

        /// <summary>外阴影 Y 方向偏移（像素）</summary>
        public static readonly StyledProperty<double> OffsetYProperty =
            AvaloniaProperty.Register<EffectShadow, double>(
                nameof(OffsetY),
                defaultValue: 3.5355,
                defaultBindingMode: BindingMode.TwoWay);

        public double OffsetY
        {
            get => GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }

        #endregion

        // 内阴影路径：IsInner=true 时显示，通过 Exclude 运算生成边缘内凹形状
        protected Avalonia.Controls.Shapes.Path _innerShadowPath;

        // 外阴影容器（Panel），包含一个 Path；IsInner=false 时显示
        protected Panel _outShadowPanel;
        protected Avalonia.Controls.Shapes.Path _outerShadowPath;

        // 更新暂停标志：批量修改属性时先挂起，ResumeUpdate 后一次性重绘
        protected bool _isSuspend = false;

        public bool IsSuspend
        {
            get
            {
                return _isSuspend;
            }
        }

        /// <summary>
        /// 构造函数：初始化内/外阴影路径，并订阅属性变更事件。
        /// </summary>
        public EffectShadow()
        {
            InitInnerShadow();

            InitOuterShadow();

            PropertyChanged += EffectShadow_PropertyChanged;
        }

        /// <summary>
        /// 初始化内阴影路径（按需创建，仅执行一次）。
        /// </summary>
        private void InitInnerShadow()
        {
            if (_innerShadowPath == null)
            {
                _innerShadowPath = new Avalonia.Controls.Shapes.Path();

                this.Children.Add(_innerShadowPath);
            }
        }

        /// <summary>
        /// 初始化外阴影路径容器（按需创建，仅执行一次）。
        /// </summary>
        public void InitOuterShadow()
        {
            if (_outShadowPanel == null)
            {
                _outShadowPanel = new Panel();

                _outerShadowPath = new Avalonia.Controls.Shapes.Path
                {
                    IsHitTestVisible = true,
                };

                _outShadowPanel.Children.Add(_outerShadowPath!);

                this.Children.Add(_outShadowPanel);
            }
        }

        /// <summary>
        /// 属性变更回调：根据属性名决定是否触发 UpdateShadow() 重新渲染阴影。
        /// _isSuspend=true 时跳过更新，用于批量修改属性时避免重复重绘。
        /// </summary>
        private void EffectShadow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_isSuspend)
            {
                return;
            }

            switch (e.Property.Name)
            {
                case nameof(Geometry):
                case nameof(OffsetX):
                case nameof(OffsetY):
                case nameof(Blur):
                case nameof(Spread):
                case nameof(FillColor):
                case nameof(FillOpacity):
                case nameof(IsInner):
                case nameof(OffsetGeometry):
                    {
                        UpdateShadow();
                    }
                    break;
            }
        }

        /// <summary>
        /// 核心渲染方法：根据当前属性生成内阴影或外阴影。
        /// 内阴影：Exclude 运算将主路径与偏移路径相减，形成边缘内凹效果。
        /// 外阴影：平移路径副本 + BlurEffect + 半透明填充。
        /// </summary>
        private void UpdateShadow()
        {
            Geometry? workGeometry = null;
            if (Geometry != null)
            {
                _innerShadowPath.IsVisible = IsInner;
                _outShadowPanel.IsVisible = !IsInner;

                if (IsInner)
                {
                    double _spread = Spread > 0 ? Spread : 0.01;

                    Pen strokePen = new Pen(Brushes.Black)
                    {
                        Thickness = 10,
                    };

                    Geometry windened = Geometry.Clone().GetWidenedGeometry(strokePen);
                    var backGeometry = new CombinedGeometry
                    {
                        GeometryCombineMode = GeometryCombineMode.Union,
                        Geometry1 = windened.Clone(),
                        Geometry2 = Geometry.Clone()
                    };

                    Geometry offsetGeometry = OffsetGeometry != null ? OffsetGeometry.Clone() : Geometry.Clone();
                    workGeometry = new CombinedGeometry
                    {
                        GeometryCombineMode = GeometryCombineMode.Exclude,
                        Geometry1 = backGeometry.Clone(),
                        Geometry2 = offsetGeometry
                    };

                    strokePen.Thickness = _spread * 2;
                    workGeometry = new CombinedGeometry
                    {
                        GeometryCombineMode = GeometryCombineMode.Union,
                        Geometry1 = workGeometry.Clone(),
                        Geometry2 = workGeometry.Clone().GetWidenedGeometry(strokePen)
                    };


                    _innerShadowPath.Data = workGeometry;
                    _innerShadowPath.Fill = new SolidColorBrush(FillColor, FillOpacity);
                    _innerShadowPath.Effect = new BlurEffect { Radius = PropertyValidator.ValidateSpacing(Blur) };
                    //_innerShadowPath.StrokeThickness = PropertyValidator.ValidateSpacing(_spread * 2);
                    //_innerShadowPath.Stroke = new SolidColorBrush(FillColor, FillOpacity);

                }
                else
                {
                    var dropShadow = new DropShadowEffect
                    {
                        Color = FillColor,
                        BlurRadius = PropertyValidator.ValidateSpacing(Spread + Blur),
                        OffsetX = OffsetX,
                        OffsetY = OffsetY,
                        Opacity = FillOpacity
                    };

                    Pen strokePen = new Pen(Brushes.Black);
                    Geometry? orgGeometry = Geometry.Clone();
                    Geometry? windened = null;

                    if (Spread == 0)
                    {
                        workGeometry = orgGeometry;
                    }
                    else if (Spread > 0)
                    {
                        strokePen.Thickness = Spread * 2;
                        windened = Geometry.GetWidenedGeometry(strokePen);
                        workGeometry = new CombinedGeometry
                        {
                            GeometryCombineMode = GeometryCombineMode.Union,
                            Geometry1 = windened,
                            Geometry2 = Geometry
                        };
                    }
                    else if (Spread < 0)
                    {
                        strokePen.Thickness = Math.Abs(Spread) * 2;
                        windened = Geometry.GetWidenedGeometry(strokePen);
                        workGeometry = new CombinedGeometry
                        {
                            GeometryCombineMode = GeometryCombineMode.Exclude,
                            Geometry1 = Geometry,
                            Geometry2 = windened
                        };
                    }


                    workGeometry!.Transform = new TranslateTransform(OffsetX, OffsetY);

                    _outerShadowPath.Data = workGeometry;
                    _outerShadowPath.Fill = new SolidColorBrush(FillColor, FillOpacity);
                    _outerShadowPath.Effect = new BlurEffect { Radius = PropertyValidator.ValidateSpacing(Blur) };
                    //_outerShadowPath.StrokeThickness = PropertyValidator.ValidateSpacing(Spread * 2);
                    //_outerShadowPath.Stroke = new SolidColorBrush(FillColor, FillOpacity);

                    double offsetX = OffsetX == 0 ? 0.001 : OffsetX;
                    _outerShadowPath.RenderTransform = new TranslateTransform(offsetX, OffsetY);

                }
            }
        }

        /// <summary>
        /// 暂停属性变更触发的自动重绘，用于批量修改多个属性时避免频繁刷新。
        /// </summary>
        public void SuspendUpdate()
        {
            _isSuspend = true;
        }

        /// <summary>
        /// 恢复属性变更触发的自动重绘，恢复后若属性已变化则立即重绘。
        /// </summary>
        public void ResumeUpdate()
        {
            _isSuspend = false;
        }

        /// <summary>
        /// 一次性设置外阴影 X/Y 偏移量，暂停更新直到两值都设置完毕再统一重绘。
        /// </summary>
        public void OffsetOuterShadow(double offsetX, double offsetY)
        {
            SuspendUpdate();
            OffsetX = offsetX;
            OffsetY = offsetY;
            ResumeUpdate();
            UpdateShadow();
        }

        /// <summary>
        /// 设置内阴影的偏移几何路径，内部会暂停更新、设置后再统一重绘。
        /// </summary>
        public void OffsetInnerShadow(Geometry offsetGeometry)
        {
            OffsetGeometry = offsetGeometry;
            UpdateShadow();
        }

        public void UpdateGeometry(Geometry newGeometry)
        {
            if (newGeometry != null)
            {
                Geometry = newGeometry;
            }
        }
    }
}
