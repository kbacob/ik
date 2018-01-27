// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Net;
    using System.Collections.Generic;

    /// <summary>
    /// Создание элемента заголовка HTTP-ответа
    /// </summary>
    static internal class HTTPHeader 
    {
        static public KeyValuePair<string, string> Make(HttpResponseHeader httpResponseHeader, object objObject = null)
        {
            switch (httpResponseHeader)
            {
                case HttpResponseHeader.AcceptRanges:
                    return AcceptRanges((bool)objObject);

                case HttpResponseHeader.Age:
                    return Age((int)objObject);

                case HttpResponseHeader.Allow:
                    return Allow((string)objObject);

                case HttpResponseHeader.CacheControl:
                    return CacheControl((string)objObject);

                case HttpResponseHeader.Connection:
                    return Connection((bool)objObject);

                case HttpResponseHeader.ContentEncoding:
                    return ContentEncoding((string)objObject);

                case HttpResponseHeader.ContentLanguage:
                    return ContentLanguage((string)objObject);

                case HttpResponseHeader.ContentLength:
                    return ContentLength((ulong)objObject);

                case HttpResponseHeader.ContentLocation:
                    return ContentLocation((string)objObject);

                case HttpResponseHeader.ContentMd5:
                    return ContentMd5((string)objObject);

                case HttpResponseHeader.ContentRange:
                    return ContentRange((string)objObject);

                case HttpResponseHeader.ContentType:
                    return ContentType((string)objObject);

                case HttpResponseHeader.Date:
                    return Date((DateTime)objObject);

                case HttpResponseHeader.ETag:
                    return ETag((string)objObject);

                case HttpResponseHeader.Expires:
                    return Expires((DateTime)objObject);

                case HttpResponseHeader.KeepAlive:
                    return KeepAlive((string)objObject);

                case HttpResponseHeader.LastModified:
                    return LastModified((DateTime)objObject);

                case HttpResponseHeader.Location:
                    return Location((string)objObject);

                case HttpResponseHeader.Pragma:
                    return Pragma((string)objObject);

                case HttpResponseHeader.ProxyAuthenticate:
                    return ProxyAuthenticate((string)objObject);

                case HttpResponseHeader.RetryAfter:
                    return RetryAfter((string)objObject);

                case HttpResponseHeader.Server:
                    return Server((string)objObject);

                case HttpResponseHeader.SetCookie:
                    return SetCookie((string)objObject);

                case HttpResponseHeader.Trailer:
                    return Trailer((string)objObject);

                case HttpResponseHeader.TransferEncoding:
                    return TransferEncoding((string)objObject);

                case HttpResponseHeader.Upgrade:
                    return Upgrade((string)objObject);

                case HttpResponseHeader.Vary:
                    return Vary((string)objObject);

                case HttpResponseHeader.Via:
                    return Via((string)objObject);

                case HttpResponseHeader.Warning:
                    return Warining((string)objObject);

                case HttpResponseHeader.WwwAuthenticate:
                    return WwwAuthenticate((string)objObject);

                default:
                    throw new ArgumentOutOfRangeException("httpResponseHeader");
            }
        }
        static public KeyValuePair<string, string> Make(string strKey, string strValue) => new KeyValuePair<string, string>(strKey, strValue);

        /*
         * RFC https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
        */

        /// <summary>
        /// RFC 2616, секция 14.5
        /// Указание на то, поддерживает ли сервер отдачу контента частями.
        /// </summary>
        /// <param name="boolAccept">true = отдача по частям поддерживается</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> AcceptRanges(bool boolAccept = false) => Make("Accept-Ranges", boolAccept ? "bytes" : "none");

        /// <summary>
        /// RFC 2616, секция 14.6
        /// Для промежуточных proxy-серверов, возраст передаваемого контента в секундах  
        /// </summary>
        /// <param name="intAge"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Age(int intAge) => Make("Age", intAge.ToString());

        /// <summary>
        /// RFC 2616, секция 14.7
        /// Допустимые методы обращения к серверу 
        /// </summary>
        /// <param name="strMethods">Например, 'GET, POST'</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Allow(string strMethods) => Make("Allow", strMethods);

        /// <summary>
        /// RFC 2616, секция 14.9 
        /// Описывает поведение кеша для текущего контента.
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Cache-Control
        /// </summary>
        /// <param name="strCacheControl"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> CacheControl(string strCacheControl = null) => Make("Cache-Control", strCacheControl ?? "no-cache, no-store, must-revalidate");

        /// <summary>
        /// RFC 2616, секция 14.10
        /// Указвает на то, закрываеть ли соеденение после обработки текущего контента.
        /// </summary>
        /// <param name="boolKeep">Если = true, соединение не закрывается (keep-alive)</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Connection(bool boolKeep = false) => Make("Connection", boolKeep ? "Keep-Alive" : "close");

        /// <summary>
        /// RFC 2616, секция 14.11
        /// Указывает на то если к контенту были применены методы обработки, например упаковка. Так-же описывает порядок их применения
        /// https://developer.mozilla.org/ru/docs/Web/HTTP/Headers/Content-Encoding
        /// </summary>
        /// <param name="strEngcodingMethods">Методы обработки, например 'gzip'</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ContentEncoding(string strEngcodingMethods) => Make("Content-Encoding", strEngcodingMethods ?? "identity");

        /// <summary>
        /// RFC 2616, секция 14.12
        /// Указывает языки, для которых сервер может отдавать контент
        /// </summary>
        /// <param name="strLanguagesList">Например 'en, ru, de', плюс есть дополнения связанные с "весом" языков</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ContentLanguage(string strLanguagesList) => Make("Content-Language", strLanguagesList ?? "en");

        /// <summary>
        /// RFC 2616, секция 14.13
        /// Указывает размер (длину) конетента
        /// </summary>
        /// <param name="longContentLength">Длина в байтах</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ContentLength(ulong longContentLength) => Make("Content-Length", longContentLength.ToString());

        /// <summary>
        /// RFC 2616, секция 14.14
        /// Указание местарасположения контента, лучше почитать примеры у мозиллы
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Location
        /// </summary>
        /// <param name="strLocationUrl"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ContentLocation(string strLocationUrl) => Make("Content-Location", strLocationUrl);

        /// <summary>
        /// RFC 2616, секция 14.15
        /// Указание контрольной суммы контента
        /// </summary>
        /// <param name="strHash"></param>
        /// <returns></returns>;
        public static KeyValuePair<string, string> ContentMd5(string strHash) => Make("Content-Md5", strHash);

        /// <summary>
        /// RFC 2616, секция 14.16
        /// Укзание размера и местарасположения передаваемой части контента
        /// </summary>
        /// <param name="strRangeDefinition">'bytes x-y/z' где x=offset от начала контента, указывающий начало передаваемой части данных, y=offset от начала контента, указывающий конец передаваемой части данных, z=полный размер контента. Внимание, в случае нарезки частями,дя каждой из них поле Content-Length это результат вычисления y-x а не полный размер!</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ContentRange(string strRangeDefinition) => Make("Content-Range", strRangeDefinition);

        /// <summary>
        /// RFC 2616, секция 14.17
        /// Указание на тип передаваемого контента
        /// </summary>
        /// <param name="strContentType">Если это бинарные данные, то MIME-тип, например 'image/jpeg'. Если строковые то можно добавить кодировку, например 'text/html; charset=ISO-8859-4'</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ContentType(string strContentType = null) => Make("Content-Type", strContentType ?? "application/octet-stream");

        /// <summary>
        /// RFC 2616, секция 14.18
        /// Указание даты отправки контента (не его создания, если это файл)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Date(DateTime dateTime) => Make("Date", dateTime.ToUniversalTime().ToString("r"));

        /// <summary>
        /// RFC 2116, секция 14.19
        /// </summary>
        /// <param name="strETag">ETag считается по разному, это на совести сервера. Например, CRC32</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ETag(string strETag) => Make("ETag", strETag);

        /// <summary>
        /// RFC 2116, секция 14.21
        /// Указывает время истечения актуальности контента
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Expires(DateTime dateTime) => Make("Expires", dateTime.ToUniversalTime().ToString("r"));

        /// <summary>
        /// НЕ СТАНДАРТНЫЙ
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Keep-Alive
        /// Указание времени поддержания открытого соединения и числа запросов которые могут быть переданы до закрытия соединения.
        /// </summary>
        /// <param name="strKeepAliveAttributes">Строка вида 'timeout=5, max=1000'</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> KeepAlive(string strKeepAliveAttributes = null) => Make("Keep-Alive", strKeepAliveAttributes ?? "timeout=5, max=1000");

        /// <summary>
        /// RFC 2116, секция 14.29
        /// Указание даты модификаци контента
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> LastModified(DateTime dateTime) => Make("Last-Modified", dateTime.ToUniversalTime().ToString("r"));

        /// <summary>
        /// RFC 2116, секция 14.30
        /// Указание аболютнного URL нового расположения запрошеного контента. В общем-то редирект
        /// </summary>
        /// <param name="strLocation"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Location(string strLocation) => Make("Location", strLocation);

        /// <summary>
        /// RFC 2116, секция 14.32
        /// Устаревший. Указание необязательных директив для устаревшего протокола HTTP/1.0 например no-cache
        /// </summary>
        /// <param name="strPragmaAttributes"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Pragma(string strPragmaAttributes) => Make("Pragma", strPragmaAttributes);

        /// <summary>
        /// RFC 2116, секция 14.33
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Proxy-Authenticate
        /// Определяет требования авторизации на прокси-сервере
        /// </summary>
        /// <param name="strProxyAuthenticateAttributes">Строка вида 'Basic realm="Access to the internal site"'</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> ProxyAuthenticate(string strProxyAuthenticateAttributes) => Make("Proxy-Authenticate", strProxyAuthenticateAttributes);

        /// <summary>
        /// RFC 2116, секция 14.33
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Retry-After
        /// Указывает число секунд, или конкретное время после которых можно повторить отвергнутый с кодами 503, 429, 301
        /// </summary>
        /// <param name="strRetryValue">GMT-время или секунды</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> RetryAfter(string strRetryValue) => Make("Retry-After", strRetryValue);

        /// <summary>
        /// RFC 2116, секция 14.38
        /// Указание текстовых данных, описывающих сервер, его версию и платформу его выполнения. В принципе, чтобы запутать кулхацкеров, можно лепить всё что угодно.
        /// </summary>
        /// <param name="strServerDescriptionString"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Server(string strServerDescriptionString = null) => Make("Server", strServerDescriptionString ?? Main.strLibraryDesription + " (" + Utils.Environment.GetOSName() + ")");

        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie
        /// Установка данных Cookie
        /// </summary>
        /// <param name="strCookieData"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> SetCookie(string strCookieData) => Make("Set-Cookie", strCookieData);

        /// <summary>
        /// RFC 2116, секция 14.40
        /// Зачем это я пока не понял.
        /// </summary>
        /// <param name="strTrailerAttributes"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Trailer(string strTrailerAttributes) => Make("Trailer", strTrailerAttributes);

        /// <summary>
        /// RFC 2116, секция 14.41
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Transfer-Encoding
        /// </summary>
        /// <param name="strTransferEncodigAttributes"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> TransferEncoding(string strTransferEncodigAttributes) => Make("Transfer-Encoding", strTransferEncodigAttributes);

        /// <summary>
        /// RFC 2116, секция 14.41
        /// Указывает что клиенту следет переключиться на использование указнного или указанных протоколов, не совместимых с HTTP/1.0
        /// </summary>
        /// <param name="strUpgradeProtocolList">Например 'HTTP/2.0, websocket'</param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Upgrade(string strUpgradeProtocolList) => Make("Upgrade", strUpgradeProtocolList);

        /// <summary>
        /// RFC 2116, секция 14.44
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Vary
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<string, string> Vary(string strVaryHeaderList) => Make("Vary", strVaryHeaderList);

        /// <summary>
        /// RFC 2116, секция 14.45
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Via
        /// </summary>
        /// <param name="strViaDescription"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Via(string strViaDescription) => Make("Via", strViaDescription);

        /// <summary>
        /// RFC 2116, секция 14.46
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Warning
        /// </summary>
        /// <param name="strWaringAttributes"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> Warining(string strWaringAttributes) => Make("Warining", strWaringAttributes);

        /// <summary>
        /// RFC 2116, секция 14.47
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/WWW-Authenticate
        /// </summary>
        /// <param name="strWwwAuthenticateAttributes"></param>
        /// <returns></returns>
        public static KeyValuePair<string, string> WwwAuthenticate(string strWwwAuthenticateAttributes) => Make("WWW-Authenticate", strWwwAuthenticateAttributes);
    }
}
