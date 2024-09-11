using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using DbCommand = System.Data.Common.DbCommand;
using DbTransaction = System.Data.Common.DbTransaction;
namespace Vitorm.Sql
{

    public partial class SqlExecutor
    {

        public Func<DbConnection, Task> CloseAsync = (DbConnection conn) => { conn.Close(); return Task.CompletedTask; };


        public virtual async Task<int> ExecuteAsync(IDbConnection _conn, string sql, IDictionary<string, object> param = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (_conn is DbConnection conn)
            {
                // #1 setup command
                using var cmd = conn.CreateCommand();
                if (transaction != null) cmd.Transaction = (DbTransaction)transaction;
                if (commandTimeout.HasValue) cmd.CommandTimeout = commandTimeout.Value;
                cmd.Connection = conn;
                cmd.CommandText = sql;
                AddParameter(cmd, param);


                // #2 execute
                bool wasClosed = conn.State == ConnectionState.Closed;
                try
                {
                    if (wasClosed) await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    if (wasClosed) await CloseAsync(conn);
                }
            }

            return await Task.Run(() => Execute(_conn, sql, param: param, transaction: transaction, commandTimeout: commandTimeout));
        }

        public virtual async Task<object> ExecuteScalarAsync(IDbConnection _conn, string sql, IDictionary<string, object> param = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (_conn is DbConnection conn)
            {
                // #1 setup command
                using var cmd = conn.CreateCommand();
                if (transaction != null) cmd.Transaction = (DbTransaction)transaction;
                if (commandTimeout.HasValue) cmd.CommandTimeout = commandTimeout.Value;
                cmd.Connection = conn;
                cmd.CommandText = sql;
                AddParameter(cmd, param);

                // #2 execute
                bool wasClosed = conn.State == ConnectionState.Closed;
                try
                {
                    if (wasClosed) await conn.OpenAsync();
                    return await cmd.ExecuteScalarAsync();
                }
                finally
                {

                    if (wasClosed) await CloseAsync(conn);
                }
            }

            return await Task.Run(() => ExecuteScalar(_conn, sql, param: param, transaction: transaction, commandTimeout: commandTimeout));
        }

        public virtual async Task<IDataReader> ExecuteReaderAsync(IDbConnection _conn, string sql, IDictionary<string, object> param = null, IDbTransaction transaction = null, int? commandTimeout = null)
        {
            if (_conn is DbConnection conn)
            {
                DbCommand cmd = null;

                bool wasClosed = conn.State == ConnectionState.Closed, disposeCommand = true;
                try
                {
                    // #1 setup command
                    cmd = conn.CreateCommand();
                    if (transaction != null) cmd.Transaction = (DbTransaction)transaction;
                    if (commandTimeout.HasValue) cmd.CommandTimeout = commandTimeout.Value;
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    AddParameter(cmd, param);

                    // #2 execute
                    var commandBehavior = wasClosed ? CommandBehavior.CloseConnection : CommandBehavior.Default;
                    if (wasClosed) await conn.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync(commandBehavior);

                    wasClosed = false; // don't dispose before giving it to them!
                    disposeCommand = false;
                    return reader;
                }
                finally
                {
                    if (wasClosed) await CloseAsync(conn);

                    if (disposeCommand)
                    {
                        //cmd.Parameters.Clear();
                        cmd.Dispose();
                    }
                }
            }

            return await Task.Run(() => ExecuteReader(_conn, sql, param: param, transaction: transaction, commandTimeout: commandTimeout));
        }

    }
}
