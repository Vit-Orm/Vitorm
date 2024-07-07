using System;
using System.Collections.Generic;
using System.Data;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader.EntityReader.CompiledLambda
{

    class ModelReader : IArgReader
    {
        public string argName { get; set; }
        public string argUniqueKey { get; set; }
        public Type entityType { get; }

        List<(IColumnDescriptor columnDescriptor, SqlFieldReader sqlFieldReader)> properties = new();


        public ModelReader(SqlColumns sqlColumns, ISqlTranslateService sqlTranslateService, string tableName, string argUniqueKey, string argName,  IEntityDescriptor entityDescriptor)
        {
            this.argUniqueKey = argUniqueKey;
            this.argName = argName;

            this.entityType = entityDescriptor.entityType;

            foreach (var column in entityDescriptor.allColumns)
            {
                var sqlColumnIndex = sqlColumns.AddSqlColumnAndGetIndex(sqlTranslateService, tableName, columnDescriptor: column);

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
