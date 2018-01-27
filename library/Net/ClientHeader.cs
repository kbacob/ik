// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Web;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using System.Net;

    /// <summary>
    /// Парсинг заголовков клиентского запроса HTTP
    /// </summary>
    public sealed class ClientHeader : WebHeaderCollection
    {
        /// <summary>
        /// Пары key=value для query-части строки url-запроса.
        /// </summary>
        public NameValueCollection query = null;

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
            if (match != null && match.Groups.Count > 1)
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
                if (match != null && match.Groups.Count > 1)
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
                if (match != null && match.Groups.Count > 1)
                {
                    strUrl = match.Groups["Path"].Value;
                    strQuery = match.Groups["Query"].Value;
                }
                else throw new Exception();
            }
            else strQuery = "";

            if (!String.IsNullOrEmpty(strQuery))
            {
                query = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(strQuery));
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
        public ClientHeader(string strClientRequest)
        {
            Clear();
            if (String.IsNullOrEmpty(strClientRequest)) throw new ArgumentNullException("strClientRequest");

            var strStrings = strClientRequest.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (strStrings.Length > 0)
            {
                if (!String.IsNullOrEmpty(strStrings[0]))
                {
                    var match = Regex.Match(strStrings[0], @"^(?<Method>\S+)\s+(?<Uri>\S+)\s+(?<Protocol>\S+).*$"); 

                    if (match != null && match.Groups.Count > 1)
                    {
                        Add("Method", match.Groups["Method"].Value);
                        Add("Uri", match.Groups["Uri"].Value);
                        Add("Protocol", match.Groups["Protocol"].Value);
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

                        if (match != null && match.Groups.Count > 1)
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