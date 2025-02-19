﻿/***********************************
 * Developer: Lio.Huang
 * Create Date：2018-11-16
 * 
 * Last Update：2018-12-18
 * 2019-01-04  1.更新Query和QueryAsync 新增stringId
 * 2019-01-07  1.Add && _dbConnection.State!= ConnectionState.Connecting
 *             2.GetAdapter(_dbConnection)
 * 2019-01-11  1.Query(int) => Query(obj)
 * 2019-01-21  1.Current DB Setting
 * 2019-03-22  1.优化QueryListAsync
 *             2.InsertBatch Add open transaction's param
 * 2019-07-31  1.Fix bug Issues#7
 **********************************/

using System;
using System.Collections.Generic;
using Dapper;
using System.Data;
using System.Linq;
using Banana.Uow.Models;
using Banana.Uow.Interface;
using System.Threading.Tasks;
using Banana.Uow.Extension;

namespace Banana.Uow
{
    /// <summary>
    /// 仓储基类| Base Repository
    /// </summary>
    public class Repository<T> : SimpleRepository, IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// 仓储基类| Base Repository
        /// </summary>
        public Repository(string dbAliase = "")
        {
            this.dbAliase = dbAliase;
        }


        /// <summary>
        /// 仓储基类| Base Repository
        /// </summary>
        public Repository(IDbConnection dbConnection, IDbTransaction dbTransaction = null)
        {
            this._dbConnection = dbConnection;
            this._dbTransaction = dbTransaction;
        }

        #region Field & method

        /// <summary>
        /// 表名|
        /// To get the name of the table
        /// </summary>
        public string TableName
        {
            get
            {
                return SqlMapperExtensions.GetTableName(EntityType, null);
            }
        }

        /// <summary>
        /// 对象类型|
        /// type of entity
        /// </summary>
        public Type EntityType => typeof(T);
        #endregion

        #region Sync

        /// <summary>
        /// 删除实体|
        /// Delete entity in table "Ts".
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true if deleted, false if not found</returns>
        public bool Delete(T entity)
        {
            return Delete(null, entity);
        }

