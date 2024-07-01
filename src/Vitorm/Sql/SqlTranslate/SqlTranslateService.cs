using System;
using System.Collections.Generic;

using Vit.Linq.ExpressionTree.ComponentModel;
using Vitorm.Entity;
using System.Linq;
using Vitorm.StreamQuery;
using System.Collections;
using System.Text;
using System.Linq.Expressions;
using static Vitorm.Sql.SqlDbContext;
using System.Data;
using Vit.Linq.ExpressionTree.ExpressionConvertor;
using Vit.Extensions.Vitorm_Extensions;

namespace Vitorm.Sql.SqlTranslate
{
    public abstract class SqlTranslateService : ISqlTranslateService
    {

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


        public virtual string DelimitTableName(IEntityDescriptor entityDescriptor) => DelimitIdentifier(entityDescriptor.tableName);
        #endregion



        public virtual string GetSqlField(string tableName, string columnName)
        {
            return $"{DelimitIdentifier(tableName)}.{DelimitIdentifier(columnName)}";
        }

        /// <summary>
        /// user.id
        /// </summary>
        /// <param name="member"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public virtual string GetSqlField(ExpressionNode_Member member, DbContext dbContext)
        {
            var memberName = member.memberName;
            if (string.IsNullOrWhiteSpace(memberName))
            {
                var entityType = member.Member_GetType();
                var entityDescriptor = dbContext.GetEntityDescriptor(entityType);
                memberName = entityDescriptor?.keyName;
            }
            else if (member.objectValue != null)
            {
                var entityType = member.objectValue.Member_GetType();
                if (entityType != null)
                {
                    var entityDescriptor = dbContext.GetEntityDescriptor(entityType);
                    if (entityDescriptor != null)
                    {
                        var columnName = entityDescriptor.GetColumnNameByPropertyName(memberName);
                        if (string.IsNullOrEmpty(columnName)) throw new NotSupportedException("[QueryTranslator] can not find database column name for property : " + memberName);
                        memberName = columnName;
                    }
                }
            }

            // 1: {"nodeType":"Member","parameterName":"a0","memberName":"id"}
            // 2: {"nodeType":"Member","objectValue":{"parameterName":"a0","nodeType":"Member"},"memberName":"id"}
            return GetSqlField(member.objectValue?.parameterName ?? member.parameterName, memberName);
        }

        protected abstract string GetColumnDbType(Type type);


        #region EvalExpression

