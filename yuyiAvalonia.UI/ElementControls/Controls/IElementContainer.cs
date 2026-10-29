using Avalonia.Controls;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public interface IElementContainer
    {
        public void RemoveChild(Control? childControl);

        public void Insert(int insertedIndex, Control? childControl);

        public void Add(Control? childControl);
    }
}
