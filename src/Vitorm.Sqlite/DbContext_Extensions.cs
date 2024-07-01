using System;
using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;

namespace Vit.Extensions.Vitorm_Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlite(this SqlDbContext dbContext, string connectionString, int? commandTimeout = null)
        {
            ISqlTranslateService sqlTranslateService = Vitorm.Sqlite.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.Sqlite.SqliteConnection(connectionString);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, dbHashCode: connectionString.GetHashCode().ToString());

            dbContext.createTransactionScope = (dbContext) => new Vitorm.Sqlite.SqlTransactionScope(dbContext);

            if (commandTimeout.HasValue) dbContext.commandTimeout = commandTimeout.Value;

            return dbContext;
        }



    }
}
