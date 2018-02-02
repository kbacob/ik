// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.Sockets
{
    using System;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Базовый слушатель порта
    /// </summary>
    public class Listener : TcpListener
    {
        private Thread threadListener = null;
        private const int intListenerSleepInterval = 250;
        private static bool boolStopListener = true;
        public delegate bool ManageConnection(TcpClient tcpClient);
        private ManageConnection ManageConnectionFunc = null;

        public Listener(IPAddress ipAddress, int intPort) : base(ipAddress, intPort)
        {
        }
        ~Listener()
        {
            Stop();
            threadListener = null;
        }

        public void Start(ManageConnection ManageConnectionFunc)
        {
            this.ManageConnectionFunc = ManageConnectionFunc;
            Start();
            boolStopListener = false;
            threadListener = new Thread(new ThreadStart(MainCycle));
            threadListener.Start();
        }
        public new void Stop()
        {
            boolStopListener = true;
        }

        private async void MainCycle()
        {
            while (!boolStopListener)
            {
                var TcpClient = await AcceptTcpClientAsync();

                ThreadPool.QueueUserWorkItem(new WaitCallback(OpenConnection), TcpClient);
                Thread.Sleep(intListenerSleepInterval);
            }
            base.Stop();
        }
        private void OpenConnection(Object StateInfo)
        {
            using (var tcpStateInfo = (TcpClient)StateInfo)
            {
                if (tcpStateInfo.Connected && tcpStateInfo.Available > 0)
                {
                    if (!ManageConnectionFunc(tcpStateInfo)) return;
                }

                try
                {
                    tcpStateInfo.GetStream().Close();
                    tcpStateInfo.Close();
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }
    }
}