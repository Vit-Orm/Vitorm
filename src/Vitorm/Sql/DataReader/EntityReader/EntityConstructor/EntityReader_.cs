using System;
using System.Collections.Generic;
using System.Data;

using Vitorm.Entity;

namespace Vitorm.Sql.DataReader.EntityReader.EntityConstructor
{

    public class EntityReader_ : IValueReader
    {
        protected Type entityType;
        protected List<(IColumnDescriptor columnDescriptor, SqlFieldReader sqlFieldReader)> properties = new();

        public EntityReader_(EntityReaderConfig config, string tableName, Type entityType, IEntityDescriptor entityDescriptor)
        {
            this.entityType = entityDescriptor.entityType;

            foreach (var column in entityDescriptor.allColumns)
            {
                var sqlColumnIndex = config.sqlColumns.AddSqlColumnAndGetIndex(config.sqlTranslateService, tableName, columnDescriptor: column);

                var sqlFieldReader = new SqlFieldReader(column.type, sqlColumnIndex);
                properties.Add((column, sqlFieldReader));
            }
        }

        public object Read(IDataReader reader)
        {
            var entity = Activator.CreateInstance(entityType);

            foreach (var (column, sqlFieldReader) in properties)
            {
                var value = sqlFieldReader.Read(reader);
                if (value == null)
                {
                    if (column.isKey)
                        return null;
                }
                else
                {
                    column.SetValue(entity, value);
                }
            }
            return entity;
        }
    }


}
