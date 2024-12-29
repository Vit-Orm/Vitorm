using Vitorm.Sql;
using Vitorm.SqlServer;
using Vitorm.Transaction;

namespace Vitorm
{
    public static class DbContext_Extensions_UseSqlServer
    {
        public static SqlDbContext UseSqlServer(this SqlDbContext dbContext, string connectionString, int? commandTimeout = null)
             => UseSqlServer(dbContext, new DbConfig(connectionString: connectionString, commandTimeout: commandTimeout));

        public static SqlDbContext UseSqlServer(this SqlDbContext dbContext, DbConfig config)
        {
            dbContext.Init(
                sqlTranslateService: Vitorm.SqlServer.SqlTranslateService.Instance,
                dbConnectionProvider: config.ToDbConnectionProvider()
                );

            dbContext.createTransactionManager = createTransactionManager;

            if (config.commandTimeout.HasValue) dbContext.commandTimeout = config.commandTimeout.Value;

            return dbContext;
        }

        static ITransactionManager createTransactionManager(SqlDbContext dbContext) => new Vitorm.SqlServer.SqlTransactionManager(dbContext);

    }
}
