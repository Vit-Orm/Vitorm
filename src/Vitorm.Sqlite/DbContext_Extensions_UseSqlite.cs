using Vitorm.Sql;
using Vitorm.Sqlite;
using Vitorm.Transaction;

namespace Vitorm
{
    public static class DbContext_Extensions_UseSqlite
    {
        public static SqlDbContext UseSqlite(this SqlDbContext dbContext, string connectionString, int? commandTimeout = null)
            => UseSqlite(dbContext, new DbConfig(connectionString: connectionString, commandTimeout: commandTimeout));

        public static SqlDbContext UseSqlite(this SqlDbContext dbContext, DbConfig config)
        {
            dbContext.Init(
                sqlTranslateService: Vitorm.Sqlite.SqlTranslateService.Instance,
                dbConnectionProvider: config.ToDbConnectionProvider()
                );

            dbContext.createTransactionManager = createTransactionManager;

            if (config.commandTimeout.HasValue) dbContext.commandTimeout = config.commandTimeout.Value;

            return dbContext;
        }

        static ITransactionManager createTransactionManager(SqlDbContext dbContext) => new Vitorm.Sqlite.SqlTransactionManager(dbContext);
    }
}
