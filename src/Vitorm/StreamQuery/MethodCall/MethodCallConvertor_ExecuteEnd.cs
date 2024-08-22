using System.Collections.Generic;

using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery.MethodCall
{
    public class MethodCallConvertor_ExecuteEnd : IMethodConvertor
    {

        public static MethodCallConvertor_ExecuteEnd Instance =
            new MethodCallConvertor_ExecuteEnd(
                new List<string>()
                {
                    nameof(Vit.Linq.Queryable_Extensions.ToListAndTotalCount),
                    nameof(Vit.Linq.Queryable_Extensions.TotalCount),
                    nameof(Vit.Linq.Queryable_Extensions.ToListAsync),
                }
            );


        public MethodCallConvertor_ExecuteEnd(List<string> methodNames = null)
        {
            this.methodNames = methodNames ?? new();
        }

        public List<string> methodNames { get; protected set; }

        public IStream Convert(MethodCallConvertArgrument methodConvertArg)
        {
            ExpressionNode_MethodCall call = methodConvertArg.node;
            var reader = methodConvertArg.reader;
            var arg = methodConvertArg.arg;

            if (call.arguments?.Length != 1) return null;
            if (!methodNames.Contains(call.methodName)) return null;


            var source = reader.ReadStream(arg, call.arguments[0]);
            CombinedStream combinedStream = reader.AsCombinedStream(arg, source);

            combinedStream.method = call.methodName;
            return combinedStream;

        }
    }
}
