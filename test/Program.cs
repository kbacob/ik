namespace ConsoleApp1
{
    using System;
    using System.Threading;
    using System.Net.Sockets;
    using ik.Net;
    using ik.Utils;


    class Program
    {
        static int intMaxThreads = 32;

        static void Main(string[] args)
        {
            ThreadPool.SetMaxThreads(intMaxThreads, intMaxThreads);
            ThreadPool.SetMinThreads(2, 2);

            ContentTypes.Init();
            HTTPServer http = new HTTPServer("any", 80);

            http.AddHttpWorker(@"^hell\S$", Test1);
            http.AddHttpWorker(@"/", Test2);

            Console.WriteLine(ik.Utils.Environment.GetVariable("TMP"));
            http.Start();
            Console.Read();
            http.Stop();

            ContentTypes.Exit();
        }

        static string Test1(ClientHeader clientHeader, NetworkStream stream)
        {
            Console.WriteLine(clientHeader["ClientIp"]);
            return "Hello";
        }

        static string Test2(ClientHeader clientHeader, NetworkStream stream)
        {
            Console.WriteLine(clientHeader["ClientIp"]);
            return ContentTypes.GetContentType("mks");
        }

    }
}
