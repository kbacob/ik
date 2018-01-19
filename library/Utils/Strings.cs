// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru> 

namespace ik.Utils
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Вспомогательные методы для работы со строковыми переменными.
    /// </summary>
    public static class Strings
    {
        /// <summary>
        /// Создаёт regex-выражение на основе wildcard-маски, например из маски 'index*.htm*'  будет построено выражение 'index.*?\.htm.*?'
        /// </summary>
        /// <param name="strPattern"></param>
        /// <returns></returns>
        static private string ConvertFromWildmaskToRegex(string strPattern)
        {
            if(Exists(strPattern))
            {
                string strRegexPattern = "";

                foreach (char chrSymbol in strPattern)
                {
                    switch (chrSymbol)
                    {
                        case '?':
                            strRegexPattern += ".";
                            break;

                        case '*':
                            strRegexPattern += ".*?";
                            break;

                        case '.':
                            strRegexPattern += @"\.";
                            break;

                        default:
                            strRegexPattern += chrSymbol;
                            break;
                    }
                }
                return strRegexPattern;
            }
            throw new ArgumentNullException();
        }
        
        /// <summary>
        /// Способы поиска подстрок для Strings.Contains()
        /// </summary>
        public enum EqualAnalysisType
        {
            /// <summary>
            /// Строгое соответствие. Подстрока типа 'test' будет найдена в 'this is test' и не будет в this is Test'
            /// </summary>
            Strong = 1,
            /// <summary>
            /// DOS-style Wildcard (? и *). Подстрока типа '?est' будет найдена в 'is fest', 'is fest', 'is est'. Подстрока типа 'fa*os' будет найдена в 'go to faros', 'check your fallos', 'far not running under msdos'
            /// </summary>
            WildMask,
            /// <summary>
            /// Полноценный Regex. Подстроки типа '\s?' или '^\.*\Z'. Непонятно - зачем и как? Не юзай.
            /// </summary>
            Regex
        }

        /// <summary>
        /// Проверяет строку на существование и не нулевую длину
        /// </summary>
        /// <param name="strCheckedString">Указатель на строку для проверки</param>
        /// <returns>true, если строка существует и её длина не нулевая</returns>
        public static bool Exists(string strCheckedString)
        {
            if (strCheckedString != null)
                if (strCheckedString.Length > 0) return true;

            return false;
        }

        /// <summary>
        /// Проверяет список строк на существование и не нулевую длину всего списка и каждой строки в списке
        /// </summary>
        /// <param name="strCheckedStringsList">Указатель на список строк</param>
        /// <returns>true, если переданный список существует, как и всё строки в нём</returns>
        public static bool Exists(string[] strCheckedStringsList)
        {
            if (strCheckedStringsList != null)
            {
                if (strCheckedStringsList.Length > 0)
                {
                    foreach (string strTmp in strCheckedStringsList)
                    {
                        if (!Exists(strTmp)) return false;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверить наличие подстроки в указанной строке.
        /// </summary>
        /// <param name="strForCheck">Строка, которую нужно проверять</param>
        /// <param name="strPattern">Подстрока, которую нужно искать</param>
        /// <returns>true, если указанная подстрока присутствует</returns>
        public static bool ContainsByStrong(string strForCheck, string strPattern)
        {
            return strForCheck.Contains(strPattern); // наверное нет смысла юзать здесь regex, хотя скорости я не сравнивал
        }

        /// <summary>
        /// Проверить наличие подстроки-маски в указанной строке.
        /// </summary>
        /// <param name="strForCheck">Строка, которую нужно проверять</param>
        /// <param name="strPattern">Маска, которую нужно искать</param>
        /// <returns>true, если указанная подстрока присутствует</returns>
        public static bool ContainsByWildmask(string strForCheck, string strPattern)
        {
            if (strForCheck == null || strPattern == null) throw new ArgumentNullException();

            return ContainsByRegex(strForCheck, ConvertFromWildmaskToRegex(strPattern));
        }

        /// <summary>
        /// Проверить наличие Regex-выражения в указанной строке.
        /// </summary>
        /// <param name="strForCheck">Строка, которую нужно проверять</param>
        /// <param name="strPattern">Regex-выражение, которое нужно искать</param>
        /// <returns>true, если указанное regex-выражение присутствует</returns>
        public static bool ContainsByRegex(string strForCheck, string strPattern)
        {
            if (strForCheck == null || strPattern == null) throw new ArgumentNullException();

            Match mt = Regex.Match(strForCheck, strPattern);

            if (mt != null) return mt.Success;

            return false;
        }

        /// <summary>
        /// Проверить наличие подстроки, маски, или Regex-выражения в указанной строке. 
        /// </summary>
        /// <param name="strForCheck">Строка, которую нужно проверять</param>
        /// <param name="strPattern">Подстрока, маска или regex-выражение, которые нужно искать</param>
        /// <param name="equalAnalysisType">Способ поиска подстроки</param>
        /// <returns>true, если указанная подстрока (или маска) присутствует</returns>
        public static bool Contains(string strForCheck, string strPattern, EqualAnalysisType equalAnalysisType = EqualAnalysisType.Strong)
        {
            if (strForCheck == null || strPattern == null) throw new ArgumentNullException();

            switch (equalAnalysisType)
            {
                case EqualAnalysisType.Strong:
                    return ContainsByStrong(strForCheck, strPattern);

                case EqualAnalysisType.WildMask:
                    return ContainsByWildmask(strForCheck, strPattern);

                case EqualAnalysisType.Regex:
                    return ContainsByRegex(strForCheck, strPattern);
            }
            return false;
        }

        /// <summary>
        /// Заменить часть (или части) исходной строки, совпадающую с подстрокой, на указанную подстроку
        /// </summary>
        /// <param name="strOriginal">Исходная строка</param>
        /// <param name="strPattern">Подстрока, которую нужно искать</param>
        /// <param name="strNew">Новое значение</param>
        /// <returns>Новая строка с совершенными заменами, или исходная если подстроки для замены не были найдены</returns>
        public static string ReplaceByStrong(string strOriginal, string strPattern, string strNew = "")
        {
            if (strOriginal == null || strPattern == null || strPattern == null) throw new ArgumentNullException();

            return strOriginal.Replace(strPattern, strNew); // наверное нет смысла юзать здесь regex, хотя скорости я не сравнивал
        }

        /// <summary>
        /// Заменить часть (или части) исходной строки, совпадающую с wildcard-маской на указанную подстроку
        /// </summary>
        /// <param name="strOriginal">Исходная строка</param>
        /// <param name="strPattern">Wildcard-маска, которуе нужно искать</param>
        /// <param name="strNew">Новое значение</param>
        /// <returns>Новая строка с совершенными заменами, или исходная если подстроки для замены не были найдены</returns>
        public static string ReplaceByWildmask(string strOriginal, string strPattern, string strNew = "")
        {
            if (strOriginal == null || strPattern == null || strPattern == null) throw new ArgumentNullException();

            return ReplaceByRegex(strOriginal, ConvertFromWildmaskToRegex(strPattern), strNew);
        }

        /// <summary>
        /// Заменить часть (или части) исходной строки, совпадающую с regex-выражением на указанную подстроку
        /// </summary>
        /// <param name="strOriginal">Исходная строка</param>
        /// <param name="strPattern">Regex-выражение, которое нужно искать</param>
        /// <param name="strNew">Новое значение</param>
        /// <returns>Новая строка с совершенными заменами, или исходная если подстроки для замены не были найдены</returns>
        public static string ReplaceByRegex(string strOriginal, string strPattern, string strNew = "")
        {
            if (strOriginal == null || strPattern == null || strPattern == null) throw new ArgumentNullException();

            if (ContainsByRegex(strOriginal, strPattern)) return Regex.Replace(strOriginal, strPattern, strNew);

            return strOriginal;
        }

        /// <summary>
        /// Заменить часть (или части) исходной строки, совпадающую с  подстрокой, wildcard-маской или regex-выражением на указанную подстроку
        /// </summary>
        /// <param name="strOriginal">Исходная строка</param>
        /// <param name="strPattern">Подстрока, маска или regex-выражение, которые нужно искать</param>
        /// <param name="strNew">Новое значение</param>
        /// <param name="equalAnalysisType">Способ поиска подстроки</param>
        /// <returns>Новая строка с совершенными заменами, или исходная если подстроки для замены не были найдены</returns>
        public static string Replace(string strOriginal, string strPattern, string strNew = "", EqualAnalysisType equalAnalysisType = EqualAnalysisType.Strong)
        {
            if (strOriginal == null || strPattern == null || strPattern == null) throw new ArgumentNullException();

            switch (equalAnalysisType)
            {
                case EqualAnalysisType.Strong:
                    return ReplaceByStrong(strOriginal, strPattern, strNew);

                case EqualAnalysisType.WildMask:
                    return ReplaceByWildmask(strOriginal, strPattern, strNew);

                case EqualAnalysisType.Regex:
                    return ReplaceByRegex(strOriginal, strPattern, strNew);
            }
            return null;
        }
    }
}
