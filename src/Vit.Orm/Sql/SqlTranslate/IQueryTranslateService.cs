using Vit.Linq.ExpressionTree.CollectionsQuery;

namespace Vit.Orm.Sql.SqlTranslate
{
    public interface IQueryTranslateService 
    {
        string BuildQuery(QueryTranslateArgument arg, CombinedStream stream);
    }

}
