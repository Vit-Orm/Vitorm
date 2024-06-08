using System;
using System.Collections.Generic;
using System.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Orm.Sql;
using System.Linq.Expressions;
using Vit.Linq.ExpressionTree.CollectionsQuery;

namespace Vit.Orm.Sql.Translator
{
    public abstract class BaseQueryTranslator
    {
        public SqlTranslator sqlTranslator { get; protected set; }


        public BaseQueryTranslator(SqlTranslator sqlTranslator)
        {
            this.sqlTranslator = sqlTranslator;
        }



        public IDbDataReader dataReader;
        public Dictionary<string, object> sqlParam { get; protected set; } = new Dictionary<string, object>();

        protected int paramIndex = 0;
        protected string NewParamName() => "param" + (paramIndex++);


        /// <summary>
        /// return "*";
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual string ReadSelect(CombinedStream stream)
        {
            return "*";
        }

        public virtual string BuildQuery(CombinedStream stream)
        {

            string sql = "";

            // #0  select
            sql += "select " + ReadSelect(stream);


            #region #1 source
            // from User u
            sql += "\r\n from " + ReadInnerTable(stream.source);
            #endregion

            #region #2 join
            {
                stream.joins?.ForEach(streamToJoin =>
                {
                    sql += "\r\n " + (streamToJoin.joinType == EJoinType.InnerJoin ? "inner join" : "left join");
                    sql += " " + ReadInnerTable(streamToJoin.right);

                    var where = ReadEval(streamToJoin.on);
                    if (!string.IsNullOrWhiteSpace(where)) sql += " on " + where;
                });
            }
            #endregion

            // #3 where 1=1
            if (stream.where != null)
            {
                var where = ReadEval(stream.where);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n where " + where;
            }

            // #4 GROUP BY xxxx  Having XXX
            if (stream.isGroupedStream)
            {
                #region ##1 group by
                var node = stream.groupByFields;
                List<string> fields = new();
                if (node?.nodeType == NodeType.New)
                {
                    ExpressionNode_New newNode = node;
                    newNode.constructorArgs.ForEach((Action<MemberBind>)(arg =>
                    {
                        fields.Add(this.ReadEval((ExpressionNode)arg.value));
                    }));
                }
                else if (node?.nodeType == NodeType.Member)
                {
                    fields.Add(ReadEval(node));
                }
                else
                {
                    throw new NotSupportedException("[QueryTranslator] groupByFields is not valid: must be New or Member");
                }
                sql += "\r\n group by " + String.Join(", ", fields);
                #endregion

                #region ##2 having
                if (stream.having != null)
                {
                    var where = ReadEval(stream.having);
                    if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n having " + where;
                }
                #endregion

            }

            // #5 OrderBy
            if (stream.orders?.Any() == true)
            {
                var fields = stream.orders.Select(field => (sqlTranslator.GetSqlField(field.member) + " " + (field.asc ? "asc" : "desc"))).ToList();
                sql += "\r\n order by " + String.Join(", ", fields);
            }

            // #6 limit 1000,10       limit {skip},{take}   |     limit {take}
            if (stream.take != null || stream.skip != null)
            {
                if (stream.skip == null)
                {
                    sql += "\r\n limit " + stream.take;
                }
                else
                {
                    sql += "\r\n limit " + stream.skip + "," + (stream.take ?? 100000000);
                }
            }

            return sql;
        }

        protected string ReadInnerTable(IStream stream)
        {
            if (stream is SourceStream sourceStream)
            {
                IQueryable query = sourceStream.GetSource() as IQueryable;
                var tableName = sqlTranslator.GetTableName(query.ElementType);
                return $"{sqlTranslator.DelimitIdentifier(tableName)} as " + stream.alias;
            }
            if (stream is CombinedStream baseStream)
            {
                var innerQuery = BuildQuery(baseStream);
                return $"({innerQuery}) as " + stream.alias;
            }
            throw new NotSupportedException();
        }



