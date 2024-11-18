using System;

using Vitorm.Entity.PropertyType;

namespace Vitorm.Entity
{
    public partial class EntityDescriptorWithAlias : IEntityDescriptor
    {
        public IEntityDescriptor originEntityDescriptor { get; protected set; }
        public EntityDescriptorWithAlias(IEntityDescriptor entityDescriptor, string tableName)
        {
            this.originEntityDescriptor = entityDescriptor;
            this.tableName = tableName;
        }

        public IPropertyObjectType propertyType => originEntityDescriptor.propertyType;
        public Type entityType => originEntityDescriptor?.entityType;
        public string tableName { get; protected set; }
        public string schema => originEntityDescriptor?.schema;

        /// <summary>
        /// primary key name
        /// </summary>
        public string keyName => originEntityDescriptor?.keyName;

        /// <summary>
        /// primary key
        /// </summary>
        public IPropertyDescriptor key => originEntityDescriptor?.key;

        /// <summary>
        /// not include primary key
        /// </summary>
        public IPropertyDescriptor[] propertiesWithoutKey => originEntityDescriptor?.propertiesWithoutKey;


        public IPropertyDescriptor[] properties => originEntityDescriptor?.properties;


    }
}
