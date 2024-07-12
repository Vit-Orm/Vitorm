using System.Collections.Generic;

using Vitorm.Entity;

namespace Vitorm.Sql.SqlTranslate
{
    public class SqlTranslateArgument
    {
        public DbContext dbContext { get; protected set; }
        public IEntityDescriptor entityDescriptor { get; protected set; }

        public SqlTranslateArgument(DbContext dbContext, IEntityDescriptor entityDescriptor)
        {
            this.dbContext = dbContext;
            this.entityDescriptor = entityDescriptor;
        }


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
