using System;
using System.Linq;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    /// <summary>
    /// Queryable.Count or Queryable_Extensions.TotalCount
    /// </summary>
    public partial class CountAsync : IQueryExecutor
    {
        public static readonly CountAsync Instance = new();

        public string methodName => nameof(Queryable_AsyncExtensions.CountAsync);

        public object ExecuteQuery(QueryExecutorArgument execArg) => Execute(execArg);


        public static async Task<int> Execute(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;
            var queryArg = (combinedStream.orders, combinedStream.skip, combinedStream.take);

            // #2 Prepare sql
            // deal with skip and take , no need to pass to PrepareCountQuery
            var arg = new QueryTranslateArgument(dbContext, null);
            (combinedStream.orders, combinedStream.skip, combinedStream.take) = (null, null, null);
            string method = combinedStream.method;
            combinedStream.method = nameof(Queryable.Count);
            var sql = sqlTranslateService.PrepareCountQuery(arg, combinedStream);
            combinedStream.method = method;

            // #3 Execute
            var countValue = await dbContext.ExecuteScalarAsync(sql: sql, parameters: arg.sqlParam, useReadOnly: true);
            var count = Convert.ToInt32(countValue);

            // Count and TotalCount
            if (count > 0 && method == nameof(Queryable_AsyncExtensions.CountAsync))
            {
                if (queryArg.skip > 0) count = Math.Max(count - queryArg.skip.Value, 0);

                if (queryArg.take.HasValue)
                    count = Math.Min(count, queryArg.take.Value);
            }

            (combinedStream.orders, combinedStream.skip, combinedStream.take) = queryArg;
            return count;
        }

    }
}
