// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.IO;
    using System.Text;
    using ik.Utils;
    using ik.Net.HTTP;

    /// <summary>
    /// Подготовка и отправка заголовков и тела ответа сервера
    /// </summary>
    public sealed class Response : Header
    {
        private string strSourceFile = null;
        private string strResponseHeader = null;
        private string strResponseData = null;
        private Settings Settings = null;

        private void MakeDefaultSettings()
        {
            Settings = new Settings
            {
                { "Main", "1", "2" }
            };

        }

        private void AddDefaultHeaderItems()
        {
            Add(HeaderItem.Type.Date, DateTime.UtcNow.ToString("r"));
            Add(HeaderItem.Type.Allow);
            Add(HeaderItem.Type.CacheControl);
            Add(HeaderItem.Type.Connection);
            Add(HeaderItem.Type.Pragma, "no-cache");
            Add(HeaderItem.Type.Server, Main.strLibraryDesription + " (" + Utils.Environment.GetOSName() + ")");
        }
        private void MakeHeaderString()
        {
            strResponseHeader += ToString();
        }
        private void MakeResponseHeader(HttpStatusCode httpStatusCode)
        {
           strResponseHeader = "HTTP/1.1 " + (int)httpStatusCode + " " + httpStatusCode.ToString();
        }
        private void MakeSimpleResponse(string strResponce, HttpStatusCode httpStatusCode)
        {
            if (strResponce == null) strResponce = "";

            Add(HeaderItem.Type.ContentType, ContentTypes.GetContentType("html"));
            Add(HeaderItem.Type.ContentLength, strResponce.Length.ToString());
            Add(HeaderItem.Type.ETag, Strings.CalcMD5(ref strResponce));
            AddDefaultHeaderItems();
            MakeResponseHeader(httpStatusCode);
            strResponseData = strResponce;
            MakeHeaderString();
        }

        /// <summary>
        /// Ответ на основе строки данных и кода HTTP-статуса
        /// </summary>
        /// <param name="strResponce">Тело ответа (HTML-документ, или иные текстовые данные, JSON к примеру</param>
        /// <param name="httpStatusCode">Код ответа (200, 404, 500, etc)</param>
        public Response(string strResponce, HttpStatusCode httpStatusCode) 
        {
            MakeDefaultSettings();
            if (strResponce != null) MakeSimpleResponse(strResponce, httpStatusCode);
            else MakeSimpleResponse("", httpStatusCode);
        }
        public Response(string[] strResponse, HttpStatusCode httpStatusCode) 
        {
            MakeDefaultSettings();
            if (!Strings.IsNullOrEmpty(strResponse))
            {
                var stringBuilder = new StringBuilder("");

                foreach (var Item in strResponse)
                {
                    stringBuilder.Append(Item);
                }
                MakeSimpleResponse(stringBuilder.ToString(), httpStatusCode);
            }
            else MakeSimpleResponse("", httpStatusCode);
        }
        /// <summary>
        /// Ответ на основе бинарного файла. Код ответа и типа контекста берётся на основе доступности и типа файла
        /// </summary>
        /// <param name="strPath">Путь к файлу</param>
        public Response(string strPath) 
        {
            MakeDefaultSettings();
            if (String.IsNullOrEmpty(strPath)) throw new ArgumentNullException();

            if (Files.Exists(strPath))
            {
                var FileInfo = new FileInfo(strPath);

                Add(HeaderItem.Type.ContentType, ContentTypes.GetContentType(Files.GetOnlyExtension(strPath)));
                Add(HeaderItem.Type.ContentLength, FileInfo.Length.ToString());
                Add(HeaderItem.Type.LastModified, FileInfo.LastWriteTime.ToUniversalTime().ToString("s"));
                Add(HeaderItem.Type.ETag, Files.CalcMD5fast(strPath));
                AddDefaultHeaderItems();

                MakeResponseHeader(HttpStatusCode.OK);
                strSourceFile = strPath;
                MakeHeaderString();
            }
            else
            {
                MakeSimpleResponse("404", HttpStatusCode.NotFound);
            }
        }

        ~Response()
        {
            Settings.Clear();
        }

        /// <summary>
        /// Отправить готовые заголовок и тело ответа
        /// </summary>
        /// <param name="objStream">Поток взаимодействия с клиентом</param>
        internal void Send(NetworkStream objStream) 
        {
            if (objStream != null)
            {
                objStream.Write(Encoding.ASCII.GetBytes(strResponseHeader), 0, strResponseHeader.Length);

                if (!String.IsNullOrEmpty(strResponseData))
                {
                    try
                    {
                        objStream.Write(Encoding.ASCII.GetBytes(strResponseData), 0, strResponseData.Length);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        //throw;
                    }
                    
                }
                else if (!String.IsNullOrEmpty(strSourceFile))
                {
                    var FileStream = File.Open(strSourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                    if (FileStream != null)
                    {
                        try
                        {
                            FileStream.CopyTo(objStream);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        
                        FileStream.Close();
                    }
                }
            }
            else throw new ArgumentNullException();
        }
    }
}