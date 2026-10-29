namespace yuyiAvalonia.UI.Models
{
    public class StrokeStyleItem
    {
        public StrokeStyleType Value { get; set; }

        public string DashArray { get; set; } = "";

        public StrokeType StrokeType { get; set; }

        public StrokeStyleItem(StrokeStyleType value, string dashArray, StrokeType strokeType)
        {
            Value = value;
            DashArray = dashArray;
            StrokeType = strokeType;
        }
    }

}
