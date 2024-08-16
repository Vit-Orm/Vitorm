using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        static object Query_ExecuteDelete(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;


            var entityType = (combinedStream.source as SourceStream)?.GetEntityType();
            var arg = new QueryTranslateArgument(dbContext, entityType);

            var sql = sqlTranslateService.PrepareExecuteDelete(arg, combinedStream);
            var count = dbContext.Execute(sql: sql, param: arg.sqlParam);
            return count;
        }

    }
}
