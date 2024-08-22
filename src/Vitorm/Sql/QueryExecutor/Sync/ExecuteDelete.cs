using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ExecuteDelete : IQueryExecutor
    {
        public static readonly ExecuteDelete Instance = new();

        public string methodName => nameof(Orm_Extensions.ExecuteDelete);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var entityType = (combinedStream.source as SourceStream)?.GetEntityType();
            var arg = new QueryTranslateArgument(dbContext, entityType);
            var sql = sqlTranslateService.PrepareExecuteDelete(arg, combinedStream);

            // #3 Execute
            return dbContext.Execute(sql: sql, param: arg.sqlParam);
        }

    }
}
