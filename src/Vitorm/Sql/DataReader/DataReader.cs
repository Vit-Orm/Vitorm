using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using Vit.Linq.ExpressionNodes;
using Vit.Linq.ExpressionNodes.ComponentModel;

using Vitorm.Sql.DataReader.EntityReader;
using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.DataReader
{
    public partial class DataReader : IDbDataReader
    {
        public SqlColumns sqlColumns { get; } = new();

        protected IEntityReader entityReader;


        protected Type entityType;


        public string BuildSelect(
            QueryTranslateArgument arg, Type entityType, ISqlTranslateService sqlTranslateService, ExpressionConvertService convertService,
            ResultSelector resultSelector, ExpressionNode selectedFields)
        {
            this.entityType = entityType;

            var config = new EntityReaderConfig { queryTranslateArgument = arg, convertService = convertService, sqlTranslateService = sqlTranslateService, sqlColumns = sqlColumns };

            entityReader = Activator.CreateInstance(arg.dbContext.entityReaderType) as IEntityReader;

            entityReader.Init(config, entityType, selectedFields);

            return sqlColumns.GetSqlColumns();
        }



        public virtual object ReadData(IDataReader reader)
        {
            return new Func<IDataReader, object>(ReadEntity<string>)
              .GetMethodInfo().GetGenericMethodDefinition().MakeGenericMethod(entityType)
              .Invoke(this, new object[] { reader });
        }


        object ReadEntity<Entity>(IDataReader reader)
        {
            var list = new List<Entity>();

            while (reader.Read())
            {
                var row = (Entity)entityReader.ReadEntity(reader);
                list.Add(row);
            }
            return list;
        }



    }
}
