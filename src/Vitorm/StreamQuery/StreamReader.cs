using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

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
            var arg = new Argument();

            arg.parameterMap = parameterMap?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();
            arg.parameterMap[parameterName] = parameterValue;
            return arg;
        }

        #region SupportNoChildParameter
        public Argument WithParameter(string parameterName, ExpressionNode parameterValue, ExpressionNode noChildParameterValue)
        {
            var arg = new Argument_SupportNoChildParameter { noChildParameterName = parameterName, noChildParameterValue = noChildParameterValue };

            arg.parameterMap = parameterMap?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();
            arg.parameterMap[parameterName] = parameterValue;
            return arg;
        }

        class Argument_SupportNoChildParameter : Argument
        {
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

        /// <summary>
        /// lambda:
        ///     (query,query2) => query.SelectMany(query2).Where().OrderBy().Skip().Take().Select().ToList();
        /// </summary>
        /// <param name="lambda"> </param>
        /// <returns> </returns>
        public static IStream ReadNode(ExpressionNode_Lambda lambda)
        {
            return new StreamReader().ReadFromNode(lambda);
        }


        /// <summary>
        /// lambda:
        ///     (query,query2) => query.SelectMany(query2).Where().OrderBy().Skip().Take().Select().ToList();
        /// </summary>
        /// <param name="lambda"> </param>
        /// <returns> </returns>
        public IStream ReadFromNode(ExpressionNode_Lambda lambda)
        {
            var arg = new Argument();
            return ReadStream(arg, lambda.body);
        }
        int aliasNameCount = 0;
        string NewAliasName()
        {
            return "t" + (aliasNameCount++);
        }

        CombinedStream ReadStreamWithWhere(Argument arg, IStream source, ExpressionNode_Lambda predicateLambda)
        {
            switch (source)
            {
                case SourceStream sourceStream:
                    {
                        ExpressionNode where = ReadWhere(arg, source, predicateLambda);

                        var combinedStream = ToCombinedStream(sourceStream);
                        combinedStream.where = where;

                        return combinedStream;
                    }
                case CombinedStream groupedStream when groupedStream.isGroupedStream:
                    {
                        var parameterName = predicateLambda.parameterNames[0];
                        var groupByFields = groupedStream.groupByFields;
                        var memberBind = new MemberBind { name = "Key", value = groupByFields };
                        var parameterValue = ExpressionNode.New(constructorArgs: new() { memberBind });
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
        CombinedStream ToCombinedStream(SourceStream source)
        {
            Type entityType = source.GetEntityType();
            var selectedFields = ExpressionNode.Member(parameterName: source.alias, memberName: null).Member_SetType(entityType);
            var select = new SelectedFields { fields = selectedFields, isDefaultSelect = true };

            return new CombinedStream(NewAliasName()) { source = source, select = select };
        }
        CombinedStream AsCombinedStream(IStream source)
        {
            if (source is CombinedStream combinedStream) return combinedStream;
            if (source is SourceStream sourceStream) return ToCombinedStream(sourceStream);
            return null;
        }


        // query.SelectMany(query2).Where().Where().OrderBy().Skip().Take().Select()
        IStream ReadStream(Argument arg, ExpressionNode node)
        {
            switch (node.nodeType)
            {
                case NodeType.Member:
                    {
                        ExpressionNode_Member member = node;
                        var oriValue = member.Member_GetOriValue();
                        if (oriValue != null)
                            return new SourceStream(oriValue, NewAliasName());
                        break;
                    }
                case NodeType.Constant:
                    {
                        ExpressionNode_Constant constant = node;
                        var oriValue = constant.value;
                        return new SourceStream(oriValue, NewAliasName());
                    }
                case NodeType.MethodCall:
                    {
                        ExpressionNode_MethodCall call = node;
                        var source = ReadStream(arg, call.arguments[0]);

                        switch (call.methodName)
                        {
                            case "Where":
                                {
                                    var predicateLambda = call.arguments[1] as ExpressionNode_Lambda;
                                    var stream = ReadStreamWithWhere(arg, source, predicateLambda);
                                    if (stream == default) break;
                                    return stream;
                                }
                            case "FirstOrDefault" or "First" or "LastOrDefault" or "Last" when call.arguments.Length == 2:
                                {
                                    var predicateLambda = call.arguments[1] as ExpressionNode_Lambda;
                                    var stream = ReadStreamWithWhere(arg, source, predicateLambda);
                                    if (stream == default) break;
                                    stream.method = call.methodName;
                                    return stream;
                                }
                            case "Distinct":
                                {
                                    var combinedStream = AsCombinedStream(source);

                                    combinedStream.distinct = true;
                                    return combinedStream;
                                }
                            case "Select":
                                {
                                    ExpressionNode_Lambda resultSelector = call.arguments[1];

                                    switch (source)
                                    {
                                        case SourceStream sourceStream:
                                            {
                                                var parameterName = resultSelector.parameterNames[0];
                                                var parameterValue = ExpressionNode_RenameableMember.Member(stream: sourceStream, resultSelector.Lambda_GetParamTypes()[0]);
                                                var newArg = arg.WithParameter(parameterName, parameterValue);
                                                var select = ReadFieldSelect(newArg, resultSelector);

                                                return new CombinedStream(NewAliasName()) { source = sourceStream, select = select };
                                            }
                                        case CombinedStream groupedStream when groupedStream.isGroupedStream:
                                            {
                                                var parameterName = resultSelector.parameterNames[0];
                                                var groupByFields = groupedStream.groupByFields;
                                                var memberBind = new MemberBind { name = "Key", value = groupByFields };
                                                var parameterValue = ExpressionNode.New(constructorArgs: new() { memberBind });
                                                var noChildParameterValue = ExpressionNode_RenameableMember.Member(stream: groupedStream.source, resultSelector.Lambda_GetParamTypes()[0]);
                                                var newArg = arg.WithParameter(parameterName, parameterValue, noChildParameterValue: noChildParameterValue);

                                                var select = ReadFieldSelect(newArg, resultSelector);
                                                groupedStream.select = select;
                                                return groupedStream;
                                            }
                                        case CombinedStream combinedStream:
                                            {
                                                var parameterName = resultSelector.parameterNames[0];
                                                var parameterValue = combinedStream.select.fields;
                                                var newArg = arg.WithParameter(parameterName, parameterValue);
                                                var select = ReadFieldSelect(newArg, resultSelector);

                                                combinedStream.select = select;
                                                return combinedStream;
                                            }
                                    }
                                    break;
                                }
                            case nameof(Orm_Extensions.ExecuteUpdate):
                                {
                                    ExpressionNode_Lambda resultSelector = call.arguments[1];
                                    switch (source)
                                    {
                                        case SourceStream sourceStream:
                                            {
                                                var parameterName = resultSelector.parameterNames[0];
                                                var parameterValue = ExpressionNode_RenameableMember.Member(stream: sourceStream, resultSelector.Lambda_GetParamTypes()[0]);

                                                var select = ReadFieldSelect(arg.WithParameter(parameterName, parameterValue), resultSelector);
                                                return new StreamToUpdate(sourceStream) { fieldsToUpdate = select.fields };
                                            }
                                        case CombinedStream combinedStream:
                                            {
                                                var parameterName = resultSelector.parameterNames[0];
                                                var parameterValue = combinedStream.select.fields;
                                                var select = ReadFieldSelect(arg.WithParameter(parameterName, parameterValue), resultSelector);

                                                return new StreamToUpdate(source) { fieldsToUpdate = select.fields };
                                            }
                                    }
                                    break;
                                }
                            case "Take":
                            case "Skip":
                                {
                                    CombinedStream combinedStream = AsCombinedStream(source);

                                    var value = (call.arguments[1] as ExpressionNode_Constant)?.value as int?;

                                    if (call.methodName == "Skip")
                                        combinedStream.skip = value;
                                    else
                                        combinedStream.take = value;
                                    return combinedStream;
                                }

                            case "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending":
                                {
                                    CombinedStream combinedStream = AsCombinedStream(source);

                                    var methodName = call.methodName;

                                    var sortField = ReadSortField(call.arguments[1], combinedStream);

                                    var orderParam = new OrderField { member = sortField, asc = !methodName.EndsWith("Descending") };

                                    if (methodName.StartsWith("OrderBy"))
                                    {
                                        combinedStream.orders = new List<OrderField>();
                                    }

                                    combinedStream.orders ??= new List<OrderField>();

                                    combinedStream.orders.Add(orderParam);

                                    return combinedStream;
                                }
                            case "FirstOrDefault" or "First" or "LastOrDefault" or "Last" when call.arguments.Length == 1:
                            case "Count":
                            case nameof(Orm_Extensions.ExecuteDelete):
                            case nameof(Orm_Extensions.ToExecuteString):
                            case "ToList":
                                {
                                    if (call.arguments?.Length != 1) break;

                                    CombinedStream combinedStream = AsCombinedStream(source);

                                    combinedStream.method = call.methodName;
                                    return combinedStream;
                                }
                            case nameof(Queryable.GroupBy):
                                {
                                    ExpressionNode_Lambda resultSelector = call.arguments[1];
                                    return GroupBy(arg, source, resultSelector);
                                }
                            case nameof(Queryable.SelectMany):  // LeftJoin InnerJoin
                                {
                                    ExpressionNode_Lambda rightSelector = call.arguments[1];
                                    ExpressionNode_Lambda resultSelector = call.arguments[2];
                                    return SelectMany(arg, source, rightSelector, resultSelector);
                                }
                            case nameof(Queryable.Join):  // InnerJoin (Queryable.Join)
                            case nameof(Queryable.GroupJoin):  // LeftJoin (Queryable.GroupJoin)
                                {
                                    var rightStream = ReadStream(arg, call.arguments[1]);
                                    ExpressionNode_Lambda leftKeySelector = call.arguments[2];
                                    ExpressionNode_Lambda rightKeySelector = call.arguments[3];
                                    ExpressionNode_Lambda resultSelector = call.arguments[4];
                                    return Join(arg, source, rightStream, leftKeySelector, rightKeySelector, resultSelector);
                                }
                        }
                        throw new NotSupportedException("[StreamReader] unexpected method call : " + call.methodName);
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
        SelectedFields ReadFieldSelect(Argument arg, ExpressionNode_Lambda resultSelector)
        {
            ExpressionNode node = resultSelector.body;
            if (node?.nodeType != NodeType.New && node?.nodeType != NodeType.Member && node?.nodeType != NodeType.Convert)
                throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {node.nodeType}");

            bool isDefaultSelect = false;
            var fields = arg.DeepClone(node);

            if (fields?.nodeType == NodeType.Member)
            {
                ExpressionNode_Member member = fields;
                if (member.parameterName == resultSelector.parameterNames[0] && member.memberName == null) 
                    isDefaultSelect = true;
            }
            else if (fields?.nodeType == NodeType.New)
            {
                bool? existCalculatedField = null;
                if (existCalculatedField != true)
                    existCalculatedField = fields.constructorArgs?.Exists(m => m?.value?.nodeType != NodeType.Member && m?.value?.nodeType != NodeType.New);

                if (existCalculatedField != true)
                    existCalculatedField = fields.memberArgs?.Exists(m => m?.value?.nodeType != NodeType.Member && m?.value?.nodeType != NodeType.New);

                isDefaultSelect = !(existCalculatedField ?? false);
            }

            return new() { fields = fields, isDefaultSelect = isDefaultSelect };
        }

        ExpressionNode ReadSortField(ExpressionNode_Lambda resultSelector, CombinedStream stream)
        {
            ExpressionNode parameterValue;
            if (stream.isGroupedStream)
            {
                var groupByFields = stream.groupByFields;
                var memberBind = new MemberBind { name = "Key", value = groupByFields };
                parameterValue = ExpressionNode.New(constructorArgs: new() { memberBind });
            }
            else
            {
                var entityType = resultSelector.Lambda_GetParamTypes()[0];
                parameterValue = stream.GetSelectedFields(entityType);
            }

            var parameterName = resultSelector.parameterNames[0];
            var arg = new Argument().SetParameter(parameterName, parameterValue);

            ExpressionNode sortField = resultSelector.body;

            //if (sortField?.nodeType != NodeType.Member) throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {sortField.nodeType}");

            var member = arg.DeepClone(sortField);
            return member;
        }

    }
}
