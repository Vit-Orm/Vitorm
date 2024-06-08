using System;
using System.Collections.Generic;

using Vit.Extensions.Linq_Extensions;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Orm.Entity;
using Vit.Orm.Sql;
using Vit.Orm.SqlServer.Translator;


namespace Vit.Orm.SqlServer
{
    public class SqlTranslator : Vit.Orm.Sql.Translator.SqlTranslator
    {
        public SqlTranslator(DbContext dbContext):base(dbContext) 
        {
        }

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to delimit.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string DelimitIdentifier(string identifier) => $"[{EscapeIdentifier(identifier)}]"; // Interpolation okay; strings

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string EscapeIdentifier(string identifier) => identifier.Replace("[", "\"[").Replace("]", "\"]");

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
            sqlFields.Add(GetColumnSql(entityDescriptor.key) + " PRIMARY KEY IDENTITY(1,1)");

            // #2 columns
            entityDescriptor.columns?.ForEach(column => sqlFields.Add(GetColumnSql(column)));

            return $@"
CREATE TABLE [dbo].{DelimitIdentifier(entityDescriptor.tableName)} (
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

        protected readonly static Dictionary<Type, string> dbTypeMap = new ()
        {
            [typeof(DateTime)] = "datetime",
            [typeof(string)] = "varchar(1000)",

            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(Int32)] = "int",
            [typeof(Int16)] = "smallint",
            [typeof(byte)] = "tinyint",
            [typeof(bool)] = "bit",
        };
        protected override string GetDbType(Type type)
        { 
            if (dbTypeMap.TryGetValue(type,out var dbType))return dbType;
                return "datetime";

            throw new NotSupportedException("unsupported column type:" + type.Name);
        }
        #endregion


        public override (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) PrepareAdd<Entity>(DbSet<Entity> dbSet) 
        {
            var result=base.PrepareAdd(dbSet);

            result.sql += "select convert(int,isnull(SCOPE_IDENTITY(),-1));";

            return result;
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
