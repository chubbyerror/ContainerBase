using DevelopBase.Common;
using DevelopBase.Protocol;
using Microsoft.Extensions.DependencyInjection;
using System;
using LocalDiscovery;
using DevelopBase.DPBase;
using DPServer;

namespace TestServer
{
    class Program
    {
        public static IServiceProvider _serviceProvider = null;

        static void Main(string[] args)
        {

            //创建依赖注入收集器
            ServiceCollection service = new ServiceCollection();

            //读取配置文件
            Newtonsoft.Json.Linq.JObject cfgobj = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(System.IO.File.ReadAllTextAsync("config.json").Result);


            //手写动态注册信息
            //add handle
            var handle = Newtonsoft.Json.JsonConvert.DeserializeObject<RegisterInfo[]>(cfgobj["handle"].ToString());
            service.AddHandlers(handle);
            //add servers
            var services = Newtonsoft.Json.JsonConvert.DeserializeObject<RegisterInfo[]>(cfgobj["server"].ToString());
            service.AddServices(services);

            //设置服务器配置信息
            var srvconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<DPBase.BaseServerInfo>(cfgobj["srvconfig"].ToString());
            //创建通讯实例和服务注册实例
            BaseService sbase = new BaseService(service,srvconfig);
            
            //创建依赖注入容器
            _serviceProvider = service.BuildServiceProvider();

            sbase.StartService(_serviceProvider);

            sbase.RegisterServer(_serviceProvider);

            Console.WriteLine("server is runing, press a key stop .");

            Console.ReadKey();
        }

    }
}
