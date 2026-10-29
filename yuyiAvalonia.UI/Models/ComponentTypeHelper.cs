namespace yuyiAvalonia.UI.Models
{
    public class ComponentTypeHelper
    {
        public static string GetTypeDisplayName(ComponentType type) => type switch
        {
            ComponentType.Text => "文本",
            ComponentType.Card => "卡片",
            ComponentType.Button => "按钮",
            ComponentType.Panel => "面板",
            ComponentType.VerticalStackPanel => "垂直布局",
            ComponentType.HorizontalStackPanel => "水平布局",
            ComponentType.Grid => "网格",
            ComponentType.Canvas => "画布",
            ComponentType.Image => "图片",
            _ => type.ToString()
        };

        public static string GetTypeIcon(ComponentType type) => type switch
        {
            ComponentType.Text => "\uE8D2",
            ComponentType.Card => "\uE8C8",
            ComponentType.Button => "\uE8D8",
            ComponentType.Panel => "\uE8A5",
            ComponentType.VerticalStackPanel => "\uE8C0",
            ComponentType.HorizontalStackPanel => "\uE8C1",
            ComponentType.Grid => "\uE80A",
            ComponentType.Canvas => "\uE80B",
            ComponentType.Image => "\uE8B9",
            _ => "\uE7C3"
        };
    }
}
