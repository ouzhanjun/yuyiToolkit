using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{
    /// <summary>
    /// 基本图形元素基类
    /// </summary>
    public abstract partial class ShapeElement : CanvasElement
    {
        [ObservableProperty]
        private ObservableCollection<Point> points = new();

        [ObservableProperty]
        private bool _drawPoint = true;

        public ShapeElement()
        {
        }
    }

}
