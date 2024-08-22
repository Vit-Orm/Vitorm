using System.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class FirstOrDefault : IQueryExecutor
    {
        public static readonly FirstOrDefault Instance = new();

        public string methodName => nameof(Queryable.FirstOrDefault);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var resultEntityType = execArg.expression.Type;
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);
            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            // #3 Execute
            using var reader = dbContext.ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return arg.dataReader.ReadData(reader);
        }

    }
}
