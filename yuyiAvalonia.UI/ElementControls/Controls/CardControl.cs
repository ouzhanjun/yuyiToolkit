using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class CardControl : BorderControlBase
    {
        private Border? _workspaceOverlay;

        public override StackPanel? Control
        {
            get
            {
                return control as StackPanel;
            }
        }

        public CardControl(CardElement element):base(element)
        {
            // 使用基类的 control 字段
            control = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Spacing = PropertyValidator.ValidateSpacing(element.Spacing),
                Background = Brushes.Transparent,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };

            // 创建工作区覆盖层
            _workspaceOverlay = new Border
            {
                IsHitTestVisible = false,
                IsVisible = false,
                Background = WorkspaceBrush,
                BorderBrush = new ImmutableSolidColorBrush(Color.Parse("#80CC66CC"), 0.6),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
            };

            // 将内部 StackPanel 添加到内容 Grid
            _contentPanel!.Children.Add(control);

            // 将覆盖层添加到内容 Grid
            _contentPanel!.Children.Add(_workspaceOverlay);

            // 将内容 Grid 设置为 Border 的 Child
            this.Child = _contentPanel;

            // 初始化描边控件
            InitStrokes();
        }

        public override void UpdateElement(CanvasElement element)
        {
            CardElement cardElement = (CardElement)element;
            base.UpdateElement(cardElement);

            if (Control != null)
            {
                this.UpdateSpacing(cardElement.Spacing);
            }
        }

        public void UpdateSpacing(double spacing)
        {
            if (Control != null)
            {
                Control.Spacing = PropertyValidator.ValidateSpacing(spacing);
            }
        }

        /// <summary>
        /// 显示工作区覆盖层
        /// </summary>
        public override void ShowWorkspaceOverlay()
        {
            if (_workspaceOverlay == null || _contentPanel == null) return;

            _contentPanel.Children.Remove(_workspaceOverlay);
            _contentPanel.Children.Add(_workspaceOverlay);

            _workspaceOverlay.IsVisible = true;
            _isShowOverlay = _workspaceOverlay.IsVisible;
        }

        /// <summary>
        /// 隐藏工作区覆盖层
        /// </summary>
        public override void HideWorkspaceOverlay()
        {
            if (_workspaceOverlay != null)
            {
                _workspaceOverlay.IsVisible = false;
                _isShowOverlay = _workspaceOverlay.IsVisible;
            }
        }

        /// <summary>
        /// 工作区覆盖层画刷
        /// </summary>
        public static readonly StyledProperty<IBrush> WorkspaceBrushProperty =
            AvaloniaProperty.Register<CardControl, IBrush>(nameof(WorkspaceBrush),
                new ImmutableSolidColorBrush(Color.Parse("#80CC66CC"), 0.15));

        public IBrush WorkspaceBrush
        {
            get => GetValue(WorkspaceBrushProperty);
            set => SetValue(WorkspaceBrushProperty, value);
        }
    }
}
