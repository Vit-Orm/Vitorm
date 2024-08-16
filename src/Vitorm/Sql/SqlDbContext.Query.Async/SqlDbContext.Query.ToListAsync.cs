using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        static object Query_ToListAsync(QueryExecutorArgument execArg)
        {
            var resultEntityType = execArg.expression.Type.GetGenericArguments()?.FirstOrDefault().GetGenericArguments()?.FirstOrDefault();
            return Query_ToListAsync_MethodInfo(resultEntityType).Invoke(null, new object[] { execArg });
        }


        static async Task<List<Entity>> Query_ToListAsync<Entity>(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            var arg = new QueryTranslateArgument(dbContext, typeof(Entity));

            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            using var reader = await dbContext.ExecuteReaderAsync(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return (List<Entity>)arg.dataReader.ReadData(reader);
        }


        private static MethodInfo Query_ToListAsync_MethodInfo_;
        static MethodInfo Query_ToListAsync_MethodInfo(Type entityType) =>
            (Query_ToListAsync_MethodInfo_ ??= new Func<QueryExecutorArgument, Task<List<string>>>(Query_ToListAsync<string>).Method.GetGenericMethodDefinition())
            .MakeGenericMethod(entityType);

    }
}
