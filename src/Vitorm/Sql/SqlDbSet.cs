using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql
{
    public class SqlDbSetConstructor
    {
        public static IDbSet CreateDbSet(IDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityDescriptor.entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }

        static readonly MethodInfo _CreateDbSet = new Func<SqlDbContext, IEntityDescriptor, IDbSet>(CreateDbSet<object>)
                   .Method.GetGenericMethodDefinition();
        public static IDbSet<Entity> CreateDbSet<Entity>(SqlDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return new SqlDbSet<Entity>(dbContext, entityDescriptor);
        }

    }


    public partial class SqlDbSet<Entity> : IDbSet<Entity>
    {
        public virtual IDbContext dbContext { get; protected set; }

        protected IEntityDescriptor _entityDescriptor;
        public virtual IEntityDescriptor entityDescriptor => _entityDescriptor;


        public SqlDbSet(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this._entityDescriptor = entityDescriptor;
        }

        // #0 Schema :  ChangeTable
        public virtual IEntityDescriptor ChangeTable(string tableName) => _entityDescriptor = _entityDescriptor.WithTable(tableName);
        public virtual IEntityDescriptor ChangeTableBack() => _entityDescriptor = _entityDescriptor.GetOriginEntityDescriptor();


        public virtual SqlDbContext sqlDbContext => (SqlDbContext)dbContext;

        protected virtual ISqlTranslateService sqlTranslateService => sqlDbContext.sqlTranslateService;

        #region #0 Schema :  Create Drop Truncate
        public virtual void TryCreateTable()
        {
            string sql = sqlTranslateService.PrepareTryCreateTable(entityDescriptor);
            sqlDbContext.Execute(sql: sql);
        }
        public virtual void TryDropTable()
        {
            string sql = sqlTranslateService.PrepareTryDropTable(entityDescriptor);
            sqlDbContext.Execute(sql: sql);
        }
        public virtual void Truncate()
        {
            string sql = sqlTranslateService.PrepareTruncate(entityDescriptor);
            sqlDbContext.Execute(sql: sql);
        }
        #endregion


        #region #1 Create :  Add AddRange
        public virtual Entity Add(Entity entity)
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
                var newKeyValue = sqlDbContext.ExecuteScalar(sql: sql, param: sqlParam);

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
                sqlDbContext.Execute(sql: sql, param: sqlParam);
            }

            return entity;
        }
        public virtual void AddRange(IEnumerable<Entity> entities)
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
                    var newKeyValue = sqlDbContext.ExecuteScalar(sql: sqlAndGetParams.sql, param: sqlParam);

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
                    sqlDbContext.Execute(sql: sqlAndGetParams.sql, param: sqlParam);
                    affectedRowCount++;
                }
            }
        }
        #endregion


        #region #2 Retrieve : Get Query
        public virtual Entity Get(object keyValue)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);


            // #1 prepare sql
            string sql = sqlTranslateService.PrepareGet(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            using var reader = sqlDbContext.ExecuteReader(sql: sql, param: sqlParam, useReadOnly: true);
            if (reader.Read())
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
        #endregion

        #region #3 Update: Update UpdateRange
        public virtual int Update(Entity entity)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareUpdate(arg);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 execute
            var affectedRowCount = sqlDbContext.Execute(sql: sql, param: sqlParam);

            return affectedRowCount;
        }
        public virtual int UpdateRange(IEnumerable<Entity> entities)
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
                affectedRowCount += sqlDbContext.Execute(sql: sql, param: sqlParam);
            }
            return affectedRowCount;
        }
        #endregion

        #region #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public virtual int Delete(Entity entity)
        {
            var key = entityDescriptor.key.GetValue(entity);
            return DeleteByKey(key);
        }

        public virtual int DeleteRange(IEnumerable<Entity> entities)
        {
            var keys = entities.Select(entity => entityDescriptor.key.GetValue(entity)).ToList();
            return DeleteByKeys(keys);
        }

        public virtual int DeleteByKey(object keyValue)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            string sql = sqlTranslateService.PrepareDelete(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            var affectedRowCount = sqlDbContext.Execute(sql: sql, param: sqlParam);

            return affectedRowCount;
        }

        public virtual int DeleteByKeys<Key>(IEnumerable<Key> keys)
        {
            // #0 get arg
            SqlTranslateArgument arg = new SqlTranslateArgument(sqlDbContext, entityDescriptor);

            // #1 prepare sql
            var sql = sqlTranslateService.PrepareDeleteByKeys(arg, keys);

            // #2 execute
            var affectedRowCount = sqlDbContext.Execute(sql: sql, param: arg.sqlParam);
            return affectedRowCount;
        }
        #endregion

    }
}
