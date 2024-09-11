using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Vitorm.Sql.Transaction;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        protected SqlExecutor sqlExecutor;
        public static int? defaultCommandTimeout;
        public int? commandTimeout;


        #region Transaction
        public virtual Func<SqlDbContext, ITransactionScope> createTransactionScope { set; get; }
                    = (dbContext) => new SqlTransactionScope(dbContext);
        protected virtual ITransactionScope transactionScope { get; set; }

        public virtual IDbTransaction BeginTransaction()
        {
            transactionScope ??= createTransactionScope(this);
            return transactionScope.BeginTransaction();
        }
        public virtual IDbTransaction GetCurrentTransaction() => transactionScope?.GetCurrentTransaction();

        #endregion



        #region Sync Method
        public virtual int ExecuteWithTransaction(string sql, IDictionary<string, object> param = null, IDbTransaction transaction = null)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.Execute(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
        }

        public virtual int Execute(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            this.Event_OnExecuting(sql, param);

            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return sqlExecutor.Execute(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return sqlExecutor.Execute(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        public virtual IDataReader ExecuteReader(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            this.Event_OnExecuting(sql, param);

            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return sqlExecutor.ExecuteReader(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return sqlExecutor.ExecuteReader(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        public virtual object ExecuteScalar(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            this.Event_OnExecuting(sql, param);

            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return sqlExecutor.ExecuteScalar(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return sqlExecutor.ExecuteScalar(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        #endregion



        #region Async Method

        public virtual async Task<int> ExecuteWithTransactionAsync(string sql, IDictionary<string, object> param = null, IDbTransaction transaction = null)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return await sqlExecutor.ExecuteAsync(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
        }

        public virtual async Task<int> ExecuteAsync(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return await sqlExecutor.ExecuteAsync(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return await sqlExecutor.ExecuteAsync(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        public virtual async Task<IDataReader> ExecuteReaderAsync(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return await sqlExecutor.ExecuteReaderAsync(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return await sqlExecutor.ExecuteReaderAsync(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        public virtual async Task<object> ExecuteScalarAsync(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return await sqlExecutor.ExecuteScalarAsync(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return await sqlExecutor.ExecuteScalarAsync(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        #endregion



    }
}
