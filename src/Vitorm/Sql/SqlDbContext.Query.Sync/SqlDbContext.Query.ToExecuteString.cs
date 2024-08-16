using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        static object Query_ToExecuteString(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // get arg
            var arg = new QueryTranslateArgument(dbContext, null);

            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);
            return sql;
        }

    }
}
