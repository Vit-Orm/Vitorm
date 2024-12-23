using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql
{
    public partial class SqlDbSet<Entity>
    {
        #region #0 Schema :  Create Drop Truncate
        public virtual async Task TryCreateTableAsync()
        {
            string sql = sqlTranslateService.PrepareTryCreateTable(entityDescriptor);
            await sqlDbContext.ExecuteAsync(sql: sql);
        }

        public virtual async Task TryDropTableAsync()
        {
            string sql = sqlTranslateService.PrepareTryDropTable(entityDescriptor);
            await sqlDbContext.ExecuteAsync(sql: sql);
        }
        public virtual async Task TruncateAsync()
        {
            string sql = sqlTranslateService.PrepareTruncate(entityDescriptor);
            await sqlDbContext.ExecuteAsync(sql: sql);
        }
        #endregion


        #region #1 Create :  Add AddRange
        public virtual async Task<Entity> AddAsync(Entity entity)
        {
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            var addType = sqlTranslateService.Entity_GetAddType(arg, entity);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareAdd(arg, addType);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 Execute
            if (addType == EAddType.identityKey)
            {
                var newKeyValue = await sqlDbContext.ExecuteScalarAsync(sql: sql, parameters: sqlParam);

                // set key value to entity
                var keyType = TypeUtil.GetUnderlyingType(entityDescriptor.key.type);
                newKeyValue = TypeUtil.ConvertToUnderlyingType(newKeyValue, keyType);
                if (newKeyValue != null)
                {
                    entityDescriptor.key.SetValue(entity, newKeyValue);
                }
            }
            else
            {
                await sqlDbContext.ExecuteAsync(sql: sql, parameters: sqlParam);
            }

            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<Entity> entities)
        {
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);
            Dictionary<EAddType, (string sql, Func<object, Dictionary<string, object>> GetSqlParams)> sqlMaps = new();
            var affectedRowCount = 0;

            List<(Entity entity, EAddType addType)> entityAndAddTypes = entities.Select(entity => (entity, sqlTranslateService.Entity_GetAddType(arg, entity))).ToList();

            foreach (var (entity, addType) in entityAndAddTypes)
            {
                // #1 prepare sql
                if (!sqlMaps.TryGetValue(addType, out var sqlAndGetParams))
                {
                    sqlMaps[addType] = sqlAndGetParams = sqlTranslateService.PrepareAdd(arg, addType);
                }

                // #2 get sql params
                var sqlParam = sqlAndGetParams.GetSqlParams(entity);

                // #3 Execute
                if (addType == EAddType.identityKey)
                {
                    var newKeyValue = await sqlDbContext.ExecuteScalarAsync(sql: sqlAndGetParams.sql, parameters: sqlParam);

                    // set key value to entity
                    var keyType = TypeUtil.GetUnderlyingType(entityDescriptor.key.type);
                    newKeyValue = TypeUtil.ConvertToUnderlyingType(newKeyValue, keyType);
                    if (newKeyValue != null)
                    {
                        entityDescriptor.key.SetValue(entity, newKeyValue);
                    }
                    affectedRowCount++;
                }
                else
                {
                    await sqlDbContext.ExecuteAsync(sql: sqlAndGetParams.sql, parameters: sqlParam);
                    affectedRowCount++;
                }
            }
        }
        #endregion


        #region #2 Retrieve : Get Query
        public virtual async Task<Entity> GetAsync(object keyValue)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);


            // #1 prepare sql
            string sql = sqlTranslateService.PrepareGet(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            using var reader = await sqlDbContext.ExecuteReaderAsync(sql: sql, parameters: sqlParam, useReadOnly: true);

            if (reader is DbDataReader dataReader ? await dataReader.ReadAsync() : reader.Read())
            {
                var entity = (Entity)Activator.CreateInstance(entityDescriptor.entityType);
                foreach (var column in entityDescriptor.properties)
                {
                    var value = TypeUtil.ConvertToType(reader[column.columnName], column.type);
                    if (value != null)
                        column.SetValue(entity, value);
                }
                return entity;
            }

            return default;

        }
        //public override IQueryable<Entity> Query() => dbContext.Query<Entity>();
        #endregion

        #region #3 Update: Update UpdateRange
        public virtual async Task<int> UpdateAsync(Entity entity)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareUpdate(arg);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 execute
            var affectedRowCount = await sqlDbContext.ExecuteAsync(sql: sql, parameters: sqlParam);

            return affectedRowCount;
        }
        public virtual async Task<int> UpdateRangeAsync(IEnumerable<Entity> entities)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareUpdate(arg);

            // #2 execute
            var affectedRowCount = 0;

            foreach (var entity in entities)
            {
                var sqlParam = GetSqlParams(entity);
                affectedRowCount += await sqlDbContext.ExecuteAsync(sql: sql, parameters: sqlParam);
            }
            return affectedRowCount;
        }
        #endregion

        #region #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual async Task<int> DeleteAsync(Entity entity)
        {
            var key = entityDescriptor.key.GetValue(entity);
            return await DeleteByKeyAsync(key);
        }

        public virtual async Task<int> DeleteRangeAsync(IEnumerable<Entity> entities)
        {
            var keys = entities.Select(entity => entityDescriptor.key.GetValue(entity)).ToList();
            return await DeleteByKeysAsync(keys);
        }

        public virtual async Task<int> DeleteByKeyAsync(object keyValue)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            string sql = sqlTranslateService.PrepareDelete(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            var affectedRowCount = await sqlDbContext.ExecuteAsync(sql: sql, parameters: sqlParam);

            return affectedRowCount;
        }

        public virtual async Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            var sql = sqlTranslateService.PrepareDeleteByKeys(arg, keys);

            // #2 execute
            var affectedRowCount = await sqlDbContext.ExecuteAsync(sql: sql, parameters: arg.sqlParam);
            return affectedRowCount;
        }
        #endregion
    }
}
