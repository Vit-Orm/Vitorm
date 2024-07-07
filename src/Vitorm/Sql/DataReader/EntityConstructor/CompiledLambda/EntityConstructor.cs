using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader.EntityConstructor.CompiledLambda
{
    public class EntityConstructor: IEntityConstructor
    {
        protected List<IArgReader> entityArgReaders = new List<IArgReader>();
        protected Delegate lambdaCreateEntity;

        public void Init(EntityConstructorConfig config, Type entityType, ExpressionNode resultSelector)
        {
            QueryTranslateArgument arg = config.arg;
            ExpressionConvertService convertService = config.convertService;
            ISqlTranslateService sqlTranslateService = config.sqlTranslateService;

            var cloner = new ExpressionNodeCloner();
            cloner.clone = (node) =>
            {
                if (node?.nodeType == NodeType.Member)
                {
                    ExpressionNode_Member member = node;

                    var argName = GetArgument(config, member);

                    if (argName != null)
                    {
                        return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                    }
                }
                else if (node?.nodeType == NodeType.MethodCall)
                {
                    ExpressionNode_MethodCall methodCall = node;

                    // deal with aggregate functions like Sum(id)
                    if (methodCall.methodCall_typeName == "Enumerable")
                    {
                        string argName = null;

                        var sqlColumnSentence = sqlTranslateService.EvalExpression(arg, node);
                        var columnType = methodCall.MethodCall_GetReturnType();
                        argName = GetArgument(config, sqlColumnSentence, columnType);
                        if (argName != null)
                        {
                            return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                        }
                    }
                }
                return default;
            };
            ExpressionNode newResultSelector = cloner.Clone(resultSelector);

            // compile ResultCreate lambda
            lambdaCreateEntity = CompileExpression(convertService, entityArgReaders.Select(m => m.argName).ToArray(), newResultSelector);
        }

        public object ReadEntity(IDataReader reader)
        {
            var lambdaArgs = entityArgReaders.Select(m => m.Read(reader)).ToArray();
            var entity = lambdaCreateEntity.DynamicInvoke(lambdaArgs);
            return entity;
        }



        protected string GetArgument(EntityConstructorConfig config, ExpressionNode_Member member)
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
                    var sqlColumnIndex = config.sqlColumns.AddSqlColumnAndGetIndex(config.sqlTranslateService, member, config.arg.dbContext);
                    argReader = new ValueReader(argType, argUniqueKey, argName, sqlColumnIndex);
                }
                else
                {
                    // Entity arg
                    var entityDescriptor = config.arg.dbContext.GetEntityDescriptor(argType);

                    argReader = new ModelReader(config.sqlColumns, config.sqlTranslateService, tableName, argUniqueKey, argName, argType, entityDescriptor);
                }
                entityArgReaders.Add(argReader);
            }
            return argReader.argName;
        }

        protected string GetArgument(EntityConstructorConfig config, string sqlColumnSentence, Type columnType)
        {
            var argUniqueKey = $"argFunc_{sqlColumnSentence}";

            IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

            if (argReader == null)
            {
                var argName = "arg_" + entityArgReaders.Count;

                var sqlColumnIndex = config.sqlColumns.AddSqlColumnAndGetIndex(sqlColumnSentence);
                argReader = new ValueReader(columnType, argUniqueKey, argName, sqlColumnIndex);

                entityArgReaders.Add(argReader);
            }
            return argReader.argName;
        }











        Delegate CompileExpression(ExpressionConvertService convertService, string[] parameterNames, ExpressionNode newExp)
        {
            var lambdaNode = ExpressionNode.Lambda(entityArgReaders.Select(m => m.argName).ToArray(), newExp);
            // var strNode = Json.Serialize(lambdaNode);

            var lambdaExp = convertService.ToLambdaExpression(lambdaNode, entityArgReaders.Select(m => m.argType).ToArray());

            return lambdaExp.Compile();
        }

        #region CompileExpressionWithCache
        /*
        // If it's anonymous CompilerGenerated type, reuse Compiled invoke.
        // not work  because even if it's anonymous CompilerGenerated type, it also can be reused in same method of different lines.
        static bool cacheEntityCompile = true;
        static ConcurrentDictionary<Type, Delegate> delegateCache = new();
        Delegate CompileExpressionWithCache(ExpressionConvertService convertService, string[] parameterNames, ExpressionNode_New newExp)
        {
            var type = entityType ?? newExp.New_GetType();

            if (!cacheEntityCompile || type == null)
                return CompileExpression(convertService, parameterNames, newExp);

            var isCacheable = Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);

            if (isCacheable && delegateCache.TryGetValue(type, out var lambda)) return lambda;

            lambda = CompileExpression(convertService, parameterNames, newExp);

            if (isCacheable) delegateCache[type] = lambda;

            return lambda;
        }
        //*/
        #endregion

    }
}
