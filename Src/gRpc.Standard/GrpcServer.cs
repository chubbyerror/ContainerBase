using System;
using System.Collections.Generic;
using System.Text;
using DevelopBase.Services;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using static Grpcconnect.Standard.gConnect;


namespace Grpc.Standard
{
    public class GrpcServer : ServiceBase, IServer
    {
        public GrpcServer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
        Server server = null;
        public void StartRpc( string host, int port)
        {
            server = new Server()
            {
                Services = { BindService(new ServerImpl(ServiceProvider)) },
                Ports = { new ServerPort(host, port, Core.ServerCredentials.Insecure) }
            };
            server.Start();
        }

        public void StopRpc()
        {
            server.ShutdownAsync();
        }
    }
}
