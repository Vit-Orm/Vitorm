using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public class SelectedFields
    {
        // root value of ExpressionNode_Member is IStream
        public ExpressionNode fields { get; set; }

        public bool? isDefaultSelect { get; set; }
    }
}
