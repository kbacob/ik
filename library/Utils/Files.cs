// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

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
            if (String.IsNullOrEmpty(strName)) throw new ArgumentNullException();

            if (strName.Contains(@"\") || strName.Contains(@"/"))
            {
                var mt = Regex.Match(strName, @".*[\\\/](.*)\Z");

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
            if (String.IsNullOrEmpty(strName)) throw new ArgumentNullException();

            if (strName.Contains(@"\") || strName.Contains(@"/"))
            {
                var mt = Regex.Match(strName, @"(.*)?[\\\/].*\Z");

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
            if (String.IsNullOrEmpty(strName)) throw new ArgumentNullException();

            var strTmp = GetOnlyName(strName);

            if (strTmp.Contains("."))
            {
                var mt = Regex.Match(strTmp, @".*\.(.+)?\Z");

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
            if (String.IsNullOrEmpty(strName)) throw new ArgumentNullException();

            return File.Exists(FixPathByOS(strName));
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
            if (String.IsNullOrEmpty(strFileName) || Strings.IsNullOrEmpty(strFileMasksList)) throw new ArgumentNullException();

            foreach (string strMask in strFileMasksList)
            {
                if (Strings.ContainsByWildmask(strFileName, strMask)) return true;
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
            if(String.IsNullOrEmpty(strPathForFix)) throw new ArgumentNullException();

            if (strPathForFix.Contains(@"\") || strPathForFix.Contains(@"/"))
            {
                var OS = Environment.GetOS(); 

                if (OS == OSPlatform.Windows) return strPathForFix.Replace(@"/", @"\");
                if (OS == OSPlatform.Linux || OS == OSPlatform.OSX) return strPathForFix.Replace(@"\", @"/");
            }
            return strPathForFix;
        }

        /// <summary>
        /// Посчитать MD5-Hash файла и вернуть его в виде строки
        /// </summary>
        /// <param name="strFileName">Файл для расчёта</param>
        /// <returns></returns>
        static public string CalcMD5(string strFileName)
        {
            string strResult = null;

            if (String.IsNullOrEmpty(strFileName)) throw new ArgumentNullException();
            if (Exists(strFileName))
            {
                var objStream = File.Open(strFileName, FileMode.Open, FileAccess.Read);

                if(objStream != null && objStream.CanRead)
                {
                    var md5 = MD5.Create();
                    var hash = md5.ComputeHash(objStream);

                    objStream.Close();
                    strResult = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    md5.Clear();
                }
            }
            return strResult;
        }
        
        /// <summary>
        /// Расчитать MD5-Hash только атрибутов файла, в качестве быстрой альтернативы полному чтению всего файла.
        /// Рачёт будет сделан из строки "ИМЯ_ФАЙЛА_БЕЗ_ПУТИ/РАЗМЕР/ДАТА_СОЗДАНИЯ/ДАТА_ИЗМЕНЕНИЯ"
        /// </summary>
        /// <param name="strFileName">Файл для расчёта</param>
        /// <returns></returns>
        static public string CalcMD5fast(string strFileName)
        {
            if (String.IsNullOrEmpty(strFileName)) throw new ArgumentNullException();

            var strTimeFormat = "yyyyMMddHHmmssffff";
            var fileInfo = new FileInfo(strFileName);
            var strETagSource = GetOnlyName(strFileName) + "/" + fileInfo.Length.ToString() + "/" + fileInfo.CreationTimeUtc.ToString(strTimeFormat) + "/" + fileInfo.LastWriteTimeUtc.ToString(strTimeFormat);

            return Strings.CalcMD5(ref strETagSource);
        }
    }
}
