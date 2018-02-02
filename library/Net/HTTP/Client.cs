// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Net.HTTP
{
    using System;
    using System.IO;
    using System.Net;
    using ik.Utils;

    public class Client
    {
        public bool DownloadFile(string strUrl, string strStorePath)
        {
            if (String.IsNullOrEmpty(strUrl) && String.IsNullOrEmpty(strStorePath)) throw new ArgumentNullException();

            var webClient = new WebClient();

            strStorePath = Files.FixPathByOS(strStorePath);
            if(Files.Exists(strStorePath)) File.Delete(strStorePath);
            webClient.DownloadFile(strUrl, strStorePath);

            return Files.Exists(strStorePath);
        }
    }
}