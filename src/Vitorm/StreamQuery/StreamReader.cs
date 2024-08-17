using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.StreamQuery.MethodCall;

namespace Vitorm.StreamQuery
{

    public class ExpressionNode_RenameableMember : ExpressionNode
    {
        protected IStream stream;
        public override string parameterName
        {
            get => stream?.alias;
            set { }
        }
        public static ExpressionNode Member(IStream stream, Type memberType)
        {
            var node = new ExpressionNode_RenameableMember
            {
                nodeType = NodeType.Member,
                stream = stream
            };
            node.Member_SetType(memberType);
            return node;
        }
    }



    public class Argument
    {
        public Argument(Argument arg) : this(arg.aliasConfig)
        {
        }
        public Argument(AliasConfig aliasConfig)
        {
            this.aliasConfig = aliasConfig;
        }

        public class AliasConfig
        {
            int aliasNameCount = 0;
            public string NewAliasName()
            {
                return "t" + (aliasNameCount++);
            }
        }

        protected AliasConfig aliasConfig;

        public string NewAliasName() => aliasConfig.NewAliasName();

        Dictionary<string, ExpressionNode> parameterMap { get; set; }

        public virtual ExpressionNode GetParameter(ExpressionNode_Member member)
        {
            if (member.nodeType == NodeType.Member && !string.IsNullOrWhiteSpace(member.parameterName))
            {
                if (parameterMap?.TryGetValue(member.parameterName, out var parameterValue) == true)
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
            }
            return null;
        }


        public Argument SetParameter(string parameterName, ExpressionNode parameterValue)
        {
            parameterMap ??= new();
            parameterMap[parameterName] = parameterValue;
            return this;
        }


        public Argument WithParameter(string parameterName, ExpressionNode parameterValue)
        {
            var arg = new Argument(aliasConfig);

            arg.parameterMap = parameterMap?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();
            arg.parameterMap[parameterName] = parameterValue;
            return arg;
        }

        #region SupportNoChildParameter
        public Argument WithParameter(string parameterName, ExpressionNode parameterValue, ExpressionNode noChildParameterValue)
        {
            var arg = new Argument_SupportNoChildParameter(this) { noChildParameterName = parameterName, noChildParameterValue = noChildParameterValue };

            arg.parameterMap = parameterMap?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();
            arg.parameterMap[parameterName] = parameterValue;
            return arg;
        }

        class Argument_SupportNoChildParameter : Argument
        {
            public Argument_SupportNoChildParameter(Argument arg) : base(arg)
            {
            }

            public string noChildParameterName;
            public ExpressionNode noChildParameterValue;
            public override ExpressionNode GetParameter(ExpressionNode_Member member)
            {
                if (member.nodeType == NodeType.Member && member.parameterName == noChildParameterName && member.memberName == null)
                {
                    return noChildParameterValue;
                }
                return base.GetParameter(member);
            }

        }
        #endregion


        public ExpressionNode DeepClone(ExpressionNode node)
        {
            Func<ExpressionNode_Member, ExpressionNode> GetParameter = this.GetParameter;

            return StreamReader.DeepClone(node, GetParameter);
        }

    }

    public partial class StreamReader
    {
        public static StreamReader Instance = new StreamReader();

        /// <summary>
        /// lambda:
        ///     (query,query2) => query.SelectMany(query2).Where().OrderBy().Skip().Take().Select().ToList();
        /// </summary>
        /// <param name="lambda"> </param>
        /// <returns> </returns>
        public static IStream ReadNode(ExpressionNode_Lambda lambda)
        {
            return Instance.ReadFromNode(lambda);
        }


        /// <summary>
        /// lambda:
        ///     (query,query2) => query.SelectMany(query2).Where().OrderBy().Skip().Take().Select().ToList();
        /// </summary>
        /// <param name="lambda"> </param>
        /// <returns> </returns>
        public IStream ReadFromNode(ExpressionNode_Lambda lambda)
        {
            var arg = new Argument(new Argument.AliasConfig());
            return ReadStream(arg, lambda.body);
        }


