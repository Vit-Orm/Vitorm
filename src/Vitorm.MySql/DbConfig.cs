using System.Collections.Generic;
using System.Data;

using Vitorm.Sql;

using ConnectionStringBuilder = MySqlConnector.MySqlConnectionStringBuilder;
using DbConnection = MySqlConnector.MySqlConnection;

namespace Vitorm.MySql
{
    public class DbConfig
    {
        public DbConfig(string connectionString, int? commandTimeout = null)
        {
            this.connectionString = connectionString;
            this.commandTimeout = commandTimeout;
        }
        public DbConfig(string connectionString, string readOnlyConnectionString, int? commandTimeout = null)
        {
            this.connectionString = connectionString;
            this.readOnlyConnectionString = readOnlyConnectionString;
            this.commandTimeout = commandTimeout;
        }
        public DbConfig(Dictionary<string, object> config)
        {
            object value;
            if (config.TryGetValue("connectionString", out value))
                this.connectionString = value as string;

            if (config.TryGetValue("readOnlyConnectionString", out value))
                this.readOnlyConnectionString = value as string;

            if (config.TryGetValue("commandTimeout", out value) && int.TryParse(value as string, out var commandTimeout))
                this.commandTimeout = commandTimeout;
        }

        public string connectionString { get; set; }
        public string readOnlyConnectionString { get; set; }
        public int? commandTimeout { get; set; }


        public static string ChangeDatabaseForConnectionString(string connectionString, string databaseName)
            => new ConnectionStringBuilder(connectionString) { Database = databaseName }.ConnectionString;

        public static string GetDatabaseName(string connectionString) => new ConnectionStringBuilder(connectionString).Database;

        public static IDbConnection CreateDbConnection(string connectionString) => new DbConnection(connectionString);


        public DbConnectionProvider ToDbConnectionProvider()
            => new DbConnectionProvider(
                createDbConnection: CreateDbConnection,
                changeDatabaseForConnectionString: ChangeDatabaseForConnectionString,
                getDatabaseName: GetDatabaseName,
                connectionString: connectionString,
                readOnlyConnectionString: readOnlyConnectionString
                );

    }
}
