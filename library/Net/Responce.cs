// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System.Net;

    public sealed class Response
    {
        private string strHeader;
        private string strData;

        public byte[] Value;

        private string ResponseHeader(string strContentType, int intContentLength, HttpStatusCode httpStatusCode) => "HTTP/1.1 " + (int)httpStatusCode + " " + httpStatusCode.ToString() + "\nContent - type: " + strContentType + "\nContent - Length: " + intContentLength.ToString() + "\n\n";

        public Response(string strResponce, HttpStatusCode httpStatusCode)
        {
            Value = System.Text.Encoding.ASCII.GetBytes(ResponseHeader("text/html", strResponce.Length, httpStatusCode) + strResponce);
        }
    }
}