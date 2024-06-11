using System;
using System.Collections.Generic;
using System.Data;

using Vit.Linq.ExpressionTree.ComponentModel;
using Vitorm.Entity;
using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader
{
    partial class ModelReader : IArgReader
    {
        public string argName { get; set; }
        public string argUniqueKey { get; set; }
        public Type argType { get; set; }

        List<EntityPropertyReader> proppertyReaders = new();

        public ModelReader(EntityReader entityReader, ISqlTranslateService sqlTranslator, string tableName, string argUniqueKey, string argName, Type argType, IEntityDescriptor entityDescriptor)
        {
            this.argUniqueKey = argUniqueKey;
            this.argName = argName;
            this.argType = argType;

            // ##1 key
            string sqlFieldName = sqlTranslator.GetSqlField(tableName, entityDescriptor.keyName);
            proppertyReaders.Add(new EntityPropertyReader(entityReader, entityDescriptor.key, true, sqlFieldName));

            // ##2 properties
            foreach (var column in entityDescriptor.columns)
            {
                sqlFieldName = sqlTranslator.GetSqlField(tableName, column.name);
                proppertyReaders.Add(new EntityPropertyReader(entityReader, column, false, sqlFieldName));
            }
        }
        public object Read(IDataReader reader)
        {
            var entity = Activator.CreateInstance(argType);
            foreach (var perpertyReader in proppertyReaders)
            {
                if (!perpertyReader.Read(reader, entity))
                    return null;
            }
            return entity;
        }
    }



}
