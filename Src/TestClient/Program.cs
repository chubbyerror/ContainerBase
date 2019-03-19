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

            //生成客户端配置信息1（基于本地配置字符串的服务发现，grpc服务器）
            
            DiscoveryInfo cfg = new DiscoveryInfo()
            {
                Connstring = "[{\"AppName\":\"echo\",\"HostAddr\":\"192.168.2.199\",\"HostPort\":5000,\"Addr\":\"192.168.2.199:5000\",\"ProtocolType\":0,\"HostName\":\"testechoaaa\",\"Infos\":null},{\"AppName\":\"echo\",\"HostAddr\":\"192.168.2.199\",\"HostPort\":5001,\"Addr\":\"192.168.2.199:5001\",\"ProtocolType\":1,\"HostName\":\"testechoaaa\",\"Infos\":null}]",
                ProtocolType = DiscoveryType.Local,
                 TimeOut = 5000
            };

            //生成客户端配置信息2（基于zookeeper的服务发现，thrift服务器）
            DiscoveryInfo cfgz = new DiscoveryInfo()
            {
                //Connstring = "192.168.100.159:2181",
                Connstring = "{\"host\":\"192.168.100.162\",\"port\":23791}",
                ProtocolType = DiscoveryType.Etcd,
                TimeOut = 5000
            };

            //生成容器
            var provider = services.BuildServiceProvider();

            baseClient.GetRemoteApps(provider, cfg);
            baseClientz.GetRemoteApps(provider, cfgz);
            //通过客户端实例调用方法

            for (int i = 0; i < 10; i++)
            {
                var a1 = baseClient.RunApp(provider, "echo", "{\"name\":\"jone\"}");
                Console.WriteLine($"{a1}");
            }

            var a = baseClient.RunApp(provider, "echo", "{\"name\":\"jone\"}");
            var b = baseClientz.RunApp(provider, "echo", "{\"name\":\"simis\"}");

            Console.WriteLine($"{a} {b}");
            Console.ReadKey();
        }
    }
}
