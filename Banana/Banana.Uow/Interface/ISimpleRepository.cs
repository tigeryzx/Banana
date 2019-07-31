using Banana.Uow.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banana.Uow.Interface
{
    /// <summary>
    /// 与实体无关的仓储接口|The interface for simple operations 
    /// </summary> 
    public interface ISimpleRepository
    {
        #region Sync
     
        /// <summary>
        /// 执行单条语句
        /// |Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">parameterized SQL</param>
        /// <param name="parms">The parameters to use for this query.</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        int Execute(string sql, dynamic parms = null);

        #endregion

        #region Async

        /// <summary>
        /// 执行单条语句
        /// |Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">parameterized SQL</param>
        /// <param name="parms">The parameters to use for this query.</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        Task<int> ExecuteAsync(string sql, dynamic parms = null);
        #endregion

        #region Field & method

        /// <summary>
        /// DBConnection
        /// </summary>
        IDbConnection DBConnection { get; }

        /// <summary>
        /// 开启事务|
        /// Open transaction
        /// </summary>
        IDbTransaction OpenTransaction();

        /// <summary>
        /// 事务状态|
        /// transaction's state
        /// </summary>
        ETrancationState TrancationState { get; }

        /// <summary>
        /// Current repository's DBSetting
        /// </summary>
        DBSetting CurrentDBSetting { get; }
        #endregion
    }
}
