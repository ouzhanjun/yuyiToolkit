namespace yuyiAvalonia.UI.Models.Element
{
    public class ElementSize
    {
        public ElementSize()
        { }

        public ElementSize(double width, double height)
        {
            SetSize(width, height);
        }

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double Height { get; set; }

        public double Width { get; set; }
    }
}
