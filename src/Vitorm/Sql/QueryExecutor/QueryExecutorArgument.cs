using System;
using System.Linq.Expressions;

using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public class QueryExecutorArgument : IDisposable
    {
        public CombinedStream combinedStream;
        public SqlDbContext dbContext;

        public Expression expression;
        public Type expressionResultType;

        public Action dispose;

        public void Dispose()
        {
            dispose?.Invoke();
        }
    }

}
