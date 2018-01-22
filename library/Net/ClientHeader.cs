// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Web;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using ik.Utils;

    /// <summary>
    /// Парсинг заголовков клиентского запроса HTTP в удобно читаемый словарь.
    /// </summary>
    public sealed class ClientHeader : Dictionary<string, string>
    {
        /// <summary>
        /// Пары key=value для query-части строки url-запроса.
        /// </summary>
        public NameValueCollection query = null;

        /// <summary>
        /// Парсинг заголовков клиентского запроса HTTP в удобно читаемый словарь.
        /// </summary>
        /// <param name="strClientRequest">RAW-строка запроса, полученная методом чтения NetworkStream клиентского соединения.</param>
        /// <param name="dEnviromentVariables">Дополнительные значения в создаваемы словарь. Например, ClientIp, данные о котоом знает диспечер Socket-соединения</param>
        public ClientHeader(string strClientRequest, Dictionary<string, string> dEnviromentVariables)
        {
            Clear();
            if (!String.IsNullOrEmpty(strClientRequest))
            {
                var strStrings = strClientRequest.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (strStrings.Length > 0)
                {
                    if (!String.IsNullOrEmpty(strStrings[0]))
                    {
                        var match = Regex.Match(strStrings[0], @"^(?<Method>\S+)\s+(?<Uri>\S+)\s+(?<Protocol>\S+).*$"); 

                        if (match != null)
                        {
                            if (match.Groups.Count > 1)
                            {
                                Add("Method", match.Groups["Method"].Value);
                                Add("Uri", match.Groups["Uri"].Value);
                                Add("Protocol", match.Groups["Protocol"].Value);

                                match = Regex.Match(this["Uri"], @"^(?<Path>\/.*){0,1}(?<Entry>\/.*)"); 

                                if (match.Groups.Count > 1)
                                {
                                    var strEntryPoint = match.Groups["Entry"].Value;
                                    Add("Path", match.Groups["Path"].Value);

                                    if (strEntryPoint.Contains("?"))
                                    {
                                        match = Regex.Match(strEntryPoint, @"(?<EntryPoint>.*)\?(?<Query>.*){0,1}");

                                        Add("EntryPoint", match.Groups["EntryPoint"].Value);
                                        Add("QueryString", match.Groups["Query"].Value);
                                        query = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(match.Groups["Query"].Value));
                                    }
                                    else
                                    {
                                        Add("EntryPoint", strEntryPoint);
                                        Add("QueryString", "");
                                    }
                                }
                                else
                                {
                                    Add("EntryPoint", @"/");
                                    Add("QueryString", "");
                                }
                            }
                        }
                    }
                }
                if (strStrings.Length > 1)
                {
                    for (var intTmp = 1; intTmp < strStrings.Length; intTmp++)
                    {
                        if (!String.IsNullOrEmpty(strStrings[intTmp]))
                        {
                            var match = Regex.Match(strStrings[intTmp], @"^(\S+)\:\s*(.*)?$");

                            if (match != null)
                            {
                                if (match.Groups.Count == 3)
                                {
                                    Add(match.Groups[1].Value, match.Groups[2].Value);
                                }
                            }
                        }
                    }
                }
            }
            if (dEnviromentVariables != null)
            {
                if(dEnviromentVariables.Count > 0)
                {
                    foreach(var tmpEntry in dEnviromentVariables)
                    {
                        if (!ContainsKey(tmpEntry.Key))
                        {
                            Add(tmpEntry.Key, tmpEntry.Value);
                        }
                        else
                        {
                            this[tmpEntry.Key] = tmpEntry.Value;
                        } 
                    }
                }
            }
        }

        private bool MethodIs(string strMethodCode)
        {
            if (!String.IsNullOrEmpty(strMethodCode))
            {
                if (ContainsKey("Method"))
                {
                    if (String.Equals(this["Method"], strMethodCode, StringComparison.CurrentCultureIgnoreCase)) return true;
                }
            }
            return false;
        }

        public bool MethodIsGet => MethodIs("GET");
        public bool MethodIsPost => MethodIs("POST");
        public bool MethodIsHead => MethodIs("HEAD");
        public bool MethodIsOptions => MethodIs("OPTIONS");
        public bool MethodIsPut => MethodIs("PUT");
        public bool MethodIsPatch => MethodIs("PATCH");
        public bool MethodIsLink => MethodIs("LINK");
        public bool MethodIsUnlink => MethodIs("UNLINK");
        public bool MethodIsTrace => MethodIs("TRACE");
        public bool MethodIsConnnect => MethodIs("CONNECT");
    }
}