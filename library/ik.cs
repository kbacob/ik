using System.Collections.Generic;
using System.Text;

namespace ik
{
    using System;
    using System.Threading;
    using ik.Types;
    using ik.Net;
    using ik.Utils;

    /// <summary>
    /// Основной класс библиотеки 
    /// </summary>
    public static class Main
    {
        [Flags]
        public enum UseIkLibraryPart : uint
        {
            Log                 = 1,
            Debug               = 2,
            Ini                 = 4,
            Args                = 8,
            AllowOverrideByArgs = 16,
            Net                 = 32,
            All                 = uint.MaxValue
        }

        public static int intMaxThreads = 4;
        public static LogFile.LogType flagsLogType = LogFile.LogType.Console | LogFile.LogType.Debug;
        public static LogFile.TimeFormat enumTimeFormat = LogFile.TimeFormat.ShortLocalized;
        public static string strIniFile = null;
        public static string strLogFile = null;
        public static Variables varsArgs = null;
        public static Variables varsIni = null;
        public static UseIkLibraryPart flagsLibraryUsedParts = UseIkLibraryPart.All;

        public static void Init(string[] args = null)
        {
            ThreadPool.SetMaxThreads(intMaxThreads, intMaxThreads);
            ThreadPool.SetMinThreads(2, 2);

            Init_Args(args);
            Init_Ini();
            Init_Log();
            Init_Net();
        }
        private static void Init_Args(string[] args)
        {
            if (flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.Args))
            {
                if (Strings.IsNullOrEmpty(args)) throw new ArgumentNullException();
                varsArgs = CommandLine.Read(args);
            }
        }
        private static void Init_Ini()
        {
            if (flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.Ini))
            {
                var strTmpIniFile = strIniFile;

                if (varsArgs != null && flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.AllowOverrideByArgs))
                {
                    if (varsArgs.ContainsValue("args", "ini")) strTmpIniFile = varsArgs["args"]["ini"];
                }

                if (String.IsNullOrEmpty(strTmpIniFile)) throw new ArgumentNullException();

                varsIni = IniFile.Read(strTmpIniFile);
            }
        }
        private static void Init_Log()
        {
            if (flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.Log))
            {
                var strTmpLogFile = strLogFile;

                if (varsIni != null)
                {
                    if (varsIni.ContainsValue("Logging", "LogFile")) strTmpLogFile = varsIni["Logging"]["LogFile"];
                    if (varsIni.ContainsValue("Logging", "LogReceivers")) flagsLogType = (LogFile.LogType)Convert.ToInt32(varsIni["Logging"]["LogReceivers"]);
                    if (varsIni.ContainsValue("Logging", "LogTimeFormat")) enumTimeFormat = (LogFile.TimeFormat)Convert.ToInt32(varsIni["Logging"]["LogTimeFormat"]);
                }

                if (varsArgs != null && flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.AllowOverrideByArgs))
                {
                    if (varsArgs.ContainsValue("args", "log")) strTmpLogFile = varsArgs["args"]["log"];
                }

                if (String.IsNullOrEmpty(strTmpLogFile)) throw new ArgumentNullException();
                LogFile.Init(flagsLogType, enumTimeFormat, strTmpLogFile);
            }
        }
        private static void Init_Net()
        {
            ContentTypes.Init();
        }

        public static void Exit()
        {
            Exit_Net();
            Exit_Log();
            Exit_Ini();
            Exit_Args();
        }
        private static void Exit_Args()
        {
            if (varsArgs != null)
            {
                varsArgs.Clear();
                varsArgs = null;
            }
        }
        private static void Exit_Ini()
        {
            if (varsIni != null)
            {
                varsIni.Clear();
                varsIni = null;
            }
        }
        private static void Exit_Log()
        {
            if (flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.Log))
            {
                LogFile.Close();
            }
        }
        private static void Exit_Net()
        {
            ContentTypes.Exit();
        }
    }
}
