using System;
using System.Data;

using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;

namespace Vit.Extensions.Vitorm_Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlServer(this SqlDbContext dbContext, string connectionString, int? commandTimeout = null)
        {
            ISqlTranslateService sqlTranslateService = Vitorm.SqlServer.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.SqlClient.SqlConnection(connectionString);

            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, dbHashCode: connectionString.GetHashCode().ToString());

            dbContext.createTransactionScope = (dbContext) => new Vitorm.SqlServer.SqlTransactionScope(dbContext);

            if (commandTimeout.HasValue) dbContext.commandTimeout = commandTimeout.Value;

            return dbContext;
        }



    }
}
