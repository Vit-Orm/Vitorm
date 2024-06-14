using System;
using System.Collections.Generic;
using System.Linq;

using Vit.Linq.ExpressionTree.ComponentModel;

using Vitorm.Sql.DataReader;
using Vitorm.StreamQuery;

namespace Vitorm.Sql.SqlTranslate
{

    public abstract class BaseQueryTranslateService : IQueryTranslateService
    {
        public SqlTranslateService sqlTranslator { get; protected set; }


        public BaseQueryTranslateService(SqlTranslateService sqlTranslator)
        {
            this.sqlTranslator = sqlTranslator;
        }



        public virtual string BuildQuery(QueryTranslateArgument arg, CombinedStream stream)
        {

            string sql = "";

            // #0  select
            sql += ReadSelect(arg, stream);


            #region #1 from
            sql += "\r\n from " + ReadInnerTable(arg, stream.source);
            #endregion

            #region #2 join
            if (stream.joins != null)
            {
                sql +=  ReadJoin(arg,stream);
            }
            #endregion

            // #3 where 1=1
            if (stream.where != null)
            {
                var where = sqlTranslator.EvalExpression(arg, stream.where);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n where " + where;
            }

            #region #4 group by
            if (stream.groupByFields != null)
            {
                sql += "\r\n group by " + ReadGroupBy(arg, stream);
            }
            #endregion

            #region #5 having
            if (stream.having != null)
            {
                var where = sqlTranslator.EvalExpression(arg, stream.having);
                if (!string.IsNullOrWhiteSpace(where)) sql += "\r\n having " + where;
            }
            #endregion


            // #6 OrderBy
            if (stream.orders?.Any() == true)
            {
                sql += "\r\n order by " + ReadOrderBy(arg, stream);
            }

            // #7 Range,  limit 1000,10       limit {skip},{take}   |     limit {take}
            if (stream.take != null || stream.skip != null)
            {
                string sqlRange = "limit " + (stream.skip == null ? "" : (stream.skip + ",")) + (stream.take ?? 100000000);
                sql += "\r\n " + sqlRange;
            }

            return sql;
        }


        #region Read partial query


        /// <summary>
        /// "select *";
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected virtual string ReadSelect(QueryTranslateArgument arg, CombinedStream stream, string prefix = "select")
        {
            return prefix + " *";
        }

        protected virtual string ReadJoin(QueryTranslateArgument arg, CombinedStream stream)
        {
            var sql = "";
            stream.joins?.ForEach(streamToJoin =>
            {
                sql += "\r\n " + (streamToJoin.joinType == EJoinType.InnerJoin ? "inner join" : "left join");
                sql += " " + ReadInnerTable(arg, streamToJoin.right);

                var on = sqlTranslator.EvalExpression(arg, streamToJoin.on);
                if (!string.IsNullOrWhiteSpace(on)) sql += " on " + on;
            });
            return sql;
        }
        protected virtual string ReadGroupBy(QueryTranslateArgument arg, CombinedStream stream)
        {
            var node = stream.groupByFields;
            List<string> fields = new();
            if (node?.nodeType == NodeType.New)
            {
                ExpressionNode_New newNode = node;
                newNode.constructorArgs.ForEach((Action<MemberBind>)(nodeArg =>
                {
                    fields.Add(sqlTranslator.EvalExpression(arg, (ExpressionNode)nodeArg.value));
                }));
            }
            else if (node?.nodeType == NodeType.Member)
            {
                fields.Add(sqlTranslator.EvalExpression(arg, node));
            }
            else
            {
                throw new NotSupportedException("[QueryTranslator] groupByFields is not valid: must be New or Member");
            }
           return String.Join(", ", fields);
        }
        protected virtual string ReadOrderBy(QueryTranslateArgument arg, CombinedStream stream) 
        {
            var fields = stream.orders.Select(field =>
                {
                    var sqlField = sqlTranslator.EvalExpression(arg, field.member);
                    return sqlField + " " + (field.asc ? "asc" : "desc");
                }
            ).ToList();

            return String.Join(", ", fields);
        }
        #endregion

        protected string ReadInnerTable(QueryTranslateArgument arg, IStream stream)
        {
            if (stream is SourceStream sourceStream)
            {
                IQueryable query = sourceStream.GetSource() as IQueryable;
                var tableName = arg.dbContext.GetEntityDescriptor(query.ElementType)?.tableName;
                return $"{sqlTranslator.DelimitIdentifier(tableName)} as " + stream.alias;
            }
            if (stream is CombinedStream baseStream)
            {
                var innerQuery = BuildQuery(arg, baseStream);
                return $"({innerQuery}) as " + stream.alias;
            }
            throw new NotSupportedException();
        }
        protected virtual string BuildReader(QueryTranslateArgument arg, CombinedStream stream, EntityReader reader)
        {
            var resultEntityType = arg.resultEntityType;
            ExpressionNode selectedFields = stream.select?.fields;
            if (selectedFields == null)
            {
                if (stream.joins?.Any() != true && resultEntityType != null)
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

            var sqlFields = reader.BuildSelect(arg, resultEntityType, sqlTranslator, arg.dbContext.convertService, selectedFields);
            if (arg.dataReader == null) arg.dataReader = reader;
            return (stream.distinct == true ? "distinct " : "") + sqlFields;
        }

        protected virtual void ReverseOrder(QueryTranslateArgument arg, CombinedStream stream)
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
                        var entityDescriptor = arg.dbContext.GetEntityDescriptor(entityType);
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
