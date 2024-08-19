using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Vitorm.Entity.Loader.DataAnnotations
{
    public class EntityLoader : IEntityLoader
    {
        /// <summary>
        /// if strictMode is false: will get typeName as tableName if not specify TableAttribute, and will set property named Id (or tableName + "Id") as key
        /// </summary>
        public static bool strictMode { get; set; } = false;

        public void CleanCache()
        {
        }

        /// <summary>
        /// if strictMode is false: will get typeName as tableName if not specify TableAttribute, and will set property named Id (or tableName + "Id") as key
        /// </summary>
        public bool? StrictMode { get; set; }

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptor(Type entityType) => LoadDescriptorWithoutCache(entityType);

        public (bool success, IEntityDescriptor entityDescriptor) LoadDescriptorWithoutCache(Type entityType) => LoadFromType(entityType, strictMode: StrictMode ?? strictMode);


        public static (string tableName, string schema) GetTableName(Type entityType)
        {
            var attribute = entityType?.GetCustomAttribute<global::System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
            var tableName = attribute?.Name;
            var schema = attribute?.Schema;
            return (tableName, schema);
        }


        /// <summary>
        /// if strictMode is false: will get typeName as tableName if not specify TableAttribute, and will set property named Id (or tableName + "Id") as key
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="strictMode"></param>
        /// <returns></returns>
        public static (bool success, EntityDescriptor entityDescriptor) LoadFromType(Type entityType, bool strictMode = false)
        {
            (string tableName, string schema) = GetTableName(entityType);

            if (string.IsNullOrEmpty(tableName))
            {
                if (strictMode) return (true, null);
                tableName = entityType.Name;
            }

            var columns = LoadColumns(entityType);

            // key
            if (!strictMode && !columns.Any(col => col.isKey))
            {
                var keyNames = new[] { "id", tableName + "id" };
                var keyColumn = columns.FirstOrDefault(col => keyNames.Contains(col.columnName, StringComparer.OrdinalIgnoreCase));
                if (keyColumn != null) keyColumn.isKey = true;
            }

            IColumnDescriptor[] allColumns = columns.Select(m => (IColumnDescriptor)m).ToArray();

            return (true, new EntityDescriptor(entityType, allColumns, tableName, schema));
        }


        public static List<ColumnDescriptor> LoadColumns(Type entityType)
        {
            return entityType?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo =>
                {
                    if (propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null) return null;

                    // #1 isKey
                    bool isKey = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null;

                    // #2 column name and type
                    string columnName; string databaseType; int? columnOrder;
                    var columnAttr = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
                    columnName = columnAttr?.Name ?? propertyInfo.Name;
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

                    return new ColumnDescriptor(propertyInfo, columnName: columnName, isKey: isKey, isIdentity: isIdentity, databaseType: databaseType, isNullable: isNullable, columnOrder: columnOrder);
                }).Where(column => column != null).ToList();
        }



    }
}
