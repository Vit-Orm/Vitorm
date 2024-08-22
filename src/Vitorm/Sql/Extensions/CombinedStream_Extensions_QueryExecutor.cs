using Vitorm.Sql.QueryExecutor;
using Vitorm.StreamQuery;

namespace Vitorm
{
    public static partial class CombinedStream_Extensions_QueryExecutor
    {
        public static IQueryExecutor GetQueryExecutor(this CombinedStream data)
            => data?.GetExtraArg("QueryExecutor") as IQueryExecutor;

        public static CombinedStream SetQueryExecutor(this CombinedStream data, IQueryExecutor queryExecutor)
            => data?.SetExtraArg("QueryExecutor", queryExecutor);

    }

}