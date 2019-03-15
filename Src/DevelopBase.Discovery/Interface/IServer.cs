using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Discovery
{
    public interface IServer:Services.IService
    {
        void SetServer(string Connstring, int SessionTimeout = -1);
        string Register(string ServiceName, string SrvInfo);
        bool Update(string ServiceName, string SrvInfo);
        void Delete(string ServiceName);
    }
}
