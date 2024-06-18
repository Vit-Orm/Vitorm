using System;
using System.Collections.Generic;
using System.Linq;

using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sqlite.TranslateService
{
    public class ExecuteUpdateTranslateService : BaseQueryTranslateService
    {
        /*

-- multiple
WITH tmp AS (
    select   ('u' || u.id || '_' || COALESCE(father.id,'') ) as _name , u.id 
    from User u
    left join User father on u.fatherId = father.id 
    where u.id > 0
)
UPDATE User  
  SET name =  ( SELECT _name FROM tmp WHERE tmp.id =User.id )
where id in ( SELECT id FROM tmp );


--- single
UPDATE User SET name = 'u'||id  where id > 0;
         */
        public override string BuildQuery(QueryTranslateArgument arg, CombinedStream stream)
        {
            var sqlInner = base.BuildQuery(arg, stream);


            var entityDescriptor = arg.dbContext.GetEntityDescriptor(arg.resultEntityType);
            var columnsToUpdate = (stream as StreamToUpdate)?.fieldsToUpdate?.memberArgs;

            var NewLine = "\r\n";
            var keyName = entityDescriptor.keyName;


            var sql = $"WITH tmp AS ( {NewLine}";
            sql += sqlInner;

            sql += $"{NewLine}){NewLine}";
            sql += $"UPDATE {sqlTranslator.DelimitTableName(entityDescriptor)}{NewLine}";
            sql += $"Set ";

            var sqlToUpdateCols = columnsToUpdate
                .Select(m => m.name)
                .Select(name => $"{NewLine}  {sqlTranslator.DelimitIdentifier(name)} = (SELECT {sqlTranslator.DelimitIdentifier("_" + name)} FROM tmp WHERE tmp.{sqlTranslator.DelimitIdentifier(keyName)} = {sqlTranslator.DelimitTableName(entityDescriptor)}.{sqlTranslator.DelimitIdentifier(keyName)} )");

            sql += string.Join(",", sqlToUpdateCols);

            sql += $"{NewLine}where {sqlTranslator.DelimitIdentifier(keyName)} in ( SELECT {sqlTranslator.DelimitIdentifier(keyName)} FROM tmp ); {NewLine}";

            return sql;
        }
 

        public ExecuteUpdateTranslateService(SqlTranslateService sqlTranslator) : base(sqlTranslator)
        {
        }

        protected override string ReadSelect(QueryTranslateArgument arg, CombinedStream stream, string prefix = "select")
        {
            var entityDescriptor = arg.dbContext.GetEntityDescriptor(arg.resultEntityType);
            var columnsToUpdate = (stream as StreamToUpdate) ?.fieldsToUpdate?.memberArgs;

            if (columnsToUpdate?.Any() != true) throw new ArgumentException("can not get columns to update");

            var sqlFields = new List<string>();

            foreach (var column in columnsToUpdate)
            {
                sqlFields.Add($"({sqlTranslator.EvalExpression( arg,  column.value)}) as {sqlTranslator.DelimitIdentifier("_" + column.name)}");
            }

            // primary key
            sqlFields.Add($"{sqlTranslator.GetSqlField(stream.source.alias, entityDescriptor.keyName)} as {sqlTranslator.DelimitIdentifier(entityDescriptor.keyName)}");

            return prefix + " " + String.Join(",", sqlFields);
        }



    }
}
