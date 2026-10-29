using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using yuyiAvalonia.Base.Core;

namespace yuyiAvalonia.Base.Plugin
{
    public abstract class PluginBase : IPlugin
    {
        public PluginBase(IOptions<AppSettings> appSettings)
        {
            AppSettings = appSettings.Value;
        }
        public abstract string PluginName {get;  }

        protected AppSettings AppSettings { get; }

        public abstract void ConfigureMenu(SystemContext pluginContext);

        public abstract R? Execute<T, R>(string methodName, T? parameters) where R : class;

        public abstract void RegisterServices(IServiceCollection services);
    }
}
