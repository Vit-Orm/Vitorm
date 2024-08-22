using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Vit.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ToListAsync : IQueryExecutor
    {
        public static readonly ToListAsync Instance = new();

        public string methodName => nameof(Queryable_Extensions.ToListAsync);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            var resultEntityType = execArg.expression.Type.GetGenericArguments().FirstOrDefault().GetGenericArguments().FirstOrDefault();
            return Execute_MethodInfo(resultEntityType).Invoke(null, new object[] { execArg });
        }


        public static async Task<List<Result>> Execute<Result>(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var resultEntityType = typeof(Result);
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);
            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            // #3 Execute
            using var reader = await dbContext.ExecuteReaderAsync(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return (List<Result>)arg.dataReader.ReadData(reader);
        }


        private static MethodInfo Execute_MethodInfo_;
        static MethodInfo Execute_MethodInfo(Type entityType) =>
            (Execute_MethodInfo_ ??= new Func<QueryExecutorArgument, Task<List<string>>>(Execute<string>).Method.GetGenericMethodDefinition())
            .MakeGenericMethod(entityType);

    }
}