        CombinedStream ReadStreamWithWhere(Argument arg, IStream source, ExpressionNode_Lambda predicateLambda)
        {
            switch (source)
            {
                case SourceStream sourceStream:
                    {
                        ExpressionNode where = ReadWhere(arg, source, predicateLambda);

                        var combinedStream = ToCombinedStream(arg, sourceStream);
                        combinedStream.where = where;

                        return combinedStream;
                    }
                case CombinedStream groupedStream when groupedStream.isGroupedStream:
                    {
                        var parameterName = predicateLambda.parameterNames[0];
                        var groupByFields = groupedStream.groupByFields;
                        var memberBind = new MemberBind { name = "Key", value = groupByFields };
                        var parameterValue = ExpressionNode.New(type: null, constructorArgs: new() { memberBind });
                        var newArg = arg.WithParameter(parameterName, parameterValue);

                        ExpressionNode having = ReadWhere(newArg, predicateLambda.body);
                        if (groupedStream.having == null)
                        {
                            groupedStream.having = having;
                        }
                        else
                        {
                            groupedStream.having = ExpressionNode.AndAlso(groupedStream.having, having);
                        }
                        return groupedStream;
                    }
                case CombinedStream combinedStream:
                    {
                        var parameterName = predicateLambda.parameterNames[0];
                        //var parameterValue = (ExpressionNode)combinedStream.select.fields;
                        var entityType = predicateLambda.Lambda_GetParamTypes()[0];
                        var parameterValue = combinedStream.GetSelectedFields(entityType);

                        var newArg = arg.WithParameter(parameterName, parameterValue);

                        ExpressionNode where = ReadWhere(newArg, predicateLambda.body);
                        if (combinedStream.where == null)
                        {
                            combinedStream.where = where;
                        }
                        else
                        {
                            combinedStream.where = ExpressionNode.AndAlso(combinedStream.where, where);
                        }
                        return combinedStream;
                    }
            }
            return default;
        }
        CombinedStream ToCombinedStream(Argument arg, SourceStream source)
        {
            Type entityType = source.GetEntityType();
            var selectedFields = ExpressionNode.Member(parameterName: source.alias, memberName: null).Member_SetType(entityType);
            var select = new ResultSelector { fields = selectedFields, isDefaultSelect = true };

            return new CombinedStream(arg.NewAliasName()) { source = source, select = select };
        }
        public CombinedStream AsCombinedStream(Argument arg, IStream source)
        {
            if (source is CombinedStream combinedStream) return combinedStream;
            if (source is SourceStream sourceStream) return ToCombinedStream(arg, sourceStream);
            return null;
        }


