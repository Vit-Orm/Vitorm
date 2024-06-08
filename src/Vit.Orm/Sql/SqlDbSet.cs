using System;
using System.Collections.Generic;
using System.Linq;
using Vit.Linq.ExpressionTree.CollectionsQuery;

using Vit.Linq.ExpressionTree.ComponentModel;
using System.Linq.Expressions;
using Vit.Orm.Entity;
using System.Reflection;
using Vit.Linq;
using Vit.Orm.Sql.DataReader;
using Vit.Extensions.Linq_Extensions;

namespace Vit.Orm.Sql
{
    public class SqlDbSetConstructor
    {
        public static IDbSet CreateDbSet( SqlDbContext dbContext, Type entityType, IEntityDescriptor entityDescriptor)
        {
            return _CreateDbSet.MakeGenericMethod(entityType)
                     .Invoke(null, new object[] { dbContext, entityDescriptor }) as IDbSet;
        }
       
        static MethodInfo _CreateDbSet = new Func<SqlDbContext, IEntityDescriptor,IDbSet>(CreateDbSet<object>)
                   .Method.GetGenericMethodDefinition();
        public static IDbSet CreateDbSet<Entity>(SqlDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            return new SqlDbSet<Entity>(dbContext, entityDescriptor);
        }

    }

    public class SqlDbSet<Entity> : Vit.Orm.DbSet<Entity>
    {
        protected SqlDbContext dbContext;

        protected IEntityDescriptor _entityDescriptor;
        public override IEntityDescriptor entityDescriptor => _entityDescriptor;


        public virtual ISqlTranslator sqlTranslator => dbContext.sqlTranslator;

        public SqlDbSet(SqlDbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this._entityDescriptor = entityDescriptor;
        }

        public override void Create()
        {
            string sql = sqlTranslator.PrepareCreate(entityDescriptor);

            dbContext.Execute(sql: sql);
        }




        public override Entity Add(Entity entity)
        {
            // #1 prepare sql
            (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) = sqlTranslator.PrepareAdd(this);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 execute
            var newKeyValue = dbContext.ExecuteScalar(sql: sql, param: (object)sqlParam);

            if (newKeyValue != null)
            {
                var keyType = TypeUtil.GetUnderlyingType(entityDescriptor.key.type);
                newKeyValue = TypeUtil.ConvertToUnderlyingType(newKeyValue, keyType);
                entityDescriptor.key.Set(entity, newKeyValue);
            }
            return entity;
        }

        public override void AddRange(IEnumerable<Entity> entitys)
        {
            // #1 prepare sql
            (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) = sqlTranslator.PrepareAdd(this);

            // #2 execute
            var affectedRowCount = 0;

            var keyType = TypeUtil.GetUnderlyingType(entityDescriptor.key.type);
            foreach (var entity in entitys)
            {
                var sqlParam = GetSqlParams(entity);
                var newKeyValue = dbContext.ExecuteScalar(sql: sql, param: (object)sqlParam);
                if (newKeyValue != null)
                {
                    newKeyValue = TypeUtil.ConvertToUnderlyingType(newKeyValue, keyType);
                    entityDescriptor.key.Set(entity, newKeyValue);
                }
                affectedRowCount++;
            }
        }


        public override Entity Get(object keyValue)
        {
            // #1 prepare sql
            string sql = sqlTranslator.PrepareGet(this);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            using var reader = dbContext.ExecuteReader(sql: sql, param: (object)sqlParam);
            if (reader.Read())
            {
                var entity = (Entity)Activator.CreateInstance(typeof(Entity));
                foreach (var column in entityDescriptor.allColumns)
                {
                    column.Set(entity, TypeUtil.ConvertToType(reader[column.name], column.type));
                }
                return entity;
            }
            return default;


        }


