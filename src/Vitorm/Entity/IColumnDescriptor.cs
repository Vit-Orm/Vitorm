using System;

namespace Vitorm.Entity
{
    public interface IColumnDescriptor
    {
        Type type { get; }
        /// <summary>
        /// property name in Entity Type
        /// </summary>
        string propertyName { get; }
        /// <summary>
        /// column name in database
        /// </summary>
        string columnName { get; }
        bool isKey { get; }

        /// <summary>
        /// whether column is Identity
        /// </summary>
        bool isIdentity { get; }
        /// <summary>
        /// whether column could be null
        /// </summary>
        bool isNullable { get; }

        int? columnOrder { get; }
        bool? isIndex { get; }

        /// <summary>
        /// database provider specific data type of the column the property is mapped to.  example:  varchar(1000)
        /// </summary>
        string columnDbType { get; }
        /// <summary>
        /// database column length , for example:  varchar(100)
        /// </summary>
        int? columnLength { get; }


        void SetValue(object entity, object value);
        object GetValue(object entity);


    }
}
