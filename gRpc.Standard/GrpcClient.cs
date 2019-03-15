using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Grpc.Standard
{
    public class GrpcClient : DevelopBase.Services.ServiceBase, IGrpcClient
    {
        public GrpcClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        Grpcconnect.Standard.gConnect.gConnectClient Client = null;
        public string GetReponse(string actionkey, string requestobject)
        {
            try
            {
                var result = Client.GetReponse(new Grpcconnect.Standard.RequestMsg() { ActionKey = actionkey, RequestObject = requestobject });
                return Newtonsoft.Json.JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new Grpcconnect.Standard.ReponseMsg() { ErrorCode = ex.HResult, ErrorInfo = ex.Message, Result = null });
                throw;
            }
        }

        public void UseClient(string addr, int port)
        {
            if (Client != null)
            {
                Client.WithHost($"{addr}:{port}");
            }
            else
            {
                Core.Channel channel = new Core.Channel($"{addr}:{port}", Core.ChannelCredentials.Insecure);
                Client = new Grpcconnect.Standard.gConnect.gConnectClient(channel);
            }
        }

        public Task<string> GetReponseAsync(string actionkey, string requestobject)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var result = await Client.GetReponseAsync(new Grpcconnect.Standard.RequestMsg() { ActionKey = actionkey, RequestObject = requestobject });
                    return Newtonsoft.Json.JsonConvert.SerializeObject(result);
                }
                catch (Exception ex)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new Grpcconnect.Standard.ReponseMsg() { ErrorCode = ex.HResult, ErrorInfo = ex.Message, Result = null });
                    throw;
                }
            });
        }
    }
}
