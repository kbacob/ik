// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    
    /// <summary>
    /// Реализация определения Content Type / MIME Type
    /// Данные о типах берутся из внешнего файла
    /// </summary>
    public static class ContentTypes
    {
        private static Dictionary<string, List<string>> DTypes = null; // "content/type", {"extension1", "extension2", "etc"}
        public static string strMimeTypesListUrl = @"https://svn.apache.org/repos/asf/httpd/httpd/trunk/docs/conf/mime.types";

        /// <summary>
        ///  Инициализирует систему работы с опрелениями типов контента
        /// </summary>
        /// <param name="strPathToMimeTypesFile">Путь к файлу с определениями в формате Apache. Если NULL или не указано, то определения во время инициализации подгружаться не будут</param>
        public static void Init(bool boolAlwaysLoadMimeTypesFile = false)
        {
            if (DTypes == null)
            {
                var strMimeTypesFilePath = Files.FixPathByOS(Utils.Environment.GetDirectory(Utils.Environment.HostDirectory.Tmp) + @"\" + Files.GetOnlyName(strMimeTypesListUrl));
                var hc = new Net.HTTP.Client();
                
                DTypes = new Dictionary<string, List<string>>();
                
                if(!Files.Exists(strMimeTypesFilePath) || boolAlwaysLoadMimeTypesFile )
                {
                    hc.DownloadFile(strMimeTypesListUrl, strMimeTypesFilePath); 
                }
            
                Load(strMimeTypesFilePath);
            }
        }
        /// <summary>
        /// Очистить подгруженные из файла данные и все переменные. В принципе, вызывается автоматически.
        /// </summary>
        public static void Exit()
        {
            Clear();
            DTypes = null;
        }
        /// <summary>
        /// Очистить подгруженные из файла данные. В принципе, вызывается автоматически.
        /// </summary>
        public static void Clear()
        {
            if (DTypes != null)
            {
                DTypes.Clear();
            }
        }
        /// <summary>
        ///  Загразить данные о типах из файла. 
        ///  Можно с очистить ранее подгруженных, можно с добавлением к ним.
        /// </summary>
        /// <param name="strPathToMimeTypesFile">Путь к файлу с определениями в формате Apache</param>
        /// <param name="boolAppendData">Если TRUE, уже существующие определения типов удаляться не будут. При этом, ксли в новых данных будет описываться уже существующий MIME-тип, то его список расширений будет дополнен.</param>
        public static void Load(string strPathToMimeTypesFile, bool boolAppendData = false)
        {
            if (DTypes != null)
            {
                if (boolAppendData == false)
                {
                    DTypes.Clear();
                }
                    
                strPathToMimeTypesFile = Files.FixPathByOS(strPathToMimeTypesFile);
                if (Files.Exists(strPathToMimeTypesFile))
                {
                    var objStream = new StreamReader(Files.FixPathByOS(strPathToMimeTypesFile));

                    if (objStream != null)
                    {
                        while (!objStream.EndOfStream)
                        {
                            string strTmp = objStream.ReadLine().Trim(' ', '\t');

                            if (!String.IsNullOrEmpty(strTmp))
                            {
                                if (strTmp[0] != '#')
                                {
                                    var rx = new Regex(@"\s{2,}");
                                    var strList = rx.Replace(strTmp, " ").Split(null);

                                    if (strList.Length > 1)
                                    {
                                        var lExtensions = new List<string>();

                                        for (var intTmp = 1; intTmp < strList.Length; intTmp++)
                                        {
                                            lExtensions.Add(strList[intTmp]);
                                        }

                                        if (!DTypes.ContainsKey(strList[0]))
                                        {
                                            DTypes.Add(strList[0], lExtensions);
                                        }
                                        else
                                        {
                                            if(boolAppendData == true)
                                            {
                                                foreach (var strExtension in lExtensions)
                                                {
                                                    if (!DTypes[strList[0]].Contains(strExtension)) DTypes[strList[0]].Add(strExtension);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        objStream.Close();
                    }
                }
                else throw new FileNotFoundException(strPathToMimeTypesFile);
            }
        }

        /// <summary>
        /// Получить MIME-тип по расширению файла
        /// </summary>
        /// <param name="strExtension">Расширение</param>
        /// <returns>MIME-тип, или "application/octet-stream", если для указанного расширения не было найдено заранее описанного типа</returns>
        public static string GetContentType(string strExtension)
        {
            if (String.IsNullOrEmpty(strExtension)) throw new ArgumentNullException();

            if (DTypes != null && DTypes.Count > 0)
            {
                foreach (var entry in DTypes)
                {
                    if (entry.Value.Contains(strExtension)) return entry.Key;
                }
            }
            else throw new ObjectDisposedException("MIME types not initialised");

            return @"application/octet-stream";
        }
        /// <summary>
        /// Получить список возможных расширений файла по MIME-типу
        /// </summary>
        /// <param name="strContentType">MIME-тип</param>
        /// <returns>Список с возможными расширениями или NULL, если MIME-тип не описан</returns>
        public static List<string> GetExtensions(string strContentType)
        {
            if (String.IsNullOrEmpty(strContentType)) throw new ArgumentNullException();

            if (DTypes != null  && DTypes.Count > 0)
            {
                if (DTypes.ContainsKey(strContentType)) return DTypes[strContentType];
            }
            else throw new ObjectDisposedException("MIME types not initialised");

            return null;
        }
    }
}
