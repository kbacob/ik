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
        public delegate string HTTPWorker(ClientHeader clientHeader, NetworkStream objStream);

        private SocketListener CSocketListener;
        private SocketDispatcher CSocketDispatcher;
        
        private Dictionary<string, HTTPWorker> httpWorkers;
        private Dictionary<string, string> httpFiles;
        private Dictionary<string, string> httpPathes;

        public HTTPServer(string strListenAddress = "127.0.0.1", int intPort = 80)
        {
            CSocketListener = new SocketListener(Strings.Equals(strListenAddress, "any") ? IPAddress.Any : IPAddress.Parse(strListenAddress), intPort);
            CSocketDispatcher = new SocketDispatcher();

            httpWorkers = new Dictionary<string, HTTPWorker>();
            httpFiles = new Dictionary<string, string>();
            httpPathes = new Dictionary<string, string>();
        }
        ~HTTPServer()
        {
            Stop();
            CSocketDispatcher = null;
            CSocketListener = null;
            httpWorkers.Clear();
            httpFiles.Clear();
            httpPathes.Clear();
            httpWorkers = null;
            httpFiles = null;
            httpPathes = null;
        }

        public void Start()
        {
            CSocketListener.Start(CSocketDispatcher.Main);
        }
        public void Stop()
        {
            CSocketListener.Stop();
        }

        public void AddHttpWorker(string strEntryPoint, HTTPWorker func)
        {
            if (Strings.Exists(strEntryPoint))
            {
                if (!httpWorkers.ContainsKey(strEntryPoint))
                {
                    httpWorkers.Add(strEntryPoint, func);
                }
                else
                {
                    httpWorkers[strEntryPoint] = func;
                }
            }
        }
        public void AddHttpFileAlias(string strFileNameAlias, string strRealFileName)
        {
            if (Files.Exists(strRealFileName))
            {
                if (!httpFiles.ContainsKey(strFileNameAlias))
                {
                    httpFiles.Add(strFileNameAlias, strRealFileName);
                }
                else
                {
                    httpFiles[strFileNameAlias] = strRealFileName;
                }
            }
        }
        public void AddHttpPathAlias(string strPathAlias, string strRealPath)
        {
            if (Files.Exists(strRealPath))
            {
                if (!httpPathes.ContainsKey(strPathAlias))
                {
                    httpPathes.Add(strPathAlias, strRealPath);
                }
                else
                {
                    httpPathes[strPathAlias] = strRealPath;
                }
            }
        }

        private string _Worker(ClientHeader headerRequest, NetworkStream objStream)
        {
            string strResult = null;
            HTTPWorker func = null;

            if (httpWorkers != null)
            {
                if (headerRequest.ContainsKey("EntryPoint"))
                {
                    if (Strings.Exists(headerRequest["EntryPoint"]))
                    {
                        string strEntryPoint = headerRequest["EntryPoint"];

                        if (httpWorkers.ContainsKey(strEntryPoint)) // Вариант 1) - "в лоб" смотрим, нет ли обработчика для такого имени
                        {
                            func = httpWorkers[strEntryPoint];
                        }
                        else
                        {
                            foreach (string strMask in httpWorkers.Keys) // Вариант 2) - пытаемся применить ключ обработчика как маску сравнения
                            {
                                if (Regex.IsMatch(strEntryPoint, strMask))
                                {
                                    func = httpWorkers[strMask];
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (func != null)
            {
                strResult = func(headerRequest, objStream);
            }


            return strResult;
        }

    }
}