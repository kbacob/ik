// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;

    class SocketListener
    {
        public delegate void ConnectionDispatcher(Object tcpClient);

        private TcpListener tcpListener;
        private Thread threadListener;
        private ConnectionDispatcher fnConnectionDispatcher;

        private const int intListenerSleepInterval = 250;
        private static bool boolStopListener;

        public SocketListener(IPAddress ipAddress, int intPort)
        {
            tcpListener = new TcpListener(ipAddress, intPort);
        }
        ~SocketListener()
        {
            Stop();
            threadListener = null;
            tcpListener = null;
        }

        public void Start(ConnectionDispatcher fnConnectionDispatcher)
        {
            this.fnConnectionDispatcher = fnConnectionDispatcher;
            tcpListener.Start();
            boolStopListener = false;
            threadListener = new Thread(new ThreadStart(MainCycle));
            threadListener.Start();
        }
        public void Stop()
        {
            boolStopListener = true;
            //Thread.Sleep(500);
        }

        async private void MainCycle()
        {
            while (boolStopListener == false)
            {
                var TcpClient = await tcpListener.AcceptTcpClientAsync();

                ThreadPool.QueueUserWorkItem(new WaitCallback(fnConnectionDispatcher), TcpClient);
                Thread.Sleep(intListenerSleepInterval);
            }
            tcpListener.Stop();
        }
    }
}