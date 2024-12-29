using Vitorm.DataProvider;
using Vitorm.Sql;
using Vitorm.Sql.DataProvider;

namespace Vitorm
{
    public static class IDataProvider_Extensions
    {
        public static SqlDbContext CreateSqlDbContext(this IDataProvider dataProvider)
        {
            return (dataProvider as SqlDataProvider)?.CreateDbContext();
        }
    }
}
