
using FuncQueryExecutor = System.Func<Vitorm.Sql.QueryExecutorArgument, object>;

namespace Vitorm.Sql
{
    public class QueryExecutor_Delegate : IQueryExecutor
    {
        public QueryExecutor_Delegate(string methodName, FuncQueryExecutor queryExecutor)
        {
            this.methodName = methodName;
            this.queryExecutor = queryExecutor;
        }
        public string methodName { get; set; }
        FuncQueryExecutor queryExecutor;
        public object ExecuteQuery(QueryExecutorArgument execArg) => queryExecutor(execArg);
    }
}
