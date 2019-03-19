using DPClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static DevelopBase.DPBase.DPBase;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        BaseClient baseClient;
        ServiceProvider provider;
        public UnitTest1()
        {
            ServiceCollection services = new ServiceCollection();

            //实例化客户端
            baseClient = new BaseClient(services);
            //服务发现配置
            DiscoveryInfo cfg = new DiscoveryInfo()
            {
                Connstring = "192.168.100.159:2181",
                ProtocolType = DiscoveryType.Zookeeper,
                TimeOut = 5000
            };
            //生成容器
            provider = services.BuildServiceProvider();
            //初始化客户端
            baseClient.GetRemoteApps(provider, cfg);
        }
        [TestMethod]
        public void TestMethod1()
        {
            int maxtime = 100;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            System.Threading.Tasks.Parallel.For(0, maxtime, task =>
            {
                string name = $"tester{task}";
                stopwatch.Start();
                var returns = baseClient.RunApp(provider, "echo", $"{{\"name\":\"{name}\"}}");
                System.Diagnostics.Trace.WriteLine(returns);
                stopwatch.Stop();
            });
            System.Diagnostics.Trace.WriteLine($" {maxtime} retuns use {stopwatch.ElapsedMilliseconds}ms .");
        }

    }
}
