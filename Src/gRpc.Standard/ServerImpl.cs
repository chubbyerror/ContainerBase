using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Grpc.Standard
{
    public class ServerImpl: Grpcconnect.Standard.gConnect.gConnectBase
    {
        protected IServiceProvider DependencyContainer { get; } = null;
        public ServerImpl(IServiceProvider dependencyContainer)
        {
            DependencyContainer = dependencyContainer ?? throw new ArgumentNullException();
        }

        public override Task<Grpcconnect.Standard.ReponseMsg> GetReponse(Grpcconnect.Standard.RequestMsg request, ServerCallContext context)
        {
            return Task.Run(async () =>
            {
                var req = new DevelopBase.Protocol.BaseRequest { ActionKey = request.ActionKey, RequestObject = request.RequestObject };
                dynamic result = await DependencyContainer.GetService<DevelopBase.Protocol.IBaseExtend>().HandlerAsync(req);
                return new Grpcconnect.Standard.ReponseMsg() { ErrorCode = result.ErrorCode, ErrorInfo = result.ErrorInfo, Result = Newtonsoft.Json.JsonConvert.SerializeObject( $"{result.Result}" ) };
            });
        }

    }
}
