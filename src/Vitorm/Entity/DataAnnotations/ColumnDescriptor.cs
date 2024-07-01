using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Vitorm.Entity
{
    public class ColumnDescriptor : IColumnDescriptor
    {
        public ColumnDescriptor(PropertyInfo propertyInfo, string name, bool isKey, DatabaseGeneratedOption? databaseGenerated, string databaseType, bool nullable)
        {
            this.propertyInfo = propertyInfo;
            type = propertyInfo.PropertyType;

            this.name = name;
            this.isKey = isKey;
            this.databaseGenerated = databaseGenerated;
            this.databaseType = databaseType;
            this.nullable = nullable;
        }

        PropertyInfo propertyInfo;
        public Type type { get; private set; }


        public string name { get; private set; }

        public bool isKey { get; private set; }
        /// <summary>
        /// Specifies how the database generates values for a property.   None / Identity / Computed
        /// </summary>
        public DatabaseGeneratedOption? databaseGenerated { get; private set; }
        /// <summary>
        /// database provider specific data type of the column the property is mapped to.  example:  varchar(1000)
        /// </summary>
        public string databaseType { get; private set; }
        /// <summary>
        /// whether column could be null
        /// </summary>
        public bool nullable { get; private set; }




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
