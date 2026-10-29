using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{

    /// <summary>
    /// 按钮元素
    /// </summary>
    public partial class ButtonElement : TextElement
    {
        public ButtonElement() : base()
        {
            ElementType = ComponentType.Button;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;

            Text = "按钮";
            ForeColor = "#FFFFFF";
            TextAlignment = TextAlignment.Center;
        }

        [ObservableProperty]
        private ButtonStyle _buttonStyle = ButtonStyle.Primary;

        public override CanvasElement Clone()
        {
            var element = (ButtonElement)base.Clone();

            element.ButtonStyle = ButtonStyle;

            return element;
        }
    }

}
