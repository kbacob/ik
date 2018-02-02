namespace ik.Template
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Text;
    using ik.Utils;
    using ik.Types;

    [Flags]
    public enum ParserPolicy
    {
        AllowIncludes = 1,
        AllowDefines = 2
    }
    public delegate string ParserFunc(ref string strInput);

    /*
     *  Формат подстановок:
     *  {% KEYWORD Param = "Value" %} и {% KEYWORD "Value" %} пробелы не обязательны, то есть можно и так {%KEYWORD Param="Value"%} {%KEYWORD "Value"%}
     * 
     *  Стандартные:
     *  {%INCLUDE "path"%} - тут всё понятно, надеюсь. Допускается рекурсивность. 
     *  {%DEFINE Key="Value"%} определяем и потом юзаем как {% Key %} или {%Key%} Любые директивы-обьявления в подгружаемых темеплйтах будут доступны и "родителях". 
     *  ERR: в одной строке низя определить DEFINE и сразу его юзать. Но, зато, можно где-то определить DEFIENE а потом использовать его в другом определении: {% DEFINE ExpandedVar = "this {%DeifenedBefore%} variable!" %}
     * 
     *  Ну и добавление своих: Set("KEYWORD", ParserFunc) (см делегат)
     * 
     */

    public class Parser : Dictionary<string, ParserFunc> 
    {
        private string strCurrentFile = null;
        private KeyValueList LDefines = null;
        private ParserPolicy policy = 0;
        private Parser parent;

        private Parser GetFirstParent()
        {
            var objParent = this;

            while (objParent.parent != null) objParent = objParent.parent;

            return objParent;
        }
        private ParserFunc Get(string strPattern) => GetFirstParent()[strPattern];

        public Parser(Parser parent = null)
        {
            if(parent != null) this.parent = parent;

            strCurrentFile = null;
            policy = 0;

            if (this.parent == null)
            {
                LDefines = new KeyValueList();
                Set("INCLUDE", _Include);
                Set("DEFINE", _Define);
            }
        }
        ~Parser()
        {
            if (parent == null)
            {
                LDefines.Clear();
                LDefines = null;
                Clear();
            }
        }

        private string _Include(ref string strParams)
        {
            string strResult = null;
            var mt = Regex.Match(strParams, @"\""(?<Value>.*)\""");

            if(mt.Success)
            {
                var strPath = Files.FixPathByOS(Files.GetOnlyPath(strCurrentFile) + "/" + mt.Groups["Value"].Value);
                var objParser = new Parser(this);

                strResult = objParser.ParseFile(strPath, GetFirstParent().policy);
            }

            return strResult;
        }
        private string _Define(ref string strParams) // TODO: добавить "локальные" определения DEFINE_LOCAL и сервисные UNDEFINE, IF_DEFINED, IF_NODEFINED, ENDIF, ELSE
        {
            var mt = Regex.Match(strParams, @"(?<Key>\w+)\s*\=?\s*\""(?<Value>.*)\""");

            if (mt.Success)
            {
                GetFirstParent().LDefines.Add(mt.Groups["Key"].Value, mt.Groups["Value"].Value);
            }

            return null;
        }

        public void Set(string strPattern, ParserFunc func) 
        {
            if (GetFirstParent().ContainsKey(strPattern))
            {
                GetFirstParent()[strPattern] = func;
            }
            else
            {
              GetFirstParent().Add(strPattern, func);
            }
        }
        public string ParseString(string strSource, ParserPolicy flagsPolicy = ParserPolicy.AllowDefines | ParserPolicy.AllowIncludes) // TODO: Порезать на методы, а то читается сложно
        {
            var sbResult = new StringBuilder("");

            policy = flagsPolicy;
            if (strSource == null) throw new ArgumentNullException("strSource");
            if (strSource.Length > 0)
            {
                var strStrings = Regex.Split(strSource, @"\r?\n|\r");

                foreach(var strString in strStrings)
                {
                    if (strString.Contains("{%"))
                    {
                        string strProceed = strString;
                        string strFull = null;
                        string strDirective = null;
                        string strParams = null;
                        Match mt;

                        // Сначала проверим, нет ли в обрабатываемой строке ранее определёных DEFINES, вида {%ItemName%}
                        do
                        {
                            strFull = null;
                            strDirective = null;
                            mt = Regex.Match(strProceed, @"\{\%\s*(?<Key>\w+)\s*\%\}"); // TODO поддержка дефолтных значений с форматом вида {%ItemName|"Value if ItemName not exists"%}

                            if (mt.Success)
                            {
                                strFull = mt.Groups[0].Value;
                                strDirective = mt.Groups["Key"].Value;

                                if (LDefines.Count > 0 && !String.IsNullOrWhiteSpace(strDirective) && !String.IsNullOrWhiteSpace(strFull))
                                {
                                    strProceed = Strings.ReplaceByStrong(strProceed, strFull, GetFirstParent().LDefines.Get(strDirective));
                                }
                                else
                                {
                                    strProceed = Strings.ReplaceByStrong(strProceed, strFull, "");
                                }
                            }
                        } while (mt.Success);
                        

                        // Теперь смотрим на конструкции типа {% DEFINE a = "b" %} или {% INCLUDE "zzzz.tpl" %}
                        do
                        {
                            strFull = null;
                            strDirective = null;
                            strParams = null;
                            mt = Regex.Match(strProceed, @"\{\%\s*(?<Directive>\w+?)[\s\=\s]+(?<Params>.*?)\s*?\%\}");

                            if (mt.Success)
                            {
                                strFull = mt.Groups[0].Value;
                                strDirective = mt.Groups["Directive"].Value;
                                strParams = mt.Groups["Params"].Value;

                                if (String.Equals(strDirective, "DEFINE") && !flagsPolicy.HasFlag(ParserPolicy.AllowDefines)) strDirective = null;
                                if (String.Equals(strDirective, "INCLUDE") && !flagsPolicy.HasFlag(ParserPolicy.AllowIncludes)) strDirective = null;

                                if (!String.IsNullOrWhiteSpace(strDirective) && strParams != null && !String.IsNullOrWhiteSpace(strFull))
                                {
                                    strProceed = Strings.ReplaceByStrong(strProceed, strFull, GetFirstParent()[strDirective](ref strParams));
                                }
                                else
                                {
                                    strProceed = Strings.ReplaceByStrong(strProceed, strFull, "");
                                }
                            }
                        } while (mt.Success);

                        if(!String.IsNullOrWhiteSpace(strProceed)) sbResult.AppendLine(strProceed);
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(strString)) sbResult.AppendLine(strString);
                    }
                }
            }
            return sbResult.ToString();
        }
        public string ParseFile(string strFileName, ParserPolicy flagsPolicy = ParserPolicy.AllowDefines | ParserPolicy.AllowIncludes)
        {
            if (Files.Exists(strFileName))
            {
                strCurrentFile = strFileName;

                return ParseString(File.ReadAllText(strFileName), flagsPolicy);
            }
            return null;
        }
    }
}
