// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net
{
    using System;
    using System.IO;
    using System.Net;
    using ik.Utils;

    public class HTTPClient
    {
        public bool DownloadFile(string strUrl, string strStorePath)
        {
            if(Strings.Exists(strUrl) && Strings.Exists(strStorePath))
            {
                WebClient webClient = new WebClient();

                strStorePath = Files.FixPathByOS(strStorePath);
                
                if(Files.Exists(strStorePath))
                {
                    File.Delete(strStorePath);
                }
                
                webClient.DownloadFile(strUrl, strStorePath);

                return Files.Exists(strStorePath);
            }
            throw new ArgumentNullException();
        }
    }
}