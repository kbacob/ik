// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Types
{
    using System;
    using System.Collections.Generic;

    public class KeyValueList : List<KeyValuePair<string, string>>
    {
        private int Index(string strKey, out bool boolHaveMore)
        {
            var intResult = -1;

            boolHaveMore = false;
            for (int intTmp = 0; intTmp < Count; intTmp++)
            {
                if (String.Equals(this[intTmp].Key, strKey))
                {
                    if (intResult == -1)
                    {
                        intResult = intTmp;
                    }
                    else
                    {
                        boolHaveMore = true;
                        break;
                    }
                }
            }

            return intResult;
        }

        public void Add(string strKey, string strValue) => Add(new KeyValuePair<string, string>(strKey, strValue));
        public string Get(string strKey, out bool boolHaveMore)
        {
            int intIndex = Index(strKey, out bool boolTmp);

            boolHaveMore = boolTmp;
            if (intIndex != -1) return this[intIndex].Value;

            return null;
        }
        public string Get(string strKey)
        {
            bool boolTmp = false;

            return Get(strKey, out boolTmp);
        }
        public void Set(string strKey, string strValue)
        {
            Remove(strKey, strValue);
            Add(strKey, strValue);
        }
        public void Remove(string strKey, string strValue)
        {
            int intIndex = Index(strKey, out bool boolTmp);

            if (intIndex != -1)
            {
                RemoveAt(intIndex);
            }
        }

        public string[] Keys(string strPrefix = "", string strPostfix = "")
        {
            string[] strResult = null;

            if (Count > 0)
            {
                strResult = new string[Count];

                for (var intTmp = 0; intTmp < Count; intTmp++)
                {
                    strResult[intTmp] = strPrefix + this[intTmp].Key + strPostfix;
                }
            }

            return strResult;
        }
    }
}
