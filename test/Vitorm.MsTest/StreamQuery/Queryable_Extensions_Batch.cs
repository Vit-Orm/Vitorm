using System.Linq.Expressions;

using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Linq.ExpressionTree.ExpressionConvertor.MethodCalls;

using Vitorm.StreamQuery;
using Vitorm.StreamQuery.MethodCall;

namespace Vitorm.MsTest.StreamQuery
{

    public static partial class Queryable_Extensions_Batch
    {

        [CustomMethod]
        public static IEnumerable<List<Result>> Batch<Result>(this IQueryable<Result> source, int batchSize = 5000)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source.Provider.Execute<IEnumerable<List<Result>>>(
                Expression.Call(
                    null,
                    new Func<IQueryable<Result>, int, IEnumerable<List<Result>>>(Batch<Result>).Method,
                    source.Expression, Expression.Constant(batchSize)
                ));
        }


        #region StreamConvertor
        public static IStream Convert(MethodCallConvertArgrument methodConvertArg)
        {
            ExpressionNode_MethodCall call = methodConvertArg.node;
            var reader = methodConvertArg.reader;
            var arg = methodConvertArg.arg;

            //if (call.arguments?.Length != 2) return null;
            if (call.methodName != nameof(Batch)) return null;

            if (call.arguments[1].value is not int batchSize) batchSize = 5000;


            var source = reader.ReadStream(arg, call.arguments[0]);
            CombinedStream combinedStream = reader.AsCombinedStream(arg, source);

            combinedStream.method = call.methodName;
            combinedStream.methodArguments = new object[] { batchSize };

            return combinedStream;

        }
        #endregion
    }
}