        /// <summary>
        /// read where or value or on
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        /// <param name="data"></param>
        public virtual string EvalExpression(QueryTranslateArgument arg, ExpressionNode data)
        {
            switch (data.nodeType)
            {
                case NodeType.AndAlso:
                    {
                        ExpressionNode_AndAlso and = data;
                        return $"({EvalExpression(arg, and.left)} and {EvalExpression(arg, and.right)})";
                    }
                case NodeType.OrElse:
                    {
                        ExpressionNode_OrElse or = data;
                        return $"({EvalExpression(arg, or.left)} or {EvalExpression(arg, or.right)})";
                    }
                case NodeType.Not:
                    {
                        ExpressionNode_Not not = data;
                        return $"(not {EvalExpression(arg, not.body)})";
                    }
                case NodeType.ArrayIndex:
                    {
                        throw new NotSupportedException(data.nodeType);
                        //ExpressionNode_ArrayIndex arrayIndex = data;
                        //return Expression.ArrayIndex(ToExpression(arg, arrayIndex.left), ToExpression(arg, arrayIndex.right));
                    }
                case NodeType.Equal:
                case NodeType.NotEqual:
                    {
                        ExpressionNode_Binary binary = data;

                        //   "= null"  ->   "is null" ,    "!=null" -> "is not null"   
                        if (binary.right.nodeType == NodeType.Constant && binary.right.value == null)
                        {
                            var opera = data.nodeType == NodeType.Equal ? "is null" : "is not null";
                            return $"{EvalExpression(arg, binary.left)} " + opera;
                        }
                        else if (binary.left.nodeType == NodeType.Constant && binary.left.value == null)
                        {
                            var opera = data.nodeType == NodeType.Equal ? "is null" : "is not null";
                            return $"{EvalExpression(arg, binary.right)} " + opera;
                        }

                        var @operator = operatorMap[data.nodeType];
                        return $"({EvalExpression(arg, binary.left)} {@operator} {EvalExpression(arg, binary.right)})";
                    }
                case NodeType.LessThan:
                case NodeType.LessThanOrEqual:
                case NodeType.GreaterThan:
                case NodeType.GreaterThanOrEqual:
                case nameof(ExpressionType.Divide):
                case nameof(ExpressionType.Modulo):
                case nameof(ExpressionType.Multiply):
                case nameof(ExpressionType.Power):
                case nameof(ExpressionType.Subtract):
                    {
                        ExpressionNode_Binary binary = data;
                        var @operator = operatorMap[data.nodeType];
                        return $"({EvalExpression(arg, binary.left)} {@operator} {EvalExpression(arg, binary.right)})";
                    }
                case nameof(ExpressionType.Negate):
                    {
                        ExpressionNode_Unary unary = data;
                        return $"(-{EvalExpression(arg, unary.body)})";
                    }
                case NodeType.MethodCall:
                    {
                        ExpressionNode_MethodCall methodCall = data;

                        switch (methodCall.methodName)
                        {
                            // ##1 in
                            case nameof(Enumerable.Contains):
                                {
                                    var values = methodCall.arguments[0];
                                    var member = methodCall.arguments[1];
                                    return $"{EvalExpression(arg, member)} in {EvalExpression(arg, values)}";
                                }

                            // ##2 db primitive function
                            case nameof(DbFunction.Call):
                                {
                                    var functionName = methodCall.arguments[0].value as string;
                                    var argList = methodCall.arguments.AsQueryable().Skip(1).Select(argNode => EvalExpression(arg, argNode)).ToList();
                                    var funcArgs = string.Join(",", argList);
                                    return $"{functionName}({funcArgs})";
                                }


                            #region ##3 Aggregate
                            case nameof(Enumerable.Count) when methodCall.arguments.Length == 1:
                                {
                                    var stream = methodCall.arguments[0] as ExpressionNode_Member;
                                    //if (stream?.nodeType != NodeType.Member) break;
                                    return "Count(*)";
                                }
                            case nameof(Enumerable.Max) or nameof(Enumerable.Min) or nameof(Enumerable.Sum) or nameof(Enumerable.Average) when methodCall.arguments.Length == 2:
                                {
                                    var source = methodCall.arguments[0];
                                    if (source?.nodeType != NodeType.Member) break;

                                    var entityType = methodCall.MethodCall_GetParamTypes()[0].GetGenericArguments()[0];
                                    source = TypeUtil.Clone(source).Member_SetType(entityType);


                                    var lambdaFieldSelect = methodCall.arguments[1] as ExpressionNode_Lambda;

                                    var parameterName = lambdaFieldSelect.parameterNames[0];
                                    var parameterValue = source;


                                    Func<ExpressionNode_Member, ExpressionNode> GetParameter = (member) =>
                                    {
                                        if (member.nodeType == NodeType.Member && member.parameterName == parameterName)
                                        {
                                            if (string.IsNullOrWhiteSpace(member.memberName))
                                            {
                                                return parameterValue;
                                            }
                                            else
                                            {
                                                return ExpressionNode.Member(objectValue: parameterValue, memberName: member.memberName).Member_SetType(member.Member_GetType());
                                            }
                                        }
                                        return default;
                                    };

                                    var body = StreamReader.DeepClone(lambdaFieldSelect.body, GetParameter);
                                    var funcName = methodCall.methodName;
                                    if (funcName == nameof(Enumerable.Average)) funcName = "AVG";

                                    return $"{funcName}({EvalExpression(arg, body)})";
                                }
                                #endregion
                        }
                        throw new NotSupportedException("[QueryTranslator] not suported MethodCall: " + methodCall.methodName);
                    }

                #region Read Value

                case NodeType.Member:
                    return GetSqlField(data, arg.dbContext);

                case NodeType.Constant:
                    {
                        ExpressionNode_Constant constant = data;
                        var value = constant.value;
                        if (value == null)
                        {
                            return "null";
                        }
                        else if (value is not string && value is IEnumerable enumerable)
                        {
                            StringBuilder sql = null;

                            foreach (var item in enumerable)
                            {
                                if (item == null) continue;
                                if (sql == null)
                                {
                                    sql = new StringBuilder("(");
                                    var paramName = arg.NewParamName();
                                    arg.sqlParam[paramName] = item;
                                    sql.Append(GenerateParameterName(paramName));
                                }
                                else
                                {
                                    var paramName = arg.NewParamName();
                                    arg.sqlParam[paramName] = item;
                                    sql.Append(",").Append(GenerateParameterName(paramName));
                                }
                            }
                            if (sql == null) return "(null)";
                            return sql.Append(")").ToString();
                        }
                        else
                        {
                            var paramName = arg.NewParamName();
                            arg.sqlParam[paramName] = value;
                            return GenerateParameterName(paramName);
                        }

                    }
                    #endregion
            }
            throw new NotSupportedException("[QueryTranslator] not suported nodeType: " + data.nodeType);
        }


