using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Sql.DataReader.EntityReader;
using Vitorm.Sql.SqlTranslate;
using Vitorm.Sql.Transaction;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        /// <summary>
        /// Vitorm.Sql.DataReader.EntityReader.IEntityReader
        /// 
        /// SqlDbContext.defaultEntityReaderType = typeof(global::Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader) ;  // default
        /// SqlDbContext.defaultEntityReaderType = typeof(global::Vitorm.Sql.DataReader.EntityReader.CompiledLambda.EntityReader) ;
        /// </summary>
        public static Type defaultEntityReaderType = typeof(Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader);
        public Type entityReaderType { get; protected set; } = defaultEntityReaderType;

        /// <summary>
        ///   SetEntityReader<global::Vitorm.Sql.DataReader.EntityReader.EntityConstructor.EntityReader>();  // default
        ///   SetEntityReader<global::Vitorm.Sql.DataReader.EntityReader.CompiledLambda.EntityReader>();
        /// </summary>
        /// <typeparam name="EntityReader"></typeparam>
        public virtual void SetEntityReader<EntityReader>()
            where EntityReader : IEntityReader, new()
        {
            entityReaderType = typeof(EntityReader);
        }


        protected Func<IDbConnection> createDbConnection { get; set; }
        protected Func<IDbConnection> createReadOnlyDbConnection { get; set; }
        protected IDbConnection _dbConnection;
        protected IDbConnection _readOnlyDbConnection;
        public override void Dispose()
        {
            try
            {
                transactionScope?.Dispose();
            }
            finally
            {
                transactionScope = null;
                try
                {
                    _dbConnection?.Dispose();
                }
                finally
                {
                    _dbConnection = null;

                    try
                    {
                        _readOnlyDbConnection?.Dispose();
                    }
                    finally
                    {
                        _readOnlyDbConnection = null;

                        base.Dispose();
                    }
                }
            }
        }
        public virtual IDbConnection dbConnection => _dbConnection ??= createDbConnection();
        public virtual IDbConnection readOnlyDbConnection
        {
            get
            {
                if (_readOnlyDbConnection != null) return _readOnlyDbConnection;
                if (createReadOnlyDbConnection != null) return _readOnlyDbConnection = createReadOnlyDbConnection();

                return dbConnection;
            }
        }


        public virtual ISqlTranslateService sqlTranslateService { get; private set; }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlTranslateService"></param>
        /// <param name="createDbConnection"></param>
        /// <param name="createReadOnlyDbConnection"></param>
        /// <param name="sqlExecutor"></param>
        /// <param name="dbHashCode"> to identify whether contexts are from the same database </param>
        public virtual void Init(ISqlTranslateService sqlTranslateService, Func<IDbConnection> createDbConnection, Func<IDbConnection> createReadOnlyDbConnection = null, SqlExecutor sqlExecutor = null, string dbHashCode = null)
        {
            this.sqlTranslateService = sqlTranslateService;
            this.createDbConnection = createDbConnection;
            this.createReadOnlyDbConnection = createReadOnlyDbConnection;
            this.sqlExecutor = sqlExecutor ?? SqlExecutor.Instance;

            if (string.IsNullOrEmpty(dbHashCode))
                dbHashCode = GetHashCode().ToString();

            dbGroupName = "SqlDbSet_" + dbHashCode;
        }


        #region #0 Schema :  Create Drop

        public override void Create<Entity>()
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));

            string sql = sqlTranslateService.PrepareCreate(entityDescriptor);
            Execute(sql: sql);
        }
        public override void Drop<Entity>()
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));

            string sql = sqlTranslateService.PrepareDrop(entityDescriptor);
            Execute(sql: sql);
        }
        #endregion


        #region #1 Create :  Add AddRange

        public override Entity Add<Entity>(Entity entity)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);

            var addType = sqlTranslateService.Entity_GetAddType(arg, entity);
            //if (addType == EAddType.unexpectedEmptyKey) throw new ArgumentException("Key could not be empty.");




            if (addType == EAddType.identityKey)
            {
                // #1 prepare sql
                (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareIdentityAdd(arg);

                // #2 get sql params
                var sqlParam = GetSqlParams(entity);

                // #3 add
                var newKeyValue = ExecuteScalar(sql: sql, param: sqlParam);

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
                Execute(sql: sql, param: sqlParam);
            }

            return entity;
        }
        public override void AddRange<Entity>(IEnumerable<Entity> entities)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);
            List<(Entity entity, EAddType addType)> entityAndTypes = entities.Select(entity => (entity, sqlTranslateService.Entity_GetAddType(arg, entity))).ToList();
            //if (entityAndTypes.Any(row => row.addType == EAddType.unexpectedEmptyKey)) throw new ArgumentException("Key could not be empty.");


            var affectedRowCount = 0;

            // #2 keyWithValue
            {
                var rows = entityAndTypes.Where(row => row.addType == EAddType.keyWithValue);
                if (rows.Any())
                {
                    // ##1 prepare sql
                    (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareAdd(arg);

                    foreach ((var entity, _) in rows)
                    {
                        // #2 get sql params
                        var sqlParam = GetSqlParams(entity);

                        // #3 add
                        Execute(sql: sql, param: sqlParam);
                        affectedRowCount++;
                    }
                }
            }

            // #3 identityKey
            {
                var rows = entityAndTypes.Where(row => row.addType == EAddType.identityKey);
                if (rows.Any())
                {
                    var keyType = TypeUtil.GetUnderlyingType(entityDescriptor.key.type);

                    // ##1 prepare sql
                    (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareIdentityAdd(arg);

                    foreach ((var entity, _) in rows)
                    {
                        // ##2 get sql params
                        var sqlParam = GetSqlParams(entity);

                        // ##3 add
                        var newKeyValue = ExecuteScalar(sql: sql, param: sqlParam);

                        // ##4 set key value to entity
                        newKeyValue = TypeUtil.ConvertToUnderlyingType(newKeyValue, keyType);
                        if (newKeyValue != null)
                        {
                            entityDescriptor.key.SetValue(entity, newKeyValue);
                        }

                        affectedRowCount++;
                    }
                }
            }
        }

        #endregion



        #region #2 Retrieve : Get Query

        public override Entity Get<Entity>(object keyValue)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);


            // #1 prepare sql
            string sql = sqlTranslateService.PrepareGet(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            using var reader = ExecuteReader(sql: sql, param: sqlParam, useReadOnly: true);
            if (reader.Read())
            {
                var entity = (Entity)Activator.CreateInstance(typeof(Entity));
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


        /// <summary>
        /// to identify whether contexts are from the same database
        /// </summary>
        protected string dbGroupName { get; set; }
        protected bool QueryIsFromSameDb(object query, Type elementType)
        {
            return dbGroupName == QueryableBuilder.GetQueryConfig(query as IQueryable) as string;
        }
        public Action<SqlDbContext, Expression, Type, object> AfterQuery;
        protected object QueryExecutor(Expression expression, Type type)
        {
            object result = null;
            try
            {
                return result = ExecuteQuery(expression, type);
            }
            finally
            {
                AfterQuery?.Invoke(this, expression, type, result);
            }
        }
        public virtual SqlDbContext AutoDisposeAfterQuery()
        {
            AfterQuery += (_, _, _, _) => Dispose();
            return this;
        }

        protected virtual object ExecuteQuery(Expression expression, Type type)
        {
            // #1 convert to ExpressionNode 
            ExpressionNode node = convertService.ConvertToLambdaData(expression, autoReduce: true, isArgument: QueryIsFromSameDb);
            //var strNode = Json.Serialize(node);


            // #2 convert to Stream
            var stream = StreamReader.ReadNode(node);
            //var strStream = Json.Serialize(stream);


            // #3.1 ExecuteUpdate
            if (stream is StreamToUpdate streamToUpdate)
            {
                // get arg
                var resultEntityType = streamToUpdate.fieldsToUpdate.New_GetType();
                var arg = new QueryTranslateArgument(this, resultEntityType);

                (string sql, Dictionary<string, object> sqlParam) = sqlTranslateService.PrepareExecuteUpdate(arg, streamToUpdate);

                return Execute(sql: sql, param: sqlParam);
            }


            // #3.3 Query
            // #3.3.1
            if (stream is not CombinedStream combinedStream) combinedStream = new CombinedStream("tmp") { source = stream };

            // #3.3.2 execute and read result
            switch (combinedStream.method)
            {
                case nameof(Orm_Extensions.ToExecuteString):
                    {
                        // ToExecuteString

                        // get arg
                        var arg = new QueryTranslateArgument(this, null);

                        (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslateService.PrepareQuery(arg, combinedStream);
                        return sql;
                    }
                case "Count":
                    {
                        // Count

                        // deal with skip and take , no need to pass to PrepareCountQuery
                        combinedStream.orders = null;
                        (int skip, int? take) range = (combinedStream.skip ?? 0, combinedStream.take);
                        combinedStream.take = null;
                        combinedStream.skip = null;

                        // get arg
                        var arg = new QueryTranslateArgument(this, null);

                        (string sql, Dictionary<string, object> sqlParam) = sqlTranslateService.PrepareCountQuery(arg, combinedStream);

                        var count = ExecuteScalar(sql: sql, param: sqlParam, useReadOnly: true);
                        var rowCount = Convert.ToInt32(count);
                        if (rowCount > 0)
                        {
                            if (range.skip > 0) rowCount = Math.Max(rowCount - range.skip, 0);

                            if (combinedStream.take.HasValue)
                                rowCount = Math.Min(rowCount, range.take.Value);
                        }
                        return rowCount;
                    }
                case nameof(Orm_Extensions.ExecuteDelete):
                    {
                        // ExecuteDelete

                        // get arg
                        var resultEntityType = (combinedStream.source as SourceStream)?.GetEntityType();
                        var arg = new QueryTranslateArgument(this, resultEntityType);

                        (string sql, Dictionary<string, object> sqlParam) = sqlTranslateService.PrepareExecuteDelete(arg, combinedStream);

                        var count = Execute(sql: sql, param: sqlParam);
                        return count;
                    }
                case "FirstOrDefault" or "First" or "LastOrDefault" or "Last":
                    {
                        // get arg
                        var resultEntityType = expression.Type;
                        var arg = new QueryTranslateArgument(this, resultEntityType);

                        (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslateService.PrepareQuery(arg, combinedStream);

                        using var reader = ExecuteReader(sql: sql, param: sqlParam, useReadOnly: true);
                        return dataReader.ReadData(reader);
                    }
                case "ToList":
                case "":
                case null:
                    {
                        // ToList

                        // get arg
                        var resultEntityType = expression.Type.GetGenericArguments()?.FirstOrDefault();
                        var arg = new QueryTranslateArgument(this, resultEntityType);

                        (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) = sqlTranslateService.PrepareQuery(arg, combinedStream);

                        using var reader = ExecuteReader(sql: sql, param: sqlParam, useReadOnly: true);
                        return dataReader.ReadData(reader);
                    }
            }
            throw new NotSupportedException("not supported query type: " + combinedStream.method);
        }

        public override IQueryable<Entity> Query<Entity>()
        {
            return QueryableBuilder.Build<Entity>(QueryExecutor, dbGroupName);
        }

        #endregion



        #region #3 Update: Update UpdateRange

        public override int Update<Entity>(Entity entity)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareUpdate(arg);

            // #2 get sql params
            var sqlParam = GetSqlParams(entity);

            // #3 execute
            var affectedRowCount = Execute(sql: sql, param: sqlParam);

            return affectedRowCount;

        }

        public override int UpdateRange<Entity>(IEnumerable<Entity> entities)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);

            // #1 prepare sql
            (string sql, Func<object, Dictionary<string, object>> GetSqlParams) = sqlTranslateService.PrepareUpdate(arg);

            // #2 execute
            var affectedRowCount = 0;

            foreach (var entity in entities)
            {
                var sqlParam = GetSqlParams(entity);
                affectedRowCount += Execute(sql: sql, param: sqlParam);
            }
            return affectedRowCount;
        }

        #endregion


        #region #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public override int Delete<Entity>(Entity entity)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));

            var key = entityDescriptor.key.GetValue(entity);
            return DeleteByKey<Entity>(key);
        }

        public override int DeleteRange<Entity>(IEnumerable<Entity> entities)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));

            var keys = entities.Select(entity => entityDescriptor.key.GetValue(entity)).ToList();
            return DeleteByKeys<Entity, object>(keys);
        }


        public override int DeleteByKey<Entity>(object keyValue)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);

            // #1 prepare sql
            string sql = sqlTranslateService.PrepareDelete(arg);

            // #2 get sql params
            var sqlParam = new Dictionary<string, object>();
            sqlParam[entityDescriptor.keyName] = keyValue;

            // #3 execute
            var affectedRowCount = Execute(sql: sql, param: sqlParam);

            return affectedRowCount;

        }
        public override int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys)
        {
            // #0 get arg
            var entityDescriptor = GetEntityDescriptor(typeof(Entity));
            SqlTranslateArgument arg = new SqlTranslateArgument(this, entityDescriptor);

            // #1 prepare sql
            (string sql, Dictionary<string, object> sqlParam) = sqlTranslateService.PrepareDeleteByKeys(arg, keys);

            // #2 execute
            var affectedRowCount = Execute(sql: sql, param: sqlParam);
            return affectedRowCount;
        }

        #endregion


        #region Transaction
        public virtual Func<SqlDbContext, ITransactionScope> createTransactionScope { set; get; }
                    = (dbContext) => new SqlTransactionScope(dbContext);
        protected virtual ITransactionScope transactionScope { get; set; }

        public virtual IDbTransaction BeginTransaction()
        {
            transactionScope ??= createTransactionScope(this);
            return transactionScope.BeginTransaction();
        }
        public virtual IDbTransaction GetCurrentTransaction() => transactionScope?.GetCurrentTransaction();

        #endregion



        #region Execute
        protected SqlExecutor sqlExecutor;
        public static int? defaultCommandTimeout;
        public int? commandTimeout;

        public virtual int ExecuteWithTransaction(string sql, IDictionary<string, object> param = null, IDbTransaction transaction = null)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;

            return sqlExecutor.Execute(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
        }

        public virtual int Execute(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return sqlExecutor.Execute(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return sqlExecutor.Execute(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        public virtual IDataReader ExecuteReader(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return sqlExecutor.ExecuteReader(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return sqlExecutor.ExecuteReader(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }

        public virtual object ExecuteScalar(string sql, IDictionary<string, object> param = null, int? commandTimeout = null, bool useReadOnly = false)
        {
            commandTimeout ??= this.commandTimeout ?? defaultCommandTimeout;
            var transaction = GetCurrentTransaction();

            if (useReadOnly && transaction == null)
            {
                return sqlExecutor.ExecuteScalar(readOnlyDbConnection, sql, param: param, commandTimeout: commandTimeout);
            }
            else
            {
                return sqlExecutor.ExecuteScalar(dbConnection, sql, param: param, transaction: transaction, commandTimeout: commandTimeout);
            }
        }
        #endregion

    }
}
