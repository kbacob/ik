// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public static class Environment
    {
        public enum DirectoryClass
        {
            Tmp
        }
        
        /// <summary>
        /// Определяем, на чём нас запустили.
        /// </summary>
        /// <returns></returns>
        static public OSPlatform GetOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            }
            throw new Exception("Cannot detect OS Type");
        }

        /// <summary>
        /// Получить значение переменной окружения.
        /// </summary>
        /// <param name="strVariableName">Название переменной окружения, обрамлять в % не нужно</param>
        /// <returns>Значение или null</returns>
        static public string GetVariable(string strVariableName)
        {
            if(Strings.Exists(strVariableName))
            {
                return System.Environment.GetEnvironmentVariable(strVariableName);
            }
            throw new ArgumentNullException();
        }

        static public string GetDirectory(DirectoryClass DirectoryClass)
        {
            switch(DirectoryClass)
            {
                case DirectoryClass.Tmp:
                    if (GetOS() == OSPlatform.Windows) 
                    {
                        return GetVariable("TMP");
                    }
                    else if(GetOS() == OSPlatform.Linux)
                    {
                        return @"/var/tmp";
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
            return null;
        }

    }
}