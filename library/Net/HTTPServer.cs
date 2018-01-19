// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using ik.Utils;

    /// <summary>
    /// Реализация многопоточного HTTP-сервера
    /// </summary>
    public sealed class HTTPServer
    {
        private SocketListener CSocketListener;
        public SocketWorker CSocketWorker;

        public HTTPServer(string strListenAddress = "127.0.0.1", int intPort = 80)
        {
            CSocketListener = new SocketListener(Equals(strListenAddress, "any") ? IPAddress.Any : IPAddress.Parse(strListenAddress), intPort);
            CSocketWorker = new SocketWorker();
        }
        ~HTTPServer()
        {
            Stop();
            CSocketWorker = null;
            CSocketListener = null;
        }

        public void Start()
        {
            CSocketListener.Start(CSocketWorker.Dispatcher);
        }
        public void Stop()
        {
            CSocketListener.Stop();
        }
    }
}