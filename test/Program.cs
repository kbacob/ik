// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ConsoleApp1
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using ik.Net;
    using ik.Net.HTTP.Parsers;
    using ik.Utils;

    /// <summary>
    ///  Test app
    /// </summary>
    class Program
    {
        static ik.Net.HTTP.Server http;

        static void Main(string[] args)
        {
            var parser = new ik.Template.Parser();

            
            Console.Write(parser.ParseFile(@"C:\Temp\www\1.tpl"));





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
            http = new ik.Net.HTTP.Server("any", 80); // если на Linux то либо порт > 1024 либо от суперюзера гонять, иначе будет исключение по AccessDenied
            http.SocketWorker.strRootDirectory = @"C:\Temp\www";
            http.SocketWorker.Add("Test Worker 1", "hello", Test1, ik.Net.Sockets.Worker.EntryType.Url, Strings.EqualAnalysisType.Strong, ik.Net.Sockets.Worker.JobType.SendText); // https://127.0.0.1/HEYhello[/пофиг-чё?и&тут&тоже&плевать 
            http.SocketWorker.Add("Test Worker 2", "get_image", Test2, ik.Net.Sockets.Worker.EntryType.Command, Strings.EqualAnalysisType.Strong, ik.Net.Sockets.Worker.JobType.SendFile); // https://127.0.0.1/[/пофиг-чё]?command=get_image&id=123
            http.SocketWorker.Add("Test Worker 3", "CONNECT", Test3, ik.Net.Sockets.Worker.EntryType.Method, Strings.EqualAnalysisType.Equal, ik.Net.Sockets.Worker.JobType.SendText);
        }

        static Response Test1(Request clientHeader, NetworkStream stream)
        {
            Console.WriteLine(clientHeader.Get("ClientIP"));
            return new Response("Hello", HttpStatusCode.OK);
        }
        static Response Test2(Request clientHeader, NetworkStream objStream)
        {
            if (clientHeader.Query.Get("id") != null)
            {
                string strFileName = null;
                var id =  Convert.ToUInt32(clientHeader.Query.Get("id"));

                switch(Convert.ToUInt32(clientHeader.Query.Get("id")))
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
        static Response Test3(Request clientHeader, NetworkStream objStream) // TODO: Сейчас тупо шлём в сад. Наваять костыль вида "целиком Скачал - целиком Отдал"
        {
            var strRemoteUrl = clientHeader.Get("Uri");

            if(!String.IsNullOrEmpty(strRemoteUrl))
            {
                
            }
            return new Response("CONNECT Not Allowed", HttpStatusCode.MethodNotAllowed); ;
        }
    }
}
