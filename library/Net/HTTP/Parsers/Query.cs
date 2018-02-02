// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.HTTP.Parsers
{
    using System;
    using System.Web;
    using ik.Types;

    /// <summary>
    ///  Работа с query-частью url, полученного в результате обработки запроса к HTTP-серверу
    /// </summary>
    public class Query : KeyValueList
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strQuery"></param>
        public Query(string strQuery)
        {
            Clear();
            if (!String.IsNullOrEmpty(strQuery))
            {
                var lQuery = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(strQuery));
                
                if (lQuery != null && lQuery.Count > 0)
                {
                    foreach (string strKey in lQuery)
                    {
                        Add(strKey, lQuery[strKey]);
                    }
                    lQuery.Clear();
                }
            }
        }
        ~Query()
        {
            Clear();
        }
    }
}