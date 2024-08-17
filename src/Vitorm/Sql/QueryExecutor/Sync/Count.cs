using System;
using System.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    /// <summary>
    /// Queryable.Count or Queryable_Extensions.TotalCount
    /// </summary>
    public partial class Count : IQueryExecutor
    {
        public static readonly Count Instance = new();

        public string methodName => nameof(Queryable.Count);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            return Execute(execArg);
        }


        public static int Execute(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            // deal with skip and take , no need to pass to PrepareCountQuery
            var queryArg = (combinedStream.orders, combinedStream.skip, combinedStream.take, combinedStream.method);

            (combinedStream.orders, combinedStream.skip, combinedStream.take, combinedStream.method) = (null, null, null, nameof(Queryable.Count));
            var arg = new QueryTranslateArgument(dbContext, null);
            var sql = sqlTranslateService.PrepareCountQuery(arg, combinedStream);

            // #3 Execute
            var countValue = dbContext.ExecuteScalar(sql: sql, param: arg.sqlParam, useReadOnly: true);
            var count = Convert.ToInt32(countValue);

            // Count and TotalCount
            if (count > 0 && queryArg.method == nameof(Queryable.Count))
            {
                if (queryArg.skip > 0) count = Math.Max(count - queryArg.skip.Value, 0);

                if (queryArg.take.HasValue)
                    count = Math.Min(count, queryArg.take.Value);
            }

            (combinedStream.orders, combinedStream.skip, combinedStream.take, combinedStream.method) = queryArg;
            return count;
        }

    }
}