        protected readonly static Dictionary<string, string> operatorMap = new Dictionary<string, string>
        {
            [NodeType.Equal] = "=",
            [NodeType.NotEqual] = "!=",
            [NodeType.LessThan] = "<",
            [NodeType.LessThanOrEqual] = "<=",
            [NodeType.GreaterThan] = ">",
            [NodeType.GreaterThanOrEqual] = ">=",

            [nameof(ExpressionType.Divide)] = "/",
            [nameof(ExpressionType.Modulo)] = "%",
            [nameof(ExpressionType.Multiply)] = "*",
            [nameof(ExpressionType.Power)] = "^",
            [nameof(ExpressionType.Subtract)] = "-",
        };
        #endregion


        // #0 Schema :  PrepareCreate PrepareDrop
        public abstract string PrepareCreate(IEntityDescriptor entityDescriptor);
        public abstract string PrepareDrop(IEntityDescriptor entityDescriptor);


        #region #1 Create :  PrepareAdd
        public virtual EAddType Entity_GetAddType(SqlTranslateArgument arg, object entity)
        {
            var key = arg.entityDescriptor.key;
            if (key == null) return EAddType.noKeyColumn;

            var keyValue = key.GetValue(entity);
            if (keyValue is not null && !keyValue.Equals(TypeUtil.DefaultValue(arg.entityDescriptor.key.type))) return EAddType.keyWithValue;

            if (key.isIdentity) return EAddType.identityKey;

            throw new ArgumentException("Key could not be empty.");
            //return EAddType.unexpectedEmptyKey;
        }

