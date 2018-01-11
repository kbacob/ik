// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Система записи сообщений в журнал. 
    /// <para>Позволяет просто указать, куда писать и дальше тупо вызывать LofFile.WriteLine("BlaBlaBla"), что на выходе даст файл со строками вида "1980-01-01 12:12:14 UTC [I] BlaBlaBla"</para>
    /// <para>При необходимости кастомизируется префикс времени и тип сообщения</para>
    /// <para>Сейчас пишет на консоль или в файл. В планах syslog Windows и Linux, если конечно в Net.Standart это возможно вообще</para>
    /// </summary>
    static public class LogFile
    {
        /// <summary>
        ///  Тип log-файла
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// Не выводить сообщения
            /// </summary>
            Null,
            /// <summary>
            /// Выводить сообщения на консоль
            /// </summary>
            Console,
            /// <summary>
            /// Записывать сообщения в файл
            /// </summary>
            File,
            /// <summary>
            /// [TODO] Записывать сообщения в лог системы, пока что пишутся в файл 
            /// </summary>
            Syslog
        }
        /// <summary>
        /// Формат префикса строки сообщения с указанием времени события 
        /// </summary>
        public enum TimeFormat
        {
            /// <summary>
            /// Не указывать время
            /// </summary>
            None,
            /// <summary>
            /// Короткое локальное время
            /// </summary>
            ShortLocalized,
            /// <summary>
            /// Полное локальное время
            /// </summary>
            FullLocalized,
            /// <summary>
            /// Короткое универсальное время
            /// </summary>
            ShortUTC,
            /// <summary>
            /// Длинное универсальное время
            /// </summary>
            FullUTC,
            /// <summary>
            /// [INTERNAL] Использовать формат времени, указанный при инициализации
            /// </summary>
            Default
        }
        /// <summary>
        /// Тип сообщения, отбражаемый как префикс, непосредствено перед сообщением
        /// </summary>
        public enum MessageType
        {
            Verbose,
            Trace,
            Debug,
            Info,
            Warning,
            Error,
            Critical,
            Panic
        }

        static private Dictionary<MessageType, String> LMessageTypePrefix;
        static private Dictionary<TimeFormat, String> LTimeFormat;
        static private LogType logType;
        static private TimeFormat timeFormat;
        static private string strLogFile;
        static private OSPlatform OS;

        /// <summary>
        /// Инициализация системы записи журнала сообщений
        /// </summary>
        /// <param name="logType">Тип журнала сообшений</param>
        /// <param name="timeFormat">Формат записи времени сообщения</param>
        /// <param name="strLogFile">Имя файла журнала сообщений</param>
        /// <returns></returns>
        static public bool Init(LogType logType = LogType.File, TimeFormat timeFormat = TimeFormat.ShortLocalized, string strLogFile = @"application.log")
        {
            LogFile.logType = logType;
            LogFile.timeFormat = timeFormat;
            OS = Environment.GetOS();
            LMessageTypePrefix = new Dictionary<MessageType, string>
            {
                { MessageType.Verbose , "" },
                { MessageType.Trace   , "[T]" },
                { MessageType.Debug   , "[D]" },
                { MessageType.Info    , "[I]" },
                { MessageType.Warning , "[W]" },
                { MessageType.Error   , "[E]" },
                { MessageType.Critical, "[C]" },
                { MessageType.Panic   , "[P]" }
            };
            LTimeFormat = new Dictionary<TimeFormat, string>
            {
                { TimeFormat.None, "" },
                { TimeFormat.ShortLocalized, "yyyyMMddHHmmss" },
                { TimeFormat.FullLocalized , "yyyy/MM/dd HH:mm:ss:ffff K" },
                { TimeFormat.ShortUTC      , "yyyyMMddHHmmss" },
                { TimeFormat.FullUTC       , "yyyy/MM/dd HH:mm:ss:ffff UTC" }
            };

            switch (LogFile.logType)
            {
                case LogType.Null:
                case LogType.Console:
                    LogFile.strLogFile = null;
                    return true;

                case LogType.File:
                    LogFile.strLogFile = strLogFile;
                    return true;

                case LogType.Syslog:
                    return true;

                default:
                    LogFile.logType = LogType.Null;
                    LogFile.strLogFile = null;
                    return false;
            }
        }
        /// <summary>
        /// Закрытие журнала сообщений
        /// </summary>
        static public void Close()
        {
            LogFile.WriteLine("", MessageType.Verbose, TimeFormat.None);
            return;
        }

        /// <summary>
        /// Переопределение префиксов типов сообщений, если стандартные [D], [E], [C] и т.п. не устраивают
        /// </summary>
        /// <param name="messageType">Тип сообщения</param>
        /// <param name="strPrefix">Префикс для указания типа сообщения</param>
        static public void SetMessagePrefix(MessageType messageType, string strPrefix)
        {
            if (LMessageTypePrefix != null)
            {
                if (LMessageTypePrefix.ContainsKey(messageType))
                {
                    LMessageTypePrefix[messageType] = strPrefix;
                }
            }
        }

        /// <summary>
        /// <para>Переопределение формата строк с указанием времени события, если стандартные не устраивают.</para>
        /// По поводу формата смотри описания DateTime.Now.ToString(fmt) или DateTime.UtcNow.ToString(fmt) в документации от Microsoft
        /// </summary>
        /// <param name="timeFormat">Тип формата префикса времени сообщения</param>
        /// <param name="strTimeFormat">Строка, описывающая формат префикса времени сообщения</param>
        static public void SetTimeFormat(TimeFormat timeFormat, string strTimeFormat)
        {
            if (LTimeFormat != null)
            {
                if (LTimeFormat.ContainsKey(timeFormat))
                {
                    LTimeFormat[timeFormat] = strTimeFormat;
                }
            }
        }

        static public MessageType GetMessageType(int intMessageType = 0)
        {
            if (intMessageType >= 0 && intMessageType <= 7)
            {
                switch (intMessageType)
                {
                    case 0: return MessageType.Verbose; 
                    case 1: return MessageType.Trace; 
                    case 2: return MessageType.Debug; 
                    case 3: return MessageType.Info; 
                    case 4: return MessageType.Warning;
                    case 5: return MessageType.Error; 
                    case 6: return MessageType.Critical;
                    case 7: return MessageType.Panic;
                }
            }
            throw new Exception("Invalid intMessageType=" + intMessageType.ToString());
        }
        static public MessageType GetMessageType(string strMessageType = null)
        {
            if (Strings.Exists(strMessageType))
            {
                int intMessageType;

                switch (strMessageType)
                {
                    case "Verbose":
                        intMessageType = 0;
                        break;

                    case "Trace":
                        intMessageType = 0;
                        break;

                    case "Debug":
                        intMessageType = 0;
                        break;

                    case "Info":
                        intMessageType = 0;
                        break;

                    case "Warning":
                        intMessageType = 0;
                        break;

                    case "Error":
                        intMessageType = 0;
                        break;

                    case "Critical":
                        intMessageType = 0;
                        break;

                    case "Panic":
                        intMessageType = 0;
                        break;

                    default:
                        throw new Exception("Unknown strMessageType \"" + strMessageType + "\"");
                }
                return GetMessageType(intMessageType);
            }
            throw new Exception("Null strMessageType");
        }
        static public LogType GetLogType(int intLogType = 0)
        {
            if (intLogType >= 0 && intLogType <= 3)
            {
                switch(intLogType)
                {
                    case 0: return LogType.Null;
                    case 1: return LogType.Console;
                    case 2: return LogType.File;
                    case 3: return LogType.Syslog;
                }
            }
            throw new Exception("Invalid intLogType=" + intLogType.ToString());
        }
        static public LogType GetLogType(string strLogType)
        {
            if (Strings.Exists(strLogType))
            {
                int intLogType;
                switch (strLogType)
                {
                    case "Null":
                        intLogType = 0;
                        break;

                    case "Console":
                        intLogType = 1;
                        break;

                    case "File":
                        intLogType = 2;
                        break;

                    case "Syslog":
                        intLogType = 3;
                        break;

                    default:
                        throw new Exception("Invalid strLogType = \"" + strLogType + "\"");
                }
                return GetLogType(intLogType);
            }
            throw new Exception("Empty strLogType");
        }

        /// <summary>
        /// Отправить сообщение в журнал.
        /// </summary>
        /// <param name="strText">Текст соообщения</param>
        /// <param name="messageType">Тип сообщения. По умолчанию это Verbose, и если вы ничего не меняли, то префикс для таких сообщений отсутствует</param>
        /// <param name="timeFormat">Формат указания времени сообщения. По умолчанию будет использован формат, переданный LogFile.Init()</param>
        static public void WriteLine(string strText, MessageType messageType = MessageType.Verbose, TimeFormat timeFormat = TimeFormat.Default)
        {
            string strTime = "";
            string strMessageType = LMessageTypePrefix[messageType];
            TimeFormat tmpTimeFormat = timeFormat == TimeFormat.Default ? LogFile.timeFormat : timeFormat;

            switch (tmpTimeFormat)
            {
                case TimeFormat.ShortLocalized:
                case TimeFormat.FullLocalized:
                    strTime = DateTime.Now.ToString(LTimeFormat[tmpTimeFormat]);
                    break;

                case TimeFormat.ShortUTC:
                case TimeFormat.FullUTC:
                    strTime = DateTime.UtcNow.ToString(LTimeFormat[tmpTimeFormat]);
                    break;
            }

            if (strTime.Length > 0) strTime += " ";
            if (strMessageType.Length > 0) strMessageType += " ";

            if (logType == LogType.Console)
            {
                Console.WriteLine(strTime + strMessageType + strText);
            }
            else if (logType == LogType.File || logType == LogType.Syslog) // TODO: Windows Log, Linux syslog and may be OSX
            {
                if (Strings.Exists(strLogFile))
                { 
                    StreamWriter objStream = new StreamWriter(strLogFile, encoding: Encoding.Default, append: true);

                    if (objStream != null)
                    {
                        objStream.WriteLine(strTime + strMessageType + strText);
                        objStream.Flush();
                        objStream.Close();
                        objStream.Dispose();
                        objStream = null;
                    }
                }
            }
        }
    }
}
