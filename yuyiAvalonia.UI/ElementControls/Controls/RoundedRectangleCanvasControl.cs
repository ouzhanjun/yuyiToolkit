using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 圆角矩形 + Canvas 容器控件。
    /// 继承自 <see cref="RoundedRactangleControlBase"/>，复用 _innerBorder 布局更新、CornerRadius 同步等基类能力。
    /// 在此基础上叠加 Canvas 容器：内部以一个 Border（按圆角裁剪，由基类持有）包裹 Canvas，
    /// 子元素通过 <see cref="IElementContainer"/> 接口添加，按子元素自身的 Left/Top 自由定位。
    /// 与 <see cref="RoundedRectangleStackPanelControl"/> 平级：后者按 Orientation/Spacing 排列子元素，
    /// 本类按子元素绝对定位。
    /// </summary>
    public class RoundedRectangleCanvasControl : RoundedRactangleControlBase
    {
        // 内部 Canvas：子元素通过 Left/Top/Right/Bottom 自由定位。
        private readonly Canvas _canvas = new()
        {
            Background = Brushes.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        /// <summary>
        /// 内部 Panel 容器（子元素将被添加到其中）。
        /// 用于 FlowContainerManager / 其他依赖容器面板位置的代码。
        /// </summary>
        public override Panel? Container => _canvas;

        public RoundedRectangleCanvasControl(RoundedRectangleCanvasElement element)
            : base(element)
        {
            // 基类已将 _innerBorder 和 _contentPanel 加入 _canvasBackground。
            // 把 Canvas 放到基类 _contentPanel 中。
            _contentPanel.Children.Add(_canvas);

            // 订阅元素属性变化
            element.PropertyChanged += OnElementPropertyChanged;

            // 初始布局已在基类构造函数中同步，此处无需重复调用。
        }

        private void OnElementPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not RoundedRectangleCanvasElement)
                return;

            // _innerBorder 大小 / Padding / CornerRadius 由基类 RoundedRactangleControlBase 自动同步，
            // 此处空实现即可（基类已订阅 _currentElement.PropertyChanged 并处理 Padding）。
        }

        #region IElementContainer

        public override void Add(Control? childControl)
        {
            if (childControl == null)
                return;

            if (!_canvas.Children.Contains(childControl))
            {
                _canvas.Children.Add(childControl);

                if (this.Tag is CanvasElement element)
                {
                    if (childControl.Tag is CanvasElement childElement)
                    {
                        childElement.Parent = element;

                        if (!element.Children.Contains(childElement))
                        {
                            element.Children.Add(childElement);
                        }

                        // Canvas 容器：子元素按其自身的 Left/Top 自由定位，
                        // 不像 StackPanel 那样强制拉伸对齐。
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
            if (insertedIndex > _canvas.Children.Count)
                insertedIndex = _canvas.Children.Count;

            _canvas.Children.Insert(insertedIndex, childControl);

            if (this.Tag is CanvasElement element)
            {
                if (childControl.Tag is CanvasElement childElement)
                {
                    childElement.Parent = element;

                    if (!element.Children.Contains(childElement))
                    {
                        element.Children.Insert(insertedIndex, childElement);
                    }
                }
            }
        }

        public override void RemoveChild(Control? childControl)
        {
            if (childControl == null)
                return;

            if (_canvas.Children.Contains(childControl))
            {
                _canvas.Children.Remove(childControl);

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

        #endregion
    }
}
