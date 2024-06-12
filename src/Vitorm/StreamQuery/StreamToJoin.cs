
using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public class StreamToJoin
    {
        // LeftJoin , InnerJoin
        public EJoinType joinType { get; set; }
        public IStream right { get; set; }

        //  a1.id==b2.id
        public ExpressionNode on { get; set; }
    }
}
