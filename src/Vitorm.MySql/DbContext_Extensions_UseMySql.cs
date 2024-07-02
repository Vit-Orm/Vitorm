using Vitorm.MySql;
using Vitorm.Sql;
using Vitorm.Sql.Transaction;

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
                createDbConnection: config.createDbConnection,
                createReadOnlyDbConnection: config.createReadOnlyDbConnection,
                    dbHashCode: config.dbHashCode
                );

            dbContext.createTransactionScope = createTransactionScope;

            if (config.commandTimeout.HasValue) dbContext.commandTimeout = config.commandTimeout.Value;

            return dbContext;
        }

        static ITransactionScope createTransactionScope(SqlDbContext dbContext) => new Vitorm.MySql.SqlTransactionScope(dbContext);
        //static ITransactionScope createTransactionScope(SqlDbContext dbContext) => new Vitorm.MySql.SqlTransactionScope_Command(dbContext);


    }
}
