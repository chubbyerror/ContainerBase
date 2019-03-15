using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public System.Net.Http.HttpClient _client;
        public UnitTest1()
        {
            _client = new System.Net.Http.HttpClient();
        }
        [TestMethod]
        public void TestMethod1()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            int maxtime = 100;
            System.Threading.Tasks.Parallel.For(0, maxtime, task => {
                string name = $"tester{task}";
                stopwatch.Start();
                var returns = _client.GetAsync($"http://127.0.0.1:5000/api/echo/{name}").Result.Content.ReadAsStringAsync().Result;
                System.Diagnostics.Trace.WriteLine(returns);
                stopwatch.Stop();
            });
            System.Diagnostics.Trace.WriteLine($" {maxtime} retuns use {stopwatch.ElapsedMilliseconds}ms .");
        }
    }
}