        // query.SelectMany(query2).Where().Where().OrderBy().Skip().Take().Select()
        public IStream ReadStream(Argument arg, ExpressionNode node)
        {
            switch (node.nodeType)
            {
                case NodeType.Member:
                    {
                        ExpressionNode_Member member = node;
                        var oriValue = member.Member_GetOriValue();
                        if (oriValue != null)
                            return new SourceStream(oriValue, arg.NewAliasName());
                        break;
                    }
                case NodeType.Constant:
                    {
                        ExpressionNode_Constant constant = node;
                        var oriValue = constant.value;
                        return new SourceStream(oriValue, arg.NewAliasName());
                    }
                case NodeType.MethodCall:
                    {
                        #region #1 System method
                        {
                            ExpressionNode_MethodCall call = node;
                            switch (call.methodName)
                            {
                                case nameof(Queryable.Where):
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        var predicateLambda = call.arguments[1] as ExpressionNode_Lambda;
                                        var stream = ReadStreamWithWhere(arg, source, predicateLambda);
                                        if (stream == default) break;
                                        return stream;
                                    }
                                case nameof(Queryable.FirstOrDefault) or nameof(Queryable.First) or nameof(Queryable.LastOrDefault) or nameof(Queryable.Last) when call.arguments.Length == 2:
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        var predicateLambda = call.arguments[1] as ExpressionNode_Lambda;
                                        var stream = ReadStreamWithWhere(arg, source, predicateLambda);
                                        if (stream == default) break;
                                        stream.method = call.methodName;
                                        return stream;
                                    }
                                case nameof(Queryable.Distinct):
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        var combinedStream = AsCombinedStream(arg, source);

                                        combinedStream.distinct = true;
                                        return combinedStream;
                                    }
                                case nameof(Queryable.Select):
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        ExpressionNode_Lambda resultSelector = call.arguments[1];

                                        switch (source)
                                        {
                                            case SourceStream sourceStream:
                                                {
                                                    var parameterName = resultSelector.parameterNames[0];
                                                    var parameterValue = ExpressionNode_RenameableMember.Member(stream: sourceStream, resultSelector.Lambda_GetParamTypes()[0]);
                                                    var newArg = arg.WithParameter(parameterName, parameterValue);
                                                    var select = ReadResultSelector(newArg, resultSelector);

                                                    return new CombinedStream(arg.NewAliasName()) { source = sourceStream, select = select };
                                                }
                                            case CombinedStream groupedStream when groupedStream.isGroupedStream:
                                                {
                                                    var parameterName = resultSelector.parameterNames[0];
                                                    var groupByFields = groupedStream.groupByFields;
                                                    var memberBind = new MemberBind { name = "Key", value = groupByFields };
                                                    var parameterValue = ExpressionNode.New(type: null, constructorArgs: new() { memberBind });
                                                    var noChildParameterValue = ExpressionNode_RenameableMember.Member(stream: groupedStream.source, resultSelector.Lambda_GetParamTypes()[0]);
                                                    var newArg = arg.WithParameter(parameterName, parameterValue, noChildParameterValue: noChildParameterValue);

                                                    var select = ReadResultSelector(newArg, resultSelector);
                                                    groupedStream.select = select;
                                                    return groupedStream;
                                                }
                                            case CombinedStream combinedStream:
                                                {
                                                    var parameterName = resultSelector.parameterNames[0];
                                                    var parameterValue = combinedStream.select.fields;
                                                    var newArg = arg.WithParameter(parameterName, parameterValue);
                                                    var select = ReadResultSelector(newArg, resultSelector);

                                                    combinedStream.select = select;
                                                    return combinedStream;
                                                }
                                        }
                                        break;
                                    }
                                case nameof(Queryable.Take) or nameof(Queryable.Skip):
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        CombinedStream combinedStream = AsCombinedStream(arg, source);

                                        var value = (call.arguments[1] as ExpressionNode_Constant)?.value as int?;

                                        if (call.methodName == nameof(Queryable.Skip))
                                            combinedStream.skip = value;
                                        else
                                            combinedStream.take = value;
                                        return combinedStream;
                                    }

                                case nameof(Queryable.OrderBy) or nameof(Queryable.OrderByDescending) or nameof(Queryable.ThenBy) or nameof(Queryable.ThenByDescending):
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        CombinedStream combinedStream = AsCombinedStream(arg, source);

                                        var methodName = call.methodName;

                                        var sortField = ReadSortField(arg, call.arguments[1], combinedStream);

                                        var orderParam = new ExpressionNodeOrderField { member = sortField, asc = !methodName.EndsWith("Descending") };

                                        if (methodName.StartsWith("OrderBy"))
                                        {
                                            combinedStream.orders = new List<ExpressionNodeOrderField>();
                                        }

                                        combinedStream.orders ??= new List<ExpressionNodeOrderField>();

                                        combinedStream.orders.Add(orderParam);

