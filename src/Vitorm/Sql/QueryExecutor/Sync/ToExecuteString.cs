using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ToExecuteString : IQueryExecutor
    {
        public static readonly ToExecuteString Instance = new();

        public string methodName => nameof(Orm_Extensions.ToExecuteString);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var arg = new QueryTranslateArgument(dbContext, null);
            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            dbContext.Event_OnExecuting(sql, arg.sqlParam);

            return sql;
        }

    }
}
