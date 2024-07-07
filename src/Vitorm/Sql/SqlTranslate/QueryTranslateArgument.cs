using System;
using System.Collections.Generic;

namespace Vitorm.Sql.SqlTranslate
{
    public class QueryTranslateArgument
    {
        public DbContext dbContext { get; protected set; }

        public Type resultEntityType { get; protected set; }


        public QueryTranslateArgument(DbContext dbContext, Type resultEntityType)
        {
            this.dbContext = dbContext;
            this.resultEntityType = resultEntityType;
        }



        public IDbDataReader dataReader { get; set; }
        public Dictionary<string, object> sqlParam { get; protected set; } = new Dictionary<string, object>();

        protected int paramIndex = 0;
        public string NewParamName() => "p" + (paramIndex++);
    }
}
