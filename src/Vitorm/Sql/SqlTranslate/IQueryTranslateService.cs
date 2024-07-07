using Vitorm.StreamQuery;

namespace Vitorm.Sql.SqlTranslate
{
    public interface IQueryTranslateService
    {
        string BuildQuery(QueryTranslateArgument arg, CombinedStream stream);
    }

}
