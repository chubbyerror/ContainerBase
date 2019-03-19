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

            //ʵ�����ͻ���
            baseClient = new BaseClient(services);
            //����������
            DiscoveryInfo cfg = new DiscoveryInfo()
            {
                Connstring = "192.168.100.159:2181",
                ProtocolType = DiscoveryType.Zookeeper,
                TimeOut = 5000
            };
            //��������
            provider = services.BuildServiceProvider();
            //��ʼ���ͻ���
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
