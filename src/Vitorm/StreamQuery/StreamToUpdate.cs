
using Vit.Linq.ExpressionTree.ComponentModel;

namespace Vitorm.StreamQuery
{
    public partial class StreamToUpdate : CombinedStream
    {
        public StreamToUpdate(IStream source) : base(source.alias)
        {
            if (source is CombinedStream combinedStream)
            {
                this.select = combinedStream.select;
                this.distinct = combinedStream.distinct;
                this.source = combinedStream.source;
                this.joins = combinedStream.joins;
                this.where = combinedStream.where;
                this.groupByFields = combinedStream.groupByFields;
                this.having = combinedStream.having;
                this.orders = combinedStream.orders;
                this.skip = combinedStream.skip;
                this.take = combinedStream.take;
            }
            else
            {
                base.source = source;
            }
            method = nameof(Orm_Extensions.ExecuteUpdate);
        }

        // ExpressionNode_New   new { name = name + "_" }
        public ExpressionNode_New fieldsToUpdate { get; set; }

    }
}
