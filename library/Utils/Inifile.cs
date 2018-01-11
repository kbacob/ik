// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
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
            if (Strings.Exists(strPathToIniFile))
            {
                if (File.Exists(strPathToIniFile))
                {
                    StreamReader objStream = new StreamReader(strPathToIniFile);

                    if (objStream != null)
                    {
                        Variables objIniFile = new Variables();
                        string strCurrentSectionName = strDefaultSectionName;

                        objIniFile.Add(strCurrentSectionName);

                        while (!objStream.EndOfStream)
                        {
                            string strTmp = objStream.ReadLine().Trim(' ', '\t');

                            if (Strings.Exists(strTmp))
                            {
                                if (strTmp[0] != '#')
                                {
                                    string strSectionPattern = @"^\[\s*([\w+\s*]+)\s*\]";

                                    if (Regex.IsMatch(strTmp, strSectionPattern))
                                    {
                                        strCurrentSectionName = Regex.Match(strTmp, strSectionPattern).Groups[1].Value;
                                    }
                                    else
                                    {
                                        string strParamPattern = @"(\w+)\s*=\s*\""(.*)?\""\s*\#*.?";

                                        if (Regex.IsMatch(strTmp, strParamPattern))
                                        {
                                            Match matchTmp = Regex.Match(strTmp, strParamPattern);

                                            objIniFile.Add(strCurrentSectionName, matchTmp.Groups[1].Value, matchTmp.Groups[2].Value);
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
                if (Strings.Exists(strFileName))
                {
                    StreamWriter objStream = new StreamWriter(strFileName, false);

                    if (objStream != null)
                    {
                        if (strHeader != null)
                        {
                            if (strHeader.Length > 0)
                            {
                                foreach (string strTmp in strHeader)
                                {
                                    objStream.WriteLine("# {0}", strTmp);
                                }
                            }
                        }

                        if (objSettings.Count > 0)
                        {


                            for (int intSectionIndex = 0; intSectionIndex < objSettings.Keys.Count; intSectionIndex++)
                            {
                                string strSectionName = objSettings.SectionNameByIndex(intSectionIndex);

                                objStream.WriteLine("[{0}]", strSectionName);

                                for (int intParam = 0; intParam < objSettings[strSectionName].Count; intParam++)
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
