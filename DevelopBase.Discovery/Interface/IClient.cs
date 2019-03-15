using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Discovery
{
    public interface IClient:Services.IService
    {
        void SetClient(string SrvCfgint,int SessionTimeout = -1);
        void SetAction(Action<string> SrvDeleted, Action FindNewSrv, Action<string,string> SrvChanged);
        string GetServer(string ServiceName, bool needWatch = true);
        List<string> GetServerList();
    }
}
