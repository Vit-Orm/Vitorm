using System;
using System.Reflection;

namespace Vitorm.Entity.DataAnnotations
{
    public class ColumnDescriptor : IColumnDescriptor
    {
        public ColumnDescriptor(PropertyInfo propertyInfo, string name, bool isKey, bool isIdentity, string databaseType, bool isNullable, int? columnOrder = null, bool? isIndex = null)
        {
            this.propertyInfo = propertyInfo;
            type = propertyInfo.PropertyType;

            this.name = name;
            this.isKey = isKey;
            this.isIdentity = isIdentity;
            this.databaseType = databaseType;
            this.isNullable = isNullable;

            this.columnOrder = columnOrder;
            this.isIndex = isIndex;
        }

        PropertyInfo propertyInfo;
        public Type type { get; private set; }


        public string name { get; private set; }

        public bool isKey { get; private set; }

        /// <summary>
        /// whether column is Identity
        /// </summary>
        public bool isIdentity { get; private set; }
        /// <summary>
        /// whether column could be null
        /// </summary>
        public bool isNullable { get; private set; }

        public int? columnOrder { get; private set; }
        public bool? isIndex { get; private set; }

        /// <summary>
        /// database provider specific data type of the column the property is mapped to.  example:  varchar(1000)
        /// </summary>
        public string databaseType { get; private set; }


        public void SetValue(object entity, object value)
        {
            propertyInfo?.SetValue(entity, value);
        }
        public object GetValue(object entity)
        {
            return propertyInfo?.GetValue(entity, null);
        }
    }


}
