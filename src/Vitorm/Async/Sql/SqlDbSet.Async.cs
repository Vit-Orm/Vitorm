using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql
{
    public partial class SqlDbSet<Entity> : DbSet<Entity>
    {
        #region #0 Schema :  Create Drop Truncate
        public override async Task TryCreateTableAsync()
        {
            string sql = sqlTranslateService.PrepareTryCreateTable(entityDescriptor);
            await sqlDbContext.ExecuteAsync(sql: sql);
        }

        public override async Task TryDropTableAsync()
        {
            string sql = sqlTranslateService.PrepareTryDropTable(entityDescriptor);
            await sqlDbContext.ExecuteAsync(sql: sql);
        }
        public override async Task TruncateAsync()
        {
            string sql = sqlTranslateService.PrepareTruncate(entityDescriptor);
            await sqlDbContext.ExecuteAsync(sql: sql);
        }
        #endregion


        #region #1 Create :  Add AddRange
        public override async Task<Entity> AddAsync(Entity entity)
        {
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            var addType = sqlTranslateService.Entity_GetAddType(arg, entity);
            //if (addType == EAddType.unexpectedEmptyKey) throw new ArgumentException("Key could not be empty.");

            if (addType == EAddType.identityKey)
            {
                // #1 prepare sql
                (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareIdentityAdd(arg);

                // #2 get sql params
                var sqlParam = GetSqlParams(entity);

                // #3 add
                var newKeyValue = await sqlDbContext.ExecuteScalarAsync(sql: sql, param: sqlParam);

                // #4 set key value to entity
                var keyType = TypeUtil.GetUnderlyingType(entityDescriptor.key.type);
                newKeyValue = TypeUtil.ConvertToUnderlyingType(newKeyValue, keyType);
                if (newKeyValue != null)
                {
                    entityDescriptor.key.SetValue(entity, newKeyValue);
                }
            }
            else
            {
                // #1 prepare sql
                (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareAdd(arg);

                // #2 get sql params
                var sqlParam = GetSqlParams(entity);

                // #3 add
                await sqlDbContext.ExecuteAsync(sql: sql, param: sqlParam);
            }

            return entity;
        }

        public override async Task AddRangeAsync(IEnumerable<Entity> entities)
        {
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) sql_IdentityKey = default;
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) sql_Others = default;
            var affectedRowCount = 0;

            foreach (var entity in entities)
            {
                var addType = sqlTranslateService.Entity_GetAddType(arg, entity);
                //if (addType == EAddType.unexpectedEmptyKey) throw new ArgumentException("Key could not be empty.");

                if (addType == EAddType.identityKey)
                {
                    // #1 prepare sql
                    if (sql_IdentityKey == default)
                        sql_IdentityKey = sqlTranslateService.PrepareIdentityAdd(arg);

                    // #2 get sql params
                    var sqlParam = sql_IdentityKey.GetSqlParams(entity);

                    // #3 add
                    var newKeyValue = await sqlDbContext.ExecuteScalarAsync(sql: sql_IdentityKey.sql, param: sqlParam);

                    // #4 set key value to entity
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
                    // #1 prepare sql
                    if (sql_Others == default)
                        sql_Others = sqlTranslateService.PrepareAdd(arg);

                    // #2 get sql params
                    var sqlParam = sql_Others.GetSqlParams(entity);

                    // #3 add
                    await sqlDbContext.ExecuteAsync(sql: sql_Others.sql, param: sqlParam);

                    affectedRowCount++;
                }
            }
        }
        #endregion


        #region #2 Retrieve : Get Query
        public override async Task<Entity> GetAsync(object keyValue)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);


            // #1 prepare sql
            string sql = sqlTranslateService.PrepareGet(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            using var reader = await sqlDbContext.ExecuteReaderAsync(sql: sql, param: sqlParam, useReadOnly: true);

            if (reader is DbDataReader dataReader ? await dataReader.ReadAsync() : reader.Read())
            {
                var entity = (Entity)Activator.CreateInstance(entityDescriptor.entityType);
                foreach (var column in entityDescriptor.allColumns)
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
        public override async Task<int> UpdateAsync(Entity entity)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareUpdate(arg);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 execute
            var affectedRowCount = await sqlDbContext.ExecuteAsync(sql: sql, param: sqlParam);

            return affectedRowCount;
        }
        public override async Task<int> UpdateRangeAsync(IEnumerable<Entity> entities)
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
                affectedRowCount += await sqlDbContext.ExecuteAsync(sql: sql, param: sqlParam);
            }
            return affectedRowCount;
        }
        #endregion

        #region #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public override async Task<int> DeleteAsync(Entity entity)
        {
            var key = entityDescriptor.key.GetValue(entity);
            return await DeleteByKeyAsync(key);
        }

        public override async Task<int> DeleteRangeAsync(IEnumerable<Entity> entities)
        {
            var keys = entities.Select(entity => entityDescriptor.key.GetValue(entity)).ToList();
            return await DeleteByKeysAsync(keys);
        }

        public override async Task<int> DeleteByKeyAsync(object keyValue)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            string sql = sqlTranslateService.PrepareDelete(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            var affectedRowCount = await sqlDbContext.ExecuteAsync(sql: sql, param: sqlParam);

            return affectedRowCount;
        }

        public override async Task<int> DeleteByKeysAsync<Key>(IEnumerable<Key> keys)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            var sql = sqlTranslateService.PrepareDeleteByKeys(arg, keys);

            // #2 execute
            var affectedRowCount = await sqlDbContext.ExecuteAsync(sql: sql, param: arg.sqlParam);
            return affectedRowCount;
        }
        #endregion
    }
}
