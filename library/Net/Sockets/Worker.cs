// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.Sockets
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using ik.Utils;
    using ik.Net;


    /// <summary>
    /// Диспетчер обработки строк запроса в url. Можно анализировать как сам url  - http://site.net/get/somestuff так и query - ?command=get&what=somestuff
    /// Остальное в коде, плюс пока что ещё нет окончательного варианта, который бы имело смысл документировать
    /// </summary>
    public class Worker 
    {
        /// <summary>
        /// Тип действия функции, добавляемой в качестве воркера
        /// </summary>
        public enum JobType
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
        public enum EntryType
        {
            /// <summary>
            /// Смотреть URL но не QUERY
            /// </summary>
            Url = 1,
            /// <summary>
            /// Смотреть в QUERY command=pattern, то есть указав в качестве паттерна show получим триггер при, например, http://site.ne/fake.cgi?command=show&id=666 (в данном случае триггеру пофигу на всё кроме command=show анализ прочего - забота воркера)
            /// </summary>
            Command,
            /// <summary>
            /// Смотреть поле в заголовке запроса
            /// </summary>
            Header,
            /// <summary>
            /// Смотреть метод
            /// </summary>
            Method
        }

        public delegate Response SocketWorkerFunction(HTTP.Parsers.Request clientHeader, NetworkStream objStream);
        public string strCommand = "command";
        public string strRootDirectory = null;

        private const int intClientReaderBufferSize = 4096;
        private const int intClientRequestMaxSize = 4096;

        private struct SocketWorkerItem
        {
            public string strEntryPointPattern;
            public JobType SocketWorkerJobType;
            public Strings.EqualAnalysisType SocketWorkerEntryAnalysisType;
            public EntryType SocketWorkerEntryType;
            public SocketWorkerFunction SocketWorkerFunction;
        }
        private Dictionary<string, SocketWorkerItem> Items = null;

        public Worker()
        {
            Items = new Dictionary<string, SocketWorkerItem>();
        }
        ~Worker()
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
        private Response EmptyWorker(HTTP.Parsers.Request clientHeader, NetworkStream objStreem) => new Response("", HttpStatusCode.OK);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientHeader"></param>
        /// <param name="socketWorkerItem"></param>
        /// <returns></returns>
        private bool CheckPattern(HTTP.Parsers.Request clientHeader, SocketWorkerItem socketWorkerItem)
        {
            switch (socketWorkerItem.SocketWorkerEntryType)
            {
                case EntryType.Url:
                    if (!String.IsNullOrEmpty(clientHeader.Get("EntryPoint")))
                    {
                        var strEntryPoint = clientHeader.Get("EntryPoint");

                        return Strings.Contains(strEntryPoint, socketWorkerItem.strEntryPointPattern, socketWorkerItem.SocketWorkerEntryAnalysisType);
                    }

                    break;

                case EntryType.Command:
                    if (clientHeader.Query != null)
                    {
                        if (clientHeader.Query.Get(strCommand) != null)
                        {
                            var strTmp = clientHeader.Query.Get(strCommand);

                            return String.Equals(strTmp, socketWorkerItem.strEntryPointPattern);
                        }
                    }
                    break;

                case EntryType.Method:
                    var strMethod = clientHeader.Get("Method");

                    if(!String.IsNullOrEmpty(strMethod))
                    {
                        return Strings.Contains(strMethod, socketWorkerItem.strEntryPointPattern, socketWorkerItem.SocketWorkerEntryAnalysisType);
                    }
                    break;

                case EntryType.Header:
                    if (!String.IsNullOrEmpty(clientHeader.Get(socketWorkerItem.strEntryPointPattern))) return true;
                    break;
            }

            return false;
        }
        private string MakeLocalFilePath(HTTP.Parsers.Request ClientHeader)
        {
            if (!String.IsNullOrEmpty(strRootDirectory))
            {
                var strPath = strRootDirectory;

                if (!String.IsNullOrEmpty(ClientHeader.Get("Path"))) strPath += ClientHeader.Get("Path");
                if (!String.IsNullOrEmpty(ClientHeader.Get("EntryPoint")) && ClientHeader.Get("EntryPoint") != "/")
                {
                    strPath += ClientHeader.Get("EntryPoint");
                    return Files.FixPathByOS(strPath);
                }
            }
            return null;
        }
        private bool CheckFile(string strFileName)
        {
            return Files.Exists(strFileName);
        }

        public string ReadClientData(NetworkStream objStream)
        {
            var strRequest = "";

            if (objStream.CanRead)
            {
                var byteBuffer = new byte[intClientReaderBufferSize];
                int intCount;

                try
                {
                    while ((intCount = objStream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
                    {
                        strRequest += System.Text.Encoding.ASCII.GetString(byteBuffer, 0, intCount);
                        if (strRequest.IndexOf("\r\n\r\n") >= 0 || strRequest.Length > intClientRequestMaxSize) break;
                    }
                }
                catch (Exception)
                {
                    //throw;
                }

            }
            return strRequest;
        }
        public void WriteClientData(NetworkStream objStream, Response Response)
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
        public void Add(string strWorkerName, string strPattern, SocketWorkerFunction socketWorkerFunction = null, EntryType socketWorkerEntryType = EntryType.Command,  Strings.EqualAnalysisType socketWorkerEntryAnalysisType = Strings.EqualAnalysisType.Strong, JobType socketWorkerJobType = JobType.SendText)
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
        public void Do(ref HTTP.Parsers.Request clientHeader, ref NetworkStream objStream)
        {
            Response objResponse = null;
            var JobType = Worker.JobType.Other;

            // пробуем отработать воркеры
            if (Items != null && Items.Count > 0) 
            {
                foreach (var Item in Items)
                {
                    if (CheckPattern(clientHeader, Item.Value))
                    {
                        objResponse = Item.Value.SocketWorkerFunction(clientHeader, objStream);
                        JobType = Item.Value.SocketWorkerJobType;
                        break;
                    }
                }
            }

            // если Response == null значит подходящего воркера не нашлось, пробуем тупо отдать файл
            if (objResponse == null)
            {
                var strFileName = MakeLocalFilePath(clientHeader);

                if (!String.IsNullOrEmpty(strFileName) && CheckFile(strFileName))
                {
                    objResponse = new Response(strFileName);
                    JobType = JobType.SendFile;
                }
            }

            // если Response == null значит подходящих воркера или файлоа не нашлось, отдаём Not Found
            if (objResponse == null)
            {
                objResponse = new Response("404", HttpStatusCode.NotFound);
                JobType = JobType.SendCode;
            }

            switch(JobType)
            {
                case JobType.Other:
                case JobType.SendText:
                case JobType.SendCode:
                case JobType.SendFile:
                    WriteClientData(objStream, objResponse);
                    break;
            }
        }
    }
}