        public override IQueryable<Entity> Query()
        {
            var dbContextId = "SqlDbSet_" + dbContext.GetHashCode();

            Func<Expression, Type, object> QueryExecutor = (expression, type) =>
            {
                // #1 convert to ExpressionNode
                // (query) => query.Where().OrderBy().Skip().Take().Select().ToList();
                // (users) => users.SelectMany(
                //      user => users.Where(father => (father.id == user.fatherId)).DefaultIfEmpty(),
                //      (user, father) => new <>f__AnonymousType4`2(user = user, father = father)
                //  ).Where().Select();
                var isArgument = QueryableBuilder.QueryTypeNameCompare(dbContextId);
                ExpressionNode node = dbContext.convertService.ConvertToData(expression, autoReduce: true, isArgument: isArgument);
                //var strNode = Json.Serialize(node);


                // #2 convert to Streams
                // {select,left,joins,where,order,skip,take}
                var stream = StreamReader.ReadNode(node);
                //var strStream = Json.Serialize(stream);


                // #3.1 ExecuteUpdate
                if (stream is StreamToUpdate streamToUpdate)
                {
                    (string sql, Dictionary<string, object> sqlParam) = sqlTranslator.PrepareExecuteUpdate(streamToUpdate);

                    return dbContext.Execute(sql: sql, param: (object)sqlParam);
                }


                // #3.3 Query
                // #3.3.1
                var combinedStream = stream as CombinedStream;
                if (combinedStream == null) combinedStream = new CombinedStream("tmp") { source = stream };

                // #3.3.2 execute and read result
                switch (combinedStream.method)
                {
                    case nameof(Queryable_Extensions.ToExecuteString):
                        {
                            // ToExecuteString
                            (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslator.PrepareQuery(combinedStream, entityType: null);
                            return sql;
                        }
                    case "Count":
                        {
                            // Count
                            (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslator.PrepareQuery(combinedStream, entityType: null);

                            var count = dbContext.ExecuteScalar(sql: sql, param: (object)sqlParam);
                            return Convert.ToInt32(count);
                        }
                    case nameof(Queryable_Extensions.ExecuteDelete):
                        {
                            // ExecuteDelete
                            (string sql, Dictionary<string, object> sqlParam) = sqlTranslator.PrepareExecuteDelete(combinedStream);

                            var count = dbContext.Execute(sql: sql, param: (object)sqlParam);
                            return count;
                        }
                    case "FirstOrDefault" or "First" or "LastOrDefault" or "Last":
                        {
                            var entityType = expression.Type;
                            (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslator.PrepareQuery(combinedStream, entityType);

                            using var reader = dbContext.ExecuteReader(sql: sql, param: (object)sqlParam);
                            return dataReader.ReadData(reader);
                        }
                    case "ToList":
                    case "":
                    case null:
                        {
                            // ToList
                            var entityType = expression.Type.GetGenericArguments()?.FirstOrDefault();
                            (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslator.PrepareQuery(combinedStream, entityType);

                            using var reader = dbContext.ExecuteReader(sql: sql, param: (object)sqlParam);
                            return dataReader.ReadData(reader);
                        }
                }
                throw new NotSupportedException("not supported query type: " + combinedStream.method);
            };
            return QueryableBuilder.Build<Entity>(QueryExecutor, dbContextId);
        }





        public override int Update(Entity entity)
        {
            // #1 prepare sql
            (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) = sqlTranslator.PrepareUpdate(this);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 execute
            var affectedRowCount = dbContext.Execute(sql: sql, param: (object)sqlParam);

            return affectedRowCount;
        }

        public override int UpdateRange(IEnumerable<Entity> entitys)
        {
            // #1 prepare sql
            (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) = sqlTranslator.PrepareUpdate(this);

            // #2 execute
            var affectedRowCount = 0;

            foreach (var entity in entitys)
            {
                var sqlParam = GetSqlParams(entity);
                affectedRowCount += dbContext.Execute(sql: sql, param: (object)sqlParam);
            }
            return affectedRowCount;
        }



        public override int Delete(Entity entity)
        {
            var key = entityDescriptor.key.Get(entity);
            return DeleteByKey(key);
        }

        public override int DeleteRange(IEnumerable<Entity> entitys)
        {
            var keys = entitys.Select(entity => entityDescriptor.key.Get(entity)).ToList();
            return DeleteByKeys(keys);
        }


        public override int DeleteByKey(object keyValue)
        {
            // #1 prepare sql
            string sql = sqlTranslator.PrepareDelete(this);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            var affectedRowCount = dbContext.Execute(sql: sql, param: (object)sqlParam);

            return affectedRowCount;
        }

        public override int DeleteByKeys<Key>(IEnumerable<Key> keys)
        {
            // #1 prepare sql
            string sql = sqlTranslator.PrepareDeleteRange(this);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam["keys"] = keys;

            // #3 execute
            var affectedRowCount = dbContext.Execute(sql: sql, param: (object)sqlParam);

            return affectedRowCount;
        }

    }
}
