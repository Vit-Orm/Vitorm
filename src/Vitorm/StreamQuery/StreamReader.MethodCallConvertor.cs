using System.Collections.Generic;

using Vitorm.StreamQuery.MethodCall;

using Convertor = System.Func<Vitorm.StreamQuery.MethodCall.MethodCallConvertArgrument, Vitorm.StreamQuery.IStream>;


namespace Vitorm.StreamQuery
{
    public partial class StreamReader
    {

        public List<IMethodConvertor> methodCallConvertors = new()
        {
            MethodCallConvertor_ExecuteEnd.Instance,

            MethodCallConvertor_FromAttribute.Instance,
        };


        public virtual void AddMethodCallConvertor(IMethodConvertor convertor)
        {
            methodCallConvertors.Add(convertor);
        }

        public virtual void AddMethodCallConvertor(Convertor convertor)
        {
            methodCallConvertors.Add(new MethodCallConvertor_Delegate(convertor));
        }
    }
}
