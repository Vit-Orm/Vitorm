using System;
using System.Data;

using Vit.Orm.Entity;
using Vit.Orm.Entity.Dapper;
using Vit.Orm.Sql;
using Vit.Orm.Sql.SqlTranslate;
using Vit.Orm.SqlServer;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlServer(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService = Vit.Orm.SqlServer.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);
            dbContext.createTransactionScope = (dbContext) => new Vit.Orm.SqlServer.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
