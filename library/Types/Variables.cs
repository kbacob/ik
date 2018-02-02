// Copyright © 2017,2018 Igor Sergienko. Contacts: <kbacob@mail.ru>

namespace ik.Types
{
    using System.Collections.Generic;

    /// <summary>
    /// Реализация трёхмерного массива хранимых строковых переменных вида ["Секция"].["Параметр"]."Значение" на основе двух Dictionary.
    /// Мне понадобилось, когда не понравился способ чтения классических ini-файлов от Microsoft
    /// </summary> 
    public class Variables : Dictionary<string, Dictionary<string, string>>  
    {
        /// <summary>
        /// Добавление новой секции
        /// </summary>
        /// <param name="strSection">Название добавляемой секции</param>
        public void Add(string strSection)
        {
            if (!ContainsKey(strSection)) Add(strSection, new Dictionary<string, string>());
        }

        /// <summary>
        /// Добавление новой секции и новой переменной, секция уже может существовать - переменная будет добавлена к ней.
        /// </summary>
        /// <param name="strSection">Название добавляемой или существующей секции</param>
        /// <param name="strParam">Название добавляемой переменной</param>
        public void Add(string strSection, string strParam)
        {
            Add(strSection);
            if (!this[strSection].ContainsKey(strParam)) this[strSection].Add(strParam, "");
        }

        /// <summary>
        /// Добавление новой секции и новой переменной со значением, секция и переменная могут уже существовать - значение будет применено к их содержимому.
        /// </summary>
        /// <param name="strSection">Название добавляемой или существующей секции</param>
        /// <param name="strParam">Название добавляемоей или существующей переменной</param>
        /// <param name="strValue">Значения для хранения</param>
        public void Add(string strSection, string strParam, string strValue)
        {
            Add(strSection, strParam);
            this[strSection][strParam] = strValue;
        }

        /// <summary>
        /// Проверка на существование секции с указанным именем
        /// </summary>
        /// <param name="strSection">Имя секции для проверки</param>
        /// <returns></returns>
        public bool ContainsSection(string strSection)
        {
            return ContainsKey(strSection);
        }

        /// <summary>
        ///  Проверка на существование секции и вложенного в неё параметра с указанными именами
        /// </summary>
        /// <param name="strSection">Имя секции для проверки</param>
        /// <param name="strParam">Имя параметра для проверки</param>
        /// <returns></returns>
        public bool ContainsParam(string strSection, string strParam)
        {
            if (ContainsSection(strSection))
            {
                return this[strSection].ContainsKey(strParam);
            }
            return false;
        }

        /// <summary>
        /// Прверка на существование значение для вложенного параметра указанной секции
        /// </summary>
        /// <param name="strSection">Имя секции для проверки</param>
        /// <param name="strParam">Имя параметра для проверки</param>
        /// <returns></returns>
        public bool ContainsValue(string strSection, string strParam)
        {
            if (ContainsParam(strSection, strParam))
            {
                if (this[strSection][strParam] != null) return true;
            }
            return false;
        }

        /// <summary>
        /// При указанных индексах секции и парметра, возвращает или имя параметра или его значение. Internal.
        /// </summary>
        /// <param name="intSectionIndex">Индекс секции</param>
        /// <param name="intParamIndex">Индекс параметра</param>
        /// <param name="boolNeedValue">FALSE - вернёт имя параметра, TRUE  - значение пармаетра</param>
        /// <returns>Имя или значение параметра, или NULL если что-то не так.</returns>
        private string _NameOrValueByIndex(int intSectionIndex, int intParamIndex, bool boolNeedValue = false)
        {
            var strSectionName = SectionNameByIndex(intSectionIndex);

            if (strSectionName != null)
            {
                if (intParamIndex < this[strSectionName].Count)
                {
                    var intTmp = 0;
                    foreach (var val in this[strSectionName])
                    {
                        if (intParamIndex == intTmp)
                        {
                            if (boolNeedValue) return val.Value;
                            return val.Key;
                        }
                        intTmp++;
                    }
                }
            }
            return null;
        }
        
        /// <summary>
        /// Возвращает название секции с указанным индексом.
        /// </summary>
        /// <param name="intSectionIndex">Индекс секции</param>
        /// <returns></returns>
        public string SectionNameByIndex(int intSectionIndex)
        {
            if (intSectionIndex < this.Count)
            {
                var iCnt = 0;

                foreach (var val in this)
                {
                    if (intSectionIndex == iCnt) return val.Key;
                    iCnt++;
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает название параметра для указанных индексов секции и параметра
        /// </summary>
        /// <param name="intSectionIndex">Индекс секции</param>
        /// <param name="intParamIndex">Индекс параметра</param>
        /// <returns></returns>
        public string ParamNameByIndex(int intSectionIndex, int intParamIndex)
        {
            return _NameOrValueByIndex(intSectionIndex, intParamIndex, false);
        }
        
        /// <summary>
        /// Возвращает значение параметра для указанных индексов секции и параметра
        /// </summary>
        /// <param name="intSectionIndex">Индекс секции</param>
        /// <param name="intParamIndex">Индекс параметра</param>
        /// <returns></returns>
        public string ValueByIndex(int intSectionIndex, int intParamIndex)
        {
            return _NameOrValueByIndex(intSectionIndex, intParamIndex, true);
        }
        
        /// <summary>
        /// Возвращает значение для указанных секции и параметра.
        /// Можно, конечено получать параметр как Variables[SectionName][ParamName], но тода нужно городить обработку исключений. 
        /// А это не всегда и нужно. Иногда достаточно просто получить NULL.
        /// </summary>
        /// <param name="strSectionName">Имя секции</param>
        /// <param name="strParamName">Имя параметра</param>
        /// <returns>Значение параметра, или NULL</returns>
        public string Get(string strSectionName, string strParamName)
        {
            if (ContainsValue(strSectionName, strParamName))
            {
                return this[strSectionName][strParamName];
            }
            return null;
        }

        /// <summary>
        /// Копирует всё содержимое
        /// </summary>
        /// <param name="varDestination"></param>
        public void CopyTo(Variables varDestination)
        {
            if(Count>0)
            {
                if(varDestination != null)
                {
                    if (Count > 0)
                    {
                        for (var intSectionIndex = 0; intSectionIndex < Keys.Count; intSectionIndex++)
                        {
                            var strSectionName = SectionNameByIndex(intSectionIndex);

                            for (var intParam = 0; intParam < this[strSectionName].Count; intParam++)
                            {
                                var strKey = ParamNameByIndex(intSectionIndex, intParam);
                                var strValue = ValueByIndex(intSectionIndex, intParam);

                                varDestination.Add(strSectionName, strKey, strValue);
                            }
                        }
                    }
                }
            }
        }
    }
}
