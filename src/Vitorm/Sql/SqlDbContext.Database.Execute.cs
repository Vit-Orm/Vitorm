using System.Collections.Generic;
using System.Data;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        #region Execute
        protected SqlExecutor sqlExecutor;
        public static int? defaultCommandTimeout;
        public int? commandTimeout;

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



    }
}
