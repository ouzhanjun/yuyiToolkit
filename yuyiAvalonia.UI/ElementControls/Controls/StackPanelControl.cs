using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ColorTextBlock.Avalonia;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class StackPanelControl : BorderControlBase, IElementContainer
    {
        private Border? _workspaceOverlay;

        public override StackPanel? Control
        {
            get
            {
                return control as StackPanel;
            }
        }

        public StackPanelControl(StackPanelElement element) : base(element)
        {
            // 使用基类的 control 字段
            control = new StackPanel
            {
                Orientation = element.Orientation == StackPanelOrientation.Vertical
                    ? Avalonia.Layout.Orientation.Vertical
                    : Avalonia.Layout.Orientation.Horizontal,
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
            StackPanelElement stackElement = (StackPanelElement)element;
            base.UpdateElement(stackElement);

            this.UpdateOrientation(stackElement.Orientation);
            this.UpdateSpacing(stackElement.Spacing);
        }

        public void UpdateOrientation(StackPanelOrientation orientation)
        {
            if (control is StackPanel _panel)
            {
                _panel.Orientation = orientation == StackPanelOrientation.Vertical
                    ? Avalonia.Layout.Orientation.Vertical
                    : Avalonia.Layout.Orientation.Horizontal;
            }
        }

        public void UpdateSpacing(double spacing)
        {
            if (control is StackPanel _panel)
            {
                _panel.Spacing = PropertyValidator.ValidateSpacing(spacing);
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
            AvaloniaProperty.Register<StackPanelControl, IBrush>(nameof(WorkspaceBrush),
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

                            if(Control.Orientation==Avalonia.Layout.Orientation.Vertical)
                            {
                                childElement.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                            }
                            else
                            {
                                childElement.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                            }

                            IElementControlBase elementControl = childControl as IElementControlBase;
                            if (elementControl != null)
                            {
                                // 设置子元素填充整个单元格
                                elementControl.UpdateHorizontalAlignment(childElement);
                                elementControl.UpdateVerticalAlignment(childElement);
                            }
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

                        if (Control.Orientation == Avalonia.Layout.Orientation.Vertical)
                        {
                            childElement.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                        }
                        else
                        {
                            childElement.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                        }

                        // 设置子元素填充整个单元格
                        IElementControlBase elementControl = childControl as IElementControlBase;
                        if (elementControl != null)
                        {
                            // 设置子元素填充整个单元格
                            elementControl.UpdateHorizontalAlignment(childElement);
                            elementControl.UpdateVerticalAlignment(childElement);
                        }
                    }
                }
            }
        }
    }
}
