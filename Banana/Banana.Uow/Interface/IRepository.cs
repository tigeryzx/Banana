﻿/***********************************
 * Developer: Lio.Huang
 * Date：2018-11-16
 * 
 * Last Update：2018-12-18
 * 2019-01-21  1.Current DB Setting
 **********************************/

using Banana.Uow.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Uow.Interface
{
    /// <summary>
    /// 仓储接口|The interface for all CURD operations 
    /// </summary> 
    public interface IRepository<T> : ISimpleRepository 
        where T : class, IEntity
    {
        #region Sync
        /// <summary>
        /// 插入实体|
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        long Insert(T entity);

        /// <summary>
        /// 插入实体|
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        long Insert(string tableNameFormat, T entity);

        /// <summary>
        /// 插入实体列表
        /// |Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="entityList">entity list</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        long Insert(IEnumerable<T> entityList);

        /// <summary>
        /// 插入实体列表
        /// |Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entityList">entity list</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        long Insert(string tableNameFormat, IEnumerable<T> entityList);

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        bool Update(T entity);

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        bool Update(string tableNameFormat, T entity);

        /// <summary>
        /// 删除实体|
        /// Delete entity in table "Ts".
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true if deleted, false if not found</returns>
        bool Delete(T entity);

        /// <summary>
        /// 删除实体|
        /// Delete entity in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>true if deleted, false if not found</returns>
        bool Delete(string tableNameFormat, T entity);

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        bool Delete(string whereString, object param);

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        bool Delete(string tableNameFormat, string whereString, object param);

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        bool DeleteAll();

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        bool DeleteAll(string tableNameFormat);

        /// <summary>
        /// 查询单个实体|
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key]/[ExplicitKey] attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <param name="id">Id of the entity to get, must be marked with [Key]/[ExplicitKey] attribute</param>
        /// <returns>Entity of T</returns>
        T Query(object id);

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
        T Query(string tableNameFormat, object id);

        /// <summary>
        /// 查询总数|
        /// Returns the number of rows
        /// </summary>
        /// <param name="whereString">where sql</param>
        /// <param name="param">param</param>
        /// <returns>number of rows</returns>
        int QueryCount(string whereString = null, object param = null);

        /// <summary>
        /// 查询总数|
        /// Returns the number of rows
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">where sql</param>
        /// <param name="param">param</param>
        /// <returns>number of rows</returns>
        int QueryCount(string tableNameFormat, string whereString = null, object param = null);

        /// <summary>
        /// 查询列表|
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="whereString">whereString,(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param> 
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>returning the data list typed as T.</returns>
        List<T> QueryList(string whereString = null, object param = null, string order = null, bool asc = false);

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
        List<T> QueryList(string tableNameFormat, string whereString = null, object param = null, string order = null, bool asc = false);

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
        IPage<T> QueryList(int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false);

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
        IPage<T> QueryList(string tableNameFormat, int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false);

        #endregion

        #region Async
        /// <summary>
        /// 插入实体|
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        Task<int> InsertAsync(T entity);

        /// <summary>
        /// 插入实体|
        /// Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>/// 
        /// <param name="entity">entity</param>
        /// <returns>返回自增Id|Identity of inserted entity.</returns>
        Task<int> InsertAsync(string tableNameFormat, T entity);

        /// <summary>
        /// 插入实体列表
        /// |Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="entityList">entity list</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        Task<int> InsertAsync(IEnumerable<T> entityList);

        /// <summary>
        /// 插入实体列表
        /// |Inserts an entity into table "Ts" and returns identity id or number of inserted rows if inserting a list.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entityList">entity list</param>
        /// <returns>返回受影响行数|number of inserted rows if inserting a list.</returns>
        Task<int> InsertAsync(string tableNameFormat, IEnumerable<T> entityList);

        /// <summary>
        /// 删除实体|
        /// Delete entity in table "Ts".
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true if deleted, false if not found</returns>
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// 删除实体|
        /// Delete entity in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>true if deleted, false if not found</returns>
        Task<bool> DeleteAsync(string tableNameFormat, T entity);

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        Task<bool> DeleteAsync(string whereString, object param);

        /// <summary>
        /// 删除|
        /// Delete data in table "Ts".
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">parameterized sql of "where",(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        Task<bool> DeleteAsync(string tableNameFormat, string whereString, object param);


        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        Task<bool> DeleteAllAsync();

        /// <summary>
        /// 删除全部|
        /// Delete all data
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        Task<bool> DeleteAllAsync(string tableNameFormat);

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// 更新|
        /// Updates entity in table "Ts", checks if the entity is modified if the entity is tracked by the Get() extension.
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="entity">entity</param>
        /// <returns>true if updated, false if not found or not modified (tracked entities)</returns>
        Task<bool> UpdateAsync(string tableNameFormat, T entity);

        /// <summary>
        /// 查询单个实体|
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key]/[ExplicitKey]  attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <param name="id">Id of the entity to get, must be marked with [Key]/[ExplicitKey]  attribute</param>
        /// <returns>Entity of T</returns>
        Task<T> QueryAsync(object id);

        /// <summary>
        /// 查询单个实体|
        /// Returns a single entity by a single id from table "Ts".  
        /// Id must be marked with [Key]/[ExplicitKey]  attribute.
        /// Entities created from interfaces are tracked/intercepted for changes and used by the Update() extension
        /// for optimal performance. 
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="id">Id of the entity to get, must be marked with [Key]/[ExplicitKey]  attribute</param>
        /// <returns>Entity of T</returns>
        Task<T> QueryAsync(string tableNameFormat, object id);

        /// <summary>
        /// 查询总数|
        /// Returns the number of rows
        /// </summary>
        /// <param name="whereString">where sql</param>
        /// <param name="param">param</param>
        /// <returns>number of rows</returns>
        Task<int> QueryCountAsync(string whereString = null, object param = null);

        /// <summary>
        /// 查询总数|
        /// Returns the number of rows
        /// </summary>
        /// <param name="tableNameFormat">Table Name Format placeholder</param>
        /// <param name="whereString">where sql</param>
        /// <param name="param">param</param>
        /// <returns>number of rows</returns>
        Task<int> QueryCountAsync(string tableNameFormat, string whereString = null, object param = null);

        /// <summary>
        /// 查询列表|
        /// Executes a query, returning the data typed as T.
        /// </summary>
        /// <param name="whereString">whereString,(example:whereString:name like @name)</param>
        /// <param name="param">whereString's param，(example:new { name = "google%" })</param> 
        /// <param name="order">order param,(example:order:"createTime")</param>
        /// <param name="asc">Is ascending</param>
        /// <returns>returning the data list typed as T.</returns>
        Task<IEnumerable<T>> QueryListAsync(string whereString = null, object param = null, string order = null, bool asc = false);

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
        Task<IEnumerable<T>> QueryListAsync(string tableNameFormat, string whereString = null, object param = null, string order = null, bool asc = false);

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
        Task<IPage<T>> QueryListAsync(int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false);

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
        Task<IPage<T>> QueryListAsync(string tableNameFormat, int pageNum, int pageSize, string whereString = null, object param = null, string order = null, bool asc = false);

        #endregion

        #region Field & method
        /// <summary>
        /// 表名|
        /// To get the name of the table
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// 对象类型|
        /// type of entity
        /// </summary>
        Type EntityType { get; }

        #endregion
    }
}
