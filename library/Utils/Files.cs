// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Runtime.InteropServices;

    /// <summary>
    ///  Вспомогательные методы для работы с файловой системой, именами и путями.
    /// </summary>
    public static class Files
    {
        /// <summary>
        ///  Выделяет имя файла с расширением из строки вида /путь/имя файла.расширение.
        ///  В отличие от библиотечной, использует Regex и не вызывает исключений если в обрабатываемой строке присутствуют "недопустимые" символы. 
        /// </summary>
        /// <param name="strName">Полное имя файла с путём</param>
        /// <returns>Имя файла с расширением</returns>
        static public string GetOnlyName(string strName)
        {
            if (strName.Contains(@"\") || strName.Contains(@"/"))
            {
                Match mt = Regex.Match(strName, @".*[\\\/](.*)\Z");

                if (mt.Groups.Count == 2) return mt.Groups[1].Value;
            }
            return strName;
        }

        /// <summary>
        /// Выделяет путь из строки вида /путь/имя файла.расширение.
        /// В отличие от библиотечной, использует Regex и не вызывает исключений если в обрабатываемой строке присутствуют "недопустимые" символы. 
        /// </summary>
        /// <param name="strName">Полное имя файла с путём</param>
        /// <returns>Путь к файлу</returns>
        static public string GetOnlyPath(string strName)
        {
            if (strName.Contains(@"\") || strName.Contains(@"/"))
            {
                Match mt = Regex.Match(strName, @"(.*)?[\\\/].*\Z");

                if (mt.Groups.Count == 2) return FixPathByOS(mt.Groups[1].Value);
            }
            return "";
        }

        /// <summary>
        /// Выделяет разсширение файла из строки вида /путь/имя файла.расширение.
        /// В отличие от библиотечной, использует Regex и не вызывает исключений если в обрабатываемой строке присутствуют "недопустимые" символы. 
        /// </summary>
        /// <param name="strName">Полное имя файла с путём или просто имя файла с расширением</param>
        /// <returns>Расширение файла</returns>
        static public string GetOnlyExtension(string strName)
        {
            string strTmp = GetOnlyName(strName);

            if (strTmp.Contains("."))
            {
                Match mt = Regex.Match(strTmp, @".*\.(.+)?\Z");

                if (mt.Groups.Count == 2) return mt.Groups[1].Value;
            }
            return "";
        }

        /// <summary>
        /// Проверяет, существует ли файл
        /// </summary>
        /// <param name="strName">Имя файла с путём</param>
        /// <returns>true, если указанный файл существует</returns>
        static public bool Exists(string strName)
        {
            if (Strings.Exists(strName))
            {
                return File.Exists(FixPathByOS(strName));
            }
            throw new ArgumentNullException();
        }

        /// <summary>
        /// Сравнение имени файла со списком имен и/или wildcard-масок
        /// </summary>
        /// <param name="strFileName">Имя файла для сравнения</param>
        /// <param name="strFileMasksList">Список имён и/мом wildcard-масок</param>
        /// <param name="boolCaseSentetivity">Учитывать регистр символов при сравнении</param>
        /// <returns></returns>
        static public bool EqualByMask(string strFileName, string[] strFileMasksList, bool boolCaseSentetivity = false)
        {
            if (Strings.Exists(strFileName))
            {
                if (strFileMasksList != null)
                {
                    foreach (string strMask in strFileMasksList)
                    {
                        if (Strings.Exists(strMask))
                        {
                            string strRegexPattern = @"^";

                            foreach (char chrSymbol in strMask)
                            {
                                switch (chrSymbol)
                                {
                                    case '?':
                                        strRegexPattern += @".";
                                        break;

                                    case '*':
                                        strRegexPattern += @".*?";
                                        break;

                                    case '.':
                                        strRegexPattern += @"\\.";
                                        break;

                                    default:
                                        strRegexPattern += chrSymbol;
                                        break;
                                }
                            }

                            strRegexPattern += @"\\Z";

                            Match mt = Regex.Match(strFileName, strRegexPattern);

                            if (mt != null)
                            {
                                if (mt.Success == true)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Коррекция пути к файлу, в зависимости от среды исполненения 
        /// </summary>
        /// <param name="strPathForFix"></param>
        /// <returns></returns>
        static public string FixPathByOS(string strPathForFix)
        {
            if(Strings.Exists(strPathForFix))
            {
                if (strPathForFix.Contains(@"\") || strPathForFix.Contains(@"/"))
                {
                    OSPlatform OS = Environment.GetOS();

                    if (OS == OSPlatform.Windows)
                    {
                        return strPathForFix.Replace(@"/", @"\");
                    }
                    else if(OS == OSPlatform.Linux || OS == OSPlatform.OSX)
                    {
                        return strPathForFix.Replace(@"\", @"/");
                    }
                }
                return strPathForFix;
            }
            throw new ArgumentNullException();
        }
    }
}
