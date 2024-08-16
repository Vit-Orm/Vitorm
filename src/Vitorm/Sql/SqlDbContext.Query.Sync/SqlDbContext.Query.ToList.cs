using System;
using System.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        static object Query_ToList(QueryExecutorArgument execArg)
        {
            var resultEntityType = execArg.expression.Type.GetGenericArguments()?.FirstOrDefault();
            return Query_ToList(execArg, resultEntityType);
        }


        static object Query_ToList(QueryExecutorArgument execArg, Type resultEntityType)
        {
            CombinedStream combinedStream = execArg.combinedStream;
            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            var arg = new QueryTranslateArgument(dbContext, resultEntityType);

            var sql = sqlTranslateService.PrepareQuery(arg, combinedStream);

            using var reader = dbContext.ExecuteReader(sql: sql, param: arg.sqlParam, useReadOnly: true);
            return arg.dataReader.ReadData(reader);
        }

    }
}
