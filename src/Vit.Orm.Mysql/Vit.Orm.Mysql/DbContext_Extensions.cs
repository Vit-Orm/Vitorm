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
        public static SqlDbContext UseMysql(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService =   Vit.Orm.Mysql.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);

            //dbContext.createTransactionScope = (dbContext) => new Vit.Orm.Mysql.SqlTransactionScope(dbContext);

            return dbContext;
        }



    }
}
