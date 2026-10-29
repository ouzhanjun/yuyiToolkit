using Avalonia.Media;

namespace yuyiAvalonia.UI.Models
{
    public class StrokeTypeItem
    {
        public StrokeType Index { get; set; }

        public string Type { get; set; } = "";

        public StreamGeometry? Icon { get; set; }

        public StrokeTypeItem(StrokeType index, string type, StreamGeometry? icon = null)
        {
            Index = index;
            Type = type;
            Icon = icon;
        }
    }
}
