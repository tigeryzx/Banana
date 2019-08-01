using Banana.Uow.Interface;
using Banana.Uow.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Banana.Uow
{
    /// <summary>
    /// 与实体无关仓储基类| Simple Repository
    /// </summary>
    public class SimpleRepository : ISimpleRepository
    {
        /// <summary>
        /// 仓储基类| Base Repository
        /// </summary>
        public SimpleRepository(string dbAliase = "")
        {
            this.dbAliase = dbAliase;
        }


        /// <summary>
        /// 仓储基类| Base Repository
        /// </summary>
        public SimpleRepository(IDbConnection dbConnection, IDbTransaction dbTransaction = null)
        {
            this._dbConnection = dbConnection;
            this._dbTransaction = dbTransaction;
        }

        #region Field & method

        /// <summary>
        /// 数据库别名
        /// </summary>
        protected string dbAliase;

        /// <summary>
        /// CurrentDB Setting
        /// </summary>
        public DBSetting CurrentDBSetting => ConnectionBuilder.GetDBSetting(dbAliase);

        /// <summary>
        /// 数据库连接
        /// </summary>
        protected IDbConnection _dbConnection;

        /// <summary>
        /// IDbConnection
        /// </summary>
        public IDbConnection DBConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    _dbConnection = ConnectionBuilder.CreateConnection(dbAliase);
                }
                if (_dbConnection.State == ConnectionState.Closed && _dbConnection.State != ConnectionState.Connecting)
                {
                    _dbConnection.Open();
                }
                return _dbConnection;
            }
            private set { this._dbConnection = value; }
        }

        /// <summary>
        /// 数据库事务
        /// </summary>
        protected IDbTransaction _dbTransaction;

        /// <summary>
        /// 开启事务|
        /// Open transaction
        /// </summary>
        public IDbTransaction OpenTransaction()
        {
            if (TrancationState == ETrancationState.Closed)
                _dbTransaction = DBConnection.BeginTransaction();
            TrancationState = ETrancationState.Opened;
            return _dbTransaction;
        }

        /// <summary>
        /// 事务状态|
        /// transaction's state
        /// </summary>
        public ETrancationState TrancationState { get; protected set; } = ETrancationState.Closed;

        #endregion

        #region Sync

        /// <summary>
        /// 执行单条语句
        /// |Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">parameterized SQL</param>
        /// <param name="parms">The parameters to use for this query.</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public int Execute(string sql, dynamic parms)
        {
            return DBConnection.Execute(sql, (object)parms, transaction: _dbTransaction);
        }
        #endregion

        #region Async

        /// <summary>
        /// 执行单条语句
        /// |Execute parameterized SQL.
        /// </summary>
        /// <param name="sql">parameterized SQL</param>
        /// <param name="parms">The parameters to use for this query.</param>
        /// <returns>受影响的行数|The number of rows affected.</returns>
        public async Task<int> ExecuteAsync(string sql, dynamic parms)
        {
            return await DBConnection.ExecuteAsync(sql, (object)parms, transaction: _dbTransaction);
        }
        #endregion
    }
}
