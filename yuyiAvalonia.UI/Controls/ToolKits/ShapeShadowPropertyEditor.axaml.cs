using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace yuyiAvalonia.UI.Controls
{
    public partial class ShapeShadowPropertyEditor : UserControl
    {
        public static readonly StyledProperty<bool> IsShadowEnabledProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, bool>(nameof(IsShadowEnabled));

        public static readonly StyledProperty<bool> IsGlowEnabledProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, bool>(nameof(IsGlowEnabled));

        public static readonly StyledProperty<double> ShadowOffsetXProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(ShadowOffsetX));

        public static readonly StyledProperty<double> ShadowOffsetYProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(ShadowOffsetY));

        public static readonly StyledProperty<double> ShadowSpreadProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(ShadowSpread));

        public static readonly StyledProperty<double> ShadowBlurProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(ShadowBlur));

        public static readonly StyledProperty<string> ShadowColorProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, string>(nameof(ShadowColor), "#000000");

        public static readonly StyledProperty<double> GlowOffsetXProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(GlowOffsetX));

        public static readonly StyledProperty<double> GlowOffsetYProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(GlowOffsetY));

        public static readonly StyledProperty<double> GlowBlurRadiusProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(GlowBlurRadius));

        public static readonly StyledProperty<double> GlowOpacityProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, double>(nameof(GlowOpacity), 1.0);

        public static readonly StyledProperty<string> GlowColorProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, string>(nameof(GlowColor), "#FFFFFF");

        public static readonly StyledProperty<ICommand?> ClearEffectsCommandProperty =
            AvaloniaProperty.Register<ShapeShadowPropertyEditor, ICommand?>(nameof(ClearEffectsCommand));

        public bool IsShadowEnabled
        {
            get => GetValue(IsShadowEnabledProperty);
            set => SetValue(IsShadowEnabledProperty, value);
        }

        public bool IsGlowEnabled
        {
            get => GetValue(IsGlowEnabledProperty);
            set => SetValue(IsGlowEnabledProperty, value);
        }

        public double ShadowOffsetX
        {
            get => GetValue(ShadowOffsetXProperty);
            set => SetValue(ShadowOffsetXProperty, value);
        }

        public double ShadowOffsetY
        {
            get => GetValue(ShadowOffsetYProperty);
            set => SetValue(ShadowOffsetYProperty, value);
        }

        public double ShadowSpread
        {
            get => GetValue(ShadowSpreadProperty);
            set => SetValue(ShadowSpreadProperty, value);
        }

        public double ShadowBlur
        {
            get => GetValue(ShadowBlurProperty);
            set => SetValue(ShadowBlurProperty, value);
        }

        public string ShadowColor
        {
            get => GetValue(ShadowColorProperty);
            set => SetValue(ShadowColorProperty, value);
        }

        public double GlowOffsetX
        {
            get => GetValue(GlowOffsetXProperty);
            set => SetValue(GlowOffsetXProperty, value);
        }

        public double GlowOffsetY
        {
            get => GetValue(GlowOffsetYProperty);
            set => SetValue(GlowOffsetYProperty, value);
        }

        public double GlowBlurRadius
        {
            get => GetValue(GlowBlurRadiusProperty);
            set => SetValue(GlowBlurRadiusProperty, value);
        }

        public double GlowOpacity
        {
            get => GetValue(GlowOpacityProperty);
            set => SetValue(GlowOpacityProperty, value);
        }

        public string GlowColor
        {
            get => GetValue(GlowColorProperty);
            set => SetValue(GlowColorProperty, value);
        }

        public ICommand? ClearEffectsCommand
        {
            get => GetValue(ClearEffectsCommandProperty);
            set => SetValue(ClearEffectsCommandProperty, value);
        }

        private bool _isUpdatingBindings = false;

        public ShapeShadowPropertyEditor()
        {
            InitializeComponent();

            // 注册属性变化处理器
            IsShadowEnabledProperty.Changed.AddClassHandler<ShapeShadowPropertyEditor>((sender, e) => sender.OnIsShadowEnabledChanged(e));
            IsGlowEnabledProperty.Changed.AddClassHandler<ShapeShadowPropertyEditor>((sender, e) => sender.OnIsGlowEnabledChanged(e));
        }

        private void InitializeComponent()
        {
            Avalonia.Controls.Shapes.Path path = new Avalonia.Controls.Shapes.Path();
        }

        private void OnIsShadowEnabledChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (_isUpdatingBindings) return;

            if (e.NewValue is true)
            {
                // 启用阴影时，关闭发光
                _isUpdatingBindings = true;
                IsGlowEnabled = false;
                _isUpdatingBindings = false;
            }
        }

        private void OnIsGlowEnabledChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (_isUpdatingBindings) return;

            if (e.NewValue is true)
            {
                // 启用发光时，关闭阴影
                _isUpdatingBindings = true;
                IsShadowEnabled = false;
                _isUpdatingBindings = false;
            }
        }
    }
}
