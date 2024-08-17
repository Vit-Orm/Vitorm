using Convertor = System.Func<Vitorm.StreamQuery.MethodCall.MethodCallConvertArgrument, Vitorm.StreamQuery.IStream>;

namespace Vitorm.StreamQuery.MethodCall
{
    public class MethodCallConvertor_Delegate : IMethodConvertor
    {
        public MethodCallConvertor_Delegate(Convertor convertor) => this.convertor = convertor;

        Convertor convertor;
        public IStream Convert(MethodCallConvertArgrument methodConvertArg) => convertor(methodConvertArg);
    }
}
