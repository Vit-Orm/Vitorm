using System;
using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlServer(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService = Vitorm.SqlServer.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);

            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection);
            dbContext.createTransactionScope = (dbContext) => new Vitorm.SqlServer.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
