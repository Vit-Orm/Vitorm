using System;
using System.Collections.Generic;
using System.Data;

using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate; 

namespace Vitorm.Sql.DataReader.EntityConstructor.CompiledLambda
{
    class ModelReader : IArgReader
    {
        public string argName { get; set; }
        public string argUniqueKey { get; set; }
        public Type argType { get; set; }

        readonly List<EntityPropertyReader> propertyReaders = new();

        public ModelReader(SqlColumns sqlColumns, ISqlTranslateService sqlTranslator, string tableName, string argUniqueKey, string argName, Type argType, IEntityDescriptor entityDescriptor)
        {
            this.argUniqueKey = argUniqueKey;
            this.argName = argName;
            this.argType = argType;

            // properties
            {
                foreach (var column in entityDescriptor.allColumns)
                {
                    var sqlColumnIndex = sqlColumns.AddSqlColumnAndGetIndex(sqlTranslator, tableName, columnDescriptor: column);
                    propertyReaders.Add(new EntityPropertyReader(column, sqlColumnIndex));
                }
            }
        }
        public object Read(IDataReader reader)
        {
            var entity = Activator.CreateInstance(argType);

            // properties
            foreach (var perpertyReader in propertyReaders)
            {
                if (!perpertyReader.Read(reader, entity))
                    return null;
            }
            return entity;
        }
    }



}
