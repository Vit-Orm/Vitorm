using System;
using System.Linq;

using Vitorm.Entity.PropertyType;

namespace Vitorm.Entity.Loader.DataAnnotations
{
    public partial class EntityDescriptor : IEntityDescriptor
    {
        public EntityDescriptor(IPropertyObjectType propertyType, string tableName, string schema = null)
        {
            this.propertyType = propertyType;
            this.tableName = tableName;
            this.schema = schema;

            var allProperties = propertyType.properties;

            this.key = allProperties.FirstOrDefault(m => m.isKey);
            this.propertiesWithoutKey = allProperties.Where(m => !m.isKey).OrderBy(col => col.columnOrder ?? int.MaxValue).ToArray();
            this.properties = allProperties.OrderBy(col => col.columnOrder ?? int.MaxValue).ToArray();
        }

        public IPropertyObjectType propertyType { get; protected set; }

        public Type entityType => propertyType?.type;
        public string tableName { get; protected set; }
        public string schema { get; protected set; }

        /// <summary>
        /// primary key name
        /// </summary>
        public string keyName => key?.columnName;

        /// <summary>
        /// primary key
        /// </summary>
        public IPropertyDescriptor key { get; protected set; }

        /// <summary>
        /// not include primary key
        /// </summary>
        public IPropertyDescriptor[] propertiesWithoutKey { get; protected set; }


        public IPropertyDescriptor[] properties { get; protected set; }


    }
}
