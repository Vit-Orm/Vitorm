using System;
using System.Data;

using Vit.Orm.Entity;
using Vit.Orm.Entity.Dapper;
using Vit.Orm.Sql;
using Vit.Orm.Sql.SqlTranslate;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlite(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService =   Vit.Orm.Sqlite.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);

            dbContext.createTransactionScope = (dbContext) => new Vit.Orm.Sqlite.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
