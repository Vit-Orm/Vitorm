using System;
using System.Collections.Generic;

using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Entity;

namespace Vit.Orm.Sql
{
    public interface ISqlTranslator
    {
        IEntityDescriptor GetEntityDescriptor(Type entityType);

        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to delimit.</param>
        /// <returns>
        ///     The generated string.
        string DelimitIdentifier(string identifier);

        /// <summary>
        ///     Generates a valid parameter name for the given candidate name.
        /// </summary>
        /// <param name="name">The candidate name for the parameter.</param>
        /// <returns>
        ///     A valid name based on the candidate name.
        /// </returns>
        string GenerateParameterName(string name);
        string GetTableName(Type entityType);
        string GetSqlField(string tableName, string columnName);
        string GetSqlField(ExpressionNode_Member member);


        /// <summary>
        /// functionName example:  Count, Max, Min, Sum, Average
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        string GetSqlField_Aggregate(string functionName, string tableName, string columnName);



        string PrepareCreate(IEntityDescriptor entityDescriptor);

        string PrepareGet<Entity>(DbSet<Entity> dbSet);

        (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) PrepareQuery(CombinedStream combinedStream, Type entityType);


        (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) PrepareAdd<Entity>(DbSet<Entity> dbSet);


        (string sql, Func<Entity, Dictionary<string, object>> GetSqlParams) PrepareUpdate<Entity>(DbSet<Entity> dbSet);
        (string sql, Dictionary<string, object> sqlParam) PrepareExecuteUpdate(CombinedStream combinedStream);

        string PrepareDelete<Entity>(DbSet<Entity> dbSet);

        string PrepareDeleteRange<Entity>(DbSet<Entity> dbSet);

        (string sql, Dictionary<string, object> sqlParam) PrepareExecuteDelete(CombinedStream combinedStream);





    }
}
