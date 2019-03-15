using System;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
            string name = "tester";
            var returns= client.GetAsync($"http://127.0.0.1:5000/api/echo/{name}").Result.Content.ReadAsStringAsync().Result;
            Console.WriteLine($"{returns}");
            Console.ReadKey();
        }
    }
}
