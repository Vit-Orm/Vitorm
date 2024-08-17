using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery.MethodCall
{
    public class MethodCallConvertor_ExecuteUpdate
    {
        public static IStream Convert(MethodCallConvertArgrument methodConvertArg)
        {
            ExpressionNode_MethodCall call = methodConvertArg.node;
            var reader = methodConvertArg.reader;
            var arg = methodConvertArg.arg;

            if (call.methodName != nameof(Orm_Extensions.ExecuteUpdate)) return null;


            var source = reader.ReadStream(arg, call.arguments[0]);
            ExpressionNode_Lambda resultSelector = call.arguments[1];
            switch (source)
            {
                case SourceStream sourceStream:
                    {
                        var parameterName = resultSelector.parameterNames[0];
                        var parameterValue = ExpressionNode_RenameableMember.Member(stream: sourceStream, resultSelector.Lambda_GetParamTypes()[0]);

                        var select = reader.ReadResultSelector(arg.WithParameter(parameterName, parameterValue), resultSelector);
                        return new StreamToUpdate(sourceStream) { fieldsToUpdate = select.fields };
                    }
                case CombinedStream combinedStream:
                    {
                        var parameterName = resultSelector.parameterNames[0];
                        var parameterValue = combinedStream.select.fields;
                        var select = reader.ReadResultSelector(arg.WithParameter(parameterName, parameterValue), resultSelector);

                        return new StreamToUpdate(source) { fieldsToUpdate = select.fields };
                    }
            }

            return null;
        }
    }
}
