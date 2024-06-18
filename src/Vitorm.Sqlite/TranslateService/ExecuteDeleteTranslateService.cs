using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sqlite.TranslateService
{
    public class ExecuteDeleteTranslateService : BaseQueryTranslateService
    {
        /*
WITH tmp AS (
    select u.id 
    from User u
    left join User father on u.fatherId = father.id 
    where u.id > 0
)
delete from User where id in ( SELECT id FROM tmp );
         */
        public override string BuildQuery(QueryTranslateArgument arg, CombinedStream stream)
        {
            var entityDescriptor = arg.dbContext.GetEntityDescriptor(arg.resultEntityType);

            var sqlInner = base.BuildQuery(arg, stream);


            var NewLine = "\r\n";
            var keyName = entityDescriptor.keyName;


            var sql = $"WITH tmp AS ( {NewLine}";
            sql += sqlInner;

            sql += $"{NewLine}){NewLine}";
            sql += $"delete from {sqlTranslator.DelimitTableName(entityDescriptor)} ";

            sql += $"{NewLine}where {sqlTranslator.DelimitIdentifier(keyName)} in ( SELECT {sqlTranslator.DelimitIdentifier(keyName)} FROM tmp ); {NewLine}";

            return sql;
        }


        public ExecuteDeleteTranslateService(SqlTranslateService sqlTranslator) : base(sqlTranslator)
        {
        }

        protected override string ReadSelect(QueryTranslateArgument arg, CombinedStream stream, string prefix = "select")
        {
            var entityDescriptor = arg.dbContext.GetEntityDescriptor(arg.resultEntityType);

            // primary key
            return $"{prefix} {sqlTranslator.GetSqlField(stream.source.alias, entityDescriptor.keyName)} as {sqlTranslator.DelimitIdentifier(entityDescriptor.keyName)}";
        }



    }
}