        protected virtual (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareAdd(SqlTranslateArgument arg, IColumnDescriptor[] columns)
        {
            /* //sql
             insert into user(name,fatherId,motherId) values('',0,0);
              */

            var entityDescriptor = arg.entityDescriptor;

            // #1 GetSqlParams 
            Func<object, Dictionary<string, object>> GetSqlParams = (entity) =>
            {
                var sqlParam = new Dictionary<string, object>();
                foreach (var column in columns)
                {
                    sqlParam[column.columnName] = column.GetValue(entity);
                }
                return sqlParam;
            };

            #region #2 columns 
            List<string> columnNames = new List<string>();
            List<string> valueParams = new List<string>();

            foreach (var column in columns)
            {
                columnNames.Add(DelimitIdentifier(column.columnName));
                valueParams.Add(GenerateParameterName(column.columnName));
            }
            #endregion

            // #3 build sql
            string sql = $@"insert into {DelimitTableName(entityDescriptor)}({string.Join(",", columnNames)}) values({string.Join(",", valueParams)});";

            return (sql, GetSqlParams);
        }

        public virtual (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareAdd(SqlTranslateArgument arg)
        {
            return PrepareAdd(arg, arg.entityDescriptor.allColumns);
        }

        public virtual (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareIdentityAdd(SqlTranslateArgument arg) => throw new NotImplementedException();
        #endregion


        #region #2 Retrieve : PrepareGet PrepareQuery
        public virtual string PrepareGet(SqlTranslateArgument arg)
        {
            var entityDescriptor = arg.entityDescriptor;

            // #2 build sql
            string sql = $@"select * from {DelimitTableName(entityDescriptor)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

            return sql;
        }

        public abstract (string sql, Dictionary<string, object> sqlParam, IDbDataReader dataReader) PrepareQuery(QueryTranslateArgument arg, CombinedStream combinedStream);
        #endregion



        #region #3 Update: PrepareUpdate PrepareExecuteUpdate
        public virtual (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareUpdate(SqlTranslateArgument arg)
        {
            /* //sql
                update user set name='' where id=7;
            */

            var entityDescriptor = arg.entityDescriptor;
            var sqlParam = new Dictionary<string, object>();

            // #1 GetSqlParams
            Func<object, Dictionary<string, object>> GetSqlParams = (entity) =>
            {
                var sqlParam = new Dictionary<string, object>();
                foreach (var column in entityDescriptor.allColumns)
                {
                    var columnName = column.columnName;
                    var value = column.GetValue(entity);

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
                columnName = column.columnName;
                columnsToUpdate.Add($"{DelimitIdentifier(columnName)}={GenerateParameterName(columnName)}");
            }

            // #3 build sql
            string sql = $@"update {DelimitTableName(entityDescriptor)} set {string.Join(",", columnsToUpdate)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

            return (sql, GetSqlParams);
        }

        public abstract (string sql, Dictionary<string, object> sqlParam) PrepareExecuteUpdate(QueryTranslateArgument arg, CombinedStream combinedStream);

        #endregion


        #region #4 Delete: PrepareDelete PrepareDeleteRange PrepareExecuteDelete

        public virtual string PrepareDelete(SqlTranslateArgument arg)
        {
            /* //sql
            delete from user where id = 7;
            */
            var entityDescriptor = arg.entityDescriptor;

            // #2 build sql
            string sql = $@"delete from {DelimitTableName(entityDescriptor)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)} ; ";

            return sql;
        }

        public virtual (string sql, Dictionary<string, object> sqlParam) PrepareDeleteByKeys<Key>(SqlTranslateArgument arg, IEnumerable<Key> keys)
        {
            //  delete from user where id in ( 7 ) ;

            var entityDescriptor = arg.entityDescriptor;

            StringBuilder sql = new StringBuilder();
            Dictionary<string, object> sqlParam = new();

            sql.Append("delete from ").Append(DelimitTableName(entityDescriptor)).Append(" where ").Append(DelimitIdentifier(entityDescriptor.keyName)).Append(" in (");

            int keyIndex = 0;
            foreach (var key in keys)
            {
                var paramName = "p" + (keyIndex++);
                sql.Append(GenerateParameterName(paramName)).Append(",");
                sqlParam[paramName] = key;
            }
            if (keyIndex == 0) sql.Append("null);");
            else
            {
                sql.Length--;
                sql.Append(");");
            }
            return (sql.ToString(), sqlParam);
        }



        public abstract (string sql, Dictionary<string, object> sqlParam) PrepareExecuteDelete(QueryTranslateArgument arg, CombinedStream combinedStream);


        #endregion



    }
}
