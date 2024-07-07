using System;
using System.Data;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.Sql.DataReader.EntityConstructor
{
    public interface IEntityConstructor
    {
        void Init(EntityConstructorConfig config, Type entityType, ExpressionNode resultSelector);
        object ReadEntity(IDataReader reader);

    }
}
