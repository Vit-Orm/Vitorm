using Vit.Extensions.Vitorm_Extensions;
using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ExpressionConvertor.MethodCalls;

namespace Vitorm
{
    public class Environment
    {
        public static ExpressionConvertService convertService;
        static Environment()
        {
            convertService = GetInitedConvertService();
        }

        public static ExpressionConvertService GetInitedConvertService()
        {
            var convertService = new ExpressionConvertService();
            convertService.RegisterMethodConvertor(new MethodConvertor_ForType(typeof(DbFunction)));
            convertService.RegisterMethodConvertor(new MethodConvertor_ForType(typeof(Orm_Extensions)));
            return convertService;
        }
    }
}
