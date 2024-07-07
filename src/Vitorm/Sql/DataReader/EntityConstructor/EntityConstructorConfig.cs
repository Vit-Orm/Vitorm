using Vit.Linq.ExpressionTree;

using Vitorm.Sql.SqlTranslate;

namespace Vitorm.Sql.DataReader.EntityConstructor
{
    public class EntityConstructorConfig
    {
        public QueryTranslateArgument arg;

        public ExpressionConvertService convertService;
        public ISqlTranslateService sqlTranslateService;

        public SqlColumns sqlColumns;
    }
}
