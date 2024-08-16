using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.StreamQuery;

using QueryExecutor = System.Func<Vitorm.Sql.SqlDbContext.QueryExecutorArgument, object>;
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
            try
            {
                return result = ExecuteQuery(expression, expressionResultType);
            }
            finally
            {
                AfterQuery?.Invoke(this, expression, expressionResultType, result);
            }
        }



        #region QueryExecutor

        public class QueryExecutorArgument
        {
            public CombinedStream combinedStream;
            public SqlDbContext dbContext;

            public Expression expression;
            public Type expressionResultType;
        }


        public static Dictionary<string, QueryExecutor> defaultQueryExecutors = new Dictionary<string, QueryExecutor>()
        {
            #region Sync
            [nameof(Orm_Extensions.ExecuteUpdate)] = Query_ExecuteUpdate,
            [nameof(Orm_Extensions.ExecuteDelete)] = Query_ExecuteDelete,
            [nameof(Orm_Extensions.ToExecuteString)] = Query_ToExecuteString,

            [nameof(Queryable.Count)] = Query_Count,
            [nameof(Queryable_Extensions.TotalCount)] = Query_Count,

            [nameof(Enumerable.ToList)] = Query_ToList,

            [nameof(Queryable_Extensions.ToListAndTotalCount)] = Query_ToListAndTotalCount,

            [nameof(Queryable.FirstOrDefault)] = Query_First,
            [nameof(Queryable.First)] = Query_First,
            [nameof(Queryable.LastOrDefault)] = Query_First,
            [nameof(Queryable.Last)] = Query_First,
            #endregion


            #region Async
            [nameof(Queryable_Extensions.ToListAsync)] = Query_ToListAsync,

            #endregion

        };


        public Dictionary<string, QueryExecutor> queryExecutors = defaultQueryExecutors;

        #endregion


        #region StreamReader
        public static StreamReader defaultStreamReader = new StreamReader();
        public StreamReader streamReader = defaultStreamReader;
        #endregion



        protected bool QueryIsFromSameDb(object query, Type elementType)
        {
            return dbGroupName == QueryableBuilder.GetQueryConfig(query as IQueryable) as string;
        }
        protected virtual object ExecuteQuery(Expression expression, Type expressionResultType)
        {
            // #1 convert to ExpressionNode 
            ExpressionNode_Lambda node = convertService.ConvertToData_LambdaNode(expression, autoReduce: true, isArgument: QueryIsFromSameDb);
            //var strNode = Json.Serialize(node);


            // #2 convert to Stream
            var stream = streamReader.ReadFromNode(node);
            //var strStream = Json.Serialize(stream);


            // #3 Execute
            if (stream is not CombinedStream combinedStream) combinedStream = new CombinedStream("tmp") { source = stream };

            var method = combinedStream.method;
            if (string.IsNullOrWhiteSpace(method)) method = nameof(Enumerable.ToList);
            queryExecutors.TryGetValue(method, out var executor);

            if (executor != null)
                return executor(new QueryExecutorArgument
                {
                    combinedStream = combinedStream,
                    dbContext = this,
                    expression = expression,
                    expressionResultType = expressionResultType
                });

            throw new NotSupportedException("not supported query method: " + combinedStream.method);
        }




    }
}
