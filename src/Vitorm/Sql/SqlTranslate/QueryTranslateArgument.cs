using System;
using System.Collections.Generic;

namespace Vitorm.Sql.SqlTranslate
{
    public class QueryTranslateArgument
    {
        public SqlDbContext dbContext { get; protected set; }

        public Type resultEntityType { get; protected set; }


        public QueryTranslateArgument(SqlDbContext dbContext, Type resultEntityType)
        {
            this.dbContext = dbContext;
            this.resultEntityType = resultEntityType;
        }



        public IDbDataReader dataReader { get; set; }

        public Dictionary<string, object> sqlParam { get; protected set; }

        protected int paramIndex = 0;

        /// <summary>
        /// add sqlParam and get the generated sqlParam name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string AddParamAndGetName(object value)
        {
            sqlParam ??= new();
            var paramName = "p" + (paramIndex++);

            sqlParam[paramName] = value;
            return paramName;
        }

    }
}
