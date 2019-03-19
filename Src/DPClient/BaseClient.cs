using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DevelopBase.Discovery;
using DevelopBase.Protocol;
using DevelopBase.Common;
using System.Threading.Tasks;
using DevelopBase.DPBase;

namespace DPClient
{
    public class BaseClient
    {
        private Dictionary<string, DPBase.RemoteAppInfo> Appinfos { get; set; }

        private Dictionary<DPBase.ProtocolType, DPBase.Mapinfo> ProtocolMap { set; get; } = new Dictionary<DPBase.ProtocolType, DPBase.Mapinfo>();
        private Dictionary<DPBase.DiscoveryType, DPBase.Mapinfo> DiscoveryMap { set; get; } = new Dictionary<DPBase.DiscoveryType, DPBase.Mapinfo>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseClient(IServiceCollection serviceCollection)
        {
            //注入映射信息
            serviceCollection.AddServices(MapBaseSettings());
        }

        private RegisterInfo[] MapBaseSettings()
        {
            ProtocolMap.Add(DPBase.ProtocolType.Grpc
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(Grpc.Standard.GrpcClient),
                    BaseInterface = typeof(Grpc.Standard.IGrpcClient),
                    RootInterface = typeof(DevelopBase.Protocol.IClient)
                });
            ProtocolMap.Add(DPBase.ProtocolType.Thrift
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(Thrift.Standard.ThriftClient),
                    BaseInterface = typeof(Thrift.Standard.IThriftClient),
                    RootInterface = typeof(DevelopBase.Protocol.IClient)
                });
            DiscoveryMap.Add(DPBase.DiscoveryType.Zookeeper
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(ZooDiscovery.ZooClient),
                    BaseInterface = typeof(ZooDiscovery.IClient),
                    RootInterface = typeof(DevelopBase.Discovery.IClient)
                });
            DiscoveryMap.Add(DPBase.DiscoveryType.Local
                , new DPBase.Mapinfo()
                {
                    BaseType = typeof(LocalDiscovery.Client),
                    BaseInterface = typeof(LocalDiscovery.IClient),
                    RootInterface = typeof(DevelopBase.Discovery.IClient)
                });
            DiscoveryMap.Add(DPBase.DiscoveryType.Etcd,
                new DPBase.Mapinfo()
                {
                    BaseType = typeof(EtcdDiscovery.Client),
                    BaseInterface = typeof(EtcdDiscovery.IClient),
                    RootInterface = typeof(DevelopBase.Discovery.IClient)
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


        /// <summary>
        /// 获取远程应用信息
        /// 
        /// 获取后序列化，保存本地缓存Appinfo
        /// 获取后注册被动更新方法到相关调用位置
        /// </summary>
        /// <param name="RemoteAddr">远程应用保存服务信息</param>
        public void GetRemoteApps(IServiceProvider provider,DPBase.DiscoveryInfo discoveryInfo)
        {
            var services = provider.GetServices<DevelopBase.Discovery.IClient>();
            var client = services.First(c => c.GetType().GetInterfaces().Contains(DiscoveryMap[discoveryInfo.ProtocolType].BaseInterface));
            client.SetClient(discoveryInfo.Connstring, discoveryInfo.TimeOut);
            //注册被动更新
            client.SetAction(
                new Action<string>((k) =>
                {
                    lock (Appinfos)
                    {
                        Appinfos.Remove(k);
                    }
                }),
                new Action(() =>
                {
                    GetNewAppInfos(client);
                }),
                new Action<string, string>((k, v) =>
                {
                    lock (Appinfos)
                    {
                        Appinfos[k]= Newtonsoft.Json.JsonConvert.DeserializeObject<DPBase.RemoteAppInfo>(v);
                    }
                })
                );
            GetNewAppInfos(client);
        }

        private void GetNewAppInfos(DevelopBase.Discovery.IClient client,bool isWatch=true)
        {
            //获取远程列表
            List<string> srvs = client.GetServerList();
            if (Appinfos==null)
            {
                Appinfos = new Dictionary<string, DPBase.RemoteAppInfo>();
            }
            lock (Appinfos)
            {
                //剔除已有项目
                srvs.RemoveAll(c => Appinfos.Any(k => k.Key == c));
                //增加新增项目
                foreach (var item in srvs)
                {
                    lock (Appinfos)
                    {
                        if (!Appinfos.Any(c => c.Key == item))
                        {
                            var val = Newtonsoft.Json.JsonConvert.DeserializeObject<DPBase.RemoteAppInfo>(client.GetServer(item, isWatch));
                            Appinfos.Add(item, val);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据应用名获取应用信息
        /// 
        /// 加入轮询机制
        /// 返回由GetApps序列化好的本地缓存中的应用信息
        /// </summary>
        /// <param name="AppName">应用名</param>
        /// <returns>单一应用信息</returns>
        private DPBase.RemoteAppInfo GetAppInfo(string AppName)
        {
            var apps = Appinfos?.Where(c => c.Value.AppName == AppName).ToList();
            //未找到运行的服务
            if (apps.Count<1)
            {
                return null;
            }
            //只有一个直接返回
            if (apps.Count==1)
            {
                return apps[0].Value;
            }

            //随机轮询
            int use = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            if (apps.Count>1)
            {
                use = r.Next((apps.Count) * 10000) / 10000;
            }
            if (use==apps.Count)
            {
                use = use - 1;
            }
            return apps[use].Value;
        }

        /// <summary>
        /// 调用app
        /// </summary>
        /// <param name="appname">app名</param>
        /// <param name="appmsg">app传入参数</param>
        /// <returns>应用调用结果</returns>
        public string RunApp(IServiceProvider provider, string appname, string appmsg)
        {
            DPBase.RemoteAppInfo app = GetAppInfo(appname);//获取app信息
            if (app==null)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new Grpcconnect.Standard.ReponseMsg() { ErrorCode = -1, ErrorInfo = "no action", Result = null });
            }
            //获取client
            var clients = provider.GetServices<DevelopBase.Protocol.IClient>();
            var client = clients.First(c => c.GetType().GetInterfaces().Contains(ProtocolMap[app.ProtocolType].BaseInterface));
            //设置client参数
            client.UseClient(app.HostAddr, app.HostPort);
            //调用数据
            return client.GetReponse(app.AppName, appmsg);
        }

        /// <summary>
        /// 调用app
        /// </summary>
        /// <param name="appname">app名</param>
        /// <param name="appmsg">app传入参数</param>
        /// <returns>异步应用调用结果</returns>
        public async Task<string> RunAppAsync(IServiceProvider provider, string appname, string appmsg)
        {
            DPBase.RemoteAppInfo app = GetAppInfo(appname);//获取app信息
            //获取client
            var clients = provider.GetServices<DevelopBase.Protocol.IClient>();
            var client = clients.First(c => c.GetType().GetInterfaces().Contains(ProtocolMap[app.ProtocolType].BaseInterface));
            //设置client参数
            client.UseClient(app.HostAddr, app.HostPort);
            //调用数据
            return await client.GetReponseAsync(app.AppName, appmsg);
        }
    }
}
