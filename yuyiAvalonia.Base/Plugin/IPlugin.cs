using Microsoft.Extensions.DependencyInjection;
using yuyiAvalonia.Base.Core;

namespace yuyiAvalonia.Base.Plugin
{
    /// <summary>
    /// 插件接口
    /// </summary>
    public interface IPlugin
    {
        string PluginName { get; }

        R? Execute<T, R>(string methodName, T? parameters) where  R : class;

        void RegisterServices(IServiceCollection services);

        void ConfigureMenu(SystemContext pluginContext);
    }
}
