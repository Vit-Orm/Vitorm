namespace Vitorm.StreamQuery.MethodCall
{
    public interface IMethodConvertor
    {
        IStream Convert(MethodCallConvertArgrument methodConvertArg);
    }
}
