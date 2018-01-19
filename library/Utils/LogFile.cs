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
        [Flags]
        public enum LogType
        {
            /// <summary>
            /// Выводить сообщения на консоль
            /// </summary>
            Console = 1,
            /// <summary>
            /// Выводить сообщения в консоль отладки
            /// </summary>
            Debug,
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
            Default = int.MaxValue
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

        static private Dictionary<MessageType, String> LMessageTypePrefix = null;
        static private Dictionary<TimeFormat, String> LTimeFormat = null;
        static private LogType logType = LogType.Debug | LogType.File;
        static private TimeFormat timeFormat;
        static private string strLogFile = @"application.log";
        static private OSPlatform OS = Environment.GetOS();

        /// <summary>
        /// Инициализация системы записи журнала сообщений
        /// </summary>
        /// <param name="logType">Тип журнала сообшений</param>
        /// <param name="timeFormat">Формат записи времени сообщения</param>
        /// <param name="strLogFile">Имя файла журнала сообщений</param>
        /// <returns></returns>
        static public void Init(LogType logType = LogType.Debug | LogType.File , TimeFormat timeFormat = TimeFormat.ShortLocalized, string strLogFile = @"application.log")
        {
            LogFile.logType = logType;
            LogFile.timeFormat = timeFormat;
            LogFile.strLogFile = strLogFile;

            OS = Environment.GetOS();
            LMessageTypePrefix = new Dictionary<MessageType, string>
            {
                { MessageType.Verbose , "[ ]" },
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
        }
        /// <summary>
        /// Закрытие журнала сообщений
        /// </summary>
        static public void Close()
        {
            LogFile.WriteLine("End", MessageType.Verbose, TimeFormat.None);

            LMessageTypePrefix.Clear();
            LTimeFormat.Clear();
            LMessageTypePrefix = null;
            LTimeFormat = null;
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
        
        /// <summary>
        /// Отправить сообщение в журнал.
        /// </summary>
        /// <param name="strText">Текст соообщения</param>
        /// <param name="messageType">Тип сообщения. По умолчанию это Verbose, и если вы ничего не меняли, то префикс для таких сообщений отсутствует</param>
        /// <param name="timeFormat">Формат указания времени сообщения. По умолчанию будет использован формат, переданный LogFile.Init()</param>
        static public void WriteLine(string strText, MessageType messageType = MessageType.Verbose, TimeFormat timeFormat = TimeFormat.Default)
        {
            if (LMessageTypePrefix == null || LTimeFormat == null) return;
            if (LMessageTypePrefix.Count == 0 || LTimeFormat.Count == 0) return;

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

            if (logType.HasFlag(LogType.Console))
            {
                Console.WriteLine(strTime + strMessageType + strText);
            }
            if (logType.HasFlag(LogType.Debug))
            {
                System.Diagnostics.Debug.WriteLine(strMessageType + strText);
            }
            if (logType.HasFlag(LogType.File) || logType.HasFlag(LogType.Syslog))
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

        static public void Verbose(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Verbose, timeFormat);
        static public void Trace(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Trace, timeFormat);
        static public void Debug(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Debug, timeFormat);
        static public void Info(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Info, timeFormat);
        static public void Warning(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Warning, timeFormat);
        static public void Error(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Error, timeFormat);
        static public void Critical(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Critical, timeFormat);
        static public void Panic(string strText, TimeFormat timeFormat = TimeFormat.Default) => WriteLine(strText, MessageType.Panic, timeFormat);
    }
}
