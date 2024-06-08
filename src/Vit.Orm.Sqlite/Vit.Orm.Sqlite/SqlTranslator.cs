using System.Collections.Generic;

using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Sqlite.Translator;


namespace Vit.Orm.Sqlite
{
    public class SqlTranslator : Vit.Orm.Sql.Translator.SqlTranslator
    {
        public SqlTranslator(DbContext dbContext):base(dbContext) 
        {
        }

        public override (string sql, Dictionary<string, object> sqlParam) PrepareExecuteUpdate(CombinedStream combinedStream)
        {
            var query = new ExecuteUpdateTranslator(this);
            string sql = query.BuildQuery(combinedStream);
            return (sql, query.sqlParam);
        }

        public override (string sql, Dictionary<string, object> sqlParam) PrepareExecuteDelete(CombinedStream combinedStream)
        {
            var query = new ExecuteDeleteTranslator(this);
            string sql = query.BuildQuery(combinedStream);
            return (sql, query.sqlParam);
        }



    }
}
