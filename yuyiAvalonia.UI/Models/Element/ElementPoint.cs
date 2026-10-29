using Avalonia;

namespace yuyiAvalonia.UI.Models.Element
{
    public class ElementPoint
    {
        private double _x, _y;

        public double X { get => _x; set => _x = value; }

        public double Y { get => _y; set => _y = value; }

        public ElementPoint()
        { }

        public ElementPoint(double x, double y)
        {
            SetPoint(x, y);
        }

        public void SetPoint(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public Avalonia.Point GetPoint()
        {
            return new Point(X,Y);
        }

    }
}
