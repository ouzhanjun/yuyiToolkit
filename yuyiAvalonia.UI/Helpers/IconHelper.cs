using Avalonia.Media;
using yuyiAvalonia.Base.Helpers;

namespace yuyiAvalonia.UI.Helpers
{
    /// <summary>
    /// 图标辅助类，提供XAML可访问的几何图标
    /// </summary>
    public static class IconHelper
    {
        public static Geometry AlignLeftGeometry => Icon.AlignLeft;
        public static Geometry AlignHorizontalCenterGeometry => Icon.AlignHorizontalCenter;
        public static Geometry AlignRightGeometry => Icon.AlignRight;
        public static Geometry AlignTopGeometry => Icon.AlignTop;
        public static Geometry AlignVerticalCenterGeometry => Icon.AlignVerticalCenter;
        public static Geometry AlignBottomGeometry => Icon.AlignBottom;
        public static Geometry HorizontalSpacingGeometry => Icon.HorizontalSpacing;
        public static Geometry VerticalSpacingGeometry => Icon.VerticalSpacing;
        public static Geometry SameSizeGeometry => Icon.SameSize;
        public static Geometry SameWidthGeometry => Icon.SameWidth;
        public static Geometry SameHeightGeometry => Icon.SameHeight;
        public static Geometry DeleteGeometry => Icon.Delete;
        public static Geometry ClearGeometry => Icon.Clear;

        public static Geometry Preview => Icon.Preview;

        public static Geometry ExportHtml => Icon.ExportHtml;

        public static Geometry ResetSize => Icon.ResetSize;

        public static Geometry Save => Icon.Save;

        public static Geometry LoadFile => Icon.LoadFile;

        public static Geometry Hide => Icon.Hide;

        public static Geometry Display => Icon.Display;

        public static Geometry Undo => Icon.Undo;

        public static Geometry Redo => Icon.Redo;

        public static Geometry MobileGeometry => Icon.Phone;

        public static Geometry TabletGeometry => Icon.Tablet;

        public static Geometry PageGeometry => Icon.Page;

        public static Geometry MoveUp => Icon.MoveUp;

        public static Geometry MoveDown => Icon.MoveDown;
    }
}
