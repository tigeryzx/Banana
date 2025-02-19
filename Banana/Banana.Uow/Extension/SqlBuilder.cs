﻿/***********************************
 * Developer: Lio.Huang
 * Date：2018-11-20
 * 
 * Last Update：2018-12-18
 * 2019-08-01  1.允许同名参数
 **********************************/

using Banana.Uow.Interface;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Banana.Uow.Extension
{
    /// <summary>
    /// SQL builder
    /// </summary>
    public class SqlBuilder: ISqlBuilder
    {
        /// <summary>
        /// SQL builder
        /// </summary>
        public SqlBuilder()
        {
        }

        /// <summary>
        /// SQL builder
        /// </summary>
        public SqlBuilder(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        private readonly string _escapeCode = "@@";

        private readonly string _sql;
        private readonly object[] _args;
        private SqlBuilder _rhs;
        private string _sqlFinal;
        private object _argsFinal;

        private void Build()
        {
            if (_sqlFinal != null)
                return;
            
            var sb = new StringBuilder();

            Dictionary<string, object> argsObj = new Dictionary<string, object>();
            Build(sb, argsObj, null);
            _sqlFinal = sb.ToString();

            //动态创建对象
            dynamic obj = new ExpandoObject(); 
            foreach (KeyValuePair<string, object> item in argsObj)
            {
                ((IDictionary<string, object>)obj).Add(item.Key, item.Value);
            } 
            _argsFinal = obj;
        }

        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        /// <summary>
        /// 与SQL性质一样，为处理了转义后的SQL
        /// </summary>
        public string ESQL
        {
            get
            {
                Build();
                return _sqlFinal.Replace(_escapeCode, "@");
            }
        }

        public object Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        public SqlBuilder Append(SqlBuilder sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            return this;
        }

        public SqlBuilder Append(string sql, params object[] args)
        {
            return Append(new SqlBuilder(sql, args));
        }

        public SqlBuilder IsAse(bool asc)
        {
            if (asc)
            {
                return Append(new SqlBuilder("ASC"));
            }
            else
            {
                return Append(new SqlBuilder("DESC"));
            }
        }

        public SqlBuilder Where(string sql, params object[] args)
        {
            sql = sql.RevomeThePrefix("WHERE");
            return Append(new SqlBuilder("WHERE " + sql, args));
        }

        public SqlBuilder OrderBy(params object[] args)
        {
            return Append(new SqlBuilder("ORDER BY " + GetArgsString("ORDER BY", args: args)));
        }

        public SqlBuilder Select(string prefix = "",params object[] args)
        {
            return Append(new SqlBuilder("SELECT " + GetArgsString("SELECT", prefix: prefix, args: args)));
        }

        public SqlBuilder From(params object[] args)
        {
            return Append(new SqlBuilder("FROM " + string.Join(", ", (from x in args select x.ToString()).ToArray())));
        } 

        private static bool Is(SqlBuilder sql, string sqltype)
        {
            return sql?._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Build(StringBuilder sb, Dictionary<string, object> argsObj, SqlBuilder lhs)
        {
            if (!string.IsNullOrEmpty(_sql))
            {
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                    sb.Append(" ");
                }

                var sql = ProcessParams(_sql, _args, argsObj);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);

                sb.Append(sql);
            }
            
            _rhs?.Build(sb, argsObj, this);
        }

        public static string GetArgsString(string dbKeywordFix, string prefix = "", params object[] args)
        {
            return string.Join(", ", (from x in args select prefix + x.ToString().RevomeThePrefix(dbKeywordFix)).ToArray());
        }
        
        private static readonly Regex rxParams = new Regex(@"(?<!@)@\w+|(?<!:):\w+", RegexOptions.Compiled); 
        private static string ProcessParams(string _sql, object[] args_src, Dictionary<string, object> temp)
        {
            return rxParams.Replace(_sql, m =>
            {
                string param = m.Value.Substring(1);

                bool found = false;
                if (int.TryParse(param, out int paramIndex))
                { 
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException(string.Format("参数 '@{0}' 已指定，但只提供了参数 {1}|The parameter '@{0}' is specified, but only the parameter {1} is provided. (sql: `{2}`)", paramIndex, args_src.Length, _sql)); 
                    var o = args_src[paramIndex]; 
                    var pi = o.GetType().GetProperty(param);
                    if (pi != null)
                    {
                        if (!temp.ContainsKey(pi.Name))
                        {
                            temp.Add(pi.Name, pi.GetValue(o, null));
                        }
                        found = true; 
                    } 
                }
                else
                {  
                    foreach (var o in args_src)
                    { 
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            if (temp.ContainsKey(pi.Name))
                            {
                                found = true;
                                continue;
                            }
                            temp.Add(pi.Name, pi.GetValue(o, null));
                            found = true;
                            break;
                        }
                        else if (o is ExpandoObject)
                        {
                            IDictionary<string, object> dic = o as System.Dynamic.ExpandoObject;
                            foreach (var key in dic.Keys)
                            {
                                if (temp.ContainsKey(key))
                                {
                                    found = true;
                                    continue;
                                }
                                found = true;
                                temp.Add(key, dic[key]);
                                break;
                            } 
                            break;
                        }
                    }  
                }
                if (!found)
                    throw new ArgumentException(string.Format("参数 '@{0}' 已指定， 但传递的参数中没有一个具有该名称的属性|The parameter '@{0}' is specified, but none of the passed parameters has an attribute with that name. (sql: '{1}')", param, _sql));
                //return "@" + (args_dest.Count - 1).ToString();
                return m.Value;
            }
            );
        }
    }
}
