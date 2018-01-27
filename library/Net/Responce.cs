// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.IO;
    using System.Text;
    using ik.Utils;
    using System.Collections.Generic;

    /// <summary>
    /// Подготовка и отправка заголовков и тела ответа сервера
    /// </summary>
    public sealed class Response : MemoryStream
    {
        private List<KeyValuePair<string, string>> LResponceHeaderItems;

        private string strSourceFile = null;
        private string strResponseHeader = null;
        private string strResponseData = null;
        
        private void AddDefaultHeaderItems()
        {
            LResponceHeaderItems.Add(HTTPHeader.Date(DateTime.Now));
            LResponceHeaderItems.Add(HTTPHeader.Allow("GET, POST"));
            LResponceHeaderItems.Add(HTTPHeader.CacheControl());
            LResponceHeaderItems.Add(HTTPHeader.Connection());
            LResponceHeaderItems.Add(HTTPHeader.Server(null));
        }
        private void MakeHeaderString()
        {
            StringBuilder stringBuilder = new StringBuilder(strResponseHeader);

            if(LResponceHeaderItems.Count > 0)
            {
                foreach(var Item in LResponceHeaderItems)
                {
                    stringBuilder.Append("\n" + Item.Key + ": " + Item.Value);
                }
            }
            stringBuilder.Append("\n\n");
            strResponseHeader = stringBuilder.ToString();
        }
        private void MakeResponseHeader(HttpStatusCode httpStatusCode)
        {
           strResponseHeader = "HTTP/1.1 " + (int)httpStatusCode + " " + httpStatusCode.ToString();
        }
        private void MakeSimpleResponse(string strResponce, HttpStatusCode httpStatusCode)
        {
            if (strResponce == null) strResponce = "";

            LResponceHeaderItems.Add(HTTPHeader.ContentType(ContentTypes.GetContentType("html")));
            LResponceHeaderItems.Add(HTTPHeader.ContentLength((ulong)strResponce.Length));
            if(httpStatusCode == HttpStatusCode.OK) LResponceHeaderItems.Add(HTTPHeader.ETag(Strings.CalcMD5(ref strResponce)));
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
            LResponceHeaderItems = new List<KeyValuePair<string, string>>();

            if(strResponce != null) MakeSimpleResponse(strResponce, httpStatusCode);
            else MakeSimpleResponse("", httpStatusCode);
        }

        /// <summary>
        /// Ответ на основе массива строк данных и кода HTTP-статуса
        /// </summary>
        /// <param name="strResponce">Массив строк с телом ответа (HTML-документ, или иные текстовые данные, JSON к примеру</param>
        /// <param name="httpStatusCode">Код ответа (200, 404, 500, etc)</param>
        public Response(string[] strResponse, HttpStatusCode httpStatusCode)
        {
            StringBuilder stringBuilder = new StringBuilder("");

            LResponceHeaderItems = new List<KeyValuePair<string, string>>();
            if (strResponse != null && strResponse.Length > 0)
            {
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
            if (String.IsNullOrEmpty(strPath)) throw new ArgumentNullException();
            LResponceHeaderItems = new List<KeyValuePair<string, string>>();

            if (Files.Exists(strPath))
            {
                var FileInfo = new FileInfo(strPath);

                LResponceHeaderItems.Add(HTTPHeader.ContentType(ContentTypes.GetContentType(Files.GetOnlyExtension(strPath))));
                LResponceHeaderItems.Add(HTTPHeader.ContentLength((ulong)FileInfo.Length));
                LResponceHeaderItems.Add(HTTPHeader.LastModified(FileInfo.LastWriteTime));
                LResponceHeaderItems.Add(HTTPHeader.ETag(Files.CalcMD5fast(strPath)));
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