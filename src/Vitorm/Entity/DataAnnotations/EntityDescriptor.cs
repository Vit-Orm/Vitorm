using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Vitorm.Entity.Dapper
{
    public partial class EntityDescriptor : IEntityDescriptor
    {

        static ConcurrentDictionary<Type, EntityDescriptor> descMap = new();


        public static IEntityDescriptor GetEntityDescriptor(Type entityType)
        {
            if (descMap.TryGetValue(entityType, out var entityDescriptor)) return entityDescriptor;

            entityDescriptor = LoadFromType(entityType);
            if (entityDescriptor != null) descMap[entityType] = entityDescriptor;

            return entityDescriptor;
        }

        public static IEntityDescriptor GetEntityDescriptor<Entity>()
        {
            return GetEntityDescriptor(typeof(Entity));
        }

        public EntityDescriptor(Type entityType, IColumnDescriptor[] allColumns, string tableName, string schema = null)
        {
            this.entityType = entityType;
            this.tableName = tableName;
            this.schema = schema;

            this.allColumns = allColumns;
            this.key = allColumns.FirstOrDefault(m => m.isKey);
            this.columns = allColumns.Where(m => !m.isKey).ToArray();
        }


        public Type entityType { get; protected set; }
        public string tableName { get; protected set; }
        public string schema { get; protected set; }

        /// <summary>
        /// primary key name
        /// </summary>
        public string keyName => key?.name;

        /// <summary>
        /// primary key
        /// </summary>
        public IColumnDescriptor key { get; protected set; }

        /// <summary>
        /// not include primary key
        /// </summary>
        public IColumnDescriptor[] columns { get; protected set; }


        public IColumnDescriptor[] allColumns { get; protected set; }


    }
}
