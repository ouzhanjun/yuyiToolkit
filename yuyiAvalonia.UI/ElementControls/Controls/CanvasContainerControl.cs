using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class CanvasContainerControl : BorderControlBase, IElementContainer
    {
        private Border? _workspaceOverlay;

        public override Canvas? Control
        {
            get
            {
                return control as Canvas;
            }
        }

        public CanvasContainerControl(CanvasElement element) : base(element)
        {
            // 使用基类的 control 字段
            control = new Canvas
            {
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

            // 将内部 Canvas 添加到内容 Grid
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
            base.UpdateElement(element);
            // CanvasContainerControl 当前没有特殊的子属性需要更新
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
            AvaloniaProperty.Register<CanvasContainerControl, IBrush>(nameof(WorkspaceBrush),
                new ImmutableSolidColorBrush(Color.Parse("#80CC66CC"), 0.15));

        public IBrush WorkspaceBrush
        {
            get => GetValue(WorkspaceBrushProperty);
            set => SetValue(WorkspaceBrushProperty, value);
        }


        public void RemoveChild(Control? childControl)
        {
            if (childControl == null)
                return;

            if (Control != null && !Control.Children.Contains(childControl!))
            {
                Control.Children.Remove(childControl!);

                if (this.Tag != null && this.Tag is CanvasElement element)
                {
                    if (childControl.Tag != null && childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = null;

                        if (element.Children.Contains(childElement))
                        {
                            element.Children.Remove(childElement);
                        }
                    }
                }
            }
        }

        public void Insert(int insertedIndex, Control? childControl)
        {
            if (childControl == null)
                return;

            if (insertedIndex > -1)
            {
                Control!.Children.Insert(insertedIndex, childControl);

                if (this.Tag != null && this.Tag is CanvasElement element)
                {
                    if (childControl.Tag != null && childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = element;

                        if (!element.Children.Contains(childElement))
                        {
                            element.Children.Insert(insertedIndex, childElement);
                        }
                    }
                }
            }
        }

        public void Add(Control? childControl)
        {
            if (childControl == null)
                return;

            if (!Control!.Children.Contains(childControl))
            {
                Control.Children.Add(childControl);

                if (this.Tag != null && this.Tag is CanvasElement element)
                {
                    if (childControl.Tag != null && childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = element;

                        if (!element.Children.Contains(childElement))
                        {
                            element.Children.Add(childElement);
                        }
                    }
                }
            }
        }
    }
}
