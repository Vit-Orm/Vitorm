using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Vitorm.Sql.SqlExecute;
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
        public virtual int ExecuteWithTransaction(string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null)
        {
            transaction ??= GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.Execute(dbConnection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }

        public virtual int Execute(ExecuteArgument arg, bool useReadOnly = false)
        {
            arg.connection ??= useReadOnly ? readOnlyDbConnection : dbConnection;
            arg.transaction ??= GetDbTransaction();
            arg.commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.Execute(arg);
        }
        public virtual int Execute(string sql, IDictionary<string, object> parameters = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            this.Event_OnExecuting(sql, parameters);

            var connection = useReadOnly ? readOnlyDbConnection : dbConnection;
            var transaction = GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.Execute(dbConnection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }




        public virtual IDataReader ExecuteReader(ExecuteArgument arg, bool useReadOnly = false)
        {
            arg.connection ??= useReadOnly ? readOnlyDbConnection : dbConnection;
            arg.transaction ??= GetDbTransaction();
            arg.commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteReader(arg);
        }
        public virtual IDataReader ExecuteReader(string sql, IDictionary<string, object> parameters = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            this.Event_OnExecuting(sql, parameters);

            var connection = useReadOnly ? readOnlyDbConnection : dbConnection;
            var transaction = GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteReader(dbConnection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }


        public virtual object ExecuteScalar(ExecuteArgument arg, bool useReadOnly = false)
        {
            arg.connection ??= useReadOnly ? readOnlyDbConnection : dbConnection;
            arg.transaction ??= GetDbTransaction();
            arg.commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteScalar(arg);
        }
        public virtual object ExecuteScalar(string sql, IDictionary<string, object> parameters = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            this.Event_OnExecuting(sql, parameters);

            var connection = useReadOnly ? readOnlyDbConnection : dbConnection;
            var transaction = GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteScalar(connection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }

        #endregion



        #region Async Method

        public virtual Task<int> ExecuteAsync(ExecuteArgument arg, bool useReadOnly = false)
        {
            arg.connection ??= useReadOnly ? readOnlyDbConnection : dbConnection;
            arg.transaction ??= GetDbTransaction();
            arg.commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteAsync(arg);
        }

        public virtual async Task<int> ExecuteAsync(string sql, IDictionary<string, object> parameters = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            var connection = useReadOnly ? readOnlyDbConnection : dbConnection;
            var transaction = GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return await sqlExecutor.ExecuteAsync(connection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }


        public virtual Task<IDataReader> ExecuteReaderAsync(ExecuteArgument arg, bool useReadOnly = false)
        {
            arg.connection ??= useReadOnly ? readOnlyDbConnection : dbConnection;
            arg.transaction ??= GetDbTransaction();
            arg.commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteReaderAsync(arg);
        }
        public virtual async Task<IDataReader> ExecuteReaderAsync(string sql, IDictionary<string, object> parameters = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            var connection = useReadOnly ? readOnlyDbConnection : dbConnection;
            var transaction = GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return await sqlExecutor.ExecuteReaderAsync(connection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }


        public virtual Task<object> ExecuteScalarAsync(ExecuteArgument arg, bool useReadOnly = false)
        {
            arg.connection ??= useReadOnly ? readOnlyDbConnection : dbConnection;
            arg.transaction ??= GetDbTransaction();
            arg.commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.ExecuteScalarAsync(arg);
        }
        public virtual async Task<object> ExecuteScalarAsync(string sql, IDictionary<string, object> parameters = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            var connection = useReadOnly ? readOnlyDbConnection : dbConnection;
            var transaction = GetDbTransaction();
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return await sqlExecutor.ExecuteScalarAsync(connection, sql, parameters: parameters, transaction: transaction, commandTimeout: commandTimeout);
        }

        #endregion



    }
}
