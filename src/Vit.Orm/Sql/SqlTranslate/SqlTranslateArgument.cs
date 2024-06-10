using System;
using System.Collections.Generic;
using System.Text;

using Vit.Orm.Entity;

namespace Vit.Orm.Sql.SqlTranslate
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
    }
}
