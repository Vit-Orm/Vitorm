using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public class ResultSelector
    {
        public ExpressionNode_Lambda resultSelector { get; set; }
        public ExpressionNode fields { get; set; }

        public bool? isDefaultSelect { get; set; }
    }
}
