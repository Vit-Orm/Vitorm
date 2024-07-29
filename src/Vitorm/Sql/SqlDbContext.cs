using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Entity;
using Vitorm.Sql.DataReader.EntityReader;
using Vitorm.Sql.SqlTranslate;
using Vitorm.Sql.Transaction;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        public SqlDbContext()
        {
            dbSetCreator = DefaultDbSetCreator;
        }

        #region DbSet
        protected IDbSet DefaultDbSetCreator(IEntityDescriptor entityDescriptor)
        {
            return DbSetConstructor.CreateDbSet(this, entityDescriptor);
        }

        #endregion



        #region EntityReader


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
        #endregion


        #region dbConnection
        protected DbConnectionProvider dbConnectionProvider;
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
        public virtual IDbConnection dbConnection => _dbConnection ??= dbConnectionProvider.CreateDbConnection();
        public virtual IDbConnection readOnlyDbConnection
            => _readOnlyDbConnection ??
                (dbConnectionProvider.ableToCreateReadOnly ? (_readOnlyDbConnection = dbConnectionProvider.CreateReadOnlyDbConnection()) : dbConnection);

        /// <summary>
        /// to identify whether contexts are from the same database
        /// </summary>
        protected virtual string dbGroupName => "SqlDbSet_" + dbConnectionProvider.dbHashCode;
        public virtual string databaseName => dbConnectionProvider.databaseName;

        public virtual void ChangeDatabase(string databaseName)
        {
            if (_dbConnection != null || _readOnlyDbConnection != null) throw new InvalidOperationException("can not change database after connected, please try in an new DbContext.");

            dbConnectionProvider = dbConnectionProvider.WithDatabase(databaseName);
        }

        #endregion

        public virtual ISqlTranslateService sqlTranslateService { get; private set; }

        public virtual void Init(ISqlTranslateService sqlTranslateService, DbConnectionProvider dbConnectionProvider, SqlExecutor sqlExecutor = null, Dictionary<string, object> extraConfig = null)
        {
            this.sqlTranslateService = sqlTranslateService;
            this.dbConnectionProvider = dbConnectionProvider;
            this.sqlExecutor = sqlExecutor ?? SqlExecutor.Instance;

            extraConfig?.ForEach(kv =>
            {
                switch (kv.Key)
                {
                    case nameof(query_ToListAndTotalCount_InvokeInOneExecute):
                        {
                            if (kv.Value is bool invokeInOneExecute) query_ToListAndTotalCount_InvokeInOneExecute = invokeInOneExecute;
                            break;
                        }
                }
            });
        }




        // #0 Schema :  Create Drop
        public override void TryCreateTable<Entity>() => DbSet<Entity>().TryCreateTable();
        public override void TryDropTable<Entity>() => DbSet<Entity>().TryDropTable();


        // #1 Create :  Add AddRange
        public override Entity Add<Entity>(Entity entity) => DbSet<Entity>().Add(entity);
        public override void AddRange<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().AddRange(entities);




        #region #2 Retrieve : Get Query

        public override Entity Get<Entity>(object keyValue) => DbSet<Entity>().Get(keyValue);


        public override IQueryable<Entity> Query<Entity>()
        {
            return QueryableBuilder.Build<Entity>(QueryExecutor, dbGroupName);
        }

        protected bool QueryIsFromSameDb(object query, Type elementType)
        {
            return dbGroupName == QueryableBuilder.GetQueryConfig(query as IQueryable) as string;
        }
        public Action<SqlDbContext, Expression, Type, object> AfterQuery;
        protected object QueryExecutor(Expression expression, Type expressionResultType)
        {
            object result = null;
            try
            {
                return result = ExecuteQuery(expression, expressionResultType);
            }
            finally
            {
                AfterQuery?.Invoke(this, expression, expressionResultType, result);
            }
        }
        public virtual SqlDbContext AutoDisposeAfterQuery()
        {
            AfterQuery += (_, _, _, _) => Dispose();
            return this;
        }

        protected virtual object ExecuteQuery(Expression expression, Type expressionResultType)
        {
            // #1 convert to ExpressionNode 
            ExpressionNode_Lambda node = convertService.ConvertToData_LambdaNode(expression, autoReduce: true, isArgument: QueryIsFromSameDb);
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

                var sql = sqlTranslateService.PrepareExecuteUpdate(arg, streamToUpdate);

                return Execute(sql: sql, param: arg.sqlParam);
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

                        var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);
                        return sql;
                    }
                case nameof(Orm_Extensions.ExecuteDelete):
                    {
                        // ExecuteDelete

                        // get arg
                        var entityType = (combinedStream.source as SourceStream)?.GetEntityType();
                        var arg = new QueryTranslateArgument(this, entityType);

                        var sql = sqlTranslateService.PrepareExecuteDelete(arg, combinedStream);
                        var count = Execute(sql: sql, param: arg.sqlParam);
                        return count;
                    }
                case nameof(Queryable.FirstOrDefault) or nameof(Queryable.First) or nameof(Queryable.LastOrDefault) or nameof(Queryable.Last):
                    {
                        // get arg
                        var resultEntityType = expression.Type;
                        var arg = new QueryTranslateArgument(this, resultEntityType);

                        var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);
                        using var reader = ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
                        return arg.dataReader.ReadData(reader);
                    }
                case nameof(Queryable.Count) or nameof(Queryable_Extensions.TotalCount):
                    {
                        return ExecuteQuery_Count(combinedStream);
                    }
                case nameof(Queryable_Extensions.ToListAndTotalCount):
                    {
                        var resultEntityType = expression.Type.GetGenericArguments()?.FirstOrDefault()?.GetGenericArguments()?.FirstOrDefault();
                        return ExecuteQuery_ToListAndTotalCount(combinedStream, resultEntityType);
                    }
                case nameof(Enumerable.ToList):
                case "":
                case null:
                    {
                        // ToList
                        var resultEntityType = expression.Type.GetGenericArguments()?.FirstOrDefault();
                        return ExecuteQuery_ToList(combinedStream, resultEntityType);
                    }
            }
            throw new NotSupportedException("not supported query type: " + combinedStream.method);
        }

        protected bool query_ToListAndTotalCount_InvokeInOneExecute = true;
        protected virtual object ExecuteQuery_ToListAndTotalCount(CombinedStream combinedStream, Type resultEntityType)
        {
            object list; int totalCount;

            if (query_ToListAndTotalCount_InvokeInOneExecute)
            {
                // get arg
                var arg = new QueryTranslateArgument(this, resultEntityType);

                string sqlToList, sqlCount;
                IDbDataReader dataReader;
                // #1 ToList
                {
                    combinedStream.method = nameof(Enumerable.ToList);
                    sqlToList = sqlTranslateService.PrepareQuery(arg, combinedStream);
                    dataReader = arg.dataReader;
                }

                // #2 TotalCount
                {
                    combinedStream.method = nameof(Enumerable.Count);
                    (combinedStream.orders, combinedStream.skip, combinedStream.take) = (null, null, null);

                    sqlCount = sqlTranslateService.PrepareCountQuery(arg, combinedStream);
                }

                // #3 read data
                {
                    var sql = sqlCount + " ;\r\n" + sqlToList;
                    using var reader = ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
                    reader.Read();
                    totalCount = Convert.ToInt32(reader[0]);
                    reader.NextResult();
                    list = dataReader.ReadData(reader);
                }
            }
            else
            {
                combinedStream.method = nameof(Enumerable.ToList);
                list = ExecuteQuery_ToList(combinedStream, resultEntityType);

                combinedStream.method = nameof(Queryable_Extensions.TotalCount);
                totalCount = ExecuteQuery_Count(combinedStream);
            }

            //combinedStream.method = nameof(Queryable_Extensions.ToListAndTotalCount);

            return new Func<object, int, (object, int)>(ValueTuple.Create<object, int>)
                .Method.GetGenericMethodDefinition()
                .MakeGenericMethod(list.GetType(), typeof(int))
                .Invoke(null, new[] { list, totalCount });
        }


        /// <summary>
        /// Queryable.Count or Queryable_Extensions.TotalCount
        /// </summary>
        /// <param name="combinedStream"></param>
        /// <returns></returns>
        protected virtual int ExecuteQuery_Count(CombinedStream combinedStream)
        {
            // deal with skip and take , no need to pass to PrepareCountQuery
            var queryArg = (combinedStream.orders, combinedStream.skip, combinedStream.take);
            (combinedStream.orders, combinedStream.skip, combinedStream.take) = (null, null, null);

            // get arg
            var arg = new QueryTranslateArgument(this, null);

            var sql = sqlTranslateService.PrepareCountQuery(arg, combinedStream);

            var countValue = ExecuteScalar(sql: sql, param: arg.sqlParam, useReadOnly: true);
            var count = Convert.ToInt32(countValue);
            if (count > 0 && combinedStream.method == nameof(Queryable.Count))
            {
                if (queryArg.skip > 0) count = Math.Max(count - queryArg.skip.Value, 0);

                if (queryArg.take.HasValue)
                    count = Math.Min(count, queryArg.take.Value);
            }

            (combinedStream.orders, combinedStream.skip, combinedStream.take) = queryArg;
            return count;
        }

        protected virtual object ExecuteQuery_ToList(CombinedStream combinedStream, Type resultEntityType)
        {
            var arg = new QueryTranslateArgument(this, resultEntityType);

            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            using var reader = ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return arg.dataReader.ReadData(reader);
        }

        #endregion



        // #3 Update: Update UpdateRange
        public override int Update<Entity>(Entity entity) => DbSet<Entity>().Update(entity);

        public override int UpdateRange<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().UpdateRange(entities);



        // #4 Delete : Delete DeleteRange DeleteByKey DeleteByKeys
        public override int Delete<Entity>(Entity entity) => DbSet<Entity>().Delete(entity);
        public override int DeleteRange<Entity>(IEnumerable<Entity> entities) => DbSet<Entity>().DeleteRange(entities);

        public override int DeleteByKey<Entity>(object keyValue) => DbSet<Entity>().DeleteByKey(keyValue);
        public override int DeleteByKeys<Entity, Key>(IEnumerable<Key> keys) => DbSet<Entity>().DeleteByKeys<Key>(keys);




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
