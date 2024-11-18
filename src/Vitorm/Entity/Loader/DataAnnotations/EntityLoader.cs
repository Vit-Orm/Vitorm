using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

using Vitorm.Entity.PropertyType;

using ValueType = Vitorm.Entity.PropertyType.PropertyValueType;

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
            var attribute = entityType?.GetCustomAttribute<global::System.ComponentModel.DataAnnotations.Schema.TableAttribute>(inherit: true);
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

            var propertyType = ConvertToObjectType(entityType, new());

            // key
            if (!strictMode && !propertyType.properties.Any(col => col.isKey))
            {
                var keyNames = new[] { "id", tableName + "id" };
                var keyColumn = (PropertyDescriptor)propertyType.properties.FirstOrDefault(col => keyNames.Contains(col.columnName, StringComparer.OrdinalIgnoreCase));
                if (keyColumn != null) keyColumn.isKey = true;
            }

            return (true, new EntityDescriptor(propertyType, tableName, schema));
        }


        public static IPropertyType ConvertToPropertyType(Type propertyClrType, Dictionary<Type, IPropertyType> typeCache)
        {
            // value
            if (TypeUtil.IsValueType(propertyClrType))
            {
                return new ValueType(propertyClrType);
            }

            // try get for cache
            {
                if (typeCache?.TryGetValue(propertyClrType, out IPropertyType propertyType) == true) return propertyType;
            }


            // array
            var arrayElementType = TypeUtil.GetElementTypeFromArray(propertyClrType);
            if (arrayElementType != null)
            {
                var propertyType = new PropertyArrayType(propertyClrType);
                typeCache[propertyClrType] = propertyType;

                propertyType.elementPropertyType = ConvertToPropertyType(arrayElementType, typeCache);
                return propertyType;
            }

            // object
            return ConvertToObjectType(propertyClrType, typeCache);

        }


        public static PropertyObjectType ConvertToObjectType(Type propertyClrType, Dictionary<Type, IPropertyType> typeCache)
        {
            var propertyType = new PropertyObjectType(propertyClrType);
            typeCache[propertyClrType] = propertyType;

            var propertyDescriptors = propertyClrType?.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(propertyInfo =>
                {
                    if (propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>(inherit: true) != null) return null;

                    // #1 isKey
                    bool isKey = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>(inherit: true) != null;

                    // #2 column name and type
                    string columnName; string columnDbType; int? columnLength; int? columnOrder;
                    var columnAttr = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>(inherit: true);
                    columnName = columnAttr?.Name ?? propertyInfo.Name;
                    columnDbType = columnAttr?.TypeName;
                    columnOrder = columnAttr?.Order;
                    columnLength = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.MaxLengthAttribute>(inherit: true)?.Length;

                    // #3 isIdentity
                    var isIdentity = propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute>(inherit: true)?.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;

                    // #4 isNullable
                    bool isNullable;
                    if (propertyInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.RequiredAttribute>(inherit: true) != null) isNullable = false;
                    else
                    {
                        var type = propertyInfo.PropertyType;
                        if (type == typeof(string)) isNullable = true;
                        else
                        {
                            isNullable = TypeUtil.IsNullable(type);
                        }
                    }

                    return new PropertyDescriptor(
                        propertyInfo, propertyType: ConvertToPropertyType(propertyInfo.PropertyType, typeCache),
                        columnName: columnName,
                        isKey: isKey, isIdentity: isIdentity, isNullable: isNullable,
                        columnDbType: columnDbType, columnLength: columnLength,
                        columnOrder: columnOrder
                        );
                }).Where(column => column != null);

            propertyType.properties = propertyDescriptors.Select(m => (IPropertyDescriptor)m).ToArray();
            return propertyType;
        }



    }
}
