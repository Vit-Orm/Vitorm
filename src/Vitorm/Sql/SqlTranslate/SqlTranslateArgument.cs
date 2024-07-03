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
    }
}
