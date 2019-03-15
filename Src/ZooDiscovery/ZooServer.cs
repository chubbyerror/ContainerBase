using System;
using System.Collections.Generic;
using System.Text;

namespace ZooDiscovery
{
    public class ZooServer : DevelopBase.Services.ServiceBase, IServer
    {
        public ZooServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        KdZookeeper.Standard.ZooDiscovery.Server Server = null;
        public void Delete(string ServiceName)
        {
            Server.Delete(ServiceName);
        }

        public string Register(string ServiceName, string SrvInfo)
        {
            return Server.Register(ServiceName,SrvInfo);
        }

        public void SetServer(string Connstring, int SessionTimeout = -1)
        {
            if (Server == null)
            {
                Server = new KdZookeeper.Standard.ZooDiscovery.Server(Connstring, SessionTimeout);
            }
            else
            {
                Server.Dispose();
                Server = new KdZookeeper.Standard.ZooDiscovery.Server(Connstring, SessionTimeout);
            }
        }

        public bool Update(string ServiceName, string SrvInfo)
        {
            return Server.Update(ServiceName, SrvInfo);
        }
    }
}
