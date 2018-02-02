// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.HTTP.Parsers
{
    using System;
    using ik.Utils;

    /// <summary>
    /// Вспомогательные функции для анализа полей заголовка HTTP-запроса
    /// </summary>
    public static class RequestHeader
    {
        public static bool Connection(string strConnection)
        {
            if (!String.IsNullOrEmpty(strConnection) && String.Equals(strConnection, "keep-alive", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
        public static Tuple<int,int> KeepAlive(string strKeepAlive)
        {
            if (!String.IsNullOrEmpty(strKeepAlive))
            {
                var strTimeout = Strings.GetPairsValue(strKeepAlive, "timeout");
                var strMaxConnections = Strings.GetPairsValue(strKeepAlive, "max");
                int intTimeout = 0;
                int intMaxConnections = 0;

                Int32.TryParse(strTimeout, out intTimeout);
                Int32.TryParse(strMaxConnections, out intMaxConnections);

                return Tuple.Create(intTimeout, intMaxConnections);
            }
            return Tuple.Create(0, 0);
        }
    }
}