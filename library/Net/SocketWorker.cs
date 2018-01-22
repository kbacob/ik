// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using ik.Utils;

    /// <summary>
    /// Диспетчер обработки строк запроса в url. Можно анализировать как сам url  - http://site.net/get/somestuff так и query - ?command=get&what=somestuff
    /// Остальное в коде, плюс пока что ещё нет окончательного варианта, который бы имело смысл документировать
    /// </summary>
    public sealed class SocketWorker 
    {
        /// <summary>
        /// Тип действия функции, добавляемой в качестве воркера
        /// </summary>
        public enum SocketWorkerJobType
        {
            /// <summary>
            /// Отображает шаблонный файл-заготовку
            /// </summary>
            ShowTemplate = 1,
            /// <summary>
            /// Послылает какой-либо файл
            /// </summary>
            SendFile,
            /// <summary>
            /// Посылает какой-либо текст, в том числе JSON
            /// </summary>
            SendText,
            /// <summary>
            /// Посылает код результата обработки запроса (404, 500, и т.д.)
            /// </summary>
            SendCode,
            /// <summary>
            /// Принимает файл
            /// </summary>
            GetFile,
            /// <summary>
            /// Принимает текст
            /// </summary>
            GetText,
            /// <summary>
            /// Принимает данные
            /// </summary>
            GetData,
            /// <summary>
            /// Делает что либо внутри себя, не возвращая ничего. Чтобы клиент не жаловался на потерю соединения, обработчик сам добавит корректный ответ.
            /// </summary>
            Silent,
            Other = int.MaxValue
        }
        /// <summary>
        /// Какую часть запроса анализировать для проверки требований на запуск воркера
        /// </summary>
        public enum SocketWorkerEntryType
        {
            /// <summary>
            /// Смотреть URL но не QUERY
            /// </summary>
            Url = 1,
            /// <summary>
            /// Смотреть в QUERY command=pattern, то есть указав в качестве паттерна show получим триггер при, например, http://site.ne/fake.cgi?command=show&id=666 (в данном случае триггеру пофигу на всё кроме command=show анализ прочего - забота воркера)
            /// </summary>
            Command
        }
        public delegate Response SocketWorkerFunction(ClientHeader clientHeader, NetworkStream objStream);
        public string strCommand = "command";
        public string strRootDirectory = null;

        private const int intClientReaderBufferSize = 4096;
        private const int intClientRequestMaxSize = 4096;

        private struct SocketWorkerItem
        {
            public string strEntryPointPattern;
            public SocketWorkerJobType SocketWorkerJobType;
            public Strings.EqualAnalysisType SocketWorkerEntryAnalysisType;
            public SocketWorkerEntryType SocketWorkerEntryType;
            public SocketWorkerFunction SocketWorkerFunction;
        }
        private Dictionary<string, SocketWorkerItem> Items = null;

        public SocketWorker()
        {
            Items = new Dictionary<string, SocketWorkerItem>();
        }
        ~SocketWorker()
        {
            Items.Clear();
            Items = null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientHeader"></param>
        /// <param name="objStreem"></param>
        /// <returns></returns>
        private Response EmptyWorker(ClientHeader clientHeader, NetworkStream objStreem) => new Response("", HttpStatusCode.OK);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientHeader"></param>
        /// <param name="socketWorkerItem"></param>
        /// <returns></returns>
        private bool CheckPattern(ClientHeader clientHeader, SocketWorkerItem socketWorkerItem)
        {
            switch (socketWorkerItem.SocketWorkerEntryType)
            {
                case SocketWorkerEntryType.Url:
                    var strEntryPoint = clientHeader["EntryPoint"];

                    if(!String.IsNullOrEmpty(strEntryPoint)) return Strings.Contains(strEntryPoint, socketWorkerItem.strEntryPointPattern, socketWorkerItem.SocketWorkerEntryAnalysisType);
                    break;

                case SocketWorkerEntryType.Command:
                    if (clientHeader.query != null)
                    {
                        if (clientHeader.query[strCommand] != null)
                        {
                            return Strings.Contains(clientHeader.query[strCommand], socketWorkerItem.strEntryPointPattern, socketWorkerItem.SocketWorkerEntryAnalysisType);
                        }
                    }
                    break;
            }

            return false;
        }
        private string MakeLocalFilePath(ClientHeader ClientHeader)
        {
            if (!String.IsNullOrEmpty(strRootDirectory))
            {
                var strPath = strRootDirectory;

                if (!String.IsNullOrEmpty(ClientHeader["Path"])) strPath += ClientHeader["Path"];
                if (!String.IsNullOrEmpty(ClientHeader["EntryPoint"]) && ClientHeader["EntryPoint"] != "/")
                {
                    strPath += ClientHeader["EntryPoint"];
                    return Files.FixPathByOS(strPath);
                }
            }
            return null;
        }
        private bool CheckFile(string strFileName)
        {
            return Files.Exists(strFileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="StateInfo"></param>
        public void Dispatcher(Object StateInfo)
        {
            var tcpStateInfo = (TcpClient)StateInfo;

            if (tcpStateInfo.Connected)
            {
                var objStream = tcpStateInfo.GetStream();
                var strRawClientRequestHeader = ReadClientData(objStream);
                var headerRequest = new ClientHeader(strRawClientRequestHeader, new Dictionary<string, string>() { { "ClientIP", tcpStateInfo.Client.RemoteEndPoint.ToString() } });

                Do(ref headerRequest, ref objStream);
                tcpStateInfo.Close();
            }
            return;
        }

        private string ReadClientData(NetworkStream objStream)
        {
            var strRequest = "";

            if (objStream.CanRead)
            {
                var byteBuffer = new byte[intClientReaderBufferSize];
                int intCount;

                while ((intCount = objStream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                {
                    strRequest += System.Text.Encoding.ASCII.GetString(byteBuffer, 0, intCount);
                    if (strRequest.IndexOf("\r\n\r\n") >= 0 || strRequest.Length > intClientRequestMaxSize) break;
                }
            }
            return strRequest;
        }
        private void WriteClientData(NetworkStream objStream, Response Response)
        {
            if (Response != null)
            {
                if (objStream.CanWrite)
                {
                   Response.Send(objStream);
                }
            }
        }

        /// <summary>
        /// Добавить воркер
        /// </summary>
        /// <param name="strWorkerName">Внутреннее имя, просто для удобства</param>
        /// <param name="strPattern">Паттерн-тригер, это может быть как подстрока, так и wildcard-маска или regex-выражение</param>
        /// <param name="socketWorkerFunction">Имя воркера</param>
        /// <param name="socketWorkerEntryType">Способ анализа для проверки условий вызова</param>
        /// <param name="socketWorkerEntryAnalysisType">Тип анализа - подстрока, так и wildcard-маска или regex-выражение</param>
        /// <param name="socketWorkerJobType">Что планируется делать, пока что это не сильно важно но, думаю, потом пригодитя для пре или пост работы с потоком соединения или какими-нибудь буфферами. В общем, TODO</param>
        public void Add(string strWorkerName, string strPattern, SocketWorkerFunction socketWorkerFunction = null, SocketWorkerEntryType socketWorkerEntryType = SocketWorkerEntryType.Command,  Strings.EqualAnalysisType socketWorkerEntryAnalysisType = Strings.EqualAnalysisType.Strong, SocketWorkerJobType socketWorkerJobType = SocketWorkerJobType.SendText)
        {
            if (String.IsNullOrEmpty(strWorkerName) && String.IsNullOrEmpty(strPattern)) throw new ArgumentNullException();

            var _item = new SocketWorkerItem
            {
                strEntryPointPattern = strPattern,
                SocketWorkerEntryType = socketWorkerEntryType,
                SocketWorkerEntryAnalysisType = socketWorkerEntryAnalysisType,
                SocketWorkerJobType = socketWorkerJobType,
                SocketWorkerFunction = socketWorkerFunction
            };

            if (socketWorkerFunction == null)
            {
                LogFile.Debug("Null function for SocketWorker named '" + strWorkerName + "' with pattern '" + strPattern + "'");
                _item.SocketWorkerFunction = EmptyWorker;
            }

            Items.Add(strWorkerName, _item);
        }

        /// <summary>
        /// После состоявшегося запроса от клиента, требуется понять как это дело обработать. Эта функция прогоняет таблицу добавленных воркеров и завершает работу на первом сработавшем триггере.
        /// </summary>
        /// <param name="clientHeader">Обработанный запрос клиента</param>
        /// <param name="objStream">Поток для чтения или записи </param>
        /// <returns></returns>
        private void Do(ref ClientHeader clientHeader, ref NetworkStream objStream)
        {
            var Response = (Response)null;
            var JobType = SocketWorkerJobType.Other;

            // пробуем отработать воркеры
            if (Items != null && Items.Count > 0) 
            {
                foreach (var Item in Items)
                {
                    if (CheckPattern(clientHeader, Item.Value))
                    {
                        Response = Item.Value.SocketWorkerFunction(clientHeader, objStream);
                        JobType = Item.Value.SocketWorkerJobType;
                        break;
                    }
                }
            }

            // если Response == null значит подходящего воркера не нашлось, пробуем тупо отдать файл
            if (Response == null)
            {
                var strFileName = MakeLocalFilePath(clientHeader);

                if (CheckFile(strFileName))
                {
                    Response = new Response(strFileName);
                    JobType = SocketWorkerJobType.SendFile;
                }
            }

            // если Response == null значит подходящих воркера или файлоа не нашлось, отдаём Not Found
            if (Response == null)
            {
                Response = new Response("404", HttpStatusCode.NotFound);
                JobType = SocketWorkerJobType.SendCode;
            }

            switch(JobType)
            {
                case SocketWorkerJobType.Other:
                case SocketWorkerJobType.SendText:
                case SocketWorkerJobType.SendCode:
                case SocketWorkerJobType.SendFile:
                    WriteClientData(objStream, Response);
                    break;
            }
        }
    }
}
