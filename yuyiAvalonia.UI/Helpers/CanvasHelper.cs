using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using yuyiAvalonia.UI.Controls;

namespace yuyiAvalonia.UI.Helpers
{
    public class CanvasHelper
    {
        /// <summary>
        /// 检查点是否在旋转后的矩形内（以左上角为旋转中心）。
        /// 通过 TranslatePoint 将点从顶级 Canvas 坐标系转换到控件局部坐标系，
        /// 再遍历父链逆推所有 RotateTransform 角，将点反旋回元素无旋转时的局部坐标，
        /// 最后在 [0, Width] × [0, Height] 范围内判定命中。
        /// </summary>
        /// <param name="point">点在顶级 Canvas 坐标系中的位置</param>
        /// <param name="checkControl">被检测的控件</param>
        /// <param name="TopParentCanvas">顶级 Canvas（TranslatePoint 的目标空间）</param>
        /// <returns>点是否在控件的轴对齐矩形内（含旋转前状态）</returns>
        public bool CheckPointInRotatedRect(Point point, Control checkControl, Control TopParentCanvas)
        {
            if (checkControl == null || TopParentCanvas == null)
                return false;

            // 1. 把点从 TopParentCanvas 坐标系转换到 checkControl 的本地坐标系
            var local = TopParentCanvas.TranslatePoint(point, checkControl);
            if (local == null)
                return false;

            double localX = local.Value.X;
            double localY = local.Value.Y;

            // 2. 遍历父链，逆推所有 RotateTransform 角，将点反旋回"元素无旋转时的局部坐标"
            double totalAngle = 0;
            var parent = checkControl.Parent as Control;
            while (parent != null && parent != TopParentCanvas)
            {
                if (parent.RenderTransform is RotateTransform rotate)
                    totalAngle += rotate.Angle;
                parent = parent.Parent as Control;
            }

            // 无旋转时，点的本地坐标直接判定
            if (Math.Abs(totalAngle) < 0.01)
                return localX >= 0 && localX <= checkControl.Width &&
                       localY >= 0 && localY <= checkControl.Height;

            // 3. 以 top-left (0,0) 为旋转中心，逆旋（负角）
            double rad = -totalAngle * Math.PI / 180;
            double cos = Math.Cos(rad), sin = Math.Sin(rad);
            double rx = localX * cos - localY * sin;
            double ry = localX * sin + localY * cos;

            return rx >= 0 && rx <= checkControl.Width &&
                   ry >= 0 && ry <= checkControl.Height;
        }

        /// <summary>
        /// 反推元素在层层上级容器（带旋转角）情况下，在顶级容器中的视觉位置（未旋转时）和旋转角。
        /// 使用 TranslatePoint 获取四个角点在顶级容器中的视觉位置，计算中心点，再用中心点和宽高计算左上角。
        /// </summary>
        /// <param name="control">控件（对应 element）</param>
        /// <param name="element">CanvasElement</param>
        /// <param name="TopParentCanvas">目标顶级 Canvas</param>
        /// <returns>元素在顶级容器中的轴对齐视觉信息，如果无法计算则返回 null</returns>
        public static AxisAlignedElementInfo? GetAxisAlignedTopLeft(Control control, Control TopParentCanvas)
        {
            if (control == null)
                return null;

            // 元素四个角点在本地坐标系中的位置
            var localCorners = new[]
                {
                new Point(0, 0),                         // 左上
                new Point(control.Width, 0),              // 右上
                new Point(control.Width, control.Height),  // 右下
                new Point(0, control.Height)              // 左下
            };

            // 使用 TranslatePoint 获取四个角点在 TopParentCanvas 坐标系中的视觉位置
            Point?[] visualCorners = new Point?[4];
            for (int i = 0; i < 4; i++)
            {
                visualCorners[i] = control.TranslatePoint(localCorners[i], TopParentCanvas);
                if (visualCorners[i] == null)
                    return null;
            }

            // 计算四个视觉角点的中心点
            double centerX = (visualCorners[0]!.Value.X + visualCorners[1]!.Value.X +
                              visualCorners[2]!.Value.X + visualCorners[3]!.Value.X) / 4;
            double centerY = (visualCorners[0]!.Value.Y + visualCorners[1]!.Value.Y +
                              visualCorners[2]!.Value.Y + visualCorners[3]!.Value.Y) / 4;

            var visualCenter = new Point(centerX, centerY);

            // 遍历父链，收集所有旋转角度（直到 TopParentCanvas 为止）
            double totalRotation = 0;
            var parent = control.Parent as Control;
            while (parent != null && parent != TopParentCanvas)
            {
                double angle = 0;

                if (parent.RenderTransform is RotateTransform rotate)
                {
                    angle = rotate.Angle; // 拿到旋转角 45
                }

                totalRotation += angle;
                parent = parent.Parent as Control;
            }

            // 用中心点和元素宽高计算轴对齐外包矩形的左上角
            double topLeftX = centerX - control.Width / 2;
            double topLeftY = centerY - control.Height / 2;

            return new AxisAlignedElementInfo(
                topLeft: new Point(topLeftX, topLeftY),
                center: visualCenter,
                width: control.Width,
                height: control.Height,
                totalRotation: totalRotation
            );
        }
    }
}
