using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;


namespace DevelopBase.Protocol
{
    public interface IClient : Services.IService
    {
        void UseClient(string addr,int port);
        string GetReponse(string actionkey, string requestobject);
        System.Threading.Tasks.Task<string> GetReponseAsync(string actionkey, string requestobject);
    }
}
