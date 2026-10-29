using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.ComponentModel;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 圆角矩形 + StackPanel 容器控件。
    /// 继承自 <see cref="RoundedRactangleControlBase"/>，复用 _innerBorder / _contentPanel / 覆盖层等基类能力。
    /// 在此基础上叠加 StackPanel 容器：子元素通过 <see cref="IElementContainer"/> 接口添加，按 Orientation/HSpacing 排列。
    /// </summary>
    public class RoundedRectangleStackPanelControl : RoundedRactangleControlBase, IElementContainer
    {
        // 内部 StackPanel：按 Orientation / HSpacing 排列子元素。
        private readonly StackPanel _stackPanel = new()
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        public override Panel? Container => _stackPanel;

        public RoundedRectangleStackPanelControl(RoundedRectangleStackPanelElement element)
            : base(element)
        {
            // 初始化 StackPanel 方向 / 间距
            _stackPanel.Orientation = element.Orientation == StackPanelOrientation.Vertical
                ? Orientation.Vertical
                : Orientation.Horizontal;
            // 初始间距按当前方向取 HSpacing / VSpacing
            _stackPanel.Spacing = PropertyValidator.ValidateSpacing(
                _stackPanel.Orientation == Orientation.Vertical
                    ? element.VSpacing
                    : element.HSpacing);

            // 把 StackPanel 放到基类 _contentPanel 中（_contentPanel 已在基类构造函数中置入 _innerBorder）
            _contentPanel.Children.Add(_stackPanel);

            // 订阅额外属性变化（基类已订阅部分，这里补充与容器相关的属性）
            element.PropertyChanged += OnStackPanelElementPropertyChanged;
        }

        private void OnStackPanelElementPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not RoundedRectangleStackPanelElement element)
                return;

            switch (e.PropertyName)
            {
                case nameof(RoundedRectangleStackPanelElement.Orientation):
                    UpdateOrientation(element.Orientation);
                    break;
                case nameof(RoundedRectangleStackPanelElement.Spacing):
                    UpdateSpacing(element.Spacing);
                    break;
                case nameof(RoundedRectangleStackPanelElement.HSpacing):
                    UpdateHSpacing(element.HSpacing);
                    break;
                case nameof(RoundedRectangleStackPanelElement.VSpacing):
                    UpdateVSpacing(element.VSpacing);
                    break;
            }
        }

        public void UpdateOrientation(StackPanelOrientation orientation)
        {
            // FreeLayout / GridLayout 是占位选项，尚未实现；保留当前方向不变。
            if (orientation is StackPanelOrientation.FreeLayout or StackPanelOrientation.GridLayout)
                return;
            var oldOrientation = _stackPanel.Orientation;
            _stackPanel.Orientation = orientation == StackPanelOrientation.Vertical
                ? Orientation.Vertical
                : Orientation.Horizontal;

            // 方向切换时，从旧方向同步保持当前显示一致：以新方向对应的 HSpacing / VSpacing 覆盖 _stackPanel.Spacing。
            if (oldOrientation != _stackPanel.Orientation && _currentElement is RoundedRectangleStackPanelElement element)
            {
                double spacing = _stackPanel.Orientation == Orientation.Vertical
                    ? element.VSpacing
                    : element.HSpacing;
                _stackPanel.Spacing = PropertyValidator.ValidateSpacing(spacing);
            }
        }

        public void UpdateSpacing(double spacing)
        {
            _stackPanel.Spacing = PropertyValidator.ValidateSpacing(spacing);
        }

        /// <summary>水平子元素间距（仅在 Horizontal 方向生效）。</summary>
        public void UpdateHSpacing(double hSpacing)
        {
            // 当前控件实际只有一个 StackPanel.Spacing，按 Orientation 自动同步。
            if (_stackPanel.Orientation == Orientation.Horizontal)
            {
                _stackPanel.Spacing = PropertyValidator.ValidateSpacing(hSpacing);
            }
        }

        /// <summary>垂直子元素间距（仅在 Vertical 方向生效）。</summary>
        public void UpdateVSpacing(double vSpacing)
        {
            if (_stackPanel.Orientation == Orientation.Vertical)
            {
                _stackPanel.Spacing = PropertyValidator.ValidateSpacing(vSpacing);
            }
        }

        #region IElementContainer

        public override void RemoveChild(Control? childControl)
        {
            if (childControl == null)
                return;

            if (_stackPanel.Children.Contains(childControl))
            {
                _stackPanel.Children.Remove(childControl);

                if (this.Tag is CanvasElement element)
                {
                    if (childControl.Tag is CanvasElement childElement)
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

        public override void Insert(int insertedIndex, Control? childControl)
        {
            if (childControl == null)
                return;

            if (insertedIndex < 0)
                return;

            // 越界则附加到末尾
            if (insertedIndex > _stackPanel.Children.Count)
                insertedIndex = _stackPanel.Children.Count;

            _stackPanel.Children.Insert(insertedIndex, childControl);

            if (this.Tag is CanvasElement element)
            {
                if (childControl.Tag is CanvasElement childElement)
                {
                    childElement.Parent = element;

                    if (!element.Children.Contains(childElement))
                    {
                        element.Children.Insert(insertedIndex, childElement);
                    }

                    if (_stackPanel.Orientation == Orientation.Vertical)
                    {
                        childElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }
                    else
                    {
                        childElement.VerticalAlignment = VerticalAlignment.Stretch;
                    }

                    if (childControl is IElementControlBase elementControl)
                    {
                        elementControl.UpdateHorizontalAlignment(childElement);
                        elementControl.UpdateVerticalAlignment(childElement);
                    }
                }
            }
        }

        public override void Add(Control? childControl)
        {
            if (childControl == null)
                return;

            if (!_stackPanel.Children.Contains(childControl))
            {
                _stackPanel.Children.Add(childControl);

                if (this.Tag is CanvasElement element)
                {
                    if (childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = element;

                        if (!element.Children.Contains(childElement))
                        {
                            element.Children.Add(childElement);
                        }

                        if (_stackPanel.Orientation == Orientation.Vertical)
                        {
                            childElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                        }
                        else
                        {
                            childElement.VerticalAlignment = VerticalAlignment.Stretch;
                        }

                        if (childControl is IElementControlBase elementControl)
                        {
                            elementControl.UpdateHorizontalAlignment(childElement);
                            elementControl.UpdateVerticalAlignment(childElement);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
