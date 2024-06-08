using System;
using System.Collections.Generic;

using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Entity;
using System.Linq;

namespace Vit.Orm.Sql.Translator
{
    public abstract class SqlTranslator : ISqlTranslator
    {

        public DbContext dbContext { get; private set; }

        public SqlTranslator(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public virtual IEntityDescriptor GetEntityDescriptor(Type entityType) => dbContext.GetEntityDescriptor(entityType);





        #region DelimitIdentifier
        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to delimit.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string DelimitIdentifier(string identifier) => $"\"{EscapeIdentifier(identifier)}\""; // Interpolation okay; strings

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public virtual string EscapeIdentifier(string identifier) => identifier.Replace("\"", "\"\"");

        /// <summary>
        ///     Generates a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="name">The candidate name for the parameter.</param>
        /// <returns>
        ///     A valid name based on the candidate name.
        /// </returns>
        public virtual string GenerateParameterName(string name) => name.StartsWith("@", StringComparison.Ordinal) ? name : "@" + name;
        #endregion





        public virtual string GetTableName(Type entityType)
        {
            return dbContext.GetEntityDescriptor(entityType)?.tableName;
        }

        public virtual string GetSqlField(string tableName, string columnName)
        {
            return $"{DelimitIdentifier(tableName)}.{DelimitIdentifier(columnName)}";
        }


        public virtual string GetSqlField(ExpressionNode_Member member)
        {
            var memberName = member.memberName;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                memberName = dbContext.GetEntityDescriptor(member.Member_GetType())?.keyName;
            }

            // 1: {"nodeType":"Member","parameterName":"a0","memberName":"id"}
            // 2: {"nodeType":"Member","objectValue":{"parameterName":"a0","nodeType":"Member"},"memberName":"id"}
            return GetSqlField(member.objectValue?.parameterName ?? member.parameterName, memberName);
        }


        /// <summary>
        /// functionName example:  Count, Max, Min, Sum, Average
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public virtual string GetSqlField_Aggregate(string functionName, string tableName, string columnName)
        {
            switch (functionName)
            {
                case nameof(Enumerable.Count):
                    {
                        if (columnName == null) return $"{functionName}(*)";
                        return $"{functionName}({GetSqlField(tableName, columnName)})";
                    }
                case nameof(Enumerable.Max) or nameof(Enumerable.Min) or nameof(Enumerable.Sum):
                    {
                        return $"{functionName}({GetSqlField(tableName, columnName)})";
                    }
                case nameof(Enumerable.Average):
                    {
                        return $"AVG({GetSqlField(tableName, columnName)})";
                    }
            }
            throw new NotSupportedException("[SqlTranslator] unsupported aggregate function : " + functionName);
        }



        public abstract string PrepareCreate(IEntityDescriptor entityDescriptor);
        protected abstract string GetDbType(Type type);


        public virtual string PrepareGet<Entity>(DbSet<Entity> dbSet)
        {
            /* //sql
            delete from user where id = 7;
            */
            var entityDescriptor = dbSet.entityDescriptor;

            // #2 build sql
            string sql = $@"select * from {DelimitIdentifier(entityDescriptor.tableName)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

            return sql;
        }

        public virtual (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) PrepareQuery(CombinedStream combinedStream, Type entityType)
        {
            var query = new QueryTranslator(this, entityType: entityType);
            string sql = query.BuildQuery(combinedStream);
            return (sql, query.sqlParam, query.dataReader);
        }

        public abstract (string sql, Dictionary<string, object> sqlParam) PrepareExecuteUpdate(CombinedStream combinedStream);

        public abstract (string sql, Dictionary<string, object> sqlParam) PrepareExecuteDelete(CombinedStream combinedStream);

        public virtual (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) PrepareAdd<Entity>(DbSet<Entity> dbSet)
        {
            /* //sql
             insert into user(name,birth,fatherId,motherId) values('','','');
             select seq from sqlite_sequence where name='user';
              */
            var entityDescriptor = dbSet.entityDescriptor;

            var columns = entityDescriptor.columns;

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

            return (sql, GetSqlParams);
        }

        public virtual (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) PrepareUpdate<Entity>(DbSet<Entity> dbSet)
        {
            /* //sql
                update user set name='' where id=7;
            */

            var entityDescriptor = dbSet.entityDescriptor;
            var sqlParam = new Dictionary<string, object>();

            // #1 GetSqlParams
            Func<Entity, Dictionary<string, object>> GetSqlParams = (entity) =>
            {
                var sqlParam = new Dictionary<string, object>();
                foreach (var column in entityDescriptor.allColumns)
                {
                    var columnName = column.name;
                    var value = column.Get(entity);

                    sqlParam[columnName] = value;
                }
                //sqlParam[entityDescriptor.keyName] = entityDescriptor.key.Get(entity);
                return sqlParam;
            };

            // #2 columns
            List<string> columnsToUpdate = new List<string>();
            string columnName;
            foreach (var column in entityDescriptor.columns)
            {
                columnName = column.name;
                columnsToUpdate.Add($"{DelimitIdentifier(columnName)}={GenerateParameterName(columnName)}");
            }

            // #3 build sql
            string sql = $@"update {DelimitIdentifier(entityDescriptor.tableName)} set {string.Join(",", columnsToUpdate)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

            return (sql, GetSqlParams);
        }


        public virtual string PrepareDelete<Entity>(DbSet<Entity> dbSet)
        {
            /* //sql
            delete from user where id = 7;
            */
            var entityDescriptor = dbSet.entityDescriptor;

            // #2 build sql
            string sql = $@"delete from {DelimitIdentifier(entityDescriptor.tableName)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

            return sql;
        }

        public virtual string PrepareDeleteRange<Entity>(DbSet<Entity> dbSet)
        {
            /* //sql
            delete from user where id in ( 7 ) ;
            */
            var entityDescriptor = dbSet.entityDescriptor;

            // #2 build sql
            string sql = $@"delete from {DelimitIdentifier(entityDescriptor.tableName)} where {DelimitIdentifier(entityDescriptor.keyName)} in {GenerateParameterName("keys")};";

            return sql;
        }

    }
}
