using Vit.Linq.ExpressionNodes;

using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader.EntityReader
{
    public class EntityReaderConfig
    {
        public QueryTranslateArgument queryTranslateArgument;

        public ExpressionConvertService convertService;
        public ISqlTranslateService sqlTranslateService;

        public SqlColumns sqlColumns;
    }
}
