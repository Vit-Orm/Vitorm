using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public partial class StreamReader
    {
        // InnerJoin (Queryable.Join)
        //   userQuery.Join(
        //       userQuery
        //       , user => user.fatherId
        //       , father => father.id
        //       , (user, father) => new { user, father }
        //   );

        CombinedStream Join(Argument arg, IStream source, IStream rightStream, ExpressionNode_Lambda leftKeySelector, ExpressionNode_Lambda rightKeySelector, ExpressionNode_Lambda resultSelector)
        {

            CombinedStream finalStream;
            ExpressionNode parameterValueForLeftStream;

            #region #1 get finalStream and parameterValueForLeftStream
            switch (source)
            {
                case SourceStream sourceStream:
                    {
                        finalStream = new CombinedStream(NewAliasName()) { source = source };
                        parameterValueForLeftStream = ExpressionNode_RenameableMember.Member(stream: sourceStream, leftKeySelector.Lambda_GetParamTypes()[0]);
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


            // #2 read leftKey and rightKey
            ExpressionNode leftKeyFields, rightKeyFields;
            {
                var parameterName = leftKeySelector.parameterNames[0];
                var parameterValue = parameterValueForLeftStream;
                var newArg = arg.WithParameter(parameterName, parameterValue);
                leftKeyFields = ReadFields(newArg, leftKeySelector);
            }
            {
                var parameterName = rightKeySelector.parameterNames[0];
                var parameterValue = ExpressionNode_RenameableMember.Member(stream: rightStream, rightKeySelector.Lambda_GetParamTypes()[0]);
                var newArg = arg.WithParameter(parameterName, parameterValue);
                rightKeyFields = ReadFields(newArg, rightKeySelector);
            }



            StreamToJoin rightStreamToJoin;
            #region #3 read rightStreamToJoin
            {
                // read on
                ExpressionNode on = null;
                if (leftKeyFields.nodeType == NodeType.New)
                {
                    // ##1 key is multiple fields

                    var leftKeys = leftKeyFields.constructorArgs;
                    var rightKeys = rightKeyFields.constructorArgs;

                    leftKeys.ForEach(leftKey =>
                    {
                        var rightKey = rightKeys.First(key => key.name == leftKey.name);
                        var curWhere = ExpressionNode.Binary(NodeType.Equal, leftKey.value, rightKey.value);

                        if (on == null) on = curWhere;
                        else on = ExpressionNode.Binary(NodeType.AndAlso, on, curWhere);
                    });
                }
                else
                {
                    // ##2 key is single field
                    on = ExpressionNode.Binary(NodeType.Equal, leftKeyFields, rightKeyFields);
                }

                rightStreamToJoin = new StreamToJoin { joinType = EJoinType.InnerJoin, right = rightStream, on = on };
            }
            #endregion


            // #4 read SelectedFields
            SelectedFields select;
            {
                // left
                var parameterName = resultSelector.parameterNames[0];
                var parameterValue = parameterValueForLeftStream;
                var argForSelect = arg.WithParameter(parameterName, parameterValue);

                // right
                parameterName = resultSelector.parameterNames[1];
                parameterValue = ExpressionNode_RenameableMember.Member(stream: rightStreamToJoin.right, resultSelector.Lambda_GetParamTypes()[1]);
                argForSelect = argForSelect.SetParameter(parameterName, parameterValue);

                select = ReadFieldSelect(argForSelect, resultSelector);
            }


            // #4 combine stream
            finalStream.joins ??= new List<StreamToJoin>();
            finalStream.joins.Add(rightStreamToJoin);
            finalStream.select = select;

            return finalStream;

        }



    }
}
