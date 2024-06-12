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
        /*
         // ref: https://dev.mysql.com/doc/refman/8.4/en/savepoint.html
         //  https://dev.mysql.com/doc/refman/8.4/en/commit.html

        START TRANSACTION;
            SET autocommit=0;
            SAVEPOINT tran0;
                select '';
            -- ROLLBACK WORK TO SAVEPOINT tran0;
            RELEASE SAVEPOINT tran0;
        COMMIT;
        -- ROLLBACK;
         */
        public static SqlDbContext UseMySql(this SqlDbContext dbContext, string ConnectionString)
        {
            ISqlTranslateService sqlTranslateService =   Vitorm.MySql.SqlTranslateService.Instance;

            Func<IDbConnection> createDbConnection = () => new MySqlConnector.MySqlConnection(ConnectionString);

            Func<Type, IEntityDescriptor> getEntityDescriptor = (type) => EntityDescriptor.GetEntityDescriptor(type);


            dbContext.Init(sqlTranslateService: sqlTranslateService, createDbConnection: createDbConnection, getEntityDescriptor: getEntityDescriptor);

            dbContext.createTransactionScope = (dbContext) => new Vitorm.MySql.SqlTransactionScope(dbContext);
            //dbContext.createTransactionScope = (dbContext) => new Vitorm.Mysql.SqlTransactionScope_Command(dbContext);

            return dbContext;
        }



    }
}
