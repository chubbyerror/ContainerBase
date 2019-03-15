using System;
using System.Collections.Generic;
using System.Text;

namespace LocalDiscovery
{
    public class Server :DevelopBase.Services.ServiceBase, IServer
    {
        public Server(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Delete(string ServiceName)
        {
            return;
        }

        public string Register(string ServiceName, string SrvInfo)
        {
            return "";
        }

        public void SetServer(string Connstring, int SessionTimeout = -1)
        {
            return;
        }

        public bool Update(string ServiceName, string SrvInfo)
        {
            return false;
        }
    }
}
