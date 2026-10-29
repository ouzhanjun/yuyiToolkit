using Avalonia.Controls;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls
{
    public interface IUIControlCreator
    {
        bool TryGetControl(CanvasElement element, out Control? control);
        Dictionary<CanvasElement, Control> ElementControlMap { get; }
        bool ShowElementId { get; set ; }
        void DeleteElement(CanvasElement element);

        Control? CreateElementControl(CanvasElement element, Panel? container = null);

        Control? CreateElementControl(ComponentDefinition definition, out CanvasElement element, Panel? container = null);

        (Control?, Panel?) CreateElementControlForPreview(CanvasElement element);
    }
}
