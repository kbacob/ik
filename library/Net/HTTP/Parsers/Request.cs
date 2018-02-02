// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.HTTP.Parsers
{
    using System;
    using System.Text.RegularExpressions;
 
    /// <summary>
    /// Парсинг заголовков клиентского запроса HTTP
    /// </summary>
    public class Request : HTTP.Header
    {
        /// <summary>
        /// Разбираем Url запроса
        /// </summary>
        private void ParseUrl()
        {
            string strScheme = null;
            string strAddress = null;
            string strUrl = null;
            string strBookmark = null;
            string strQuery = null;

            var match = Regex.Match(Get("Uri"), @"^(?<Scheme>https*)\:\/\/(?<Address>.+?)(?<Path>\/.*)");
            if (match.Success)
            {
                strScheme = match.Groups["Scheme"].Value;
                strAddress = match.Groups["Address"].Value;
                strUrl = match.Groups["Path"].Value;
            }
            else
            {
                strScheme = Get("Protocol");
                strAddress = "";
                strUrl = Get("Uri");

                if (!String.IsNullOrEmpty(Get("Host"))) strAddress = Get("Host");
            }

            if (strUrl.Contains("#"))
            {
                match = Regex.Match(strUrl, @"^(?<Path>\/.*?)\#(?<Bookmark>.*)");
                if (match.Success)
                {
                    strUrl = match.Groups["Path"].Value;
                    strBookmark = match.Groups["Bookmark"].Value;
                }
                else throw new Exception();
            }
            else strBookmark = "";

            if (strUrl.Contains("?"))
            {
                match = Regex.Match(strUrl, @"^(?<Path>\/.*)\?(?<Query>.*)");
                if (match.Success)
                {
                    strUrl = match.Groups["Path"].Value;
                    strQuery = match.Groups["Query"].Value;
                }
                else throw new Exception();
            }
            else strQuery = "";

            if (!String.IsNullOrEmpty(strQuery))
            {
                Query = new Query(strQuery);
            }

            Add("EntryPoint", strUrl);    // Path-часть зароса
            Add("Scheme", strScheme);     // http, https, ftp, ws и т.п.
            Add("Address", strAddress);   // fqdn или сетевое имя
            Add("Bookmark", strBookmark); // если в запросе был букмарк (#blablabla)
            Add("QueryString", strQuery); // елси в запросе было ?aaa=bbb&ccc=ddd 
        }

        /// <summary>
        /// Парсинг заголовков клиентского запроса HTTP
        /// </summary>
        /// <param name="strClientRequest">RAW-строка запроса, полученная методом чтения NetworkStream клиентского соединения.</param>
        /// <param name="dEnviromentVariables">Дополнительные значения в создаваемы словарь. Например, ClientIp, данные о котоом знает диспечер Socket-соединения</param>
        /// 
        public Request(string strClientRequest) : base()
        {
            if (String.IsNullOrEmpty(strClientRequest)) throw new ArgumentNullException("strClientRequest");

            var strStrings = strClientRequest.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (strStrings.Length > 0)
            {
                if (!String.IsNullOrEmpty(strStrings[0]))
                {
                    var match = Regex.Match(strStrings[0], @"^(?<Method>\S+)\s+(?<Uri>\S+)\s+(?<ProtocolFamily>\S+)\/(?<ProtocolVersion>\d+)\.(?<ProtocolSubversion>\d+).*$"); 

                    if (match.Success)
                    {
                        Add("Method", match.Groups["Method"].Value);
                        Add("Uri", match.Groups["Uri"].Value);
                        Add("Protocol", match.Groups["ProtocolFamily"].Value);
                        Add("ProtocolVersion", match.Groups["ProtocolVersion"].Value);
                        Add("ProtocolSubversion", match.Groups["ProtocolSubversion"].Value);
                    }
                }
            }

            if (strStrings.Length > 1)
            {
                for (var intTmp = 1; intTmp < strStrings.Length; intTmp++)
                {
                    if (!String.IsNullOrEmpty(strStrings[intTmp]))
                    {
                        var match = Regex.Match(strStrings[intTmp], @"^(?<Key>\S+)\:\s*(?<Value>.*)?$");

                        if (match.Success)
                        {
                             Add(match.Groups["Key"].Value, match.Groups["Value"].Value);
                        }
                    }
                }
            }

            if (!String.IsNullOrEmpty(Get("Uri")))
            {
                ParseUrl();
            }
        }
    }
}