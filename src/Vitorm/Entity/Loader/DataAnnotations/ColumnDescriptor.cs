using System;
using System.Reflection;

namespace Vitorm.Entity.Loader.DataAnnotations
{
    public class ColumnDescriptor : IColumnDescriptor
    {
        public ColumnDescriptor(
            PropertyInfo propertyInfo, string columnName,
            bool isKey, bool isIdentity, bool isNullable,
            string columnDbType, int? columnLength = null,
            int? columnOrder = null, bool? isIndex = null
            )
        {
            this.propertyInfo = propertyInfo;
            type = propertyInfo.PropertyType;

            this.columnName = columnName;

            this.isKey = isKey;
            this.isIdentity = isIdentity;
            this.isNullable = isNullable;

            this.columnDbType = columnDbType;
            this.columnLength = columnLength;

            this.columnOrder = columnOrder;
            this.isIndex = isIndex;
        }

        readonly PropertyInfo propertyInfo;
        public Type type { get; private set; }

        /// <summary>
        /// property name in Entity Type
        /// </summary>
        public string propertyName => propertyInfo?.Name;
        /// <summary>
        /// column name in database
        /// </summary>
        public string columnName { get; private set; }

        public bool isKey { get; set; }

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
        public string columnDbType { get; private set; }
        /// <summary>
        /// database column length , for example:  varchar(100)
        /// </summary>
        public int? columnLength { get; private set; }


        public void SetValue(object entity, object value)
        {
            if (propertyInfo?.CanWrite == true)
                propertyInfo.SetValue(entity, value);
        }
        public object GetValue(object entity)
        {
            if (propertyInfo?.CanRead == true)
                return propertyInfo.GetValue(entity, default);
            return default;
        }
    }


}
