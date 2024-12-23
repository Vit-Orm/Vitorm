using System;
using System.Linq;
using System.Reflection;

using Vit.Linq;

using Vitorm.Sql.DataReader;
using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ToListAndTotalCount : IQueryExecutor
    {
        public static readonly ToListAndTotalCount Instance = new();

        public string methodName => nameof(Queryable_Extensions.ToListAndTotalCount);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            var originMethod = combinedStream.method;
            var resultEntityType = execArg.expression.Type.GetGenericArguments().FirstOrDefault().GetGenericArguments().FirstOrDefault();

            object list; int totalCount;

            if (dbContext.query_ToListAndTotalCount_InvokeInOneExecute)
            {
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
                    using var reader = dbContext.ExecuteReader(sql: sql, parameters: arg.sqlParam, useReadOnly: true);
                    reader.Read();
                    totalCount = Convert.ToInt32(reader[0]);
                    reader.NextResult();
                    list = dataReader.ReadData(reader);
                }
            }
            else
            {
                combinedStream.method = nameof(Enumerable.ToList);
                list = ToList.Execute(execArg, resultEntityType);

                combinedStream.method = nameof(Queryable_Extensions.TotalCount);
                totalCount = Count.Execute(execArg);
            }

            combinedStream.method = originMethod;

            return Tuple_MethodInfo(list.GetType(), typeof(int))
                .Invoke(null, new[] { list, totalCount });
        }


        private static MethodInfo Tuple_MethodInfo_;
        static MethodInfo Tuple_MethodInfo(Type type1, Type type2) =>
            (Tuple_MethodInfo_ ??= new Func<object, int, (object, int)>(ValueTuple.Create<object, int>).Method.GetGenericMethodDefinition())
            .MakeGenericMethod(type1, type2);

    }
}
