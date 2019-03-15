using System;
using System.Collections.Generic;

namespace ZooDiscovery
{
    public class ZooClient : DevelopBase.Services.ServiceBase, IClient
    {
        public ZooClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        KdZookeeper.Standard.ZooDiscovery.Client Client;
        public string GetServer(string ServiceName, bool needWatch = true)
        {
            return Client.GetServer(ServiceName, needWatch);
        }

        public List<string> GetServerList()
        {
            return Client.GetServerList();
        }

        public void SetAction(Action<string> SrvDeleted, Action FindNewSrv, Action<string,string> SrvChanged)
        {
            Client.FindNewSrv = FindNewSrv;
            Client.SrvChanged = SrvChanged;
            Client.SrvDeleted = SrvDeleted;
        }

        public void SetClient(string SrvCfg,int SessionTimeout = -1)
        {
            if (Client==null)
            {
                Client = new KdZookeeper.Standard.ZooDiscovery.Client(SrvCfg, SessionTimeout);
            }
        }
    }
}
