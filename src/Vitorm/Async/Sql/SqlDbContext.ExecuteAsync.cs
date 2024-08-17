using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

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



    }
}
