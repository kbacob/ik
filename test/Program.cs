// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ConsoleApp1
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using ik.Net;
    using ik.Utils;

    /// <summary>
    ///  Teas app
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ik.Main.Init(args);
           
            HTTPServer http = new HTTPServer("any", 80); // если на Linux то либо порт > 1024 либо от суперюзера гонять, иначе будет исключение по AccessDenied
            http.CSocketWorker.Add("Test Worker 1", "hello", Test1, SocketWorker.SocketWorkerEntryType.Url, Strings.EqualAnalysisType.Strong, SocketWorker.SocketWorkerJobType.SendText); // https://127.0.0.1/HEYhello[/пофиг-чё?и&тут&тоже&плевать 
            
            http.Start();
            Console.WriteLine("Press Enter to exit");
            Console.Read();
            http.Stop();

            ik.Main.Exit();
        }

        static Response Test1(ClientHeader clientHeader, NetworkStream stream)
        {
            Console.WriteLine(clientHeader["ClientIP"]);
            return new Response("Hello", HttpStatusCode.OK);
        }

    }
}
