using System.Linq;

using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery.MethodCall
{
    public class MethodCallConvertor_FromAttribute : IMethodConvertor
    {

        public static MethodCallConvertor_FromAttribute Instance = new();


        public IStream Convert(MethodCallConvertArgrument methodConvertArg)
        {
            ExpressionNode_MethodCall call = methodConvertArg.node;

            IMethodConvertor convertor = call.MethodCall_GetMethod()?.GetCustomAttributes(true).FirstOrDefault(attr => attr is IMethodConvertor) as IMethodConvertor;

            return convertor?.Convert(methodConvertArg);
        }
    }
}
