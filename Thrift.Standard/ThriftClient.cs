using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace Thrift.Standard
{
    public class ThriftClient : DevelopBase.Services.ServiceBase, IThriftClient
    {
        public ThriftClient(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public string GetReponse(string actionkey, string requestobject)
        {
            TTransport transport = new TSocket($"{Addr}", Port); ;
            TProtocol protocol = new TBinaryProtocol(transport);
            tConnect.Client Client = new tConnect.Client(protocol);
            transport.Open();
            try
            {
                tReponseMsg result = Client.GetReponseAsync(new tRequestMsg() { ActionKey = actionkey, RequestObject = requestobject });
                transport.Close();
                protocol.Dispose();
                Client.Dispose();
                return Newtonsoft.Json.JsonConvert.SerializeObject(new { result.ErrorCode, result.ErrorInfo, result.Result });
                //thrift hack 否则这货会自动增加一个属性返回值很难看
            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(new tReponseMsg() { ErrorCode = ex.HResult, ErrorInfo = ex.Message });
            }
        }
        private int Port { get; set; }
        private string Addr { get; set; }
        public void UseClient(string addr, int port)
        {
            Addr = addr;
            Port = port;
        }
        public Task<string> GetReponseAsync(string actionkey, string requestobject)
        {
            TTransport transport = new TSocket($"{Addr}", Port); ;
            TProtocol protocol = new TBinaryProtocol(transport); 
            tConnect.Client Client = new tConnect.Client(protocol);
            transport.Open();

            return Task.Run(() =>
            {
                try
                {
                    tReponseMsg result = Client.GetReponseAsync(new tRequestMsg() { ActionKey = actionkey, RequestObject = requestobject });
                    transport.Close();
                    protocol.Dispose();
                    Client.Dispose();
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new { result.ErrorCode, result.ErrorInfo, result.Result });
                    //thrift hack 否则这货会自动增加一个属性返回值很难看
                }
                catch (Exception ex)
                {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new tReponseMsg() { ErrorCode = ex.HResult, ErrorInfo = ex.Message });
                }
            });
        }
    }
}
