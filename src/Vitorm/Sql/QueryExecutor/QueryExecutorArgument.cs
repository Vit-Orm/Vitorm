using System;
using System.Linq.Expressions;

using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public class QueryExecutorArgument
    {
        public CombinedStream combinedStream;
        public SqlDbContext dbContext;

        public Expression expression;
        public Type expressionResultType;
    }

}