        /// <summary>
        /// 删除实体|
        /// Delete entity in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>true if deleted, false if not found</returns>
        public bool Delete(string tableNameFormat, T entity)
        {
            return DBConnection.Delete(entity, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 插入实体|
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        public long Insert(T entity)
        {
            return Insert(null, entity);
        }

        /// <summary>
        /// 插入实体|
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        public long Insert(string tableNameFormat,T entity)
        {
            this.OnEntityInsert(entity);
            return DBConnection.Insert(entity, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 插入实体列表
        /// |Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="entityList">entity list</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        public long Insert(IEnumerable<T> entityList)
        {
            return Insert(null, entityList);
        }

        /// <summary>
        /// 插入实体列表
        /// |Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entityList">entity list</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        public long Insert(string tableNameFormat, IEnumerable<T> entityList)
        {
            this.TriggerEntityListInsertHandle(entityList);
            return DBConnection.Insert(entityList, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 查询单个实体|
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key]/[ExplicitKey] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <param name="id">Id of the entity to get, must be marked with [Key]/[ExplicitKey] attribute</param>
        /// <returns>Entity of T</returns>
        public T Query(object id)
        {
            return Query(null, id);
        }

        /// <summary>
        /// 查询单个实体|
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key]/[ExplicitKey] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key]/[ExplicitKey] attribute</param>
        /// <returns>Entity of T</returns>
        public T Query(string tableNameFormat, object id)
        {
            return DBConnection.Get<T>(id, tableNameFormat, transaction: _dbTransaction);
        }

        /// <summary>
        /// 查询总数|
        /// Returns the number of rows
        /// </summary>
        /// <param name="whereString">where sql</param>
        /// <param name="param">param</param>
        /// <returns>number of rows</returns>
        public int QueryCount(string whereString = null, object param = null)
        {
            return QueryCount(null, whereString: whereString, param: param);
        }

        /// <summary>
        /// 查询总数|
        /// Returns the number of rows
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">where sql</param>
        /// <param name="param">param</param>
        /// <returns>number of rows</returns>
        public int QueryCount(string tableNameFormat, string whereString = null, object param = null)
        {
            SqlBuilder sb = new SqlBuilder();
            sb.Select(args: "Count(*)");
            sb.From(SqlMapperExtensions.GetTableName(TableName, tableNameFormat));
            if (!string.IsNullOrEmpty(whereString))
            {
                sb.Where(whereString, param);
            }
            return DBConnection.QueryFirst<int>(sb.ESQL, sb.Arguments, transaction: _dbTransaction);
        }

        /// <summary>
        /// 查询列表|
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="whereString">whereString,(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param> 
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>returning the data list typed as T.</returns>
        public List<T> QueryList(string whereString = null, object param = null, string order = null, bool asc = false)
        {
            return QueryList(null, whereString: whereString, param: param, order: order, asc: asc);
        }

        /// <summary>
        /// 查询列表|
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">whereString,(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param> 
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>returning the data list typed as T.</returns>
        public List<T> QueryList(string tableNameFormat, string whereString = null, object param = null, string order = null, bool asc = false)
        {
            if (string.IsNullOrEmpty(whereString) && string.IsNullOrEmpty(order))
            {
                return DBConnection.GetAll<T>(tableNameFormat, transaction: _dbTransaction).ToList();
            }
            else
            {
                ISqlAdapter adapter = ConnectionBuilder.GetAdapter(this.DBConnection);
                var sqlbuilder = adapter.GetPageList(tableNameFormat, this, whereString: whereString, param: param, order: order, asc: asc);
                return DBConnection.Query<T>(sqlbuilder.ESQL, sqlbuilder.Arguments, transaction: _dbTransaction).ToList();
            }
        }

        /// <summary>
        /// 分页查询|
        /// Executes a query, returning the paging data typed as T.
        /// </summary>
        /// <param name="pageNum">页码|page number</param>
        /// <param name="pageSize">页大小|page size</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>返回分页数据|returning the paging data typed as T</returns>
        public IPage<T> QueryList(int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false)
        {
            return QueryList(null, pageNum: pageNum, pageSize: pageSize, whereString: whereString, param: param, order: order, asc: asc);
        }

        /// <summary>
        /// 分页查询|
        /// Executes a query, returning the paging data typed as T.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="pageNum">页码|page number</param>
        /// <param name="pageSize">页大小|page size</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>返回分页数据|returning the paging data typed as T</returns>
        public IPage<T> QueryList(string tableNameFormat, int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false)
        {
            IPage<T> paging = new Paging<T>(pageNum, pageSize);
            ISqlAdapter adapter = ConnectionBuilder.GetAdapter(this.DBConnection);
            var sqlbuilder = adapter.GetPageList(tableNameFormat, this, pageNum, pageSize, whereString, param, order, asc);
            paging.data = DBConnection.Query<T>(sqlbuilder.ESQL, sqlbuilder.Arguments, transaction: _dbTransaction).ToList();
            paging.dataCount = QueryCount(whereString, param);
            return paging;
        }

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public bool Update(T entity)
        {
            return Update(null, entity);
        }

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        public bool Update(string tableNameFormat, T entity)
        {
            this.OnEntityUpdate(entity);
            return DBConnection.Update<T>(entity, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 批量插入数据|
        /// Execute insert SQL.
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="entities">entityList</param>
        /// <param name="openTransaction"></param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public virtual bool InsertBatch(string sql, IEnumerable<T> entities, bool openTransaction = true)
        {
            this.TriggerEntityListInsertHandle(entities);
            if (openTransaction)
            {
                using (IDbTransaction trans = OpenTransaction())
                {
                    try
                    {
                        int res = Execute(sql, entities);
                        TrancationState = ETrancationState.Closed;
                        if (res > 0)
                        {
                            trans.Commit();
                            return true;
                        }
                        else
                        {
                            trans.Rollback();
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        TrancationState = ETrancationState.Closed;
                        throw ex;
                    }
                }
            }
            else
            {
                return Execute(sql, entities) > 0;
            }
        }

     
        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public bool Delete(string whereString, object param)
        {
            return Delete(null, whereString: whereString, param: param);
        }

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public bool Delete(string tableNameFormat, string whereString, object param)
        {
            SqlBuilder sb = new SqlBuilder();
            sb.Append("DELETE FROM " + SqlMapperExtensions.GetTableName(TableName, tableNameFormat));
            sb.Where(whereString, param);
            return Execute(sb.ESQL, sb.Arguments) > 0;
        }
        #endregion

        #region Async

        /// <summary>
        /// 插入|
        /// Inserts an entity into table "Ts" asynchronously using .NET 4.5 Task and returns identity id.
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        public async Task<int> InsertAsync(T entity)
        {
            return await InsertAsync(null, entity);
        }

        /// <summary>
        /// 插入|
        /// Inserts an entity into table "Ts" asynchronously using .NET 4.5 Task and returns identity id.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">Entity to insert</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        public async Task<int> InsertAsync(string tableNameFormat,T entity)
        {
            return await DBConnection.InsertAsync(entity, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 插入实体列表|
        ///  Inserts an entity list into table "Ts" asynchronously using .NET 4.5 Task and returns identity id.
        /// </summary>
        /// <param name="entityList">Entity list to insert</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        public async Task<int> InsertAsync(IEnumerable<T> entityList)
        {
            return await InsertAsync(null, entityList);
        }

        /// <summary>
        /// 插入实体列表|
        ///  Inserts an entity list into table "Ts" asynchronously using .NET 4.5 Task and returns identity id.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entityList">Entity list to insert</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        public async Task<int> InsertAsync(string tableNameFormat, IEnumerable<T> entityList)
        {
            return await DBConnection.InsertAsync(entityList, tableNameFormat, _dbTransaction);
        }


        /// <summary>
        /// 删除|
        /// Delete entity in table "Ts" asynchronously using .NET 4.5 Task.
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <returns>是否成功|true if deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(T entity)
        {
            return await DeleteAsync(null, entity);
        }

        /// <summary>
        /// 删除|
        /// Delete entity in table "Ts" asynchronously using .NET 4.5 Task.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">Entity to delete</param>
        /// <returns>是否成功|true if deleted, false if not found</returns>
        public async Task<bool> DeleteAsync(string tableNameFormat, T entity)
        {
            return await DBConnection.DeleteAsync(entity, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts" asynchronously using .NET 4.5 Task, checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="entity">Entity to be updated</param>
        /// <returns>是否更新成功|true if updated, false if not found or not modified (tracked entities)</returns>
        public async Task<bool> UpdateAsync(T entity)
        {
            return await UpdateAsync(null, entity);
        }

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts" asynchronously using .NET 4.5 Task, checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">Entity to be updated</param>
        /// <returns>是否更新成功|true if updated, false if not found or not modified (tracked entities)</returns>
        public async Task<bool> UpdateAsync(string tableNameFormat, T entity)
        {
            this.OnEntityUpdate(entity);
            return await DBConnection.UpdateAsync(entity, tableNameFormat, _dbTransaction);
        }

        /// <summary>
        /// 查询|
        /// Returns a single entity by a single id from table "Ts" asynchronously using .NET 4.5 Task. T must be of interface type. 
        /// Id must be marked with [Key] attribute.
        /// Created entity is tracked/intercepted for changes and used by the Update() extension. 
        /// </summary>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <returns>返回实体|Entity of T</returns>
        public async Task<T> QueryAsync(object id)
        {
            return await QueryAsync(null, id);
        }

        /// <summary>
        /// 查询|
        /// Returns a single entity by a single id from table "Ts" asynchronously using .NET 4.5 Task. T must be of interface type. 
        /// Id must be marked with [Key] attribute.
        /// Created entity is tracked/intercepted for changes and used by the Update() extension. 
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key] attribute</param>
        /// <returns>返回实体|Entity of T</returns>
        public async Task<T> QueryAsync(string tableNameFormat, object id)
        {
            return await DBConnection.GetAsync<T>(id, tableNameFormat, transaction: _dbTransaction);
        }

        /// <summary>
        /// 异步查询总数|
        ///  Returns the number of rows
        /// </summary>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>总数|Returns the number of rows</returns>
        public async Task<int> QueryCountAsync(string whereString = null, object param = null)
        {
            return await QueryCountAsync(null, whereString: whereString, param: param);
        }

        /// <summary>
        /// 异步查询总数|
        ///  Returns the number of rows
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>/// 
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>总数|Returns the number of rows</returns>
        public async Task<int> QueryCountAsync(string tableNameFormat, string whereString = null, object param = null)
        {
            SqlBuilder sb = new SqlBuilder();
            sb.Select(args: "Count(*)");
            sb.From(SqlMapperExtensions.GetTableName(TableName, tableNameFormat));
            if (!string.IsNullOrEmpty(whereString))
            {
                sb.Where(whereString, param);
            }
            return await DBConnection.QueryFirstAsync<int>(sb.ESQL, sb.Arguments, transaction: _dbTransaction);
        }

        /// <summary>
        /// 查询列表|
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="whereString">whereString,(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param> 
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>returning the data list typed as T.</returns>
        public async Task<IEnumerable<T>> QueryListAsync(string whereString = null, object param = null, string order = null, bool asc = false)
        {
            return await QueryListAsync(null, whereString: whereString, param: param, order: order, asc: asc);
        }

        /// <summary>
        /// 查询列表|
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">whereString,(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param> 
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>returning the data list typed as T.</returns>
        public async Task<IEnumerable<T>> QueryListAsync(string tableNameFormat, string whereString = null, object param = null, string order = null, bool asc = false)
        {
            if (string.IsNullOrEmpty(whereString) && string.IsNullOrEmpty(order))
            {
                return await DBConnection.GetAllAsync<T>(transaction: _dbTransaction);
            }
            else
            {
                ISqlAdapter adapter = ConnectionBuilder.GetAdapter(this.DBConnection);
                var sqlbuilder = adapter.GetPageList(tableNameFormat, this, whereString: whereString, param: param, order: order, asc: asc);
                return await DBConnection.QueryAsync<T>(sqlbuilder.ESQL, sqlbuilder.Arguments, transaction: _dbTransaction);
            }
        }

        /// <summary>
        /// 分页查询|
        /// Executes a query, returning the paging data typed as T.
        /// </summary>
        /// <param name="pageNum">页码|page number</param>
        /// <param name="pageSize">页大小|page size</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>返回分页数据|returning the paging data typed as T</returns>
        public async Task<IPage<T>> QueryListAsync(int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false)
        {
            return await QueryListAsync(null, pageNum, pageSize, whereString: whereString, param: param, order: order, asc: asc);
        }

        /// <summary>
        /// 分页查询|
        /// Executes a query, returning the paging data typed as T.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="pageNum">页码|page number</param>
        /// <param name="pageSize">页大小|page size</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>返回分页数据|returning the paging data typed as T</returns>
        public async Task<IPage<T>> QueryListAsync(string tableNameFormat, int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false)
        {
            IPage<T> paging = new Paging<T>(pageNum, pageSize);
            ISqlAdapter adapter = ConnectionBuilder.GetAdapter(this.DBConnection);
            var sqlbuilder = adapter.GetPageList(tableNameFormat, this, pageNum, pageSize, whereString, param, order, asc);
            var data = await DBConnection.QueryAsync<T>(sqlbuilder.ESQL, sqlbuilder.Arguments, transaction: _dbTransaction);
            var dataCount = await QueryCountAsync(whereString, param);
            paging.data = data.ToList();
            paging.dataCount = dataCount;
            return paging;
        }

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        public bool DeleteAll()
        {
            return DeleteAll(null);
        }

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        public bool DeleteAll(string tableNameFormat)
        {
            return DBConnection.DeleteAll<T>(tableNameFormat, transaction: _dbTransaction);
        }

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        public async Task<bool> DeleteAllAsync()
        {
            return await DeleteAllAsync(null);
        }

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        public async Task<bool> DeleteAllAsync(string tableNameFormat)
        {
            return await DBConnection.DeleteAllAsync<T>(tableNameFormat, transaction: _dbTransaction);
        }

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public async Task<bool> DeleteAsync(string whereString, object param)
        {
            return await DeleteAsync(null, whereString: whereString, param: param);
        }

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public async Task<bool> DeleteAsync(string tableNameFormat, string whereString, object param)
        {
            SqlBuilder sb = new SqlBuilder();
            sb.Append("DELETE FROM " + SqlMapperExtensions.GetTableName(TableName, tableNameFormat));
            sb.Where(whereString, param);
            return await ExecuteAsync(sb.ESQL, sb.Arguments) > 0;
        }
        #endregion


        private void TriggerEntityListInsertHandle(IEnumerable<T> entities)
        {
            if (entities != null && entities.Count() > 0)
            {
                foreach (var item in entities)
                    this.OnEntityInsert(item);
            }
        }

        /// <summary>
        /// 实体新增事件
        /// </summary>
        /// <param name="entity">entity</param>
        protected virtual void OnEntityInsert(T entity)
        {

        }

        /// <summary>
        /// 实体更新事件
        /// </summary>
        /// <param name="entity">entity</param>
        protected virtual void OnEntityUpdate(T entity)
        {

        }

        public T QuerySingleOrDefault(string whereString = null, object param = null)
        {
            return this.QuerySingleOrDefault(null, whereString: whereString, param: param);
        }

        public T QuerySingleOrDefault(string tableNameFormat, string whereString = null, object param = null)
        {
            ISqlAdapter adapter = ConnectionBuilder.GetAdapter(this.DBConnection);
            var sqlbuilder = adapter.GetPageList(tableNameFormat, this, whereString: whereString, param: param);
            return DBConnection.QuerySingleOrDefault<T>(sqlbuilder.ESQL, sqlbuilder.Arguments, transaction: _dbTransaction);
        }

        public T QuerySingle(string whereString = null, object param = null)
        {
            return this.QuerySingle(null, whereString, param);
        }

        public T QuerySingle(string tableNameFormat, string whereString = null, object param = null)
        {
            ISqlAdapter adapter = ConnectionBuilder.GetAdapter(this.DBConnection);
            var sqlbuilder = adapter.GetPageList(tableNameFormat, this, whereString: whereString, param: param);
            return DBConnection.QuerySingle<T>(sqlbuilder.ESQL, sqlbuilder.Arguments, transaction: _dbTransaction);
        }
    }
}
