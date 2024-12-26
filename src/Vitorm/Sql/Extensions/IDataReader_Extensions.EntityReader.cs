using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Vitorm
{
    public static partial class IDataReader_Extensions
    {

        class EntityReader
        {
            Type entityType;
            (PropertyInfo property, int columnIndex, Type underlyingType)[] columns;

            public EntityReader(Type entityType, List<string> columnNames) : this(entityType, columnNames.Select((m, i) => (m, i)).ToList())
            { }

            public EntityReader(Type entityType, List<(string columName, int index)> columnIndexes)
            {
                this.entityType = entityType;

                var properties =
                    entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => (
                        property: property,
                        name: property.GetCustomAttribute<ColumnAttribute>(inherit: true)?.Name ?? property.Name
                        )
                    )
                    .ToList();

                columns = properties.Select(m =>
                {
                    var property = m.property;
                    var columnIndex = columnIndexes.FirstOrDefault(col => m.name.Equals(col.columName, StringComparison.OrdinalIgnoreCase));
                    if (columnIndex.columName == null) return default;

                    var index = columnIndex.index;
                    var underlyingType = TypeUtil.GetUnderlyingType(property.PropertyType);
                    return (property, index, underlyingType);
                })
                .Where(m => m != default)
                .ToArray();
            }


            public object Read(IDataReader reader)
            {
                var entity = Activator.CreateInstance(entityType);
                foreach (var column in columns)
                {
                    var value = reader.GetValue(column.columnIndex);
                    var convertedValue = TypeUtil.ConvertToUnderlyingType(value, column.underlyingType);
                    column.property.SetValue(entity, convertedValue);
                }
                return entity;
            }

        }

    }
}
