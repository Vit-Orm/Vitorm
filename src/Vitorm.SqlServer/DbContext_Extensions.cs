using System;
using System.Data;

using Vitorm.Entity;
using Vitorm.Entity.Dapper;
using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;
using Vitorm.SqlServer;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlServer(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService = Vitorm.SqlServer.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);
            dbContext.createTransactionScope = (dbContext) => new Vitorm.SqlServer.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