        /// <summary>
        /// read where or value or on
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        protected string ReadEval(ExpressionNode data)
        {
            switch (data.nodeType)
            {

                case NodeType.And:
                    ExpressionNode_And and = data;
                    return $"({ReadEval(and.left)}) and ({ReadEval(and.right)})";

                case NodeType.Or:
                    ExpressionNode_Or or = data;
                    return $"({ReadEval(or.left)}) or ({ReadEval(or.right)})";

                case NodeType.Not:
                    ExpressionNode_Not not = data;
                    return $"not ({ReadEval(not.body)})";

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
                            return $"{ReadEval(binary.left)} " + opera;
                        }
                        else if (binary.left.nodeType == NodeType.Constant && binary.left.value == null)
                        {
                            var opera = data.nodeType == NodeType.Equal ? "is null" : "is not null";
                            return $"{ReadEval(binary.right)} " + opera;
                        }

                        var @operator = operatorMap[data.nodeType];
                        return $"{ReadEval(binary.left)} {@operator} {ReadEval(binary.right)}";
                    }
                case NodeType.LessThan:
                case NodeType.LessThanOrEqual:
                case NodeType.GreaterThan:
                case NodeType.GreaterThanOrEqual:
                    {
                        ExpressionNode_Binary binary = data;
                        var @operator = operatorMap[data.nodeType];
                        return $"{ReadEval(binary.left)} {@operator} {ReadEval(binary.right)}";
                    }
                case NodeType.MethodCall:
                    {
                        ExpressionNode_MethodCall call = data;

                        switch (call.methodName)
                        {
                            case nameof(object.ToString):
                                {
                                    return $"cast({ReadEval(call.@object)} as text)";
                                }

                            #region String method:  StartsWith EndsWith Contains
                            case nameof(string.StartsWith): // String.StartsWith
                                {
                                    var str = call.@object;
                                    var value = call.arguments[0];
                                    return $"{ReadEval(str)} like {ReadEval(value)}||'%'";
                                }
                            case nameof(string.EndsWith): // String.EndsWith
                                {
                                    var str = call.@object;
                                    var value = call.arguments[0];
                                    return $"{ReadEval(str)} like '%'||{ReadEval(value)}";
                                }
                            case nameof(string.Contains) when call.methodCall_typeName == "String": // String.Contains
                                {
                                    var str = call.@object;
                                    var value = call.arguments[0];
                                    return $"{ReadEval(str)} like '%'||{ReadEval(value)}||'%'";
                                }
                            #endregion

                            case nameof(Enumerable.Contains):
                                {
                                    var values = call.arguments[0];
                                    var member = call.arguments[1];
                                    return $"{ReadEval(member)} in {ReadEval(values)}";
                                }

                            case nameof(DbFunction.Call):
                                {
                                    var functionName = call.arguments[0].value as string;
                                    var argList = call.arguments.AsQueryable().Skip(1).Select(arg => ReadEval(arg)).ToList();
                                    var arg = string.Join(",", argList);
                                    return $"{functionName}({arg})";
                                }
                        }
                        throw new NotSupportedException("[QueryTranslator] not suported MethodCall: " + call.methodName);
                    }


                #region Read Value

                case NodeType.Member:
                    return sqlTranslator.GetSqlField(data);

                case NodeType.Constant:
                    ExpressionNode_Constant constant = data;
                    var paramName = NewParamName();
                    sqlParam[paramName] = constant.value;
                    return "@" + paramName;

                case NodeType.Convert:
                    {
                        // cast( 4.1 as signed)

                        ExpressionNode_Convert convert = data;

                        Type targetType = convert.valueType?.ToType();

                        if (targetType == typeof(object)) return ReadEval(convert.body);

                        // Nullable
                        if (targetType.IsGenericType) targetType = targetType.GetGenericArguments()[0];

                        string targetDbType = GetDbType(targetType);

                        var sourceType = convert.body.Member_GetType();
                        if (sourceType != null)
                        {
                            if (sourceType.IsGenericType) sourceType = sourceType.GetGenericArguments()[0];

                            if (targetDbType == GetDbType(sourceType)) return ReadEval(convert.body);
                        }

                        if (targetDbType == "datetime")
                        {
                            return $"DATETIME({ReadEval(convert.body)})";
                        }
                        return $"cast({ReadEval(convert.body)} as {targetDbType})";

                        #region GetDbType
                        string GetDbType(Type type)
                        {
                            if (type == typeof(DateTime))
                                return "datetime";

                            if (type == typeof(string))
                                return "text";

                            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                                return "numeric";

                            if (type == typeof(bool) || type.Name.ToLower().Contains("int")) return "integer";

                            throw new NotSupportedException("[QueryTranslator] unsupported column type:" + type.Name);
                        }
                        #endregion
                    }
                case nameof(ExpressionType.Add):
                    {
                        ExpressionNode_Binary binary = data;

                        // ##1 String Add
                        if (data.valueType?.ToType() == typeof(string))
                        {
                            return $"{ReadEval(binary.left)} || {ReadEval(binary.right)}";
                        }

                        // ##2 Numberic Add
                        return $"{ReadEval(binary.left)} + {ReadEval(binary.right)}";
                    }
                case nameof(ExpressionType.Coalesce):
                    {
                        ExpressionNode_Binary binary = data;
                        return $"COALESCE({ReadEval(binary.left)},{ReadEval(binary.right)})";
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
        };

    }

}
