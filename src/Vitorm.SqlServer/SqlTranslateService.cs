using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.SqlServer
{
    public class SqlTranslateService : Vitorm.Sql.SqlTranslate.SqlTranslateService
    {
        public static readonly SqlTranslateService Instance = new SqlTranslateService();

        protected override BaseQueryTranslateService queryTranslateService { get; }
        protected override BaseQueryTranslateService executeUpdateTranslateService { get; }
        protected override BaseQueryTranslateService executeDeleteTranslateService { get; }


        public SqlTranslateService()
        {
            queryTranslateService = new Vitorm.SqlServer.SqlTranslate.QueryTranslateService(this);
            executeUpdateTranslateService = new Vitorm.SqlServer.SqlTranslate.ExecuteUpdateTranslateService(this);
            executeDeleteTranslateService = new Vitorm.SqlServer.SqlTranslate.ExecuteDeleteTranslateService(this);
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
        public override string EscapeIdentifier(string identifier) => identifier?.Replace("[", "\"[").Replace("]", "\"]");


        #region EvalExpression

        /// <summary>
        /// evaluate column in select / order by / group by,  for example :  "select (u.id + 100) as newId",  "order by a.name",  "group by a.name"
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="node"></param>
        /// <param name="columnType"></param>
        /// <returns></returns>
        public override string EvalSelectExpression(QueryTranslateArgument arg, ExpressionNode node, Type columnType = null)
        {
            using var expressionTreeStack = arg.ExpressionTree_Push(selectExpressionTreeStackHolder);

            var selectFieldSentence = base.EvalSelectExpression(arg, node, columnType);
            if (columnType == typeof(bool) || columnType == typeof(bool?))
            {
                if (!selectFieldSentence.StartsWith("IIF(", StringComparison.OrdinalIgnoreCase))
                {
                    // select IIF(userFatherId is not null, 1, 0) as hasFather, * from [User]
                    return $"IIF({selectFieldSentence}, 1, 0)";
                }
            }
            return selectFieldSentence;
        }
        static ExpressionNode selectExpressionTreeStackHolder = ExpressionNode.Binary(nodeType: NodeType.Equal, null, null);


        /// <summary>
        /// read where or value or on
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="node"></param>
        public override string EvalExpression(QueryTranslateArgument arg, ExpressionNode node)
        {
            var parentNode = arg.ExpressionTree_Current();
            using var expressionTreeStack = arg.ExpressionTree_Push(node);

            switch (node.nodeType)
            {
                case NodeType.Member:
                    {
                        ExpressionNode_Member member = node;

                        var sqlField = GetSqlField(member, arg.dbContext);

                        var memberType = member.Member_GetType();
                        if (memberType == typeof(bool) || memberType == typeof(bool?))
                        {
                            if (parentNode == null || new[] { NodeType.Not, NodeType.AndAlso, NodeType.OrElse, }.Contains(parentNode.nodeType))
                            {
                                return $"({sqlField} = 1)";
                            }
                        }
                        return sqlField;
                    }
                case NodeType.Constant:
                    {
                        ExpressionNode_Constant constant = node;
                        var value = constant.value;
                        if (value is bool boolean)
                        {
                            return boolean ? "1" : "0";
                        }
                        break;
                    }
                case NodeType.MethodCall:
                    {
                        ExpressionNode_MethodCall methodCall = node;
                        switch (methodCall.methodName)
                        {
                            // ##1 ToString
                            case nameof(object.ToString):
                                {
                                    return $"cast({EvalExpression(arg, methodCall.@object)} as varchar(max))";
                                }

                            #region ##2 String method:  StartsWith EndsWith Contains
                            case nameof(string.StartsWith): // String.StartsWith
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like {EvalExpression(arg, value)}+'%'";
                                }
                            case nameof(string.EndsWith): // String.EndsWith
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like '%'+{EvalExpression(arg, value)}";
                                }
                            case nameof(string.Contains) when methodCall.methodCall_typeName == "String": // String.Contains
                                {
                                    var str = methodCall.@object;
                                    var value = methodCall.arguments[0];
                                    return $"{EvalExpression(arg, str)} like '%'+{EvalExpression(arg, value)}+'%'";
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

                        return $"cast({EvalExpression(arg, convert.body)} as {targetDbType})";
                    }
                case nameof(ExpressionType.Add):
                    {
                        ExpressionNode_Binary binary = node;

                        // ##1 String Add
                        if (node.valueType?.ToType() == typeof(string))
                        {
                            return $"CONCAT({EvalExpression(arg, binary.left)} ,{EvalExpression(arg, binary.right)})";
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
                        // IIF(`t0`.`fatherId` is not null, 1, 0)
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
if object_id(N'[dbo].[User]', N'U') is null
    create table [dbo].[User] (
      id int not null primary key identity(1,1),
      name varchar(100) default null,
      birth date default null,
      fatherId int default null,
      motherId int default null
    ) ;
              */
            List<string> sqlFields = new();

            // #1 primary key
            if (entityDescriptor.key != null)
                sqlFields.Add(GetColumnSql(entityDescriptor.key));

            // #2 columns
            entityDescriptor.columns?.ForEach(column => sqlFields.Add(GetColumnSql(column)));

            return $@"
if object_id(N'{DelimitTableName(entityDescriptor)}', N'U') is null
create table {DelimitTableName(entityDescriptor)} (
  {string.Join(",\r\n  ", sqlFields)}
)";


            string GetColumnSql(IColumnDescriptor column)
            {
                var columnDbType = column.columnDbType ?? GetColumnDbType(column);
                var defaultValue = column.isNullable ? "default null" : "";
                if (column.isIdentity)
                {
                    var type = TypeUtil.GetUnderlyingType(column.type);
                    if (type == typeof(Guid)) defaultValue = "default NewId()";
                    else defaultValue = "identity(1,1)";
                }

                /*
                  name  type    nullable        defaultValue                    primaryKey 
                  id    int     not null/null   default null                    primary key
                                                identity(1,1)
                                                default NewId()
                                                default NewSequentialId()
                                                default 12
                 */

                return $"  {DelimitIdentifier(column.columnName)}  {columnDbType}  {(column.isNullable ? "null" : "not null")}  {defaultValue}  {(column.isKey ? "primary key" : "")}";
            }
        }

        public readonly static Dictionary<Type, string> columnDbTypeMap = new()
        {
            [typeof(DateTime)] = "datetime",
            [typeof(string)] = "varchar(max)",

            [typeof(float)] = "float",
            [typeof(double)] = "float",
            [typeof(decimal)] = "float",

            [typeof(Int32)] = "int",
            [typeof(Int16)] = "smallint",
            [typeof(byte)] = "tinyint",
            [typeof(bool)] = "bit",

            [typeof(Guid)] = "uniqueIdentifier",

        };
        protected override string GetColumnDbType(IColumnDescriptor column)
        {
            Type type = column.type;

            if (column.columnLength.HasValue && type == typeof(string))
            {
                // varchar(1000)
                return $"varchar({(column.columnLength.Value)})";
            }
            return GetColumnDbType(type);
        }
        protected override string GetColumnDbType(Type type)
        {
            var underlyingType = TypeUtil.GetUnderlyingType(type);

            if (columnDbTypeMap.TryGetValue(underlyingType, out var dbType)) return dbType;
            throw new NotSupportedException("unsupported column type:" + underlyingType.Name);
        }
        #endregion

        public override string PrepareTryDropTable(IEntityDescriptor entityDescriptor)
        {
            // IF OBJECT_ID(N'User', N'U') IS NOT NULL  DROP TABLE [User];
            var tableName = DelimitTableName(entityDescriptor);
            return $@"IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL  DROP TABLE {tableName};";
        }

        public override EAddType Entity_GetAddType(SqlTranslateArgument arg, object entity)
        {
            var key = arg.entityDescriptor.key;
            if (key == null) return EAddType.noKeyColumn;

            var keyValue = key.GetValue(entity);
            var keyIsEmpty = keyValue is null || keyValue.Equals(TypeUtil.DefaultValue(arg.entityDescriptor.key.type));

            if (key.isIdentity)
            {
                return keyIsEmpty ? EAddType.identityKey : throw new ArgumentException("Cannot insert explicit value for identity column.");
            }
            else
            {
                return !keyIsEmpty ? EAddType.keyWithValue : throw new ArgumentException("Key could not be empty.");
            }
        }
        public override (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareAdd(SqlTranslateArgument arg, EAddType addType)
        {
            if (addType == EAddType.identityKey)
            {
                // insert into UserInfo(name) output inserted.guid values('dd');

                var entityDescriptor = arg.entityDescriptor;
                var (columnNames, sqlColumnParams, GetSqlParams) = PrepareAdd_Columns(arg, entityDescriptor.columns);
                var sqlOutput = "output inserted." + DelimitIdentifier(entityDescriptor.key.columnName);

                string sql = $@"insert into {DelimitTableName(entityDescriptor)}({string.Join(",", columnNames)}) {sqlOutput} values({string.Join(",", sqlColumnParams)});";
                return (sql, GetSqlParams);
            }
            else
            {
                // insert into user(name,fatherId,motherId) values('',0,0);

                var entityDescriptor = arg.entityDescriptor;
                var (columnNames, sqlColumnParams, GetSqlParams) = PrepareAdd_Columns(arg, entityDescriptor.allColumns);
                string sql = $@"insert into {DelimitTableName(entityDescriptor)}({string.Join(",", columnNames)}) values({string.Join(",", sqlColumnParams)});";
                return (sql, GetSqlParams);
            }
        }


    }
}
