using System;
using System.Collections.Generic;
using System.Data;

namespace Vitorm.Sql.SqlExecute
{
    public partial class SqlExecutor
    {
        public readonly static SqlExecutor Instance = new();


        public virtual int Execute(IDbConnection connection, string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, bool isProcedure = false)
            => Execute(new(connection, sql, parameters, transaction, commandTimeout, commandType: isProcedure ? CommandType.StoredProcedure : null));

        public virtual int Execute(ExecuteArgument arg)
        {
            // #1 setup command
            using var cmd = arg.connection.CreateCommand();
            cmd.Connection = arg.connection;

            if (arg.transaction != null) cmd.Transaction = arg.transaction;
            if (arg.commandTimeout.HasValue) cmd.CommandTimeout = arg.commandTimeout.Value;

            if (arg.commandType.HasValue) cmd.CommandType = arg.commandType.Value;
            cmd.CommandText = arg.text;

            AddParameters(cmd, arg.parameters);

            // #2 execute
            bool wasClosed = arg.connection.State == ConnectionState.Closed;
            try
            {
                if (wasClosed) arg.connection.Open();
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                if (wasClosed) arg.connection.Close();
            }

        }



        public virtual object ExecuteScalar(IDbConnection connection, string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, bool isProcedure = false)
            => ExecuteScalar(new(connection, sql, parameters, transaction, commandType: isProcedure ? CommandType.StoredProcedure : null));

        public virtual object ExecuteScalar(ExecuteArgument arg)
        {
            // #1 setup command
            using var cmd = arg.connection.CreateCommand();
            cmd.Connection = arg.connection;

            if (arg.transaction != null) cmd.Transaction = arg.transaction;
            if (arg.commandTimeout.HasValue) cmd.CommandTimeout = arg.commandTimeout.Value;

            if (arg.commandType.HasValue) cmd.CommandType = arg.commandType.Value;
            cmd.CommandText = arg.text;

            AddParameters(cmd, arg.parameters);

            // #2 execute
            bool wasClosed = arg.connection.State == ConnectionState.Closed;
            try
            {
                if (wasClosed) arg.connection.Open();
                return cmd.ExecuteScalar();
            }
            finally
            {
                if (wasClosed) arg.connection.Close();
            }
        }



        public virtual IDataReader ExecuteReader(IDbConnection connection, string sql, IDictionary<string, object> parameters = null, IDbTransaction transaction = null, int? commandTimeout = null, bool isProcedure = false)
            => ExecuteReader(new(connection, sql, parameters, transaction, commandTimeout, commandType: isProcedure ? CommandType.StoredProcedure : null));

        public virtual IDataReader ExecuteReader(ExecuteArgument arg)
        {
            IDbCommand cmd = null;

            bool wasClosed = arg.connection.State == ConnectionState.Closed, disposeCommand = true;
            try
            {
                // #1 setup command
                cmd = arg.connection.CreateCommand();
                cmd.Connection = arg.connection;

                if (arg.transaction != null) cmd.Transaction = arg.transaction;
                if (arg.commandTimeout.HasValue) cmd.CommandTimeout = arg.commandTimeout.Value;

                if (arg.commandType.HasValue) cmd.CommandType = arg.commandType.Value;
                cmd.CommandText = arg.text;

                AddParameters(cmd, arg.parameters);

                // #2 execute
                var commandBehavior = wasClosed ? CommandBehavior.CloseConnection : CommandBehavior.Default;
                if (wasClosed) arg.connection.Open();

                var reader = cmd.ExecuteReader(commandBehavior);
                wasClosed = false; // don't dispose before giving it to them!
                disposeCommand = false;
                return reader;
            }
            finally
            {
                if (wasClosed) arg.connection.Close();
                if (disposeCommand)
                {
                    //cmd.Parameters.Clear();
                    cmd.Dispose();
                }
            }
        }


        public virtual void AddParameters(IDbCommand cmd, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = parameter.Key;
                    p.Value = parameter.Value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }
            }
        }
    }
}
