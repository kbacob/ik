// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using ik.Utils;

    public sealed class Response 
    {
        private string strSourceFile = null;
        private string strHeader = null;

        private string ResponseHeader(string strContentType, long longContentLength, HttpStatusCode httpStatusCode, string strAdditionalTabs = null)
        {
            var strResult = "HTTP/1.1 " + (int)httpStatusCode + " " + httpStatusCode.ToString() + "\ncontent-type: " + strContentType + "\ncontent-length: " + longContentLength.ToString();

            if (!String.IsNullOrEmpty(strAdditionalTabs)) strResult += "\n" + strAdditionalTabs;
            return strResult + "\n\n";
        }
     
        public Response(string strResponce, HttpStatusCode httpStatusCode)
        {
            strHeader = ResponseHeader("text/html", strResponce.Length, httpStatusCode) + strResponce;
        }
        public Response(string strPath)
        {
            if (String.IsNullOrEmpty(strPath)) throw new ArgumentNullException();

            if (Files.Exists(strPath))
            {
                var FileInfo = new FileInfo(strPath);
                var strData = "date: " + FileInfo.CreationTime.ToUniversalTime().ToString("r");

                strHeader = ResponseHeader(ContentTypes.GetContentType(Files.GetOnlyExtension(strPath)), FileInfo.Length, HttpStatusCode.OK, strData);
                strSourceFile = strPath;
            }
            else
            {
                strHeader = ResponseHeader("text/html", 3, HttpStatusCode.NotFound) + "404";
            }
        }

        public void Send(NetworkStream objStream) // TODO: async
        {
            if (objStream != null)
            {
                objStream.Write(Encoding.ASCII.GetBytes(strHeader), 0, strHeader.Length);

                if (!String.IsNullOrEmpty(strSourceFile))
                {
                    var FileStream = File.Open(strSourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                    if (FileStream != null)
                    {
                        FileStream.CopyTo(objStream);
                        FileStream.Close();
                    }
                }
            }
            else throw new ArgumentNullException();
        }
    }
}