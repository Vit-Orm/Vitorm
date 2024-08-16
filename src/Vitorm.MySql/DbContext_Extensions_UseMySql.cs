using System.Threading.Tasks;

using Vitorm.MySql;
using Vitorm.Sql;
using Vitorm.Sql.Transaction;

using DbConnection = MySqlConnector.MySqlConnection;

namespace Vitorm
{
    public static class DbContext_Extensions_UseMySql
    {
        public static SqlDbContext UseMySql(this SqlDbContext dbContext, string connectionString, int? commandTimeout = null)
               => UseMySql(dbContext, new DbConfig(connectionString: connectionString, commandTimeout: commandTimeout));

        public static SqlDbContext UseMySql(this SqlDbContext dbContext, DbConfig config)
        {
            dbContext.Init(
                sqlTranslateService: Vitorm.MySql.SqlTranslateService.Instance,
                dbConnectionProvider: config.ToDbConnectionProvider(),
                sqlExecutor: sqlExecutor
                );

            dbContext.createTransactionScope = createTransactionScope;


            if (config.commandTimeout.HasValue) dbContext.commandTimeout = config.commandTimeout.Value;

            return dbContext;
        }


        #region sqlExecutor
        static SqlExecutor sqlExecutor = new SqlExecutor() { CloseAsync = CloseAsync };
        static async Task CloseAsync(System.Data.Common.DbConnection conn)
        {
            if (conn is DbConnection mySqlConn)
            {
                await mySqlConn.CloseAsync();
                return;
            }
            conn.Close();
        }
        #endregion


        static ITransactionScope createTransactionScope(SqlDbContext dbContext) => new Vitorm.MySql.SqlTransactionScope(dbContext);
        //static ITransactionScope createTransactionScope(SqlDbContext dbContext) => new Vitorm.MySql.SqlTransactionScope_Command(dbContext);


    }
}
