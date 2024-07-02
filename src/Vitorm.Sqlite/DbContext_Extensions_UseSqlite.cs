using Vitorm.Sql;
using Vitorm.Sql.Transaction;
using Vitorm.Sqlite;

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
                createDbConnection: config.createDbConnection,
                createReadOnlyDbConnection: config.createReadOnlyDbConnection,
                dbHashCode: config.dbHashCode
                );

            dbContext.createTransactionScope = createTransactionScope;

            if (config.commandTimeout.HasValue) dbContext.commandTimeout = config.commandTimeout.Value;

            return dbContext;
        }

        static ITransactionScope createTransactionScope(SqlDbContext dbContext) => new Vitorm.Sqlite.SqlTransactionScope(dbContext);
    }
}
