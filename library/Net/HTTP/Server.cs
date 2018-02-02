// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.HTTP
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using ik.Net.Sockets;
    using ik.Net.HTTP.Parsers;
  
    /// <summary>
    /// Реализация многопоточного HTTP-сервера
    /// </summary>
    public sealed class Server : Listener
    {
        public Worker SocketWorker;
        private const int intTimeOutSeconds = 15;

        public Server(string strListenAddress = "127.0.0.1", int intPort = 80) : base(String.Equals(strListenAddress, "any") ? IPAddress.Any : IPAddress.Parse(strListenAddress), intPort)
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
            var boolCloseConnection = false;
            var dateBeginConnection = DateTime.Now;

            objStream.ReadTimeout = intTimeOutSeconds * 1000;

            while (boolCloseConnection == false)
            {
                var strRawClientRequestHeader = SocketWorker.ReadClientData(objStream);

                if (DateTime.Now >= dateBeginConnection.AddSeconds(intTimeOutSeconds)) boolCloseConnection = true;

                if (!String.IsNullOrEmpty(strRawClientRequestHeader))
                {
                    var headerRequest = new Request(strRawClientRequestHeader);

                    if (!RequestHeader.Connection(headerRequest.Get(HeaderItem.GetRfcString(HeaderItem.Type.Connection)))) boolCloseConnection = true;

                    if (boolCloseConnection) headerRequest.Set(HeaderItem.GetRfcString(HeaderItem.Type.Connection), "close");
                    headerRequest.Add("ClientIP", tcpClient.Client.RemoteEndPoint.ToString());
                    SocketWorker.Do(ref headerRequest, ref objStream);
                }
                else boolCloseConnection = true;
            }

            return boolCloseConnection;
        }
    }
}