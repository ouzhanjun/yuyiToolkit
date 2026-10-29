using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Xml.Linq;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public interface IElementControlBase
    {
        Control Control { get; }
        bool IsShowOverlay { get; }

        #region 通用属性
        void HideWorkspaceOverlay();
        void ParentSizeChanged();
        void ShowWorkspaceOverlay();

        void UpdateElement(CanvasElement element);
        void UpdateHeight(double height);
        void UpdateHorizontalAlignment(CanvasElement element);
        void UpdateIdLabel(CanvasElement element);
        void UpdateIsVisible(bool isVisible);
        void UpdateMargin(CanvasElement element);
        void UpdateOpacity(double opacity);
        void UpdatePadding(CanvasElement element);

        void UpdateVerticalAlignment(CanvasElement element);
        void UpdateWidth(double width);
        #endregion

        #region 位置

        public void UpdateZIndex(int zIndex);

        public void UpdateRotation(double rotation);

        public void UpdateLeft(double? value);

        public void UpdateTop(double? value);

        public void UpdateRight(double? value);

        public void UpdateBottom(double? value);

        #endregion

        #region 背景填充

        void ApplyFillType(FillItem? fillItem);

        public void ApplyMultiFill(CanvasElement element);

        #endregion

        #region 描边

        void UpdateStroke(CanvasElement element);
        void UpdateBorderColor(string color);
        void UpdateBorderWidth(double width);
        void UpdateCornerRadius(double radius);
        void ApplyMultiFillsStroke(CanvasElement element, double actualWidth, double actualHeight);
        void ApplyStrokePath(CanvasElement element);

        #endregion

    }
}