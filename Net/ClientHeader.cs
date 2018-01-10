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
            if (Strings.Exists(strClientRequest))
            {
                string[] strStrings = strClientRequest.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (strStrings.Length > 0)
                {
                    if (Strings.Exists(strStrings[0]))
                    {
                        Match match = Regex.Match(strStrings[0], @"^(\S*)?\s(\S+)?\s(\S+)$"); // parse request method string like 'GET something HTTP/X.Y'

                        if (match != null)
                        {
                            if (match.Groups.Count == 4)
                            {
                                Add("Method", match.Groups[1].Value);
                                Add("Uri", match.Groups[2].Value);
                                Add("Protocol", match.Groups[3].Value);

                                match = Regex.Match(this["Uri"], @"^(?:https*:\/\/\S+){0,1}(?:.*)\/(\S*)\?(\S+)$"); // parse request url string like '/index.php?param1=value&param2=value'

                                if (match.Groups.Count == 3)
                                {
                                    Add("EntryPoint", match.Groups[1].Value.Length > 0 ? match.Groups[1].Value : @"/");
                                    Add("QueryString", match.Groups[2].Value);
                                    query = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(match.Groups[2].Value));
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
                    for (int intTmp = 1; intTmp < strStrings.Length; intTmp++)
                    {
                        if (Strings.Exists(strStrings[intTmp]))
                        {
                            Match match = Regex.Match(strStrings[intTmp], @"^(\S+)\:\s*(.*)?$");

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
                    foreach(KeyValuePair<string,string> tmpEntry in dEnviromentVariables)
                    {
                        if (!this.ContainsKey(tmpEntry.Key))
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
            if (Strings.Exists(strMethodCode))
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