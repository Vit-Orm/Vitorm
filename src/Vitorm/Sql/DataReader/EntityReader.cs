using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader
{
    public class EntityReader : IDbDataReader
    {

        public class SqlColumns
        {
            List<Column> columns = new();

            /// <summary>
            /// entity field , try get sql column and return sqlColumnIndex
            /// </summary>
            /// <param name="sqlTranslator"></param>
            /// <param name="tableName"></param>
            /// <param name="columnDescriptor"></param>
            /// <returns></returns>
            public int AddSqlColumnAndGetIndex(ISqlTranslateService sqlTranslator, string tableName, IColumnDescriptor columnDescriptor)
            {
                var sqlColumnName = sqlTranslator.GetSqlField(tableName, columnDescriptor.columnName);

                var sqlColumnIndex = columns.FirstOrDefault(m => m.sqlColumnName == sqlColumnName)?.sqlColumnIndex ?? -1;
                if (sqlColumnIndex < 0)
                {
                    sqlColumnIndex = columns.Count;
                    columns.Add(new Column { tableName = tableName, columnDescriptor = columnDescriptor, sqlColumnName = sqlColumnName, sqlColumnAlias = "c" + sqlColumnIndex, sqlColumnIndex = sqlColumnIndex });
                }
                return sqlColumnIndex;
            }

            /// <summary>
            ///  aggregate column in GroupBy
            /// </summary>
            /// <param name="sqlColumnSentence"> for example:   Sum([t0].[userId])  ,  [t0].[userFatherId]  </param>
            /// <returns></returns>
            public int AddSqlColumnAndGetIndex(string sqlColumnSentence)
            {
                var sqlColumnName = sqlColumnSentence;

                var sqlColumnIndex = columns.FirstOrDefault(m => m.sqlColumnName == sqlColumnName)?.sqlColumnIndex ?? -1;
                if (sqlColumnIndex < 0)
                {
                    sqlColumnIndex = columns.Count;
                    columns.Add(new Column { sqlColumnName = sqlColumnName, sqlColumnAlias = "c" + sqlColumnIndex, sqlColumnIndex = sqlColumnIndex });
                }
                return sqlColumnIndex;
            }

            /// <summary>
            ///  alias table column  (  users.Select(u=> new { u.id } )   )
            /// </summary>
            /// <param name="sqlTranslator"></param>
            /// <param name="member"></param>
            /// <param name="dbContext"></param>
            /// <returns></returns>
            public int AddSqlColumnAndGetIndex(ISqlTranslateService sqlTranslator, ExpressionNode_Member member, DbContext dbContext)
            {
                var sqlColumnName = sqlTranslator.GetSqlField(member, dbContext);

                var sqlColumnIndex = columns.FirstOrDefault(m => m.sqlColumnName == sqlColumnName)?.sqlColumnIndex ?? -1;
                if (sqlColumnIndex < 0)
                {
                    sqlColumnIndex = columns.Count;
                    columns.Add(new Column { member = member, sqlColumnName = sqlColumnName, sqlColumnAlias = "c" + sqlColumnIndex, sqlColumnIndex = sqlColumnIndex });
                }
                return sqlColumnIndex;
            }


            public string GetSqlColumns()
            {
                var sqlColumns = columns.Select(column => column.sqlColumnName + " as " + column.sqlColumnAlias);
                return String.Join(", ", sqlColumns);
            }

            public string GetColumnAliasBySqlColumnName(string sqlColumnName)
            {
                return columns.FirstOrDefault(col => col.sqlColumnName == sqlColumnName)?.sqlColumnAlias;
            }

            class Column
            {
                // or table alias
                public string tableName;
                public IColumnDescriptor columnDescriptor;
                public ExpressionNode_Member member;

                public string sqlColumnName;
                public string sqlColumnAlias;

                public int sqlColumnIndex;
            }

        }

        public SqlColumns sqlColumns = new();

       


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

                    // deal with aggregate functions like Sum(id)
                    if (methodCall.methodCall_typeName == "Enumerable")
                    {
                        string argName = null;

                        var sqlColumnSentence = sqlTranslateService.EvalExpression(arg, node);
                        var columnType = methodCall.MethodCall_GetReturnType();
                        argName = GetArgument(sqlColumnSentence, columnType);
                        if (argName != null)
                        {
                            return (true, ExpressionNode.Member(parameterName: argName, memberName: null));
                        }
                    }
                }
                return default;
            };
            ExpressionNode newSelectedFields = cloner.Clone(selectedFields);

            // Compile Lambda
            lambdaCreateEntity = CompileExpression(convertService, entityArgReaders.Select(m => m.argName).ToArray(), newSelectedFields);

            return sqlColumns.GetSqlColumns();
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
                    var sqlColumnIndex = sqlColumns.AddSqlColumnAndGetIndex(sqlTranslator, member, arg.dbContext);
                    argReader = new ValueReader(argType, argUniqueKey, argName, sqlColumnIndex);
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

        protected string GetArgument(string sqlColumnSentence, Type columnType)
        {
            var argUniqueKey = $"argFunc_{sqlColumnSentence}";

            IArgReader argReader = entityArgReaders.FirstOrDefault(reader => reader.argUniqueKey == argUniqueKey);

            if (argReader == null)
            {
                var argName = "arg_" + entityArgReaders.Count;

                var sqlColumnIndex = sqlColumns.AddSqlColumnAndGetIndex(sqlColumnSentence);
                argReader = new ValueReader(columnType, argUniqueKey, argName, sqlColumnIndex);

                entityArgReaders.Add(argReader);
            }
            return argReader.argName;
        }



    }
}
