// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.HTTP
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.ComponentModel;
    using ik.Utils;
    using ik.Types;

    public static class HeaderItem
    {
        public enum Type
        {
            [Description("Accept")]
            [DefaultValue("*")]
            Accept = 1,

            [Description("Accept-Charset")]
            [DefaultValue("*")]
            AcceptCharset,

            [Description("Accept-Encoding")]
            [DefaultValue("*")]
            AcceptEncoding,

            [Description("Accept-Language")]
            [DefaultValue("*")]
            AcceptLanguage,

            [Description("Accept-Ranges")]
            [DefaultValue("bytes")]
            AcceptRanges,

            [Description("Age")]
            [DefaultValue(null)]
            Age,

            [Description("Allow")]
            [DefaultValue("GET, PUT")]
            Allow,

            [Description("Authorization")]
            [DefaultValue(null)]
            Authorization,

            [Description("Cache-Control")]
            [DefaultValue("no-cache, no-store, must-revalidate")]
            CacheControl,

            [Description("Connection")]
            [DefaultValue("keep-alive")]
            Connection,

            [Description("Content-Encoding")]
            [DefaultValue(null)]
            ContentEncoding,

            [Description("Content-Language")]
            [DefaultValue(null)]
            ContentLanguage,

            [Description("Content-Length")]
            [DefaultValue(null)]
            ContentLength,

            [Description("Content-Location")]
            [DefaultValue(null)]
            ContentLocation,

            [Description("Content-MD5")]
            [DefaultValue(null)]
            ContentMd5,

            [Description("Content-Range")]
            [DefaultValue(null)]
            ContentRange,

            [Description("Content-Type")]
            [DefaultValue("application/octet-stream")]
            ContentType,

            [Description("Cookie")]
            [DefaultValue(null)]
            Cookie,

            [Description("Date")]
            [DefaultValue(null)]
            Date,

            [Description("ETag")]
            [DefaultValue(null)]
            ETag,

            [Description("Expect")]
            [DefaultValue(null)]
            Expect,

            [Description("Expires")]
            [DefaultValue(null)]
            Expires,

            [Description("From")]
            [DefaultValue(null)]
            From,

            [Description("Host")]
            [DefaultValue(null)]
            Host,

            [Description("If-Match")]
            [DefaultValue(null)]
            IfMatch,

            [Description("If-Modified-Since")]
            [DefaultValue(null)]
            IfModifiedSince,

            [Description("If-None-Match")]
            [DefaultValue(null)]
            IfNoneMatch,

            [Description("If-Range")]
            [DefaultValue(null)]
            IfRange,

            [Description("If-Unmodified-Since")]
            [DefaultValue(null)]
            IfUnmodifiedSince,

            [Description("Keep-Alive")]
            [DefaultValue("timeout=15, max=1000")]
            KeepAlive,

            [Description("Last-Modified")]
            [DefaultValue(null)]
            LastModified,

            [Description("Location")]
            [DefaultValue(null)]
            Location,

            [Description("Max-Forwards")]
            [DefaultValue(null)]
            MaxForwards,

            [Description("Pragma")]
            [DefaultValue(null)]
            Pragma,

            [Description("Proxy-Authenticate")]
            [DefaultValue(null)]
            ProxyAuthenticate,

            [Description("Proxy-Authorization")]
            [DefaultValue(null)]
            ProxyAuthorization,

            [Description("Range")]
            [DefaultValue(null)]
            Range,

            [Description("Referer")]
            [DefaultValue(null)]
            Referer,

            [Description("Retry-After")]
            [DefaultValue(null)]
            RetryAfter,

            [Description("Server")]
            [DefaultValue(null)]
            Server,

            [Description("Set-Cookie")]
            [DefaultValue(null)]
            SetCookie,

            [Description("Te")]
            [DefaultValue(null)]
            Te,

            [Description("Trailer")]
            [DefaultValue(null)]
            Trailer,

            [Description("Transfer-Encoding")]
            [DefaultValue(null)]
            TransferEncoding,

            [Description("Translate")]
            [DefaultValue(null)]
            Translate,

            [Description("Upgrade")]
            [DefaultValue(null)]
            Upgrade,

            [Description("User-Agent")]
            [DefaultValue(null)]
            UserAgent,

            [Description("Vary")]
            [DefaultValue(null)]
            Vary,

            [Description("Via")]
            [DefaultValue(null)]
            Via,

            [Description("Warning")]
            [DefaultValue(null)]
            Warning,

            [Description("WWW-Authenticate")]
            [DefaultValue(null)]
            WwwAuthenticate,

            Unknown = int.MaxValue
        }

        public static KeyValuePair<string, string> Make(Type enumKey, string strValue) => Make(GetRfcString(enumKey), strValue);
        public static KeyValuePair<string, string> Make(string strKey, string strValue) => new KeyValuePair<string, string>(strKey, strValue);
        public static string GetRfcString(Type httpHeaderItemType) => Strings.GetDescription(httpHeaderItemType);
        public static Type GetTypeEnum(string strRefString)
        {
            if (string.IsNullOrEmpty(strRefString)) throw new ArgumentNullException("strRrcString");

            foreach (Type enumItem in Enum.GetValues(typeof(Type)))
            {
                if (String.Equals(strRefString, GetRfcString(enumItem))) return enumItem;
            }
            return Type.Unknown;
        }
    }

    public class Header : KeyValueList
    {
        public Parsers.Query Query = null;

        public Header()
        {

            Clear();
            Query = null;
        }
        ~Header()
        {
            if (Query != null)
            {
                Query.Clear();
                Query = null;
            }
            Clear();
        }

        public new void Add(string strKey, string strValue) => Add(HeaderItem.Make(strKey, strValue));
        public void Add(HeaderItem.Type enumKey, string strValue) => Add(HeaderItem.Make(enumKey, strValue));
        public void Add(HeaderItem.Type enumKey)
        {
            var strDefaultValue = Strings.GetDefaultValue(enumKey);

            if (!String.IsNullOrEmpty(strDefaultValue)) Add(enumKey, strDefaultValue);
        }

        public new string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("");

            if (Count > 0)
            {
                foreach (var Item in this)
                {
                    stringBuilder.Append("\n" + Item.Key + ": " + Item.Value);
                }
            }
            stringBuilder.Append("\n\n");
            return stringBuilder.ToString();
        }
    }
}
