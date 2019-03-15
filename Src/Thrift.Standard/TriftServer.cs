using System;
using System.Collections.Generic;
using System.Text;
using Thrift.Server;
using Thrift.Transport;

namespace Thrift.Standard
{
    public class TriftServer : DevelopBase.Services.ServiceBase, IServer
    {
        public TriftServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        TServer server = null;

        public void StartRpc(string host, int port)
        {
            var Service = new ServerImpl(ServiceProvider);
            tConnect.Processor processor = new tConnect.Processor(Service);
            TServerTransport transport = new TServerSocket(port);
            server = new TThreadPoolServer(processor, transport);

            System.Threading.Tasks.Task.Run(()=>{
                server.Serve();
            });
        }

        public void StopRpc()
        {
            server.Stop();
        }
    }
}
