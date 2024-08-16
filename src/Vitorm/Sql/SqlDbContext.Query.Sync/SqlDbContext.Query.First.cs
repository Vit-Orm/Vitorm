using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        static object Query_First(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            var resultEntityType = execArg.expression.Type;
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);

            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);
            using var reader = dbContext.ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return arg.dataReader.ReadData(reader);
        }

    }
}
