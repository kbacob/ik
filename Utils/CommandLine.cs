﻿// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    // You also needs ik_Variables and ik_Strings
    using ik.Types;

    /// <summary>
    /// Анализ ключей запуска приложения.
    /// </summary>
    public static class CommandLine
    {
        /// <summary>
        ///  Анализирует ключи запуска, возвращая заполенный список вида Переменная=Значение
        /// </summary>
        /// <param name="args">Список ключей запуска приложения, получаемые на точке входа</param>
        /// <param name="strSectionName">Наименование секции для хранения результата анализа</param>
        /// <returns>Список переменных</returns>
        public static Variables Read(string[] args, string strSectionName = "args")
        {
            Variables varResult = new Variables();

            if (args.Length > 0)
            {
                for (uint intTmp = 0; intTmp < args.Length; intTmp++)
                {
                    if (args[intTmp].Contains("="))
                    {
                        string[] strTmp = args[intTmp].Split('=');

                        if (strTmp.Length == 2)
                        {
                            if (Strings.Exists(strTmp[0]))
                            {
                                varResult.Add(strSectionName, strTmp[0], strTmp[1]);
                            }
                        }
                        else
                        {
                            varResult.Add(strSectionName, strTmp[0], null);
                        }
                    }
                    else
                    {
                        bool isPair = false;

                        if (intTmp < args.Length - 2)
                        {
                            if (args[intTmp + 1] == "=")
                            {
                                varResult.Add(strSectionName, args[intTmp], args[intTmp + 2]);
                                isPair = true;
                            }
                        }

                        if (isPair)
                        {
                            intTmp += 2;
                        }
                        else
                        {
                            varResult.Add(strSectionName, args[intTmp], null);
                        }
                    }
                }
            }
            return varResult;
        }
    }
}
