#region << Version-v3 >>
/*
 * ========================================================================
 * Version： v3
 * Time   ： 2024-07-28
 * Author ： lith
 * Email  ： LithWang@outlook.com
 * Remarks： 
 * ========================================================================
*/
#endregion

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Vit.Db.Module.Schema;
using Vit.DynamicCompile.EntityGenerate;

namespace Vitorm.EntityGenerate
{
    public class EntityHelp
    {
        public static Type GenerateEntityBySchema(TableSchema tableSchema, string entityNamespace, string typeName = null, string moduleName = "Main", string assemblyName = "DynamicEntity")
        {
            typeName ??= tableSchema.table_name;

            // #1
            var typeDescriptor = new TypeDescriptor
            {
                assemblyName = assemblyName,
                moduleName = moduleName,
                typeName = entityNamespace + "." + typeName,
            };

            // #2 Type Attribute
            if (tableSchema.schema_name == null)
            {
                typeDescriptor.AddAttribute<TableAttribute>(constructorArgs: new object[] { tableSchema.table_name });
            }
            else
            {
                typeDescriptor.AddAttribute<TableAttribute>(constructorArgs: new object[] { tableSchema.table_name }, propertyValues: new[] { ("Schema", (object)tableSchema.schema_name) });
            }


            // #3 properties
            {
                tableSchema.columns.ForEach(column =>
                {
                    var property = new PropertyDescriptor(column.column_name, column.column_clr_type);

                    //property.AddAttribute<RequiredAttribute>();

                    if (column.primary_key == 1) property.AddAttribute<KeyAttribute>();

                    if (column.autoincrement == 1) property.AddAttribute<DatabaseGeneratedAttribute>(constructorArgs: new object[] { DatabaseGeneratedOption.Identity });

                    if (!string.IsNullOrEmpty(column.column_type))
                        property.AddAttribute<ColumnAttribute>(constructorArgs: new object[] { column.column_name }, propertyValues: new (string, object)[] { ("TypeName", column.column_type) });

                    typeDescriptor.AddProperty(property);
                });
            }

            return EntityGenerator.CreateType(typeDescriptor);
        }
    }
}
