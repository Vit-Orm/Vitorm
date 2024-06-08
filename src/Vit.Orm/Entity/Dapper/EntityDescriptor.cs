using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vit.Orm.Entity.Dapper
{
    public class EntityDescriptor : IEntityDescriptor
    {
        static ConcurrentDictionary<Type, EntityDescriptor> descMap = new();
        static EntityDescriptor New(Type entityType) => new EntityDescriptor(entityType);

        public static string GetTableName(Type entityType) => entityType?.GetCustomAttribute<global::Dapper.Contrib.Extensions.TableAttribute>()?.Name;
        public static EntityDescriptor GetEntityDescriptor(Type entityType)
        {
            if (GetTableName(entityType) == null) return null;

            return descMap.GetOrAdd(entityType, New);
        }

        public static EntityDescriptor GetEntityDescriptor<Entity>()
        {
            return GetEntityDescriptor(typeof(Entity));
        }

        EntityDescriptor(Type entityType)
        {
            tableName = GetTableName(entityType);

            var entityProperties = entityType?.GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? new PropertyInfo[0];

            var keyProperty = entityProperties.FirstOrDefault(p => p.GetCustomAttribute<global::Dapper.Contrib.Extensions.KeyAttribute>() != null);
            this.key = new ColumnDescriptor(keyProperty, true);

            var properties = entityProperties.Where(p => p.GetCustomAttribute<global::Dapper.Contrib.Extensions.KeyAttribute>() == null);
            this.columns = properties.Select(p => new ColumnDescriptor(p, false)).ToArray();

            allColumns = new List<IColumnDescriptor> { key }.Concat(columns).ToArray();
        }

        public string tableName { get; private set; }

        /// <summary>
        /// primary key name
        /// </summary>
        public string keyName => key?.name;

        /// <summary>
        /// primary key
        /// </summary>
        public IColumnDescriptor key { get; private set; }

        /// <summary>
        /// not include primary key
        /// </summary>
        public IColumnDescriptor[] columns { get; private set; }


        public IColumnDescriptor[] allColumns { get; private set; }


    }
}
