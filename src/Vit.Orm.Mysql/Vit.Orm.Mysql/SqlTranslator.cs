using System;
using System.Collections.Generic;

using Vit.Extensions.Linq_Extensions;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Entity;
using Vit.Orm.Mysql.Translator;


namespace Vit.Orm.Mysql
{
    public class SqlTranslator : Vit.Orm.Sql.Translator.SqlTranslator
    {
        public SqlTranslator(DbContext dbContext):base(dbContext) 
        {
        }

        #region PrepareCreate
        public override string PrepareCreate(IEntityDescriptor entityDescriptor)
        {
            /* //sql
CREATE TABLE user (
  id int NOT NULL PRIMARY KEY AUTO_INCREMENT,
  name varchar(100) DEFAULT NULL,
  birth date DEFAULT NULL,
  fatherId int DEFAULT NULL,
  motherId int DEFAULT NULL
) ;
              */
            List<string> sqlFields = new();

            // #1 primary key
            sqlFields.Add(GetColumnSql(entityDescriptor.key) + " PRIMARY KEY AUTO_INCREMENT");

            // #2 columns
            entityDescriptor.columns?.ForEach(column => sqlFields.Add(GetColumnSql(column)));

            return $@"
CREATE TABLE {DelimitIdentifier(entityDescriptor.tableName)} (
{string.Join(",\r\n  ", sqlFields)}
)";


            string GetColumnSql(IColumnDescriptor column)
            {
                bool nullable = false;

                var type = column.type;
                if (type.IsGenericType)
                {
                    nullable = true;
                    type = type.GetGenericArguments()[0];
                }
                // name varchar(100) DEFAULT NULL
                return $"  {DelimitIdentifier(column.name)} {GetDbType(type)} {(nullable ? "DEFAULT NULL" : "NOT NULL")}";
            }
        }
        protected override string GetDbType(Type type)
        {
            if (type == typeof(DateTime))
                return "DATETIME";

            if (type == typeof(string))
                return "TEXT";

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "REAL";

            if (type == typeof(bool) || type.Name.ToLower().Contains("int")) return "INTEGER";

            throw new NotSupportedException("unsupported column type:" + type.Name);
        }
        #endregion


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
