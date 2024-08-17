using System;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ExecuteDeleteAsync : IQueryExecutor
    {
        public static readonly ExecuteDeleteAsync Instance = new();

        public string methodName => nameof(Orm_Extensions.ExecuteDeleteAsync);

        public object ExecuteQuery(QueryExecutorArgument execArg) => Execute(execArg);

        public async Task<int> Execute(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var entityType = (combinedStream.source as SourceStream)?.GetEntityType();
            var arg = new QueryTranslateArgument(dbContext, entityType);
            string method = combinedStream.method;
            combinedStream.method = nameof(Orm_Extensions.ExecuteDelete);
            var sql = sqlTranslateService.PrepareExecuteDelete(arg, combinedStream);
            combinedStream.method = method;

            // #3 Execute
            return await dbContext.ExecuteAsync(sql: sql, param: arg.sqlParam);
        }
    }
}
