using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionNodes.ComponentModel;

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

            delimitTableName = Vitorm.Sql.SqlTranslate.SqlTranslateService.DelimitTableNameWithoutSchema;
        }

        #region EvalExpression
        /// <summary>
        /// read where or value or on
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="node"></param>
        public override string EvalExpression(QueryTranslateArgument arg, ExpressionNode node)
        {
            switch (node.nodeType)
            {
                case NodeType.MethodCall:
                    {
                        ExpressionNode_MethodCall methodCall = node;
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

                        ExpressionNode_Convert convert = node;

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
                        ExpressionNode_Binary binary = node;

                        // ##1 String Add
                        if (node.valueType?.ToType() == typeof(string))
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

                        // ##2 Numeric Add
                        return $"{EvalExpression(arg, binary.left)} + {EvalExpression(arg, binary.right)}";
                    }
                case nameof(ExpressionType.Coalesce):
                    {
                        ExpressionNode_Binary binary = node;
                        return $"COALESCE({EvalExpression(arg, binary.left)},{EvalExpression(arg, binary.right)})";
                    }
                case nameof(ExpressionType.Conditional):
                    {
                        // IIF(`t0`.`fatherId` is not null, true, false)
                        ExpressionNode_Conditional conditional = node;
                        return $"IIF({EvalExpression(arg, conditional.Conditional_GetTest())},{EvalExpression(arg, conditional.Conditional_GetIfTrue())},{EvalExpression(arg, conditional.Conditional_GetIfFalse())})";
                    }
                    #endregion

            }

            return base.EvalExpression(arg, node);
        }
        #endregion



        #region PrepareCreate
        public override string PrepareTryCreateTable(IEntityDescriptor entityDescriptor)
        {
            /* //sql
CREATE TABLE IF NOT EXISTS "User" (
  id int PRIMARY KEY NOT NULL,
  name varchar(100) DEFAULT NULL,
  birth date DEFAULT NULL,
  fatherId int DEFAULT NULL,
  motherId int DEFAULT NULL
) ;
              */
            List<string> sqlFields = new();

            // #1 primary key
            sqlFields.Add(GetColumnSql(entityDescriptor.key));

            // #2 columns
            entityDescriptor.columns?.ForEach(column => sqlFields.Add(GetColumnSql(column)));

            return $@"
CREATE TABLE IF NOT EXISTS {DelimitTableName(entityDescriptor)} (
  {string.Join(",\r\n  ", sqlFields)}
)";

            string GetColumnSql(IColumnDescriptor column)
            {
                var columnDbType = column.columnDbType ?? GetColumnDbType(column);
                var defaultValue = column.isNullable ? "default null" : "";
                if (column.isIdentity)
                {
                    throw new NotSupportedException("identity for Sqlite is not supported yet.");
                }

                /*
                  name  type    nullable        defaultValue    primaryKey
                  id    int     not null/null   default null    primary key

                 */

                return $"  {DelimitIdentifier(column.columnName)}  {columnDbType}  {(column.isNullable ? "null" : "not null")}  {defaultValue}  {(column.isKey ? "primary key" : "")}";
            }
        }

        public readonly static Dictionary<Type, string> columnDbTypeMap = new()
        {
            [typeof(DateTime)] = "datetime",
            [typeof(string)] = "text",

            [typeof(float)] = "real",
            [typeof(double)] = "real",
            [typeof(decimal)] = "real",

            [typeof(Int32)] = "integer",
            [typeof(Int16)] = "integer",
            [typeof(byte)] = "integer",
            [typeof(bool)] = "integer",

            [typeof(Guid)] = "text",

        };


        protected override string GetColumnDbType(IColumnDescriptor column)
        {
            Type type = column.type;

            if (column.columnLength.HasValue && type == typeof(string))
            {
                // Name TEXT CHECK(length(Name) <= 50),
                return $"TEXT CHECK(length({DelimitIdentifier(column.columnName)}) <= {column.columnLength})";
            }
            return GetColumnDbType(type);
        }

        protected override string GetColumnDbType(Type type)
        {
            var underlyingType = TypeUtil.GetUnderlyingType(type);

            if (columnDbTypeMap.TryGetValue(underlyingType, out var dbType)) return dbType;
            if (underlyingType.Name.ToLower().Contains("int")) return "integer";

            throw new NotSupportedException("unsupported column type:" + underlyingType.Name);
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
