using System;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql
{
    public partial class SqlDbContext : DbContext
    {

        static object Query_ExecuteUpdate(QueryExecutorArgument execArg)
        {
            CombinedStream combinedStream = execArg.combinedStream;


            if (combinedStream is not StreamToUpdate streamToUpdate) throw new NotSupportedException("not supported query type: " + combinedStream.method);


            var dbContext = execArg.dbContext;
            var sqlTranslateService = dbContext.sqlTranslateService;

            // get arg
            var resultEntityType = streamToUpdate.fieldsToUpdate.New_GetType();
            var arg = new QueryTranslateArgument(dbContext, resultEntityType);

            var sql = sqlTranslateService.PrepareExecuteUpdate(arg, streamToUpdate);

            return dbContext.Execute(sql: sql, param: arg.sqlParam);
        }
    }
}
