using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Vitorm.Sql.Transaction;
using Vitorm.Transaction;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        public override void Dispose()
        {
            try
            {
                transactionManager?.Dispose();
            }
            finally
            {
                transactionManager = null;
                try
                {
                    _dbConnection?.Dispose();
                }
                finally
                {
                    _dbConnection = null;

                    try
                    {
                        _readOnlyDbConnection?.Dispose();
                    }
                    finally
                    {
                        _readOnlyDbConnection = null;

                        base.Dispose();
                    }
                }
            }
        }




        /// <summary>
        /// to identify whether contexts are from the same database
        /// </summary>
        public virtual string dbGroupName => "SqlDbSet_" + dbConnectionProvider.dbHashCode;


        #region dbConnection

        protected DbConnectionProvider dbConnectionProvider;
        protected IDbConnection _dbConnection;
        protected IDbConnection _readOnlyDbConnection;
        public virtual IDbConnection dbConnection => _dbConnection ??= dbConnectionProvider.CreateDbConnection();
        public virtual IDbConnection readOnlyDbConnection
            => _readOnlyDbConnection ??
                (dbConnectionProvider.ableToCreateReadOnly ? (_readOnlyDbConnection = dbConnectionProvider.CreateReadOnlyDbConnection()) : dbConnection);

        #endregion



        public virtual string databaseName => dbConnectionProvider.databaseName;
        public virtual void ChangeDatabase(string databaseName)
        {
            if (_dbConnection != null || _readOnlyDbConnection != null) throw new InvalidOperationException("can not change database after connected, please try in an new DbContext.");

            dbConnectionProvider = dbConnectionProvider.WithDatabase(databaseName);
        }




        protected SqlExecutor sqlExecutor;
        public static int? defaultCommandTimeout;
        public int? commandTimeout;


        #region Transaction
        public virtual Func<SqlDbContext, ITransactionManager> createTransactionManager { set; get; }
                    = (dbContext) => new SqlTransactionManager(dbContext);
        protected virtual ITransactionManager transactionManager { get; set; }

        public override ITransaction BeginTransaction()
        {
            transactionManager ??= createTransactionManager(this);
            return transactionManager.BeginTransaction();
        }
        public virtual IDbTransaction GetDbTransaction() => (transactionManager as SqlTransactionManager)?.GetDbTransaction();

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
            var transaction = GetDbTransaction();

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
            var transaction = GetDbTransaction();

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
            var transaction = GetDbTransaction();

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
            var transaction = GetDbTransaction();

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
            var transaction = GetDbTransaction();

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
            var transaction = GetDbTransaction();

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
