using System;
using System.Data;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.Sql.DataReader.EntityReader.EntityConstructor
{
    public partial class EntityReader : IEntityReader
    {
        IValueReader valueReader;

        public void Init(EntityReaderConfig config, Type entityType, ExpressionNode resultSelector)
        {
            valueReader = BuildValueReader(config, resultSelector, entityType);
        }


        public IValueReader BuildValueReader(EntityReaderConfig config, ExpressionNode resultSelector, Type entityType = null)
        {
            switch (resultSelector.nodeType)
            {
                case NodeType.New:
                    {
                        ExpressionNode_New newNode = resultSelector;

                        return new ModelReader(config, this, newNode);
                    }
                case NodeType.Member:
                    {
                        ExpressionNode_Member member = resultSelector;

                        var argType = entityType ?? member.Member_GetType();

                        if (argType == null || TypeUtil.IsValueType(argType))
                        {
                            // Value
                            return new ValueReader(config, argType, resultSelector);
                        }
                        else
                        {
                            // Entity
                            var entityDescriptor = config.queryTranslateArgument.dbContext.GetEntityDescriptor(argType);

                            // 1: {"nodeType":"Member","parameterName":"a0","memberName":"id"}
                            // 2: {"nodeType":"Member","objectValue":{"parameterName":"a0","nodeType":"Member"},"memberName":"id"}
                            var tableName = member.objectValue?.parameterName ?? member.parameterName;

                            return new EntityReader_(config, tableName, argType, entityDescriptor);
                        }
                    }
                default:
                    {
                        // Value
                        return new ValueReader(config, entityType, resultSelector);
                    }
            }

            //throw new NotImplementedException("the select type was not implemented yet.");
        }


        public object ReadEntity(IDataReader reader)
        {
            return valueReader.Read(reader);
        }


    }
}
