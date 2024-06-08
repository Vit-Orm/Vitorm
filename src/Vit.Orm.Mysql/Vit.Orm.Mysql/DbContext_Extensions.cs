using System;
using System.Data;

using Vit.Orm.Entity;
using Vit.Orm.Entity.Dapper;
using Vit.Orm.Sql;
using Vit.Orm.Mysql;

namespace Vit.Extensions
{
    public static class DbContext_Extensions
    {
        public static SqlDbContext UseMysql(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslator sqlTranslator = new SqlTranslator(dbContext);

            Func<IDbConnection> createDbConnection = () => new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslator: sqlTranslator, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);

            return dbContext;
        }



    }
}
