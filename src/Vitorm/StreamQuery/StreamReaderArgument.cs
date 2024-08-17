using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionNodes.ComponentModel;

namespace Vitorm.StreamQuery
{
    public class StreamReaderArgument
    {
        public StreamReaderArgument(StreamReaderArgument arg) : this(arg.aliasConfig)
        {
        }
        public StreamReaderArgument(AliasConfig aliasConfig)
        {
            this.aliasConfig = aliasConfig;
        }

        public class AliasConfig
        {
            int aliasNameCount = 0;
            public string NewAliasName()
            {
                return "t" + (aliasNameCount++);
            }
        }

        protected AliasConfig aliasConfig;

        public string NewAliasName() => aliasConfig.NewAliasName();

        Dictionary<string, ExpressionNode> parameterMap { get; set; }

        public virtual ExpressionNode GetParameter(ExpressionNode_Member member)
        {
            if (member.nodeType == NodeType.Member && !string.IsNullOrWhiteSpace(member.parameterName))
            {
                if (parameterMap?.TryGetValue(member.parameterName, out var parameterValue) == true)
                {
                    if (string.IsNullOrWhiteSpace(member.memberName))
                    {
                        return parameterValue;
                    }
                    else
                    {
                        return ExpressionNode.Member(objectValue: parameterValue, memberName: member.memberName).Member_SetType(member.Member_GetType());
                    }
                }
            }
            return null;
        }


        public StreamReaderArgument SetParameter(string parameterName, ExpressionNode parameterValue)
        {
            parameterMap ??= new();
            parameterMap[parameterName] = parameterValue;
            return this;
        }


        public StreamReaderArgument WithParameter(string parameterName, ExpressionNode parameterValue)
        {
            var arg = new StreamReaderArgument(aliasConfig);

            arg.parameterMap = parameterMap?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();
            arg.parameterMap[parameterName] = parameterValue;
            return arg;
        }

        #region SupportNoChildParameter
        public StreamReaderArgument WithParameter(string parameterName, ExpressionNode parameterValue, ExpressionNode noChildParameterValue)
        {
            var arg = new Argument_SupportNoChildParameter(this) { noChildParameterName = parameterName, noChildParameterValue = noChildParameterValue };

            arg.parameterMap = parameterMap?.ToDictionary(kv => kv.Key, kv => kv.Value) ?? new();
            arg.parameterMap[parameterName] = parameterValue;
            return arg;
        }

        class Argument_SupportNoChildParameter : StreamReaderArgument
        {
            public Argument_SupportNoChildParameter(StreamReaderArgument arg) : base(arg)
            {
            }

            public string noChildParameterName;
            public ExpressionNode noChildParameterValue;
            public override ExpressionNode GetParameter(ExpressionNode_Member member)
            {
                if (member.nodeType == NodeType.Member && member.parameterName == noChildParameterName && member.memberName == null)
                {
                    return noChildParameterValue;
                }
                return base.GetParameter(member);
            }

        }
        #endregion


        public ExpressionNode DeepClone(ExpressionNode node)
        {
            Func<ExpressionNode_Member, ExpressionNode> GetParameter = this.GetParameter;

            return StreamReader.DeepClone(node, GetParameter);
        }

    }
}
