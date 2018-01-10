// Copyright В© 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System.Net;

    public sealed class Response
    {
        public class ResponseHeader
        {
            public byte[] Value;

            public ResponseHeader(string strContentType, int intContentLength, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
            {
                string strTmp;

                strTmp = "HTTP/1.1 " + (int)httpStatusCode + " " + httpStatusCode.ToString() + "\n";
                strTmp += "Content-type: " + strContentType + "\nContent-Length: " + intContentLength.ToString() + "\n\n";
                Value = System.Text.Encoding.ASCII.GetBytes(strTmp);
            }
        }
        public byte[] Value;

        public Response(string strResponce = "", HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            ResponseHeader responseHeader = new ResponseHeader(@"text/html", strResponce.Length, httpStatusCode);
            byte[] tmpBody = System.Text.Encoding.ASCII.GetBytes(strResponce);

            Value = new byte[responseHeader.Value.Length + tmpBody.Length];
            for (int intTmp = 0; intTmp < responseHeader.Value.Length; intTmp++) Value[intTmp] = responseHeader.Value[intTmp];
            for (int intTmp = 0; intTmp < tmpBody.Length; intTmp++) Value[responseHeader.Value.Length + intTmp] = tmpBody[intTmp];
        }
    }
}