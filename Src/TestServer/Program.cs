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
            ServiceCollection services = new ServiceCollection();

            //手写动态注册信息
            //add handle
            var handle = new RegisterInfo();
            handle.FromName = "SimpleBusiness.Message.Request.EchoRequest,SimpleBusiness";
            handle.ToName = "SimpleBusiness.Message.Handler.EchoHandler,SimpleBusiness";
            services.AddHandlers(new RegisterInfo[] { handle });

            //add server
            var server = new RegisterInfo();
            server.FromName = "SimpleBusiness.Service.IEcho,SimpleBusiness";
            server.ToName = "SimpleBusiness.Service.Echo,SimpleBusiness";
            services.AddServices(new RegisterInfo[] { server });

            //设置服务器配置信息
            var srvconfig = new DPBase.BaseServerInfo
            {
                //暂无作用的服务器名
                HostName = "testechoaaa",

                //本地映射的服务名对应服务Request配置
                Appinfos = new DPBase.Appinfo[]{
                    new DPBase.Appinfo()
                    {
                        AppName ="echo",
                        AppTypeName="SimpleBusiness.Message.Request.EchoRequest, SimpleBusiness",
                        SupportProtocolTypes=new DPBase.ProtocolType[]
                        {
                            DPBase.ProtocolType.Grpc,
                            DPBase.ProtocolType.Thrift,
                        }
                    }
                },

                //服务发现服务器配置
                DiscoveryInfo=new DPBase.DiscoveryInfo() {
                    Connstring= "192.168.100.159:2182,192.168.100.159:2183",
                    TimeOut = 5000,
                    ProtocolType=DPBase.DiscoveryType.Zookeeper
                },

                //支持的协议列表配置
                ProtocolInfos=new DPBase.ProtocolInfo[]
                {
                    new DPBase.ProtocolInfo(){ HostAddr="192.168.2.199", HostPort=5000, ProtocolType= DPBase.ProtocolType.Grpc },
                    new DPBase.ProtocolInfo(){ HostAddr="192.168.2.199", HostPort=5001, ProtocolType= DPBase.ProtocolType.Thrift }
                }
            };
            //创建通讯实例和服务注册实例
            BaseService sbase = new BaseService(services,srvconfig);
            
            //创建依赖注入容器
            _serviceProvider = services.BuildServiceProvider();

            sbase.StartService(_serviceProvider);

            sbase.RegisterServer(_serviceProvider);

            Console.WriteLine("server is runing, press a key stop .");

            Console.ReadKey();
        }

    }
}
