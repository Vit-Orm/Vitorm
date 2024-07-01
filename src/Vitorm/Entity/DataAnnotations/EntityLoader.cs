using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using Vitorm.Entity.Loader;

namespace Vitorm.Entity.DataAnnotations
{
    public class EntityLoader : IEntityLoader
    {
        public void CleanCache()
        {
        }

        public IEntityDescriptor LoadDescriptor(Type entityType)
        {
            return LoadFromType(entityType);
        }

        public static bool GetTableName(Type entityType, out string tableName, out string schema)
        {
            var attribute = entityType?.GetCustomAttribute<global::System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
            tableName = attribute?.Name;
            schema = attribute?.Schema;
            return attribute != null;
        }

        public static EntityDescriptor LoadFromType(Type entityType)
        {
            if (!GetTableName(entityType, out var tableName, out var schema)) return null;

            IColumnDescriptor[] allColumns = entityType?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo =>
             {
                 if (propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null) return null;

                 // #1 isKey
                 bool isKey = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null;

                 // #2 column name and type
                 string name; string databaseType; int? columnOrder;
                 var columnAttr = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
                 name = columnAttr?.Name ?? propertyInfo.Name;
                 databaseType = columnAttr?.TypeName;
                 columnOrder = columnAttr?.Order;

                 // #3 isIdentity
                 var isIdentity = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>()?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;

                 // #4 isNullable
                 bool isNullable;
                 if (propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>() != null) isNullable = false;
                 else
                 {
                     var type = propertyInfo.PropertyType;
                     if (type == typeof(string)) isNullable = true;
                     else
                     {
                         isNullable = (type.IsGenericType && typeof(Nullable<>) == type.GetGenericTypeDefinition());
                     }
                 }

                 return new ColumnDescriptor(propertyInfo, name: name, isKey: isKey, isIdentity: isIdentity, databaseType: databaseType, isNullable: isNullable, columnOrder: columnOrder);
             }).Where(column => column != null).ToArray();

            return new EntityDescriptor(entityType, allColumns, tableName, schema);
        }


    }
}
