namespace yuyiAvalonia.Base.Controls.Models
{
    public class EnumItem
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";

        /// <summary>
        /// 显示文本。如果未设置，使用 Name 作为默认值。
        /// </summary>
        public string DisplayText { get; set; } = "";

        public EnumItem() { }

        public EnumItem(string name, string value)
        {
            Name = name;
            Value = value;
            DisplayText = name;
        }

        public EnumItem(string name, string value, string displayText)
        {
            Name = name;
            Value = value;
            DisplayText = displayText;
        }
    }
}
