using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    /// <summary>
    /// 圆角矩形容器控件的通用基类。
    /// 继承自 <see cref="RoundedRectangleControl"/>，复用形状渲染、调试 overlay、圆角裁剪等能力。
    /// 在此基础上叠加容器：内部以一个 Border（按圆角裁剪）承载具体面板（StackPanel / Canvas / Grid），
    /// Border 自身不可点击，点击穿透到 _canvasBackground 触发形状选中。
    /// 同时支持工作区覆盖层（拖拽 hover 时显示），由基类统一管理，子类只需实现 <see cref="DrawWorkspaceOverlayContent"/>。
    /// 子类负责实现 <see cref="Container"/> 与 <see cref="IElementContainer"/> 的 Add/Insert/RemoveChild。
    /// </summary>
    public abstract class RoundedRactangleControlBase : RoundedRectangleControl, IElementContainer
    {
        // 内层 Border：承载 _contentPanel（具体面板 + 覆盖层），并按圆角矩形裁剪子元素。
        // Border 自身不可点击，点击穿透到 _canvasBackground 触发形状选中。
        protected readonly Border _innerBorder = new()
        {
            IsHitTestVisible = false,
            Background = Brushes.Transparent,
            ClipToBounds = true,
        };

        // 内容面板（具体容器 + 覆盖层都在它里面），用于 FlowContainerManager 算 local 坐标。
        // 子类通过 Container 属性对外暴露具体面板，并将子元素添加到 _contentPanel 中。
        protected readonly Panel _contentPanel = new()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            IsHitTestVisible = false,
        };

        // 工作区覆盖层（hover 状态时显示）。
        private Border? _workspaceOverlay;

        /// <summary>
        /// 内部 Panel 容器（子元素将被添加到其中）。
        /// 用于 FlowContainerManager / 其他依赖容器面板位置的代码。
        /// 容器子类重写以提供具体面板（StackPanel / Canvas / Grid）；
        /// 非容器子类保持默认（返回 null）。
        /// </summary>
        public virtual Panel? Container => null;

        /// <summary>
        /// 工作区覆盖层画刷。
        /// </summary>
        public static readonly StyledProperty<IBrush> WorkspaceBrushProperty =
            AvaloniaProperty.Register<RoundedRactangleControlBase, IBrush>(nameof(WorkspaceBrush),
                new ImmutableSolidColorBrush(Color.Parse("#80CC66CC"), 0.15));

        public IBrush WorkspaceBrush
        {
            get => GetValue(WorkspaceBrushProperty);
            set => SetValue(WorkspaceBrushProperty, value);
        }

        public RoundedRactangleControlBase(RoundedRectangleElement element)
            : base(element)
        {
            // 把 _contentPanel 放入 _innerBorder，再把 _innerBorder 加到 _canvasBackground 之上（_debugOverlay 之下）。
            _innerBorder.Child = _contentPanel;
            this._canvasBackground.Children.Insert(0, _innerBorder);

            // 创建工作区覆盖层
            _workspaceOverlay = new Border
            {
                IsHitTestVisible = false,
                IsVisible = false,
            };

            // 初始同步布局
            UpdateInnerLayout();
        }

        public override void UpdateShape(CanvasElement? shapeElement)
        {
            base.UpdateShape(shapeElement);
            UpdateInnerLayout();
        }

        /// <summary>
        /// 同步内部容器布局：让 _innerBorder 跟随形状矩形大小，并把内边距 (Padding) 留给具体面板。
        /// </summary>
        protected void UpdateInnerLayout()
        {
            if (_currentElement is not RoundedRectangleElement element)
                return;

            double w = this.Bounds.Width > 0 ? this.Bounds.Width : element.Width;
            double h = this.Bounds.Height > 0 ? this.Bounds.Height : element.Height;

            _innerBorder.Width = w;
            _innerBorder.Height = h;

            _contentPanel.Width = w;
            _contentPanel.Height = h;

            UpdatePadding(element);
        }

        public override void UpdatePadding(CanvasElement element)
        {
            _innerBorder.Padding = new Thickness(
                PropertyValidator.ValidateSpacing(element.PaddingLeft),
                PropertyValidator.ValidateSpacing(element.PaddingTop),
                PropertyValidator.ValidateSpacing(element.PaddingRight),
                PropertyValidator.ValidateSpacing(element.PaddingBottom));
        }

        /// <summary>
        /// 显示工作区覆盖层。子类可重写以自定义显示逻辑。
        /// </summary>
        public override void ShowWorkspaceOverlay()
        {
            if (_workspaceOverlay == null) return;

            _contentPanel.Children.Remove(_workspaceOverlay);
            _contentPanel.Children.Add(_workspaceOverlay);

            _contentPanel.InvalidateArrange();

            Avalonia.Threading.Dispatcher.UIThread.Post(() => DrawWorkspaceOverlayContent());

            _workspaceOverlay.IsVisible = true;
            _isShowOverlay = _workspaceOverlay.IsVisible;
        }

        /// <summary>
        /// 隐藏工作区覆盖层。
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
        /// 绘制工作区覆盖层内容（当覆盖层可见或大小变化时调用）。
        /// 默认实现为空白，子类可重写以绘制容器特有的覆盖层（如 Grid 的网格线）。
        /// </summary>
        protected virtual void DrawWorkspaceOverlayContent()
        {
            if (_workspaceOverlay == null) return;
            _workspaceOverlay.Child = null;
        }

        /// <summary>
        /// 获取工作区覆盖层 Border（子类可在 DrawWorkspaceOverlayContent 中使用）。
        /// </summary>
        protected Border? WorkspaceOverlayBorder => _workspaceOverlay;

        #region IElementContainer（基类提供空默认实现，容器子类按需重写）

        public virtual void RemoveChild(Control? childControl)
        {
        }

        public virtual void Insert(int insertedIndex, Control? childControl)
        {
        }

        public virtual void Add(Control? childControl)
        {
        }

        #endregion
    }
}
