using DevelopBase.Common;
using DevelopBase.DPBase;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DPServer
{
    public class BaseService
    {
        //本地缓存的应用信息
        public HashSet<DPBase.Appinfo> Appinfos { get; set; }
        //本地缓存的协议信息
        public DPBase.ProtocolInfo[] ProtocolInfos { get; set; }
        //本地缓存的协议信息
        public DPBase.DiscoveryInfo DiscoveryInfo { get; set; }
        //本地缓存的基础信息
        public DPBase.BaseServerInfo BaseServerInfo { get; set; }
        //包装类内的映射，用于注册映射信息，修改包装类修改位置
        private Dictionary<DPBase.ProtocolType, DPBase.Mapinfo> ProtocolMap { set; get; } = new Dictionary<DPBase.ProtocolType, DPBase.Mapinfo>();
        private Dictionary<DPBase.DiscoveryType, DPBase.Mapinfo> DiscoveryMap { set; get; } = new Dictionary<DPBase.DiscoveryType, DPBase.Mapinfo>();

        /// <summary>
        /// 构造函数
        /// 设置引用服务信息
        /// 
        /// 将用户设置信息序列化为对应的应用、协议和基础信息
        /// </summary>
        /// <param name="BaseInfo">具体服务信息</param>
        public BaseService(IServiceCollection serviceCollection,DPBase.BaseServerInfo BaseInfo)
        {
            if (BaseInfo==null)
            {
                throw new Exception($" {nameof(BaseInfo)} can't be null .");
            }
            //保存信息
            ProtocolInfos = BaseInfo.ProtocolInfos;
            BaseServerInfo = BaseInfo;

            //本地映射信息因为是单例，所以要更新方式添加不能直接赋值
            Appinfos = serviceCollection.BuildServiceProvider().GetService<HashSet<DPBase.Appinfo>>();
            if (Appinfos==null)
            {
                Appinfos = new HashSet<DPBase.Appinfo>();
            }
            foreach (var item in BaseInfo.Appinfos)
            {
                lock (Appinfos)
                {
                    if (!Appinfos.Any(c => c.AppName == item.AppName))
                    {
                        Appinfos.Add(item);
                    }
                }
            }

            //依赖注入
            serviceCollection.AddServices(MapBaseSettings());
            var BaseProcess = new RegisterInfo()
            {
                FromName = "DevelopBase.Protocol.IBaseExtend, DevelopBase.Protocol",
                ToName = "DevelopBase.Protocol.BaseExtend, DevelopBase.Protocol"
            };
            serviceCollection.AddServices(new RegisterInfo[] { BaseProcess });
            //单例注入
            serviceCollection.AddSingleton(Appinfos);
        }

        //自动映射信息，调整这里就能实现增删协议或服务发现端
        private RegisterInfo[] MapBaseSettings()
        {
            ProtocolMap.Add(DPBase.ProtocolType.Grpc
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(Grpc.Standard.GrpcServer),
                    BaseInterface = typeof(Grpc.Standard.IServer),
                    RootInterface = typeof(DevelopBase.Protocol.IServer)
                });
            ProtocolMap.Add(DPBase.ProtocolType.Thrift
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(Thrift.Standard.TriftServer),
                    BaseInterface = typeof(Thrift.Standard.IServer),
                    RootInterface = typeof(DevelopBase.Protocol.IServer)
                });
            DiscoveryMap.Add(DPBase.DiscoveryType.Zookeeper
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(ZooDiscovery.ZooServer),
                    BaseInterface = typeof(ZooDiscovery.IServer),
                    RootInterface = typeof(DevelopBase.Discovery.IServer)
                });
            DiscoveryMap.Add(DPBase.DiscoveryType.Local
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(LocalDiscovery.Server),
                    BaseInterface = typeof(LocalDiscovery.IServer),
                    RootInterface = typeof(DevelopBase.Discovery.IServer)
                });
            DiscoveryMap.Add(DPBase.DiscoveryType.Etcd, new DPBase.Mapinfo()
            {
                BaseType = typeof(EtcdDiscovery.Server),
                BaseInterface = typeof(EtcdDiscovery.IServer),
                RootInterface = typeof(DevelopBase.Discovery.IServer)
            });
            List<RegisterInfo> result = new List<RegisterInfo>();
            //转换注册类
            foreach (var item in ProtocolMap)
            {
                var info = new RegisterInfo()
                {
                    FromName = $"{item.Value.RootInterface.FullName}, {item.Value.RootInterface.Namespace}",
                    ToName = $"{item.Value.BaseType.FullName}, {item.Value.BaseType.Namespace}",
                    //单例
                    LifeScope = LifeScope.Singleton
                };
                result.Add(info);
            }
            foreach (var item in DiscoveryMap)
            {
                var info = new RegisterInfo()
                {
                    FromName = $"{item.Value.RootInterface.FullName}, {item.Value.RootInterface.Namespace}",
                    ToName = $"{item.Value.BaseType.FullName}, {item.Value.BaseType.Namespace}",
                    //单例
                    LifeScope = LifeScope.Singleton
                };
                result.Add(info);
            }
            return result.ToArray();
        }

        Dictionary<string, string> Registered = new Dictionary<string, string>();
        /// <summary>
        /// 向远端注册服务信息
        /// </summary>
        public void RegisterServer(IServiceProvider provider)
        {
            //注册服务
            var servers = provider.GetServices<DevelopBase.Discovery.IServer>();
            var Register = servers.First(c => c.GetType().GetInterfaces().Contains(DiscoveryMap[BaseServerInfo.DiscoveryInfo.ProtocolType].BaseInterface));
            Register.SetServer(BaseServerInfo.DiscoveryInfo.Connstring,BaseServerInfo.DiscoveryInfo.TimeOut);
            //转换本地配置为远端配置
            foreach (var localInfo in Appinfos)
            {
                foreach (var SupportProtocolType in localInfo.SupportProtocolTypes)
                {
                    var remoteInfo = new DPBase.RemoteAppInfo()
                    {
                        AppName = localInfo.AppName,
                        ProtocolType = SupportProtocolType,
                        HostName = BaseServerInfo.HostName,
                        Infos = BaseServerInfo.Infos,
                        HostAddr = ProtocolInfos.First(c => c.ProtocolType == SupportProtocolType).HostAddr,
                        HostPort = ProtocolInfos.First(c => c.ProtocolType == SupportProtocolType).HostPort,
                    };
                    lock (Registered)
                    {
                        var key = $"{remoteInfo.AppName}-{remoteInfo.HostName}-{remoteInfo.ProtocolType}";
                        Registered.Add(key, Newtonsoft.Json.JsonConvert.SerializeObject(remoteInfo));
                        Register.Register(key, Registered[key]);
                    }
                }
            }
        }

        /// <summary>
        /// 向远端注册服务信息
        /// </summary>
        public void UpdateServer(IServiceProvider provider, DPBase.BaseServerInfo NewInfos)
        {
            //移除旧注册信息
            var servers = provider.GetServices<DevelopBase.Discovery.IServer>();
            var Register = servers.First(c => c.GetType().GetInterfaces().Contains(DiscoveryMap[BaseServerInfo.DiscoveryInfo.ProtocolType].BaseInterface));
            //重置连接将删除以前全部数据
            Register.SetServer(BaseServerInfo.DiscoveryInfo.Connstring);
            lock (Registered)
            {
                Registered.Clear();
            }
            //更新本地缓存信息
            Appinfos = provider.GetService<HashSet<DPBase.Appinfo>>();
            if (Appinfos == null)
            {
                Appinfos = new HashSet<DPBase.Appinfo>();
            }
            foreach (var item in NewInfos.Appinfos)
            {
                lock (Appinfos)
                {
                    if (!Appinfos.Any(c => c.AppName == item.AppName))
                    {
                        Appinfos.Add(item);
                    }
                }
            }
            ProtocolInfos = NewInfos.ProtocolInfos;
            BaseServerInfo = NewInfos;
            //重新注册信息
            foreach (var localInfo in Appinfos)
            {
                foreach (var SupportProtocolType in localInfo.SupportProtocolTypes)
                {
                    var remoteInfo = new DPBase.RemoteAppInfo()
                    {
                        AppName = localInfo.AppName,
                        ProtocolType = SupportProtocolType,
                        HostName = BaseServerInfo.HostName,
                        Infos = BaseServerInfo.Infos,
                        HostAddr = ProtocolInfos.First(c => c.ProtocolType == SupportProtocolType).HostAddr,
                        HostPort = ProtocolInfos.First(c => c.ProtocolType == SupportProtocolType).HostPort,
                    };
                    lock (Registered)
                    {
                        var key = $"{remoteInfo.AppName}-{remoteInfo.HostName}-{remoteInfo.ProtocolType}";
                        Registered.Add(key, Newtonsoft.Json.JsonConvert.SerializeObject(remoteInfo));
                        Register.Register(key, Registered[key]);
                    }
                }
            }
        }

        /// <summary>
        /// 开启应用服务
        /// </summary>
        public void StartService(IServiceProvider provider)
        {
            //将支持的协议全部开启服务
            foreach (var item in BaseServerInfo.ProtocolInfos)
            {
                var services = provider.GetServices<DevelopBase.Protocol.IServer>();
                var server = services.First(c => c.GetType().GetInterfaces().Contains(ProtocolMap[item.ProtocolType].BaseInterface));
                if (server == null)
                {
                    throw new Exception(" no server config or server inject error . ");//这里需要输出错误还是输出到log？
                }
                server.StartRpc(item.HostAddr, item.HostPort);
            }
        }
    }
}