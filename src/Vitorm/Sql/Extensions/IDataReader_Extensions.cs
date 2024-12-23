using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Vitorm
{
    public static class IDataReader_Extensions
    {
        public static IEnumerable<Entity> ReadEntity<Entity>(this IDataReader reader) where Entity : class, new()
        {
            object[] values = null;
            PropertyInfo[] properties = null;
            Type[] underlyingTypes = null;
            while (reader.Read())
            {
                if (values == null)
                {
                    var names = Enumerable.Range(0, reader.FieldCount).Select(i => reader.GetName(i)).ToArray();
                    values = new object[names.Length];
                    properties = new PropertyInfo[names.Length];
                    underlyingTypes = new Type[names.Length];

                    var propertyInfos =
                        typeof(Entity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(property =>
                            (property: property,
                            name: property.GetCustomAttribute<ColumnAttribute>(inherit: true)?.Name ?? property.Name
                            )
                        ).ToList();

                    for (var i = 0; i < names.Length; i++)
                    {
                        var name = names[i];
                        var property = propertyInfos.FirstOrDefault(p => p.name.Equals(name, StringComparison.OrdinalIgnoreCase)).property;
                        if (property != null)
                        {
                            properties[i] = property;
                            underlyingTypes[i] = TypeUtil.GetUnderlyingType(property.PropertyType);
                        }
                    }
                }

                reader.GetValues(values);

                var entity = new Entity();
                for (int i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    var property = properties[i];
                    if (property == null) continue;

                    var convertedValue = TypeUtil.ConvertToUnderlyingType(value, underlyingTypes[i]);
                    property.SetValue(entity, convertedValue);
                }
                yield return entity;
            }

        }
    }
}
