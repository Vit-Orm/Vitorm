using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using DbCommand = System.Data.Common.DbCommand;
using DbTransaction = System.Data.Common.DbTransaction;

namespace Vitorm.Sql.SqlExecute
{
    public partial class SqlExecutor
    {

        public Func<DbConnection, Task> CloseAsync = (DbConnection conn) => { conn.Close(); return Task.CompletedTask; };

        public virtual Task<int> ExecuteAsync(IDbConnection connection, string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, bool isProcedure = false)
         => ExecuteAsync(new(connection, sql, parameters, transaction, commandTimeout, commandType: isProcedure ? CommandType.StoredProcedure : null));

        public virtual async Task<int> ExecuteAsync(ExecuteArgument arg)
        {
            if (arg.connection is DbConnection connection)
            {
                // #1 setup command
                using var cmd = connection.CreateCommand();
                cmd.Connection = connection;

                if (arg.transaction != null) cmd.Transaction = (DbTransaction)arg.transaction;
                if (arg.commandTimeout.HasValue) cmd.CommandTimeout = arg.commandTimeout.Value;

                if (arg.commandType.HasValue) cmd.CommandType = arg.commandType.Value;
                cmd.CommandText = arg.text;

                AddParameters(cmd, arg.parameters);


                // #2 execute
                bool wasClosed = connection.State == ConnectionState.Closed;
                try
                {
                    if (wasClosed) await connection.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    if (wasClosed) await CloseAsync(connection);
                }
            }

            return await Task.Run(() => Execute(arg));
        }

        public virtual Task<object> ExecuteScalarAsync(IDbConnection connection, string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, bool isProcedure = false)
            => ExecuteScalarAsync(new(connection, sql, parameters, transaction, commandTimeout, commandType: isProcedure ? CommandType.StoredProcedure : null));
        public virtual async Task<object> ExecuteScalarAsync(ExecuteArgument arg)
        {
            if (arg.connection is DbConnection connection)
            {
                // #1 setup command
                using var cmd = connection.CreateCommand();
                cmd.Connection = connection;

                if (arg.transaction != null) cmd.Transaction = (DbTransaction)arg.transaction;
                if (arg.commandTimeout.HasValue) cmd.CommandTimeout = arg.commandTimeout.Value;

                if (arg.commandType.HasValue) cmd.CommandType = arg.commandType.Value;
                cmd.CommandText = arg.text;

                AddParameters(cmd, arg.parameters);

                // #2 execute
                bool wasClosed = connection.State == ConnectionState.Closed;
                try
                {
                    if (wasClosed) await connection.OpenAsync();
                    return await cmd.ExecuteScalarAsync();
                }
                finally
                {

                    if (wasClosed) await CloseAsync(connection);
                }
            }

            return await Task.Run(() => ExecuteScalar(arg));
        }

        public Task<IDataReader> ExecuteReaderAsync(IDbConnection connection, string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, bool isProcedure = false)
             => ExecuteReaderAsync(new(connection, sql, parameters, transaction, commandTimeout, commandType: isProcedure ? CommandType.StoredProcedure : null));
        public virtual async Task<IDataReader> ExecuteReaderAsync(ExecuteArgument arg)
        {
            if (arg.connection is DbConnection connection)
            {
                DbCommand cmd = null;

                bool wasClosed = connection.State == ConnectionState.Closed, disposeCommand = true;
                try
                {
                    // #1 setup command
                    cmd = connection.CreateCommand();
                    cmd.Connection = connection;

                    if (arg.transaction != null) cmd.Transaction = (DbTransaction)arg.transaction;
                    if (arg.commandTimeout.HasValue) cmd.CommandTimeout = arg.commandTimeout.Value;

                    if (arg.commandType.HasValue) cmd.CommandType = arg.commandType.Value;
                    cmd.CommandText = arg.text;

                    AddParameters(cmd, arg.parameters);

                    // #2 execute
                    var commandBehavior = wasClosed ? CommandBehavior.CloseConnection : CommandBehavior.Default;
                    if (wasClosed) await connection.OpenAsync();

                    var reader = await cmd.ExecuteReaderAsync(commandBehavior);

                    wasClosed = false; // don't dispose before giving it to them!
                    disposeCommand = false;
                    return reader;
                }
                finally
                {
                    if (wasClosed) await CloseAsync(connection);

                    if (disposeCommand)
                    {
                        //cmd.Parameters.Clear();
                        cmd.Dispose();
                    }
                }
            }

            return await Task.Run(() => ExecuteReader(arg));
        }

    }
}
