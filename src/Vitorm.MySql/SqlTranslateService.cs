using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Entity;
using Vitorm.MySql.TranslateService;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.MySql
{
    public class SqlTranslateService : Vitorm.Sql.SqlTranslate.SqlTranslateService
    {
        public static readonly SqlTranslateService Instance = new SqlTranslateService();

        protected override BaseQueryTranslateService queryTranslateService { get; }
        protected override BaseQueryTranslateService executeUpdateTranslateService { get; }
        protected override BaseQueryTranslateService executeDeleteTranslateService { get; }

        public SqlTranslateService()
        {
            queryTranslateService = new QueryTranslateService(this);
            executeUpdateTranslateService = new ExecuteUpdateTranslateService(this);
            executeDeleteTranslateService = new ExecuteDeleteTranslateService(this);
        }
        /// <summary>
        ///     Generates the delimited SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to delimit.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string DelimitIdentifier(string identifier) => $"`{EscapeIdentifier(identifier)}`"; // Interpolation okay; strings

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string EscapeIdentifier(string identifier) => identifier?.Replace("`", "\\`");


        #region EvalExpression
        /// <summary>
        /// read where or value or on
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="data"></param>
        public override string EvalExpression(QueryTranslateArgument arg, ExpressionNode data)
        {
            switch (data.nodeType)
            {
                case NodeType.MethodCall:
                    {
                        ExpressionNode_MethodCall methodCall = data;
                        switch (methodCall.methodName)
                        {
                            // ##1 ToString
                            case nameof(object.ToString):
                                {
                                    return $"cast({EvalExpression(arg, methodCall.@object)} as char)";
                                }

                            #region ##2 String method:  StartsWith EndsWith Contains
                            case nameof(string.StartsWith): // String.StartsWith
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like concat({EvalExpression(arg, value)},'%')";
                                }
                            case nameof(string.EndsWith): // String.EndsWith
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like concat('%',{EvalExpression(arg, value)})";
                                }
                            case nameof(string.Contains) when methodCall.methodCall_typeName == "String": // String.Contains
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like concat('%',{EvalExpression(arg, value)},'%')";
                                }
                                #endregion
                        }
                        break;
                    }

                #region Read Value
                case NodeType.Convert:
                    {
                        // cast( 4.1 as signed)

                        ExpressionNode_Convert convert = data;

                        Type targetType = convert.valueType?.ToType();

                        if (targetType == typeof(object)) return EvalExpression(arg, convert.body);

                        // Nullable
                        if (targetType.IsGenericType) targetType = targetType.GetGenericArguments()[0];

                        string targetDbType = GetColumnDbType(targetType);

                        var sourceType = convert.body.Member_GetType();
                        if (sourceType != null)
                        {
                            if (sourceType.IsGenericType) sourceType = sourceType.GetGenericArguments()[0];

                            if (targetDbType == GetColumnDbType(sourceType)) return EvalExpression(arg, convert.body);
                        }

                        if (targetType == typeof(string))
                        {
                            return $"cast({EvalExpression(arg, convert.body)} as char)";
                        }

                        return $"cast({EvalExpression(arg, convert.body)} as {targetDbType})";
                    }
                case nameof(ExpressionType.Add):
                    {
                        ExpressionNode_Binary binary = data;

                        // ##1 String Add
                        if (data.valueType?.ToType() == typeof(string))
                        {
                            //  select ifNull( cast( (userFatherId) as char ) , '' )  from `User`

                            return $"CONCAT( {BuildSqlSentence(binary.left)} , {BuildSqlSentence(binary.right)} )";

                            string BuildSqlSentence(ExpressionNode node)
                            {
                                if (node.nodeType == NodeType.Constant)
                                {
                                    ExpressionNode_Constant constant = node;
                                    if (constant.value == null) return "''";
                                    else return $"cast( ({EvalExpression(arg, node)}) as char )";
                                }
                                else
                                    return $"ifNull( cast( ({EvalExpression(arg, node)}) as char ) , '')";
                            }
                        }

                        // ##2 Numberic Add
                        return $"{EvalExpression(arg, binary.left)} + {EvalExpression(arg, binary.right)}";
                    }
                case nameof(ExpressionType.Coalesce):
                    {
                        ExpressionNode_Binary binary = data;
                        return $"COALESCE({EvalExpression(arg, binary.left)},{EvalExpression(arg, binary.right)})";
                    }
                case nameof(ExpressionType.Conditional):
                    {
                        // IF(500<1000,true,false)
                        ExpressionNode_Conditional conditional = data;
                        return $"IF({EvalExpression(arg, conditional.Conditional_GetTest())},{EvalExpression(arg, conditional.Conditional_GetIfTrue())},{EvalExpression(arg, conditional.Conditional_GetIfFalse())})";
                    }
                    #endregion

            }

            return base.EvalExpression(arg, data);
        }
        #endregion



        #region PrepareCreate
        public override string PrepareTryCreateTable(IEntityDescriptor entityDescriptor)
        {
            /* //sql
CREATE TABLE IF NOT EXISTS `User` (
  id int NOT NULL PRIMARY KEY,
  name varchar(100) DEFAULT NULL,
  birth date DEFAULT NULL,
  fatherId int DEFAULT NULL,
  motherId int DEFAULT NULL
) ;
              */
            List<string> sqlFields = new();

            // #1 primary key
            if (entityDescriptor.key != null)
                sqlFields.Add(GetColumnSql(entityDescriptor.key) + " PRIMARY KEY " + (entityDescriptor.key.isIdentity ? "AUTO_INCREMENT " : ""));

            // #2 columns
            entityDescriptor.columns?.ForEach(column => sqlFields.Add(GetColumnSql(column)));

            return $@"
CREATE TABLE IF NOT EXISTS {DelimitTableName(entityDescriptor)} (
{string.Join(",\r\n  ", sqlFields)}
)";


            string GetColumnSql(IColumnDescriptor column)
            {
                var columnDbType = column.databaseType ?? GetColumnDbType(column.type);
                // name varchar(100) DEFAULT NULL
                return $"  {DelimitIdentifier(column.columnName)} {columnDbType} {(column.isNullable ? "DEFAULT NULL" : "NOT NULL")}";
            }
        }
        protected override string GetColumnDbType(Type type)
        {
            type = TypeUtil.GetUnderlyingType(type);

            if (type == typeof(DateTime))
                return "DATETIME";

            if (type == typeof(string))
                return "varchar(1000)";

            if (type == typeof(float)) return "FLOAT";
            if (type == typeof(double) || type == typeof(decimal))
                return "DOUBLE";

            if (type == typeof(Int32)) return "INTEGER";
            if (type == typeof(Int16)) return "SMALLINT";
            if (type == typeof(byte)) return "TINYINT";
            if (type == typeof(bool)) return "TINYINT";

            if (type.Name.ToLower().Contains("int")) return "INTEGER";

            throw new NotSupportedException("unsupported column type:" + type.Name);
        }
        #endregion

        public override string PrepareTryDropTable(IEntityDescriptor entityDescriptor)
        {
            // drop table if exists `User`;
            return $@"drop table if exists {DelimitTableName(entityDescriptor)};";
        }


        public override (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareIdentityAdd(SqlTranslateArgument arg)
        {
            var result = PrepareAdd(arg, arg.entityDescriptor.columns);

            // get generated id
            result.sql += "select last_insert_id();";

            return result;
        }

    }
}
