using System;
using System.Linq;
using Vit.Linq.ExpressionTree.ComponentModel;
using Vit.Orm.DataReader;
using Vit.Orm.Sql.DataReader;
using Vit.Linq.ExpressionTree.CollectionsQuery;
using Vit.Extensions.Linq_Extensions;


namespace Vit.Orm.Sql.Translator
{
    public class QueryTranslator : BaseQueryTranslator
    {
        /* //sql
        select u.id, u.name, u.birth ,u.fatherId ,u.motherId,    father.name,  mother.name
        from User u
        inner join User father on u.fatherId = father.id 
        left join User mother on u.motherId = mother.id
        where u.id > 1
        limit 1,5;
         */


        /// <summary>
        /// only used for Method ReadSelect
        /// </summary>
        public Type entityType { get; private set; }

        public QueryTranslator(SqlTranslator sqlTranslator, Type entityType) : base(sqlTranslator)
        {
            this.entityType = entityType;
        }


        protected override string ReadSelect(CombinedStream stream)
        {
            switch (stream.method)
            {
                case "Count":
                    {
                        var reader = new NumScalarReader();
                        if (this.dataReader == null) this.dataReader = reader;
                        return "count(*)";
                    }
                case "" or null or "ToList" or nameof(Queryable_Extensions.ToExecuteString):
                    {
                        var reader = new EntityReader();
                        return BuildReader(reader);
                    }
                case "FirstOrDefault" or "First" or "LastOrDefault" or "Last":
                    {
                        stream.take = 1;
                        stream.skip = null;

                        if (stream.method.Contains("Last"))
                            ReverseOrder(stream);

                        var nullable = stream.method.Contains("OrDefault");
                        var reader = new FirstEntityReader { nullable = nullable };
                        return BuildReader(reader);
                    }
            }
            throw new NotSupportedException("not supported method: " + stream.method);


            #region BuildReader
            string BuildReader(EntityReader reader)
            {
                var resultEntityType = entityType;
                ExpressionNode selectedFields = stream.select?.fields as ExpressionNode;
                if (selectedFields == null)
                {
                    if (stream.joins?.Any() != true)
                    {
                        selectedFields = ExpressionNode.Member(parameterName: stream.source.alias, memberName: null).Member_SetType(resultEntityType);
                    }
                }

                if (selectedFields == null)
                    throw new NotSupportedException("select could not be null");

                if (resultEntityType == null && selectedFields.nodeType == NodeType.New)
                {
                    resultEntityType = selectedFields.New_GetType();
                }

                //if (resultEntityType == null)
                //    throw new NotSupportedException("resultEntityType could not be null");

                var sqlFields = reader.BuildSelect(resultEntityType, sqlTranslator, sqlTranslator.dbContext.convertService, selectedFields);
                if (dataReader == null) dataReader = reader;
                return (stream.distinct == true ? "distinct " : "") + sqlFields;
            }
            #endregion
        }
        void ReverseOrder(CombinedStream stream)
        {
            stream.orders ??= new();
            var orders = stream.orders;
            // make sure orders exist
            if (!orders.Any())
            {
                AddOrder(stream.source);
                stream.joins?.ForEach(right => AddOrder(right.right));

                #region AddOrder
                void AddOrder(IStream source)
                {
                    if (source is SourceStream sourceStream)
                    {
                        var entityType = sourceStream.GetEntityType();
                        var entityDescriptor = sqlTranslator.GetEntityDescriptor(entityType);
                        if (entityDescriptor != null)
                        {
                            var member = ExpressionNode_RenameableMember.Member(stream: source, entityType);
                            member.memberName = entityDescriptor.keyName;
                            orders.Add(new OrderField { member = member, asc = true });
                        }
                    }
                }
                #endregion
            }

            // reverse order
            orders?.ForEach(order => order.asc = !order.asc);
        }


    }
}
