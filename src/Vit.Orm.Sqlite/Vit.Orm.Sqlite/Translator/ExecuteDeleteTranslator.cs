using System;
using System.Collections.Generic;

using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Entity;
using Vit.Orm.Sql.Translator;

namespace Vit.Orm.Sqlite.Translator
{
    public class ExecuteDeleteTranslator : BaseQueryTranslator
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
        public override string BuildQuery(CombinedStream stream)
        {
            var sqlInner = base.BuildQuery(stream);


            var NewLine = "\r\n";
            var keyName = entityDescriptor.keyName;
            var tableName = entityDescriptor.tableName;


            var sql = $"WITH tmp AS ( {NewLine}";
            sql += sqlInner;

            sql += $"{NewLine}){NewLine}";
            sql += $"delete from {sqlTranslator.DelimitIdentifier(tableName)} ";

            sql += $"{NewLine}where {sqlTranslator.DelimitIdentifier(keyName)} in ( SELECT {sqlTranslator.DelimitIdentifier(keyName)} FROM tmp ); {NewLine}";

            return sql;
        }



        IEntityDescriptor entityDescriptor;

        public ExecuteDeleteTranslator(SqlTranslator sqlTranslator) : base(sqlTranslator)
        {
        }

        protected override string ReadSelect(CombinedStream stream)
        {
            var entityType = (stream.source as SourceStream)?.GetEntityType();
            entityDescriptor = sqlTranslator.GetEntityDescriptor(entityType);
            if (entityDescriptor == null) throw new ArgumentException("Entity can not be deleted");

            var sqlFields = new List<string>();

            // primary key
            sqlFields.Add($"{sqlTranslator.GetSqlField(stream.source.alias, entityDescriptor.keyName)} as `{entityDescriptor.keyName}`");
            return String.Join(",", sqlFields);
        }



    }
}
