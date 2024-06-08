using System;
using System.Reflection;

namespace Vit.Orm.Entity
{
    public class ColumnDescriptor : IColumnDescriptor
    {
        public ColumnDescriptor(PropertyInfo propertyInfo, bool isPrimaryKey)
        {
            this.propertyInfo = propertyInfo;
            this.isPrimaryKey = isPrimaryKey;
        }

        PropertyInfo propertyInfo;
        public bool isPrimaryKey { get; private set; }
        public string name => propertyInfo?.Name;

        public Type type => propertyInfo?.PropertyType;

        public void Set(object entity, object value)
        {
            propertyInfo?.SetValue(entity, value);
        }
        public object Get(object entity)
        {
            return propertyInfo?.GetValue(entity, null);
        }
    }


}
