using System;
using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public partial class StreamReader
    {
        public static ExpressionNode DeepClone(ExpressionNode node, Func<ExpressionNode_Member, ExpressionNode> GetParameter = null)
        {
            var cloner = new ExpressionNodeCloner();
            cloner.clone = (node) =>
            {
                if (node?.nodeType == NodeType.Member)
                {
                    ExpressionNode_Member member = node;

                    ExpressionNode valueNode = null;

                    if (!string.IsNullOrWhiteSpace(member.parameterName))
                    {
                        // {"nodeType":"Member", "parameterName":"a0", "memberName":"id"}
                        valueNode = GetParameter?.Invoke(member);
                    }
                    else if (member.objectValue?.nodeType == NodeType.Member)
                    {
                        // reduce level:  {"nodeType":"Member","objectValue":{"parameterName":"a0","nodeType":"Member"},"memberName":"id"}
                        var objectNode = cloner.Clone(member.objectValue);
                        valueNode = GetMember(objectNode, member.memberName, sourceMember: member);
                    }

                    valueNode = Reduce(valueNode);

                    if (valueNode != null) return (true, valueNode);
                }
                return default;
            };

            return cloner.Clone(node);

            ExpressionNode Reduce(ExpressionNode node)
            {
                if (node?.nodeType == NodeType.Member && node.objectValue != null)
                {
                    if (node.memberName == null)
                    {
                        return Reduce(node.objectValue);
                    }
                    else
                    {
                        if (node.objectValue.nodeType == NodeType.New)
                        {
                            return Reduce(GetMemberFromNewNode(node.objectValue, node.memberName));
                        }
                    }
                }
                return node;
            }
            ExpressionNode GetMember(ExpressionNode node, string memberName, ExpressionNode_Member sourceMember)
            {
                if (node == null || string.IsNullOrEmpty(memberName)) return node;

                if (node.nodeType == NodeType.New)
                {
                    return GetMemberFromNewNode(node, memberName);
                }
                return ExpressionNode.Member(objectValue: node, memberName: memberName).Member_SetType(sourceMember.Member_GetType());
            }

            ExpressionNode GetMemberFromNewNode(ExpressionNode_New newNode, string memberName)
            {
                return newNode.constructorArgs?.FirstOrDefault(bind => bind.name == memberName)?.value ?? newNode.memberArgs?.FirstOrDefault(bind => bind.name == memberName)?.value;
            }
        }


    }
}
