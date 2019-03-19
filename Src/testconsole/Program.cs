using System;
using Microsoft.Extensions.DependencyInjection;

namespace testconsole
{
    class Program
    {
        static void Main(string[] args)
        {

            var services = new ServiceCollection();
            var  provider = services.BuildServiceProvider();

            EtcdDiscovery.Server server = new EtcdDiscovery.Server(provider);

            server.SetServer("{\"host\":\"192.168.100.162\",\"port\":23791}", 10000);

            server.Register("test", "testreg");

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
