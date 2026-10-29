using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.UI.Helpers;

namespace yuyiAvalonia.UI.Models
{

    /// <summary>
    /// 画布尺寸分类
    /// </summary>
    public enum CanvasSizeCategory
    {
        Mobile,    // 手机
        Tablet,    // 平板
        Other      // 其他
    }

    /// <summary>
    /// 画布尺寸预设
    /// </summary>
    public partial class CanvasSize : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private double _width;

        [ObservableProperty]
        private double _height;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private CanvasSizeCategory _category = CanvasSizeCategory.Other;

        /// <summary>
        /// 获取对应的图标
        /// </summary>
        public Avalonia.Media.Geometry Icon
        {
            get
            {
                return Category switch
                {
                    CanvasSizeCategory.Mobile => IconHelper.MobileGeometry,
                    CanvasSizeCategory.Tablet => IconHelper.TabletGeometry,
                    _ => IconHelper.PageGeometry
                };
            }
        }
    }

}
