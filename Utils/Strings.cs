// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Utils
{
    /// <summary>
    /// Вспомогательные методы для работы со строковыми переменными.
    /// </summary>
    public static class Strings
    {
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
    }
}
