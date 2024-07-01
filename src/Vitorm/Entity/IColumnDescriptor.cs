using System;

namespace Vitorm.Entity
{
    public interface IColumnDescriptor
    {
        Type type { get; }
        string name { get; }
        bool isKey { get; }

        /// <summary>
        /// whether column is Identity
        /// </summary>
        bool isIdentity { get; }
        /// <summary>
        /// whether column could be null
        /// </summary>
        bool isNullable { get; }

        /// <summary>
        /// database provider specific data type of the column the property is mapped to.  example:  varchar(1000)
        /// </summary>
        string databaseType { get; }

    

        void SetValue(object entity, object value);
        object GetValue(object entity);

        //bool? isIndex { get; }
        //int? columnOrder { get; }
    }
}
