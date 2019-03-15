using DevelopBase.DPBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LocalDiscovery
{
    public class Client :DevelopBase.Services.ServiceBase,  IClient
    {
        public HashSet<DPBase.RemoteAppInfo> srvInfos { get; set; }

        public Client(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public string GetServer(string ServiceName, bool needWatch = true)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(srvInfos.Where(c => $"{c.AppName}-{c.HostName}-{c.ProtocolType}" == ServiceName).FirstOrDefault());
        }

        public List<string> GetServerList()
        {
            return srvInfos?.Select(c => $"{c.AppName}-{c.HostName}-{c.ProtocolType}").ToList();
        }

        public void SetAction(Action<string> SrvDeleted, Action FindNewSrv, Action<string, string> SrvChanged)
        {

        }
        public void SetClient(string SrvCfgint, int SessionTimeout = -1)
        {
            srvInfos?.Clear();
            srvInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<HashSet<DPBase.RemoteAppInfo>>(SrvCfgint);
        }
    }
}
