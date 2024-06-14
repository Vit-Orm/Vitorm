using System;
using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlite(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService = Vitorm.Sqlite.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection);

            dbContext.createTransactionScope = (dbContext) => new Vitorm.Sqlite.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
