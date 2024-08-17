using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery
{
    public partial class StreamReader
    {
        // Join with  Queryable.SelectMany
        // users.SelectMany(
        //      user => users.Where(father => (father.id == user.fatherId)).DefaultIfEmpty(),
        //      (user, father) => new <>f__AnonymousType4`2(user = user, father = father)
        //  )

        CombinedStream SelectMany(Argument arg, IStream source, ExpressionNode_Lambda rightSelector, ExpressionNode_Lambda resultSelector)
        {
            CombinedStream finalStream;
            ExpressionNode parameterValueForLeftStream;

            #region #1 get finalStream and parameterValueForLeftStream
            switch (source)
            {
                case SourceStream sourceStream:
                    {
                        finalStream = new CombinedStream(arg.NewAliasName()) { source = source };
                        parameterValueForLeftStream = ExpressionNode_RenameableMember.Member(stream: sourceStream, rightSelector.Lambda_GetParamTypes()[0]);
                        break;
                    }
                case CombinedStream combinedStream:
                    {
                        if (combinedStream.where == null && combinedStream.orders == null
                            && combinedStream.skip == null && combinedStream.take == null
                            && combinedStream.select?.isDefaultSelect == true)
                        {
                            // merge multiple join
                            finalStream = combinedStream;
                            parameterValueForLeftStream = combinedStream.select.fields;
                            break;
                        }
                        throw new NotSupportedException($"[StreamReader] not support inner select in join sentence");
                    }
                default: throw new NotSupportedException($"[StreamReader] not supported StreamType : " + source?.GetType().Name);
            }
            #endregion



            StreamToJoin rightStreamToJoin;
            #region #2 read rightStreamToJoin
            {
                // rightSelector:
                //      user => users.Where(father => (father.id == user.fatherId)).DefaultIfEmpty()

                var parameterName = rightSelector.parameterNames[0];
                var parameterValue = parameterValueForLeftStream;
                var argForRightStream = arg.WithParameter(parameterName, parameterValue);


                rightStreamToJoin = new StreamToJoin();
                rightStreamToJoin.joinType = EJoinType.InnerJoin;

                ReadNode(argForRightStream, rightSelector.body);

                void ReadNode(Argument argForRightStream, ExpressionNode node)
                {
                    switch (node.nodeType)
                    {
                        case NodeType.Member:
                            {
                                // to get rightStream (row.fathers) in line 08

                                /* // LeftJoin (Queryable.GroupJoin)
                                01  userQuery.GroupJoin(
                                02      userQuery
                                03      , user => user.fatherId
                                04      , father => father.id
                                05      , (user, fathers) => new { user, fathers }
                                06  )
                                07  .SelectMany(
                                08      row => row.fathers.DefaultIfEmpty()
                                09      , (row, father) => new { row, father }
                                10  )
                                11  .Where(row2 => row2.row.user.id > 2)
                                12  .Select(row2 => new { row2.row.user, row2.father }); 
                                 */

                                var rightStream = argForRightStream.DeepClone(node);
                                var rightStreamFromJoin = finalStream.joins.Last();
                                if (rightStream.parameterName != rightStreamFromJoin.right.alias)
                                {
                                    throw new NotSupportedException("[StreamReader] unexpected expression sentence of GroupJoin");
                                }
                                finalStream.joins.Remove(rightStreamFromJoin);
                                rightStreamToJoin = rightStreamFromJoin;
                                return;
                            }
                        case NodeType.MethodCall:
                            {
                                ExpressionNode_MethodCall call = node;
                                switch (call.methodName)
                                {
                                    case "Where":
                                        {
                                            if (rightStreamToJoin.on != null)
                                                throw new Exception("[StreamReader] unexpected multiple where in join");

                                            var source = ReadStream(argForRightStream, call.arguments[0]);
                                            var predicateLambda = call.arguments[1] as ExpressionNode_Lambda;

                                            rightStreamToJoin.right = source;
                                            rightStreamToJoin.on = ReadWhere(argForRightStream, source, predicateLambda);

                                            return;
                                        }
                                    case "DefaultIfEmpty":
                                        {
                                            var source = call.arguments[0];
                                            ReadNode(argForRightStream, source);
                                            rightStreamToJoin.joinType = EJoinType.LeftJoin;
                                            return;
                                        }
                                }
                                throw new Exception("[StreamReader] unexpected method call : " + call.methodName);
                            }
                    }
                    throw new NotSupportedException($"[StreamReader] unexpected expression nodeType : {node.nodeType}");
                }
            }
            #endregion


            // #3 read SelectedFields
            ResultSelector select;
            {
                // left
                var parameterName = resultSelector.parameterNames[0];
                var parameterValue = parameterValueForLeftStream;
                var argForSelect = arg.WithParameter(parameterName, parameterValue);

                // right
                parameterName = resultSelector.parameterNames[1];
                parameterValue = ExpressionNode_RenameableMember.Member(stream: rightStreamToJoin.right, resultSelector.Lambda_GetParamTypes()[1]);
                argForSelect = argForSelect.SetParameter(parameterName, parameterValue);

                select = ReadResultSelector(argForSelect, resultSelector);
            }

            // #4 combine stream
            finalStream.joins ??= new List<StreamToJoin>();
            finalStream.joins.Add(rightStreamToJoin);
            finalStream.select = select;

            return finalStream;

        }



    }
}
