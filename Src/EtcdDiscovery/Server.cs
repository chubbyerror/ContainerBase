using System;
using System.Collections.Generic;
using System.Text;

namespace EtcdDiscovery
{
    public class Server : DevelopBase.Services.ServiceBase, IServer
    {
        public Server(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        Kdetcd.Standard.EtcdDiscovery.Server server = null;

        public void Delete(string ServiceName)
        {
            server.Delete(ServiceName);
        }

        public string Register(string ServiceName, string SrvInfo)
        {
            return server.Register(ServiceName, SrvInfo);
        }

        public void SetServer(string Connstring, int SessionTimeout = -1)
        {
            EtcdConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<EtcdConfig>(Connstring);
            if (server!=null)
            {
                server.Dispose();
            }
            server = new Kdetcd.Standard.EtcdDiscovery.Server(config.host, config.port, 
                SessionTimeout, 
                config.username, config.password,
                config.caCert, config.clientCert, config.clientKey,
                config.publicRootCa);
        }

        public bool Update(string ServiceName, string SrvInfo)
        {
            return server.Update(ServiceName, SrvInfo);
        }
    }
}
