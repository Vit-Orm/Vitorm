using System;

namespace Vitorm.Entity
{
    public interface IEntityDescriptor
    {
        Type entityType { get; }
        string schema { get; }
        string tableName { get; }
        string keyName { get; }
        /// <summary>
        /// primary key
        /// </summary>
        public IPropertyDescriptor key { get; }

        /// <summary>
        /// columns except primary key
        /// </summary>
        public IPropertyDescriptor[] properties { get; }

        /// <summary>
        /// columns including primary key
        /// </summary>
        public IPropertyDescriptor[] allProperties { get; }
    }
}
