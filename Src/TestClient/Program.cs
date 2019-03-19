using DevelopBase.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using DPClient;
using DevelopBase.DPBase;
using static DevelopBase.DPBase.DPBase;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {


            ServiceCollection services = new ServiceCollection();

            //实例化两个客户端
            BaseClient baseClient = new BaseClient(services);
            BaseClient baseClientz = new BaseClient(services);

            //使用JObject读取配置文件
            Newtonsoft.Json.Linq.JObject cfgobj = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(System.IO.File.ReadAllTextAsync("config.json").Result);
            //反序列化配置生成客户端配置信息（local
            DiscoveryInfo cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscoveryInfo>(cfgobj["cfglocal"].ToString());
            //生成客户端配置信息2（基于zookeeper的服务发现，thrift服务器）
            DiscoveryInfo cfgz = Newtonsoft.Json.JsonConvert.DeserializeObject<DiscoveryInfo>(cfgobj["cfgzookeeper"].ToString());

            //生成容器
            var provider = services.BuildServiceProvider();

            baseClient.GetRemoteApps(provider, cfg);
            baseClientz.GetRemoteApps(provider, cfgz);
            //通过客户端实例调用方法

            var a = baseClient.RunApp(provider, "echo", "{\"name\":\"jone\"}");
            var b = baseClientz.RunApp(provider, "echo", "{\"name\":\"simis\"}");

            Console.WriteLine($"{a} {b}");
            Console.ReadKey();
        }
    }
}
