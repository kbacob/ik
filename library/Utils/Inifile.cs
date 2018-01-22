// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using ik.Types;
    
    /// <summary>
    /// Реализация чтения и записи примитивных ini-файлов где каждая строка это 
    /// <para>либо # комментарий</para>
    /// <para>либо [секция]</para>
    /// <para>либо название параметра = "его значение"</para>
    /// </summary>
    public static class IniFile
    {
        /// <summary>
        /// Читает и анализирует указанный ini-файл, возвращая разделённый на секции список переменных.
        /// </summary>
        /// <param name="strPathToIniFile">Путь к ini-файлу</param>
        /// <param name="strDefaultSectionName">Если в ini-файле не будет ни одного названия секции, все полученные переменные будут отнесены к здесь указанному</param>
        /// <returns>Разделённый на секции список переменных</returns>
        public static Variables Read(string strPathToIniFile, string strDefaultSectionName = "Main")
        {
            if (!String.IsNullOrEmpty(strPathToIniFile))
            {
                if (File.Exists(strPathToIniFile))
                {
                    var objStream = new StreamReader(strPathToIniFile);

                    if (objStream != null)
                    {
                        var objIniFile = new Variables();
                        var LDefines = new Dictionary<string, string>();
                        var strCurrentSectionName = strDefaultSectionName;

                        objIniFile.Add(strCurrentSectionName);

                        while (!objStream.EndOfStream)
                        {
                            var strTmp = objStream.ReadLine().Trim(' ', '\t');

                            if (!String.IsNullOrEmpty(strTmp))
                            {
                                if (strTmp[0] != '#')
                                {
                                    var strDefinePattern  = @"^\s*DEFINE\s+(?<Key>\w+)\s*=\s*\""(?<Value>.*)\""";  // DEFINE SomeDefineName = "something here" 
                                    var strIncludePattern = @"^\s*INCLUDE\s+\""(?<Value>.+)\""";                   // INCLUDE "path/to/some/ini/file" 
                                    var strSectionPattern = @"^\s*\[(?<SectionName>.+)\]";                         // [SectionName] 
                                    var strParamPattern   = @"^\s*(?<Key>\w+)\s*=\s*\""(?<Value>.*)\""";           // Param = "abcd" / or Param = "%SomeDefineName%" / or also Param = "%SomeDefineName% and little bit more or %OtherDefineName%"

                                    if (Regex.IsMatch(strTmp,strIncludePattern))
                                    {
                                        // TODO
                                    }
                                    else if (Regex.IsMatch(strTmp, strDefinePattern))
                                    {
                                        var matchTmp = Regex.Match(strTmp, strDefinePattern);
                                        var strDefineName = matchTmp.Groups["Key"].Value;
                                        var strDefineValue = matchTmp.Groups["Value"].Value;

                                        if (LDefines.ContainsKey(strDefineName))
                                        {
                                            LDefines[strDefineName] = strDefineValue;
                                        }
                                        else
                                        {
                                            LDefines.Add(strDefineName, strDefineValue);
                                        }
                                    }
                                    else if (Regex.IsMatch(strTmp, strSectionPattern))
                                    {
                                        strCurrentSectionName = Regex.Match(strTmp, strSectionPattern).Groups["SectionName"].Value;
                                    }
                                    else
                                    {
                                        if (Regex.IsMatch(strTmp, strParamPattern))
                                        {
                                            var matchTmp = Regex.Match(strTmp, strParamPattern);
                                            var strParamName = matchTmp.Groups["Key"].Value;
                                            var strParamValue = matchTmp.Groups["Value"].Value;

                                            if (Strings.ContainsByRegex(strParamValue, @"\%(.+)?\%")) // DEFINE Lookup
                                            {
                                                foreach (var Item in LDefines)
                                                {
                                                    while (Strings.ContainsByRegex(strParamValue, @"\%" + Item.Key + @"\%")) // DEFINE subst
                                                    {
                                                        strParamValue = Strings.ReplaceByRegex(strParamValue, @"\%" + Item.Key + @"\%", Item.Value);
                                                    }
                                                }
                                                if (Strings.ContainsByRegex(strParamValue, @"\%(.+)?\%"))
                                                {
                                                    // if we found %NOT DEFIENED% then we not touch it
#if DEBUG
                                                    // or
                                                    throw new KeyNotFoundException(strParamValue);
#endif
                                                }

                                            }

                                            objIniFile.Add(strCurrentSectionName, strParamName, strParamValue);
                                        }
                                    }
                                }
                            }
                        }
                        objStream.Close();
                        return objIniFile;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Сохраняет указанный список переменных в виде ini-файла.
        /// </summary>
        /// <param name="objSettings">Список переменных</param>
        /// <param name="strFileName">Имя ini-файла, если файл существует то будет перезаписан</param>
        /// <param name="strHeader">Список строк для заголовка-комментария если таковой нужен. Символ # в начала строк не нужен - он будет добавлен автоматически, по умолчанию заголовок не создаётся.</param>
        public static void Save(Variables objSettings, string strFileName, string[] strHeader = null)
        {
            if (objSettings != null)
            {
                if (!String.IsNullOrEmpty(strFileName))
                {
                    var objStream = new StreamWriter(strFileName, false);

                    if (objStream != null)
                    {
                        if (strHeader != null && strHeader.Length > 0)
                        {
                            foreach (string strTmp in strHeader)
                            {
                                 objStream.WriteLine("# {0}", strTmp);
                            }
                        }

                        if (objSettings.Count > 0)
                        {
                            for (var intSectionIndex = 0; intSectionIndex < objSettings.Keys.Count; intSectionIndex++)
                            {
                                var strSectionName = objSettings.SectionNameByIndex(intSectionIndex);

                                objStream.WriteLine("[{0}]", strSectionName);

                                for (var intParam = 0; intParam < objSettings[strSectionName].Count; intParam++)
                                {
                                    objStream.WriteLine("{0} = \"{1}\"", objSettings.ParamNameByIndex(intSectionIndex, intParam), objSettings.ValueByIndex(intSectionIndex, intParam));
                                }
                            }
                        }
                        objStream.Flush();
                        objStream.Close();
                    }
                }
            }
        }
    }
}
