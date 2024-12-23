using System;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ExecuteUpdateAsync : IQueryExecutor
    {
        public static readonly ExecuteUpdateAsync Instance = new();

        public string methodName => nameof(Orm_Extensions.ExecuteUpdateAsync);

        public object ExecuteQuery(QueryExecutorArgument execArg) => Execute(execArg);

        public async Task<int> Execute(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;

            if (combinedStream is not StreamToUpdate streamToUpdate) throw new NotSupportedException("not supported query type: " + combinedStream.method);

            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var resultEntityType = streamToUpdate.fieldsToUpdate.New_GetType();
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);
            string method = combinedStream.method;
            combinedStream.method = nameof(Orm_Extensions.ExecuteDelete);
            var sql = sqlTranslateService.PrepareExecuteUpdate(arg, streamToUpdate);
            combinedStream.method = method;

            // #3 Execute
            return await dbContext.ExecuteAsync(sql: sql, parameters: arg.sqlParam);
        }
    }
}
