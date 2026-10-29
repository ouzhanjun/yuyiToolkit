namespace yuyiAvalonia.Base.Plugin
{
    /// <summary>
    /// 插件执行结果类
    /// </summary>
    /// <param name="ok"></param>
    public class PluginResult(bool ok)
    {
        public bool? OK { get; set; } = ok;


        public static PluginResult SetOk(bool ok)
        {
            return new PluginResult(ok);
        }
    }
}
