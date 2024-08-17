namespace Vitorm.Sql
{
    public interface IQueryExecutor
    {
        public string methodName { get; }
        object ExecuteQuery(QueryExecutorArgument execArg);
    }
}
