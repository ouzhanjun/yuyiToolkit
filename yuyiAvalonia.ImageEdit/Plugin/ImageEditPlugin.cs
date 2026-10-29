using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Localization;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.Plugin;
using yuyiAvalonia.Base.Services;
using yuyiAvalonia.ImageEdit.ViewModels;
using yuyiAvalonia.ImageEdit.Commands;

namespace yuyiAvalonia.ImageEdit.Plugin
{
    public class MethodParameter
    {
        public string Name { get; set; }
    }

    public class ImageEditPlugin : PluginBase
    {
        public override string PluginName => "First";

        public ImageEditPlugin(IOptions<AppSettings> appSettings) : base(appSettings)
        {
        }

        public override void ConfigureMenu(SystemContext context)
        {
            var item = context.MenuItems.FirstOrDefault(x => x.Key == "ImageEdit");
            if (item != null)
            {
                return;
            }

            List<CustomMenuItem> items = new List<CustomMenuItem>();

            CustomMenuItem mainItem = new CustomMenuItem
            {
                Key = "ImageEdit",
                LocaleKey = "Menu.Toolbar.ImageEdit",
                Group = "Toolbar",
                CommandType = nameof(MenuClickCommandType.Active),
                CommandParameter = "ImageEdit",
                Icon = "M1058.21710513 612.18421027V756.24342136a168.75 168.75 0 0 1-168.75 168.75H267.75657864A168.75 168.75 0 0 1 99.00657864 756.24342136V267.75657864A168.75 168.75 0 0 1 267.75657864 99.00657864h621.71052649A168.75 168.75 0 0 1 1058.21710513 267.75657864v344.42763163z m-71.05263162-55.50986838V267.75657864A97.69736838 97.69736838 0 0 0 889.46710513 170.05921027H267.75657864A97.69736838 97.69736838 0 0 0 170.05921027 267.75657864v488.48684272A97.69736838 97.69736838 0 0 0 267.75657864 853.94078973h74.29440812l330.52796027-324.31085513a168.75 168.75 0 0 1 190.6875-31.8848692l123.89802648 58.92927649z m0 78.69078973l-154.45065811-73.49506622a97.69736838 97.69736838 0 0 0-110.39802567 18.51809271L443.52302649 853.94078973H889.46710513a97.69736838 97.69736838 0 0 0 97.69736838-97.69736837v-120.87828974zM356.57236838 525.32236838a124.34210513 124.34210513 0 1 0 0-248.68421027 124.34210513 124.34210513 0 0 0 0 248.68421027z m0-71.05263162a53.28947351 53.28947351 0 1 1 0-106.57894702 53.28947351 53.28947351 0 0 1 0 106.57894702z"
            };
            items.Add(mainItem);

            context.AddMenus(items);

        }

        public override R? Execute<T, R>(string methodName, T? parameters)
            where T : default
            where R : class
        {
            return methodName switch
            {
                "method1" => Method1(parameters as MethodParameter) as R,
                _ => throw new Exception("插件方法不存在")
            };
        }

        private PluginResult Method1(MethodParameter? parameter)
        {
            //方法逻辑
            return new PluginResult(true);
        }

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IPlugin, ImageEditPlugin>();
            services.AddTransient<ImageEditViewModel>();
            services.AddCommand(new ImageEditCommand(GlobalProvider.SystemContext));
            services.AddSingleton<ILocalizaitonResourceContributor, LocalizationResourceContributor>();

        }
    }
}