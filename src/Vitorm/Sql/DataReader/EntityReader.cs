using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using Vit.Linq.ExpressionTree;
using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Sql.DataReader.EntityConstructor;
using Vitorm.Sql.SqlTranslate;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.DataReader
{
    public partial class EntityReader : IDbDataReader
    {
        public SqlColumns sqlColumns { get; } = new();

        protected IEntityConstructor entityConstructor = new EntityConstructor.CompiledLambda.EntityConstructor();


        protected Type entityType;


        public string BuildSelect(
            QueryTranslateArgument arg, Type entityType, ISqlTranslateService sqlTranslateService, ExpressionConvertService convertService,
            ResultSelector resultSelector, ExpressionNode selectedFields)
        {
            this.entityType = entityType;

            var config = new EntityConstructorConfig { arg = arg, convertService = convertService, sqlTranslateService = sqlTranslateService, sqlColumns = sqlColumns };

            entityConstructor.Init(config, entityType, selectedFields);

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
                var row = (Entity)entityConstructor.ReadEntity(reader);
                list.Add(row);
            }
            return list;
        }



    }
}
