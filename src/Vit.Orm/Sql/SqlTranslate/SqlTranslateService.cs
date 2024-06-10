using System;
using System.Collections.Generic;

using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Orm.Entity;
using System.Linq;
using System.Linq.Expressions;
using Vit.Linq.ExpressionTree.ExpressionConvertor;

namespace Vit.Orm.Sql.SqlTranslate
{
    public abstract class SqlTranslateService : ISqlTranslateService
    {
        public SqlTranslateService()
        {
        }



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
                memberName = dbContext.GetEntityDescriptor(entityType)?.keyName;
            }

            // 1: {"nodeType":"Member","parameterName":"a0","memberName":"id"}
            // 2: {"nodeType":"Member","objectValue":{"parameterName":"a0","nodeType":"Member"},"memberName":"id"}
            return GetSqlField(member.objectValue?.parameterName ?? member.parameterName, memberName);
        }

        protected abstract string GetDbType(Type type);


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
                case NodeType.And:
                    ExpressionNode_And and = data;
                    return $"({EvalExpression(arg, and.left)}) and ({EvalExpression(arg, and.right)})";

                case NodeType.Or:
                    ExpressionNode_Or or = data;
                    return $"({EvalExpression(arg, or.left)}) or ({EvalExpression(arg, or.right)})";

                case NodeType.Not:
                    ExpressionNode_Not not = data;
                    return $"not ({EvalExpression(arg, not.body)})";

                case NodeType.ArrayIndex:
                    throw new NotSupportedException(data.nodeType);
                //ExpressionNode_ArrayIndex arrayIndex = data;
                //return Expression.ArrayIndex(ToExpression(arg, arrayIndex.left), ToExpression(arg, arrayIndex.right));
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
                        return $"{EvalExpression(arg, binary.left)} {@operator} {EvalExpression(arg, binary.right)}";
                    }
                case NodeType.LessThan:
                case NodeType.LessThanOrEqual:
                case NodeType.GreaterThan:
                case NodeType.GreaterThanOrEqual:
                    {
                        ExpressionNode_Binary binary = data;
                        var @operator = operatorMap[data.nodeType];
                        return $"{EvalExpression(arg, binary.left)} {@operator} {EvalExpression(arg, binary.right)}";
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
                                    var stream = methodCall.arguments[0] as ExpressionNode_Member;
                                    if (stream?.nodeType != NodeType.Member) break;


                                    var lambdaFieldSelect = methodCall.arguments[1] as ExpressionNode_Lambda;

                                    var parameterName = lambdaFieldSelect.parameterNames[0];
                                    var parameterValue = (ExpressionNode)stream;
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
                    ExpressionNode_Constant constant = data;
                    var paramName = arg.NewParamName();
                    arg.sqlParam[paramName] = constant.value;
                    return "@" + paramName;

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
        };
        #endregion


        // #0 Schema :  PrepareCreate
        public abstract string PrepareCreate(IEntityDescriptor entityDescriptor);


        #region #1 Create :  PrepareAdd
        public virtual (string sql, Func<object, Dictionary<string, object>> GetSqlParams) PrepareAdd(SqlTranslateArgument arg)
        {
            /* //sql
             insert into user(name,birth,fatherId,motherId) values('','','');
             select seq from sqlite_sequence where name='user';
              */
            var entityDescriptor = arg.entityDescriptor;

            var columns = entityDescriptor.columns;

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

            return (sql, GetSqlParams);
        }
        #endregion


        #region #2 Retrieve : PrepareGet PrepareQuery
        public virtual string PrepareGet(SqlTranslateArgument arg)
        {
            var entityDescriptor = arg.entityDescriptor;

            // #2 build sql
            string sql = $@"select * from {DelimitIdentifier(entityDescriptor.tableName)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

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
            string sql = $@"delete from {DelimitIdentifier(entityDescriptor.tableName)} where {DelimitIdentifier(entityDescriptor.keyName)}={GenerateParameterName(entityDescriptor.keyName)};";

            return sql;
        }


        public virtual string PrepareDeleteRange(SqlTranslateArgument arg)
        {
            /* //sql
            delete from user where id in ( 7 ) ;
            */
            var entityDescriptor = arg.entityDescriptor;

            // #2 build sql
            string sql = $@"delete from {DelimitIdentifier(entityDescriptor.tableName)} where {DelimitIdentifier(entityDescriptor.keyName)} in {GenerateParameterName("keys")};";

            return sql;
        }


        public abstract (string sql, Dictionary<string, object> sqlParam) PrepareExecuteDelete(QueryTranslateArgument arg, CombinedStream combinedStream);


        #endregion



    }
}
