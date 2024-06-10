using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Orm.Sql.SqlTranslate;

namespace Vit.Orm.Sql.DataReader
{
    public class EntityReader : IDbDataReader
    {
        public List<string> sqlFields { get; private set; } = new List<string>();

        protected Type entityType;
        protected List<IArgReader> entityArgReaders = new List<IArgReader>();
        protected Delegate lambdaCreateEntity;

        public string BuildSelect(QueryTranslateArgument arg, Type entityType, ISqlTranslateService sqlTranslateService, ExpressionConvertService convertService, ExpressionNode selectedFields)
        {
            this.entityType = entityType;

            var cloner = new ExpressionNodeCloner();
            cloner.clone = (node) =>
            {
                if (node?.nodeType == NodeType.Member)
                {
                    ExpressionNode_Member member = node;

                    var argName = GetArgument(arg, sqlTranslateService, member);

                    if (argName != null)
                    {
                        return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                    }
                }
                else if (node?.nodeType == NodeType.MethodCall)
                {
                    ExpressionNode_MethodCall methodCall = node;
                    if (methodCall.methodCall_typeName == "Enumerable")
                    {
                        string argName = null;

                        var sqlField = sqlTranslateService.EvalExpression(arg, node);
                        var fieldType = methodCall.MethodCall_GetReturnType();
                        argName = GetArgument(sqlField, fieldType);
                        if (argName != null)
                        {
                            return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                        }
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
            var fields = sqlFields.Select((f, index) => f + " as c" + index);
            return String.Join(", ", fields);
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

        protected string GetArgument(QueryTranslateArgument arg, ISqlTranslateService sqlTranslator, ExpressionNode_Member member)
        {

            // 1: {"nodeType":"Member","parameterName":"a0","memberName":"id"}
            // 2: {"nodeType":"Member","objectValue":{"parameterName":"a0","nodeType":"Member"},"memberName":"id"}
            var tableName = member.objectValue?.parameterName ?? member.parameterName;


            // tableName_fieldName   tableName_
            var argUniqueKey = $"arg_{tableName}_{member.memberName}";

            IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

            if (argReader == null)
            {
                var argName = "arg_" + entityArgReaders.Count;

                var argType = member.Member_GetType();

                bool isValueType = TypeUtil.IsValueType(argType);
                if (isValueType)
                {
                    // Value arg
                    string sqlFieldName = sqlTranslator.GetSqlField(member,arg.dbContext);
                    argReader = new ValueReader(this, argType, argUniqueKey, argName, sqlFieldName);
                }
                else
                {
                    // Entity arg
                    var entityDescriptor = arg.dbContext.GetEntityDescriptor(argType);

                    argReader = new ModelReader(this, sqlTranslator, tableName, argUniqueKey, argName, argType, entityDescriptor);
                }
                entityArgReaders.Add(argReader);
            }
            return argReader.argName;
        }

        protected string GetArgument(string sqlField, Type fieldType)
        {
            var argUniqueKey = $"argFunc_{sqlField}";

            IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

            if (argReader == null)
            {
                var argName = "arg_" + entityArgReaders.Count;

                argReader = new ValueReader(this, fieldType, argUniqueKey, argName, sqlField);

                entityArgReaders.Add(argReader);
            }
            return argReader.argName;
        }
 


    }
}
