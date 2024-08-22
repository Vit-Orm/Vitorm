using System;

using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.StreamQuery.MethodCall;

namespace Vitorm.StreamQuery
{
    /// <summary>
    /// Mark this method to be able to convert to IStream from ExpressionNode when executing query. Arguments count must be 0, for example : query.ToListAsync() 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class StreamQuery_CustomMethodAttribute : Attribute, Vitorm.StreamQuery.MethodCall.IMethodConvertor
    {
        public IStream Convert(MethodCallConvertArgrument methodConvertArg)
        {
            ExpressionNode_MethodCall call = methodConvertArg.node;
            var reader = methodConvertArg.reader;
            var arg = methodConvertArg.arg;

            if (call.arguments?.Length != 1) return null;

            var source = reader.ReadStream(arg, call.arguments[0]);
            CombinedStream combinedStream = reader.AsCombinedStream(arg, source);

            combinedStream.method = call.methodName;
            return combinedStream;
        }
    }


}
