using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vitorm.Entity
{
    public interface IColumnDescriptor
    {
        Type type { get; }
        string name { get; }
        bool isKey { get; }

        /// <summary>
        /// Specifies how the database generates values for a property.   None / Identity / Computed
        /// </summary>
        DatabaseGeneratedOption? databaseGenerated { get; }
        /// <summary>
        /// database provider specific data type of the column the property is mapped to.  example:  varchar(1000)
        /// </summary>
        string databaseType { get; }
        /// <summary>
        /// whether column could be null
        /// </summary>
        bool nullable { get; }

        void SetValue(object entity, object value);
        object GetValue(object entity);
    }
}
