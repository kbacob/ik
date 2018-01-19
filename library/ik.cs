using System.Collections.Generic;
using System.Text;

namespace ik
{
    using System;
    using System.Threading;
    using ik.Types;
    using ik.Net;
    using ik.Utils;

    public static class Main
    {
        [Flags]
        public enum UseIkLibraryPart : uint
        {
            Log = 1,
            Debug,
            Ini,
            Args,
            AllowOverrideByArgs,
            Net,
            All = uint.MaxValue
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
                if (Strings.Exists(args))
                {
                    varsArgs = CommandLine.Read(args);
                }
                else throw new ArgumentNullException();
            }
        }
        private static void Init_Ini()
        {
            if (flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.Ini))
            {
                string strTmpIniFile = strIniFile;

                if (varsArgs != null && flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.AllowOverrideByArgs))
                {
                    if (varsArgs.ContainsValue("args", "ini")) strTmpIniFile = varsArgs["args"]["ini"];
                }

                if (Strings.Exists(strTmpIniFile))
                {
                    varsIni = IniFile.Read(strTmpIniFile);
                }
                else throw new ArgumentNullException();
            }
        }
        private static void Init_Log()
        {
            if (flagsLibraryUsedParts.HasFlag(UseIkLibraryPart.Log))
            {
                string strTmpLogFile = strLogFile;

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

                if (Strings.Exists(strTmpLogFile))
                {
                    LogFile.Init(flagsLogType, enumTimeFormat, strTmpLogFile);
                }
                else throw new ArgumentNullException();
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
