using System;

using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery
{
    public class ExpressionNode_RenameableMember : ExpressionNode
    {
        protected IStream stream;
        public override string parameterName
        {
            get => stream?.alias;
            set { }
        }
        public static ExpressionNode Member(IStream stream, Type memberType)
        {
            var node = new ExpressionNode_RenameableMember
            {
                nodeType = NodeType.Member,
                stream = stream
            };
            node.Member_SetType(memberType);
            return node;
        }
    }
}
