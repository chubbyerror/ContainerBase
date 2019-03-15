using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevelopBase.Protocol
{
    public interface IServer: Services.IService
    {
        void StartRpc(string host,int port);
        void StopRpc();
    }
}
