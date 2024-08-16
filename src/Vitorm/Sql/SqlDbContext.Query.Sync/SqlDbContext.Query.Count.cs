using System;
using System.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        /// <summary>
        /// Queryable.Count or Queryable_Extensions.TotalCount
        /// </summary>
        /// <param name="execArg"></param>
        /// <returns></returns>
        static object Query_Count(QueryExecutorArgument execArg)
        {
            return ExecuteQuery_Count(execArg);
        }


        static int ExecuteQuery_Count(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // deal with skip and take , no need to pass to PrepareCountQuery
            var queryArg = (combinedStream.orders, combinedStream.skip, combinedStream.take);
            (combinedStream.orders, combinedStream.skip, combinedStream.take) = (null, null, null);

            // get arg
            var arg = new QueryTranslateArgument(dbContext, null);

            var sql = sqlTranslateService.PrepareCountQuery(arg, combinedStream);

            var countValue = dbContext.ExecuteScalar(sql: sql, param: arg.sqlParam, useReadOnly: true);
            var count = Convert.ToInt32(countValue);
            if (count > 0 && combinedStream.method == nameof(Queryable.Count))
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
