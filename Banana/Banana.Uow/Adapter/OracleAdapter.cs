﻿/***********************************
 * Developer: Lio.Huang
 * Date：2018-12-12
 * 
 * UpdateDate:
 * 2018-12-28  1.更新GetPageList中的Select *  => Select {ColumnList}
 * 2019-01-03  1.更新GetPageList中的property.Name => SqlMapperExtensions.GetColumnAlias(property)
 *             2.更新AppendColumnName、AppendColumnNameEqualsValue 新增别名
 * 2019-08-01  1.Fix bug Issues#8
 **********************************/

using Banana.Uow.Extension;
using Banana.Uow.Interface;
using Banana.Uow.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Uow.Adapter
{
    /// <summary>
    /// The Oracle adapter.
    /// </summary>
    internal partial class OracleAdapter : ISqlAdapter
    {
        /// <summary>
        /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
        /// </summary>
        /// <param name="connection">The connection to use.</param>
        /// <param name="transaction">The transaction to use.</param>
        /// <param name="commandTimeout">The command timeout to use.</param>
        /// <param name="tableName">The table to insert into.</param>
        /// <param name="columnList">The columns to set with this insert.</param>
        /// <param name="parameterList">The parameters to set for this insert.</param>
        /// <param name="keyProperties">The key columns in this table.</param>
        /// <param name="entityToInsert">The entity to insert.</param>
        /// <returns>The Id of the row created.</returns>
        public async Task<int> InsertAsync(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert, bool isList)
        { 
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            string oracleSequence =""; 
            for(int i = 0; i < propertyInfos.Length; i++)
            {
                var sequencePropertyDescriptor = propertyInfos[i].GetCustomAttribute<KeyAttribute>();
                if (sequencePropertyDescriptor != null)
                {
                    oracleSequence = sequencePropertyDescriptor.OracleSequence;
                    break;
                }
            }

            string cmd = "";
            dynamic id = 0;
            if (propertyInfos.Length == 0)
            {
                cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
            }
            else
            {
                if (string.IsNullOrEmpty(oracleSequence))
                    throw new Exception("KeyAttribute's OracleSequence is Null");
                var idp = propertyInfos[0];
               
                var r = connection.Query($"SELECT {oracleSequence}.NEXTVAL ID FROM DUAL", transaction: transaction, commandTimeout: commandTimeout);
                id = r.First().ID; 
                if (id == null)
                    return 0;
                idp.SetValue(entityToInsert, Convert.ChangeType(id, idp.PropertyType), null);
                //cmd = $"insert into {tableName} ({idp.Name},{columnList}) values ({oracleSequence}.Nextval,{parameterList})"; 
                cmd = $"insert into {tableName} ({idp.Name},{columnList}) values ({id},{parameterList})";
            }
            if (isList)
            {
                return await connection.ExecuteAsync(cmd, entityToInsert, transaction, commandTimeout);
            }
            await connection.ExecuteAsync(cmd, entityToInsert, transaction, commandTimeout);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Inserts <paramref name="entityToInsert"/> into the database, returning the Id of the row created.
        /// </summary>
        /// <param name="connection">The connection to use.</param>
        /// <param name="transaction">The transaction to use.</param>
        /// <param name="commandTimeout">The command timeout to use.</param>
        /// <param name="tableName">The table to insert into.</param>
        /// <param name="columnList">The columns to set with this insert.</param>
        /// <param name="parameterList">The parameters to set for this insert.</param>
        /// <param name="keyProperties">The key columns in this table.</param>
        /// <param name="entityToInsert">The entity to insert.</param>
        /// <returns>The Id of the row created.</returns>
        public int Insert(IDbConnection connection, IDbTransaction transaction, int? commandTimeout, string tableName, string columnList, string parameterList, IEnumerable<PropertyInfo> keyProperties, object entityToInsert, bool isList)
        { 
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            string oracleSequence = "";
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                var sequencePropertyDescriptor = propertyInfos[i].GetCustomAttribute<KeyAttribute>();
                if (sequencePropertyDescriptor != null)
                {
                    oracleSequence = sequencePropertyDescriptor.OracleSequence;
                    break;
                }
            }
            
            string cmd = "";
            dynamic id = 0;
            if (propertyInfos.Length == 0)
            {
                cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
            }
            else
            {
                if (string.IsNullOrEmpty(oracleSequence))
                    throw new Exception("KeyAttribute's oracleSequence is Null");
                var idp = propertyInfos[0];
                
                var r = connection.Query($"SELECT {oracleSequence}.NEXTVAL ID FROM DUAL", transaction: transaction, commandTimeout: commandTimeout);
                id = r.First().ID;
                if (id == null)
                    return 0;
                idp.SetValue(entityToInsert, Convert.ChangeType(id, idp.PropertyType), null);
                cmd = $"insert into {tableName} ({idp.Name},{columnList}) values ({id},{parameterList})";
            }
            if (isList)
            {
                return connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            }
            connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            return Convert.ToInt32(id);
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnAlias">The column alias.</param>
        public void AppendColumnName(StringBuilder sb, string columnName, string columnAlias)
        {
            if (string.IsNullOrEmpty(columnAlias) || columnName.Equals(columnAlias))
                sb.AppendFormat("{0}", columnName);
            else
                sb.AppendFormat("{0} as {1}", columnName, columnAlias);
        }

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnAlias">The column alias.</param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName, string columnAlias)
        {
            if (string.IsNullOrEmpty(columnAlias) || columnName.Equals(columnAlias))
                sb.AppendFormat("{0} = :{1}", columnName, columnName);
            else
                sb.AppendFormat("{0} = :{1}", columnName, columnAlias);
        }

        /// <summary>
        /// Adds parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="paramName">The parametr name.</param>
        public void AppendParametr(StringBuilder sb, string paramName)
        {
            sb.AppendFormat(":{0}", paramName);
        }

        public ISqlBuilder GetPageList<T>(IRepository<T> repository, int pageNum = 0, int pageSize = 0, string whereString = null, object param = null, object order = null, bool asc = false)
            where T : class, IEntity
        {
            return GetPageList<T>(null, repository, pageNum: pageNum, pageSize: pageSize, whereString: whereString, param: param, order: order, asc: asc);
        }
        public ISqlBuilder GetPageList<T>(string tableNameFormat, IRepository<T> repository, int pageNum = 0, int pageSize = 0, string whereString = null, object param = null, object order = null, bool asc = false)
          where T : class, IEntity
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            var sbColumnList = new StringBuilder(null);
            var allProperties = SqlMapperExtensions.TypePropertiesCache(typeof(T));
            var tableName = SqlMapperExtensions.GetTableName(repository.TableName, tableNameFormat);

            for (var i = 0; i < allProperties.Count; i++)
            {
                var property = allProperties[i];
                AppendColumnName(sbColumnList, SqlMapperExtensions.GetColumnName(property), property.Name);
                if (i < allProperties.Count - 1)
                    sbColumnList.Append(", ");
            }

            sqlBuilder.Select(args: sbColumnList.ToString());

            if (pageSize > 0)
            { 
                SqlBuilder sqlBuilderRows = new SqlBuilder();
                string ascSql = " asc";
                if (!asc)
                {
                    ascSql = " desc";
                }
                string orderSql = "";
                if (order == null)
                {
                    var type = typeof(T);
                    var keys = SqlMapperExtensions.KeyPropertiesCache(type);
                    orderSql = keys.Count > 0 ? SqlMapperExtensions.GetColumnName(keys[0]) : string.Empty;
                    if(string.IsNullOrEmpty(orderSql))
                        throw new Exception($"Need set table poco key [{tableName}]");
                }
                else
                {
                    orderSql = SqlBuilder.GetArgsString("ORDER BY", args: order);
                }

                sqlBuilderRows.Select(args: $"SELECT ROW_NUMBER() OVER(ORDER BY {orderSql}  {ascSql} ) AS row_id,it.*");
                sqlBuilderRows.From($"{tableName} it");
                if (!string.IsNullOrEmpty(whereString))
                {
                    sqlBuilderRows.Where(whereString, param);
                }
                sqlBuilder.Append($"From ({sqlBuilderRows.SQL}) TT", sqlBuilderRows.Arguments);

                if (pageNum <= 0)
                    pageNum = 1;
                int numMin = (pageNum - 1) * pageSize + 1, 
                    numMax = pageNum * pageSize;
                sqlBuilder.Where("TT.row_id between :numMin and :numMax", new { numMin, numMax });
            }
            else
            {
                sqlBuilder.From(tableName);
                if (!string.IsNullOrEmpty(whereString))
                {
                    sqlBuilder.Where(whereString, param);
                }
                if (order != null)
                {
                    sqlBuilder.OrderBy(order);
                    sqlBuilder.IsAse(asc);
                }
            }
            return sqlBuilder;
        }
    }
}
