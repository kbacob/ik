// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.PROXY
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using ik.Net.Sockets;

    public sealed class Server : Listener
    {
        public Worker SocketWorker;

        public Server(string strListenAddress = "any", int intPort = 8080) : base(String.Equals(strListenAddress, "any", StringComparison.OrdinalIgnoreCase) ? IPAddress.Any : IPAddress.Parse(strListenAddress), intPort)
        { 
            SocketWorker = new Worker();
        }
        ~Server()
        {
            Stop();
            SocketWorker = null;
        }

        public new void Start()
        {
            Start(ConnectionManager);
        }

        private bool ConnectionManager(TcpClient tcpClient)
        {
            var objStream = tcpClient.GetStream();
            var strRawClientRequestHeader = SocketWorker.ReadClientData(objStream);

            if (!String.IsNullOrEmpty(strRawClientRequestHeader))
            {
                var headerRequest = new HTTP.Parsers.Request(strRawClientRequestHeader);

                headerRequest.Add("ClientIP", tcpClient.Client.RemoteEndPoint.ToString());
                SocketWorker.Do(ref headerRequest, ref objStream);
            }

            return true;
        }
    }
}
