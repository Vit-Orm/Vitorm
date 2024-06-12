using System;
using System.Data;

using Vitorm.Entity;
using Vitorm.Entity.Dapper;
using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseSqlite(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService =   Vitorm.Sqlite.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);

            dbContext.createTransactionScope = (dbContext) => new Vitorm.Sqlite.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
