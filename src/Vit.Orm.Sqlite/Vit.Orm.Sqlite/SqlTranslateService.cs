using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Vit.Extensions.Linq_Extensions;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Linq.ExpressionTree.ComponentModel;
using Vitorm.Entity;
using Vitorm.Sql;
using Vitorm.Sql.SqlTranslate;
using Vitorm.Sqlite.TranslateService;


namespace Vitorm.Sqlite
{
    public class SqlTranslateService : Vitorm.Sql.SqlTranslate.SqlTranslateService
    {
        public static readonly SqlTranslateService Instance = new SqlTranslateService();

        protected QueryTranslateService queryTranslateService;
        protected ExecuteUpdateTranslateService executeUpdateTranslateService;
        protected ExecuteDeleteTranslateService executeDeleteTranslateService;

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

                        string targetDbType = GetDbType(targetType);

                        var sourceType = convert.body.Member_GetType();
                        if (sourceType != null)
                        {
                            if (sourceType.IsGenericType) sourceType = sourceType.GetGenericArguments()[0];

                            if (targetDbType == GetDbType(sourceType)) return EvalExpression(arg, convert.body);
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
                            return $"{EvalExpression(arg, binary.left)} || {EvalExpression(arg, binary.right)}";
                        }

                        // ##2 Numberic Add
                        return $"{EvalExpression(arg, binary.left)} + {EvalExpression(arg, binary.right)}";
                    }
                case nameof(ExpressionType.Coalesce):
                    {
                        ExpressionNode_Binary binary = data;
                        return $"COALESCE({EvalExpression(arg, binary.left)},{EvalExpression(arg, binary.right)})";
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
                return "datetime";

            if (type == typeof(string))
                return "text";

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "real";

            if (type == typeof(bool) || type.Name.ToLower().Contains("int")) return "integer";

            throw new NotSupportedException("unsupported column type:" + type.Name);
        }
        #endregion


        public override (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareAdd(SqlTranslateArgument arg)
        {
            /* //sql
             insert into user(name,birth,fatherId,motherId) values('','','');
             select seq from sqlite_sequence where name='user';
              */
            var entityDescriptor = arg.entityDescriptor;

            var columns = entityDescriptor.allColumns;

            // #1 GetSqlParams 
            Func<object, Dictionary<string, object>> GetSqlParams = (entity) =>
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
