using System.Collections.Generic;

using Vitorm.StreamQuery.MethodCall;

using Convertor = System.Func<Vitorm.StreamQuery.MethodCall.MethodCallConvertArgrument, Vitorm.StreamQuery.IStream>;

namespace Vitorm.StreamQuery
{
    public partial class StreamReader
    {

        public List<Convertor> methodCallConvertors = new()
        {
            MethodCallConvertor_ExecuteUpdate.Convert,
            MethodCallConvertor_ExecuteEnd.Instance.Convert,
        };


        public virtual void AddMethodCallConvertor(Convertor convertor)
        {
            methodCallConvertors.Add(convertor);
        }
    }
}
