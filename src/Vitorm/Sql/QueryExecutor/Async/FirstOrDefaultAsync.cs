using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class FirstOrDefaultAsync : IQueryExecutor
    {
        public static readonly FirstOrDefaultAsync Instance = new();

        public string methodName => nameof(Queryable_AsyncExtensions.FirstOrDefaultAsync);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            var resultEntityType = execArg.expression.Type.GetGenericArguments().FirstOrDefault();
            return Execute_MethodInfo(resultEntityType).Invoke(null, new object[] { execArg });
        }

        public static async Task<Result> Execute<Result>(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var resultEntityType = typeof(Result);
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);

            var method = combinedStream.method;
            combinedStream.method = method.Substring(0, method.Length - "Async".Length);
            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);
            combinedStream.method = method;

            // #3 Execute
            using var reader = await dbContext.ExecuteReaderAsync(sql: sql, param: arg.sqlParam, useReadOnly: true);

            return (Result)arg.dataReader.ReadData(reader);
        }

        private static MethodInfo Execute_MethodInfo_;
        static MethodInfo Execute_MethodInfo(Type entityType) =>
            (Execute_MethodInfo_ ??= new Func<QueryExecutorArgument, Task<string>>(Execute<string>).Method.GetGenericMethodDefinition())
            .MakeGenericMethod(entityType);

    }
}
