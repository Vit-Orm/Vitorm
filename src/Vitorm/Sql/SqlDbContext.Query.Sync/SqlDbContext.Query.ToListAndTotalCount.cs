using System;
using System.Linq;
using System.Reflection;

using Vit.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {
        protected bool query_ToListAndTotalCount_InvokeInOneExecute = true;
        static object Query_ToListAndTotalCount(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            var originMethod = combinedStream.method;
            var resultEntityType = execArg.expression.Type.GetGenericArguments()?.FirstOrDefault()?.GetGenericArguments()?.FirstOrDefault();

            object list; int totalCount;

            if (dbContext.query_ToListAndTotalCount_InvokeInOneExecute)
            {
                // get arg
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
                    using var reader = dbContext.ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
                    reader.Read();
                    totalCount = Convert.ToInt32(reader[0]);
                    reader.NextResult();
                    list = dataReader.ReadData(reader);
                }
            }
            else
            {
                combinedStream.method = nameof(Enumerable.ToList);
                list = Query_ToList(execArg, resultEntityType);

                combinedStream.method = nameof(Queryable_Extensions.TotalCount);
                totalCount = ExecuteQuery_Count(execArg);
            }

            combinedStream.method = originMethod;

            return Query_ToListAndTotalCount_MethodInfo(list.GetType(), typeof(int))
                .Invoke(null, new[] { list, totalCount });
        }


        private static MethodInfo Query_ToListAndTotalCount_MethodInfo_;
        static MethodInfo Query_ToListAndTotalCount_MethodInfo(Type type1, Type type2) =>
            (Query_ToListAndTotalCount_MethodInfo_ ??= new Func<object, int, (object, int)>(ValueTuple.Create<object, int>).Method.GetGenericMethodDefinition())
            .MakeGenericMethod(type1, type2);

    }
}
