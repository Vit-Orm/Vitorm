using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery.MethodCall
{
    public class MethodCallConvertArgrument
    {
        public StreamReader reader { get; set; }
        public StreamReaderArgument arg { get; set; }

        public ExpressionNode_MethodCall node { get; set; }
    }
}