                                        return combinedStream;
                                    }
                                case nameof(Queryable.FirstOrDefault) or nameof(Queryable.First) or nameof(Queryable.LastOrDefault) or nameof(Queryable.Last) when call.arguments.Length == 1:
                                case nameof(Queryable.Count) or nameof(Enumerable.ToList) when call.arguments.Length == 1:
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);

                                        CombinedStream combinedStream = AsCombinedStream(arg, source);

                                        combinedStream.method = call.methodName;
                                        return combinedStream;
                                    }
                                case nameof(Queryable.GroupBy):
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        ExpressionNode_Lambda resultSelector = call.arguments[1];
                                        return GroupBy(arg, source, resultSelector);
                                    }
                                case nameof(Queryable.SelectMany):  // LeftJoin InnerJoin
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        ExpressionNode_Lambda rightSelector = call.arguments[1];
                                        ExpressionNode_Lambda resultSelector = call.arguments[2];
                                        return SelectMany(arg, source, rightSelector, resultSelector);
                                    }
                                case nameof(Queryable.Join):  // InnerJoin (Queryable.Join)
                                case nameof(Queryable.GroupJoin):  // LeftJoin (Queryable.GroupJoin)
                                    {
                                        var source = ReadStream(arg, call.arguments[0]);
                                        var rightStream = ReadStream(arg, call.arguments[1]);
                                        ExpressionNode_Lambda leftKeySelector = call.arguments[2];
                                        ExpressionNode_Lambda rightKeySelector = call.arguments[3];
                                        ExpressionNode_Lambda resultSelector = call.arguments[4];
                                        return Join(arg, source, rightStream, leftKeySelector, rightKeySelector, resultSelector);
                                    }
                            }

                        }
                        #endregion

                        #region #2 MethodCall convertor
                        {
                            var methodConvertArg = new MethodCallConvertArgrument { reader = this, arg = arg, node = node };
                            foreach (var convertor in methodCallConvertors)
                            {
                                var stream = convertor(methodConvertArg);
                                if (stream != null) return stream;
                            }
                        }
                        #endregion


                        throw new NotSupportedException("[StreamReader] unexpected method call : " + node.methodName);
                    }
            }
            throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {node.nodeType}");
        }



        // predicateLambda:          father => (father.id == user.fatherId)
        ExpressionNode ReadWhere(Argument arg, IStream source, ExpressionNode_Lambda predicateLambda)
        {
            var parameterName = predicateLambda.parameterNames[0];
            var parameterValue = ExpressionNode_RenameableMember.Member(stream: source, predicateLambda.Lambda_GetParamTypes()[0]);
            arg = arg.WithParameter(parameterName, parameterValue);

            return ReadWhere(arg, predicateLambda.body);
        }

        // predicate:           (father.id == user.fatherId)
        ExpressionNode ReadWhere(Argument arg, ExpressionNode predicate)
        {
            return arg.DeepClone(predicate);
        }


        ExpressionNode ReadFields(Argument arg, ExpressionNode_Lambda resultSelector)
        {
            ExpressionNode node = resultSelector.body;
            if (node?.nodeType != NodeType.New && node?.nodeType != NodeType.Member && node?.nodeType != NodeType.Convert)
                throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {node.nodeType}");

            var fields = arg.DeepClone(node);
            return fields;
        }
        public ResultSelector ReadResultSelector(Argument arg, ExpressionNode_Lambda resultSelector)
        {
            ExpressionNode node = resultSelector.body;
            //if (node?.nodeType != NodeType.New && node?.nodeType != NodeType.Member && node?.nodeType != NodeType.Convert)  // could be calculated result like  query.Select(u=>u.id+10)
            //    throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {node.nodeType}");

            bool isDefaultSelect = false;
            var fields = arg.DeepClone(node);

            if (fields?.nodeType == NodeType.Member)
            {
                ExpressionNode_Member member = fields;
                if (member.parameterName == resultSelector.parameterNames[0] && member.memberName == null)
                    isDefaultSelect = true;
            }
            else if (fields?.nodeType == NodeType.New)  // Select sentence in SelectMany or GroupBy method
            {
                bool? existCalculatedField = null;
                if (existCalculatedField != true)
                    existCalculatedField = fields.constructorArgs?.Exists(m => m?.value?.nodeType != NodeType.Member && m?.value?.nodeType != NodeType.New);

                if (existCalculatedField != true)
                    existCalculatedField = fields.memberArgs?.Exists(m => m?.value?.nodeType != NodeType.Member && m?.value?.nodeType != NodeType.New);

                isDefaultSelect = !(existCalculatedField ?? false);
            }

            return new() { fields = fields, isDefaultSelect = isDefaultSelect, resultSelector = resultSelector };
        }

        ExpressionNode ReadSortField(Argument arg, ExpressionNode_Lambda resultSelector, CombinedStream stream)
        {
            ExpressionNode parameterValue;
            if (stream.isGroupedStream)
            {
                var groupByFields = stream.groupByFields;
                var memberBind = new MemberBind { name = "Key", value = groupByFields };
                parameterValue = ExpressionNode.New(type: null, constructorArgs: new() { memberBind });
            }
            else
            {
                var entityType = resultSelector.Lambda_GetParamTypes()[0];
                parameterValue = stream.GetSelectedFields(entityType);
            }

            var parameterName = resultSelector.parameterNames[0];
            var argForClone = new Argument(arg).SetParameter(parameterName, parameterValue);

            ExpressionNode sortField = resultSelector.body;

            //if (sortField?.nodeType != NodeType.Member) throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {sortField.nodeType}");

            var member = argForClone.DeepClone(sortField);
            return member;
        }

    }
}
