using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vit.Orm.Sql.DataReader
{
    public class EntityReader : IDbDataReader
    {
        public List<string> sqlFields { get; private set; } = new List<string>();

        protected Type entityType;
        protected List<IArgReader> entityArgReaders = new List<IArgReader>();
        protected Delegate lambdaCreateEntity;

        public string BuildSelect(Type entityType, ISqlTranslator sqlTranslator, ExpressionConvertService convertService, ExpressionNode selectedFields)
        {
            this.entityType = entityType;

            var cloner = new ExpressionNodeCloner();
            cloner.clone = (node) =>
            {
                if (node?.nodeType == NodeType.Member)
                {
                    ExpressionNode_Member member = node;

                    var argName = GetArgument(sqlTranslator, member);

                    if (argName != null)
                    {
                        return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                    }
                }
                else if (node?.nodeType == NodeType.MethodCall)
                {
                    ExpressionNode_MethodCall methodCall = node;

                    var argName = GetArgument(sqlTranslator, methodCall);

                    if (argName != null)
                    {
                        return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                    }
                }
                return default;
            };
            ExpressionNode_New newExp = cloner.Clone(selectedFields);


            #region Compile Lambda
            var lambdaNode = ExpressionNode.Lambda(entityArgReaders.Select(m => m.argName).ToArray(), (ExpressionNode)newExp);
            //var strNode = Json.Serialize(lambdaNode);

            var lambdaExp = convertService.ToLambdaExpression(lambdaNode, entityArgReaders.Select(m => m.argType).ToArray());

            lambdaCreateEntity = lambdaExp.Compile();
            #endregion

            // sqlFields
            return String.Join(", ", sqlFields);
        }


        public virtual object ReadData(IDataReader reader)
        {
            return new Func<IDataReader, object>(ReadEntity<string>)
              .GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(entityType)
              .Invoke(this, new object[] { reader });
        }

        object ReadEntity<Entity>(IDataReader reader)
        {
            var list = new List<Entity>();

            while (reader.Read())
            {
                var lambdaArgs = entityArgReaders.Select(m => m.Read(reader)).ToArray();
                var obj = (Entity)lambdaCreateEntity.DynamicInvoke(lambdaArgs);
                list.Add(obj);
            }

            return list;
        }

        protected string GetArgument(ISqlTranslator sqlTranslator, ExpressionNode_Member member)
        {
            // tableName_fieldName   tableName_
            var argUniqueKey = $"arg_{member.objectValue?.parameterName ?? member.parameterName}_{member.memberName}";

            IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

            if (argReader == null)
            {
                var argName = "arg_" + entityArgReaders.Count;

                var argType = member.Member_GetType();

                bool isValueType = TypeUtil.IsValueType(argType);
                if (isValueType)
                {
                    // Value arg
                    string sqlFieldName = sqlTranslator.GetSqlField(member);
                    argReader = new ValueReader(this, argType, argUniqueKey, argName, sqlFieldName);
                }
                else
                {
                    // Entity arg
                    argReader = new ModelReader(this, sqlTranslator, member, argUniqueKey, argName, argType);
                }
                entityArgReaders.Add(argReader);
            }
            return argReader.argName;
        }
        protected string GetArgument(ISqlTranslator sqlTranslator, ExpressionNode_MethodCall methodCall)
        {
            var functionName = methodCall.methodName;
            switch (methodCall.methodName)
            {
                case nameof(Enumerable.Count):
                    {
                        var stream = methodCall.arguments[0] as ExpressionNode_Member;
                        if (stream?.nodeType == NodeType.Member && stream.parameterName != null && stream.memberName == null)
                        {
                            var tableName = stream.parameterName;
                            var columnName = stream.memberName;

                            var argUniqueKey = $"argFunc_{functionName}_{tableName}_{columnName}";

                            IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

                            if (argReader == null)
                            {
                                var argName = "arg_" + entityArgReaders.Count;

                                var argType = typeof(int);

                                // Value arg
                                string sqlFieldName = sqlTranslator.GetSqlField_Aggregate(functionName,tableName, columnName: columnName);
                                argReader = new ValueReader(this, argType, argUniqueKey, argName, sqlFieldName);

                                entityArgReaders.Add(argReader);
                            }
                            return argReader.argName;
                        }
                    }
                    break;
                case nameof(Enumerable.Max) or nameof(Enumerable.Min) or nameof(Enumerable.Sum) or nameof(Enumerable.Average) when methodCall.arguments.Length == 2:
                    {
                        var stream = methodCall.arguments[0] as ExpressionNode_Member;
                        if (stream?.nodeType == NodeType.Member && stream.parameterName != null && stream.memberName == null)
                        {
                            var lambdaFieldSelect = methodCall.arguments[1] as ExpressionNode_Lambda;
                            if (lambdaFieldSelect?.body?.nodeType == NodeType.Member)
                            {
                                var tableName = stream.parameterName;
                                string columnName = lambdaFieldSelect.body.memberName;

                                var argUniqueKey = $"argFunc_{functionName}_{tableName}_{columnName}";

                                IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

                                if (argReader == null)
                                {
                                    var argName = "arg_" + entityArgReaders.Count;

                                    var argType = methodCall.MethodCall_GetReturnType();

                                    // Value arg
                                    string sqlFieldName = sqlTranslator.GetSqlField_Aggregate(functionName,tableName, columnName: columnName);
                                    argReader = new ValueReader(this, argType, argUniqueKey, argName, sqlFieldName);

                                    entityArgReaders.Add(argReader);
                                }
                                return argReader.argName;
                            }
                        }
                    }
                    break;

            }
            //throw new NotSupportedException("[CollectionStream] unexpected method call : " + methodCall.methodName);
            return default;
        }


    }
}
