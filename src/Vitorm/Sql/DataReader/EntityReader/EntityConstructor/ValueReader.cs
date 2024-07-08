using System;

using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.Sql.DataReader.EntityReader.EntityConstructor
{
    public class ValueReader : SqlFieldReader, IValueReader
    {
        public ValueReader(EntityReaderConfig config, Type valueType, ExpressionNode valueNode)
            : base(valueType, config.sqlColumns.AddSqlColumnAndGetIndex(config, valueNode, valueType))
        {
        }
    }

}
