using System;
using System.Collections.Generic;

using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.Entity;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.SqlTranslate
{
    public interface ISqlTranslateService
    {

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
        string GetSqlField(string tableName, string columnName);
        string GetSqlField(ExpressionNode_Member member, DbContext dbContext);

        /// <summary>
        /// evaluate column in select,  for example :  "select (u.id + 100) as newId"
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="data"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        string EvalSelectExpression(QueryTranslateArgument arg, ExpressionNode data, Type columnType = null);
        string EvalExpression(QueryTranslateArgument arg, ExpressionNode data);

        // #0 Schema :  PrepareCreate PrepareDrop
        string PrepareTryCreateTable(IEntityDescriptor entityDescriptor);
        string PrepareTryDropTable(IEntityDescriptor entityDescriptor);
        string PrepareTruncate(IEntityDescriptor entityDescriptor);


        // #1 Create :  PrepareAdd
        EAddType Entity_GetAddType(SqlTranslateArgument arg, object entity);
        (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareAdd(SqlTranslateArgument arg, EAddType addType);


        // #2 Retrieve : PrepareGet PrepareQuery
        string PrepareGet(SqlTranslateArgument arg);
        string PrepareQuery(QueryTranslateArgument arg, CombinedStream combinedStream);
        string PrepareCountQuery(QueryTranslateArgument arg, CombinedStream combinedStream);



        // #3 Update: PrepareUpdate PrepareExecuteUpdate
        (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareUpdate(SqlTranslateArgument arg);
        string PrepareExecuteUpdate(QueryTranslateArgument arg, CombinedStream combinedStream);


        // #4 Delete: PrepareDelete PrepareDeleteRange PrepareExecuteDelete
        string PrepareDelete(SqlTranslateArgument arg);

        string PrepareDeleteByKeys<Key>(SqlTranslateArgument arg, IEnumerable<Key> keys);

        string PrepareExecuteDelete(QueryTranslateArgument arg, CombinedStream combinedStream);





    }
}
