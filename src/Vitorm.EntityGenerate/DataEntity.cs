using Vitorm.Sql;


namespace Vitorm.EntityGenerate
{
    public static partial class DataEntity
    {
        public static IDbSet GenerateDbSet(string entityNamespace, string tableName, string schemaName = null, bool cacheEntity = false)
        {
            var dataProvider = Data.DataProvider(entityNamespace);
            SqlDbContext dbContext = dataProvider?.CreateSqlDbContext();
            return dbContext?.GenerateDbSet(entityNamespace, tableName, schemaName, cacheEntity);
        }

    }
}
