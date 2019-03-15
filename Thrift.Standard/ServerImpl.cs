using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Thrift.Standard
{
    public class ServerImpl : tConnect.Iface
    {
        protected IServiceProvider DependencyContainer { get; } = null;
        public ServerImpl(IServiceProvider dependencyContainer)
        {
            DependencyContainer = dependencyContainer ?? throw new ArgumentNullException();
        }

        public tReponseMsg GetReponseAsync(tRequestMsg Msg)
        {
            var req = new DevelopBase.Protocol.BaseRequest { ActionKey = Msg.ActionKey, RequestObject = Msg.RequestObject };
            dynamic result = DependencyContainer.GetService<DevelopBase.Protocol.IBaseExtend>().Handler(req);

            return new tReponseMsg() { ErrorCode = result.ErrorCode, ErrorInfo = result.ErrorInfo, Result = Newtonsoft.Json.JsonConvert.SerializeObject($"{result.Result}") };
        }
    }
}
