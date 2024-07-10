using Vitorm.Sql;
using Vitorm.Sql.Transaction;
using Vitorm.SqlServer;

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

            dbContext.createTransactionScope = createTransactionScope;

            if (config.commandTimeout.HasValue) dbContext.commandTimeout = config.commandTimeout.Value;

            return dbContext;
        }

        static ITransactionScope createTransactionScope(SqlDbContext dbContext) => new Vitorm.SqlServer.SqlTransactionScope(dbContext);

    }
}
