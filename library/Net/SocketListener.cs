// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;

    class SocketListener : TcpListener
    {
        public delegate void ConnectionDispatcher(Object tcpClient);

        private Thread threadListener;
        private ConnectionDispatcher fnConnectionDispatcher;

        private const int intListenerSleepInterval = 250;
        private static bool boolStopListener;

        public SocketListener(IPAddress ipAddress, int intPort) : base(ipAddress, intPort)
        {
        }
        ~SocketListener()
        {
            Stop();
            threadListener = null;
        }

        public void Start(ConnectionDispatcher fnConnectionDispatcher)
        {
            this.fnConnectionDispatcher = fnConnectionDispatcher;
            Start();
            boolStopListener = false;
            threadListener = new Thread(new ThreadStart(MainCycle));
            threadListener.Start();
        }
        public new void Stop()
        {
            boolStopListener = true;
        }

        async private void MainCycle()
        {
            while (!boolStopListener)
            {
                var TcpClient = await AcceptTcpClientAsync();

                ThreadPool.QueueUserWorkItem(new WaitCallback(fnConnectionDispatcher), TcpClient);
                Thread.Sleep(intListenerSleepInterval);
            }
            base.Stop();
        }
    }
}