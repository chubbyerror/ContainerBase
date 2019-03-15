using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.DPBase
{
    public class DPBase
    {
        //本地缓存的应用信息
        public class Appinfo
        {
            public string AppName { get; set; }
            public string AppTypeName { get; set; }
            public ProtocolType[] SupportProtocolTypes { get; set; }
            private Type _AppType;
            public Type AppType
            {
                get
                {
                    if (_AppType == null)
                    {
                        _AppType = Type.GetType(AppTypeName, true, true); ;
                    }
                    return _AppType;
                }
            }
        }

        //本地缓存的协议信息
        public class ProtocolInfo
        {
            public ProtocolType ProtocolType { get; set; }
            public string HostAddr { get; set; }
            public int HostPort { get; set; }
            public string Addr
            {
                get
                {
                    return $"{HostAddr}:{HostPort}";
                }
            }
            public string Connstring { get; set; }
        }
        //协议类型
        public enum ProtocolType
        {
            Grpc,
            Thrift
        }

        public class DiscoveryInfo
        {
            public DiscoveryType ProtocolType { get; set; }
            public string HostAddr { get; set; }
            public int HostPort { get; set; }
            public string Addr
            {
                get
                {
                    return $"{HostAddr}:{HostPort}";
                }
            }
            public string Connstring { get; set; }
            public int TimeOut { get; set; }
        }
        public enum DiscoveryType
        {
            Zookeeper, Etcd, Local
        }

        //本地缓存的基础信息
        public class BaseServerInfo
        {
            public string HostName { get; set; }
            public string Infos { get; set; }
            public Appinfo[] Appinfos { get; set; }
            public ProtocolInfo[] ProtocolInfos { get; set; }
            public DiscoveryInfo DiscoveryInfo { get; set; }
        }

        //本地缓存的注册后信息，用于更新服务信息
        public class RemoteAppInfo
        {
            public string AppName { get; set; }
            public string HostAddr { get; set; }
            public int HostPort { get; set; }
            public string Addr
            {
                get
                {
                    return $"{HostAddr}:{HostPort}";
                }
            }
            public ProtocolType ProtocolType { get; set; }
            public string HostName { get; set; }
            public string Infos { get; set; }
        }

        //映射类
        public class Mapinfo
        {
            public Type BaseType { get; set; }
            public Type BaseInterface { get; set; }
            public Type RootInterface { get; set; }
        }
    }
}
