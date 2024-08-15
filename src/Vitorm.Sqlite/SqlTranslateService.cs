using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;
using Vitorm.Sqlite.TranslateService;

namespace Vitorm.Sqlite
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
                                    return $"cast({EvalExpression(arg, methodCall.@object)} as text)";
                                }

                            #region ##2 String method:  StartsWith EndsWith Contains
                            case nameof(string.StartsWith): // String.StartsWith
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like {EvalExpression(arg, value)}||'%'";
                                }
                            case nameof(string.EndsWith): // String.EndsWith
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like '%'||{EvalExpression(arg, value)}";
                                }
                            case nameof(string.Contains) when methodCall.methodCall_typeName == "String": // String.Contains
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like '%'||{EvalExpression(arg, value)}||'%'";
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

                        if (targetDbType == "datetime")
                        {
                            return $"DATETIME({EvalExpression(arg, convert.body)})";
                        }
                        return $"cast({EvalExpression(arg, convert.body)} as {targetDbType})";
                    }
                case nameof(ExpressionType.Add):
                    {
                        ExpressionNode_Binary binary = data;

                        // ##1 String Add
                        if (data.valueType?.ToType() == typeof(string))
                        {
                            // select ifNull( cast( (userFatherId) as text ) , '' )  from `User`

                            return $"{BuildSqlSentence(binary.left)} || {BuildSqlSentence(binary.right)}";

                            string BuildSqlSentence(ExpressionNode node)
                            {
                                if (node.nodeType == NodeType.Constant)
                                {
                                    ExpressionNode_Constant constant = node;
                                    if (constant.value == null) return "''";
                                    else return $"cast( ({EvalExpression(arg, node)}) as text )";
                                }
                                else
                                    return $"ifNull( cast( ({EvalExpression(arg, node)}) as text ) , '')";
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
                        // IIF(`t0`.`fatherId` is not null, true, false)
                        ExpressionNode_Conditional conditional = data;
                        return $"IIF({EvalExpression(arg, conditional.Conditional_GetTest())},{EvalExpression(arg, conditional.Conditional_GetIfTrue())},{EvalExpression(arg, conditional.Conditional_GetIfFalse())})";
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
CREATE TABLE IF NOT EXISTS "User" (
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
                return "datetime";

            if (type == typeof(string))
                return "text";

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "real";

            if (type == typeof(bool) || type.Name.ToLower().Contains("int")) return "integer";

            throw new NotSupportedException("unsupported column type:" + type.Name);
        }
        #endregion

        public override string PrepareTryDropTable(IEntityDescriptor entityDescriptor)
        {
            // drop table if exists "User";
            return $@"drop table if exists {DelimitTableName(entityDescriptor)};";
        }
        public override string PrepareTruncate(IEntityDescriptor entityDescriptor)
        {
            // delete from 'User';
            return $@"delete from {DelimitTableName(entityDescriptor)};";
        }

    }
}
