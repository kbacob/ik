// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Threading;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using ik.Utils;

    class SocketListener
    {
        public delegate void ConnectionDispatcher(Object tcpClient);

        private TcpListener tcpListener;
        private Thread threadListener;
        private ConnectionDispatcher fnConnectionDispatcher;

        private const int intListenerSleepInterval = 250;
        private bool boolStopListener;

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
            tcpListener.Stop();
        }

        async private void MainCycle()
        {
            while (boolStopListener == false)
            {
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();

                ThreadPool.QueueUserWorkItem(new WaitCallback(fnConnectionDispatcher), tcpClient);
                Thread.Sleep(intListenerSleepInterval);
            }
        }
    }

    sealed class SocketDispatcher
    {
        private const int intClientReaderBufferSize = 4096;
        private const int intClientRequestMaxSize = 4096;

        public void Main(Object StateInfo)
        {
            TcpClient tcpStateInfo = (TcpClient)StateInfo;

            if (tcpStateInfo.Connected)
            {
                NetworkStream objStream = tcpStateInfo.GetStream();
                string strRawClientRequestHeader = _ReadClientData(objStream);
                ClientHeader headerRequest = new ClientHeader(strRawClientRequestHeader, new Dictionary<string, string>(){{"ClientIP", tcpStateInfo.Client.RemoteEndPoint.ToString()}});
                Response response;
                string strWorks;

                strWorks = ""; //_Worker(headerRequest, objStream);

                if (Strings.Exists(strWorks))
                {
                    response = new Response(strWorks);
                }
                else
                {
                    response = new Response("404", HttpStatusCode.NotFound);
                }

                _WriteClientData(objStream, response.Value);
                tcpStateInfo.Close();
            }
            return;
        }

        private string _ReadClientData(NetworkStream objStream)
        {
            string strRequest = "";

            if (objStream.CanRead)
            {
                byte[] byteBuffer = new byte[intClientReaderBufferSize];
                int intCount;

                while ((intCount = objStream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                {
                    strRequest += System.Text.Encoding.ASCII.GetString(byteBuffer, 0, intCount);
                    if (strRequest.IndexOf("\r\n\r\n") >= 0 || strRequest.Length > intClientRequestMaxSize) break;
                }
            }
            return strRequest;
        }
        private void _WriteClientData(NetworkStream objStream, byte[] byteBuffer)
        {
            if (byteBuffer != null)
            {
                if (byteBuffer.Length > 0)
                    if (objStream.CanWrite)
                    {
                        objStream.Write(byteBuffer, 0, byteBuffer.Length);
                    }
            }
        }
    }
}