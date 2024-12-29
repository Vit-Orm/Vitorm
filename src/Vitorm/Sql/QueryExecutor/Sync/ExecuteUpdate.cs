using System;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ExecuteUpdate : IQueryExecutor
    {
        public static readonly ExecuteUpdate Instance = new();

        public string methodName => nameof(Orm_Extensions.ExecuteUpdate);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;

            if (combinedStream is not StreamToUpdate streamToUpdate) throw new NotSupportedException("not supported query type: " + combinedStream.method);

            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var resultEntityType = streamToUpdate.fieldsToUpdate.New_GetType();
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);
            var sql = sqlTranslateService.PrepareExecuteUpdate(arg, streamToUpdate);

            // #3 Execute
            return dbContext.Execute(sql: sql, parameters: arg.sqlParam);
        }
    }
}
