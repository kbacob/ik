    // Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ConsoleApp1
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using ik.Net;
    using ik.Utils;

    /// <summary>
    ///  Test app
    /// </summary>
    class Program
    {
        static HTTPServer http;

        static void Main(string[] args)
        {
            ik.Main.Init(args);

            InitHTTP();
           
            http.Start();
            Console.WriteLine("Press Enter to exit");
            Console.Read();
            http.Stop();

            ik.Main.Exit();
        }


        static void InitHTTP()
        {
            http = new HTTPServer("any", 80); // если на Linux то либо порт > 1024 либо от суперюзера гонять, иначе будет исключение по AccessDenied
            http.CSocketWorker.strRootDirectory = @"C:\Temp\www";
            http.CSocketWorker.Add("Test Worker 1", "hello", Test1, SocketWorker.SocketWorkerEntryType.Url, Strings.EqualAnalysisType.Strong, SocketWorker.SocketWorkerJobType.SendText); // https://127.0.0.1/HEYhello[/пофиг-чё?и&тут&тоже&плевать 
            http.CSocketWorker.Add("Test Worker 2", "get_image", Test2, SocketWorker.SocketWorkerEntryType.Command, Strings.EqualAnalysisType.Strong, SocketWorker.SocketWorkerJobType.SendFile); // https://127.0.0.1/[/пофиг-чё]?command=get_image&id=123
        }


        static Response Test1(ClientHeader clientHeader, NetworkStream stream)
        {
            Console.WriteLine(clientHeader["ClientIP"]);
            return new Response("Hello", HttpStatusCode.OK);
        }


        static Response Test2(ClientHeader clientHeader, NetworkStream objStream)
        {
            if (clientHeader.query["id"] != null)
            {
                string strFileName = null;
                var id =  Convert.ToUInt32(clientHeader.query["id"]);

                switch(Convert.ToUInt32(clientHeader.query["id"]))
                {
                    case 1:
                        strFileName = "tiny_image.jpg";
                        break;
                    case 2:
                        strFileName = "big_image.jpg";
                        break;
                }
                return new Response(@"C:\Users\User\source\repos\ik\test\" + strFileName);
            }
            return null;
        }

    }
}
