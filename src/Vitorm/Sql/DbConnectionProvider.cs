using System;
using System.Data;

namespace Vitorm.Sql
{
    public class DbConnectionProvider
    {
        protected DbConnectionProvider() { }
        public DbConnectionProvider(
            Func<string, IDbConnection> createDbConnection,
            Func<string, string, string> changeDatabaseForConnectionString,
            Func<string, string> getDatabaseName,
            string connectionString,
            string readOnlyConnectionString
            )
        {
            this.createDbConnection = createDbConnection;
            this.changeDatabaseForConnectionString = changeDatabaseForConnectionString;
            this.getDatabaseName = getDatabaseName;
            this.connectionString = connectionString;
            this.readOnlyConnectionString = readOnlyConnectionString;
        }


        protected readonly string connectionString;
        protected readonly string readOnlyConnectionString;

        protected Func<string, IDbConnection> createDbConnection;

        /// <summary>
        ///  (string connectionString, string databaseName) => (string connectionStringWithNewDatabase)
        /// </summary>
        protected Func<string, string, string> changeDatabaseForConnectionString;

        /// <summary>
        ///  (string connectionString) => (string databaseName)
        /// </summary>
        protected Func<string, string> getDatabaseName;

        public virtual bool ableToCreateReadOnly => readOnlyConnectionString != null;

        public virtual string dbHashCode => connectionString.GetHashCode().ToString();


        public IDbConnection CreateDbConnection() => createDbConnection(connectionString);
        public IDbConnection CreateReadOnlyDbConnection() => ableToCreateReadOnly ? createDbConnection(readOnlyConnectionString) : null;

        public virtual string databaseName => getDatabaseName(connectionString ?? readOnlyConnectionString);

        public virtual DbConnectionProvider WithDatabase(string databaseName)
        {
            if (changeDatabaseForConnectionString == null) throw new InvalidOperationException("do not able to change database.");

            var _connectionString = connectionString;
            var _readOnlyConnectionString = readOnlyConnectionString;

            if (_connectionString != null) _connectionString = changeDatabaseForConnectionString(_connectionString, databaseName);
            if (_readOnlyConnectionString != null) _readOnlyConnectionString = changeDatabaseForConnectionString(_readOnlyConnectionString, databaseName);

            return new DbConnectionProvider(
                createDbConnection, changeDatabaseForConnectionString, getDatabaseName,
                _connectionString, _readOnlyConnectionString
                );
        }



    }
}
