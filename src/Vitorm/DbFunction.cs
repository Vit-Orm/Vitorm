using System;

using Vit.Linq.ExpressionNodes;

namespace Vitorm
{

    [ExpressionNode_DataValueType(EDataValueType.other)]
    public static partial class DbFunction
    {
        public static Return Call<Return>(string functionName) => throw new NotImplementedException();
        public static Return Call<Return>(string functionName, object arg) => throw new NotImplementedException();
        public static Return Call<Return>(string functionName, object arg0, object arg1) => throw new NotImplementedException();
        public static Return Call<Return>(string functionName, object arg0, object arg1, object arg2) => throw new NotImplementedException();
        public static Return Call<Return>(string functionName, object arg0, object arg1, object arg2, object arg3) => throw new NotImplementedException();
        public static Return Call<Return>(string functionName, object arg0, object arg1, object arg2, object arg3, object arg4) => throw new NotImplementedException();
        public static Return Call<Return>(string functionName, object arg0, object arg1, object arg2, object arg3, object arg4, object arg5) => throw new NotImplementedException();
    }
}
