using System;
using System.Data;

namespace Vitorm.Sql.DataReader.EntityReader.EntityConstructor
{
    public class ConstantValueReader : IValueReader
    {
        protected object value;
        public ConstantValueReader(object value, Type valueType = null)
        {
            if (valueType != null) value = TypeUtil.ConvertToType(value, valueType);
            this.value = value;
        }

        public object Read(IDataReader reader)
        {
            return value;
        }
    }

}
