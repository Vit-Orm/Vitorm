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
    public partial class ToListAndTotalCountAsync : IQueryExecutor
    {
        public static readonly ToListAndTotalCountAsync Instance = new();

        public string methodName => nameof(Queryable_AsyncExtensions.ToListAndTotalCountAsync);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            // Task<(List<Result> list, int totalCount)>
            var resultEntityType = execArg.expression.Type.GetGenericArguments().FirstOrDefault().GetGenericArguments().FirstOrDefault().GetGenericArguments().FirstOrDefault();
            return Execute_MethodInfo(resultEntityType).Invoke(null, new object[] { execArg });
        }


        static async Task<(List<Result> list, int totalCount)> Execute<Result>(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            var originMethod = combinedStream.method;

            List<Result> list; int totalCount;

            if (dbContext.query_ToListAndTotalCount_InvokeInOneExecute)
            {
                var resultEntityType = typeof(Result);
                var arg = new QueryTranslateArgument(dbContext, resultEntityType);

                string sqlToList, sqlCount;
                IDbDataReader dataReader;

                // #1 TotalCount
                {
                    combinedStream.method = nameof(Enumerable.Count);
                    var originParams = (combinedStream.orders, combinedStream.skip, combinedStream.take);
                    (combinedStream.orders, combinedStream.skip, combinedStream.take) = (null, null, null);

                    sqlCount = sqlTranslateService.PrepareCountQuery(arg, combinedStream);

                    (combinedStream.orders, combinedStream.skip, combinedStream.take) = originParams;
                }

                // #2 ToList
                {
                    combinedStream.method = nameof(Enumerable.ToList);
                    sqlToList = sqlTranslateService.PrepareQuery(arg, combinedStream);
                    dataReader = arg.dataReader;
                }

                // #3 read data
                {
                    var sql = sqlCount + " ;\r\n" + sqlToList;
                    using var reader = await dbContext.ExecuteReaderAsync(sql: sql, param: arg.sqlParam, useReadOnly: true);
                    reader.Read();
                    totalCount = Convert.ToInt32(reader[0]);
                    reader.NextResult();
                    list = (List<Result>)dataReader.ReadData(reader);
                }
            }
            else
            {
                combinedStream.method = nameof(Enumerable.ToList);
                list = await ToListAsync.Execute<Result>(execArg);

                combinedStream.method = nameof(Queryable_Extensions.TotalCount);
                totalCount = await CountAsync.Execute(execArg);
            }

            combinedStream.method = originMethod;

            return (list, totalCount);
        }


        private static MethodInfo Execute_MethodInfo_;
        static MethodInfo Execute_MethodInfo(Type entityType) =>
            (Execute_MethodInfo_ ??= new Func<QueryExecutorArgument, Task<(List<string> list, int totalCount)>>(Execute<string>).Method.GetGenericMethodDefinition())
            .MakeGenericMethod(entityType);

    }
}
