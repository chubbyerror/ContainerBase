using System;
using System.Collections.Generic;
using System.Text;

namespace EtcdDiscovery
{
    public class Client :DevelopBase.Services.ServiceBase,  IClient
    {
        public Client(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        Kdetcd.Standard.EtcdDiscovery.Client client = null;
        public string GetServer(string ServiceName, bool needWatch = true)
        {
            return client.GetServer(ServiceName, needWatch);
        }

        public List<string> GetServerList()
        {
            return client.GetServerList();
        }

        public void SetAction(Action<string> SrvDeleted, Action FindNewSrv, Action<string, string> SrvChanged)
        {
            client.FindNewSrv = FindNewSrv;
            client.SrvChanged = SrvChanged;
            client.SrvDeleted = SrvDeleted;
        }

        public void SetClient(string SrvCfg, int SessionTimeout = -1)
        {
            if (client!=null)
            {
                client.Dispose();
            }
            EtcdConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<EtcdConfig>(SrvCfg);
            client = new Kdetcd.Standard.EtcdDiscovery.Client(config.host, config.port,
                config.username, config.password,
                config.caCert, config.clientCert, config.clientKey,
                config.publicRootCa);
        }
    }
}
