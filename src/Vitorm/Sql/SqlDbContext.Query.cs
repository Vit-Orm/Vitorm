using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.Sql.QueryExecutor;
using Vitorm.StreamQuery;

using StreamReader = Vitorm.StreamQuery.StreamReader;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        public Action<SqlDbContext, Expression, Type, object> AfterQuery;
        public virtual SqlDbContext AutoDisposeAfterQuery()
        {
            AfterQuery += (_, _, _, _) => Dispose();
            return this;
        }


        public override IQueryable<Entity> Query<Entity>()
        {
            return QueryableBuilder.Build<Entity>(QueryExecutor, dbGroupName);
        }

        protected object QueryExecutor(Expression expression, Type expressionResultType)
        {
            object result = null;
            Action dispose = () => AfterQuery?.Invoke(this, expression, expressionResultType, result);
            try
            {
                return result = ExecuteQuery(expression, expressionResultType, dispose);
            }
            catch
            {
                dispose();
                throw;
            }
        }

        #region QueryExecutor

        public static Dictionary<string, IQueryExecutor> defaultQueryExecutors = CreateDefaultQueryExecutors();
        public static Dictionary<string, IQueryExecutor> CreateDefaultQueryExecutors()
        {
            Dictionary<string, IQueryExecutor> defaultQueryExecutors = new();

            #region AddDefaultQueryExecutor
            void AddDefaultQueryExecutor(IQueryExecutor queryExecutor, string methodName = null)
            {
                defaultQueryExecutors[methodName ?? queryExecutor.methodName] = queryExecutor;
            }
            #endregion


            #region Sync
            // Orm_Extensions
            AddDefaultQueryExecutor(ExecuteUpdate.Instance);
            AddDefaultQueryExecutor(ExecuteDelete.Instance);
            AddDefaultQueryExecutor(ToExecuteString.Instance);

            // ToList
            AddDefaultQueryExecutor(ToList.Instance);
            // Count TotalCount
            AddDefaultQueryExecutor(Count.Instance);
            AddDefaultQueryExecutor(Count.Instance, methodName: nameof(Queryable_Extensions.TotalCount));

            // ToListAndTotalCount
            AddDefaultQueryExecutor(ToListAndTotalCount.Instance);

            // FirstOrDefault First LastOrDefault Last
            AddDefaultQueryExecutor(FirstOrDefault.Instance);
            AddDefaultQueryExecutor(FirstOrDefault.Instance, methodName: nameof(Queryable.First));
            AddDefaultQueryExecutor(FirstOrDefault.Instance, methodName: nameof(Queryable.LastOrDefault));
            AddDefaultQueryExecutor(FirstOrDefault.Instance, methodName: nameof(Queryable.Last));
            #endregion


            #region Async
            // Orm_Extensions
            AddDefaultQueryExecutor(ExecuteUpdateAsync.Instance);
            AddDefaultQueryExecutor(ExecuteDeleteAsync.Instance);

            // ToList
            AddDefaultQueryExecutor(ToListAsync.Instance);
            // Count TotalCount
            AddDefaultQueryExecutor(CountAsync.Instance);
            AddDefaultQueryExecutor(CountAsync.Instance, methodName: nameof(Queryable_AsyncExtensions.TotalCountAsync));

            // ToListAndTotalCount
            AddDefaultQueryExecutor(ToListAndTotalCountAsync.Instance);

            // FirstOrDefault First LastOrDefault Last
            AddDefaultQueryExecutor(FirstOrDefaultAsync.Instance);
            AddDefaultQueryExecutor(FirstOrDefaultAsync.Instance, methodName: nameof(Queryable_AsyncExtensions.FirstAsync));
            AddDefaultQueryExecutor(FirstOrDefaultAsync.Instance, methodName: nameof(Queryable_AsyncExtensions.LastOrDefaultAsync));
            AddDefaultQueryExecutor(FirstOrDefaultAsync.Instance, methodName: nameof(Queryable_AsyncExtensions.LastAsync));
            #endregion

            return defaultQueryExecutors;
        }

        public Dictionary<string, IQueryExecutor> queryExecutors = defaultQueryExecutors;

        #endregion


        #region StreamReader
        public static StreamReader defaultStreamReader = new StreamReader();
        public StreamReader streamReader = defaultStreamReader;
        #endregion


        public bool query_ToListAndTotalCount_InvokeInOneExecute = true;

        protected virtual bool QueryIsFromSameDb(object obj, Type elementType)
        {
            if (obj is not IQueryable query) return false;

            if (dbGroupName == QueryableBuilder.GetQueryConfig(query) as string) return true;

            throw new InvalidOperationException("do not allow to use queryable from different datasource , queryable type: " + obj?.GetType().FullName);
            //return false;
        }
        protected virtual object ExecuteQuery(Expression expression, Type expressionResultType, Action dispose)
        {
            // #1 convert to ExpressionNode 
            ExpressionNode_Lambda node = convertService.ConvertToData_LambdaNode(expression, autoReduce: true, isArgument: QueryIsFromSameDb);
            //var strNode = Json.Serialize(node);


            // #2 convert to Stream
            var stream = streamReader.ReadFromNode(node);
            //var strStream = Json.Serialize(stream);

            if (stream is not CombinedStream combinedStream) combinedStream = new CombinedStream("tmp") { source = stream };

            var executorArg = new QueryExecutorArgument
            {
                combinedStream = combinedStream,
                dbContext = this,
                expression = expression,
                expressionResultType = expressionResultType,
                dispose = dispose,
            };


            #region #3 Execute by executor from CombinedStream
            {
                var queryExecutor = combinedStream.GetQueryExecutor();
                if (queryExecutor != null)
                    return queryExecutor.ExecuteQuery(executorArg);
            }
            #endregion

            #region #4 Execute by registered executor
            {
                var method = combinedStream.method;
                if (string.IsNullOrWhiteSpace(method)) method = nameof(Enumerable.ToList);
                if (queryExecutors.TryGetValue(method, out var queryExecutor))
                {
                    return queryExecutor.ExecuteQuery(executorArg);
                }
            }
            #endregion

            throw new NotSupportedException("not supported query method: " + combinedStream.method);
        }




    }
}
