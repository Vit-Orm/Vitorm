using System;
using System.Collections.Generic;

using Vit.Extensions.Linq_Extensions;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Entity;
using Vit.Orm.Sql;
using Vit.Orm.Sqlite.Translator;


namespace Vit.Orm.Sqlite
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
  id int NOT NULL PRIMARY KEY,
  name varchar(100) DEFAULT NULL,
  birth date DEFAULT NULL,
  fatherId int DEFAULT NULL,
  motherId int DEFAULT NULL
) ;
              */
            List<string> sqlFields = new();

            // #1 primary key
            sqlFields.Add(GetColumnSql(entityDescriptor.key) + " PRIMARY KEY");

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
                if (type.IsGenericType || type == typeof(string))
                {
                    nullable = true;
                    type = TypeUtil.GetUnderlyingType(type);
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


        public override (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) PrepareAdd<Entity>(DbSet<Entity> dbSet)
        {
            /* //sql
             insert into user(name,birth,fatherId,motherId) values('','','');
             select seq from sqlite_sequence where name='user';
              */
            var entityDescriptor = dbSet.entityDescriptor;

            var columns = entityDescriptor.allColumns;

            // #1 GetSqlParams 
            Func<Entity, Dictionary<string, object>> GetSqlParams = (entity) =>
            {
                var sqlParam = new Dictionary<string, object>();
                foreach (var column in columns)
                {
                    var columnName = column.name;
                    var value = column.Get(entity);

                    sqlParam[columnName] = value;
                }
                return sqlParam;
            };

            #region #2 columns 
            List<string> columnNames = new List<string>();
            List<string> valueParams = new List<string>();
            string columnName;

            foreach (var column in columns)
            {
                columnName = column.name;

                columnNames.Add(DelimitIdentifier(columnName));
                valueParams.Add(GenerateParameterName(columnName));
            }
            #endregion

            // #3 build sql
            string sql = $@"insert into {DelimitIdentifier(entityDescriptor.tableName)}({string.Join(",", columnNames)}) values({string.Join(",", valueParams)});";
            //sql+=$"select seq from sqlite_sequence where name = '{tableName}'; ";
            sql += "select null;";
            return (sql, GetSqlParams);
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
