using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yuyiAvalonia.ImageEdit.Common
{
    /// <summary>
    /// 编辑工具模式
    /// </summary>
    public enum EditToolMode
    {
        Normal,
        Select,   // 选择/移动模式
        Crop,     // 裁剪模式
        Brush,    // 画笔模式
        Text,     // 文字模式
    }
}
