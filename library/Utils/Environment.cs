// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Решение задач в той, или иной мере, связананых со средой выполнения кода.
    /// </summary>
    public static class Environment
    {
        /// <summary>
        /// Системный каталог
        /// </summary>
        public enum HostDirectory
        {
            /// <summary>
            /// %USER%\AppData\Local\Temp (%TMP%) в Windows или /var/tmp в Linux
            /// </summary>
            Tmp = 1
        }
        
        /// <summary>
        /// Определяем, на чём нас запустили.
        /// </summary>
        /// <returns></returns>
        public static OSPlatform GetOS()
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
        /// Возвращает текстовое описание платформы запуска
        /// </summary>
        /// <returns></returns>
        public static string GetOSName()
        {
            if (GetOS() == OSPlatform.Linux) return "unix/linux";
            else if (GetOS() == OSPlatform.Windows) return "windows";
            else if (GetOS() == OSPlatform.OSX) return "osx";
            else return "unknown";
        }

        /// <summary>
        /// Получить значение переменной окружения.
        /// </summary>
        /// <param name="strVariableName">Название переменной окружения, обрамлять в % не нужно</param>
        /// <returns>Значение или null</returns>
        public static string GetVariable(string strVariableName)
        {
            if(String.IsNullOrEmpty(strVariableName)) throw new ArgumentNullException();
            
            return System.Environment.GetEnvironmentVariable(strVariableName);
        }

        /// <summary>
        /// Получить путь к системному каталогу. Так как разных средах исполнения они разные, наверное проще так.
        /// </summary>
        /// <param name="hostDirectory">Предопределённый системный каталог</param>
        /// <returns></returns>
        public static string GetDirectory(HostDirectory hostDirectory)
        {
            switch(hostDirectory)
            {
                case HostDirectory.Tmp:
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