using System;
using System.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.QueryExecutor
{
    public partial class ToList : IQueryExecutor
    {
        public static readonly ToList Instance = new();

        public string methodName => nameof(Enumerable.ToList);

        public object ExecuteQuery(QueryExecutorArgument execArg)
        {
            using var _ = execArg;

            var resultEntityType = execArg.expression.Type.GetGenericArguments()?.FirstOrDefault();
            return Execute(execArg, resultEntityType);
        }


        public static object Execute(QueryExecutorArgument execArg, Type resultEntityType)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // #2 Prepare sql
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);
            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            // #3 Execute
            using var reader = dbContext.ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return arg.dataReader.ReadData(reader);
        }

    }
}
