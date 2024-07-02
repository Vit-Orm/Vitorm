using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Vit.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Entity;
using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;
using Vitorm.SqlServer.TranslateService;
using Vitorm.StreamQuery;

namespace Vitorm.SqlServer
{
    public class SqlTranslateService : Vitorm.Sql.SqlTranslate.SqlTranslateService
    {
        public static readonly SqlTranslateService Instance = new SqlTranslateService();

        protected Vitorm.SqlServer.SqlTranslate.QueryTranslateService queryTranslateService;
        protected ExecuteUpdateTranslateService executeUpdateTranslateService;
        protected ExecuteDeleteTranslateService executeDeleteTranslateService;


        public SqlTranslateService()
        {
            queryTranslateService = new(this);
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
        public override string DelimitIdentifier(string identifier) => $"[{EscapeIdentifier(identifier)}]"; // Interpolation okay; strings

        /// <summary>
        ///     Generates the escaped SQL representation of an identifier (column name, table name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to be escaped.</param>
        /// <returns>
        ///     The generated string.
        /// </returns>
        public override string EscapeIdentifier(string identifier) => identifier.Replace("[", "\"[").Replace("]", "\"]");

        public override string DelimitTableName(IEntityDescriptor entityDescriptor)
        {
            if (entityDescriptor.schema == null) return DelimitIdentifier(entityDescriptor.tableName);

            return $"{DelimitIdentifier(entityDescriptor.schema)}.{DelimitIdentifier(entityDescriptor.tableName)}";
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

                        return $"cast({EvalExpression(arg, convert.body)} as {targetDbType})";
                    }
                case nameof(ExpressionType.Add):
                    {
                        ExpressionNode_Binary binary = data;

                        // ##1 String Add
                        if (data.valueType?.ToType() == typeof(string))
                        {
                            return $"CONCAT({EvalExpression(arg, binary.left)} ,{EvalExpression(arg, binary.right)})";
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
                        // IIF(`t0`.`fatherId` is not null,true, false)
                        ExpressionNode_Conditional conditional = data;
                        return $"IIF({EvalExpression(arg, conditional.Conditional_GetTest())},{EvalExpression(arg, conditional.Conditional_GetIfTrue())},{EvalExpression(arg, conditional.Conditional_GetIfFalse())})";
                    }
                    #endregion

            }

            return base.EvalExpression(arg, data);
        }

        #endregion





        #region PrepareCreate
        public override string PrepareCreate(IEntityDescriptor entityDescriptor)
        {
            /* //sql
if object_id(N'[dbo].[User]', N'U') is null
    CREATE TABLE [dbo].[User] (
      id int NOT NULL PRIMARY KEY AUTO_INCREMENT,
      name varchar(100) DEFAULT NULL,
      birth date DEFAULT NULL,
      fatherId int DEFAULT NULL,
      motherId int DEFAULT NULL
    ) ;
              */
            List<string> sqlFields = new();

            // #1 primary key
            if (entityDescriptor.key != null)
                sqlFields.Add(GetColumnSql(entityDescriptor.key) + " " + (entityDescriptor.key.isIdentity ? "PRIMARY KEY IDENTITY(1,1) " : ""));

            // #2 columns
            entityDescriptor.columns?.ForEach(column => sqlFields.Add(GetColumnSql(column)));

            return $@"
if object_id(N'{DelimitTableName(entityDescriptor)}', N'U') is null
CREATE TABLE {DelimitTableName(entityDescriptor)} (
{string.Join(",\r\n  ", sqlFields)}
)";


            string GetColumnSql(IColumnDescriptor column)
            {
                var columnDbType = column.databaseType ?? GetColumnDbType(column.type);
                // name varchar(100) DEFAULT NULL
                return $"  {DelimitIdentifier(column.columnName)} {columnDbType} {(column.isNullable ? "DEFAULT NULL" : "NOT NULL")}";
            }
        }

        protected readonly static Dictionary<Type, string> columnDbTypeMap = new()
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
        };
        protected override string GetColumnDbType(Type type)
        {
            type = TypeUtil.GetUnderlyingType(type);

            if (columnDbTypeMap.TryGetValue(type, out var dbType)) return dbType;
            throw new NotSupportedException("unsupported column type:" + type.Name);
        }
        #endregion

        public override string PrepareDrop(IEntityDescriptor entityDescriptor)
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
        public override (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareIdentityAdd(SqlTranslateArgument arg)
        {
            var result = PrepareAdd(arg, arg.entityDescriptor.columns);

            // get generated id
            result.sql += "select convert(int,isnull(SCOPE_IDENTITY(),-1));";

            return result;
        }

        public override (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) PrepareQuery(QueryTranslateArgument arg, CombinedStream combinedStream)
        {
            string sql = queryTranslateService.BuildQuery(arg, combinedStream);
            return (sql, arg.sqlParam, arg.dataReader);
        }
        public override (string sql, Dictionary<string, object> sqlParam) PrepareExecuteUpdate(QueryTranslateArgument arg, CombinedStream combinedStream)
        {
            string sql = executeUpdateTranslateService.BuildQuery(arg, combinedStream);
            return (sql, arg.sqlParam);
        }

        public override (string sql, Dictionary<string, object> sqlParam) PrepareExecuteDelete(QueryTranslateArgument arg, CombinedStream combinedStream)
        {
            string sql = executeDeleteTranslateService.BuildQuery(arg, combinedStream);
            return (sql, arg.sqlParam);
        }



    }
}
