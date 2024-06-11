using System;

namespace Vitorm.Entity
{
    public interface IEntityDescriptor
    {
        Type entityType { get; }

        string tableName { get; }
        string keyName { get; }
        /// <summary>
        /// primary key
        /// </summary>
        public IColumnDescriptor key { get; }

        /// <summary>
        /// columns except primary key
        /// </summary>
        public IColumnDescriptor[] columns { get; }

        /// <summary>
        /// columns including primary key
        /// </summary>
        public IColumnDescriptor[] allColumns { get; }
    }
}
