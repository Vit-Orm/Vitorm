using System;
using System.Linq;

using Vit.Extensions.Db_Extensions;

using Vitorm.EntityGenerate;
using Vitorm.Sql;

namespace Vitorm
{
    public static partial class SqlDbContext_EntityType_Extensions
    {
        public static Type GenerateEntityType(this SqlDbContext dbContext, string entityNamespace, string tableName, string schemaName = null)
        {
            var dbConn = dbContext.dbConnection;
            var tableSchema = dbConn.GetSchema(new[] { tableName }).First(m => string.IsNullOrEmpty(schemaName) || schemaName.Equals(m.schema_name, StringComparison.OrdinalIgnoreCase));
            var entityType = EntityHelp.GenerateEntityBySchema(tableSchema, entityNamespace);
            return entityType;
        }

        public static IDbSet GenerateDbSet(this SqlDbContext dbContext, string entityNamespace, string tableName, string schemaName = null, bool cacheEntity = false)
        {
            var entityType = GenerateEntityType(dbContext, entityNamespace, tableName, schemaName);

            IDbSet dbSet;

            if (cacheEntity)
            {
                dbSet = dbContext.DbSet(entityType);
            }
            else
            {
                var entityDescriptor = dbContext.entityLoader.LoadDescriptorWithoutCache(entityType).entityDescriptor;
                dbSet = Vitorm.Sql.DbSetConstructor.CreateDbSet(dbContext, entityDescriptor);
            }
            return dbSet;
        }


    }
}
