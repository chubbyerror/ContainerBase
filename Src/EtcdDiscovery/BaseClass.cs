using System;
using System.Collections.Generic;
using System.Text;

namespace EtcdDiscovery
{
    class EtcdConfig
    {
        public string host { set; get; }
        public int port { set; get; }
        public string username { set; get; } = "";
        public string password { set; get; } = "";
        public string caCert { set; get; } = "";
        public string clientCert { set; get; } = "";
        public string clientKey { set; get; } = "";
        public bool publicRootCa { set; get; } = false;
    }
}
