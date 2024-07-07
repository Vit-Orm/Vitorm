using System;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public partial class StreamReader
    {
        CombinedStream GroupBy(Argument arg, IStream source, ExpressionNode_Lambda resultSelector)
        {
            switch (source)
            {
                case SourceStream sourceStream:
                case CombinedStream groupedStream when groupedStream.isGroupedStream:
                    {
                        var parameterName = resultSelector.parameterNames[0];
                        var parameterValue = ExpressionNode_RenameableMember.Member(stream: source, resultSelector.Lambda_GetParamTypes()[0]);
                        var newArg = arg.WithParameter(parameterName, parameterValue);
                        var groupByFields = ReadFields(newArg, resultSelector);

                        return new CombinedStream(NewAliasName()) { source = source, groupByFields = groupByFields };
                    }
                case CombinedStream combinedStream:
                    {
                        if (combinedStream.select?.isDefaultSelect == true && combinedStream.joins == null
                            && combinedStream.groupByFields == null && combinedStream.having == null
                            && combinedStream.orders == null
                            && combinedStream.skip == null && combinedStream.take == null
                           )
                        {
                            // nested GroupedStream

                            var parameterName = resultSelector.parameterNames[0];
                            var parameterValue = ExpressionNode_RenameableMember.Member(stream: combinedStream.source, resultSelector.Lambda_GetParamTypes()[0]);

                            var newArg = arg.WithParameter(parameterName, parameterValue);
                            var groupByFields = ReadFields(newArg, resultSelector);

                            combinedStream.groupByFields = groupByFields;
                            return combinedStream;
                        }
                        else
                        {
                            var parameterName = resultSelector.parameterNames[0];
                            var parameterValue = combinedStream.select.fields;

                            var newArg = arg.WithParameter(parameterName, parameterValue);
                            var groupByFields = ReadFields(newArg, resultSelector);

                            return new CombinedStream(NewAliasName()) { source = source, groupByFields = groupByFields };
                        }
                    }
            }

            throw new NotSupportedException($"[StreamReader] do not support StreamType");
        }

    }
}
