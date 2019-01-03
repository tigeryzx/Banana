﻿/***********************************
 * Coder：EminemJK
 * Date：2018-12-07
 * 
 * UpdateDate:
 * 2018-12-28  1.更新GetPageList中的Select *  => Select {ColumnList}
 * 2019-01-03  1.更新GetPageList中的property.Name => SqlMapperExtensions.GetColumnAlias(property)
 *             2.更新AppendColumnName、AppendColumnNameEqualsValue 新增别名
 **********************************/

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

namespace Banana.Uow.Extension
{
    /// <summary>
    /// Postgres分页扩展
    /// </summary>
    internal partial class PostgresAdapter : ISqlAdapter
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
            string cmd = "";
            if (isList)
            {
                cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
                return await connection.ExecuteAsync(cmd, entityToInsert, transaction, commandTimeout);
            }
            var sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2})", tableName, columnList, parameterList);

            // If no primary key then safe to assume a join table with not too much data to return
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (propertyInfos.Length == 0)
            {
                sb.Append(" RETURNING *");
            }
            else
            {
                sb.Append(" RETURNING ");
                bool first = true;
                foreach (var property in propertyInfos)
                {
                    if (!first)
                        sb.Append(", ");
                    first = false;
                    sb.Append(property.Name);
                }
            }

            var results = await connection.QueryAsync(sb.ToString(), entityToInsert, transaction, commandTimeout).ConfigureAwait(false);

            // Return the key by assinging the corresponding property in the object - by product is that it supports compound primary keys
            var id = 0;
            foreach (var p in propertyInfos)
            {
                var value = ((IDictionary<string, object>)results.First())[p.Name.ToLower()];
                p.SetValue(entityToInsert, value, null);
                if (id == 0)
                    id = Convert.ToInt32(value);
            }
            return id;
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
            string cmd = "";
            if (isList)
            {
                cmd = $"insert into {tableName} ({columnList}) values ({parameterList})";
                return connection.Execute(cmd, entityToInsert, transaction, commandTimeout);
            }
            var sb = new StringBuilder();
            sb.AppendFormat("insert into {0} ({1}) values ({2})", tableName, columnList, parameterList);

            // If no primary key then safe to assume a join table with not too much data to return
            var propertyInfos = keyProperties as PropertyInfo[] ?? keyProperties.ToArray();
            if (propertyInfos.Length == 0)
            {
                sb.Append(" RETURNING *");
            }
            else
            {
                sb.Append(" RETURNING ");
                var first = true;
                foreach (var property in propertyInfos)
                {
                    if (!first)
                        sb.Append(", ");
                    first = false;
                    sb.Append(property.Name);
                }
            }

            var results = connection.Query(sb.ToString(), entityToInsert, transaction, commandTimeout: commandTimeout).ToList();

            // Return the key by assinging the corresponding property in the object - by product is that it supports compound primary keys
            var id = 0;
            foreach (var p in propertyInfos)
            {
                var value = ((IDictionary<string, object>)results[0])[p.Name.ToLower()];
                p.SetValue(entityToInsert, value, null);
                if (id == 0)
                    id = Convert.ToInt32(value);
            }
            return id;
        }

        /// <summary>
        /// Adds the name of a column.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnAlias">The column alias.</param>
        public void AppendColumnName(StringBuilder sb, string columnName, string columnAlias = "")
        {
            if (string.IsNullOrEmpty(columnAlias) || columnName.Equals(columnAlias))
                sb.AppendFormat("\"{0}\"", columnName);
            else
                sb.AppendFormat("\"{0}\" as {1}", columnName, columnAlias);
        }

        /// <summary>
        /// Adds a column equality to a parameter.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="columnAlias">The column alias.</param>
        public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName, string columnAlias = "")
        {
            if (string.IsNullOrEmpty(columnAlias) || columnName.Equals(columnAlias))
                sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
            else
                sb.AppendFormat("\"{0}\" = @{1}", columnName, columnAlias);
        }

        /// <summary>
        /// Adds the parametr to sql.
        /// </summary>
        /// <param name="sb">The string builder  to append to.</param>
        /// <param name="paramName">The column name.</param>
        public void AppendParametr(StringBuilder sb, string paramName)
        {
            sb.AppendFormat("@{0}", paramName);
        }

        /// <summary>
        /// Postgres分页扩展
        /// </summary>
        public PostgresAdapter() { }

        public ISqlBuilder GetPageList<T>(IRepository<T> repository, int pageNum = 0, int pageSize = 0, string whereString = null, object param = null, object order = null, bool asc = false)
            where T : class, IEntity
        {
            SqlBuilder sqlBuilder = new SqlBuilder();
            var sbColumnList = new StringBuilder(null);
            var allProperties = SqlMapperExtensions.TypePropertiesCache(typeof(T));
            for (var i = 0; i < allProperties.Count; i++)
            {
                var property = allProperties[i];
                AppendColumnName(sbColumnList, SqlMapperExtensions.GetColumnAlias(property), property.Name);
                if (i < allProperties.Count - 1)
                    sbColumnList.Append(", ");
            }

            sqlBuilder.Select(args: sbColumnList.ToString());
            sqlBuilder.From(repository.TableName);

            if (!string.IsNullOrEmpty(whereString))
            {
                sqlBuilder.Where(whereString, param);
            }
            if (order != null)
            {
                sqlBuilder.OrderBy(order);
                sqlBuilder.IsAse(asc);
            }

            if (pageNum >= 0 && pageSize > 0)
            {
                int numMin = (pageNum - 1) * pageSize;
                sqlBuilder.Append($" limit {pageSize} offset {numMin}");
            }
            return sqlBuilder;
        }
    }
}
