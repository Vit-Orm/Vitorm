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

        //EntityPropertyReader keyPropertyReader;
        List<EntityPropertyReader> proppertyReaders = new();

        public ModelReader(EntityReader entityReader, ISqlTranslateService sqlTranslator, string tableName, string argUniqueKey, string argName, Type argType, IEntityDescriptor entityDescriptor)
        {
            this.argUniqueKey = argUniqueKey;
            this.argName = argName;
            this.argType = argType;

            // #1 key
            {
                //var column = entityDescriptor.key;
                //var sqlFieldName = sqlTranslator.GetSqlField(tableName, column.name);
                //keyPropertyReader = new EntityPropertyReader(entityReader, column, sqlFieldName);
            }
            // #2 properties
            {
                foreach (var column in entityDescriptor.allColumns)
                {
                    var sqlFieldName = sqlTranslator.GetSqlField(tableName, column.name);
                    proppertyReaders.Add(new EntityPropertyReader(entityReader, column, sqlFieldName));
                }
            }
        }
        public object Read(IDataReader reader)
        {
            // #1 key           
            //var value = keyPropertyReader.Read(reader);
            //if (value == null) return null;

            var entity = Activator.CreateInstance(argType);
            //keyPropertyReader.column.SetValue(entity, value);

            //#2 properties
            foreach (var perpertyReader in proppertyReaders)
            {
                if (!perpertyReader.Read(reader, entity))
                    return null;
            }
            return entity;
        }
    }



}
